// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.World.Entities;
using Aura.Data;
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
	/// Handler for the hidden Enchant skill used by items.
	/// </summary>
	[Skill(SkillId.HiddenEnchant)]
	public class HiddenEnchant : IPreparable, ICompletable
	{
		/// <summary>
		/// Prepares skill.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		/// <returns></returns>
		public bool Prepare(Creature creature, Skill skill, Packet packet)
		{
			var itemEntityId = packet.GetLong();
			var enchantEntityId = packet.GetLong();

			// Get items
			var item = creature.Inventory.GetItem(itemEntityId);
			if (item == null)
			{
				Log.Warning("HiddenEnchant.Prepare: Creature '{0:X16}' tried to enchant non-existing item.");
				return false;
			}

			var enchant = creature.Inventory.GetItem(enchantEntityId);
			if (enchant == null)
			{
				Log.Warning("HiddenEnchant.Prepare: Creature '{0:X16}' tried to enchant with non-existing enchant item.");
				return false;
			}

			// Only elementals supported for now
			if (enchant.HasTag("/elemental/"))
			{
			}
			else
			{
				Send.ServerMessage(creature, Localization.Get("Enchanting hasn't been implemented yet."));
				return false;
			}

			creature.Temp.SkillItem1 = item;
			creature.Temp.SkillItem2 = enchant;

			Send.Echo(creature, Op.SkillUse, packet);
			skill.State = SkillState.Used;

			return true;
		}

		/// <summary>
		/// Completes skill, applying the enchant.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void Complete(Creature creature, Skill skill, Packet packet)
		{
			var item = creature.Temp.SkillItem1;
			var enchant = creature.Temp.SkillItem2;
			var optionSetId = 0;

			// Decrement enchant
			creature.Inventory.Decrement(creature.Temp.SkillItem2);

			// Get option set id

			// Elementals
			if (enchant.HasTag("/elemental/"))
			{
				optionSetId = enchant.MetaData1.GetInt("ENELEM");
			}
			// Enchants
			else
			{
				// ...
				goto L_End;
			}

			// Get and apply option set
			var optionSetData = AuraData.OptionSetDb.Find(optionSetId);
			if (optionSetData == null)
			{
				Log.Error("HiddenEnchant.Complete: Unknown option set '{0}'.", optionSetId);
				goto L_End;
			}

			var result = EnchantResult.Success;
			item.ApplyOptionSet(optionSetData, true);

			Send.Effect(creature, Effect.Enchant, (byte)result);
			Send.ItemUpdate(creature, item);
			Send.AcquireEnchantedItemInfo(creature, item.EntityId, item.Info.Id, optionSetId);

		L_End:
			Send.Echo(creature, packet);

			creature.Temp.SkillItem1 = null;
			creature.Temp.SkillItem2 = null;
		}
	}
}
