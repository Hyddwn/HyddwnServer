// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.Skills.Magic;
using Aura.Channel.World.Entities;
using Aura.Mabi.Const;
using Aura.Mabi.Network;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.Skills.Combat
{
	/// <summary>
	/// Support Shot handler.
	/// </summary>
	/// <remarks>
	/// Var1: Damage
	/// Var2: ?
	/// Var3: ?
	/// Var4: ?
	/// Var5: ?
	/// 
	/// Current behavior: Doesn't modify stability, stuns for 4.2s, as logged
	/// on NA, and aggroes target. It doesn't aim faster than Ranged
	/// (not that we'd have control over that), but it gets the same speed
	/// bonus, and it does 80% damage, as specified by skill var 1.
	/// </remarks>
	[Skill(SkillId.SupportShot)]
	public class SupportShot : ISkillHandler, IPreparable, IReadyable, ICompletable, ICancelable, ICombatSkill, IInitiableSkillHandler
	{
		/// <summary>
		/// Distance to knock the target back.
		/// </summary>
		private const int KnockBackDistance = 400;

		/// <summary>
		/// Stun for the attacker
		/// </summary>
		private const int AttackerStun = 800;

		/// <summary>
		/// Stun for the target
		/// </summary>
		private const int TargetStun = 4200;

		/// <summary>
		/// Bonus damage for fire arrows
		/// </summary>
		public const float FireBonus = 1.5f;

		/// <summary>
		/// Sets up subscriptions for skill training.
		/// </summary>
		public void Init()
		{
			ChannelServer.Instance.Events.CreatureAttack += this.OnCreatureAttacks;
		}

		/// <summary>
		/// Prepares skill.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		/// <returns></returns>
		public bool Prepare(Creature creature, Skill skill, Packet packet)
		{
			Send.SkillPrepare(creature, skill.Info.Id, skill.GetCastTime());

			// Lock running if not elf
			if (!creature.CanRunWithRanged)
				creature.Lock(Locks.Run);

			return true;
		}

		/// <summary>
		/// Readies skill, activates fire effect.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		/// <returns></returns>
		public bool Ready(Creature creature, Skill skill, Packet packet)
		{
			// Light arrows (!) on fire if there's a campfire nearby
			if (creature.RightHand != null && creature.RightHand.HasTag("/bow/"))
				creature.Temp.FireArrow = creature.Region.GetProps(a => a.Info.Id == 203 && a.GetPosition().InRange(creature.GetPosition(), 500)).Count > 0;

			// Add fire arrow effect to arrow
			if (creature.Temp.FireArrow)
				Send.Effect(creature, Effect.FireArrow, true);

			Send.SkillReady(creature, skill.Info.Id);

			// Lock running
			if (!creature.CanRunWithRanged)
				creature.Lock(Locks.Run);

			return true;
		}

		/// <summary>
		/// Completes skill, stopping the aim meter and disabling the fire effect.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void Complete(Creature creature, Skill skill, Packet packet)
		{
			this.Cancel(creature, skill);
			Send.SkillComplete(creature, skill.Info.Id);
		}

		/// <summary>
		/// Cancels skill, stopping the aim meter and disabling the fire effect.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		public void Cancel(Creature creature, Skill skill)
		{
			creature.AimMeter.Stop();

			// Disable fire arrow effect
			if (creature.Temp.FireArrow)
				Send.Effect(creature, Effect.FireArrow, false);
		}

		/// <summary>
		/// Uses the skill.
		/// </summary>
		/// <param name="attacker"></param>
		/// <param name="skill"></param>
		/// <param name="targetEntityId"></param>
		/// <returns></returns>
		public CombatSkillResult Use(Creature attacker, Skill skill, long targetEntityId)
		{
			// Get target
			var target = attacker.Region.GetCreature(targetEntityId);
			if (target == null)
				return CombatSkillResult.InvalidTarget;

			// "Cancels" the skill
			// 800 = old load time? == aAction.Stun? Varies? Doesn't seem to be a stun.
			Send.SkillUse(attacker, skill.Info.Id, AttackerStun, 1);

			var chance = attacker.AimMeter.GetAimChance(target);
			var rnd = RandomProvider.Get().NextDouble() * 100;
			var successfulHit = (rnd < chance);

			// Actions
			var cap = new CombatActionPack(attacker, skill.Info.Id);

			var aAction = new AttackerAction(CombatActionType.RangeHit, attacker, targetEntityId);
			aAction.Set(AttackerOptions.Result);
			aAction.Stun = AttackerStun;
			cap.Add(aAction);

			// Target action if hit
			if (successfulHit)
			{
				target.StopMove();

				var tAction = new TargetAction(CombatActionType.TakeHit, target, attacker, skill.Info.Id);
				tAction.Set(TargetOptions.Result);
				tAction.Stun = TargetStun;

				cap.Add(tAction);

				// Damage
				var damage = attacker.GetRndRangedDamage() * (skill.RankData.Var1 / 100f);

				// Elementals
				damage *= attacker.CalculateElementalDamageMultiplier(target);

				// More damage with fire arrow
				if (attacker.Temp.FireArrow)
					damage *= FireBonus;

				// Critical Hit
				var critChance = attacker.GetRightCritChance(target.Protection);
				CriticalHit.Handle(attacker, critChance, ref damage, tAction);

				// Subtract target def/prot
				SkillHelper.HandleDefenseProtection(target, ref damage);

				// Conditions
				SkillHelper.HandleConditions(attacker, target, ref damage);

				// Defense
				Defense.Handle(aAction, tAction, ref damage);

				// Mana Shield
				ManaShield.Handle(target, ref damage, tAction);

				// Natural Shield
				var delayReduction = NaturalShield.Handle(attacker, target, ref damage, tAction);

				// Deal with it!
				if (damage > 0)
				{
					target.TakeDamage(tAction.Damage = damage, attacker);
					SkillHelper.HandleInjury(attacker, target, damage);
				}

				// Aggro
				target.Aggro(attacker);

				// Knock down on deadly
				if (target.Conditions.Has(ConditionsA.Deadly))
					tAction.Set(TargetOptions.KnockDown);

				// Death/Knockback
				if (target.IsDead)
					tAction.Set(TargetOptions.FinishingKnockDown);

				// Knock Back
				if (tAction.IsKnockBack)
					attacker.Shove(target, KnockBackDistance);

				// Reduce stun, based on ping
				if (delayReduction > 0)
					tAction.Stun = (short)Math.Max(0, tAction.Stun - (tAction.Stun / 100 * delayReduction));

				// TODO: "Weakened" state (G12S2 gfSupportShotRenewal)
			}

			// Update current weapon
			SkillHelper.UpdateWeapon(attacker, target, ProficiencyGainType.Ranged, attacker.RightHand);

			// Reduce arrows
			if (attacker.Magazine != null && !ChannelServer.Instance.Conf.World.InfiniteArrows && !attacker.Magazine.HasTag("/unlimited_arrow/"))
				attacker.Inventory.Decrement(attacker.Magazine);

			cap.Handle();

			// Disable fire arrow effect
			if (attacker.Temp.FireArrow)
				Send.Effect(attacker, Effect.FireArrow, false);

			return CombatSkillResult.Okay;
		}

		/// <summary>
		/// Handles the skill training.
		/// </summary>
		/// <param name="tAction"></param>
		private void OnCreatureAttacks(TargetAction tAction)
		{
			if (tAction.AttackerSkillId != SkillId.SupportShot)
				return;

			var skill = tAction.Attacker.Skills.Get(SkillId.SupportShot);
			if (skill == null)
				return;

			var powerRating = tAction.Attacker.GetPowerRating(tAction.Creature);

			if (skill.Info.Rank >= SkillRank.RF && skill.Info.Rank <= SkillRank.RA)
			{
				skill.Train(1); // Successfully use the skill.

				if (!tAction.IsKnockBack)
					skill.Train(2); // Successfully use Support Shot without knocking down the enemy.

				if (tAction.Has(TargetOptions.Critical))
					skill.Train(3); // Succeed in a Critical Hit with Support Shot.
			}
			else if (skill.Info.Rank == SkillRank.R9)
			{
				if (powerRating == PowerRating.Normal)
					skill.Train(1); // Successfully use the skill on a similarly ranked enemy.

				if (powerRating == PowerRating.Strong)
					skill.Train(2); // Successfully use the skill on a strong enemy.

				if (!tAction.IsKnockBack)
					skill.Train(3); // Successfully use Support Shot without knocking down the enemy.

				if (tAction.Has(TargetOptions.Critical))
					skill.Train(4); // Succeed in a Critical Hit with Support Shot.
			}
			else if (skill.Info.Rank == SkillRank.R8)
			{
				if (powerRating == PowerRating.Normal)
					skill.Train(1); // Successfully use the skill on a similarly ranked enemy.

				if (powerRating == PowerRating.Strong)
				{
					skill.Train(2); // Successfully use the skill on a Strong enemy.

					if (tAction.Has(TargetOptions.Critical))
						skill.Train(4); // Succeed in a Critical Hit against a Strong enemy.
				}

				if (!tAction.IsKnockBack)
					skill.Train(3); // Successfully use Support Shot without knocking down the enemy.
			}
			else if (skill.Info.Rank >= SkillRank.R7 && skill.Info.Rank <= SkillRank.R6)
			{
				if (powerRating == PowerRating.Strong)
				{
					skill.Train(1); // Successfully use the skill on a Strong enemy.

					if (tAction.Has(TargetOptions.Critical))
						skill.Train(4); // Succeed in a Critical Hit against a Strong enemy.
				}

				if (powerRating == PowerRating.Awful)
					skill.Train(2); // Successfully use the skill on an Awful enemy.

				if (!tAction.IsKnockBack)
					skill.Train(3); // Successfully use Support Shot without knocking down the enemy.
			}
			else if (skill.Info.Rank >= SkillRank.R5 && skill.Info.Rank <= SkillRank.R4)
			{
				if (powerRating == PowerRating.Strong)
					skill.Train(1); // Successfully use the skill on a Strong enemy.

				if (powerRating == PowerRating.Awful)
				{
					skill.Train(2); // Successfully use the skill on an Awful enemy.

					if (tAction.Has(TargetOptions.Critical))
						skill.Train(4); // Succeed in a Critical Hit against an Awful enemy.
				}

				if (!tAction.IsKnockBack)
					skill.Train(3); // Successfully use Support Shot without knocking down the enemy.
			}
			else if (skill.Info.Rank >= SkillRank.R3 && skill.Info.Rank <= SkillRank.R1)
			{
				if (powerRating == PowerRating.Awful)
					skill.Train(1); // Successfully use the skill on an Awful enemy.

				if (powerRating == PowerRating.Boss)
				{
					skill.Train(2); // Successfully use the skill on a Boss-level enemy.

					if (tAction.Has(TargetOptions.Critical))
						skill.Train(4); // Succeed in a Critical Hit against a Boss-level enemy.
				}

				if (!tAction.IsKnockBack)
					skill.Train(3); // Successfully use Support Shot without knocking down the enemy.
			}
		}
	}
}
