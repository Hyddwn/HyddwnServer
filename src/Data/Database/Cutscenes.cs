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
		/// Name of the cutscene
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// Creatures appearing in the cutscene
		/// </summary>
		public string[] Actors { get; set; }
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

			var actors = new List<string>();
			if (entry.ContainsKey("actors"))
			{
				foreach (var actor in entry["actors"])
					actors.Add((string)actor);
			}
			data.Actors = actors.ToArray();

			this.Entries[data.Name] = data;
		}
	}
}
