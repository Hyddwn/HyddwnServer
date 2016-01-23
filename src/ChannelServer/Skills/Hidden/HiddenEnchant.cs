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

namespace Aura.Channel.Skills.Hidden
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
	[Skill(SkillId.HiddenEnchant, SkillId.Enchant)]
	public class HiddenEnchant : IPreparable, ICompletable
	{
		private float[] _baseChanceB00 = { 69, 65, 60, 55, 51, 46, 32, 30, 27, 25, 20, 14, 10, 6, 4 };
		private float[] _baseChanceB05 = { 73, 68, 63, 58, 53, 48, 34, 32, 29, 26, 21, 15, 10, 6, 4 };
		private float[] _baseChanceB10 = { 77, 71, 66, 61, 56, 51, 35, 33, 30, 27, 22, 16, 11, 7, 5 };
		private float[] _baseChanceB50 = { 90, 90, 90, 85, 78, 71, 50, 47, 42, 38, 31, 22, 15, 10, 7 };
		private float[] _baseChanceB60 = { 90, 90, 90, 90, 84, 76, 53, 50, 45, 41, 33, 24, 16, 10, 7 };

		/// <summary>
		/// Prepares skill.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		/// <returns></returns>
		public bool Prepare(Creature creature, Skill skill, Packet packet)
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
					Log.Warning("HiddenEnchant.Prepare: Creature '{0:X16}' tried to use Enchant without powder.");
					return false;
				}

				if (magazine == null || !magazine.HasTag("/lefthand/enchant/"))
				{
					Log.Warning("HiddenEnchant.Prepare: Creature '{0:X16}' tried to use Enchant without enchant.");
					return false;
				}
			}

			// Get items
			var item = creature.Inventory.GetItem(itemEntityId);
			var enchant = creature.Inventory.GetItem(enchantEntityId);

			// Check item
			if (item == null)
			{
				Log.Warning("HiddenEnchant.Prepare: Creature '{0:X16}' tried to enchant non-existing item.");
				return false;
			}

			// Check enchant
			if (enchant == null)
			{
				Log.Warning("HiddenEnchant.Prepare: Creature '{0:X16}' tried to enchant with non-existing enchant item.");
				return false;
			}

			if (!enchant.HasTag("/enchant/"))
			{
				Log.Warning("HiddenEnchant.Prepare: Creature '{0:X16}' tried to enchant with invalid enchant scroll.");
				return false;
			}

			if (enchant.Durability == 0)
			{
				Send.Notice(creature, Localization.Get("This scroll is no longer valid for enchantment."));
				return false;
			}

			// Check ranks
			bool prefix, suffix;
			var optionSetId = this.GetOptionSetid(enchant, out prefix, out suffix);
			var optionSetData = AuraData.OptionSetDb.Find(optionSetId);
			if (optionSetData == null)
			{
				Log.Warning("HiddenEnchant.Prepare: Creature '{0:X16}' tried to enchant with unknown option set '{0}'.", optionSetId);
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

			var item = creature.Temp.SkillItem1;
			var enchant = creature.Temp.SkillItem2;
			var rightHand = creature.RightHand;
			var rnd = RandomProvider.Get();

			var optionSetId = 0;
			var prefix = false;
			var suffix = false;

			creature.Temp.SkillItem1 = null;
			creature.Temp.SkillItem2 = null;

			// Get option set id
			optionSetId = this.GetOptionSetid(enchant, out prefix, out suffix);

			// Get and apply option set
			var optionSetData = AuraData.OptionSetDb.Find(optionSetId);
			if (optionSetData == null)
			{
				Log.Error("HiddenEnchant.Complete: Unknown option set '{0}'.", optionSetId);
				goto L_End;
			}

			// Check target
			if (!item.HasTag(optionSetData.Allow) || item.HasTag(optionSetData.Disallow))
			{
				Log.Warning("HiddenEnchant.Complete: Creature '{0:X16}' tried to use set '{0}' on invalid item '{1}'.", optionSetData.Id, item.Info.Id);
				goto L_End;
			}

			// Check success
			var success = optionSetData.AlwaysSuccess;
			if (!success)
			{
				var num = rnd.Next(100);
				var chance = this.GetChance(creature, rightHand, skill, optionSetData);
				success = num < chance;
			}

			// Handle result
			var result = EnchantResult.Fail;
			var destroy = true;
			if (success)
			{
				item.ApplyOptionSet(optionSetData, true);
				if (prefix) item.OptionInfo.Prefix = (ushort)optionSetId;
				if (suffix) item.OptionInfo.Suffix = (ushort)optionSetId;

				result = EnchantResult.Success;
			}
			else
			{
				// Don't default destroy enchant when using Enchant
				if (skill.Info.Id == SkillId.Enchant)
					destroy = false;

				// Random item durability loss, based on rank.
				var durabilityLoss = this.GetDurabilityLoss(rnd, optionSetData.Rank);
				if (durabilityLoss == -1)
					creature.Inventory.Remove(item);
				else if (durabilityLoss != 0)
					creature.Inventory.ReduceMaxDurability(item, durabilityLoss);
			}

			// Destroy or decrement items
			if (skill.Info.Id == SkillId.Enchant && rightHand != null)
				creature.Inventory.Decrement(rightHand);

			if (destroy)
				creature.Inventory.Decrement(enchant);
			else
				creature.Inventory.ReduceDurability(enchant, (int)skill.RankData.Var1 * 100);

			// Response
			Send.Effect(creature, Effect.Enchant, (byte)result);
			if (result == EnchantResult.Success)
			{
				Send.ItemUpdate(creature, item);
				Send.AcquireEnchantedItemInfo(creature, item.EntityId, item.Info.Id, optionSetId);
			}

		L_End:
			Send.Echo(creature, packet);
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
		private float GetChance(Creature creature, Item rightHand, Skill skill, OptionSetData optionSetData)
		{
			// Check right hand, only use it if it's powder
			if (rightHand != null && !rightHand.HasTag("/enchant/powder/"))
				rightHand = null;

			// Get base chance, based on skill and powder
			var baseChance = _baseChanceB00; // (Blessed) Magic Powder/None
			if (skill.Info.Id == SkillId.Enchant && rightHand != null)
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
			if (skill.Info.Id == SkillId.Enchant && rightHand != null)
				intBonus = 1f + ((creature.Int - 35f) / 350f);

			// Thursday bonus
			if (ErinnTime.Now.Day == 4)
				thursdayBonus = Math.Max(0, (15 - rank) / 2f);

			// Result
			var result = Math2.Clamp(0, 90, chance * intBonus + thursdayBonus);

			// Debug
			if (creature.Titles.SelectedTitle == TitleId.devCAT)
			{
				Send.ServerMessage(creature,
					"Debug: Enchant success chance: {0} (base: {1}, int: {2}, thu: {3})",
					result.ToInvariant("0"),
					chance.ToInvariant("0"),
					(chance / 1f * (intBonus - 1f)).ToInvariant("0"),
					thursdayBonus.ToInvariant("0"));
			}

			return result;
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
		private int GetDurabilityLoss(Random rnd, SkillRank enchantRank)
		{
			var points = 0;

			switch (enchantRank)
			{
				case SkillRank.Novice: return 0;
				case SkillRank.RF: points = rnd.Next(0, 1); break;
				case SkillRank.RE: points = rnd.Next(0, 2); break;
				case SkillRank.RD: points = rnd.Next(0, 2); break;
				case SkillRank.RC: points = rnd.Next(0, 3); break;
				case SkillRank.RB: points = rnd.Next(0, 3); break;
				case SkillRank.RA: points = rnd.Next(0, 4); break;
				case SkillRank.R9: points = rnd.Next(1, 6); break;
				case SkillRank.R8: points = rnd.Next(2, 7); break;
				case SkillRank.R7: points = rnd.Next(2, 8); break;
				default: return -1;
			}

			return points * 1000;
		}

		private int GetOptionSetid(Item enchant, out bool prefix, out bool suffix)
		{
			var optionSetId = 0;
			prefix = false;
			suffix = false;

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
				{
					optionSetId = prefixId;
					prefix = true;
				}
				else if (suffixId != 0)
				{
					optionSetId = suffixId;
					suffix = true;
				}
			}
			// Fallback? (Pages)
			else
			{
				var prefixId = enchant.OptionInfo.Prefix;
				var suffixId = enchant.OptionInfo.Suffix;

				if (prefixId != 0)
				{
					optionSetId = prefixId;
					prefix = true;
				}
				else if (suffixId != 0)
				{
					optionSetId = suffixId;
					suffix = true;
				}
			}

			return optionSetId;
		}
	}
}
