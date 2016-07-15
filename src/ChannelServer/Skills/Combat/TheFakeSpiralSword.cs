// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.Skills.Magic;
using Aura.Channel.Skills.Combat;
using Aura.Channel.World;
using Aura.Channel.World.Entities;
using Aura.Data.Database;
using Aura.Mabi.Const;
using Aura.Mabi.Network;
using Aura.Shared.Network;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.Skills.Combat
{
	/// <summary>
	/// Skill handler for The Fake Spiral Sword
	/// </summary>
	/// Var1: Damage Percentage ?
	/// Var2: Attack Range ?
	/// Var3: Explosion Radius ?
	/// Var4: ?
	/// Var5: ?
	[Skill(SkillId.TheFakeSpiralSword)]
	public class TheFakeSpiralSword : ISkillHandler, IPreparable, IReadyable, ICombatSkill, ICompletable, ICancelable
	{
		/// <summary>
		/// Attacker's stun
		/// </summary>
		private const int AttackerStun = 500;

		/// <summary>
		/// Target's stun
		/// </summary>
		private const int TargetStun = 2000;

		private const int KnockbackDistance = 250;

		/// <summary>
		/// Prepares the skill
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		/// <returns></returns>
		public bool Prepare(Creature creature, Skill skill, Packet packet)
		{
			Send.Effect(creature, Effect.TheFakeSpiralSword, (byte)1, (long)63604117580388, 1500);

			Send.SkillPrepare(creature, skill.Info.Id, skill.GetCastTime());

			return true;
		}

		/// <summary>
		/// Readies the skill
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		/// <returns></returns>
		public bool Ready(Creature creature, Skill skill, Packet packet)
		{
			skill.Stacks += 1;

			Send.Effect(creature, Effect.TheFakeSpiralSword, (byte)2);

			Send.SkillReady(creature, skill.Info.Id);

			return true;
		}

		/// <summary>
		/// Uses the skill
		/// </summary>
		/// <param name="attacker"></param>
		/// <param name="skill"></param>
		/// <param name="targetEntityId"></param>
		/// <returns></returns>
		public CombatSkillResult Use(Creature attacker, Skill skill, long targetEntityId)
		{
			// Get Target
			var target = attacker.Region.GetCreature(targetEntityId);

			// Check Target
			if (target == null)
				return CombatSkillResult.InvalidTarget;

			var targetPos = target.GetPosition();

			// Check for Collisions
			if (attacker.Region.Collisions.Any(attacker.GetPosition(), targetPos))
				return CombatSkillResult.InvalidTarget;

			// Check Range
			var range = (int)skill.RankData.Var2;
			if (!attacker.GetPosition().InRange(targetPos, range))
			{
				Send.Notice(attacker, Localization.Get("You are too far away."));
				return CombatSkillResult.OutOfRange;
			}

			attacker.StopMove();
			target.StopMove();

			// Effects
			Send.Effect(attacker, Effect.TheFakeSpiralSword, (byte)3, (long)63604117582936, (byte)1);

			// Prepare Combat Actions
			var cap = new CombatActionPack(attacker, skill.Info.Id);

			var aAction = new AttackerAction(CombatActionType.RangeHit, attacker, targetEntityId);
			aAction.Set(AttackerOptions.Result | AttackerOptions.KnockBackHit1 | AttackerOptions.KnockBackHit2);
			cap.Add(aAction);

			aAction.Stun = AttackerStun;

			var explosionRadius = (int)skill.RankData.Var3;

			var targets = attacker.Region.GetCreaturesInRange(targetPos, explosionRadius).Where(x => attacker.CanTarget(x) && !attacker.Region.Collisions.Any(targetPos, x.GetPosition()));

			var rnd = RandomProvider.Get();

			// Check crit
			var crit = false;
			if (attacker.Skills.Has(SkillId.CriticalHit, SkillRank.RF))
			{
				var critChance = attacker.GetRightCritChance(0);
				crit = (rnd.Next(100) < critChance);
			}

			foreach (var explosionTarget in targets)
			{
				var tAction = new TargetAction(CombatActionType.TakeHit, explosionTarget, attacker, SkillId.CombatMastery);
				tAction.Set(TargetOptions.Result);
				cap.Add(tAction);

				// Damage
				var damage = (attacker.GetRndRightHandDamage() * (skill.RankData.Var1 / 100f));

				// Critical Hit
				if (crit)
					CriticalHit.Handle(attacker, 100, ref damage, tAction);

				// Defense and Prot
				SkillHelper.HandleDefenseProtection(target, ref damage);

				// Defense
				Defense.Handle(aAction, tAction, ref damage);

				// Mana Shield
				ManaShield.Handle(target, ref damage, tAction);

				// Heavy Stander
				HeavyStander.Handle(attacker, target, ref damage, tAction);

				// Apply Damage
				target.TakeDamage(tAction.Damage = damage, attacker);

				// Aggro
				// target.Aggro(attacker);

				// Stun Time
				tAction.Stun = TargetStun;

				// Death or Knockback
                if (target.IsDead)
				{
					tAction.Set(TargetOptions.FinishingKnockDown);
					attacker.Shove(target, KnockbackDistance);
				}
				else
				{
					// Always knock back
					if (target.Is(RaceStands.KnockBackable))
					{
						tAction.Set(TargetOptions.KnockBack);
						attacker.Shove(target, KnockbackDistance);
					}
					tAction.Creature.Stun = tAction.Stun;
				}
			}

			aAction.Creature.Stun = aAction.Stun;
			cap.Handle();

			return CombatSkillResult.Okay;
		}

		/// <summary>
		/// Completes the skill
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void Complete(Creature creature, Skill skill, Packet packet)
		{

		}

		/// <summary>
		/// Cancels the skill
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void Cancel(Creature creature, Skill skill)
		{
		}
	}
}
