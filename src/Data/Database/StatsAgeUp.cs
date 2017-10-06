// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Aura.Data.Database
{
	public class StatsAgeUpData
	{
		public int Age { get; set; }

		public int AP { get; set; }
		public float Life { get; set; }
		public float Mana { get; set; }
		public float Stamina { get; set; }
		public float Str { get; set; }
		public float Int { get; set; }
		public float Dex { get; set; }
		public float Will { get; set; }
		public float Luck { get; set; }

		public float? Height { get; set; }
		public float? Weight { get; set; }
		public float? Upper { get; set; }
		public float? Lower { get; set; }
	}

	public class StatsAgeUpDb : DatabaseJsonIndexed<int, Dictionary<int, StatsAgeUpData>>
	{
		public StatsAgeUpData Find(int raceId, int age)
		{
			var race = this.Entries.GetValueOrDefault(raceId);
			if (race == null)
				return null;

			// Get data for age, if age doesn't exist, use last entry.
			// Creatures can get pretty old, but data is only available
			// until age 15/25. That last entry usually has 0 for all
			// stats.
			var data = race.GetValueOrDefault(age);
			if (data == null)
				data = race.Values.Last();

			return data;
		}

		protected override void ReadEntry(JObject entry)
		{
			entry.AssertNotMissing("race", "age", "life", "mana", "stamina", "str", "int", "dex", "will", "luck");

			var data = new StatsAgeUpData();

			data.Age = entry.ReadByte("age");
			data.AP = entry.ReadInt("ap");
			data.Life = entry.ReadFloat("life");
			data.Mana = entry.ReadFloat("mana");
			data.Stamina = entry.ReadFloat("stamina");
			data.Str = entry.ReadFloat("str");
			data.Int = entry.ReadFloat("int");
			data.Dex = entry.ReadFloat("dex");
			data.Will = entry.ReadFloat("will");
			data.Luck = entry.ReadFloat("luck");

			if (entry.ContainsKey("height")) data.Height = entry.ReadFloat("height");
			if (entry.ContainsKey("weight")) data.Weight = entry.ReadFloat("weight");
			if (entry.ContainsKey("upper")) data.Upper = entry.ReadFloat("upper");
			if (entry.ContainsKey("lower")) data.Lower = entry.ReadFloat("lower");

			foreach (int raceId in entry["race"])
			{
				if (!this.Entries.ContainsKey(raceId))
					this.Entries[raceId] = new Dictionary<int, StatsAgeUpData>();

				this.Entries[raceId][data.Age] = data;
			}
		}
	}
}
