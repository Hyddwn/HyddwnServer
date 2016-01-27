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
		/// Dummy handler for TradeStart.
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

			Send.Notice(creature, Localization.Get("Trading isn't possible yet."));

			Send.TradeStartR(creature, false);
		}
	}
}
