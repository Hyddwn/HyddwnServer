// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Skills.Base;
using Aura.Channel.World.Entities;
using Aura.Data.Database;
using Aura.Mabi.Const;

namespace Aura.Channel.Skills.Hidden
{
	/// <summary>
	/// Handles Milling production skill.
	/// </summary>
	/// <remarks>
	/// Automatically used when clicking the Windmill.
	/// </remarks>
	[Skill(SkillId.Milling)]
	public class Milling : ProductionSkill
	{
		protected override bool RequiresProp { get { return true; } }

		protected override bool CheckCategory(Creature creature, ProductionCategory category)
		{
			return (category == ProductionCategory.Milling);
		}

		protected override void SkillTraining(Creature creature, Skill skill, ProductionData data, bool success, Item producedItem)
		{
			// Hidden Novice Rank skill, no training.
		}
	}
}
