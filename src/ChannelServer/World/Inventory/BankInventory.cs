// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.World.Entities;
using Aura.Data;
using Aura.Mabi.Const;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.World.Inventory
{
	public class BankInventory
	{
		/// <summary>
		/// Bank tabs, indexed by the tab (char) name.
		/// </summary>
		private Dictionary<string, BankTabPocket> Tabs { get; set; }

		/// <summary>
		/// Amount of silver. Use Add/RemoveGold to automatically update the client.
		/// </summary>
		public int Gold { get; set; }

		/// <summary>
		/// Creates new account bank.
		/// </summary>
		public BankInventory()
		{
			this.Tabs = new Dictionary<string, BankTabPocket>();
		}

		/// <summary>
		/// Adds bank tab pocket.
		/// </summary>
		/// <param name="name">Name of the tab (character)</param>
		/// <param name="race">Race filter id (1|2|3)</param>
		/// <param name="width">Width of the tab pocket</param>
		/// <param name="height">Height of the tab pocket</param>
		public void AddTab(string name, long creatureId, BankTabRace race, int width, int height)
		{
			if (this.Tabs.ContainsKey(name))
				throw new InvalidOperationException("Bank tab " + name + " already exists.");

			this.Tabs[name] = new BankTabPocket(name, creatureId, race, width, height);
		}

		/// <summary>
		/// Returns thread-safe list of tabs.
		/// </summary>
		/// <returns></returns>
		public IList<BankTabPocket> GetTabList()
		{
			lock (this.Tabs)
				return this.Tabs.Values.ToList();
		}

		/// <summary>
		/// Returns thread-safe list of tabs.
		/// </summary>
		/// <param name="race"></param>
		/// <returns></returns>
		public IList<BankTabPocket> GetTabList(BankTabRace race)
		{
			lock (this.Tabs)
				return this.Tabs.Values.Where(a => a.Race == race).ToList();
		}

		/// <summary>
		/// Returns all items in specified tab. Returns an empty
		/// list if tab doesn't exist.
		/// </summary>
		/// <param name="tabName"></param>
		/// <returns></returns>
		public IList<Item> GetTabItems(string tabName)
		{
			if (!this.Tabs.ContainsKey(tabName))
				return new List<Item>();

			return this.Tabs[tabName].GetItemList();
		}

		/// <summary>
		/// Returns all items in the bank.
		/// </summary>
		/// <returns></returns>
		public IList<Item> GetAllItems()
		{
			return Tabs.Values.SelectMany(t => t.GetItemList()).ToList();
		}

		/// <summary>
		/// Adds item to tab without any checks (only use for initialization).
		/// </summary>
		/// <param name="tabName"></param>
		/// <param name="item"></param>
		public bool InitAdd(string tabName, Item item)
		{
			if (!this.Tabs.ContainsKey(tabName))
				return false;

			this.Tabs[tabName].AddUnsafe(item);

			return true;
		}

		/// <summary>
		/// Adds gold to bank, sends update, and returns the new amount.
		/// </summary>
		/// <param name="creature">Creature removing gold (needed for updating)</param>
		/// <param name="amount">Amount of gold</param>
		/// <returns>New gold amount after adding</returns>
		public int AddGold(Creature creature, int amount)
		{
			this.Gold += amount;
			Send.BankUpdateGold(creature, this.Gold);

			return this.Gold;
		}

		/// <summary>
		/// Removes gold to bank, sends update, and returns the new amount.
		/// </summary>
		/// <param name="creature">Creature adding gold (needed for updating)</param>
		/// <param name="amount">Amount of gold</param>
		/// <returns>New gold amount after removing</returns>
		public int RemoveGold(Creature creature, int amount)
		{
			this.Gold -= amount;
			Send.BankUpdateGold(creature, this.Gold);

			return this.Gold;
		}

		/// <summary>
		/// Moves item from creature's inventory into bank.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="item"></param>
		/// <param name="bankId"></param>
		/// <param name="tabName"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public bool DepositItem(Creature creature, long itemEntityId, string bankId, string tabName, int x, int y)
		{
			// Check tab
			if (!this.Tabs.ContainsKey(tabName))
				return false;

			var tab = this.Tabs[tabName];

			// Check item
			var item = creature.Inventory.GetItem(itemEntityId);
			if (item == null) return false;

			// Add gold directly
			if (item.Info.Id == 2000)
			{
				this.Gold += item.Info.Amount;
				Send.BankUpdateGold(creature, this.Gold);
			}
			// Add check as gold
			else if (item.Info.Id == 2004)
			{
				var amount = item.MetaData1.GetInt("EVALUE");

				this.Gold += amount;
				Send.BankUpdateGold(creature, this.Gold);
			}
			// Add license as gold
			else if (item.HasTag("/personalshoplicense/"))
			{
				var fee = 0f;
				var license = item.Data.PersonalShopLicense;
				var licenseData = AuraData.ShopLicenseDb.Find(license);
				if (licenseData != null)
					fee = licenseData.ExchangeFee;

				var amount = item.MetaData1.GetInt("EVALUE");
				amount = (int)(amount - amount * fee);

				if (amount > 0)
				{
					this.Gold += amount;
					Send.BankUpdateGold(creature, this.Gold);
				}
			}
			// Normal items
			else
			{
				// Generate a new item, makes moving and updating easier.
				var newItem = new Item(item);

				// Try adding item to tab
				if (!tab.TryAdd(newItem, x, y))
					return false;

				// Update bank id
				newItem.Bank = bankId;

				// Update client
				Send.BankAddItem(creature, newItem, bankId, tabName);
			}

			// Remove item from inventory
			creature.Inventory.Remove(item);

			return true;
		}

		/// <summary>
		/// Moves item with given id from bank to creature's cursor pocket.
		/// Returns whether it was successful or not.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="itemEntityId"></param>
		/// <returns></returns>
		public bool WithdrawItem(Creature creature, string tabName, long itemEntityId)
		{
			// Check tab
			if (!this.Tabs.ContainsKey(tabName))
				return false;

			var tab = this.Tabs[tabName];

			// Check item
			var item = tab.GetItem(itemEntityId);
			if (item == null)
				return false;

			// Don't allow withdrawing while the item is being transferred
			// to the current bank.
			if (item.Bank == creature.Temp.CurrentBankId && item.BankTransferRemaining != 0)
				return false;

			// Ask about transfer if item is not in current bank.
			if (item.Bank != creature.Temp.CurrentBankId)
			{
				var fromBankId = item.Bank;
				var toBankId = creature.Temp.CurrentBankId;

				var time = GetTransferTime(fromBankId, toBankId);
				var price = GetTransferFee(item, fromBankId, toBankId);
				var bankTitle = GetName(item.Bank);

				Send.BankTransferInquiry(creature, itemEntityId, bankTitle, time, price);

				return false;
			}

			// Generate a new item, makes moving and updating easier.
			var newItem = new Item(item);

			// Try moving item into cursor pocket
			if (!creature.Inventory.Add(newItem, Pocket.Cursor))
				return false;

			// Remove item from bank
			tab.Remove(item);
			Send.BankRemoveItem(creature, tabName, itemEntityId);

			return true;
		}

		/// <summary>
		/// Returns item with the given entity id, or null if the item
		/// doesn't exist.
		/// </summary>
		/// <param name="itemEntityId"></param>
		/// <returns></returns>
		public Item GetItem(long itemEntityId)
		{
			foreach (var tab in this.Tabs.Values)
			{
				var item = tab.GetItem(itemEntityId);
				if (item != null)
					return item;
			}

			return null;
		}

		/// <summary>
		/// Returns name for the bank with the given id, or null if the bank
		/// doesn't exist.
		/// </summary>
		/// <param name="bankId"></param>
		/// <returns></returns>
		public static string GetName(string bankId)
		{
			if (bankId == "Global")
				return Localization.Get("Global Bank");

			var data = AuraData.BankDb.Find(bankId);
			if (data == null)
				return null;

			return data.Name;
		}

		/// <summary>
		/// Returns the gold fee to transfer an item from one bank to the
		/// other. The parameters are the ids.
		/// </summary>
		/// <remarks>
		/// Unofficial, but works well.
		/// </remarks>
		/// <param name="item">Item that is to be transferred.</param>
		/// <param name="fromBankId">The id of the bank the item is at.</param>
		/// <param name="toBankId">The id of the bank the item is transferred to.</param>
		/// <returns></returns>
		public static int GetTransferFee(Item item, string fromBankId, string toBankId)
		{
			if (fromBankId == "Global" || toBankId == "Global")
				return 0;

			var data1 = AuraData.BankDb.Find(fromBankId);
			var data2 = AuraData.BankDb.Find(toBankId);

			if (data1 == null || data2 == null)
			{
				Log.Warning("BankInventory.GetFee: Unknown bank, returning 0 fee. ({0} -> {1})", fromBankId, toBankId);
				return 0;
			}

			var pos1 = new Position(data1.X, data1.Y);
			var pos2 = new Position(data2.X, data2.Y);
			var distance = pos1.GetDistance(pos2);
			var itemSize = (item.Data.Width * item.Data.Height);
			var fee = (item.OptionInfo.Price / 2000f * itemSize * distance);

			return (int)Math.Max(10, fee);
		}

		/// <summary>
		/// Returns the time it takes to transfer an item from one bank to
		/// the other in milliseconds.
		/// </summary>
		/// <remarks>
		/// Unofficial, but works well.
		/// </remarks>
		/// <param name="fromBankId">The id of the bank the item is at.</param>
		/// <param name="toBankId">The id of the bank the item is transferred to.</param>
		/// <returns></returns>
		public static int GetTransferTime(string fromBankId, string toBankId)
		{
			if (fromBankId == "Global" || toBankId == "Global")
				return 0;

			var data1 = AuraData.BankDb.Find(fromBankId);
			var data2 = AuraData.BankDb.Find(toBankId);

			if (data1 == null || data2 == null)
			{
				Log.Warning("BankInventory.GetTransferTime: Unknown bank, returning 0 time. ({0} -> {1})", fromBankId, toBankId);
				return 0;
			}

			var pos1 = new Position(data1.X, data1.Y);
			var pos2 = new Position(data2.X, data2.Y);
			var distance = pos1.GetDistance(pos2);

			return distance * 5000;
		}

		/// <summary>
		/// Attempts to start a transfer of the given item to the bank the
		/// creature is currently using.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="itemEntityId"></param>
		/// <param name="instantTransfer"></param>
		/// <returns></returns>
		public bool Transfer(Creature creature, long itemEntityId, bool instantTransfer)
		{
			// Check bank
			var currentBank = creature.Temp.CurrentBankId;
			if (string.IsNullOrWhiteSpace(currentBank))
			{
				Log.Warning("BankTransferRequest: Player '0x{0:X16}' (Account: '{1}') tried to transfer item while not being in a bank.", creature.EntityId, creature.Client.Account.Id);
				return false;
			}

			// Get item and tab
			Item item = null;
			string tabName = null;
			foreach (var tab in this.Tabs.Values)
			{
				item = tab.GetItem(itemEntityId);
				if (item != null)
				{
					tabName = tab.Name;
					break;
				}
			}

			if (item == null)
			{
				Log.Warning("BankTransferRequest: Player '0x{0:X16}' (Account: '{1}') tried to transfer a non-existing or in-transit item.", creature.EntityId, creature.Client.Account.Id);
				return false;
			}

			// Get fee and time
			var fee = BankInventory.GetTransferFee(item, item.Bank, currentBank);
			var time = BankInventory.GetTransferTime(item.Bank, currentBank);

			// Incrase fee for instant transfer.
			if (instantTransfer)
			{
				// Don't change, hardcoded in the client.
				fee = 100 + (fee * 5);
				time = 0;
			}

			// Check gold
			if (this.Gold < fee)
			{
				Send.MsgBox(creature, Localization.Get("Unable to pay the fee, Insufficient balance."));
				return false;
			}

			// Transfer
			item.Bank = currentBank;
			item.BankTransferStart = DateTime.Now;
			item.BankTransferDuration = time;

			this.RemoveGold(creature, fee);

			Send.BankTransferInfo(creature, tabName, item);

			return true;
		}
	}

	public class BankTabPocket : InventoryPocketNormal
	{
		public string Name { get; private set; }
		public long CreatureId { get; private set; }
		public BankTabRace Race { get; private set; }
		public int Width { get { return _width; } }
		public int Height { get { return _height; } }

		public BankTabPocket(string name, long creatureId, BankTabRace race, int width, int height)
			: base(Pocket.None, width, height)
		{
			this.Name = name;
			this.CreatureId = creatureId;
			this.Race = race;
		}

		/// <summary>
		/// Returns thread-safe list of items in this pocket.
		/// </summary>
		/// <returns></returns>
		public IList<Item> GetItemList()
		{
			lock (_items)
				return _items.Values.ToList();
		}

		/// <summary>
		/// Attempts to add the item at the given position.
		/// </summary>
		/// <remarks>
		/// TODO: Inventory really needs some refactoring, we shouldn't have
		///   to override such methods.
		/// </remarks>
		/// <param name="item"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public bool TryAdd(Item item, int x, int y)
		{
			if (x + item.Data.Width > _width || y + item.Data.Height > _height)
				return false;

			var collidingItems = this.GetCollidingItems((uint)x, (uint)y, item);
			if (collidingItems.Count > 0)
				return false;

			item.Move(this.Pocket, x, y);
			this.AddUnsafe(item);

			return true;
		}
	}

	public enum BankTabRace : byte
	{
		Human, Elf, Giant
	}
}
