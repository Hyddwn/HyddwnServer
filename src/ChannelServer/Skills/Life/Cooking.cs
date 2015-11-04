// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
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
using System.Text;
using System.Threading.Tasks;

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
	public class Cooking : IPreparable, ICompletable
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
				if (judgement.Quality > 0)
				{
					// Create food if quality was good enough
					item = new Item(recipe.ItemId);
					item.MetaData1.SetInt("QUAL", (int)judgement.Quality);
					item.MetaData1.SetString("MKNAME", creature.Name);
					item.MetaData1.SetShort("MKSLV", (short)skill.Info.Rank);
					item.MetaData1.SetString("MKACT", creature.Temp.CookingMethod);

					// Notice
					var msg = "";
					if (judgement.Quality > 95)
						msg += Localization.Get("You just made a delicious dish!");
					else if (judgement.Quality > 75)
						msg += Localization.Get("You just made a tasty dish!");
					else if (judgement.Quality > 55)
						msg += Localization.Get("You just made an edible dish.");
					else if (judgement.Quality > 35)
						msg += Localization.Get("You just made a pretty unappetizing dish...");
					else
						msg += Localization.Get("You just made a dish... that you probably shouldn't eat. Yuck!");

					// Help message
					if (judgement.HelpItem != null)
					{
						var rnd = RandomProvider.Get();
						var helpmsg = "";
						if (judgement.HelpAmount < 0)
							helpmsg += Localization.Get("There may have been {1}%-{2}% less {0} than required.");
						else
							helpmsg += Localization.Get("There may have been {1}%-{2}% more {0} than required.");

						var name = judgement.HelpItem.Data.Name;
						var amount = Math.Abs(judgement.HelpAmount) * 100;
						var min = (amount - rnd.NextDouble()).ToString("0.0", CultureInfo.InvariantCulture);
						var max = (amount + rnd.NextDouble()).ToString("0.0", CultureInfo.InvariantCulture);

						msg += string.Format("\n" + helpmsg, name, min, max);
					}

					Send.Notice(creature, msg);

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
			if (item == null)
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
				var ideal = ingredientData.Amount / 100f;
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

		private struct Judgement
		{
			public float Quality;

			public Item HelpItem;
			public float HelpAmount;
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
