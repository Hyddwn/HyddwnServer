// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Mabi.Const;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Data.Database
{
	public class ProductionData
	{
		public ProductionCategory Category { get; set; }
		public int Id { get; set; }
		public int ItemId { get; set; }
		public int Amount { get; set; }
		public int Exp { get; set; }
		public SkillRank Rank { get; set; }
		public string Tool { get; set; }
		public int Durability { get; set; }
		public int Mana { get; set; }
		public List<ProductionMaterialData> Materials { get; set; }
		public Dictionary<SkillRank, int> SuccessRates { get; set; }
		public int RainBonus { get; set; }
		public ItemData ItemData { get; set; }

		public ProductionData()
		{
			this.Materials = new List<ProductionMaterialData>();

			this.SuccessRates = new Dictionary<SkillRank, int>();
			for (int i = 0; i <= 15; ++i)
				this.SuccessRates.Add((SkillRank)i, 0);
		}

		public List<ProductionMaterialData> GetMaterialList()
		{
			var result = new List<ProductionMaterialData>();
			foreach (var material in this.Materials)
				result.Add(new ProductionMaterialData(material.Tag, material.Amount));
			return result;
		}
	}

	public class ProductionMaterialData
	{
		public string Tag { get; set; }
		public int Amount { get; set; }

		public ProductionMaterialData()
		{
		}

		public ProductionMaterialData(string tag, int amount)
		{
			this.Tag = tag;
			this.Amount = amount;
		}
	}

	public class ProductionDb : DatabaseJson<ProductionData>
	{
		public ProductionData[] Find(ProductionCategory category, int productId)
		{
			return this.Entries.Where(a => a.Category == category && a.Id == productId).ToArray();
		}

		protected override void ReadEntry(JObject entry)
		{
			entry.AssertNotMissing("category", "id", "title", "itemId", "amount", "exp", "rank", "materials", "successRates", "rainBonus");

			var data = new ProductionData();

			data.Category = (ProductionCategory)entry.ReadInt("category");
			data.Id = entry.ReadInt("id");
			data.ItemId = entry.ReadInt("itemId");
			data.Amount = entry.ReadInt("amount");
			data.Exp = entry.ReadInt("exp");
			data.Tool = entry.ReadString("tool", null);
			data.Durability = entry.ReadInt("durability");
			data.Mana = entry.ReadInt("mana");

			// Rank
			var difficultyRank = entry.ReadString("rank");
			if (difficultyRank == "Novice")
				data.Rank = SkillRank.Novice;
			else
				data.Rank = (SkillRank)(16 - int.Parse(difficultyRank, NumberStyles.HexNumber));

			// Materials
			foreach (var material in entry["materials"])
			{
				var materialData = new ProductionMaterialData();

				materialData.Tag = (string)material[0];
				materialData.Amount = (int)material[1];

				data.Materials.Add(materialData);
			}

			// Success Rates
			var ratesObj = (JObject)entry["successRates"];
			ratesObj.AssertNotMissing("Novice", "F", "E", "D", "C", "B", "A", "9", "8", "7", "6", "5", "4", "3", "2", "1");
			for (SkillRank rank = SkillRank.Novice; rank <= SkillRank.R1; ++rank)
				data.SuccessRates[rank] = ratesObj.ReadInt(rank.ToString().TrimStart('R'));

			data.RainBonus = entry.ReadInt("rainBonus");

			data.ItemData = AuraData.ItemDb.Find(data.ItemId);
			if (data.ItemData == null)
				throw new DatabaseErrorException("Item not found: " + data.ItemId);

			this.Entries.Add(data);
		}
	}
}
