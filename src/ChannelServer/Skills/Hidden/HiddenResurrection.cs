// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
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

namespace Aura.Channel.Skills.Hidden
{
	/// <summary>
	/// Handler for HiddenResurrection, aka. the skill used by Phoenix
	/// Feathers and the like.
	/// </summary>
	[Skill(SkillId.HiddenResurrection)]
	public class HiddenResurrection : IPreparable, IUseable, ICompletable, ICancelable
	{
		/// <summary>
		/// Prepares skill, getting the used item.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		/// <returns></returns>
		public bool Prepare(Creature creature, Skill skill, Packet packet)
		{
			var parameters = packet.GetString();

			// Check item
			var itemEntityId = MabiDictionary.Fetch<long>("ITEMID", parameters);
			if (itemEntityId == 0)
			{
				Log.Warning("HiddenResurrection: Creature '{0:X16}' tried to use skill without item.");
				return false;
			}

			var item = creature.Inventory.GetItem(itemEntityId);
			if (item == null)
			{
				Log.Warning("HiddenResurrection: Creature '{0:X16}' tried to use skill with non-existing item.");
				return false;
			}

			if (!item.HasTag("/usable/resurrection/"))
			{
				Log.Warning("HiddenResurrection: Creature '{0:X16}' tried to use skill with invalid item.");
				return false;
			}

			creature.Temp.SkillItem1 = item;

			// Response
			Send.Echo(creature, Op.SkillReady, packet);
			skill.State = SkillState.Ready;

			return true;
		}

		/// <summary>
		/// Uses skill, showing effects.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void Use(Creature creature, Skill skill, Packet packet)
		{
			var targetEntityId = packet.GetLong();
			// 2 unk ints in older logs
			//var unkInt1 = packet.GetInt();
			//var unkInt2 = packet.GetInt();

			Send.Effect(creature, Effect.UseMagic, "healing_phoenix", targetEntityId);
			Send.Echo(creature, packet);
		}

		/// <summary>
		/// Completes skill, actually reviving the target.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void Complete(Creature creature, Skill skill, Packet packet)
		{
			var targetEntityId = packet.GetLong();
			// 2 unk ints in older logs
			//var unkInt1 = packet.GetInt();
			//var unkInt2 = packet.GetInt();

			var item = creature.Temp.SkillItem1;

			// Check for target in world, as remote feathers probably use this
			// handler as well. Check for region or remote afterwards.
			var target = ChannelServer.Instance.World.GetCreature(targetEntityId);
			if (target == null || !target.IsDead || !target.DeadMenu.Has(ReviveOptions.PhoenixFeather) || (creature.RegionId != target.RegionId && !item.HasTag("/remote/")))
			{
				// Unofficial
				// This could easily happen due to lag, or the target
				// suddenly logging out, don't punish or warn.
				Send.Notice(creature, Localization.Get("Invalid target."));
			}
			else
			{
				target.Revive(ReviveOptions.PhoenixFeather);
				creature.Inventory.Decrement(item);
			}

			Send.Echo(creature, packet);
		}

		/// <summary>
		/// Cancels the skill.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		public void Cancel(Creature creature, Skill skill)
		{
		}
	}
}
