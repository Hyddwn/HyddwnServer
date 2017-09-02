// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Data.Database
{
	[Serializable]
	public class CutsceneData
	{
		/// <summary>
		/// Name of the cutscene.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Creatures appearing in the cutscene.
		/// </summary>
		public CutsceneActorData[] Actors { get; set; }
	}

	[Serializable]
	public class CutsceneActorData
	{
		/// <summary>
		/// Name of the actor.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Name of the actor to use as default if first one wasn't available.
		/// </summary>
		public string Default { get; set; }
	}

	/// <summary>
	/// Database of cutscenes.
	/// </summary>
	public class CutscenesDb : DatabaseJsonIndexed<string, CutsceneData>
	{
		protected override void ReadEntry(JObject entry)
		{
			entry.AssertNotMissing("name");

			var data = new CutsceneData();
			data.Name = entry.ReadString("name");

			var actors = new List<CutsceneActorData>();
			if (entry.ContainsKey("actors"))
			{
				foreach (var actor in entry["actors"])
				{
					string name = (string)actor;
					string def = null;

					var actorData = new CutsceneActorData();

					var idx = name.IndexOf(" (");
					if (idx != -1)
					{
						def = name.Substring(idx + 2, name.Length - idx - 3).Trim();
						name = name.Substring(0, idx).Trim();
					}

					actorData.Name = name;
					actorData.Default = def;

					actors.Add(actorData);
				}
			}
			data.Actors = actors.ToArray();

			this.Entries[data.Name] = data;
		}
	}
}
