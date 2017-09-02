// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Mabi;
using Aura.Mabi.Const;
using Aura.Msgr.Database;
using Aura.Msgr.Network;
using Aura.Shared.Database;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Aura.Msgr
{
	public class GuildManager : AbstractGuildManager
	{
		private const int UpdateInterval = 10 * 1000;
		private Timer _updateTick;

		protected override AuraDb Database { get { return MsgrServer.Instance.Database; } }

		/// <summary>
		/// Initializes manager, loading all guilds and starting update tick.
		/// </summary>
		public override void Initialize()
		{
			base.Initialize();
			_updateTick = new Timer(OnUpdateTick, null, UpdateInterval, UpdateInterval);
		}

		/// <summary>
		/// Called every 5 minutes to synchronize guild information.
		/// </summary>
		/// <param name="now"></param>
		private void OnUpdateTick(object state)
		{
			try
			{
				this.SynchronizeGuilds();
			}
			catch (Exception ex)
			{
				Log.Exception(ex, "Exception during guild update tick.");
			}
		}

		/// <summary>
		/// Executes the given action for all members of guild that are online.
		/// </summary>
		/// <param name="guild"></param>
		/// <param name="action"></param>
		public static void ForOnlineMembers(Guild guild, Action<User> action)
		{
			var members = guild.GetMembers();
			foreach (var member in members)
			{
				// Applicants aren't full members and shouldn't receive
				// "broadcasts" yet.
				if (member.Rank > GuildMemberRank.Member)
					continue;

				var user = MsgrServer.Instance.UserManager.GetUserByCharacterId(member.CharacterId);
				if (user == null)
					continue;

				action(user);
			}
		}

		protected override void OnSyncGuildAdded(Guild guild)
		{
		}

		protected override void OnSyncGuildRemoved(Guild guild)
		{
			var members = guild.GetMembers();
			foreach (var member in members)
				this.OnSyncGuildMemberRemoved(guild, member);
		}

		protected override void OnSyncGuildMemberAdded(Guild guild, GuildMember member)
		{
			// Only announce new members once they've become actual members,
			// no applicants.
			if (member.Rank > GuildMemberRank.Member)
				return;

			var user = MsgrServer.Instance.UserManager.GetUserByCharacterId(member.CharacterId);
			if (user == null)
			{
				user = MsgrServer.Instance.Database.GetUserByCharacterId(member.CharacterId);
				user.Status = ContactStatus.Offline;
			}

			ForOnlineMembers(guild, memberUser =>
			{
				if (memberUser != user)
					Send.GuildMemberState(memberUser.Client, guild, member, user, user.Status);
			});
		}

		protected override void OnSyncGuildMemberRemoved(Guild guild, GuildMember member)
		{
			var user = MsgrServer.Instance.Database.GetUserByCharacterId(member.CharacterId);
			ForOnlineMembers(guild, memberUser =>
			{
				if (memberUser != user)
					Send.GuildMemberRemove(memberUser.Client, user.Id);
			});
		}

		protected override void OnSyncGuildMemberAccepted(Guild guild, GuildMember member)
		{
			this.OnSyncGuildMemberAdded(guild, member);
		}

		protected override void OnSyncGuildMemberDeclined(Guild guild, GuildMember member)
		{
			this.OnSyncGuildMemberRemoved(guild, member);
		}

		protected override void OnSyncGuildMemberUpdated(Guild guild, GuildMember member)
		{
			this.OnSyncGuildMemberAdded(guild, member);
		}
	}
}
