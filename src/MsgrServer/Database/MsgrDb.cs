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
		/// Returns a contact for the given values, either from the db,
		/// or by creating a new one.
		/// </summary>
		/// <param name="accountId"></param>
		/// <param name="characterEntityId"></param>
		/// <param name="characterName"></param>
		/// <param name="server"></param>
		/// <param name="channelName"></param>
		/// <returns></returns>
		public Contact GetOrCreateContact(string accountId, long characterEntityId, string characterName, string server, string channelName)
		{
			using (var conn = this.Connection)
			{
				var contact = new Contact();
				contact.AccountId = accountId;
				contact.Name = characterName;
				contact.Server = server;
				contact.ChannelName = channelName;

				// Try to get contact from db
				using (var mc = new MySqlCommand("SELECT * FROM `contacts` WHERE `characterEntityId` = @characterEntityId", conn))
				{
					mc.Parameters.AddWithValue("@characterEntityId", characterEntityId);

					using (var reader = mc.ExecuteReader())
					{
						if (reader.Read())
						{
							contact.Id = reader.GetInt32("contactId");
							contact.Status = (ContactStatus)reader.GetByte("status");
							contact.ChatOptions = (ChatOptions)reader.GetUInt32("chatOptions");
							contact.Nickname = reader.GetStringSafe("nickname") ?? "";

							if (!Enum.IsDefined(typeof(ContactStatus), contact.Status) || contact.Status == ContactStatus.None)
								contact.Status = ContactStatus.Online;

							return contact;
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

					cmd.Execute();

					contact.Id = (int)cmd.LastId;

					return contact;
				}
			}
		}

		/// <summary>
		/// Returns all notes for contact.
		/// </summary>
		/// <param name="contact"></param>
		/// <returns></returns>
		public List<Note> GetNotes(Contact contact)
		{
			var result = new List<Note>();

			using (var conn = this.Connection)
			using (var mc = new MySqlCommand("SELECT * FROM `notes` WHERE `receiver` = @receiver", conn))
			{
				mc.Parameters.AddWithValue("@receiver", contact.FullName);

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
		/// <param name="contact"></param>
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
		/// Saves contact's options to database.
		/// </summary>
		/// <param name="contact"></param>
		public void SaveOptions(Contact contact)
		{
			using (var conn = this.Connection)
			using (var cmd = new UpdateCommand("UPDATE `contacts` SET {0} WHERE `contactId` = @contactId", conn))
			{
				cmd.Set("status", (byte)contact.Status);
				cmd.Set("chatOptions", (uint)contact.ChatOptions);
				cmd.Set("nickname", contact.Nickname ?? "");
				cmd.AddParameter("@contactId", contact.Id);

				cmd.Execute();
			}
		}

		/// <summary>
		/// Returns list of all groups in contact's friend list.
		/// </summary>
		/// <param name="contact"></param>
		/// <returns></returns>
		public List<Group> GetGroups(Contact contact)
		{
			var result = new List<Group>();

			using (var conn = this.Connection)
			using (var mc = new MySqlCommand("SELECT * FROM `groups` WHERE `contactId` = @contactId", conn))
			{
				mc.Parameters.AddWithValue("@contactId", contact.Id);

				using (var reader = mc.ExecuteReader())
				{
					while (reader.Read())
					{
						var group = new Group();
						group.Id = reader.GetInt32("groupId");
						group.Name = reader.GetStringSafe("name");

						result.Add(group);
					}
				}
			}

			return result;
		}
	}
}
