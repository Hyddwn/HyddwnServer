// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Mabi.Const;
using Aura.Mabi.Network;
using Aura.Msgr.Database;
using Aura.Shared.Network;
using Aura.Shared.Util;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Aura.Msgr.Chat;

namespace Aura.Msgr.Network
{
	public partial class MsgrServerHandlers : PacketHandlerManager<MsgrClient>
	{
		private Regex _receiverRegex = new Regex(@"^[a-z0-9]+@[a-z0-9_]+$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

		/// <summary>
		/// Checks if user is logged in before calling non-login handlers.
		/// </summary>
		/// <param name="client"></param>
		/// <param name="packet"></param>
		public override void Handle(MsgrClient client, Packet packet)
		{
			// Check if logged in for non-login packets
			if (packet.Op != Op.Msgr.Login && client.User == null)
			{
				Log.Warning("Attempted sending of non-login packet from '{0}' before login.", client.Address);
				return;
			}

			//Log.Debug(packet);

			base.Handle(client, packet);
		}

		/// <summary>
		/// Sent upon logging into to channel, to log into msgr as well.
		/// </summary>
		/// <example>
		/// 001 [................] String : -1
		/// 002 [0010000000000002] Long   : 4503599627370498
		/// 003 [................] String : Zerono
		/// 004 [................] String : Aura
		/// 005 [................] String : Ch1
		/// 006 [................] String : admin
		/// 007 [0AACDC6249D7B0EC] Long   : 769231951077290220
		/// 008 [0000000000000000] Long   : 0
		/// 009 [........00000000] Int    : 0
		/// </example>
		[PacketHandler(Op.Msgr.Login)]
		public void Login(MsgrClient client, Packet packet)
		{
			var unkString = packet.GetString();
			var entityId = packet.GetLong();
			var entityName = packet.GetString();
			var server = packet.GetString();
			var channelName = packet.GetString();
			var accountId = packet.GetString();
			var sessionKey = packet.GetLong();
			var unkLong = packet.GetLong();
			var unkInt = packet.GetInt();

			// Check session
			if (!MsgrServer.Instance.Database.CheckSession(accountId, sessionKey))
			{
				Log.Warning("Someone tried to login with invalid session (Account: {0}).", accountId);
				client.Kill();
				return;
			}

			// Check if pet
			if (entityId >= MabiId.Pets)
			{
				Send.LoginR(client, LoginResult.Pet);
				return;
			}

			// Check character
			if (!MsgrServer.Instance.Database.AccountHasCharacter(accountId, entityId, server))
			{
				Log.Warning("User '{0}' tried to login with invalid character.", accountId);
				client.Kill();
				return;
			}

			// Get contact
			// If no contact with this data exists in the db yet it's created,
			// this way we don't have to worry about creating contacts for existing
			// characters and the msgr server can operate independently.
			client.User = MsgrServer.Instance.Database.GetOrCreateContact(accountId, entityId, entityName, server, channelName);
			if (client.User == null)
			{
				Log.Warning("Failed to get or create contact for user '{0}'.", accountId);
				Send.LoginR(client, LoginResult.Fail);
				return;
			}

			client.User.Client = client;

			Log.Info("User '{0}' logged in as '{1}'.", client.User.AccountId, client.User.FullName);

			Send.LoginR(client, LoginResult.Okay);

			MsgrServer.Instance.UserManager.Add(client.User);
		}

		/// <summary>
		/// Sent when opening notes, requests lists of existing notes.
		/// </summary>
		/// <remarks>
		/// When you open your inbox, log out, and log back in, the note
		/// list will be empty, since the client only requests the notes
		/// every once in a while, but empties the box on log out.
		/// This is expected behavior, even though it shouldn't be.
		/// TODO: Try to fix devCAT's mistakes?
		/// </remarks>
		/// <example>
		/// 001 [0000000000000000] Long   : 0
		/// </example>
		[PacketHandler(Op.Msgr.NoteListRequest)]
		public void NoteListRequest(MsgrClient client, Packet packet)
		{
			var unkLong = packet.GetLong();

			var notes = MsgrServer.Instance.Database.GetNotes(client.User);

			Send.NoteListRequestR(client, notes);
		}

		/// <summary>
		/// Sent when double-clicking note, to open and read it.
		/// </summary>
		/// <example>
		/// 001 [0000000000000001] Long   : 1
		/// </example>
		[PacketHandler(Op.Msgr.ReadNote)]
		public void ReadNote(MsgrClient client, Packet packet)
		{
			var noteId = packet.GetLong();

			// TODO: Cache everything in memory?
			var note = MsgrServer.Instance.Database.GetNote(noteId);

			// Check note
			if (note == null)
			{
				Log.Warning("User '{0}' requested a non-existent note.", client.User.AccountId);
				Send.ReadNoteR(client, null);
				return;
			}

			// Check receiver
			if (note.Receiver != client.User.FullName)
			{
				Log.Warning("User '{0}' tried to read a note that's not his.", client.User.AccountId);
				Send.ReadNoteR(client, null);
				return;
			}

			MsgrServer.Instance.Database.SetNoteRead(noteId);

			Send.ReadNoteR(client, note);
		}

		/// <summary>
		/// Sent when opening notes, requests lists of existing notes.
		/// </summary>
		/// <example>
		/// 001 [................] String : admin
		/// 002 [................] String : Foobar@Aura
		/// 003 [................] String : hi there
		/// </example>
		[PacketHandler(Op.Msgr.SendNote)]
		public void SendNote(MsgrClient client, Packet packet)
		{
			var fromAccountId = packet.GetString().Trim();
			var receiver = packet.GetString().Trim();
			var message = packet.GetString().Trim();

			// Check message length
			if (message.Length > 200)
			{
				Log.Warning("User '{0}' tried to send a message that's longer than 200 characters.", client.User.AccountId);
				return;
			}

			// Check validity of receiver
			if (!_receiverRegex.IsMatch(receiver))
			{
				Log.Warning("User '{0}' tried to send a message to invalid receiver '{1}'.", client.User.AccountId, receiver);
				return;
			}

			// TODO: The messenger is kinda made for direct database access,
			//   but doing that with MySQL isn't exactly efficient...
			//   Maybe we should use a different solution for the msgr?
			//   On the other hand, we might want to create a web interface,
			//   in which case MySQL offers more flexibility.

			// TODO: You should be able to send a message to a character that
			//   has never logged in, so we can't check for contact existence,
			//   but this way someone could flood the database. Spam check?

			MsgrServer.Instance.Database.AddNote(client.User.FullName, receiver, message);

			Send.SendNoteR(client);
		}

		/// <summary>
		/// Sent when clicking Delete in note inbox.
		/// </summary>
		/// <remarks>
		/// Please tell me you don't let the client tell you which account
		/// to delete that note from, devCAT...
		/// </remarks>
		/// <example>
		/// 001 [................] String : admin
		/// 002 [0000000000000007] Long   : 7
		/// </example>
		[PacketHandler(Op.Msgr.DeleteNote)]
		public void DeleteNote(MsgrClient client, Packet packet)
		{
			var accountId = packet.GetString();
			var noteId = packet.GetLong();

			MsgrServer.Instance.Database.DeleteNote(client.User.FullName, noteId);

			// Delete doesn't seem to have a response, the note disappears as
			// soon as you click Delete, the server is only notified.
		}

		/// <summary>
		/// Sent every minute, to check for new notes.
		/// </summary>
		/// <remarks>
		/// The offical server seems to only send the latest unread note,
		/// sending the response multiple times, for multiple notes,
		/// doesn't seem to do anything.
		/// 
		/// The moment this packet is sent is probably also the moment at
		/// which the client empties the inbox cache, otherwise you
		/// wouldn't get the new note in the list.
		/// </remarks>
		/// <example>
		/// 001 [0000000000000009] Long   : 9
		/// </example>
		[PacketHandler(Op.Msgr.CheckNotes)]
		public void CheckNotes(MsgrClient client, Packet packet)
		{
			// Id of the newest note the client knows about
			var noteId = packet.GetLong();

			var note = MsgrServer.Instance.Database.GetLatestUnreadNote(client.User.FullName, noteId);
			if (note != null)
				Send.YouGotNote(client, note);
		}

		/// <summary>
		/// 
		/// </summary>
		/// <example>
		/// 001 [................] String : asd
		/// 002 [..............30] Byte   : 48
		/// 003 [........80000000] Int    : -2147483648
		/// </example>
		[PacketHandler(Op.Msgr.ChangeOptions)]
		public void ChangeOptions(MsgrClient client, Packet packet)
		{
			var nickname = packet.GetString();
			var status = (ContactStatus)packet.GetByte();
			var chatOptions = (ChatOptions)packet.GetUInt();

			var user = client.User;

			// Check nickname
			if (nickname.Length > 50)
			{
				Log.Warning("User '{0}' tried to use a nickname that's longer than 50 characters.", user.AccountId);
				Send.ChangeOptionsR(client, false);
				return;
			}

			// Check status
			if (!Enum.IsDefined(typeof(ContactStatus), status))
			{
				Log.Warning("User '{0}' tried to use an invalid or unknown status ({1}).", user.AccountId, status);
				Send.ChangeOptionsR(client, false);
				return;
			}

			// Check options
			if (!Enum.IsDefined(typeof(ChatOptions), chatOptions))
			{
				Log.Warning("User '{0}' tried to use a invalid or unknown options ({1}).", user.AccountId, status);
				Send.ChangeOptionsR(client, false);
				return;
			}

			// TODO: Notify friends about changed options?

			user.Nickname = nickname;
			user.Status = status;
			user.ChatOptions = chatOptions;

			MsgrServer.Instance.Database.SaveOptions(user);

			Send.ChangeOptionsR(client, true);
		}

		/// <summary>
		/// Sent upon login, to request the group and the friend list.
		/// </summary>
		/// <example>
		/// No parameters.
		/// </example>
		[PacketHandler(Op.Msgr.FriendListRequest)]
		public void FriendListRequest(MsgrClient client, Packet packet)
		{
			var user = client.User;

			// Lists are sorted alphabetically by the client
			var groups = MsgrServer.Instance.Database.GetGroups(user);
			var friends = MsgrServer.Instance.Database.GetFriends(user);

			user.Groups.Clear();
			foreach (var group in groups)
				user.Groups.Add(group.Id);

			user.Friends.Clear();
			user.Friends.AddRange(friends);

			Send.GroupList(client, groups);
			Send.FriendListRequestR(client, friends);

			// Notify friends about user going online
			var friendUsers = MsgrServer.Instance.UserManager.Get(friends.Select(a => a.Id));
			if (friendUsers.Count != 0)
			{
				Send.FriendOnline(friendUsers, user);

				// Notify user about online friends
				foreach (var friendUser in friendUsers)
					Send.FriendOnline(client.User, friendUser);
			}
		}

		/// <summary>
		/// Notification about creation of a new group,
		/// doesn't seem to have a response.
		/// </summary>
		/// <remarks>
		/// Right, let the client decide the group id, what could possibly
		/// go wrong. Is this fixable? There's no response, the client will
		/// try to work with that id =/
		/// </remarks>
		/// <example>
		/// 001 [........00000001] Int    : 1
		/// 002 [................] String : test
		/// </example>
		[PacketHandler(Op.Msgr.AddGroup)]
		public void AddGroup(MsgrClient client, Packet packet)
		{
			var groupId = packet.GetInt();
			var groupName = packet.GetString();

			if (groupId <= 0)
			{
				Log.Warning("User '{0}' tried to add group with invalid id '{1}'.", client.User.AccountId, groupId);
				client.Kill(); // Not killing the connection would desync the client
				return;
			}

			var group = new Database.Group();
			group.Id = groupId;
			group.Name = groupName;

			client.User.Groups.Add(groupId);
			MsgrServer.Instance.Database.AddGroup(client.User, group);
		}

		/// <summary>
		/// Notification about group renaming, no response.
		/// </summary>
		/// <example>
		/// 001 [........00000002] Int    : 2
		/// 002 [................] String : test23
		/// </example>
		[PacketHandler(Op.Msgr.RenameGroup)]
		public void RenameGroup(MsgrClient client, Packet packet)
		{
			var groupId = packet.GetInt();
			var groupName = packet.GetString();

			// Check if predefined group
			if (groupId <= 0)
			{
				Log.Warning("User '{0}' tried to rename group with invalid id '{1}'.", client.User.AccountId, groupId);
				return; // No need for a client kill, we don't care about the name.
			}

			// Check group
			if (!client.User.Groups.Contains(groupId))
			{
				Log.Warning("User '{0}' tried to rename non-existent group.", client.User.AccountId, groupId);
				return;
			}

			MsgrServer.Instance.Database.RenameGroup(client.User, groupId, groupName);
		}

		/// <summary>
		/// Notification about moving friend to another group, no response.
		/// </summary>
		/// <example>
		/// 001 [........00000003] Int    : 3
		/// 002 [........00000001] Int    : 1
		/// </example>
		[PacketHandler(Op.Msgr.ChangeGroup)]
		public void ChangeGroup(MsgrClient client, Packet packet)
		{
			var friendContactId = packet.GetInt();
			var groupId = packet.GetInt();

			// Check friend
			var friend = client.User.GetFriend(friendContactId);
			if (friend == null)
			{
				Log.Warning("ChangeGroup: User '{0}' tried to change group of non-existent friend.", client.User.AccountId);
				client.Kill();
				return;
			}

			// Check group
			if (groupId != -1 && groupId != -4 && !client.User.Groups.Contains(groupId))
			{
				Log.Warning("ChangeGroup: User '{0}' tried to change friend's group to an invalid one.", client.User.AccountId);
				client.Kill();
				return;
			}

			friend.GroupId = groupId;
			MsgrServer.Instance.Database.ChangeGroup(client.User, friendContactId, groupId);
		}

		/// <summary>
		/// Notification about deletion of a group, no response.
		/// </summary>
		/// <example>
		/// 002 [........00000001] Int    : 1
		/// </example>
		[PacketHandler(Op.Msgr.DeleteGroup)]
		public void DeleteGroup(MsgrClient client, Packet packet)
		{
			var groupId = packet.GetInt();

			// Check group
			if (!client.User.Groups.Contains(groupId))
			{
				Log.Warning("DeleteGroup: User '{0}' tried to delete an invalid group.", client.User.AccountId);
				client.Kill();
				return;
			}

			client.User.Groups.Remove(groupId);
			MsgrServer.Instance.Database.DeleteGroup(client.User, groupId);
		}

		/// <summary>
		/// Request for adding a friend.
		/// </summary>
		/// <example>
		/// 001 [................] String : Sway
		/// 002 [................] String : Aura
		/// </example>
		[PacketHandler(Op.Msgr.FriendInvite)]
		public void FriendInvite(MsgrClient client, Packet packet)
		{
			var characterName = packet.GetString();
			var serverName = packet.GetString();

			// Get user
			var friend = MsgrServer.Instance.Database.GetFriendFromUser(characterName, serverName);
			if (friend == null)
			{
				Send.FriendInviteR(client, FriendInviteResult.UserNotFound);
				return;
			}

			// Check account
			if (friend.AccountId == client.User.AccountId)
			{
				Send.FriendInviteR(client, FriendInviteResult.OwnAccount);
				return;
			}

			// Check existing friends
			if (client.User.Friends.Exists(a => a.Id == friend.Id))
			{
				Send.FriendInviteR(client, FriendInviteResult.AlreadyFriends);
				return;
			}

			// Check max friends
			var max = MsgrServer.Instance.Conf.Msgr.MaxFriends;
			if (max != 0 && client.User.Friends.Count >= max)
			{
				Send.FriendInviteR(client, FriendInviteResult.MaxReached);
				return;
			}

			// Check friend's max friends
			var count = MsgrServer.Instance.Database.CountFriends(friend.Id);
			if (max != 0 && count >= max)
			{
				Send.FriendInviteR(client, FriendInviteResult.UserMaxReached);
				return;
			}

			friend.FriendshipStatus = FriendshipStatus.Inviting;

			// Add
			client.User.Friends.Add(friend);
			MsgrServer.Instance.Database.InviteFriend(client.User.Id, friend.Id);

			Send.FriendInviteR(client, FriendInviteResult.Success, friend);

			// Notify friend
			var friendUser = MsgrServer.Instance.UserManager.Get(friend.Id);
			if (friendUser != null)
			{
				// Get user as friend object to add to friend's friends,
				// otherwise the relation check in the live answer will fail.
				// TODO: I hate this, the current Friend concept sucks,
				//   they're not even *really* needed... remove them.
				var userAsFriend = MsgrServer.Instance.Database.GetFriendFromUser(client.User.Name, client.User.Server);
				if (userAsFriend == null)
				{
					Log.Error("FriendInvite: Failed to query user as friend for new friend's friends. Friends friends friends.");
				}
				else
				{
					friendUser.Friends.Add(userAsFriend);
					Send.FriendConfirm(friendUser, client.User);
				}
			}
		}

		/// <summary>
		/// Notification about friend deletion, no response.
		/// </summary>
		/// <example>
		/// 001 [........00000005] Int    : 5
		/// </example>
		[PacketHandler(Op.Msgr.DeleteFriend)]
		public void DeleteFriend(MsgrClient client, Packet packet)
		{
			var contactId = packet.GetInt();

			// Check friend
			var friend = client.User.GetFriend(contactId);
			if (friend == null)
			{
				Log.Warning("DeleteFriend: User '{0}' tried to delete non-existent friend.", client.User.AccountId);
				client.Kill(); // Out of sync, close connection.
				return;
			}

			// TODO: Live update for friend.

			client.User.Friends.Remove(friend);
			MsgrServer.Instance.Database.DeleteFriend(client.User.Id, contactId);
		}

		/// <summary>
		/// Notification about friend accept, no response.
		/// </summary>
		/// <example>
		/// 001 [........00000002] Int    : 2
		/// 002 [..............01] Byte   : 1
		/// </example>
		[PacketHandler(Op.Msgr.FriendReply)]
		public void FriendReply(MsgrClient client, Packet packet)
		{
			var contactId = packet.GetInt();
			var accepted = packet.GetBool();

			// Check friend
			var friend = client.User.GetFriend(contactId);
			if (friend == null)
			{
				Log.Warning("FriendReply: User '{0}' tried to accept non-existent invitation.", client.User.AccountId);
				client.Kill(); // Out of sync, close connection.
				return;
			}

			// Check relation
			if (friend.FriendshipStatus != FriendshipStatus.Invited)
			{
				// Don't log, could be sent twice due to lag.
				return;
			}

			// Accept or remove friend
			if (accepted)
			{
				friend.FriendshipStatus = FriendshipStatus.Normal;
				MsgrServer.Instance.Database.AcceptFriend(client.User.Id, contactId);

				// Notify user and friend if friend is online
				var friendUser = MsgrServer.Instance.UserManager.Get(contactId);
				if (friendUser != null)
				{
					Send.FriendOnline(client.User, friendUser);
					Send.FriendOnline(friendUser, client.User);
				}
			}
			else
			{
				client.User.Friends.Remove(friend);
				MsgrServer.Instance.Database.DeleteFriend(client.User.Id, contactId);

				// TODO: Live update for friend?
			}
		}

		/// <summary>
		/// Sent to open chat window.
		/// </summary>
		/// <example>
		/// 001 [........00000002] Int    : 2
		/// 002 [..............00] Byte   : 0
		/// </example>
		[PacketHandler(Op.Msgr.ChatBegin)]
		public void ChatBegin(MsgrClient client, Packet packet)
		{
			var contactId = packet.GetInt();
			var unkByte = packet.GetByte();

			// Check friend and relation
			var friend = client.User.GetFriend(contactId);
			if (friend == null || friend.FriendshipStatus != FriendshipStatus.Normal)
			{
				Log.Warning("ChatBegin: User '{0}' tried to start chat without being friends.", client.User.AccountId);
				return;
			}

			// Check if online
			var user = MsgrServer.Instance.UserManager.Get(contactId);
			if (user == null)
			{
				Log.Warning("ChatBegin: User '{0}' tried to start chat with offline friend.", client.User.AccountId);
				return;
			}

			// Create session
			var session = new ChatSession();
			session.Join(client.User);
			session.PreJoin(user);

			Send.ChatBeginR(client.User, session.Id, contactId);
		}

		/// <summary>
		/// Sent upon sending a message in a chat.
		/// </summary>
		/// <example>
		/// 001 [0000000000000001] Long   : 1
		/// 002 [................] String : test
		/// </example>
		[PacketHandler(Op.Msgr.Chat)]
		public void Chat(MsgrClient client, Packet packet)
		{
			var sessionId = packet.GetLong();
			var message = packet.GetString();

			// Check session
			var session = MsgrServer.Instance.ChatSessionManager.Get(sessionId);
			if (session == null || !session.HasUser(client.User.Id))
			{
				Log.Warning("Chat: User '{0}' tried to chat in invalid session.", client.User.AccountId);
				return;
			}

			// Notify waiting users
			var waiting = session.GetWaitingUsers();
			foreach (var user in waiting)
				session.Join(user);

			Send.ChatR(session, client.User.Id, message);
		}

		/// <summary>
		/// Sent when closing chat window.
		/// </summary>
		/// <example>
		/// 001 [0000000000000001] Long   : 1
		/// </example>
		[PacketHandler(Op.Msgr.ChatEnd)]
		public void ChatEnd(MsgrClient client, Packet packet)
		{
			var sessionId = packet.GetLong();

			// Check session
			var session = MsgrServer.Instance.ChatSessionManager.Get(sessionId);
			if (session == null || !session.HasUser(client.User.Id))
			{
				Log.Warning("ChatEnd: User '{0}' tried to end invalid chat session.", client.User.AccountId);
				return;
			}

			Send.ChatLeave(session, client.User);
		}
	}
}
