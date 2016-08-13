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
		/// Sends EntrustmentR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		/// <param name="partner"></param>
		public static void EntrustmentR(Creature creature, bool success, Creature partner)
		{
			var packet = new Packet(Op.EntrustmentR, creature.EntityId);
			packet.PutByte(success);
			packet.PutLong(partner.EntityId);
			packet.PutString(partner.Name);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends EntrustmentR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void EntrustmentR(Creature creature, bool success)
		{
			var packet = new Packet(Op.EntrustmentR, creature.EntityId);
			packet.PutByte(success);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends EntrustmentChanceUpdate to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="chance"></param>
		/// <param name="unkShort"></param>
		public static void EntrustmentChanceUpdate(Creature creature, float chance, SkillRank skillRank)
		{
			var packet = new Packet(Op.EntrustmentChanceUpdate, creature.EntityId);
			packet.PutFloat(chance);
			packet.PutShort((short)skillRank);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends EntrustmentClose to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		public static void EntrustmentClose(Creature creature)
		{
			var packet = new Packet(Op.EntrustmentClose, creature.EntityId);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends EntrustmentRequest to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="partnerEntityId"></param>
		/// <param name="unkByte"></param>
		public static void EntrustmentRequest(Creature creature, long partnerEntityId, byte unkByte)
		{
			var packet = new Packet(Op.EntrustmentRequest, creature.EntityId);
			packet.PutLong(partnerEntityId);
			packet.PutByte(unkByte);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends EntrustmentAddItem to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="pocket"></param>
		/// <param name="item"></param>
		public static void EntrustmentAddItem(Creature creature, Pocket pocket, Item item)
		{
			var packet = new Packet(Op.EntrustmentAddItem, creature.EntityId);
			packet.PutByte((byte)pocket);
			packet.AddItemInfo(item, ItemPacketType.Private);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends EntrustmentRemoveItem to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="pocket"></param>
		/// <param name="itemEntityId"></param>
		public static void EntrustmentRemoveItem(Creature creature, Pocket pocket, long itemEntityId)
		{
			var packet = new Packet(Op.EntrustmentRemoveItem, creature.EntityId);
			packet.PutByte((byte)pocket);
			packet.PutLong(itemEntityId);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends EntrustmentRequestFinalized to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		public static void EntrustmentRequestFinalized(Creature creature)
		{
			var packet = new Packet(Op.EntrustmentRequestFinalized, creature.EntityId);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends EntrustmentFinalizing to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		public static void EntrustmentFinalizing(Creature creature)
		{
			var packet = new Packet(Op.EntrustmentFinalizing, creature.EntityId);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends EntrustmentEnd to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		public static void EntrustmentEnd(Creature creature)
		{
			var packet = new Packet(Op.EntrustmentEnd, creature.EntityId);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends EntrustmentAcceptRequestR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void EntrustmentAcceptRequestR(Creature creature, bool success)
		{
			var packet = new Packet(Op.EntrustmentAcceptRequestR, creature.EntityId);
			packet.PutByte(success);
			packet.PutByte(0);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends EntrustmentEnableRequest to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		public static void EntrustmentEnableRequest(Creature creature)
		{
			var packet = new Packet(Op.EntrustmentEnableRequest, creature.EntityId);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends EntrustmentDisableRequest to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		public static void EntrustmentDisableRequest(Creature creature)
		{
			var packet = new Packet(Op.EntrustmentDisableRequest, creature.EntityId);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends EntrustmentFinalizeRequestR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		public static void EntrustmentFinalizeRequestR(Creature creature, bool success)
		{
			var packet = new Packet(Op.EntrustmentFinalizeRequestR, creature.EntityId);
			packet.PutByte(success);

			creature.Client.Send(packet);
		}
	}
}
