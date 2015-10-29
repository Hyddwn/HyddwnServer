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
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.Skills.Life
{
	/// <summary>
	/// Handler for the skill Tailoring.
	/// </summary>
	/// <remarks>
	/// Starting Tailoring calls Prepare, once the creation process is done,
	/// Complete is called. There is no way to cancel the skill once Prepare
	/// was called.
	/// 
	/// Due to the nature of the skill and its requirements, and a lack of
	/// research data caused by those factors, some areas of this handler
	/// are based on guesses. If you have deeper insights or new information
	/// that shed new light on things, please fix the mistakes and make a PR
	/// or tell us about them.
	/// </remarks>
	[Skill(SkillId.Tailoring)]
	public class Tailoring : IPreparable, ICompletable
	{
		// Item meta data var names
		private const string ProgressVar = "PRGRATE";
		private const string UnkVar = "STCLMT";
		private const string QualityVar = "QUAL";
		private const string SignNameVar = "MKNAME";
		private const string SignRankVar = "MKSLV";

		/// <summary>
		/// Amount of durability the kit loses on each try.
		/// </summary>
		private const int ToolDurabilityLoss = 75;

		/// <summary>
		/// Amount of durability the pattern loses on each try.
		/// </summary>
		private const int PatternDurabilityLoss = 1000;

		/// <summary>
		/// Base success chance used in success formula.
		/// </summary>
		private static readonly int[] SuccessTable = { 96, 93, 91, 88, 87, 85, 84, 83, 81, 79, 77, 74, 72, 71, 70, 54, 39, 27, 19, 12, 7, 5, 3, 1, 1, 1, 0, 0, 0, 0 };

		/// <summary>
		/// Prepares the skill.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		/// <returns></returns>
		public bool Prepare(Creature creature, Skill skill, Packet packet)
		{
			var materials = new List<ProductionMaterial>();
			var stitches = new List<Point>();

			var stage = (Stage)packet.GetByte();
			var propEntityId = packet.GetLong();
			var unkInt1 = packet.GetInt();
			var existingItemEntityId = packet.GetLong();
			var unkInt2 = packet.GetInt();

			if (stage == Stage.Progression)
			{
				// Materials
				if (!this.ReadMaterials(creature, packet, out materials))
					return false;
			}
			else if (stage == Stage.Finish)
			{
				// Stitches
				if (!this.ReadStitches(creature, packet, out stitches))
					return false;
			}
			else
			{
				Send.ServerMessage(creature, Localization.Get("Stage error, please report."));
				Log.Error("Tailoring: Unknown progress stage '{0}'.", stage);
				return false;
			}

			// Check tools
			if (!CheckTools(creature))
			{
				Send.MsgBox(creature, Localization.Get("You need a Tailoring Kit in your right hand\nand a Sewing Pattern in your left."));
				return false;
			}

			// Check if ready for completion
			if (stage == Stage.Progression && existingItemEntityId != 0)
			{
				// Check item
				var item = creature.Inventory.GetItem(existingItemEntityId);
				if (item == null)
				{
					Log.Warning("Tailoring.Complete: Creature '{0:X16}' tried to work on non-existent item.", creature.EntityId);
					return false;
				}

				// Check item progress
				if (item.MetaData1.GetFloat(ProgressVar) == 1)
				{
					var rnd = RandomProvider.Get();

					// Get manual
					var manualId = creature.Magazine.MetaData1.GetInt("FORMID");
					var manualData = AuraData.ManualDb.Find(ManualCategory.Tailoring, manualId);
					if (manualData == null)
					{
						Log.Error("Tailoring.Complete: Manual '{0}' not found.", manualId);
						Send.ServerMessage(creature, Localization.Get("Failed to look up pattern, please report."));
						return false;
					}

					// Get items to decrement
					List<ProductionMaterial> toDecrement;
					if (!this.GetItemsToDecrement(creature, Stage.Finish, manualData, materials, out toDecrement))
						return false;

					// Decrement mats
					this.DecrementMaterialItems(creature, toDecrement, rnd);

					// Start minigame
					var xOffset = (short)rnd.Next(30, 50);
					var yOffset = (short)rnd.Next(20, 30);
					var deviation = new byte[6];
					var deviation2 = (byte)(skill.Info.Rank < SkillRank.R9 ? 4 : 2);
					for (int i = 0; i < deviation.Length; ++i)
						deviation[i] = (byte)rnd.Next(0, deviation2 + 1);

					Send.TailoringMiniGame(creature, item, xOffset, yOffset, deviation, deviation2);

					// Save offsets for complete
					creature.Temp.TailoringMiniGameX = xOffset;
					creature.Temp.TailoringMiniGameY = yOffset;

					return false;
				}
			}

			// Skill training
			if (skill.Info.Rank == SkillRank.Novice)
				skill.Train(1); // Use the skill.

			Send.Echo(creature, Op.SkillUse, packet);
			skill.State = SkillState.Used;

			return true;
		}

		/// <summary>
		/// Completes skill, creating the items.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void Complete(Creature creature, Skill skill, Packet packet)
		{
			var materials = new List<ProductionMaterial>();
			var stitches = new List<Point>();

			var stage = (Stage)packet.GetByte();
			var propEntityId = packet.GetLong();
			var unkInt1 = packet.GetInt();
			var existingItemEntityId = packet.GetLong();
			var unkInt2 = packet.GetInt();

			if (stage == Stage.Progression)
			{
				// Materials
				if (!this.ReadMaterials(creature, packet, out materials))
					goto L_Fail;
			}
			else if (stage == Stage.Finish)
			{
				// Stitches
				if (!this.ReadStitches(creature, packet, out stitches))
					goto L_Fail;
			}
			else
			{
				Send.ServerMessage(creature, Localization.Get("Stage error, please report."));
				Log.Error("Tailoring: Unknown progress stage '{0}'.", stage);
				goto L_Fail;
			}

			// Check tools
			if (!CheckTools(creature))
				goto L_Fail;

			// Get manual
			var manualId = creature.Magazine.MetaData1.GetInt("FORMID");
			var manualData = AuraData.ManualDb.Find(ManualCategory.Tailoring, manualId);
			if (manualData == null)
			{
				Log.Error("Tailoring.Complete: Manual '{0}' not found.", manualId);
				Send.ServerMessage(creature, Localization.Get("Failed to look up pattern, please report."));
				goto L_Fail;
			}

			// Check existing item
			Item existingItem = null;
			if (existingItemEntityId != 0)
			{
				// Get item
				existingItem = creature.Inventory.GetItem(existingItemEntityId);
				if (existingItem == null)
				{
					Log.Warning("Tailoring.Complete: Creature '{0:X16}' tried to work on non-existing item.", creature.EntityId);
					goto L_Fail;
				}

				// Check id against manual
				if (existingItem.Info.Id != manualData.ItemId)
				{
					Log.Warning("Tailoring.Complete: Creature '{0:X16}' tried use an item with a different id than the manual.", creature.EntityId);
					goto L_Fail;
				}

				// Check progress
				if (!existingItem.MetaData1.Has(ProgressVar))
				{
					Log.Warning("Tailoring.Complete: Creature '{0:X16}' tried work on an item that is already finished.", creature.EntityId);
					goto L_Fail;
				}
			}

			var rnd = RandomProvider.Get();

			// Materials are only sent to Complete for progression,
			// finish materials are handled in Prepare.
			if (stage == Stage.Progression)
			{
				// Get items to decrement
				List<ProductionMaterial> toDecrement;
				if (!this.GetItemsToDecrement(creature, Stage.Progression, manualData, materials, out toDecrement))
					goto L_Fail;

				// Decrement mats
				this.DecrementMaterialItems(creature, toDecrement, rnd);
			}

			// Reduce durability
			creature.Inventory.ReduceDurability(creature.RightHand, ToolDurabilityLoss);
			creature.Inventory.ReduceDurability(creature.Magazine, PatternDurabilityLoss);

			// Get to work
			var newItem = false;
			var success = false;
			var msg = "";

			// Create new item
			if (existingItem == null)
			{
				existingItem = new Item(manualData.ItemId);
				existingItem.OptionInfo.Flags |= ItemFlags.Incomplete;
				existingItem.MetaData1.SetFloat(ProgressVar, 0);
				existingItem.MetaData1.SetLong(UnkVar, DateTime.Now);

				newItem = true;
			}

			// Finish item if progress is >= 1, otherwise increase progress.
			var progress = (newItem ? 0 : existingItem.MetaData1.GetFloat(ProgressVar));
			ProgressResult result;
			if (progress < 1)
			{
				// TODO: Random quality gain/loss?
				//   "The combination of tools and materials was quite good! (Quality +{0}%)"

				// Get success
				// Unofficial and mostly based on guessing. If process was
				// determined to be successful, a good result will happen,
				// if not, a bad one. Both are then split into very good
				// and very bad, based on another random number.
				var chance = this.GetSuccessChance(creature, skill.Info.Rank, manualData.Rank);
				success = (rnd.NextDouble() * 100 < chance);
				var rngFailSuccess = rnd.NextDouble();

				// Calculate progress to add
				// Base line is between 50 and 100% of the max progress from
				// the db. For example, a Popo's skirt has 200%, which should
				// always put it on 100% instantly, as long as it's a success.
				var addProgress = rnd.Between(manualData.MaxProgress / 2, manualData.MaxProgress);
				var rankDiff = ((int)skill.Info.Rank - (int)manualData.Rank);

				// Apply RNG fail/success
				if (!success)
				{
					// 25% chance for very bad
					if (rngFailSuccess < 0.25f)
					{
						msg += Localization.Get("Catastrophic failure!");
						addProgress /= 2f;
						result = ProgressResult.VeryBad;
					}
					// 75% chance for bad
					else
					{
						msg += Localization.Get("That didn't go so well...");
						addProgress /= 1.5f;
						result = ProgressResult.Bad;
					}
				}
				else
				{
					// 25% chance for best, if manual is >= 2 ranks
					if (rngFailSuccess < 0.25f && rankDiff <= -2)
					{
						msg += Localization.Get("You created a masterpiece!");
						addProgress *= 2f;
						result = ProgressResult.VeryGood;
					}
					// 85% chance for good
					else
					{
						// Too easy if more than two ranks below, which counts
						// as a training fail, according to the Wiki.
						if (rankDiff >= 2)
						{
							msg += Localization.Get("You did it, but that was way too easy.");
							result = ProgressResult.Bad;
						}
						else
						{
							msg += Localization.Get("Success!");
							result = ProgressResult.Good;
						}
					}
				}

				// Weather bonus
				if (ChannelServer.Instance.Weather.GetWeatherType(creature.RegionId) == WeatherType.Rain)
					addProgress += manualData.RainBonus;

				progress = Math.Min(1, progress + addProgress);
				existingItem.MetaData1.SetFloat(ProgressVar, progress);

				if (progress == 1)
					msg += Localization.Get("\nFinal Stage remaining");
				else
					msg += string.Format(Localization.Get("\n{0}% completed."), (int)(progress * 100));

				this.OnProgress(creature, skill, result);
				Send.Notice(creature, msg);
			}
			else
			{
				var quality = this.CalculateQuality(stitches, creature.Temp.TailoringMiniGameX, creature.Temp.TailoringMiniGameY);
				this.FinishItem(creature, skill, manualData, existingItem, quality);

				result = ProgressResult.Finish;
				success = true;
			}

			// Add or update item
			if (!newItem)
				Send.ItemUpdate(creature, existingItem);
			else
				creature.Inventory.Add(existingItem, true);

			// Acquire info once it's finished and updated.
			if (result == ProgressResult.Finish)
				Send.AcquireInfo2(creature, "tailoring", existingItem.EntityId);

			// Success motion if it was a good result, otherwise keep
			// going to fail.
			if (success)
			{
				Send.UseMotion(creature, 14, 0); // Success motion
				Send.Echo(creature, packet);
				return;
			}

		L_Fail:
			Send.UseMotion(creature, 14, 3); // Fail motion
			Send.Echo(creature, packet);
		}

		/// <summary>
		/// Checks right and left hand for kits and manuals and sends message
		/// if something is missing. Returns whether the necessary tools
		/// are there or not.
		/// </summary>
		/// <param name="creature"></param>
		/// <returns></returns>
		private bool CheckTools(Creature creature)
		{
			// Check for kit and valid manual
			// Sanity check, client checks it as well.
			if (
				creature.RightHand == null || !creature.RightHand.HasTag("/tailor/kit/") ||
				creature.Magazine == null || !creature.Magazine.HasTag("/tailor/manual/") || !creature.Magazine.MetaData1.Has("FORMID")
			)
			{
				Send.MsgBox(creature, Localization.Get("You need a Tailoring Kit in your right hand\nand a Sewing Pattern in your left."));
				return false;
			}

			// Check if kit has enough durability
			// Does the client check this?
			if (creature.RightHand.Durability < ToolDurabilityLoss)
			{
				Send.MsgBox(creature, Localization.Get("You can't use this Tailoring Kit anymore."));
				return false;
			}

			// Check if pattern has enough durability
			// Does the client check this?
			if (creature.Magazine.Durability < PatternDurabilityLoss)
			{
				Send.MsgBox(creature, Localization.Get("You can't use this Sewing Pattern anymore. You'll stab your fingers!"));
				return false;
			}

			return true;
		}

		/// <summary>
		/// Calculates quality, based on the stitches made by the player.
		/// </summary>
		/// <remarks>
		/// Calculates the distances between the points and the stitches made
		/// by the player and calculates the quality based on the total of
		/// all distances. Unofficial, but seems to work rather well. Except
		/// for 100 quality, which is practically impossible with this atm.
		/// </remarks>
		/// <param name="stitches"></param>
		/// <param name="offsetX"></param>
		/// <param name="offsetY"></param>
		/// <returns></returns>
		private int CalculateQuality(List<Point> stitches, int offsetX, int offsetY)
		{
			var points = new List<Point>();

			points.Add(new Point(100 - offsetX, 100 + (int)(offsetY * 2.5f))); // Lower Left
			points.Add(new Point(100 + offsetX, 100 + (int)(offsetY * 1.5f))); // Lower Right
			points.Add(new Point(100 - offsetX, 100 + (int)(offsetY * 0.5f))); // Mid Left
			points.Add(new Point(100 + offsetX, 100 - (int)(offsetY * 0.5f))); // Mid Right
			points.Add(new Point(100 - offsetX, 100 - (int)(offsetY * 1.5f))); // Upper Left
			points.Add(new Point(100 + offsetX, 100 - (int)(offsetY * 2.5f))); // Upper Right

			var total = 0.0;
			for (int i = 0; i < stitches.Count; ++i)
			{
				var p1 = points[i];
				var p2 = stitches[i];

				// Calculate distance between point and stitch made by the player
				total += Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
			}

			// Quality = 100 - (Total Distance * 2)
			// Min = -100
			// Max = 100 (increasing the max would increase the chance for 100 quality)
			return Math.Max(-100, 100 - (int)(total * 2));
		}

		/// <summary>
		/// Returns success chance between 0 and 100.
		/// </summary>
		/// <remarks>
		/// Unofficial. It's unlikely that officials use a table, instead of
		/// a formula, but for a lack of formula, we're forced to go with
		/// this. The success rates actually seem to be rather static,
		/// so it should work fine. We have all possible combinations,
		/// and with this function we do get the correct base chance.
		/// </remarks>
		/// <param name="creature"></param>
		/// <param name="skillRank"></param>
		/// <param name="manualRank"></param>
		/// <returns></returns>
		private int GetSuccessChance(Creature creature, SkillRank skillRank, SkillRank manualRank)
		{
			var diff = ((int)skillRank - (int)manualRank);
			var chance = SuccessTable[29 - (diff + 15)];

			// Production Mastery bonus
			var pm = creature.Skills.Get(SkillId.ProductionMastery);
			if (pm != null)
				chance += (byte)pm.Info.Rank;

			// Party bonus
			// http://mabination.com/threads/579-Sooni-s-Guide-to-Tailoring!-(Please-claim-back-from-me)
			if (creature.IsInParty)
			{
				var members = creature.Party.GetMembers();
				var tailorsCount = members.Where(a => a != creature && a.Skills.Has(SkillId.Tailoring, SkillRank.RF)).Count();
				if (tailorsCount != 0)
					chance += (int)(tailorsCount * 5 / 100f * chance);
			}

			return Math2.Clamp(0, 99, chance);
		}

		/// <summary>
		/// Sets appropriete flags, bonuses, and signatures, and sends
		/// notices about quality and bonuses.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="manualData"></param>
		/// <param name="item"></param>
		/// <param name="quality"></param>
		private void FinishItem(Creature creature, Skill skill, ManualData manualData, Item item, int quality)
		{
			item.OptionInfo.Flags &= ~ItemFlags.Incomplete;
			item.OptionInfo.Flags |= ItemFlags.Reproduction;
			item.MetaData1.Remove(ProgressVar);
			item.MetaData1.Remove(UnkVar);
			item.MetaData1.SetInt(QualityVar, quality);

			// Signature
			if (skill.Info.Rank >= SkillRank.R9 && manualData.Rank >= SkillRank.RA && quality >= 80)
			{
				item.MetaData1.SetString(SignNameVar, creature.Name);
				item.MetaData1.SetByte(SignRankVar, (byte)skill.Info.Rank);
			}

			// Get quality based msg
			var msg = "";
			if (quality == 100) // god
			{
				msg = Localization.Get("You created a godly {0}! Excellent work!");
			}
			else if (quality >= 80) // best
			{
				msg = Localization.Get("Your hard work produced a very nice {0}!");
			}
			else if (quality >= 50) // better
			{
				msg = Localization.Get("Your efforts created an above-average {0}!");
			}
			else if (quality >= 20) // reasonable
			{
				msg = Localization.Get("Your creation is far superior to anything you can get from a store.");
			}
			else if (quality >= -20) // normal
			{
				msg = Localization.Get("Your creation is just as good as anything you can get from a store.");
			}
			else if (quality >= -50) // worse
			{
				msg = Localization.Get("Your creation could be nice with a few repairs.");
			}
			else if (quality >= -80) // worst
			{
				msg = Localization.Get("You managed to finish the {0}, but it's pretty low quality.");
			}
			else
			{
				msg = Localization.Get("Your {0} isn't really fit for use. Maybe you could get some decent scrap wood out of it.");
			}

			// Get quality based bonuses
			var bonuses = new Dictionary<Bonus, int>();
			if (item.HasTag("/armor/cloth/") || item.HasTag("/armor/lightarmor/"))
			{
				if (quality >= 90)
				{
					bonuses[Bonus.Protection] = 2;
					bonuses[Bonus.Durability] = 5;
				}
				else if (quality >= 80)
				{
					bonuses[Bonus.Protection] = 2;
					bonuses[Bonus.Durability] = 4;
				}
				else if (quality >= 70)
				{
					bonuses[Bonus.Protection] = 1;
					bonuses[Bonus.Durability] = 4;
				}
				else if (quality >= 30)
				{
					bonuses[Bonus.Protection] = 1;
					bonuses[Bonus.Durability] = 3;
				}
				else if (quality >= 20)
				{
					bonuses[Bonus.Protection] = 1;
					bonuses[Bonus.Durability] = 2;
				}
			}
			else if (item.HasTag("/hand/glove/"))
			{
				if (quality >= 80)
					bonuses[Bonus.Protection] = 1;
			}
			else if (item.HasTag("/foot/shoes/"))
			{
				if (quality >= 90)
					bonuses[Bonus.Durability] = 4;
				else if (quality >= 40)
					bonuses[Bonus.Durability] = 3;
				else if (quality >= 20)
					bonuses[Bonus.Durability] = 2;
			}
			else if (item.HasTag("/robe/"))
			{
				if (quality >= 80)
					bonuses[Bonus.Durability] = 2;
			}

			// Apply bonuses and append msgs
			if (bonuses.Count != 0)
			{
				msg += "\n";
				foreach (var bonus in bonuses)
				{
					if (bonus.Key == Bonus.Protection)
					{
						msg += string.Format("Protection Increase {0}, ", bonus.Value);
						item.OptionInfo.Protection += (short)bonus.Value;
					}
					else if (bonus.Key == Bonus.Durability)
					{
						msg += string.Format("Durability Increase {0}, ", bonus.Value);
						item.OptionInfo.Durability += bonus.Value;
						item.OptionInfo.DurabilityMax = item.OptionInfo.DurabilityOriginal = item.OptionInfo.Durability;
					}
				}
				msg = msg.TrimEnd(',', ' ');
			}

			this.OnProgress(creature, skill, ProgressResult.Finish);

			// Send notice
			Send.Notice(creature, msg, item.Data.Name);
		}

		/// <summary>
		/// Returns list of items that have to be decremented to satisfy the
		/// manual's required materials. Actual return value is whether
		/// this process was successful, or if materials are missing.
		/// Handles necessary messages and logs.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="manualData"></param>
		/// <param name="materials"></param>
		/// <param name="toDecrement"></param>
		/// <returns></returns>
		private bool GetItemsToDecrement(Creature creature, Stage stage, ManualData manualData, List<ProductionMaterial> materials, out List<ProductionMaterial> toDecrement)
		{
			toDecrement = new List<ProductionMaterial>();

			// TODO: While Tailoring currently doesn't have multiple finishes,
			//   this could be an interesting customization,
			//   allowing finishes with a dye ampoule for example.
			var requiredMaterials = (stage == Stage.Progression ? manualData.GetMaterialList() : manualData.GetFirstFinishMaterialList());
			var inUse = new HashSet<long>();
			foreach (var reqMat in requiredMaterials)
			{
				// Check all selected items for tag matches
				foreach (var material in materials)
				{
					// Satisfy requirement with item, up to the max amount
					// needed or available
					if (material.Item.HasTag(reqMat.Tag))
					{
						// Cancel if one item matches multiple materials.
						// It's unknown how this would be handled, can it even
						// happen? Can one item maybe only be used as one material?
						if (inUse.Contains(material.Item.EntityId))
						{
							Send.ServerMessage(creature, Localization.Get("Unable to handle request, please report, with this information: ({0}/{1}:{2})."), material.Item.Info.Id, manualData.Category, manualData.Id);
							Log.Warning("ProductionSkill.Complete: Item '{0}' matches multiple materials for manual '{1}:{2}'.", material.Item.Info.Id, manualData.Category, manualData.Id);
							return false;
						}

						var amount = Math.Min(reqMat.Amount, material.Item.Amount);
						reqMat.Amount -= amount;
						toDecrement.Add(new ProductionMaterial(material.Item, amount));
						inUse.Add(material.Item.EntityId);
					}

					// Break once we got what we need
					if (reqMat.Amount == 0)
						break;
				}
			}

			if (requiredMaterials.Any(a => a.Amount != 0))
			{
				// Unofficial, the client should normally prevent this.
				Send.ServerMessage(creature, Localization.Get("Insufficient materials."));
				return false;
			}

			return true;
		}

		/// <summary>
		/// Decrements the given materials.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="toDecrement"></param>
		/// <param name="rnd"></param>
		private void DecrementMaterialItems(Creature creature, List<ProductionMaterial> toDecrement, Random rnd)
		{
			foreach (var material in toDecrement)
				creature.Inventory.Decrement(material.Item, (ushort)material.Amount);
		}

		/// <summary>
		/// Reads materuals from packet, starting with the count.
		/// Returns false if a material item wasn't found, after logging it.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="packet"></param>
		/// <param name="materials"></param>
		/// <returns></returns>
		private bool ReadMaterials(Creature creature, Packet packet, out List<ProductionMaterial> materials)
		{
			materials = new List<ProductionMaterial>();

			var count = packet.GetByte();
			for (int i = 0; i < count; ++i)
			{
				var itemEntityId = packet.GetLong();
				var amount = packet.GetShort();

				// Check item
				var item = creature.Inventory.GetItem(itemEntityId);
				if (item == null)
				{
					Log.Warning("Tailoring.Complete: Creature '{0:X16}' tried to use non-existent material item.", creature.EntityId);
					return false;
				}

				materials.Add(new ProductionMaterial(item, amount));
			}

			return true;
		}

		/// <summary>
		/// Reads stitches from packet, starting with the bool, saying whether
		/// there are any. Returns false if bool is false.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="packet"></param>
		/// <param name="stitches"></param>
		/// <returns></returns>
		private bool ReadStitches(Creature creature, Packet packet, out List<Point> stitches)
		{
			stitches = new List<Point>();

			var gotStitches = packet.GetBool();
			if (!gotStitches)
				return false;

			for (int i = 0; i < 6; ++i)
			{
				var x = packet.GetShort();
				var y = packet.GetShort();

				stitches.Add(new Point(x, y));
			}

			return true;
		}

		/// <summary>
		/// Handles skill training by progress.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="result"></param>
		private void OnProgress(Creature creature, Skill skill, ProgressResult result)
		{
			if (skill.Info.Rank == SkillRank.Novice)
			{
				skill.Train(2); // Use the skill successfully.
				return;
			}

			if (skill.Info.Rank >= SkillRank.RF && skill.Info.Rank <= SkillRank.R3)
			{
				skill.Train(1); // Use the skill successfully.
				switch (result)
				{
					case ProgressResult.VeryGood: skill.Train(2); break; // Achieve a very good result.
					case ProgressResult.Bad: skill.Train(3); break; // The result is a failure.
					case ProgressResult.VeryBad: skill.Train(4); break; // The result is very bad.
					case ProgressResult.Finish: skill.Train(5); break; // Clothes are finished.
				}

				return;
			}

			if (skill.Info.Rank >= SkillRank.R2 && skill.Info.Rank <= SkillRank.R1)
			{
				skill.Train(1); // Use the skill successfully.
				switch (result)
				{
					case ProgressResult.Bad: skill.Train(2); break; // The result is a failure.
					case ProgressResult.VeryBad: skill.Train(3); break; // The result is very bad.
					case ProgressResult.Finish: skill.Train(4); break; // Clothes are finished.
				}

				return;
			}
		}

		private enum Bonus
		{
			Protection,
			Durability,
		}

		private enum Stage
		{
			Progression = 1,
			Finish = 2,
		}

		private enum ProgressResult
		{
			VeryBad,
			Bad,
			Good,
			VeryGood,
			Finish,
		}
	}
}
