using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.Skills.Magic;
using Aura.Channel.Skills.Combat;
using Aura.Channel.World;
using Aura.Channel.World.Entities;
using Aura.Mabi;
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

namespace Aura.Channel.Skills.Fighter
{
	/// <summary>
	/// Charging Strike skill handler
	/// </summary>
	/// Var1: Damage
	/// Var2: Cooldown Decreased
	/// Var3: Range
	[Skill(SkillId.ChargingStrike)]
	public class ChargingStrike : ISkillHandler, IPreparable, IReadyable, IUseable, ICancelable // also IInitiable
	{
		/// <summary>
		/// Attacker's stun after skill use
		/// </summary>
		private const int AttackerStun = 0;

		/// <summary>
		/// Target's stun after being hit
		/// </summary>
		private const int TargetStun = 4000;

		/// <summary>
		/// Target's stability reduction on hit
		/// </summary>
		private const int StabilityReduction = 10;

		/// <summary>
		/// Target's knockback distance if killed
		/// </summary>
		private const int KnockbackDistance = 190;

		public bool Prepare(Creature creature, Skill skill, Packet packet)
		{
			// Item check
			if (creature.RightHand == null)
				return false;

			// Unlock Walk/Run since it oddly locks the user
			creature.Unlock(Locks.Walk | Locks.Run);

			Send.SkillPrepare(creature, skill.Info.Id, skill.GetCastTime());
			return true;
		}

		public bool Ready(Creature creature, Skill skill, Packet packet)
		{
			// Unlock Walk/Run since it oddly locks the user
			creature.Unlock(Locks.Walk | Locks.Run);

			Send.SkillReady(creature, skill.Info.Id);
			return true;
		}

		public void Use(Creature attacker, Skill skill, Packet packet)
		{
			// Get Target
			var targetEntityId = packet.GetLong();

			var target = attacker.Region.GetCreature(targetEntityId);

			// Stop movement
			attacker.StopMove();
			target.StopMove();

			var attackerPos = attacker.GetPosition();
			var targetPos = target.GetPosition();

			// Check target + collisions
			if (target == null || attacker.Region.Collisions.Any(attackerPos, targetPos))
			{
				Send.SkillUseSilentCancel(attacker);
				attacker.Unlock(Locks.All);
				return;
			}

			// Effects
			Send.EffectDelayed(attacker, attackerPos.GetDistance(targetPos), Effect.ChargingStrike, (byte)0, targetEntityId);

			// Conditions
			var extra = new MabiDictionary();
			extra.SetBool("CONDITION_FAST_MOVE_NO_LOCK", false);
			attacker.Conditions.Activate(ConditionsC.FastMove, extra);

			Send.ForceRunTo(attacker, targetPos);
			attacker.SetPosition(targetPos.X, targetPos.Y);

			Send.SkillUse(attacker, skill.Info.Id, targetEntityId);

			Send.EffectDelayed(attacker, attackerPos.GetDistance(targetPos), Effect.ChargingStrike, (byte)1, targetEntityId);

			// Prepare Combat Actions
			var cap = new CombatActionPack(attacker, skill.Info.Id);

			var aAction = new AttackerAction(CombatActionType.SpecialHit, attacker, targetEntityId);
			aAction.Set(AttackerOptions.UseEffect);
			aAction.PropId = targetEntityId;

			var tAction = new TargetAction(CombatActionType.TakeHit, target, attacker, skill.Info.Id);
			tAction.Set(TargetOptions.Result);
			tAction.Delay = attackerPos.GetDistance(targetPos);

			cap.Add(aAction, tAction);

			// Damage
			var damage = (attacker.GetRndFighterDamage() * (skill.RankData.Var1 / 100f));

			// Chain Mastery Damage Bonus
			var chainMasterySkill = attacker.Skills.Get(SkillId.ChainMastery);
			var damageBonus = (chainMasterySkill == null ? 0 : chainMasterySkill.RankData.Var1);
			damage += damage * (damageBonus / 100f);

			// Master Title - Damage +30%
			if (attacker.Titles.SelectedTitle == skill.Data.MasterTitle)
				damage += (damage * 0.3f);

			// Critical Hit
			var critChance = attacker.GetRightCritChance(target.Protection);
			CriticalHit.Handle(attacker, critChance, ref damage, tAction);

			// Handle skills and reductions
			SkillHelper.HandleDefenseProtection(target, ref damage);
			HeavyStander.Handle(attacker, target, ref damage, tAction);
			SkillHelper.HandleConditions(attacker, target, ref damage);
			ManaShield.Handle(target, ref damage, tAction);

			// Apply Damage
			target.TakeDamage(tAction.Damage = damage, attacker);

			// Aggro
			target.Aggro(attacker);

			// Stun Times
			tAction.Stun = TargetStun;
			aAction.Stun = AttackerStun;

			// Death and Knockback
			if (target.IsDead)
			{
				tAction.Set(TargetOptions.FinishingKnockDown);
				attacker.Shove(target, KnockbackDistance);
			}
			else // This skill never knocks back normally
			{
				if (!target.IsKnockedDown)
					target.Stability -= StabilityReduction;
			}
			cap.Handle();

			attacker.Conditions.Deactivate(ConditionsC.FastMove);
			Send.SkillComplete(attacker, skill.Info.Id);
			
			// Chain Start
			attacker.Temp.ChainStartTime = DateTime.Now;

			// For later skills, use this as a time check.
			// Example for spinning uppercut
			// if (DateTime.Now <= attacker.Temp.ChainStartTime.AddMilliseconds(ChainMasteryInterval.Stage2))
			//	  prepare, ready, etc
			// else
			//	  dont prepare, skill unusable

			attacker.Skills.ActiveSkill = null;

			// Charging strike locks EVERYTHING for some reason...
			attacker.Unlock(Locks.All);
		}

		public void Cancel(Creature creature, Skill skill)
		{
		
		}
	}
}
