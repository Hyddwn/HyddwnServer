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
	public class RecipeData
	{
		public string Method { get; set; }
		public int ItemId { get; set; }

		public List<IngredientData> MainIngredients { get; set; }
		public List<IngredientData> OtherIngredients { get; set; }

		public RecipeData()
		{
			this.MainIngredients = new List<IngredientData>();
			this.OtherIngredients = new List<IngredientData>();
		}
	}

	public class IngredientData
	{
		public int ItemId { get; set; }
		public int Amount { get; set; }
		public int QualityMin { get; set; }
		public int QualityMax { get; set; }
	}

	public static class CookingMethod
	{
		public const string Mixing = "mix";
		public const string Baking = "cook_with_strong_fire";
		public const string Simmering = "steam";
		public const string Kneading = "knead";
		public const string Boiling = "boil";
		public const string NoodleMaking = "make_noodle";
		public const string DeepFrying = "fry_with_much_oil";
		public const string StirFrying = "fry";
		public const string PastaMaking = "make_pasta";
		public const string JamMaking = "make_jam";
		public const string PieMaking = "make_pie";
		public const string Steaming = "steamed_dish";
	}

	public class CookingDb : DatabaseJson<RecipeData>
	{
		protected override void ReadEntry(JObject entry)
		{
			entry.AssertNotMissing("method", "itemId", "mainIngredients");

			var data = new RecipeData();

			data.Method = entry.ReadString("method");
			data.ItemId = entry.ReadInt("itemId");

			foreach (JObject ingEntry in entry["mainIngredients"])
			{
				ingEntry.AssertNotMissing("itemId", "amount", "qualityMin", "qualityMax");

				var ingData = new IngredientData();

				ingData.ItemId = ingEntry.ReadInt("itemId");
				ingData.Amount = ingEntry.ReadInt("amount");
				ingData.QualityMin = ingEntry.ReadInt("qualityMin");
				ingData.QualityMax = ingEntry.ReadInt("qualityMax");

				data.MainIngredients.Add(ingData);
			}

			if (entry.ContainsKey("otherIngredients"))
			{
				foreach (JObject ingEntry in entry["otherIngredients"])
				{
					ingEntry.AssertNotMissing("itemId", "amount", "qualityMin", "qualityMax");

					var ingData = new IngredientData();

					ingData.ItemId = ingEntry.ReadInt("itemId");
					ingData.Amount = ingEntry.ReadInt("amount");
					ingData.QualityMin = ingEntry.ReadInt("qualityMin");
					ingData.QualityMax = ingEntry.ReadInt("qualityMax");

					data.MainIngredients.Add(ingData);
				}
			}

			this.Entries.Add(data);
		}
	}
}
