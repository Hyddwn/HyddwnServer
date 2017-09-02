// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Mabi;
using Aura.Mabi.Const;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Aura.Shared.Database
{
	/// <summary>
	/// Base class for holding references to all guilds and synchronizing
	/// them with the database.
	/// </summary>
	public abstract class AbstractGuildManager
	{
		protected object _syncLock = new object();
		private Dictionary<long, Guild> _guilds = new Dictionary<long, Guild>();

		/// <summary>
		/// The database to use to retrive the guilds.
		/// </summary>
		protected abstract AuraDb Database { get; }

		/// <summary>
		/// Returns number of guilds.
		/// </summary>
		public int Count { get { lock (_syncLock) return _guilds.Count; } }

		/// <summary>
		/// Initializes manager, loading all guilds from database.
		/// </summary>
		public virtual void Initialize()
		{
			this.LoadGuilds();
		}

		/// <summary>
		/// Loads all guilds from database.
		/// </summary>
		private void LoadGuilds()
		{
			var guilds = this.Database.GetGuilds();
			foreach (var guild in guilds.Values)
				this.LoadGuild(guild);
		}

		/// <summary>
		/// Loads given guild, adding it to the manager.
		/// </summary>
		/// <param name="guild"></param>
		protected virtual void LoadGuild(Guild guild)
		{
			lock (_syncLock)
				_guilds[guild.Id] = guild;
		}

		/// <summary>
		/// Returns the guild with the given id.
		/// </summary>
		/// <param name="guildId"></param>
		/// <returns></returns>
		public Guild GetGuild(long guildId)
		{
			Guild result;
			lock (_syncLock)
				_guilds.TryGetValue(guildId, out result);
			return result;
		}

		/// <summary>
		/// Returns the guild that has a character with the given id as
		/// member if any.
		/// </summary>
		/// <param name="characterId"></param>
		/// <returns></returns>
		public Guild FindGuildWithMember(long characterId)
		{
			lock (_syncLock)
			{
				foreach (var guild in _guilds.Values)
				{
					var member = guild.GetMember(characterId);
					if (member != null)
						return guild;
				}
			}

			return null;
		}

		/// <summary>
		/// Synchronizes loaded guilds with current information
		/// from the database.
		/// </summary>
		public void SynchronizeGuilds()
		{
			lock (_syncLock)
			{
				var dbGuilds = this.Database.GetGuilds();

				var addedGuilds = new List<Guild>();
				var removedGuilds = new List<Guild>();

				var addedMembers = new List<GuildMember>();
				var removedMembers = new List<GuildMember>();
				var acceptedMembers = new List<GuildMember>();
				var declinedMembers = new List<GuildMember>();
				var updatedMembers = new List<GuildMember>();

				// Check for new guilds
				foreach (var dbGuild in dbGuilds.Values.Where(a => !a.Disbanded))
				{
					if (!_guilds.ContainsKey(dbGuild.Id))
						addedGuilds.Add(dbGuild);
				}

				// Check for removed guilds and member changes
				foreach (var guild in _guilds.Values)
				{
					// Check for removed or disbanded guilds
					Guild dbGuild;
					if (!dbGuilds.TryGetValue(guild.Id, out dbGuild) || dbGuild.Disbanded == true)
					{
						removedGuilds.Add(guild);
						break;
					}

					// Update guild
					guild.LeaderName = dbGuild.LeaderName;
					guild.Title = dbGuild.Title;
					guild.IntroMessage = dbGuild.IntroMessage;
					guild.WelcomeMessage = dbGuild.WelcomeMessage;
					guild.LeavingMessage = dbGuild.LeavingMessage;
					guild.RejectionMessage = dbGuild.RejectionMessage;
					guild.Type = dbGuild.Type;
					guild.Visibility = dbGuild.Visibility;
					guild.Level = dbGuild.Level;
					guild.Options = dbGuild.Options;

					// Check members
					var members = guild.GetMembers();
					var dbMembers = dbGuild.GetMembers();

					// Check for new members
					foreach (var dbMember in dbMembers.Where(a => a.Rank < GuildMemberRank.Declined))
					{
						if (!members.Exists(a => a.CharacterId == dbMember.CharacterId))
							addedMembers.Add(dbMember);
					}

					// Check for removed and changed members
					foreach (var member in members)
					{
						// Check if the member was removed or set to declined.
						var dbMember = dbGuild.GetMember(member.CharacterId);
						if (dbMember == null || (member.Rank != GuildMemberRank.Applied && dbMember.Rank == GuildMemberRank.Declined))
						{
							removedMembers.Add(member);
						}
						// Check for accepted members
						else if (member.Rank == GuildMemberRank.Applied && dbMember.Rank <= GuildMemberRank.Member)
						{
							member.Rank = dbMember.Rank;
							acceptedMembers.Add(member);
						}
						// Check for declined members
						else if (member.Rank == GuildMemberRank.Applied && dbMember.Rank == GuildMemberRank.Declined)
						{
							member.Rank = dbMember.Rank;
							declinedMembers.Add(member);
						}
						// Check for rank changes
						else if (member.Rank != dbMember.Rank)
						{
							member.Rank = dbMember.Rank;
							updatedMembers.Add(member);
						}
					}
				}

				// Add guilds
				foreach (var guild in addedGuilds)
				{
					_guilds[guild.Id] = guild;
					this.OnSyncGuildAdded(guild);
				}

				// Remove guilds
				foreach (var guild in removedGuilds)
				{
					_guilds.Remove(guild.Id);
					this.OnSyncGuildRemoved(guild);
				}

				// Add members
				foreach (var member in addedMembers)
				{
					var guild = _guilds[member.GuildId];
					guild.AddMember(member);
					this.OnSyncGuildMemberAdded(guild, member);
				}

				// Remove members
				foreach (var member in removedMembers)
				{
					var guild = _guilds[member.GuildId];
					guild.RemoveMember(member);
					this.OnSyncGuildMemberRemoved(guild, member);
				}

				// Accept members
				foreach (var member in acceptedMembers)
				{
					var guild = _guilds[member.GuildId];
					this.OnSyncGuildMemberAccepted(guild, member);
				}

				// Decline members
				foreach (var member in declinedMembers)
				{
					var guild = _guilds[member.GuildId];
					this.OnSyncGuildMemberDeclined(guild, member);
				}

				// Update members
				foreach (var member in updatedMembers)
				{
					var guild = _guilds[member.GuildId];
					this.OnSyncGuildMemberUpdated(guild, member);
				}
			}
		}

		/// <summary>
		/// Called when a guild is removed from the manager during
		/// synchronization.
		/// </summary>
		/// <param name="guild"></param>
		protected virtual void OnSyncGuildAdded(Guild guild)
		{
		}

		/// <summary>
		/// Called when a guild is removed from the manager during
		/// synchronization.
		/// </summary>
		/// <param name="guild"></param>
		protected virtual void OnSyncGuildRemoved(Guild guild)
		{
		}

		/// <summary>
		/// Called when a guild member is added to a guild during
		/// synchronization.
		/// </summary>
		/// <param name="guild"></param>
		protected virtual void OnSyncGuildMemberAdded(Guild guild, GuildMember guildMember)
		{
		}

		/// <summary>
		/// Called when a guild member is removed from a guild during
		/// synchronization.
		/// </summary>
		/// <param name="guild"></param>
		protected virtual void OnSyncGuildMemberRemoved(Guild guild, GuildMember guildMember)
		{
		}

		/// <summary>
		/// Called when a guild member is accepted into a guild during
		/// synchronization.
		/// </summary>
		/// <param name="guild"></param>
		protected virtual void OnSyncGuildMemberAccepted(Guild guild, GuildMember guildMember)
		{
		}

		/// <summary>
		/// Called when a guild member is declined from a guild during
		/// synchronization.
		/// </summary>
		/// <param name="guild"></param>
		protected virtual void OnSyncGuildMemberDeclined(Guild guild, GuildMember guildMember)
		{
		}

		/// <summary>
		/// Called when a guild member's rank changes during synchronization.
		/// </summary>
		/// <param name="guild"></param>
		protected virtual void OnSyncGuildMemberUpdated(Guild guild, GuildMember guildMember)
		{
		}
	}
}
