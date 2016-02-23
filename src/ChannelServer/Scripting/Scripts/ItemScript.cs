// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.World.Entities;
using Aura.Channel.World.Entities.Creatures;
using Aura.Data;
using Aura.Mabi;
using Aura.Mabi.Const;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Aura.Channel.Scripting.Scripts
{
	/// <summary>
	/// Item script base
	/// </summary>
	/// <remarks>
	/// Stat updates are done automatically, after running the scripts.
	/// </remarks>
	public abstract class ItemScript : GeneralScript
	{
		private const float WeightChangePlus = 0.0015f;
		private const float WeightChangeMinus = 0.000375f;

		/// <summary>
		/// Called when script is initialized after loading it.
		/// </summary>
		/// <returns></returns>
		public override bool Init()
		{
			var attr = this.GetType().GetCustomAttribute<ItemScriptAttribute>();
			if (attr == null)
			{
				Log.Error("ItemScript.Init: Missing ItemScript attribute.");
				return false;
			}

			foreach (var itemId in attr.ItemIds)
				ChannelServer.Instance.ScriptManager.ItemScripts.Add(itemId, this);

			return true;
		}

		/// <summary>
		/// Executed when item is used.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="item"></param>
		public virtual void OnUse(Creature creature, Item item)
		{ }

		/// <summary>
		/// Executed when item is equipped.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="item"></param>
		public virtual void OnEquip(Creature creature, Item item)
		{ }

		/// <summary>
		/// Executed when item is unequipped.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="item"></param>
		public virtual void OnUnequip(Creature creature, Item item)
		{ }

		/// <summary>
		/// Executed when item is first created.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="item"></param>
		public virtual void OnCreation(Item item)
		{ }

		// Functions
		// ------------------------------------------------------------------

		/// <summary>
		/// Heals a certain amount of life, mana, and stamina.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="life"></param>
		/// <param name="mana"></param>
		/// <param name="stamina"></param>
		protected void Heal(Creature creature, double life, double mana, double stamina)
		{
			creature.Life += (float)life;
			creature.Mana += (float)mana;
			creature.Stamina += (float)stamina * creature.StaminaRegenMultiplicator;
		}

		/// <summary>
		/// Heals a certain percentage of life, mana, and stamina.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="life"></param>
		/// <param name="mana"></param>
		/// <param name="stamina"></param>
		protected void HealRate(Creature creature, double life, double mana, double stamina)
		{
			if (life != 0)
				creature.Life += (float)(creature.LifeMax / 100f * life);
			if (mana != 0)
				creature.Mana += (float)(creature.ManaMax / 100f * life);
			if (stamina != 0)
				creature.Stamina += (float)(creature.StaminaMax / 100f * life);
		}

		/// <summary>
		/// Heals life, mana, and stamina completely.
		/// </summary>
		/// <param name="creature"></param>
		protected void HealFull(Creature creature)
		{
			creature.Injuries = 0;
			creature.Hunger = 0;
			this.HealRate(creature, 100, 100, 100);
		}

		/// <summary>
		/// Adds to pot poisoning.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="foodPoison"></param>
		protected void Poison(Creature creature, double foodPoison)
		{
			//creature.X += (float)foodPoison;
		}

		/// <summary>
		/// Reduces hunger by amount and handles weight gain/loss
		/// and stat bonuses.
		/// </summary>
		/// <remarks>
		/// Body and stat changes are applied inside Creature,
		/// on MabiTick (every 5 minutes).
		/// </remarks>
		protected void Feed(Creature creature, double hunger, double weight = 0, double upper = 0, double lower = 0, double str = 0, double int_ = 0, double dex = 0, double will = 0, double luck = 0, double life = 0, double mana = 0, double stm = 0)
		{
			// Hunger
			var diff = creature.Hunger;
			creature.Hunger -= (float)hunger;
			diff -= creature.Hunger;

			// Weight (multiplicators guessed, based on packets)
			// Only increase weight if you eat above 0% Hunger?
			if (diff < hunger)
			{
				creature.Temp.WeightFoodChange += (float)weight * (weight >= 0 ? WeightChangePlus : WeightChangeMinus);
				creature.Temp.UpperFoodChange += (float)upper * (upper >= 0 ? WeightChangePlus : WeightChangeMinus);
				creature.Temp.LowerFoodChange += (float)lower * (lower >= 0 ? WeightChangePlus : WeightChangeMinus);
			}

			// Stats
			creature.Temp.StrFoodChange += MabiMath.FoodStatBonus(str, hunger, diff, creature.Age);
			creature.Temp.IntFoodChange += MabiMath.FoodStatBonus(int_, hunger, diff, creature.Age);
			creature.Temp.DexFoodChange += MabiMath.FoodStatBonus(dex, hunger, diff, creature.Age);
			creature.Temp.WillFoodChange += MabiMath.FoodStatBonus(will, hunger, diff, creature.Age);
			creature.Temp.LuckFoodChange += MabiMath.FoodStatBonus(luck, hunger, diff, creature.Age);
		}

		/// <summary>
		/// Reduces injuries by amount.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="injuries"></param>
		protected void Treat(Creature creature, double injuries)
		{
			creature.Injuries -= (float)injuries;
		}

		/// <summary>
		/// Adds gesture by keyword.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="keyword"></param>
		/// <param name="name"></param>
		protected void AddGesture(Creature creature, string keyword, string name)
		{
			creature.Keywords.Give(keyword);
			Send.Notice(creature, Localization.Get("The {0} Gesture has been added. Check your gestures window."), name);
		}

		/// <summary>
		/// Adds magic seal meta data to item.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="color"></param>
		/// <param name="script"></param>
		protected void MagicSeal(Item item, string color, string script = null)
		{
			item.MetaData1.SetString("MGCSEL", color);
			if (script != null)
				item.MetaData1.SetString("MGCWRD", script);
		}

		/// <summary>
		/// Trains the specified condition for skill by one.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skillId"></param>
		/// <param name="condition"></param>
		protected void TrainSkill(Creature creature, SkillId skillId, int condition)
		{
			var skill = creature.Skills.Get(skillId);
			if (skill == null)
				return;

			skill.Train(condition);
		}

		/// <summary>
		/// Activates the sticker for the given duration.
		/// </summary>
		/// <param name="sticker">Sticker to activate.</param>
		/// <param name="duration">Duration in seconds.</param>
		protected void ActivateChatSticker(Creature creature, ChatSticker sticker, int duration)
		{
			var end = DateTime.Now.AddSeconds(duration);

			creature.Vars.Perm["ChatStickerId"] = (int)sticker;
			creature.Vars.Perm["ChatStickerEnd"] = end;

			Send.ChatSticker(creature, sticker, end);
			Send.Notice(creature, Localization.Get("You carefully attach the sticker to your Chat Bubble."));
		}

		/// <summary>
		/// Activates food stat mods for the given timeout and stats.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="item"></param>
		/// <param name="timeout"></param>
		/// <param name="str"></param>
		/// <param name="int_"></param>
		/// <param name="dex"></param>
		/// <param name="will"></param>
		/// <param name="luck"></param>
		/// <param name="life"></param>
		/// <param name="mana"></param>
		/// <param name="stamina"></param>
		/// <param name="lifeRecovery"></param>
		/// <param name="manaRecovery"></param>
		/// <param name="staminaRecovery"></param>
		/// <param name="injuryRecovery"></param>
		/// <param name="defense"></param>
		/// <param name="protection"></param>
		protected void Buff(Creature creature, Item item, int timeout, double str = 0, double int_ = 0, double dex = 0, double will = 0, double luck = 0, double life = 0, double mana = 0, double stamina = 0, double lifeRecovery = 0, double manaRecovery = 0, double staminaRecovery = 0, double injuryRecovery = 0, int defense = 0, int protection = 0)
		{
			var source = StatModSource.Food;
			var ident = item.EntityId;
			var group = "Food_" + ident;

			// Prepare quality value for the calculations
			// Bonus Formula: floor(dbValue / 200 * (quality + 100))
			var quality = 0;
			if (item.MetaData1.Has("QUAL"))
				quality = item.MetaData1.GetInt("QUAL");
			quality += 100;

			// Before applying the mods/regens, we have to make sure they
			// aren't set yet, to prevent stacking. This would be a job for
			// a cooldown system, which we don't have yet =< TODO.

			if (!creature.StatMods.Has(source, ident))
			{
				if (str != 0)
					creature.StatMods.Add(Stat.StrMod, (float)Math.Floor(str / 200f * quality), source, ident, timeout);

				if (int_ != 0)
					creature.StatMods.Add(Stat.IntMod, (float)Math.Floor(int_ / 200f * quality), source, ident, timeout);

				if (dex != 0)
					creature.StatMods.Add(Stat.DexMod, (float)Math.Floor(dex / 200f * quality), source, ident, timeout);

				if (will != 0)
					creature.StatMods.Add(Stat.WillMod, (float)Math.Floor(will / 200f * quality), source, ident, timeout);

				if (luck != 0)
					creature.StatMods.Add(Stat.LuckMod, (float)Math.Floor(luck / 200f * quality), source, ident, timeout);

				if (defense != 0)
					creature.StatMods.Add(Stat.DefenseMod, (float)Math.Floor(defense / 200f * quality), source, ident, timeout);

				if (protection != 0)
					creature.StatMods.Add(Stat.ProtectionMod, (float)Math.Floor(protection / 200f * quality), source, ident, timeout);

				if (life != 0)
				{
					creature.StatMods.Add(Stat.LifeMaxMod, (float)Math.Floor(life / 200f * quality), source, ident, timeout);
					creature.Life = creature.Life; // Cap in case max was reduced
				}

				if (mana != 0)
				{
					creature.StatMods.Add(Stat.ManaMaxMod, (float)Math.Floor(mana / 200f * quality), source, ident, timeout);
					creature.Mana = creature.Mana; // Cap in case max was reduced
				}

				if (stamina != 0)
				{
					creature.StatMods.Add(Stat.StaminaMaxMod, (float)Math.Floor(stamina / 200f * quality), source, ident, timeout);
					creature.Stamina = creature.Stamina; // Cap in case max was reduced
				}
			}

			if (!creature.Regens.Has(group))
			{
				if (lifeRecovery != 0)
					creature.Regens.Add(group, Stat.Life, (float)Math.Floor(lifeRecovery / 200f * quality), creature.LifeInjured, timeout);

				if (manaRecovery != 0)
					creature.Regens.Add(group, Stat.Mana, (float)Math.Floor(manaRecovery / 200f * quality), creature.ManaMax, timeout);

				if (staminaRecovery != 0)
					creature.Regens.Add(group, Stat.Stamina, (float)Math.Floor(staminaRecovery / 200f * quality), creature.StaminaMax, timeout);

				if (injuryRecovery != 0)
					creature.Regens.Add(group, Stat.LifeInjured, (float)Math.Floor(injuryRecovery / 200f * quality), creature.LifeMax, timeout);
			}

			Send.StatUpdate(creature, StatUpdateType.Private,
				Stat.StrMod, Stat.IntMod, Stat.DexMod, Stat.WillMod, Stat.LuckMod,
				Stat.Life, Stat.LifeInjured, Stat.LifeMaxMod, Stat.LifeMax,
				Stat.Mana, Stat.ManaMaxMod, Stat.ManaMax,
				Stat.Stamina, Stat.StaminaMaxMod, Stat.StaminaMax,
				Stat.DefenseMod, Stat.ProtectionMod
			);
			Send.StatUpdate(creature, StatUpdateType.Public, Stat.Life, Stat.LifeInjured, Stat.LifeMaxMod, Stat.LifeMax);
		}
	}

	/// <summary>
	/// Attribute for item scripts, to specify which items the script is for.
	/// </summary>
	/// <remarks>
	/// Takes lists of item ids or tags. If a list of tags is passed the item
	/// db will be searched for item ids that match *any* of the tags.
	/// </remarks>
	public class ItemScriptAttribute : Attribute
	{
		/// <summary>
		/// List of item ids
		/// </summary>
		public int[] ItemIds { get; private set; }

		/// <summary>
		/// New attribute based on ids
		/// </summary>
		/// <param name="itemIds"></param>
		public ItemScriptAttribute(params int[] itemIds)
		{
			this.ItemIds = itemIds;
		}

		/// <summary>
		/// New attribute based on tags
		/// </summary>
		/// <param name="tags"></param>
		public ItemScriptAttribute(params string[] tags)
		{
			var ids = new HashSet<int>();

			foreach (var tag in tags)
			{
				foreach (var itemData in AuraData.ItemDb.Entries.Values.Where(a => a.HasTag(tag)))
					ids.Add(itemData.Id);
			}

			this.ItemIds = ids.ToArray();
		}
	}
}
