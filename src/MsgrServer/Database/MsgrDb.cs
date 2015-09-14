// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

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
	}
}
