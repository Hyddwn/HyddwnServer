//--- Aura Script -----------------------------------------------------------
// Guild List XML file
//--- Description -----------------------------------------------------------
// Returns a list of all guilds on the server in XML format, sorted and
// filtered by various parameters, which the client uses to display
// the guild list.
//--- Instructions ----------------------------------------------------------
// Set "GuildListPage" in client's urls.xml to use it.
//--- Parameters ------------------------------------------------------------
// CharacterId        long     Character entity Id
// Name_Server        string   Server name
// Page               int      Page to return
// SortField          int      What to sort by
// SortType           int      How to sort (Asc/Desc)
// GuildLevelIndex    int      Guild levels to search for
// GuildMemberIndex   int      Number of member to search for
// GuildType          int      Guild type to search for
// SearchWord         string   Part of guild name to search for
// Passport           string   Nexon password hash, totally gonna use this
//---------------------------------------------------------------------------

using Aura.Data;
using Aura.Mabi.Const;
using Aura.Web;
using Swebs;
using Swebs.RequestHandlers.CSharp;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;

public class GuildListController : Controller
{
	public override void Handle(HttpRequestEventArgs args, string requestuestPath, string localPath)
	{
		var request = args.Request;
		var response = args.Response;

		// Check feature
		if (!AuraData.FeaturesDb.IsEnabled("GuildListBoard"))
		{
			var sbe = new StringBuilder();
			using (var sw = new StringWriter(sbe))
			using (var writer = new XmlTextWriter(sw))
			{
				writer.Formatting = Formatting.Indented;

				writer.WriteStartDocument();
				writer.WriteStartElement("Guildlist");
				writer.WriteAttributeString("RowCount", "0");
				writer.WriteAttributeString("NowPage", "1");
				writer.WriteEndElement();
				writer.WriteEndDocument();
			}

			response.Send(sbe.ToString());
			return;
		}

		//var characterIdStr = request.Parameter("CharacterId");
		var serverName = request.Parameter("Name_Server");
		var pageStr = request.Parameter("Page");
		var sortByStr = request.Parameter("SortField");
		var sortTypeStr = request.Parameter("SortType");
		var guildLevelStr = request.Parameter("GuildLevelIndex");
		var memberAmountStr = request.Parameter("GuildMemberIndex");
		var guildTypeStr = request.Parameter("GuildType");
		var searchText = request.Parameter("SearchWord");
		//var passport = request.Parameter("Passport");

		// Check parameters
		if (serverName == null || pageStr == null || sortByStr == null || sortTypeStr == null || guildLevelStr == null || memberAmountStr == null || guildTypeStr == null || searchText == null)
		{
			response.Send("Invalid parameter (1).");
			return;
		}

		int page;
		GuildSortBy sortBy;
		GuildSortType sortType;
		GuildSearchLevel guildLevel;
		GuildSearchMembers memberAmount;
		GuildType guildType;

		if (!int.TryParse(pageStr, out page)
			|| !Enum.TryParse<GuildSortBy>(sortByStr, out sortBy)
			|| !Enum.TryParse<GuildSortType>(sortTypeStr, out sortType)
			|| !Enum.TryParse<GuildSearchLevel>(guildLevelStr, out guildLevel)
			|| !Enum.TryParse<GuildSearchMembers>(memberAmountStr, out memberAmount)
			|| !Enum.TryParse<GuildType>(guildTypeStr, out guildType))
		{
			response.Send("Invalid parameter (2).");
			return;
		}

		var perPage = 10; // Client limit
		if (page < 1)
			page = 1;

		var allGuilds = WebServer.Instance.Database.GetGuildList(serverName, sortBy, sortType, guildLevel, memberAmount, guildType, searchText);
		var guilds = allGuilds.Skip((page - 1) * perPage).Take(perPage);

		var sb = new StringBuilder();
		using (var sw = new StringWriter(sb))
		using (var writer = new XmlTextWriter(sw))
		{
			writer.Formatting = Formatting.Indented;

			writer.WriteStartDocument();
			writer.WriteStartElement("Guildlist");
			writer.WriteAttributeString("RowCount", allGuilds.Count.ToString());
			writer.WriteAttributeString("NowPage", page.ToString());
			foreach (var guild in guilds)
			{
				writer.WriteStartElement("Guild");
				writer.WriteAttributeString("guildid", guild.Id.ToString());
				writer.WriteAttributeString("levelindex", guild.LevelIndex.ToString());
				writer.WriteAttributeString("membercnt", guild.MemberCount.ToString());
				writer.WriteAttributeString("guildname", guild.Name);
				writer.WriteAttributeString("guildtype", ((int)guild.Type).ToString());
				writer.WriteAttributeString("mastername", guild.LeaderName);
				writer.WriteEndElement();
			}
			writer.WriteEndElement();
			writer.WriteEndDocument();
		}

		response.Send(sb.ToString());
	}
}
