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
using System.Drawing;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.Skills.Combat
{
	/// <summary>
	/// Skill handler for Excalibur
	/// </summary>
	/// <remarks>
	/// Var1: Charging Time
	/// Var2: Damage ?
	/// Var3: Skill Length
	/// Var4: Skill Width
	/// Var5: ?
	/// Var6: ?
	/// Var7: ?
	/// Var8: ?
	/// Var9: ?
	/// 
	/// There isn't much data on this skill, so skill variable use
	/// is mostly based on speculation from gameplay and packet data.
	/// Note: Effects only fully work with [Caliburn (For NPC)] and Female Character
	/// </remarks>
	[Skill(SkillId.Excalibur)]
	public class Excalibur : ISkillHandler, IPreparable, IUseable, ICompletable, ICancelable
	{
		/// <summary>
		/// Attacker's stun
		/// </summary>
		private const int AttackerStun = 0;

		/// <summary>
		/// Target's stun
		/// </summary>
		private const int TargetStun = 5000;

		/// <summary>
		/// Distance the target is knocked back
		/// </summary>
		private const int KnockbackDistance = 300;

		/// <summary>
		/// Prepares the skill
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		/// <returns></returns>
		public bool Prepare(Creature creature, Skill skill, Packet packet)
		{
			// 2-Hand check
			if (creature.RightHand == null || !creature.RightHand.HasTag("/twohand/"))
			{
				Send.SkillPrepareSilentCancel(creature, skill.Info.Id);
				return false;
			}

			// Stop movement
			creature.StopMove();

			Send.MotionCancel2(creature, 0);
			Send.Effect(creature, Effect.Excalibur, ExcaliburEffect.Prepare, 0);

			skill.State = SkillState.Ready;
			Send.SkillReady(creature, skill.Info.Id, skill.RankData.LoadTime);

			creature.Temp.ExcaliburPrepareTime = DateTime.Now;

			return true;
		}

		/// <summary>
		/// Uses the skill
		/// </summary>
		/// <param name="attacker"></param>
		/// <param name="skill"></param>
		/// <param name="targetEntityId"></param>
		/// <returns></returns>
		public void Use(Creature attacker, Skill skill, Packet packet)
		{
			// Check for full charge
			if (DateTime.Now < attacker.Temp.ExcaliburPrepareTime.AddMilliseconds(skill.RankData.Var1)) // Not enough time has passed during charging
			{
				Send.SkillUseSilentCancel(attacker);
				Send.Effect(attacker, Effect.Excalibur, ExcaliburEffect.Cancel);
				return;
			}
			
			// Skill Data
			var skillDamage = skill.RankData.Var2 / 100f;
			var skillLength = (int)skill.RankData.Var3;
			var skillWidth = (int)skill.RankData.Var4;

			// Get targets in rectangular area
			Position endPos; // Position on the skill area rectangle opposite of the attacker
			var targets = SkillHelper.GetTargetableCreaturesInSkillArea(attacker, skillLength, skillWidth, out endPos);

			// TargetProp
			var lProp = new Prop(280, attacker.RegionId, endPos.X, endPos.Y, Mabi.MabiMath.ByteToRadian(attacker.Direction), 1f, 0f, "single");
			attacker.Region.AddProp(lProp);

			// Turn to target area
			attacker.TurnTo(endPos);

			// Prepare Combat Actions
			var cap = new CombatActionPack(attacker, skill.Info.Id);

			var targetAreaId = new Location(attacker.RegionId, endPos).ToLocationId();

			var aAction = new AttackerAction(CombatActionType.SpecialHit, attacker, targetAreaId);
			aAction.Set(AttackerOptions.UseEffect);
			aAction.PropId = lProp.EntityId;
			cap.Add(aAction);

			var rnd = RandomProvider.Get();

			// Check crit
			var crit = false;
			var critSkill = attacker.Skills.Get(SkillId.CriticalHit);
			if (critSkill != null && critSkill.Info.Rank > SkillRank.Novice)
			{
				var critChance = Math2.Clamp(0, 30, attacker.GetTotalCritChance(0));
				if (rnd.NextDouble() * 100 < critChance)
					crit = true;
			}

			foreach (var target in targets)
			{
				var tAction = new TargetAction(CombatActionType.TakeHit, target, attacker, SkillId.CombatMastery);
				tAction.Set(TargetOptions.None);
				tAction.Delay = 1200;
				cap.Add(tAction);

				// Stop target movement
				target.StopMove();

				var damage = (attacker.GetRndTotalDamage() * skillDamage);

				// Critical Hit
				if (crit)
				{
					var bonus = critSkill.RankData.Var1 / 100f;
					damage = damage + (damage * bonus);

					tAction.Set(TargetOptions.Critical);
				}

				// Def and Prot
				SkillHelper.HandleDefenseProtection(target, ref damage);

				// Defense
				Defense.Handle(aAction, tAction, ref damage);

				// Heavy Stander
				HeavyStander.Handle(attacker, target, ref damage, tAction);

				// Conditions
				SkillHelper.HandleConditions(attacker, target, ref damage);

				// Mana Shield
				ManaShield.Handle(target, ref damage, tAction);

				// Apply Damage
				target.TakeDamage(tAction.Damage = damage, attacker);

				// Aggro
				target.Aggro(attacker);

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
					// Always knock down
					if (target.Is(RaceStands.KnockDownable))
					{
						tAction.Set(TargetOptions.KnockDown);
						attacker.Shove(target, KnockbackDistance);
					}
				}
			}

			// Update current weapon
			SkillHelper.UpdateWeapon(attacker, targets.FirstOrDefault(), ProficiencyGainType.Melee, attacker.RightHand);

			cap.Handle();

			Send.Effect(attacker, Effect.Excalibur, ExcaliburEffect.Attack, (float)endPos.X, (float)endPos.Y);
			Send.SkillUse(attacker, skill.Info.Id, targetAreaId, 0, 1);

			// Remove skill prop
			attacker.Region.RemoveProp(lProp);
		}

		/// <summary>
		/// Completes the skill
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void Complete(Creature creature, Skill skill, Packet packet)
		{
			Send.Effect(creature, Effect.Excalibur, ExcaliburEffect.Cancel);
			Send.SkillComplete(creature, skill.Info.Id);
		}

		/// <summary>
		/// Cancels the skill
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void Cancel(Creature creature, Skill skill)
		{
			Send.Effect(creature, Effect.Excalibur, ExcaliburEffect.Cancel);
			Send.SkillCancel(creature);
		}
	}
}
