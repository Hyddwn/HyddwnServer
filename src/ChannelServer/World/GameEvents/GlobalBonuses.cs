// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using Aura.Channel.World.Entities;
using Aura.Data.Database;
using Aura.Mabi.Const;

namespace Aura.Channel.World.GameEvents
{
    public class GlobalBonusManager
    {
        private readonly List<GlobalBonus> _bonuses = new List<GlobalBonus>();
        private readonly List<GlobalDrop> _drops = new List<GlobalDrop>();
        private readonly List<GlobalFishingGround> _fishingGrounds = new List<GlobalFishingGround>();

        /// <summary>
        ///     Adds global bonus.
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="name"></param>
        /// <param name="stat"></param>
        /// <param name="multiplier"></param>
        public void AddBonus(string identifier, string name, GlobalBonusStat stat, float multiplier)
        {
            var bonus = new GlobalBonus(identifier, name, stat, multiplier);
            lock (_bonuses)
            {
                _bonuses.Add(bonus);
            }
        }

        /// <summary>
        ///     Removes all global bonuses with given event id.
        /// </summary>
        /// <param name="identifier"></param>
        public void RemoveBonuses(string identifier)
        {
            lock (_bonuses)
            {
                _bonuses.RemoveAll(a => a.Identifier == identifier);
            }
        }

        /// <summary>
        ///     Returns whether there are any bonuses for the given stat,
        ///     and if so returns the total multiplier and the names of the
        ///     events that affected it via out parameter.
        /// </summary>
        /// <param name="stat"></param>
        /// <param name="multiplier"></param>
        /// <param name="eventNames"></param>
        public bool GetBonusMultiplier(GlobalBonusStat stat, out float multiplier, out string eventNames)
        {
            multiplier = 0;
            eventNames = "";

            lock (_bonuses)
            {
                if (!_bonuses.Any(a => a.Stat == stat))
                    return false;
            }

            var names = new HashSet<string>();

            lock (_bonuses)
            {
                foreach (var bonus in _bonuses.Where(a => a.Stat == stat))
                {
                    multiplier += bonus.Multiplier;
                    if (!string.IsNullOrWhiteSpace(bonus.Name))
                        names.Add(bonus.Name);
                }
            }

            eventNames = string.Join(", ", names);

            return true;
        }

        /// <summary>
        ///     Adds global drop.
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="drop"></param>
        public void AddDrop(string identifier, GlobalDrop drop)
        {
            lock (_drops)
            {
                _drops.Add(drop);
            }
        }

        /// <summary>
        ///     Adds global drop.
        /// </summary>
        /// <param name="drop"></param>
        public void AddDrop(GlobalDrop drop)
        {
            lock (_drops)
            {
                _drops.Add(drop);
            }
        }

        /// <summary>
        ///     Removes all global drops associated with the given identifier.
        /// </summary>
        /// <param name="identifier"></param>
        public void RemoveAllDrops(string identifier)
        {
            lock (_drops)
            {
                _drops.RemoveAll(a => a.Identifier == identifier);
            }
        }

        /// <summary>
        ///     Returns list of all global drops the given creature might drop.
        /// </summary>
        /// <param name="creature"></param>
        /// <returns></returns>
        public List<DropData> GetDrops(Creature creature, Creature killer)
        {
            var result = new List<DropData>();

            lock (_drops)
            {
                result.AddRange(_drops.Where(a => a.Matches(creature, killer)).Select(a => a.Data));
            }

            return result;
        }

        /// <summary>
        ///     Adds event fishing ground.
        /// </summary>
        /// <param name="identifier"></param>
        /// <param name="drop"></param>
        public void AddFishingGround(string identifier, FishingGroundData fishingGroundData)
        {
            lock (_fishingGrounds)
            {
                _fishingGrounds.Add(new GlobalFishingGround(identifier, fishingGroundData));
            }
        }

        /// <summary>
        ///     Removes all event fishing grounds associated with the given
        ///     identifier.
        /// </summary>
        /// <param name="identifier"></param>
        public void RemoveAllFishingGrounds(string identifier)
        {
            lock (_fishingGrounds)
            {
                _fishingGrounds.RemoveAll(a => a.Identifier == identifier);
            }
        }

        /// <summary>
        ///     Returns a list of all event fishing grounds.
        /// </summary>
        /// <param name="creature"></param>
        /// <param name="fishingGroundData"></param>
        /// <returns></returns>
        public List<FishingGroundData> GetFishingGrounds()
        {
            var result = new List<FishingGroundData>();

            lock (_fishingGrounds)
            {
                result.AddRange(_fishingGrounds.Select(a => a.Data));
            }

            return result;
        }
    }

    public class GlobalBonus
    {
        public GlobalBonus(string identifier, string name, GlobalBonusStat stat, float multiplier)
        {
            Identifier = identifier;
            Name = name;
            Stat = stat;
            Multiplier = multiplier;
        }

        public string Identifier { get; }
        public string Name { get; }
        public GlobalBonusStat Stat { get; }
        public float Multiplier { get; }
    }

    public abstract class GlobalDrop
    {
        public GlobalDrop(string identifier, DropData data)
        {
            Identifier = identifier;
            Data = data;
        }

        public string Identifier { get; }
        public DropData Data { get; }

        public abstract bool Matches(Creature creature, Creature killer);
    }

    public class GlobalDropById : GlobalDrop
    {
        public GlobalDropById(string identifier, int raceId, DropData data)
            : base(identifier, data)
        {
            RaceId = raceId;
        }

        public int RaceId { get; }

        public override bool Matches(Creature creature, Creature killer)
        {
            var isRace = creature.RaceId == RaceId;
            return isRace;
        }
    }

    public class GlobalDropByMatch : GlobalDrop
    {
        public GlobalDropByMatch(string identifier, Func<Creature, Creature, bool> isMatch, DropData data)
            : base(identifier, data)
        {
            IsMatch = isMatch;
        }

        public Func<Creature, Creature, bool> IsMatch { get; }

        public override bool Matches(Creature creature, Creature killer)
        {
            return IsMatch(creature, killer);
        }
    }

    public class GlobalDropByTag : GlobalDrop
    {
        public GlobalDropByTag(string identifier, string tag, DropData data)
            : base(identifier, data)
        {
            Tag = tag;
        }

        public string Tag { get; }

        public override bool Matches(Creature creature, Creature killer)
        {
            var isTag = creature.HasTag(Tag);
            return isTag;
        }
    }

    public class GlobalDropByType : GlobalDrop
    {
        public GlobalDropByType(string identifier, GlobalDropType type, DropData data)
            : base(identifier, data)
        {
            Type = type;
        }

        public GlobalDropType Type { get; }

        public override bool Matches(Creature creature, Creature killer)
        {
            switch (Type)
            {
                case GlobalDropType.Npcs:
                    var isMonster = creature.Has(CreatureStates.Npc);
                    return isMonster;

                case GlobalDropType.Players:
                    var isPlayer = creature.IsPlayer;
                    return isPlayer;
            }

            return false;
        }
    }

    public class GlobalDropByRegion : GlobalDrop
    {
        public GlobalDropByRegion(string identifier, int regionId, DropData data)
            : base(identifier, data)
        {
            RegionId = regionId;
        }

        public int RegionId { get; }

        public override bool Matches(Creature creature, Creature killer)
        {
            return creature.RegionId == RegionId;
        }
    }

    public class GlobalFishingGround
    {
        public GlobalFishingGround(string identifier, FishingGroundData data)
        {
            Identifier = identifier;
            Data = data;
        }

        public string Identifier { get; }
        public FishingGroundData Data { get; }
    }

    public enum GlobalBonusStat
    {
        LevelUpAp,
        CombatExp,
        QuestExp,
        SkillTraining,
        ItemDropRate,
        GoldDropRate,
        GoldDropAmount,
        LuckyFinishRate
    }

    public enum GlobalDropType
    {
        Npcs,
        Players
    }
}