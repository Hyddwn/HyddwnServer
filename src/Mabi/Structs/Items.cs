// Copyright (c) Aura development team - Licensed nder GNU GPL
// For more information, see license file in the main folder

using System.Runtime.InteropServices;
using Aura.Mabi.Const;
using System;

namespace Aura.Mabi.Structs
{
	/// <summary>
	/// Public item info.
	/// </summary>
	/// <remarks>
	/// FigureA-D appear to be a union. Some items use specific bytes
	/// for byte long information, other items use them in other ways,
	/// for example, A+B as short for giant's beards on their hair
	/// items.
	/// </remarks>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct ItemInfo
	{
		public Pocket Pocket;
		public int Id;
		public uint Color1;
		public uint Color2;
		public uint Color3;
		public ushort Amount;
		private short __unknown7;
		public int Region;
		public int X;
		public int Y;

		/// <summary>
		/// State of the item? (eg. hoods and helmets)
		/// Part of giant's beards
		/// </summary>
		public byte State; // FigureA

		/// <summary>
		/// - Ego aura level (0-21)
		/// - Related to giant's beards
		/// </summary>
		public byte FigureB;

		/// <summary>
		/// Direction? (in region)
		/// </summary>
		public byte FigureC;

		public byte FigureD;
		public byte KnockCount;
		private byte __unknown12;
		private byte __unknown13;
		private byte __unknown14;
	}

	/// <summary>
	/// Private item info.
	/// </summary>
	/// <remarks>
	/// Explicit structure, to write LinkedPocketId as an int.
	/// </remarks>
	[StructLayout(LayoutKind.Explicit)]
	public struct ItemOptionInfo
	{
		[FieldOffset(0)]
		public ItemFlags Flags;
		[FieldOffset(1)]
		public byte __unknown15;
		[FieldOffset(2)]
		public byte __unknown16;
		[FieldOffset(3)]
		public byte __unknown17;

		[FieldOffset(4)]
		public int Price;
		[FieldOffset(8)]
		public int SellingPrice;

		[FieldOffset(12)]
		public Pocket LinkedPocketId;

		[FieldOffset(16)]
		public int Durability;
		[FieldOffset(20)]
		public int DurabilityMax;
		[FieldOffset(24)]
		public int DurabilityOriginal;

		[FieldOffset(28)]
		public ushort AttackMin;
		[FieldOffset(30)]
		public ushort AttackMax;

		[FieldOffset(32)]
		public ushort InjuryMin;
		[FieldOffset(34)]
		public ushort InjuryMax;

		[FieldOffset(36)]
		public byte Balance;
		[FieldOffset(37)]
		public sbyte Critical;
		[FieldOffset(38)]
		public byte __unknown24;
		[FieldOffset(39)]
		public byte __unknown25;

		[FieldOffset(40)]
		public int Defense;

		[FieldOffset(44)]
		public short Protection;
		[FieldOffset(46)]
		public short EffectiveRange;

		[FieldOffset(48)]
		public AttackSpeed AttackSpeed;
		[FieldOffset(49)]
		public byte KnockCount;
		[FieldOffset(50)]
		public short Experience;

		[FieldOffset(52)]
		public byte EP;
		[FieldOffset(53)]
		public byte Upgraded;
		[FieldOffset(54)]
		public byte UpgradeMax;
		[FieldOffset(55)]
		public byte WeaponType;

		[FieldOffset(56)]
		public int Grade;

		[FieldOffset(60)]
		public ushort Prefix;
		[FieldOffset(62)]
		public ushort Suffix;

		[FieldOffset(64)]
		public short Elemental;
		[FieldOffset(66)]
		public short __unknown31;

		[FieldOffset(68)]
		public int ExpireTime;
		[FieldOffset(72)]
		public int StackRemainingTime;
		[FieldOffset(76)]
		public int JoustPointPrice;
		[FieldOffset(80)]
		public int DucatPrice;
		[FieldOffset(84)]
		public int StarPrice;
		[FieldOffset(88)]
		public int PointPrice;
		[FieldOffset(92)]
		public int __unknown3;

		// [1XXXXX] Purpose unknown, added some time
		// between G15 and G19.
		[FieldOffset(96)]
		public int __unknown96;
	}

	/// <summary>
	/// Positions of the pickers when using regular dye.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct DyePickers
	{
		public DyePicker Picker1;
		public DyePicker Picker2;
		public DyePicker Picker3;
		public DyePicker Picker4;
		public DyePicker Picker5;
	}

	/// <summary>
	/// Position of a picker when using regular dye.
	/// </summary>
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct DyePicker
	{
		public short X, Y;
	}

	/// <summary>
	/// Information about an upgrade effect.
	/// </summary>
	/// <remarks>
	/// Used for enchants, certain upgrades, reforging, etc.
	/// 
	/// If ValueType.Percent is used, the value is interpreted as a percentage.
	/// Example: 1 = 0.1, 10 = 1.0, 156 = 15.6, etc
	///
	/// We make heavy use of "unions" here, for easier usability, as a result,
	/// you have to be a little careful what you set. It's best to use the
	/// provided helper methods.
	/// </remarks>
	/// <example>
	/// var effect1 = new UpgradeEffect(UpgradeType.Suffix);
	/// effect1.SetStatEffect(UpgradeStat.STR, 9, UpgradeValueType.Value);
	/// 
	/// var effect2 = new UpgradeEffect(UpgradeType.Suffix);
	/// effect2.SetSkillEffect(SkillId.Smash, 1, 10, UpgradeValueType.Percent);
	/// effect2.SetStatCheck(UpgradeStat.HP, UpgradeCheckType.LowerEqualThan, 100, UpgradeValueType.Percent);
	/// 
	/// var item = new Item(63101); // Dagger
	/// item.OptionInfo.Suffix = 30511; // Aquarius
	/// item.AddUpgradeEffect(effect1);
	/// item.AddUpgradeEffect(effect2);
	/// </example>
	[StructLayout(LayoutKind.Explicit, Pack = 1)]
	public struct UpgradeEffect
	{
		/// <summary>
		/// Type of upgrade.
		/// </summary>
		[FieldOffset(0)]
		public UpgradeType Type;

		/// <summary>
		/// ?
		/// </summary>
		[FieldOffset(4)]
		public int Unk1;

		/// <summary>
		/// ?
		/// </summary>
		/// <remarks>
		/// Default: 0
		/// Skill bonus: 0x1B
		/// Set bonus: 0x16 ?
		/// Elemental: 0x02 ?
		/// </remarks>
		[FieldOffset(8)]
		public int Unk2;

		/// <summary>
		/// Stat modified by the upgrade.
		/// </summary>
		[FieldOffset(12)]
		public UpgradeStat Stat;

		/// <summary>
		/// How the value is applied.
		/// </summary>
		[FieldOffset(13)]
		public UpgradeValueType ValueType;

		/// <summary>
		/// Modifier for the stat.
		/// </summary>
		[FieldOffset(14)]
		public short Value;

		/// <summary>
		/// Skill id used for UpgradeStat.Skill.
		/// </summary>
		[FieldOffset(16)]
		public SkillId SkillId;

		/// <summary>
		/// Skill variable to modify for UpgradeStat.Skill.
		/// </summary>
		[FieldOffset(18)]
		public short SkillVar;

		/// <summary>
		/// ?
		/// </summary>
		/// <remarks>
		/// Has to be 0x0A?
		/// </remarks>
		[FieldOffset(20)]
		public int Unk4;

		/// <summary>
		/// ?
		/// </summary>
		[FieldOffset(24)]
		public int Unk5;

		/// <summary>
		/// The type of check done to see if the effect should be applied.
		/// </summary>
		/// <remarks>
		/// Default value is 0x0A (None).
		/// </remarks>
		[FieldOffset(28)]
		public UpgradeCheckType CheckType;

		// v Stat check v

		// 0E000000 XX YY ZZZZ
		// XX = stat
		// YY = value type
		// ZZZZ = value

		/// <summary>
		/// Stat to check.
		/// </summary>
		[FieldOffset(32)]
		public UpgradeStat CheckStat;

		/// <summary>
		/// Race to check for with UpgradeCheckType.WhenSupporting.
		/// </summary>
		[FieldOffset(32)]
		public SupportRace CheckRace;

		/// <summary>
		/// Ptj to check for with UpgradeCheckType.IfPtjCompletedMoreThan.
		/// </summary>
		[FieldOffset(32)]
		public PtjType CheckPtj;

		/// <summary>
		/// Month to check for with UpgradeCheckType.WhileBeing.
		/// </summary>
		[FieldOffset(32)]
		public Month CheckMonth;

		/// <summary>
		/// State to check for with UpgradeCheckType.WhenBroken.
		/// </summary>
		/// <remarks>
		/// false=intact, true=broken
		/// </remarks>
		[FieldOffset(32)]
		public bool CheckBroken;

		/// <summary>
		/// Title id to check for with UpgradeCheckType.HoldingTitle.
		/// </summary>
		[FieldOffset(32)]
		public ushort CheckTitleId;

		/// <summary>
		/// Condition to check for with UpgradeCheckType.InAStateOf.
		/// </summary>
		[FieldOffset(32)]
		public ushort CheckCondition;

		/// <summary>
		/// How to apply the check, e.g. value vs percentage.
		/// </summary>
		[FieldOffset(33)]
		public UpgradeValueType CheckValueType;

		/// <summary>
		/// The value to check for.
		/// </summary>
		[FieldOffset(34)]
		public short CheckValue;

		// ^ Stat check ^

		// v Skill check v

		// 0E000000 XXXX 00 YY
		// XXXX = skill id
		// YY = rank

		/// <summary>
		/// Id of the skill to check.
		/// </summary>
		[FieldOffset(32)]
		public SkillId CheckSkillId;

		/// <summary>
		/// Skill rank to compare to.
		/// </summary>
		[FieldOffset(35)]
		public SkillRank CheckSkillRank;

		// ^ Skill check ^

		/// <summary>
		/// Initializes upgrade effect, setting required default values.
		/// You should generally use this constructor!
		/// </summary>
		/// <param name="type"></param>
		public UpgradeEffect(UpgradeType type)
		{
			Type = type;
			Unk1 = 0;
			Unk2 = 0;
			Stat = 0;
			ValueType = 0;
			Value = 0;
			SkillId = 0;
			SkillVar = 0;
			Unk4 = 0x0A;
			Unk5 = 0;
			CheckType = UpgradeCheckType.None;
			CheckStat = 0;
			CheckRace = 0;
			CheckPtj = 0;
			CheckMonth = 0;
			CheckBroken = false;
			CheckTitleId = 0;
			CheckCondition = 0;
			CheckValueType = 0;
			CheckValue = 0;
			CheckSkillId = 0;
			CheckSkillRank = 0;
		}

		/// <summary>
		/// Changes effect's type.
		/// </summary>
		/// <param name="type"></param>
		public void SetType(UpgradeType type)
		{
			Type = type;
		}

		/// <summary>
		/// Changes effect to give a stat bonus.
		/// </summary>
		/// <param name="stat"></param>
		/// <param name="value"></param>
		/// <param name="valueType"></param>
		public void SetStatEffect(UpgradeStat stat, short value, UpgradeValueType valueType)
		{
			if (stat < UpgradeStat.Fire || stat > UpgradeStat.Lightning)
				Unk2 = 0x00;
			else
				Unk2 = 0x02;

			Stat = stat;
			ValueType = valueType;
			Value = value;
		}

		/// <summary>
		/// Changes effect to give a skill bonus.
		/// </summary>
		/// <param name="skillId"></param>
		/// <param name="skillVar"></param>
		/// <param name="value"></param>
		public void SetSkillEffect(SkillId skillId, short skillVar, short value, UpgradeValueType valueType)
		{
			Unk2 = 0x1B;

			Stat = UpgradeStat.Skill;
			ValueType = valueType;
			Value = value;
			SkillId = skillId;
			SkillVar = skillVar;
		}

		/// <summary>
		/// Changes effect to check a stat for a value.
		/// </summary>
		/// <param name="stat"></param>
		/// <param name="checkType"></param>
		/// <param name="value"></param>
		public void SetStatCheck(UpgradeStat stat, UpgradeCheckType checkType, short value, UpgradeValueType valueType)
		{
			if (checkType < UpgradeCheckType.GreaterThan || checkType > UpgradeCheckType.Equal)
				throw new ArgumentException(checkType + " is not a stat check.");

			CheckType = checkType;
			CheckStat = stat;
			CheckValueType = valueType;
			CheckValue = value;
		}

		/// <summary>
		/// Changes effect to check for a skill rank.
		/// </summary>
		/// <param name="skillId"></param>
		/// <param name="checkType">SkillRankEqual, SkillRankGreaterThan, or SkillRankLowerThan</param>
		/// <param name="rank"></param>
		public void SetSkillCheck(SkillId skillId, UpgradeCheckType checkType, SkillRank rank)
		{
			if (checkType < UpgradeCheckType.SkillRankEqual || checkType > UpgradeCheckType.SkillRankLowerThan)
				throw new ArgumentException(checkType + " is not a skill check.");

			CheckType = checkType;
			CheckSkillId = skillId;
			CheckValue = 0;
			CheckSkillRank = rank;
		}

		/// <summary>
		/// Changes effect to check if the item is broken (0 durability).
		/// </summary>
		/// <param name="broken">False = intact, True = broken.</param>
		public void SetBrokenCheck(bool broken)
		{
			CheckType = UpgradeCheckType.WhenBroken;
			CheckBroken = broken;
			CheckValueType = 0;
			CheckValue = 0;
		}

		/// <summary>
		/// Changes effect to check if the creature uses the given title.
		/// </summary>
		/// <param name="titleId"></param>
		public void SetTitleCheck(int titleId)
		{
			CheckType = UpgradeCheckType.HoldingTitle;
			CheckTitleId = (ushort)titleId;
			CheckValue = 0;
		}

		/// <summary>
		/// Changes effect to check for the given condition.
		/// </summary>
		/// <param name="conditionId"></param>
		public void SetConditionCheck(int conditionId)
		{
			CheckType = UpgradeCheckType.InAStateOf;
			CheckCondition = (ushort)conditionId;
			CheckValue = 0;
		}

		/// <summary>
		/// Changes effect to check if the given PTJ has been done x times.
		/// </summary>
		/// <param name="ptjType"></param>
		/// <param name="count"></param>
		public void SetPtjCheck(PtjType ptjType, int count)
		{
			CheckType = UpgradeCheckType.IfPtjCompletedMoreThan;
			CheckPtj = ptjType;
			CheckValue = (short)count;
		}

		/// <summary>
		/// Changes effect to check if today is the given Erinn month.
		/// </summary>
		/// <param name="month"></param>
		public void SetMonthCheck(Month month)
		{
			CheckType = UpgradeCheckType.WhileBeing;
			CheckMonth = month;
			CheckValueType = 0;
			CheckValue = 0;
		}

		/// <summary>
		/// Changes effect to check for the given summon to be there.
		/// </summary>
		/// <param name="summonStat"></param>
		public void SetSummonCheck(UpgradeStat summonStat)
		{
			if (summonStat != UpgradeStat.Pet && summonStat != UpgradeStat.BarrierSpikes && summonStat != UpgradeStat.Golem)
				throw new ArgumentException(summonStat + " is not a summon stat.");

			CheckType = UpgradeCheckType.WhileSummoned;
			CheckStat = summonStat;
			CheckValueType = 0;
			CheckValue = 0;
		}

		/// <summary>
		/// Changes effect to check if the given race is supported by the
		/// item owner.
		/// </summary>
		/// <param name="race"></param>
		public void SetSupportCheck(SupportRace race)
		{
			CheckType = UpgradeCheckType.WhenSupporting;
			CheckRace = race;
			CheckValueType = 0;
			CheckValue = 0;
		}
	}
}
