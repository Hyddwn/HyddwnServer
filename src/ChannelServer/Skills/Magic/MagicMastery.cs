// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Skills.Base;
using Aura.Channel.World.Entities;
using Aura.Mabi.Const;

namespace Aura.Channel.Skills.Magic
{
	/// <summary>
	/// Skill training handler for the passive skill Magic Mastery.
	/// </summary>
	[Skill(SkillId.MagicMastery)]
	public class MagicMastery : IInitiableSkillHandler, ISkillHandler
	{
		/// <summary>
		/// Sets up event subscriptions.
		/// </summary>
		public void Init()
		{
			ChannelServer.Instance.Events.CreatureAttackedByPlayer += this.OnCreatureAttackedByPlayer;
			ChannelServer.Instance.Events.PlayerHealsCreature += this.OnPlayerHealsCreature;
		}

		/// <summary>
		/// Handles healing training.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="target"></param>
		/// <param name="skill"></param>
		private void OnPlayerHealsCreature(Creature creature, Creature target, Skill healingSkill)
		{
			var skill = creature.Skills.Get(SkillId.MagicMastery);
			if (skill == null)
				return;

			if (healingSkill.Info.Id == SkillId.Healing)
			{
				var index = 0;

				if (skill.Info.Rank >= SkillRank.RF && skill.Info.Rank <= SkillRank.RD)
					index = 1;
				else if (skill.Info.Rank >= SkillRank.RC && skill.Info.Rank <= SkillRank.R6)
					index = 3;

				if (index != 0)
				{
					skill.Train(index); // Use Healing magic on an injured person.
					if (target.Life < 0)
						skill.Train(index + 1); // Use Healing magic on a critically injured person.
				}
			}
			else if (healingSkill.Info.Id == SkillId.PartyHealing && !creature.IsGiant)
			{
				if (skill.Info.Rank >= SkillRank.R5 && skill.Info.Rank <= SkillRank.R1)
					skill.Train(7); // Use Party Healing.
			}
		}

		/// <summary>
		/// Handles attack training.
		/// </summary>
		/// <param name="tAction"></param>
		private void OnCreatureAttackedByPlayer(TargetAction tAction)
		{
			var target = tAction.Creature;
			var attacker = tAction.Attacker;

			var isBoltMagic = (tAction.AttackerSkillId == SkillId.Icebolt || tAction.AttackerSkillId == SkillId.Firebolt || tAction.AttackerSkillId == SkillId.Lightningbolt);
			var isAdvMagic = (tAction.AttackerSkillId == SkillId.Thunder || tAction.AttackerSkillId == SkillId.Fireball || tAction.AttackerSkillId == SkillId.IceSpear || tAction.AttackerSkillId == SkillId.Blaze);

			if (!isBoltMagic && !isAdvMagic)
				return;

			var attackerSkill = attacker.Skills.Get(SkillId.MagicMastery);
			if (attackerSkill == null)
				return;

			var rating = attacker.GetPowerRating(tAction.Creature);

			if (attackerSkill.Info.Rank == SkillRank.Novice)
			{
				if (isBoltMagic)
					attackerSkill.Train(1); // Attack any monster with bolt magic.
			}
			else if (attackerSkill.Info.Rank >= SkillRank.RF && attackerSkill.Info.Rank <= SkillRank.RD)
			{
				if (isBoltMagic)
				{
					attackerSkill.Train(1); // Attack any monster with bolt magic.

					if (tAction.IsKnockBack)
						attackerSkill.Train(2); // Knock back a monster with bolt magic.

					if (target.IsDead)
						attackerSkill.Train(3); // Kill a monster with bolt magic.
				}
			}
			else if (attackerSkill.Info.Rank >= SkillRank.RC && attackerSkill.Info.Rank <= SkillRank.RB)
			{
				if (isBoltMagic && rating == PowerRating.Normal)
				{
					attackerSkill.Train(1); // Attack any monster of equal level with bolt magic.

					if (target.IsDead)
						attackerSkill.Train(2); // Kill a monster of equal level with bolt magic.
				}
			}
			else if (attackerSkill.Info.Rank >= SkillRank.RA && attackerSkill.Info.Rank <= SkillRank.R9)
			{
				if (isBoltMagic && rating == PowerRating.Strong)
				{
					attackerSkill.Train(1); // Attack a Strong monster with bolt magic.

					if (target.IsDead)
						attackerSkill.Train(2); // Kill a Strong monster with bolt magic.
				}
			}
			else if (attackerSkill.Info.Rank >= SkillRank.R8 && attackerSkill.Info.Rank <= SkillRank.R7)
			{
				if (isBoltMagic && rating == PowerRating.Awful)
				{
					attackerSkill.Train(1); // Attack an Awful monster with bolt magic.

					if (target.IsDead)
						attackerSkill.Train(2); // Kill an Awful monster with bolt magic.
				}
			}
			else if (attackerSkill.Info.Rank >= SkillRank.R6 && attackerSkill.Info.Rank <= SkillRank.R6)
			{
				if (isBoltMagic && rating == PowerRating.Boss)
				{
					attackerSkill.Train(1); // Attack a Boss monster with bolt magic.

					if (target.IsDead)
						attackerSkill.Train(2); // Kill a Boss monster with bolt magic.
				}
			}
			else if (attackerSkill.Info.Rank >= SkillRank.R5 && attackerSkill.Info.Rank <= SkillRank.R4)
			{
				if (isBoltMagic && rating == PowerRating.Boss)
				{
					attackerSkill.Train(1); // Attack a Boss monster with bolt magic.

					if (target.IsDead)
						attackerSkill.Train(2); // Kill a Boss monster with bolt magic.
				}

				if (rating == PowerRating.Strong && target.IsDead)
				{
					switch (attackerSkill.Info.Id)
					{
						case SkillId.Thunder: attackerSkill.Train(3); break; // Kill a Strong monster with Thunder.
						case SkillId.Fireball: attackerSkill.Train(4); break; // Kill a Strong monster with Fireball.
						case SkillId.IceSpear: if (!attacker.IsGiant) attackerSkill.Train(5); break; // Kill a Strong monster with Ice Spear.
						case SkillId.Blaze: if (!attacker.IsGiant) attackerSkill.Train(6); break; // Kill a Strong monster with Blaze.
					}
				}
			}
			else if (attackerSkill.Info.Rank == SkillRank.R3)
			{
				if (isBoltMagic && rating == PowerRating.Boss)
				{
					attackerSkill.Train(1); // Attack a Boss monster with bolt magic.

					if (target.IsDead)
						attackerSkill.Train(2); // Kill a Boss monster with bolt magic.
				}

				if (rating == PowerRating.Awful && target.IsDead)
				{
					switch (attackerSkill.Info.Id)
					{
						case SkillId.Thunder: attackerSkill.Train(3); break; // Kill an Awful monster with Thunder.
						case SkillId.Fireball: attackerSkill.Train(4); break; // Kill an Awful monster with Fireball.
						case SkillId.IceSpear: if (!attacker.IsGiant) attackerSkill.Train(5); break; // Kill an Awful monster with Ice Spear.
						case SkillId.Blaze: if (!attacker.IsGiant) attackerSkill.Train(6); break; // Kill an Awful monster with Blaze.
					}
				}
			}
			else if (attackerSkill.Info.Rank >= SkillRank.R2 && attackerSkill.Info.Rank <= SkillRank.R1)
			{
				if (isBoltMagic && rating == PowerRating.Boss)
				{
					attackerSkill.Train(1); // Attack a Boss monster with bolt magic.

					if (target.IsDead)
						attackerSkill.Train(2); // Kill a Boss monster with bolt magic.
				}

				if (rating == PowerRating.Boss && target.IsDead)
				{
					switch (attackerSkill.Info.Id)
					{
						case SkillId.Thunder: attackerSkill.Train(3); break; // Kill a Boss monster with Thunder.
						case SkillId.Fireball: attackerSkill.Train(4); break; // Kill a Boss monster with Fireball.
						case SkillId.IceSpear: if (!attacker.IsGiant) attackerSkill.Train(5); break; // Kill a Boss monster with Ice Spear.
						case SkillId.Blaze: if (!attacker.IsGiant) attackerSkill.Train(6); break; // Kill a Boss monster with Blaze.
					}
				}
			}
		}
	}
}
