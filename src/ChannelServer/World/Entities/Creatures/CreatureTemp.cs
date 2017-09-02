// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using Aura.Channel.Scripting.Scripts;
using Aura.Channel.Skills.Life;
using Aura.Channel.World.Shops;
using Aura.Data.Database;
using Aura.Mabi.Const;
using Aura.Mabi.Structs;

namespace Aura.Channel.World.Entities.Creatures
{
    public class CreatureTemp
    {
        public Entrustment ActiveEntrustment;
        public PersonalShop ActivePersonalShop;

        public Trade ActiveTrade;
        public List<BlacksmithDot> BlacksmithingMiniGameDots;
        public long CampfireKitItemEntityId;
        public CatchSize CatchSize;
        public int ColorWheelResult;

        public List<Ingredient> CookingIngredients;
        public string CookingMethod;

        public object CounterSyncLock = new object();
        public int CreationFinishId;

        public string CurrentBankId;

        public string CurrentBankTitle;

        // Sitting
        public ChairData CurrentChairData;

        // Currently playing cutscene
        public Cutscene CurrentCutscene;

        // Last open shop
        public NpcShopScript CurrentShop;

        public NPC CurrentShopOwner;
        public int DyeDistortA1, DyeDistortA2, DyeDistortA3, DyeDistortA4;

        // Excalibur charge time
        public DateTime ExcaliburPrepareTime;

        // Final Hit training counters
        public int FinalHitKillCount, FinalHitKillCountStrong, FinalHitKillCountAwful, FinalHitKillCountBoss;

        public bool FireArrow;

        public int FirewoodItemId;
        public bool FishingActionRequested;
        public DropData FishingDrop;

        public Prop FishingProp;

        // Backup of target's position when gathering, for run away check
        public Position GatheringTargetPosition;

        // True while visiting Nao
        public bool InSoulStream;

        public float LifeFoodChange, ManaFoodChange, StaminaFoodChange;

        // Lightning Rod variables
        public bool LightningRodFullCharge;

        public DateTime LightningRodPrepareTime;

        public long NameColorItemEntityId;

        public PlayingInstrumentProp PlayingInstrumentProp;

        // Random dyeing cursors for regular dyes
        public DyePickers RegularDyePickers;

        public Prop SittingProp;

        // Items temporarily used by skills
        public Item SkillItem1, SkillItem2;

        public float StrFoodChange, IntFoodChange, DexFoodChange, WillFoodChange, LuckFoodChange;

        public int TailoringMiniGameX, TailoringMiniGameY;

        // Food cache
        public float WeightFoodChange, UpperFoodChange, LowerFoodChange;
    }
}