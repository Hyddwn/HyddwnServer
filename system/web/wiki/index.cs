//--- Aura Script -----------------------------------------------------------
// Wiki
//--- Description -----------------------------------------------------------
// Provides a simple wiki like page that takes it pages from Markdown files.
//---------------------------------------------------------------------------

using Aura.Mabi.Const;
using Aura.Shared.Database;
using Aura.Shared.Util;
using Aura.Web;
using Swebs;
using Swebs.RequestHandlers.CSharp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

public class WikiController : Controller
{
	private Dictionary<string, string> pages;

	public override void Handle(HttpRequestEventArgs args, string requestuestPath, string localPath)
	{
		var request = args.Request;
		var response = args.Response;
		var server = args.Context.Server;

		if (pages == null)
			pages = GetPages(server);

		var handlebars = server.GetEngine("hbs");
		var commonmark = server.GetEngine("md");

		var name = "main";
		var exists = true;

		var pageName = GetPageName(request.RawQueryString);
		if (!string.IsNullOrWhiteSpace(pageName))
		{
			if (!pages.TryGetValue(pageName, out name))
				exists = false;
		}

		var pageFilePath = server.GetLocalPath("wiki/pages/" + name + ".md");
		if (pageFilePath == null)
			exists = false;

		string content;
		if (exists)
			content = commonmark.RenderFile(pageFilePath);
		else
			content = handlebars.RenderFile(server.GetLocalPath("wiki/templates/notfound.htm"), new { pageName });

		var menu = commonmark.RenderFile(server.GetLocalPath("wiki/pages/_menu.md"));

		// Render
		response.Send(handlebars.RenderFile(server.GetLocalPath("wiki/templates/main.htm"), new
		{
			menu,
			content,
		}));
	}

	private string GetPageName(string queryString)
	{
		if (string.IsNullOrWhiteSpace(queryString))
			return "";

		var result = queryString;
		result = HttpUtil.UriDecode(result);
		result = result.Replace("_", " ");

		return result;
	}

	private Dictionary<string, string> GetPages(HttpServer server)
	{
		var result = new Dictionary<string, string>();

		var commonmark = server.GetEngine("md");
		var titleRegex = new Regex(@"<h1>(?<name>.+?)<\/h1>", RegexOptions.Compiled);

		var pageFiles = server.GetLocalFilesIn("/wiki/pages/");
		foreach (var filePath in pageFiles)
		{
			var fileName = Path.GetFileName(filePath);
			var name = Path.GetFileNameWithoutExtension(filePath);
			var pageName = name;
			var contents = commonmark.RenderFile(filePath);

			// Name = First H1 header
			var match = titleRegex.Match(contents);
			if (match.Success)
				pageName = match.Groups["name"].Value;

			result[pageName] = name;
		}

		return result;
	}
}
