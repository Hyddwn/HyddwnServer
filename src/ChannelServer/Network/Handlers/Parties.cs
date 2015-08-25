// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Network.Sending.Helpers;
using Aura.Mabi.Network;
using Aura.Shared.Network;

namespace Aura.Channel.Network.Handlers
{
    public partial class ChannelServerHandlers : PacketHandlerManager<ChannelClient>
    {
        [PacketHandler(Op.PartyCreate)]
        public void PartyCreate(ChannelClient client, Packet packet)
        {
            var creature = client.GetCreatureSafe(packet.Id);
            var party = creature.Party;

            // Needs check like the below, but do homesteads count as dynamic regions?
            // I know you can't make a party in the shadow realm (you can in dungeons), but are they the only places? Need to look into that.
            if ((creature.IsInParty) /*|| (creature.Region.IsDynamic) */)
            {
                Send.CreatePartyR(creature);
                return;
            }

            party.CreateParty(creature);
            packet.SettingsParse(party);

            Send.CreatePartyR(creature);

            party.Open();

        }

        [PacketHandler(Op.PartyJoin)]
        public void PartyJoin(ChannelClient client, Packet packet)
        {
            var creature = client.GetCreatureSafe(packet.Id);
            var partyLeader = ChannelServer.Instance.World.GetCreature(packet.GetLong());
            var party = partyLeader.Party;
            var password = packet.GetString();

            if ((creature.IsInParty) || (!partyLeader.IsInParty) || (partyLeader.Party.Leader.EntityId != partyLeader.EntityId))
                return;

            var result = party.AddMember(creature, password);
            
            Send.PartyJoinR(creature, result);

            if (result == World.PartyJoinResult.Success)
                Send.PartyCreateUpdate(creature);
        }

        [PacketHandler(Op.PartyLeave)]
        public void LeaveParty(ChannelClient client, Packet packet)
        {
            var creature = client.GetCreatureSafe(packet.Id);
            var party = creature.Party;
            var canLeave = !creature.Region.IsDynamic;

            if (!creature.IsInParty)
            {
                Send.PartyLeaveR(creature, false);
                return;
            }

            // TODO: Check if there are other regions or situations in which you cannot leave a party.
            if (!canLeave)
            {
                Send.PartyLeaveR(creature, false);
                return;
            }

            creature.Party.RemoveMember(creature);

            Send.PartyLeaveR(creature, canLeave);
        }

        [PacketHandler(Op.PartyRemove)]
        public void RemoveFromParty(ChannelClient client, Packet packet)
        {
            var creature = client.GetCreatureSafe(packet.Id);
            var party = creature.Party;
            var canRemove = !creature.Region.IsDynamic;
            var target = party.GetMember(packet.GetLong());

            if ((!creature.IsInParty) || (party.Leader != creature))
                return;

            if ((canRemove) && (target != null) && (target != creature))
            {
                party.RemoveMember(target);
                Send.PartyRemoved(target);
            }
            else
                canRemove = false;

            Send.PartyRemoveR(creature, canRemove);
        }

        [PacketHandler(Op.PartyChangeSetting)]
        public void PartyChangeSettings(ChannelClient client, Packet packet)
        {
            var creature = client.GetCreatureSafe(packet.Id);
            var party = creature.Party;

            if ((!creature.IsInParty) || (creature != party.Leader))
                return;

            packet.SettingsParse(party);
            Send.PartyChangeSettingR(creature);
        }

        [PacketHandler(Op.PartyChangePassword)]
        public void PartyChangePassword(ChannelClient client, Packet packet)
        {
            var creature = client.GetCreatureSafe(packet.Id);
            var party = creature.Party;

            if ((!creature.IsInParty) || (party.Leader != creature))
                return;
            
            party.SetPassword(packet.GetString());
            Send.PartyChangePasswordR(creature);
        }

        [PacketHandler(Op.PartyChangeLeader)]
        public void PartyChangeLeader(ChannelClient client, Packet packet)
        {
            var creature = client.GetCreatureSafe(packet.Id);
            var party = creature.Party;

            if ((!creature.IsInParty) || (party.Leader != creature))
                return;

            // IS there any instance in which you're NOT allowed to change party leader?
            var success = party.SetLeader(packet.GetLong());
            Send.PartyChangeLeaderR(creature, success);

        }

        [PacketHandler(Op.PartyWantedHide)]
        public void PartyWantedClose(ChannelClient client, Packet packet)
        {
            var creature = client.GetCreatureSafe(packet.Id);
            var party = creature.Party;

            if ((!creature.IsInParty) || (creature != party.Leader))
                return;
            
            party.Close();
            Send.PartyWantedClosedR(creature);
        }

        [PacketHandler(Op.PartyWantedShow)]
        public void PartyWantedOpen(ChannelClient client, Packet packet)
        {
            var creature = client.GetCreatureSafe(packet.Id);
            var party = creature.Party;

            if ((!creature.IsInParty) || (creature != party.Leader))
                return;

            party.Open();
            Send.PartyWantedOpenR(creature);
        }

        [PacketHandler(Op.PartyChangeFinish)]
        public void PartyChangeFinish(ChannelClient client, Packet packet)
        {
            var creature = client.GetCreatureSafe(packet.Id);
            var party = creature.Party;

            if ((!creature.IsInParty) || (party.Leader != creature))
                return;

            party.ChangeFinish(packet.GetInt());
            Send.PartyChangeFinishR(creature);
        }

        [PacketHandler(Op.PartyChangeExp)]
        public void PartyChangeExp(ChannelClient client, Packet packet)
        {
            var creature = client.GetCreatureSafe(packet.Id);
            var party = creature.Party;

            if ((!creature.IsInParty) || (party.Leader != creature))
                return;
            
            party.ChangeExp(packet.GetInt());
            Send.PartyChangeExpR(creature);
        }
    }
}