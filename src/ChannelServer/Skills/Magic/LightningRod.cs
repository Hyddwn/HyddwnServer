// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.Skills.Combat;
using Aura.Channel.World.Entities;
using Aura.Channel.World;
using Aura.Data.Database;
using Aura.Mabi.Const;
using Aura.Mabi.Network;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.Skills.Magic
{
	/// <summary>
	/// Lightning Rod Handler
	/// </summary>
	/// Var1: Min Damage
	/// Var2: Max Damage
	/// Var3: Max Chargning Time (ms)
	/// Var4: Max Charge Damage Bonus (%)
	/// Var5: ?
	[Skill(SkillId.LightningRod)]
	public class LightningRod : ISkillHandler, IPreparable, IUseable, ICompletable, ICancelable
	{
		/// <summary>
		/// Length of attack area; unofficial.
		/// </summary>
		private const int skillLength = 1400;

		/// <summary>
		/// Width of attack area; unnoficial.
		/// </summary>
		private const int skillWidth = 200;

		/// <summary>
		/// Length of target's stun
		/// </summary>
		private const int targetStun = 2000;

		/// <summary>
		/// Distance target gets knocked back
		/// </summary>
		private const int knockbackDistance = 720;

		/// <summary>
		/// Prepares the skill
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		/// <returns></returns>
		public bool Prepare(Creature creature, Skill skill, Packet packet)
		{
			if (creature.RightHand == null)
				Send.SkillPrepareSilentCancel(creature, skill.Info.Id);

			creature.StopMove();

			skill.State = SkillState.Prepared;

			Send.MotionCancel2(creature, 0);
			Send.Effect(creature, 418, 2, 0); // Magic Circle?

			Send.SkillReady(creature, skill.Info.Id);
			skill.State = SkillState.Ready;

			/* Locks -----------------------
			Walk|Run
			---------------------------- - */
			creature.Lock(Locks.Walk | Locks.Run);

			creature.Temp.LightningRodPrepareTime = DateTime.Now;

			return true;
		}

		/// <summary>
		/// Uses LightningRod
		/// </summary>
		/// <param name="attacker"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void Use(Creature attacker, Skill skill, Packet packet)
		{
			// Set full charge variable
			if (DateTime.Now >= attacker.Temp.LightningRodPrepareTime.AddMilliseconds(skill.RankData.Var3))
				attacker.Temp.LightningRodFullCharge = true;

			Log.Debug("Lightning Rod Full Charge: " + attacker.Temp.LightningRodFullCharge.ToString());

			/* Locks -----------------------
			Walk|Run
			---------------------------- - */
			attacker.Lock(Locks.Walk | Locks.Run);

			// Get direction for target Area
			var direction = Mabi.MabiMath.ByteToRadian(attacker.Direction);
			Log.Debug("New Direction: " + direction.ToString());

			var attackerPos = attacker.GetPosition();

			// The goal here is to get a random position
			// that is distance skillLength from the attacker.
			// Afterwards, it will be rotated to where the attack should be.
			var endPos = new Position(attackerPos.X + (skillLength / 2), (int)(attackerPos.Y + ((skillLength / 2) * Math.Sqrt(3)))); // 30, 60, 90 triangle.

			// Calculate Center Points
			var attackerPoint = new Point(attackerPos.X, attackerPos.Y);
			var endPoint = new Point(endPos.X, endPos.Y); // Point where length from attacker point is skillLength.

			var pivotAngle = direction - (Math.Atan(endPoint.Y / endPoint.X)); // Angle to rotate endpoint.
			endPoint = this.RotatePoint(endPoint, attackerPoint, pivotAngle); // Rotate point to where the endPoint should truly be.

			// Restate Position
			var newEndPos = new Position(endPoint.X, endPoint.Y);

			var pointDist = Math.Sqrt((skillLength * skillLength) + (skillWidth * skillWidth)); // Pythagorean Theorem - Distance between point and opposite side's center.
			var rotationAngle = Math.Asin(skillWidth / pointDist);

			// Calculate Points 1 and 2
			var posTemp1 = attackerPos.GetRelative(newEndPos, (int)(pointDist - skillLength));
			var pointTemp1 = new Point(posTemp1.X, posTemp1.Y);
			var p1 = this.RotatePoint(pointTemp1, attackerPoint, rotationAngle);
			var p2 = this.RotatePoint(pointTemp1, attackerPoint, (rotationAngle * -1));

			// Calculate Points 3 and 4
			var posTemp2 = newEndPos.GetRelative(attackerPos, (int)(pointDist - skillLength));
			var pointTemp2 = new Point(posTemp2.X, posTemp2.Y);
			var p3 = this.RotatePoint(pointTemp2, endPoint, rotationAngle);
			var p4 = this.RotatePoint(pointTemp2, endPoint, (rotationAngle * -1));

			// Debug
			var prop1 = new Prop(1, attacker.RegionId, p1.X, p1.Y, 1, 0.5f);
			var prop2 = new Prop(1, attacker.RegionId, p2.X, p2.Y, 1, 0.5f);
			var prop3 = new Prop(1, attacker.RegionId, p3.X, p3.Y, 1, 0.5f);
			var prop4 = new Prop(1, attacker.RegionId, p4.X, p4.Y, 1, 0.5f);
			attacker.Region.AddProp(prop1);
			attacker.Region.AddProp(prop2);
			attacker.Region.AddProp(prop3);
			attacker.Region.AddProp(prop4);

			System.Threading.Timer t = null;
			t = new System.Threading.Timer (_ =>
			{
				attacker.Region.RemoveProp(prop1);
				attacker.Region.RemoveProp(prop2);
				attacker.Region.RemoveProp(prop3);
				attacker.Region.RemoveProp(prop4);
				GC.KeepAlive(t);
			}, null, 10000, System.Threading.Timeout.Infinite);
			// Debug

			// TargetProp
			var LProp = new Prop(280, attacker.RegionId, endPoint.X, endPoint.Y, attacker.Direction, 1f, 0f, "single");
			attacker.Region.AddProp(LProp);

			// Prepare Combat Actions
			var cap = new CombatActionPack(attacker, skill.Info.Id);

			var targetAreaId = new Location(attacker.RegionId, newEndPos).ToLocationId();

			var aAction = new AttackerAction(CombatActionType.SpecialHit, attacker, targetAreaId);
			aAction.Set(AttackerOptions.KnockBackHit1 | AttackerOptions.UseEffect);
			aAction.PropId = LProp.EntityId;
			cap.Add(aAction);

			// Get targets in Polygon
			var targets = attacker.Region.GetCreaturesInPolygon(p1, p2, p3, p4).Where(x => attacker.CanTarget(x)).ToList();

			foreach (var target in targets)
			{
				var tAction = new TargetAction(CombatActionType.TakeHit, target, attacker, SkillId.CombatMastery);
				tAction.Set(TargetOptions.None);
				tAction.AttackerSkillId = skill.Info.Id;
				cap.Add(tAction);

				var damage = attacker.GetRndMagicDamage(skill, skill.RankData.Var1, skill.RankData.Var2);

				// Add damage if the skill is fully charged
				var dmgMultiplier = skill.RankData.Var4 / 100f;
				if (attacker.Temp.LightningRodFullCharge == true)
				{
					damage += (damage * dmgMultiplier);
				}

				// Master Title

				// Critical Hit
				var critChance = attacker.GetRightCritChance(target.MagicProtection);
				CriticalHit.Handle(attacker, critChance, ref damage, tAction);

				// MDef and MProt
				SkillHelper.HandleMagicDefenseProtection(target, ref damage);

				// Mana Shield
				ManaShield.Handle(target, ref damage, tAction);

				// Mana Deflector
				var delayReduction = ManaDeflector.Handle(attacker, target, ref damage, tAction);

				// Apply Damage
				target.TakeDamage(tAction.Damage = damage, attacker);

				// Stun Time
				tAction.Stun = targetStun;

				// Reduce stun, based on ping
				if (delayReduction > 0)
					tAction.Stun = (short)Math.Max(0, tAction.Stun - (tAction.Stun / 100 * delayReduction));

				// Death or Knockback
				if (target.IsDead)
				{
					tAction.Set(TargetOptions.FinishingKnockDown);
					attacker.Shove(target, knockbackDistance);
				}
				else
				{
					// Always knock down
					if (target.Is(RaceStands.KnockDownable))
					{
						tAction.Set(TargetOptions.KnockDown);
						attacker.Shove(target, knockbackDistance);
					}
				}
				tAction.Creature.Stun = tAction.Stun;
			}
			cap.Handle();

			Send.Effect(attacker, 418, 3, newEndPos.X, newEndPos.Y); // Lightning Shooting Effect?

			Send.SkillUse(attacker, skill.Info.Id, targetAreaId, 0, 1);

			attacker.Region.RemoveProp(LProp);
		}

		public void Complete(Creature creature, Skill skill, Packet packet)
		{
			/* Unlocks ---------------------
			Walk|Run
			---------------------------- - */
			creature.Unlock(Locks.Walk | Locks.Run);

			creature.Temp.LightningRodFullCharge = false;

			Send.Effect(creature, 418, 0); // End Magic Circle
			Send.SkillComplete(creature, skill.Info.Id);
		}

		public void Cancel(Creature creature, Skill skill)
		{
			/* Unlocks ---------------------
			Walk|Run
			---------------------------- - */
			creature.Unlock(Locks.Walk | Locks.Run);

			creature.Temp.LightningRodFullCharge = false;

			Send.Effect(creature, 418, 0); // End Magic Circle
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
