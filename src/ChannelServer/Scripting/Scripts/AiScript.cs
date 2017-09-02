// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading;
using Aura.Channel.Network.Sending;
using Aura.Channel.Skills;
using Aura.Channel.Skills.Base;
using Aura.Channel.Skills.Combat;
using Aura.Channel.Skills.Life;
using Aura.Channel.Skills.Magic;
using Aura.Channel.Skills.Music;
using Aura.Channel.World;
using Aura.Channel.World.Entities;
using Aura.Data;
using Aura.Mabi;
using Aura.Mabi.Const;
using Aura.Shared.Network;
using Aura.Shared.Scripting.Scripts;
using Aura.Shared.Util;

namespace Aura.Channel.Scripting.Scripts
{
    // TODO: Rewrite into the new tree design before we make more
    //   of a mess out of this than necessary.
    public abstract class AiScript : IScript, IDisposable
    {
        public enum AiEvent
        {
            None,
            Hit,
            CriticalHit,
            DefenseHit,
            MagicHit,
            KnockDown,
            CriticalKnockDown
        }

        public enum AiState
        {
            /// <summary>
            ///     Doing nothing
            /// </summary>
            Idle,

            /// <summary>
            ///     Doing nothing, but noticed a potential target
            /// </summary>
            Aware,

            /// <summary>
            ///     Watching target (!)
            /// </summary>
            Alert,

            /// <summary>
            ///     Aggroing target (!!)
            /// </summary>
            Aggro,

            /// <summary>
            ///     Likes target
            /// </summary>
            Love
        }

        protected bool _active;
        protected AggroLimit _aggroLimit;

        // Settings
        protected int _aggroRadius, _aggroMaxRadius;

        protected TimeSpan _alertDelay, _aggroDelay, _hateBattleStanceDelay, _hateOverTimeDelay;
        protected DateTime _awareTime, _alertTime;
        protected IEnumerator _curAction;
        protected bool _hatesBattleStance;
        protected Dictionary<string, string> _hateTags, _loveTags, _doubtTags;
        protected int _heartbeat;

        // Maintenance
        protected Timer _heartbeatTimer;

        private bool _inside;
        protected DateTime _lastBeat;
        protected int _maxDistanceFromSpawn;
        protected DateTime _minRunTime;
        protected Creature _newAttackable;

        // Heartbeat cache
        protected IList<Creature> _playersInRange;

        protected Dictionary<AiState, Dictionary<AiEvent, Dictionary<SkillId, Func<IEnumerable>>>> _reactions;

        protected Random _rnd;
        protected AiState _state;
        private int _stuckTestCount;

        protected Dictionary<int, int> _summons = new Dictionary<int, int>();

        // Misc
        private int _switchRandomN, _switchRandomM;

        protected double _timestamp;
        protected double _visualRadian;
        protected int _visualRadius;
        protected int AggroHeartbeat = 50; // ms

        protected int IdleHeartbeat = 250; // ms
        // Official heartbeat while following a target seems
        // to be about 100-200ms?

        protected int MinHeartbeat = 50; // ms

        protected AiScript()
        {
            Phrases = new List<string>();

            _lastBeat = DateTime.MinValue;
            _heartbeat = IdleHeartbeat;
            _heartbeatTimer = new Timer(Heartbeat, null, -1, -1);

            _rnd = new Random(RandomProvider.Get().Next());
            _reactions = new Dictionary<AiState, Dictionary<AiEvent, Dictionary<SkillId, Func<IEnumerable>>>>();
            _reactions[AiState.Idle] = new Dictionary<AiEvent, Dictionary<SkillId, Func<IEnumerable>>>();
            _reactions[AiState.Aware] = new Dictionary<AiEvent, Dictionary<SkillId, Func<IEnumerable>>>();
            _reactions[AiState.Alert] = new Dictionary<AiEvent, Dictionary<SkillId, Func<IEnumerable>>>();
            _reactions[AiState.Aggro] = new Dictionary<AiEvent, Dictionary<SkillId, Func<IEnumerable>>>();
            _reactions[AiState.Love] = new Dictionary<AiEvent, Dictionary<SkillId, Func<IEnumerable>>>();

            _state = AiState.Idle;
            _aggroRadius = 500;
            _aggroMaxRadius = 3000;
            _visualRadius = 900;
            _visualRadian = 90;
            _alertDelay = TimeSpan.FromMilliseconds(6000);
            _aggroDelay = TimeSpan.FromMilliseconds(500);
            _hateOverTimeDelay = TimeSpan.FromDays(365);
            _hateBattleStanceDelay = TimeSpan.FromMilliseconds(3000);
            _hateTags = new Dictionary<string, string>();
            _loveTags = new Dictionary<string, string>();
            _doubtTags = new Dictionary<string, string>();

            _maxDistanceFromSpawn = 3000;

            _aggroLimit = AggroLimit.One;
        }

        /// <summary>
        ///     Creature controlled by AI.
        /// </summary>
        public Creature Creature { get; protected set; }

        /// <summary>
        ///     List of random phrases
        /// </summary>
        public List<string> Phrases { get; protected set; }

        /// <summary>
        ///     Returns whether the AI is currently active.
        /// </summary>
        public bool Active => _active;

        /// <summary>
        ///     Returns state of the AI
        /// </summary>
        public AiState State => _state;

        /// <summary>
        ///     Disables heartbeat timer.
        /// </summary>
        public void Dispose()
        {
            _heartbeatTimer.Change(-1, -1);
            _heartbeatTimer.Dispose();
            _heartbeatTimer = null;
        }

        /// <summary>
        ///     Called when script is initialized after loading it.
        /// </summary>
        /// <returns></returns>
        public bool Init()
        {
            var attr = GetType().GetCustomAttribute<AiScriptAttribute>();
            if (attr == null)
            {
                Log.Error("AiScript.Init: Missing AiScript attribute.");
                return false;
            }

            foreach (var name in attr.Names)
                ChannelServer.Instance.ScriptManager.AiScripts.Add(name, GetType());

            return true;
        }

        /// <summary>
        ///     Starts AI
        /// </summary>
        public void Activate(double minRunTime)
        {
            if (!_active && _heartbeatTimer != null)
            {
                _active = true;
                _minRunTime = DateTime.Now.AddMilliseconds(minRunTime);
                _heartbeatTimer.Change(_heartbeat, _heartbeat);
            }
        }

        /// <summary>
        ///     Pauses AI
        /// </summary>
        public void Deactivate()
        {
            if (_active && _heartbeatTimer != null)
            {
                _active = false;
                _curAction = null;
                _heartbeatTimer.Change(-1, -1);
            }
        }

        /// <summary>
        ///     Sets AI's creature.
        /// </summary>
        /// <param name="creature"></param>
        public void Attach(Creature creature)
        {
            Creature = creature;
            Creature.Finish += OnDeath;
        }

        /// <summary>
        ///     Unsets AI's creature.
        /// </summary>
        /// <param name="creature"></param>
        public void Detach()
        {
            var npc = Creature as NPC;
            if (npc == null || npc.AI == null)
                return;

            npc.AI.Dispose();
            npc.Finish -= OnDeath;
            npc.AI = null;
            Creature = null;
        }

        /// <summary>
        ///     Main "loop"
        /// </summary>
        /// <param name="state"></param>
        private void Heartbeat(object state)
        {
            if (Creature == null || Creature.Region == Region.Limbo)
                return;

            // Skip tick if the previous one is still on.
            if (_inside)
            {
                if (++_stuckTestCount == 10)
                    Log.Warning("AiScript.Heartbeat: {0} stuck?", GetType().Name);
                return;
            }

            _inside = true;
            _stuckTestCount = 0;
            try
            {
                var now = UpdateTimestamp();
                var pos = Creature.GetPosition();

                // Stop if no players in range
                _playersInRange = Creature.Region.GetPlayersInRange(pos);
                if (_playersInRange.Count == 0 && now > _minRunTime)
                {
                    Deactivate();
                    Reset();
                    return;
                }

                if (Creature.IsDead)
                    return;

                SelectState();

                // Stop and clear if stunned
                if (Creature.IsStunned)
                    return;

                // Select and run state
                var prevAction = _curAction;
                if (_curAction == null || !_curAction.MoveNext())
                    if (_curAction == prevAction)
                    {
                        switch (_state)
                        {
                            default:
                            case AiState.Idle:
                                SwitchAction(Idle);
                                break;
                            case AiState.Alert:
                                SwitchAction(Alert);
                                break;
                            case AiState.Aggro:
                                SwitchAction(Aggro);
                                break;
                            case AiState.Love:
                                SwitchAction(Love);
                                break;
                        }

                        _curAction.MoveNext();
                    }
            }
            catch (Exception ex)
            {
                Log.Exception(ex, "Exception in {0}", GetType().Name);
            }
            finally
            {
                _inside = false;
            }
        }

        /// <summary>
        ///     Updates timestamp and returns DateTime.Now.
        /// </summary>
        /// <returns></returns>
        private DateTime UpdateTimestamp()
        {
            var now = DateTime.Now;
            _timestamp += (now - _lastBeat).TotalMilliseconds;
            return _lastBeat = now;
        }

        /// <summary>
        ///     Clears action, target, and sets state to Idle.
        /// </summary>
        private void Reset()
        {
            Clear();
            _state = AiState.Idle;

            if (Creature.IsInBattleStance)
                Creature.IsInBattleStance = false;

            if (Creature.Target != null)
            {
                Creature.Target = null;
                Send.SetCombatTarget(Creature, 0, 0);
            }
        }

        /// <summary>
        ///     Changes state based on (potential) targets.
        /// </summary>
        private void SelectState()
        {
            var pos = Creature.GetPosition();

            // Get perceivable targets
            var radius = Math.Max(_aggroRadius, _visualRadius);
            var potentialTargets = Creature.Region.GetVisibleCreaturesInRange(Creature, radius).Where(c => !c.Warping);
            potentialTargets = potentialTargets.Where(a => CanPerceive(pos, Creature.Direction, a.GetPosition()));

            // Stay in idle if there's no visible creature in aggro range
            if (!potentialTargets.Any() && Creature.Target == null)
            {
                if (_state != AiState.Idle)
                    Reset();

                return;
            }

            // Find a new target
            if (Creature.Target == null)
            {
                // Get hated targets
                var hated = potentialTargets.Where(cr =>
                    !cr.IsDead && DoesHate(cr) && !cr.Has(CreatureStates.NamedNpc));
                var hatedCount = hated.Count();

                // Get doubted targets
                var doubted =
                    potentialTargets.Where(cr => !cr.IsDead && DoesDoubt(cr) && !cr.Has(CreatureStates.NamedNpc));
                var doubtedCount = doubted.Count();

                // Get loved targets
                var loved = potentialTargets.Where(cr => !cr.IsDead && DoesLove(cr));
                var lovedCount = loved.Count();

                // Handle hate and doubt
                if (hatedCount != 0 || doubtedCount != 0)
                {
                    // Try to hate first, then doubt
                    if (hatedCount != 0)
                        Creature.Target = hated.ElementAt(Random(hatedCount));
                    else
                        Creature.Target = doubted.ElementAt(Random(doubtedCount));

                    // Switch to aware
                    _state = AiState.Aware;
                    _awareTime = DateTime.Now;
                }
                // Handle love
                else if (lovedCount != 0)
                {
                    Creature.Target = loved.ElementAt(Random(lovedCount));

                    _state = AiState.Love;
                }
                // Stop if no targets were found
                else
                {
                    return;
                }

                // Stop for this tick, the aware delay needs a moment anyway
                return;
            }

            // TODO: Monsters switch targets under certain circumstances,
            //   e.g. a wolf will aggro a player, even if it has already
            //   noticed a cow.

            // Reset on...
            if (Creature.Target.IsDead // target dead
                || Creature.Region == Region.Limbo // invalid region (e.g. unsummoned pet)
                || !Creature.GetPosition().InRange(Creature.Target.GetPosition(), _aggroMaxRadius) // out of aggro range
                || Creature.Target.Warping // target is warping
                || Creature.Target.Client.State == ClientState.Dead // target disconnected
                || _state != AiState.Aggro &&
                Creature.Target.Conditions.Has(ConditionsA.Invisible) // target hid before reaching aggro state
            )
            {
                Reset();
                return;
            }

            // Switch to alert from aware after the delay
            if (_state == AiState.Aware && DateTime.Now >= _awareTime + _alertDelay)
                if (CanPerceive(pos, Creature.Direction, Creature.Target.GetPosition()))
                {
                    Clear();

                    _state = AiState.Alert;
                    _alertTime = DateTime.Now;
                    Creature.IsInBattleStance = true;

                    Send.SetCombatTarget(Creature, Creature.Target.EntityId, TargetMode.Alert);
                }
                // Reset if target ran away like a coward.
                else
                {
                    Reset();
                    return;
                }

            // Switch to aggro from alert
            if (_state == AiState.Alert &&
                (
                    // Aggro hated creatures after aggro delay
                    DoesHate(Creature.Target) && DateTime.Now >= _alertTime + _aggroDelay ||

                    // Aggro battle stance targets
                    _hatesBattleStance && Creature.Target.IsInBattleStance &&
                    DateTime.Now >= _alertTime + _hateBattleStanceDelay ||

                    // Hate over time
                    DateTime.Now >= _awareTime + _hateOverTimeDelay
                ))
            {
                // Check aggro limit
                var aggroCount = Creature.Region.CountAggro(Creature.Target, Creature.RaceId);
                if (aggroCount >= (int) _aggroLimit) return;

                Clear();

                _state = AiState.Aggro;
                Send.SetCombatTarget(Creature, Creature.Target.EntityId, TargetMode.Aggro);
            }
        }

        /// <summary>
        ///     Returns true if AI can hear or see at target pos from pos.
        /// </summary>
        /// <param name="pos">Position AI's creature is at.</param>
        /// <param name="direction">AI creature's current direction.</param>
        /// <param name="targetPos">Position of the potential target.</param>
        /// <returns></returns>
        protected virtual bool CanPerceive(Position pos, byte direction, Position targetPos)
        {
            return CanHear(pos, targetPos) || CanSee(pos, direction, targetPos);
        }

        /// <summary>
        ///     Returns true if target position is within hearing range.
        /// </summary>
        /// <param name="pos">Position from which AI creature listens.</param>
        /// <param name="targetPos">Position of the potential target.</param>
        /// <returns></returns>
        protected virtual bool CanHear(Position pos, Position targetPos)
        {
            return pos.InRange(targetPos, _aggroRadius);
        }

        /// <summary>
        ///     Returns true if target position is within visual field.
        /// </summary>
        /// <param name="pos">Position from which AI creature listens.</param>
        /// <param name="direction">AI creature's current direction.</param>
        /// <param name="targetPos">Position of the potential target.</param>
        /// <returns></returns>
        protected virtual bool CanSee(Position pos, byte direction, Position targetPos)
        {
            return targetPos.InCone(pos, MabiMath.ByteToRadian(direction), _visualRadius, _visualRadian);
        }

        /// <summary>
        ///     Idle state
        /// </summary>
        protected virtual IEnumerable Idle()
        {
            yield break;
        }

        /// <summary>
        ///     Alert state
        /// </summary>
        protected virtual IEnumerable Alert()
        {
            yield break;
        }

        /// <summary>
        ///     Aggro state
        /// </summary>
        protected virtual IEnumerable Aggro()
        {
            yield break;
        }

        /// <summary>
        ///     Love state
        /// </summary>
        protected virtual IEnumerable Love()
        {
            yield break;
        }

        // Setup
        // ------------------------------------------------------------------

        /// <summary>
        ///     Changes the hearbeat interval.
        /// </summary>
        /// <param name="interval"></param>
        protected void SetHeartbeat(int interval)
        {
            _heartbeat = Math.Max(MinHeartbeat, interval);
            _heartbeatTimer.Change(_heartbeat, _heartbeat);
        }

        /// <summary>
        ///     Milliseconds before creature notices.
        /// </summary>
        /// <param name="time"></param>
        protected void SetAlertDelay(int time)
        {
            _alertDelay = TimeSpan.FromMilliseconds(time);
        }

        /// <summary>
        ///     Milliseconds before creature attacks.
        /// </summary>
        /// <param name="time"></param>
        protected void SetAggroDelay(int time)
        {
            _aggroDelay = TimeSpan.FromMilliseconds(time);
        }

        /// <summary>
        ///     Radius in which creature become potential targets.
        /// </summary>
        /// <param name="radius"></param>
        protected void SetAggroRadius(int radius)
        {
            _aggroRadius = radius;
        }

        /// <summary>
        ///     Sets visual field used for aggroing.
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="angle"></param>
        protected void SetVisualField(int radius, double angle)
        {
            var a = Math2.Clamp(0, 160, (int) angle);

            _visualRadius = radius;
            _visualRadian = MabiMath.DegreeToRadian(a);
        }

        /// <summary>
        ///     The way the AI decides whether to go into Alert/Aggro.
        /// </summary>
        /// <param name="type"></param>
        protected void SetAggroType(AggroType type)
        {
            //_aggroType = type;
            Log.Warning("{0}: SetAggroType is obsolete, use 'Doubts' and 'HatesBattleStance' instead.", GetType().Name);
        }

        /// <summary>
        ///     Milliseconds before creature attacks.
        /// </summary>
        /// <param name="limit"></param>
        protected void SetAggroLimit(AggroLimit limit)
        {
            _aggroLimit = limit;
        }

        /// <summary>
        ///     Adds a race tag that the AI hates and will target.
        /// </summary>
        /// <param name="tags"></param>
        protected void Hates(params string[] tags)
        {
            foreach (var tag in tags)
            {
                var key = tag.Trim(' ', '/');
                if (_hateTags.ContainsKey(key))
                    return;

                _hateTags.Add(key, tag);
            }
        }

        /// <summary>
        ///     Adds a race tag that the AI likes and will not target unless
        ///     provoked.
        /// </summary>
        /// <param name="tags"></param>
        protected void Loves(params string[] tags)
        {
            foreach (var tag in tags)
            {
                var key = tag.Trim(' ', '/');
                if (_loveTags.ContainsKey(key))
                    return;

                _loveTags.Add(key, tag);
            }
        }

        /// <summary>
        ///     Adds a race tag that the AI doubts.
        /// </summary>
        /// <param name="tags"></param>
        protected void Doubts(params string[] tags)
        {
            foreach (var tag in tags)
            {
                var key = tag.Trim(' ', '/');
                if (_hateTags.ContainsKey(key))
                    return;

                _doubtTags.Add(key, tag);
            }
        }

        /// <summary>
        ///     Specifies that the AI will go from alert into aggro when enemy
        ///     changes into battle mode.
        /// </summary>
        protected void HatesBattleStance(int delay = 3000)
        {
            _hatesBattleStance = true;
            _hateBattleStanceDelay = TimeSpan.FromMilliseconds(delay);
        }

        /// <summary>
        ///     Specifies that the AI will go from alert into aggro when a
        ///     doubted target sticks around for too long.
        /// </summary>
        /// <param name="delay"></param>
        protected void HatesNearby(int delay = 6000)
        {
            _hateOverTimeDelay = TimeSpan.FromMilliseconds(delay);
        }

        /// <summary>
        ///     Sets the max distance an NPC can wander away from its spawn.
        /// </summary>
        /// <param name="distance"></param>
        protected void SetMaxDistanceFromSpawn(int distance)
        {
            _maxDistanceFromSpawn = distance;
        }

        /// <summary>
        ///     Reigsters a reaction.
        /// </summary>
        /// <param name="ev">The event on which func should be executed.</param>
        /// <param name="func">The reaction to the event.</param>
        protected void On(AiState state, AiEvent ev, Func<IEnumerable> func)
        {
            On(state, ev, SkillId.None, func);
        }

        /// <summary>
        ///     Reigsters a reaction.
        /// </summary>
        /// <param name="state">The state the event is for.</param>
        /// <param name="ev">The event on which func should be executed.</param>
        /// <param name="skillId">The skill the should trigger the event.</param>
        /// <param name="func">The reaction to the event.</param>
        protected void On(AiState state, AiEvent ev, SkillId skillId, Func<IEnumerable> func)
        {
            lock (_reactions)
            {
                if (!_reactions[state].ContainsKey(ev))
                    _reactions[state][ev] = new Dictionary<SkillId, Func<IEnumerable>>();
                _reactions[state][ev][skillId] = func;
            }
        }

        // Functions
        // ------------------------------------------------------------------

        /// <summary>
        ///     Returns random number between 0.0 and 100.0.
        /// </summary>
        /// <returns></returns>
        protected double Random()
        {
            lock (_rnd)
            {
                return 100 * _rnd.NextDouble();
            }
        }

        /// <summary>
        ///     Returns random number between 0 and max-1.
        /// </summary>
        /// <param name="max">Exclusive upper bound</param>
        /// <returns></returns>
        protected int Random(int max)
        {
            lock (_rnd)
            {
                return _rnd.Next(max);
            }
        }

        /// <summary>
        ///     Returns random number between min and max-1.
        /// </summary>
        /// <param name="min">Inclusive lower bound</param>
        /// <param name="max">Exclusive upper bound</param>
        /// <returns></returns>
        protected int Random(int min, int max)
        {
            lock (_rnd)
            {
                return _rnd.Next(min, max);
            }
        }

        /// <summary>
        ///     Returns a random value from the given ones.
        /// </summary>
        /// <param name="values"></param>
        protected T Rnd<T>(params T[] values)
        {
            lock (_rnd)
            {
                return _rnd.Rnd(values);
            }
        }

        /// <summary>
        ///     Returns true if AI hates target creature.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        protected bool DoesHate(Creature target)
        {
            return _hateTags.Values.Any(tag => target.RaceData.HasTag(tag));
        }

        /// <summary>
        ///     Returns true if AI loves target creature.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        protected bool DoesLove(Creature target)
        {
            return _loveTags.Values.Any(tag => target.RaceData.HasTag(tag));
        }

        /// <summary>
        ///     Returns true if AI doubts target creature.
        /// </summary>
        /// <param name="target"></param>
        /// <returns></returns>
        protected bool DoesDoubt(Creature target)
        {
            return _doubtTags.Values.Any(tag => target.RaceData.HasTag(tag));
        }

        /// <summary>
        ///     Sends SharpMind to all applicable creatures.
        /// </summary>
        /// <remarks>
        ///     The Wiki is speaking of a passive Sharp Mind skill, but it doesn't
        ///     seem to be a skill at all anymore.
        ///     A failed Sharp Mind is supposed to be displayed as an "X",
        ///     assumingly statuses 3 and 4 were used for this in the past,
        ///     but the current NA client doesn't do anything when sending
        ///     them, so we use skill id 0 instead, which results in a
        ///     question mark, originally used for skills unknown to the
        ///     player.
        ///     Even on servers that didn't have Sharp Mind officially,
        ///     the packets were still sent to the client, it just didn't
        ///     display them, assumingly because the players didn't have
        ///     the skill. Since this is not the case for the NA client,
        ///     we control it from the server.
        ///     TODO: When we move AIs to an NPC client, the entire SharpMind
        ///     handling would move to the SkillPrepare handler.
        /// </remarks>
        /// <param name="skillId"></param>
        /// <param name="status"></param>
        protected void SharpMind(SkillId skillId, SharpMindStatus status)
        {
            // Some races are "immune" to Sharp Mind
            if (Creature.RaceData.SharpMindImmune)
                return;

            // Check if SharpMind is enabled
            if (!AuraData.FeaturesDb.IsEnabled("SharpMind"))
                return;

            var passive = AuraData.FeaturesDb.IsEnabled("PassiveSharpMind");

            // Send to players in range, one after the other, so we have control
            // over the recipients.
            foreach (var creature in _playersInRange)
            {
                // Handle active (old) Sharp Mind
                if (!passive)
                {
                    // Don't send if player doesn't have Sharp Mind.
                    if (!creature.Skills.Has(SkillId.SharpMind))
                        continue;

                    var success = Random() < ChannelServer.Instance.Conf.World.SharpMindChance;

                    // Set skill id to 0, so the bubble displays a question mark,
                    // if skill is unknown to the player or Sharp Mind fails.
                    if (!creature.Skills.Has(skillId) || !success)
                        skillId = SkillId.None;

                    SharpMindHandler.Train(Creature, creature, success);
                }

                // Cancel and None are sent for removing the bubble
                if (status == SharpMindStatus.Cancelling || status == SharpMindStatus.None)
                {
                    Send.SharpMind(Creature, creature, skillId, SharpMindStatus.Cancelling);
                    Send.SharpMind(Creature, creature, skillId, SharpMindStatus.None);
                }
                else
                {
                    Send.SharpMind(Creature, creature, skillId, status);
                }
            }
        }

        /// <summary>
        ///     Proxy for Localization.Get.
        /// </summary>
        /// <param name="phrase"></param>
        protected static string L(string phrase)
        {
            return Localization.Get(phrase);
        }

        /// <summary>
        ///     Proxy for Localization.GetParticular.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="phrase"></param>
        protected static string LX(string context, string phrase)
        {
            return Localization.GetParticular(context, phrase);
        }

        /// <summary>
        ///     Proxy for Localization.GetPlural.
        /// </summary>
        /// <param name="phrase"></param>
        /// <param name="phrasePlural"></param>
        /// <param name="count"></param>
        protected static string LN(string phrase, string phrasePlural, int count)
        {
            return Localization.GetPlural(phrase, phrasePlural, count);
        }

        /// <summary>
        ///     Proxy for Localization.GetParticularPlural.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="phrase"></param>
        /// <param name="phrasePlural"></param>
        /// <param name="count"></param>
        protected static string LXN(string context, string phrase, string phrasePlural, int count)
        {
            return Localization.GetParticularPlural(context, phrase, phrasePlural, count);
        }

        /// <summary>
        ///     Returns true if AI creature has the skill.
        /// </summary>
        /// <param name="skillId"></param>
        /// <returns></returns>
        protected bool HasSkill(SkillId skillId)
        {
            return Creature.Skills.Has(skillId);
        }

        /// <summary>
        ///     Returns true if the AI creature has equipped an item with the given
        ///     id in one of its equip slots.
        /// </summary>
        /// <param name="itemId"></param>
        /// <returns></returns>
        public bool HasEquipped(int itemId)
        {
            var items = Creature.Inventory.GetEquipment(a => a.Info.Id == itemId);
            return items.Any();
        }

        /// <summary>
        ///     Returns true if the AI creature has equipped an item that matches
        ///     the given tag in one of its equip slots.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public bool HasEquipped(string tag)
        {
            var items = Creature.Inventory.GetEquipment(a => a.HasTag(tag));
            return items.Any();
        }

        /// <summary>
        ///     Generates and saves a random number between 0 and 99,
        ///     for Case to use.
        /// </summary>
        /// <remarks>
        ///     SwitchRandom only keeps track of one random number at a time.
        ///     You can nest SwitchRandom-if-constructs, but randomly calling
        ///     SwitchRandom in between might give unexpected results.
        /// </remarks>
        /// <example>
        ///     SwitchRandom();
        ///     if (Case(40))
        ///     {
        ///     Do(Wander(250, 500));
        ///     }
        ///     else if (Case(40))
        ///     {
        ///     Do(Wander(250, 500, false));
        ///     }
        ///     else if (Case(20))
        ///     {
        ///     Do(Wait(4000, 6000));
        ///     }
        ///     SwitchRandom();
        ///     if (Case(60))
        ///     {
        ///     SwitchRandom();
        ///     if (Case(20))
        ///     {
        ///     Do(Wander(250, 500));
        ///     }
        ///     else if (Case(80))
        ///     {
        ///     Do(Wait(4000, 6000));
        ///     }
        ///     }
        ///     else if (Case(40))
        ///     {
        ///     Do(Wander(250, 500, false));
        ///     }
        /// </example>
        protected void SwitchRandom()
        {
            _switchRandomN = Random(100);
            _switchRandomM = 0;
        }

        /// <summary>
        ///     Returns true if value matches the last random percentage
        ///     generated by SwitchRandom().
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        protected bool Case(int value)
        {
            _switchRandomM += value;
            return _switchRandomN < _switchRandomM;
        }

        // Flow control
        // ------------------------------------------------------------------

        /// <summary>
        ///     Cleares action queue.
        /// </summary>
        protected void Clear()
        {
            _curAction = null;
        }

        /// <summary>
        ///     Clears AI and sets new current action.
        /// </summary>
        /// <param name="action"></param>
        protected void SwitchAction(Func<IEnumerable> action)
        {
            ExecuteOnce(CancelSkill());

            // Cancel rest
            if (Creature.Has(CreatureStates.SitDown))
            {
                var restHandler = ChannelServer.Instance.SkillManager.GetHandler<Rest>(SkillId.Rest);
                if (restHandler != null)
                    restHandler.Stop(Creature, Creature.Skills.Get(SkillId.Rest));
            }

            _curAction = action().GetEnumerator();
        }

        /// <summary>
        ///     Creates enumerator and runs it once.
        /// </summary>
        /// <remarks>
        ///     Useful if you want to make a creature go somewhere, but you don't
        ///     want to wait for it to arrive there. Effectively running the action
        ///     with a 0 timeout.
        /// </remarks>
        /// <param name="action"></param>
        protected void ExecuteOnce(IEnumerable action)
        {
            action.GetEnumerator().MoveNext();
        }

        /// <summary>
        ///     Sets target and puts creature in battle mode.
        /// </summary>
        /// <param name="creature"></param>
        public void AggroCreature(Creature creature)
        {
            _state = AiState.Aggro;
            Clear();
            Creature.IsInBattleStance = true;
            Creature.Target = creature;
            Send.SetCombatTarget(Creature, Creature.Target.EntityId, TargetMode.Aggro);
        }

        // Actions
        // ------------------------------------------------------------------

        /// <summary>
        ///     Makes creature say something in public chat.
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        protected IEnumerable Say(string msg)
        {
            if (!string.IsNullOrWhiteSpace(msg))
                Send.Chat(Creature, msg);

            yield break;
        }

        /// <summary>
        ///     Makes creature say one of the messages in public chat.
        /// </summary>
        /// <param name="msg"></param>
        /// <returns></returns>
        protected IEnumerable Say(params string[] msgs)
        {
            if (msgs == null || msgs.Length == 0)
                yield break;

            var msg = msgs[Random(msgs.Length)];
            if (!string.IsNullOrWhiteSpace(msg))
                Send.Chat(Creature, msg);
        }

        /// <summary>
        ///     Makes creature say a random phrase in public chat.
        /// </summary>
        /// <returns></returns>
        protected IEnumerable SayRandomPhrase()
        {
            if (Phrases.Count > 0)
                Send.Chat(Creature, Phrases[Random(Phrases.Count)]);
            yield break;
        }

        /// <summary>
        ///     Makes AI wait for a random amount of ms, between min and max.
        /// </summary>
        /// <param name="min"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        protected IEnumerable Wait(int min, int max = 0)
        {
            if (max < min)
                max = min;

            var duration = min == max ? min : Random(min, max + 1);
            var target = _timestamp + duration;

            while (_timestamp < target)
                yield return true;
        }

        /// <summary>
        ///     Makes creature walk to a random position in range.
        /// </summary>
        /// <param name="minDistance"></param>
        /// <param name="maxDistance"></param>
        /// <returns></returns>
        protected IEnumerable Wander(int minDistance = 100, int maxDistance = 600, bool walk = true)
        {
            if (maxDistance < minDistance)
                maxDistance = minDistance;

            var rnd = RandomProvider.Get();
            var pos = Creature.GetPosition();
            var destination = pos.GetRandomInRange(minDistance, maxDistance, rnd);

            // Make sure NPCs don't wander off
            var npc = Creature as NPC;
            if (npc != null && destination.GetDistance(npc.SpawnLocation.Position) > _maxDistanceFromSpawn)
                destination = pos.GetRelative(npc.SpawnLocation.Position, (minDistance + maxDistance) / 2);

            foreach (var action in MoveTo(destination, walk))
                yield return action;
        }

        /// <summary>
        ///     Runs action till it's done or the timeout is reached.
        /// </summary>
        /// <param name="timeout"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        protected IEnumerable Timeout(double timeout, IEnumerable action)
        {
            timeout += _timestamp;

            foreach (var a in action)
            {
                if (_timestamp >= timeout)
                    yield break;
                yield return true;
            }
        }

        /// <summary>
        ///     Creature runs to destination.
        /// </summary>
        /// <param name="destination"></param>
        /// <returns></returns>
        protected IEnumerable RunTo(Position destination)
        {
            return MoveTo(destination, false);
        }

        /// <summary>
        ///     Creature walks to destination.
        /// </summary>
        /// <param name="destination"></param>
        /// <returns></returns>
        protected IEnumerable WalkTo(Position destination)
        {
            return MoveTo(destination, true);
        }

        /// <summary>
        ///     Creature moves to destination.
        /// </summary>
        /// <param name="destination"></param>
        /// <param name="walk"></param>
        /// <returns></returns>
        protected IEnumerable MoveTo(Position destination, bool walk)
        {
            var pos = Creature.GetPosition();

            // Limit AI to walking if Defense is active (hot-fix)
            if (Creature.Skills.IsActive(SkillId.Defense))
                walk = true;

            // Check for collision
            Position intersection;
            if (Creature.Region.Collisions.Find(pos, destination, out intersection))
            {
                destination = pos.GetRelative(intersection, -100);

                // If new destination is invalid as well don't move at all
                if (Creature.Region.Collisions.Any(pos, destination))
                    destination = pos;
            }

            Creature.Move(destination, walk);

            var time = Creature.MoveDuration * 1000;
            var walkTime = _timestamp + time;

            do
            {
                // Yield at least once, even if it took 0 time,
                // to avoid unexpected problems, like infinite outer loops,
                // because an action expected the walk to yield at least once.
                yield return true;
            } while (_timestamp < walkTime);
        }

        /// <summary>
        ///     Creature circles around target.
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="timeMin"></param>
        /// <param name="timeMax"></param>
        /// <returns></returns>
        protected IEnumerable Circle(int radius, int timeMin = 1000, int timeMax = 5000, bool walk = true)
        {
            return Circle(radius, timeMin, timeMax, Random() < 50, walk);
        }

        /// <summary>
        ///     Creature circles around target.
        /// </summary>
        /// <param name="radius"></param>
        /// <param name="timeMin"></param>
        /// <param name="timeMax"></param>
        /// <param name="clockwise"></param>
        /// <returns></returns>
        protected IEnumerable Circle(int radius, int timeMin, int timeMax, bool clockwise, bool walk)
        {
            if (timeMin < 500)
                timeMin = 500;
            if (timeMax < timeMin)
                timeMax = timeMin;

            var time = timeMin == timeMax ? timeMin : Random(timeMin, timeMax + 1);
            var until = _timestamp + time;

            for (var i = 0; _timestamp < until || i == 0; ++i)
            {
                // Stop if target vanished somehow
                if (Creature.Target == null)
                    yield break;

                var targetPos = Creature.Target.GetPosition();
                var pos = Creature.GetPosition();

                var deltaX = pos.X - targetPos.X;
                var deltaY = pos.Y - targetPos.Y;
                var angle = Math.Atan2(deltaY, deltaX) + Math.PI / 8 * 2 * (clockwise ? -1 : 1);

                var x = targetPos.X + Math.Cos(angle) * radius;
                var y = targetPos.Y + Math.Sin(angle) * radius;
                var actualMovePos = new Position((int) x, (int) y);

                // Move a little further, so the creature doesn't stop for
                // a tick while calculating the next position to walk to
                var distanceMovePos = pos.GetRelative(actualMovePos, radius);

                // Get time it takes to get to the actual position we want to
                // go to, so we can wait till we're there before issuing the
                // next WalkTo.
                var diffX = actualMovePos.X - pos.X;
                var diffY = actualMovePos.Y - pos.Y;
                var moveDuration = (int) (Math.Sqrt(diffX * diffX + diffY * diffY) / Creature.GetSpeed() * 1000);

                ExecuteOnce(MoveTo(distanceMovePos, walk));

                foreach (var action in Wait(moveDuration))
                    yield return action;
            }

            // Stop movement after circling is done, so the creature doesn't
            // finish walking to the more distant position.
            Creature.StopMove();
        }

        /// <summary>
        ///     Creature follows its target.
        /// </summary>
        /// <param name="maxDistance"></param>
        /// <param name="walk"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        protected IEnumerable Follow(int maxDistance, bool walk = false, int timeout = 5000)
        {
            var until = _timestamp + Math.Max(0, timeout);

            while (_timestamp < until)
            {
                // Stop if target vanished somehow
                if (Creature.Target == null)
                    yield break;

                var pos = Creature.GetPosition();
                var targetPos = Creature.Target.GetPosition();

                if (!pos.InRange(targetPos, maxDistance))
                    ExecuteOnce(MoveTo(pos.GetRelative(targetPos, -maxDistance + 50), walk));

                yield return true;
            }
        }

        /// <summary>
        ///     Creature tries to get away from target.
        /// </summary>
        /// <param name="minDistance"></param>
        /// <param name="walk"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        protected IEnumerable KeepDistance(int minDistance, bool walk = false, int timeout = 5000)
        {
            var until = _timestamp + Math.Max(0, timeout);

            while (_timestamp < until)
            {
                var pos = Creature.GetPosition();
                var targetPos = Creature.Target.GetPosition();

                if (pos.InRange(targetPos, minDistance))
                    ExecuteOnce(MoveTo(pos.GetRelative(targetPos, -(minDistance + 50)), walk));

                yield return true;
            }
        }

        /// <summary>
        ///     Attacks target creature "KnockCount" times.
        /// </summary>
        /// <returns></returns>
        protected IEnumerable Attack()
        {
            var count = 1 + (Creature.Inventory.RightHand != null
                            ? Creature.Inventory.RightHand.Info.KnockCount
                            : Creature.RaceData.KnockCount);
            return Attack(count);
        }

        /// <summary>
        ///     Attacks target creature x times.
        /// </summary>
        /// <returns></returns>
        protected IEnumerable Attack(int count, int timeout = 300000)
        {
            if (Creature.Target == null)
            {
                Reset();
                yield break;
            }

            timeout = Math2.Clamp(0, 300000, timeout);
            var until = _timestamp + timeout;

            // Each successful hit counts, attack until count or timeout is reached.
            for (var i = 0;;)
            {
                // Get skill
                var skill = Creature.Skills.ActiveSkill;
                if (skill == null && (skill = Creature.Skills.Get(SkillId.CombatMastery)) == null)
                {
                    Log.Warning("AI.Attack: Creature '{0}' doesn't have Combat Mastery.", Creature.RaceId);
                    yield break;
                }

                // Get skill handler
                var skillHandler = ChannelServer.Instance.SkillManager.GetHandler<ICombatSkill>(skill.Info.Id);
                if (skillHandler == null)
                {
                    Log.Error("AI.Attack: Skill handler not found for '{0}'.", skill.Info.Id);
                    yield break;
                }

                // Stop timeout was reached
                if (_timestamp >= until)
                    break;

                // Stop if target vanished somehow
                if (Creature.Target == null)
                    yield break;

                // Attack
                var result = skillHandler.Use(Creature, skill, Creature.Target.EntityId);
                if (result == CombatSkillResult.Okay)
                {
                    // Stop when max attack count is reached
                    if (++i >= count)
                        break;

                    yield return true;
                }
                else if (result == CombatSkillResult.OutOfRange)
                {
                    // Run to target if out of range
                    var pos = Creature.GetPosition();
                    var targetPos = Creature.Target.GetPosition();

                    //var attackRange = this.Creature.AttackRangeFor(this.Creature.Target);
                    //this.ExecuteOnce(this.RunTo(pos.GetRelative(targetPos, -attackRange + 50)));
                    ExecuteOnce(RunTo(targetPos));

                    yield return true;
                }
                else if (result == CombatSkillResult.InvalidTarget)
                {
                    // Reset if target couldn't be found.
                    Reset();
                    yield break;
                }
                else
                {
                    Log.Error("AI.Attack: Unhandled combat skill result ({0}).", result);
                    yield break;
                }
            }

            // Complete is called automatically from OnUsedSkill
        }

        /// <summary>
        ///     Attacks target with a ranged attack.
        /// </summary>
        /// <param name="timeout"></param>
        /// <returns></returns>
        protected IEnumerable RangedAttack(int timeout = 5000)
        {
            var target = Creature.Target;

            // Check active skill
            var activeSkill = Creature.Skills.ActiveSkill;
            if (activeSkill != null)
            {
                if (activeSkill.Data.Type != SkillType.RangedCombat)
                {
                    Log.Warning("AI.RangedAttack: Active skill is no ranged skill.", Creature.RaceId);
                    yield break;
                }
            }
            else
            {
                // Get skill
                activeSkill = Creature.Skills.Get(SkillId.RangedAttack);
                if (activeSkill == null)
                {
                    Log.Warning("AI.RangedAttack: Creature '{0}' doesn't have RangedAttack.", Creature.RaceId);
                    yield break;
                }

                // Get handler
                var rangedHandler = ChannelServer.Instance.SkillManager.GetHandler<RangedAttack>(activeSkill.Info.Id);

                // Start loading
                SharpMind(activeSkill.Info.Id, SharpMindStatus.Loading);

                // Prepare skill
                rangedHandler.Prepare(Creature, activeSkill, null);

                Creature.Skills.ActiveSkill = activeSkill;
                activeSkill.State = SkillState.Prepared;

                // Wait for loading to be done
                foreach (var action in Wait(activeSkill.RankData.LoadTime))
                    yield return action;

                // Call ready
                rangedHandler.Ready(Creature, activeSkill, null);
                activeSkill.State = SkillState.Ready;

                // Done loading
                SharpMind(activeSkill.Info.Id, SharpMindStatus.Loaded);
            }

            // Get combat handler for active skill
            var combatHandler = ChannelServer.Instance.SkillManager.GetHandler<ICombatSkill>(activeSkill.Info.Id);

            // Start aiming
            Creature.AimMeter.Start(target.EntityId);

            // Wait till aim is 99% or timeout is reached
            var until = _timestamp + Math.Max(0, timeout);
            var aim = 0.0;
            while (_timestamp < until && (aim = Creature.AimMeter.GetAimChance(target)) < 90)
                yield return true;

            // Cancel if 90 aim weren't reached
            if (aim < 90)
            {
                SharpMind(activeSkill.Info.Id, SharpMindStatus.Cancelling);
                Creature.Skills.CancelActiveSkill();
                Creature.AimMeter.Stop();
                yield break;
            }

            // Attack
            combatHandler.Use(Creature, activeSkill, target.EntityId);
            activeSkill.State = SkillState.Completed;

            // Complete is called automatically from OnUsedSkill
        }

        /// <summary>
        ///     Attacks with the given skill, charging it first, if it doesn't
        ///     have the given amount of stacks yet. Attacks until all stacks
        ///     have been used, or timeout is reached.
        /// </summary>
        /// <param name="skillId"></param>
        /// <param name="stacks"></param>
        /// <param name="timeout"></param>
        /// <returns></returns>
        protected IEnumerable StackAttack(SkillId skillId, int stacks = 1, int timeout = 30000)
        {
            var target = Creature.Target;
            var until = _timestamp + Math.Max(0, timeout);

            // Get handler
            var prepareHandler = ChannelServer.Instance.SkillManager.GetHandler<IPreparable>(skillId);
            var readyHandler = prepareHandler as IReadyable;
            var combatHandler = prepareHandler as ICombatSkill;

            if (prepareHandler == null || readyHandler == null || combatHandler == null)
            {
                Log.Warning(
                    "AI.StackAttack: {0}'s handler doesn't exist, or doesn't implement the necessary interfaces.",
                    skillId);
                yield break;
            }

            // Cancel active skill if it's not the one we want
            var skill = Creature.Skills.ActiveSkill;
            if (skill != null && skill.Info.Id != skillId)
                foreach (var action in CancelSkill())
                    yield return action;

            // Get skill if we don't have one yet
            if (skill == null)
            {
                // Get skill
                skill = Creature.Skills.Get(skillId);
                if (skill == null)
                {
                    Log.Warning("AI.StackAttack: Creature '{0}' doesn't have {1}.", Creature.RaceId, skillId);
                    yield break;
                }
            }

            // Stack up
            stacks = Math2.Clamp(1, skill.RankData.StackMax, stacks);
            while (skill.Stacks < stacks)
            {
                // Start loading
                SharpMind(skill.Info.Id, SharpMindStatus.Loading);

                // Prepare skill
                prepareHandler.Prepare(Creature, skill, null);

                Creature.Skills.ActiveSkill = skill;
                skill.State = SkillState.Prepared;

                // Wait for loading to be done
                foreach (var action in Wait(skill.RankData.LoadTime))
                    yield return action;

                // Call ready
                readyHandler.Ready(Creature, skill, null);
                skill.State = SkillState.Ready;

                // Done loading
                SharpMind(skill.Info.Id, SharpMindStatus.Loaded);
            }

            // Small delay
            foreach (var action in Wait(1000, 2000))
                yield return action;

            // Attack
            while (skill.Stacks > 0)
            {
                if (_timestamp >= until)
                    break;

                combatHandler.Use(Creature, skill, target.EntityId);
                yield return true;
            }

            // Cancel skill if there are left over stacks
            if (skill.Stacks != 0)
                foreach (var action in CancelSkill())
                    yield return action;
        }

        /// <summary>
        ///     Makes creature prepare given skill.
        /// </summary>
        /// <param name="skillId"></param>
        /// <returns></returns>
        protected IEnumerable PrepareSkill(SkillId skillId)
        {
            return PrepareSkill(skillId, 1);
        }

        /// <summary>
        ///     Makes creature prepare given skill.
        /// </summary>
        /// <param name="skillId"></param>
        /// <returns></returns>
        protected IEnumerable PrepareSkill(SkillId skillId, int stacks)
        {
            // Get skill
            var skill = Creature.Skills.Get(skillId);
            if (skill == null)
                yield break;

            // Cancel previous skill
            var activeSkill = Creature.Skills.ActiveSkill;
            if (activeSkill != null && activeSkill.Info.Id != skillId)
                ExecuteOnce(CancelSkill());

            stacks = Math2.Clamp(1, skill.RankData.StackMax, skill.Stacks + stacks);
            while (skill.Stacks < stacks)
            {
                // Explicit handling
                if (skillId == SkillId.WebSpinning)
                {
                    var skillHandler = ChannelServer.Instance.SkillManager.GetHandler<WebSpinning>(skillId);
                    skillHandler.Prepare(Creature, skill, null);
                    Creature.Skills.ActiveSkill = skill;
                    skillHandler.Complete(Creature, skill, null);
                    Creature.Skills.ActiveSkill = null;
                }
                else if (skillId == SkillId.DarkLord)
                {
                    var skillHandler = ChannelServer.Instance.SkillManager.GetHandler<DarkLordSkill>(skillId);
                    skillHandler.Prepare(Creature, skill, null);
                    Creature.Skills.ActiveSkill = skill;
                    skillHandler.Complete(Creature, skill, null);
                    Creature.Skills.ActiveSkill = null;
                }
                else if (skillId == SkillId.PlayingInstrument)
                {
                    var skillHandler = ChannelServer.Instance.SkillManager.GetHandler<PlayingInstrument>(skillId);
                    skillHandler.Prepare(Creature, skill, null);
                    Creature.Skills.ActiveSkill = skill;
                }
                // Try 
                // Try to handle implicitly
                else
                {
                    // Get preparable handler
                    var skillHandler = ChannelServer.Instance.SkillManager.GetHandler<IPreparable>(skillId);
                    if (skillHandler == null)
                    {
                        Log.Unimplemented("AI.PrepareSkill: Missing handler or IPreparable for '{0}'.", skillId);
                        yield break;
                    }

                    // Get readyable handler.
                    // TODO: There are skills that don't have ready, but go right to
                    //   use from Prepare. Handle somehow.
                    var readyHandler = skillHandler as IReadyable;
                    if (readyHandler == null)
                    {
                        Log.Unimplemented("AI.PrepareSkill: Missing IReadyable for '{0}'.", skillId);
                        yield break;
                    }

                    SharpMind(skillId, SharpMindStatus.Loading);

                    // Prepare skill
                    try
                    {
                        if (!skillHandler.Prepare(Creature, skill, null))
                            yield break;

                        Creature.Skills.ActiveSkill = skill;
                        skill.State = SkillState.Prepared;
                    }
                    catch (NullReferenceException)
                    {
                        Log.Warning(
                            "AI.PrepareSkill: Null ref exception while preparing '{0}', skill might have parameters.",
                            skillId);
                    }
                    catch (NotImplementedException)
                    {
                        Log.Unimplemented("AI.PrepareSkill: Skill prepare method for '{0}'.", skillId);
                    }

                    // Wait for loading to be done
                    foreach (var action in Wait(skill.RankData.LoadTime))
                        yield return action;

                    // Call ready
                    readyHandler.Ready(Creature, skill, null);
                    skill.State = SkillState.Ready;

                    SharpMind(skillId, SharpMindStatus.Loaded);
                }

                // If stacks are still 0 after preparing, we'll have to assume
                // that the skill didn't set it. We have to break the loop,
                // otherwise the AI would prepare the skill indefinitely.
                if (skill.Stacks == 0)
                    break;
            }
        }

        /// <summary>
        ///     Makes creature cancel currently loaded skill.
        /// </summary>
        /// <returns></returns>
        protected IEnumerable CancelSkill()
        {
            if (Creature.Skills.ActiveSkill != null)
            {
                SharpMind(Creature.Skills.ActiveSkill.Info.Id, SharpMindStatus.Cancelling);
                Creature.Skills.CancelActiveSkill();
            }

            yield break;
        }

        /// <summary>
        ///     Makes creature use currently loaded skill.
        /// </summary>
        /// <returns></returns>
        protected IEnumerable UseSkill()
        {
            var activeSkillId =
                Creature.Skills.ActiveSkill != null ? Creature.Skills.ActiveSkill.Info.Id : SkillId.None;

            if (activeSkillId == SkillId.None)
                yield break;

            if (activeSkillId == SkillId.Windmill)
            {
                var wmHandler = ChannelServer.Instance.SkillManager.GetHandler<Windmill>(activeSkillId);
                wmHandler.Use(Creature, Creature.Skills.ActiveSkill, 0, 0, 0);
                SharpMind(activeSkillId, SharpMindStatus.Cancelling);
            }
            else if (activeSkillId == SkillId.Stomp)
            {
                var handler = ChannelServer.Instance.SkillManager.GetHandler<Stomp>(activeSkillId);
                handler.Use(Creature, Creature.Skills.ActiveSkill, 0, 0, 0);
                SharpMind(activeSkillId, SharpMindStatus.Cancelling);
            }
            else if (activeSkillId == SkillId.GlasGhaibhleannSkill)
            {
                var pos = Creature.GetPosition();
                var targetPos = Creature.Target.GetPosition();
                var hitPos = targetPos.GetRelative(pos, -1000);
                var hitLocation = new Location(Creature.RegionId, hitPos);
                var targetAreaEntityId = hitLocation.ToLocationId();

                var handler = ChannelServer.Instance.SkillManager.GetHandler<GlasGhaibhleannSkill>(activeSkillId);
                handler.Use(Creature, Creature.Skills.ActiveSkill, targetAreaEntityId);
                SharpMind(activeSkillId, SharpMindStatus.Cancelling);
            }
            else if (activeSkillId == SkillId.Fireball)
            {
                var target = Creature.Target;

                var handler = ChannelServer.Instance.SkillManager.GetHandler<Fireball>(activeSkillId);
                handler.Use(Creature, Creature.Skills.ActiveSkill, target.EntityId, 0, 0);
                SharpMind(activeSkillId, SharpMindStatus.Cancelling);
            }
            else
            {
                Log.Unimplemented("AI.UseSkill: Skill '{0}'", activeSkillId);
            }
        }

        /// <summary>
        ///     Makes creature cancel currently loaded skill.
        /// </summary>
        /// <returns></returns>
        protected IEnumerable CompleteSkill()
        {
            if (Creature.Skills.ActiveSkill == null)
                yield break;

            var skill = Creature.Skills.ActiveSkill;
            var skillId = Creature.Skills.ActiveSkill.Info.Id;

            // Get skill handler
            var skillHandler = ChannelServer.Instance.SkillManager.GetHandler<ICompletable>(skillId);
            if (skillHandler == null)
            {
                Log.Unimplemented("AI.CompleteSkill: Missing handler or ICompletable for '{0}'.", skillId);
                yield break;
            }

            // Run complete
            try
            {
                skillHandler.Complete(Creature, skill, null);
            }
            catch (NullReferenceException)
            {
                Log.Warning("AI.CompleteSkill: Null ref exception while preparing '{0}', skill might have parameters.",
                    skillId);
            }
            catch (NotImplementedException)
            {
                Log.Unimplemented("AI.CompleteSkill: Skill complete method for '{0}'.", skillId);
            }

            // Finalize complete or ready again
            if (skill.Stacks == 0)
            {
                Creature.Skills.ActiveSkill = null;
                skill.State = SkillState.Completed;
                SharpMind(skillId, SharpMindStatus.Cancelling);
            }
            else if (skill.State != SkillState.Canceled)
            {
                skill.State = SkillState.Ready;
            }
        }

        /// <summary>
        ///     Makes creature start given skill.
        /// </summary>
        /// <param name="skillId"></param>
        /// <returns></returns>
        protected IEnumerable StartSkill(SkillId skillId)
        {
            // Get skill
            var skill = Creature.Skills.Get(skillId);
            if (skill == null)
            {
                Log.Warning("AI.StartSkill: AI '{0}' tried to start skill '{2}', that its creature '{1}' doesn't have.",
                    GetType().Name, Creature.RaceId, skillId);
                yield break;
            }

            // Get handler
            var skillHandler = ChannelServer.Instance.SkillManager.GetHandler<IStartable>(skillId);
            if (skillHandler == null)
            {
                Log.Unimplemented("AI.StartSkill: Missing handler or interface for '{0}'.", skillId);
                yield break;
            }

            // Run handler
            try
            {
                if (skillHandler is Rest)
                {
                    var restHandler = (Rest) skillHandler;
                    restHandler.Start(Creature, skill, MabiDictionary.Empty);
                }
                else
                {
                    skillHandler.Start(Creature, skill, null);
                }
            }
            catch (NullReferenceException)
            {
                Log.Warning("AI.StartSkill: Null ref exception while starting '{0}', skill might have parameters.",
                    skillId);
            }
            catch (NotImplementedException)
            {
                Log.Unimplemented("AI.StartSkill: Skill start method for '{0}'.", skillId);
            }
        }

        /// <summary>
        ///     Makes creature stop given skill.
        /// </summary>
        /// <param name="skillId"></param>
        /// <returns></returns>
        protected IEnumerable StopSkill(SkillId skillId)
        {
            // Get skill
            var skill = Creature.Skills.Get(skillId);
            if (skill == null)
            {
                Log.Warning("AI.StopSkill: AI '{0}' tried to stop skill '{2}', that its creature '{1}' doesn't have.",
                    GetType().Name, Creature.RaceId, skillId);
                yield break;
            }

            // Get handler
            var skillHandler = ChannelServer.Instance.SkillManager.GetHandler<IStoppable>(skillId);
            if (skillHandler == null)
            {
                Log.Unimplemented("AI.StopSkill: Missing handler or interface for '{0}'.", skillId);
                yield break;
            }

            // Run handler
            try
            {
                if (skillHandler is Rest)
                {
                    var restHandler = (Rest) skillHandler;
                    restHandler.Stop(Creature, skill, MabiDictionary.Empty);
                }
                else
                {
                    skillHandler.Stop(Creature, skill, null);
                }
            }
            catch (NullReferenceException)
            {
                Log.Warning("AI.StopSkill: Null ref exception while stopping '{0}', skill might have parameters.",
                    skillId);
            }
            catch (NotImplementedException)
            {
                Log.Unimplemented("AI.StopSkill: Skill stop method for '{0}'.", skillId);
            }
        }

        /// <summary>
        ///     Switches to the given weapon set.
        /// </summary>
        /// <param name="set"></param>
        /// <returns></returns>
        protected IEnumerable SwitchTo(WeaponSet set)
        {
            if (Creature.Inventory.WeaponSet == set)
                yield break;

            // Wait a moment before and after switching,
            // to let the animation play.
            var waitTime = 500;

            foreach (var action in Wait(waitTime))
                yield return action;

            Creature.Inventory.ChangeWeaponSet(set);

            foreach (var action in Wait(waitTime))
                yield return action;
        }

        /// <summary>
        ///     Changes the AI's creature's height.
        /// </summary>
        /// <param name="height"></param>
        /// <returns></returns>
        protected IEnumerable SetHeight(double height)
        {
            Creature.Height = (float) height;
            Send.CreatureBodyUpdate(Creature);

            yield break;
        }

        /// <summary>
        ///     Plays sound effect in rage of AI's creature.
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        protected IEnumerable PlaySound(string file)
        {
            Send.PlaySound(Creature, file);

            yield break;
        }

        /// <summary>
        ///     Adds stat mod to the AI's creature.
        /// </summary>
        /// <param name="stat"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        protected IEnumerable SetStat(Stat stat, float value)
        {
            switch (stat)
            {
                case Stat.Str:
                    Creature.StrBase = value;
                    break;
                case Stat.Int:
                    Creature.IntBase = value;
                    break;
                case Stat.Dex:
                    Creature.DexBase = value;
                    break;
                case Stat.Will:
                    Creature.WillBase = value;
                    break;
                case Stat.Luck:
                    Creature.LuckBase = value;
                    break;
                default:
                    Log.Warning("AI.SetState: Unhandled stat: {0}", stat);
                    break;
            }

            yield break;
        }

        /// <summary>
        ///     Changes armor in sequence, starting the first item id that
        ///     matches the current armor.
        /// </summary>
        /// <example>
        ///     itemIds = [15046, 15047, 15048, 15049, 15050]
        ///     If current armor is 15046, it's changed to 15047,
        ///     if current armor is 15047, it's changed to 15048,
        ///     and so on, until there are no more ids.
        ///     The first id needs to be the default armor, otherwise no
        ///     change will occur, since no starting point can be found.
        ///     If a creature doesn't have any armor, 0 can be used as the
        ///     default, to make it put on armor.
        ///     Duplicate item ids will not work.
        /// </example>
        /// <param name="itemIds"></param>
        protected IEnumerable SwitchArmor(params int[] itemIds)
        {
            if (itemIds == null || itemIds.Length == 0)
                throw new ArgumentException("A minimum of 1 item id is required.");

            var current = 0;
            var newItemId = -1;

            // Get current item
            var item = Creature.Inventory.GetItemAt(Pocket.Armor, 0, 0);
            if (item != null)
                current = item.Info.Id;

            // Search for next item id
            for (var i = 0; i < itemIds.Length - 1; ++i)
                if (itemIds[i] == current)
                {
                    newItemId = itemIds[i + 1];
                    break;
                }

            // No new id, current not found or end reached
            if (newItemId == -1)
                yield break;

            // Create new item
            Item newItem = null;
            if (newItemId != 0)
            {
                newItem = new Item(newItemId);
                if (item != null)
                {
                    // Use same color as the previous armor. Succubi go through
                    // more and more revealing clothes, making it look like they
                    // lose them, but the colors are variable if we don't set them.
                    newItem.Info.Color1 = item.Info.Color1;
                    newItem.Info.Color2 = item.Info.Color2;
                    newItem.Info.Color3 = item.Info.Color3;
                }
            }

            // Equip new item and remove old one
            if (item != null)
                Creature.Inventory.Remove(item);
            if (newItem != null)
                Creature.Inventory.Add(newItem, Pocket.Armor);
        }

        /// <summary>
        ///     Makes creature controlled by the AI drop gold.
        /// </summary>
        /// <param name="amount"></param>
        /// <returns></returns>
        protected IEnumerable DropGold(int amount)
        {
            var gold = Item.CreateGold(amount);
            gold.Drop(Creature.Region, Creature.GetPosition(), 200, Creature, true);

            yield break;
        }

        /// <summary>
        ///     Sets the skin color of the creature controlled by the AI.
        /// </summary>
        /// <param name="skinColor"></param>
        /// <returns></returns>
        protected IEnumerable SetSkinColor(int skinColor)
        {
            Creature.SkinColor = (byte) skinColor;

            Send.CreatureFaceUpdate(Creature);
            Send.StatUpdate(Creature, StatUpdateType.Public, Stat.SkinColor);

            yield break;
        }

        /// <summary>
        ///     Gives skill to the creature controlled by the AI.
        /// </summary>
        /// <param name="skillIds"></param>
        /// <returns></returns>
        protected IEnumerable AddSkill(SkillId skillId, SkillRank rank)
        {
            Creature.Skills.Give(skillId, rank);

            yield break;
        }

        /// <summary>
        ///     Removes skills from the creature controlled by the AI.
        /// </summary>
        /// <param name="skillIds"></param>
        /// <returns></returns>
        protected IEnumerable RemoveSkills(params SkillId[] skillIds)
        {
            foreach (var skillId in skillIds)
                Creature.Skills.RemoveSilent(skillId);

            yield break;
        }

        /// <summary>
        ///     Summons given amount of monsters of given race.
        /// </summary>
        /// <remarks>
        ///     Keeps track of the summoned monsters and only summons up to the
        ///     given amount.
        /// </remarks>
        /// <param name="raceId"></param>
        /// <param name="amount"></param>
        /// <param name="minDistance"></param>
        /// <param name="maxDistance"></param>
        /// <returns></returns>
        protected IEnumerable Summon(int raceId, int amount, int minDistance, int maxDistance)
        {
            // Max amount reached?
            var count = GetSummonCount(raceId);
            var diff = amount - count;
            if (diff <= 0)
                yield break;

            // Summon
            var pos = Creature.GetPosition();
            var rnd = _rnd;
            for (var i = 0; i < diff; ++i)
            {
                var summonPos = pos.GetRandomInRange(minDistance, maxDistance, _rnd);

                // If there's a collision between the creature's position
                // and the summon's position, the monster that is to be
                // summoned should not spawn at all.
                if (Creature.Region.Collisions.Any(pos, summonPos))
                    continue;

                var regionId = Creature.RegionId;
                var x = summonPos.X;
                var y = summonPos.Y;

                var creature = ChannelServer.Instance.World.SpawnManager.Spawn(raceId, regionId, x, y, true, true);
                creature.Finish += (_, __) => ModifySummonCount(raceId, -1);

                ModifySummonCount(raceId, +1);
            }
        }

        /// <summary>
        ///     Modifies amount of summons of race id.
        /// </summary>
        /// <param name="raceId"></param>
        /// <param name="modifier"></param>
        /// <returns></returns>
        private int ModifySummonCount(int raceId, int modifier)
        {
            int count;

            lock (_summons)
            {
                if (!_summons.TryGetValue(raceId, out count) || count <= 0)
                    count = 0;

                count += modifier;
                _summons[raceId] = count;
            }

            return count;
        }

        /// <summary>
        ///     Returns the amount of current summons for race id.
        /// </summary>
        /// <param name="raceId"></param>
        /// <returns></returns>
        private int GetSummonCount(int raceId)
        {
            int count;

            lock (_summons)
            {
                if (!_summons.TryGetValue(raceId, out count) || count <= 0)
                    count = 0;
            }

            return count;
        }

        // ------------------------------------------------------------------

        /// <summary>
        ///     Called when creature is hit.
        /// </summary>
        /// <param name="action"></param>
        public virtual void OnTargetActionHit(TargetAction action)
        {
            if (Creature.Skills.ActiveSkill != null)
                SharpMind(Creature.Skills.ActiveSkill.Info.Id, SharpMindStatus.Cancelling);

            lock (_reactions)
            {
                var state = _reactions[_state];
                var ev = AiEvent.None;
                var fallback = AiEvent.None;

                // Knock down event
                if (action.Has(TargetOptions.KnockDown) || action.Has(TargetOptions.Smash))
                {
                    // Windmill doesn't trigger the knock down event
                    if (action.AttackerSkillId != SkillId.Windmill)
                        if (action.Has(TargetOptions.Critical))
                            ev = AiEvent.CriticalKnockDown;
                        else
                            ev = AiEvent.KnockDown;
                }
                // Defense event
                else if (action.SkillId == SkillId.Defense)
                {
                    ev = AiEvent.DefenseHit;
                }
                // Magic hit event
                // Use skill ids for now, until we know more about what
                // exactly classifies as a magic hit and what doesn't.
                else if (action.AttackerSkillId >= SkillId.Lightningbolt &&
                         action.AttackerSkillId <= SkillId.Inspiration)
                {
                    ev = AiEvent.MagicHit;
                    if (action.Has(TargetOptions.Critical))
                        fallback = AiEvent.CriticalHit;
                    else
                        fallback = AiEvent.Hit;
                }
                // Hit event
                else
                {
                    if (action.Has(TargetOptions.Critical))
                        ev = AiEvent.CriticalHit;
                    else
                        ev = AiEvent.Hit;
                }

                // Try to find and execute event
                Dictionary<SkillId, Func<IEnumerable>> evs = null;
                if (state.ContainsKey(ev))
                    evs = state[ev];
                else if (state.ContainsKey(fallback))
                    evs = state[fallback];

                if (evs != null)
                    if (evs.ContainsKey(action.AttackerSkillId))
                    {
                        SwitchAction(evs[action.AttackerSkillId]);
                        return;
                    }
                    // Try general event
                    else if (evs.ContainsKey(SkillId.None))
                    {
                        SwitchAction(evs[SkillId.None]);
                        return;
                    }

                // Creature was hit, but there's no event

                // If the queue isn't cleared, the AI won't restart the
                // Aggro state, which will make it keep attacking.
                // This also causes a bug, where when you attack a
                // monster while it's attacking you with Smash,
                // it will keep attacking you with Smash, even though
                // the skill was canceled, due to the received hit.
                // The result is a really confusing situation, where
                // normal looking attacks suddenly break through Defense.
                Clear();
            }
        }

        /// <summary>
        ///     Raised from Creature.Kill when creature died,
        ///     before active skill is canceled.
        /// </summary>
        /// <param name="creature"></param>
        /// <param name="killer"></param>
        private void OnDeath(Creature creature, Creature killer)
        {
            if (Creature.Skills.ActiveSkill != null)
                SharpMind(Creature.Skills.ActiveSkill.Info.Id, SharpMindStatus.Cancelling);
        }

        /// <summary>
        ///     Called when the AI hit someone with a skill.
        /// </summary>
        /// <param name="aAction"></param>
        public void OnUsedSkill(AttackerAction aAction)
        {
            if (Creature.Skills.ActiveSkill != null)
                ExecuteOnce(CompleteSkill());
        }

        // ------------------------------------------------------------------

        protected enum AggroType
        {
            /// <summary>
            ///     Stays in Idle unless provoked
            /// </summary>
            Passive,

            /// <summary>
            ///     Goes into alert, but doesn't attack unprovoked.
            /// </summary>
            Careful,

            /// <summary>
            ///     Goes into alert and attacks if target is in battle mode.
            /// </summary>
            CarefulAggressive,

            /// <summary>
            ///     Goes straight into alert and aggro.
            /// </summary>
            Aggressive
        }

        protected enum AggroLimit
        {
            /// <summary>
            ///     Only auto aggroes if no other creature of the same race
            ///     aggroed target yet.
            /// </summary>
            One = 1,

            /// <summary>
            ///     Only auto aggroes if at most one other creature of the same
            ///     race aggroed target.
            /// </summary>
            Two,

            /// <summary>
            ///     Only auto aggroes if at most two other creatures of the same
            ///     race aggroed target.
            /// </summary>
            Three,

            /// <summary>
            ///     Auto aggroes regardless of other enemies.
            /// </summary>
            None = int.MaxValue
        }
    }

    /// <summary>
    ///     Attribute for AI scripts, to specify which races the script is for.
    /// </summary>
    public class AiScriptAttribute : Attribute
    {
        /// <summary>
        ///     New attribute
        /// </summary>
        /// <param name="names"></param>
        public AiScriptAttribute(params string[] names)
        {
            Names = names;
        }

        /// <summary>
        ///     List of AI names
        /// </summary>
        public string[] Names { get; }
    }
}