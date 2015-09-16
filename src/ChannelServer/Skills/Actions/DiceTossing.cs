// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.World;
using Aura.Channel.World.Entities;
using Aura.Mabi;
using Aura.Mabi.Const;
using Aura.Mabi.Network;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.Skills.Action
{
	/// <summary>
	/// Dice Tossing handler
	/// </summary>
	/// <remarks>
	/// Var2: Location
	/// Var3: Unknown
	/// Var4: Unknown
	///
	/// *TODO
	/// Remove 1 dice from hand during use.
	/// </remarks>
	///

	[Skill(SkillId.DiceTossing)]
	public class DiceTossing : ISkillHandler, IPreparable, IReadyable, IUseable, ICompletable, ICancelable
	{
		private const int ItemId = 62021;
		private const int Range = 400;

		/// <summary>
		/// Prepares skill, fails if no Dice is found.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		/// <returns></returns>
		public bool Prepare(Creature creature, Skill skill, Packet packet)
		{
			if (!creature.Inventory.Has(ItemId))
			{
				Send.Notice(creature, Localization.Get("You need at least one Six Sided Dice"));
				return false;
			}

			// there seems to be client side code to prevent this, but just incase..
			if (creature.RightHand != null && !creature.RightHand.HasTag("/dice/"))
			{
				Send.Notice(creature, Localization.Get("You must equip one Six Sided Dice to use this Action."));
				return false;
			}

			creature.StopMove();

			Send.UseMotion(creature, 27, 0, false, false);
			Send.Effect(creature, Effect.Dice, "prepare");
			Send.SkillPrepare(creature, skill.Info.Id, skill.GetCastTime());

			return true;
		}

		/// <summary>
		/// Readies the skill.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		/// <returns></returns>
		public bool Ready(Creature creature, Skill skill, Packet packet)
		{
			skill.Stacks = 1;
			Send.UseMotion(creature, 27, 1, true, false);
			Send.Effect(creature, Effect.Dice, "wait");
			Send.SkillReady(creature, skill.Info.Id);

			return true;
		}

		/// <summary>
		/// Uses skill, the actual usage is in Complete.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void Use(Creature creature, Skill skill, Packet packet)
		{
			var location = packet.GetLong();
			var unkInt3 = packet.GetInt();
			var unkInt4 = packet.GetInt();

			// Reduce Dice
			if (creature.Inventory.RightHand != null)
				creature.Inventory.Decrement(creature.Inventory.RightHand);

			Send.UseMotion(creature, 27, 2, false, false);
			Send.Effect(creature, Effect.Dice, "process", location, (byte)3);
			Send.SkillUse(creature, skill.Info.Id, location, unkInt3, unkInt4);
			skill.Stacks = 0;
		}

		/// <summary>
		/// Completes skill
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void Complete(Creature creature, Skill skill, Packet packet)
		{
			var location = packet.GetLong();
			var unkInt3 = packet.GetInt();
			var unkInt4 = packet.GetInt();

			var areaPosition = new Position(location);

			// Check range
			if (!creature.GetPosition().InRange(areaPosition, Range))
			{
				Send.Notice(creature, Localization.Get("Out of range."));
			}

			Send.SkillComplete(creature, skill.Info.Id, location, unkInt3, unkInt4);
		}

		/// <summary>
		/// Cancels skill, Cancels Motion and returns the character to idle position.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		public void Cancel(Creature creature, Skill skill)
		{
			skill.Stacks = 0;
			Send.MotionCancel2(creature, 1);
			Send.Effect(creature, Effect.Dice, "cancel");
		}
	}
}
