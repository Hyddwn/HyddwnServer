// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Newtonsoft.Json.Linq;
using System.Collections.Generic;

namespace Aura.Data.Database
{
	public class ShopLicenseData
	{
		public string Name { get; set; }
		public int Limit { get; set; }
		public bool DirectBankAllowed { get; set; }
		public float SalesFee { get; set; }
		public float ExchangeFee { get; set; }
		public bool AllowAnywhere { get; set; }
		public int[] Regions { get; set; }
		public string[] Zones { get; set; }
	}

	public class ShopLicenseDb : DatabaseJsonIndexed<string, ShopLicenseData>
	{
		protected override void ReadEntry(JObject entry)
		{
			entry.AssertNotMissing("name", "limit", "directBankAllowed", "salesFee", "exchangeFee");

			var data = new ShopLicenseData();

			data.Name = entry.ReadString("name");
			data.Limit = entry.ReadInt("limit");
			data.DirectBankAllowed = entry.ReadBool("directBankAllowed");
			data.SalesFee = entry.ReadFloat("salesFee");
			data.ExchangeFee = entry.ReadFloat("exchangeFee");
			data.AllowAnywhere = entry.ReadBool("allowAnywhere");

			var regions = new List<int>();
			if (entry.ContainsKey("regions"))
			{
				foreach (var region in entry["regions"])
					regions.Add((int)region);
			}

			var zones = new List<string>();
			if (entry.ContainsKey("zones"))
			{
				foreach (var zone in entry["zones"])
					zones.Add((string)zone);
			}

			data.Regions = regions.ToArray();
			data.Zones = zones.ToArray();

			this.Entries[data.Name] = data;
		}
	}
}
