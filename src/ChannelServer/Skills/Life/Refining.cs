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
	/// Handles Refining production skill.
	/// </summary>
	/// <remarks>
	/// Var20: Success Rate?
	/// </remarks>
	[Skill(SkillId.Refining)]
	public class Refining : ProductionSkill
	{
		protected override bool CheckCategory(Creature creature, ProductionCategory category)
		{
			return (category == ProductionCategory.Refining);
		}

		protected override bool CheckProp(Creature creature, long propEntityId)
		{
			// Check existence
			var prop = (propEntityId == 0 ? null : creature.Region.GetProp(propEntityId));
			if (prop == null || !prop.HasTag("/refine/"))
			{
				Log.Warning("Refining.Prepare: Creature '{0:X16}' tried to use production skill with invalid prop.", creature.EntityId);
				return false;
			}

			// Check distance
			if (!creature.GetPosition().InRange(prop.GetPosition(), 1000))
			{
				// Don't warn, could happen due to lag.
				Send.Notice(creature, Localization.Get("You are too far away."));
				return false;
			}

			// Check state
			// Sanity check, client should handle it.
			if (prop.State != "on")
			{
				Send.Notice(creature, Localization.Get("The Waterwheel isn't working,\nand that means the Furnace won't fire."));
				return false;
			}

			return true;
		}

		protected override void SkillTraining(Creature creature, Skill skill, ProductionData data, bool success)
		{
			if (skill.Info.Rank == SkillRank.Novice)
			{
				skill.Train(1); // Use the skill.
				if (success)
					skill.Train(2); // Use the skill successfully.
				return;
			}

			if (skill.Info.Rank >= SkillRank.RF && skill.Info.Rank <= SkillRank.RD)
			{
				if (data.ItemData.HasTag("/ironingot/"))
				{
					if (success)
						skill.Train(1); // Successfully refine Iron Ore.
					else
						skill.Train(2); // Fail to refine Iron Ore.
				}
				return;
			}

			if (skill.Info.Rank >= SkillRank.RC && skill.Info.Rank <= SkillRank.RB)
			{
				if (data.ItemData.HasTag("/copperingot/"))
				{
					if (success)
						skill.Train(1); // Successfully refine Copper Ore.
					else
						skill.Train(2); // Fail to refine Copper Ore.
				}
				else if (data.ItemData.HasTag("/ironingot/"))
				{
					if (success)
						skill.Train(3); // Successfully refine Iron Ore.
					else
						skill.Train(4); // Fail to refine Iron Ore.
				}
				return;
			}

			if (skill.Info.Rank >= SkillRank.RA && skill.Info.Rank <= SkillRank.R9)
			{
				if (data.ItemData.HasTag("/silveringot/"))
				{
					if (success)
						skill.Train(1); // Successfully refine Silver Ore.
					else
						skill.Train(2); // Fail to refine Silver Ore.
				}
				else if (data.ItemData.HasTag("/copperingot/"))
				{
					if (success)
						skill.Train(3); // Successfully refine Copper Ore.
					else
						skill.Train(4); // Fail to refine Copper Ore.
				}
				else if (data.ItemData.HasTag("/ironingot/"))
				{
					if (success)
						skill.Train(5); // Successfully refine Iron Ore.
					else
						skill.Train(6); // Fail to refine Iron Ore.
				}
				return;
			}

			if (skill.Info.Rank >= SkillRank.R8 && skill.Info.Rank <= SkillRank.R7)
			{
				if (data.ItemData.HasTag("/goldingot/"))
				{
					if (success)
						skill.Train(1); // Successfully refine Gold Ore.
					else
						skill.Train(2); // Fail to refine Gold Ore.
				}
				else if (data.ItemData.HasTag("/silveringot/"))
				{
					if (success)
						skill.Train(3); // Successfully refine Silver Ore.
					else
						skill.Train(4); // Fail to refine Silver Ore.
				}
				else if (data.ItemData.HasTag("/copperingot/"))
				{
					if (success)
						skill.Train(5); // Successfully refine Copper Ore.
					else
						skill.Train(6); // Fail to refine Copper Ore.
				}
				else if (data.ItemData.HasTag("/ironingot/"))
				{
					if (success)
						skill.Train(7); // Successfully refine Iron Ore.
					else
						skill.Train(8); // Fail to refine Iron Ore.
				}
				return;
			}

			if (skill.Info.Rank == SkillRank.R6)
			{
				if (data.ItemData.HasTag("/mythrilingot/"))
				{
					if (success)
						skill.Train(1); // Successfully refine Mythril Ore.
					else
						skill.Train(2); // Fail to refine Mythril Ore.
				}
				else if (data.ItemData.HasTag("/goldingot/"))
				{
					if (success)
						skill.Train(3); // Successfully refine Gold Ore.
					else
						skill.Train(4); // Fail to refine Gold Ore.
				}
				else if (data.ItemData.HasTag("/silveringot/"))
				{
					if (success)
						skill.Train(5); // Successfully refine Silver Ore.
					else
						skill.Train(6); // Fail to refine Silver Ore.
				}
				else if (data.ItemData.HasTag("/copperingot/"))
				{
					if (success)
						skill.Train(7); // Successfully refine Copper Ore.
					else
						skill.Train(8); // Fail to refine Copper Ore.
				}
				else if (data.ItemData.HasTag("/ironingot/"))
				{
					if (success)
						skill.Train(9); // Successfully refine Iron Ore.
				}
				return;
			}

			if (skill.Info.Rank == SkillRank.R5)
			{
				if (data.ItemData.HasTag("/plate/iron/"))
				{
					if (success)
						skill.Train(1); // Successfully refine an Iron Plate.
				}
				else if (data.ItemData.HasTag("/mythrilingot/"))
				{
					if (success)
						skill.Train(2); // Successfully refine Mythril Ore.
					else
						skill.Train(3); // Fail to refine Mythril Ore.
				}
				else if (data.ItemData.HasTag("/goldingot/"))
				{
					if (success)
						skill.Train(4); // Successfully refine Gold Ore.
					else
						skill.Train(5); // Fail to refine Gold Ore.
				}
				else if (data.ItemData.HasTag("/silveringot/"))
				{
					if (success)
						skill.Train(6); // Successfully refine Silver Ore.
					else
						skill.Train(7); // Fail to refine Silver Ore.
				}
				else if (data.ItemData.HasTag("/copperingot/"))
				{
					if (success)
						skill.Train(8); // Successfully refine Copper Ore.
				}
				else if (data.ItemData.HasTag("/ironingot/"))
				{
					if (success)
						skill.Train(9); // Successfully refine Iron Ore.
				}
				return;
			}

			if (skill.Info.Rank == SkillRank.R4)
			{
				if (data.ItemData.HasTag("/plate/copper/"))
				{
					if (success)
						skill.Train(1); // Successfully refine a Copper Plate.
				}
				else if (data.ItemData.HasTag("/mythrilingot/"))
				{
					if (success)
						skill.Train(2); // Successfully refine Mythril Ore.
					else
						skill.Train(3); // Fail to refine Mythril Ore.
				}
				else if (data.ItemData.HasTag("/goldingot/"))
				{
					if (success)
						skill.Train(4); // Successfully refine Gold Ore.
					else
						skill.Train(5); // Fail to refine Gold Ore.
				}
				else if (data.ItemData.HasTag("/silveringot/"))
				{
					if (success)
						skill.Train(6); // Successfully refine Silver Ore.
					else
						skill.Train(7); // Fail to refine Silver Ore.
				}
				else if (data.ItemData.HasTag("/copperingot/"))
				{
					if (success)
						skill.Train(8); // Successfully refine Copper Ore.
				}
				else if (data.ItemData.HasTag("/ironingot/"))
				{
					if (success)
						skill.Train(9); // Successfully refine Iron Ore.
				}
				return;
			}

			if (skill.Info.Rank == SkillRank.R3)
			{
				if (data.ItemData.HasTag("/plate/silver/"))
				{
					if (success)
						skill.Train(1); // Successfully refine a Silver Plate.
				}
				else if (data.ItemData.HasTag("/mythrilingot/"))
				{
					if (success)
						skill.Train(2); // Successfully refine Mythril Ore.
					else
						skill.Train(3); // Fail to refine Mythril Ore.
				}
				else if (data.ItemData.HasTag("/goldingot/"))
				{
					if (success)
						skill.Train(4); // Successfully refine Gold Ore.
					else
						skill.Train(5); // Fail to refine Gold Ore.
				}
				else if (data.ItemData.HasTag("/silveringot/"))
				{
					if (success)
						skill.Train(6); // Successfully refine Silver Ore.
				}
				else if (data.ItemData.HasTag("/copperingot/"))
				{
					if (success)
						skill.Train(7); // Successfully refine Copper Ore.
				}
				else if (data.ItemData.HasTag("/ironingot/"))
				{
					if (success)
						skill.Train(8); // Successfully refine Iron Ore.
				}
				return;
			}

			if (skill.Info.Rank == SkillRank.R2)
			{
				if (data.ItemData.HasTag("/plate/gold/"))
				{
					if (success)
						skill.Train(1); // Successfully refine a Gold Plate.
				}
				else if (data.ItemData.HasTag("/mythrilingot/"))
				{
					if (success)
						skill.Train(2); // Successfully refine Mythril Ore.
					else
						skill.Train(3); // Fail to refine Mythril Ore.
				}
				else if (data.ItemData.HasTag("/goldingot/"))
				{
					if (success)
						skill.Train(4); // Successfully refine Gold Ore.
				}
				else if (data.ItemData.HasTag("/silveringot/"))
				{
					if (success)
						skill.Train(5); // Successfully refine Silver Ore.
				}
				else if (data.ItemData.HasTag("/copperingot/"))
				{
					if (success)
						skill.Train(6); // Successfully refine Copper Ore.
				}
				else if (data.ItemData.HasTag("/ironingot/"))
				{
					if (success)
						skill.Train(7); // Successfully refine Iron Ore.
				}
				return;
			}

			if (skill.Info.Rank == SkillRank.R1)
			{
				if (data.ItemData.HasTag("/plate/mythril/"))
				{
					if (success)
						skill.Train(1); // Successfully refine a Mythril Plate.
				}
				else if (data.ItemData.HasTag("/mythrilingot/"))
				{
					if (success)
						skill.Train(2); // Successfully refine Mythril Ore.
				}
				else if (data.ItemData.HasTag("/goldingot/"))
				{
					if (success)
						skill.Train(3); // Successfully refine Gold Ore.
				}
				else if (data.ItemData.HasTag("/silveringot/"))
				{
					if (success)
						skill.Train(4); // Successfully refine Silver Ore.
				}
				else if (data.ItemData.HasTag("/copperingot/"))
				{
					if (success)
						skill.Train(5); // Successfully refine Copper Ore.
				}
				else if (data.ItemData.HasTag("/ironingot/"))
				{
					if (success)
						skill.Train(6); // Successfully refine Iron Ore.
				}
				return;
			}
		}
	}
}
