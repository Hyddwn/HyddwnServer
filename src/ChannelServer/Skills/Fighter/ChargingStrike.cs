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
	public class ChargingStrike : ISkillHandler, IPreparable, IReadyable, ICombatSkill, ICancelable // also IInitiable
	{
		private const int AttackerStun = 0;
		private const int TargetStun = 4000;
		private const int StabilityReduction = 10;
		private const int KnockbackDistance = 190; // Only knockback on death or deadly

		public bool Prepare(Creature creature, Skill skill, Packet packet)
		{
			// Item check
			if (creature.RightHand == null)
				return false;

			// Unlock Walk/Run since it oddly locks you??
			creature.Unlock(Locks.Walk | Locks.Run);

			Send.SkillPrepare(creature, skill.Info.Id, skill.GetCastTime());
			return true;
		}

		public bool Ready(Creature creature, Skill skill, Packet packet)
		{
			// Unlock Walk/Run since it oddly locks you??
			creature.Unlock(Locks.Walk | Locks.Run);

			Send.SkillReady(creature, skill.Info.Id);
			return true;
		}

		public CombatSkillResult Use(Creature attacker, Skill skill, long targetEntityId)
		{
			// Get Target
			var target = attacker.Region.GetCreature(targetEntityId);

			// Stop movement
			attacker.StopMove();
			target.StopMove();

			var attackerPos = attacker.GetPosition();
			var targetPos = target.GetPosition();

			// Check target + collisions
			if (target == null || attacker.Region.Collisions.Any(attackerPos, targetPos))
				return CombatSkillResult.InvalidTarget;

			// Check Range
			if (!attackerPos.InRange(targetPos, (int)skill.RankData.Var3))
				return CombatSkillResult.OutOfRange;

			// Effects
			Send.EffectDelayed(attacker, 494, Effect.ChargingStrike, (byte)0, targetEntityId);

			// Conditions
			var extra = new MabiDictionary();
			extra.SetBool("CONDITION_FAST_MOVE_NO_LOCK", false);
			attacker.Conditions.Activate(ConditionsC.FastMove, extra);

			Send.ForceRunTo(attacker, targetPos);

			Send.SkillUse(attacker, skill.Info.Id, targetEntityId);

			Send.Effect(attacker, Effect.ChargingStrike, (byte)1, targetEntityId);

			// Prepare Combat Actions
			var cap = new CombatActionPack(attacker, skill.Info.Id);

			var aAction = new AttackerAction(CombatActionType.SpecialHit, attacker, targetEntityId);
			aAction.Set(AttackerOptions.UseEffect);
			aAction.PropId = targetEntityId;

			// Chain Mastery Damage Increase

			var tAction = new TargetAction(CombatActionType.TakeHit, target, attacker, skill.Info.Id);

			attacker.Conditions.Deactivate(ConditionsC.FastMove);
			Send.SkillComplete(attacker, skill.Info.Id);

			return CombatSkillResult.Okay;
		}

		public void Cancel(Creature creature, Skill skill)
		{
		
		}
	}
}
