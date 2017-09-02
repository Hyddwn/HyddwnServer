// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

namespace Aura.Mabi.Const
{
	public enum ObjectiveType : byte
	{
		Kill = 1,
		Collect = 2,
		Talk = 3,
		Deliver = 4,
		Create = 8,
		ReachRank = 9,
		ClearDungeon = 13,
		ReachLevel = 15,
		Equip = 31,
		Gather = 32,
		UseSkill = 98, // TODO: Actual type?
		GetKeyword = 99, // TODO: Actual type?
	}

	public enum RewardType : byte
	{
		Item = 1,
		Gold = 2,
		Exp = 3,
		ExplExp = 4,
		AP = 5,
		Skill = 8, // ?
		Keyword = 98, // TODO: Actual type?
		QuestScroll = 99, // TODO: Actual type?
	}

	public enum QuestType : byte
	{
		/// <summary>
		/// Collection quest (blue icon)
		/// </summary>
		Collect = 0,

		/// <summary>
		/// PTJ?
		/// </summary>
		Deliver = 1,

		/// <summary>
		/// Normal quest
		/// </summary>
		Normal = 2,
	}

	public enum PtjType : short
	{
		General = 0,
		GroceryStore = 1,
		Church = 2,
		CombatSchool = 3,
		MagicSchool = 4,
		HealersHouse = 5,
		Bank = 6,
		BlacksmithShop = 7,
		GeneralShop = 8,
		AdventurersAssociation = 9,
		ClothingShop = 10,
		Bookstore = 11,
		WeaponsShop = 12,
		Inn = 13,
		Stope = 14,
		Pub = 15,
		MusicShop = 16,
		FlowerShop = 17,
		TheAmazingRaceOfMabinogi = 18,
		Library = 19,
		//근면_어린이_대회 = 20,
		SecretWatchGuard = 21,
		SecretRoyalGuard = 22,
		SecretCombatInstructor = 23,
		SecretMagicInstructor = 24,
		SecretFlowerShop = 25,
		AlchemistHouse = 26,
		StreetArtist = 27,
		JoustingArena = 28,
		//타티스 = 29,
		//멘텀 = 30,
		//바스텟 = 31,
		//데이곤 = 32,
		PontiffsCourt = 33,
		Lighthouse = 34,
		PuppetShop = 35,
	}

	public enum RewardGroupType
	{
		Gold = 0,
		Exp = 1,
		Item = 2,
		Scroll = 3, // ?
	}

	/// <summary>
	/// Quest result, describes how much the player got or has to get done.
	/// </summary>
	public enum QuestResult : byte
	{
		Perfect = 0,
		Mid = 1,
		Low = 2,
		None = 99,
	}

	/// <summary>
	/// Used to specify the tab the quest appears under.
	/// </summary>
	public enum QuestCategory
	{
		/// <summary>
		/// Uses old system, based on id?
		/// </summary>
		ById = -1,

		Tutorial = 0,
		Event = 1,
		Repeat = 2,
		Basic = 3,
		Skill = 4,
		TheDivineKnights = 5,
		Saga = 6,
		Shakespeare = 7,
		Alchemist = 8,
		Iria = 9,
		AdventOfTheGoddess = 10,
		Squires = 11,
		Field = 12,
		MidsummerMission = 54,
	}

	/// <summary>
	/// Quest class, displayed in brackets before the quest name.
	/// </summary>
	public enum QuestClass
	{
		None = 0,
		Adventure = 1,
		CloseCombat = 2,
		Magic = 3,
		Archery = 4,
		Mercantile = 5,
		BattleAlchemy = 6,
		MartialArts = 7,
		Music = 8,
		Puppetry = 9,
		Lance = 10,
		HolyArts = 11,
		Transmutation = 12,
		Cooking = 13,
		Smithing = 14,
		Tailoring = 15,
		Medicine = 16,
		Carpentry = 17,
		Gunslinger = 18,
		Ninja = 19,
		Merlin = 20,
		Starlet = 21,
		ProfessorJ = 22,
		CulinaryArtist = 23,
		TreasureHunter = 24,
		Event = 25,
		Guide = 26,
		Normal = 27,
		Life = 28,
		Common = 29,
		Action = 30,
		Paladin = 31,
		HiddenTalent = 32,
		Contents = 33,
		Exploration = 34,
		Partner = 35,
		Guild = 36,
		Daily1 = 37,
		Party = 38,
		Tutorial = 39,
		PartTimeJob = 40,
		ReturnIncentive = 41,
		Daily2 = 42,
		Weekly = 43,
		Monthly = 44,
	}

	/// <summary>
	/// Used to specify a quest's icon.
	/// </summary>
	public enum QuestIcon
	{
		Default = 0,
		Collect = 1,
		GoldReward = 2,
		Icon3 = 3,
		Saga = 4,
		Party = 5,
		ExpReward = 6,
		Theater = 7,
		TheDivineKnights = 8,
		PartyActive = 9,
		ItemReward = 10,
		RomeoAndJuliet = 11,
		Icon12 = 12,
		Icon13 = 13,
		Icon14 = 14,
		Shakespeare = 15,
		Icon16 = 16,
		Normal = 17,
		AdventOfTheGoddess = 18,
		MerchantOfVenice = 19,
		CloseCombat = 20,
		Exploration = 21,
		Alchemist = 22,
		Macbeth = 23,
		Archery = 24,
		ExplorationCap = 25,
		ShadowMission = 26,
		Icon27 = 27,
		Magic = 28,
		Icon29 = 29,
		Hamlet = 30,
		Icon31 = 31,
		BattleAlchemy = 32,

		Transmutation = 33,
		Medicine = 34,
		Icon35 = 35,
		MartialArts = 36,
		Mercantile = 37,
		Icon38 = 38,
		Music = 39,
		Caprentry = 40,
		CulinaryArtist = 41,
		HolyArts = 42,
		Adventure = 43,
		Icon44 = 44,
		Lance = 45,
		Puppetry = 46,
		Cooking = 47,
		Gunslinger = 48,
		Smithing = 49,
		Ninja = 50,
		Tailoring = 51,
		Icon52 = 52,
	}
}
