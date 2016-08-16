// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Channel.Network.Sending.Helpers;
using Aura.Channel.World.Entities;
using Aura.Shared.Network;
using Aura.Channel.World;
using Aura.Mabi.Network;
using Aura.Channel.World.Shops;

namespace Aura.Channel.Network.Sending
{
	public static partial class Send
	{
		/// <summary>
		/// Sends PersonalShopPriceUpdate to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="itemEntityId"></param>
		/// <param name="price"></param>
		public static void PersonalShopPriceUpdate(Creature creature, long itemEntityId, int price)
		{
			var packet = new Packet(Op.PersonalShopPriceUpdate, creature.EntityId);
			packet.PutLong(itemEntityId);
			packet.PutInt(price);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends PersonalShopCustomerPriceUpdate to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="itemEntityId"></param>
		/// <param name="price"></param>
		public static void PersonalShopCustomerPriceUpdate(Creature creature, long itemEntityId, int price)
		{
			var packet = new Packet(Op.PersonalShopCustomerPriceUpdate, creature.EntityId);
			packet.PutLong(itemEntityId);
			packet.PutInt(price);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends PersonalShopSetPriceR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void PersonalShopSetPriceR(Creature creature, bool success)
		{
			var packet = new Packet(Op.PersonalShopSetPriceR, creature.EntityId);
			packet.PutByte(success);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends positive PersonalShopSetUpR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="shopProp"></param>
		public static void PersonalShopSetUpR(Creature creature, Prop shopProp)
		{
			var location = shopProp.GetLocation();

			var packet = new Packet(Op.PersonalShopSetUpR, creature.EntityId);
			packet.PutByte(true);
			packet.PutLong(shopProp.EntityId);
			packet.PutByte(1); // no location if 0?
			packet.PutInt(location.RegionId);
			packet.PutInt(location.X);
			packet.PutInt(location.Y);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends negative PersonalShopSetUpR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		/// <param name="propEntityId"></param>
		/// <param name="location"></param>
		public static void PersonalShopSetUpR_Fail(Creature creature)
		{
			var packet = new Packet(Op.PersonalShopSetUpR, creature.EntityId);
			packet.PutByte(false);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends PersonalShopUpdateBrownie to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="licenseEntityId"></param>
		/// <param name="brownieNpcEntityId"></param>
		public static void PersonalShopUpdateBrownie(Creature creature, long licenseEntityId, long brownieNpcEntityId)
		{
			var packet = new Packet(Op.PersonalShopUpdateBrownie, creature.EntityId);
			packet.PutLong(licenseEntityId);
			packet.PutLong(brownieNpcEntityId);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends PersonalShopTakeDownR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void PersonalShopTakeDownR(Creature creature, bool success)
		{
			var packet = new Packet(Op.PersonalShopTakeDownR, creature.EntityId);
			packet.PutByte(success);
			packet.PutByte(0);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends PersonalShopCheckR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		/// <param name="bagEntityId"></param>
		/// <param name="licenseEntityId"></param>
		public static void PersonalShopCheckR(Creature creature, bool success, long bagEntityId, long licenseEntityId)
		{
			var packet = new Packet(Op.PersonalShopCheckR, creature.EntityId);
			packet.PutByte(success);
			packet.PutLong(bagEntityId);
			packet.PutLong(licenseEntityId);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends PersonalShopOpenR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="shop">Null for negative response</param>
		public static void PersonalShopOpenR(Creature creature, PersonalShop shop)
		{
			var items = shop.GetPricedItems();

			var packet = new Packet(Op.PersonalShopOpenR, creature.EntityId);
			packet.PutByte(shop != null);
			if (shop != null)
			{
				packet.PutLong(shop.Owner.EntityId);
				packet.PutString(shop.Owner.Name);
				packet.PutString(shop.Description);
				packet.PutString(shop.GetBagLayout());
				packet.PutByte(0);

				foreach (var item in items)
				{
					packet.AddItemInfo(item, ItemPacketType.Private);
					packet.PutInt(item.PersonalShopPrice);
				}
			}

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends PersonalShopUpdateDescription to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="description"></param>
		public static void PersonalShopUpdateDescription(Creature creature, string description)
		{
			var packet = new Packet(Op.PersonalShopUpdateDescription, creature.EntityId);
			packet.PutString(description);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends PersonalShopAddItem to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="item"></param>
		public static void PersonalShopAddItem(Creature creature, Item item)
		{
			var packet = new Packet(Op.PersonalShopAddItem, creature.EntityId);
			packet.AddItemInfo(item, ItemPacketType.Private);
			packet.PutInt(item.PersonalShopPrice);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends PersonalShopRemoveItem to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="itemEntityId"></param>
		/// <param name="buyerEntityId"></param>
		public static void PersonalShopRemoveItem(Creature creature, long itemEntityId, long buyerEntityId)
		{
			var packet = new Packet(Op.PersonalShopRemoveItem, creature.EntityId);
			packet.PutLong(itemEntityId);
			packet.PutLong(buyerEntityId);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends PersonalShopBuyR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		/// <param name="itemEntityId"></param>
		public static void PersonalShopBuyR(Creature creature, bool success, long itemEntityId)
		{
			var packet = new Packet(Op.PersonalShopBuyR, creature.EntityId);
			packet.PutByte(success);
			packet.PutLong(itemEntityId);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends PersonalShopCloseWindow to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		public static void PersonalShopCloseWindow(Creature creature)
		{
			var packet = new Packet(Op.PersonalShopCloseWindow, creature.EntityId);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends PersonalShopCloseR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void PersonalShopCloseR(Creature creature, bool success)
		{
			var packet = new Packet(Op.PersonalShopCloseR, creature.EntityId);
			packet.PutByte(success);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends PersonalShopSetPriceForAllR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void PersonalShopSetPriceForAllR(Creature creature, bool success)
		{
			var packet = new Packet(Op.PersonalShopSetPriceForAllR, creature.EntityId);
			packet.PutByte(success);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends PersonalShopPricePetProtectRequestR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void PersonalShopPetProtectRequestR(Creature creature, bool success)
		{
			var packet = new Packet(Op.PersonalShopPetProtectRequestR, creature.EntityId);
			packet.PutByte(success);

			creature.Client.Send(packet);
		}
	}
}
