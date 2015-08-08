// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Util;
using Aura.Channel.World.Entities;
using Aura.Data;
using Aura.Data.Database;
using Aura.Mabi.Const;
using Aura.Mabi.Network;
using Aura.Shared.Network;
using System;

namespace Aura.Channel.Network.Handlers
{
    public partial class ChannelServerHandlers : PacketHandlerManager<ChannelClient>
    {
        [PacketHandler(Op.PartyCreate)]
        public void PartyCreate(ChannelClient client, Packet packet)
        {
            var creature = client.GetCreatureSafe(packet.Id);
            
            if (creature == null)
                return;

            if (creature.IsInParty)
                return;

            // Needs check like the below, but do homesteads count as dynamic regions? I know you can't make a party in the shadow realm (you can in dungeons), but are they the only places? Need to look into that.
            //if (!creature.Region.IsDynamic)          
            {
                var party = creature.Party;

                party.CreateParty(creature, packet);

                Send.CreatePartyR(creature);

                party.Open();
                return;
            }
            //else
            //Send.CreatePartyR(creature);
        }

        [PacketHandler(Op.PartyJoin)]
        public void PartyJoin(ChannelClient client, Packet packet)
        {
            var creature = client.GetCreatureSafe(packet.Id);

            if (creature == null)
                return;

            if (creature.IsInParty)
                return;

            var partyLeader = ChannelServer.Instance.World.GetCreature(packet.GetLong());

            if (!partyLeader.IsInParty)
                return;
            if (partyLeader.Party.Leader.EntityId != partyLeader.EntityId)
                return;

            var party = partyLeader.Party;
            var password = packet.GetString();

            if(party.TotalMembers >= party.MaxSize)
            {
                Send.PartyJoinR(creature, World.PartyJoinResult.Full);
                return;
            }

            if (party.Password != password)
            {
                Send.PartyJoinR(creature, World.PartyJoinResult.WrongPass);
                return;
            }

            party.AddMember(creature);
            Send.PartyJoinUpdateMembers(creature);

            if (party.IsOpen)
                Send.PartyMemberWantedRefresh(party);

            Send.PartyJoinR(creature, World.PartyJoinResult.Success);
            Send.PartyCreateUpdate(creature);
        }

        [PacketHandler(Op.PartyLeave)]
        public void LeaveParty(ChannelClient client, Packet packet)
        {
            var creature = client.GetCreatureSafe(packet.Id);

            if (creature == null)
            {
                Send.PartyLeaveR(creature, false);
                return;
            }

            if (!creature.IsInParty)
            {
                Send.PartyLeaveR(creature, false);
                return;
            }

            var party = creature.Party;

            // TODO: Check if there are other regions or situations in which you cannot leave a party.
            var canLeave = !creature.Region.IsDynamic;

            if (canLeave)
            {
                creature.Party.RemoveMember(creature);


                if (party.TotalMembers > 0)
                {
                    Send.PartyLeaveUpdate(creature, party);

                    if (party.IsOpen)
                        Send.PartyMemberWantedRefresh(party);

                    // What is this?
                    //Send.PartyWindowUpdate(creature, party);

                    if (party.Leader == creature)
                    {
                        party.SetLeader(party.GetNextLeader());
                        if (party.IsOpen)
                            party.Close();
                        Send.PartyChangeLeader(party);
                    }
                }
                else
                {
                    if (party.IsOpen)
                        party.Close();
                }
            }

            Send.PartyLeaveR(creature, canLeave);
        }

        [PacketHandler(Op.PartyRemove)]
        public void RemoveFromParty(ChannelClient client, Packet packet)
        {
            var creature = client.GetCreatureSafe(packet.Id);

            if (creature == null)
                return;

            if (!creature.IsInParty)
                return;

            var party = creature.Party;
            if (party.Leader != creature)
                return;

            
            var canRemove = !creature.Region.IsDynamic;
            if (canRemove)
            {
                var target = party.ContainsMember(packet.GetLong());
                if (target != null && (target != creature))
                {
                    party.RemoveMember(target);
                    Send.PartyRemoved(target);
                    Send.PartyLeaveUpdate(target, party);
                }
                else
                    canRemove = false;
            }

            Send.PartyRemoveR(creature, canRemove);
        }

        [PacketHandler(Op.PartyChangeSetting)]
        public void PartyChangeSettings(ChannelClient client, Packet packet)
        {
            var creature = client.GetCreatureSafe(packet.Id);
            if (creature == null)
                return;

            if (!creature.IsInParty)
                return;

            var party = creature.Party;
            if (creature != party.Leader)
                return;

            party.ChangeSettings(packet);
            Send.PartyChangeSettingR(creature);
        }

        [PacketHandler(Op.PartyChangePassword)]
        public void PartyChangePassword(ChannelClient client, Packet packet)
        {
            var creature = client.GetCreatureSafe(packet.Id);
            if (creature == null)
                return;

            if (!creature.IsInParty)
                return;

            var party = creature.Party;
            if (party.Leader != creature)
                return;

            party.SetPassword(packet.GetString());

            Send.PartyChangePasswordR(creature);
        }

        [PacketHandler(Op.PartyChangeLeader)]
        public void PartyChangeLeader(ChannelClient client, Packet packet)
        {
            var creature = client.GetCreatureSafe(packet.Id);
            if (creature == null)
                return;
            if (!creature.IsInParty)
                return;

            var party = creature.Party;
            if (party.Leader != creature)
                return;

            // IS there any instance in which you're not allowed to change party leader?
            var success = party.SetLeader(packet.GetLong());
            Send.PartyChangeLeaderR(creature, success);

        }

        [PacketHandler(Op.PartyWantedHide)]
        public void PartyWantedClose(ChannelClient client, Packet packet)
        {
            var creature = client.GetCreatureSafe(packet.Id);
            if (creature == null)
                return;

            if (!creature.IsInParty)
                return;

            var party = creature.Party;

            if (creature != party.Leader)
                return;

            party.Close();
            Send.PartyWantedClosedR(creature);
        }

        [PacketHandler(Op.PartyWantedShow)]
        public void PartyWantedOpen(ChannelClient client, Packet packet)
        {
            var creature = client.GetCreatureSafe(packet.Id);
            if (creature == null)
                return;

            if (!creature.IsInParty)
                return;

            var party = creature.Party;

            if (creature != party.Leader)
                return;

            party.Open();
            Send.PartyWantedOpenR(creature);
        }

        [PacketHandler(Op.PartyChangeFinish)]
        public void PartyChangeFinish(ChannelClient client, Packet packet)
        {
            var creature = client.GetCreatureSafe(packet.Id);

            if (creature == null)
                return;

            if (!creature.IsInParty)
                return;

            var party = creature.Party;
            if (party.Leader != creature)
                return;

            party.ChangeFinish(packet.GetInt());
            Send.PartyChangeFinishR(creature);
        }

        [PacketHandler(Op.PartyChangeExp)]
        public void PartyChangeExp(ChannelClient client, Packet packet)
        {
            var creature = client.GetCreatureSafe(packet.Id);

            if (creature == null)
                return;

            if (!creature.IsInParty)
                return;

            var party = creature.Party;
            if (party.Leader != creature)
                return;

            party.ChangeExp(packet.GetInt());
            Send.PartyChangeExpR(creature);
        }
    }
}