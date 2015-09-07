// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.World.Entities;
using Aura.Channel.World;
using Aura.Channel.Network.Sending.Helpers;
using Aura.Mabi.Network;
using Aura.Mabi.Const;
using System.Collections.Generic;

namespace Aura.Channel.Network.Sending
{
	public static partial class Send
	{
		/// <summary>
		/// Sends SquadUnkR to creature's client.
		/// </summary>
		/// <remarks>
		/// I assume this is a list of missions the squad can do?
		/// </remarks>
		/// <param name="creature"></param>
		/// <param name="num"></param>
		public static void SquadUnkR(Creature creature, int num)
		{
			var packet = new Packet(Op.SquadUnkR, creature.EntityId);
			packet.PutInt(num);
			packet.PutInt(0); // count
			{
				// 003 [........000AD959] Int    : 711001
				// 004 [..............00] Byte   : 0
				// 005 [................] String : Girgashiy
				// 006 [..............01] Byte   : 1
				// 007 [..............10] Byte   : 16
				// 008 [........001B7740] Int    : 1800000
				// 009 [........00011365] Int    : 70501
				// 010 [................] String : These mystic beings appeared near Abb Neagh Lake, terrorizing the land with god-like powers of destruction. Fanatics have declared them sent from the heavens and branded them the Girgashiy, a race of divine beings. They must be stopped before they reach a populated area. Form a squad with any brave companions you can find, and end their reign of terror.
				// 011 [................] String : Girgashiy;Girgashiy will perform a high jump before a very powerful area attack. Use this opportunity to attack them with Crusader Skills.
				// 012 [................] String : 20000 Experience Point4000G
				// 013 [........00000000] Int    : 0
				// 014 [........00000000] Int    : 0
				// 015 [........00000000] Int    : 0
				// 016 [........00000000] Int    : 0
				// 017 [..............00] Byte   : 0
			}

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Response to the party creation request, sends the client the relevant party data.
		/// </summary>
		/// <remarks>
		/// I feel like I'm the MSDN with that summary.
		/// </remarks>
		/// <param name="creature"></param>
		/// <param name="party">Set null for negative response.</param>
		public static void CreatePartyR(Creature creature, Party party)
		{
			var packet = new Packet(Op.PartyCreateR, creature.EntityId);

			packet.PutByte(party != null);
			if (party != null)
				packet.AddParty(party);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends the correct response to the client sending a party join request,
		/// and success grants full party information.
		/// </summary>
		/// <remarks>
		/// This is also sent when changing channel whilst in a party,
		/// upon reaching the new channel.
		/// </remarks>
		/// <param name="creature"></param>
		/// <param name="result"></param>
		public static void PartyJoinR(Creature creature, PartyJoinResult result)
		{
			var packet = new Packet(Op.PartyJoinR, creature.EntityId);

			packet.PutByte((byte)result);
			if (result == PartyJoinResult.Success)
				packet.AddParty(creature.Party);

			creature.Client.Send(packet);

		}

		/// <summary>
		/// Updates all members of the new creature that has joined the party.
		/// </summary>
		/// <param name="creature"></param>
		public static void PartyJoinUpdateMembers(Creature creature)
		{
			var party = creature.Party;

			var packet = new Packet(Op.PartyJoinUpdate, 0);

			packet.AddPartyMember(creature);

			party.Broadcast(packet, true, creature);
		}

		/// <summary>
		/// Updates the party title with new information, such as a change in the total party members,
		/// name, type, etc.
		/// </summary>
		/// <param name="party"></param>
		public static void PartyMemberWantedRefresh(Party party)
		{
			var packet = new Packet(Op.PartyWantedUpdate, party.Leader.EntityId);

			packet.PutByte(party.IsOpen);
			packet.PutString(party.ToString());

			party.Leader.Region.Broadcast(packet, party.Leader);
		}

		/// <summary>
		/// I THINK this one is for actually updating the UI element of the party (with leader controls).
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="party"></param>
		public static void PartyWindowUpdate(Creature creature, Party party)
		{
			var packet = new Packet(Op.PartyWindowUpdate, 0);

			packet.PutLong(creature.EntityId);

			// TODO: Find out what these actually mean.
			packet.PutByte(1);
			packet.PutByte(1);
			packet.PutByte(0);
			packet.PutByte(0);

			party.Broadcast(packet, true);
		}

		/// <summary>
		/// Updates remaining party members of a member who has left the party.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="party"></param>
		public static void PartyLeaveUpdate(Creature creature, Party party)
		{
			var packet = new Packet(Op.PartyLeaveUpdate, 0);

			packet.PutLong(creature.EntityId);

			party.Broadcast(packet, true);
		}

		/// <summary>
		/// Informs party members of a party type change (Dungeon, Normal, Jam, etc)
		/// </summary>
		/// <param name="party"></param>
		public static void PartyTypeUpdate(Party party)
		{
			var packet = new Packet(Op.PartyTypeUpdate, 0);

			packet.PutInt((int)party.Type);

			party.Broadcast(packet, true);
		}

		/// <summary>
		/// Updates members on changes to the party settings
		/// </summary>
		/// <remarks>
		/// Apparently they only get to know about name changes?
		/// </remarks>
		/// <param name="party"></param>
		public static void PartySettingUpdate(Party party)
		{
			var packet = new Packet(Op.PartySettingUpdate, 0);

			packet.PutString(party.Name);

			party.Broadcast(packet, true);
		}

		/// <summary>
		/// Response to the leader changing party settings.
		/// </summary>
		/// <param name="creature"></param>
		public static void PartyChangeSettingR(Creature creature)
		{
			var packet = new Packet(Op.PartyChangeSettingR, creature.EntityId);

			packet.AddParty(creature.Party);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Responding whether the leader can (did) or can't (didn't) remove the requested party member.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="canRemove"></param>
		public static void PartyRemoveR(Creature creature, bool canRemove)
		{
			var packet = new Packet(Op.PartyRemoveR, creature.EntityId);

			packet.PutByte(canRemove);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Informing the leader on the status of their password request
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void PartyChangePasswordR(Creature creature, bool success)
		{
			var packet = new Packet(Op.PartyChangePasswordR, creature.EntityId);

			packet.PutByte(success);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Informs all members of a change in leadership.
		/// </summary>
		/// <param name="party"></param>
		public static void PartyChangeLeader(Party party)
		{
			var packet = new Packet(Op.PartyChangeLeaderUpdate, 0);

			packet.PutLong(party.Leader.EntityId);

			party.Broadcast(packet, true);
		}

		/// <summary>
		/// Response to the leader's request to change leadership.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void PartyChangeLeaderR(Creature creature, bool success)
		{
			var packet = new Packet(Op.PartyChangeLeaderR, creature.EntityId);

			packet.PutByte(success);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Opens the party's Member Wanted window.
		/// </summary>
		/// <param name="party"></param>
		public static void PartyWantedOpened(Party party)
		{
			var packet = new Packet(Op.PartyWantedOpened, 0);

			party.Broadcast(packet, true);
		}

		/// <summary>
		/// Closes the party's Member Wanted window.
		/// </summary>
		/// <param name="party"></param>
		public static void PartyWantedClosed(Party party)
		{
			var packet = new Packet(Op.PartyWantedClosed, 0);

			party.Broadcast(packet, true);
		}

		/// <summary>
		/// Informs a client on their removal from the party.
		/// </summary>
		/// <param name="creature"></param>
		public static void PartyRemoved(Creature creature)
		{
			var packet = new Packet(Op.PartyRemoved, creature.EntityId);
			creature.Client.Send(packet);
		}

		/// <summary>
		/// Sends new finish rule setting to all clients in the party
		/// </summary>
		/// <param name="party"></param>
		public static void PartyFinishUpdate(Party party)
		{
			var packet = new Packet(Op.PartyFinishUpdate, 0);

			packet.PutInt((int)party.Finish);

			party.Broadcast(packet, true);
		}

		/// <summary>
		/// Updates clients on new party EXP distribution settings.
		/// </summary>
		/// <param name="party"></param>
		public static void PartyExpUpdate(Party party)
		{
			var packet = new Packet(Op.PartyExpUpdate, 0);

			packet.PutInt((int)party.ExpRule);

			party.Broadcast(packet, true);
		}

		// TODO: Consider merging all these response packets?

		/// <summary>
		/// Response to leader changing finish rule.
		/// </summary>
		/// <remarks>Currently only successful</remarks>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void PartyChangeFinishR(Creature creature, bool success)
		{
			var packet = new Packet(Op.PartyChangeFinishR, creature.EntityId);

			packet.PutByte(success);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Response to leader changing EXP distribution rule.
		/// </summary>
		/// <remarks>Currently only successful</remarks>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void PartyChangeExpR(Creature creature, bool success)
		{
			var packet = new Packet(Op.PartyChangeExpR, creature.EntityId);

			packet.PutByte(success);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Response to leader closing the party.
		/// </summary>
		/// <remarks>Currently only successful</remarks>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void PartyWantedClosedR(Creature creature, bool success)
		{
			var packet = new Packet(Op.PartyWantedHideR, creature.EntityId);

			packet.PutByte(success);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Response to leader opening party.
		/// </summary>
		/// <remarks>Currently only successful</remarks>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void PartyWantedOpenR(Creature creature, bool success)
		{
			var packet = new Packet(Op.PartyWantedShowR, creature.EntityId);

			packet.PutByte(success);

			creature.Client.Send(packet);
		}

		/// <summary>
		/// Response to a member attempting to leave the party.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="success"></param>
		public static void PartyLeaveR(Creature creature, bool success)
		{
			var packet = new Packet(Op.PartyLeaveR, creature.EntityId);

			packet.PutByte(success);

			creature.Client.Send(packet);
		}

		// TODO: Consolidate this with AddMember functionality.
		/// <summary>
		/// Sent to clients when a new creature joins.
		/// </summary>
		/// <remarks>I'm not entirely sure of the purpose of this packet, I don't think the client would mind if you didn't send it.</remarks>
		/// <param name="creature"></param>
		public static void PartyCreateUpdate(Creature creature)
		{
			var loc = creature.GetPosition();

			var packet = new Packet(Op.PartyCreateUpdate, 0);

			packet.PutLong(creature.EntityId);
			packet.PutInt(creature.PartyPosition);
			packet.PutLong(creature.EntityId);
			packet.PutString(creature.Name);
			packet.PutByte(1);
			packet.PutInt(creature.Region.Id);
			packet.PutInt(loc.X);
			packet.PutInt(loc.Y);
			packet.PutByte(0);

			packet.PutInt((int)((creature.Life * 100) / creature.LifeMax));
			packet.PutInt((int)100);
			packet.PutInt(creature.Party.Leader == creature ? 3 : 1); // TODO: Check what this actually is (I've only seen the leader with a 3 so far)
			packet.PutLong(0);

			creature.Party.Broadcast(packet, true);
		}

		/// <summary>
		/// Sends PartyBoardRequestR to creature's client.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="parties"></param>
		public static void PartyBoardRequestR(Creature creature, List<Party> parties)
		{
			var packet = new Packet(Op.PartyBoardRequestR, creature.EntityId);

			packet.PutInt(0); // count
			{
				// 002 [0040000000000105] Long   : 18014398509482245
				// 003 [0010000000000004] Long   : 4503599600000020
				// 004 [................] String : xxxxxxx
				// 005 [..............01] Byte   : 1
				// 006 [..............00] Byte   : 0
				// 007 [..............00] Byte   : 0
				// 008 [................] String : [S-xxxxx] note/xxxx
				// 009 [................] String : Unrestricted
				// 010 [................] String : Unrestricted
				// 011 [................] String : 
				// 012 [............0000] Short  : 0
				// 013 [........00000000] Int    : 0
				// 014 [........00000001] Int    : 1
				// 015 [........00000008] Int    : 8
				// 016 [..............00] Byte   : 0
				// 017 [................] String : Ch1
				// 018 [........00000001] Int    : 1
			}

			creature.Client.Send(packet);
		}
	}
}
