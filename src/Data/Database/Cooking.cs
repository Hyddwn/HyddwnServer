// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Newtonsoft.Json.Linq;
using System.Collections.Generic;
using System.Linq;

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
		/// <summary>
		/// Returns the first recipe that fits the given method and ingredients.
		/// </summary>
		/// <param name="method"></param>
		/// <param name="itemIds"></param>
		/// <returns></returns>
		public RecipeData Find(string method, IEnumerable<int> itemIds)
		{
			// Go through all method's recipes
			foreach (var entry in this.Entries.Where(a => a.Method == method))
			{
				// Can't match if more main ingredients are required than
				// items provided.
				if (entry.MainIngredients.Count > itemIds.Count())
					continue;

				var fail = false;
				var gotOther = false;

				// Check one item after the other
				foreach (var itemId in itemIds)
				{
					// Found in main? Good.
					if (entry.MainIngredients.Any(a => a.ItemId == itemId))
						continue;

					// Found in others and there's no other used yet? Good.
					if (!gotOther && entry.OtherIngredients.Any(a => a.ItemId == itemId))
					{
						gotOther = true;
						continue;
					}

					// Not found anywhere? Good luck next time.
					fail = true;
					break;
				}

				// If there was no fail, we found an entry that works.
				if (!fail)
					return entry;
			}

			return null;
		}

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

					data.OtherIngredients.Add(ingData);
				}
			}

			this.Entries.Add(data);
		}
	}
}
