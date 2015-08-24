using System.Collections.Generic;
using Aura.Channel.World.Entities;
using Aura.Mabi.Network;
using Aura.Channel.Network.Sending;
using System;
using System.Text;

namespace Aura.Channel.World
{
    public class Party
    {
        private object _sync = new object();

        public PartyType Type { get; private set; }
        public string Name { get; private set; }
        public string DungeonLevel { get; private set; }
        public string Info { get; private set; }
        public string Password { get; private set; }

        public bool HasPassword { get { return Password != ""; } }

        public int MaxSize { get; private set; }

        public bool IsOpen { get; private set; }


        public bool LeaderChangeAllowed { get; set; }

        public PartyFinishRule Finish { get; private set; }
        public PartyExpSharing ExpRule { get; private set; }

        public List<Creature> Members
        {
            get
            {
                lock (_sync)
                {
                    return new List<Creature>(_members) ;
                }
            }
        }

        private List<Creature> _members { get; set; }

        public Creature Leader { get; private set; }

        public int TotalMembers {  get {  lock(_sync) return _members.Count; } }

        public Dictionary<int, Creature> OccupiedSlots { get; private set; }



        /// <summary>
        /// Returns the party name in the format the Party Member Wanted functionality requires.
        /// </summary>
        public string MemberWantedString
        {
            get
            {
                StringBuilder result = new StringBuilder();
                result.AppendFormat("p{ 0}", (int)Type);
                result.AppendFormat("{0:d2}", TotalMembers);
                result.AppendFormat("{0:d2}", MaxSize);
                result.AppendFormat("{0}", (HasPassword ? "y" : "n"));
                result.AppendFormat("{0}", (Type == PartyType.Dungeon ? "[Dungeon] " + Name + "/" + DungeonLevel + "-" + Info : Name));

                return result.ToString();
            }
        }

        public long ID { get; private set; }

        /// <summary>
        /// Creates a dummy party
        /// </summary>
        public Party()
        {
            ID = 0;
            Password = "";
        }

        /// <summary>
        /// Fleshes the party out, with information from the CreateParty packet.
        /// </summary>
        /// <param name="creature"></param>
        /// <param name="packet"></param>
        public void CreateParty(Creature creature)
        {
            _members = new List<Creature>();
            _members.Add(creature);
            creature.PartyPosition = 1;
            LeaderChangeAllowed = true;

            OccupiedSlots = new Dictionary<int, Creature>();
            OccupiedSlots.Add(1, creature);
            Leader = creature;

            ID = PartyManager.GetNextPartyID();
            
            // TODO: add check/conf for maximum party size
        }
        
        public Creature GetMember(long entityID)
        {
            lock (_sync)
            {
                foreach (Creature member in _members)
                {
                    if (member.EntityId == entityID)
                        return member;
                }
            }
            return null;
        }

        private int GetAvailableSlot()
        {
            for(int i = 1; i < MaxSize; i++)
            {
                if (!OccupiedSlots.ContainsKey(i)) return i;
            }
            return 200;
        }

        /// <summary>
        /// Get the next available leader
        /// </summary>
        /// <remarks>Official gives the character that has been created for the longest period of time precedence</remarks>
        /// <returns></returns>
        public Creature GetNextLeader()
        {
            lock (_sync)
            {
                var time = _members[0].CreationTime;
                Creature result = _members[0];

                for (int i = 1; i < _members.Count; i++)
                {
                    if (time < _members[i].CreationTime)
                    {
                        result = _members[i];
                        time = _members[i].CreationTime;
                    }
                }

                return result;
            }
        }

        public bool SetLeader(Creature creature)
        {
            if (LeaderChangeAllowed)
            {
                lock (_sync)
                {
                    if (_members.Contains(creature))
                    {
                        Leader = creature;
                        Send.PartyChangeLeader(this);
                        return true;
                    }
                }
            }
            return false;
        }

        public bool SetLeader(long entityID)
        {
            var creature = GetMember(entityID);

            if(creature != null)
            {
                return SetLeader(creature);
            }

            return false;
        }

        public PartyJoinResult AddMember(Creature creature, string password)
        {
            if (TotalMembers >= MaxSize)
                return PartyJoinResult.Full;           

            if (Password != password)
                return PartyJoinResult.WrongPass;

            this.AddMemberSilent(creature);
            Send.PartyJoinUpdateMembers(creature);

            if (this.IsOpen)
                Send.PartyMemberWantedRefresh(this);
            return PartyJoinResult.Success;
        }

        public void AddMemberSilent(Creature creature)
        {
            lock (_sync)
            {
                _members.Add(creature);

                creature.Party = this;
                creature.PartyPosition = GetAvailableSlot();

                OccupiedSlots.Add(creature.PartyPosition, creature);
            }
        }

        public void RemoveMember(Creature creature)
        {
            RemoveMemberSilent(creature);

            if (TotalMembers == 0)
            {
                Close();
                return;
            }
            Send.PartyLeaveUpdate(creature, this);

            if (IsOpen)
                Send.PartyMemberWantedRefresh(this);

            // What is this?
            //Send.PartyWindowUpdate(creature, party);

            if (Leader == creature)
            {
                SetLeader(GetNextLeader());
                Close();

                Send.PartyChangeLeader(this);
            }

        }

        public void RemoveMemberSilent(Creature creature)
        {
            lock(_sync)
            {
                _members.Remove(creature);
                OccupiedSlots.Remove(creature.PartyPosition);
            }
            
            creature.Party = new Party();
        }

        public void Close()
        {
            if (IsOpen)
            {
                IsOpen = false;
                Send.PartyMemberWantedStateChange(this);
            }
        }

        public void Open()
        {
            if (!IsOpen)
            {
                IsOpen = true;
                Send.PartyMemberWantedStateChange(this);
            }
        }

        /// <summary>
        /// Sends the supplied packet to all members, with the option of replacing the EntityID with each member's personal ID,
        /// and excluding a specific creature.
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="changeID"></param>
        /// <param name="exclude"></param>
        public void Broadcast(Packet packet, bool changeID = false, Creature exclude = null)
        {
            lock (_sync)
            {
                foreach (var member in _members)
                {
                    if (changeID)
                        packet.Id = member.EntityId;

                    if (exclude != member)
                        member.Client.Send(packet);
                }
            }
        }

        public void SetType(PartyType type)
        {
            if (type != Type)
            {
                Type = type;
                Send.PartyTypeUpdate(this);
            }
        }

        public void SetName(string name)
        {
            Name = name;
        }

        public void SetDungeonLevel(string dungeonLevel)
        {
            DungeonLevel = dungeonLevel;
        }

        public void SetInfo(string info)
        {
            Info = info;
        }

        public void SetSize(int size)
        {
            if ((TotalMembers <= size) /* && (Conf max size check here)*/)
                MaxSize = size;
        }

        public void ChangeFinish(int rule)
        {
            Finish = (PartyFinishRule)rule;

            Send.PartyFinishUpdate(this);
        }

        public void ChangeExp(int rule)
        {
            ExpRule = (PartyExpSharing)rule;

            Send.PartyExpUpdate(this);
        }

        public void SetPassword(string pass)
        {
            Password = pass;

            if (IsOpen)
                Send.PartyMemberWantedRefresh(this);
        }

        /// <summary>
        /// Returns a list of all creatures on the altar in the same region as the leader.
        /// </summary>
        /// <returns></returns>
        public List<Creature> OnAltar()
        {
            List<Creature> result = new List<Creature>();

            lock (_sync)
            {
                foreach (Creature member in _members)
                {
                    if (member != Leader)
                        if (member.Region.Id == Leader.Region.Id)
                        {
                            {
                                var pos = member.GetPosition();

                                var clientEvent = member.Region.GetClientEvent(a => a.Data.IsAltar);

                                if (clientEvent.IsInside(pos.X, pos.Y))
                                {
                                    result.Add(member);
                                }
                            }
                        }
                }
            }
            return result;
        }

        /// <summary>
        /// Returns which creatures in the party are both in region, and a specified range.
        /// If no range is supplied, it returns all party creatures within visual(?) range.
        /// </summary>
        /// <remarks>3000 is a total guess as to the actual visible range.</remarks>
        /// <param name="creature"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public List<Creature> GetMembersInRange(Creature creature, int range = -1)
        {
            List<Creature> result = new List<Creature>();
            var pos = creature.GetPosition();

            if (range < 0)
                range = 3000;

            lock(_sync)
            {
                foreach (Creature member in _members)
                {
                    if (member != creature)
                        if (member.Region.Id == creature.Region.Id)
                            if (range > 0)
                            {
                                if (pos.InRange(member.GetPosition(), range))
                                    result.Add(member);
                            }
                            else
                                result.Add(member);

                }
            }
            return result;
        }

        /// <summary>
        /// Returns a list of all creatures in the same region as the specified creature.
        /// </summary>
        /// <param name="creature"></param>
        /// <returns></returns>
        public List<Creature> GetMembersInRegion(Creature creature)
        {
            return GetMembersInRange(creature, 0);
        }

        /// <summary>
        /// Returns a list of all creatures in the region specified.
        /// </summary>
        /// <param name="regionID"></param>
        /// <returns></returns>
        public List<Creature> GetMembersInRegion(int regionID)
        {
            List<Creature> result = new List<Creature>();
            
            lock (_sync)
            {
                foreach (Creature member in _members)
                {
                    if (member.Region.Id == regionID)
                    {
                        result.Add(member);
                    }
                }
            }
            return result;
        }


        /// <summary>
        /// Deals with removing disconnected players from the party.
        /// </summary>
        /// <param name="creature"></param>
        public void DisconnectedMember(Creature creature)
        {
            lock (_sync)
            {
                _members.Remove(creature);
                OccupiedSlots.Remove(creature.PartyPosition);
            }

            if (TotalMembers > 0)
            {
                if (Leader == creature)
                {
                    SetLeader(GetNextLeader());
                    if (IsOpen)
                        Close();
                    Send.PartyChangeLeader(this);
                }
                else
                {
                    if (IsOpen)
                        Send.PartyMemberWantedRefresh(this);
                }
                Send.PartyLeaveUpdate(creature, this);
            }

        }
    }

    public enum PartyFinishRule
    {
        BiggestContributer = 0, Turn = 1, Anyone = 2
    }
    public enum PartyExpSharing
    {
        Equal = 0, MoreToFinish = 1, AllToFinish = 2
    }

    public enum PartyType
    {
        Normal = 0, Dungeon = 1, Jam = 3
    }

    public enum PartyJoinResult
    {
        Full = 0,           // Full or failure?
        Success = 1,
        WrongPass = 4
    }
}
