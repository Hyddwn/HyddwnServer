using Aura.Channel.World.Entities;
using Aura.Mabi.Network;
using Aura.Channel.World;

namespace Aura.Channel.Network.Sending.Helpers
{
    public static class PartyHelper
    {

        public static void SettingsParse(Party party, Packet packet)
        {
            var type = (PartyType)packet.GetInt();
            var name = packet.GetString();

            if (type == PartyType.Dungeon)
            {
                var dungeonLevel = packet.GetString();
                var info = packet.GetString();

                party.SetDungeonLevel(dungeonLevel);
                party.SetInfo(info);
            }
            var password = packet.GetString();
            var maxSize = packet.GetInt();

            party.SetType(type);
            party.SetName(name);
            party.SetPassword(password);
            party.SetSize(maxSize);

            Send.PartySettingUpdate(party);

            if (party.IsOpen)
                Send.PartyMemberWantedRefresh(party);
        }

        /// <summary>
        /// Constructs the party info packet, because this is used in a number of packets.
        /// </summary>
        /// <param name="party"></param>
        /// <param name="packet"></param>
        public static void BuildPartyInfo(Party party, ref Packet packet)
        {
            packet.PutLong(party.ID);
            packet.PutString(party.Name);
            packet.PutLong(party.Leader.EntityId);

            packet.PutByte(party.IsOpen);
            packet.PutInt((int)party.Finish);
            packet.PutInt((int)party.ExpRule);

            packet.PutLong(0);                                                      // Quest ID?

            packet.PutInt(party.MaxSize);
            packet.PutInt((int)party.Type);

            packet.PutString(party.DungeonLevel);
            packet.PutString(party.Info);

            packet.PutInt(party.TotalMembers);

            AddPartyMembers(party, ref packet);
        }

        /// <summary>
        /// Adds party member data to the referenced packet.
        /// </summary>
        /// <param name="party"></param>
        /// <param name="packet"></param>
        public static void AddPartyMembers(Party party, ref Packet packet)
        {
            var partyMembers = party.Members;
            for (int i = partyMembers.Count - 1; i >= 0; i--)
            {
                AddPartyMember(partyMembers[i], ref packet);

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
        public static void AddPartyMember(Creature creature, ref Packet packet)
        {
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
        }
    }
}
