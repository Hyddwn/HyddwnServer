// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using Aura.Channel.Network.Sending;
using Aura.Channel.World.Entities;
using Aura.Channel.World.Quests;
using Aura.Mabi;
using Aura.Mabi.Const;
using Aura.Mabi.Network;
using Aura.Shared.Util;

namespace Aura.Channel.World
{
    public class Party
    {
        private int _adTimer;
        private int _finisher;

        private readonly List<Creature> _members;
        private readonly Dictionary<int, Creature> _occupiedSlots;
        private readonly object _sync = new object();

        /// <summary>
        ///     Initializes party.
        /// </summary>
        private Party()
        {
            _members = new List<Creature>();
            _occupiedSlots = new Dictionary<int, Creature>();
        }

        /// <summary>
        ///     Party's unique identifier (comparable to EntityId).
        /// </summary>
        public long Id { get; private set; }

        /// <summary>
        ///     Party type.
        /// </summary>
        public PartyType Type { get; private set; }

        /// <summary>
        ///     Party's name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        ///     Dungeon level.
        /// </summary>
        public string DungeonLevel { get; private set; }

        /// <summary>
        ///     Dungeon info.
        /// </summary>
        public string Info { get; private set; }

        /// <summary>
        ///     Password necessary to join the party,
        ///     null or empty for none.
        /// </summary>
        public string Password { get; private set; }

        /// <summary>
        ///     Maximum allowed number of members.
        /// </summary>
        public int MaxSize { get; private set; }

        /// <summary>
        ///     True if the party recruitment window is open.
        /// </summary>
        public bool IsOpen { get; private set; }

        /// <summary>
        ///     Rule about who gets to finish enemies.
        /// </summary>
        public PartyFinishRule Finish { get; private set; }

        /// <summary>
        ///     Rule about how the exp are shared between the members.
        /// </summary>
        public PartyExpSharing ExpRule { get; private set; }

        /// <summary>
        ///     The party's current leader.
        /// </summary>
        public Creature Leader { get; private set; }

        /// <summary>
        ///     Amount of current members.
        /// </summary>
        public int MemberCount
        {
            get
            {
                lock (_sync)
                {
                    return _members.Count;
                }
            }
        }

        /// <summary>
        ///     Returns true if password is not empty.
        /// </summary>
        public bool HasPassword => !string.IsNullOrWhiteSpace(Password);

        /// <summary>
        ///     Returns true if member count is lower than max size.
        /// </summary>
        public bool HasFreeSpace => MemberCount < MaxSize;

        /// <summary>
        ///     Unique id of the quest set as party quest.
        /// </summary>
        public Quest Quest { get; private set; }

        /// <summary>
        ///     Used in guild creation.
        /// </summary>
        public bool GuildCreationInProgress { get; set; }

        /// <summary>
        ///     Used in guild creation.
        /// </summary>
        public string GuildNameToBe { get; set; }

        /// <summary>
        ///     Used in guild creation.
        /// </summary>
        public GuildType GuildTypeToBe { get; set; }

        /// <summary>
        ///     Used in guild creation.
        /// </summary>
        public GuildVisibility GuildVisibilityToBe { get; set; }

        /// <summary>
        ///     Used in guild creation.
        /// </summary>
        public bool GuildNameVoteRequested { get; set; }

        /// <summary>
        ///     Used in guild creation to keep track of name votes.
        /// </summary>
        public int GuildNameVoteCount { get; set; }

        /// <summary>
        ///     Used in guild creation to keep track of name votes.
        /// </summary>
        public int GuildNameVotes { get; set; }

        /// <summary>
        ///     Returns true if any of the party members has a pet spawned.
        /// </summary>
        public bool HasPets
        {
            get { return GetMembers().Any(a => a.Pet != null); }
        }

        /// <summary>
        ///     Unsubscribes from ad tick.
        /// </summary>
        ~Party()
        {
            // Just in case if didn't happen anywhere else for some reason.
            ChannelServer.Instance.Events.MinutesTimeTick -= OnMinutesTimeTick;
        }

        /// <summary>
        ///     Creates new party with creature as leader.
        /// </summary>
        /// <param name="creature"></param>
        public static Party Create(Creature creature, PartyType type, string name, string dungeonLevel, string info,
            string password, int maxSize)
        {
            var party = new Party();

            party.Id = ChannelServer.Instance.PartyManager.GetNextPartyId();

            party._members.Add(creature);
            party._occupiedSlots.Add(1, creature);
            party.Leader = creature;
            party.SetSettings(type, name, dungeonLevel, info, password, maxSize);

            creature.PartyPosition = 1;

            ChannelServer.Instance.Events.MinutesTimeTick += party.OnMinutesTimeTick;

            return party;
        }

        /// <summary>
        ///     Creates new dummy party for creature.
        /// </summary>
        /// <param name="creature"></param>
        public static Party CreateDummy(Creature creature)
        {
            var party = new Party();

            party._members.Add(creature);
            party._occupiedSlots.Add(1, creature);
            party.Leader = creature;

            creature.PartyPosition = 1;

            return party;
        }

        /// <summary>
        ///     Changes settings and updates clients.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="dungeonLevel"></param>
        /// <param name="info"></param>
        /// <param name="password"></param>
        /// <param name="maxSize"></param>
        public void ChangeSettings(PartyType type, string name, string dungeonLevel, string info, string password,
            int maxSize)
        {
            SetSettings(type, name, dungeonLevel, info, password, maxSize);

            Send.PartyTypeUpdate(this);

            if (IsOpen)
                Send.PartyMemberWantedRefresh(this);

            Send.PartySettingUpdate(this);
        }

        /// <summary>
        ///     Sets given options without updating the clients.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="name"></param>
        /// <param name="dungeonLevel"></param>
        /// <param name="info"></param>
        /// <param name="password"></param>
        /// <param name="maxSize"></param>
        private void SetSettings(PartyType type, string name, string dungeonLevel, string info, string password,
            int maxSize)
        {
            Type = type;
            Name = name;
            DungeonLevel = string.IsNullOrWhiteSpace(dungeonLevel) ? null : dungeonLevel;
            Info = string.IsNullOrWhiteSpace(info) ? null : info;
            Password = string.IsNullOrWhiteSpace(password) ? null : password;
            MaxSize = Math2.Clamp(MemberCount, ChannelServer.Instance.Conf.World.PartyMaxSize, maxSize);
        }

        /// <summary>
        ///     Returns party member by entity id, or null if it doesn't exist.
        /// </summary>
        /// <param name="entityId"></param>
        /// <returns></returns>
        public Creature GetMember(long entityId)
        {
            lock (_sync)
            {
                return _members.FirstOrDefault(a => a.EntityId == entityId);
            }
        }

        /// <summary>
        ///     Returns list of all members.
        /// </summary>
        /// <returns></returns>
        public Creature[] GetMembers()
        {
            lock (_sync)
            {
                return _members.ToArray();
            }
        }

        /// <summary>
        ///     Returns list of all members, sorted by their position in the party.
        /// </summary>
        /// <returns></returns>
        public Creature[] GetSortedMembers()
        {
            lock (_sync)
            {
                return _members.OrderBy(a => a.PartyPosition).ToArray();
            }
        }

        /// <summary>
        ///     Returns list of all members that match the predicate, sorted by
        ///     their position in the party.
        /// </summary>
        /// <returns></returns>
        public Creature[] GetSortedMembers(Func<Creature, bool> predicate)
        {
            lock (_sync)
            {
                return _members.Where(predicate).OrderBy(a => a.PartyPosition).ToArray();
            }
        }

        /// <summary>
        ///     Returns first available slot, throws if none are available.
        ///     Check availability before adding members.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="Exception">
        ///     Thrown if no free slots are available.
        /// </exception>
        private int GetAvailableSlot()
        {
            for (var i = 1; i <= MaxSize; i++)
                if (!_occupiedSlots.ContainsKey(i))
                    return i;

            throw new Exception("No free slot found.");
        }

        /// <summary>
        ///     Sets next leader automatically.
        /// </summary>
        /// <remarks>
        ///     Official gives the character that has been created for the
        ///     longest period of time precedence.
        /// </remarks>
        /// <returns></returns>
        public void AutoChooseNextLeader()
        {
            Creature newLeader;

            lock (_sync)
            {
                var time = _members[0].CreationTime;
                newLeader = _members[0];

                for (var i = 1; i < _members.Count; i++)
                    if (time < _members[i].CreationTime)
                    {
                        newLeader = _members[i];
                        time = _members[i].CreationTime;
                    }
            }

            SetLeader(newLeader);
        }

        /// <summary>
        ///     Sets leader to given creature, if possible.
        /// </summary>
        /// <param name="creature"></param>
        /// <returns></returns>
        public bool SetLeader(Creature creature)
        {
            lock (_sync)
            {
                if (!_members.Contains(creature))
                    return false;
            }

            // Close ad if it was open and reopen it after changing
            // leader, to make it appear above his head instead.
            var wasOpen = IsOpen;
            if (wasOpen)
                Close();

            Leader = creature;
            Send.PartyChangeLeader(this);

            if (wasOpen)
                Open();

            return true;
        }

        /// <summary>
        ///     Sets leader to given entity, if possible.
        /// </summary>
        /// <param name="entitiyId"></param>
        /// <returns></returns>
        public bool SetLeader(long entitiyId)
        {
            var creature = GetMember(entitiyId);

            if (creature != null)
                return SetLeader(creature);

            Close();

            return false;
        }

        /// <summary>
        ///     Adds creature to party and updates the clients.
        /// </summary>
        /// <param name="creature"></param>
        /// <returns></returns>
        public void AddMember(Creature creature)
        {
            AddMemberSilent(creature);
            Send.PartyJoinUpdateMembers(creature);

            // Inform about party quest if set
            var quest = Quest;
            if (quest != null)
            {
                creature.Quests.AddSilent(quest);
                Send.NewQuest(creature, quest);
            }

            if (IsOpen)
                Send.PartyMemberWantedRefresh(this);
        }

        /// <summary>
        ///     Adds creature to party without updating the clients.
        /// </summary>
        /// <param name="creature"></param>
        public void AddMemberSilent(Creature creature)
        {
            lock (_sync)
            {
                _members.Add(creature);

                creature.Party = this;
                creature.PartyPosition = GetAvailableSlot();

                _occupiedSlots.Add(creature.PartyPosition, creature);
            }
        }

        /// <summary>
        ///     Removes creature from party if it's in it and updates the clients.
        /// </summary>
        /// <param name="creature"></param>
        public void RemoveMember(Creature creature)
        {
            RemoveMemberSilent(creature);

            // Handle quest
            if (Quest != null)
                if (creature == Leader || MemberCount < ChannelServer.Instance.Conf.World.PartyQuestMinSize)
                    UnsetPartyQuest();
                else
                    creature.Quests.Remove(Quest);

            if (MemberCount == 0)
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
                AutoChooseNextLeader();
                Close();

                Send.PartyChangeLeader(this);
            }
        }

        /// <summary>
        ///     Removes creature from party without updating the clients.
        /// </summary>
        /// <param name="creature"></param>
        public void RemoveMemberSilent(Creature creature)
        {
            // TODO: Unify removing/leaving/dcing

            lock (_sync)
            {
                _members.Remove(creature);
                _occupiedSlots.Remove(creature.PartyPosition);
            }

            if (MemberCount == 0)
                ChannelServer.Instance.Events.MinutesTimeTick -= OnMinutesTimeTick;

            creature.Party = CreateDummy(creature);
        }

        /// <summary>
        ///     Closes members wanted ad.
        /// </summary>
        public void Close()
        {
            if (!IsOpen)
                return;

            IsOpen = false;

            Send.PartyWantedClosed(this);
            Send.PartyMemberWantedRefresh(this);
        }

        /// <summary>
        ///     Opens members wanted ad.
        /// </summary>
        public void Open()
        {
            if (IsOpen)
                return;

            IsOpen = true;

            Send.PartyWantedOpened(this);
            Send.PartyMemberWantedRefresh(this);
        }

        /// <summary>
        ///     Sends the supplied packet to all members, with the option of replacing the EntityID with each member's personal ID,
        ///     and excluding a specific creature.
        /// </summary>
        /// <param name="packet"></param>
        /// <param name="useMemberEntityId"></param>
        /// <param name="exclude"></param>
        public void Broadcast(Packet packet, bool useMemberEntityId = false, Creature exclude = null)
        {
            lock (_sync)
            {
                foreach (var member in _members)
                {
                    if (useMemberEntityId)
                        packet.Id = member.EntityId;

                    if (exclude != member)
                        member.Client.Send(packet);
                }
            }
        }

        /// <summary>
        ///     Sets party type.
        /// </summary>
        /// <param name="type"></param>
        public void SetType(PartyType type)
        {
            if (type == Type)
                return;

            Type = type;
            Send.PartyTypeUpdate(this);
        }

        /// <summary>
        ///     Sets party name.
        /// </summary>
        /// <remarks>
        ///     TODO: Kinda redundant, use property?
        /// </remarks>
        /// <param name="name"></param>
        public void SetName(string name)
        {
            Name = name;
        }

        /// <summary>
        ///     Sets dungeon level.
        /// </summary>
        /// <param name="dungeonLevel"></param>
        public void SetDungeonLevel(string dungeonLevel)
        {
            DungeonLevel = dungeonLevel;
        }

        /// <summary>
        ///     Sets party info.
        /// </summary>
        /// <param name="info"></param>
        public void SetInfo(string info)
        {
            Info = info;
        }

        /// <summary>
        ///     Sets party's max size.
        /// </summary>
        /// <param name="size"></param>
        public void SetMaxSize(int size)
        {
            MaxSize = Math2.Clamp(MemberCount, ChannelServer.Instance.Conf.World.PartyMaxSize, size);

            if (IsOpen)
                Send.PartyMemberWantedRefresh(this);
        }

        /// <summary>
        ///     Change finishing rule.
        /// </summary>
        /// <param name="rule"></param>
        public void ChangeFinish(PartyFinishRule rule)
        {
            Finish = rule;

            Send.PartyFinishUpdate(this);
        }

        /// <summary>
        ///     Changes exp sharing rule.
        /// </summary>
        /// <param name="rule"></param>
        public void ChangeExp(PartyExpSharing rule)
        {
            ExpRule = rule;

            Send.PartyExpUpdate(this);
        }

        /// <summary>
        ///     Sets party's password, set to empty string or null to disable.
        /// </summary>
        /// <param name="pass"></param>
        public void SetPassword(string pass)
        {
            if (string.IsNullOrWhiteSpace(pass))
                pass = null;

            Password = pass;

            if (IsOpen)
                Send.PartyMemberWantedRefresh(this);
        }

        /// <summary>
        ///     Returns a list of all members standing on the altar in the given region.
        /// </summary>
        /// <remarks>
        ///     This and other functions assume that there's only ever one altar
        ///     per region. Should this change at any point, these functions have
        ///     to be fixed.
        /// </remarks>
        /// <param name="regionId"></param>
        /// <returns></returns>
        public List<Creature> GetCreaturesOnAltar(int regionId)
        {
            var result = new List<Creature>();

            lock (_sync)
            {
                foreach (var member in _members.Where(a => a.RegionId == regionId))
                {
                    var pos = member.GetPosition();
                    var clientEvent = member.Region.GetClientEvent(a => a.Data.IsAltar);

                    if (clientEvent != null && clientEvent.IsInside(pos))
                        result.Add(member);
                }
            }

            return result;
        }

        /// <summary>
        ///     Returns party members in range of given creature, but not the
        ///     creature itself.
        /// </summary>
        /// <remarks>3000 is a total guess as to the actual visible range.</remarks>
        /// <param name="creature">Reference creature</param>
        /// <param name="range">Pass -1 for visual range.</param>
        /// <returns></returns>
        public List<Creature> GetMembersInRange(Creature creature, int range = -1)
        {
            var result = new List<Creature>();
            var pos = creature.GetPosition();

            if (range < 0)
                range = 3000;

            lock (_sync)
            {
                foreach (var member in _members.Where(a => a != creature && a.RegionId == Leader.RegionId))
                    if (range == 0 || pos.InRange(member.GetPosition(), range))
                        result.Add(member);
            }

            return result;
        }

        /// <summary>
        ///     Returns a list of all members in the region specified.
        /// </summary>
        /// <param name="regionId"></param>
        /// <returns></returns>
        public List<Creature> GetMembersInRegion(int regionId)
        {
            var result = new List<Creature>();

            lock (_sync)
            {
                result.AddRange(_members.Where(a => a.RegionId == regionId));
            }

            return result;
        }

        /// <summary>
        ///     Deals with removing disconnected players from the party.
        /// </summary>
        /// <param name="creature"></param>
        public void DisconnectedMember(Creature creature)
        {
            lock (_sync)
            {
                _members.Remove(creature);
                _occupiedSlots.Remove(creature.PartyPosition);
            }

            // Handle quest
            if (Quest != null)
                if (creature == Leader || MemberCount < ChannelServer.Instance.Conf.World.PartyQuestMinSize)
                    UnsetPartyQuest();
                else
                    creature.Quests.Remove(Quest);

            if (MemberCount > 0)
            {
                // Choose new leader if the old one disconnected
                if (Leader == creature)
                {
                    AutoChooseNextLeader();
                    Close();

                    Send.PartyChangeLeader(this);
                }

                if (IsOpen)
                    Send.PartyMemberWantedRefresh(this);

                Send.PartyLeaveUpdate(creature, this);
            }

            if (MemberCount == 0)
                ChannelServer.Instance.Events.MinutesTimeTick -= OnMinutesTimeTick;
        }

        /// <summary>
        ///     Raised once every minute.
        /// </summary>
        /// <param name="time"></param>
        private void OnMinutesTimeTick(ErinnTime time)
        {
            if (Type == PartyType.Dungeon && IsOpen)
            {
                if (_adTimer++ < 5)
                    return;

                Send.PartyAdChat(this);
            }

            _adTimer = 0;
        }

        /// <summary>
        ///     Returns the party name in the format the Party Member Wanted
        ///     functionality requires.
        /// </summary>
        public override string ToString()
        {
            var name = Type != PartyType.Dungeon ? Name : "[Dungeon] " + Name + "/" + DungeonLevel + "-" + Info;
            var password = HasPassword ? "y" : "n";

            return string.Format("p{0}{1:d2}{2:d2}{3}{4}", (int) Type, MemberCount, MaxSize, password, name);
        }

        /// <summary>
        ///     Returns true if password is correct or none is set.
        /// </summary>
        /// <returns></returns>
        public bool CheckPassword(string password)
        {
            if (string.IsNullOrWhiteSpace(Password))
                return true;

            return password == Password;
        }

        /// <summary>
        ///     Sets party quest, removing previous ones and updating all members.
        /// </summary>
        /// <param name="quest"></param>
        public void SetPartyQuest(Quest quest)
        {
            if (Quest != null)
                UnsetPartyQuest();

            Quest = quest;

            // Give quest to other members
            lock (_sync)
            {
                foreach (var member in _members.Where(a => a != Leader))
                {
                    member.Quests.AddSilent(quest);
                    Send.NewQuest(member, quest);
                }
            }

            Send.PartySetActiveQuest(this, quest.UniqueId);
        }

        /// <summary>
        ///     Unsets party quest, removes it from all normal member's managers,
        ///     and updates the clients. Returns false if no party quest was set.
        /// </summary>
        /// <param name="quest"></param>
        public bool UnsetPartyQuest()
        {
            var quest = Quest;
            if (quest == null)
                return false;

            Quest = null;

            // Remove quest from other members
            lock (_sync)
            {
                foreach (var member in _members.Where(a => a != Leader))
                    member.Quests.Remove(quest);
            }

            Send.PartyUnsetActiveQuest(this, quest.UniqueId);
            return true;
        }

        /// <summary>
        ///     Returns the next eligable finisher for the "in turn" finisher
        ///     rule.
        /// </summary>
        /// <returns></returns>
        public Creature GetNextFinisher()
        {
            var members = GetMembers();

            _finisher++;
            if (_finisher >= members.Length)
                _finisher = 0;

            return members[_finisher];
        }
    }
}