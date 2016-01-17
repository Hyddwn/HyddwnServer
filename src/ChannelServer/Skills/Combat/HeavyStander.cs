// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.World.Entities;
using Aura.Mabi;
using Aura.Mabi.Const;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.Skills.Combat
{
	/// <summary>
	/// Skill handler for active Heavy Stander. Also handles the effect of
	/// all Heavy Standings.
	/// </summary>
	/// <remarks>
	/// Var1: Damage Reduction (%)
	/// Var3: Auto Defend Rate (%)
	/// 
	/// Reference: http://wiki.mabinogiworld.com/view/Heavy_Stander
	/// 
	/// Use `Has(SkillFlags.InUse)` to check if skill is active.
	/// </remarks>
	[Skill(SkillId.HeavyStander)]
	public class HeavyStander : StartStopSkillHandler
	{
		private static string[] Lv1Msgs =
		{
			Localization.Get("My attack is being defended by a skill!"),
			Localization.Get("My attack is very ineffective right now..."),
			Localization.Get("That didn't feel right..."),
			Localization.Get("My attack is a bit off the mark...")
		};

		private static string[] Lv2Msgs =
		{
			Localization.Get("This is not enough to stop the target..."),
			Localization.Get("Can't shake the target's balance!"),
			Localization.Get("That may have inflicted some damage, but the target still has its guard up.")
		};

		private static string[] Lv3Msgs =
		{
			Localization.Get("The target takes no damage at all!"),
			Localization.Get("This attack is completely useless!")
		};

		/// <summary>
		/// Starts Heavy Stander.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="dict"></param>
		/// <returns></returns>
		public override StartStopResult Start(Creature creature, Skill skill, MabiDictionary dict)
		{
			// Give an indication of activation, official behavior unknown.
			Send.Notice(creature, Localization.Get("Heavy Stander activated."));

			return StartStopResult.Okay;
		}

		/// <summary>
		/// Stops Heavy Stander.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="dict"></param>
		/// <returns></returns>
		public override StartStopResult Stop(Creature creature, Skill skill, MabiDictionary dict)
		{
			Send.Notice(creature, Localization.Get("Heavy Stander deactivated."));

			return StartStopResult.Okay;
		}

		/// <summary>
		/// Handles Heavy Stander bonuses and auto-defense, reducing damage
		/// and setting the appropriate options on tAction. Returns whether
		/// or not Heavy Stander pinged.
		/// </summary>
		/// <remarks>
		/// All active and passive Heavy Standers are check in sequence,
		/// followed by the equipment, until one pings. Until one does,
		/// the passive damage reduction stacks. It's unknown whether
		/// this is official, and assumedly no monsters have multiple
		/// Heavy Stander skills.
		/// </remarks>
		/// <param name="attacker"></param>
		/// <param name="target"></param>
		/// <param name="damage"></param>
		/// <param name="tAction"></param>
		public static bool Handle(Creature attacker, Creature target, ref float damage, TargetAction tAction)
		{
			var pinged = false;
			var rank = SkillRank.R1;
			var rnd = RandomProvider.Get();

			// Check active Heavy Stander
			var skill = target.Skills.Get(SkillId.HeavyStander);
			if (skill != null && skill.Has(SkillFlags.InUse))
			{
				pinged = ReduceDamage(ref damage, skill.RankData.Var1, skill.RankData.Var3, rnd);
				rank = skill.Info.Rank;
			}

			// Check passive Heavy Stander
			if (!pinged)
			{
				skill = target.Skills.Get(SkillId.HeavyStanderPassive);
				if (skill != null)
				{
					pinged = ReduceDamage(ref damage, skill.RankData.Var1, skill.RankData.Var3, rnd);
					rank = skill.Info.Rank;
				}
			}

			// Check equipment
			if (!pinged)
			{
				var equipment = target.Inventory.GetMainEquipment();
				foreach (var item in equipment)
				{
					var chance = item.Data.AutoDefenseMelee;
					if (chance > 0)
					{
						if (pinged = ReduceDamage(ref damage, 0, chance, rnd))
							break;
					}
				}
			}

			// Notice and flag
			if (pinged)
			{
				tAction.EffectFlags |= EffectFlags.HeavyStander;

				var msg = "";
				if (rank >= SkillRank.Novice && rank <= SkillRank.RA)
					msg = rnd.Rnd(Lv1Msgs);
				else if (rank >= SkillRank.R9 && rank <= SkillRank.R5)
					msg = rnd.Rnd(Lv2Msgs);
				else if (rank >= SkillRank.R4 && rank <= SkillRank.R1)
					msg = rnd.Rnd(Lv3Msgs);

				Send.Notice(attacker, msg);
			}

			return pinged;
		}

		/// <summary>
		/// Reduces damage based on parameters, and whether auto defense
		/// activates. If it does, this returns true.
		/// </summary>
		/// <param name="damage"></param>
		/// <param name="damageReduction"></param>
		/// <param name="activationChance"></param>
		/// <param name="rnd"></param>
		/// <returns></returns>
		private static bool ReduceDamage(ref float damage, float damageReduction, float activationChance, Random rnd)
		{
			// Apply damage reduction
			if (damageReduction > 0)
				damage = Math.Max(1, damage - (damage / 100 * damageReduction));

			// Apply auto defense
			if (rnd.Next(100) < activationChance)
			{
				damage = Math.Max(1, damage / 2);
				return true;
			}

			return false;
		}
	}
}
