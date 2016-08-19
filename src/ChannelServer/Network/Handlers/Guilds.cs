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

		/// <summary>
		/// Sent when clicking OK on the guild gold donation window.
		/// </summary>
		/// <example>
		/// 001 [........00000159] Int    : 345
		/// 002 [0000000000000000] Long   : 0
		/// </example>
		[PacketHandler(Op.GuildDonate)]
		public void GuildDonate(ChannelClient client, Packet packet)
		{
			var amount = packet.GetInt();
			var checkId = packet.GetLong();

			var creature = client.GetCreatureSafe(packet.Id);

			if (creature.Guild == null)
			{
				Log.Warning("ConvertGpConfirm: User '{0}' is not in a guild.", client.Account.Id);
				Send.GuildDonateR(creature, false);
				return;
			}

			// Check
			if (checkId != 0)
			{
				var check = creature.Inventory.GetItem(checkId);
				if (check == null)
				{
					Log.Warning("GuildDonate: User '{0}' tried to donate non-exitent check.", client.Account.Id);
					Send.GuildDonateR(creature, false);
					return;
				}
				else if (!check.HasTag("/check/"))
				{
					Log.Warning("GuildDonate: User '{0}' tried to donate invalid check ({1}).", client.Account.Id, check.Info.Id);
					Send.GuildDonateR(creature, false);
					return;
				}

				ChannelServer.Instance.GuildManager.DonateItem(creature, creature.Guild, check);
			}
			// Gold
			else
			{
				if (amount == 0)
				{
					Send.GuildDonateR(creature, false);
					return;
				}
				else if (creature.Inventory.Gold < amount)
				{
					Send.MsgBox(creature, Localization.Get("You don't have enough gold."));
					Send.GuildDonateR(creature, false);
					return;
				}

				ChannelServer.Instance.GuildManager.DonateGold(creature, creature.Guild, amount);
			}

			Send.GuildDonateR(creature, true);
		}

		/// <summary>
		/// Sent choosing to destroy the guild stone.
		/// </summary>
		/// <example>
		/// No parameters.
		/// </example>
		[PacketHandler(Op.GuildDestroyStone)]
		public void GuildDestroyStone(ChannelClient client, Packet packet)
		{
			var creature = client.GetCreatureSafe(packet.Id);

			if (creature.Guild == null)
			{
				Log.Warning("GuildDestroyStone: User '{0}' is not in a guild.", client.Account.Id);
				Send.GuildDonateR(creature, false);
				return;
			}
			else if (creature.GuildMember.Rank != GuildMemberRank.Leader)
			{
				Log.Warning("GuildDestroyStone: User '{0}' tried to destroy stone without being leader.", client.Account.Id);
				Send.GuildDonateR(creature, false);
				return;
			}

			ChannelServer.Instance.GuildManager.DestroyStone(creature, creature.Guild);
		}
	}
}
