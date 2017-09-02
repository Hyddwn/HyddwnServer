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
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.Skills.Life
{
	/// <summary>
	/// Handler for the skill Blacksmithing.
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
	[Skill(SkillId.Blacksmithing)]
	public class Blacksmithing : CreationSkill, IPreparable, ICompletable
	{
		/// <summary>
		/// Size of the mini-game field.
		/// </summary>
		private const int FieldSize = 200;

		/// <summary>
		/// Min position value for dots on the field.
		/// </summary>
		private const int FieldMin = FieldSize / 10;

		/// <summary>
		/// Max position value for dots on the field.
		/// </summary>
		private const int FieldMax = FieldSize - FieldMin;

		/// <summary>
		/// Prepares skill.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		/// <returns></returns>
		public bool Prepare(Creature creature, Skill skill, Packet packet)
		{
			var materials = new List<ProductionMaterial>();
			var hits = new List<HammerHit>();

			var stage = (Stage)packet.GetByte();
			var propEntityId = packet.GetLong();
			var unkInt1 = packet.GetInt();
			var existingItemEntityId = packet.GetLong();
			var finishId = packet.GetInt();

			if (stage == Stage.Progression)
			{
				// Materials
				if (!this.ReadMaterials(creature, packet, out materials))
					return false;
			}
			else if (stage == Stage.Finish)
			{
				// Hits
				if (!this.ReadHits(creature, packet, out hits))
					return false;
			}
			else
			{
				Send.ServerMessage(creature, Localization.Get("Stage error, please report."));
				Log.Error("Tailoring: Unknown progress stage '{0}'.", stage);
				return false;
			}

			// Check tools
			if (!this.CheckTools(creature))
				return false;

			// Check if ready for completion
			if (stage == Stage.Progression && existingItemEntityId != 0)
			{
				// Check item
				var item = creature.Inventory.GetItem(existingItemEntityId);
				if (item == null)
				{
					Log.Warning("Blacksmithing.Complete: Creature '{0:X16}' tried to work on non-existent item.", creature.EntityId);
					return false;
				}

				// Check prop
				var prop = creature.Region.GetProp(propEntityId);
				if (prop == null || !creature.GetPosition().InRange(prop.GetPosition(), 500))
				{
					Send.Notice(creature, Localization.Get("You need an anvil."));
					return false;
				}

				// Check item progress
				if (item.MetaData1.GetFloat(ProgressVar) == 1)
				{
					var rnd = RandomProvider.Get();

					// Get manual
					var manualId = creature.Magazine.MetaData1.GetInt("FORMID");
					var manualData = AuraData.ManualDb.Find(ManualCategory.Blacksmithing, manualId);
					if (manualData == null)
					{
						Log.Error("Blacksmithing.Complete: Manual '{0}' not found.", manualId);
						Send.ServerMessage(creature, Localization.Get("Failed to look up pattern, please report."));
						return false;
					}

					// Get items to decrement
					var requiredMaterials = manualData.GetFinish(finishId).Materials;
					List<ProductionMaterial> toDecrement;
					if (!this.GetItemsToDecrement(creature, Stage.Finish, manualData, requiredMaterials, materials, out toDecrement))
						return false;

					// Decrement mats
					this.DecrementMaterialItems(creature, toDecrement, rnd);

					// Start minigame
					var deviation = (byte)(skill.Info.Rank < SkillRank.R9 ? 3 : 2);
					var dots = new List<BlacksmithDot>();
					for (int i = 0; i < 5; ++i)
					{
						var dot = new BlacksmithDot();

						dot.Deviation = rnd.Next(0, deviation + 1);
						dot.X = rnd.Next(FieldMin, FieldMax + 1);
						dot.Y = rnd.Next(FieldMin, FieldMax + 1);

						// Use static displacement until we know the formula.
						dot.TimeDisplacement = 1; // rnd.Between(0.81f, 0.98f);

						dots.Add(dot);
					}

					Send.BlacksmithingMiniGame(creature, prop, item, dots, deviation);

					// Save dots for finish
					creature.Temp.BlacksmithingMiniGameDots = dots;
					creature.Temp.CreationFinishId = finishId;

					return false;
				}
			}

			// Response
			Send.UseMotion(creature, 11, 1);
			Send.Echo(creature, Op.SkillUse, packet);
			skill.State = SkillState.Used;

			return true;
		}

		/// <summary>
		/// Completes skill, increasting item's progress or finishing it.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void Complete(Creature creature, Skill skill, Packet packet)
		{
			var rnd = RandomProvider.Get();
			var materials = new List<ProductionMaterial>();
			var hits = new List<HammerHit>();

			var stage = (Stage)packet.GetByte();
			var propEntityId = packet.GetLong();
			var unkInt1 = packet.GetInt();
			var existingItemEntityId = packet.GetLong();
			var finishId = packet.GetInt();

			if (stage == Stage.Progression)
			{
				// Materials
				if (!this.ReadMaterials(creature, packet, out materials))
					goto L_Fail;
			}
			else if (stage == Stage.Finish)
			{
				// Hits
				if (!this.ReadHits(creature, packet, out hits))
					goto L_Fail;
			}
			else
			{
				Send.ServerMessage(creature, Localization.Get("Stage error, please report."));
				Log.Error("Tailoring: Unknown progress stage '{0}'.", stage);
				goto L_Fail;
			}

			// Check tools
			if (!this.CheckTools(creature))
				goto L_Fail;

			// Get manual
			var manualId = creature.Magazine.MetaData1.GetInt("FORMID");
			var manualData = AuraData.ManualDb.Find(ManualCategory.Blacksmithing, manualId);
			if (manualData == null)
			{
				Log.Error("Blacksmithing.Complete: Manual '{0}' not found.", manualId);
				Send.ServerMessage(creature, Localization.Get("Failed to look up manual, please report."));
				goto L_Fail;
			}

			// Materials are only sent to Complete for progression,
			// finish materials are handled in Prepare.
			if (stage == Stage.Progression)
			{
				var requiredMaterials = manualData.GetMaterialList();

				// Get items to decrement
				List<ProductionMaterial> toDecrement;
				if (!this.GetItemsToDecrement(creature, Stage.Progression, manualData, requiredMaterials, materials, out toDecrement))
					goto L_Fail;

				// Decrement mats
				this.DecrementMaterialItems(creature, toDecrement, rnd);
			}

			// Reduce durability
			creature.Inventory.ReduceDurability(creature.RightHand, ToolDurabilityLoss);
			creature.Inventory.ReduceDurability(creature.Magazine, ManualDurabilityLoss);

			var success = true;
			var isNewItem = false;
			Item item = null;

			// Get item to work on
			if (existingItemEntityId == 0)
			{
				// Create new item
				item = new Item(manualData.ItemId);
				item.OptionInfo.Flags |= ItemFlags.Incomplete;
				item.MetaData1.SetFloat(ProgressVar, 0);

				isNewItem = true;
			}
			else
			{
				// Get item
				item = creature.Inventory.GetItem(existingItemEntityId);
				if (item == null)
				{
					Log.Warning("Blacksmithing.Complete: Creature '{0:X16}' tried to work on non-existing item.", creature.EntityId);
					goto L_Fail;
				}

				// Check id against manual
				if (item.Info.Id != manualData.ItemId)
				{
					Log.Warning("Blacksmithing.Complete: Creature '{0:X16}' tried use an item with a different id than the manual.", creature.EntityId);
					goto L_Fail;
				}

				// Check progress
				if (!item.MetaData1.Has(ProgressVar))
				{
					Log.Warning("Blacksmithing.Complete: Creature '{0:X16}' tried work on an item that is already finished.", creature.EntityId);
					goto L_Fail;
				}
			}

			// Finish item if progress is >= 1, otherwise increase progress.
			var progress = item.MetaData1.GetFloat(ProgressVar);
			if (progress < 1)
			{
				// Get success
				// Unofficial and mostly based on guessing. If process was
				// determined to be successful, a good result will happen,
				// if not, a bad one. Both are then split into very good
				// and very bad, based on another random number.
				var chance = this.GetSuccessChance(creature, skill, manualData.Rank);
				success = (rnd.NextDouble() * 100 < chance);
				var rngFailSuccess = rnd.NextDouble();

				// Calculate progress to add
				// Base line is between 50 and 100% of the max progress from
				// the db. For example, a Popo's skirt has 200%, which should
				// always put it on 100% instantly, as long as it's a success.
				var addProgress = rnd.Between(manualData.MaxProgress / 2, manualData.MaxProgress);
				var rankDiff = ((int)skill.Info.Rank - (int)manualData.Rank);

				var msg = "";
				ProgressResult result;

				// Apply RNG fail/success
				if (!success)
				{
					// 25% chance for very bad
					if (rngFailSuccess < 0.25f)
					{
						msg += Localization.Get("Yikes. That was terrible. Are you feeling okay?");
						addProgress /= 2f;
						result = ProgressResult.VeryBad;
					}
					// 75% chance for bad
					else
					{
						msg += Localization.Get("Failed...");
						addProgress /= 1.5f;
						result = ProgressResult.Bad;
					}
				}
				else
				{
					// 25% chance for best, if manual is >= 2 ranks
					if (rngFailSuccess < 0.25f && rankDiff <= -2)
					{
						msg += Localization.Get("A smashing success!!!");
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
							msg += Localization.Get("That was way too easy.");
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

				// Progress
				progress = Math.Min(1, progress + addProgress);
				item.MetaData1.SetFloat(ProgressVar, progress);

				// Message
				if (progress == 1)
					msg += Localization.Get("\nFinal Stage remaining");
				else
					msg += string.Format(Localization.Get("\n{0}% completed."), (int)(progress * 100));

				Send.Notice(creature, msg);
				this.OnProgress(creature, skill, item, result);

				// Event
				ChannelServer.Instance.Events.OnCreatureFinishedProductionOrCollection(creature, result >= ProgressResult.Good);
			}
			else
			{
				var quality = this.CalculateQuality(hits, creature.Temp.BlacksmithingMiniGameDots);
				this.FinishItem(creature, skill, manualData, creature.Temp.CreationFinishId, item, quality);
				this.OnProgress(creature, skill, item, ProgressResult.Finish);

				// Creation event
				ChannelServer.Instance.Events.OnCreatureCreatedItem(new CreationEventArgs(creature, CreationMethod.Blacksmithing, item, manualData.Rank));
			}

			// Add or update item
			if (!isNewItem)
				Send.ItemUpdate(creature, item);
			else
				creature.Inventory.Add(item, true);

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
		/// Checks hands for hammer and manual, returns false if equip isn't
		/// present, and sends a notice about it.
		/// </summary>
		/// <param name="creature"></param>
		/// <returns></returns>
		private bool CheckTools(Creature creature)
		{
			if (creature.RightHand == null || creature.Magazine == null || !creature.RightHand.HasTag("/tool/blacksmith/*/hammer/") || !creature.Magazine.HasTag("/blacksmith/manual/"))
			{
				// Sanity check, client checks it as well.
				Send.Notice(creature, Localization.Get("You need a Hammer in your right hand\nand a Blacksmith Manual in your left."));
				return false;
			}

			// Check if kit has enough durability
			if (creature.RightHand.Durability < ToolDurabilityLoss)
			{
				Send.MsgBox(creature, Localization.Get("You can't use this Blacksmith Hammer anymore."));
				return false;
			}

			// Check if manual has enough durability
			if (creature.Magazine.Durability < ManualDurabilityLoss)
			{
				Send.MsgBox(creature, Localization.Get("You can't use this blueprint anymore. It's too faded."));
				return false;
			}

			return true;
		}

		/// <summary>
		/// Reads stitches from packet, starting with the bool, saying whether
		/// there are any. Returns false if bool is false.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="packet"></param>
		/// <param name="hits"></param>
		/// <returns></returns>
		private bool ReadHits(Creature creature, Packet packet, out List<HammerHit> hits)
		{
			hits = new List<HammerHit>();

			for (int i = 0; i < 5; ++i)
			{
				var hit = new HammerHit();

				hit.Performed = packet.GetBool();
				hit.X = packet.GetShort();
				hit.Y = packet.GetShort();
				hit.Timing = packet.GetInt();

				hits.Add(hit);
			}

			return true;
		}

		/// <summary>
		/// Calculates quality, based on the stitches made by the player.
		/// </summary>
		/// <remarks>
		/// Calculates the distances between the dots and the hits performed
		/// by the player and calculates the quality based on the total of
		/// all distances. Unofficial, but seems to work rather well. Except
		/// for 100 quality, which is practically impossible with this atm.
		/// 
		/// Slightly different from Tailoring, due to the addition of timing
		/// and the ability to *not* hit. Official formula unknown, based on
		/// guesses and some minor testing. Won't match official, but it
		/// should feel good enough.
		/// </remarks>
		/// <param name="hits"></param>
		/// <param name="offsetX"></param>
		/// <param name="offsetY"></param>
		/// <returns></returns>
		private int CalculateQuality(List<HammerHit> hits, List<BlacksmithDot> dots)
		{
			var total = 0.0;
			for (int i = 0; i < hits.Count; ++i)
			{
				var p1 = dots[i];
				var p2 = hits[i];

				// Static -30 when not hit at all or timing was wrong
				// Time should be between 4k and 5k with time
				// displacement 1.
				if (!p2.Performed || p2.Timing < 4000 || p2.Timing > 5000)
				{
					total += 30;
					continue;
				}

				// Calculate distance between dot and hit performed by the player
				total += Math.Sqrt(Math.Pow(p1.X - p2.X, 2) + Math.Pow(p1.Y - p2.Y, 2));
			}

			// Quality = 100 - (Total Distance * 2)
			// Min = -100
			// Max = 100 (increasing the max would increase the chance for 100 quality)
			return Math.Max(-100, 100 - (int)(total * 2));
		}

		/// <summary>
		/// Handles skill training.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="item"></param>
		/// <param name="result"></param>
		private void OnProgress(Creature creature, Skill skill, Item item, ProgressResult result)
		{
			if (skill.Info.Rank == SkillRank.RF)
			{
				if (item.HasTag("/weapon/|/tool/"))
				{
					switch (result)
					{
						case ProgressResult.VeryGood: skill.Train(1); break; // Achieve a great result forging a tool or a weapon.
						case ProgressResult.Good: skill.Train(2); break;     // Achieve a good result forging a tool or a weapon.
						case ProgressResult.VeryBad: skill.Train(3); break;  // Achieve a failing result forging a tool or a weapon.
						case ProgressResult.Bad: skill.Train(4); break;      // Achieve a bad result forging a tool or a weapon.
						case ProgressResult.Finish: skill.Train(5); break;   // Successfully forge a tool or a weapon.
					}
				}

				return;
			}

			if (skill.Info.Rank == SkillRank.RE)
			{
				if (item.HasTag("/shield/"))
				{
					switch (result)
					{
						case ProgressResult.VeryGood: skill.Train(1); break; // Achieve a great result forging a shield.
						case ProgressResult.Good: skill.Train(2); break;     // Achieve a good result forging a shield.
						case ProgressResult.VeryBad: skill.Train(3); break;  // Achieve a failing result forging a shield.
						case ProgressResult.Bad: skill.Train(4); break;      // Achieve a bad result forging a shield.
						case ProgressResult.Finish: skill.Train(5); break;   // Successfully forge a shield.
					}
				}

				return;
			}

			if (skill.Info.Rank == SkillRank.RD)
			{
				if (item.HasTag("/helmet/"))
				{
					switch (result)
					{
						case ProgressResult.VeryGood: skill.Train(1); break; // Achieve a great result forging a helmet.
						case ProgressResult.Good: skill.Train(2); break;     // Achieve a good result forging a helmet.
						case ProgressResult.VeryBad: skill.Train(3); break;  // Achieve a failing result forging a helmet.
						case ProgressResult.Bad: skill.Train(4); break;      // Achieve a bad result forging a helmet.
						case ProgressResult.Finish: skill.Train(5); break;   // Successfully forge a helmet.
					}
				}

				return;
			}

			if (skill.Info.Rank == SkillRank.RC)
			{
				if (item.HasTag("/gauntlet/"))
				{
					switch (result)
					{
						case ProgressResult.VeryGood: skill.Train(1); break; // Achieve a great result forging gauntlets.
						case ProgressResult.Good: skill.Train(2); break;     // Achieve a good result forging gauntlets.
						case ProgressResult.VeryBad: skill.Train(3); break;  // Achieve a failing result forging gauntlets.
						case ProgressResult.Bad: skill.Train(4); break;      // Achieve a bad result forging gauntlets.
						case ProgressResult.Finish: skill.Train(5); break;   // Successfully forge gauntlets.
					}
				}

				return;
			}

			if (skill.Info.Rank == SkillRank.RB)
			{
				if (item.HasTag("/armorboots/"))
				{
					switch (result)
					{
						case ProgressResult.VeryGood: skill.Train(1); break; // Achieve a great result forging greaves.
						case ProgressResult.Good: skill.Train(2); break;     // Achieve a good result forging greaves.
						case ProgressResult.VeryBad: skill.Train(3); break;  // Achieve a failing result forging greaves.
						case ProgressResult.Bad: skill.Train(4); break;      // Achieve a bad result forging greaves.
						case ProgressResult.Finish: skill.Train(5); break;   // Successfully forge greaves.
					}
				}

				return;
			}

			if (skill.Info.Rank == SkillRank.RA)
			{
				if (item.HasTag("/armor/"))
				{
					switch (result)
					{
						case ProgressResult.VeryGood: skill.Train(1); break; // Greatly successful in making Armor
						case ProgressResult.Good: skill.Train(2); break;     // Successful in making Armors
						case ProgressResult.VeryBad: skill.Train(3); break;  // If unsuccessful in making Armor
						case ProgressResult.Bad: skill.Train(4); break;      // The result of making Armor is very poor.
						case ProgressResult.Finish: skill.Train(5); break;   // Completed making Armor 100%.
					}
				}

				return;
			}

			if (skill.Info.Rank == SkillRank.R9)
			{
				if (item.HasTag("/armor/"))
				{
					switch (result)
					{
						case ProgressResult.VeryGood: skill.Train(1); break; // Greatly successful in making Armor
						case ProgressResult.Good: skill.Train(2); break;     // Successful in making Armors
						case ProgressResult.Finish: skill.Train(3); break;   // Completed making Armor 100%.
					}
				}
				else if (item.HasTag("/weapon/|/tool/"))
				{
					switch (result)
					{
						case ProgressResult.Good: skill.Train(4); break;     // Successful in making a tool or a weapon.
						case ProgressResult.Finish: skill.Train(5); break;   // Completed making tool or a weapon 100%.
					}
				}

				return;
			}

			if (skill.Info.Rank == SkillRank.R8)
			{
				if (item.HasTag("/armor/"))
				{
					switch (result)
					{
						case ProgressResult.VeryGood: skill.Train(1); break; // Greatly successful in making Armor
						case ProgressResult.Good: skill.Train(2); break;     // Successful in making Armors
						case ProgressResult.Finish: skill.Train(3); break;   // Completed making Armor 100%.
					}
				}
				else if (item.HasTag("/shield/"))
				{
					switch (result)
					{
						case ProgressResult.Good: skill.Train(4); break;     // Successful in making a Shield.
						case ProgressResult.Finish: skill.Train(5); break;   // Completed making a Shield 100%.
					}
				}

				return;
			}

			if (skill.Info.Rank == SkillRank.R7)
			{
				if (item.HasTag("/armor/"))
				{
					switch (result)
					{
						case ProgressResult.VeryGood: skill.Train(1); break; // Achieve a great result forging armor.
						case ProgressResult.Good: skill.Train(2); break;     // Achieve a good result forging armor.
						case ProgressResult.Finish: skill.Train(3); break;   // Successfully forge armor.
					}
				}
				else if (item.HasTag("/helmet/"))
				{
					switch (result)
					{
						case ProgressResult.Good: skill.Train(4); break;     // Achieve a good result forging a helmet.
						case ProgressResult.Finish: skill.Train(5); break;   // Successfully forge a helmet.
					}
				}

				return;
			}

			if (skill.Info.Rank == SkillRank.R6)
			{
				if (item.HasTag("/armor/"))
				{
					switch (result)
					{
						case ProgressResult.VeryGood: skill.Train(1); break; // Achieve a great result forging armor.
						case ProgressResult.Good: skill.Train(2); break;     // Achieve a good result forging armor.
						case ProgressResult.Finish: skill.Train(3); break;   // Successfully forge armor.
					}
				}
				else if (item.HasTag("/gauntlet/"))
				{
					switch (result)
					{
						case ProgressResult.Good: skill.Train(4); break;     // Achieve a good result forging gauntlets.
						case ProgressResult.Finish: skill.Train(5); break;   // Successfully forge gauntlets.
					}
				}

				return;
			}

			if (skill.Info.Rank == SkillRank.R5)
			{
				if (item.HasTag("/armor/"))
				{
					switch (result)
					{
						case ProgressResult.VeryGood: skill.Train(1); break; // Achieve a great result forging armor.
						case ProgressResult.Good: skill.Train(2); break;     // Achieve a good result forging armor.
						case ProgressResult.Finish: skill.Train(3); break;   // Successfully forge armor.
					}
				}
				else if (item.HasTag("/armorboots/"))
				{
					switch (result)
					{
						case ProgressResult.Good: skill.Train(4); break;     // Achieve a good result forging greaves.
						case ProgressResult.Finish: skill.Train(5); break;   // Successfully forge greaves.
					}
				}

				return;
			}

			if (skill.Info.Rank >= SkillRank.R4 && skill.Info.Rank <= SkillRank.R3)
			{
				if (item.HasTag("/armor/"))
				{
					switch (result)
					{
						case ProgressResult.VeryGood: skill.Train(1); break; // Achieve a great result forging armor.
						case ProgressResult.Good: skill.Train(2); break;     // Achieve a good result forging armor.
						case ProgressResult.Finish: skill.Train(3); break;   // Successfully forge armor.
					}
				}
				else if (item.HasTag("/weapon/|/tool/"))
				{
					switch (result)
					{
						case ProgressResult.Good: skill.Train(4); break;     // Achieve a good result forging a tool or a weapon.
						case ProgressResult.Finish: skill.Train(5); break;   // Successfully forge a tool or a weapon.
					}
				}

				return;
			}

			if (skill.Info.Rank >= SkillRank.R2 && skill.Info.Rank <= SkillRank.R1)
			{
				if (item.HasTag("/armor/"))
				{
					switch (result)
					{
						case ProgressResult.Good: skill.Train(1); break;     // Achieve a good result forging armor.
						case ProgressResult.Finish: skill.Train(2); break;   // Successfully forge armor.
					}
				}
				else if (item.HasTag("/weapon/|/tool/"))
				{
					switch (result)
					{
						case ProgressResult.Good: skill.Train(3); break;     // Achieve a good result forging a tool or a weapon.
						case ProgressResult.Finish: skill.Train(4); break;   // Successfully forge a tool or a weapon.
					}
				}

				return;
			}
		}
	}

	public class HammerHit
	{
		public bool Performed { get; set; }
		public int X { get; set; }
		public int Y { get; set; }
		public int Timing { get; set; }
	}

	public class BlacksmithDot
	{
		public int Deviation { get; set; }
		public int X { get; set; }
		public int Y { get; set; }
		public float TimeDisplacement { get; set; }
	}
}
