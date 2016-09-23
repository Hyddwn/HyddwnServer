// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Mabi.Const;
using Aura.Shared.Util;
using MySql.Data.MySqlClient;
using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Aura.Shared.Database
{
	public class AuraDb
	{
		private string _connectionString;

		private Regex _nameCheckRegex = new Regex(@"^[a-zA-Z][a-z0-9]{2,15}$", RegexOptions.Compiled);

		/// <summary>
		/// Returns a valid connection.
		/// </summary>
		public MySqlConnection Connection
		{
			get
			{
				if (_connectionString == null)
					throw new Exception("AuraDb has not been initialized.");

				var result = new MySqlConnection(_connectionString);
				result.Open();
				return result;
			}
		}

		/// <summary>
		/// Sets connection string and calls TestConnection.
		/// </summary>
		/// <param name="host"></param>
		/// <param name="port"></param>
		/// <param name="user"></param>
		/// <param name="pass"></param>
		/// <param name="db"></param>
		public void Init(string host, int port, string user, string pass, string db)
		{
			_connectionString = string.Format("server={0}; port={1}; database={2}; uid={3}; password={4}; pooling=true; min pool size=0; max pool size=100; ConvertZeroDateTime=true", host, port, db, user, pass);
			this.TestConnection();
		}

		/// <summary>
		/// Tests connection, throws on error.
		/// </summary>
		public void TestConnection()
		{
			MySqlConnection conn = null;
			try
			{
				conn = this.Connection;
			}
			finally
			{
				if (conn != null)
					conn.Close();
			}
		}

		// ------------------------------------------------------------------

		/// <summary>
		/// Returns whether the account exists.
		/// </summary>
		/// <param name="accountId"></param>
		/// <returns></returns>
		public bool AccountExists(string accountId)
		{
			using (var conn = this.Connection)
			{
				var mc = new MySqlCommand("SELECT `accountId` FROM `accounts` WHERE `accountId` = @accountId", conn);
				mc.Parameters.AddWithValue("@accountId", accountId);

				using (var reader = mc.ExecuteReader())
					return reader.HasRows;
			}
		}

		/// <summary>
		/// Returns whether the account is marked as being logged in.
		/// </summary>
		/// <param name="accountId"></param>
		/// <returns></returns>
		public bool AccountIsLoggedIn(string accountId)
		{
			using (var conn = this.Connection)
			{
				var mc = new MySqlCommand("SELECT `accountId` FROM `accounts` WHERE `accountId` = @accountId AND `loggedIn` = true", conn);
				mc.Parameters.AddWithValue("@accountId", accountId);

				using (var reader = mc.ExecuteReader())
					return reader.HasRows;
			}
		}

		/// <summary>
		/// Set's account's loggedIn field.
		/// </summary>
		/// <param name="accountId"></param>
		/// <returns></returns>
		public void SetAccountLoggedIn(string accountId, bool loggedIn)
		{
			using (var conn = this.Connection)
			using (var mc = new MySqlCommand("UPDATE `accounts` SET `loggedIn` = @loggedIn WHERE `accountId` = @accountId", conn))
			{
				mc.Parameters.AddWithValue("@accountId", accountId);
				mc.Parameters.AddWithValue("@loggedIn", loggedIn);

				mc.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// Adds new account to the database.
		/// </summary>
		/// <param name="accountId"></param>
		/// <param name="password"></param>
		/// <param name="points">Starter cash points.</param>
		public void CreateAccount(string accountId, string password, int points)
		{
			password = Password.Hash(password);

			using (var conn = this.Connection)
			using (var cmd = new InsertCommand("INSERT INTO `accounts` {0}", conn))
			{
				cmd.Set("accountId", accountId);
				cmd.Set("password", password);
				cmd.Set("creation", DateTime.Now);
				cmd.Set("points", points);

				cmd.Execute();
			}
		}

		/// <summary>
		/// Adds card to database and returns it as Card.
		/// </summary>
		/// <param name="accountId"></param>
		/// <param name="type"></param>
		/// <param name="race"></param>
		/// <returns></returns>
		public Card AddCard(string accountId, int type, int race)
		{
			using (var conn = this.Connection)
			{
				var mc = new MySqlCommand("INSERT INTO `cards` (`accountId`, `type`, `race`) VALUES (@accountId, @type, @race)", conn);
				mc.Parameters.AddWithValue("@accountId", accountId);
				mc.Parameters.AddWithValue("@type", type);
				mc.Parameters.AddWithValue("@race", race);

				mc.ExecuteNonQuery();

				return new Card(mc.LastInsertedId, type, race);
			}
		}

		/// <summary>
		/// Returns true if the name is valid and available.
		/// </summary>
		/// <param name="name"></param>
		/// <param name="serverName"></param>
		/// <returns></returns>
		public NameCheckResult NameOkay(string name, string serverName)
		{
			if (!_nameCheckRegex.IsMatch(name))
				return NameCheckResult.Invalid;

			using (var conn = this.Connection)
			{
				var mc = new MySqlCommand("SELECT `creatureId` FROM `creatures` WHERE `name` = @name AND `server` = @serverName", conn);
				mc.Parameters.AddWithValue("@name", name);
				mc.Parameters.AddWithValue("@serverName", serverName);

				using (var reader = mc.ExecuteReader())
				{
					if (reader.HasRows)
						return NameCheckResult.Exists;
				}
			}

			return NameCheckResult.Okay;
		}

		/// <summary>
		/// Resets password for account to its name.
		/// </summary>
		/// <param name="accountName"></param>
		/// <param name="password"></param>
		public void SetAccountPassword(string accountName, string password)
		{
			using (var conn = this.Connection)
			using (var mc = new MySqlCommand("UPDATE `accounts` SET `password` = @password WHERE `accountId` = @accountId", conn))
			{
				mc.Parameters.AddWithValue("@accountId", accountName);
				mc.Parameters.AddWithValue("@password", Password.HashRaw(password));

				mc.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// Sets new randomized session key for the account and returns it.
		/// </summary>
		/// <param name="accountId"></param>
		/// <returns></returns>
		public long CreateSession(string accountId)
		{
			using (var conn = this.Connection)
			using (var mc = new MySqlCommand("UPDATE `accounts` SET `sessionKey` = @sessionKey WHERE `accountId` = @accountId", conn))
			{
				var sessionKey = RandomProvider.Get().NextInt64();

				mc.Parameters.AddWithValue("@accountId", accountId);
				mc.Parameters.AddWithValue("@sessionKey", sessionKey);

				mc.ExecuteNonQuery();

				return sessionKey;
			}
		}

		/// <summary>
		/// Returns true if sessionKey is correct for account.
		/// </summary>
		/// <param name="accountId"></param>
		/// <param name="sessionKey"></param>
		/// <returns></returns>
		public bool CheckSession(string accountId, long sessionKey)
		{
			using (var conn = this.Connection)
			using (var mc = new MySqlCommand("SELECT `sessionKey` FROM `accounts` WHERE `accountId` = @accountId AND `sessionKey` = @sessionKey", conn))
			{
				mc.Parameters.AddWithValue("@accountId", accountId);
				mc.Parameters.AddWithValue("@sessionKey", sessionKey);

				using (var reader = mc.ExecuteReader())
					return reader.HasRows;
			}
		}

		/// <summary>
		/// Returns true if account has a character with the given id on the
		/// given server.
		/// </summary>
		/// <param name="accountId"></param>
		/// <param name="entityId"></param>
		/// <param name="server"></param>
		/// <returns></returns>
		public bool AccountHasCharacter(string accountId, long entityId, string server)
		{
			using (var conn = this.Connection)
			using (var mc = new MySqlCommand(
				"SELECT `c`.`entityId` " +
				"FROM `characters` AS `c` " +
				"INNER JOIN `creatures` AS `cr` ON `c`.`creatureId` = `cr`.`creatureId` " +
				"WHERE `accountId` = @accountId AND `entityId` = @entityId AND `server` = @server"
			, conn))
			{
				mc.Parameters.AddWithValue("@accountId", accountId);
				mc.Parameters.AddWithValue("@entityId", entityId);
				mc.Parameters.AddWithValue("@server", server);

				using (var reader = mc.ExecuteReader())
					return reader.HasRows;
			}
		}

		/// <summary>
		/// Returns true if account has a pet with the given id on the
		/// given server.
		/// </summary>
		/// <param name="accountId"></param>
		/// <param name="entityId"></param>
		/// <param name="server"></param>
		/// <returns></returns>
		public bool AccountHasPet(string accountId, long entityId, string server)
		{
			using (var conn = this.Connection)
			using (var mc = new MySqlCommand(
				"SELECT `c`.`entityId` " +
				"FROM `pets` AS `c` " +
				"INNER JOIN `creatures` AS `cr` ON `c`.`creatureId` = `cr`.`creatureId` " +
				"WHERE `accountId` = @accountId AND `entityId` = @entityId AND `server` = @server"
			, conn))
			{
				mc.Parameters.AddWithValue("@accountId", accountId);
				mc.Parameters.AddWithValue("@entityId", entityId);
				mc.Parameters.AddWithValue("@server", server);

				using (var reader = mc.ExecuteReader())
					return reader.HasRows;
			}
		}

		/// <summary>
		/// Changes auth level of account.
		/// </summary>
		/// <param name="accountId"></param>
		/// <param name="level"></param>
		/// <returns></returns>
		public bool ChangeAuth(string accountId, int level)
		{
			using (var conn = this.Connection)
			using (var cmd = new UpdateCommand("UPDATE `accounts` SET {0} WHERE `accountId` = @accountId", conn))
			{
				cmd.AddParameter("@accountId", accountId);
				cmd.Set("authority", level);

				return (cmd.Execute() > 0);
			}
		}

		/// <summary>
		/// Unsets creature's Initialized creature state flag.
		/// </summary>
		/// <param name="creatureId"></param>
		public void UninitializeCreature(long creatureId)
		{
			using (var conn = this.Connection)
			using (var mc = new MySqlCommand("UPDATE `creatures` SET `state` = `state` & ~1 WHERE `creatureId` = @creatureId", conn))
			{
				mc.Parameters.AddWithValue("@creatureId", creatureId);
				mc.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// Returns all guilds in database.
		/// </summary>
		public Dictionary<long, Guild> GetGuilds()
		{
			var result = new Dictionary<long, Guild>();

			using (var conn = this.Connection)
			{
				// Read guilds
				using (var mc = new MySqlCommand("SELECT * FROM `guilds` WHERE `guildId` > @minId", conn))
				{
					mc.Parameters.AddWithValue("@minId", MabiId.Guilds);

					using (var reader = mc.ExecuteReader())
					{
						while (reader.Read())
						{
							var guild = this.ReadGuild(reader);
							result.Add(guild.Id, guild);
						}
					}
				}

				var memberLists = new Dictionary<long, List<GuildMember>>();

				// Read members
				using (var mc = new MySqlCommand(
					"SELECT `m`.*, `cr`.name " +
					"FROM `guild_members` AS `m` " +
					"INNER JOIN `characters` AS `c` ON `m`.`characterId` = `c`.`entityId` " +
					"INNER JOIN `creatures` AS `cr` ON `c`.`creatureId` = `cr`.`creatureId` "
				, conn))
				{
					using (var reader = mc.ExecuteReader())
					{
						while (reader.Read())
						{
							var member = this.ReadGuildMember(reader);
							result[member.GuildId].AddMember(member);
						}
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Returns whether the given guild name already exists.
		/// </summary>
		public bool GuildNameExists(string name)
		{
			using (var conn = this.Connection)
			using (var mc = new MySqlCommand("SELECT * FROM `guilds` WHERE `name` = @name", conn))
			{
				mc.Parameters.AddWithValue("@name", name);

				using (var reader = mc.ExecuteReader())
					return reader.HasRows;
			}
		}

		/// <summary>
		/// Returns guild with given id from database if it exists.
		/// </summary>
		public Guild GetGuild(long guildId)
		{
			using (var conn = this.Connection)
			using (var mc = new MySqlCommand("SELECT * FROM `guilds` WHERE `guildId` = @id", conn))
			{
				mc.Parameters.AddWithValue("@id", guildId);

				using (var reader = mc.ExecuteReader())
				{
					if (reader.Read())
					{
						var guild = this.ReadGuild(reader);
						var members = this.GetGuildMembers(guild.Id);

						guild.InitMembers(members);

						return guild;
					}
					else
					{
						return null;
					}
				}
			}
		}

		/// <summary>
		/// Returns all members in guild with given id.
		/// </summary>
		/// <param name="guildId"></param>
		/// <returns></returns>
		private List<GuildMember> GetGuildMembers(long guildId)
		{
			var result = new List<GuildMember>();

			using (var conn = this.Connection)
			using (var mc = new MySqlCommand(
				"SELECT `m`.*, `cr`.name " +
				"FROM `guild_members` AS `m` " +
				"INNER JOIN `characters` AS `c` ON `m`.`characterId` = `c`.`entityId` " +
				"INNER JOIN `creatures` AS `cr` ON `c`.`creatureId` = `cr`.`creatureId` " +
				"WHERE `m`.`guildId` = @id"
			, conn))
			{
				mc.Parameters.AddWithValue("@id", guildId);

				using (var reader = mc.ExecuteReader())
				{
					while (reader.Read())
					{
						var member = this.ReadGuildMember(reader);
						result.Add(member);
					}
				}
			}

			return result;
		}

		/// <summary>
		/// Reads GuildMember from reader and returns it.
		/// </summary>
		/// <param name="reader"></param>
		/// <returns></returns>
		private GuildMember ReadGuildMember(MySqlDataReader reader)
		{
			var member = new GuildMember();
			member.GuildId = reader.GetInt64("guildId");
			member.CharacterId = reader.GetInt64("characterId");
			member.Rank = (GuildMemberRank)reader.GetInt32("rank");
			member.JoinedDate = reader.GetDateTimeSafe("joinedDate");
			member.Application = reader.GetString("application");
			member.Messages = (GuildMessages)reader.GetInt32("messages");
			member.Name = reader.GetString("name");

			return member;
		}

		/// <summary>
		/// Returns all guilds in database.
		/// </summary>
		private Guild ReadGuild(MySqlDataReader reader)
		{
			var guild = new Guild();
			guild.Id = reader.GetInt64("guildId");
			guild.Name = reader.GetString("name");
			guild.LeaderName = reader.GetString("leaderName");
			guild.Title = reader.GetString("title");
			guild.EstablishedDate = reader.GetDateTimeSafe("establishedDate");
			guild.Server = reader.GetString("server");
			guild.IntroMessage = reader.GetString("introMessage");
			guild.WelcomeMessage = reader.GetString("welcomeMessage");
			guild.LeavingMessage = reader.GetString("leavingMessage");
			guild.RejectionMessage = reader.GetString("rejectionMessage");
			guild.Type = (GuildType)reader.GetInt32("type");
			guild.Visibility = (GuildVisibility)reader.GetInt32("visibility");
			guild.Level = (GuildLevel)reader.GetInt32("level");
			guild.Options = (GuildOptions)reader.GetInt32("options");
			guild.Stone.PropId = reader.GetInt32("stonePropId");
			guild.Stone.RegionId = reader.GetInt32("stoneRegionId");
			guild.Stone.X = reader.GetInt32("stoneX");
			guild.Stone.Y = reader.GetInt32("stoneY");
			guild.Stone.Direction = reader.GetFloat("stoneDirection");
			guild.Points = reader.GetInt32("points");
			guild.Gold = reader.GetInt32("gold");
			guild.Disbanded = reader.GetBoolean("disbanded");

			var hasRobe = reader.GetBoolean("hasRobe");
			if (hasRobe)
			{
				guild.Robe = new GuildRobe();
				guild.Robe.EmblemMark = reader.GetByte("emblemMark");
				guild.Robe.EmblemOutline = reader.GetByte("emblemOutline");
				guild.Robe.Stripes = reader.GetByte("stripes");
				guild.Robe.RobeColor = reader.GetUInt32("robeColor");
				guild.Robe.BadgeColor = reader.GetByte("badgeColor");
				guild.Robe.EmblemMarkColor = reader.GetByte("emblemMarkColor");
				guild.Robe.EmblemOutlineColor = reader.GetByte("emblemOutlineColor");
				guild.Robe.StripesColor = reader.GetByte("stripesColor");
			}

			return guild;
		}

		/// <summary>
		/// Writes guild robe information to database.
		/// </summary>
		/// <param name="guild"></param>
		public void UpdateGuildRobe(Guild guild)
		{
			using (var conn = this.Connection)
			using (var cmd = new UpdateCommand("UPDATE `guilds` SET {0} WHERE `guildId` = @guildId", conn))
			{
				cmd.AddParameter("@guildId", guild.Id);

				cmd.Set("hasRobe", guild.HasRobe);
				if (guild.HasRobe)
				{
					cmd.Set("emblemMark", guild.Robe.EmblemMark);
					cmd.Set("emblemOutline", guild.Robe.EmblemOutline);
					cmd.Set("stripes", guild.Robe.Stripes);
					cmd.Set("robeColor", guild.Robe.RobeColor);
					cmd.Set("badgeColor", guild.Robe.BadgeColor);
					cmd.Set("emblemMarkColor", guild.Robe.EmblemMarkColor);
					cmd.Set("emblemOutlineColor", guild.Robe.EmblemOutlineColor);
					cmd.Set("stripesColor", guild.Robe.StripesColor);
				}

				cmd.Execute();
			}
		}

		/// <summary>
		/// Writes guild's messages to database.
		/// </summary>
		/// <param name="guild"></param>
		public bool UpdateGuildMessages(Guild guild)
		{
			using (var conn = this.Connection)
			using (var cmd = new UpdateCommand("UPDATE `guilds` SET {0} WHERE `guildId` = @guildId", conn))
			{
				cmd.AddParameter("@guildId", guild.Id);
				cmd.Set("introMessage", guild.IntroMessage);
				cmd.Set("welcomeMessage", guild.WelcomeMessage);
				cmd.Set("leavingMessage", guild.LeavingMessage);
				cmd.Set("rejectionMessage", guild.RejectionMessage);

				return (cmd.Execute() > 0);
			}
		}

		/// <summary>
		/// Writes guild's leader's name and its members current ranks to database.
		/// </summary>
		/// <param name="guild"></param>
		public void UpdateGuildLeader(Guild guild)
		{
			using (var conn = this.Connection)
			using (var transaction = conn.BeginTransaction())
			{
				using (var cmd = new UpdateCommand("UPDATE `guilds` SET {0} WHERE `guildId` = @guildId", conn, transaction))
				{
					cmd.AddParameter("@guildId", guild.Id);
					cmd.Set("leaderName", guild.LeaderName);

					cmd.Execute();
				}

				foreach (var member in guild.GetMembers())
				{
					using (var cmd = new UpdateCommand("UPDATE `guild_members` SET {0} WHERE `characterId` = @characterId", conn, transaction))
					{
						cmd.AddParameter("@characterId", member.CharacterId);
						cmd.Set("rank", (int)member.Rank);

						cmd.Execute();
					}
				}

				transaction.Commit();
			}
		}

		/// <summary>
		/// Updates guild's disbanded field in database.
		/// </summary>
		/// <param name="guild"></param>
		public void UpdateDisbandGuild(Guild guild)
		{
			using (var conn = this.Connection)
			using (var cmd = new UpdateCommand("UPDATE `guilds` SET {0} WHERE `guildId` = @guildId", conn))
			{
				cmd.AddParameter("@guildId", guild.Id);
				cmd.Set("disbanded", guild.Disbanded);

				cmd.Execute();
			}
		}

		/// <summary>
		/// Removes guild and its members from database.
		/// </summary>
		/// <param name="guild"></param>
		public void RemoveGuild(Guild guild)
		{
			using (var conn = this.Connection)
			using (var cmd = new MySqlCommand("DELETE FROM `guilds` WHERE `guildId` = @guildId", conn))
			{
				cmd.Parameters.AddWithValue("@guildId", guild.Id);
				cmd.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// Removes guild and its members from database.
		/// </summary>
		/// <param name="member"></param>
		public void RemoveGuildMember(GuildMember member)
		{
			using (var conn = this.Connection)
			using (var cmd = new MySqlCommand("DELETE FROM `guild_members` WHERE `guildId` = @guildId && `characterId` = @characterId", conn))
			{
				cmd.Parameters.AddWithValue("@guildId", member.GuildId);
				cmd.Parameters.AddWithValue("@characterId", member.CharacterId);
				cmd.ExecuteNonQuery();
			}
		}

		/// <summary>
		/// Writes guild's points and gold to database.
		/// </summary>
		/// <param name="guild"></param>
		public void UpdateGuildResources(Guild guild)
		{
			using (var conn = this.Connection)
			using (var cmd = new UpdateCommand("UPDATE `guilds` SET {0} WHERE `guildId` = @guildId", conn))
			{
				cmd.AddParameter("@guildId", guild.Id);
				cmd.Set("points", guild.Points);
				cmd.Set("gold", guild.Gold);

				cmd.Execute();
			}
		}

		/// <summary>
		/// Writes guild's level, points, and gold to database.
		/// </summary>
		/// <param name="guild"></param>
		public void UpdateGuildLevelAndResources(Guild guild)
		{
			using (var conn = this.Connection)
			using (var cmd = new UpdateCommand("UPDATE `guilds` SET {0} WHERE `guildId` = @guildId", conn))
			{
				cmd.AddParameter("@guildId", guild.Id);
				cmd.Set("points", guild.Points);
				cmd.Set("gold", guild.Gold);
				cmd.Set("level", (int)guild.Level);

				cmd.Execute();
			}
		}

		/// <summary>
		/// Writes guild's points and gold to database.
		/// </summary>
		/// <param name="guild"></param>
		public void UpdateGuildStone(Guild guild)
		{
			using (var conn = this.Connection)
			using (var cmd = new UpdateCommand("UPDATE `guilds` SET {0} WHERE `guildId` = @guildId", conn))
			{
				cmd.AddParameter("@guildId", guild.Id);
				cmd.Set("stonePropId", guild.Stone.PropId);
				cmd.Set("stoneRegionId", guild.Stone.RegionId);
				cmd.Set("stoneX", guild.Stone.X);
				cmd.Set("stoneY", guild.Stone.Y);
				cmd.Set("stoneDirection", guild.Stone.Direction);

				cmd.Execute();
			}
		}

		/// <summary>
		/// Writes guild member to database.
		/// </summary>
		/// <param name="member"></param>
		public void AddGuildMember(GuildMember member)
		{
			using (var conn = this.Connection)
			using (var cmd = new InsertCommand("INSERT INTO `guild_members` {0}", conn))
			{
				cmd.Set("guildId", member.GuildId);
				cmd.Set("characterId", member.CharacterId);
				cmd.Set("rank", (int)member.Rank);
				cmd.Set("joinedDate", member.JoinedDate);
				cmd.Set("application", member.Application);
				cmd.Set("messages", (int)member.Messages);

				cmd.Execute();
			}
		}

		/// <summary>
		/// Updates guild member's rank in the database.
		/// </summary>
		/// <param name="member"></param>
		public void UpdateGuildMemberRank(GuildMember member)
		{
			using (var conn = this.Connection)
			using (var cmd = new UpdateCommand("UPDATE `guild_members` SET {0} WHERE `guildId` = @guildId AND `characterId` = @characterId", conn))
			{
				cmd.AddParameter("@guildId", member.GuildId);
				cmd.AddParameter("@characterId", member.CharacterId);
				cmd.Set("rank", (int)member.Rank);

				cmd.Execute();
			}
		}

		/// <summary>
		/// Adds guild to db and sets its id.
		/// </summary>
		/// <param name="guild"></param>
		public void AddGuild(Guild guild)
		{
			using (var conn = this.Connection)
			using (var cmd = new InsertCommand("INSERT INTO `guilds` {0}", conn))
			{
				cmd.Set("name", guild.Name);
				cmd.Set("leaderName", guild.LeaderName);
				cmd.Set("title", guild.Title);
				cmd.Set("establishedDate", guild.EstablishedDate);
				cmd.Set("server", guild.Server);
				cmd.Set("introMessage", guild.IntroMessage);
				cmd.Set("welcomeMessage", guild.WelcomeMessage);
				cmd.Set("leavingMessage", guild.LeavingMessage);
				cmd.Set("rejectionMessage", guild.RejectionMessage);
				cmd.Set("type", (int)guild.Type);
				cmd.Set("visibility", (int)guild.Visibility);

				cmd.Execute();

				guild.Id = cmd.LastId;
			}
		}

		/// <summary>
		/// Returns a list of guilds from the database, based on the given
		/// search parameters.
		/// </summary>
		/// <param name="serverName"></param>
		/// <param name="page"></param>
		/// <param name="perPage"></param>
		/// <param name="sortBy"></param>
		/// <param name="sortType"></param>
		/// <param name="guildLevel"></param>
		/// <param name="memberAmount"></param>
		/// <param name="guildType"></param>
		/// <param name="searchText"></param>
		/// <returns></returns>
		public List<GuildSearchResult> GetGuildList(string serverName, GuildSortBy sortBy, GuildSortType sortType, GuildSearchLevel guildLevel, GuildSearchMembers memberAmount, GuildType guildType, string searchText)
		{
			var result = new List<GuildSearchResult>();

			serverName = MySqlHelper.EscapeString(serverName);
			searchText = MySqlHelper.EscapeString(searchText);

			// Build query
			var query = string.Format(
				"SELECT `g`.*, COUNT(`m`.`characterId`) AS `memberCount`" +
				" FROM `guilds` AS `g`" +
				" LEFT JOIN `guild_members` AS `m` ON `g`.`guildId` = `m`.`guildId`" +
				" WHERE `g`.`server` = '{0}' AND `g`.`name` LIKE '%{1}%' AND `g`.`guildId` > {2}", serverName, searchText, MabiId.Guilds);

			// Search for level
			if (guildLevel != GuildSearchLevel.All)
			{
				GuildLevel level;
				switch (guildLevel)
				{
					default:
					case GuildSearchLevel.Beginner: level = GuildLevel.Beginner; break;
					case GuildSearchLevel.Basic: level = GuildLevel.Basic; break;
					case GuildSearchLevel.Advanced: level = GuildLevel.Advanced; break;
					case GuildSearchLevel.Great: level = GuildLevel.Great; break;
					case GuildSearchLevel.Grand: level = GuildLevel.Grand; break;
				}

				query += string.Format(" AND `g`.`level` = {0}", (int)level);
			}

			// Search for guild type
			if (guildType != GuildType.All)
				query += string.Format(" AND `g`.`type` = {0}", (int)guildType);

			// For the join
			query += " GROUP BY `g`.`guildId`";

			// Search for member amount range
			if (memberAmount != GuildSearchMembers.All)
			{
				int min, max;
				switch (memberAmount)
				{
					default:
					case GuildSearchMembers.Lv1_5: min = 1; max = 5; break;
					case GuildSearchMembers.Lv6_10: min = 6; max = 10; break;
					case GuildSearchMembers.Lv11_20: min = 11; max = 20; break;
					case GuildSearchMembers.Lv21_50: min = 21; max = 50; break;
					case GuildSearchMembers.Lv51_X: min = 21; max = 999999; break;
				}

				query += string.Format(" HAVING `memberCount` >= {0} AND `memberCount` <= {1}", min, max);
			}

			// Sort
			if (sortBy != GuildSortBy.None)
			{
				string by;
				string type = sortType.ToString().ToUpper();
				switch (sortBy)
				{
					default:
					case GuildSortBy.Level: by = "`g`.`level`"; break;
					case GuildSortBy.Members: by = "`memberCount`"; break;
					case GuildSortBy.Type: by = "`g`.`type`"; break;
					case GuildSortBy.Name: by = "`g`.`name`"; break;
				}
				query += string.Format(" ORDER BY {0} {1}", by, type);
			}

			// Query
			using (var conn = this.Connection)
			using (var mc = new MySqlCommand(query, conn))
			using (var reader = mc.ExecuteReader())
			{
				while (reader.Read())
				{
					var guild = new GuildSearchResult();
					guild.Id = reader.GetInt64("guildId");
					guild.MemberCount = reader.GetInt32("memberCount");
					guild.Name = reader.GetString("name");
					guild.Type = (GuildType)reader.GetInt32("type");
					guild.LeaderName = reader.GetString("leaderName");

					switch ((GuildLevel)reader.GetInt32("level"))
					{
						default:
						case GuildLevel.Beginner: guild.LevelIndex = 5; break;
						case GuildLevel.Basic: guild.LevelIndex = 10; break;
						case GuildLevel.Advanced: guild.LevelIndex = 20; break;
						case GuildLevel.Great: guild.LevelIndex = 50; break;
						case GuildLevel.Grand: guild.LevelIndex = 250; break;
					}

					result.Add(guild);
				}
			}

			return result;
		}
	}

	public class GuildSearchResult
	{
		public long Id { get; set; }
		public int LevelIndex { get; set; }
		public int MemberCount { get; set; }
		public string Name { get; set; }
		public GuildType Type { get; set; }
		public string LeaderName { get; set; }
	}

	/// <summary>
	/// Extensions for the MySqlDataReader.
	/// </summary>
	public static class MySqlDataReaderExtension
	{
		/// <summary>
		/// Returns true if value at index is null.
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		private static bool IsDBNull(this MySqlDataReader reader, string index)
		{
			return reader.IsDBNull(reader.GetOrdinal(index));
		}

		/// <summary>
		/// Same as GetString, except for a is null check. Returns null if NULL.
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		public static string GetStringSafe(this MySqlDataReader reader, string index)
		{
			if (IsDBNull(reader, index))
				return null;
			else
				return reader.GetString(index);
		}

		/// <summary>
		/// Returns DateTime of the index, or DateTime.MinValue, if value is null.
		/// </summary>
		/// <param name="reader"></param>
		/// <param name="index"></param>
		/// <returns></returns>
		public static DateTime GetDateTimeSafe(this MySqlDataReader reader, string index)
		{
			return reader[index] as DateTime? ?? DateTime.MinValue;
		}
	}

	/// <summary>
	/// Result of NameOkay.
	/// </summary>
	public enum NameCheckResult : byte
	{
		Okay = 0,
		Exists = 1,
		Invalid = 2,
	}
}
