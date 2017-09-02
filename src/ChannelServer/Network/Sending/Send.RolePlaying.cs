// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.World.Entities;
using Aura.Mabi.Const;
using Aura.Mabi.Network;

namespace Aura.Channel.Network.Sending
{
	public static partial class Send
	{
		/// <summary>
		/// Sends RequestSecondaryLogin to creature's client, requesting
		/// it to send a login packet for the given entity id.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="entityId">Entity to log in.</param>
		/// <param name="channelHost">Host of channel to log in to.</param>
		/// <param name="channelPort">Port of channel to log in to.</param>
		public static void RequestSecondaryLogin(Creature creature, long entityId, string channelHost, int channelPort)
		{
			Packet packet = new Packet(Op.RequestSecondaryLogin, MabiId.Channel);
			packet.PutLong(entityId);
			packet.PutString(channelHost);
			packet.PutShort((short)channelPort);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends StartRP to creature's client, to switch to control
		/// to given RP character.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="entityId"></param>
		public static void StartRP(Creature creature, long entityId)
		{
			Packet packet = new Packet(Op.StartRP, MabiId.Channel);
			packet.PutLong(entityId);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Send EndRP to creature's client, to end RP session.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="regionId"></param>
		public static void EndRP(Creature creature, int regionId)
		{
			Packet packet = new Packet(Op.EndRP, MabiId.Channel);
			packet.PutLong(creature.EntityId);
			packet.PutInt(regionId);

			creature.Client.Send(packet);
		}
	}
}
