//--- Aura Script -----------------------------------------------------------
// Registration Page
//--- Description -----------------------------------------------------------
// Simple registration page that allows users to create new accounts.
//---------------------------------------------------------------------------

using Aura.Shared.Scripting.Scripts;
using Aura.Shared.Util;
using Aura.Web;
using Swebs;
using Swebs.RequestHandlers.CSharp;
using System.IO;
using System.Text.RegularExpressions;

public class RegistrationControllerScript : Controller
{
	public override void Handle(HttpRequestEventArgs args, string requestedPath, string localPath)
	{
		var request = args.Request;
		var response = args.Response;
		var server = args.Context.Server;

		var pageFolder = Path.GetDirectoryName(localPath);
		var templatePath = Path.Combine(pageFolder, "template.htm");

		var name = request.Parameter("name");
		var pass1 = request.Parameter("password1");
		var pass2 = request.Parameter("password2");

		var error = "";
		var success = "";

		if (name != null && pass1 != null && pass2 != null)
		{
			if (pass1 != pass2)
			{
				error = "The passwords don't match.";
				goto L_Send;
			}

			if (name.Length < 4)
			{
				name = "";
				error = "Username too short (min. 4 characters).";
				goto L_Send;
			}

			if (pass1.Length < 6)
			{
				error = "Password too short (min. 6 characters).";
				goto L_Send;
			}

			if (!Regex.IsMatch(name, @"^[0-9A-Za-z]+$"))
			{
				error = "Username contains invalid characters.";
				goto L_Send;
			}

			if (WebServer.Instance.Database.AccountExists(name))
			{
				error = "Account already exists.";
				goto L_Send;
			}

			var passHash = Password.RawToMD5SHA256(pass1);

			WebServer.Instance.Database.CreateAccount(name, passHash, 0);

			Log.Info("New account created: {0}", name);

			name = "";
			success = "Account created successfully.";
		}

	L_Send:
		var engine = server.GetEngine("hbs");
		response.Send(engine.RenderFile(templatePath, new { error, success, name }));
	}
}
