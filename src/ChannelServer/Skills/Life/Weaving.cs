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
	/// Handles Weaving production skill.
	/// </summary>
	/// <remarks>
	/// Weaving handles usage of looms and spinning wheels.
	/// 
	/// Var20: Success Rate?
	/// </remarks>
	[Skill(SkillId.Weaving)]
	public class Weaving : ProductionSkill
	{
		protected override bool CheckTools(Creature creature, Skill skill, ProductionData productData)
		{
			// The only weaving tool in the db are gloves, for silk,
			// while spinning, which is also handled by Weaving,
			// uses blades for leather straps. That's why we need
			// special handling for weaving.
			if (productData.Category == ProductionCategory.Weaving)
			{
				// Hands need to be free
				if (!creature.HandsFree)
				{
					Send.Notice(creature, Localization.Get("You're going to need both hands free to weave anything."));
					return false;
				}

				// The only production tool for weaving are gloves
				if (productData.Tool != null)
				{
					var glove = creature.Inventory.GetItemAt(Pocket.Glove, 0, 0);
					if (glove == null || !glove.HasTag(productData.Tool))
					{
						// Unofficial
						Send.Notice(creature, Localization.Get("You need access to a Spinning Wheel or Loom."));
						return false;
					}
				}
			}
			else if (productData.Category == ProductionCategory.Spinning)
			{
				// Run the standard tests for spinning.
				return base.CheckTools(creature, skill, productData);
			}
			else
			{
				Log.Error("Weaving.CheckTools: Unknown product category '{0}'.", productData.Category);
				return false;
			}

			return true;
		}

		protected override bool CheckCategory(Creature creature, ProductionCategory category)
		{
			return (category == ProductionCategory.Spinning || category == ProductionCategory.Weaving);
		}

		protected override bool CheckProp(Creature creature, long propEntityId)
		{
			// Check existence
			var prop = (propEntityId == 0 ? null : creature.Region.GetProp(propEntityId));
			if (prop == null || !prop.HasTag("/spin/|/loom/"))
			{
				Log.Warning("Weaving.Prepare: Creature '{0:X16}' tried to use production skill with invalid prop.", creature.EntityId);
				return false;
			}

			// Check distance
			if (!creature.GetPosition().InRange(prop.GetPosition(), 1000))
			{
				// Don't warn, could happen due to lag.
				Send.Notice(creature, Localization.Get("You can't reach a Spinning Wheel or Loom from here."));
				return false;
			}

			return true;
		}

		protected override void UpdateTool(Creature creature, ProductionData productData)
		{
			if (productData.Tool == null)
				return;

			Item item;

			if (productData.Category == ProductionCategory.Weaving)
			{
				item = creature.Inventory.GetItemAt(Pocket.Glove, 0, 0);
			}
			else if (productData.Category == ProductionCategory.Spinning)
			{
				item = creature.RightHand;
			}
			else
			{
				Log.Error("Weaving.UpdateTool: Unknown product category '{0}'.", productData.Category);
				return;
			}

			if (item == null)
			{
				Log.Error("Weaving.UpdateTool: No item to update found. Category: {0}", productData.Category);
				return;
			}

			creature.Inventory.ReduceDurability(item, productData.Durability);
			creature.Inventory.AddProficiency(item, Proficiency);
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

			if (!success)
				return;

			if (skill.Info.Rank >= SkillRank.RF && skill.Info.Rank <= SkillRank.RD)
			{
				if (data.ItemData.HasTag("/yarn/01/"))
					skill.Train(1); // Successfully make Thick Thread.
				else if (data.ItemData.HasTag("/yarn/02/"))
					skill.Train(2); // Successfully make Thin Thread.
				else if (data.ItemData.HasTag("/texture/04/"))
					skill.Train(3); // Successfully make Finest Fabric.
				else if (data.ItemData.HasTag("/texture/03/"))
					skill.Train(4); // Successfully make Fine Fabric.
				else if (data.ItemData.HasTag("/texture/02/"))
					skill.Train(5); // Successfully make Common Fabric.
				else if (data.ItemData.HasTag("/texture/01/"))
					skill.Train(6); // Successfully make Cheap Fabric.
				else if (data.ItemData.HasTag("/silk/01/"))
					skill.Train(7); // Successfully make Cheap Silk.

				else if (skill.Info.Rank >= SkillRank.RE && data.ItemData.HasTag("/leather_strap/01/"))
					skill.Train(8); // Successfully make Cheap Leather Strap.

				return;
			}

			if (skill.Info.Rank == SkillRank.RC)
			{
				if (data.ItemData.HasTag("/yarn/02/"))
					skill.Train(1); // Successfully make Thin Thread.
				return;
			}

			if (skill.Info.Rank >= SkillRank.RB && skill.Info.Rank <= SkillRank.RA)
			{
				if (data.ItemData.HasTag("/yarn/02/"))
					skill.Train(1); // Successfully make Thin Thread.
				else if (data.ItemData.HasTag("/texture/04/"))
					skill.Train(2); // Successfully make Finest Fabric.
				else if (data.ItemData.HasTag("/texture/03/"))
					skill.Train(3); // Successfully make Fine Fabric.
				else if (data.ItemData.HasTag("/texture/02/"))
					skill.Train(4); // Successfully make Common Fabric.
				else if (data.ItemData.HasTag("/silk/01/"))
					skill.Train(5); // Successfully make Cheap Silk.
				else if (data.ItemData.HasTag("/leather_strap/01/"))
					skill.Train(6); // Successfully make Cheap Leather Strap.

				return;
			}

			if (skill.Info.Rank == SkillRank.R9)
			{
				if (data.ItemData.HasTag("/yarn/03/"))
					skill.Train(1); // Successfully make a Braid.
				else if (data.ItemData.HasTag("/texture/04/"))
					skill.Train(2); // Successfully make Finest Fabric.
				else if (data.ItemData.HasTag("/texture/03/"))
					skill.Train(3); // Successfully make Fine Fabric.
				else if (data.ItemData.HasTag("/silk/04/"))
					skill.Train(4); // Successfully make Finest Silk.
				else if (data.ItemData.HasTag("/silk/03/"))
					skill.Train(5); // Successfully make Fine Silk.
				else if (data.ItemData.HasTag("/silk/02/"))
					skill.Train(6); // Successfully make Common Silk.
				else if (data.ItemData.HasTag("/leather_strap/01/"))
					skill.Train(7); // Successfully make Cheap Leather Strap.

				return;
			}

			if (skill.Info.Rank == SkillRank.R8)
			{
				if (data.ItemData.HasTag("/silk/04/"))
					skill.Train(1); // Successfully make Finest Silk.
				else if (data.ItemData.HasTag("/silk/03/"))
					return;
			}

			if (skill.Info.Rank == SkillRank.R7)
			{
				if (data.ItemData.HasTag("/yarn/03/"))
					skill.Train(1); // Successfully make a Braid.
				else if (data.ItemData.HasTag("/texture/04/"))
					skill.Train(2); // Successfully make Finest Fabric.
				else if (data.ItemData.HasTag("/texture/03/"))
					skill.Train(3); // Successfully make Fine Fabric.
				else if (data.ItemData.HasTag("/silk/04/"))
					skill.Train(4); // Successfully make Finest Silk.
				else if (data.ItemData.HasTag("/silk/03/"))
					skill.Train(5); // Successfully make Fine Silk.
				else if (data.ItemData.HasTag("/leather_strap/02/"))
					skill.Train(6); // Successfully make Common Leather Strap.

				return;
			}

			if (skill.Info.Rank == SkillRank.R6)
			{
				if (data.ItemData.HasTag("/texture/04/"))
					skill.Train(1); // Successfully make Finest Fabric.
				else if (data.ItemData.HasTag("/texture/03/"))
					skill.Train(2); // Successfully make Fine Fabric.
				else if (data.ItemData.HasTag("/silk/04/"))
					skill.Train(3); // Successfully make Finest Silk.
				else if (data.ItemData.HasTag("/silk/03/"))
					skill.Train(4); // Successfully make Fine Silk.
				else if (data.ItemData.HasTag("/leather_strap/03/"))
					skill.Train(5); // Successfully make Fine Leather Strap.
				else if (data.ItemData.HasTag("/leather_strap/02/"))
					skill.Train(6); // Successfully make Common Leather Strap.

				return;
			}

			if (skill.Info.Rank == SkillRank.R5)
			{
				if (data.ItemData.HasTag("/toughband/"))
					skill.Train(1); // Successfully make a Tough String.
				else if (data.ItemData.HasTag("/texture/04/"))
					skill.Train(2); // Successfully make Finest Fabric.
				else if (data.ItemData.HasTag("/texture/03/"))
					skill.Train(3); // Successfully make Fine Fabric.
				else if (data.ItemData.HasTag("/silk/04/"))
					skill.Train(4); // Successfully make Finest Silk.
				else if (data.ItemData.HasTag("/silk/03/"))
					skill.Train(5); // Successfully make Fine Silk.
				else if (data.ItemData.HasTag("/leather_strap/03/"))
					skill.Train(6); // Successfully make Fine Leather Strap.

				return;
			}

			if (skill.Info.Rank == SkillRank.R4)
			{
				if (data.ItemData.HasTag("/toughband/"))
					skill.Train(1); // Successfully make a Tough String.
				else if (data.ItemData.HasTag("/toughyarn/"))
					skill.Train(2); // Successfully make Tough Thread.

				return;
			}

			if (skill.Info.Rank >= SkillRank.R3 && skill.Info.Rank <= SkillRank.R2)
			{
				if (data.ItemData.HasTag("/toughyarn/"))
					skill.Train(1); // Successfully make Tough Thread.
				else if (data.ItemData.HasTag("/texture/04/"))
					skill.Train(2); // Successfully make Finest Fabric.
				else if (data.ItemData.HasTag("/silk/04/"))
					skill.Train(3); // Successfully make Finest Silk.
				else if (data.ItemData.HasTag("/leather_strap/04/"))
					skill.Train(4); // Successfully make Finest Leather Strap.

				return;
			}

			if (skill.Info.Rank == SkillRank.R1)
			{
				if (data.ItemData.HasTag("/toughyarn/"))
					skill.Train(1); // Successfully make Tough Thread.
				else if (data.ItemData.HasTag("/toughband/"))
					skill.Train(2); // Successfully make a Tough String.
				else if (data.ItemData.HasTag("/texture/04/"))
					skill.Train(3); // Successfully make Finest Fabric.
				else if (data.ItemData.HasTag("/silk/04/"))
					skill.Train(4); // Successfully make Finest Silk.
				else if (data.ItemData.HasTag("/leather_strap/04/"))
					skill.Train(5); // Successfully make Finest Leather Strap.

				return;
			}
		}
	}
}
