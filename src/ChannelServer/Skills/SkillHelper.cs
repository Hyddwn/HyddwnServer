﻿// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.Skills.Combat;
using Aura.Channel.World.Entities;
using Aura.Channel.World;
using Aura.Mabi.Const;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.Skills
{
	public static class SkillHelper
	{
		/// <summary>
		/// Reduces damage by target's defense and protection.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="damage"></param>
		/// <param name="defense"></param>
		/// <param name="protection"></param>
		public static void HandleDefenseProtection(Creature target, ref float damage, bool defense = true, bool protection = true)
		{
			if (defense)
				damage = Math.Max(1, damage - target.Defense);
			if (protection && damage > 1)
				damage = Math.Max(1, damage - (damage * target.Protection));
		}

		/// <summary>
		/// Reduces damage by target's magic defense and protection.
		/// </summary>
		/// <param name="target"></param>
		/// <param name="damage"></param>
		/// <param name="defense"></param>
		/// <param name="protection"></param>
		public static void HandleMagicDefenseProtection(Creature target, ref float damage, bool defense = true, bool protection = true)
		{
			if (defense)
				damage = Math.Max(1, damage - target.MagicDefense);
			if (protection && damage > 1)
				damage = Math.Max(1, damage - (damage / 100 * target.MagicProtection));
		}

		/// <summary>
		/// Modified damage based on active conditions.
		/// </summary>
		/// <param name="attacker"></param>
		/// <param name="target"></param>
		/// <param name="damage"></param>
		public static void HandleConditions(Creature attacker, Creature target, ref float damage)
		{
			// G1 Seal Scroll = No damage for 60s
			if (target.Conditions.Has(ConditionsA.Blessed))
			{
				damage = 1;
			}
		}

		/// <summary>
		/// Reduces weapon's durability and increases its proficiency.
		/// Only updates weapon type items that are not null.
		/// </summary>
		/// <param name="attacker">Creature who's weapon is updated.</param>
		/// <param name="target">
		/// The target of the skill, used for power rating calculations.
		/// If target is null, prof will be rewarded regardless of target.
		/// </param>
		/// <param name="weapons">Weapons to update.</param>
		public static void UpdateWeapon(Creature attacker, Creature target, ProficiencyGainType profGainType, params Item[] weapons)
		{
			if (attacker == null)
				return;

			var rnd = RandomProvider.Get();

			// Add prof if no target was specified or the target is not "Weakest".
			var addProf = (target == null || attacker.GetPowerRating(target) >= PowerRating.Weak);

			foreach (var weapon in weapons.Where(a => a != null && a.IsTrainableWeapon))
			{
				// Durability
				var reduce = rnd.Next(1, 30);
				attacker.Inventory.ReduceDurability(weapon, reduce);

				// Don't add prof if weapon is broken.
				if (weapon.Durability == 0)
					addProf = false;

				// Proficiency
				if (addProf)
				{
					var amount = Item.GetProficiencyGain(attacker.Age, profGainType);
					attacker.Inventory.AddProficiency(weapon, amount);
				}
			}
		}

		/// <summary>
		/// Inflicts injuries to target, based on attacker's Injury
		/// properties and the given damage.
		/// </summary>
		/// <param name="attacker"></param>
		/// <param name="target"></param>
		/// <param name="damage"></param>
		public static void HandleInjury(Creature attacker, Creature target, float damage)
		{
			if (attacker == null || target == null || damage == 0)
				return;

			var rnd = RandomProvider.Get();
			var min = attacker.InjuryMin;
			var max = Math.Max(min, attacker.InjuryMax);

			// G1 Seal Scroll = 100% for 60s
			if (attacker.Conditions.Has(ConditionsA.Blessed))
			{
				min = 100;
				max = 100;
			}

			if (max == 0)
				return;

			var rndInjure = rnd.Next(min, max + 1);
			if (rndInjure == 0)
				return;

			var injure = damage * (rndInjure / 100f);

			target.Injuries += injure;
		}

		/// <summary>
		/// Gets targetable creatures within a skill's rectangular area
		/// (includes collision check).
		/// </summary>
		/// <param name="attacker">Source of the skill.</param>
		/// <param name="skillLength">Length of the area of effect.</param>
		/// <param name="skillWidth">Width of the area of effect.</param>
		/// <returns></returns>
		public static List<Creature> GetTargetableCreaturesInSkillArea(Creature attacker, int skillLength, int skillWidth)
		{
			Position discard;
			return GetTargetableCreaturesInSkillArea(attacker, skillLength, skillWidth, out discard);
		}

		/// <summary>
		/// Gets targetable creatures within a skill's rectangular area (includes collision check).
		/// Outs an end position on the far end of the rectangle, opposite the attacker position.
		/// </summary>
		/// <param name="attacker">Source of the skill.</param>
		/// <param name="skillLength">Length of the area of effect.</param>
		/// <param name="skillWidth">Width of the area of effect.</param>
		/// <param name="endPosition">Returns position on the far end of the rectangle.</param>
		/// <returns></returns>
		public static List<Creature> GetTargetableCreaturesInSkillArea(Creature attacker, int skillLength, int skillWidth, out Position endPosition)
		{
			if (attacker == null)
			{
				endPosition = new Position(0, 0);
				return null;
			}

			var attackerPos = attacker.GetPosition();
			var radian = Mabi.MabiMath.ByteToRadian(attacker.Direction);
			var creatures = attacker.Region.GetCreaturesInRectangle(attackerPos, radian, skillLength, skillWidth, out endPosition);
			var targetableCreatures = creatures.Where(x => attacker.CanTarget(x) && !attacker.Region.Collisions.Any(attackerPos, x.GetPosition()));

			return targetableCreatures.ToList();

		}
	}
}
