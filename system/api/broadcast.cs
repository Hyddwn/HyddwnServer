using Aura.Login;
using Aura.Login.Network;
using Swebs;
using Swebs.RequestHandlers.CSharp;

public class BroadcastController : Controller
{
	public override void Handle(HttpRequestEventArgs args, string requestuestPath, string localPath)
	{
		var request = args.Request;
		var response = args.Response;

		if (!LoginServer.Instance.Conf.Login.IsTrustedSource(request.ClientIp))
			return;

		var msg = request.Parameter("msg", null);
		if (string.IsNullOrWhiteSpace(msg))
		{
			response.Send("0");
			return;
		}

		Send.Internal_Broadcast(msg);

		response.Send("1");
	}
}
