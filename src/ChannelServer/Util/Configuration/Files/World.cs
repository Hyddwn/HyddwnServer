// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.World.Entities;
using Aura.Shared.Util;
using Aura.Shared.Util.Configuration;
using System;

namespace Aura.Channel.Util.Configuration.Files
{
	public class WorldConfFile : ConfFile
	{
		public float ExpRate { get; protected set; }
		public float QuestExpRate { get; protected set; }
		public float SkillExpRate { get; protected set; }

		public float LevelApRate { get; protected set; }
		public float QuestApRate { get; protected set; }
		public float AgeApRate { get; protected set; }

		public float DropRate { get; protected set; }
		public float GoldDropChance { get; protected set; }
		public float GoldDropRate { get; protected set; }
		public float LuckyFinishChance { get; protected set; }
		public float BigLuckyFinishChance { get; protected set; }
		public float HugeLuckyFinishChance { get; protected set; }
		public float PropDropChance { get; protected set; }
		public int LootStealProtection { get; protected set; }

		public bool DeadlyNpcs { get; protected set; }
		public bool EnableHunger { get; protected set; }
		public bool YouAreWhatYouEat { get; protected set; }

		public int GmcpMinAuth { get; protected set; }

		public bool PerfectPlay { get; protected set; }
		public bool InfiniteResources { get; protected set; }
		public bool PerfectFishing { get; protected set; }
		public bool InfiniteBait { get; protected set; }
		public bool InfiniteArrows { get; protected set; }
		public float SharpMindChance { get; protected set; }
		public bool SafeEnchanting { get; protected set; }

		public bool Bagception { get; protected set; }
		public bool NoDurabilityLoss { get; protected set; }
		public bool UnlimitedUpgrades { get; protected set; }
		public bool UncapProficiency { get; protected set; }
		public bool UnlimitedDyes { get; protected set; }
		public int DyeDifficulty { get; protected set; }
		public bool BrokenEggs { get; protected set; }
		public bool SwitchCancelBolts { get; protected set; }
		public float ProficiencyRate { get; protected set; }
		public bool GlobalBank { get; protected set; }
		public bool ReusingPersonalShopLicenses { get; protected set; }

		public TimeSpan RebirthTime { get; protected set; }

		public int BankGoldPerCharacter { get; protected set; }

		public bool PtjInfiniteMemory { get; protected set; }

		public bool PrivateDungeons { get; protected set; }
		public bool EasySwitch { get; protected set; }
		public bool RandomFloors { get; protected set; }

		public float PartyExpBonus { get; protected set; }
		public int PartyMaxSize { get; protected set; }
		public int PartyQuestMinSize { get; protected set; }

		public float GoldQuestRewardRate { get; protected set; }

		public void Load()
		{
			this.Require("system/conf/world.conf");

			this.ExpRate = this.GetFloat("exp_rate", 100) / 100.0f;
			this.QuestExpRate = this.GetFloat("quest_exp_rate", 100) / 100.0f;
			this.SkillExpRate = this.GetFloat("skill_exp_rate", 100) / 100.0f;

			this.LevelApRate = this.GetFloat("level_ap_rate", 100) / 100.0f;
			this.QuestApRate = this.GetFloat("quest_ap_rate", 100) / 100.0f;
			this.AgeApRate = this.GetFloat("age_ap_rate", 100) / 100.0f;

			this.DropRate = this.GetFloat("drop_rate", 100) / 100.0f;
			this.GoldDropChance = this.GetFloat("gold_drop_chance", 30) / 100.0f;
			this.GoldDropRate = this.GetFloat("gold_drop_rate", 100) / 100.0f;
			this.LuckyFinishChance = this.GetFloat("lucky_finish_chance", 0.015f) / 100.0f;
			this.BigLuckyFinishChance = this.GetFloat("big_lucky_finish_chance", 0.005f) / 100.0f;
			this.HugeLuckyFinishChance = this.GetFloat("huge_lucky_finish_chance", 0.001f) / 100.0f;
			this.PropDropChance = this.GetFloat("prop_drop_chance", 30) / 100.0f;
			this.LootStealProtection = this.GetInt("loot_steal_protection", NPC.DisappearDelay);

			this.DeadlyNpcs = this.GetBool("deadly_npcs", true);
			this.EnableHunger = this.GetBool("enable_hunger", true);
			this.YouAreWhatYouEat = this.GetBool("you_are_what_you_eat", true);

			var gmcpCommand = ChannelServer.Instance.CommandProcessor.GetCommand("gmcp");
			this.GmcpMinAuth = gmcpCommand != null ? gmcpCommand.Auth : this.GetInt("gmcp_min_auth", 50);

			this.PerfectPlay = this.GetBool("perfect_play", false);
			this.InfiniteResources = this.GetBool("infinite_resources", false);
			this.PerfectFishing = this.GetBool("perfect_fishing", false);
			this.InfiniteBait = this.GetBool("infinite_bait", false);
			this.InfiniteArrows = this.GetBool("infinite_arrows", false);
			this.SharpMindChance = this.GetFloat("sharp_mind_chance", 50);
			this.SafeEnchanting = this.GetBool("safe_enchanting", false);

			this.Bagception = this.GetBool("bagception", false);
			this.NoDurabilityLoss = this.GetBool("no_durability_loss", false);
			this.UnlimitedUpgrades = this.GetBool("unlimited_upgrades", false);
			this.UncapProficiency = this.GetBool("uncap_proficiency", false);
			this.UnlimitedDyes = this.GetBool("unlimited_dyes", false);
			this.DyeDifficulty = Math2.Clamp(1, 5, this.GetInt("dye_difficulty", 5));
			this.BrokenEggs = this.GetBool("broken_eggs", true);
			this.SwitchCancelBolts = this.GetBool("switch_cancel_bolts", true);
			this.ProficiencyRate = this.GetFloat("proficiency_rate", 100);
			this.GlobalBank = this.GetBool("global_bank", true);
			this.ReusingPersonalShopLicenses = this.GetBool("reusing_personal_shop_licenses", false);

			this.RebirthTime = TimeSpan.FromDays(this.GetInt("rebirth_time", 6));

			this.BankGoldPerCharacter = this.GetInt("gold_per_character", 5000000);

			this.PtjInfiniteMemory = this.GetBool("ptj_infinite_memory", false);

			this.PrivateDungeons = this.GetBool("private_dungeons", false);
			this.EasySwitch = this.GetBool("easy_switch", false);
			this.RandomFloors = this.GetBool("random_floors", false);

			this.PartyExpBonus = this.GetFloat("party_exp_bonus", 0);
			this.PartyMaxSize = Math2.Clamp(1, 99, this.GetInt("party_max_size", 8));
			this.PartyQuestMinSize = Math2.Clamp(1, this.PartyMaxSize, this.GetInt("party_quest_min_size", 2));

			this.GoldQuestRewardRate = this.GetFloat("gold_quest_reward_rate", 100) / 100.0f;
		}
	}
}
