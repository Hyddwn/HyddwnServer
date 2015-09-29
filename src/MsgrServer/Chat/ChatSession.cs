// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Mabi.Network;
using Aura.Msgr.Database;
using Aura.Msgr.Network;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Aura.Msgr.Chat
{
	public class ChatSession
	{
		private static long _ids = 0;

		private object _sync = new object();

		private Dictionary<int, User> _users;
		private Dictionary<int, User> _waitingUsers;

		/// <summary>
		/// Unique identifier for this session.
		/// </summary>
		public long Id { get; private set; }

		/// <summary>
		/// Returns number of active chat users (waiting users excluded).
		/// </summary>
		public int Count { get { lock (_sync) return _users.Count; } }

		/// <summary>
		/// Creates new chat session.
		/// </summary>
		public ChatSession()
		{
			_users = new Dictionary<int, User>();
			_waitingUsers = new Dictionary<int, User>();

			this.Id = Interlocked.Increment(ref _ids);

			MsgrServer.Instance.ChatSessionManager.Add(this);
		}

		/// <summary>
		/// Adds user to chat session and notifies clients.
		/// </summary>
		/// <param name="user"></param>
		public void Join(User user)
		{
			lock (_sync)
			{
				_users[user.Id] = user;
				_waitingUsers.Remove(user.Id);
			}

			// Update clients if this wasn't the initiating user.
			if (this.Count > 1)
			{
				// Notify users about new user
				Send.ChatInviteR(this, user);

				// Send chat information to new user
				Send.ChatJoin(user, this);
			}
		}

		/// <summary>
		/// Adds user to waiting list, to be notified when the first msg gets in.
		/// </summary>
		/// <param name="user"></param>
		public void PreJoin(User user)
		{
			lock (_sync)
				_waitingUsers[user.Id] = user;
		}

		/// <summary>
		/// Removes user from chat session.
		/// </summary>
		/// <param name="user"></param>
		public void Leave(User user)
		{
			lock (_sync)
				_users.Remove(user.Id);
		}

		/// <summary>
		/// Returns true if session has a user with the given id.
		/// </summary>
		/// <param name="contactId"></param>
		/// <returns></returns>
		public bool HasUser(int contactId)
		{
			lock (_sync)
				return _users.ContainsKey(contactId);
		}

		/// <summary>
		/// Returns list of users who are waiting for the first message.
		/// </summary>
		/// <returns></returns>
		public User[] GetWaitingUsers()
		{
			User[] result;
			lock (_sync)
				result = _waitingUsers.Values.ToArray();
			return result;
		}

		/// <summary>
		/// Returns list of all active members.
		/// </summary>
		/// <returns></returns>
		public User[] GetUsers()
		{
			User[] result;
			lock (_sync)
				result = _users.Values.ToArray();
			return result;
		}

		/// <summary>
		/// Sends packet to every user in chat.
		/// </summary>
		public void Broadcast(Packet packet)
		{
			lock (_sync)
			{
				foreach (var user in _users.Values)
					user.Client.Send(packet);
			}
		}

		/// <summary>
		/// Returns true if session is only between the two given users.
		/// </summary>
		/// <param name="contactId1"></param>
		/// <param name="contactId2"></param>
		/// <returns></returns>
		public bool IsBetween(int contactId1, int contactId2)
		{
			// TODO: Optimize... maybe hash the users?
			lock (_sync)
			{
				if (_users.Count == 2 && _users.ContainsKey(contactId1) && _users.ContainsKey(contactId2))
					return true;

				if (
					_users.Count + _waitingUsers.Count == 2 &&
					(_users.ContainsKey(contactId1) || _waitingUsers.ContainsKey(contactId1)) &&
					(_users.ContainsKey(contactId2) || _waitingUsers.ContainsKey(contactId2))
				)
					return true;
			}

			return false;
		}
	}
}
