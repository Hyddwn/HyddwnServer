// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.Skills.Combat;
using Aura.Channel.World.Entities;
using Aura.Mabi.Const;
using Aura.Shared.Network;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.Skills.Magic
{
	/// <summary>
	/// Lightningbolt skill handler
	/// </summary>
	/// <remarks>
	/// Var1: Min damage
	/// Var2: Max damage
	/// Var3: ?
	/// 
	/// Contrary to what the Wiki says, training seems to be exactly the
	/// same as Icebolt, the Wiki is either outdated or incorrect.
	/// </remarks>
	[Skill(SkillId.Lightningbolt)]
	public class Lightningbolt : MagicBolt
	{
		/// <summary>
		/// Splash Range for target search.
		/// </summary>
		private const int SplashRange = 500;

		/// <summary>
		/// Stability reduction on overcharge.
		/// </summary>
		protected const float OverchargeStabilityReduction = 110;

		/// <summary>
		/// ID of the skill, used in training.
		/// </summary>
		protected override SkillId SkillId { get { return SkillId.Lightningbolt; } }

		/// <summary>
		/// String used in effect packets.
		/// </summary>
		protected override string EffectSkillName { get { return "lightningbolt"; } }

		/// <summary>
		/// Weapon tag that's looked for in range calculation.
		/// </summary>
		protected override string SpecialWandTag { get { return "lightning_wand"; } }

		/// <summary>
		/// Bolt specific use code.
		/// </summary>
		/// <param name="attacker"></param>
		/// <param name="skill"></param>
		/// <param name="target"></param>
		protected override void UseSkillOnTarget(Creature attacker, Skill skill, Creature mainTarget)
		{
			// Create actions
			var aAction = new AttackerAction(CombatActionType.RangeHit, attacker, mainTarget.EntityId);
			aAction.Set(AttackerOptions.Result);

			var cap = new CombatActionPack(attacker, skill.Info.Id, aAction);

			// Get targets
			// Add the main target as first target, so it gets the first hit,
			// and the full damage.
			var targets = new List<Creature>();
			targets.Add(mainTarget);

			var inSplashRange = attacker.GetTargetableCreaturesAround(mainTarget.GetPosition(), SplashRange);
			targets.AddRange(inSplashRange.Where(a => a != mainTarget));

			// Damage
			var damage = this.GetDamage(attacker, skill);

			var max = Math.Min(targets.Count, skill.Stacks);
			for (int i = 0; i < max; ++i)
			{
				var target = targets[i];
				var targetDamage = damage;

				target.StopMove();

				var tAction = new TargetAction(CombatActionType.TakeHit, target, attacker, skill.Info.Id);
				tAction.Set(TargetOptions.Result);
				tAction.Stun = TargetStun;

				// Full damage for the first target, -10% for every subsequent one.
				targetDamage -= (targetDamage * 0.1f) * i;

				// Critical Hit
				var critChance = attacker.GetTotalCritChance(target.Protection, true);
				CriticalHit.Handle(attacker, critChance, ref damage, tAction);

				// Reduce damage
				Defense.Handle(aAction, tAction, ref targetDamage);
				SkillHelper.HandleMagicDefenseProtection(target, ref targetDamage);
				SkillHelper.HandleConditions(attacker, target, ref damage);
				ManaShield.Handle(target, ref targetDamage, tAction);

				// Mana Deflector
				var delayReduction = ManaDeflector.Handle(attacker, target, ref targetDamage, tAction);

				// Deal damage
				if (targetDamage > 0)
					target.TakeDamage(tAction.Damage = targetDamage, attacker);

				if (target == mainTarget)
					target.Aggro(attacker);

				// Reduce stun, based on ping
				if (delayReduction > 0)
					tAction.Stun = (short)Math.Max(0, tAction.Stun - (tAction.Stun / 100 * delayReduction));

				// Death/Knockback
				if (target.IsDead)
				{
					tAction.Set(TargetOptions.FinishingKnockDown);
				}
				else
				{
					// If knocked down, instant recovery,
					// if repeat hit, knock down,
					// otherwise potential knock back.
					if (target.IsKnockedDown)
					{
						tAction.Stun = 0;
					}
					else if (target.Stability < MinStability)
					{
						tAction.Set(TargetOptions.KnockDown);
					}
					else
					{
						// If number of stacks is greater than the number of
						// targets hit, the targets are knocked back, which is
						// done by reducing the stability to min here.
						// Targets with high enough Mana Deflector might
						// negate this knock back, by reducing the stability
						// reduction to 0.
						var stabilityReduction = (skill.Stacks > targets.Count ? OverchargeStabilityReduction : StabilityReduction);

						// Reduce reduction, based on ping
						// While the Wiki says that "the Knockdown Gauge [does not]
						// build up", tests show that it does. However, it's
						// reduced, assumedly based on the MD rank.
						if (delayReduction > 0)
							stabilityReduction = (short)Math.Max(0, stabilityReduction - (stabilityReduction / 100 * delayReduction));

						target.Stability -= stabilityReduction;

						if (target.IsUnstable)
						{
							tAction.Set(TargetOptions.KnockBack);
						}
					}
				}

				if (tAction.IsKnockBack)
					attacker.Shove(target, KnockbackDistance);

				cap.Add(tAction);
			}

			// Override stun set by defense
			aAction.Stun = AttackerStun;

			Send.Effect(attacker, Effect.UseMagic, EffectSkillName);
			Send.SkillUseStun(attacker, skill.Info.Id, aAction.Stun, 1);

			skill.Stacks = 0;

			// Update current weapon
			SkillHelper.UpdateWeapon(attacker, targets.FirstOrDefault(), ProficiencyGainType.Melee, attacker.RightHand);

			cap.Handle();
		}

		/// <summary>
		/// Returns damage for attacker using skill.
		/// </summary>
		/// <param name="attacker"></param>
		/// <param name="skill"></param>
		/// <returns></returns>
		protected override float GetDamage(Creature attacker, Skill skill)
		{
			var damage = attacker.GetRndMagicDamage(skill, skill.RankData.Var1, skill.RankData.Var2);

			return damage;
		}

		/// <summary>
		/// Returns elemental damage multiplier for this skill.
		/// </summary>
		/// <param name="attacker"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		protected override float GetElementalDamageMultiplier(Creature attacker, Creature target)
		{
			return attacker.CalculateElementalDamageMultiplier(Creature.MaxElementalAffinity, 0, 0, target);
		}
	}
}
