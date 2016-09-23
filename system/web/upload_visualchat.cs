using Aura.Shared.Util;
using Swebs;
using Swebs.RequestHandlers.CSharp;
using System;
using System.Text.RegularExpressions;

/// <remarks>
/// Parameters:
///   Files:
///     filename   visualchat.png   Chat image, max size 256x96
///     
///   Post:
///     server        string  Server name
///     characterid   long    Character's entity id
///     charname      string  Character's name
/// </remarks>
public class UploadVisualChatController : Controller
{
	public override void Handle(HttpRequestEventArgs args, string requestuestPath, string localPath)
	{
		var request = args.Request;
		var response = args.Response;

		var server = request.Parameter("server", "");
		var characterId = request.Parameter("characterid", "");
		var characterName = request.Parameter("charname", "");
		var file = request.File("filename");

		// Check char name
		if (!Regex.IsMatch(characterName, @"^[0-9A-Za-z_]+$"))
		{
			Log.Debug("UploadVisualChatController: Invalid character name ({0}).", characterName);
			return;
		}

		// Check file
		if (file == null)
		{
			Log.Debug("UploadVisualChatController: File missing (Names: {0}).", string.Join(", ", request.Files.AllKeys));
			return;
		}

		if (file.FileName != "visualchat.png" || file.ContentType != "image/png")
		{
			Log.Debug("UploadVisualChatController: Invalid file ({0}, {1}).", file.FileName, file.ContentType);
			return;
		}

		// Move file
		var fileName = string.Format("chat_{0:yyyyMMdd_HHmmss}_{1}.png", DateTime.Now, characterName);
		file.MoveTo("user/web/save/visual-chat/" + fileName);

		// Response, URL to image
		response.Send(string.Format("http://{0}:{1}/save/visual-chat/{2}", request.LocalEndPoint.Address, request.HttpPort, fileName));
	}
}
