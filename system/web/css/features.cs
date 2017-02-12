using Aura.Data;
using Swebs;
using Swebs.RequestHandlers.CSharp;
using System.Text;

public class FeaturesCssController : Controller
{
	private string cache;

	public override void Handle(HttpRequestEventArgs args, string requestuestPath, string localPath)
	{
		var request = args.Request;
		var response = args.Response;

		response.ContentType = "text/css";
		response.CacheControl = "public, max-age: 3600";

		if (cache == null)
		{
			var sb = new StringBuilder();
			foreach (var entry in AuraData.FeaturesDb.Entries)
			{
				var feature = entry.Value.Name;
				var enabled = entry.Value.Enabled;

				sb.AppendLine(string.Format("*[data-feature='{0}']  {{ display: {1}; }}", feature, enabled ? "inline" : "none"));
				sb.AppendLine(string.Format("*[data-feature='!{0}'] {{ display: {1}; }}", feature, !enabled ? "inline" : "none"));
			}

			cache = sb.ToString();
		}

		response.Send(cache);
	}
}
