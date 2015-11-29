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
		public List<FinishData> Finish { get; set; }

		public ItemData ItemData { get; set; }

		public ManualData()
		{
			this.Materials = new List<ProductionMaterialData>();
			this.Finish = new List<FinishData>();
		}

		/// <summary>
		/// Returns a copy of the materials.
		/// </summary>
		/// <remarks>
		/// Use this if you have to modify the list in any way,
		/// *don't* modify the Materials property.
		/// </remarks>
		/// <returns></returns>
		public List<ProductionMaterialData> GetMaterialList()
		{
			var result = new List<ProductionMaterialData>();

			foreach (var material in this.Materials)
				result.Add(new ProductionMaterialData(material.Tag, material.Amount));

			return result;
		}

		/// <summary>
		/// Returns a copy of the finish with the given id, or null if
		/// it doesn't exist.
		/// </summary>
		/// <remarks>
		/// Use this if you have to modify the list in any way,
		/// *don't* modify the Finish property.
		/// </remarks>
		/// <returns></returns>
		public FinishData GetFinish(int finishId)
		{
			// Return empty finish data if no finish is required.
			// (Consists of empty material list and nulled colors.)
			if (this.Finish == null || this.Finish.Count == 0)
				return new FinishData();

			// Out-of-bounds check for finishes
			if (finishId > this.Finish.Count - 1)
				throw new IndexOutOfRangeException("finishId is out of range of available finishes.");

			return this.Finish[finishId].Copy();
		}
	}

	public class FinishData
	{
		public List<ProductionMaterialData> Materials { get; set; }
		public uint? Color1 { get; set; }
		public uint? Color2 { get; set; }
		public uint? Color3 { get; set; }

		public FinishData()
		{
			this.Materials = new List<ProductionMaterialData>();
		}

		public FinishData Copy()
		{
			var result = new FinishData();

			foreach (var material in this.Materials)
				result.Materials.Add(new ProductionMaterialData(material.Tag, material.Amount));

			result.Color1 = this.Color1;
			result.Color2 = this.Color2;
			result.Color3 = this.Color3;

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
			entry.AssertNotMissing("category", "id", "manualItemId", "itemId", "exp", "rank", "maxProgress", "rainBonus", "materials", "finish");

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

			// Finishes
			foreach (JObject finish in entry["finish"])
			{
				var finishData = new FinishData();

				foreach (var material in finish["materials"])
				{
					var materialData = new ProductionMaterialData();

					materialData.Tag = (string)material[0];
					materialData.Amount = (int)material[1];

					finishData.Materials.Add(materialData);
				}

				if (finish.ContainsKey("color1")) finishData.Color1 = finish.ReadUInt("color1");
				if (finish.ContainsKey("color2")) finishData.Color2 = finish.ReadUInt("color2");
				if (finish.ContainsKey("color3")) finishData.Color3 = finish.ReadUInt("color3");

				data.Finish.Add(finishData);
			}

			data.ItemData = AuraData.ItemDb.Find(data.ItemId);
			if (data.ItemData == null)
				throw new DatabaseErrorException("Item not found: " + data.ItemId);

			this.Entries.Add(data);
		}
	}
}
