// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.World;
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
using System.Threading;
using System.Threading.Tasks;

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

		private static int _fireworkSeed;

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
		public virtual void OnUse(Creature creature, Item item, string parameter)
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
		protected void Heal(Creature creature, double life, double mana, double stamina, double toxicity)
		{
			// Potion poisoning heal mount increase
			// http://wiki.mabinogiworld.com/view/Potion_Poisoning#Stages_of_Potion_Poisoning
			var multiplier = 1.0;
			var toxicityStage = GetToxicityStage(creature.Toxic);
			if (toxicityStage != ToxicityStage.Normal)
			{
				var rnd = RandomProvider.Get();
				switch (toxicityStage)
				{
					case ToxicityStage.Stage1: multiplier = 1.0 + rnd.NextDouble() * 0.3; break;
					case ToxicityStage.Stage2: multiplier = 1.3; break;
					case ToxicityStage.Stage3: multiplier = 1.3 + rnd.NextDouble() * 0.3; break;
					case ToxicityStage.Stage4: multiplier = 1.6; break;
					case ToxicityStage.Stage5: multiplier = 1.6 + rnd.NextDouble() * 0.4; break;
					case ToxicityStage.Stage6: multiplier = 2.0 + rnd.NextDouble() * 1.0; break;
				}

				life *= multiplier;
				mana *= multiplier;
				stamina *= multiplier;
			}

			// Friday: All potions become more potent. (Potion effect x 1.5 including toxicity).
			// +50%? Seems a lot, but that's what the Wiki says.
			if (ErinnTime.Now.Month == ErinnMonth.AlbanElved)
			{
				life *= 1.5;
				mana *= 1.5;
				stamina *= 1.5;
			}

			var beforeLife = creature.Life;
			var beforeMana = creature.Mana;
			var beforeStamina = creature.Stamina;

			creature.Life += (float)life;
			creature.Mana += (float)mana;
			creature.Stamina += (float)stamina * creature.StaminaRegenMultiplicator;

			var diffLife = creature.Life - beforeLife;
			var diffMana = creature.Mana - beforeMana;
			var diffStamina = creature.Stamina - beforeStamina;

			this.Poison(creature, diffLife, diffMana, diffStamina, toxicity);
		}

		/// <summary>
		/// Heals a certain percentage of life, mana, and stamina.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="life"></param>
		/// <param name="mana"></param>
		/// <param name="stamina"></param>
		protected void HealRate(Creature creature, double life, double mana, double stamina, double toxicity)
		{
			life = creature.LifeMax / 100f * life;
			mana = creature.ManaMax / 100f * mana;
			stamina = creature.StaminaMax / 100f * stamina;

			this.Heal(creature, life, mana, stamina, toxicity);
		}

		/// <summary>
		/// Heals life, mana, and stamina completely.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="toxicity"></param>
		protected void HealFull(Creature creature, double toxicity)
		{
			creature.Injuries = 0;
			creature.Hunger = 0;
			this.HealRate(creature, 100, 100, 100, toxicity);
		}

		/// <summary>
		/// Handles potion poisoning.
		/// </summary>
		/// <remarks>
		/// The stages and the toxicity in general are based on information
		/// from the Wiki and in-game obsvervation.
		/// </remarks>
		/// <param name="life">Amount of life healed.</param>
		/// <param name="mana">Amount of mana healed.</param>
		/// <param name="stamina">Amount of stamina healed.</param>
		/// <param name="toxicity">Toxicity to apply.</param>
		private void Poison(Creature creature, double life, double mana, double stamina, double toxicity)
		{
			var beforeStage = GetToxicityStage(creature.Toxic);

			// Increase toxicity based on healed points
			creature.Toxic -= (float)(life * toxicity);
			creature.Toxic -= (float)(mana * toxicity);
			creature.Toxic -= (float)(stamina * toxicity);

			var stage = GetToxicityStage(creature.Toxic);

			// Stage change messages
			// http://wiki.mabinogiworld.com/view/Potion_Poisoning#Stages_of_Potion_Poisoning
			if (stage != beforeStage)
			{
				switch (stage)
				{
					case ToxicityStage.Stage1: Send.Notice(creature, L("It feels like the potion works better than before.")); break;
					case ToxicityStage.Stage2: Send.Notice(creature, L("This potion is effective!")); break;
					case ToxicityStage.Stage3: Send.Notice(creature, L("This potion is clearly effective, but it feels weird somehow.")); break;
					case ToxicityStage.Stage4: Send.Notice(creature, L("The potion has some side effects.")); break;
					case ToxicityStage.Stage5: Send.Notice(creature, L("The potion worked as it should, but it had some bad side-effects too.")); break;
					case ToxicityStage.Stage6: Send.Notice(creature, L("Anymore of this potion is dangerous!")); break;
				}
			}

			// Decrease stats if toxicity becomes too great
			if (stage >= ToxicityStage.Stage1)
			{
				if (life > 0)
				{
					creature.ToxicInt -= 1;
					creature.ToxicWill -= 1;
				}

				if (mana > 0)
				{
					creature.ToxicStr -= 1;
				}

				if (stamina > 0)
				{
					creature.ToxicWill -= 1;
				}
			}
		}

		/// <summary>
		/// Returns toxicity stage based on given toxicity.
		/// </summary>
		/// <param name="toxicity"></param>
		/// <returns></returns>
		private static ToxicityStage GetToxicityStage(double toxicity)
		{
			if (toxicity < -210)
				return ToxicityStage.Stage6;
			else if (toxicity < -175)
				return ToxicityStage.Stage5;
			else if (toxicity < -140)
				return ToxicityStage.Stage4;
			else if (toxicity < -105)
				return ToxicityStage.Stage3;
			else if (toxicity < -70)
				return ToxicityStage.Stage2;
			else if (toxicity < -35)
				return ToxicityStage.Stage1;
			else
				return ToxicityStage.Normal;
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
			// Saturday: Food effects are increased. (2x weight, hunger; effects are long term)
			// +100%? Seems a lot, but that's what the Wiki says.
			if (ErinnTime.Now.Month == ErinnMonth.Samhain)
			{
				hunger *= 2;
				weight *= 2;
				upper *= 2;
				lower *= 2;
			}

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
		protected void Treat(Creature creature, double injuries, double toxicity)
		{
			// Friday: All potions become more potent. (Potion effect x 1.5 including toxicity).
			// +50%? Seems a lot, but that's what the Wiki says.
			if (ErinnTime.Now.Month == ErinnMonth.AlbanElved)
				toxicity *= 1.5;

			var beforeInjuries = creature.Injuries;
			creature.Injuries -= (float)injuries;
			var diffInjuries = beforeInjuries - creature.Injuries;

			this.Poison(creature, diffInjuries, 0, 0, toxicity);
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
			var statModSource = StatModSource.TmpFood;
			var statModIdent = 0;
			var regenGroup = "TmpFoodRegen";

			// Prepare quality value for the calculations
			// Bonus Formula: floor(dbValue / 200 * (quality + 100))
			var quality = 0;
			if (item.MetaData1.Has("QUAL"))
				quality = item.MetaData1.GetInt("QUAL");
			quality += 100;

			// Remove previous buffs
			creature.StatMods.Remove(statModSource, statModIdent);
			creature.Regens.Remove(regenGroup);

			// Add stat buffs
			if (str != 0)
				creature.StatMods.Add(Stat.StrMod, (float)Math.Floor(str / 200f * quality), statModSource, statModIdent, timeout);

			if (int_ != 0)
				creature.StatMods.Add(Stat.IntMod, (float)Math.Floor(int_ / 200f * quality), statModSource, statModIdent, timeout);

			if (dex != 0)
				creature.StatMods.Add(Stat.DexMod, (float)Math.Floor(dex / 200f * quality), statModSource, statModIdent, timeout);

			if (will != 0)
				creature.StatMods.Add(Stat.WillMod, (float)Math.Floor(will / 200f * quality), statModSource, statModIdent, timeout);

			if (luck != 0)
				creature.StatMods.Add(Stat.LuckMod, (float)Math.Floor(luck / 200f * quality), statModSource, statModIdent, timeout);

			if (defense != 0)
				creature.StatMods.Add(Stat.DefenseMod, (float)Math.Floor(defense / 200f * quality), statModSource, statModIdent, timeout);

			if (protection != 0)
				creature.StatMods.Add(Stat.ProtectionMod, (float)Math.Floor(protection / 200f * quality), statModSource, statModIdent, timeout);

			if (life != 0)
			{
				creature.StatMods.Add(Stat.LifeMaxMod, (float)Math.Floor(life / 200f * quality), statModSource, statModIdent, timeout);
				creature.Life = creature.Life; // Cap in case max was reduced
			}

			if (mana != 0)
			{
				creature.StatMods.Add(Stat.ManaMaxMod, (float)Math.Floor(mana / 200f * quality), statModSource, statModIdent, timeout);
				creature.Mana = creature.Mana; // Cap in case max was reduced
			}

			if (stamina != 0)
			{
				creature.StatMods.Add(Stat.StaminaMaxMod, (float)Math.Floor(stamina / 200f * quality), statModSource, statModIdent, timeout);
				creature.Stamina = creature.Stamina; // Cap in case max was reduced
			}

			// Add regens
			if (lifeRecovery != 0)
				creature.Regens.Add(regenGroup, Stat.Life, (float)Math.Floor(lifeRecovery / 200f * quality), creature.LifeInjured, timeout);

			if (manaRecovery != 0)
				creature.Regens.Add(regenGroup, Stat.Mana, (float)Math.Floor(manaRecovery / 200f * quality), creature.ManaMax, timeout);

			if (staminaRecovery != 0)
				creature.Regens.Add(regenGroup, Stat.Stamina, (float)Math.Floor(staminaRecovery / 200f * quality), creature.StaminaMax, timeout);

			if (injuryRecovery != 0)
				creature.Regens.Add(regenGroup, Stat.LifeInjured, (float)Math.Floor(injuryRecovery / 200f * quality), creature.LifeMax, timeout);

			// Update client
			Send.StatUpdate(creature, StatUpdateType.Private,
				Stat.StrMod, Stat.IntMod, Stat.DexMod, Stat.WillMod, Stat.LuckMod,
				Stat.Life, Stat.LifeInjured, Stat.LifeMaxMod, Stat.LifeMax,
				Stat.Mana, Stat.ManaMaxMod, Stat.ManaMax,
				Stat.Stamina, Stat.StaminaMaxMod, Stat.StaminaMax,
				Stat.DefenseMod, Stat.ProtectionMod
			);
			Send.StatUpdate(creature, StatUpdateType.Public, Stat.Life, Stat.LifeInjured, Stat.LifeMaxMod, Stat.LifeMax);
		}

		protected void ShootFirework(Location location, FireworkType type, string message)
		{
			var region = ChannelServer.Instance.World.GetRegion(location.RegionId);
			if (region == null)
			{
				Log.Warning(this.GetType().Name + ".ShootFirework: Unknown region.");
				return;
			}

			if (message == null)
				message = "";

			var delay = 500;
			var rnd = RandomProvider.Get();
			var height = rnd.Between(750, 2000);
			var heightf = height / 100f;

			var prop = new Prop(208, location.RegionId, location.X, location.Y, 0);
			prop.DisappearTime = DateTime.Now.AddMilliseconds(20000 + delay);
			region.AddProp(prop);

			Task.Delay(delay).ContinueWith(__ =>
			{
				prop.Xml.SetAttributeValue("height", height);
				prop.Xml.SetAttributeValue("message", message + " (" + heightf.ToString("0.##") + "m)");
				prop.Xml.SetAttributeValue("type", (int)type);
				prop.Xml.SetAttributeValue("seed", Interlocked.Increment(ref _fireworkSeed));
				Send.PropUpdate(prop);
			});
		}

		private enum ToxicityStage
		{
			Normal,
			Stage1,
			Stage2,
			Stage3,
			Stage4,
			Stage5,
			Stage6
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
