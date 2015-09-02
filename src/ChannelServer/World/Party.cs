// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.World.Entities;
using Aura.Mabi.Const;
using Aura.Mabi.Network;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Aura.Channel.World
{
	public class Party
	{
		private object _sync = new object();

		private List<Creature> _members;
		private Dictionary<int, Creature> _occupiedSlots;

		public PartyType Type { get; private set; }
		public string Name { get; private set; }
		public string DungeonLevel { get; private set; }
		public string Info { get; private set; }
		public string Password { get; private set; }

		public int MaxSize { get; private set; }

		public bool IsOpen { get; private set; }

		public PartyFinishRule Finish { get; private set; }
		public PartyExpSharing ExpRule { get; private set; }

		public Creature Leader { get; private set; }

		public int MemberCount { get { lock (_sync) return _members.Count; } }

		public bool HasPassword { get { return !string.IsNullOrWhiteSpace(this.Password); } }

		public long Id { get; private set; }

		/// <summary>
		/// Creates a dummy party.
		/// </summary>
		public Party()
		{
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

			_occupiedSlots = new Dictionary<int, Creature>();
			_occupiedSlots.Add(1, creature);

			this.Leader = creature;

			creature.PartyPosition = 1;

			this.Id = ChannelServer.Instance.PartyManager.GetNextPartyID();

			// TODO: add check/conf for maximum party size
		}

		public Creature GetMember(long entityId)
		{
			lock (_sync)
				return _members.FirstOrDefault(a => a.EntityId == entityId);
		}

		public Creature[] GetMembers()
		{
			lock (_sync)
				return _members.ToArray();
		}

		private int GetAvailableSlot()
		{
			for (int i = 1; i < this.MaxSize; i++)
			{
				if (!_occupiedSlots.ContainsKey(i))
					return i;
			}

			return 200;
		}

		/// <summary>
		/// Sets next leader automatically.
		/// </summary>
		/// <remarks>
		/// Official gives the character that has been created for the
		/// longest period of time precedence.
		/// </remarks>
		/// <returns></returns>
		public void AutoChooseNextLeader()
		{
			Creature newLeader;

			lock (_sync)
			{
				var time = _members[0].CreationTime;
				newLeader = _members[0];

				for (int i = 1; i < _members.Count; i++)
				{
					if (time < _members[i].CreationTime)
					{
						newLeader = _members[i];
						time = _members[i].CreationTime;
					}
				}
			}

			this.SetLeader(newLeader);
		}

		public bool SetLeader(Creature creature)
		{
			lock (_sync)
			{
				if (!_members.Contains(creature))
					return false;
			}

			this.Leader = creature;
			Send.PartyChangeLeader(this);

			return true;
		}

		public bool SetLeader(long entitiyId)
		{
			var creature = this.GetMember(entitiyId);

			if (creature != null)
				return this.SetLeader(creature);

			return false;
		}

		public PartyJoinResult AddMember(Creature creature, string password)
		{
			if (this.MemberCount >= this.MaxSize)
				return PartyJoinResult.Full;

			if (this.Password != password)
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
				creature.PartyPosition = this.GetAvailableSlot();

				_occupiedSlots.Add(creature.PartyPosition, creature);
			}
		}

		public void RemoveMember(Creature creature)
		{
			this.RemoveMemberSilent(creature);

			if (this.MemberCount == 0)
			{
				this.Close();
				return;
			}

			Send.PartyLeaveUpdate(creature, this);

			if (IsOpen)
				Send.PartyMemberWantedRefresh(this);

			// What is this?
			//Send.PartyWindowUpdate(creature, party);

			if (this.Leader == creature)
			{
				this.AutoChooseNextLeader();
				this.Close();

				Send.PartyChangeLeader(this);
			}
		}

		public void RemoveMemberSilent(Creature creature)
		{
			lock (_sync)
			{
				_members.Remove(creature);
				_occupiedSlots.Remove(creature.PartyPosition);
			}

			creature.Party = new Party();
		}

		public void Close()
		{
			if (!this.IsOpen)
				return;

			this.IsOpen = false;
			Send.PartyMemberWantedStateChange(this);
		}

		public void Open()
		{
			if (this.IsOpen)
				return;

			this.IsOpen = true;
			Send.PartyMemberWantedStateChange(this);
		}

		/// <summary>
		/// Sends the supplied packet to all members, with the option of replacing the EntityID with each member's personal ID,
		/// and excluding a specific creature.
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

		public void SetType(PartyType type)
		{
			if (type == this.Type)
				return;

			this.Type = type;
			Send.PartyTypeUpdate(this);
		}

		public void SetName(string name)
		{
			this.Name = name;
		}

		public void SetDungeonLevel(string dungeonLevel)
		{
			this.DungeonLevel = dungeonLevel;
		}

		public void SetInfo(string info)
		{
			this.Info = info;
		}

		public void SetSize(int size)
		{
			// TODO: Max size conf
			this.MaxSize = Math2.Clamp(this.MemberCount, 8, size);
		}

		public void ChangeFinish(PartyFinishRule rule)
		{
			this.Finish = rule;

			Send.PartyFinishUpdate(this);
		}

		public void ChangeExp(PartyExpSharing rule)
		{
			this.ExpRule = rule;

			Send.PartyExpUpdate(this);
		}

		public void SetPassword(string pass)
		{
			if (string.IsNullOrWhiteSpace(pass))
				pass = null;

			this.Password = pass;

			if (this.IsOpen)
				Send.PartyMemberWantedRefresh(this);
		}

		/// <summary>
		/// Returns a list of all creatures on the altar in the same region as the leader.
		/// </summary>
		/// <returns></returns>
		public List<Creature> OnAltar()
		{
			var result = new List<Creature>();

			lock (_sync)
			{
				foreach (var member in _members.Where(a => a != this.Leader && a.RegionId == this.Leader.RegionId))
				{
					var pos = member.GetPosition();
					var clientEvent = member.Region.GetClientEvent(a => a.Data.IsAltar);

					if (clientEvent.IsInside(pos.X, pos.Y))
						result.Add(member);
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
		/// <param name="range">Use 0 to get every member in the region.</param>
		/// <returns></returns>
		public List<Creature> GetMembersInRange(Creature creature, int range = -1)
		{
			var result = new List<Creature>();
			var pos = creature.GetPosition();

			if (range < 0)
				range = 3000;

			lock (_sync)
			{
				foreach (var member in _members.Where(a => a != creature && a.RegionId == this.Leader.RegionId))
				{
					if (range == 0 || pos.InRange(member.GetPosition(), range))
						result.Add(member);
				}
			}

			return result;
		}

		/// <summary>
		/// Returns a list of all members in the same region as the specified creature.
		/// </summary>
		/// <param name="creature"></param>
		/// <returns></returns>
		public List<Creature> GetMembersInRegion(Creature creature)
		{
			return this.GetMembersInRange(creature, 0);
		}

		/// <summary>
		/// Returns a list of all members in the region specified.
		/// </summary>
		/// <param name="regionId"></param>
		/// <returns></returns>
		public List<Creature> GetMembersInRegion(int regionId)
		{
			var result = new List<Creature>();

			lock (_sync)
				result.AddRange(_members.Where(a => a.RegionId == regionId));

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
				_occupiedSlots.Remove(creature.PartyPosition);
			}

			if (this.MemberCount > 0)
			{
				// Choose new leader if the old one disconnected
				if (this.Leader == creature)
				{
					this.AutoChooseNextLeader();
					this.Close();

					Send.PartyChangeLeader(this);
				}

				if (this.IsOpen)
					Send.PartyMemberWantedRefresh(this);

				Send.PartyLeaveUpdate(creature, this);
			}
		}

		/// <summary>
		/// Returns the party name in the format the Party Member Wanted
		/// functionality requires.
		/// </summary>
		public override string ToString()
		{
			var result = new StringBuilder();

			result.AppendFormat("p{ 0}", (int)this.Type);
			result.AppendFormat("{0:d2}", this.MemberCount);
			result.AppendFormat("{0:d2}", this.MaxSize);
			result.AppendFormat("{0}", (this.HasPassword ? "y" : "n"));
			if (this.Type == PartyType.Dungeon)
				result.AppendFormat("{0}", "[Dungeon] " + this.Name + "/" + this.DungeonLevel + "-" + this.Info);
			else
				result.AppendFormat("{0}", this.Name);

			return result.ToString();
		}
	}
}
