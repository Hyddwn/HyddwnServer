// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Linq;
using System.Collections.Generic;

namespace Aura.Msgr.Chat
{
	public class ChatSessionManager
	{
		private Dictionary<long, ChatSession> _sessions;

		/// <summary>
		/// Creates new user manager.
		/// </summary>
		public ChatSessionManager()
		{
			_sessions = new Dictionary<long, ChatSession>();
		}

		/// <summary>
		/// Adds session to manager.
		/// </summary>
		/// <param name="session"></param>
		public void Add(ChatSession session)
		{
			lock (_sessions)
				_sessions[session.Id] = session;
		}

		/// <summary>
		/// Removes session from manager.
		/// </summary>
		/// <param name="session"></param>
		public void Remove(ChatSession session)
		{
			lock (_sessions)
				_sessions.Remove(session.Id);
		}

		/// <summary>
		/// Returns session with given id, or null if it doesn't exist.
		/// </summary>
		/// <param name="sessionId"></param>
		/// <returns></returns>
		public ChatSession Get(long sessionId)
		{
			ChatSession result = null;
			lock (_sessions)
				_sessions.TryGetValue(sessionId, out result);
			return result;
		}

		/// <summary>
		/// Returns first session that has only the two given users,
		/// or null, if no such session exists.
		/// </summary>
		/// <param name="contactId1"></param>
		/// <param name="contactId2"></param>
		/// <returns></returns>
		public ChatSession Find(int contactId1, int contactId2)
		{
			lock (_sessions)
				return _sessions.Values.FirstOrDefault(a => a.IsBetween(contactId1, contactId2));
		}
	}
}
