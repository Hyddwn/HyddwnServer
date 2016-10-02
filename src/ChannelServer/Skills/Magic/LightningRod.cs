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
using Aura.Mabi;

namespace Aura.Channel.Skills.Magic
{
	/// <summary>
	/// Lightning Rod Handler
	/// </summary>
	/// Var1: Min Damage
	/// Var2: Max Damage
	/// Var3: Max Chargning Time (ms)
	/// Var4: Max Charge Damage Bonus (%)
	/// Var5: Cooldown
	[Skill(SkillId.LightningRod)]
	public class LightningRod : ISkillHandler, IPreparable, IUseable, ICompletable, ICancelable, IInitiableSkillHandler
	{
		/// <summary>
		/// Length of attack area; unconfirmed.
		/// </summary>
		/// <remarks>
		/// "http://wiki.mabinogiworld.com/view/Lightning_Rod#Summary"
		/// </remarks>
		private const int SkillLength = 1400;

		/// <summary>
		/// Width of attack area; unconfirmed.
		/// </summary>
		/// <remarks>
		/// "http://wiki.mabinogiworld.com/view/Lightning_Rod#Summary"
		/// </remarks>
		private const int SkillWidth = 200;

		/// <summary>
		/// Length of target's stun
		/// </summary>
		private const int TargetStun = 2000;

		/// <summary>
		/// Distance target gets knocked back
		/// </summary>
		private const int KnockbackDistance = 720;

		/// <summary>
		/// Time for mana degeneration (ms)
		/// </summary>
		private const int DegenTime = 1000;

		/// <summary>
		/// Effect Enums
		/// </summary>
		private enum LightningRodEffect : byte
		{
			/// <summary>
			/// Cancels the entire effect
			/// </summary>
			Cancel = 0,

			/// <summary>
			/// Contains magic circle and lightning ball.
			/// </summary>
			Prepare = 2,

			/// <summary>
			/// Burst of lightning; the effect for the attack.
			/// </summary>
			Attack = 3,
		}

		/// <summary>
		/// Subscribes handlers to events required for training.
		/// </summary>
		public void Init()
		{
			ChannelServer.Instance.Events.CreatureAttackedByPlayer += this.OnCreatureAttackedByPlayer;
			ChannelServer.Instance.Events.CreatureAttacks += this.OnCreatureAttacks;
		}

		/// <summary>
		/// Prepares the skill
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		/// <returns></returns>
		public bool Prepare(Creature creature, Skill skill, Packet packet)
		{
			if (creature.RightHand == null || !creature.RightHand.HasTag("/staff/"))
			{
				Send.SkillPrepareSilentCancel(creature, skill.Info.Id);
				return false;
			}

			creature.StopMove();

			skill.State = SkillState.Prepared;

			Send.MotionCancel2(creature, 0);
			Send.Effect(creature, Effect.LightningRod, (int)LightningRodEffect.Prepare, 0);

			Send.SkillReady(creature, skill.Info.Id);
			skill.State = SkillState.Ready;

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
			attacker.Temp.LightningRodFullCharge = (DateTime.Now >= attacker.Temp.LightningRodPrepareTime.AddMilliseconds(skill.RankData.Var3));

			// Get direction for target Area
			var direction = Mabi.MabiMath.ByteToRadian(attacker.Direction);

			var attackerPos = attacker.GetPosition();

			// Calculate polygon points
			var r = MabiMath.ByteToRadian(attacker.Direction);
			var poe = attackerPos.GetRelative(r, 800);
			var pivot = new Point(poe.X, poe.Y);
			var p1 = new Point(pivot.X - SkillLength / 2, pivot.Y - SkillWidth / 2);
			var p2 = new Point(pivot.X - SkillLength / 2, pivot.Y + SkillWidth / 2);
			var p3 = new Point(pivot.X + SkillLength / 2, pivot.Y + SkillWidth / 2);
			var p4 = new Point(pivot.X + SkillLength / 2, pivot.Y - SkillWidth / 2);
			p1 = this.RotatePoint(p1, pivot, r);
			p2 = this.RotatePoint(p2, pivot, r);
			p3 = this.RotatePoint(p3, pivot, r);
			p4 = this.RotatePoint(p4, pivot, r);

			// TargetProp
			var lProp = new Prop(280, attacker.RegionId, poe.X, poe.Y, MabiMath.ByteToRadian(attacker.Direction), 1f, 0f, "single");
			attacker.Region.AddProp(lProp);

			// Prepare Combat Actions
			var cap = new CombatActionPack(attacker, skill.Info.Id);

			var targetAreaId = new Location(attacker.RegionId, poe).ToLocationId();

			var aAction = new AttackerAction(CombatActionType.SpecialHit, attacker, targetAreaId);
			aAction.Set(AttackerOptions.KnockBackHit1 | AttackerOptions.UseEffect);
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
				tAction.AttackerSkillId = skill.Info.Id;
				cap.Add(tAction);

				var damage = attacker.GetRndMagicDamage(skill, skill.RankData.Var1, skill.RankData.Var2);

				// Add damage if the skill is fully charged
				var dmgMultiplier = skill.RankData.Var4 / 100f;
				if (attacker.Temp.LightningRodFullCharge)
				{
					damage += (damage * dmgMultiplier);
				}

				// Critical Hit
				if (crit)
				{
					var bonus = critSkill.RankData.Var1 / 100f;
					damage = damage + (damage * bonus);

					tAction.Set(TargetOptions.Critical);
				}

				// MDef and MProt
				SkillHelper.HandleMagicDefenseProtection(target, ref damage);

				// Conditions
				SkillHelper.HandleConditions(attacker, target, ref damage);

				// Mana Deflector
				var delayReduction = ManaDeflector.Handle(attacker, target, ref damage, tAction);

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

			Send.Effect(attacker, Effect.LightningRod, (int)LightningRodEffect.Attack, poe.X, poe.Y);

			Send.SkillUse(attacker, skill.Info.Id, targetAreaId, 0, 1);
			skill.Train(1); // Use the Skill

			attacker.Region.RemoveProp(lProp);
		}

		public void Complete(Creature creature, Skill skill, Packet packet)
		{
			creature.Temp.LightningRodFullCharge = false;

			Send.Effect(creature, Effect.LightningRod, (int)LightningRodEffect.Cancel);
			Send.SkillComplete(creature, skill.Info.Id);
		}

		public void Cancel(Creature creature, Skill skill)
		{
			creature.Temp.LightningRodFullCharge = false;

			Send.Effect(creature, Effect.LightningRod, (int)LightningRodEffect.Cancel);
		}

		/// <summary>
		/// Training, called when someone attacks something.
		/// </summary>
		/// <param name="action"></param>
		public void OnCreatureAttackedByPlayer(TargetAction action)
		{
			// Check if skill used is LightningRod
			if (action.AttackerSkillId != SkillId.LightningRod)
				return;

			// Get skill
			var attackerSkill = action.Attacker.Skills.Get(SkillId.LightningRod);
			if (attackerSkill == null) return; // Should be impossible.

			// Learning by attacking
			switch (attackerSkill.Info.Rank)
			{
				case SkillRank.RF:
				case SkillRank.RE:
					attackerSkill.Train(2); // Attack an enemy
					break;
				case SkillRank.RD:
				case SkillRank.RC:
					attackerSkill.Train(2); // Attack an enemy
					if (action.Attacker.Temp.LightningRodFullCharge) attackerSkill.Train(3); // Attack an Enemy with a Max Charge
					break;
				case SkillRank.RB:
				case SkillRank.RA:
				case SkillRank.R9:
				case SkillRank.R8:
				case SkillRank.R7:
				case SkillRank.R6:
				case SkillRank.R5:
				case SkillRank.R4:
				case SkillRank.R3:
				case SkillRank.R2:
				case SkillRank.R1:
					if (action.Creature.IsDead) attackerSkill.Train(2); // Defeat an enemy
					if (action.Creature.IsDead && action.Attacker.Temp.LightningRodFullCharge) attackerSkill.Train(3); // Defeat an Enemy with a Max Charge
					break;
			}
		}

		/// <summary>
		/// Training, called when a creature attacks another creature(s)
		/// </summary>
		/// <param name="aAction"></param>
		public void OnCreatureAttacks(AttackerAction aAction)
		{
			// Handles the multiple target training requirements

			// Check if skill used is LightningRod
			if (aAction.SkillId != SkillId.LightningRod)
				return;

			// Get skill
			var attackerSkill = aAction.Creature.Skills.Get(SkillId.LightningRod);
			if (attackerSkill == null) return; // Should be impossible.

			// Get targets
			var targets = aAction.Pack.GetTargets();

			// Kill count
			var killCount = targets.Where(a => a.IsDead).Count();

			// Learning by attacking
			switch (attackerSkill.Info.Rank)
			{
				case SkillRank.RF:
				case SkillRank.RE:
				case SkillRank.RD:
				case SkillRank.RC:
				case SkillRank.RB:
				case SkillRank.RA:
				case SkillRank.R9:
				case SkillRank.R8:
				case SkillRank.R7:
					if (killCount >= 2) // Defeat 2 or more enemies
						attackerSkill.Train(4);
					break;
				case SkillRank.R6:
				case SkillRank.R5:
				case SkillRank.R4:
					if (killCount >= 3) // Defeat 3 or more enemies
						attackerSkill.Train(4);
					break;
				case SkillRank.R3:
				case SkillRank.R2:
					if (killCount >= 4) // Defeat 4 or more enemies
					{
						attackerSkill.Train(4);

						if (aAction.Creature.Temp.LightningRodFullCharge) // Defeat 4 or more Enemies with a Max Charge
							attackerSkill.Train(5);
					}
					break;
				case SkillRank.R1:
					if (killCount >= 5) // Defeat 5 or more enemies
					{
						attackerSkill.Train(4);

						if (aAction.Creature.Temp.LightningRodFullCharge) // Defeat 5 or more Enemies with a Max Charge
							attackerSkill.Train(5);
					}
					break;
			}
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
