// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Aura.Channel.Network.Sending;
using Aura.Mabi;
using Aura.Mabi.Const;

namespace Aura.Channel.World.Entities.Creatures
{
    /// <summary>
    ///     Manages all regens for a creature.
    /// </summary>
    public class CreatureRegen
    {
        private int _minuteCounter;

        private bool _night;
        private readonly Dictionary<string, List<StatRegen>> _regenGroups;
        private readonly Dictionary<int, StatRegen> _regens;
        private float _totalToxic;

        public CreatureRegen(Creature creature)
        {
            _regens = new Dictionary<int, StatRegen>();
            _regenGroups = new Dictionary<string, List<StatRegen>>();
            Creature = creature;
        }

        public Creature Creature { get; }

        /// <summary>
        ///     Adds regen and returns the new object.
        ///     Sends stat update.
        /// </summary>
        /// <param name="stat"></param>
        /// <param name="change"></param>
        /// <param name="max"></param>
        /// <param name="duration">Duration in milliseconds, -1 (default) for constant.</param>
        /// <returns></returns>
        public StatRegen Add(Stat stat, float change, float max, int duration = -1)
        {
            var regen = new StatRegen(stat, change, max, duration);

            lock (_regens)
            {
                _regens.Add(regen.Id, regen);
            }

            // Only send updates if the creature is actually in-game already.
            if (Creature.Region != Region.Limbo)
            {
                Send.NewRegens(Creature, StatUpdateType.Private, regen);
                if (regen.Stat >= Stat.Life && regen.Stat <= Stat.LifeMaxMod)
                    Send.NewRegens(Creature, StatUpdateType.Public, regen);
            }

            return regen;
        }

        /// <summary>
        ///     Adds regen, sends stat update, saves a reference in the group,
        ///     and returns the new regen object.
        /// </summary>
        /// <param name="group"></param>
        /// <param name="stat"></param>
        /// <param name="change"></param>
        /// <param name="max"></param>
        /// <param name="duration">Duration in milliseconds, -1 (default) for constant.</param>
        /// <returns></returns>
        public StatRegen Add(string group, Stat stat, float change, float max, int duration = -1)
        {
            var regen = Add(stat, change, max, duration);

            lock (_regenGroups)
            {
                if (!_regenGroups.ContainsKey(group))
                    _regenGroups[group] = new List<StatRegen>();

                _regenGroups[group].Add(regen);
            }

            return regen;
        }

        /// <summary>
        ///     Returns true if there are any regens fort he given group name.
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public bool Has(string group)
        {
            lock (_regenGroups)
            {
                return _regenGroups.ContainsKey(group) && _regenGroups[group].Count != 0;
            }
        }

        /// <summary>
        ///     Removes regen by id, returns false if regen didn't exist.
        ///     Sends stat update if successful.
        /// </summary>
        /// <param name="regen"></param>
        /// <returns></returns>
        public bool Remove(StatRegen regen)
        {
            return Remove(regen.Id);
        }

        /// <summary>
        ///     Removes every regen in the group, returns false if the group
        ///     doesn't exist.
        /// </summary>
        /// <param name="group"></param>
        /// <returns></returns>
        public bool Remove(string group)
        {
            lock (_regenGroups)
            {
                if (!_regenGroups.ContainsKey(group))
                    return false;

                // Cretate copy, the groups are modified from Remove
                var regens = _regenGroups[group].ToArray();
                foreach (var regen in regens)
                    Remove(regen.Id);

                _regenGroups.Remove(group);
            }

            return true;
        }

        /// <summary>
        ///     Removes regen by id, returns false if regen didn't exist.
        ///     Sends stat update if successful.
        /// </summary>
        /// <param name="regenId"></param>
        /// <returns></returns>
        public bool Remove(int regenId)
        {
            StatRegen regen;

            // Remove from regens
            lock (_regens)
            {
                if (!_regens.TryGetValue(regenId, out regen))
                    return false;

                _regens.Remove(regenId);
            }

            // Remove from groups
            lock (_regenGroups)
            {
                foreach (var group in _regenGroups)
                    group.Value.Remove(regen);
            }

            // Always send private update, only send public if stat is
            // related to life.
            // TODO: When removing the regen, the stat should be updated.
            Send.RemoveRegens(Creature, StatUpdateType.Private, regen);
            if (regen.Stat >= Stat.Life && regen.Stat <= Stat.LifeMaxMod)
                Send.RemoveRegens(Creature, StatUpdateType.Public, regen);

            return true;
        }

        /// <summary>
        ///     Applies regens to creature.
        /// </summary>
        /// <remarks>
        ///     (Should be) called once a second.
        ///     - Hunger doesn't go beyond 50% of max stamina.
        ///     - Stamina regens at 20% efficiency from StaminaHunger onwards.
        /// </remarks>
        public void OnSecondsTimeTick(ErinnTime time)
        {
            if (Creature.Region == Region.Limbo || Creature.IsDead)
                return;

            // Recover from knock back/down after stun ended
            if (Creature.WasKnockedBack && !Creature.IsStunned)
            {
                Send.RiseFromTheDead(Creature);
                Creature.WasKnockedBack = false;
            }

            lock (_regens)
            {
                var toRemove = new List<int>();

                foreach (var regen in _regens.Values)
                {
                    if (regen.TimeLeft == 0)
                    {
                        toRemove.Add(regen.Id);
                        continue;
                    }

                    switch (regen.Stat)
                    {
                        case Stat.Life:
                            Creature.Life += regen.Change;
                            break;
                        case Stat.Mana:
                            Creature.Mana += regen.Change;
                            break;
                        case Stat.Stamina:
                            // Only positve regens are affected by the hunger multiplicator.
                            Creature.Stamina +=
                                regen.Change * (regen.Change > 0 ? Creature.StaminaRegenMultiplicator : 1);
                            break;
                        case Stat.Hunger:
                            // Regen can't lower hunger below a certain amount.
                            Creature.Hunger += regen.Change;
                            if (Creature.Hunger > Creature.StaminaMax / 2)
                                Creature.Hunger = Creature.StaminaMax / 2;
                            break;
                        case Stat.LifeInjured:
                            Creature.Injuries -= regen.Change;
                            break;
                    }
                }

                foreach (var id in toRemove)
                    Remove(id);
            }

            // Add additional mana regen at night
            if (_night != time.IsNight)
            {
                if (time.IsNight)
                    Add("NightMana", Stat.Mana, 0.1f, Creature.ManaMax);
                else
                    Remove("NightMana");

                _night = time.IsNight;
            }

            UpdateToxicity();
        }

        /// <summary>
        ///     Called once per second to update toxicity.
        /// </summary>
        private void UpdateToxicity()
        {
            var toxicStr = Creature.ToxicStr;
            var toxicInt = Creature.ToxicInt;
            var toxicDex = Creature.ToxicDex;
            var toxicWill = Creature.ToxicWill;
            var toxicLuck = Creature.ToxicLuck;
            var total = toxicStr + toxicInt + toxicDex + toxicWill + toxicLuck;
            var update = false;

            if (total != 0 && !Creature.Conditions.Has(ConditionsA.PotionPoisoning))
                Creature.Conditions.Activate(ConditionsA.PotionPoisoning);

            if (_minuteCounter++ == 60)
            {
                var prevTotalToxic = _totalToxic;
                _minuteCounter = 0;
                _totalToxic = total;
                update = prevTotalToxic != 0;

                if (prevTotalToxic != 0 && _totalToxic == 0)
                    Creature.Conditions.Deactivate(ConditionsA.PotionPoisoning);
            }

            // Recovery:
            // - 2 Toxic / second
            // - 2 ToxicX / minute
            Creature.Toxic = Math.Min(0, Creature.Toxic + 2);
            Creature.ToxicStr = Math.Min(0, Creature.ToxicStr + 2 / 60f);
            Creature.ToxicInt = Math.Min(0, Creature.ToxicInt + 2 / 60f);
            Creature.ToxicDex = Math.Min(0, Creature.ToxicDex + 2 / 60f);
            Creature.ToxicWill = Math.Min(0, Creature.ToxicWill + 2 / 60f);
            Creature.ToxicLuck = Math.Min(0, Creature.ToxicLuck + 2 / 60f);

            // Officials apparently send an update every minute while any
            // toxic value is greater than 0. We'll limit it to the actual
            // stats, no reason to send null values.
            if (update)
                Send.StatUpdate(Creature, StatUpdateType.Private, Stat.ToxicStr, Stat.ToxicInt, Stat.ToxicDex,
                    Stat.ToxicWill, Stat.ToxicLuck);
        }

        /// <summary>
        ///     Returns new list of all active regens.
        /// </summary>
        public ICollection<StatRegen> GetList()
        {
            lock (_regens)
            {
                return _regens.Values.ToArray();
            }
        }

        /// <summary>
        ///     Returns new list of all active regens available to the public.
        /// </summary>
        public ICollection<StatRegen> GetPublicList()
        {
            lock (_regens)
            {
                return _regens.Values.Where(a => a.Stat == Stat.Life).ToArray();
            }
        }
    }

    /// <summary>
    ///     A regen changes a stat by a specified amount each second.
    /// </summary>
    public class StatRegen
    {
        private static int _id;

        /// <summary>
        ///     Creates new regen.
        /// </summary>
        /// <param name="stat"></param>
        /// <param name="change"></param>
        /// <param name="max"></param>
        /// <param name="duration">Duration in milliseconds, -1 (default) for constant.</param>
        public StatRegen(Stat stat, float change, float max, int duration = -1)
        {
            Id = Interlocked.Increment(ref _id);
            Stat = stat;
            Change = change;
            Max = max;
            Duration = duration;
            Started = DateTime.Now;
        }

        /// <summary>
        ///     Unique id of the regen
        /// </summary>
        public int Id { get; protected set; }

        /// <summary>
        ///     Stat to be modified
        /// </summary>
        public Stat Stat { get; protected set; }

        /// <summary>
        ///     Change per second
        /// </summary>
        public float Change { get; set; }

        /// <summary>
        ///     Max value of the stat.
        /// </summary>
        /// <remarks>
        ///     This is always the max, negative regens don't need a 0 here.
        /// </remarks>
        public float Max { get; set; }

        /// <summary>
        ///     When the regen was started
        /// </summary>
        public DateTime Started { get; protected set; }

        /// <summary>
        ///     Duration in ms.
        /// </summary>
        public int Duration { get; protected set; }

        /// <summary>
        ///     How much ms are left until the regen ends
        /// </summary>
        public int TimeLeft
        {
            get
            {
                if (Duration == -1)
                    return -1;

                var passed = (int) (DateTime.Now - Started).TotalMilliseconds;

                if (passed > Duration)
                    return 0;
                return Duration - passed;
            }
        }
    }
}