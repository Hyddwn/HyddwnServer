// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Mabi.Const;
using Aura.Mabi.Structs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using Xunit;

namespace Aura.Tests.Mabi.Structs
{
	public class UpgradeEffectTests
	{
		[Fact]
		public void UpgradeEffectTypes()
		{
			var effect = new UpgradeEffect(UpgradeType.Prefix);
			Assert.Equal((
				"00 00 00 00  00 00 00 00  00 00 00 00  00 00 00 00" +
				"00 00 00 00  0A 00 00 00  00 00 00 00  0A 00 00 00" +
				"00 00 00 00"
				).Replace(" ", ""), ToHex(effect));

			effect = new UpgradeEffect(UpgradeType.Suffix);
			Assert.Equal((
				"01 00 00 00  00 00 00 00  00 00 00 00  00 00 00 00" +
				"00 00 00 00  0A 00 00 00  00 00 00 00  0A 00 00 00" +
				"00 00 00 00"
				).Replace(" ", ""), ToHex(effect));
		}

		[Fact]
		public void UpgradeEffectStatEffect()
		{
			var effect = new UpgradeEffect(UpgradeType.Suffix);
			effect.SetStatEffect(UpgradeStat.STR, 20, UpgradeValueType.Fix);
			Assert.Equal((
				"01 00 00 00  00 00 00 00  00 00 00 00  0A 03 14 00" +
				"00 00 00 00  0A 00 00 00  00 00 00 00  0A 00 00 00" +
				"00 00 00 00"
				).Replace(" ", ""), ToHex(effect));

			effect.SetType(UpgradeType.Elemental);
			effect.SetStatEffect(UpgradeStat.Fire, 1, UpgradeValueType.Value);
			Assert.Equal((
				"02 00 00 00  00 00 00 00  02 00 00 00  1B 00 01 00" +
				"00 00 00 00  0A 00 00 00  00 00 00 00  0A 00 00 00" +
				"00 00 00 00"
				).Replace(" ", ""), ToHex(effect));
		}

		[Fact]
		public void UpgradeEffectSkillEffect()
		{
			var effect = new UpgradeEffect(UpgradeType.Suffix);
			effect.SetSkillEffect(SkillId.Smash, 1, 100, UpgradeValueType.Percent);
			Assert.Equal((
				"01 00 00 00  00 00 00 00  1B 00 00 00  9B 01 64 00" +
				"22 4E 01 00  0A 00 00 00  00 00 00 00  0A 00 00 00" +
				"00 00 00 00"
				).Replace(" ", ""), ToHex(effect));
		}

		[Fact]
		public void UpgradeEffectStatCheck()
		{
			var effect = new UpgradeEffect(UpgradeType.Suffix);
			effect.SetStatCheck(UpgradeStat.MP, UpgradeCheckType.LowerEqualThan, 500, UpgradeValueType.Percent);
			Assert.Equal((
				"01 00 00 00  00 00 00 00  00 00 00 00  00 00 00 00" +
				"00 00 00 00  0A 00 00 00  00 00 00 00  07 00 00 00" +
				"02 01 F4 01"
				).Replace(" ", ""), ToHex(effect));

			Assert.Throws(typeof(ArgumentException), () => effect.SetStatCheck(UpgradeStat.MP, UpgradeCheckType.SkillRankGreaterThan, 500, UpgradeValueType.Percent));
		}

		[Fact]
		public void UpgradeEffectSkillCheck()
		{
			var effect = new UpgradeEffect(UpgradeType.Suffix);
			effect.SetSkillCheck(SkillId.Smash, UpgradeCheckType.SkillRankGreaterThan, SkillRank.R9);
			Assert.Equal((
				"01 00 00 00  00 00 00 00  00 00 00 00  00 00 00 00" +
				"00 00 00 00  0A 00 00 00  00 00 00 00  0E 00 00 00" +
				"22 4E 00 07"
				).Replace(" ", ""), ToHex(effect));

			Assert.Throws(typeof(ArgumentException), () => effect.SetSkillCheck(SkillId.Smash, UpgradeCheckType.WhenBroken, SkillRank.R9));
		}

		[Fact]
		public void UpgradeEffectBrokenCheck()
		{
			var effect = new UpgradeEffect(UpgradeType.Suffix);
			effect.SetBrokenCheck(true);
			Assert.Equal((
				"01 00 00 00  00 00 00 00  00 00 00 00  00 00 00 00" +
				"00 00 00 00  0A 00 00 00  00 00 00 00  14 00 00 00" +
				"01 00 00 00"
				).Replace(" ", ""), ToHex(effect));
		}

		[Fact]
		public void UpgradeEffectTitleCheck()
		{
			var effect = new UpgradeEffect(UpgradeType.Suffix);
			effect.SetTitleCheck(TitleId.devCAT);
			Assert.Equal((
				"01 00 00 00  00 00 00 00  00 00 00 00  00 00 00 00" +
				"00 00 00 00  0A 00 00 00  00 00 00 00  11 00 00 00" +
				"61 EA 00 00"
				).Replace(" ", ""), ToHex(effect));
		}

		[Fact]
		public void UpgradeEffectConditionCheck()
		{
			var effect = new UpgradeEffect(UpgradeType.Suffix);
			effect.SetConditionCheck(20);
			Assert.Equal((
				"01 00 00 00  00 00 00 00  00 00 00 00  00 00 00 00" +
				"00 00 00 00  0A 00 00 00  00 00 00 00  10 00 00 00" +
				"14 00 00 00"
				).Replace(" ", ""), ToHex(effect));
		}

		[Fact]
		public void UpgradeEffectPtjCheck()
		{
			var effect = new UpgradeEffect(UpgradeType.Suffix);
			effect.SetPtjCheck(PtjType.Bank, 21);
			Assert.Equal((
				"01 00 00 00  00 00 00 00  00 00 00 00  00 00 00 00" +
				"00 00 00 00  0A 00 00 00  00 00 00 00  13 00 00 00" +
				"06 00 15 00"
				).Replace(" ", ""), ToHex(effect));
		}

		[Fact]
		public void UpgradeEffectMonthCheck()
		{
			var effect = new UpgradeEffect(UpgradeType.Suffix);
			effect.SetMonthCheck(Month.Baltane);
			Assert.Equal((
				"01 00 00 00  00 00 00 00  00 00 00 00  00 00 00 00" +
				"00 00 00 00  0A 00 00 00  00 00 00 00  12 00 00 00" +
				"02 00 00 00"
				).Replace(" ", ""), ToHex(effect));
		}

		[Fact]
		public void UpgradeEffectSummonCheck()
		{
			var effect = new UpgradeEffect(UpgradeType.Suffix);
			effect.SetSummonCheck(UpgradeStat.Golem);
			Assert.Equal((
				"01 00 00 00  00 00 00 00  00 00 00 00  00 00 00 00" +
				"00 00 00 00  0A 00 00 00  00 00 00 00  18 00 00 00" +
				"65 00 00 00"
				).Replace(" ", ""), ToHex(effect));

			Assert.Throws(typeof(ArgumentException), () => effect.SetSummonCheck(UpgradeStat.STR));
		}

		[Fact]
		public void UpgradeEffectSupportCheck()
		{
			var effect = new UpgradeEffect(UpgradeType.Suffix);
			effect.SetSupportCheck(SupportRace.Elf);
			Assert.Equal((
				"01 00 00 00  00 00 00 00  00 00 00 00  00 00 00 00" +
				"00 00 00 00  0A 00 00 00  00 00 00 00  15 00 00 00" +
				"01 00 00 00"
				).Replace(" ", ""), ToHex(effect));
		}

		private static string ToHex(UpgradeEffect effect)
		{
			var size = Marshal.SizeOf(effect);
			var arr = new byte[size];
			var ptr = Marshal.AllocHGlobal(size);

			Marshal.StructureToPtr(effect, ptr, true);
			Marshal.Copy(ptr, arr, 0, size);
			Marshal.FreeHGlobal(ptr);

			return BitConverter.ToString(arr).Replace("-", "");
		}
	}
}
