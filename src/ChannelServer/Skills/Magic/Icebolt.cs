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
	/// Icebolt skill handler
	/// </summary>
	/// <remarks>
	/// Var1: Min damage
	/// Var2: Max damage
	/// Var3: ?
	/// </remarks>
	[Skill(SkillId.Icebolt)]
	public class Icebolt : MagicBolt
	{
		/// <summary>
		/// ID of the skill, used in training.
		/// </summary>
		protected override SkillId SkillId { get { return SkillId.Icebolt; } }

		/// <summary>
		/// String used in effect packets.
		/// </summary>
		protected override string EffectSkillName { get { return "icebolt"; } }

		/// <summary>
		/// Weapon tag that's looked for in range calculation.
		/// </summary>
		protected override string SpecialWandTag { get { return "ice_wand"; } }

		/// <summary>
		/// Bolt specific use code.
		/// </summary>
		/// <param name="attacker"></param>
		/// <param name="skill"></param>
		/// <param name="target"></param>
		protected override void UseSkillOnTarget(Creature attacker, Skill skill, Creature target)
		{
			attacker.StopMove();
			target.StopMove();

			// Create actions
			var aAction = new AttackerAction(CombatActionType.RangeHit, attacker, target.EntityId);
			aAction.Set(AttackerOptions.Result);

			var tAction = new TargetAction(CombatActionType.TakeHit, target, attacker, skill.Info.Id);
			tAction.Set(TargetOptions.Result);
			tAction.Stun = TargetStun;

			var cap = new CombatActionPack(attacker, skill.Info.Id, aAction, tAction);

			// Damage
			var damage = this.GetDamage(attacker, skill);

			// Elements
			damage *= this.GetElementalDamageMultiplier(attacker, target);

			// Critical Hit
			var critChance = attacker.GetTotalCritChance(target.Protection, true);
			CriticalHit.Handle(attacker, critChance, ref damage, tAction);

			// Reduce damage
			Defense.Handle(aAction, tAction, ref damage);
			SkillHelper.HandleMagicDefenseProtection(target, ref damage);
			SkillHelper.HandleConditions(attacker, target, ref damage);
			ManaShield.Handle(target, ref damage, tAction);

			// Mana Deflector
			var delayReduction = ManaDeflector.Handle(attacker, target, ref damage, tAction);

			// Deal damage
			if (damage > 0)
				target.TakeDamage(tAction.Damage = damage, attacker);
			target.Aggro(attacker);

			// Knock down on deadly
			if (target.Conditions.Has(ConditionsA.Deadly))
			{
				tAction.Set(TargetOptions.KnockDown);
				tAction.Stun = TargetStun;
			}

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
					var stabilityReduction = StabilityReduction;

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

			// Override stun set by defense
			aAction.Stun = AttackerStun;

			Send.Effect(attacker, Effect.UseMagic, EffectSkillName);
			Send.SkillUseStun(attacker, skill.Info.Id, aAction.Stun, 1);

			skill.Stacks--;

			// Update current weapon
			SkillHelper.UpdateWeapon(attacker, target, ProficiencyGainType.Melee, attacker.RightHand);

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
			return attacker.CalculateElementalDamageMultiplier(0, 0, Creature.MaxElementalAffinity, target);
		}
	}
}
