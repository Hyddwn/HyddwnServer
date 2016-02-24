// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace Aura.Data.Database
{
	[Serializable]
	public class RegionData
	{
		public int Id { get; set; }
		public string Name { get; set; }
		public bool Indoor { get; set; }
	}

	/// <summary>
	/// Indexed by region id.
	/// </summary>
	public class RegionDb : DatabaseJsonIndexed<int, RegionData>
	{
		public bool TryGetRegionName(int regionId, out string name)
		{
			name = null;

			if (!this.Entries.ContainsKey(regionId))
				return false;

			name = this.Entries[regionId].Name;

			return true;
		}

		protected override void ReadEntry(JObject entry)
		{
			entry.AssertNotMissing("id", "name", "indoor");

			var info = new RegionData();
			info.Id = entry.ReadInt("id");
			info.Name = entry.ReadString("name");
			info.Indoor = entry.ReadBool("indoor");

			this.Entries[info.Id] = info;
		}
	}
}
