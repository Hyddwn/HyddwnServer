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

			// Check if any party members are actually dead if party feather
			if (item.HasTag("/party/") && !creature.Party.GetMembers().Any(a => a != creature && a.IsDead && a.DeadMenu.Has(ReviveOptions.PhoenixFeather)))
			{
				Send.MsgBox(creature, Localization.Get("There is no one available to resurrect."));
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
			var target = this.GetTarget(packet);
			var targetEntityId = (target == null ? 0 : target.EntityId);

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
			var target = this.GetTarget(packet);
			var item = creature.Temp.SkillItem1;

			// Check target validity
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

				// Revive other party members if party feather
				if (item.HasTag("/party/"))
				{
					var targets = creature.Party.GetMembers().Where(a => a != creature && a != target && a.IsDead && a.DeadMenu.Has(ReviveOptions.PhoenixFeather));
					foreach (var target2 in targets)
						target.Revive(ReviveOptions.PhoenixFeather);
				}

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

		/// <summary>
		/// Gets target, based on packet's values.
		/// </summary>
		/// <param name="packet"></param>
		/// <returns></returns>
		private Creature GetTarget(Packet packet)
		{
			Creature target = null;

			switch (packet.Peek())
			{
				case PacketElementType.Long:
					var targetEntityId = packet.GetLong();
					// 2 unk ints in older logs
					//var unkInt1 = packet.GetInt();
					//var unkInt2 = packet.GetInt();

					// Get from world, in case there's a remote feather that
					// doesn't use names.
					target = ChannelServer.Instance.World.GetCreature(targetEntityId);

					break;

				case PacketElementType.String:
					var targetName = packet.GetString();

					// Get from world, as advanced feathers can be used remotely
					target = ChannelServer.Instance.World.GetCreature(targetName);

					break;

				default:
					Log.Warning("HiddenResurrection: Unknown target var type '{0}'.", packet.Peek());
					break;
			}

			return target;
		}
	}
}
