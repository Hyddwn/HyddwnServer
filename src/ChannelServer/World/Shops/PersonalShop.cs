// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.World.Entities;
using Aura.Channel.World.Inventory;
using Aura.Mabi;
using Aura.Mabi.Const;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.World.Shops
{
	public class PersonalShop
	{
		/// <summary>
		/// Entity id of the shop owner.
		/// </summary>
		public Creature Owner { get; private set; }

		/// <summary>
		/// Bag in owner's inventory used for the shop.
		/// </summary>
		public Item Bag { get; private set; }

		/// <summary>
		/// License used for this shop.
		/// </summary>
		public Item LicenseItem { get; private set; }

		/// <summary>
		/// Shop's title.
		/// </summary>
		public string Title { get; private set; }

		/// <summary>
		/// Shop's description.
		/// </summary>
		public string Description { get; private set; }

		/// <summary>
		/// Region the shop is in.
		/// </summary>
		public Region Region { get; private set; }

		/// <summary>
		/// Reference to the shop's prop in the region.
		/// </summary>
		public Prop Prop { get; private set; }

		/// <summary>
		/// List of customers currently looking at the shop.
		/// </summary>
		public HashSet<long> CustomerEntityIds { get; private set; }

		/// <summary>
		/// Returns true if all shop is ready to be set up.
		/// </summary>
		public bool IsReadyForBusiness
		{
			get
			{
				return (this.GetItems().Count != 0);
			}
		}

		/// <summary>
		/// Creates new PersonalShop instance. Does not actually create
		/// shop.
		/// </summary>
		/// <param name="owner"></param>
		public PersonalShop(Creature owner, Item bag, Item license)
		{
			this.CustomerEntityIds = new HashSet<long>();

			this.Owner = owner;
			this.Bag = bag;
			this.LicenseItem = license;

			this.Region = this.Owner.Region;

			this.Owner.Temp.ActivePersonalShop = this;
		}

		/// <summary>
		/// Returns new list of all items in shop.
		/// </summary>
		/// <returns></returns>
		public List<Item> GetItems()
		{
			return this.Owner.Inventory.GetItems(a => a.Info.Pocket == this.Bag.OptionInfo.LinkedPocketId && a.PersonalShopPrice != 0);
		}

		/// <summary>
		/// Returns true if creature can place shop at their current location.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="license"></param>
		/// <returns></returns>
		public static bool CanPlace(Creature creature, string license)
		{
			if (!IsValidRegion(license, creature.RegionId))
				return false;

			// TODO: Check area.

			return true;
		}

		/// <summary>
		/// Returns true license is valid for the given region.
		/// </summary>
		/// <param name="license"></param>
		/// <param name="regionId"></param>
		/// <returns></returns>
		public static bool IsValidRegion(string license, int regionId)
		{
			var str = license.ToLower();

			if (str == "gm_all_region")
				return true;
			else if (str.StartsWith("tirchonaill") && regionId == 1)
				return true;
			else if (str.StartsWith("dunbarton") && regionId == 14)
				return true;
			else if (str.StartsWith("bangor") && regionId == 31)
				return true;
			else if (str.StartsWith("emainmacha") && regionId == 52)
				return true;
			else if (str.StartsWith("rano") && regionId == 3001)
				return true;
			else if (str.StartsWith("connous") && regionId == 3100)
				return true;
			else if (str.StartsWith("physis") && regionId == 3200)
				return true;
			else if (str.StartsWith("courcle") && regionId == 3300)
				return true;
			else if (str.StartsWith("zardine") && regionId == 3400)
				return true;
			else if (str.StartsWith("taillteann") && regionId == 300)
				return true;
			else if (str.StartsWith("tara") && regionId == 401)
				return true;
			else if (str.StartsWith("nekojima") && regionId == 600)
				return true;
			else if (str.StartsWith("cobh") && regionId == 23)
				return true;
			else if (str.StartsWith("belfast") && regionId == 4005)
				return true;

			return false;
		}

		/// <summary>
		/// Changes shop's title.
		/// </summary>
		/// <param name="title"></param>
		public void ChangeTitle(string title)
		{
			this.Title = title;

			if (this.Prop != null)
			{
				this.Prop.Xml.SetAttributeValue("PSTTL", title);
				Send.PropUpdate(this.Prop);
			}
		}

		/// <summary>
		/// Changes shop's description.
		/// </summary>
		/// <param name="title"></param>
		public void ChangeDescription(string description)
		{
			this.Description = description;
		}

		/// <summary>
		/// Closes the shop for all customers and takes it down, removing
		/// the prop.
		/// </summary>
		public void TakeDown()
		{
			// Remove prop
			if (this.Prop != null)
			{
				this.Prop.Region.RemoveProp(this.Prop);
				this.Prop = null;
			}

			// Close for everybody
			this.ForAllCustomers(creature => Send.PersonalShopCloseWindow(creature));

			// Reset item prices
			var items = this.GetItems();
			foreach (var item in items)
				item.PersonalShopPrice = 0;
		}

		/// <summary>
		/// Sets up shop, spawning the prop.
		/// </summary>
		/// <param name="title"></param>
		/// <param name="description"></param>
		public bool SetUp(string title, string description)
		{
			if (!this.IsReadyForBusiness)
				return false;

			this.Title = title;
			this.Description = description;

			var location = this.Owner.GetLocation();
			var rnd = RandomProvider.Get();

			// Spawn prop
			this.Prop = new Prop(314, location.RegionId, location.X, location.Y, MabiMath.DegreeToRadian(rnd.Next(360)));
			this.Region.AddProp(this.Prop);

			// Move that body
			Send.UseMotion(this.Owner, 11, 4);

			// Update prop with owner and title
			this.Prop.Xml.SetAttributeValue("PSPID", this.Owner.EntityId);
			this.Prop.Xml.SetAttributeValue("PSTTL", title);
			Send.PropUpdate(this.Prop);

			return true;
		}

		/// <summary>
		/// Sets the price for the given item, returns true on success.
		/// </summary>
		/// <param name="itemEntityId"></param>
		/// <param name="price"></param>
		/// <returns></returns>
		public bool SetPrice(long itemEntityId, int price)
		{
			var item = this.Owner.Inventory.GetItem(itemEntityId);
			if (item == null || item.Info.Pocket != this.Bag.OptionInfo.LinkedPocketId)
				return false;

			item.PersonalShopPrice = price;
			Send.PersonalShopPriceUpdate(this.Owner, itemEntityId, price);

			this.ForAllCustomers(creature => Send.PersonalShopCustomerPriceUpdate(creature, itemEntityId, price));

			return true;
		}

		/// <summary>
		/// Executes the given action for all current customers.
		/// </summary>
		/// <param name="action"></param>
		private void ForAllCustomers(Action<Creature> action)
		{
			if (this.CustomerEntityIds.Count == 0)
				return;

			var remove = new List<long>();

			lock (this.CustomerEntityIds)
			{
				foreach (var entityId in this.CustomerEntityIds)
				{
					var creature = this.Region.GetCreature(entityId);
					if (creature != null)
						action(creature);
					else
						remove.Add(entityId);
				}

				foreach (var id in remove)
					this.CustomerEntityIds.Remove(id);
			}
		}

		/// <summary>
		/// Opens shop for given creature.
		/// </summary>
		/// <param name="creature"></param>
		public void OpenFor(Creature creature)
		{
			lock (this.CustomerEntityIds)
				this.CustomerEntityIds.Add(creature.EntityId);
			Send.PersonalShopOpenR(creature, this);
		}

		/// <summary>
		/// Opens shop for given creature.
		/// </summary>
		/// <param name="creature"></param>
		public void CloseFor(Creature creature)
		{
			lock (this.CustomerEntityIds)
				this.CustomerEntityIds.Remove(creature.EntityId);
			Send.PersonalShopCloseWindow(creature);
		}

		/// <summary>
		/// Returns the layout of the bag, used in the shop open packet.
		/// </summary>
		/// <param name="x"></param>
		/// <param name="y"></param>
		public string GetBagLayout()
		{
			if (this.Bag == null)
			{
				Log.Debug("PersonalShop.GetBagLayout: Bag not set.");
				return null;
			}

			var linkedPocket = this.Bag.OptionInfo.LinkedPocketId;
			var pocket = this.Owner.Inventory.GetPocket(linkedPocket);
			if (pocket == null)
			{
				Log.Debug("PersonalShop.GetBagLayout: Pocket '{0}' not found.", linkedPocket);
				return null;
			}

			var normalPocket = pocket as InventoryPocketNormal;
			if (normalPocket == null)
			{
				Log.Debug("PersonalShop.GetBagLayout: Invalid pocket type '{0}'.", pocket.GetType().Name);
				return null;
			}

			// TODO: Non-rectangular bag layouts.

			return string.Format("{0}/{1}", normalPocket.Width, normalPocket.Height);
		}

		/// <summary>
		/// Attempts to buy the given item for buyer, returns true if
		/// successful.
		/// </summary>
		/// <param name="buyer"></param>
		/// <param name="itemEntityId"></param>
		/// <returns></returns>
		public bool Buy(Creature buyer, long itemEntityId)
		{
			var item = this.Owner.Inventory.GetItem(a => a.PersonalShopPrice != 0 && a.Info.Pocket == this.Bag.OptionInfo.LinkedPocketId && a.EntityId == itemEntityId);
			if (item == null)
				return false;

			// Check gold
			var price = item.PersonalShopPrice;
			if (buyer.Inventory.Gold < price)
			{
				// As soon as you click buy the item is removed on the client,
				// it has to be readded if something goes wrong.
				Send.PersonalShopAddItem(buyer, item);
				Send.MsgBox(buyer, Localization.Get("You don't have enough gold."));
				return false;
			}

			// Remove item from shop
			this.Owner.Inventory.Remove(item);
			Send.PersonalShopRemoveItem(this.Owner, item.EntityId, buyer.EntityId);
			this.ForAllCustomers(creature => Send.PersonalShopRemoveItem(creature, item.EntityId, buyer.EntityId));

			// Remove gold and give item
			buyer.Inventory.RemoveGold(price);
			buyer.GiveItem(new Item(item));

			// Notice to owner
			var msg = string.Format(Localization.Get("[{0}] was sold to [{1}]."), Localization.Get(item.Data.Name), buyer.Name);
			Send.Notice(this.Owner, msg);
			Send.SystemMessage(this.Owner, "<PERSONALSHOP>", msg);

			// Add gold to the license
			var revenue = (int)(price * 0.99f);
			var val = this.LicenseItem.MetaData1.GetInt("EVALUE") + revenue;
			this.LicenseItem.MetaData1.SetInt("EVALUE", revenue);
			Send.ItemUpdate(this.Owner, this.LicenseItem);

			return true;
		}
	}
}
