// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.Skills.Magic;
using Aura.Channel.World.Entities;
using Aura.Mabi.Const;
using Aura.Mabi.Network;
using Aura.Shared.Util;

namespace Aura.Channel.Skills.Combat
{
	/// <summary>
	/// Monster stomp skill handler.
	/// </summary>
	/// <remarks>
	/// Based on Giant Stomp logs.
	/// 
	/// It appears that different races use different ranks, that make for
	/// different versions of the skill. The variables don't increase
	/// by rank, but they kinda jump back and forth.
	/// 
	/// Var1: ? (0, 200, 400, 550)
	/// Var2: ? (0, 1000, 1500)
	/// Var3: ? (0, 300, 900)
	/// </remarks>
	[Skill(SkillId.Stomp)]
	public class Stomp : IPreparable, IReadyable, IUseable, ICompletable, ICancelable
	{
		/// <summary>
		/// Units the enemy is knocked back.
		/// </summary>
		private const int KnockbackDistance = 0;

		/// <summary>
		/// Stun for attacker.
		/// </summary>
		private const int AttackerStun = 2000;

		/// <summary>
		/// Prepares the skill.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		/// <returns></returns>
		public bool Prepare(Creature creature, Skill skill, Packet packet)
		{
			Send.SkillInitEffect(creature, "");
			Send.SkillPrepare(creature, skill.Info.Id, skill.GetCastTime());

			return true;
		}

		/// <summary>
		/// Readies the skill.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		/// <returns></returns>
		public bool Ready(Creature creature, Skill skill, Packet packet)
		{
			Send.UseMotion(creature, 10, 0);
			Send.SkillReady(creature, skill.Info.Id);

			return true;
		}

		/// <summary>
		/// Handles using the skill with the information from the packet.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void Use(Creature creature, Skill skill, Packet packet)
		{
			var targetAreaId = packet.GetLong();
			var unkInt1 = packet.GetInt();
			var unkInt2 = packet.GetInt();

			Use(creature, skill, targetAreaId, unkInt1, unkInt2);
		}

		/// <summary>
		/// Handles using the skill.
		/// </summary>
		/// <param name="attacker"></param>
		/// <param name="skill"></param>
		/// <param name="targetAreaId"></param>
		/// <param name="unkInt1"></param>
		/// <param name="unkInt2"></param>
		public void Use(Creature attacker, Skill skill, long targetAreaId, int unkInt1, int unkInt2)
		{
			var range = this.GetRange(attacker, skill);
			var targets = attacker.GetTargetableCreaturesInRange(range, TargetableOptions.AddAttackRange);
			var rnd = RandomProvider.Get();

			// Create actions
			var cap = new CombatActionPack(attacker, skill.Info.Id);

			var aAction = new AttackerAction(CombatActionType.Attacker, attacker, targetAreaId);
			aAction.Set(AttackerOptions.Result);
			aAction.Stun = AttackerStun;

			cap.Add(aAction);

			foreach (var target in targets)
			{
				// Check if hit
				var hitChance = this.GetHitChance(attacker, target, skill);
				if (rnd.Next(0, 100) > hitChance)
					continue;

				target.StopMove();

				var tAction = new TargetAction(CombatActionType.TakeHit, target, attacker, skill.Info.Id);
				tAction.Set(TargetOptions.Result);
				tAction.Delay = 300;

				// Calculate damage
				var damage = this.GetDamage(attacker, skill);

				// Elementals
				damage *= attacker.CalculateElementalDamageMultiplier(target);

				// Handle skills and reductions
				CriticalHit.Handle(attacker, attacker.GetTotalCritChance(0), ref damage, tAction);
				SkillHelper.HandleDefenseProtection(target, ref damage);
				SkillHelper.HandleConditions(attacker, target, ref damage);
				ManaShield.Handle(target, ref damage, tAction);
				HeavyStander.Handle(attacker, target, ref damage, tAction);

				// Clean Hit if not critical
				if (!tAction.Has(TargetOptions.Critical))
					tAction.Set(TargetOptions.CleanHit);

				// Take damage if any is left
				if (damage > 0)
				{
					target.TakeDamage(tAction.Damage = damage, attacker);
					SkillHelper.HandleInjury(attacker, target, damage);
				}

				// Finish if dead, knock down if not defended
				if (target.IsDead)
					tAction.Set(TargetOptions.KnockDownFinish);
				else
					tAction.Set(TargetOptions.KnockDown);

				// Anger Management
				if (!target.IsDead)
					target.Aggro(attacker);

				// Stun & knock down
				tAction.Stun = CombatMastery.GetTargetStun(attacker.AverageKnockCount, attacker.AverageAttackSpeed, true);
				target.Stability = Creature.MinStability;

				// Add action
				cap.Add(tAction);
			}

			Send.UseMotion(attacker, 10, 1);

			cap.Handle();

			Send.SkillUse(attacker, skill.Info.Id, targetAreaId, unkInt1, unkInt2);
		}

		/// <summary>
		/// Handles completing.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void Complete(Creature creature, Skill skill, Packet packet)
		{
			Send.SkillComplete(creature, skill.Info.Id);
		}

		/// <summary>
		/// Finishes motions.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		public void Cancel(Creature creature, Skill skill)
		{
			if (skill.State == SkillState.Ready)
				Send.CancelMotion(creature);
		}

		/// <summary>
		/// Calculates range based on creature and skill.
		/// </summary>
		/// <param name="attacker"></param>
		/// <param name="skill"></param>
		/// <returns></returns>
		private int GetRange(Creature attacker, Skill skill)
		{
			return 1500;
		}

		/// <summary>
		/// Calculates stun based on creature and skill.
		/// </summary>
		/// <param name="attacker"></param>
		/// <param name="skill"></param>
		/// <returns></returns>
		private int GetTargetStun(Creature attacker, Skill skill)
		{
			return 600;
		}

		/// <summary>
		/// Calculates base damage based on creature and skill.
		/// </summary>
		/// <param name="attacker"></param>
		/// <param name="skill"></param>
		/// <returns></returns>
		private float GetDamage(Creature attacker, Skill skill)
		{
			return attacker.GetRndTotalDamage();
		}

		/// <summary>
		/// Calculates hit chance based on creatures and skill.
		/// </summary>
		/// <param name="attacker"></param>
		/// <param name="target"></param>
		/// <param name="skill"></param>
		/// <returns></returns>
		private float GetHitChance(Creature attacker, Creature target, Skill skill)
		{
			// If target is standing still
			if (!target.IsMoving)
				return 50;

			// If target is walking
			if (target.IsWalking)
				return 70;

			// If target is running
			return 90;
		}
	}
}
