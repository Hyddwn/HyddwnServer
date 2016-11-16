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
	private const string DefaultPageTitle = "Main Page";

	private Dictionary<string, Page> pages;

	private Regex _headerRegex = new Regex(@"<h(?<number>[1-6])>(?<title>.*?)<\/h[1-6]>", RegexOptions.Compiled);
	private string _tocCheck = "<p><strong>TOC</strong></p>";

	public override void Handle(HttpRequestEventArgs args, string requestedPath, string localPath)
	{
		var request = args.Request;
		var response = args.Response;
		var server = args.Context.Server;

		if (pages == null)
			pages = GetPages(server);

		var handlebars = server.GetEngine("hbs");
		var commonmark = server.GetEngine("md");

		Page page = null;
		string content;

		var pageTitle = GetPageName(request.RawQueryString);
		if (string.IsNullOrWhiteSpace(pageTitle))
			pageTitle = DefaultPageTitle;

		if (pages.TryGetValue(pageTitle, out page))
		{
			var pageFilePath = server.GetLocalPath("wiki/pages/" + page.FileName + ".md");

			if (page.Contents == null || File.GetLastWriteTime(pageFilePath) > page.LastUpdate)
			{
				page.Contents = GetPageContents(server, pageFilePath);
				page.LastUpdate = DateTime.Now;
			}

			content = page.Contents;
		}
		else
		{
			content = handlebars.RenderFile(server.GetLocalPath("wiki/templates/notfound.htm"), new { pageTitle });
		}

		var sidebar = commonmark.RenderFile(server.GetLocalPath("wiki/pages/sidebar.md"));

		// Render
		response.Send(handlebars.RenderFile(server.GetLocalPath("wiki/templates/main.htm"), new
		{
			sidebar,
			content,
		}));
	}

	/// <summary>
	/// Returns page name, based on query string.
	/// </summary>
	/// <param name="queryString"></param>
	/// <returns></returns>
	private string GetPageName(string queryString)
	{
		if (string.IsNullOrWhiteSpace(queryString))
			return "";

		var result = queryString;
		result = HttpUtil.UriDecode(result);
		result = result.Replace("_", " ");

		return result;
	}

	/// <summary>
	/// Returns a list of pages, the key being the title of the page.
	/// </summary>
	/// <param name="server"></param>
	/// <returns></returns>
	private Dictionary<string, Page> GetPages(HttpServer server)
	{
		var result = new Dictionary<string, Page>();

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

			result[pageName] = new Page() { Title = pageName, FileName = name };
		}

		return result;
	}

	/// <summary>
	/// Returns the contents for the page at filePath, rendered with
	/// CommonMark and with a TOC if applicable.
	/// </summary>
	/// <param name="server"></param>
	/// <param name="filePath"></param>
	/// <returns></returns>
	private string GetPageContents(HttpServer server, string filePath)
	{
		var commonmark = server.GetEngine("md");
		var contents = commonmark.RenderFile(filePath);

		if (contents.Contains(_tocCheck))
		{
			var toc = this.GenerateTableOfContents(ref contents);
			contents = contents.Replace(_tocCheck, toc);
		}

		return contents;
	}

	/// <summary>
	/// Modifies given HTML code to include anchors for all headers and
	/// returns a table of contents for them.
	/// </summary>
	/// <param name="html"></param>
	/// <returns></returns>
	private string GenerateTableOfContents(ref string html)
	{
		var currentLevel = 0;
		var levels = new List<int>();

		var headerMatches = _headerRegex.Matches(html);
		if (headerMatches.Count == 0)
			return "";

		var result = new StringBuilder();
		result.AppendLine("<div class=\"toc\"><div class=\"title\">Contents</div><ul class=\"list\">");

		var prevHeaderNumber = 0;
		foreach (Match match in headerMatches)
		{
			var headerNumber = Convert.ToInt32(match.Groups["number"].Value);

			// Ignore H1
			if (headerNumber == 1)
				continue;

			// Remove one level if we go back to a higher header
			if (prevHeaderNumber > headerNumber)
				levels.RemoveAt(levels.Count - 1);

			// Set current level to header number and add levels as needed
			currentLevel = headerNumber - 1;
			while (levels.Count < currentLevel)
				levels.Add(0);

			// Increase the current level by one.
			levels[currentLevel - 1]++;

			// Build list item
			var title = match.Groups["title"].Value;
			var href = this.ToAnchorName(title);
			var number = string.Join(".", levels.Select(a => a.ToString()));

			result.AppendLine(string.Format(
				"<li class=\"toc-level{0}\">" +
					"<a href=\"#{1}\">" +
						"<span class=\"number\">{2}</span>{3}" +
					"</a>" +
				"</li>"
				, currentLevel, href, number, title
			));
			html = html.Replace(match.Groups[0].Value, string.Format("<h{0}><span id=\"{1}\"></span>{2}</h{0}>", headerNumber, href, title));

			prevHeaderNumber = headerNumber;
		}

		result.AppendLine("</ol></div>");

		return result.ToString();
	}

	/// <summary>
	/// Replaces non-word characters with underscores.
	/// </summary>
	/// <param name="title"></param>
	/// <returns></returns>
	private string ToAnchorName(string title)
	{
		title = title.Replace("'", "");
		title = Regex.Replace(title, @"[^\w]+", "_");
		title = title.Trim('_');

		return title;
	}

	private class Page
	{
		public string Title { get; set; }
		public string FileName { get; set; }
		public string Contents { get; set; }
		public DateTime LastUpdate { get; set; }
	}
}
