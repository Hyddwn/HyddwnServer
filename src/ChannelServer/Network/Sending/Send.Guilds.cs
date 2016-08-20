// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Aura.Channel.World.Entities;
using Aura.Shared.Network;
using Aura.Channel.World;
using Aura.Channel.Network.Sending.Helpers;
using Aura.Mabi.Network;
using Aura.Channel.World.Entities.Props;
using Aura.Shared.Database;
using Aura.Mabi.Const;

namespace Aura.Channel.Network.Sending
{
	public static partial class Send
	{
		/// <summary>
		/// Sends GuildInfo to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="guild"></param>
		public static void GuildInfo(Creature creature, Guild guild)
		{
			var packet = new Packet(Op.GuildInfo, creature.EntityId);
			packet.PutLong(guild.Id);
			packet.PutString(guild.Name);
			packet.PutString(guild.LeaderName);
			packet.PutInt(guild.MemberCount);
			packet.PutString(guild.IntroMessage);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends GuildInfoNoGuild to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="guild"></param>
		public static void GuildInfoNoGuild(Creature creature, Guild guild)
		{
			var packet = new Packet(Op.GuildInfoNoGuild, creature.EntityId);
			packet.PutLong(guild.Id);
			packet.PutString(guild.Name);
			packet.PutString(guild.LeaderName);
			packet.PutInt(guild.MemberCount);
			packet.PutString(guild.IntroMessage);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends GuildInfoApplied to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="guild"></param>
		public static void GuildInfoApplied(Creature creature, Guild guild)
		{
			// The fields of this packet were guessed, something might be missing.

			var packet = new Packet(Op.GuildInfoApplied, creature.EntityId);
			packet.PutLong(guild.Id);
			packet.PutString(guild.Server);
			packet.PutLong(creature.EntityId);
			packet.PutString(guild.Name);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends GuildPanel to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="guild"></param>
		public static void GuildPanel(Creature creature, Guild guild)
		{
			var packet = new Packet(Op.GuildPanel, creature.EntityId);
			packet.PutLong(guild.Id);
			packet.PutByte(creature.GuildMember.Rank == GuildMemberRank.Leader);
			packet.PutByte(0);
			packet.PutByte(0); // 1: Go To Guild Hall,  2: Go To Guild Stone

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends ConvertGpR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		/// <param name="amount"></param>
		public static void ConvertGpR(Creature creature, bool success, int amount)
		{
			var packet = new Packet(Op.ConvertGpR, creature.EntityId);
			packet.PutByte(success);
			packet.PutInt(amount);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends ConvertGpConfirmR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void ConvertGpConfirmR(Creature creature, bool success)
		{
			var packet = new Packet(Op.ConvertGpConfirmR, creature.EntityId);
			packet.PutByte(success);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends GuildMessage to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="guild"></param>
		/// <param name="format"></param>
		/// <param name="args"></param>
		public static void GuildMessage(Creature creature, Guild guild, string format, params object[] args)
		{
			var character = creature as PlayerCreature;
			var entityId = creature.EntityId;
			var guildId = guild.Id;
			var guildName = guild.Name;
			var serverName = (character != null ? character.Server : "?");

			var packet = new Packet(Op.GuildMessage, creature.EntityId);
			packet.PutLong(guildId);
			packet.PutString(serverName);
			packet.PutLong(entityId);
			packet.PutString(guildName);
			packet.PutString(format, args);
			packet.PutByte(1);
			packet.PutByte(1);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends GuildDonateR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void GuildDonateR(Creature creature, bool success)
		{
			var packet = new Packet(Op.GuildDonateR, creature.EntityId);
			packet.PutByte(success);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends GuildApplyR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void GuildApplyR(Creature creature, bool success)
		{
			var packet = new Packet(Op.GuildApplyR, creature.EntityId);
			packet.PutByte(success);

			creature.Client.Send(packet);
		}
	}
}
