// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence.txt in the main folder

using System;
using System.Threading.Tasks;
using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.Skills.Combat;
using Aura.Channel.Skills.Magic;
using Aura.Channel.World;
using Aura.Channel.World.Entities;
using Aura.Data.Database;
using Aura.Mabi.Const;
using Aura.Mabi.Network;
using Aura.Shared.Util;

namespace Aura.Channel.Skills.Fighter
{
	/// <summary>
	/// Dropkick skill handler
	/// </summary>
	/// <remarks>
	/// STAGE 3 CHAIN
	/// Var1: Damage
	/// Var2: Cooldown Decreased
	/// Var3: Splash Width
	/// Var4: Splash Length
	/// Var5: Splash Damage
	/// </remarks>
	[Skill(SkillId.DropKick)]
	public class DropKick : ISkillHandler, IPreparable, ICompletable, ICancelable, IInitiableSkillHandler
	{
		/// <summary>
		/// Attacker's stun after skill use
		/// </summary>
		private const int AttackerStun = 800;

		/// <summary>
		/// Target's stun after being hit
		/// </summary>
		private const int TargetStun = 2000;

		/// <summary>
		/// Target's stability reduction on hit
		/// </summary>
		private const int StabilityReduction = 100;

		/// <summary>
		/// Subscribes handlers to events required for training.
		/// </summary>
		public void Init()
		{
			ChannelServer.Instance.Events.CreatureAttackedByPlayer += this.OnCreatureAttackedByPlayer;
			ChannelServer.Instance.Events.CreatureAttacks += this.OnCreatureAttacks;
		}

		/// <summary>
		/// Prepares and uses the skill
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		/// <returns></returns>
		public bool Prepare(Creature creature, Skill skill, Packet packet)
		{
			// Item check
			if (creature.RightHand == null)
				return false;

			// Chain Check - Stage 3
			if (creature.Temp.FighterChainLevel != 3 || DateTime.Now > creature.Temp.FighterChainStartTime.AddMilliseconds((double)ChainMasteryInterval.Stage3))
				return false;

			// Heavy Armor check
			var chainMasterySkill = creature.Skills.Get(SkillId.ChainMastery);
			if (creature.HasEquipped("/heavyarmor/") && (chainMasterySkill == null || chainMasterySkill.RankData.Var3 == 0))
				return false;

			// Discard unused packet string
			if (packet.Peek() == PacketElementType.String)
				packet.GetString();

			// No target entity ID...
			if (packet.Peek() == PacketElementType.None)
			{
				Send.SkillPrepareSilentCancel(creature, skill.Info.Id);
				return false;
			}

			// In the case that there is a Long in the packet...
			var targetEntityId = packet.GetLong();
			var target = creature.Region.GetCreature(targetEntityId);
			if (target == null)
			{
				Send.SkillPrepareSilentCancel(creature, skill.Info.Id);
				return false;
			}

			// Unable to move
			creature.StopMove();
			creature.Lock(Locks.Walk | Locks.Run);
			creature.TurnTo(target.GetPosition());

			// Effects
			Send.Effect(creature, Effect.DropKick, (byte)10);
			Send.EffectDelayed(creature, 100, Effect.DropKick2, "G16_F_Skill_D_kick_ready", 5000, 0.0f, (byte)0);
			Send.EffectDelayed(creature, 200, Effect.DropKick, (byte)0, creature.EntityId);

			skill.State = SkillState.Ready;

			// Wait for animation, cancel skill if the creature is stunned before the animation ends
			Task.Delay(1000).ContinueWith(_ =>
			{
				if (!creature.IsStunned)
					this.UseSkill(creature, skill, targetEntityId);
				else
					Send.SkillPrepareSilentCancel(creature, skill.Info.Id);
			});

			return true;
		}

		/// <summary>
		/// Uses the skill
		/// </summary>
		/// <param name="attacker"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void UseSkill(Creature attacker, Skill skill, long targetEntityId)
		{
			var target = attacker.Region.GetCreature(targetEntityId);

			var attackerPos = attacker.GetPosition();
			var targetPos = target.GetPosition();

			// Check target + collisions
			if (target == null || attacker.Region.Collisions.Any(attackerPos, targetPos))
			{
				Send.SkillUseSilentCancel(attacker);
				return;
			}

			// Stop movement
			target.StopMove();

			Send.SkillUseEntity(attacker, skill.Info.Id, targetEntityId);
			skill.State = SkillState.Used;

			// Variables
			var skillLength = skill.RankData.Var4;

			// Effects
			Send.Effect(attacker, Effect.DropKick, (byte)1, targetEntityId);
			Send.EffectDelayed(attacker, 300, Effect.DropKick, (byte)2, targetEntityId, (short)400);

			// Prepare Singular Target Combat Actions
			var cap = new CombatActionPack(attacker, skill.Info.Id);

			var aAction = new AttackerAction(CombatActionType.RangeHit, attacker, targetEntityId, skill.Info.Id);
			aAction.Set(AttackerOptions.Result);

			var tAction = new TargetAction(CombatActionType.TakeHit, target, attacker, skill.Info.Id);
			tAction.Set(TargetOptions.Result | TargetOptions.FighterUnk);

			cap.Add(aAction, tAction);

			// Damage
			var damage = (attacker.GetRndFighterDamage() * (skill.RankData.Var1 / 100f));

			// Chain Mastery Damage Bonus
			var chainMasterySkill = attacker.Skills.Get(SkillId.ChainMastery);
			var damageBonus = (chainMasterySkill == null ? 0 : chainMasterySkill.RankData.Var1);
			damage += damage * (damageBonus / 100f);

			// Heavy Armor Damage Reduction
			var damageReduction = (chainMasterySkill == null ? 0 : (100 - chainMasterySkill.RankData.Var3));
			if (attacker.HasEquipped("/heavyarmor/"))
				damage -= damage * (damageReduction / 100f);

			// Master Title - Damage +10%
			if (attacker.Titles.SelectedTitle == skill.Data.MasterTitle)
				damage += (damage * 0.1f);

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
				if (target.Is(RaceStands.KnockDownable))
				{
					tAction.Set(TargetOptions.FinishingKnockDown);
					attacker.Shove(target, (int)skillLength);
				}
				else
					tAction.Set(TargetOptions.Finished | TargetOptions.FinishingHit);
			}
			else
			{
				if (!target.IsKnockedDown)
					target.Stability -= StabilityReduction;

				if (target.Is(RaceStands.KnockDownable)) // Always knock down
				{
					tAction.Set(TargetOptions.KnockDown);
					attacker.Shove(target, (int)skillLength);
				}
			}
			cap.Handle();

			// Splash Damage
			this.HandleSplash(attacker, skill, target, skillLength, damageBonus, damageReduction);
		}

		/// <summary>
		/// Handles drop kick's splash attack
		/// </summary>
		/// <param name="attacker"></param>
		/// <param name="skill"></param>
		/// <param name="initTarget"></param>
		/// <param name="skillLength"></param>
		/// <param name="damageBonus"></param>
		public void HandleSplash(Creature attacker, Skill skill, Creature initTarget, float skillLength, float damageBonus, float damageReduction)
		{
			// Variables
			var skillWidth = skill.RankData.Var3;
			var attackerPos = attacker.GetPosition();

			// Set training variable
			attacker.Temp.DropKickSplashTargetCount = 0;

			// Get splash targets
			var targets = SkillHelper.GetTargetableCreaturesInSkillArea(attacker, (int)skillLength, (int)skillWidth);
			targets.Remove(initTarget);

			// Target Area?
			var targetAreaLoc = new Location(attacker.RegionId, attackerPos);
			var targetAreaId = targetAreaLoc.ToLocationId();

			// Prepare Splash Combat Actions
			var cap = new CombatActionPack(attacker, skill.Info.Id);

			var aAction = new AttackerAction(CombatActionType.SpecialHit, attacker, targetAreaId, skill.Info.Id);
			aAction.Set(AttackerOptions.Result);

			cap.Add(aAction);

			// Get critical hit (also for splash attacks)
			var rnd = RandomProvider.Get();
			var crit = false;
			var critSkill = attacker.Skills.Get(SkillId.CriticalHit);
			if (critSkill != null && critSkill.Info.Rank > SkillRank.Novice)
			{
				var critChance = Math2.Clamp(0, 30, attacker.GetTotalCritChance(0));
				if (rnd.NextDouble() * 100 < critChance)
					crit = true;
			}

			// Target Actions
			foreach (var target in targets)
			{
				attacker.Temp.DropKickSplashTargetCount += 1;

				var tAction = new TargetAction(CombatActionType.TakeHit, target, attacker, skill.Info.Id);
				tAction.Delay = target.GetPosition().GetDistance(attackerPos);
				cap.Add(tAction);

				// Splash Damage
				var damage = (attacker.GetRndFighterDamage() * (skill.RankData.Var5 / 100f));

				// Chain Mastery Damage Bonus
				damage += damage * (damageBonus / 100f);

				// Heavy Armor Damage Reduction
				if (attacker.HasEquipped("/heavyarmor/"))
					damage -= damage * (damageReduction / 100f);

				// Master Title - Damage +10%
				if (attacker.Titles.SelectedTitle == skill.Data.MasterTitle)
					damage += (damage * 0.1f);

				// Critical Hit
				if (crit)
					CriticalHit.Handle(attacker, 100, ref damage, tAction);

				// Handle skills and reductions
				SkillHelper.HandleDefenseProtection(target, ref damage);
				HeavyStander.Handle(attacker, target, ref damage, tAction);
				SkillHelper.HandleConditions(attacker, target, ref damage);
				ManaShield.Handle(target, ref damage, tAction);

				// Apply Damage
				target.TakeDamage(tAction.Damage = damage, attacker);

				// Aggro
				target.Aggro(attacker);

				// Stun
				tAction.Stun = TargetStun;

				// Death and Knockback
				if (target.IsDead)
				{
					if (target.Is(RaceStands.KnockDownable))
					{
						tAction.Set(TargetOptions.FinishingKnockDown);
						attacker.Shove(target, (int)skillLength);
					}
					else
						tAction.Set(TargetOptions.Finished | TargetOptions.FinishingHit);
				}
				else
				{
					if (!target.IsKnockedDown)
						target.Stability -= StabilityReduction;

					if (target.Is(RaceStands.KnockDownable)) // Always knock down
					{
						tAction.Set(TargetOptions.KnockDown);
						attacker.Shove(target, (int)skillLength);
					}
				}
			}
			cap.Handle();
		}

		/// <summary>
		/// Completes the skill
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void Complete(Creature creature, Skill skill, Packet packet)
		{
			Send.SkillComplete(creature, skill.Info.Id);
			creature.Unlock(Locks.Walk | Locks.Run);
		}

		/// <summary>
		/// Cancels the skill
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		public void Cancel(Creature creature, Skill skill)
		{
			creature.Unlock(Locks.Walk | Locks.Run);
		}

		/// <summary>
		/// Skill training, called when someone attacks something
		/// </summary>
		/// <param name="action"></param>
		public void OnCreatureAttackedByPlayer(TargetAction action)
		{
			// Check skill
			if (action.AttackerSkillId != SkillId.DropKick)
				return;

			// Get skill
			var attackerSkill = action.Attacker.Skills.Get(SkillId.DropKick);
			if (attackerSkill == null) return;

			// Disregard the second splash CombatActionPack for this training
			if (!action.Has(TargetOptions.FighterUnk))
				return;

			// Training
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
				case SkillRank.R6:
				case SkillRank.R5:
				case SkillRank.R4:
				case SkillRank.R3:
				case SkillRank.R1:
					if (action.Has(TargetOptions.Critical)) attackerSkill.Train(2); // Get a critical hit with Drop Kick.
					break;
			}
		}

		public void OnCreatureAttacks(AttackerAction aAction)
		{
			// Check skill
			if (aAction.SkillId != SkillId.DropKick)
				return;

			// Get skill
			var attackerSkill = aAction.Creature.Skills.Get(SkillId.DropKick);
			if (attackerSkill == null) return;

			// Disregard the initial CombatActionPack for this training
			if (aAction.Flags.HasFlag(CombatActionType.RangeHit))
				return;

			// Training
			switch (attackerSkill.Info.Rank)
			{
				case SkillRank.Novice:
					attackerSkill.Train(1); // Use the skill successfully.
					break;
				case SkillRank.RF:
				case SkillRank.RE:
				case SkillRank.RD:
				case SkillRank.RC:
				case SkillRank.RB:
				case SkillRank.RA:
				case SkillRank.R9:
				case SkillRank.R8:
				case SkillRank.R7:
				case SkillRank.R6:
				case SkillRank.R5:
				case SkillRank.R4:
				case SkillRank.R3:
					attackerSkill.Train(1); // Use the skill successfully.
					if (aAction.Creature.Temp.DropKickSplashTargetCount >= 1) attackerSkill.Train(3); // Hit 2 or more enemies with Drop Kick. (Initial target counts as 1)
					aAction.Creature.Temp.DropKickSplashTargetCount = 0; // Reset variable
					break;
				case SkillRank.R2:
					if (aAction.Creature.Temp.DropKickSplashTargetCount >= 2) attackerSkill.Train(1); // Hit 3 or more enemies with Drop Kick. (Initial target counts as 1)
					aAction.Creature.Temp.DropKickSplashTargetCount = 0; // Reset variable
					break;
				case SkillRank.R1:
					attackerSkill.Train(1); // Use the skill successfully.
					if (aAction.Creature.Temp.DropKickSplashTargetCount >= 1) attackerSkill.Train(3); // Hit 2 or more enemies with Drop Kick. (Initial target counts as 1)
					aAction.Creature.Temp.DropKickSplashTargetCount = 0; // Reset variable
					break;
			}
		}
	}
}
