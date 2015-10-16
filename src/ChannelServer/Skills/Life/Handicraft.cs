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
	/// Handles Handicraft production skill.
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
	/// Var20: Success Rate?
	/// </remarks>
	[Skill(SkillId.Handicraft)]
	public class Handicraft : ProductionSkill
	{
		protected override bool RequiresProp { get { return false; } }

		protected override void OnUse(Creature creature, Skill skill)
		{
			Send.UseMotion(creature, 11, 3);
		}

		protected override bool CheckTools(Creature creature, Skill skill)
		{
			if (creature.RightHand == null || !creature.RightHand.HasTag("/handicraft_kit/"))
			{
				// Sanity check, the client should normally handle this.
				Send.MsgBox(creature, Localization.Get("You're going to need a Handicraft Kit for that."));
				return false;
			}

			return true;
		}

		protected override void SkillTraining(Creature creature, Skill skill, ProductionData data, bool success)
		{
			if (skill.Info.Rank == SkillRank.RF)
			{
				if (success)
				{
					if (data.Rank >= SkillRank.RD)
						skill.Train(1); // Make a rank D or higher item.
					else
						skill.Train(2); // Make a rank F or E item.
				}
				else
				{
					if (data.Rank >= SkillRank.RD)
						skill.Train(3); // Fail at making a rank D or higher item.
					else
						skill.Train(4); // Fail at making a rank F or E item.
				}

				return;
			}

			if (skill.Info.Rank == SkillRank.RE)
			{
				if (success)
				{
					if (data.Rank >= SkillRank.RC)
						skill.Train(1); // Make a rank C or higher item.
					else
						skill.Train(2); // Make a rank F, E, or D item. 
				}
				else
				{
					if (data.Rank >= SkillRank.RC)
						skill.Train(3); // Fail at making a rank C or higher item.
					else
						skill.Train(4); // Fail at making a rank F, E, or D item.
				}

				return;
			}

			if (skill.Info.Rank >= SkillRank.RD && skill.Info.Rank <= SkillRank.R3)
			{
				var high = skill.Info.Rank + 2;
				var mid = high - 1;
				var low = mid - 2;

				if (success)
				{
					if (data.Rank >= high)
						skill.Train(1); // Make a rank X or higher item.
					else if (data.Rank >= low && data.Rank <= mid)
						skill.Train(2); // Make a rank X, X, or X item.
					else
						skill.Train(3); // Make a rank X (or lower) item.
				}
				else
				{
					if (data.Rank >= high)
						skill.Train(4); // Fail at making a rank X or higher item.
					else if (data.Rank >= low && data.Rank <= mid)
						skill.Train(5); // Fail at making a rank X, X, or X item.
					else
						skill.Train(6); // Fail at making a rank X (or lower) item.
				}

				return;
			}

			if (skill.Info.Rank == SkillRank.R2)
			{
				if (success)
				{
					if (data.Rank >= SkillRank.R3)
						skill.Train(1); // Make a rank 3, 2, or 1 item.
					else
						skill.Train(2); // Make a rank 4 or lower item.
				}
				else
				{
					if (data.Rank >= SkillRank.R3)
						skill.Train(3); // Fail at making a rank 2, or 1 item.
					else
						skill.Train(4); // Fail at making a rank 3 or lower item.
				}

				return;
			}

			if (skill.Info.Rank == SkillRank.R1)
			{
				if (success)
				{
					if (data.Rank >= SkillRank.R2)
						skill.Train(1); // Make a rank 3, 2, or 1 item.
					else
						skill.Train(2); // Make a rank 4 or lower item.
				}
				else
				{
					if (data.Rank >= SkillRank.R2)
						skill.Train(3); // Fail at making a rank 2, or 1 item.
					else
						skill.Train(4); // Fail at making a rank 3 or lower item.
				}

				return;
			}
		}
	}
}
