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
	/// Firebolt skill handler
	/// </summary>
	/// <remarks>
	/// Var1: Min damage
	/// Var2: Max damage
	/// Var3: ?
	/// 
	/// Contrary to what the Wiki says, training seems to be exactly the
	/// same as Icebolt, the Wiki is either outdated or incorrect.
	/// </remarks>
	[Skill(SkillId.Firebolt)]
	public class Firebolt : MagicBolt
	{
		/// <summary>
		/// ID of the skill, used in training.
		/// </summary>
		protected override SkillId SkillId { get { return SkillId.Firebolt; } }

		/// <summary>
		/// String used in effect packets.
		/// </summary>
		protected override string EffectSkillName { get { return "firebolt"; } }

		/// <summary>
		/// Weapon tag that's looked for in range calculation.
		/// </summary>
		protected override string SpecialWandTag { get { return "fire_wand"; } }

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
			SkillHelper.HandleMagicDefenseProtection(target, ref damage);
			SkillHelper.HandleConditions(attacker, target, ref damage);
			ManaShield.Handle(target, ref damage, tAction);
			ManaDeflector.Handle(attacker, target, ref damage, tAction);

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

			// Death/Knockback
			attacker.Shove(target, KnockbackDistance);
			if (target.IsDead)
				tAction.Set(TargetOptions.FinishingKnockDown);
			else
				tAction.Set(TargetOptions.KnockDown);

			// Override stun set by defense
			aAction.Stun = AttackerStun;

			Send.Effect(attacker, Effect.UseMagic, EffectSkillName);
			Send.SkillUseStun(attacker, skill.Info.Id, aAction.Stun, 1);

			skill.Stacks = 0;

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

			if (skill.Stacks < skill.RankData.StackMax)
				damage *= skill.Stacks;
			else
				damage *= 6.5f;

			return damage;
		}

		/// <summary>
		/// Called when creature is hit while Firebolt is active.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="tAction"></param>
		public override void CustomHitCancel(Creature creature, TargetAction tAction)
		{
			// Lose no stacks on r1
			var skill = creature.Skills.ActiveSkill;
			if (skill.Info.Rank < SkillRank.R1)
			{
				creature.Skills.CancelActiveSkill();
				return;
			}
		}

		/// <summary>
		/// Returns elemental damage multiplier for this skill.
		/// </summary>
		/// <param name="attacker"></param>
		/// <param name="target"></param>
		/// <returns></returns>
		protected override float GetElementalDamageMultiplier(Creature attacker, Creature target)
		{
			return attacker.CalculateElementalDamageMultiplier(0, Creature.MaxElementalAffinity, 0, target);
		}
	}
}
