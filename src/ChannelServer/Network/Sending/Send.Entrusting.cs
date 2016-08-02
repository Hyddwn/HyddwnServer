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
	}
}
