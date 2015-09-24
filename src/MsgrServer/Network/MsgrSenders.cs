// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Mabi.Const;
using Aura.Mabi.Network;
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
		public static void FriendInviteR(MsgrClient client, FriendInviteResult result, Contact friend = null)
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
				packet.PutByte((byte)FriendshipStatus.Inviting);
			}

			client.Send(packet);
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
