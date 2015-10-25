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
	public enum ManualCategory : short
	{
		Tailoring = 1,
		Blacksmithing = 2,
	}

	public class ManualData
	{
		public ManualCategory Category { get; set; }
		public int Id { get; set; }
		public int ManualItemId { get; set; }
		public int ItemId { get; set; }
		public int Exp { get; set; }
		public SkillRank Rank { get; set; }
		public float MaxProgress { get; set; }
		public float RainBonus { get; set; }
		public List<ProductionMaterialData> Materials { get; set; }
		public List<ProductionMaterialData> FinishMaterials { get; set; }

		public ItemData ItemData { get; set; }

		public ManualData()
		{
			this.Materials = new List<ProductionMaterialData>();
			this.FinishMaterials = new List<ProductionMaterialData>();
		}

		public List<ProductionMaterialData> GetMaterialList()
		{
			var result = new List<ProductionMaterialData>();
			foreach (var material in this.Materials)
				result.Add(new ProductionMaterialData(material.Tag, material.Amount));
			return result;
		}

		public List<ProductionMaterialData> GetFinishMaterialList()
		{
			var result = new List<ProductionMaterialData>();
			foreach (var material in this.FinishMaterials)
				result.Add(new ProductionMaterialData(material.Tag, material.Amount));
			return result;
		}
	}

	public class ManualDb : DatabaseJson<ManualData>
	{
		public ManualData Find(ManualCategory category, int manualId)
		{
			return this.Entries.FirstOrDefault(a => a.Category == category && a.Id == manualId);
		}

		protected override void ReadEntry(JObject entry)
		{
			entry.AssertNotMissing("category", "id", "manualItemId", "itemId", "exp", "rank", "maxProgress", "rainBonus", "materials", "finishMaterials");

			var data = new ManualData();

			data.Category = (ManualCategory)entry.ReadInt("category");
			data.Id = entry.ReadInt("id");
			data.ManualItemId = entry.ReadInt("manualItemId");
			data.ItemId = entry.ReadInt("itemId");
			data.Exp = entry.ReadInt("exp");
			data.MaxProgress = entry.ReadFloat("maxProgress");
			data.RainBonus = entry.ReadFloat("rainBonus");

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

			// Finish Materials
			foreach (var material in entry["finishMaterials"])
			{
				var materialData = new ProductionMaterialData();

				materialData.Tag = (string)material[0];
				materialData.Amount = (int)material[1];

				data.FinishMaterials.Add(materialData);
			}

			data.ItemData = AuraData.ItemDb.Find(data.ItemId);
			if (data.ItemData == null)
				throw new DatabaseErrorException("Item not found: " + data.ItemId);

			this.Entries.Add(data);
		}
	}
}
