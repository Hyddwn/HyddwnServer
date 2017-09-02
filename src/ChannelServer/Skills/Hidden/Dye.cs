// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.World.Entities;
using Aura.Mabi.Const;
using Aura.Shared.Network;
using Aura.Shared.Util;
using Aura.Mabi.Network;
using Aura.Data.Database;
using Aura.Data;
using System.Drawing;
using System.Drawing.Imaging;
using Aura.Mabi.Structs;

namespace Aura.Channel.Skills.Hidden
{
	/// <summary>
	/// Dyeing skill handler
	/// </summary>
	/// <remarks>
	/// Hidden skill for using any type of dye ampoule. Prepare is called with
	/// the item entity id of the dye and the item to be dyed and goes straight
	/// to Use from there. Before sending Use, the color is determined for
	/// regulars.
	/// </remarks>
	[Skill(SkillId.Dye)]
	public class Dye : IPreparable, IUseable, ICompletable, ICancelable
	{
		// Distort rates
		private const double R1 = 0.5, R2 = 0.3, R3 = 0.2, R4 = 0.03;

		/// <summary>
		/// Size of regular dye field.
		/// </summary>
		private const int MapSize = 256;

		/// <summary>
		/// Prepares skill, goes straight to being ready.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public bool Prepare(Creature creature, Skill skill, Packet packet)
		{
			var itemEntityId = packet.GetLong();
			var dyeEntityId = packet.GetLong();

			var item = creature.Inventory.GetItem(itemEntityId);
			var dye = creature.Inventory.GetItem(dyeEntityId);
			if (item == null || dye == null) return false;

			if (!dye.Data.HasTag("/*dye_ampul/")) return false;

			creature.Temp.SkillItem1 = item;
			creature.Temp.SkillItem2 = dye;

			Send.SkillReadyDye(creature, skill.Info.Id, itemEntityId, dyeEntityId);
			skill.State = SkillState.Ready;

			return true;
		}

		/// <summary>
		/// Handles usage of the skill, called once a part was selected.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void Use(Creature creature, Skill skill, Packet packet)
		{
			var part = packet.GetInt();

			switch (packet.Peek())
			{
				case PacketElementType.Short: this.UseRegular(creature, skill, packet, part); break;
				case PacketElementType.Byte: this.UseFixed(creature, skill, packet, part); break;
			}
		}

		/// <summary>
		/// Handles usage of the skill if it's a regular dye.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		/// <param name="part"></param>
		private void UseRegular(Creature creature, Skill skill, Packet packet, int part)
		{
			var x = packet.GetShort();
			var y = packet.GetShort();

			Send.SkillUseDye(creature, skill.Info.Id, part, x, y);
		}

		/// <summary>
		/// Handles usage of the skill if it's a fixed dye.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		/// <param name="part"></param>
		private void UseFixed(Creature creature, Skill skill, Packet packet, int part)
		{
			var unk = packet.GetByte();

			Send.SkillUseDye(creature, skill.Info.Id, part, unk);
		}

		/// <summary>
		/// Completes skill usage, called once the dyeing is completed.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void Complete(Creature creature, Skill skill, Packet packet)
		{
			var part = packet.GetInt();

			if (creature.Skills.ActiveSkill != skill)
				return;

			if (creature.Temp.SkillItem1 == null || creature.Temp.SkillItem2 == null)
				return;

			switch (packet.Peek())
			{
				case PacketElementType.Short: this.CompleteRegular(creature, packet, skill, part); break;
				case PacketElementType.Byte: this.CompleteFixed(creature, packet, skill, part); break;
			}
		}

		/// <summary>
		/// Completes skill usage if it was a regular dye.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="packet"></param>
		/// <param name="skill"></param>
		/// <param name="part"></param>
		private void CompleteRegular(Creature creature, Packet packet, Skill skill, int part)
		{
			var x = packet.GetShort();
			var y = packet.GetShort();

			// Choose random picker
			var rnd = RandomProvider.Get();
			DyePicker picker;
			switch (rnd.Next(5))
			{
				default:
				case 0: picker = creature.Temp.RegularDyePickers.Picker1; break;
				case 1: picker = creature.Temp.RegularDyePickers.Picker2; break;
				case 2: picker = creature.Temp.RegularDyePickers.Picker3; break;
				case 3: picker = creature.Temp.RegularDyePickers.Picker4; break;
				case 4: picker = creature.Temp.RegularDyePickers.Picker5; break;
			}

			// Apply picker offset
			x = (short)((x + picker.X) % MapSize);
			y = (short)((y + picker.Y) % MapSize);

			// Arguments
			var a1 = creature.Temp.DyeDistortA1;
			var a2 = creature.Temp.DyeDistortA2;
			var a3 = creature.Temp.DyeDistortA3;
			var a4 = creature.Temp.DyeDistortA4;

			// Color item
			var item = creature.Temp.SkillItem1;
			var data = item.Data;
			try
			{
				switch (part)
				{
					default:
					case 0: item.Info.Color1 = GetRegularColor(data.ColorMap1, a1, a2, a3, a4, x, y); break;
					case 1: item.Info.Color2 = GetRegularColor(data.ColorMap2, a1, a2, a3, a4, x, y); break;
					case 2: item.Info.Color3 = GetRegularColor(data.ColorMap3, a1, a2, a3, a4, x, y); break;
				}
				this.DyeSuccess(creature);

				Send.AcquireFixedDyedItemInfo(creature, item.EntityId);
			}
			catch (Exception ex)
			{
				Log.Exception(ex);
				Send.ServerMessage(creature, Localization.Get("Error, please report."));
			}

			Send.SkillCompleteDye(creature, skill.Info.Id, part);
		}

		/// <summary>
		/// Completes skill usage if it was a fixed dye.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="packet"></param>
		/// <param name="skill"></param>
		/// <param name="part"></param>
		private void CompleteFixed(Creature creature, Packet packet, Skill skill, int part)
		{
			// Older logs seem to have an additional byte (like Use?)
			//var unk = packet.GetByte();

			switch (part)
			{
				default:
				case 0: creature.Temp.SkillItem1.Info.Color1 = creature.Temp.SkillItem2.Info.Color1; break;
				case 1: creature.Temp.SkillItem1.Info.Color2 = creature.Temp.SkillItem2.Info.Color1; break;
				case 2: creature.Temp.SkillItem1.Info.Color3 = creature.Temp.SkillItem2.Info.Color1; break;
			}

			this.DyeSuccess(creature);

			Send.AcquireFixedDyedItemInfo(creature, creature.Temp.SkillItem1.EntityId);
			Send.SkillCompleteDye(creature, skill.Info.Id, part);
		}

		/// <summary>
		/// Sends success effect, deletes dye, and updates item color.
		/// </summary>
		/// <param name="creature"></param>
		private void DyeSuccess(Creature creature)
		{
			// Remove dye
			if (!ChannelServer.Instance.Conf.World.UnlimitedDyes)
				creature.Inventory.Decrement(creature.Temp.SkillItem2);

			// Update item color
			Send.ItemUpdate(creature, creature.Temp.SkillItem1);
			if (creature.Temp.SkillItem1.Info.Pocket.IsEquip())
				Send.EquipmentChanged(creature, creature.Temp.SkillItem1);

			// Success effect
			Send.Effect(creature, 2, (byte)4);
		}

		/// <summary>
		/// Called when canceling the skill (do nothing).
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		public void Cancel(Creature creature, Skill skill)
		{
		}

		/// <summary>
		/// Returns the color at the specified location, on a color map,
		/// distorted with the given parameters.
		/// </summary>
		/// <param name="colorMapId">Id of the color map to use.</param>
		/// <param name="a1">Distortion algorithm parameter.</param>
		/// <param name="a2">Distortion algorithm parameter.</param>
		/// <param name="a3">Distortion algorithm parameter.</param>
		/// <param name="a4">Distortion algorithm parameter.</param>
		/// <param name="x">X position of the selected picker.</param>
		/// <param name="y">Y position of the selected picker.</param>
		/// <returns></returns>
		private static uint GetRegularColor(int colorMapId, int a1, int a2, int a3, int a4, int x, int y)
		{
			var mapData = AuraData.ColorMapDb.Find(colorMapId);

			// Some dyes use two different palettes for default and dye colors.
			if (mapData != null && mapData.DyeId != mapData.Id)
				mapData = AuraData.ColorMapDb.Find(mapData.DyeId);

			if (mapData == null)
				throw new Exception("Color map '" + colorMapId + "' not found.");

			// Create map out of data
			var raw = new byte[MapSize * MapSize * 4];
			if (mapData.Width == MapSize)
			{
				Buffer.BlockCopy(mapData.Raw, 0, raw, 0, mapData.Raw.Length);
			}
			else
			{
				// Create tiled image
				var instride = mapData.Width * 4;
				var outstride = MapSize * 4;

				for (int i = 0; i < MapSize; ++i)
				{
					var midx = (i % mapData.Height) * instride;
					for (int fidx = 0; fidx < outstride; fidx += instride)
						Buffer.BlockCopy(mapData.Raw, midx, raw, fidx + i * outstride, Math.Min(instride, outstride - fidx));
				}
			}

			raw = Distort(raw, a1, a2, a3, a4);

			var p = y * MapSize * 4 + x * 4;
			var a = raw[p + 3];
			var r = raw[p + 0];
			var g = raw[p + 1];
			var b = raw[p + 2];

			var color = 0u;
			color |= (uint)(b << (8 * 0));
			color |= (uint)(g << (8 * 1));
			color |= (uint)(r << (8 * 2));
			color |= (uint)(a << (8 * 3));

			return color;
		}

		/// <summary>
		/// Distorts raw image, using the given parameters.
		/// </summary>
		/// <param name="rawIn"></param>
		/// <param name="a1"></param>
		/// <param name="a2"></param>
		/// <param name="a3"></param>
		/// <param name="a4"></param>
		/// <returns></returns>
		private static byte[] Distort(byte[] rawIn, int a1, int a2, int a3, int a4)
		{
			if (rawIn.Length < MapSize * MapSize * 4)
				throw new ArgumentException("rawIn too small.");

			var maxIn = rawIn.Length;
			var rawOut = new byte[rawIn.Length];
			var xf = new Func<int, int>[] { x => CoordDistort(R1, x, a1), x => 0, x => CoordDistort(R3, x, a3), x => 0 };
			var yf = new Func<int, int>[] { y => 0, y => CoordDistort(R2, y, a2), y => 0, y => CoordDistort(R4, y, a4) };

			for (int i = 0; i < xf.Length; ++i)
			{
				var xDistort = xf[i];
				var yDistort = yf[i];

				for (var y = 0; y < MapSize; y++)
				{
					for (var x = 0; x < MapSize; x++)
					{
						var dx = (x + xDistort(y)) % MapSize;
						var dy = (y + yDistort(x)) % MapSize;

						var ptrIn = ((y * 1024) + (x * 4)) % maxIn;
						var ptrOut = (dy * 1024) + (dx * 4);

						rawOut[ptrOut + 0] = rawIn[ptrIn + 0];
						rawOut[ptrOut + 1] = rawIn[ptrIn + 1];
						rawOut[ptrOut + 2] = rawIn[ptrIn + 2];
						rawOut[ptrOut + 3] = rawIn[ptrIn + 3];
					}
				}

				if (i != xf.Length - 1)
				{
					var tmp = rawIn;
					rawIn = rawOut;
					rawOut = tmp;
				}
			}

			return rawOut;
		}

		/// <summary>
		/// Used in Distort.
		/// </summary>
		/// <param name="rate"></param>
		/// <param name="orig"></param>
		/// <param name="arg"></param>
		/// <returns></returns>
		private static int CoordDistort(double rate, int orig, int arg)
		{
			return (int)(rate * (ColorMapDb.DistortionMap[(orig + arg) % ColorMapDb.DistortionMap.Length] ^ 37));
		}
	}
}
