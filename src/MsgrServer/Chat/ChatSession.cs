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
	/// <summary>
	/// Represents and manages a chat session between users.
	/// </summary>
	/// <remarks>
	/// Users are differentiated by being active or implicit. The session has
	/// a list of implicit user's ids, the users in that list will auto-join
	/// the chat once a message arrives. They are only removed from that list
	/// when they leave a group chat, this way users always get back into
	/// the same session, even across relogs, because they're pulled back in,
	/// based on their id.
	/// </remarks>
	public class ChatSession
	{
		private static long _ids = 0;

		private object _sync = new object();

		private Dictionary<int, User> _users;
		private HashSet<int> _implicitUsers;

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
			_implicitUsers = new HashSet<int>();

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
				_implicitUsers.Add(user.Id);
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
		/// Adds user to implicit user list, to be notified once a msg gets in.
		/// </summary>
		/// <param name="user"></param>
		public void Add(User user)
		{
			lock (_sync)
				_implicitUsers.Add(user.Id);
		}

		/// <summary>
		/// Removes user from chat session.
		/// </summary>
		/// <param name="user"></param>
		public void Leave(User user)
		{
			lock (_sync)
			{
				_users.Remove(user.Id);

				// Remove user from implicit list if more than one user is left
				// in chat, so the user leaves group chats for good.
				if (_users.Count > 1)
				{
					_implicitUsers.Remove(user.Id);
				}
				// Clear user lists if no active user is left.
				else if (_users.Count == 0)
				{
					_users.Clear();
					_implicitUsers.Clear();
				}
			}

			Send.ChatLeave(this, user);
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
		/// Returns list of implicit users that are currently not in the chat.
		/// </summary>
		/// <returns></returns>
		public List<User> GetInactiveImplicitUsers()
		{
			List<User> result;
			lock (_sync)
				result = MsgrServer.Instance.UserManager.Get(_implicitUsers.Where(id => !_users.ContainsKey(id)));
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
			lock (_sync)
				return (_implicitUsers.Count == 2 && _implicitUsers.Contains(contactId1) && _implicitUsers.Contains(contactId2));
		}
	}
}
