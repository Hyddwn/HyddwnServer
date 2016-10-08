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

	private Regex _headerRegex = new Regex(@"<h(?<number>[1-6])>(?<title>.*?)<\/h[1-6]>", RegexOptions.Compiled);
	private string _tocCheck = "<p><strong>TOC</strong></p>";

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

		var sidebar = commonmark.RenderFile(server.GetLocalPath("wiki/pages/sidebar.md"));

		// Insert table of contents
		// (TODO: Cache, so it doesn't have to be done every time.)
		//if (content.Contains(_tocCheck))
		//{
		//	var toc = this.GenerateTableOfContents(ref content);
		//	content = content.Replace(_tocCheck, toc);
		//}

		// Render
		response.Send(handlebars.RenderFile(server.GetLocalPath("wiki/templates/main.htm"), new
		{
			sidebar,
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

	private string GenerateTableOfContents(ref string html)
	{
		var level = 0;
		var number = 1;

		var headerMatches = _headerRegex.Matches(html);
		if (headerMatches.Count == 0)
			return "";

		var result = new StringBuilder();
		result.AppendLine("<div class=\"toc\"><div class=\"title\">Contents</div><ol>");

		var prevHeaderNumber = 0;
		foreach (Match match in headerMatches)
		{
			var headerNumber = Convert.ToInt32(match.Groups["number"].Value);
			if (headerNumber == 1)
				continue;
			else if (headerNumber == 2)
				number++;

			if (prevHeaderNumber < headerNumber)
				level++;
			else if (prevHeaderNumber > headerNumber)
				level--;

			var title = match.Groups["title"].Value;
			var href = this.ToAnchorName(title);

			result.AppendLine(string.Format("<li class=\"toc-level{2}\"><a href=\"#{3}\">{1}</a></li>", number, title, level, href));
			html = html.Replace(match.Groups[0].Value, string.Format("<h{0}><span id=\"{1}\"></span>{2}</h{0}>", headerNumber, href, title));

			prevHeaderNumber = headerNumber;
		}

		result.AppendLine("</ol></div>");

		return result.ToString();
	}

	private string ToAnchorName(string title)
	{
		title = title.Replace("'", "");
		title = Regex.Replace(title, @"[^\w]+", "_");

		return title;
	}
}
