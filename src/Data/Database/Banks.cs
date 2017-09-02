// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Newtonsoft.Json.Linq;
using System;

namespace Aura.Data.Database
{
	[Serializable]
	public class BankData
	{
		/// <summary>
		/// The bank's unique identifier.
		/// </summary>
		public string Id { get; set; }

		/// <summary>
		/// Name of the Bank, as displayed in-game.
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// X coordinate of the position of the bank in the world.
		/// </summary>
		public int X { get; set; }

		/// <summary>
		/// Y coordinate of the position of the bank in the world.
		/// </summary>
		public int Y { get; set; }
	}

	/// <summary>
	/// Bank database.
	/// </summary>
	public class BankDb : DatabaseJsonIndexed<string, BankData>
	{
		protected override void ReadEntry(JObject entry)
		{
			entry.AssertNotMissing("id", "name", "x", "y");

			var data = new BankData();
			data.Id = entry.ReadString("id");
			data.Name = entry.ReadString("name");
			data.X = entry.ReadInt("x");
			data.Y = entry.ReadInt("y");

			this.Entries[data.Id] = data;
		}
	}
}
