// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using Aura.Channel.Scripting.Scripts;
using Aura.Data.Database;
using Aura.Mabi.Structs;
using System.Collections.Generic;
using Aura.Channel.Skills.Life;
using Aura.Mabi.Const;
using Aura.Channel.World.Shops;

namespace Aura.Channel.World.Entities.Creatures
{
	public class CreatureTemp
	{
		// Sitting
		public ChairData CurrentChairData;
		public Prop SittingProp;

		// Food cache
		public float WeightFoodChange, UpperFoodChange, LowerFoodChange;
		public float LifeFoodChange, ManaFoodChange, StaminaFoodChange;
		public float StrFoodChange, IntFoodChange, DexFoodChange, WillFoodChange, LuckFoodChange;

		// True while visiting Nao
		public bool InSoulStream;

		// Currently playing cutscene
		public Cutscene CurrentCutscene;

		// Last open shop
		public NpcShopScript CurrentShop;
		public NPC CurrentShopOwner;

		// Items temporarily used by skills
		public Item SkillItem1, SkillItem2;

		// Random dyeing cursors for regular dyes
		public DyePickers RegularDyePickers;
		public int DyeDistortA1, DyeDistortA2, DyeDistortA3, DyeDistortA4;

		// Final Hit training counters
		public int FinalHitKillCount, FinalHitKillCountStrong, FinalHitKillCountAwful, FinalHitKillCountBoss;

		// Lightning Rod variables
		public bool LightningRodFullCharge;
		public DateTime LightningRodPrepareTime;

		// Backup of target's position when gathering, for run away check
		public Position GatheringTargetPosition;

		public int FirewoodItemId;
		public long CampfireKitItemEntityId;

		public Prop FishingProp;
		public bool FishingActionRequested;
		public DropData FishingDrop;
		public CatchSize CatchSize;

		public bool FireArrow;

		public long NameColorItemEntityId;
		public int ColorWheelResult;

		public int TailoringMiniGameX, TailoringMiniGameY;
		public List<BlacksmithDot> BlacksmithingMiniGameDots;
		public int CreationFinishId;

		public List<Ingredient> CookingIngredients;
		public string CookingMethod;

		public object CounterSyncLock = new object();

		public string CurrentBankId;
		public string CurrentBankTitle;

		public Trade ActiveTrade;
		public Entrustment ActiveEntrustment;
		public PersonalShop ActivePersonalShop;
	}
}
