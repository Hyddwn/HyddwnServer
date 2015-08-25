// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder
using Aura.Channel.World.Entities;
using Aura.Channel.World;
using Aura.Channel.Network.Sending.Helpers;
using Aura.Mabi.Network;

namespace Aura.Channel.Network.Sending
{
    public static partial class Send
    {
        /// <summary>
        /// Response to the party creation request, sends the client the relevant party data.
        /// </summary>
        /// <remarks>I feel like I'm the MSDN with that summary.</remarks>
        /// <param name="creature"></param>
        public static void CreatePartyR(Creature creature)
        {
            var packet = new Packet(Op.PartyCreateR, creature.EntityId);
            var party = creature.Party;

            if (creature.IsInParty)
            {
                packet.PutByte(1);
                packet.BuildPartyInfo(party);
            }
            else
                packet.PutByte(0);

            creature.Client.Send(packet);
        }

        /// <summary>
        /// Sends the correct response to the client sending a party join request,
        /// and success grants full party information.
        /// </summary>
        /// <remarks>This is also sent when changing channel whilst in a party, upon reaching the new channel.</remarks>
        /// <param name="creature"></param>
        /// <param name="result"></param>
        public static void PartyJoinR(Creature creature, PartyJoinResult result)
        {
            var packet = new Packet(Op.PartyJoinR, creature.EntityId);

            packet.PutByte((byte)result);

            if (PartyJoinResult.Success == result)
                packet.BuildPartyInfo(creature.Party);
            
            creature.Client.Send(packet);

        }

        /// <summary>
        /// Updates all members of the new creature that has joined the party.
        /// </summary>
        /// <param name="creature"></param>
        public static void PartyJoinUpdateMembers(Creature creature)
        {
            var packet = new Packet(Op.PartyJoinUpdate, 0);
            var party = creature.Party;

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
            packet.PutString(party.MemberWantedString);

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
        /// <remarks>(apparently they only get to know about name changes?)</remarks>
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

            packet.BuildPartyInfo(creature.Party);
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
        public static void PartyChangePasswordR(Creature creature)
        {
            var packet = new Packet(Op.PartyChangePasswordR, creature.EntityId);

            // Changing password success, assume always successful?
            packet.PutByte(true);

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
        /// Opens and closes the party's Member Wanted window.
        /// </summary>
        /// <param name="party"></param>
        public static void PartyMemberWantedStateChange(Party party)
        {
            var packet = new Packet((party.IsOpen ? Op.PartyWantedOpened : Op.PartyWantedClosed), 0);

            party.Broadcast(packet, true);
            
            PartyMemberWantedRefresh(party);
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
        public static void PartyChangeFinishR(Creature creature)
        {
            var packet = new Packet(Op.PartyChangeFinishR, creature.EntityId);

            // Can it fail?
            packet.PutByte(true);
            creature.Client.Send(packet);
        }

        /// <summary>
        /// Response to leader changing EXP distribution rule.
        /// </summary>
        /// <remarks>Currently only successful</remarks>
        /// <param name="creature"></param>
        public static void PartyChangeExpR(Creature creature)
        {
            var packet = new Packet(Op.PartyChangeExpR, creature.EntityId);

            // Can it fail?
            packet.PutByte(true);
            creature.Client.Send(packet);
        }

        /// <summary>
        /// Response to leader closing the party.
        /// </summary>
        /// <remarks>Currently only successful</remarks>
        /// <param name="creature"></param>
        public static void PartyWantedClosedR(Creature creature)
        {
            var packet = new Packet(Op.PartyWantedHideR, creature.EntityId);

            // Can it fail?
            packet.PutByte(true);
            creature.Client.Send(packet);
        }

        /// <summary>
        /// Response to leader opening party.
        /// </summary>
        /// <remarks>Currently only successful</remarks>
        /// <param name="creature"></param>
        public static void PartyWantedOpenR(Creature creature)
        {
            var packet = new Packet(Op.PartyWantedShowR, creature.EntityId);

            // Can it fail?
            packet.PutByte(true);
            creature.Client.Send(packet);
        }

        /// <summary>
        /// Response to a member attempting to leave the party.
        /// </summary>
        /// <param name="creature"></param>
        /// <param name="leaving"></param>
        public static void PartyLeaveR(Creature creature, bool leaving)
        {
            var packet = new Packet(Op.PartyLeaveR, creature.EntityId);

            packet.PutByte(leaving);
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
            var packet = new Packet(Op.PartyCreateUpdate, 0);

            packet.PutLong(creature.EntityId);
            packet.PutInt(creature.PartyPosition);
            packet.PutLong(creature.EntityId);
            packet.PutString(creature.Name);
            packet.PutByte(1);
            packet.PutInt(creature.Region.Id);

            var loc = creature.GetPosition();
            packet.PutInt(loc.X);
            packet.PutInt(loc.Y);
            packet.PutByte(0);

            packet.PutInt((int)((creature.Life * 100) / creature.LifeMax));
            packet.PutInt((int)100);
            packet.PutInt(creature.Party.Leader == creature ? 3 : 1);               // TODO: Check what this actually is (I've only seen the leader with a 3 so far)
            packet.PutLong(0);

            creature.Party.Broadcast(packet, true);
        }
    }
}
