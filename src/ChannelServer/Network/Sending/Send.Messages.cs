// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Channel.World.Entities;
using Aura.Shared.Network;
using Aura.Mabi.Const;
using Aura.Shared.Util;
using Aura.Channel.World;
using Aura.Mabi.Network;

namespace Aura.Channel.Network.Sending
{
	public static partial class Send
	{
		/// <summary>
		/// Sends system message (special Chat) to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void SystemMessage(Creature creature, string format, params object[] args)
		{
			SystemMessageFrom(creature, Localization.Get("<SYSTEM>"), format, args);
		}

		/// <summary>
		/// Sends server message (special Chat) to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void ServerMessage(Creature creature, string format, params object[] args)
		{
			SystemMessageFrom(creature, Localization.Get("<SERVER>"), format, args);
		}

		public static void System_Broadcast(string from, string format, params object[] args)
		{
			var packet = new Packet(Op.Chat, MabiId.Broadcast);

			foreach (var msg in string.Format(format, args).Chunkify(100)) // Mabi displays up to 100 chars
			{
				packet.PutByte(0);
				packet.PutString("<{0}>", from);
				packet.PutString(msg);
				packet.PutByte(true);
				packet.PutUInt(0xFFFF8080);
				packet.PutInt(0);
				packet.PutByte(0);

				ChannelServer.Instance.World.Broadcast(packet);

				packet.Clear(packet.Op, packet.Id);
			}
		}

		/// <summary>
		/// Sends combat message (special Chat) to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void CombatMessage(Creature creature, string format, params object[] args)
		{
			SystemMessageFrom(creature, Localization.Get("<COMBAT>"), format, args);
		}

		/// <summary>
		/// Sends system message (special Chat) to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="from"></param>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void SystemMessageFrom(Creature creature, string from, string format, params object[] args)
		{
			var packet = new Packet(Op.Chat, creature.EntityId);

			foreach (var msg in string.Format(format, args).Chunkify(100)) // Mabi displays up to 100 chars
			{
				packet.PutByte(0);
				packet.PutString(from);
				packet.PutString(msg);
				packet.PutByte(true);
				packet.PutUInt(0xFFFF8080);
				packet.PutInt(0);
				packet.PutByte(0);

				creature.Client.Send(packet);

				packet.Clear(packet.Op, packet.Id);
			}
		}

		/// <summary>
		/// Sends Chat in range of creature.
		/// </summary>
		/// <param name="creature">Source, in terms of name and position</param>
		/// <param name="message"></param>
		public static void Chat(Creature creature, string message)
		{
			var packet = new Packet(Op.Chat, creature.EntityId);
			packet.PutByte(0); // speech (0) vs thought (1) bubble
			packet.PutString(creature.Name);
			packet.PutString(message);

			// The following part is not required for normal chat

			packet.PutByte(0); // 1 hides chat bubble and enables --v
			packet.PutUInt(0); // custom color (supports alpha transparency, FF is 100% opaque)

			packet.PutInt(0);
			packet.PutByte(0);

			creature.Region.Broadcast(packet, creature);
		}

		/// <summary>
		/// Broadcasts VisualChat to creatures in range.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="url"></param>
		/// <param name="width"></param>
		/// <param name="height"></param>
		public static void VisualChat(Creature creature, string url, short width, short height)
		{
			var packet = new Packet(Op.VisualChat, creature.EntityId);
			packet.PutString(creature.Name);
			packet.PutString(url);
			packet.PutShort(width);
			packet.PutShort(height);
			packet.PutByte(0);

			creature.Region.Broadcast(packet, creature);
		}

		/// <summary>
		/// Broadcasts PartyChat to all party members.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="msg"></param>
		public static void PartyChat(Creature creature, string msg)
		{
			var packet = new Packet(Op.PartyChat, creature.EntityId);
			packet.PutLong(creature.EntityId);
			packet.PutString(msg);

			creature.Party.Broadcast(packet, true);
		}

		/// <summary>
		/// Sends WhisperChat to creature's client, containing whisper message.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="sender"></param>
		/// <param name="message"></param>
		public static void WhisperChat(Creature creature, string sender, string message)
		{
			var packet = new Packet(Op.WhisperChat, creature.EntityId);
			packet.PutString(sender);
			packet.PutString(message);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Broadcasts Chat on entire server, with a blue Party Chat,
		/// consisting of information about the party.
		/// </summary>
		/// <param name="party"></param>
		public static void PartyAdChat(Party party)
		{
			var packet = new Packet(Op.Chat, MabiId.Broadcast);
			packet.PutByte(0);
			packet.PutString(Localization.Get("<PARTY>"));
			packet.PutString(Localization.Get("[Wanted] {0}: [{1}/{2}] - {3}"), party.Leader.Name, party.Name, party.DungeonLevel, party.Info);
			packet.PutByte(1);
			packet.PutInt(-1);
			packet.PutInt(0);
			packet.PutByte(2);

			ChannelServer.Instance.World.Broadcast(packet);
		}

		/// <summary>
		/// Sends MsgBox to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void MsgBox(Creature creature, string format, params object[] args)
		{
			MsgBox(creature, MsgBoxTitle.Notice, MsgBoxButtons.Close, MsgBoxAlign.Center, format, args);
		}

		/// <summary>
		/// Sends MsgBox to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="title"></param>
		/// <param name="buttons"></param>
		/// <param name="align"></param>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void MsgBox(Creature creature, MsgBoxTitle title, MsgBoxButtons buttons, MsgBoxAlign align, string format, params object[] args)
		{
			MsgBox(creature, title.ToString(), MsgBoxButtons.Close, MsgBoxAlign.Center, format, args);
		}

		/// <summary>
		/// Sends MsgBox to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="title"></param>
		/// <param name="buttons"></param>
		/// <param name="align"></param>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void MsgBox(Creature creature, string title, MsgBoxButtons buttons, MsgBoxAlign align, string format, params object[] args)
		{
			var packet = new Packet(Op.MsgBox, creature.EntityId);
			packet.PutString(format, args);

			// Can be sent with the title enum as byte as well.
			packet.PutString(title);

			packet.PutByte((byte)buttons);
			packet.PutByte((byte)align);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends Notice to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="type"></param>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void Notice(Creature creature, string format, params object[] args)
		{
			Notice(creature, NoticeType.Middle, 0, format, args);
		}

		/// <summary>
		/// Sends Notice to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="type"></param>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void Notice(Creature creature, NoticeType type, string format, params object[] args)
		{
			Notice(creature, type, 0, format, args);
		}

		/// <summary>
		/// Sends Notice to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="type"></param>
		/// <param name="duration">Ignored if 0</param>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void Notice(Creature creature, NoticeType type, int duration, string format, params object[] args)
		{
			var packet = new Packet(Op.Notice, MabiId.Broadcast);
			packet.PutByte((byte)type);
			packet.PutString(string.Format(format, args));
			if (duration > 0)
				packet.PutInt(duration);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Broadcasts Notice to every player in any region.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void Notice(NoticeType type, string format, params object[] args)
		{
			Notice(type, 0, format, args);
		}

		/// <summary>
		/// Broadcasts Notice to every player in any region.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="duration">Ignored if 0</param>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void Notice(NoticeType type, int duration, string format, params object[] args)
		{
			var packet = new Packet(Op.Notice, MabiId.Broadcast);
			packet.PutByte((byte)type);
			packet.PutString(string.Format(format, args));
			if (duration > 0)
				packet.PutInt(duration);

			ChannelServer.Instance.World.Broadcast(packet);
		}

		/// <summary>
		/// Broadcasts Notice in region.
		/// </summary>
		/// <param name="region"></param>
		/// <param name="type"></param>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void Notice(Region region, string format, params object[] args)
		{
			Notice(region, NoticeType.Middle, 0, format, args);
		}

		/// <summary>
		/// Broadcasts Notice in region.
		/// </summary>
		/// <param name="region"></param>
		/// <param name="type"></param>
		/// <param name="duration"></param>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void Notice(Region region, NoticeType type, int duration, string format, params object[] args)
		{
			var packet = new Packet(Op.Notice, MabiId.Broadcast);
			packet.PutByte((byte)type);
			packet.PutString(string.Format(format, args));
			if (duration > 0)
				packet.PutInt(duration);

			region.Broadcast(packet);
		}

		/// <summary>
		/// Sends RequestClientDisconnect to creature's client notifying automatic logout.
		/// </summary>
		/// <param name="time">Time in seconds until automatic logout.</param>
		public static void RequestClientDisconnect(int time)
		{
			ChannelServer.Instance.World.Broadcast(new Packet(Op.RequestClientDisconnect, MabiId.Channel).PutInt(time * 1000));
		}
	}
}
