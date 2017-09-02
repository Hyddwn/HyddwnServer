using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.World.Entities;
using Aura.Channel.World.Entities.Creatures;
using Aura.Data;
using Aura.Mabi;
using Aura.Mabi.Const;
using Aura.Shared.Util;
using System;

namespace Aura.Channel.Skills.Transformations
{
	/// <summary>
	/// Handler for Paladin transformation.
	/// </summary>
	/// <remarks>
	/// Spirit of Order
	///   Var1: Max HP
	///   Var2: Max MP
	///   Var3: Max Stamina
	///   Var4: Defense
	///   Var5: Protection
	///   Var6: ?
	///   Var7: Passive Defense Activation Bonus (%)
	///   Var8: Magic Defense
	///   Var9: Magic Protection
	///   Var20: Duration
	///   IntVar1: Transformation Level
	///   IntVar2: Transformation Title
	///   
	/// Power of Order
	///   Var1: STR
	///   Var2: Will
	///   Var3: ?
	///   
	/// Eye of Order
	///   Var1: DEX
	///   Var2: Balance
	///   Var3: ?
	/// 
	/// Sword of Order
	///   Var1: Damage
	///   Var2: Injury Rate
	///   Var3: ?
	///   Var4: ?
	///   Var5: ?
	///   
	/// Pal. Natural Shield
	///   Var1: ?
	///   Var2: ?
	///   Var3: ?
	///   Var4: Bonus Activation Rate
	///   
	/// Pal. Heavy Stander
	///   Var1: ?
	///   Var2: ?
	///   Var3: ?
	///   Var4: Bonus Activation Rate
	///   
	/// Pal. Mana Deflector
	///   Var1: ?
	///   Var2: ?
	///   Var3: ?
	///   Var4: Bonus Activation Rate
	/// </remarks>
	[Skill(SkillId.SpiritOfOrder)]
	public class SpiritOfOrderHandler : StartStopSkillHandler, IInitiableSkillHandler
	{
		/// <summary>
		/// Delay in ms until the passive defense effects are displayed.
		/// </summary>
		private const int EffectBaseDelay = 6500;

		/// <summary>
		/// Delay in ms between the passive defense effects.
		/// </summary>
		private const int EffectAddDelay = 1500;

		/// <summary>
		/// Amount of skill training EXP a player gets per level up.
		/// </summary>
		private const int ExpPerLevelUp = 10;

		/// <summary>
		/// Called when the skill handler is loaded, sets up training
		/// event subscriptions.
		/// </summary>
		public void Init()
		{
			ChannelServer.Instance.Events.CreatureLevelUp += this.OnCreatureLevelUp;
		}

		/// <summary>
		/// Transforms creature.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="dict"></param>
		/// <returns></returns>
		public override StartStopResult Start(Creature creature, Skill skill, MabiDictionary dict)
		{
			if (skill.IsCoolingDown)
			{
				creature.Notice(Localization.Get("You can't use this for the rest of the day."));
				return StartStopResult.Fail;
			}

			creature.StopMove();

			this.Transform(creature, skill);
			this.GiveBonuses(creature, skill);
			this.SetTimers(creature, skill);

			return StartStopResult.Okay;
		}

		/// <summary>
		/// Stops creature's transformation.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="dict"></param>
		/// <returns></returns>
		public override StartStopResult Stop(Creature creature, Skill skill, MabiDictionary dict)
		{
			creature.StopMove();

			this.RemoveBonuses(creature, skill);
			this.ResetTransformation(creature, skill);

			return StartStopResult.Okay;
		}

		/// <summary>
		/// Transforms creature and updates clients.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		private void Transform(Creature creature, Skill skill)
		{
			creature.Transformation = Transformation.Paladin;
			creature.TransformationSkillRank = skill.Info.Rank;
			creature.TransformationLevel = (TransformationLevel)skill.RankData.IntVar1;
			creature.Titles.SetTempTitle((ushort)skill.RankData.IntVar2);

			Send.UpdateTransformation(creature);
		}

		/// <summary>
		/// Returns timespan after which the transformation ends.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		private TimeSpan GetDuration(Creature creature, Skill skill)
		{
			var rebirthCount = creature.RebirthCount;
			var age = Math.Min((int)creature.Age, 50);

			var modifier1 = 240 * rebirthCount;
			var modifier2 = 0;

			if (rebirthCount <= 10)
				modifier2 = modifier1 / 30 + 240;
			else if (rebirthCount <= 100)
				modifier2 = modifier1 / 180 + 320;
			else
				modifier2 = modifier1 / 5400 + 450;

			var result = 0;
			result += 1000 * modifier2;
			result -= 1000 * age * (modifier2 - 160) / (rebirthCount + 50);
			result = Math.Max(0, result);

			return TimeSpan.FromMilliseconds(result);
		}

		/// <summary>
		/// Sets skill's auto cancel time and cool down.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		private void SetTimers(Creature creature, Skill skill)
		{
			var duration = this.GetDuration(creature, skill);
			duration = TimeSpan.FromMilliseconds(Math.Max(1000, duration.TotalMilliseconds * ChannelServer.Instance.Conf.World.PaladinDurationRate));

			creature.Skills.CancelAfter(skill.Info.Id, duration);
			skill.SetCoolDownEnd(ErinnTime.GetNextTime(6, 0).DateTime);

			creature.Death += this.OnDeath;
		}

		/// <summary>
		/// Called when the transformed creature is killed.
		/// </summary>
		/// <param name="creature"></param>
		private void OnDeath(Creature creature, Creature killer)
		{
			creature.Death -= this.OnDeath;

			var skill = creature.Skills.Get(SkillId.SpiritOfOrder);
			if (skill == null)
				return;

			this.Stop(creature, skill);
		}

		/// <summary>
		/// Gives bonuses for transformation based on skill's rank and
		/// the passive transformation skill's ranks, heals creature,
		/// and updates clients.
		/// The mod identifiers are "Skill" and the transformation skill's id.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		private void GiveBonuses(Creature creature, Skill skill)
		{
			var rnd = RandomProvider.Get();

			var powerOfOrder = creature.Skills.Get(SkillId.PowerOfOrder);
			var eyeOfOrder = creature.Skills.Get(SkillId.EyeOfOrder);
			var swordOfOrder = creature.Skills.Get(SkillId.SwordOfOrder);
			var naturalShield = creature.Skills.Get(SkillId.PaladinNaturalShield);
			var heavyStander = creature.Skills.Get(SkillId.PaladinHeavyStander);
			var manaDeflector = creature.Skills.Get(SkillId.PaladinManaDeflector);

			// Spirit of Order
			creature.StatMods.Add(Stat.LifeMaxMod, skill.RankData.Var1, StatModSource.Skill, (long)skill.Info.Id);
			creature.StatMods.Add(Stat.ManaMaxMod, skill.RankData.Var2, StatModSource.Skill, (long)skill.Info.Id);
			creature.StatMods.Add(Stat.StaminaMaxMod, skill.RankData.Var3, StatModSource.Skill, (long)skill.Info.Id);
			creature.StatMods.Add(Stat.DefenseMod, skill.RankData.Var4, StatModSource.Skill, (long)skill.Info.Id);
			creature.StatMods.Add(Stat.ProtectionMod, skill.RankData.Var5, StatModSource.Skill, (long)skill.Info.Id);

			if (AuraData.FeaturesDb.IsEnabled("PaladinMagicDefense"))
			{
				creature.StatMods.Add(Stat.MagicDefenseMod, skill.RankData.Var8, StatModSource.Skill, (long)skill.Info.Id);
				creature.StatMods.Add(Stat.MagicProtectionMod, skill.RankData.Var9, StatModSource.Skill, (long)skill.Info.Id);
			}

			// Power of Order
			if (powerOfOrder != null)
			{
				powerOfOrder.Enabled = true;
				creature.StatMods.Add(Stat.StrMod, powerOfOrder.RankData.Var1, StatModSource.Skill, (long)skill.Info.Id);
				creature.StatMods.Add(Stat.WillMod, powerOfOrder.RankData.Var2, StatModSource.Skill, (long)skill.Info.Id);
			}

			// Eye of Order
			if (eyeOfOrder != null)
			{
				eyeOfOrder.Enabled = true;
				creature.StatMods.Add(Stat.DexMod, powerOfOrder.RankData.Var1, StatModSource.Skill, (long)skill.Info.Id);
				creature.StatMods.Add(Stat.BalanceMod, powerOfOrder.RankData.Var2, StatModSource.Skill, (long)skill.Info.Id);
			}

			// Sword of Order
			if (swordOfOrder != null)
			{
				swordOfOrder.Enabled = true;
				creature.StatMods.Add(Stat.AttackMinMod, swordOfOrder.RankData.Var1, StatModSource.Skill, (long)skill.Info.Id);
				creature.StatMods.Add(Stat.AttackMaxMod, swordOfOrder.RankData.Var1, StatModSource.Skill, (long)skill.Info.Id);
				creature.StatMods.Add(Stat.InjuryMinMod, swordOfOrder.RankData.Var2, StatModSource.Skill, (long)skill.Info.Id);
				creature.StatMods.Add(Stat.InjuryMaxMod, swordOfOrder.RankData.Var2, StatModSource.Skill, (long)skill.Info.Id);
			}

			// Restore stats
			creature.Injuries -= skill.RankData.Var1;
			creature.Life += skill.RankData.Var1;
			creature.Mana += skill.RankData.Var2;
			creature.Hunger -= skill.RankData.Var3;
			creature.Stamina += skill.RankData.Var3;

			this.UpdateClientStats(creature);

			// Passive defenses
			// If the user has them, they are handled. The feature
			// "PaladinPassiveDefence" only controls whether you are able
			// to *get them* via normal means.
			var effectDelay = EffectBaseDelay;

			// Natural Shield
			if (naturalShield != null && rnd.Next(100) < (skill.RankData.Var7 + naturalShield.RankData.Var4))
			{
				Send.EffectDelayed(creature, effectDelay, Effect.PaladinPassiveDefense, (ushort)naturalShield.Info.Id);
				Send.SystemMessage(creature, Localization.Get("Pal. Natural Shield activated."));
				naturalShield.Enabled = true;
				effectDelay += EffectAddDelay;
			}

			// Heavy Stander
			if (heavyStander != null && rnd.Next(100) < (skill.RankData.Var7 + heavyStander.RankData.Var4))
			{
				Send.EffectDelayed(creature, effectDelay, Effect.PaladinPassiveDefense, (ushort)heavyStander.Info.Id);
				Send.SystemMessage(creature, Localization.Get("Pal. Heavy Stander activated."));
				heavyStander.Enabled = true;
				effectDelay += EffectAddDelay;
			}

			// Mana Deflector
			if (manaDeflector != null && rnd.Next(100) < (skill.RankData.Var7 + manaDeflector.RankData.Var4))
			{
				Send.EffectDelayed(creature, effectDelay, Effect.PaladinPassiveDefense, (ushort)manaDeflector.Info.Id);
				Send.SystemMessage(creature, Localization.Get("Pal. Mana Deflector activated."));
				manaDeflector.Enabled = true;
				effectDelay += EffectAddDelay;
			}

			// Passive activation message
			var passives = "";
			if (naturalShield != null && naturalShield.Enabled) passives += "Pal. Natural Shield, \n";
			if (heavyStander != null && heavyStander.Enabled) passives += "Pal. Heavy Stander, \n";
			if (manaDeflector != null && manaDeflector.Enabled) passives += "Pal. Mana Deflector, \n";
			if (passives != "")
			{
				passives = passives.Substring(0, passives.Length - 3);
				Send.Notice(creature, NoticeType.Middle, 0, EffectBaseDelay, Localization.Get(passives + " activated."));
			}
		}

		/// <summary>
		/// Updates public and private stats relevant to the transformation
		/// on all clients in range of creature.
		/// </summary>
		/// <param name="creature"></param>
		private void UpdateClientStats(Creature creature)
		{
			Send.StatUpdate(creature, StatUpdateType.Public, Stat.Life, Stat.LifeInjured, Stat.LifeMaxMod, Stat.LifeMax);
			Send.StatUpdate(creature, StatUpdateType.Private,
				Stat.Life, Stat.LifeInjured, Stat.LifeMax, Stat.LifeMaxMod,
				Stat.Mana, Stat.ManaMax, Stat.ManaMaxMod,
				Stat.Stamina, Stat.Hunger, Stat.StaminaMax, Stat.StaminaMaxMod,
				Stat.DefenseMod, Stat.ProtectionMod, Stat.MagicDefenseMod, Stat.MagicProtectionMod,
				Stat.StrMod, Stat.WillMod, Stat.DexMod, Stat.BalanceMod,
				Stat.AttackMinMod, Stat.AttackMaxMod, Stat.InjuryMinMod, Stat.InjuryMaxMod
			);
		}

		/// <summary>
		/// Resets creature's transformation and updates clients.
		/// </summary>
		/// <param name="creature"></param>
		private void ResetTransformation(Creature creature, Skill skill)
		{
			creature.Transformation = Transformation.None;
			creature.TransformationSkillRank = SkillRank.Novice;
			creature.TransformationLevel = TransformationLevel.None;
			creature.Titles.SetTempTitle(0);

			Send.UpdateTransformation(creature);
		}

		/// <summary>
		/// Removes bonuses added when the transformation began and
		/// updates clients.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		private void RemoveBonuses(Creature creature, Skill skill)
		{
			// Save stat rates before removing the bonuses
			var injuryRate = (1f / creature.LifeMax * creature.Injuries);
			var lifeRate = (1f / creature.LifeMax * creature.Life);
			var manaRate = (1f / creature.ManaMax * creature.Mana);
			var staminaRate = (1f / creature.StaminaMax * creature.Stamina);

			creature.StatMods.Remove(StatModSource.Skill, (long)skill.Info.Id);

			// Adjust stats to the percentages they were before removing
			// the bonuses.
			creature.Injuries = (creature.LifeMax * injuryRate);
			creature.Life = (creature.LifeMax * lifeRate);
			creature.Mana = (creature.ManaMax * manaRate);
			creature.Stamina = (creature.StaminaMax * staminaRate);

			this.UpdateClientStats(creature);

			// Disable skills
			creature.Skills.SetEnabled(false, SkillId.PowerOfOrder, SkillId.EyeOfOrder, SkillId.SwordOfOrder, SkillId.PaladinNaturalShield, SkillId.PaladinHeavyStander, SkillId.PaladinManaDeflector);
		}

		/// <summary>
		/// Raised when a creature levels up.
		/// </summary>
		/// <param name="creature"></param>
		private void OnCreatureLevelUp(Creature creature)
		{
			// Give 10 exp to Spirit of Order if creature has it.
			var skill = creature.Skills.Get(SkillId.SpiritOfOrder);
			if (skill != null)
				skill.Train(1, ExpPerLevelUp);
		}
	}
}
