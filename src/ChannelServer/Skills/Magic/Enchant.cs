// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.Skills.Base;
using Aura.Channel.World.Entities;
using Aura.Data;
using Aura.Data.Database;
using Aura.Mabi;
using Aura.Mabi.Const;
using Aura.Mabi.Network;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.Skills.Magic
{
	/// <summary>
	/// Handler for both the hidden Enchant skill used by items and the normal one.
	/// </summary>
	/// <remarks>
	/// Var1: Failure Durability Loss
	/// Var2: ?
	/// Var3: Burning Success Rate
	/// Var4: ?
	/// Var5: ?
	/// Var6: ?
	/// Var7: ?
	/// </remarks>
	[Skill(SkillId.Enchant, SkillId.HiddenEnchant)]
	public class Enchant : IPreparable, ICompletable, ICancelable
	{
		private static float[] _baseChanceB00 = { 69, 65, 60, 55, 51, 46, 32, 30, 27, 25, 20, 14, 10, 6, 4 };
		private static float[] _baseChanceB05 = { 73, 68, 63, 58, 53, 48, 34, 32, 29, 26, 21, 15, 10, 6, 4 };
		private static float[] _baseChanceB10 = { 77, 71, 66, 61, 56, 51, 35, 33, 30, 27, 22, 16, 11, 7, 5 };
		private static float[] _baseChanceB50 = { 90, 90, 90, 85, 78, 71, 50, 47, 42, 38, 31, 22, 15, 10, 7 };
		private static float[] _baseChanceB60 = { 90, 90, 90, 90, 84, 76, 53, 50, 45, 41, 33, 24, 16, 10, 7 };

		/// <summary>
		/// Chance for a huge success/fail.
		/// </summary>
		private const float HugeSuccessFailChance = 10;

		/// <summary>
		/// Prepares skill.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		/// <returns></returns>
		public bool Prepare(Creature creature, Skill skill, Packet packet)
		{
			Item item, enchant;

			if (creature.Temp.ActiveEntrustment == null)
			{
				var itemEntityId = packet.GetLong();
				long enchantEntityId = 0;

				if (skill.Info.Id == SkillId.HiddenEnchant)
				{
					enchantEntityId = packet.GetLong();
				}
				else if (skill.Info.Id == SkillId.Enchant)
				{
					var rightHand = creature.RightHand;
					var magazine = creature.Magazine;

					enchantEntityId = (magazine == null ? 0 : magazine.EntityId);

					if (rightHand == null || !rightHand.HasTag("/enchant/powder/"))
					{
						Log.Warning("Enchant.Prepare: Creature '{0:X16}' tried to use Enchant without powder.");
						return false;
					}

					if (magazine == null || !magazine.HasTag("/lefthand/enchant/"))
					{
						Log.Warning("Enchant.Prepare: Creature '{0:X16}' tried to use Enchant without enchant.");
						return false;
					}
				}

				// Get items
				item = creature.Inventory.GetItem(itemEntityId);
				enchant = creature.Inventory.GetItem(enchantEntityId);
			}
			else
			{
				item = creature.Temp.ActiveEntrustment.GetItem1();
				enchant = creature.Temp.ActiveEntrustment.GetItem2();
			}

			// Check item
			if (item == null)
			{
				Log.Warning("Enchant.Prepare: Creature '{0:X16}' tried to enchant non-existing item.");
				return false;
			}

			// Check enchant
			if (enchant == null)
			{
				Log.Warning("Enchant.Prepare: Creature '{0:X16}' tried to enchant with non-existing enchant item.");
				return false;
			}

			if (!enchant.HasTag("/enchant/"))
			{
				Log.Warning("Enchant.Prepare: Creature '{0:X16}' tried to enchant with invalid enchant scroll.");
				return false;
			}

			if (enchant.OptionInfo.Durability == 0 && enchant.OptionInfo.DurabilityOriginal != 0)
			{
				Send.Notice(creature, Localization.Get("This scroll is no longer valid for enchantment."));
				return false;
			}

			// Check ranks
			var optionSetId = this.GetOptionSetid(enchant);
			var optionSetData = AuraData.OptionSetDb.Find(optionSetId);
			if (optionSetData == null)
			{
				Log.Warning("Enchant.Prepare: Creature '{0:X16}' tried to enchant with unknown option set '{0}'.", optionSetId);
				return false;
			}

			if (!optionSetData.IgnoreRank)
			{
				// Skill rank for enchants of r5 and above
				if (optionSetData.Rank >= SkillRank.R5 && skill.Info.Rank < SkillRank.R5)
				{
					Send.Notice(creature, Localization.Get("Your Enchant skill must be Rank 5 or above to use this Enchant Scroll."));
					return false;
				}

				// Sequence for enchants of r9 and above
				if (optionSetData.Rank >= SkillRank.R9)
				{
					var checkSetId = (optionSetData.Type == UpgradeType.Prefix ? item.OptionInfo.Prefix : item.OptionInfo.Suffix);
					var checkSetData = AuraData.OptionSetDb.Find(checkSetId);
					if (checkSetData == null || checkSetData.Rank + 1 < optionSetData.Rank)
					{
						// Unofficial
						Send.Notice(creature, Localization.Get("You need to enchant Enchantments of R9 and above in sequence."));
						return false;
					}
				}
			}

			// Check expiration
			var checkExpiration = (optionSetData.Type == UpgradeType.Prefix ? "XPRPFX" : "XPRSFX");
			if (enchant.MetaData1.Has(checkExpiration))
			{
				var expiration = enchant.MetaData1.GetDateTime(checkExpiration);
				if (DateTime.Now > expiration)
				{
					Send.MsgBox(creature, Localization.Get("No enchantment options available.\nThis scroll is expired."));
					return false;
				}
			}

			// Save items for Complete
			creature.Temp.SkillItem1 = item;
			creature.Temp.SkillItem2 = enchant;

			// Response
			Send.Echo(creature, Op.SkillUse, packet);
			skill.State = SkillState.Used;

			return true;
		}

		/// <summary>
		/// Completes skill, applying the enchant.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void Complete(Creature creature, Skill skill, Packet packet)
		{
			// Ignore parameters, use data saved in Prepare.

			var rnd = RandomProvider.Get();
			var item = creature.Temp.SkillItem1;
			var enchant = creature.Temp.SkillItem2;

			var skillUser = creature;
			var itemOwner = creature;
			var powder = creature.RightHand;
			var entrustment = skillUser.Temp.ActiveEntrustment;

			if (entrustment != null)
			{
				itemOwner = entrustment.Creature1;
				powder = entrustment.GetMagicPowder(itemOwner);
			}

			var optionSetId = 0;

			skillUser.Temp.SkillItem1 = null;
			skillUser.Temp.SkillItem2 = null;

			// Get option set id
			optionSetId = this.GetOptionSetid(enchant);

			// Get and apply option set
			var optionSetData = AuraData.OptionSetDb.Find(optionSetId);
			if (optionSetData == null)
			{
				Log.Error("Enchant.Complete: Unknown option set '{0}'.", optionSetId);
				goto L_End;
			}

			// Check target
			if (!item.HasTag(optionSetData.Allow) || item.HasTag(optionSetData.Disallow))
			{
				Log.Warning("Enchant.Complete: Creature '{0:X16}' tried to use set '{0}' on invalid item '{1}'.", optionSetData.Id, item.Info.Id);
				goto L_End;
			}

			// Check success
			var success = optionSetData.AlwaysSuccess;
			if (!success)
			{
				var num = rnd.Next(100);
				var chance = GetChance(skillUser, powder, skill.Info.Id, optionSetData);
				success = num < chance;
			}

			// Handle result
			var result = EnchantResult.Fail;
			var destroy = true;
			if (success)
			{
				item.ApplyOptionSet(optionSetData, true);
				if (optionSetData.Category == OptionSetCategory.Prefix) item.OptionInfo.Prefix = (ushort)optionSetId;
				if (optionSetData.Category == OptionSetCategory.Suffix) item.OptionInfo.Suffix = (ushort)optionSetId;

				result = (rnd.Next(100) < HugeSuccessFailChance ? EnchantResult.HugeSuccess : EnchantResult.Success);
			}
			else
			{
				// Don't default destroy enchant when using Enchant
				if (skill.Info.Id == SkillId.Enchant)
					destroy = false;

				result = (rnd.Next(100) < HugeSuccessFailChance ? EnchantResult.HugeFail : EnchantResult.Fail);

				// Random item durability loss, based on rank.
				var durabilityLoss = this.GetDurabilityLoss(rnd, optionSetData.Rank, result);
				if (durabilityLoss == -1)
					itemOwner.Inventory.Remove(item);
				else if (durabilityLoss != 0)
					itemOwner.Inventory.ReduceMaxDurability(item, durabilityLoss);
			}

			if (skill.Info.Id == SkillId.Enchant)
			{
				// Training
				this.Training(skill, result);

				// Decrement powder
				if (powder != null)
					itemOwner.Inventory.Decrement(powder);
			}

			// Destroy or decrement enchant
			if (destroy)
				itemOwner.Inventory.Decrement(enchant);
			else
				itemOwner.Inventory.ReduceDurability(enchant, (int)skill.RankData.Var1 * 100);

			// Response
			Send.Effect(skillUser, Effect.Enchant, (byte)result);
			if (skillUser != itemOwner)
				Send.Effect(itemOwner, Effect.Enchant, (byte)result);

			if (success)
			{
				Send.ItemUpdate(itemOwner, item);
				Send.AcquireEnchantedItemInfo(itemOwner, item.EntityId, item.Info.Id, optionSetId);
			}

			if (entrustment != null)
				entrustment.End();

		L_End:
			Send.Echo(skillUser, packet);
		}

		/// <summary>
		/// Cancel actions.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		public void Cancel(Creature creature, Skill skill)
		{
		}

		/// <summary>
		/// Returns success chance, based on skill, option set, and powder
		/// used.
		/// <remarks>
		/// Unofficial. It kinda matches the debug output of the client,
		/// but it is a little off.
		/// </remarks>
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="rightHand"></param>
		/// <param name="skill"></param>
		/// <param name="optionSetData"></param>
		/// <returns></returns>
		public static float GetChance(Creature creature, Item rightHand, SkillId skillId, OptionSetData optionSetData)
		{
			// Check right hand, only use it if it's powder
			if (rightHand != null && !rightHand.HasTag("/enchant/powder/"))
				rightHand = null;

			// Get base chance, based on skill and powder
			var baseChance = _baseChanceB00; // (Blessed) Magic Powder/None
			if (skillId == SkillId.Enchant && rightHand != null)
			{
				if (rightHand.HasTag("/powder02/")) // Elite Magic Powder
					baseChance = _baseChanceB05;
				else if (rightHand.HasTag("/powder03/")) // Elven Magic Powder
					baseChance = _baseChanceB10;
				else if (rightHand.HasTag("/powder01/")) // Ancient Magic Powder
					baseChance = _baseChanceB50;
				else if (rightHand.HasTag("/powder04/") && rightHand.Info.Id == 85865) // Notorious Magic Powder
					baseChance = _baseChanceB60;
			}

			// Get chance
			var rank = Math2.Clamp(0, _baseChanceB00.Length - 1, (int)optionSetData.Rank - 1);
			var chance = baseChance[rank];
			var intBonus = 1f;
			var thursdayBonus = 0f;

			// Int bonus if using powder
			if (skillId == SkillId.Enchant && rightHand != null)
				intBonus = 1f + ((creature.Int - 35f) / 350f);

			// Thursday bonus
			if (ErinnTime.Now.Month == 4)
				thursdayBonus = Math.Max(0, (15 - rank) / 2f);

			// Result
			var result = Math2.Clamp(0, 90, chance * intBonus + thursdayBonus);

			// Debug
			if (creature.Titles.SelectedTitle == TitleId.devCAT)
			{
				Send.ServerMessage(creature,
					"Debug: Enchant success chance: {0:0} (base: {1:0}, int: {2:0}, thu: {3:0})",
					result, chance, (chance / 1f * (intBonus - 1f)), thursdayBonus);
			}

			return result;
		}

		/// <summary>
		/// Handles item burning, retruns whether it was successful.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="item"></param>
		/// <param name="campfire"></param>
		/// <param name="enchantersBurn"></param>
		public bool Burn(Creature creature, Item item, Prop campfire, bool enchantersBurn)
		{
			var skill = creature.Skills.Get(SkillId.Enchant);
			var enchantBurnSuccess = false;
			var powderBurnSuccess = false;
			var exp = 0;

			// Enchanter's Burn
			if (enchantersBurn)
			{
				var rnd = RandomProvider.Get();

				var isEquip = item.HasTag("/equip/");
				var hasEnchantBurnItems = (creature.Inventory.Has(51102) && creature.Inventory.Has(63016)); // Mana Herb + Holy Water

				// Enchant burning
				if (!isEquip || !hasEnchantBurnItems)
				{
					// Unofficial
					Send.SystemMessage(creature, Localization.Get("You don't the necessary items."));
					return false;
				}

				// Get chances
				// All unofficial
				var rank = (skill == null ? 16 : (int)skill.Info.Rank);
				var enchantChance = (skill == null ? 0 : skill.RankData.Var3);

				// Campfire r8+ bonus
				if (enchantChance > 0 && campfire.Temp.CampfireSkillRank.Rank >= SkillRank.R8)
					enchantChance += (16 - rank);

				// Powder = double enchant chance, based on the Wiki saying
				// r1 doesn't guarantee getting the enchants, but it does
				// guarantee getting powder.
				var powderChance = enchantChance * 2;

				// Debug
				if (creature.Titles.SelectedTitle == TitleId.devCAT)
					Send.ServerMessage(creature, "Debug: Chance for enchant: {0:0}, chance for powder: {1:0}", enchantChance, powderChance);

				// Try prefix
				if (item.OptionInfo.Prefix != 0 && rnd.Next(100) < enchantChance)
				{
					var enchant = Item.CreateEnchant(item.OptionInfo.Prefix);
					creature.AcquireItem(enchant);
					enchantBurnSuccess = true;
				}

				// Try suffix
				if (item.OptionInfo.Suffix != 0 && rnd.Next(100) < enchantChance)
				{
					var enchant = Item.CreateEnchant(item.OptionInfo.Suffix);
					creature.AcquireItem(enchant);
					enchantBurnSuccess = true;
				}

				// Try suffix
				if (item.OptionInfo.Prefix + item.OptionInfo.Suffix != 0 && rnd.Next(100) < powderChance)
				{
					var powder = new Item(62003); // Blessed Magic Powder
					creature.AcquireItem(powder);
					powderBurnSuccess = true;
				}

				// Reduce items
				creature.Inventory.Remove(51102, 1); // Mana Herb
				creature.Inventory.Remove(63016, 1); // Holy Water

				// Training
				this.BurnTraining(skill, enchantBurnSuccess, powderBurnSuccess);

				// Success/Fail motion
				Send.UseMotion(creature, 14, enchantBurnSuccess ? 0 : 3);
			}

			// Seal Scroll (G1 Glas fight, drop from Gargoyles)
			if (item.HasTag("/evilscroll/55/"))
			{
				creature.Conditions.Activate(ConditionsA.Blessed, null, 60 * 1000);
				Send.Notice(creature, Localization.Get("I feel the blessing of the Goddess."));
			}
			// Other items
			else
			{
				// Add exp based on item buying price (random+unofficial)
				if (item.OptionInfo.Price > 0)
				{
					exp = 40 + (int)(item.OptionInfo.Price / (float)item.Data.StackMax / 100f * item.Info.Amount);
					creature.GiveExp(exp);
				}

				Send.Notice(creature, NoticeType.MiddleSystem, Localization.Get("Burning EXP {0}"), exp);
			}

			// Remove item from cursor
			creature.Inventory.Remove(item);

			Send.Effect(MabiId.Broadcast, creature, Effect.BurnItem, campfire.EntityId, enchantBurnSuccess);

			return true;
		}

		/// <summary>
		/// Returns random durability loss for rank. Returns -1 if item should
		/// be destroyed.
		/// </summary>
		/// <remarks>
		/// Reference: http://wiki.mabinogiworld.com/view/Enchant#Enchanting
		/// </remarks>
		/// <param name="rnd"></param>
		/// <param name="enchantRank"></param>
		/// <returns></returns>
		private int GetDurabilityLoss(Random rnd, SkillRank enchantRank, EnchantResult result)
		{
			var points = 0;

			// Destroy item if safe enchanting is disabled and rank is over 6.
			if (!ChannelServer.Instance.Conf.World.SafeEnchanting && enchantRank >= SkillRank.R6)
				return -1;

			switch (enchantRank)
			{
				// A huge fail results in a bigger durability loss.
				case SkillRank.RF: points = (result == EnchantResult.Fail ? rnd.Next(0, 1) : rnd.Next(0, 2)); break;
				case SkillRank.RE: points = (result == EnchantResult.Fail ? rnd.Next(0, 2) : rnd.Next(0, 4)); break;
				case SkillRank.RD: points = (result == EnchantResult.Fail ? rnd.Next(0, 2) : rnd.Next(0, 4)); break;
				case SkillRank.RC: points = (result == EnchantResult.Fail ? rnd.Next(0, 3) : rnd.Next(0, 6)); break;
				case SkillRank.RB: points = (result == EnchantResult.Fail ? rnd.Next(0, 3) : rnd.Next(0, 7)); break;
				case SkillRank.RA: points = (result == EnchantResult.Fail ? rnd.Next(0, 4) : rnd.Next(0, 8)); break;
				case SkillRank.R9: points = (result == EnchantResult.Fail ? rnd.Next(1, 6) : rnd.Next(1, 10)); break;
				case SkillRank.R8: points = (result == EnchantResult.Fail ? rnd.Next(2, 7) : rnd.Next(2, 12)); break;
				case SkillRank.R7: points = (result == EnchantResult.Fail ? rnd.Next(2, 8) : rnd.Next(2, 14)); break;

				// Custom durability loss for R6+, for safe enchanting option.
				case SkillRank.R6: points = (result == EnchantResult.Fail ? rnd.Next(3, 9) : rnd.Next(3, 16)); break;
				case SkillRank.R5: points = (result == EnchantResult.Fail ? rnd.Next(3, 9) : rnd.Next(3, 16)); break;
				case SkillRank.R4: points = (result == EnchantResult.Fail ? rnd.Next(4, 10) : rnd.Next(4, 18)); break;
				case SkillRank.R3: points = (result == EnchantResult.Fail ? rnd.Next(4, 10) : rnd.Next(4, 18)); break;
				case SkillRank.R2: points = (result == EnchantResult.Fail ? rnd.Next(5, 11) : rnd.Next(5, 20)); break;
				case SkillRank.R1: points = (result == EnchantResult.Fail ? rnd.Next(5, 11) : rnd.Next(5, 20)); break;
			}

			return points * 1000;
		}

		/// <summary>
		/// Returns option set id from "enchant scrolls", based on their data.
		/// </summary>
		/// <param name="enchant"></param>
		/// <returns></returns>
		private int GetOptionSetid(Item enchant)
		{
			var optionSetId = 0;

			// Elementals
			if (enchant.HasTag("/elemental/"))
			{
				optionSetId = enchant.MetaData1.GetInt("ENELEM");
			}
			// Enchants
			else if (enchant.MetaData1.Has("ENPFIX") || enchant.MetaData1.Has("ENSFIX"))
			{
				var prefixId = enchant.MetaData1.GetInt("ENPFIX");
				var suffixId = enchant.MetaData1.GetInt("ENSFIX");

				if (prefixId != 0)
					optionSetId = prefixId;
				else if (suffixId != 0)
					optionSetId = suffixId;
			}
			// Fallback? (Pages)
			else
			{
				var prefixId = enchant.OptionInfo.Prefix;
				var suffixId = enchant.OptionInfo.Suffix;

				if (prefixId != 0)
					optionSetId = prefixId;
				else if (suffixId != 0)
					optionSetId = suffixId;
			}

			return optionSetId;
		}

		/// <summary>
		/// Handles skill training from enchanting.
		/// </summary>
		/// <param name="skill"></param>
		/// <param name="result"></param>
		private void Training(Skill skill, EnchantResult result)
		{
			// Novice and r8 have no relevant training
			if (skill.Info.Rank < SkillRank.RF || skill.Info.Rank > SkillRank.R1 || skill.Info.Rank == SkillRank.R8)
				return;

			switch (result)
			{
				case EnchantResult.Success:
					skill.Train(1); // Get a success.
					return;

				case EnchantResult.HugeSuccess:
					skill.Train(2); // Get a great success.
					goto case EnchantResult.Success;

				case EnchantResult.Fail:
					if (skill.Info.Rank <= SkillRank.R6)
						skill.Train(3); // Get a failure.
					return;

				case EnchantResult.HugeFail:
					if (skill.Info.Rank <= SkillRank.RE)
						skill.Train(4); // Get a horrible result.
					goto case EnchantResult.Fail;
			}
		}

		/// <summary>
		/// Handles skill training from burning.
		/// </summary>
		/// <param name="skill"></param>
		/// <param name="enchantSuccess"></param>
		/// <param name="powderSuccess"></param>
		private void BurnTraining(Skill skill, bool enchantSuccess, bool powderSuccess)
		{
			if (skill == null || skill.Info.Rank >= SkillRank.R7)
				return;

			if (skill.Info.Rank >= SkillRank.RF && skill.Info.Rank <= SkillRank.RE)
			{
				if (enchantSuccess) skill.Train(5); // Get an Enchant Scroll from enchant burning.
				if (powderSuccess) skill.Train(6); // Get a Magic Powder from enchant burning.
			}
			else if (skill.Info.Rank >= SkillRank.RD && skill.Info.Rank <= SkillRank.R9)
			{
				if (enchantSuccess) skill.Train(4); // Get an Enchant Scroll from enchant burning.
				if (powderSuccess) skill.Train(5); // Get a Magic Powder from enchant burning.
			}
			else if (skill.Info.Rank == SkillRank.R8)
			{
				if (enchantSuccess) skill.Train(1); // Get an Enchant Scroll from enchant burning.
				if (powderSuccess) skill.Train(2); // Get a Magic Powder from enchant burning.
			}
		}
	}
}
