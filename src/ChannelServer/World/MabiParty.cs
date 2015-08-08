using System.Collections.Generic;
using Aura.Channel.World.Entities;
using Aura.Mabi.Network;
using Aura.Channel.Network.Sending;
using System;

namespace Aura.Channel.World
{

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

    /// <summary>
    /// This has a range because parties have to keep their ID when moving from channel to channel.
    /// Whilst NONE of that functionality is currently in place, that was a consideration taken into account,
    /// and so I decided (with help, of course) that giving each channel a range of 100k, and allowing parties to traverse
    /// in this way, as a full entity, would be the best course of action.
    /// 
    /// They'll need to be given space in the DB at a later date (I really hope I'm not the one who has to write that MySQL DB update..)
    /// </summary>
    public static class PartyManager
    {
        /// <summary>
        /// The last ID used by a party.
        /// </summary>
        /// <remarks>I'm not really sure what this would be used for,
        /// but I feel like I'd regret not putting in place..?</remarks>
        public static long CurrentID
        {
            get
            {
                lock(_lock)
                {
                    return Mabi.Const.MabiId.Parties + _offset + _range;
                }
            }
        }

        private static long _offset = 0;
        private static int _range = 0;
        private static object _lock;

        private const int _maxPartyRange = 100000;
        
        static PartyManager()
        {
            ChannelServer.Instance.Events.PlayerDisconnect += PlayerDisconnect;
        }

        /// <summary>
        /// This removes players who disconnect from any party they're in.
        /// </summary>
        /// <param name="creature"></param>
        public static void PlayerDisconnect(Creature creature)
        {
            if (creature.IsInParty)
            {
                creature.Party.DisconnectedMember(creature);
            }
        }

        /// <summary>
        /// Passes the next available ID for use in a party.
        /// </summary>
        /// <returns></returns>
        public static long GetNextPartyID()
        {
            lock (_lock)
            {
                _offset++;
                if (_offset == _maxPartyRange) _offset = 0;

                return Mabi.Const.MabiId.Parties + _offset + _range;
            }
        }

        // TODO: Implement the below >_>
        /// <summary>
        /// This is for setting the range of party IDs, which is set by the server upon connecting to the Login Server.
        /// </summary>
        /// <param name="range"></param>
        internal static void RangeSet(int range)
        {
            _range = range * _maxPartyRange;
        }
    }

    public class MabiParty
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
                    return new List<Creature>(OccupiedSlots.Values) ;
                }
            }
        }

        List<Creature> _members { get; set; }

        public Creature Leader { get; private set; }

        public int TotalMembers {  get {  lock(_sync) return _members.Count; } }

        public Dictionary<int, Creature> OccupiedSlots { get; private set; }



        /// <summary>
        /// Returns the party name in the format the Party Member Wanted functionality requires.
        /// </summary>
        /// <remarks>Using ToString isn't clear/obvious enough that it's got this functionality IMO, but I don't think this is very well named either.</remarks>
        public string MemberWanted
        {
            get
            {
                return string.Format("p{0}{1:d2}{2:d2}{3}{4}", (int)Type, TotalMembers, MaxSize, (HasPassword ? "y" : "n"), (Type == PartyType.Dungeon ? "[Dungeon] " + Name + "/" + DungeonLevel + "-" + Info : Name));
            }
        }

        public long ID { get; private set; }


        /// <summary>
        /// Creates a dummy party
        /// </summary>
        public MabiParty()
        {
            ID = 0;
            Password = "";
        }

        /// <summary>
        /// Fleshes the party out, with information from the CreateParty packet.
        /// </summary>
        /// <param name="creature"></param>
        /// <param name="packet"></param>
        public void CreateParty(Creature creature, Packet packet)
        {
            _members = new List<Creature>();
            _members.Add(creature);
            creature.PartyPosition = 1;
            LeaderChangeAllowed = true;

            OccupiedSlots = new Dictionary<int, Creature>();
            OccupiedSlots.Add(1, creature);
            Leader = creature;

            

            ID = PartyManager.GetNextPartyID();

            ParsePacket(packet);

            // TODO: add check/conf for maximum party size
        }

        private void ParsePacket(Packet packet)
        {
            Type = (PartyType)packet.GetInt();
            Name = packet.GetString();
            if (Type == PartyType.Dungeon)
            {
                DungeonLevel = packet.GetString();
                Info = packet.GetString();
            }
            Password = packet.GetString();
            MaxSize = packet.GetInt();
        }

        public Creature ContainsMember(long entityID)
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
            var creature = ContainsMember(entityID);

            if(creature != null)
            {
                return SetLeader(creature);
            }

            return false;
            
        }

        public void AddMember(Creature creature)
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
            lock(_sync)
            {
                _members.Remove(creature);
                OccupiedSlots.Remove(creature.PartyPosition);
            }
            
            creature.Party = new MabiParty();
        }

        public void Close()
        {
            IsOpen = false;
            Send.PartyMemberWantedStateChange(this);
        }

        public void Open()
        {
            IsOpen = true;
            Send.PartyMemberWantedStateChange(this);
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

        public void ChangeSettings(Packet packet)
        {
            var prevType = Type;
            ParsePacket(packet);

            if (prevType != Type)
                Send.PartyTypeUpdate(this);

            Send.PartySettingUpdate(this);

            if (IsOpen)
                Send.PartyMemberWantedRefresh(this);
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
        /// Returns which creatures in the party are both in region, and a specified range. If no range is supplied, it returns all party creatures within visual(?) range.
        /// </summary>
        /// <remarks>3000 is a total guess as to the actual visible range.</remarks>
        /// <param name="creature"></param>
        /// <param name="range"></param>
        /// <returns></returns>
        public List<Creature> RegionRangeCheck(Creature creature, int range = -1)
        {
            List<Creature> result = new List<Creature>();
            var pos = creature.GetPosition();

            if (range == -1)
                range = 3000;

            lock(_sync)
            {
                foreach (Creature member in _members)
                {
                    if (member != creature)
                        if (member.Region.Id == creature.Region.Id)
                        {
                            if (range > -1)
                            {
                                if (pos.InRange(member.GetPosition(), range))
                                    result.Add(member);
                            }
                        }
                }
            }
            return result;
        }

        /// <summary>
        /// Returns a list of all creatures in the same region as the specified creature.
        /// </summary>
        /// <param name="creature"></param>
        /// <returns></returns>
        public List<Creature> RegionCheck(Creature creature)
        {
            List<Creature> result = new List<Creature>();
            var pos = creature.GetPosition();
            
            lock (_sync)
            {
                foreach (Creature member in _members)
                {
                    if (member != creature)
                        if (member.Region.Id == creature.Region.Id)
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
}
