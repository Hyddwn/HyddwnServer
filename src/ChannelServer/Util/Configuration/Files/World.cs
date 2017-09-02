// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using Aura.Channel.World.Entities;
using Aura.Shared.Util;
using Aura.Shared.Util.Configuration;

namespace Aura.Channel.Util.Configuration.Files
{
    public class WorldConfFile : ConfFile
    {
        public float ExpRate { get; protected set; }
        public float QuestExpRate { get; protected set; }
        public float SkillExpRate { get; protected set; }
        public float LifeSkillExpRate { get; protected set; }
        public float CombatSkillExpRate { get; protected set; }
        public float MagicSkillExpRate { get; protected set; }
        public float AlchemySkillExpRate { get; protected set; }
        public float FighterSkillExpRate { get; protected set; }
        public float MusicSkillExpRate { get; protected set; }
        public float PuppetSkillExpRate { get; protected set; }
        public float GunsSkillExpRate { get; protected set; }
        public float NinjaSkillExpRate { get; protected set; }
        public float TransformationSkillExpRate { get; protected set; }
        public float DemiSkillExpRate { get; protected set; }
        public float DivineKnightsSkillExpRate { get; protected set; }

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
        public float PaladinDurationRate { get; protected set; }
        public bool PaladinCancelOnDeath { get; protected set; }

        public bool Bagception { get; protected set; }
        public bool NoDurabilityLoss { get; protected set; }
        public bool NoDecay { get; protected set; }
        public bool UnlimitedUpgrades { get; protected set; }
        public bool UncapProficiency { get; protected set; }
        public bool UnlimitedDyes { get; protected set; }
        public int DyeDifficulty { get; protected set; }
        public bool BrokenEggs { get; protected set; }
        public bool SwitchCancelBolts { get; protected set; }
        public float ProficiencyRate { get; protected set; }
        public bool GlobalBank { get; protected set; }
        public bool ReusingPersonalShopLicenses { get; protected set; }
        public int MaxExtraSets { get; protected set; }

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
            Require("system/conf/world.conf");

            ExpRate = GetFloat("exp_rate", 100) / 100.0f;
            QuestExpRate = GetFloat("quest_exp_rate", 100) / 100.0f;
            SkillExpRate = GetFloat("skill_exp_rate", 100) / 100.0f;
            LifeSkillExpRate = GetFloat("life_skill_exp_rate", 100) / 100.0f;
            CombatSkillExpRate = GetFloat("combat_skill_exp_rate", 100) / 100.0f;
            MagicSkillExpRate = GetFloat("magic_skill_exp_rate", 100) / 100.0f;
            AlchemySkillExpRate = GetFloat("alchemy_skill_exp_rate", 100) / 100.0f;
            FighterSkillExpRate = GetFloat("fighter_skill_exp_rate", 100) / 100.0f;
            MusicSkillExpRate = GetFloat("music_skill_exp_rate", 100) / 100.0f;
            PuppetSkillExpRate = GetFloat("puppetry_skill_exp_rate", 100) / 100.0f;
            GunsSkillExpRate = GetFloat("dualgun_skill_exp_rate", 100) / 100.0f;
            NinjaSkillExpRate = GetFloat("ninja_skill_exp_rate", 100) / 100.0f;
            TransformationSkillExpRate = GetFloat("transformations_skill_exp_rate", 100) / 100.0f;
            DemiSkillExpRate = GetFloat("demigod_skill_exp_rate", 100) / 100.0f;
            DivineKnightsSkillExpRate = GetFloat("crusader_skill_exp_rate", 100) / 100.0f;

            LevelApRate = GetFloat("level_ap_rate", 100) / 100.0f;
            QuestApRate = GetFloat("quest_ap_rate", 100) / 100.0f;
            AgeApRate = GetFloat("age_ap_rate", 100) / 100.0f;

            DropRate = GetFloat("drop_rate", 100) / 100.0f;
            GoldDropChance = GetFloat("gold_drop_chance", 30) / 100.0f;
            GoldDropRate = GetFloat("gold_drop_rate", 100) / 100.0f;
            LuckyFinishChance = GetFloat("lucky_finish_chance", 0.015f) / 100.0f;
            BigLuckyFinishChance = GetFloat("big_lucky_finish_chance", 0.005f) / 100.0f;
            HugeLuckyFinishChance = GetFloat("huge_lucky_finish_chance", 0.001f) / 100.0f;
            PropDropChance = GetFloat("prop_drop_chance", 30) / 100.0f;
            LootStealProtection = GetInt("loot_steal_protection", NPC.DisappearDelay);

            DeadlyNpcs = GetBool("deadly_npcs", true);
            EnableHunger = GetBool("enable_hunger", true);
            YouAreWhatYouEat = GetBool("you_are_what_you_eat", true);

            var gmcpCommand = ChannelServer.Instance.CommandProcessor.GetCommand("gmcp");
            GmcpMinAuth = gmcpCommand != null ? gmcpCommand.Auth : GetInt("gmcp_min_auth", 50);

            PerfectPlay = GetBool("perfect_play", false);
            InfiniteResources = GetBool("infinite_resources", false);
            PerfectFishing = GetBool("perfect_fishing", false);
            InfiniteBait = GetBool("infinite_bait", false);
            InfiniteArrows = GetBool("infinite_arrows", false);
            SharpMindChance = GetFloat("sharp_mind_chance", 50);
            SafeEnchanting = GetBool("safe_enchanting", false);
            PaladinDurationRate = GetFloat("paladin_duration_rate", 100) / 100.0f;
            PaladinCancelOnDeath = GetBool("paladin_cancel_on_death", true);

            Bagception = GetBool("bagception", false);
            NoDurabilityLoss = GetBool("no_durability_loss", false);
            NoDecay = GetBool("no_decay", false);
            UnlimitedUpgrades = GetBool("unlimited_upgrades", false);
            UncapProficiency = GetBool("uncap_proficiency", false);
            UnlimitedDyes = GetBool("unlimited_dyes", false);
            DyeDifficulty = Math2.Clamp(1, 5, GetInt("dye_difficulty", 5));
            BrokenEggs = GetBool("broken_eggs", true);
            SwitchCancelBolts = GetBool("switch_cancel_bolts", true);
            ProficiencyRate = GetFloat("proficiency_rate", 100);
            GlobalBank = GetBool("global_bank", true);
            ReusingPersonalShopLicenses = GetBool("reusing_personal_shop_licenses", false);
            MaxExtraSets = GetInt("max_extra_sets", 3);

            RebirthTime = TimeSpan.FromDays(GetInt("rebirth_time", 6));

            BankGoldPerCharacter = GetInt("gold_per_character", 5000000);

            PtjInfiniteMemory = GetBool("ptj_infinite_memory", false);

            PrivateDungeons = GetBool("private_dungeons", false);
            EasySwitch = GetBool("easy_switch", false);
            RandomFloors = GetBool("random_floors", false);

            PartyExpBonus = GetFloat("party_exp_bonus", 0);
            PartyMaxSize = Math2.Clamp(1, 99, GetInt("party_max_size", 8));
            PartyQuestMinSize = Math2.Clamp(1, PartyMaxSize, GetInt("party_quest_min_size", 2));

            GoldQuestRewardRate = GetFloat("gold_quest_reward_rate", 100) / 100.0f;
        }
    }
}