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
	/// Var1: ?
	/// Var2: Damage ?
	/// Var3: Skill Length ?
	/// Var4: Skill Width ?
	/// Var5: ?
	/// Var6: ?
	/// Var7: ?
	/// Var8: ?
	/// Var9: ?
	/// 
	/// There isn't much data on this skill, so skill variable use
	/// is mostly based on speculation from gameplay and packet data.
	/// Note: Effects only work with NPC Caliburn item.
	/// </remarks>
	[Skill(SkillId.Excalibur)]
	public class Excalibur : ISkillHandler, IPreparable, IUseable, ICompletable, ICancelable
	{
		/// <summary>
		/// Attacker's stun
		/// </summary>
		private const int AttackerStun = 0;

		/// <summary>
		/// Target's stun [Unofficial]
		/// </summary>
		private const int TargetStun = 2000;

		/// <summary>
		/// Distance the target is knocked back [Unofficial]
		/// </summary>
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
			if (creature.RightHand == null || !creature.RightHand.HasTag("/twohand/"))
			{
				Send.SkillPrepareSilentCancel(creature, skill.Info.Id);
				return false;
			}

			creature.StopMove();

			skill.State = SkillState.Prepared;

			Send.MotionCancel2(creature, 0);
			Send.Effect(creature, Effect.Excalibur, ExcaliburEffect.Prepare, 0);

			skill.State = SkillState.Ready;
			Send.SkillReady(creature, skill.Info.Id, skill.RankData.LoadTime);

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
			// Get Skill Data
			var skillDamage = skill.RankData.Var2 / 100f;
			var skillLength = (int)skill.RankData.Var3;
			var skillWidth = (int)skill.RankData.Var4;
			var radius = skillWidth / 2;

			var attackerPos = attacker.GetPosition();

			// Calculate polygon points
			var r = Mabi.MabiMath.ByteToRadian(attacker.Direction);
			var poe = attackerPos.GetRelative(r, skillLength);

			var attackerPoint = new Point(attackerPos.X, attackerPos.Y);
			var poePoint = new Point(poe.X, poe.Y);

			var pointDist = Math.Sqrt((skillLength * skillLength) + (radius * radius)); // Pythagorean Theorem - Distance between point and opposite side's center.
			var rotationAngle = Math.Asin(radius / pointDist);

			// Calculate Points 1 & 2
			var posTemp1 = attackerPos.GetRelative(poe, (int)(pointDist - skillLength));
			var pointTemp1 = new Point(posTemp1.X, posTemp1.Y);
			var p1 = this.RotatePoint(pointTemp1, attackerPoint, rotationAngle); // Rotate Positive - moves point to position where distance from poe is range and Distance from attackerPos is pointDist.
			var p2 = this.RotatePoint(pointTemp1, attackerPoint, (rotationAngle * -1)); // Rotate Negative - moves point to opposite side of p1

			// Calculate Points 3 & 4
			var posTemp2 = poe.GetRelative(attackerPos, (int)(pointDist - skillLength));
			var pointTemp2 = new Point(posTemp2.X, posTemp2.Y);
			var p3 = this.RotatePoint(pointTemp2, poePoint, rotationAngle); // Rotate Positive
			var p4 = this.RotatePoint(pointTemp2, poePoint, (rotationAngle * -1)); // Rotate Negative

			// TargetProp
			var lProp = new Prop(42, attacker.RegionId, poe.X, poe.Y, Mabi.MabiMath.ByteToRadian(attacker.Direction), 1f, 0f, "single"); // Curently a lamppost for debug. Normal prop is 280
			attacker.Region.AddProp(lProp);

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
				var tAction = new TargetAction(CombatActionType.TakeHit, target, attacker, skill.Info.Id);
				tAction.Set(TargetOptions.None);
				tAction.Delay = (int)skill.RankData.Var7; // cast time?
				cap.Add(tAction);

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

			attacker.TurnTo(poe);
			Send.Effect(attacker, Effect.Excalibur, ExcaliburEffect.Attack, poe.X, poe.Y);
			Send.SkillUse(attacker, skill.Info.Id, targetAreaId, 0, 1);

			// Debug
			Task.Delay(10000).ContinueWith(_ =>
			{
				attacker.Region.RemoveProp(lProp);
			});
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
