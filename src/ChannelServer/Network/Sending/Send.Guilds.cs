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
using Aura.Channel.World.Dungeons.Guilds;
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
	}
}
