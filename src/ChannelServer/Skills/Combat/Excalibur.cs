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

			skill.State = SkillState.Prepared;

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
			var skillRadius = ((int)skill.RankData.Var4) / 2;

			var attackerPos = attacker.GetPosition();

			// Calculate polygon points
			var r = Mabi.MabiMath.ByteToRadian(attacker.Direction);
			var poe = attackerPos.GetRelative(r, skillLength);

			var attackerPoint = new Point(attackerPos.X, attackerPos.Y);
			var poePoint = new Point(poe.X, poe.Y);
			var pointDist = Math.Sqrt((skillLength * skillLength) + (skillRadius * skillRadius));
			var rotationAngle = Math.Asin(skillRadius / pointDist);

			var posTemp1 = attackerPos.GetRelative(poe, (int)(pointDist - skillLength));
			var pointTemp1 = new Point(posTemp1.X, posTemp1.Y);
			var p1 = this.RotatePoint(pointTemp1, attackerPoint, rotationAngle);
			var p2 = this.RotatePoint(pointTemp1, attackerPoint, (rotationAngle * -1));

			var posTemp2 = poe.GetRelative(attackerPos, (int)(pointDist - skillLength));
			var pointTemp2 = new Point(posTemp2.X, posTemp2.Y);
			var p3 = this.RotatePoint(pointTemp2, poePoint, rotationAngle);
			var p4 = this.RotatePoint(pointTemp2, poePoint, (rotationAngle * -1));

			// TargetProp
			var lProp = new Prop(280, attacker.RegionId, poe.X, poe.Y, Mabi.MabiMath.ByteToRadian(attacker.Direction), 1f, 0f, "single");
			attacker.Region.AddProp(lProp);

			// Turn to target area
			attacker.TurnTo(poe);

			// Prepare Combat Actions
			var cap = new CombatActionPack(attacker, skill.Info.Id);

			var targetAreaId = new Location(attacker.RegionId, poe).ToLocationId();

			var aAction = new AttackerAction(CombatActionType.SpecialHit, attacker, targetAreaId);
			aAction.Set(AttackerOptions.UseEffect);
			aAction.PropId = lProp.EntityId;
			cap.Add(aAction);

			// Get targets in Polygon - includes collission check
			var targets = attacker.Region.GetCreaturesInPolygon(p1, p2, p3, p4).Where(x => attacker.CanTarget(x) && !attacker.Region.Collisions.Any(attacker.GetPosition(), x.GetPosition())).ToList();

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

			Send.Effect(attacker, Effect.Excalibur, ExcaliburEffect.Attack, (float)poe.X, (float)poe.Y);
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

		private Point RotatePoint(Point point, Point pivot, double radians)
		{
			var cosTheta = Math.Cos(radians);
			var sinTheta = Math.Sin(radians);

			var x = (int)(cosTheta * (point.X - pivot.X) - sinTheta * (point.Y - pivot.Y) + pivot.X);
			var y = (int)(sinTheta * (point.X - pivot.X) + cosTheta * (point.Y - pivot.Y) + pivot.Y);

			return new Point(x, y);
		}
	}
}
