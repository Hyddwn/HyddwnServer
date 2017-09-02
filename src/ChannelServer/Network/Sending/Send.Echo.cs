// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.World.Entities;
using Aura.Mabi.Network;

namespace Aura.Channel.Network.Sending
{
	public static partial class Send
	{
		/// <summary>
		/// Sends received packet back to creature.
		/// </summary>
		/// <remarks>
		/// Use these functions if you have a lengthy packet, that you have
		/// to send back 1:1, so we don't have to create a sender and recreate
		/// the packet every time.
		/// </remarks>
		/// <param name="creature"></param>
		/// <param name="packet"></param>
		public static void Echo(Creature creature, Packet packet)
		{
			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends received packet back to creature, after changing the op.
		/// </summary>
		/// <remarks>
		/// Use these functions if you have a lengthy packet, that you have
		/// to send back 1:1, so we don't have to create a sender and recreate
		/// the packet every time.
		/// </remarks>
		/// <param name="creature"></param>
		/// <param name="packet"></param>
		public static void Echo(Creature creature, int op, Packet packet)
		{
			packet.Op = op;
			creature.Client.Send(packet);
		}
	}
}
