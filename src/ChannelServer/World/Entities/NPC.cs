// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Linq;
using System.Threading;
using Aura.Channel.Network.Sending;
using Aura.Channel.Scripting.Scripts;
using Aura.Channel.World.GameEvents;
using Aura.Data;
using Aura.Data.Database;
using Aura.Mabi.Const;
using Aura.Shared.Util;

namespace Aura.Channel.World.Entities
{
    public class NPC : Creature
    {
        /// <summary>
        ///     Time in seconds it takes a creature (i.e. a monster)
        ///     to disappear after being finished.
        /// </summary>
        public const int DisappearDelay = 20;

        /// <summary>
        ///     Time in seconds it takes a creature (i.e. a monster)
        ///     to disappear after dying, even if it's not finished.
        /// </summary>
        public const int DisappearDelayDeath = 60;

        /// <summary>
        ///     Unique entity id increased and used for each NPC.
        /// </summary>
        private static long _npcId = MabiId.Npcs;

        /// <summary>
        ///     Creates new NPC
        /// </summary>
        public NPC()
        {
            EntityId = GetNewNpcEntityId();

            // Some default values to prevent errors
            Name = "_undefined";
            RaceId = 190140; // Wood dummy
            Height = Weight = Upper = Lower = 1;
            RegionId = 0;
            Life = LifeMaxBase = 1000;
            Color1 = Color2 = Color3 = 0x808080;
            GiftWeights = new GiftWeightInfo();
        }

        /// <summary>
        ///     Creates new NPC and loads defaults for race.
        /// </summary>
        /// <param name="raceId"></param>
        public NPC(int raceId)
            : this()
        {
            RaceId = raceId;
            LoadDefault();

            // Technically the following would belong in LoadDefault,
            // but NPCs in NPC scripts are loaded a little weird,
            // so we only load the following for NPCs who's race id
            // we get in advance.

            var rnd = RandomProvider.Get();

            // Set some base information
            Name = RaceData.Name;
            Color1 = RaceData.Color1;
            Color2 = RaceData.Color2;
            Color3 = RaceData.Color3;
            Height = (float) (RaceData.SizeMin + rnd.NextDouble() * (RaceData.SizeMax - RaceData.SizeMin));
            Life = LifeMaxBase = RaceData.Life;
            Mana = ManaMaxBase = RaceData.Mana;
            Stamina = StaminaMaxBase = RaceData.Stamina;
            State = (CreatureStates) RaceData.DefaultState;
            Direction = (byte) rnd.Next(256);

            // Set drops
            Drops.GoldMin = RaceData.GoldMin;
            Drops.GoldMax = RaceData.GoldMax;
            Drops.Add(RaceData.Drops);

            // Give skills
            foreach (var skill in RaceData.Skills)
                Skills.Add((SkillId) skill.SkillId, (SkillRank) skill.Rank, RaceId);

            // Equipment
            foreach (var itemData in RaceData.Equip)
            {
                var item = new Item(itemData.GetRandomId(rnd));
                if (itemData.Color1s.Count > 0) item.Info.Color1 = itemData.GetRandomColor1(rnd);
                if (itemData.Color2s.Count > 0) item.Info.Color2 = itemData.GetRandomColor2(rnd);
                if (itemData.Color3s.Count > 0) item.Info.Color3 = itemData.GetRandomColor3(rnd);

                var pocket = (Pocket) itemData.Pocket;
                if (pocket != Pocket.None)
                    Inventory.Add(item, pocket);
            }

            // Face
            if (RaceData.Face.EyeColors.Count > 0) EyeColor = (byte) RaceData.Face.GetRandomEyeColor(rnd);
            if (RaceData.Face.EyeTypes.Count > 0) EyeType = (short) RaceData.Face.GetRandomEyeType(rnd);
            if (RaceData.Face.MouthTypes.Count > 0) MouthType = (byte) RaceData.Face.GetRandomMouthType(rnd);
            if (RaceData.Face.SkinColors.Count > 0) SkinColor = (byte) RaceData.Face.GetRandomSkinColor(rnd);

            // Set AI
            if (!string.IsNullOrWhiteSpace(RaceData.AI) && RaceData.AI != "none")
            {
                AI = ChannelServer.Instance.ScriptManager.AiScripts.CreateAi(RaceData.AI, this);
                if (AI == null)
                    Log.Warning("ScriptManager.Spawn: Missing AI '{0}' for '{1}'.", RaceData.AI, RaceId);
            }
        }

        /// <summary>
        ///     Creates new NPC from actor data.
        /// </summary>
        /// <param name="actorData"></param>
        public NPC(ActorData actorData)
            : this(actorData.RaceId)
        {
            if (actorData.HasColors)
            {
                Color1 = actorData.Color1;
                Color2 = actorData.Color2;
                Color3 = actorData.Color3;
            }

            Height = actorData.Height;
            Weight = actorData.Weight;
            Upper = actorData.Upper;
            Lower = actorData.Lower;
            EyeColor = (byte) actorData.EyeColor;
            EyeType = (short) actorData.EyeType;
            MouthType = (byte) actorData.MouthType;
            SkinColor = (byte) actorData.SkinColor;

            Titles.SelectedTitle = (ushort) actorData.Title;
            Age = (short) actorData.Age;
            Level = (short) actorData.Level;
            AbilityPoints = (short) actorData.AP;
            LifeMaxBase = actorData.Life;
            ManaMaxBase = actorData.Mana;
            StaminaMaxBase = actorData.Stamina;
            StrBase = actorData.Str;
            IntBase = actorData.Int;
            DexBase = actorData.Dex;
            WillBase = actorData.Will;
            LuckBase = actorData.Luck;

            // Hair and Face
            if (actorData.FaceItemId != 0)
            {
                var face = new Item(actorData.FaceItemId);
                face.Info.Color1 = (byte) actorData.SkinColor;
                Inventory.Add(face, Pocket.Face);
            }
            if (actorData.HairItemId != 0)
            {
                var hair = new Item(actorData.HairItemId);
                hair.Info.Color1 = actorData.HairColor;
                Inventory.Add(hair, Pocket.Hair);
            }

            // Items
            foreach (var itemData in actorData.Items)
            {
                var item = new Item(itemData.ItemId);
                item.Info.State = (byte) itemData.State;

                item.Info.Amount = (ushort) itemData.Amount;
                if (item.Data.StackType != StackType.Sac && item.Info.Amount < 1)
                    item.Info.Amount = 1;

                if (itemData.HasColors)
                {
                    item.Info.Color1 = itemData.Color1;
                    item.Info.Color2 = itemData.Color2;
                    item.Info.Color3 = itemData.Color3;
                }

                var pocket = itemData.Pocket;
                if (pocket != Pocket.None)
                    Inventory.Add(item, pocket);
            }

            // Skills
            foreach (var skillData in actorData.Skills)
                Skills.Add(skillData.SkillId, skillData.Rank, RaceId);

            // Max out after skills and items were added (bonuses)
            Life = LifeMax;
            Mana = ManaMax;
            Stamina = StaminaMax;
        }

        /// <summary>
        ///     Type of the NpcScript used by the NPC.
        /// </summary>
        public Type ScriptType { get; set; }

        /// <summary>
        ///     AI controlling the NPC
        /// </summary>
        public AiScript AI { get; set; }

        /// <summary>
        ///     Creature spawn id, used for respawning.
        /// </summary>
        public int SpawnId { get; set; }

        /// <summary>
        ///     NPCs preferences regarding gifts.
        /// </summary>
        public GiftWeightInfo GiftWeights { get; set; }

        /// <summary>
        ///     Location the NPC was spawned at.
        /// </summary>
        public Location SpawnLocation { get; set; }

        /// <summary>
        ///     Custom portrait in dialog.
        /// </summary>
        public string DialogPortrait { get; set; }

        /// <summary>
        ///     Returns a new, unused entity id in the NPC range.
        /// </summary>
        public static long GetNewNpcEntityId()
        {
            return Interlocked.Increment(ref _npcId);
        }

        /// <summary>
        ///     Disposes AI.
        /// </summary>
        public override void Dispose()
        {
            base.Dispose();

            if (AI != null)
                AI.Dispose();
        }

        /// <summary>
        ///     Moves NPC to target location and adds it to the region.
        ///     Returns false if region doesn't exist.
        /// </summary>
        /// <param name="regionId"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns></returns>
        public override bool Warp(int regionId, int x, int y)
        {
            var region = ChannelServer.Instance.World.GetRegion(regionId);
            if (region == null)
            {
                Log.Error("NPC.Warp: Region '{0}' doesn't exist.", regionId);
                return false;
            }

            RemoveFromRegion();
            SetLocation(regionId, x, y);

            region.AddCreature(this);

            return true;
        }

        /// <summary>
        ///     Like <see cref="Warp" />, except it sends a screen flash
        ///     and sound effect to the departing region and arriving region.
        /// </summary>
        /// <param name="regionId"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <remarks>
        ///     Ideal for NPCs like Tarlach. Be careful not to "double flash"
        ///     if you're swapping two NPCs. Only ONE of the NPCs needs to use this method,
        ///     the other can use the regular <see cref="Warp" />.
        /// </remarks>
        /// <returns></returns>
        public bool WarpFlash(int regionId, int x, int y)
        {
            // "Departing" effect
            if (Region != Region.Limbo)
            {
                Send.Effect(this, Effect.ScreenFlash, 3000, 0);
                Send.PlaySound(this, "data/sound/Tarlach_change.wav");
            }

            if (!Warp(regionId, x, y))
                return false;

            // "Arriving" effect
            Send.Effect(this, Effect.ScreenFlash, 3000, 0);
            Send.PlaySound(this, "data/sound/Tarlach_change.wav");

            return true;
        }

        /// <summary>
        ///     Returns whether the NPC can target the given creature.
        /// </summary>
        /// <param name="creature"></param>
        /// <returns></returns>
        public override bool CanTarget(Creature creature)
        {
            if (!base.CanTarget(creature))
                return false;

            // Named NPCs (normal dialog ones) can't be targeted.
            // Important because AIs target /pc/ and most NPCs are humans.
            if (creature.Has(CreatureStates.NamedNpc))
                return false;

            return true;
        }

        /// <summary>
        ///     Kills NPC, rewarding the killer.
        /// </summary>
        /// <param name="killer"></param>
        public override bool Kill(Creature killer)
        {
            if (!base.Kill(killer))
            {
                DisappearTime = DateTime.Now.AddSeconds(DisappearDelayDeath);
                return false;
            }

            DisappearTime = DateTime.Now.AddSeconds(DisappearDelay);

            if (killer == null)
                return true;

            // Prepare exp
            var exp = (long) (RaceData.Exp * ChannelServer.Instance.Conf.World.ExpRate);
            var expRule = killer.Party.ExpRule;
            var expMessage = "+{0} EXP";

            // Add global bonus
            float bonusMultiplier;
            string bonuses;
            if (ChannelServer.Instance.GameEventManager.GlobalBonuses.GetBonusMultiplier(GlobalBonusStat.CombatExp,
                out bonusMultiplier, out bonuses))
                exp = (long) (exp * bonusMultiplier);

            if (!string.IsNullOrWhiteSpace(bonuses))
                expMessage += " (" + bonuses + ")";

            // Give exp
            if (!killer.IsInParty || expRule == PartyExpSharing.AllToFinish)
            {
                killer.GiveExp(exp);
                Send.CombatMessage(killer, expMessage, exp);
            }
            else
            {
                var members = killer.Party.GetMembers();
                var eaExp = 0L;
                var killerExp = 0L;
                var killerPos = killer.GetPosition();

                // Apply optional exp bonus
                if (members.Length > 1)
                {
                    var extra = members.Length - 1;
                    var bonus = ChannelServer.Instance.Conf.World.PartyExpBonus;

                    exp += (long) (exp * (extra * bonus / 100f));
                }

                // Official simply ALWAYS divides by party member total,
                // even if they cannot recieve the experience.
                if (expRule == PartyExpSharing.Equal)
                {
                    eaExp = exp / members.Length;
                    killerExp = eaExp;
                }
                else if (expRule == PartyExpSharing.MoreToFinish)
                {
                    exp /= 2;
                    eaExp = exp / members.Length;
                    killerExp = exp;
                }

                // Killer's exp
                killer.GiveExp(killerExp);
                Send.CombatMessage(killer, expMessage, killerExp);

                // Exp for members in range of killer, the range is unofficial
                foreach (var member in members.Where(a => a != killer && a.GetPosition().InRange(killerPos, 3000)))
                {
                    member.GiveExp(eaExp);
                    Send.CombatMessage(member, expMessage, eaExp);
                }
            }

            return true;
        }

        /// <summary>
        ///     NPCs may survive randomly.
        /// </summary>
        /// <remarks>
        ///     http://wiki.mabinogiworld.com/view/Stats#Life
        ///     More Will supposedly increases the chance. Unknown if this
        ///     applies to players as well. Before certain Gs, NPCs weren't
        ///     able to survive attacks under any circumstances.
        /// </remarks>
        /// <param name="damage"></param>
        /// <param name="from"></param>
        /// <param name="lifeBefore"></param>
        /// <returns></returns>
        protected override bool ShouldSurvive(float damage, Creature from, float lifeBefore)
        {
            // No surviving once you're in deadly
            if (lifeBefore < 0)
                return false;

            if (!AuraData.FeaturesDb.IsEnabled("DeadlyNPCs"))
                return false;

            // Chance = Will/10, capped at 50%
            // (i.e 80 Will = 8%, 500+ Will = 50%)
            // Actual formula unknown
            var chance = Math.Min(50, Will / 10);
            return RandomProvider.Get().Next(101) < chance;
        }

        /// <summary>
        ///     Returns how well the NPC remembers the other creature.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int GetMemory(Creature other)
        {
            // Get NPC memory and last change date
            var memory = other.Vars.Perm["npc_memory_" + Name] ?? 0;
            var change = other.Vars.Perm["npc_memory_change_" + Name];

            // Reduce memory by 1 each day
            if (change != null && memory > 0)
            {
                TimeSpan diff = DateTime.Now - change;
                memory = Math.Max(0, memory - Math.Floor(diff.TotalDays));
            }

            return (int) memory;
        }

        /// <summary>
        ///     Modifies how well the NPC remembers the other creature.
        /// </summary>
        /// <param name="other"></param>
        /// <param name="value"></param>
        /// <returns>New memory value</returns>
        public int SetMemory(Creature other, int value)
        {
            value = Math.Max(0, value);

            other.Vars.Perm["npc_memory_" + Name] = value;
            other.Vars.Perm["npc_memory_change_" + Name] = DateTime.Now;

            return value;
        }

        /// <summary>
        ///     Sets how well the NPC remembers the other creature.
        /// </summary>
        /// <param name="other"></param>
        /// <param name="value"></param>
        /// <returns>New memory value</returns>
        public int ModifyMemory(Creature other, int value)
        {
            return SetMemory(other, GetMemory(other) + value);
        }

        /// <summary>
        ///     Returns favor of the NPC towards the other creature.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int GetFavor(Creature other)
        {
            // Get NPC favor and last change date
            var favor = other.Vars.Perm["npc_favor_" + Name] ?? 0;
            var change = other.Vars.Perm["npc_favor_change_" + Name];

            // Reduce favor by 1 each hour
            if (change != null && favor > 0)
            {
                TimeSpan diff = DateTime.Now - change;
                favor = Math.Max(0, favor - Math.Floor(diff.TotalHours));
            }

            return (int) favor;
        }

        /// <summary>
        ///     Sets favor of the NPC towards the other creature.
        /// </summary>
        /// <param name="other"></param>
        /// <param name="value"></param>
        /// <returns>New favor value</returns>
        public int SetFavor(Creature other, int value)
        {
            other.Vars.Perm["npc_favor_" + Name] = value;
            other.Vars.Perm["npc_favor_change_" + Name] = DateTime.Now;

            return value;
        }

        /// <summary>
        ///     Modifies favor of the NPC towards the other creature.
        /// </summary>
        /// <param name="other"></param>
        /// <param name="value"></param>
        /// <returns>New favor value</returns>
        public int ModifyFavor(Creature other, int value)
        {
            return SetFavor(other, GetFavor(other) + value);
        }

        /// <summary>
        ///     Gets how much the other creature is stressing the NPC.
        /// </summary>
        /// <param name="other"></param>
        /// <returns></returns>
        public int GetStress(Creature other)
        {
            // Get NPC stress and last change date
            var stress = other.Vars.Perm["npc_stress_" + Name] ?? 0;
            var change = other.Vars.Perm["npc_stress_change_" + Name];

            // Reduce stress by 1 each minute
            if (change != null && stress > 0)
            {
                TimeSpan diff = DateTime.Now - change;
                stress = Math.Max(0, stress - Math.Floor(diff.TotalMinutes));
            }

            return (int) stress;
        }

        /// <summary>
        ///     Sets how much the other creature is stressing the NPC.
        /// </summary>
        /// <param name="other"></param>
        /// <param name="value"></param>
        /// <returns>New stress value</returns>
        public int SetStress(Creature other, int value)
        {
            value = Math.Max(0, value);

            other.Vars.Perm["npc_stress_" + Name] = value;
            other.Vars.Perm["npc_stress_change_" + Name] = DateTime.Now;

            return value;
        }

        /// <summary>
        ///     Modifies how much the other creature is stressing the NPC.
        /// </summary>
        /// <param name="other"></param>
        /// <param name="value"></param>
        /// <returns>New stress value</returns>
        public int ModifyStress(Creature other, int value)
        {
            return SetStress(other, GetStress(other) + value);
        }

        /// <summary>
        ///     Aggroes target, setting target and putting creature in battle stance.
        /// </summary>
        /// <param name="creature"></param>
        public override void Aggro(Creature target)
        {
            if (AI == null)
                return;

            // Aggro attacker if there is not current target,
            // or if there is a target but it's not a player, and the attacker is one,
            // or if the current target is not aggroed yet.
            if (Target == null || Target != null && target != null && !Target.IsPlayer && target.IsPlayer ||
                AI.State != AiScript.AiState.Aggro)
                AI.AggroCreature(target);
        }

        /// <summary>
        ///     Sets SpawnLocation and places NPC in region.
        /// </summary>
        /// <param name="regionId"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <returns>Returns false if NPC is spawned already or region doesn't exist.</returns>
        public bool Spawn(int regionId, int x, int y)
        {
            // Already spawned
            if (Region != Region.Limbo)
            {
                Log.Error("NPC.Spawn: Failed to spawn '{0}', it was spawned already.", RaceId, RegionId);
                return false;
            }

            // Save spawn location
            SpawnLocation = new Location(RegionId, x, y);

            // Warp to spawn point
            if (!Warp(regionId, x, y))
            {
                Log.Error("NPC.Spawn: Failed to spawn '{0}', region '{1}' doesn't exist.", RaceId, RegionId);
                return false;
            }

            return true;
        }

        /// <summary>
        ///     TODO: Move somewhere? =/
        /// </summary>
        public class GiftWeightInfo
        {
            public float Adult { get; set; }
            public float Anime { get; set; }
            public float Beauty { get; set; }
            public float Individuality { get; set; }
            public float Luxury { get; set; }
            public float Maniac { get; set; }
            public float Meaning { get; set; }
            public float Rarity { get; set; }
            public float Sexy { get; set; }
            public float Toughness { get; set; }
            public float Utility { get; set; }

            public int CalculateScore(Item gift)
            {
                var score = 0f;

                var taste = gift.Data.Taste;

                score += Adult * taste.Adult;
                score += Anime * taste.Anime;
                score += Beauty * taste.Beauty;
                score += Individuality * taste.Individuality;
                score += Luxury * taste.Luxury;
                score += Maniac * taste.Maniac;
                score += Meaning * taste.Meaning;
                score += Rarity * taste.Rarity;
                score += Sexy * taste.Sexy;
                score += Toughness * taste.Toughness;
                score += Utility * taste.Utility;

                score /= 8;

                return (int) score;
            }
        }
    }
}