// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.World.Entities;
using Aura.Data.Database;
using Aura.Mabi.Const;
using Aura.Shared.Util;

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
		protected override bool CheckCategory(Creature creature, ProductionCategory category)
		{
			return (category == ProductionCategory.Milling);
		}

		protected override bool CheckProp(Creature creature, long propEntityId)
		{
			// Milling behaves like a skill that doesn't require a prop,
			// but as we know, it does, so we'll do a static check for
			// Tir's Windmill.

			propEntityId = 0xA000010009042B;

			// Check prop
			var prop = creature.Region.GetProp(propEntityId);
			if (prop == null)
			{
				Log.Error("Milling.CheckProp: Windmill not found ({0:X16}).", propEntityId);
				Send.ServerMessage(creature, Localization.Get("Error in prop check, please report."));
				return false;
			}

			// Check range
			if (!creature.GetPosition().InRange(prop.GetPosition(), 1500))
			{
				Send.Notice(creature, Localization.Get("You are too far away."));
				return false;
			}

			// Check state
			// Sanity check, client checks this.
			if (prop.State == "off")
			{
				Send.Notice(creature, Localization.Get("The Mill isn't working."));
				return false;
			}

			return true;
		}

		protected override void SkillTraining(Creature creature, Skill skill, ProductionData data, bool success)
		{
			// Hidden Novice Rank skill, no training.
		}
	}
}
