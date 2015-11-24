// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Newtonsoft.Json.Linq;

namespace Aura.Data.Database
{
	public class PortalData
	{
		public string Name { get; set; }
		public string Location { get; set; }
	}

	public class PortalDb : DatabaseJsonIndexed<string, PortalData>
	{
		protected override void ReadEntry(JObject entry)
		{
			entry.AssertNotMissing("name", "location");

			var data = new PortalData();

			data.Name = entry.ReadString("name");
			data.Location = entry.ReadString("location");

			this.Entries[data.Name] = data;
		}
	}
}
