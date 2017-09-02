// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Linq;
using Aura.Channel.World.Entities;
using Aura.Mabi.Const;

namespace Aura.Channel.World.Quests
{
    public abstract class QuestPrerequisite
    {
        /// <summary>
        ///     Returns true if character meets the requirement.
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        public abstract bool Met(Creature character);

        /// <summary>
        ///     Returns true if this prerequisite, or one of its nested ones,
        ///     is of the given type.
        /// </summary>
        /// <param name="type"></param>
        /// <returns></returns>
        public abstract bool Is(Type type);
    }

    /// <summary>
    ///     Quest prerequisite, met if quest is complete.
    /// </summary>
    public class QuestPrerequisiteQuestCompleted : QuestPrerequisite
    {
        public QuestPrerequisiteQuestCompleted(int id)
        {
            Id = id;
        }

        public int Id { get; protected set; }

        public override bool Met(Creature character)
        {
            return character.Quests.IsComplete(Id);
        }

        public override bool Is(Type type)
        {
            return GetType() == type;
        }
    }

    /// <summary>
    ///     Level prerequisite, met if character's level is greater or equal.
    /// </summary>
    public class QuestPrerequisiteReachedLevel : QuestPrerequisite
    {
        public QuestPrerequisiteReachedLevel(int level)
        {
            Level = level;
        }

        public int Level { get; protected set; }

        public override bool Met(Creature character)
        {
            return character.Level >= Level;
        }

        public override bool Is(Type type)
        {
            return GetType() == type;
        }
    }

    /// <summary>
    ///     Total Level prerequisite, met if character's total level is greater or equal.
    /// </summary>
    public class QuestPrerequisiteReachedTotalLevel : QuestPrerequisite
    {
        public QuestPrerequisiteReachedTotalLevel(int level)
        {
            Level = level;
        }

        public int Level { get; protected set; }

        public override bool Met(Creature character)
        {
            return character.TotalLevel >= Level;
        }

        public override bool Is(Type type)
        {
            return GetType() == type;
        }
    }

    /// <summary>
    ///     Skill prerequisite, met if character reaches the given rank.
    /// </summary>
    public class QuestPrerequisiteReachedRank : QuestPrerequisite
    {
        public QuestPrerequisiteReachedRank(SkillId skillId, SkillRank rank)
        {
            Id = skillId;
            Rank = rank;
        }

        public SkillId Id { get; protected set; }
        public SkillRank Rank { get; protected set; }

        public override bool Met(Creature character)
        {
            return character.Skills.Has(Id, Rank);
        }

        public override bool Is(Type type)
        {
            return GetType() == type;
        }
    }

    /// <summary>
    ///     Age prerequisite, met if character's age is greater or equal.
    /// </summary>
    /// <remarks>
    ///     Since aging only happens on relog, we don't need any special handling
    ///     for this one, the quests will simply check their prerequisites on
    ///     login, as they always do, and will give the quest if the age is
    ///     reached.
    /// </remarks>
    public class QuestPrerequisiteReachedAge : QuestPrerequisite
    {
        public QuestPrerequisiteReachedAge(int age)
        {
            Age = age;
        }

        public int Age { get; protected set; }

        public override bool Met(Creature character)
        {
            return character.Age >= Age;
        }

        public override bool Is(Type type)
        {
            return GetType() == type;
        }
    }

    /// <summary>
    ///     Skill prerequisite, met if character doesn't have the skill or rank yet.
    /// </summary>
    public class QuestPrerequisiteNotSkill : QuestPrerequisite
    {
        public QuestPrerequisiteNotSkill(SkillId skillId, SkillRank rank = SkillRank.Novice)
        {
            Id = skillId;
            Rank = rank;
        }

        public SkillId Id { get; protected set; }
        public SkillRank Rank { get; protected set; }

        public override bool Met(Creature character)
        {
            return !character.Skills.Has(Id, Rank);
        }

        public override bool Is(Type type)
        {
            return GetType() == type;
        }
    }

    /// <summary>
    ///     Skill prerequisite, met if a certain event is in progress.
    /// </summary>
    public class QuestPrerequisiteEventActive : QuestPrerequisite
    {
        public QuestPrerequisiteEventActive(string gameEventId)
        {
            GameEventId = gameEventId;
        }

        public string GameEventId { get; protected set; }

        public override bool Met(Creature character)
        {
            return ChannelServer.Instance.GameEventManager.IsActive(GameEventId);
        }

        public override bool Is(Type type)
        {
            return GetType() == type;
        }
    }

    /// <summary>
    ///     Collection of prerequisites, met if all are met.
    /// </summary>
    public class QuestPrerequisiteAnd : QuestPrerequisite
    {
        public QuestPrerequisiteAnd(params QuestPrerequisite[] prerequisites)
        {
            Prerequisites = prerequisites;
        }

        public QuestPrerequisite[] Prerequisites { get; protected set; }

        public override bool Met(Creature character)
        {
            if (Prerequisites.Length == 0)
                return true;

            return Prerequisites.All(p => p.Met(character));
        }

        public override bool Is(Type type)
        {
            return Prerequisites.Any(a => a.Is(type));
        }
    }

    /// <summary>
    ///     Collection of prerequisites, met if at least one of them is met.
    /// </summary>
    public class QuestPrerequisiteOr : QuestPrerequisite
    {
        public QuestPrerequisiteOr(params QuestPrerequisite[] prerequisites)
        {
            Prerequisites = prerequisites;
        }

        public QuestPrerequisite[] Prerequisites { get; protected set; }

        public override bool Met(Creature character)
        {
            if (Prerequisites.Length == 0)
                return true;

            return Prerequisites.Any(p => p.Met(character));
        }

        public override bool Is(Type type)
        {
            return Prerequisites.Any(a => a.Is(type));
        }
    }

    /// <summary>
    ///     Inverts the return of a prerequisite's Met()
    /// </summary>
    public class QuestPrerequisiteNot : QuestPrerequisite
    {
        protected QuestPrerequisite _prereq;

        public QuestPrerequisiteNot(QuestPrerequisite prerequiste)
        {
            _prereq = prerequiste;
        }

        public override bool Met(Creature character)
        {
            return !_prereq.Met(character);
        }

        public override bool Is(Type type)
        {
            return _prereq.GetType() == type;
        }
    }
}