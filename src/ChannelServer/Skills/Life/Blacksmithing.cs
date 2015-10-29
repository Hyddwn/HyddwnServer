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
	public class Blacksmithing : IPreparable, ICompletable
	{
		// Item meta data var names
		private const string ProgressVar = "PRGRATE";
		private const string QualityVar = "QUAL";
		private const string SignNameVar = "MKNAME";
		private const string SignRankVar = "MKSLV";

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
		/// Amount of durability the hammer loses on each try.
		/// </summary>
		private const int ToolDurabilityLoss = 75;

		/// <summary>
		/// Amount of durability the manual loses on each try.
		/// </summary>
		private const int ManualDurabilityLoss = 1000;

		/// <summary>
		/// Base success chance used in success formula.
		/// </summary>
		private static readonly int[] SuccessTable = { 96, 93, 91, 88, 87, 85, 84, 83, 81, 79, 77, 74, 72, 71, 70, 54, 39, 27, 19, 12, 7, 5, 3, 1, 1, 1, 0, 0, 0, 0 };

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
			var finish = packet.GetInt();

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
					var deviation = (byte)(skill.Info.Rank < SkillRank.R9 ? 3 : 2);

					var dots = new List<BlacksmithDot>();
					for (int i = 0; i < 5; ++i)
					{
						var dot = new BlacksmithDot();

						dot.Deviation = rnd.Next(0, deviation + 1);
						dot.X = rnd.Next(FieldMin, FieldMax + 1);
						dot.Y = rnd.Next(FieldMin, FieldMax + 1);
						dot.TimeDisplacement = rnd.Between(0.81f, 0.98f);

						dots.Add(dot);
					}

					// Start minigame
					Send.BlacksmithingMiniGame(creature, prop, item, dots, deviation);

					// Save dots for finish
					creature.Temp.BlacksmithingMiniGameDots = dots;

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
			var unkInt2 = packet.GetInt();

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
				var chance = this.GetSuccessChance(creature, skill.Info.Rank, manualData.Rank);
				success = (rnd.NextDouble() * 100 < chance);

				// Calculate progress to add
				// Base line is between 50 and 100% of the max progress from
				// the db. For example, a Popo's skirt has 200%, which should
				// always put it on 100% instantly, as long as it's a success.
				var addProgress = rnd.Between(manualData.MaxProgress / 2, manualData.MaxProgress);
				var rankDiff = ((int)skill.Info.Rank - (int)manualData.Rank);

				// Weather bonus
				if (ChannelServer.Instance.Weather.GetWeatherType(creature.RegionId) == WeatherType.Rain)
					addProgress += manualData.RainBonus;

				// Progress
				progress = Math.Min(1, progress + addProgress);
				item.MetaData1.SetFloat(ProgressVar, progress);

				// Message
				var msg = Localization.Get("Success!");

				if (progress == 1)
					msg += Localization.Get("\nFinal Stage remaining");
				else
					msg += string.Format(Localization.Get("\n{0}% completed."), (int)(progress * 100));

				Send.Notice(creature, msg);
			}
			else
			{
				var quality = this.CalculateQuality(hits, creature.Temp.BlacksmithingMiniGameDots);
				this.FinishItem(creature, skill, manualData, item, quality);
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
				Send.Notice(creature, Localization.Get("You ned a Hammer in your right hand\nand a Blacksmith Manual in your left."));
				return false;
			}

			return true;
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
					Log.Warning("Blacksmithing.Complete: Creature '{0:X16}' tried to use non-existent material item.", creature.EntityId);
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
				var smithCount = members.Where(a => a != creature && a.Skills.Has(SkillId.Blacksmithing, SkillRank.RF)).Count();
				if (smithCount != 0)
					chance += (int)(smithCount * 5 / 100f * chance);
			}

			return Math2.Clamp(0, 99, chance);
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

				var timingMax = 6000 * (1f + (1f - p1.TimeDisplacement)) * 0.85f;
				var timingMin = timingMax * 0.80f;

				// Static -30 when not hit at all or timing was wrong
				if (!p2.Performed || p2.Timing < timingMin || p2.Timing > timingMax)
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
			// ...

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

			// Send notice
			Send.Notice(creature, msg, item.Data.Name);
		}

		private class HammerHit
		{
			public bool Performed { get; set; }
			public int X { get; set; }
			public int Y { get; set; }
			public int Timing { get; set; }
		}
	}

	public class BlacksmithDot
	{
		public int Deviation { get; set; }
		public int X { get; set; }
		public int Y { get; set; }
		public float TimeDisplacement { get; set; }
	}
}
