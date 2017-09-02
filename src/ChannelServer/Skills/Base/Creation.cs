// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.World.Entities;
using Aura.Data.Database;
using Aura.Mabi;
using Aura.Mabi.Const;
using Aura.Mabi.Network;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Aura.Channel.Skills.Base
{
	/// <summary>
	/// Base class for Tailoring and Blacksmithing.
	/// </summary>
	public abstract class CreationSkill
	{
		// Item meta data var names
		protected const string ProgressVar = "PRGRATE";
		protected const string StclmtVar = "STCLMT";
		protected const string QualityVar = "QUAL";
		protected const string SignNameVar = "MKNAME";
		protected const string SignRankVar = "MKSLV";

		/// <summary>
		/// Amount of durability the tool loses on each try.
		/// </summary>
		protected const int ToolDurabilityLoss = 75;

		/// <summary>
		/// Amount of durability the manual loses on each try.
		/// </summary>
		protected const int ManualDurabilityLoss = 1000;

		/// <summary>
		/// Base success chance used in success formula.
		/// </summary>
		protected static readonly int[] SuccessTable = { 96, 93, 91, 88, 87, 85, 84, 83, 81, 79, 77, 74, 72, 71, 70, 54, 39, 27, 19, 12, 7, 5, 3, 1, 1, 1, 0, 0, 0, 0 };

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
		/// <param name="skill"></param>
		/// <param name="manualRank"></param>
		/// <returns></returns>
		protected int GetSuccessChance(Creature creature, Skill skill, SkillRank manualRank)
		{
			var diff = ((int)skill.Info.Rank - (int)manualRank);
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
				var tailorsCount = members.Where(a => a != creature && a.Skills.Has(skill.Info.Id, SkillRank.RF)).Count();
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
		/// <param name="finishId">
		/// The id of the finish to use, make sure to get the actual
		/// finish id, which in Blacksmithing comes from the Prepare
		/// that switches to the mini-game.
		/// </param>
		/// <param name="item"></param>
		/// <param name="quality"></param>
		protected void FinishItem(Creature creature, Skill skill, ManualData manualData, int finishId, Item item, int quality)
		{
			var finish = manualData.GetFinish(finishId);
			if (finish == null)
			{
				Log.Error("CreationSkill.FinishItem: Finish '{0}' not found.", finishId);
				Send.ServerMessage(creature, Localization.Get("Unknown finish recipe, please report."));
				return;
			}

			// Monday: Increase in quality of output for productions.
			// +5, unofficial.
			if (ErinnTime.Now.Month == ErinnMonth.AlbanEiler)
				quality = Math.Min(100, quality + 5);

			// Update item
			item.OptionInfo.Flags &= ~ItemFlags.Incomplete;
			item.OptionInfo.Flags |= ItemFlags.Reproduction;
			item.MetaData1.Remove(ProgressVar);
			item.MetaData1.Remove(StclmtVar);
			item.MetaData1.SetInt(QualityVar, quality);

			if (finish.Color1 != null) item.Info.Color1 = (uint)finish.Color1;
			if (finish.Color2 != null) item.Info.Color2 = (uint)finish.Color2;
			if (finish.Color3 != null) item.Info.Color3 = (uint)finish.Color3;

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

			// Apply bonuses and append msgs
			var bonuses = this.GetBonusesFor(item, quality);
			if (bonuses.Count != 0)
			{
				msg += "\n";
				foreach (var bonus in bonuses)
				{
					var bonusName = "?";
					switch (bonus.Key)
					{
						case Bonus.Defense:
							item.OptionInfo.Defense += bonus.Value;
							bonusName = "Defense";
							break;

						case Bonus.Protection:
							item.OptionInfo.Protection += (short)bonus.Value;
							bonusName = "Protection";
							break;

						case Bonus.Durability:
							item.OptionInfo.Durability += bonus.Value * 1000;
							bonusName = "Durability";
							break;

						case Bonus.DurabilityMax:
							item.OptionInfo.DurabilityMax += bonus.Value * 1000;
							bonusName = "Max Durability";
							break;

						case Bonus.AttackMin:
							item.OptionInfo.AttackMin += (ushort)bonus.Value;
							bonusName = "Min Attack";
							break;

						case Bonus.AttackMax:
							item.OptionInfo.AttackMax += (ushort)bonus.Value;
							bonusName = "Max Attack";
							break;

						case Bonus.Critical:
							item.OptionInfo.Critical += (sbyte)bonus.Value;
							bonusName = "Critical";
							break;

						case Bonus.Balance:
							item.OptionInfo.Balance += (byte)bonus.Value;
							bonusName = "Balance";
							break;
					}

					if (bonus.Value > 0)
						msg += string.Format("{0} Increase {1}, ", bonusName, bonus.Value);
					else
						msg += string.Format("{0} Decrease {1}, ", bonusName, bonus.Value);
				}
				msg = msg.TrimEnd(',', ' ');

				// Check attack
				if (item.OptionInfo.AttackMin > item.OptionInfo.AttackMax)
					item.OptionInfo.AttackMin = item.OptionInfo.AttackMax;

				// Check durability
				if (item.OptionInfo.Durability > item.OptionInfo.DurabilityMax)
					item.OptionInfo.Durability = item.OptionInfo.DurabilityMax;
			}

			// Send notice
			Send.Notice(creature, msg, item.Data.Name);
		}

		/// <summary>
		/// Returns a list of bonuses that the given item would get with
		/// the given quality.
		/// </summary>
		/// <remarks>
		/// Reference: http://mabination.com/threads/85245-Player-made-Item-Quality
		/// </remarks>
		/// <param name="item"></param>
		/// <param name="quality"></param>
		/// <returns></returns>
		private Dictionary<Bonus, int> GetBonusesFor(Item item, int quality)
		{
			var bonuses = new Dictionary<Bonus, int>();

			// Weapons (except bows)
			if (item.HasTag("/weapon/") && !item.HasTag("/bow/|/bow01|/crossbow/"))
			{
				// Balance
				if (quality >= 98)
					bonuses[Bonus.Balance] = 10;
				else if (quality >= 95)
					bonuses[Bonus.Balance] = 9;
				else if (quality >= 92)
					bonuses[Bonus.Balance] = 8;
				else if (quality >= 90)
					bonuses[Bonus.Balance] = 7;
				else if (quality >= 85)
					bonuses[Bonus.Balance] = 6;
				else if (quality >= 80)
					bonuses[Bonus.Balance] = 5;
				else if (quality >= 70)
					bonuses[Bonus.Balance] = 4;
				else if (quality >= 60)
					bonuses[Bonus.Balance] = 3;
				else if (quality >= 50)
					bonuses[Bonus.Balance] = 2;
				else if (quality >= 30)
					bonuses[Bonus.Balance] = 1;

				// Critical
				if (quality >= 95)
					bonuses[Bonus.Critical] = 5;
				else if (quality >= 90)
					bonuses[Bonus.Critical] = 4;
				else if (quality >= 80)
					bonuses[Bonus.Critical] = 3;
				else if (quality >= 70)
					bonuses[Bonus.Critical] = 2;
				else if (quality >= 50)
					bonuses[Bonus.Critical] = 1;

				// Max Attack
				if (quality >= 75)
					bonuses[Bonus.AttackMax] = 2;
				else if (quality >= 20)
					bonuses[Bonus.AttackMax] = 1;
				else if (quality < -80)
					bonuses[Bonus.AttackMax] = -1;

				// Min Attack
				if (quality >= 75)
					bonuses[Bonus.AttackMin] = 2;
				else if (quality >= 20)
					bonuses[Bonus.AttackMin] = 1;
				else if (quality < -80)
					bonuses[Bonus.AttackMin] = -1;

				// Durability
				if (quality >= 80)
				{
					bonuses[Bonus.Durability] = 5;
					bonuses[Bonus.DurabilityMax] = 5;
				}
				else if (quality >= 60)
				{
					bonuses[Bonus.Durability] = 4;
					bonuses[Bonus.DurabilityMax] = 4;
				}
				else if (quality >= 50)
				{
					bonuses[Bonus.Durability] = 3;
					bonuses[Bonus.DurabilityMax] = 3;
				}
				else if (quality >= 45)
				{
					bonuses[Bonus.Durability] = 2;
					bonuses[Bonus.DurabilityMax] = 2;
				}
				else if (quality >= 40)
				{
					bonuses[Bonus.Durability] = 1;
					bonuses[Bonus.DurabilityMax] = 1;
				}
				else if (quality >= -20)
				{
				}
				else if (quality >= -60)
				{
					bonuses[Bonus.Durability] = -2;
					bonuses[Bonus.DurabilityMax] = 0;
				}
				else if (quality >= -80)
				{
					bonuses[Bonus.Durability] = -2;
					bonuses[Bonus.DurabilityMax] = -1;
				}
				else
				{
					bonuses[Bonus.Durability] = -3;
					bonuses[Bonus.DurabilityMax] = -2;
				}
			}

			// Bows
			else if (item.HasTag("/bow/|/bow01/|/crossbow/"))
			{
				// Balance
				if (quality >= 98)
					bonuses[Bonus.Balance] = 5;
				else if (quality >= 80)
					bonuses[Bonus.Balance] = 4;
				else if (quality >= 70)
					bonuses[Bonus.Balance] = 3;
				else if (quality >= 50)
					bonuses[Bonus.Balance] = 2;
				else if (quality >= 30)
					bonuses[Bonus.Balance] = 1;

				// Critical
				if (quality >= 98)
					bonuses[Bonus.Critical] = 5;
				else if (quality >= 95)
					bonuses[Bonus.Critical] = 4;
				else if (quality >= 80)
					bonuses[Bonus.Critical] = 3;
				else if (quality >= 70)
					bonuses[Bonus.Critical] = 2;
				else if (quality >= 10)
					bonuses[Bonus.Critical] = 1;

				// Max Attack
				if (quality >= 90)
					bonuses[Bonus.AttackMax] = 2;
				else if (quality >= 10)
					bonuses[Bonus.AttackMax] = 1;
				else
					bonuses[Bonus.AttackMax] = -1;

				// Min Attack
				if (quality >= 95)
					bonuses[Bonus.AttackMin] = 2;
				else if (quality >= 30)
					bonuses[Bonus.AttackMin] = 1;
				else if (quality < -10)
					bonuses[Bonus.AttackMin] = -1;

				// Durability
				if (quality >= 98)
				{
					bonuses[Bonus.Durability] = 3;
					bonuses[Bonus.DurabilityMax] = 3;
				}
				else if (quality >= 90)
				{
					bonuses[Bonus.Durability] = 2;
					bonuses[Bonus.DurabilityMax] = 2;
				}
				else if (quality >= 50)
				{
					bonuses[Bonus.Durability] = 1;
					bonuses[Bonus.DurabilityMax] = 1;
				}
				else if (quality >= -10)
				{
				}
				else if (quality >= -30)
				{
					bonuses[Bonus.Durability] = -1;
					bonuses[Bonus.DurabilityMax] = -1;
				}
				else
				{
					bonuses[Bonus.Durability] = -2;
					bonuses[Bonus.DurabilityMax] = -2;
				}
			}

			// Armors and clothes
			else if (item.HasTag("/armor/cloth/|/armor/lightarmor/|/armor/heavyarmor/"))
			{
				// Defense
				if (quality >= 90)
					bonuses[Bonus.Defense] = 1;

				// Protection
				if (quality >= 95)
					bonuses[Bonus.Protection] = 3;
				else if (quality >= 75)
					bonuses[Bonus.Protection] = 2;
				else if (quality >= 50)
					bonuses[Bonus.Protection] = 1;

				// Durability
				if (quality >= 80)
				{
					bonuses[Bonus.Durability] = 5;
					bonuses[Bonus.DurabilityMax] = 5;
				}
				else if (quality >= 70)
				{
					bonuses[Bonus.Durability] = 4;
					bonuses[Bonus.DurabilityMax] = 4;
				}
				else if (quality >= 55)
				{
					bonuses[Bonus.Durability] = 3;
					bonuses[Bonus.DurabilityMax] = 3;
				}
				else if (quality >= 35)
				{
					bonuses[Bonus.Durability] = 2;
					bonuses[Bonus.DurabilityMax] = 2;
				}
				else if (quality >= -20)
				{
				}
				else if (quality >= -60)
				{
					bonuses[Bonus.Durability] = -2;
					bonuses[Bonus.DurabilityMax] = 0;
				}
				else if (quality >= -80)
				{
					bonuses[Bonus.Durability] = -2;
					bonuses[Bonus.DurabilityMax] = -1;
				}
				else
				{
					bonuses[Bonus.Durability] = -3;
					bonuses[Bonus.DurabilityMax] = -2;
				}
			}

			// Gloves and Gauntles
			else if (item.HasTag("/hand/glove/|/hand/gauntlet/"))
			{
				// Protection
				if (quality >= 80)
					bonuses[Bonus.Protection] = 1;
			}

			// Boots, Shoes, and Greaves
			else if (item.HasTag("/foot/shoes/|/foot/armorboots/"))
			{
				// Durability
				if (quality >= 95)
				{
					bonuses[Bonus.Durability] = 5;
					bonuses[Bonus.DurabilityMax] = 5;
				}
				else if (quality >= 85)
				{
					bonuses[Bonus.Durability] = 4;
					bonuses[Bonus.DurabilityMax] = 4;
				}
				else if (quality >= 60)
				{
					bonuses[Bonus.Durability] = 3;
					bonuses[Bonus.DurabilityMax] = 3;
				}
				else if (quality >= 40)
				{
					bonuses[Bonus.Durability] = 2;
					bonuses[Bonus.DurabilityMax] = 2;
				}
				else if (quality >= 20)
				{
					bonuses[Bonus.Durability] = 1;
					bonuses[Bonus.DurabilityMax] = 1;
				}
				else if (quality >= -20)
				{
				}
				else if (quality >= -60)
				{
					bonuses[Bonus.Durability] = -2;
					bonuses[Bonus.DurabilityMax] = 0;
				}
				else if (quality >= -80)
				{
					bonuses[Bonus.Durability] = -2;
					bonuses[Bonus.DurabilityMax] = -1;
				}
				else
				{
					bonuses[Bonus.Durability] = -3;
					bonuses[Bonus.DurabilityMax] = -2;
				}
			}

			// Shields
			else if (item.HasTag("/lefthand/shield/"))
			{
				// Defense
				if (quality >= 90)
					bonuses[Bonus.Defense] = 1;

				// Durability
				if (quality >= 95)
				{
					bonuses[Bonus.Durability] = 5;
					bonuses[Bonus.DurabilityMax] = 5;
				}
				else if (quality >= 90)
				{
					bonuses[Bonus.Durability] = 4;
					bonuses[Bonus.DurabilityMax] = 4;
				}
				else if (quality >= 85)
				{
					bonuses[Bonus.Durability] = 3;
					bonuses[Bonus.DurabilityMax] = 3;
				}
				else if (quality >= 80)
				{
					bonuses[Bonus.Durability] = 2;
					bonuses[Bonus.DurabilityMax] = 2;
				}
				else if (quality >= 40)
				{
					bonuses[Bonus.Durability] = 1;
					bonuses[Bonus.DurabilityMax] = 1;
				}
				else if (quality >= -20)
				{
				}
				else if (quality >= -60)
				{
					bonuses[Bonus.Durability] = -2;
					bonuses[Bonus.DurabilityMax] = 0;
				}
				else if (quality >= -80)
				{
					bonuses[Bonus.Durability] = -2;
					bonuses[Bonus.DurabilityMax] = -1;
				}
				else
				{
					bonuses[Bonus.Durability] = -3;
					bonuses[Bonus.DurabilityMax] = -2;
				}
			}

			// Hats
			else if (item.HasTag("/headgear/"))
			{
				// Durability
				if (quality >= 90)
				{
					bonuses[Bonus.Durability] = 1;
					bonuses[Bonus.DurabilityMax] = 1;
				}
			}

			// Helmets
			else if (item.HasTag("/helmet/"))
			{
				// Durability
				if (quality >= 80)
				{
					bonuses[Bonus.Durability] = 1;
					bonuses[Bonus.DurabilityMax] = 1;
				}
			}

			// Robes
			else if (item.HasTag("/robe/"))
			{
				if (quality >= 80)
				{
					bonuses[Bonus.Durability] = 1;
					bonuses[Bonus.DurabilityMax] = 1;
				}
			}

			return bonuses;
		}

		/// <summary>
		/// Returns list of items that have to be decremented to satisfy the
		/// manual's required materials. Actual return value is whether
		/// this process was successful, or if materials are missing.
		/// Handles necessary messages and logs.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="manualData"></param>
		/// <param name="materials">List of materials the client supplies.</param>
		/// <param name="toDecrement"></param>
		/// <returns></returns>
		protected bool GetItemsToDecrement(Creature creature, Stage stage, ManualData manualData, List<ProductionMaterialData> requiredMaterials, List<ProductionMaterial> materials, out List<ProductionMaterial> toDecrement)
		{
			toDecrement = new List<ProductionMaterial>();

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
							Log.Warning("CreationSkill.GetItemsToDecrement: Item '{0}' matches multiple materials for manual '{1}:{2}'.", material.Item.Info.Id, manualData.Category, manualData.Id);
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
		protected void DecrementMaterialItems(Creature creature, List<ProductionMaterial> toDecrement, Random rnd)
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
		protected bool ReadMaterials(Creature creature, Packet packet, out List<ProductionMaterial> materials)
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
					Log.Warning("CreationSkill.ReadMaterials: Creature '{0:X16}' tried to use non-existent material item.", creature.EntityId);
					return false;
				}

				materials.Add(new ProductionMaterial(item, amount));
			}

			return true;
		}
	}

	public enum Bonus
	{
		Defense,
		Protection,
		Durability,
		DurabilityMax,
		AttackMin,
		AttackMax,
		Critical,
		Balance,
	}

	public enum Stage
	{
		Progression = 1,
		Finish = 2,
	}

	public enum ProgressResult
	{
		VeryBad,
		Bad,
		Good,
		VeryGood,
		Finish,
	}
}
