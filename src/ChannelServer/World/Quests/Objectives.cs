// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Linq;
using Aura.Channel.World.Entities;
using Aura.Data;
using Aura.Mabi;
using Aura.Mabi.Const;

namespace Aura.Channel.World.Quests
{
    public abstract class QuestObjective
    {
        protected QuestObjective(int amount)
        {
            MetaData = new MabiDictionary();
            Amount = amount;
        }

        public string Ident { get; set; }
        public string Description { get; set; }

        public int Amount { get; set; }

        public int RegionId { get; set; }
        public int X { get; set; }
        public int Y { get; set; }

        public MabiDictionary MetaData { get; protected set; }

        public abstract ObjectiveType Type { get; }
    }

    /// <summary>
    ///     Objective to kill creatures of a race type.
    /// </summary>
    public class QuestObjectiveKill : QuestObjective
    {
        public QuestObjectiveKill(int amount, params string[] raceTypes)
            : base(amount)
        {
            RaceTypes = raceTypes;

            MetaData.SetString("TGTSID", string.Join("|", raceTypes));
            MetaData.SetInt("TARGETCOUNT", amount);
            MetaData.SetShort("TGTCLS", 0);
        }

        public override ObjectiveType Type => ObjectiveType.Kill;

        public string[] RaceTypes { get; set; }

        /// <summary>
        ///     Returns true if creature matches one of the race types.
        /// </summary>
        /// <param name="killedCreature"></param>
        /// <returns></returns>
        public bool Check(Creature killedCreature)
        {
            return RaceTypes.Any(type => killedCreature.RaceData.HasTag(type));
        }
    }

    /// <summary>
    ///     Objective to collect a certain item.
    /// </summary>
    public class QuestObjectiveCollect : QuestObjective
    {
        public QuestObjectiveCollect(int itemId, int amount)
            : base(amount)
        {
            ItemId = itemId;
            Amount = amount;

            MetaData.SetInt("TARGETITEM", itemId);
            MetaData.SetInt("TARGETCOUNT", amount);
            MetaData.SetInt("QO_FLAG", 1);
        }

        public override ObjectiveType Type => ObjectiveType.Collect;

        public int ItemId { get; set; }
    }

    /// <summary>
    ///     Objective to talk to a specific NPC.
    /// </summary>
    public class QuestObjectiveTalk : QuestObjective
    {
        public QuestObjectiveTalk(string npcName)
            : base(1)
        {
            Name = npcName;

            MetaData.SetString("TARGECHAR", npcName);
            MetaData.SetInt("TARGETCOUNT", 1);
        }

        public override ObjectiveType Type => ObjectiveType.Talk;

        public string Name { get; set; }
    }

    /// <summary>
    ///     Objective to deliver something to a specific NPC.
    /// </summary>
    /// <remarks>
    ///     The item is automatically given to the player on quest start,
    ///     if this is the first quest objective.
    /// </remarks>
    public class QuestObjectiveDeliver : QuestObjective
    {
        public QuestObjectiveDeliver(int itemId, int amount, string npcName)
            : base(amount)
        {
            ItemId = itemId;
            NpcName = npcName;

            MetaData.SetString("TARGECHAR", NpcName);
            MetaData.SetInt("TARGETCOUNT", Amount);
            MetaData.SetInt("TARGETITEM", ItemId);
        }

        public override ObjectiveType Type => ObjectiveType.Deliver;

        public int ItemId { get; set; }
        public string NpcName { get; set; }
    }

    /// <summary>
    ///     Objective to reach a rank in a certain skill.
    /// </summary>
    public class QuestObjectiveReachRank : QuestObjective
    {
        public QuestObjectiveReachRank(SkillId skillId, SkillRank rank)
            : base(1)
        {
            Id = skillId;
            Rank = rank;

            MetaData.SetUShort("TGTSKL", (ushort) skillId);
            MetaData.SetShort("TGTLVL", (short) rank);
            MetaData.SetInt("TARGETCOUNT", 1);
        }

        public override ObjectiveType Type => ObjectiveType.ReachRank;

        public SkillId Id { get; set; }
        public SkillRank Rank { get; set; }
    }

    /// <summary>
    ///     Objective to reach a certain level.
    /// </summary>
    public class QuestObjectiveReachLevel : QuestObjective
    {
        public QuestObjectiveReachLevel(int level)
            : base(level)
        {
            MetaData.SetShort("TGTLVL", (short) level);
            MetaData.SetInt("TARGETCOUNT", 1);
        }

        public override ObjectiveType Type => ObjectiveType.ReachLevel;
    }

    /// <summary>
    ///     Objective to get a certain keyword.
    /// </summary>
    public class QuestObjectiveGetKeyword : QuestObjective
    {
        public QuestObjectiveGetKeyword(string keyword)
            : base(1)
        {
            var keywordData = AuraData.KeywordDb.Find(keyword);
            if (keywordData == null)
                throw new ArgumentException("Keyword '" + keyword + "' not found.");

            KeywordId = keywordData.Id;
            MetaData.SetInt("TGTKEYWORD", KeywordId); // Unofficial
            MetaData.SetInt("TARGETCOUNT", 1);
        }

        public QuestObjectiveGetKeyword(int keywordId)
            : base(1)
        {
            KeywordId = keywordId;
            MetaData.SetInt("TGTKEYWORD", KeywordId); // Unofficial
            MetaData.SetInt("TARGETCOUNT", 1);
        }

        public override ObjectiveType Type => ObjectiveType.GetKeyword;

        public int KeywordId { get; }
    }

    /// <summary>
    ///     Objective to equip a certain type of item.
    /// </summary>
    public class QuestObjectiveEquip : QuestObjective
    {
        public QuestObjectiveEquip(string tag)
            : base(1)
        {
            Tag = tag;
            MetaData.SetString("TGTSID", Tag);
            MetaData.SetInt("TARGETCOUNT", 1);
        }

        public override ObjectiveType Type => ObjectiveType.Equip;

        public string Tag { get; }
    }

    /// <summary>
    ///     Objective to gather a specific item.
    /// </summary>
    public class QuestObjectiveGather : QuestObjective
    {
        public QuestObjectiveGather(int itemId, int amount)
            : base(amount)
        {
            ItemId = itemId;
            MetaData.SetInt("TARGETITEM", ItemId);
            //this.MetaData.SetString("TGTSID", "/Gathering_Knife/"); // Tool to use (ignored?)
            MetaData.SetInt("TARGETCOUNT", Amount);
        }

        public override ObjectiveType Type => ObjectiveType.Gather;

        public int ItemId { get; }
    }

    /// <summary>
    ///     Objective to use a certain skill.
    /// </summary>
    public class QuestObjectiveUseSkill : QuestObjective
    {
        public QuestObjectiveUseSkill(SkillId skillId)
            : base(1)
        {
            Id = skillId;

            MetaData.SetUShort("TGTSKL", (ushort) skillId);
            MetaData.SetInt("TARGETCOUNT", 1);
        }

        public override ObjectiveType Type => ObjectiveType.UseSkill;

        public SkillId Id { get; set; }
    }

    /// <summary>
    ///     Objective to clear a certain dungeon.
    /// </summary>
    public class QuestObjectiveClearDungeon : QuestObjective
    {
        public QuestObjectiveClearDungeon(string dungeonName)
            : base(1)
        {
            DungeonName = dungeonName;

            MetaData.SetInt("TARGETCOUNT", 1);
            MetaData.SetString("TGTCLS", dungeonName);
        }

        public override ObjectiveType Type => ObjectiveType.ClearDungeon;

        public string DungeonName { get; set; }
    }

    public class QuestObjectiveCreate : QuestObjective
    {
        public QuestObjectiveCreate(int itemId, int amount, SkillId skillId, int quality = -1000)
            : base(amount)
        {
            SkillId = skillId;
            MinQuality = quality;
            ItemId = itemId;
            Amount = amount;

            MetaData.SetUShort("TGTSKL", (ushort) skillId);
            MetaData.SetInt("TARGETQUALITY", quality);
            MetaData.SetInt("TARGETITEM", itemId);
            MetaData.SetInt("TARGETCOUNT", amount);
        }

        public override ObjectiveType Type => ObjectiveType.Create;

        public SkillId SkillId { get; }
        public int MinQuality { get; }
        public int ItemId { get; }
    }
}