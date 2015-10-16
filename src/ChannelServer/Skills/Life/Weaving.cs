// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.World.Entities;
using Aura.Data;
using Aura.Data.Database;
using Aura.Mabi.Const;
using Aura.Mabi.Network;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.Skills.Life
{
	/// <summary>
	/// Handles Weaving production skill.
	/// </summary>
	/// <remarks>
	/// Starting production calls Prepare, once the creation process is done,
	/// Complete is called. There is no way to cancel the skill once Prepare
	/// was called.
	/// 
	/// While the client tells us how many items are gonna be produced,
	/// it Prepares the skill again and again, so we must only create
	/// one product at a time.
	/// 
	/// Weaving handles usage of looms and spinning wheels.
	/// 
	/// Var20: Success Rate?
	/// </remarks>
	[Skill(SkillId.Weaving)]
	public class Weaving : ProductionSkill
	{
		protected override bool RequiresProp { get { return true; } }

		protected override bool CheckCategory(Creature creature, ProductionCategory category)
		{
			return (category == ProductionCategory.Spinning || category == ProductionCategory.Weaving);
		}

		protected override void SkillTraining(Creature creature, Skill skill, ProductionData data, bool success, Item producedItem)
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
				if (producedItem.HasTag("/yarn/01/"))
					skill.Train(1); // Successfully make Thick Thread.
				else if (producedItem.HasTag("/yarn/02/"))
					skill.Train(2); // Successfully make Thin Thread.
				else if (producedItem.HasTag("/texture/04/"))
					skill.Train(3); // Successfully make Finest Fabric.
				else if (producedItem.HasTag("/texture/03/"))
					skill.Train(4); // Successfully make Fine Fabric.
				else if (producedItem.HasTag("/texture/02/"))
					skill.Train(5); // Successfully make Common Fabric.
				else if (producedItem.HasTag("/texture/01/"))
					skill.Train(6); // Successfully make Cheap Fabric.
				else if (producedItem.HasTag("/silk/01/"))
					skill.Train(7); // Successfully make Cheap Silk.

				else if (skill.Info.Rank >= SkillRank.RE && producedItem.HasTag("/leather_strap/01/"))
					skill.Train(8); // Successfully make Cheap Leather Strap.

				return;
			}

			if (skill.Info.Rank == SkillRank.RC)
			{
				if (producedItem.HasTag("/yarn/02/"))
					skill.Train(1); // Successfully make Thin Thread.
				return;
			}

			if (skill.Info.Rank >= SkillRank.RB && skill.Info.Rank <= SkillRank.RA)
			{
				if (producedItem.HasTag("/yarn/02/"))
					skill.Train(1); // Successfully make Thin Thread.
				else if (producedItem.HasTag("/texture/04/"))
					skill.Train(2); // Successfully make Finest Fabric.
				else if (producedItem.HasTag("/texture/03/"))
					skill.Train(3); // Successfully make Fine Fabric.
				else if (producedItem.HasTag("/texture/02/"))
					skill.Train(4); // Successfully make Common Fabric.
				else if (producedItem.HasTag("/silk/01/"))
					skill.Train(5); // Successfully make Cheap Silk.
				else if (producedItem.HasTag("/leather_strap/01/"))
					skill.Train(6); // Successfully make Cheap Leather Strap.

				return;
			}

			if (skill.Info.Rank == SkillRank.R9)
			{
				if (producedItem.HasTag("/yarn/03/"))
					skill.Train(1); // Successfully make a Braid.
				else if (producedItem.HasTag("/texture/04/"))
					skill.Train(2); // Successfully make Finest Fabric.
				else if (producedItem.HasTag("/texture/03/"))
					skill.Train(3); // Successfully make Fine Fabric.
				else if (producedItem.HasTag("/silk/04/"))
					skill.Train(4); // Successfully make Finest Silk.
				else if (producedItem.HasTag("/silk/03/"))
					skill.Train(5); // Successfully make Fine Silk.
				else if (producedItem.HasTag("/silk/02/"))
					skill.Train(6); // Successfully make Common Silk.
				else if (producedItem.HasTag("/leather_strap/01/"))
					skill.Train(7); // Successfully make Cheap Leather Strap.

				return;
			}

			if (skill.Info.Rank == SkillRank.R8)
			{
				if (producedItem.HasTag("/silk/04/"))
					skill.Train(1); // Successfully make Finest Silk.
				else if (producedItem.HasTag("/silk/03/"))
					return;
			}

			if (skill.Info.Rank == SkillRank.R7)
			{
				if (producedItem.HasTag("/yarn/03/"))
					skill.Train(1); // Successfully make a Braid.
				else if (producedItem.HasTag("/texture/04/"))
					skill.Train(2); // Successfully make Finest Fabric.
				else if (producedItem.HasTag("/texture/03/"))
					skill.Train(3); // Successfully make Fine Fabric.
				else if (producedItem.HasTag("/silk/04/"))
					skill.Train(4); // Successfully make Finest Silk.
				else if (producedItem.HasTag("/silk/03/"))
					skill.Train(5); // Successfully make Fine Silk.
				else if (producedItem.HasTag("/leather_strap/02/"))
					skill.Train(7); // Successfully make Common Leather Strap.

				return;
			}

			if (skill.Info.Rank == SkillRank.R6)
			{
				if (producedItem.HasTag("/texture/04/"))
					skill.Train(2); // Successfully make Finest Fabric.
				else if (producedItem.HasTag("/texture/03/"))
					skill.Train(3); // Successfully make Fine Fabric.
				else if (producedItem.HasTag("/silk/04/"))
					skill.Train(4); // Successfully make Finest Silk.
				else if (producedItem.HasTag("/silk/03/"))
					skill.Train(5); // Successfully make Fine Silk.
				else if (producedItem.HasTag("/leather_strap/03/"))
					skill.Train(7); // Successfully make Fine Leather Strap.
				else if (producedItem.HasTag("/leather_strap/02/"))
					skill.Train(7); // Successfully make Common Leather Strap.

				return;
			}

			if (skill.Info.Rank == SkillRank.R5)
			{
				if (producedItem.HasTag("/toughband/"))
					skill.Train(1); // Successfully make a Tough String.
				else if (producedItem.HasTag("/texture/04/"))
					skill.Train(2); // Successfully make Finest Fabric.
				else if (producedItem.HasTag("/texture/03/"))
					skill.Train(3); // Successfully make Fine Fabric.
				else if (producedItem.HasTag("/silk/04/"))
					skill.Train(4); // Successfully make Finest Silk.
				else if (producedItem.HasTag("/silk/03/"))
					skill.Train(5); // Successfully make Fine Silk.
				else if (producedItem.HasTag("/leather_strap/03/"))
					skill.Train(6); // Successfully make Fine Leather Strap.

				return;
			}

			if (skill.Info.Rank == SkillRank.R4)
			{
				if (producedItem.HasTag("/toughband/"))
					skill.Train(1); // Successfully make a Tough String.
				else if (producedItem.HasTag("/toughyarn/"))
					skill.Train(2); // Successfully make Tough Thread.

				return;
			}

			if (skill.Info.Rank >= SkillRank.R3 && skill.Info.Rank <= SkillRank.R2)
			{
				if (producedItem.HasTag("/toughyarn/"))
					skill.Train(1); // Successfully make Tough Thread.
				else if (producedItem.HasTag("/texture/04/"))
					skill.Train(2); // Successfully make Finest Fabric.
				else if (producedItem.HasTag("/silk/04/"))
					skill.Train(3); // Successfully make Finest Silk.
				else if (producedItem.HasTag("/leather_strap/04/"))
					skill.Train(4); // Successfully make Finest Leather Strap.

				return;
			}

			if (skill.Info.Rank == SkillRank.R1)
			{
				if (producedItem.HasTag("/toughyarn/"))
					skill.Train(1); // Successfully make Tough Thread.
				else if (producedItem.HasTag("/toughband/"))
					skill.Train(1); // Successfully make a Tough String.
				else if (producedItem.HasTag("/texture/04/"))
					skill.Train(2); // Successfully make Finest Fabric.
				else if (producedItem.HasTag("/silk/04/"))
					skill.Train(3); // Successfully make Finest Silk.
				else if (producedItem.HasTag("/leather_strap/04/"))
					skill.Train(4); // Successfully make Finest Leather Strap.

				return;
			}
		}
	}
}
