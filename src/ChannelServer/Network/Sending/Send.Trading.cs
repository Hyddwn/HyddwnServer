// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending.Helpers;
using Aura.Channel.World;
using Aura.Channel.World.Entities;
using Aura.Channel.World.Entities.Props;
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
		/// Sends TradeStartR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void TradeStartR(Creature creature, bool success)
		{
			var packet = new Packet(Op.TradeStartR, creature.EntityId);
			packet.PutByte(success);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends TradeInfo to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="tradeId"></param>
		/// <param name="tradePartnerEntityId"></param>
		public static void TradeInfo(Creature creature, long tradeId, long tradePartnerEntityId)
		{
			var packet = new Packet(Op.TradeInfo, creature.EntityId);
			packet.PutLong(tradeId);
			packet.PutLong(tradePartnerEntityId);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends TradeCancelR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void TradeCancelR(Creature creature, bool success)
		{
			var packet = new Packet(Op.TradeCancelR, creature.EntityId);
			packet.PutByte(success);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends TradeWait to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="milliseconds"></param>
		public static void TradeWait(Creature creature, int milliseconds)
		{
			var packet = new Packet(Op.TradeWait, creature.EntityId);
			packet.PutInt(milliseconds);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends TradeItemAdded to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="item"></param>
		public static void TradeItemAdded(Creature creature, Item item)
		{
			var packet = new Packet(Op.TradeItemAdded, creature.EntityId);
			packet.AddItemInfo(item, ItemPacketType.Private);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends TradeItemRemoved to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="itemEntityId"></param>
		public static void TradeItemRemoved(Creature creature, long itemEntityId)
		{
			var packet = new Packet(Op.TradeItemRemoved, creature.EntityId);
			packet.PutLong(itemEntityId);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends TradeReadied to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="entityId">Entity id of the creature that pressed Ready.</param>
		public static void TradeReadied(Creature creature, long entityId)
		{
			var packet = new Packet(Op.TradeReadied, creature.EntityId);
			packet.PutLong(entityId);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends TradeComplete to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		public static void TradeComplete(Creature creature)
		{
			var packet = new Packet(Op.TradeComplete, creature.EntityId);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends TradeReadyR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void TradeReadyR(Creature creature, bool success)
		{
			var packet = new Packet(Op.TradeReadyR, creature.EntityId);
			packet.PutByte(success);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends TradePartnerInfo to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="tradePartnerEntityId"></param>
		/// <param name="tradePartnerName"></param>
		public static void TradePartnerInfo(Creature creature, long tradePartnerEntityId, string tradePartnerName)
		{
			var packet = new Packet(Op.TradePartnerInfo, creature.EntityId);
			packet.PutLong(tradePartnerEntityId);
			packet.PutString(tradePartnerName);
			packet.PutInt(0);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends TradeRequest to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="tradePartnerEntityId"></param>
		/// <param name="tradePartnerName"></param>
		public static void TradeRequest(Creature creature, long tradePartnerEntityId, string tradePartnerName)
		{
			var packet = new Packet(Op.TradeRequest, creature.EntityId);
			packet.PutLong(tradePartnerEntityId);
			packet.PutString(tradePartnerName);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends TradeRequestCanceled to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void TradeRequestCanceled(Creature creature, bool success)
		{
			var packet = new Packet(Op.TradeRequestCanceled, creature.EntityId);
			packet.PutByte(success);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends TradeAcceptRequestR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void TradeAcceptRequestR(Creature creature, bool success)
		{
			var packet = new Packet(Op.TradeAcceptRequestR, creature.EntityId);
			packet.PutByte(success);

			creature.Client.Send(packet);
		}
	}
}
