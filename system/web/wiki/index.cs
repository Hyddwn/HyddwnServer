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

public class WikiController : Controller
{
	public override void Handle(HttpRequestEventArgs args, string requestuestPath, string localPath)
	{
		var request = args.Request;
		var response = args.Response;
		var server = args.Context.Server;

		var handlebars = server.GetEngine("hbs");
		var commonmark = server.GetEngine("md");

		var menu = commonmark.RenderFile(server.GetLocalPath("wiki/pages/_menu.md"));
		var content = commonmark.RenderFile(server.GetLocalPath("wiki/pages/main.md"));

		// Render
		response.Send(handlebars.RenderFile(server.GetLocalPath("wiki/templates/main.htm"), new
		{
			menu,
			content,
		}));
	}
}
