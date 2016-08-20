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

			// Get non-declined members, ordered by their rank and name, putting applicants after the leaders.
			var members = guild.GetMembers().Where(a => a.Rank != GuildMemberRank.Declined).OrderBy(a => a.Rank == GuildMemberRank.Applied ? 25 : (int)a.Rank * 10).ThenBy(a => a.Name);
			var url = string.Format("/guild?guildid={0}&userid={1}&userserver={2}&userchar={3}&key={4}", guildIdStr, accountName, server, characterIdStr, sessionKeyStr);

			res.Render("system/web/guild.htm", new { url = url, guild = guild, members = members, member = guildMember, success = success, error = error });
		}
	}
}
