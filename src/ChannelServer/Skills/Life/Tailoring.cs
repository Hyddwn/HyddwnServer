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
	/// </remarks>
	[Skill(SkillId.Tailoring)]
	public class Tailoring : IPreparable, ICompletable
	{
		private const string ProgressVar = "PRGRATE";
		private const string UnkVar = "STCLMT";
		private const string QualityVar = "QUAL";
		private const string SignNameVar = "MKNAME";
		private const string SignRankVar = "MKSLV";

		/// <summary>
		/// Prepares the skill.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		/// <returns></returns>
		public bool Prepare(Creature creature, Skill skill, Packet packet)
		{
			var stage = packet.GetByte();
			var unkLong1 = packet.GetLong();
			var unkInt1 = packet.GetInt();
			var existingItemEntityId = packet.GetLong();
			// ...

			// Check tools
			if (!CheckTools(creature))
			{
				Send.MsgBox(creature, Localization.Get("You need a Tailoring Kit in your right hand\nand a Sewing Pattern in your left."));
				return false;
			}

			// Check if ready for completion
			if (existingItemEntityId != 0 && stage == 1)
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
					// Start minigame if item is complete
					var rnd = RandomProvider.Get();

					var xOffset = (short)rnd.Next(30, 50);
					var yOffset = (short)rnd.Next(20, 30);
					var deviation = new byte[6];
					for (int i = 0; i < deviation.Length; ++i)
						deviation[i] = (byte)rnd.Next(0, 5);

					Send.TailoringMiniGame(creature, item, xOffset, yOffset, deviation);

					// Save offsets for complete
					creature.Temp.TailoringMiniGameX = xOffset;
					creature.Temp.TailoringMiniGameY = yOffset;

					return false;
				}
			}

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

			var stage = packet.GetByte();
			var unkLong1 = packet.GetLong();
			var unkInt1 = packet.GetInt();
			var existingItemEntityId = packet.GetLong();
			var unkInt2 = packet.GetInt();

			if (stage == 1)
			{
				// Materials
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
						goto L_Fail;
					}

					materials.Add(new ProductionMaterial(item, amount));
				}
			}
			else if (stage == 2)
			{
				var gotStitches = packet.GetBool();
				if (!gotStitches)
					goto L_Fail;

				// Stitches
				for (int i = 0; i < 6; ++i)
				{
					var x = packet.GetShort();
					var y = packet.GetShort();

					stitches.Add(new Point(x, y));
				}
			}
			else
				throw new Exception("Unknown progress stage.");

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

			// Get to work
			if (existingItem == null)
			{
				var addProgress = rnd.Between(manualData.MaxProgress / 2, manualData.MaxProgress);

				// Create new item
				var item = new Item(manualData.ItemId);
				item.OptionInfo.Flags |= ItemFlags.Incomplete;
				item.MetaData1.SetFloat(ProgressVar, addProgress);
				item.MetaData1.SetLong(UnkVar, DateTime.Now);

				creature.Inventory.Add(item, true);
			}
			else
			{
				// Increase progress
				var progress = existingItem.MetaData1.GetFloat(ProgressVar);
				var finished = false;
				if (progress < 1)
				{
					var addProgress = rnd.Between(manualData.MaxProgress / 2, manualData.MaxProgress);

					progress = Math.Min(1, progress + addProgress);
					existingItem.MetaData1.SetFloat(ProgressVar, progress);
				}
				else
				{
					var quality = this.CalculateQuality(stitches, creature.Temp.TailoringMiniGameX, creature.Temp.TailoringMiniGameY);
					this.FinishItem(creature, skill, manualData, existingItem, quality);
					finished = true;
				}

				Send.ItemUpdate(creature, existingItem);
				if (finished)
					Send.AcquireInfo2(creature, "tailoring", existingItem.EntityId);
			}

			Send.UseMotion(creature, 14, 0); // Success motion
			Send.Echo(creature, packet);
			return;

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
			// Sanity check, client checks it as well.
			if (
				creature.RightHand == null || !creature.RightHand.HasTag("/tailor/kit/") ||
				creature.Magazine == null || !creature.Magazine.HasTag("/tailor/manual/") || !creature.Magazine.MetaData1.Has("FORMID")
			)
			{
				Send.MsgBox(creature, Localization.Get("You need a Tailoring Kit in your right hand\nand a Sewing Pattern in your left."));
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

			// Send notice
			Send.Notice(creature, msg, item.Data.Name);
		}

		private enum Bonus
		{
			Protection,
			Durability,
		}
	}
}
