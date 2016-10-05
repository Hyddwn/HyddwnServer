//--- Aura Script -----------------------------------------------------------
// Guild Home Page
//--- Description -----------------------------------------------------------
// Provides basic guild management system, for functions that are missing
// in-game, namely guild member management and message editing.
//--- Instructions ----------------------------------------------------------
// Set "UserGuildHomePage" in client's urls.xml to use it.
//--- Parameters ------------------------------------------------------------
// guildid      long     Guild Id
// userid       string   Account name
// userserver   string   Server name
// userchar     long     Character id
// key          long     Session key
//---------------------------------------------------------------------------

using Aura.Mabi.Const;
using Aura.Shared.Database;
using Aura.Shared.Util;
using Aura.Web;
using Swebs;
using Swebs.RequestHandlers.CSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public class GuildController : Controller
{
	private const int MessageMaxLength = 1000;

	private Regex _number = new Regex(@"^\d+$", RegexOptions.Compiled);

	public override void Handle(HttpRequestEventArgs args, string requestedPath, string localPath)
	{
		var request = args.Request;
		var response = args.Response;
		var server = args.Context.Server;

		var guildIdStr = request.Parameter("guildid");
		var accountName = request.Parameter("userid");
		var serverName = request.Parameter("userserver");
		var characterIdStr = request.Parameter("userchar");
		var sessionKeyStr = request.Parameter("key");

		// Check parameters
		if (guildIdStr == null || accountName == null || serverName == null || characterIdStr == null || sessionKeyStr == null)
		{
			response.Send("Invalid parameter (1).");
			return;
		}

		if (!_number.IsMatch(guildIdStr) || !_number.IsMatch(characterIdStr) || !_number.IsMatch(sessionKeyStr))
		{
			response.Send("Invalid parameter (2).");
			return;
		}

		var guildId = Convert.ToInt64(guildIdStr);
		var characterId = Convert.ToInt64(characterIdStr);
		var sessionKey = Convert.ToInt64(sessionKeyStr);

		// Check session
		if (!WebServer.Instance.Database.CheckSession(accountName, sessionKey))
		{
			response.Send("Please log in first.");
			return;
		}

		// Check character
		if (!WebServer.Instance.Database.AccountHasCharacter(accountName, characterId, serverName))
		{
			response.Send("Invalid character.");
			return;
		}

		// Check guild
		var guild = WebServer.Instance.Database.GetGuild(guildId);
		if (guild == null)
		{
			response.Send("Guild not found.");
			return;
		}

		// Check member
		var guildMember = guild.GetMember(characterId);
		if (guildMember == null)
		{
			response.Send("You're not a member of this guild.");
			return;
		}

		// Determine ability to level up
		var levelUpDisabled = true;
		var levelUpRequirements = "";
		if (guild.Level < GuildLevel.Grand)
		{
			int points, gold;
			if (guild.GetLevelUpRequirements(out points, out gold) && points < guild.Points && gold < guild.Gold)
				levelUpDisabled = false;

			levelUpRequirements = string.Format("Guild Points: {0:n0}, Gold: {1:n0}", points, gold);
		}

		string success = null;
		string error = null;

		// Leader actions
		if (guildMember.IsLeader)
		{
			// Settings: Change messages
			if (request.Parameter("intro") != null)
				this.ChangeMessages(request, guild, ref success, ref error);
			// Settings: Change leader
			else if (request.Parameter("leader") != null)
				this.ChangeLeader(request, guild, ref success, ref error);
			// Settings: Disband
			else if (request.Parameter("disband") != null)
				this.Disband(request, guild, ref success, ref error);
			// Settings: Level up
			else if (!levelUpDisabled && request.Parameter("levelUp") != null)
				this.LevelUp(request, guild, ref success, ref error);
		}

		// Leader/Officer actions
		if (guildMember.IsLeaderOrOfficer)
		{
			// Members: Accept application
			if (request.Parameter("acceptApplication") != null)
			{
				var id = Convert.ToInt64(request.Parameter("acceptApplication"));
				var member = guild.GetMember(id);
				if (member == null)
					error = "Applicant not found.";
				else
					this.AcceptApplication(request, guild, member, ref success, ref error);
			}
			// Members: Decline application
			else if (request.Parameter("declineApplication") != null)
			{
				var id = Convert.ToInt64(request.Parameter("declineApplication"));
				var member = guild.GetMember(id);
				if (member == null)
					error = "Applicant not found.";
				else
					this.DeclineApplication(request, guild, member, ref success, ref error);
			}
			// Members: Remove member
			else if (request.Parameter("removeMember") != null)
			{
				var memberId = Convert.ToInt64(request.Parameter("removeMember"));
				var member = guild.GetMember(memberId);
				if (member == null)
					error = "Character not found.";
				else
					this.RemoveMember(request, guild, member, ref success, ref error);
			}
		}

		// Applicant actions
		if (guildMember.IsApplicant)
		{
			// Menu: Cancel application
			if (request.Parameter("cancelApplication") != null)
				this.CancelApplication(request, guild, guildMember, ref success, ref error);
		}

		// Non-leader actions
		if (!guildMember.IsLeader)
		{
			// Menu: Leave guild
			if (request.Parameter("leaveGuild") != null)
				this.LeaveGuild(request, guild, guildMember, ref success, ref error);
		}

		// Get non-declined members, ordered by their rank and name, putting applicants after the leaders.
		var members = guild.GetMembers().Where(a => a.Rank != GuildMemberRank.Declined).OrderBy(a => a.Rank == GuildMemberRank.Applied ? 25 : (int)a.Rank * 10).ThenBy(a => a.Name);
		var url = string.Format("/guild?guildid={0}&userid={1}&userserver={2}&userchar={3}&key={4}", guildIdStr, accountName, serverName, characterIdStr, sessionKeyStr);

		// Render
		var engine = server.GetEngine("hbs");
		response.Send(engine.RenderFile(server.GetLocalPath("templates/guild.htm"), new
		{
			url = url,
			guild = guild,
			members = members,
			member = guildMember,
			success = success,
			error = error,
			messageMaxLength = MessageMaxLength,
			levelUpDisabled = levelUpDisabled,
			levelUpRequirements = levelUpRequirements,
		}));
	}

	private void LevelUp(HttpRequest req, Guild guild, ref string success, ref string error)
	{
		int points, gold;
		guild.GetLevelUpRequirements(out points, out gold);

		guild.Points -= points;
		guild.Gold -= gold;

		if (++guild.Level > GuildLevel.Grand)
			guild.Level = GuildLevel.Grand;

		WebServer.Instance.Database.UpdateGuildLevelAndResources(guild);

		success = string.Format("The guild has reached level {0}.", guild.Level);
	}

	private void ChangeMessages(HttpRequest req, Guild guild, ref string success, ref string error)
	{
		guild.IntroMessage = ClampString(req.Parameter("intro", ""), 1000);
		guild.WelcomeMessage = ClampString(req.Parameter("welcome", ""), 1000);
		guild.LeavingMessage = ClampString(req.Parameter("leaving", ""), 1000);
		guild.RejectionMessage = ClampString(req.Parameter("rejection", ""), 1000);

		WebServer.Instance.Database.UpdateGuildMessages(guild);

		success = "The messages were updated.";
	}

	private string ClampString(string str, int length)
	{
		if (str.Length > length)
			str = str.Substring(0, length);

		return str;
	}

	private void ChangeLeader(HttpRequest req, Guild guild, ref string success, ref string error)
	{
		var leaderName = req.Parameter("leader", "");

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

	private void Disband(HttpRequest req, Guild guild, ref string success, ref string error)
	{
		guild.Disbanded = true;
		WebServer.Instance.Database.UpdateDisbandGuild(guild);

		success = "The guild has been marked to be disbanded.";
	}

	private void CancelApplication(HttpRequest req, Guild guild, GuildMember member, ref string success, ref string error)
	{
		member.Rank = GuildMemberRank.Declined;
		WebServer.Instance.Database.UpdateGuildMemberRank(member);

		success = "Your application will be canceled shortly.";
	}

	private void LeaveGuild(HttpRequest req, Guild guild, GuildMember member, ref string success, ref string error)
	{
		member.Rank = GuildMemberRank.Declined;
		WebServer.Instance.Database.UpdateGuildMemberRank(member);

		success = "You will be leaving the guild shortly.";
	}

	private void AcceptApplication(HttpRequest req, Guild guild, GuildMember member, ref string success, ref string error)
	{
		member.Rank = GuildMemberRank.Member;
		WebServer.Instance.Database.UpdateGuildMemberRank(member);

		success = string.Format("{0}'s application has been accepted.", member.Name);
	}

	private void DeclineApplication(HttpRequest req, Guild guild, GuildMember member, ref string success, ref string error)
	{
		member.Rank = GuildMemberRank.Declined;
		WebServer.Instance.Database.UpdateGuildMemberRank(member);

		success = string.Format("{0}'s application has been declined.", member.Name);
	}

	private void RemoveMember(HttpRequest req, Guild guild, GuildMember member, ref string success, ref string error)
	{
		member.Rank = GuildMemberRank.Declined;
		WebServer.Instance.Database.UpdateGuildMemberRank(member);

		success = string.Format("{0} will get removed from the guild shortly.", member.Name);
	}
}
