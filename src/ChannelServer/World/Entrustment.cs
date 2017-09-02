// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Magic;
using Aura.Channel.World.Entities;
using Aura.Channel.World.Inventory;
using Aura.Data;
using Aura.Mabi.Const;
using Aura.Shared.Util;
using System.Threading;

namespace Aura.Channel.World
{
	public class Entrustment
	{
		private static long _ids;

		/// <summary>
		/// Id of this entrustment session.
		/// </summary>
		public long Id { get; private set; }

		/// <summary>
		/// Creature that initiated the entrustment.
		/// </summary>
		public Creature Creature1 { get; private set; }

		/// <summary>
		/// Creature that received the entrustment request.
		/// </summary>
		public Creature Creature2 { get; private set; }

		/// <summary>
		/// Creature 2's enchant rank.
		/// </summary>
		public SkillRank EnchantRank { get; private set; }

		/// <summary>
		/// Returns true if creature 2 accepted the request.
		/// </summary>
		public bool Accepted { get; private set; }

		/// <summary>
		/// The current status of the entrustment.
		/// </summary>
		public EntrustmentStatus Status { get; private set; }

		/// <summary>
		/// Returns true if entrustment is ready to be processed.
		/// </summary>
		public bool IsReady
		{
			get
			{
				return this.CheckItems(this.Creature1);
			}
		}

		/// <summary>
		/// Creates a new entrustment session.
		/// </summary>
		/// <param name="creature1"></param>
		/// <param name="creature2"></param>
		public Entrustment(Creature creature1, Creature creature2)
		{
			this.Id = GetNewId();
			this.Creature1 = creature1;
			this.Creature2 = creature2;

			this.Creature1.Temp.ActiveEntrustment = this;
			this.Creature2.Temp.ActiveEntrustment = this;

			var skill = this.Creature2.Skills.Get(SkillId.Enchant);
			if (skill != null)
				this.EnchantRank = skill.Info.Rank;
		}

		/// <summary>
		/// Returns a new trade id.
		/// </summary>
		/// <returns></returns>
		private static long GetNewId()
		{
			return Interlocked.Increment(ref _ids);
		}

		/// <summary>
		/// Initiates entrustment, sending a request to creature 2.
		/// </summary>
		public void Initiate()
		{
			Send.EntrustmentRequest(this.Creature2, this.Creature1.EntityId, 0);
		}

		/// <summary>
		/// Cancels entrustment and moves items back to main inv.
		/// </summary>
		public void Cancel()
		{
			// Client sends the respective cancel packet from the client that
			// didn't invoke the cancelation when it receives Close.
			// This is a simple way to prevent a loop of Close->Cancel->Close.
			if (this.Status == EntrustmentStatus.Canceled)
				return;
			this.Status = EntrustmentStatus.Canceled;

			this.Creature1.Inventory.MoveItemsToInvFrom(Pocket.EntrustmentItem1, Pocket.EntrustmentItem2, Pocket.EntrustmentReward);

			Send.Notice(this.Creature1, Localization.Get("The entrusting for enchantment has been cancelled."));
			Send.Notice(this.Creature2, Localization.Get("The entrusting for enchantment has been cancelled."));

			Send.EntrustmentClose(this.Creature1);
			Send.EntrustmentClose(this.Creature2);
		}

		/// <summary>
		/// Adds item to creature 2's window.
		/// </summary>
		/// <param name="item">Item added.</param>
		public void AddItem(Item item, Pocket pocket)
		{
			Send.EntrustmentAddItem(this.Creature2, pocket, item);

			// Update chance
			var chance = this.GetChance();
			Send.EntrustmentChanceUpdate(this.Creature1, chance, this.EnchantRank);
			Send.EntrustmentChanceUpdate(this.Creature2, chance, this.EnchantRank);

			// Update request button
			if (this.CheckItems(this.Creature1))
				Send.EntrustmentEnableRequest(this.Creature1);
			else
				Send.EntrustmentDisableRequest(this.Creature1);
		}

		/// <summary>
		/// Removes item from partner's window.
		/// </summary>
		/// <param name="item">Item removed.</param>
		public void RemoveItem(Item item, Pocket pocket)
		{
			Send.EntrustmentRemoveItem(this.Creature2, pocket, item.EntityId);

			// Update chance
			var chance = this.GetChance();
			Send.EntrustmentChanceUpdate(this.Creature1, chance, this.EnchantRank);
			Send.EntrustmentChanceUpdate(this.Creature2, chance, this.EnchantRank);

			// Update request button
			if (this.CheckItems(this.Creature1))
				Send.EntrustmentEnableRequest(this.Creature1);
			else
				Send.EntrustmentDisableRequest(this.Creature1);
		}

		/// <summary>
		/// Returns the current success chance.
		/// </summary>
		/// <returns></returns>
		private float GetChance()
		{
			var item2 = this.GetItem2();
			var optionSetId = this.GetOptionSetId(item2);
			var chance = 0f;

			// Get chance
			if (optionSetId != 0)
			{
				var powder = this.GetMagicPowder(this.Creature1);
				var optionSetData = AuraData.OptionSetDb.Find(optionSetId);

				if (powder != null && optionSetData != null)
					chance = Enchant.GetChance(this.Creature2, powder, SkillId.Enchant, optionSetData);
			}

			return chance;
		}

		/// <summary>
		/// Returns true if the items in creature's entrustment pockets
		/// are valid.
		/// </summary>
		/// <param name="creature"></param>
		/// <returns></returns>
		private bool CheckItems(Creature creature)
		{
			var item1 = this.GetItem1();
			var item2 = this.GetItem2();

			if (item1 == null || item2 == null)
				return false;

			if (!item2.IsEnchant)
				return false;

			var optionSetId = this.GetOptionSetId(item2);
			if (optionSetId == 0)
			{
				Log.Debug("Entrustment.CheckItems: Given enchant doesn't have a prefix or a suffix.");
				return false;
			}

			var optionset = AuraData.OptionSetDb.Find(optionSetId);
			var optionsetAllowed = (item1.HasTag(optionset.Allow) && !item1.HasTag(optionset.Disallow));

			return optionsetAllowed;
		}

		/// <summary>
		/// Returns option set id from the item's meta data, or 0.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		private int GetOptionSetId(Item item)
		{
			if (item == null)
				return 0;

			var id = item.MetaData1.GetInt("ENSFIX");
			if (id == 0)
				id = item.MetaData1.GetInt("ENPFIX");

			return id;
		}

		/// <summary>
		/// Returns the magic powder to use from creature.
		/// </summary>
		/// <param name="creature"></param>
		/// <returns></returns>
		public Item GetMagicPowder(Creature creature)
		{
			// Check right hand first
			var rhItem = creature.RightHand;
			if (rhItem != null && this.CheckPowder(rhItem))
				return rhItem;

			// Check all pockets afterwards
			var items = creature.Inventory.GetItems(a => this.CheckPowder(a), StartAt.BottomRight);
			if (items.Count == 0)
				return null;

			return items[0];
		}

		/// <summary>
		/// Returns true if item is a valid powder to be used for entrusted
		/// enchanting.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		private bool CheckPowder(Item item)
		{
			return (item.HasTag("/enchant/powder/") && !item.HasTag("/powder05/"));
		}

		/// <summary>
		/// Notifies creature 2 that the request is ready.
		/// </summary>
		public void Ready()
		{
			this.Status = EntrustmentStatus.Ready;
			Send.EntrustmentRequestFinalized(this.Creature2);
		}

		/// <summary>
		/// Accepts the request for creature 2, informing creature 1.
		/// </summary>
		public void Accept()
		{
			this.Status = EntrustmentStatus.Ready;
			Send.EntrustmentFinalizing(this.Creature1);
			Send.Notice(this.Creature1, Localization.Get("The player has accepted the request."));
		}

		/// <summary>
		/// Ends entrustment.
		/// </summary>
		public void End()
		{
			var items = this.Creature1.Inventory.GetItems(a => a.Info.Pocket == Pocket.EntrustmentReward);
			foreach (var item in items)
			{
				this.Creature1.Inventory.Remove(item);
				this.Creature2.Inventory.Add(new Item(item), true);

				Send.SystemMessageFrom(this.Creature1, Localization.Get("<ENCHANT_ENTRUST>"), Localization.Get("Gave the item  as a reward."), Localization.Get(item.Data.Name));
				Send.SystemMessageFrom(this.Creature1, Localization.Get("<ENCHANT_ENTRUST>"), Localization.Get("Received the item {0} as a reward."), Localization.Get(item.Data.Name));
			}

			var item1 = this.GetItem1();
			var item2 = this.GetItem2();

			if (item1 != null)
			{
				this.Creature1.Inventory.Remove(item1);
				this.Creature1.Inventory.Add(item1, true);
			}
			if (item2 != null)
				this.Creature1.Inventory.Remove(item2);

			Send.EntrustmentEnd(this.Creature1);
			Send.EntrustmentEnd(this.Creature2);

			this.Creature1.Temp.ActiveEntrustment = null;
			this.Creature2.Temp.ActiveEntrustment = null;
		}

		/// <summary>
		/// Returns the item on the upper right.
		/// </summary>
		/// <returns></returns>
		public Item GetItem1()
		{
			return this.Creature1.Inventory.GetItemAt(Pocket.EntrustmentItem1, 0, 0);
		}

		/// <summary>
		/// Returns the item on the upper left.
		/// </summary>
		/// <returns></returns>
		public Item GetItem2()
		{
			return this.Creature1.Inventory.GetItemAt(Pocket.EntrustmentItem2, 0, 0);
		}
	}

	public enum EntrustmentStatus
	{
		NotReady = 0,
		Ready = 1,
		Finalizing = 2,
		Canceled = 99,
	}
}
