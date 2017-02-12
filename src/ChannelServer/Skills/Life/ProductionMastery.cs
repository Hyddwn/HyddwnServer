// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Skills.Base;
using Aura.Channel.World;
using Aura.Channel.World.Entities;
using Aura.Data.Database;
using Aura.Mabi.Const;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.Skills.Life
{
	/// <summary>
	/// Handles Production Mastery training.
	/// </summary>
	[Skill(SkillId.ProductionMastery)]
	public class ProductionMastery : IInitiableSkillHandler, ISkillHandler
	{
		/// <summary>
		/// Subscribes to events required for training.
		/// </summary>
		public void Init()
		{
			ChannelServer.Instance.Events.CreatureGathered += this.OnCreatureGathered;
			ChannelServer.Instance.Events.CreatureProducedItem += this.OnCreatureProducedItem;
			ChannelServer.Instance.Events.CreatureCreatedItem += this.OnCreatureCreatedItem;
			ChannelServer.Instance.Events.CreatureCookedMeal += this.OnCreatureCookedMeal;
		}

		/// <summary>
		/// Returns success chance, increased according to creature's
		/// Production Mastery rank.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="chance"></param>
		public static float IncreaseChance(Creature creature, float chance)
		{
			var skill = creature.Skills.Get(SkillId.ProductionMastery);
			if (skill != null)
				chance += skill.RankData.Var1;

			return chance;
		}

		/// <summary>
		/// Raised when creature collects something, handles gathering conditions.
		/// </summary>
		/// <param name="args"></param>
		private void OnCreatureGathered(CollectEventArgs args)
		{
			var skill = args.Creature.Skills.Get(SkillId.ProductionMastery);
			if (skill == null || !args.Success) return;

			skill.Train(1); // Collect any material without using a skill.

			if (skill.Info.Rank == SkillRank.R9 && args.ItemId == 51101)
				skill.Train(5); // Successfully pick a Bloody Herb.

			if (skill.Info.Rank == SkillRank.R8 && args.ItemId == 51103)
				skill.Train(5); // Successfully pick a Sunlight Herb

			if (skill.Info.Rank == SkillRank.R7 && args.ItemId == 51102)
				skill.Train(5); // Successfully pick a Mana Herb.

			if (skill.Info.Rank == SkillRank.R6 && args.ItemId == 51105)
				skill.Train(5); // Successfully pick a Golden Herb.
		}

		/// <summary>
		/// Raised when creature produces items, handles creation conditions.
		/// </summary>
		/// <param name="args"></param>
		private void OnCreatureProducedItem(ProductionEventArgs args)
		{
			var skill = args.Creature.Skills.Get(SkillId.ProductionMastery);
			if (skill == null) return;

			if (skill.Info.Rank >= SkillRank.RF && skill.Info.Rank <= SkillRank.RA)
			{
				if (args.Success)
					skill.Train(2); // Create any material through use of a skill.
				else
					skill.Train(3); // Fail at creating any material through use of a skill.

				return;
			}

			if (skill.Info.Rank == SkillRank.R9)
			{
				if (args.Success)
				{
					skill.Train(2); // Create any material through use of a skill.

					if (args.Item.HasTag("/texture/03/|/texture/04/"))
						skill.Train(3); // Successfully weave Fine or Finest Fabric.
					else if (args.Item.HasTag("/copperingot/|/silveringot/"))
						skill.Train(4); // Successfully refine a Copper Ingot or a Silver Ingot.
				}
				return;
			}

			if (skill.Info.Rank == SkillRank.R8)
			{
				if (args.Success)
				{
					skill.Train(2); // Create any material through use of a skill.

					if (args.Item.HasTag("/silk/03/|/silk/04/"))
						skill.Train(3); // Successfully weave Fine or Finest Silk.
					else if (args.Item.HasTag("/silveringot/|/goldingot/"))
						skill.Train(4); // Successfully refine a Gold or Silver Ingot.
				}
				return;
			}

			if (skill.Info.Rank == SkillRank.R7)
			{
				if (args.Success)
				{
					skill.Train(2); // Create any material through use of a skill.

					if (args.Item.HasTag("/leather_strap/03/|/leather_strap/04/"))
						skill.Train(3); // Successfully cut a Fine or Finest Leather.
					else if (args.Item.HasTag("/goldingot/"))
						skill.Train(4); // Successfully refine a Gold or Silver Ingot.
				}
				return;
			}

			if (skill.Info.Rank == SkillRank.R6)
			{
				if (args.Success)
				{
					skill.Train(2); // Create any material through use of a skill.

					if (args.Item.HasTag("/leather_strap/03/|/leather_strap/04/"))
						skill.Train(3); // Successfully cut a Fine or Finest Leather.
					else if (args.Item.HasTag("/mythrilingot/"))
						skill.Train(4); // Successfully refine a Mythril Ingot.
				}
				return;
			}

			if (skill.Info.Rank == SkillRank.R5)
			{
				if (args.Success)
				{
					skill.Train(2); // Create any material through use of a skill.

					if (args.ProductionData.Category == ProductionCategory.PotionMaking && args.ProductionData.Materials.Any(a => a.Tag.Contains("/whiteherb/")))
						skill.Train(4); // Successfully make a potion using a White Herb.
					else if (args.ProductionData.Category == ProductionCategory.Handicraft && args.ProductionData.Rank >= SkillRank.R7 && args.ProductionData.Rank <= SkillRank.R5)
						skill.Train(5); // Successfully handicraft any item from Rank 7 to Rank 5.
				}
				return;
			}

			if (skill.Info.Rank == SkillRank.R4)
			{
				if (args.Success)
				{
					skill.Train(2); // Create any material through use of a skill.

					if (args.ProductionData.Category == ProductionCategory.PotionMaking && args.ProductionData.Materials.Any(a => a.Tag.Contains("/goldenherb/")))
						skill.Train(4); // Successfully make a potion using a Golden Herb.
					else if (args.ProductionData.Category == ProductionCategory.Handicraft && args.ProductionData.Rank >= SkillRank.R6 && args.ProductionData.Rank <= SkillRank.R4)
						skill.Train(5); // Successfully handicraft any item from Rank 6 to Rank 4.
				}
				return;
			}

			if (skill.Info.Rank == SkillRank.R3)
			{
				if (args.Success)
				{
					skill.Train(2); // Create any material through use of a skill.

					if (args.ProductionData.Category == ProductionCategory.PotionMaking && args.ProductionData.Materials.Any(a => a.Tag.Contains("/mandrake/")))
						skill.Train(4); // Successfully make a potion using a Mandrake.
					else if (args.ProductionData.Category == ProductionCategory.Handicraft && args.ProductionData.Rank >= SkillRank.R5 && args.ProductionData.Rank <= SkillRank.R3)
						skill.Train(5); // Successfully handicraft any item from Rank 5 to Rank 3.
				}
				return;
			}

			if (skill.Info.Rank == SkillRank.R2)
			{
				if (args.Success)
				{
					skill.Train(2); // Create any material through use of a skill.

					if (args.ProductionData.Category == ProductionCategory.PotionMaking && args.ProductionData.Materials.Any(a => a.Tag.Contains("/antidoteherb/")))
						skill.Train(4); // Successfully make a potion using a Antidote Herb.
					else if (args.ProductionData.Category == ProductionCategory.Handicraft && args.ProductionData.Rank >= SkillRank.R4 && args.ProductionData.Rank <= SkillRank.R2)
						skill.Train(5); // Successfully handicraft any item from Rank 4 to Rank 2.
				}
				return;
			}

			if (skill.Info.Rank == SkillRank.R1)
			{
				if (args.Success)
				{
					skill.Train(2); // Create any material through use of a skill.

					if (args.ProductionData.Category == ProductionCategory.PotionMaking && args.ProductionData.Materials.Any(a => a.Tag.Contains("/poisonherb/")))
						skill.Train(3); // Successfully make a potion using a Poison Herb.
					else if (args.ProductionData.Category == ProductionCategory.Handicraft && args.ProductionData.Rank >= SkillRank.R3 && args.ProductionData.Rank <= SkillRank.R1)
						skill.Train(4); // Successfully handicraft any item from Rank 3 to Rank 1.
				}
				return;
			}
		}

		/// <summary>
		/// Raised when creature creates an item, handles creation conditions.
		/// </summary>
		/// <param name="args"></param>
		private void OnCreatureCreatedItem(CreationEventArgs args)
		{
			var skill = args.Creature.Skills.Get(SkillId.ProductionMastery);
			if (skill == null) return;

			if (skill.Info.Rank == SkillRank.R5)
			{
				if (args.Rank >= SkillRank.R7 && args.Rank <= SkillRank.R5)
				{
					if (args.Method == CreationMethod.Tailoring)
						skill.Train(6); // Successfully tailor any item from Rank 7 to Rank 5.
					else if (args.Method == CreationMethod.Blacksmithing)
						skill.Train(7); // Successfully smith any item from Rank 7 to Rank 5.
				}
				return;
			}

			if (skill.Info.Rank == SkillRank.R4)
			{
				if (args.Rank >= SkillRank.R6 && args.Rank <= SkillRank.R4)
				{
					if (args.Method == CreationMethod.Tailoring)
						skill.Train(6); // Successfully tailor any item from Rank 6 to Rank 4.
					else if (args.Method == CreationMethod.Blacksmithing)
						skill.Train(7); // Successfully smith any item from Rank 6 to Rank 4.
				}
				return;
			}

			if (skill.Info.Rank == SkillRank.R3)
			{
				if (args.Rank >= SkillRank.R5 && args.Rank <= SkillRank.R3)
				{
					if (args.Method == CreationMethod.Tailoring)
						skill.Train(6); // Successfully tailor any item from Rank 5 to Rank 3.
					else if (args.Method == CreationMethod.Blacksmithing)
						skill.Train(7); // Successfully smith any item from Rank 5 to Rank 3.
				}
				return;
			}

			if (skill.Info.Rank == SkillRank.R2)
			{
				if (args.Rank >= SkillRank.R4 && args.Rank <= SkillRank.R2)
				{
					if (args.Method == CreationMethod.Tailoring)
						skill.Train(6); // Successfully tailor any item from Rank 4 to Rank 2.
					else if (args.Method == CreationMethod.Blacksmithing)
						skill.Train(7); // Successfully smith any item from Rank 4 to Rank 2.
				}
				return;
			}

			if (skill.Info.Rank == SkillRank.R1)
			{
				if (args.Rank >= SkillRank.R3 && args.Rank <= SkillRank.R1)
				{
					if (args.Method == CreationMethod.Tailoring)
						skill.Train(5); // Successfully tailor any item from Rank 3 to Rank 1.
					else if (args.Method == CreationMethod.Blacksmithing)
						skill.Train(6); // Successfully smith any item from Rank 3 to Rank 1.
				}
				return;
			}
		}

		/// <summary>
		/// Raised when creature cooks something, handles creation conditions.
		/// </summary>
		/// <param name="args"></param>
		private void OnCreatureCookedMeal(CookingEventArgs args)
		{
			var skill = args.Creature.Skills.Get(SkillId.ProductionMastery);
			if (skill == null || !args.Success) return;

			if (skill.Info.Rank == SkillRank.R5)
			{
				if (args.Recipe.Method == CookingMethod.PastaMaking)
					skill.Train(3); // Successfully cook pasta.

				return;
			}

			if (skill.Info.Rank >= SkillRank.R4 && skill.Info.Rank <= SkillRank.R2)
			{
				skill.Train(3); // Successfully cook any meal.

				return;
			}
		}
	}
}
