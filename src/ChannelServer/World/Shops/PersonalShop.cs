// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.World.Entities;
using Aura.Channel.World.Entities.Props;
using Aura.Channel.World.Inventory;
using Aura.Data;
using Aura.Data.Database;
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
		private const int DefaultShopPropId = 314;

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
		/// Data for the license used by this shop.
		/// </summary>
		public ShopLicenseData LicenseData { get; private set; }

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
		/// The creature that whatches over the shop besides the owner.
		/// </summary>
		public Creature Overseer { get; private set; }

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
				return (this.GetPricedItems().Count != 0);
			}
		}

		/// <summary>
		/// Creates new PersonalShop instance. Does not actually create
		/// shop.
		/// </summary>
		/// <param name="owner"></param>
		public PersonalShop(Creature owner, Item bag, Item license)
		{
			this.LicenseData = AuraData.ShopLicenseDb.Find(license.Data.PersonalShopLicense);
			if (this.LicenseData == null)
				throw new NotSupportedException("Unknown license '" + license.Data.PersonalShopLicense + "'.");

			this.CustomerEntityIds = new HashSet<long>();

			this.Owner = owner;
			this.Bag = bag;
			this.LicenseItem = license;

			this.Region = this.Owner.Region;

			this.Owner.Temp.ActivePersonalShop = this;
		}

		/// <summary>
		/// Returns new list of all items with prices in shop's bag.
		/// </summary>
		/// <returns></returns>
		public List<Item> GetPricedItems()
		{
			return this.Owner.Inventory.GetItems(a => a.Info.Pocket == this.Bag.OptionInfo.LinkedPocketId && a.PersonalShopPrice != 0);
		}

		/// <summary>
		/// Returns new list of all items in shop's bag.
		/// </summary>
		/// <returns></returns>
		public List<Item> GetAllItems()
		{
			return this.Owner.Inventory.GetItems(a => a.Info.Pocket == this.Bag.OptionInfo.LinkedPocketId);
		}

		/// <summary>
		/// Returns true if creature can place shop at their current location.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="license"></param>
		/// <returns></returns>
		public static bool CanPlace(Creature creature, string license)
		{
			var licenseData = AuraData.ShopLicenseDb.Find(license);
			if (licenseData == null)
			{
				Log.Warning("PersonalShop.CanPlace: Unknown license '{0}'.", license);
				return false;
			}

			var region = creature.Region;
			var pos = creature.GetPosition();
			var placementPos = GetPlacementPosition(pos, creature.Direction);

			// Check nearby shops
			var otherShops = region.GetProps(a => a.HasTag("/personal_shop/") && a.GetPosition().InRange(placementPos, 250));
			if (otherShops.Count != 0)
				return false;

			// Allow anywhere? (GM license)
			if (licenseData.AllowAnywhere)
				return true;

			// Only allow in specific regions
			if (!licenseData.Regions.Contains(region.Id))
				return false;

			var isOnStreet = region.IsOnStreet(pos);

			// Allow if not on street
			if (!isOnStreet)
				return true;

			// Only allow on street if inside certain allowed events
			var isInAllowedZone = false;
			foreach (var zone in licenseData.Zones)
			{
				// Get event
				var ev = region.GetClientEvent(a => a.GlobalName == zone);
				if (ev == null)
				{
					Log.Warning("PersonalShop.CanPlace: Unknown zone '{0}'.", zone);
					continue;
				}

				// Break if allowed zone was found
				if (ev.IsInside(pos))
				{
					isInAllowedZone = true;
					break;
				}
			}

			return isInAllowedZone;
		}

		/// <summary>
		/// Returns position for the shop, based on given position and direction.
		/// </summary>
		/// <param name="pos"></param>
		/// <param name="direction"></param>
		/// <returns></returns>
		private static Position GetPlacementPosition(Position pos, byte direction)
		{
			var radians = MabiMath.ByteToRadian(direction);
			var x = pos.X + 50 * Math.Cos(radians);
			var y = pos.Y + 50 * Math.Sin(radians);

			return new Position((int)x, (int)y);
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
		/// <param name="description"></param>
		public void ChangeDescription(string description)
		{
			this.Description = description;
			this.ForAllCustomers(creature => Send.PersonalShopUpdateDescription(creature, description));
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

			// Remove overseer
			this.SetOverseer(null, 0);

			// Close for everybody
			this.ForAllCustomers(creature => Send.PersonalShopCloseWindow(creature));

			// Reset item prices
			var items = this.GetPricedItems();
			foreach (var item in items)
				item.PersonalShopPrice = 0;

			this.Owner.Temp.ActivePersonalShop = null;
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

			var rnd = RandomProvider.Get();
			var pos = this.Owner.GetPosition();
			var location = new Location(this.Owner.RegionId, GetPlacementPosition(pos, this.Owner.Direction));
			var direction = MabiMath.ByteToRadian(this.Owner.Direction);

			// Spawn prop
			var propId = GetShopPropId(this.Bag);
			this.Prop = new Prop(propId, location.RegionId, location.X, location.Y, (float)direction);
			this.Prop.Info.Color1 = this.Bag.Info.Color1;
			this.Prop.Info.Color2 = this.Bag.Info.Color2;
			this.Prop.Info.Color3 = this.Bag.Info.Color3;
			this.Prop.Extensions.AddSilent(new ConfirmationPropExtension("default", "이 개인상점을 여시겠습니까?", "개인상점 열기"));
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
		/// Returns the prop id to be used for the given bag.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		private static int GetShopPropId(Item item)
		{
			if (item.Data.PersonalShopProp != 0)
				return item.Data.PersonalShopProp;

			return DefaultShopPropId;
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

			this.UpdatePrice(item, price);

			return true;
		}

		/// <summary>
		/// Sets the price for the given item, returns true on success.
		/// </summary>
		/// <param name="itemEntityId"></param>
		/// <param name="price"></param>
		/// <returns></returns>
		public bool SetPrices(long itemEntityId, int price)
		{
			var refItem = this.Owner.Inventory.GetItem(itemEntityId);
			if (refItem == null || refItem.Info.Pocket != this.Bag.OptionInfo.LinkedPocketId)
				return false;

			// Update all items with the same item id as the reference item.
			var items = this.GetAllItems();
			foreach (var item in items.Where(a => a.Info.Id == refItem.Info.Id))
				this.UpdatePrice(item, price);

			return true;
		}

		/// <summary>
		/// Updates price for given item and updates clients.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="price"></param>
		private void UpdatePrice(Item item, int price)
		{
			var prev = item.PersonalShopPrice;

			item.PersonalShopPrice = price;
			Send.PersonalShopPriceUpdate(this.Owner, item.EntityId, price);

			// Add, remove, or update item, depending on how the price has changed.
			if (prev == 0 && price != 0)
				this.ForAllCustomers(creature => Send.PersonalShopAddItem(creature, item));
			else if (prev != 0 && price == 0)
				this.ForAllCustomers(creature => Send.PersonalShopRemoveItem(creature, item.EntityId, 0));
			else
				this.ForAllCustomers(creature => Send.PersonalShopCustomerPriceUpdate(creature, item.EntityId, price));
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
		/// <param name="directBankTransaction"></param>
		/// <returns></returns>
		public bool Buy(Creature buyer, long itemEntityId, bool directBankTransaction)
		{
			var item = this.Owner.Inventory.GetItem(a => a.PersonalShopPrice != 0 && a.Info.Pocket == this.Bag.OptionInfo.LinkedPocketId && a.EntityId == itemEntityId);
			if (item == null)
				return false;

			// As soon as you click buy the item is removed on the client,
			// it has to be readded if something goes wrong.

			// Check payment method
			if (directBankTransaction && !this.LicenseData.DirectBankAllowed)
			{
				Send.PersonalShopAddItem(buyer, item);
				Send.MsgBox(buyer, Localization.Get("This shop doesn't allow Direct Bank Transaction."));
				return false;
			}

			// Check for empty cursor
			if (buyer.Inventory.GetItemAt(Pocket.Cursor, 0, 0) != null)
			{
				Send.PersonalShopAddItem(buyer, item);
				Send.MsgBox(buyer, Localization.Get("Failed to buy item."));
				return false;
			}

			var gold = 0;
			var price = item.PersonalShopPrice;
			var cost = price;

			// Disable direct bank transaction if price is less than 50k
			if (directBankTransaction && price < 50000)
				directBankTransaction = false;

			// Check gold
			if (directBankTransaction)
			{
				gold = buyer.Client.Account.Bank.Gold;
				cost = cost + (int)(cost * 0.05f); // Fee
			}
			else
				gold = buyer.Inventory.Gold;

			if (gold < cost)
			{
				Send.PersonalShopAddItem(buyer, item);
				Send.MsgBox(buyer, Localization.Get("You don't have enough gold."));
				return false;
			}

			// Remove item from shop
			this.Owner.Inventory.Remove(item);
			Send.PersonalShopRemoveItem(this.Owner, item.EntityId, buyer.EntityId);
			this.ForAllCustomers(creature => Send.PersonalShopRemoveItem(creature, item.EntityId, buyer.EntityId));

			// Remove gold and give item
			if (directBankTransaction)
				buyer.Client.Account.Bank.RemoveGold(buyer, cost);
			else
				buyer.Inventory.RemoveGold(cost);
			buyer.Inventory.Add(new Item(item), Pocket.Cursor);

			// Notice to owner
			var msg = string.Format(Localization.Get("[{0}] was sold to [{1}]."), Localization.Get(item.Data.Name), buyer.Name);
			Send.Notice(this.Owner, msg);
			Send.SystemMessageFrom(this.Owner, "<PERSONALSHOP>", msg);

			// Add gold to the license
			var fee = this.LicenseData.SalesFee;
			var revenue = (int)(price - price * fee);

			// Cap at limit
			var val = this.LicenseItem.MetaData1.GetInt("EVALUE") + revenue;
			val = Math.Min(this.LicenseData.Limit, val);

			this.LicenseItem.MetaData1.SetInt("EVALUE", revenue);
			Send.ItemUpdate(this.Owner, this.LicenseItem);

			return true;
		}

		/// <summary>
		/// Sets the shop's overseer (brownie/pet).
		/// </summary>
		/// <param name="overseer">Set to null to remove overseer.</param>
		public bool SetOverseer(Creature overseer, long itemEntityId)
		{
			if (overseer == null)
			{
				if (this.Overseer != null)
					this.Region.RemoveCreature(this.Overseer);

				Send.PersonalShopUpdateBrownie(this.Owner, 0, 0);
			}
			else
			{
				if (overseer == this.Owner || overseer.Region != this.Region)
					return false;

				Send.PersonalShopUpdateBrownie(this.Owner, itemEntityId, overseer.EntityId);
			}

			this.Overseer = overseer;

			return true;
		}
	}
}
