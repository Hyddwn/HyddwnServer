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
			packet.PutByte(creature.GuildMember.IsLeader);
			if (creature.GuildMember.IsLeader)
			{
				packet.PutInt(guild.WithdrawMaxAmount);
				packet.PutLong(guild.WithdrawDeadline);
				packet.PutInt(guild.MaxMembers);
			}
			packet.PutByte(0);
			packet.PutByte(0); // 1: Go To Guild Hall,  2: Go To Guild Stone

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends GuildConvertPlayPointsR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		/// <param name="amount"></param>
		public static void GuildConvertPlayPointsR(Creature creature, bool success, int amount)
		{
			var packet = new Packet(Op.GuildConvertPlayPointsR, creature.EntityId);
			packet.PutByte(success);
			packet.PutInt(amount);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends GuildConvertPlayPointsConfirm to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void GuildConvertPlayPointsConfirmR(Creature creature, bool success)
		{
			var packet = new Packet(Op.GuildConvertPlayPointsConfirmR, creature.EntityId);
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

		/// <summary>
		/// Broadcasts GuildUpdateMember in range of creature.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="guild"></param>
		/// <param name="member"></param>
		public static void GuildUpdateMember(Creature creature, Guild guild, GuildMember member)
		{
			var packet = new Packet(Op.GuildUpdateMember, creature.EntityId);
			packet.PutInt(guild == null ? 0 : 1);
			if (guild != null)
			{
				packet.PutString(guild.Name);
				packet.PutLong(guild.Id);
				packet.PutInt((int)member.Rank);
				packet.PutByte(0); // messages?
			}

			creature.Region.Broadcast(packet, creature);
		}

		/// <summary>
		/// Sends GuildWithdrawGoldR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void GuildWithdrawGoldR(Creature creature, bool success)
		{
			var packet = new Packet(Op.GuildWithdrawGoldR, creature.EntityId);
			packet.PutByte(success);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends GuildStoneLocation to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="stone">Removes marker if null.</param>
		public static void GuildStoneLocation(Creature creature, GuildStone stone)
		{
			var packet = new Packet(Op.GuildStoneLocation, creature.EntityId);
			packet.PutByte(stone != null);
			if (stone != null)
			{
				packet.PutInt(stone.RegionId);
				packet.PutInt(stone.X);
				packet.PutInt(stone.Y);
			}

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends GuildPermitCheckR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		/// <param name="itemEntityId"></param>
		public static void GuildPermitCheckR(Creature creature, GuildPermitCheckResult result, long itemEntityId)
		{
			var packet = new Packet(Op.GuildPermitCheckR, creature.EntityId);
			packet.PutInt((int)result);
			packet.PutLong(itemEntityId);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends GuildCheckNameR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void GuildCheckNameR(Creature creature, bool success)
		{
			var packet = new Packet(Op.GuildCheckNameR, creature.EntityId);
			packet.PutByte(success);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends GuildCreateRequestR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void GuildCreateRequestR(Creature creature, bool success)
		{
			var packet = new Packet(Op.GuildCreateRequestR, creature.EntityId);
			packet.PutByte(success);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends GuildNameAgreeRequest to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="requesteeEntityId"></param>
		/// <param name="name"></param>
		public static void GuildNameAgreeRequest(Creature creature, long requesteeEntityId, string name)
		{
			var packet = new Packet(Op.GuildNameAgreeRequest, creature.EntityId);
			packet.PutLong(requesteeEntityId);
			packet.PutString(name);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends GuildNameVote to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="vote"></param>
		public static void GuildNameVote(Creature creature, string voterName, bool vote)
		{
			var packet = new Packet(Op.GuildNameVote, creature.EntityId);
			packet.PutString(voterName);
			packet.PutByte(vote);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends GuildCreationConfirmRequest to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		public static void GuildCreationConfirmRequest(Creature creature)
		{
			var packet = new Packet(Op.GuildCreationConfirmRequest, creature.EntityId);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends GuildGoldUpdate to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="amount"></param>
		public static void GuildGoldUpdate(Creature creature, int amount)
		{
			var packet = new Packet(Op.GuildGoldUpdate, creature.EntityId);
			packet.PutInt(amount);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends GuildOpenGuildRobeCreation to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="amount"></param>
		public static void GuildOpenGuildCreation(Creature creature, long entityId, string guildName, uint color)
		{
			var packet = new Packet(Op.GuildOpenGuildRobeCreation, creature.EntityId);
			packet.PutLong(entityId);
			packet.PutString(guildName);
			packet.PutUInt(color);
			packet.PutByte(0);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends GuildCreateGuildRobeUpdate to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="emblemMark"></param>
		/// <param name="emblemOutline"></param>
		/// <param name="stripes"></param>
		/// <param name="robeColor"></param>
		/// <param name="badgeColor"></param>
		/// <param name="emblemMarkColor"></param>
		/// <param name="emblemOutlineColor"></param>
		/// <param name="stripesColor"></param>
		public static void GuildCreateGuildRobeUpdate(Creature creature, byte emblemMark, byte emblemOutline, byte stripes, uint robeColor, byte badgeColor, byte emblemMarkColor, byte emblemOutlineColor, byte stripesColor, bool success)
		{
			var packet = new Packet(Op.GuildCreateGuildRobeUpdate, creature.EntityId);
			packet.PutByte(emblemMark);
			packet.PutByte(emblemOutline);
			packet.PutByte(stripes);
			packet.PutUInt(robeColor);
			packet.PutByte(badgeColor);
			packet.PutByte(emblemMarkColor);
			packet.PutByte(emblemOutlineColor);
			packet.PutByte(stripesColor);
			packet.PutByte(success);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends GuildCreateGuildRobeR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void GuildCreateGuildRobeR(Creature creature, bool success)
		{
			var packet = new Packet(Op.GuildCreateGuildRobeR, creature.EntityId);
			packet.PutByte(success);

			creature.Client.Send(packet);
		}
	}
}
