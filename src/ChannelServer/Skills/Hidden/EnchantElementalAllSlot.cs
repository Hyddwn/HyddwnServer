// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.World.Entities;
using Aura.Data;
using Aura.Mabi;
using Aura.Mabi.Const;
using Aura.Mabi.Network;
using Aura.Shared.Util;

namespace Aura.Channel.Skills.Hidden
{
	/// <summary>
	/// Handler for hidden skill used to enchant Massive [element] Elementals.
	/// </summary>
	[Skill(SkillId.EnchantElementalAllSlot)]
	public class EnchantElementalAllSlot : IPreparable, ICompletable, ICancelable
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
			var dict = packet.GetString();
			var enchantItemEntityId = MabiDictionary.Fetch<long>("ITEMID", dict);

			// Check everythig in Complete.

			// Response
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
			var dict = packet.GetString();
			var enchantItemEntityId = MabiDictionary.Fetch<long>("ITEMID", dict);

			// Check enchant
			var elemental = creature.Inventory.GetItem(enchantItemEntityId);
			if (elemental == null)
			{
				Log.Warning("EnchantElementalAllSlot.Complete: User '{0}' tried to enchant with a non-existent elemental.", creature.Client.Account.Id);
				goto L_Fail;
			}

			// Check option set
			var optionSetId = elemental.MetaData1.GetInt("ENELEM");
			var optionSetData = AuraData.OptionSetDb.Find(optionSetId);
			if (optionSetData == null)
			{
				Log.Warning("EnchantElementalAllSlot.Complete: User '{0:X16}' tried to enchant with unknown option set '{1}'.", creature.Client.Account.Id, optionSetId);
				goto L_Fail;
			}

			// Apply elementals
			var equip = creature.Inventory.GetMainEquipment();
			foreach (var item in equip)
			{
				item.ApplyOptionSet(optionSetData, true);

				Send.ItemUpdate(creature, item);
				Send.AcquireEnchantedItemInfo(creature, item.EntityId, item.Info.Id, optionSetId);
			}

			// Decrement elemental
			creature.Inventory.Decrement(elemental);

			Send.Effect(creature, Effect.Enchant, (byte)EnchantResult.Success);

		L_Fail:
			Send.Echo(creature, packet);
		}

		/// <summary>
		/// Cancel actions.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		public void Cancel(Creature creature, Skill skill)
		{
		}
	}
}
