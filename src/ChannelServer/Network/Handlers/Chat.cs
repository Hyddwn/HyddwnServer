// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Shared.Network;
using Aura.Channel.Network.Sending;
using Aura.Shared.Util;
using Aura.Mabi.Network;
using Aura.Mabi.Const;

namespace Aura.Channel.Network.Handlers
{
	public partial class ChannelServerHandlers : PacketHandlerManager<ChannelClient>
	{
		[PacketHandler(Op.Chat)]
		public void Chat(ChannelClient client, Packet packet)
		{
			var unkByte = packet.GetByte();
			var message = packet.GetString();

			var creature = client.GetCreatureSafe(packet.Id);

			// Character limit for players is 100
			if (message.Length > 100 && creature.Titles.SelectedTitle != TitleId.devCAT)
			{
				Log.Warning("Chat: Creature '{0:X16}' tried to send chat message with over 100 characters.");
				return;
			}

			if (!creature.Can(Locks.Speak))
			{
				Log.Debug("Speak locked for '{0}'.", creature.Name);
				return;
			}

			// Don't send message if it's a valid command
			if (ChannelServer.Instance.CommandProcessor.Process(client, creature, message))
				return;

			Send.Chat(creature, message);
		}

		[PacketHandler(Op.VisualChat)]
		public void VisualChat(ChannelClient client, Packet packet)
		{
			var url = packet.GetString();
			var width = packet.GetShort();
			var height = packet.GetShort();

			var creature = client.GetCreatureSafe(packet.Id);

			Send.VisualChat(creature, url, width, height);
		}

		[PacketHandler(Op.PartyChat)]
		public void PartyChat(ChannelClient client, Packet packet)
		{
			var msg = packet.GetString();

			var creature = client.GetCreatureSafe(packet.Id);

			// Character limit for players is 100
			if (msg.Length > 100 && creature.Titles.SelectedTitle != TitleId.devCAT)
			{
				Log.Warning("PartyChat: Creature '{0:X16}' tried to send chat message with over 100 characters.");
				return;
			}

			if (creature.IsInParty)
				Send.PartyChat(creature, msg);
		}

		/// <summary>
		/// Sent when whispering someone.
		/// </summary>
		/// <example>
		/// 001 [................] String : test
		/// 002 [................] String : asd
		/// </example>
		[PacketHandler(Op.WhisperChat)]
		public void WhisperChat(ChannelClient client, Packet packet)
		{
			var name = packet.GetString();
			var message = packet.GetString();

			var creature = client.GetCreatureSafe(packet.Id);

			// Get target
			var targetCreature = ChannelServer.Instance.World.GetPlayer(name);
			if (targetCreature == null)
			{
				Send.SystemMessage(creature, Localization.Get("Character not found."));
				return;
			}

			// Character limit for players is 100
			if (message.Length > 100 && creature.Titles.SelectedTitle != TitleId.devCAT)
			{
				Log.Warning("WhisperChat: Creature '{0:X16}' tried to send chat message with over 100 characters.");
				return;
			}

			// Send message
			Send.WhisperChat(creature, creature.Name, message);
			Send.WhisperChat(targetCreature, creature.Name, message);
		}
	}
}
