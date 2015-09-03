// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.World.Entities;
using Aura.Mabi.Network;
using Aura.Channel.World;
using Aura.Mabi.Const;

namespace Aura.Channel.Network.Sending.Helpers
{
	public static class PartyHelper
	{
		/// <summary>
		/// Constructs the party info packet, because this is used in a number of packets.
		/// </summary>
		/// <param name="party"></param>
		/// <param name="packet"></param>
		public static void AddParty(this Packet packet, Party party)
		{
			packet.PutLong(party.Id);
			packet.PutString(party.Name);
			packet.PutLong(party.Leader.EntityId);

			packet.PutByte(party.IsOpen);
			packet.PutInt((int)party.Finish);
			packet.PutInt((int)party.ExpRule);

			packet.PutLong(0); // Quest id?

			packet.PutInt(party.MaxSize);
			packet.PutInt((int)party.Type);

			packet.PutString(party.DungeonLevel);
			packet.PutString(party.Info);

			packet.PutInt(party.MemberCount);

			packet.AddPartyMembers(party);
		}

		/// <summary>
		/// Adds party member data to the referenced packet.
		/// </summary>
		/// <param name="party"></param>
		/// <param name="packet"></param>
		public static void AddPartyMembers(this Packet packet, Party party)
		{
			var members = party.GetMembers();

			for (int i = members.Length - 1; i >= 0; i--)
			{
				packet.AddPartyMember(members[i]);

				if (i == 0)
				{
					packet.PutInt(3);
					packet.PutLong(0);
				}
				else
				{
					packet.PutInt(1);
					packet.PutLong(0);
				}
			}
			packet.PutByte(0);
		}

		/// <summary>
		/// Adds the referred creature's data to the referenced packet.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="packet"></param>
		public static void AddPartyMember(this Packet packet, Creature creature)
		{
			var loc = creature.GetPosition();

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
		}
	}
}
