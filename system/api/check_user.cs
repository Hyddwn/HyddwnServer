using Aura.Login;
using Aura.Login.Database;
using Aura.Shared.Util;
using Swebs;
using Swebs.RequestHandlers.CSharp;

public class CheckUserController : Controller
{
	public override void Handle(HttpRequestEventArgs args, string requestuestPath, string localPath)
	{
		var request = args.Request;
		var response = args.Response;

		if (!LoginServer.Instance.Conf.Login.IsTrustedSource(request.ClientIp))
			return;

		var name = request.Parameter("name");
		var pass = request.Parameter("pass");

		// Check parameters
		if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(pass))
		{
			response.Send("0");
			return;
		}

		// Get account
		var account = LoginServer.Instance.Database.GetAccount(name);
		if (account == null)
		{
			response.Send("0");
			return;
		}

		// Check password
		var passwordCorrect = Password.Check(pass, account.Password);

		// Response
		response.Send(passwordCorrect ? "1" : "0");
	}
}
