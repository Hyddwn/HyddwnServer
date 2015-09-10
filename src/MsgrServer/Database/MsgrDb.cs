// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Shared.Database;
using MySql.Data.MySqlClient;

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
	}
}
