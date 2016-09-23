using Swebs;
using Swebs.RequestHandlers.CSharp;
using System;
using System.Text.RegularExpressions;

/// <remarks>
/// Parameters:
///   Files:
///     visualchat.png   Chat image, max size 256x96
///     
///   Post:
///     server        string  Server name
///     characterid   long    Character's entity id
///     charname      string  Character's name
/// </remarks>
public class UploadVisualChatPage : Controller
{
	public override void Handle(HttpRequestEventArgs args, string requestuestPath, string localPath)
	{
		var request = args.Request;
		var response = args.Response;

		var server = request.Parameter("server", "");
		var characterId = request.Parameter("characterid", "");
		var characterName = request.Parameter("charname", "");
		var file = request.File(0);

		// Check char name
		if (!Regex.IsMatch(characterName, @"^[0-9A-Za-z_]+$"))
			return;

		// Check file
		if (file.FileName != "visualchat.png" || file.ContentType != "image/png")
			return;

		// Move file
		var fileName = string.Format("chat_{0:yyyyMMdd_HHmmss}_{1}.png", DateTime.Now, characterName);
		file.MoveTo("user/save/visual-chat/" + fileName);

		// Response, URL to image
		response.Send("http://" + request.LocalEndPoint.Address.ToString() + ":" + request.HttpPort + "/" + "user/save/visual-chat/" + fileName);
	}
}
