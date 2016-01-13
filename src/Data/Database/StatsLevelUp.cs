// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Aura.Data.Database
{
	[Serializable]
	public class StatsLevelUpData
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

	public class StatsLevelUpDb : DatabaseJsonIndexed<int, Dictionary<int, StatsLevelUpData>>
	{
		public StatsLevelUpData Find(int raceId, int age)
		{
			var race = this.Entries.GetValueOrDefault(raceId);
			if (race == null)
				return null;

			return race.GetValueOrDefault(Math.Min((byte)25, age));
		}

		protected override void ReadEntry(JObject entry)
		{
			entry.AssertNotMissing("race", "age", "ap", "life", "mana", "stamina", "str", "int", "dex", "will", "luck");

			var data = new StatsLevelUpData();

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
					this.Entries[raceId] = new Dictionary<int, StatsLevelUpData>();

				this.Entries[raceId][data.Age] = data;
			}
		}
	}
}
