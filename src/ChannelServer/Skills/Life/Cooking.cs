// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.World;
using Aura.Channel.World.Entities;
using Aura.Data;
using Aura.Data.Database;
using Aura.Mabi.Const;
using Aura.Mabi.Network;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Aura.Channel.Skills.Life
{
	/// <summary>
	/// Handler for the Cooking skill.
	/// </summary>
	/// <remarks>
	/// Quality:
	/// > 35 = 2 stars
	/// > 55 = 3 stars
	/// > 75 = 4 stars
	/// > 95 = 5 stars
	/// </remarks>
	[Skill(SkillId.Cooking)]
	public class Cooking : IPreparable, ICompletable, IInitiableSkillHandler
	{
		/// <summary>
		/// Id of the item used as food waste, for failed cooking attempts.
		/// </summary>
		private const int FoodWasteItemId = 50157;

		/// <summary>
		/// Id of the item water and milk bottles are replaced with,
		/// as they're "emptied".
		/// </summary>
		private const int EmptyBottleItemId = 63020;

		/// <summary>
		/// Rating required for declicious training requirements.
		/// </summary>
		private const Rating DeliciousRating = Rating.FourStars;

		/// <summary>
		/// Minimum quality to not get food waste.
		/// </summary>
		private const int SuccessMinQuality = -20;

		/// <summary>
		/// Sets up subscriptions required for skill training.
		/// </summary>
		public void Init()
		{
			ChannelServer.Instance.Events.PlayerUsesItem += this.OnPlayerUsesItem;
		}

		/// <summary>
		/// Prepares skill, specifying the ingredients.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		/// <returns></returns>
		public bool Prepare(Creature creature, Skill skill, Packet packet)
		{
			var ingredients = new List<Ingredient>();

			var unkByte = packet.GetByte();
			var method = packet.GetString();
			var propEntityId = packet.GetLong();
			var unkInt1 = packet.GetInt();
			var count = packet.GetInt();
			for (int i = 0; i < count; ++i)
			{
				var itemEntityId = packet.GetLong();
				var amount = packet.GetFloat();

				// Check item
				var item = creature.Inventory.GetItem(itemEntityId);
				if (item == null)
				{
					Log.Warning("Cooking.Prepare: Creature '{0:X16}' tried to use non-existent item.", creature.EntityId);
					return false;
				}

				ingredients.Add(new Ingredient(item, amount));
			}

			// Check tools
			if (!this.CheckTools(creature, method))
				return false;

			// Check rank
			if (!this.CheckRank(creature, method, skill.Info.Rank))
				return false;

			// Check prop
			if (!this.CheckProp(creature, method, propEntityId))
				return false;

			// Save information
			creature.Temp.CookingIngredients = ingredients;
			creature.Temp.CookingMethod = method;

			// Update tools
			// Item dura
			// Item exp
			// Item dura

			Send.SkillUse(creature, skill.Info.Id, this.GetTime(method));
			skill.State = SkillState.Used;

			Send.Effect(creature, Effect.Cooking, (byte)1, method);

			return true;
		}

		/// <summary>
		/// Completes skill, creating the item.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void Complete(Creature creature, Skill skill, Packet packet)
		{
			var unkInt = packet.GetInt();

			// Get recipe and create item
			Item item = null;
			var success = false;
			var recipe = AuraData.CookingDb.Find(creature.Temp.CookingMethod, creature.Temp.CookingIngredients.Select(a => a.Item.Info.Id));
			if (recipe != null)
			{
				// Get judgement
				var judgement = this.JudgeQuality(recipe, creature.Temp.CookingIngredients);
				if (judgement.Quality >= SuccessMinQuality)
				{
					var quality = (int)judgement.Quality;
					var rating = this.GetRating((int)judgement.Quality);

					// Create food if quality was good enough
					item = new Item(recipe.ItemId);
					item.MetaData1.SetInt("QUAL", quality);
					item.MetaData1.SetString("MKNAME", creature.Name);
					item.MetaData1.SetShort("MKSLV", (short)skill.Info.Rank);
					item.MetaData1.SetString("MKACT", creature.Temp.CookingMethod);

					// Notice
					var msg = "";
					if (judgement.Quality > 75)
						msg += Localization.Get("You just made a delicious dish!");
					else if (judgement.Quality > 55)
						msg += Localization.Get("You just made a tasty dish!");
					else if (judgement.Quality > 35)
						msg += Localization.Get("You just made an edible dish.");
					else if (judgement.Quality > -35)
						msg += Localization.Get("You just made a pretty unappetizing dish...");
					else
						msg += Localization.Get("You just made a dish... that you probably shouldn't eat. Yuck!");

					// Help message
					if (judgement.HelpItem != null)
					{
						var rnd = RandomProvider.Get();
						var helpmsg = "";
						if (judgement.HelpAmount < 0)
							helpmsg += Localization.Get("There may have been {1:0.0}%-{2:0.0}% less {0} than required.");
						else
							helpmsg += Localization.Get("There may have been {1:0.0}%-{2:0.0}% more {0} than required.");

						var name = judgement.HelpItem.Data.Name;
						var amount = Math.Abs(judgement.HelpAmount) * 100;
						var min = (amount - rnd.NextDouble());
						var max = (amount + rnd.NextDouble());

						msg += string.Format("\n" + helpmsg, name, min, max);
					}

					Send.Notice(creature, msg);

					this.OnSuccessfulCooking(creature, skill, creature.Temp.CookingMethod, item, rating);

					success = true;
				}
			}
			else
			{
				//Log.Debug("Recipe not found");
				//Log.Debug("Method: " + creature.Temp.CookingMethod);
				//Log.Debug("Ingredients:");
				//foreach (var ingredient in creature.Temp.CookingIngredients)
				//	Log.Debug("- " + ingredient.Item.Info.Id);
			}

			// Create food waste if nothing halfway decent was created
			if (!success)
			{
				item = new Item(FoodWasteItemId);
				Send.Notice(creature, Localization.Get("Cooking failed"));
			}

			// Remove ingredient items
			// According to the Wiki the whole stack gets removed.
			foreach (var ingredient in creature.Temp.CookingIngredients)
				creature.Inventory.Remove(ingredient.Item);

			// Replace bottled goods with empty bottles
			foreach (var ingredient in creature.Temp.CookingIngredients)
			{
				if (ingredient.Item.HasTag("/milk/|/water/"))
					creature.Inventory.Add(new Item(EmptyBottleItemId), true);
			}

			// Add item to inv
			creature.Inventory.Add(item, true);

			// Cooking event
			ChannelServer.Instance.Events.OnCreatureCookedMeal(new CookingEventArgs(creature, recipe, success, item));

			// Effects and response
			Send.AcquireInfo2Cooking(creature, item.EntityId, item.Info.Id, success);
			Send.Effect(creature, Effect.Cooking, (byte)0, (byte)(success ? 4 : 1));

			Send.SkillComplete(creature, skill.Info.Id, unkInt);
		}

		public int GetTime(string method)
		{
			return 5000;
		}

		/// <summary>
		/// Checks tools for method, returns true if everything is in order.
		/// </summary>
		/// <param name="creature"></param>
		/// <returns></returns>
		private bool CheckTools(Creature creature, string method)
		{
			var right = "";
			var left = "";

			switch (method)
			{
				case CookingMethod.Mixing:
					right = "/cooking/cooking_knife/";
					left = "/cooking/cooking_table/";
					break;

				case CookingMethod.Kneading:
				case CookingMethod.NoodleMaking:
				case CookingMethod.PastaMaking:
				case CookingMethod.PieMaking:
					right = "/cooking/cooking_kneader/";
					left = "/cooking/cooking_table/";
					break;

				case CookingMethod.Baking:
				case CookingMethod.Simmering:
				case CookingMethod.Boiling:
				case CookingMethod.DeepFrying:
				case CookingMethod.StirFrying:
				case CookingMethod.JamMaking:
				case CookingMethod.Steaming:
					right = "/cooking/cooking_dipper/";
					left = "/cooking/cooking_pot/";
					break;

				default:
					Log.Error("Cooking.CheckTools: Unknown cooking method.");
					return false;
			}

			// Check right hand
			if (right != "")
			{
				if ((creature.RightHand == null || !creature.RightHand.HasTag(right)))
					return false;

				if (creature.RightHand.Durability == 0)
				{
					Send.Notice(creature, Localization.Get("You can't use this {0} anymore."), creature.RightHand.Data.Name);
					return false;
				}
			}

			// Check left hand
			if (left != "")
			{
				if ((creature.LeftHand == null || !creature.LeftHand.HasTag(left)))
					return false;

				if (creature.LeftHand.Durability == 0)
				{
					Send.Notice(creature, Localization.Get("You can't use this {0} anymore."), creature.RightHand.Data.Name);
					return false;
				}
			}

			return true;
		}

		/// <summary>
		/// Checks rank for method, returns true if everything is in order.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="method"></param>
		/// <param name="skillRank"></param>
		/// <returns></returns>
		private bool CheckRank(Creature creature, string method, SkillRank skillRank)
		{
			switch (method)
			{
				case CookingMethod.Mixing: return true;
				case CookingMethod.Baking: return (skillRank >= SkillRank.RF);
				case CookingMethod.Simmering: return (skillRank >= SkillRank.RE);
				case CookingMethod.Kneading: return (skillRank >= SkillRank.RD);
				case CookingMethod.Boiling: return (skillRank >= SkillRank.RC);
				case CookingMethod.NoodleMaking: return (skillRank >= SkillRank.RB);
				case CookingMethod.DeepFrying: return (skillRank >= SkillRank.RA);
				case CookingMethod.StirFrying: return (skillRank >= SkillRank.R9);
				case CookingMethod.PastaMaking: return (skillRank >= SkillRank.R8);
				case CookingMethod.JamMaking: return (skillRank >= SkillRank.R7);
				case CookingMethod.PieMaking: return (skillRank >= SkillRank.R6);
				case CookingMethod.Steaming: return (skillRank >= SkillRank.R5);

				default:
					Log.Error("Cooking.CheckRank: Unknown cooking method.");
					return false;
			}
		}

		/// <summary>
		/// Checks prop for method, returns true if everything is in order.
		/// </summary>
		/// <param name="method"></param>
		/// <param name="propEntityId"></param>
		/// <returns></returns>
		private bool CheckProp(Creature creature, string method, long propEntityId)
		{
			switch (method)
			{
				// Does't require a prop
				case CookingMethod.Mixing:
				case CookingMethod.Kneading:
				case CookingMethod.NoodleMaking:
				case CookingMethod.PastaMaking:
				case CookingMethod.PieMaking:
					return true;

				// Requires a prop, continue to checks.
				case CookingMethod.Baking:
				case CookingMethod.Simmering:
				case CookingMethod.Boiling:
				case CookingMethod.DeepFrying:
				case CookingMethod.StirFrying:
				case CookingMethod.JamMaking:
				case CookingMethod.Steaming:
					break;

				default:
					Log.Error("Cooking.CheckProp: Unknown cooking method.");
					return false;
			}

			// Check prop id
			if (propEntityId == 0)
				return false;

			// Check prop
			var prop = creature.Region.GetProp(propEntityId);
			if (prop == null)
				return false;

			// Check range
			if (!creature.GetPosition().InRange(prop.GetPosition(), 500))
			{
				Send.Notice(creature, Localization.Get("You are too far away."));
				return false;
			}

			// Check prop type
			if (!prop.HasTag("/cooker/"))
				return false;

			return true;
		}

		/// <summary>
		/// Calculates quality based on recipe and ingredients.
		/// </summary>
		/// <remarks>
		/// The formula used in this method is unofficial. While it does feel
		/// very similar in some test cases, it could potentially create
		/// very different results. Officials also RNG the results,
		/// which this method currently does not.
		/// 
		/// The Help fields in the return value specify a tip on what went
		/// wrong with the cooking attempt, if certain requirements are
		/// fulfilled. In that case it will set the item to the item in
		/// question, and the amount to the amount the ingredient differed
		/// from the recipe. If the value is lower than 0, it was less,
		/// if it's greater, it was more. If no helpful tip could be found,
		/// item is null.
		/// </remarks>
		/// <param name="recipe"></param>
		/// <param name="ingredients"></param>
		/// <returns></returns>
		private Judgement JudgeQuality(RecipeData recipe, List<Ingredient> ingredients)
		{
			Judgement result;
			result.Quality = 0;
			result.HelpItem = null;
			result.HelpAmount = 0;

			var total = (float)recipe.MainIngredients.Sum(a => a.Amount);

			foreach (var ingredient in ingredients)
			{
				// Every item *should* only appear once in main or other.
				var ingredientData = recipe.MainIngredients.FirstOrDefault(a => a.ItemId == ingredient.Item.Info.Id);
				if (ingredientData == null)
				{
					ingredientData = recipe.OtherIngredients.FirstOrDefault(a => a.ItemId == ingredient.Item.Info.Id);
					if (ingredientData == null)
					{
						Log.Error("Cooking.JudgeQuality: Failed to get ingredient data for item '{0}' in recipe '{1},{2}'.", ingredient.Item.Info.Id, recipe.Method, recipe.ItemId);
						break;
					}
				}

				// Calculate the amount difference between the provided
				// ingredient and the recipe.
				var amount = ingredient.Amount;
				var ideal = (1f / total * ingredientData.Amount);
				var difference = ideal - amount;
				var differenceAbs = Math.Abs(difference);

				// Calculate quality
				var rate = 1f - (1f / ideal * (differenceAbs * 2f));
				var qualityAdd = ingredientData.QualityMin + rate * (ingredientData.QualityMax - ingredientData.QualityMin);

				result.Quality = Math2.Clamp(-100, 100, result.Quality + qualityAdd);

				// Save the ingredient with the biggest difference,
				// for the help message.
				if (differenceAbs > 0.05f && Math.Abs(result.HelpAmount) < differenceAbs)
				{
					result.HelpAmount = -difference;
					result.HelpItem = ingredient.Item;
				}
			}

			return result;
		}

		/// <summary>
		/// Handles part of the skill training.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="method"></param>
		/// <param name="item"></param>
		/// <param name="rating"></param>
		private void OnSuccessfulCooking(Creature creature, Skill skill, string method, Item item, Rating rating)
		{
			if (skill.Info.Rank == SkillRank.Novice)
			{
				if (method == CookingMethod.Mixing)
					skill.Train(1); // Make any dish by mixing cooking ingredients.

				return;
			}

			if (skill.Info.Rank == SkillRank.RF)
			{
				if (method == CookingMethod.Baking)
				{
					if (rating == DeliciousRating)
						skill.Train(1); // Make a dish that is deliciously baked.
					else
						skill.Train(2); // Successful in baking a dish.
				}
				else if (method == CookingMethod.Mixing)
					skill.Train(5); // Make any dish by mixing cooking ingredients.

				return;
			}

			if (skill.Info.Rank == SkillRank.RE)
			{
				if (method == CookingMethod.Simmering)
				{
					if (rating == DeliciousRating)
						skill.Train(1); // Make a dish that is deliciously simmered.
					else
						skill.Train(2); // Successful in simmering a dish.
				}
				else if (method == CookingMethod.Baking)
					skill.Train(4); // Successful in baking a dish.

				return;
			}

			if (skill.Info.Rank == SkillRank.RD)
			{
				if (method == CookingMethod.Kneading)
					skill.Train(1); // Successful in kneading a dish.
				else if (method == CookingMethod.Simmering)
					skill.Train(2); // Successful in simmering a dish.
				else if (method == CookingMethod.Baking)
					skill.Train(5); // Successful in baking a dish.

				return;
			}

			if (skill.Info.Rank == SkillRank.RC)
			{
				if (method == CookingMethod.Kneading)
				{
					if (rating == DeliciousRating)
						skill.Train(1); // Make a dish that is deliciously boiled.
					else
						skill.Train(2); // Successful in boiling a dish.
				}
				else if (method == CookingMethod.Kneading)
					skill.Train(4); // Successful in baking a dish.

				return;
			}

			if (skill.Info.Rank == SkillRank.RB)
			{
				if (method == CookingMethod.NoodleMaking)
					skill.Train(1); // Make noodles.
				else if (method == CookingMethod.Boiling)
					skill.Train(2); // Successful in boiling a dish.
				else if (method == CookingMethod.Kneading)
					skill.Train(5); // Successful in baking a dish.

				return;
			}

			if (skill.Info.Rank == SkillRank.RA)
			{
				if (method == CookingMethod.DeepFrying)
				{
					if (rating == DeliciousRating)
						skill.Train(1); // Make a dish that is deliciously deep-fried.
					else
						skill.Train(2); // Successful in deep-frying a dish.
				}
				else if (method == CookingMethod.NoodleMaking)
					skill.Train(5); // Make noodles.

				return;
			}

			if (skill.Info.Rank == SkillRank.R9)
			{
				if (method == CookingMethod.StirFrying)
				{
					if (rating == DeliciousRating)
						skill.Train(1); // Make a dish that is deliciously stir-fried.
					else
						skill.Train(2); // Successful in stir-frying a dish.
				}
				else if (method == CookingMethod.DeepFrying)
					skill.Train(4); // Successful in deep-frying a dish.

				return;
			}

			if (skill.Info.Rank == SkillRank.R8)
			{
				if (method == CookingMethod.PastaMaking)
					skill.Train(1); // Make pasta.
				else if (method == CookingMethod.StirFrying)
					skill.Train(2); // Successful in stir-frying a dish.
				else if (method == CookingMethod.DeepFrying)
					skill.Train(4); // Successful in deep-frying a dish.

				return;
			}

			if (skill.Info.Rank == SkillRank.R7)
			{
				if (method == CookingMethod.JamMaking)
					skill.Train(1); // Make jam.
				else if (method == CookingMethod.PastaMaking)
					skill.Train(2); // Make pasta.
				else if (method == CookingMethod.StirFrying)
					skill.Train(4); // Successful in stir-frying a dish.

				return;
			}

			if (skill.Info.Rank == SkillRank.R6)
			{
				if (method == CookingMethod.PieMaking)
					skill.Train(1); // Make a Pie
				else if (method == CookingMethod.JamMaking)
					skill.Train(2); // Make Jam
				else if (method == CookingMethod.PastaMaking)
					skill.Train(4); // Make Pasta

				return;
			}

			if (skill.Info.Rank == SkillRank.R5)
			{
				if (method == CookingMethod.Steaming)
					skill.Train(1); // Steam a Dish
				else if (method == CookingMethod.PieMaking)
					skill.Train(2); // Make a Pie
				else if (method == CookingMethod.JamMaking)
					skill.Train(4); // Make Jam

				return;
			}
		}

		/// <summary>
		/// Handles part of the skill training.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="item"></param>
		private void OnPlayerUsesItem(Creature creature, Item item)
		{
			// Get skill
			var skill = creature.Skills.Get(SkillId.Cooking);
			if (skill == null)
				return;

			// Check if item is food
			if (!item.HasTag("/food/"))
				return;

			// Get quality and method
			var method = item.MetaData1.GetString("MKACT");
			if (method == null)
				return;

			var quality = item.MetaData1.GetInt("QUAL");
			var rating = this.GetRating(quality);

			if (skill.Info.Rank == SkillRank.RF)
			{
				if (method == CookingMethod.Simmering)
					skill.Train(3); // Eat a simmered dish without sharing.
				else if (method == CookingMethod.Baking)
				{
					if (rating == DeliciousRating)
						skill.Train(4); // Eat a deliciously baked dish.
				}

				return;
			}

			if (skill.Info.Rank == SkillRank.RE)
			{
				if (method == CookingMethod.Simmering)
				{
					if (rating == DeliciousRating)
						skill.Train(3); // Eat a deliciously simmered dish.
				}

				return;
			}

			if (skill.Info.Rank == SkillRank.RD)
			{
				if (method == CookingMethod.Boiling)
					skill.Train(3); // Eat a boiled dish without sharing.
				else if (method == CookingMethod.Simmering)
				{
					if (rating == DeliciousRating)
						skill.Train(4); // Eat a deliciously simmered dish.
				}

				return;
			}

			if (skill.Info.Rank == SkillRank.RC)
			{
				if (method == CookingMethod.Boiling)
				{
					if (rating == DeliciousRating)
						skill.Train(3); // Eat a deliciously boiled dish.
				}

				return;
			}

			if (skill.Info.Rank == SkillRank.RB)
			{
				if (method == CookingMethod.DeepFrying)
					skill.Train(3); // Eat a deep-fried dish without sharing.
				else if (method == CookingMethod.Boiling)
				{
					if (rating == DeliciousRating)
						skill.Train(4); // Eat a deliciously boiled dish.
				}

				return;
			}

			if (skill.Info.Rank == SkillRank.RA)
			{
				if (method == CookingMethod.StirFrying)
					skill.Train(3); // Eat a stir-fried dish without sharing.
				else if (method == CookingMethod.DeepFrying)
				{
					if (rating == DeliciousRating)
						skill.Train(4); // Eat a deliciously deep-fried dish.
				}

				return;
			}

			if (skill.Info.Rank >= SkillRank.R9 && skill.Info.Rank <= SkillRank.R7)
			{
				if (method == CookingMethod.StirFrying)
				{
					if (rating == DeliciousRating)
						skill.Train(3); // Eat a deliciously stir-fried dish.
				}

				return;
			}

			// TODO: Jam and pies don't have jam and pie tags,
			//   how to identify them for the last two ranks
			//   without hard-coded ids?

			if (skill.Info.Rank == SkillRank.R6)
			{
				//if (item.HasTag("/jam/"))
				//	skill.Train(3); // Eat Jam

				return;
			}

			if (skill.Info.Rank == SkillRank.R5)
			{
				//if (item.HasTag("/pie/"))
				//	skill.Train(3); // Eat a Pie

				return;
			}
		}

		/// <summary>
		/// Returns star rating based on quality.
		/// </summary>
		/// <param name="val"></param>
		/// <returns></returns>
		private Rating GetRating(int val)
		{
			return val > 95 ? Rating.FiveStars : val > 75 ? Rating.FourStars : val > 55 ? Rating.ThreeStars : val > 35 ? Rating.TwoStars : Rating.OneStar;
		}

		private struct Judgement
		{
			public float Quality;

			public Item HelpItem;
			public float HelpAmount;
		}

		private enum Rating
		{
			FiveStars,
			FourStars,
			ThreeStars,
			TwoStars,
			OneStar,
		}
	}

	public class Ingredient
	{
		public Item Item { get; private set; }
		public float Amount { get; private set; }

		public Ingredient(Item item, float amount)
		{
			this.Item = item;
			this.Amount = amount;
		}
	}
}
