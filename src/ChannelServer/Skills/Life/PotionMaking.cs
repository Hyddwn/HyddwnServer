// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.World.Entities;
using Aura.Data.Database;
using Aura.Mabi.Const;
using Aura.Shared.Util;
using System.Linq;

namespace Aura.Channel.Skills.Life
{
	/// <summary>
	/// Handles Potion Making production skill.
	/// </summary>
	[Skill(SkillId.PotionMaking)]
	public class PotionMaking : ProductionSkill, IInitiableSkillHandler
	{
		public void Init()
		{
			ChannelServer.Instance.Events.PlayerUsesItem += this.OnPlayerUsesItem;
		}

		protected override bool CheckTools(Creature creature, Skill skill, ProductionData productData)
		{
			if (creature.RightHand == null || !creature.RightHand.HasTag("/potion_making/kit/"))
			{
				// Sanity check, the client should normally handle this.
				Send.MsgBox(creature, Localization.Get("You need a Potion Concoction Kit to make potions!"));
				return false;
			}

			return true;
		}

		protected override bool CheckCategory(Creature creature, ProductionCategory category)
		{
			return (category == ProductionCategory.PotionMaking);
		}

		protected override void SkillTraining(Creature creature, Skill skill, ProductionData data, bool success)
		{
			if (skill.Info.Rank == SkillRank.Novice)
			{
				if (data.ItemData.HasTag("/potion/hp/"))
				{
					skill.Train(1); // Attempt to concoct a HP Potion.
					if (success)
						skill.Train(2); // Successful in concocting a HP Potion.
				}

				return;
			}

			if (!success)
				return;

			if (skill.Info.Rank >= SkillRank.RF && skill.Info.Rank <= SkillRank.RE)
			{
				if (data.ItemData.HasTag("/potion/hp/"))
					skill.Train(2); // Successful in concocting a HP Potion.
				else if (data.ItemData.HasTag("/potion/mana/"))
					skill.Train(4); // Successful in concocting an MP Potion.
				else if (data.ItemData.HasTag("/potion/stamina/"))
					skill.Train(6); // Successful in concocting a Stamina Potion.

				return;
			}

			if (skill.Info.Rank >= SkillRank.RD && skill.Info.Rank <= SkillRank.RA)
			{
				if (data.Materials.Any(a => a.Tag == "*/bloodyherb/*"))
					skill.Train(1); // Successful in concocting a potion using a Bloody Herb.

				if (data.Materials.Any(a => a.Tag == "*/sunlightherb/*"))
					skill.Train(2); // Successful in concocting a potion using a Sunlight Herb.

				if (data.Materials.Any(a => a.Tag == "*/manaherb/*"))
					skill.Train(3); // Successful in concocting a potion using a Mana Herb.

				if (data.ItemData.HasTag("/potion/mana/hp/"))
					skill.Train(4); // Successful in concocting a HP & MP Potion.

				if (data.Materials.Any(a => a.Tag == "*/goldenherb/*"))
					skill.Train(5); // Successful in concocting a potion using a Golden Herb.

				if (skill.Info.Rank == SkillRank.RA)
				{
					if (data.Materials.Any(a => a.Tag == "*/whiteherb/*"))
						skill.Train(6); // Successful in concocting a potion using a White Herb.
				}

				return;
			}

			if (skill.Info.Rank == SkillRank.R9)
			{
				if (data.Materials.Any(a => a.Tag == "*/sunlightherb/*"))
					skill.Train(1); // Successful in concocting a potion using a Sunlight Herb.

				if (data.Materials.Any(a => a.Tag == "*/manaherb/*"))
					skill.Train(2); // Successful in concocting a potion using a Mana Herb.

				if (data.ItemData.HasTag("/potion/mana/hp/"))
					skill.Train(3); // Successful in concocting a HP & MP Potion.

				if (data.Materials.Any(a => a.Tag == "*/goldenherb/*"))
					skill.Train(4); // Successful in concocting a potion using a Golden Herb.

				if (data.Materials.Any(a => a.Tag == "*/whiteherb/*"))
					skill.Train(5); // Successful in concocting a potion using a White Herb.

				if (data.Materials.Any(a => a.Tag == "*/mandrake/*"))
					skill.Train(6); // Successful in concocting a potion using a Mandrake.

				return;
			}

			if (skill.Info.Rank == SkillRank.R8)
			{
				if (data.Materials.Any(a => a.Tag == "*/sunlightherb/*"))
					skill.Train(1); // Successful in concocting a potion using a Sunlight Herb.

				if (data.Materials.Any(a => a.Tag == "*/manaherb/*"))
					skill.Train(2); // Successful in concocting a potion using a Mana Herb.

				if (data.Materials.Any(a => a.Tag == "*/goldenherb/*"))
					skill.Train(3); // Successful in concocting a potion using a Golden Herb.

				if (data.ItemData.HasTag("/potion/wound/"))
					skill.Train(4); // Successful in concocting a Wound Remedy Potion.

				if (data.Materials.Any(a => a.Tag == "*/whiteherb/*"))
					skill.Train(5); // Successful in concocting a potion using a White Herb.

				if (data.Materials.Any(a => a.Tag == "*/mandrake/*"))
					skill.Train(6); // Successful in concocting a potion using a Mandrake.

				return;
			}

			if (skill.Info.Rank == SkillRank.R7)
			{
				if (data.Materials.Any(a => a.Tag == "*/manaherb/*"))
					skill.Train(1); // Successful in concocting a potion using a Mana Herb.

				if (data.Materials.Any(a => a.Tag == "*/goldenherb/*"))
					skill.Train(2); // Successful in concocting a potion using a Golden Herb.

				if (data.ItemData.HasTag("/potion/wound/"))
					skill.Train(3); // Successful in concocting a Wound Remedy Potion.

				if (data.Materials.Any(a => a.Tag == "*/whiteherb/*"))
					skill.Train(4); // Successful in concocting a potion using a White Herb.

				if (data.Materials.Any(a => a.Tag == "*/mandrake/*"))
					skill.Train(5); // Successful in concocting a potion using a Mandrake.

				return;
			}

			if (skill.Info.Rank == SkillRank.R6)
			{
				if (data.Materials.Any(a => a.Tag == "*/goldenherb/*"))
					skill.Train(1); // Successful in concocting a potion using a Golden Herb.

				if (data.ItemData.HasTag("/potion/wound/"))
					skill.Train(2); // Successful in concocting a Wound Remedy Potion.

				if (data.Materials.Any(a => a.Tag == "*/whiteherb/*"))
					skill.Train(3); // Successful in concocting a potion using a White Herb.

				if (data.Materials.Any(a => a.Tag == "*/mandrake/*"))
					skill.Train(4); // Successful in concocting a potion using a Mandrake.

				return;
			}

			if (skill.Info.Rank >= SkillRank.R5 && skill.Info.Rank <= SkillRank.R4)
			{
				if (data.ItemData.HasTag("/potion/wound/"))
					skill.Train(1); // Successful in concocting a Wound Remedy Potion.

				if (data.Materials.Any(a => a.Tag == "*/mandrake/*"))
					skill.Train(2); // Successful in concocting a potion using a Mandrake.

				if (data.Materials.Any(a => a.Tag == "*/antidoteherb/*"))
					skill.Train(3); // Successful in concocting a potion using an Antidote Herb.

				return;
			}

			if (skill.Info.Rank >= SkillRank.R3 && skill.Info.Rank <= SkillRank.R1)
			{
				if (data.ItemData.HasTag("/potion/wound/"))
					skill.Train(1); // Successful in concocting a Wound Remedy Potion.

				if (data.Materials.Any(a => a.Tag == "*/mandrake/*"))
					skill.Train(2); // Successful in concocting a potion using a Mandrake.

				if (data.Materials.Any(a => a.Tag == "*/poisonherb/*"))
					skill.Train(3); // Successful in concocting a potion using a Poison Herb.

				return;
			}
		}

		private void OnPlayerUsesItem(Creature creature, Item item)
		{
			var skill = creature.Skills.Get(SkillId.PotionMaking);
			if (skill == null)
				return;

			if (skill.Info.Rank >= SkillRank.RF && skill.Info.Rank <= SkillRank.RE)
			{
				if (item.HasTag("/potion/hp/"))
					skill.Train(1); // Drink an HP Potion.
				else if (item.HasTag("/potion/mana/"))
					skill.Train(3); // Drink an MP Potion.
				else if (item.HasTag("/potion/stamina/"))
					skill.Train(5); // Drink a Stamina Potion.

				return;
			}

			if (skill.Info.Rank == SkillRank.RB)
			{
				if (item.HasTag("/potion/wound/"))
					skill.Train(6); // Try drinking a Wound Remedy Potion.

				return;
			}
		}
	}
}
