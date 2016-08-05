// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending.Helpers;
using Aura.Channel.World;
using Aura.Channel.World.Entities;
using Aura.Channel.World.Entities.Props;
using Aura.Mabi.Const;
using Aura.Mabi.Network;
using Aura.Shared.Network;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aura.Channel.Network.Sending
{
	public static partial class Send
	{
		/// <summary>
		/// Sends EntrustedEnchantR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		/// <param name="partner"></param>
		public static void EntrustedEnchantR(Creature creature, bool success, Creature partner)
		{
			var packet = new Packet(Op.EntrustedEnchantR, creature.EntityId);
			packet.PutByte(success);
			packet.PutLong(partner.EntityId);
			packet.PutString(partner.Name);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends EntrustedEnchantR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void EntrustedEnchantR(Creature creature, bool success)
		{
			var packet = new Packet(Op.EntrustedEnchantR, creature.EntityId);
			packet.PutByte(success);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends EntrustedEnchantChanceUpdate to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="chance"></param>
		/// <param name="unkShort"></param>
		public static void EntrustedEnchantChanceUpdate(Creature creature, float chance, short unkShort)
		{
			var packet = new Packet(Op.EntrustedEnchantChanceUpdate, creature.EntityId);
			packet.PutFloat(chance);
			packet.PutShort(unkShort);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends EntrustedEnchantClose to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		public static void EntrustedEnchantClose(Creature creature)
		{
			var packet = new Packet(Op.EntrustedEnchantClose, creature.EntityId);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends EntrustedEnchantRequest to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="partnerEntityId"></param>
		/// <param name="unkByte"></param>
		public static void EntrustedEnchantRequest(Creature creature, long partnerEntityId, byte unkByte)
		{
			var packet = new Packet(Op.EntrustedEnchantRequest, creature.EntityId);
			packet.PutLong(partnerEntityId);
			packet.PutByte(unkByte);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends EntrustedEnchantAddItem to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="pocket"></param>
		/// <param name="item"></param>
		public static void EntrustedEnchantAddItem(Creature creature, Pocket pocket, Item item)
		{
			var packet = new Packet(Op.EntrustedEnchantAddItem, creature.EntityId);
			packet.PutByte((byte)pocket);
			packet.AddItemInfo(item, ItemPacketType.Private);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends EntrustedEnchantRemoveItem to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="pocket"></param>
		/// <param name="itemEntityId"></param>
		public static void EntrustedEnchantRemoveItem(Creature creature, Pocket pocket, long itemEntityId)
		{
			var packet = new Packet(Op.EntrustedEnchantRemoveItem, creature.EntityId);
			packet.PutByte((byte)pocket);
			packet.PutLong(itemEntityId);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends EntrustedEnchantRequestFinalized to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		public static void EntrustedEnchantRequestFinalized(Creature creature)
		{
			var packet = new Packet(Op.EntrustedEnchantRequestFinalized, creature.EntityId);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends EntrustedEnchantAcceptRequestR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void EntrustedEnchantAcceptRequestR(Creature creature, bool success)
		{
			var packet = new Packet(Op.EntrustedEnchantAcceptRequestR, creature.EntityId);
			packet.PutByte(success);
			packet.PutByte(0);

			creature.Client.Send(packet);
		}
	}
}
