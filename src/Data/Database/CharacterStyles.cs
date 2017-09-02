// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Mabi.Const;
using Newtonsoft.Json.Linq;
using System;
using System.Linq;

namespace Aura.Data.Database
{
	public class CharacterStyleData
	{
		public CharacterStyleType Type { get; set; }
		public int Id { get; set; }
		public string Races { get; set; }
		public int Price { get; set; }
		public int Coupon { get; set; }
	}

	public class CharacterStyleDb : DatabaseJson<CharacterStyleData>
	{
		public CharacterStyleData Find(CharacterStyleType type, int id)
		{
			return this.Entries.FirstOrDefault(a => a.Type == type && a.Id == id);
		}

		protected override void ReadEntry(JObject entry)
		{
			entry.AssertNotMissing("type", "id", "races", "price");

			var data = new CharacterStyleData();

			data.Type = (CharacterStyleType)entry.ReadInt("type");
			if (!Enum.IsDefined(typeof(CharacterStyleType), data.Type))
				throw new DatabaseWarningException("Unknown type '" + data.Type + "'.");

			data.Id = entry.ReadInt("id");
			data.Races = entry.ReadString("races");
			data.Price = entry.ReadInt("price");
			data.Coupon = entry.ReadInt("coupon");

			if (this.Entries.Exists(a => a.Type == data.Type && a.Id == data.Id))
				throw new DatabaseWarningException("Duplicate: " + data.Type + ", " + data.Id);

			this.Entries.Add(data);
		}
	}
}
