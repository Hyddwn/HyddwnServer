// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Aura.Data.Database
{
	[Serializable]
	public class StatsUpData
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
	}

	public class StatsLevelUpDb : DatabaseJsonIndexed<int, Dictionary<int, StatsUpData>>
	{
		public StatsUpData Find(int raceId, int age)
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

			var data = new StatsUpData();

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

			foreach (int raceId in entry["race"])
			{
				if (!this.Entries.ContainsKey(raceId))
					this.Entries[raceId] = new Dictionary<int, StatsUpData>();

				this.Entries[raceId][data.Age] = data;
			}
		}
	}
}
