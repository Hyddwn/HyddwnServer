// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Mabi.Const;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Data.Database
{
	public class ActorData
	{
		public string Name { get; set; }
		public int RaceId { get; set; }
		public float Height { get; set; }
		public float Weight { get; set; }
		public float Upper { get; set; }
		public float Lower { get; set; }
		public int FaceItemId { get; set; }
		public int SkinColor { get; set; }
		public int EyeType { get; set; }
		public int EyeColor { get; set; }
		public int MouthType { get; set; }
		public int HairItemId { get; set; }
		public uint HairColor { get; set; }
		public uint Color1 { get; set; }
		public uint Color2 { get; set; }
		public uint Color3 { get; set; }
		public bool HasColors { get; set; }
		public List<ActorItemData> Items { get; set; }

		public ActorData()
		{
			this.Color1 = 0x808080;
			this.Color2 = 0x808080;
			this.Color3 = 0x808080;
			this.Items = new List<ActorItemData>();
		}
	}

	public class ActorItemData
	{
		public int ItemId { get; set; }
		public Pocket Pocket { get; set; }
		public uint Color1 { get; set; }
		public uint Color2 { get; set; }
		public uint Color3 { get; set; }
		public bool HasColors { get; set; }

		public ActorItemData()
		{
			this.Color1 = 0x808080;
			this.Color2 = 0x808080;
			this.Color3 = 0x808080;
		}
	}

	public class ActorDb : DatabaseJsonIndexed<string, ActorData>
	{
		protected override void ReadEntry(JObject entry)
		{
			entry.AssertNotMissing("name", "raceId");

			var data = new ActorData();

			data.Name = entry.ReadString("name");
			data.RaceId = entry.ReadInt("raceId");
			data.Height = entry.ReadFloat("height", 1);
			data.Weight = entry.ReadFloat("weight", 1);
			data.Upper = entry.ReadFloat("upper", 1);
			data.Lower = entry.ReadFloat("lower", 1);
			data.FaceItemId = entry.ReadInt("face");
			data.SkinColor = entry.ReadInt("skinColor");
			data.EyeType = entry.ReadInt("eyeType");
			data.EyeColor = entry.ReadInt("eyeColor");
			data.MouthType = entry.ReadInt("mouthType");
			data.HairItemId = entry.ReadInt("hair");
			data.HairColor = entry.ReadUInt("hairColor");

			data.HasColors = entry.ContainsAnyKeys("color1", "color2", "color3");
			if (data.HasColors)
			{
				data.Color1 = entry.ReadUInt("color1");
				data.Color2 = entry.ReadUInt("color2");
				data.Color3 = entry.ReadUInt("color3");
			}

			if (entry.ContainsKey("items"))
			{
				foreach (JObject itemEntry in entry["items"])
				{
					itemEntry.AssertNotMissing("itemId", "pocket");

					var itemData = new ActorItemData();

					itemData.ItemId = itemEntry.ReadInt("itemId");
					itemData.Pocket = (Pocket)itemEntry.ReadInt("pocket");
					itemData.HasColors = itemEntry.ContainsAnyKeys("color1", "color2", "color3");
					if (itemData.HasColors)
					{
						itemData.Color1 = itemEntry.ReadUInt("color1");
						itemData.Color2 = itemEntry.ReadUInt("color2");
						itemData.Color3 = itemEntry.ReadUInt("color3");
					}

					data.Items.Add(itemData);
				}
			}

			this.Entries[data.Name] = data;
		}
	}
}
