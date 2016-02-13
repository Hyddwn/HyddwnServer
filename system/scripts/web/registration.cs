//--- Aura Script -----------------------------------------------------------
// Registration
//--- Description -----------------------------------------------------------
// Provides a simple registration page.
//---------------------------------------------------------------------------

using Aura.Shared.Scripting.Scripts;
using Aura.Shared.Util;
using Aura.Web;
using SharpExpress;
using System.Text.RegularExpressions;

public class RegistrationControllerScript : IScript
{
	public bool Init()
	{
		WebServer.Instance.App.All("/register", (req, res) =>
		{
			var name = req.Parameter("name");
			var pass1 = req.Parameter("password1");
			var pass2 = req.Parameter("password2");

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
			res.Render("system/web/register.htm", new { error, success, name });
		});

		return true;
	}
}
