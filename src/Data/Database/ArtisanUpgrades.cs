// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;

namespace Aura.Data.Database
{
	[Serializable]
	public class ArtisanUpgradeData
	{
		public int Id { get; set; }

		/// <summary>
		/// Option sets to be randomly applied to an item.
		/// </summary>
		public List<int> Random { get; set; }

		/// <summary>
		/// Option sets that will always be applied to an item.
		/// </summary>
		public List<int> Always { get; set; }

		public ArtisanUpgradeData()
		{
			this.Random = new List<int>();
			this.Always = new List<int>();
		}
	}

	public class ArtisanUpgradesDb : DatabaseJsonIndexed<int, ArtisanUpgradeData>
	{
		protected override void ReadEntry(JObject entry)
		{
			entry.AssertNotMissing("id");

			var data = new ArtisanUpgradeData()
			{
				Id = entry.ReadInt("id")
			};

			if (entry.ContainsKey("random"))
				foreach (var setid in entry["random"])
					data.Random.Add((int)setid);

			if (entry.ContainsKey("always"))
				foreach (var setid in entry["always"])
					data.Always.Add((int)setid);

			this.Entries[data.Id] = data;
		}
	}
}
