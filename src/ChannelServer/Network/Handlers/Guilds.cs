// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.World;
using Aura.Channel.World.Entities;
using Aura.Mabi.Const;
using Aura.Mabi.Network;
using Aura.Shared.Network;
using Aura.Shared.Util;
using System.Linq;

namespace Aura.Channel.Network.Handlers
{
	public partial class ChannelServerHandlers : PacketHandlerManager<ChannelClient>
	{
		/// <summary>
		/// Sent when choosing to convert play points to guild points.
		/// </summary>
		/// <example>
		/// No parameters.
		/// </example>
		[PacketHandler(Op.ConvertGp)]
		public void ConvertGp(ChannelClient client, Packet packet)
		{
			var creature = client.GetCreatureSafe(packet.Id);

			if (creature.Guild == null)
			{
				Log.Warning("ConvertGp: User '{0}' is not in a guild.", client.Account.Id);
				Send.ConvertGpR(creature, false, 0);
				return;
			}

			Send.ConvertGpR(creature, true, creature.PlayPoints);
		}

		/// <summary>
		/// Sent when confirming the conversion of play points to guild points.
		/// </summary>
		/// <example>
		/// No parameters.
		/// </example>
		[PacketHandler(Op.ConvertGpConfirm)]
		public void ConvertGpConfirm(ChannelClient client, Packet packet)
		{
			var creature = client.GetCreatureSafe(packet.Id);

			if (creature.Guild == null)
			{
				Log.Warning("ConvertGpConfirm: User '{0}' is not in a guild.", client.Account.Id);
				Send.ConvertGpConfirmR(creature, false);
				return;
			}

			ChannelServer.Instance.GuildManager.ConvertPlayPoints(creature, creature.Guild);

			Send.ConvertGpConfirmR(creature, true);
		}
	}
}
