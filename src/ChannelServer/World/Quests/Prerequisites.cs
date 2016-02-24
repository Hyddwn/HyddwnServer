// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Linq;
using Aura.Channel.World.Entities;
using Aura.Mabi.Const;
using System;

namespace Aura.Channel.World.Quests
{
	public abstract class QuestPrerequisite
	{
		/// <summary>
		/// Returns true if character meets the requirement.
		/// </summary>
		/// <param name="character"></param>
		/// <returns></returns>
		public abstract bool Met(Creature character);

		/// <summary>
		/// Returns true if this prerequisite, or on of its nested ones,
		/// is of the given type.
		/// </summary>
		/// <param name="type"></param>
		/// <returns></returns>
		public abstract bool Is(Type type);
	}

	/// <summary>
	/// Quest prerequisite, met if quest is complete.
	/// </summary>
	public class QuestPrerequisiteQuestCompleted : QuestPrerequisite
	{
		public int Id { get; protected set; }

		public QuestPrerequisiteQuestCompleted(int id)
		{
			this.Id = id;
		}

		public override bool Met(Creature character)
		{
			return character.Quests.IsComplete(this.Id);
		}

		public override bool Is(Type type)
		{
			return (this.GetType() == type);
		}
	}

	/// <summary>
	/// Level prerequisite, met if character's level is greater or equal.
	/// </summary>
	public class QuestPrerequisiteReachedLevel : QuestPrerequisite
	{
		public int Level { get; protected set; }

		public QuestPrerequisiteReachedLevel(int level)
		{
			this.Level = level;
		}

		public override bool Met(Creature character)
		{
			return (character.Level >= this.Level);
		}

		public override bool Is(Type type)
		{
			return (this.GetType() == type);
		}
	}

	/// <summary>
	/// Total Level prerequisite, met if character's total level is greater or equal.
	/// </summary>
	public class QuestPrerequisiteReachedTotalLevel : QuestPrerequisite
	{
		public int Level { get; protected set; }

		public QuestPrerequisiteReachedTotalLevel(int level)
		{
			this.Level = level;
		}

		public override bool Met(Creature character)
		{
			return (character.TotalLevel >= this.Level);
		}

		public override bool Is(Type type)
		{
			return (this.GetType() == type);
		}
	}

	/// <summary>
	/// Skill prerequisite, met if character reaches the given rank.
	/// </summary>
	public class QuestPrerequisiteReachedRank : QuestPrerequisite
	{
		public SkillId Id { get; protected set; }
		public SkillRank Rank { get; protected set; }

		public QuestPrerequisiteReachedRank(SkillId skillId, SkillRank rank)
		{
			this.Id = skillId;
			this.Rank = rank;
		}

		public override bool Met(Creature character)
		{
			return character.Skills.Has(this.Id, this.Rank);
		}

		public override bool Is(Type type)
		{
			return (this.GetType() == type);
		}
	}

	/// <summary>
	/// Age prerequisite, met if character's age is greater or equal.
	/// </summary>
	/// <remarks>
	/// Since aging only happens on relog, we don't need any special handling
	/// for this one, the quests will simply check their prerequisites on
	/// login, as they always do, and will give the quest if the age is
	/// reached.
	/// </remarks>
	public class QuestPrerequisiteReachedAge : QuestPrerequisite
	{
		public int Age { get; protected set; }

		public QuestPrerequisiteReachedAge(int age)
		{
			this.Age = age;
		}

		public override bool Met(Creature character)
		{
			return (character.Age >= this.Age);
		}

		public override bool Is(Type type)
		{
			return (this.GetType() == type);
		}
	}

	/// <summary>
	/// Skill prerequisite, met if character doesn't have the skill or rank yet.
	/// </summary>
	public class QuestPrerequisiteNotSkill : QuestPrerequisite
	{
		public SkillId Id { get; protected set; }
		public SkillRank Rank { get; protected set; }

		public QuestPrerequisiteNotSkill(SkillId skillId, SkillRank rank = SkillRank.Novice)
		{
			this.Id = skillId;
			this.Rank = rank;
		}

		public override bool Met(Creature character)
		{
			return !character.Skills.Has(this.Id, this.Rank);
		}

		public override bool Is(Type type)
		{
			return (this.GetType() == type);
		}
	}

	/// <summary>
	/// Collection of prerequisites, met if all are met.
	/// </summary>
	public class QuestPrerequisiteAnd : QuestPrerequisite
	{
		public QuestPrerequisite[] Prerequisites { get; protected set; }

		public QuestPrerequisiteAnd(params QuestPrerequisite[] prerequisites)
		{
			this.Prerequisites = prerequisites;
		}

		public override bool Met(Creature character)
		{
			if (this.Prerequisites.Length == 0)
				return true;

			return this.Prerequisites.All(p => p.Met(character));
		}

		public override bool Is(Type type)
		{
			return this.Prerequisites.Any(a => a.Is(type));
		}
	}

	/// <summary>
	/// Collection of prerequisites, met if at least one of them is met.
	/// </summary>
	public class QuestPrerequisiteOr : QuestPrerequisite
	{
		public QuestPrerequisite[] Prerequisites { get; protected set; }

		public QuestPrerequisiteOr(params QuestPrerequisite[] prerequisites)
		{
			this.Prerequisites = prerequisites;
		}

		public override bool Met(Creature character)
		{
			if (this.Prerequisites.Length == 0)
				return true;

			return this.Prerequisites.Any(p => p.Met(character));
		}

		public override bool Is(Type type)
		{
			return this.Prerequisites.Any(a => a.Is(type));
		}
	}

	/// <summary>
	/// Inverts the return of a prerequisite's Met()
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
			return (_prereq.GetType() == type);
		}
	}
}
