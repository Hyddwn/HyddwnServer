// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
namespace Aura.Shared.Database
{
	public class GuildRobe
	{
		public byte EmblemMark { get; set; }
		public byte EmblemOutline { get; set; }
		public byte Stripes { get; set; }
		public uint RobeColor { get; set; }
		public byte BadgeColor { get; set; }
		public byte EmblemMarkColor { get; set; }
		public byte EmblemOutlineColor { get; set; }
		public byte StripesColor { get; set; }

		/// <summary>
		/// Returns color for the given type.
		/// </summary>
		/// <remarks>
		/// guildrobeset.xml
		/// </remarks>
		/// <param name="type"></param>
		/// <returns></returns>
		public static uint GetColor(byte type)
		{
			switch (type)
			{
				case 00: return 0xE8DE73;
				case 01: return 0xEFEAB5;
				case 02: return 0xF2A33A;
				case 03: return 0xB28F59;
				case 04: return 0xABA88A;
				case 05: return 0x98956E;
				case 06: return 0x74703F;
				case 07: return 0x4E7070;
				case 08: return 0x99DBE9;
				case 09: return 0x06B5DB;
				case 10: return 0x067EDB;
				case 11: return 0xC71967;
				case 12: return 0xC90947;
				case 13: return 0x77384C;
				case 14: return 0x622438;
				case 15: return 0xA2818B;
				case 16: return 0x887B7C;
				case 17: return 0xFFFFFF;
				case 18: return 0x000000;
				case 19: return 0x5A4B66;
				case 20: return 0x746B7A;
				case 21: return 0x9F56A2;
				case 22: return 0xAC79AE;
				case 23: return 0x8673C4;
				case 24: return 0x8C9697;
				case 25: return 0x578172;
				case 26: return 0x79B49F;
				case 27: return 0x317D61;
				case 28: return 0x36C72F;
				case 29: return 0x98E294;

				default:
					throw new ArgumentException("Unknown type.");
			}
		}
	}
}
