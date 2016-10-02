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
	/// Handler for Arrow Revolver 1 and 2.
	/// </summary>
	/// <remarks>
	/// Var1: ?
	/// Var2: Finish Shot
	/// Var3: Accuracy (defined only for AR2)
	/// Var18: Min Damage
	/// Var19: Max Damage
	/// Var20: Attack Range
	/// 
	/// Handles both the "prototype" version of Arrow Revolver (ArrowRevolver),
	/// that only Mari uses, and the improved one (ArrowRevolver2), introduced
	/// for players in G2.
	/// </remarks>
	[Skill(SkillId.ArrowRevolver, SkillId.ArrowRevolver2)]
	public class ArrowRevolver : ISkillHandler, IPreparable, IReadyable, ICompletable, ICancelable, ICombatSkill, IInitiableSkillHandler, ICustomHitCanceler
	{
		/// <summary>
		/// Distance to knock the target back.
		/// </summary>
		private const int KnockBackDistance = 400;

		/// <summary>
		/// Amount of stability lost on hit.
		/// </summary>
		private const float StabilityReduction = 60f;

		/// <summary>
		/// Stun for the attacker
		/// </summary>
		private const int AttackerStun = 800;

		/// <summary>
		/// Stun for the target
		/// </summary>
		private const int TargetStun = 2100;

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
			skill.Stacks = Math.Min(skill.RankData.StackMax, skill.Stacks + skill.RankData.Stack);

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
				// Formula unofficial, but it kinda matches what you would
				// expect from the skill, and what players believed the damage
				// to be, back in G2.
				// bonus = (100 - (6 - stacks) * 5 + rank, +var2 on last shot
				// I'm using rank instead of Var1, which goes from 1-15 in
				// AR2, so AR1 gets a little bonus as well, as AR1's Var1 and
				// 2 are 0.
				// With this formula, the bonus range (1st shot rF vs 5th shot
				// r1) is 76~110% for AR1, and 76~140% for AR2.
				var bonus = 100f - (6 - skill.Stacks) * 5f + (byte)skill.Info.Rank;
				if (skill.Stacks == 1)
					bonus += skill.RankData.Var2;

				var damage = attacker.GetRndRangedDamage() * (bonus / 100f);

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
				{
					tAction.Set(TargetOptions.FinishingKnockDown);
				}
				else
				{
					// Insta-recover in knock down
					if (target.IsKnockedDown)
					{
						tAction.Stun = 0;
					}
					// Knock down if hit repeatedly
					else if (target.Stability < 30)
					{
						tAction.Set(TargetOptions.KnockDown);
					}
					// Normal stability reduction
					else
					{
						var stabilityReduction = StabilityReduction;

						// Reduce reduction, based on ping
						// According to the Wiki, "the Knockdown Gauge
						// [does not] build up", but it's still possible
						// to knock back with repeated hits. The stability
						// reduction is probably reduced, just like stun.
						if (delayReduction > 0)
							stabilityReduction = (short)Math.Max(0, stabilityReduction - (stabilityReduction / 100 * delayReduction));

						target.Stability -= stabilityReduction;
						if (target.IsUnstable)
						{
							tAction.Set(TargetOptions.KnockBack);
						}
					}
				}

				// Knock Back
				if (tAction.IsKnockBack)
					attacker.Shove(target, KnockBackDistance);

				// Reduce stun, based on ping
				if (delayReduction > 0)
					tAction.Stun = (short)Math.Max(0, tAction.Stun - (tAction.Stun / 100 * delayReduction));
			}

			// Update current weapon
			SkillHelper.UpdateWeapon(attacker, target, ProficiencyGainType.Ranged, attacker.RightHand);

			// Skill training
			if (skill.Info.Rank == SkillRank.RF)
				skill.Train(1); // Try attacking with Arrow Revolver.

			// Reduce arrows
			if (attacker.Magazine != null && !ChannelServer.Instance.Conf.World.InfiniteArrows && !attacker.Magazine.HasTag("/unlimited_arrow/"))
				attacker.Inventory.Decrement(attacker.Magazine);

			// Reduce stack
			skill.Stacks--;

			// Handle
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
			if (tAction.AttackerSkillId != SkillId.ArrowRevolver && tAction.AttackerSkillId != SkillId.ArrowRevolver2)
				return;

			var skill = tAction.Attacker.Skills.Get(tAction.AttackerSkillId);
			if (skill == null)
				return;

			var powerRating = tAction.Attacker.GetPowerRating(tAction.Creature);
			var targetDown = tAction.IsKnockBack;
			var targetDead = tAction.Creature.IsDead;

			if (skill.Info.Rank == SkillRank.RF)
			{
				skill.Train(2); // Successfully hit an enemy.

				if (targetDown) skill.Train(3); // Down an enemy with continous hit.
				if (targetDead) skill.Train(4); // Kill an enemy.
			}
			else if (skill.Info.Rank == SkillRank.RE)
			{
				if (targetDown) skill.Train(1); // Down an enemy with continous hit.
				if (targetDead) skill.Train(2); // Kill an enemy.

				if (powerRating == PowerRating.Normal)
				{
					skill.Train(3); // Attack a same level enemy.

					if (targetDown) skill.Train(4); // Down a same level enemy.
					if (targetDead) skill.Train(5); // Kill a same level enemy.
				}

				if (powerRating == PowerRating.Strong)
				{
					if (targetDown) skill.Train(6); // Down a strong enemy.
					if (targetDead) skill.Train(7); // Kill a strong enemy.
				}
			}
			else if (skill.Info.Rank == SkillRank.RD)
			{
				skill.Train(1); // Successfully hit an enemy.

				if (targetDown) skill.Train(2); // Down an enemy with continous hit.
				if (targetDead) skill.Train(3); // Kill an enemy.

				if (powerRating == PowerRating.Normal)
				{
					skill.Train(4); // Attack a same level enemy.

					if (targetDown) skill.Train(5); // Down a same level enemy.
					if (targetDead) skill.Train(6); // Kill a same level enemy.
				}

				if (powerRating == PowerRating.Strong)
				{
					if (targetDown) skill.Train(7); // Down a strong enemy.
					if (targetDead) skill.Train(8); // Kill a strong enemy.
				}
			}
			else if (skill.Info.Rank >= SkillRank.RC && skill.Info.Rank <= SkillRank.RB)
			{
				if (powerRating == PowerRating.Normal)
				{
					skill.Train(1); // Attack a same level enemy.

					if (targetDown) skill.Train(2); // Down a same level enemy.
					if (targetDead) skill.Train(3); // Kill a same level enemy.
				}
				else if (powerRating == PowerRating.Strong)
				{
					if (targetDown) skill.Train(4); // Down a strong enemy.
					if (targetDead) skill.Train(5); // Kill a strong enemy.
				}
				else if (powerRating == PowerRating.Awful)
				{
					if (targetDown) skill.Train(6); // Down an awful enemy.
					if (targetDead) skill.Train(7); // Kill an awful enemy.
				}
			}
			else if (skill.Info.Rank >= SkillRank.RA && skill.Info.Rank <= SkillRank.R8)
			{
				if (powerRating == PowerRating.Normal)
				{
					if (targetDown) skill.Train(1); // Down a same level enemy.
					if (targetDead) skill.Train(2); // Kill a same level enemy.
				}
				else if (powerRating == PowerRating.Strong)
				{
					if (targetDown) skill.Train(3); // Down a strong enemy.
					if (targetDead) skill.Train(4); // Kill a strong enemy.
				}
				else if (powerRating == PowerRating.Awful)
				{
					if (targetDown) skill.Train(5); // Down an awful enemy.
					if (targetDead) skill.Train(6); // Kill an awful enemy.
				}
				else if (powerRating == PowerRating.Boss && skill.Info.Rank == SkillRank.R8)
				{
					if (targetDown) skill.Train(7); // Down a boss level enemy.
					if (targetDead) skill.Train(8); // Kill a boss level enemy.
				}
			}
			else if (skill.Info.Rank == SkillRank.R7)
			{
				if (powerRating == PowerRating.Normal)
				{
					if (targetDead) skill.Train(1); // Kill a same level enemy.
				}
				else if (powerRating == PowerRating.Strong)
				{
					if (targetDown) skill.Train(2); // Down a strong enemy.
					if (targetDead) skill.Train(3); // Kill a strong enemy.
				}
				else if (powerRating == PowerRating.Awful)
				{
					if (targetDown) skill.Train(4); // Down an awful enemy.
					if (targetDead) skill.Train(5); // Kill an awful enemy.
				}
				else if (powerRating == PowerRating.Boss)
				{
					if (targetDown) skill.Train(6); // Down a boss level enemy.
					if (targetDead) skill.Train(7); // Kill a boss level enemy.
				}
			}
			else if (skill.Info.Rank >= SkillRank.R6 && skill.Info.Rank <= SkillRank.R1)
			{
				if (powerRating == PowerRating.Strong)
				{
					if (targetDown) skill.Train(1); // Down a strong enemy.
					if (targetDead) skill.Train(2); // Kill a strong enemy.
				}
				else if (powerRating == PowerRating.Awful)
				{
					if (targetDown) skill.Train(3); // Down an awful enemy.
					if (targetDead) skill.Train(4); // Kill an awful enemy.
				}
				else if (powerRating == PowerRating.Boss)
				{
					if (targetDown) skill.Train(5); // Down a boss level enemy.
					if (targetDead) skill.Train(6); // Kill a boss level enemy.
				}
			}
		}

		/// <summary>
		/// Called when creature is hit while a bolt skill is active.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="tAction"></param>
		public void CustomHitCancel(Creature creature, TargetAction tAction)
		{
			var skill = creature.Skills.ActiveSkill;

			// Cancel skill on knock down, or if only one stack is left
			if (tAction.Has(TargetOptions.KnockDown) || skill.Stacks <= 1)
			{
				creature.Skills.CancelActiveSkill();
				return;
			}

			// Reduce stack by one on hit
			skill.Stacks -= 1;
		}
	}
}
