// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Mabi.Const;
using Aura.Shared.Database;
using Aura.Shared.Util;
using SharpExpress;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Aura.Web.Controllers
{
	/// <remarks>
	/// Parameters:
	///     guildid       long    Guild Id
	///     userid        string  Account name
	///     userserver    string  Server name
	///     userchar      long    Character id
	///     key           long    Session key
	/// </remarks>
	public class GuildController : IController
	{
		private const int MessageMaxLength = 1000;

		private Regex _number = new Regex(@"^\d+$", RegexOptions.Compiled);

		public void Index(Request req, Response res)
		{
			var guildIdStr = req.Parameter("guildid");
			var accountName = req.Parameter("userid");
			var server = req.Parameter("userserver");
			var characterIdStr = req.Parameter("userchar");
			var sessionKeyStr = req.Parameter("key");

			// Check parameters
			if (guildIdStr == null || accountName == null || server == null || characterIdStr == null || sessionKeyStr == null)
			{
				res.Send("Invalid parameter (1).");
				return;
			}

			if (!_number.IsMatch(guildIdStr) || !_number.IsMatch(characterIdStr) || !_number.IsMatch(sessionKeyStr))
			{
				res.Send("Invalid parameter (2).");
				return;
			}

			var guildId = Convert.ToInt64(guildIdStr);
			var characterId = Convert.ToInt64(characterIdStr);
			var sessionKey = Convert.ToInt64(sessionKeyStr);

			// Check session
			if (!WebServer.Instance.Database.CheckSession(accountName, sessionKey))
			{
				res.Send("Please log in first.");
				return;
			}

			// Check character
			if (!WebServer.Instance.Database.AccountHasCharacter(accountName, characterId, server))
			{
				res.Send("Invalid character.");
				return;
			}

			// Check guild
			var guild = WebServer.Instance.Database.GetGuild(guildId);
			if (guild == null)
			{
				res.Send("Guild not found.");
				return;
			}

			// Check member
			var guildMember = guild.GetMember(characterId);
			if (guildMember == null)
			{
				res.Send("You're not a member of this guild.");
				return;
			}

			string success = null;
			string error = null;
			bool disbanded = false;

			// Leader actions
			if (guildMember.IsLeader)
			{
				// Settings: Change messages
				if (req.Parameters.Has("intro"))
					this.ChangeMessages(req, guild, ref success, ref error);
				// Settings: Change leader
				else if (req.Parameters.Has("leader"))
					this.ChangeLeader(req, guild, ref success, ref error);
				// Settings: Disband
				else if (req.Parameters.Has("disband"))
				{
					this.Disband(req, guild, ref success, ref error);
					disbanded = true;
				}
			}

			// Applicant actions
			if (guildMember.IsApplicant)
			{
				// Settings: Change messages
				if (req.Parameters.Has("cancelApplication"))
					this.CancelApplication(req, guild, guildMember, ref success, ref error);
			}

			// Get non-declined members, ordered by their rank and name, putting applicants after the leaders.
			var members = guild.GetMembers().Where(a => a.Rank != GuildMemberRank.Declined).OrderBy(a => a.Rank == GuildMemberRank.Applied ? 25 : (int)a.Rank * 10).ThenBy(a => a.Name);
			var url = string.Format("/guild?guildid={0}&userid={1}&userserver={2}&userchar={3}&key={4}", guildIdStr, accountName, server, characterIdStr, sessionKeyStr);

			res.Render("system/web/guild.htm", new
			{
				url = url,
				guild = guild,
				members = members,
				member = guildMember,
				success = success,
				error = error,
				disbanded = disbanded,
				messageMaxLength = MessageMaxLength
			});
		}

		private void ChangeMessages(Request req, Guild guild, ref string success, ref string error)
		{
			guild.IntroMessage = ClampString(req.Parameters.Get("intro", ""), 1000);
			guild.WelcomeMessage = ClampString(req.Parameters.Get("welcome", ""), 1000);
			guild.LeavingMessage = ClampString(req.Parameters.Get("leaving", ""), 1000);
			guild.RejectionMessage = ClampString(req.Parameters.Get("rejection", ""), 1000);

			WebServer.Instance.Database.UpdateGuildMessages(guild);

			success = "The messages were updated.";
		}

		private string ClampString(string str, int length)
		{
			if (str.Length > length)
				str = str.Substring(0, length);

			return str;
		}

		private void ChangeLeader(Request req, Guild guild, ref string success, ref string error)
		{
			var leaderName = req.Parameters.Get("leader", "");

			// Do nothing if no change
			if (leaderName == guild.LeaderName)
				return;

			// Check characters
			var members = guild.GetMembers();
			var currentLeader = members.FirstOrDefault(a => a.Rank == GuildMemberRank.Leader && a.Name == guild.LeaderName);
			var member = members.FirstOrDefault(a => a.Rank < GuildMemberRank.Applied && a.Name == leaderName);
			if (member == null || currentLeader == null)
			{
				error = "Character not found.";
				return;
			}

			// Remove current leader
			currentLeader.Rank = GuildMemberRank.Member;

			// Set new leader
			member.Rank = GuildMemberRank.Leader;
			guild.LeaderName = leaderName;

			WebServer.Instance.Database.UpdateGuildLeader(guild);

			success = "The guild leader was changed.";
		}

		private void Disband(Request req, Guild guild, ref string success, ref string error)
		{
			WebServer.Instance.Database.DisbandGuild(guild);

			success = "The guild has been disbanded.";
		}

		private void CancelApplication(Request req, Guild guild, GuildMember member, ref string success, ref string error)
		{
			member.Rank = GuildMemberRank.Declined;
			WebServer.Instance.Database.UpdateGuildMemberRank(member);

			success = "Your application will be canceled shortly.";
		}
	}
}
