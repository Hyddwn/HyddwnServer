// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.World.Entities;
using Aura.Mabi.Const;

namespace Aura.Channel.Skills.Combat
{
	/// <summary>
	/// Handles Sharp Mind related tasks.
	/// </summary>
	public static class SharpMindHandler
	{
		/// <summary>
		/// Handles Sharp Mind training.
		/// </summary>
		/// <param name="skillUser"></param>
		/// <param name="target"></param>
		public static void Train(Creature skillUser, Creature target, bool success)
		{
			if (!success)
				return;

			var targetSkill = target.Skills.Get(SkillId.SharpMind);
			if (targetSkill == null) return;

			var rating = target.GetPowerRating(skillUser);
			var skillRank = targetSkill.Info.Rank;

			if (skillRank >= SkillRank.Novice && skillRank <= SkillRank.RB)
				targetSkill.Train(1); // Successfully notice an enemy's skill.

			if (skillRank >= SkillRank.RF && skillRank <= SkillRank.RB && rating == PowerRating.Normal)
				targetSkill.Train(2); // Successfully notice a same level enemy's skill.

			if (skillRank >= SkillRank.RD && skillRank <= SkillRank.RB && rating == PowerRating.Strong)
				targetSkill.Train(3); // Successfully notice a strong enemy's skill.

			if (skillRank == SkillRank.RB && rating == PowerRating.Awful)
				targetSkill.Train(4); // Successfully notice an awful enemy's skill.
		}
	}
}
