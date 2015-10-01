// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Mabi.Const;
using Aura.Mabi.Network;
using Aura.Msgr.Chat;
using Aura.Msgr.Database;
using System;
using System.Collections.Generic;

namespace Aura.Msgr.Network
{
	public static partial class Send
	{
		public static void LoginR(MsgrClient client, LoginResult result)
		{
			var packet = new Packet(Op.Msgr.LoginR, 0);
			packet.PutInt((int)result);
			if (result == LoginResult.Okay)
			{
				packet.PutInt(client.User.Id);
				packet.PutString(client.User.FullName);
				packet.PutString(client.User.Nickname);
				packet.PutUInt((uint)client.User.ChatOptions);
				packet.PutByte((byte)client.User.Status);
			}

			client.Send(packet);
		}

		/// <summary>
		/// Sends note list to client.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="notes">Set to null for negative response.</param>
		public static void NoteListRequestR(MsgrClient client, List<Note> notes)
		{
			var packet = new Packet(Op.Msgr.NoteListRequestR, 0);

			packet.PutByte(notes != null);
			if (notes != null)
			{
				packet.PutInt(notes.Count);
				foreach (var note in notes)
				{
					packet.PutLong(note.Id);
					packet.PutString(note.Sender);
					packet.PutString(note.Message);
					packet.PutLong(note.GetLongTime());
					packet.PutByte(note.Read);
					packet.PutByte(0); // Hidden if 1?
				}
			}

			client.Send(packet);
		}

		/// <summary>
		/// Sends note to client.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="note">Set to null for negative response.</param>
		public static void ReadNoteR(MsgrClient client, Note note)
		{
			var packet = new Packet(Op.Msgr.ReadNoteR, 0);

			packet.PutByte(note != null);
			if (note != null)
			{
				packet.PutLong(note.Id);
				packet.PutString(note.FromCharacterName);
				packet.PutString(note.FromServer);
				packet.PutLong(note.GetLongTime());
				packet.PutByte(0); // Notification note? (reply disabled)
				packet.PutString(note.Message);
			}

			client.Send(packet);
		}

		/// <summary>
		/// Sends note to client.
		/// </summary>
		/// <param name="client"></param>
		public static void SendNoteR(MsgrClient client)
		{
			var packet = new Packet(Op.Msgr.SendNoteR, 0);

			packet.PutByte(0);

			client.Send(packet);
		}

		/// <summary>
		/// Sends note to client.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="note"></param>
		public static void YouGotNote(MsgrClient client, Note note)
		{
			var packet = new Packet(Op.Msgr.YouGotNote, 0);

			packet.PutLong(note.Id);
			packet.PutString(note.FromCharacterName);
			packet.PutString(note.FromServer);

			client.Send(packet);
		}

		/// <summary>
		/// Updates options on the client on success.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="success"></param>
		public static void ChangeOptionsR(MsgrClient client, bool success)
		{
			var packet = new Packet(Op.Msgr.ChangeOptionsR, 0);

			packet.PutByte(success);
			if (success)
			{
				packet.PutString(client.User.Nickname);
				packet.PutByte((byte)client.User.Status);
				packet.PutUInt((uint)client.User.ChatOptions);
			}

			client.Send(packet);
		}

		/// <summary>
		/// Sends group list to client.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="groups"></param>
		public static void GroupList(MsgrClient client, ICollection<Group> groups)
		{
			var packet = new Packet(Op.Msgr.GroupList, 0);

			// Custom groups + ETC
			packet.PutInt(groups.Count + 1);
			foreach (var group in groups)
			{
				packet.PutInt(group.Id);
				packet.PutString(group.Name);
			}

			// ETC (id:-1) and Blacklist (id:-4) are displayed always.
			packet.PutInt(-1);
			packet.PutString("");

			client.Send(packet);
		}

		/// <summary>
		/// Sends friend list to client.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="friends"></param>
		public static void FriendListRequestR(MsgrClient client, List<Friend> friends)
		{
			var packet = new Packet(Op.Msgr.FriendListRequestR, 0);

			packet.PutInt(friends.Count);
			foreach (var friend in friends)
			{
				packet.PutInt(friend.Id);
				packet.PutByte((byte)friend.FriendshipStatus);
				packet.PutString(friend.FullName);
				packet.PutInt(friend.GroupId);
			}

			client.Send(packet);
		}

		/// <summary>
		/// Response to FriendInvite.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="result"></param>
		/// <param name="friend">Required for success, otherwise can be left null.</param>
		public static void FriendInviteR(MsgrClient client, FriendInviteResult result, Friend friend = null)
		{
			if (result == FriendInviteResult.Success && friend == null)
				throw new ArgumentNullException("friend");

			var packet = new Packet(Op.Msgr.FriendInviteR, 0);

			//packet.PutInt((int)result);
			packet.PutInt((int)result);
			if (result == FriendInviteResult.Success)
			{
				packet.PutInt(friend.Id);
				packet.PutString(friend.FullName);
				packet.PutByte((byte)friend.FriendshipStatus);
			}

			client.Send(packet);
		}

		/// <summary>
		/// Sends live invitation from friend to user.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="friend"></param>
		public static void FriendConfirm(User user, User friend)
		{
			var packet = new Packet(Op.Msgr.FriendConfirm, 0);

			packet.PutInt(friend.Id);
			packet.PutString(friend.Name);
			packet.PutString(friend.Server);
			packet.PutString(friend.FullName);

			user.Client.Send(packet);
		}

		/// <summary>
		/// Notifies user about friend being online.
		/// </summary>
		/// <param name="user"></param>
		public static void FriendOnline(User user, User friend)
		{
			var packet = new Packet(Op.Msgr.FriendOnline, 0);

			packet.PutInt(friend.Id);
			packet.PutString(friend.Nickname);
			packet.PutByte((byte)friend.Status);
			packet.PutString(friend.ChannelName);

			user.Client.Send(packet);
		}

		/// <summary>
		/// Notifies users about friend being online.
		/// </summary>
		/// <param name="user"></param>
		public static void FriendOnline(List<User> users, User friend)
		{
			var packet = new Packet(Op.Msgr.FriendOnline, 0);

			packet.PutInt(friend.Id);
			packet.PutString(friend.Nickname);
			packet.PutByte((byte)friend.Status);
			packet.PutString(friend.ChannelName);

			foreach (var user in users)
				user.Client.Send(packet);
		}

		/// <summary>
		/// Notifies user about friend being offline.
		/// </summary>
		/// <param name="user"></param>
		public static void FriendOffline(User user, User friend)
		{
			var packet = new Packet(Op.Msgr.FriendOffline, 0);
			packet.PutInt(friend.Id);
			user.Client.Send(packet);
		}

		/// <summary>
		/// Notifies users about friend being offline.
		/// </summary>
		/// <param name="user"></param>
		public static void FriendOffline(List<User> users, User friend)
		{
			var packet = new Packet(Op.Msgr.FriendOffline, 0);
			packet.PutInt(friend.Id);

			foreach (var user in users)
				user.Client.Send(packet);
		}

		/// <summary>
		/// Notifies users about friend being offline.
		/// </summary>
		/// <param name="user"></param>
		public static void ChatBeginR(User user, long sessionId, int friendId)
		{
			var packet = new Packet(Op.Msgr.ChatBeginR, 0);

			packet.PutLong(sessionId);
			packet.PutInt(friendId);

			user.Client.Send(packet);
		}

		/// <summary>
		/// Notifies users about someone joining the chat.
		/// </summary>
		/// <param name="session"></param>
		/// <param name="user"></param>
		public static void ChatInviteR(ChatSession session, User user)
		{
			var packet = new Packet(Op.Msgr.ChatInviteR, 0);

			packet.PutLong(session.Id);
			packet.PutInt(user.Id);
			packet.PutString(user.FullName);
			packet.PutString(user.Nickname);

			session.Broadcast(packet);
		}

		/// <summary>
		/// Broadcasts chat message in session.
		/// </summary>
		/// <param name="session"></param>
		/// <param name="contactId"></param>
		/// <param name="message"></param>
		public static void ChatR(ChatSession session, int contactId, string message)
		{
			var packet = new Packet(Op.Msgr.ChatR, 0);

			packet.PutLong(session.Id);
			packet.PutInt(contactId);
			packet.PutString(message);

			session.Broadcast(packet);
		}

		/// <summary>
		/// Sends initial chat session information to user.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="session"></param>
		public static void ChatJoin(User user, ChatSession session)
		{
			var users = session.GetUsers();

			var packet = new Packet(Op.Msgr.ChatJoin, 0);

			packet.PutLong(session.Id);
			packet.PutInt(users.Length);
			foreach (var sessionUser in users)
			{
				packet.PutInt(sessionUser.Id);
				packet.PutString(sessionUser.FullName);
			}

			user.Client.Send(packet);
		}

		/// <summary>
		/// Notifies session user about user closing chat window.
		/// </summary>
		/// <param name="session"></param>
		/// <param name="user"></param>
		public static void ChatLeave(ChatSession session, User user)
		{
			var packet = new Packet(Op.Msgr.ChatLeave, 0);

			packet.PutLong(session.Id);
			packet.PutInt(user.Id);

			session.Broadcast(packet);
		}

		/// <summary>
		/// Response to FriendBlock request.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="success"></param>
		/// <param name="friendId">Only required on success.</param>
		public static void FriendBlockR(User user, bool success, int friendId)
		{
			var packet = new Packet(Op.Msgr.FriendBlockR, 0);

			packet.PutByte(success);
			if (success)
				packet.PutInt(friendId);

			user.Client.Send(packet);
		}

		/// <summary>
		/// Response to FriendUnblock request.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="success"></param>
		/// <param name="friendId">Only required on success.</param>
		public static void FriendUnblockR(User user, bool success, int friendId)
		{
			var packet = new Packet(Op.Msgr.FriendUnblockR, 0);

			packet.PutByte(success);
			if (success)
				packet.PutInt(friendId);

			user.Client.Send(packet);
		}

		/// <summary>
		/// Updates user's status and nickname for all friends.
		/// </summary>
		/// <param name="friends"></param>
		/// <param name="user"></param>
		public static void FriendOptionChanged(List<User> friends, User user)
		{
			var packet = new Packet(Op.Msgr.FriendOptionChanged, 0);
			packet.PutInt(user.Id);
			packet.PutString(user.Nickname);
			packet.PutByte((byte)user.Status);

			foreach (var friendUser in friends)
				friendUser.Client.Send(packet);
		}
	}

	public enum LoginResult
	{
		Okay = 0,
		Fail = 1,
		Pet = 11,
	}

	public enum FriendInviteResult
	{
		Success = 0,
		UnknownError = 1,
		UserNotFound = 2,
		//UnknownError = 3,
		//UnknownError = 4,
		AlreadyFriends = 5,
		//UnknownError = 6,
		//UnknownError = 7,
		MaxReached = 8,
		OwnAccount = 9,
		NoPets = 10,
		//UnknownError = 11,
		UserMaxReached = 12,
	}
}
