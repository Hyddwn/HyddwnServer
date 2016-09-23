// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Mabi.Const;
using Aura.Shared.Database;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;

namespace Aura.Msgr.Database
{
	public class MsgrDb : AuraDb
	{
		/// <summary>
		/// Returns a user for the given values, either from the db,
		/// or by creating a new one.
		/// </summary>
		/// <param name="accountId"></param>
		/// <param name="characterEntityId"></param>
		/// <param name="characterName"></param>
		/// <param name="server"></param>
		/// <param name="channelName"></param>
		/// <returns></returns>
		public User GetOrCreateContact(string accountId, long characterEntityId, string characterName, string server, string channelName)
		{
			using (var conn = this.Connection)
			{
				var user = new User();
				user.AccountId = accountId;
				user.CharacterId = characterEntityId;
				user.Name = characterName;
				user.Server = server;
				user.ChannelName = channelName;
				user.Status = ContactStatus.Online;
				user.ChatOptions = ChatOptions.NotifyOnFriendLogIn;
				user.LastLogin = DateTime.Now;

				// Try to get contact from db
				using (var mc = new MySqlCommand("SELECT * FROM `contacts` WHERE `characterEntityId` = @characterEntityId", conn))
				{
					mc.Parameters.AddWithValue("@characterEntityId", characterEntityId);

					using (var reader = mc.ExecuteReader())
					{
						if (reader.Read())
						{
							user.Id = reader.GetInt32("contactId");
							user.Status = (ContactStatus)reader.GetByte("status");
							user.ChatOptions = (ChatOptions)reader.GetUInt32("chatOptions");
							user.Nickname = reader.GetStringSafe("nickname") ?? "";
							user.LastLogin = reader.GetDateTimeSafe("lastLogin");

							if (!Enum.IsDefined(typeof(ContactStatus), user.Status) || user.Status == ContactStatus.None)
								user.Status = ContactStatus.Online;

							this.UpdateLastLogin(user);

							return user;
						}
					}
				}

				// Create new contact
				using (var cmd = new InsertCommand("INSERT INTO `contacts` {0}", conn))
				{
					cmd.Set("accountId", accountId);
					cmd.Set("characterEntityId", characterEntityId);
					cmd.Set("characterName", characterName);
					cmd.Set("server", server);
					cmd.Set("status", (byte)user.Status);
					cmd.Set("chatOptions", (uint)user.ChatOptions);
					cmd.Set("nickname", "");
					cmd.Set("lastLogin", user.LastLogin);

					cmd.Execute();

					user.Id = (int)cmd.LastId;

					return user;
				}
			}
		}

		/// <summary>
		/// Updates contact's last login time.
		/// </summary>
		/// <param name="contact"></param>
		private void UpdateLastLogin(Contact contact)
		{
			using (var conn = this.Connection)
			using (var cmd = new UpdateCommand("UPDATE `contacts` SET {0} WHERE `contactId` = @contactId", conn))
			{
				cmd.AddParameter("@contactId", contact.Id);
				cmd.Set("lastLogin", contact.LastLogin);

				cmd.Execute();
			}
		}

		/// <summary>
		/// Returns contact with the given character id from the database
		/// or null if it wasn't found.
		/// </summary>
		/// <param name="characterEntityId"></param>
		/// <returns></returns>
		public User GetUserByCharacterId(long characterEntityId)
		{
			using (var conn = this.Connection)
			using (var mc = new MySqlCommand("SELECT * FROM `contacts` WHERE `characterEntityId` = @characterEntityId", conn))
			{
				mc.Parameters.AddWithValue("@characterEntityId", characterEntityId);

				using (var reader = mc.ExecuteReader())
				{
					if (!reader.Read())
						return null;

					var user = new User();
					user.Id = reader.GetInt32("contactId");
					user.AccountId = reader.GetString("accountId");
					user.CharacterId = characterEntityId;
					user.Name = reader.GetString("characterName");
					user.Server = reader.GetString("server");
					user.Status = (ContactStatus)reader.GetByte("status");
					user.ChatOptions = (ChatOptions)reader.GetUInt32("chatOptions");
					user.Nickname = reader.GetStringSafe("nickname") ?? "";
					user.LastLogin = reader.GetDateTimeSafe("lastLogin");

					if (!Enum.IsDefined(typeof(ContactStatus), user.Status) || user.Status == ContactStatus.None)
						user.Status = ContactStatus.Online;

					return user;
				}
			}
		}

		/// <summary>
		/// Returns friend for invitation, or null if the user doesn't exist.
		/// </summary>
		/// <param name="characterName"></param>
		/// <param name="server"></param>
		/// <returns></returns>
		public Friend GetFriendFromUser(string characterName, string server)
		{
			using (var conn = this.Connection)
			using (var mc = new MySqlCommand("SELECT * FROM `contacts` WHERE `characterName` = @characterName AND `server` = @server", conn))
			{
				mc.Parameters.AddWithValue("@characterName", characterName);
				mc.Parameters.AddWithValue("@server", server);

				using (var reader = mc.ExecuteReader())
				{
					if (!reader.Read())
						return null;

					var friend = new Friend();
					friend.AccountId = reader.GetString("accountId");
					friend.Name = characterName;
					friend.Server = server;
					friend.Id = reader.GetInt32("contactId");
					friend.Status = (ContactStatus)reader.GetByte("status");

					return friend;
				}
			}
		}

		/// <summary>
		/// Returns all notes for user.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		public List<Note> GetNotes(User user)
		{
			var result = new List<Note>();

			using (var conn = this.Connection)
			using (var mc = new MySqlCommand("SELECT * FROM `notes` WHERE `receiver` = @receiver", conn))
			{
				mc.Parameters.AddWithValue("@receiver", user.FullName);

				using (var reader = mc.ExecuteReader())
				{
					while (reader.Read())
					{
						var note = this.ReadNote(reader);
						if (note == null)
							continue;

						result.Add(note);
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Returns note with given id, or null on error.
		/// </summary>
		/// <param name="noteId"></param>
		/// <returns></returns>
		public Note GetNote(long noteId)
		{
			Note note = null;

			using (var conn = this.Connection)
			using (var mc = new MySqlCommand("SELECT * FROM `notes` WHERE `noteId` = @noteId", conn))
			{
				mc.Parameters.AddWithValue("@noteId", noteId);

				using (var reader = mc.ExecuteReader())
				{
					if (reader.Read())
						note = this.ReadNote(reader);
				}
			}

			return note;
		}

		/// <summary>
		/// Reads note from reader, returns null on error.
		/// </summary>
		/// <param name="reader"></param>
		/// <returns></returns>
		private Note ReadNote(MySqlDataReader reader)
		{
			var note = new Note();

			note.Id = reader.GetInt64("noteId");
			note.Sender = reader.GetStringSafe("sender");
			note.Receiver = reader.GetStringSafe("receiver");
			note.Message = reader.GetStringSafe("message");
			note.Time = reader.GetDateTimeSafe("time");
			note.Read = reader.GetBoolean("read");

			var split = note.Sender.Split('@');
			if (split.Length != 2)
				return null;

			note.FromCharacterName = split[0];
			note.FromServer = split[1];

			return note;
		}

		/// <summary>
		/// Sets read flag for given note.
		/// </summary>
		/// <param name="noteId"></param>
		public void SetNoteRead(long noteId)
		{
			using (var conn = this.Connection)
			using (var cmd = new UpdateCommand("UPDATE `notes` SET {0} WHERE `noteId` = @noteId", conn))
			{
				cmd.Set("read", true);
				cmd.AddParameter("@noteId", noteId);

				cmd.Execute();
			}
		}

		/// <summary>
		/// Adds note to database.
		/// </summary>
		/// <param name="noteId"></param>
		public void AddNote(string sender, string receiver, string message)
		{
			using (var conn = this.Connection)
			using (var cmd = new InsertCommand("INSERT INTO `notes` {0}", conn))
			{
				cmd.Set("sender", sender);
				cmd.Set("receiver", receiver);
				cmd.Set("message", message);
				cmd.Set("time", DateTime.Now);

				cmd.Execute();
			}
		}

		/// <summary>
		/// Deletes note from database.
		/// </summary>
		/// <param name="receiver"></param>
		/// <param name="noteId"></param>
		public void DeleteNote(string receiver, long noteId)
		{
			using (var conn = this.Connection)
			using (var mc = new MySqlCommand("DELETE FROM `notes` WHERE `receiver` = @receiver AND `noteId` = @noteId", conn))
			{
				mc.Parameters.AddWithValue("@receiver", receiver);
				mc.Parameters.AddWithValue("@noteId", noteId);

				mc.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// Returns first note with an id higher than the given one,
		/// or null if none exist.
		/// </summary>
		/// <param name="receiver"></param>
		/// <param name="noteId"></param>
		/// <returns></returns>
		public Note GetLatestUnreadNote(string receiver, long noteId)
		{
			Note note = null;

			using (var conn = this.Connection)
			using (var mc = new MySqlCommand("SELECT * FROM `notes` WHERE `receiver` = @receiver AND `noteId` > @noteId AND `read` = 0 ORDER BY `noteId` DESC LIMIT 1", conn))
			{
				mc.Parameters.AddWithValue("@receiver", receiver);
				mc.Parameters.AddWithValue("@noteId", noteId);

				using (var reader = mc.ExecuteReader())
				{
					if (reader.Read())
						note = this.ReadNote(reader);
				}
			}

			return note;
		}

		/// <summary>
		/// Saves user's options to database.
		/// </summary>
		/// <param name="user"></param>
		public void SaveOptions(User user)
		{
			using (var conn = this.Connection)
			using (var cmd = new UpdateCommand("UPDATE `contacts` SET {0} WHERE `contactId` = @contactId", conn))
			{
				cmd.Set("status", (byte)user.Status);
				cmd.Set("chatOptions", (uint)user.ChatOptions);
				cmd.Set("nickname", user.Nickname ?? "");
				cmd.AddParameter("@contactId", user.Id);

				cmd.Execute();
			}
		}

		/// <summary>
		/// Returns list of all groups in user's friend list.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		public ICollection<Group> GetGroups(User user)
		{
			var result = new Dictionary<int, Group>();

			using (var conn = this.Connection)
			using (var mc = new MySqlCommand("SELECT * FROM `groups` WHERE `contactId` = @contactId", conn))
			{
				mc.Parameters.AddWithValue("@contactId", user.Id);

				using (var reader = mc.ExecuteReader())
				{
					while (reader.Read())
					{
						var group = new Group();
						group.Id = reader.GetInt32("groupId");
						group.Name = reader.GetStringSafe("name");

						// Override duplicate ids
						result[group.Id] = group;
					}
				}
			}

			return result.Values;
		}

		/// <summary>
		/// Adds group to database.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="groupId"></param>
		/// <param name="groupName"></param>
		public void AddGroup(User user, Group group)
		{
			using (var conn = this.Connection)
			using (var cmd = new InsertCommand("INSERT INTO `groups` {0}", conn))
			{
				cmd.Set("groupId", group.Id);
				cmd.Set("contactId", user.Id);
				cmd.Set("name", group.Name);

				cmd.Execute();
			}
		}

		/// <summary>
		/// Renames group in database.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="groupId"></param>
		/// <param name="groupName"></param>
		public void RenameGroup(User user, int groupId, string groupName)
		{
			using (var conn = this.Connection)
			using (var cmd = new UpdateCommand("UPDATE `groups` SET {0} WHERE `groupId` = @groupId AND `contactId` = @contactId", conn))
			{
				cmd.Set("name", groupName);
				cmd.AddParameter("@groupId", groupId);
				cmd.AddParameter("@contactId", user.Id);

				cmd.Execute();
			}
		}

		/// <summary>
		/// Changes group the friend is in in the database.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="friendContactId"></param>
		/// <param name="groupId"></param>
		public void ChangeGroup(User user, int friendContactId, int groupId)
		{
			using (var conn = this.Connection)
			using (var cmd = new UpdateCommand("UPDATE `friends` SET {0} WHERE `userId1` = @userId1 AND `userId2` = @userId2", conn))
			{
				cmd.Set("groupId", groupId);
				cmd.AddParameter("@userId1", user.Id);
				cmd.AddParameter("@userId2", friendContactId);

				cmd.Execute();
			}
		}

		/// <summary>
		/// Deletes group from database and moves friends in that group to ETC.
		/// </summary>
		/// <param name="user"></param>
		/// <param name="groupId"></param>
		public void DeleteGroup(User user, int groupId)
		{
			using (var conn = this.Connection)
			{
				// Move friends
				using (var cmd = new UpdateCommand("UPDATE `friends` SET {0} WHERE `userId1` = @userId1 AND `groupId` = @oldGroupId", conn))
				{
					cmd.Set("groupId", -1);
					cmd.AddParameter("@userId1", user.Id);
					cmd.AddParameter("@oldGroupId", groupId);

					cmd.Execute();
				}

				// Delete group
				using (var mc = new MySqlCommand("DELETE FROM `groups` WHERE `contactId` = @contactId AND `groupId` = @groupId", conn))
				{
					mc.Parameters.AddWithValue("@contactId", user.Id);
					mc.Parameters.AddWithValue("@groupId", groupId);
					mc.ExecuteNonQuery();
				}
			}
		}

		/// <summary>
		/// Returns list of friends for user.
		/// </summary>
		/// <param name="user"></param>
		/// <returns></returns>
		public List<Friend> GetFriends(User user)
		{
			var result = new List<Friend>();

			using (var conn = this.Connection)
			using (var mc = new MySqlCommand(
				"SELECT f.userId2 AS friendId, c.characterName AS friendName, c.server AS friendServer, f.groupId AS groupId, f.status AS status " +
				"FROM `friends` AS f " +
				"INNER JOIN `contacts` AS c ON `f`.`userId2` = `c`.`contactId` " +
				"WHERE `f`.`userId1` = @userId", conn))
			{
				mc.Parameters.AddWithValue("@userId", user.Id);

				using (var reader = mc.ExecuteReader())
				{
					while (reader.Read())
					{
						var friend = new Friend();
						friend.Id = reader.GetInt32("friendId");
						friend.Name = reader.GetStringSafe("friendName");
						friend.Server = reader.GetStringSafe("friendServer");
						friend.GroupId = reader.GetInt32("groupId");
						friend.FriendshipStatus = (FriendshipStatus)reader.GetByte("status");

						result.Add(friend);
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Creates friend entries for user and friend with status inviting/invited.
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="friendId"></param>
		public void InviteFriend(int userId, int friendId)
		{
			using (var conn = this.Connection)
			using (var transaction = conn.BeginTransaction())
			{
				using (var cmd = new InsertCommand("INSERT INTO `friends` {0}", conn, transaction))
				{
					cmd.Set("userId1", userId);
					cmd.Set("userId2", friendId);
					cmd.Set("groupId", -1);
					cmd.Set("status", (byte)FriendshipStatus.Inviting);

					cmd.Execute();
				}

				using (var cmd = new InsertCommand("INSERT INTO `friends` {0}", conn, transaction))
				{
					cmd.Set("userId1", friendId);
					cmd.Set("userId2", userId);
					cmd.Set("groupId", -1);
					cmd.Set("status", (byte)FriendshipStatus.Invited);

					cmd.Execute();
				}

				transaction.Commit();
			}
		}

		/// <summary>
		/// Creates friend entry for contact on user with the blacklist status.
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="contactId"></param>
		public void Blacklist(int userId, int contactId)
		{
			using (var conn = this.Connection)
			using (var cmd = new InsertCommand("INSERT INTO `friends` {0}", conn))
			{
				cmd.Set("userId1", userId);
				cmd.Set("userId2", contactId);
				cmd.Set("groupId", -4);
				cmd.Set("status", (byte)FriendshipStatus.Blacklist);

				cmd.Execute();
			}
		}

		/// <summary>
		/// Returns true if user blacklisted contact.
		/// </summary>
		/// <param name="userId"></param>
		/// <param name="contactId"></param>
		public bool IsBlacklisted(int userId, int contactId)
		{
			using (var conn = this.Connection)
			using (var mc = new MySqlCommand("SELECT `friendId` FROM `friends` WHERE `userId1` = @userId1 AND `userId2` = @userId2 AND `status` = 7", conn))
			{
				mc.Parameters.AddWithValue("@userId1", userId);
				mc.Parameters.AddWithValue("@userId2", contactId);

				using (var reader = mc.ExecuteReader())
					return reader.HasRows;
			}
		}

		/// <summary>
		/// Returns the amount of friends the given contact has.
		/// </summary>
		/// <param name="contactId"></param>
		/// <returns></returns>
		public long CountFriends(int contactId)
		{
			using (var conn = this.Connection)
			using (var mc = new MySqlCommand("SELECT COUNT(`friendId`) FROM `friends` WHERE `userId1` = @userId", conn))
			{
				mc.Parameters.AddWithValue("@userId", contactId);
				return (long)mc.ExecuteScalar();
			}
		}

		/// <summary>
		/// Deletes friend entries between the two users.
		/// </summary>
		/// <param name="contactId"></param>
		/// <returns></returns>
		public void DeleteFriend(int contactId1, int contactId2)
		{
			using (var conn = this.Connection)
			using (var mc = new MySqlCommand("DELETE FROM `friends` WHERE (`userId1` = @userId1 AND `userId2` = @userId2) OR (`userId2` = @userId1 AND `userId1` = @userId2)", conn))
			{
				mc.Parameters.AddWithValue("@userId1", contactId1);
				mc.Parameters.AddWithValue("@userId2", contactId2);
				mc.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// Updates friendship status between the two users.
		/// </summary>
		/// <param name="contactId"></param>
		/// <returns></returns>
		public void SetFriendshipStatus(int contactId1, int contactId2, FriendshipStatus status)
		{
			using (var conn = this.Connection)
			using (var cmd = new UpdateCommand("UPDATE `friends` SET {0} WHERE (`userId1` = @userId1 AND `userId2` = @userId2) OR (`userId2` = @userId1 AND `userId1` = @userId2)", conn))
			{
				cmd.Set("status", (byte)status);
				cmd.AddParameter("@userId1", contactId1);
				cmd.AddParameter("@userId2", contactId2);
				cmd.Execute();
			}
		}

		/// <summary>
		/// Updates friendship status for contact 1.
		/// </summary>
		/// <param name="contactId"></param>
		/// <returns></returns>
		public void SetFriendshipStatusOneSided(int contactId1, int contactId2, FriendshipStatus status)
		{
			using (var conn = this.Connection)
			using (var cmd = new UpdateCommand("UPDATE `friends` SET {0} WHERE (`userId1` = @userId1 AND `userId2` = @userId2)", conn))
			{
				cmd.Set("status", (byte)status);
				cmd.AddParameter("@userId1", contactId1);
				cmd.AddParameter("@userId2", contactId2);
				cmd.Execute();
			}
		}
	}
}
