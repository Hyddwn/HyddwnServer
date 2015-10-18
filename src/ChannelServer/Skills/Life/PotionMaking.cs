// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.World.Entities;
using Aura.Data.Database;
using Aura.Mabi.Const;
using Aura.Shared.Util;

namespace Aura.Channel.Skills.Life
{
	/// <summary>
	/// Handles Potion Making production skill.
	/// </summary>
	[Skill(SkillId.PotionMaking)]
	public class PotionMaking : ProductionSkill
	{
		protected override bool CheckTools(Creature creature, Skill skill)
		{
			if (creature.RightHand == null || !creature.RightHand.HasTag("/potion_making/kit/"))
			{
				// Sanity check, the client should normally handle this.
				Send.MsgBox(creature, Localization.Get("You need a Potion Concoction Kit to make potions!"));
				return false;
			}

			return true;
		}

		protected override bool CheckCategory(Creature creature, ProductionCategory category)
		{
			return (category == ProductionCategory.PotionMaking);
		}

		protected override void SkillTraining(Creature creature, Skill skill, ProductionData data, bool success)
		{
			//if (skill.Info.Rank == SkillRank.RF)
			//{
			//	if (success)
			//	{
			//		if (data.Rank >= SkillRank.RD)
			//			skill.Train(1); // Make a rank D or higher item.
			//		else
			//			skill.Train(2); // Make a rank F or E item.
			//	}
			//	else
			//	{
			//		if (data.Rank >= SkillRank.RD)
			//			skill.Train(3); // Fail at making a rank D or higher item.
			//		else
			//			skill.Train(4); // Fail at making a rank F or E item.
			//	}

			//	return;
			//}
		}
	}
}
