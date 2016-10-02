// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using Aura.Channel.Network.Sending;
using Aura.Channel.World.Entities;
using Aura.Shared.Util;
using Aura.Mabi;
using Aura.Data;
using Aura.Shared.Scripting.Scripts;
using Aura.Mabi.Const;
using Aura.Shared.Database;

namespace Aura.Channel.Scripting.Scripts
{
	/// <summary>
	/// Represents a regular NPC shop.
	/// </summary>
	/// <remarks>
	/// The difference between a gold and a ducat shop is: none.
	/// The secret lies in the DucatPrice value of ItemOptionInfo. If it is > 0,
	/// the client shows that, instead of the gold price, and assumes that
	/// you're paying with ducats.
	/// 
	/// Selling items always uses gold (option to sell for ducats?).
	/// 
	/// Aside from Ducats and Gold there are two more currencies,
	/// Stars and Pons. The client will show the buy currency based on
	/// the values set, Ducats > Stars > Gold.
	/// Pons overweights everything, but it's displayed alongside
	/// other prices if they aren't 0.
	/// </remarks>
	public class NpcShopScript : GeneralScript, IDisposable
	{
		protected Dictionary<string, NpcShopTab> _tabs;

		/// <summary>
		/// Creates new NpcShopScript
		/// </summary>
		public NpcShopScript()
		{
			_tabs = new Dictionary<string, NpcShopTab>();
			ChannelServer.Instance.Events.ErinnMidnightTick += this.OnErinnMidnightTick;
		}

		/// <summary>
		/// Unsubscribes Shop from events.
		/// </summary>
		public override void Dispose()
		{
			base.Dispose();
			ChannelServer.Instance.Events.ErinnMidnightTick -= this.OnErinnMidnightTick;
		}

		/// <summary>
		/// Called at midnight (Erinn time).
		/// </summary>
		/// <param name="time"></param>
		protected virtual void OnErinnMidnightTick(ErinnTime time)
		{
			this.RandomizeItemColors();
		}

		/// <summary>
		/// Randomizes colors of all items in all tabs.
		/// </summary>
		protected virtual void RandomizeItemColors()
		{
			var rand = RandomProvider.Get();

			lock (_tabs)
			{
				foreach (var tab in _tabs.Values)
					tab.RandomizeItemColors();
			}
		}

		/// <summary>
		/// Initializes shop, calling setup and adding it to the script manager.
		/// </summary>
		/// <returns></returns>
		public override bool Init()
		{
			if (ChannelServer.Instance.ScriptManager.NpcShopScripts.ContainsKey(this.GetType().Name))
			{
				Log.Error("NpcShopScript.Init: Duplicate '{0}'", this.GetType().Name);
				return false;
			}

			this.Setup();

			ChannelServer.Instance.ScriptManager.NpcShopScripts.AddOrReplace(this);

			return true;
		}

		/// <summary>
		/// Called when creating the shop.
		/// </summary>
		public virtual void Setup()
		{
		}

		/// <summary>
		/// Adds empty tab.
		/// </summary>
		/// <param name="tabTitle">Tab title displayed in-game</param>
		/// <param name="randomizeColors">Determines whether item colors are randomized on midnight.</param>
		/// <param name="shouldDisplay">Function that determines whether tab should be displayed, set null if not used.</param>
		/// <returns></returns>
		public NpcShopTab Add(string tabTitle, bool randomizeColors, Func<Creature, NPC, bool> shouldDisplay = null)
		{
			var tab = new NpcShopTab(tabTitle, _tabs.Count, randomizeColors, shouldDisplay);
			lock (_tabs)
				_tabs.Add(tabTitle, tab);
			return tab;
		}

		/// <summary>
		/// Adds empty tab.
		/// </summary>
		/// <param name="tabTitle">Tab title displayed in-game</param>
		/// <param name="shouldDisplay">Function that determines whether tab should be displayed, set null if not used.</param>
		/// <returns></returns>
		public NpcShopTab Add(string tabTitle, Func<Creature, NPC, bool> shouldDisplay = null)
		{
			return Add(tabTitle, true, shouldDisplay);
		}

		/// <summary>
		/// Adds empty tabs.
		/// </summary>
		/// <param name="tabTitles"></param>
		/// <returns></returns>
		public void Add(params string[] tabTitles)
		{
			foreach (var title in tabTitles)
				this.Add(title);
		}

		/// <summary>
		/// Adds item to tab.
		/// </summary>
		/// <param name="tabTitle"></param>
		/// <param name="itemId"></param>
		/// <param name="amount"></param>
		/// <param name="price">Uses db value if lower than 0 (default).</param>
		/// <param name="stock">Amount of times item can be bough, unlimited if lower than 0 (default).</param>
		public void Add(string tabTitle, int itemId, int amount = 1, int price = -1, int stock = -1)
		{
			var item = new Item(itemId);
			item.Amount = amount;

			this.Add(tabTitle, item, price, stock);
		}

		/// <summary>
		/// Adds quest scroll item to tab.
		/// </summary>
		/// <param name="tabTitle"></param>
		/// <param name="itemId"></param>
		/// <param name="amount"></param>
		/// <param name="price"></param>
		/// <param name="stock">Amount of times item can be bough, unlimited if lower than 0 (default).</param>
		public void AddQuest(string tabTitle, int questId, int price, int stock = -1)
		{
			var item = Item.CreateQuestScroll(questId);

			this.Add(tabTitle, item, price, stock);
		}

		/// <summary>
		/// Adds item to tab.
		/// </summary>
		/// <param name="tabTitle"></param>
		/// <param name="itemId"></param>
		/// <param name="color1"></param>
		/// <param name="color2"></param>
		/// <param name="color3"></param>
		/// <param name="price">Uses db value if lower than 0 (default).</param>
		/// <param name="stock">Amount of times item can be bough, unlimited if lower than 0 (default).</param>
		public void Add(string tabTitle, int itemId, uint color1, uint color2, uint color3, int price = -1, int stock = -1)
		{
			var item = new Item(itemId);
			item.Info.Color1 = color1;
			item.Info.Color2 = color2;
			item.Info.Color3 = color3;

			this.Add(tabTitle, item, price, stock);
		}

		/// <summary>
		/// Adds item to tab.
		/// </summary>
		/// <param name="tabTitle"></param>
		/// <param name="itemId"></param>
		/// <param name="metaData"></param>
		/// <param name="price">Uses db value if lower than 0 (default).</param>
		/// <param name="stock">Amount of times item can be bough, unlimited if lower than 0 (default).</param>
		public void Add(string tabTitle, int itemId, string metaData, int price = -1, int stock = -1)
		{
			var item = new Item(itemId);
			item.MetaData1.Parse(metaData);

			this.Add(tabTitle, item, price, stock);
		}

		/// <summary>
		/// Adds item to tab.
		/// </summary>
		/// <param name="tabTitle"></param>
		/// <param name="item"></param>
		/// <param name="price">Uses db value if lower than 0.</param>
		/// <param name="stock">Amount of times item can be bough, unlimited if lower than 0.</param>
		public void Add(string tabTitle, Item item, int price, int stock)
		{
			var tab = this.GetOrCreateTab(tabTitle);

			// Use data price if none was set
			if (price == -1)
				price = item.Data.Price;

			// Set stock to given amount or unlimited
			item.Stock = (stock <= 0 ? -1 : stock);

			// Set the price we need
			switch (tab.PaymentMethod)
			{
				case PaymentMethod.Gold: item.SetGoldPrice(price); break;
				case PaymentMethod.Stars: item.OptionInfo.StarPrice = price; break;
				case PaymentMethod.Ducats: item.OptionInfo.DucatPrice = price; break;
				case PaymentMethod.Points: item.OptionInfo.PointPrice = price; break;
			}

			tab.Add(item);
		}

		/// <summary>
		/// Sets the payment method for the given tab.
		/// </summary>
		/// <param name="tabTitle"></param>
		/// <param name="method"></param>
		public void SetPaymentMethod(string tabTitle, PaymentMethod method)
		{
			var tab = this.GetOrCreateTab(tabTitle);
			tab.PaymentMethod = method;
		}

		/// <summary>
		/// Returns the given tab, after creating if necessary.
		/// </summary>
		/// <param name="tabTitle"></param>
		/// <returns></returns>
		protected NpcShopTab GetOrCreateTab(string tabTitle)
		{
			NpcShopTab tab;
			lock (_tabs)
				_tabs.TryGetValue(tabTitle, out tab);

			if (tab == null)
				tab = this.Add(tabTitle);

			return tab;
		}

		/// <summary>
		/// Removes all items from tab.
		/// </summary>
		/// <remarks>
		/// This, just like adding items at run-time, does not live update
		/// the currently open shops. Afaik it's not on officials either,
		/// but it would be possible, if we create a list of open shop
		/// instances and use the clear and fill shop packets.
		/// </remarks>
		/// <param name="tabTitle"></param>
		/// <returns>Whether clearing was successful. Fails if tab doesn't exist.</returns>
		protected bool ClearTab(string tabTitle)
		{
			NpcShopTab tab;
			lock (_tabs)
				_tabs.TryGetValue(tabTitle, out tab);

			if (tab == null)
				return false;

			tab.Clear();

			return true;
		}

		/// <summary>
		/// Returns item from one of the tabs by id.
		/// or null.
		/// </summary>
		/// <param name="entityId"></param>
		/// <returns></returns>
		private Item GetItem(long entityId)
		{
			lock (_tabs)
			{
				foreach (var tab in _tabs.Values)
				{
					var item = tab.Get(entityId);
					if (item != null)
						return item;
				}
			}

			return null;
		}

		/// <summary>
		/// Sends OpenNpcShop for creature and this shop.
		/// </summary>
		/// <param name="creature">Creature opening the shop</param>
		/// <param name="owner">NPC owning the shop</param>
		public void OpenFor(Creature creature, NPC owner)
		{
			// Shops without tabs are weird.
			if (_tabs.Count == 0)
				this.Add("Empty");

			creature.Temp.CurrentShop = this;
			creature.Temp.CurrentShopOwner = owner;

			Send.OpenNpcShop(creature, this.GetTabs(creature, owner));
		}

		/// <summary>
		/// Sends OpenShopRemotelyR and OpenNpcShop for creature and this shop.
		/// </summary>
		/// <param name="creature">Creature opening the shop</param>
		public void OpenRemotelyFor(Creature creature)
		{
			Send.OpenShopRemotelyR(creature, true);
			this.OpenFor(creature, null);
		}

		/// <summary>
		/// Returns thread-safe list of all tabs.
		/// </summary>
		/// <returns></returns>
		protected IList<NpcShopTab> GetTabs()
		{
			return this.GetTabs(null, null);
		}

		/// <summary>
		/// Returns thread-safe list of visible tabs, or all tabs if one of
		/// the parameters is null.
		/// </summary>
		/// <remarks>
		/// TODO: This could be cached.
		/// </remarks>
		/// <param name="creature">Creature opening the shop</param>
		/// <param name="owner">NPC owning the shop</param>
		/// <returns></returns>
		protected IList<NpcShopTab> GetTabs(Creature creature, NPC owner)
		{
			lock (_tabs)
				return creature == null || owner == null
					? _tabs.Values.ToList()
					: _tabs.Values.Where(t => t.ShouldDisplay(creature, owner)).ToList();
		}

		/// <summary>
		/// 
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="itemEntityId"></param>
		/// <param name="moveToInventory"></param>
		/// <returns></returns>
		public bool Buy(Creature creature, long itemEntityId, bool moveToInventory)
		{
			var shop = this;
			var owner = creature.Temp.CurrentShopOwner;

			// Get item
			// In theory someone could buy an item without it being visible
			// to him, but he would need the current entity id that
			// changes on each restart. It's unlikely to ever be a problem.
			var item = shop.GetItem(itemEntityId);
			if (item == null)
			{
				Log.Warning("NpcShopScript.Buy: Item '{0:X16}' doesn't exist in shop.", itemEntityId);
				return false;
			}

			// Check stock
			if (item.Stock == 0)
			{
				Send.MsgBox(creature, Localization.Get("This item is not in stock anymore."));
				return false;
			}

			// Determine which payment method to use, the same way the client
			// does to display them. Points > Stars > Ducats > Gold.
			var paymentMethod = PaymentMethod.Gold;
			if (item.OptionInfo.StarPrice > 0)
				paymentMethod = PaymentMethod.Stars;
			if (item.OptionInfo.DucatPrice > 0)
				paymentMethod = PaymentMethod.Ducats;
			if (item.OptionInfo.PointPrice > 0)
				paymentMethod = PaymentMethod.Points;

			// Get buying price
			var price = int.MaxValue;
			switch (paymentMethod)
			{
				case PaymentMethod.Gold: price = item.OptionInfo.Price; break;
				case PaymentMethod.Stars: price = item.OptionInfo.StarPrice; break;
				case PaymentMethod.Ducats: price = item.OptionInfo.DucatPrice; break;
				case PaymentMethod.Points: price = item.OptionInfo.PointPrice; break;
			}

			// The client expects the price for a full stack to be sent
			// in the ItemOptionInfo, so we have to calculate the actual price here.
			if (item.Data.StackType == StackType.Stackable)
				price = (int)(price / (float)item.Data.StackMax * item.Amount);

			// Wednesday: Decrease in prices (5%) for items in NPC shops,
			// including Remote Shop Coupons and money deposit for Exploration Quests.
			if (ErinnTime.Now.Month == ErinnMonth.AlbanHeruin)
				price = (int)(price * 0.95f);

			// Check currency
			var canPay = false;
			switch (paymentMethod)
			{
				case PaymentMethod.Gold: canPay = (creature.Inventory.Gold >= price); break;
				case PaymentMethod.Stars: canPay = (creature.Inventory.Stars >= price); break;
				case PaymentMethod.Ducats: canPay = false; break; // TODO: Implement ducats.
				case PaymentMethod.Points: canPay = (creature.Points >= price); break;
			}

			if (!canPay)
			{
				switch (paymentMethod)
				{
					case PaymentMethod.Gold: Send.MsgBox(creature, Localization.Get("Insufficient amount of gold.")); break;
					case PaymentMethod.Stars: Send.MsgBox(creature, Localization.Get("Insufficient amount of stars.")); break;
					case PaymentMethod.Ducats: Send.MsgBox(creature, Localization.Get("Insufficient amount of ducats.")); break;
					case PaymentMethod.Points: Send.MsgBox(creature, Localization.Get("You don't have enough Pon.\nYou will need to buy more.")); break;
				}

				return false;
			}

			// Buy, adding item, and removing currency
			var success = false;

			var newItem = new Item(item);

			// Set guild data
			if (newItem.HasTag("/guild_robe/") && creature.Guild != null && creature.Guild.HasRobe)
			{
				// EBCL1:4:-11042446;EBCL2:4:-7965756;EBLM1:1:45;EBLM2:1:24;EBLM3:1:6;GLDNAM:s:Name;
				newItem.Info.Color1 = creature.Guild.Robe.RobeColor;
				newItem.Info.Color2 = GuildRobe.GetColor(creature.Guild.Robe.BadgeColor);
				newItem.Info.Color3 = GuildRobe.GetColor(creature.Guild.Robe.EmblemMarkColor);
				newItem.MetaData1.SetInt("EBCL1", (int)GuildRobe.GetColor(creature.Guild.Robe.EmblemOutlineColor));
				newItem.MetaData1.SetInt("EBCL2", (int)GuildRobe.GetColor(creature.Guild.Robe.StripesColor));
				newItem.MetaData1.SetByte("EBLM1", creature.Guild.Robe.EmblemMark);
				newItem.MetaData1.SetByte("EBLM2", creature.Guild.Robe.EmblemOutline);
				newItem.MetaData1.SetByte("EBLM3", creature.Guild.Robe.Stripes);
				newItem.MetaData1.SetString("GLDNAM", creature.Guild.Name);
			}

			// Cursor
			if (!moveToInventory)
				success = creature.Inventory.Add(newItem, Pocket.Cursor);
			// Inventory
			else
				success = creature.Inventory.Add(newItem, false);

			if (success)
			{
				// Reset gold price if payment method wasn't gold, as various
				// things depend on the gold price, like repair prices.
				// If any payment method but gold was used, the gold price
				// would be 0.
				if (paymentMethod != PaymentMethod.Gold)
					newItem.ResetGoldPrice();

				// Reduce gold/points
				switch (paymentMethod)
				{
					case PaymentMethod.Gold: creature.Inventory.Gold -= price; break;
					case PaymentMethod.Stars: creature.Inventory.Stars -= price; break;
					case PaymentMethod.Ducats: break; // TODO: Implement ducats.
					case PaymentMethod.Points: creature.Points -= price; break;
				}

				// Reduce stock
				if (item.Stock > 0)
				{
					// Don't let it go below 0, that would mean unlimited.
					item.Stock = Math.Max(0, item.Stock - 1);
					if (item.Stock == 0)
					{
						// Refresh shop, so the item disappears.
						Send.ClearNpcShop(creature);
						Send.AddToNpcShop(creature, this.GetTabs(creature, owner));
					}

					Send.ServerMessage(creature, "Debug: Stock remaining: {0}", item.Stock);
				}
			}

			return success;
		}
	}

	/// <summary>
	/// Represents tab in an NPC shop, containing items.
	/// </summary>
	public class NpcShopTab
	{
		private Dictionary<long, Item> _items;

		/// <summary>
		/// Title of the tab.
		/// </summary>
		public string Title { get; protected set; }

		/// <summary>
		/// Index number in official tabs... order? (to be tested)
		/// </summary>
		public int Order { get; protected set; }

		/// <summary>
		/// Function that determines whether tab should be displayed.
		/// </summary>
		public Func<Creature, NPC, bool> ShouldDisplay { get; protected set; }

		/// <summary>
		/// Gets or sets whether item colors get randomized on RandomizeItemColors.
		/// </summary>
		public bool RandomizeColorsEnabled { get; set; }

		/// <summary>
		/// Gets or sets what items in this tab are paid for with.
		/// </summary>
		public PaymentMethod PaymentMethod { get; set; }

		/// <summary>
		/// Creatures new NpcShopTab
		/// </summary>
		/// <param name="title">Tab title display in-game.</param>
		/// <param name="order"></param>
		/// <param name="display">Function that determines whether tab should be displayed, set null if not used.</param>
		public NpcShopTab(string title, int order, bool randomizeColors, Func<Creature, NPC, bool> display)
		{
			_items = new Dictionary<long, Item>();
			this.Title = title;
			this.Order = order;
			this.RandomizeColorsEnabled = randomizeColors;
			this.ShouldDisplay = display ?? ((c, n) => true);
		}

		/// <summary>
		/// Adds item.
		/// </summary>
		/// <param name="item"></param>
		public void Add(Item item)
		{
			lock (_items)
				_items[item.EntityId] = item;
		}

		/// <summary>
		/// Removes all items from tab.
		/// </summary>
		public void Clear()
		{
			lock (_items)
				_items.Clear();
		}

		/// <summary>
		/// Returns item by entity id, or null.
		/// </summary>
		/// <param name="entityId"></param>
		/// <returns></returns>
		public Item Get(long entityId)
		{
			Item result;
			lock (_items)
				_items.TryGetValue(entityId, out result);
			return result;
		}

		/// <summary>
		/// Returns thread-safe list of all items in the tab.
		/// </summary>
		public ICollection<Item> GetItems()
		{
			lock (_items)
				return _items.Values.Where(a => a.Stock != 0).ToList();
		}

		/// <summary>
		/// Randomizes all item's colors.
		/// </summary>
		public void RandomizeItemColors()
		{
			if (!this.RandomizeColorsEnabled)
				return;

			var rand = RandomProvider.Get();

			lock (_items)
			{
				foreach (var item in _items.Values)
				{
					item.Info.Color1 = AuraData.ColorMapDb.GetRandom(item.Data.ColorMap1, rand);
					item.Info.Color2 = AuraData.ColorMapDb.GetRandom(item.Data.ColorMap2, rand);
					item.Info.Color3 = AuraData.ColorMapDb.GetRandom(item.Data.ColorMap3, rand);
				}
			}
		}
	}

	public enum PaymentMethod
	{
		Gold,
		Stars,
		Ducats,
		Points,
	}
}
