// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Network.Sending.Helpers;
using Aura.Channel.World;
using Aura.Channel.World.Entities;
using Aura.Channel.World.Entities.Props;
using Aura.Mabi.Network;
using Aura.Shared.Network;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aura.Channel.Network.Handlers
{
	public partial class ChannelServerHandlers : PacketHandlerManager<ChannelClient>
	{
		/// <summary>
		/// Initiates trade, sent when clicking Exchange on another player.
		/// </summary>
		/// <example>
		/// 001 [0010000000017000] Long   : 4503599627464704
		/// 002 [0000000000000000] Long   : 0
		/// </example>
		[PacketHandler(Op.TradeStart)]
		public void TradeStart(ChannelClient client, Packet packet)
		{
			var tradePartnerEntityId = packet.GetLong();
			var unkLong = packet.GetLong();

			var creature = client.GetCreatureSafe(packet.Id);

			// Check trade status
			if (creature.Temp.ActiveTrade != null)
			{
				Log.Warning("TradeStart: Double trade start request from user '{0}'.", client.Account.Id);
				Send.MsgBox(creature, Localization.Get("You're already trading with someone."));
				Send.TradeStartR(creature, false);
				return;
			}

			// Get trade partner
			var partner = creature.Region.GetCreature(tradePartnerEntityId);
			if (partner == null || !partner.IsPlayer || partner == creature)
			{
				Log.Warning("TradeStart: User '{0}' requested trade with invalid creature.", client.Account.Id);
				Send.MsgBox(creature, Localization.Get("Player not found."));
				Send.TradeStartR(creature, false);
				return;
			}

			// Check trade status
			if (partner.Temp.ActiveTrade != null)
			{
				Send.MsgBox(creature, Localization.Get("The Player is already trading with someone."));
				Send.TradeStartR(creature, false);
				return;
			}

			// Initiate trade
			var trade = new Trade(creature, partner);
			trade.Initiate();

			Send.TradeStartR(creature, true);
		}

		/// <summary>
		/// Sent when canceling a trade, e.g. by closing the window or
		/// refusing a trade request.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="packet"></param>
		[PacketHandler(Op.TradeCancel)]
		public void TradeCancel(ChannelClient client, Packet packet)
		{
			var unkByte = packet.GetByte();

			var creature = client.GetCreatureSafe(packet.Id);
			var trade = creature.Temp.ActiveTrade;

			// Check trade
			if (trade == null)
			{
				// Don't warn, sent by both parties on cancelation,
				// when we already removed both from the trade.
				//Log.Warning("TradeCancel: User '{0}' tried to cancel trade without being in one.", client.Account.Id);

				Send.TradeCancelR(creature, false);
				return;
			}

			trade.Cancel(creature);
		}

		/// <summary>
		/// Sent when accepting a trade request.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="packet"></param>
		[PacketHandler(Op.TradeAcceptRequest)]
		public void TradeAcceptRequest(ChannelClient client, Packet packet)
		{
			var unkByte = (byte)0;
			if (packet.Peek() != PacketElementType.None)
				unkByte = packet.GetByte();

			var creature = client.GetCreatureSafe(packet.Id);
			var trade = creature.Temp.ActiveTrade;

			// Check trade
			if (trade == null)
			{
				// Don't warn, sent when creature 2 accepts while creature 1
				// already canceled the trade.
				//Log.Warning("TradeCancel: User '{0}' tried to accept invalid trade request.", client.Account.Id);

				Send.MsgBox(creature, Localization.Get("The tread has been canceled."));
				Send.TradeAcceptRequestR(creature, false);
				return;
			}

			trade.Accept();
		}
	}
}
