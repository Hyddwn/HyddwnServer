// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.Skills.Magic;
using Aura.Channel.World.Entities;
using Aura.Mabi.Const;
using Aura.Mabi.Network;
using Aura.Shared.Util;
using System.Collections.Generic;

namespace Aura.Channel.Skills.Combat
{
	/// <summary>
	/// Magnum Shot handler
	/// </summary>
	/// <remarks>
	/// Var1: Damage
	/// Var4: Splash Damage
	/// Var5: Splash Distance (Radius?)
	/// Var6: Splash Angle?
	/// </remarks>
	[Skill(SkillId.MagnumShot)]
	public class MagnumShot : ISkillHandler, IPreparable, IReadyable, ICompletable, ICancelable, ICombatSkill,
		IInitiableSkillHandler
	{
		/// <summary>
		/// Distance to knock the target back.
		/// </summary>
		private const int KnockBackDistance = 450;

		/// <summary>
		/// Stun for the attacker
		/// </summary>
		/// <remarks>
		/// There are actions with 520 stun, dependent on something?
		/// </remarks>
		private const int AttackerStun = 600;

		/// <summary>
		/// Stun for the target
		/// </summary>
		private const int TargetStun = 3000;

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
			Send.SkillFlashEffect(creature);
			Send.SkillPrepare(creature, skill.Info.Id, skill.GetCastTime());

			if (creature.RightHand == null || !creature.RightHand.HasTag("/crossbow/"))
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
			creature.Temp.FireArrow = creature.Region.GetProps(a => a.Info.Id == 203 && a.GetPosition().InRange(creature.GetPosition(), 500)).Count > 0;
			if (creature.Temp.FireArrow)
				Send.Effect(creature, Effect.FireArrow, true);

			Send.SkillReady(creature, skill.Info.Id);

			if (creature.RightHand == null || !creature.RightHand.HasTag("/crossbow/"))
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
			var mainTarget = attacker.Region.GetCreature(targetEntityId);
			if (mainTarget == null)
				return CombatSkillResult.InvalidTarget;

			// Actions
			var cap = new CombatActionPack(attacker, skill.Info.Id);

			var aAction = new AttackerAction(CombatActionType.RangeHit, attacker, targetEntityId);
			aAction.Set(AttackerOptions.Result);
			aAction.Stun = AttackerStun;
			cap.Add(aAction);

			// Hit by chance
			var chance = attacker.AimMeter.GetAimChance(mainTarget);
			var rnd = RandomProvider.Get();
			if (rnd.NextDouble() * 100 < chance)
			{
				aAction.Set(AttackerOptions.KnockBackHit2);

				// Get targets, incl. splash.
				// Splash happens from r5 onwards, but we'll base it on Var4,
				// which is the splash damage and first != 0 on r5.
				var targets = new HashSet<Creature>() { mainTarget };
				if (skill.RankData.Var4 != 0)
				{
					var targetPosition = mainTarget.GetPosition();
					var direction = attacker.GetPosition().GetDirection(targetPosition);
					targets.UnionWith(attacker.GetTargetableCreaturesInCone(targetPosition, direction, skill.RankData.Var5, skill.RankData.Var6));
				}

				// Damage
				var mainDamage = this.GetDamage(attacker, skill);

				foreach (var target in targets)
				{
					var tAction = new TargetAction(CombatActionType.TakeHit, target, attacker, skill.Info.Id);
					tAction.Set(TargetOptions.Result | TargetOptions.CleanHit);
					tAction.Stun = TargetStun;
					cap.Add(tAction);

					// Damage
					var damage = mainDamage;

					// Elementals
					damage *= attacker.CalculateElementalDamageMultiplier(target);

					// More damage with fire arrow
					// XXX: Does this affect the element?
					if (attacker.Temp.FireArrow)
						damage *= FireBonus;

					// Splash modifier
					if (target != mainTarget)
						damage *= (skill.RankData.Var4 / 100f);

					// Critical Hit
					var critChance = attacker.GetRightCritChance(target.Protection);
					CriticalHit.Handle(attacker, critChance, ref damage, tAction);

					// Subtract target def/prot
					SkillHelper.HandleDefenseProtection(target, ref damage);

					// Conditions
					SkillHelper.HandleConditions(attacker, target, ref damage);

					// Mana Shield
					ManaShield.Handle(target, ref damage, tAction);

					// Natural Shield
					// Ignore delay reduction, as knock downs shouldn't be shortened
					NaturalShield.Handle(attacker, target, ref damage, tAction);

					// Deal with it!
					if (damage > 0)
					{
						target.TakeDamage(tAction.Damage = damage, attacker);
						SkillHelper.HandleInjury(attacker, target, damage);
					}

					// Knock down
					// If target is using a shield and defense, don't KD.
					var targetLeftHand = target.LeftHand;
					if (tAction.SkillId != SkillId.Defense || targetLeftHand == null || !targetLeftHand.IsShield)
					{
						// TODO: We have to calculate knockback distance right
						attacker.Shove(target, KnockBackDistance);
						tAction.Set(TargetOptions.KnockDownFinish);
					}

					// Aggro
					if (target == mainTarget)
						target.Aggro(attacker);

					if (target.IsDead)
					{
						tAction.Set(TargetOptions.Finished);
						if (target == mainTarget)
							aAction.Set(AttackerOptions.KnockBackHit1);
					}
				}
			}
			else
			{
				aAction.Set(AttackerOptions.Missed);
			}

			// Update current weapon
			SkillHelper.UpdateWeapon(attacker, mainTarget, ProficiencyGainType.Ranged, attacker.RightHand);

			// Reduce arrows
			if (attacker.Magazine != null && !ChannelServer.Instance.Conf.World.InfiniteArrows && !attacker.Magazine.HasTag("/unlimited_arrow/"))
				attacker.Inventory.Decrement(attacker.Magazine);

			// Disable fire arrow effect
			if (attacker.Temp.FireArrow)
				Send.Effect(attacker, Effect.FireArrow, false);

			// "Cancels" the skill
			// 800 = old load time? == aAction.Stun? Varies? Doesn't seem to be a stun.
			Send.SkillUse(attacker, skill.Info.Id, 800, 1);

			cap.Handle();

			return CombatSkillResult.Okay;
		}

		/// <summary>
		/// Returns the raw damage to be done.
		/// </summary>
		/// <param name="attacker"></param>
		/// <param name="skill"></param>
		/// <returns></returns>
		protected float GetDamage(Creature attacker, Skill skill)
		{
			var result = attacker.GetRndRangedDamage();
			result *= skill.RankData.Var1 / 100f;

			return result;
		}

		/// <summary>
		/// Handles the majority of the skill training.
		/// </summary>
		/// <param name="tAction"></param>
		private void OnCreatureAttacks(TargetAction tAction)
		{
			if (tAction.AttackerSkillId != SkillId.MagnumShot)
				return;

			var attackerSkill = tAction.Attacker.Skills.Get(SkillId.MagnumShot);

			if (attackerSkill != null)
			{
				attackerSkill.Train(1); // Attack an enemy.
				if (tAction.Has(TargetOptions.Critical))
					attackerSkill.Train(2);

				if (tAction.Creature.IsDead)  // Kill an enemy.
				{
					attackerSkill.Train(3);
					if (tAction.Has(TargetOptions.Critical))
						attackerSkill.Train(4);
				}
			}
		}
	}
}
