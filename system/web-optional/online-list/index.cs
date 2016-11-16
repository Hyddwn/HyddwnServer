//--- Aura Script -----------------------------------------------------------
// Wiki
//--- Description -----------------------------------------------------------
// Provides a simple wiki like page that takes it pages from Markdown files.
//---------------------------------------------------------------------------

using Aura.Shared.Database;
using Aura.Web;
using MySql.Data.MySqlClient;
using Swebs;
using Swebs.RequestHandlers.CSharp;
using System.Collections.Generic;

public class OnlineListController : Controller
{
	public override void Handle(HttpRequestEventArgs args, string requestuestPath, string localPath)
	{
		var request = args.Request;
		var response = args.Response;
		var server = args.Context.Server;

		var handlebars = server.GetEngine("hbs");

		var onlineCharacters = GetOnlineCharacters();
		var onlineCount = onlineCharacters.Count;
		var anyOnline = onlineCount != 0;

		// Render
		response.Send(handlebars.RenderFile(server.GetLocalPath("online-list/templates/main.htm"), new
		{
			onlineCharacters,
			onlineCount,
			anyOnline,
		}));
	}

	private List<Character> GetOnlineCharacters()
	{
		var result = new List<Character>();

		using (var conn = WebServer.Instance.Database.Connection)
		using (var mc = new MySqlCommand(
			"SELECT `c`.`entityId`, `cr`.`server`, `cr`.`name`, `cr`.`race`, `cr`.`level`, `cr`.`levelTotal`, `cr`.`age`, `cr`.`region`" +
			" FROM `characters` AS c" +
			" INNER JOIN `creatures` AS cr ON c.creatureId = cr.creatureId" +
			" WHERE `online` = true"
		, conn))
		{
			using (var reader = mc.ExecuteReader())
			{
				while (reader.Read())
				{
					var character = new Character();

					character.EntityId = reader.GetInt64("entityId");
					character.Server = reader.GetStringSafe("server");
					character.Name = reader.GetStringSafe("name");
					character.RaceId = reader.GetInt32("race");
					character.Level = reader.GetInt32("level");
					character.TotalLevel = reader.GetInt32("levelTotal");
					character.Age = reader.GetInt32("age");
					character.RegionId = reader.GetInt32("region");

					character.RaceName = GetRaceName(character.RaceId);
					character.RegionName = GetRegionName(character.RegionId);

					result.Add(character);
				}
			}
		}

		return result;
	}

	private class Character
	{
		public long EntityId { get; set; }
		public string Server { get; set; }
		public string Name { get; set; }
		public int RaceId { get; set; }
		public string RaceName { get; set; }
		public int Level { get; set; }
		public int TotalLevel { get; set; }
		public int Age { get; set; }
		public int RegionId { get; set; }
		public string RegionName { get; set; }
	}

	private string GetRegionName(int regionId)
	{
		if (regionId >= 1 && regionId <= 9)
			return "Tir Chonaill";

		if (regionId >= 11 && regionId <= 21)
			return "Dunbarton";

		if (regionId >= 52 && regionId <= 69 && regionId != 53)
			return "Emain Macha";

		switch (regionId)
		{
			case 31: return "Bangor";
			case 35: return "TirNaNog";

			default: return "Unknown";
		}
	}

	private string GetRaceName(int raceId)
	{
		switch (raceId & ~3)
		{
			case 10000: return "Human";
			case 9000: return "Elf";
			case 8000: return "Giant";

			default: return "Unknown";
		}
	}
}
