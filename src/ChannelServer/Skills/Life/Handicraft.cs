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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.Skills.Life
{
	/// <summary>
	/// Handles Handicraft production skill.
	/// </summary>
	/// <remarks>
	/// Starting production calls Prepare, once the creation process is done,
	/// Complete is called. There is no way to cancel the skill once Prepare
	/// was called.
	/// 
	/// While the client tells us how many items are gonna be produced,
	/// it Prepares the skill again and again, so we must only create
	/// one product at a time.
	/// 
	/// Var20: Success Rate?
	/// </remarks>
	[Skill(SkillId.Handicraft)]
	public class Handicraft : IPreparable, ICompletable
	{
		/// <summary>
		/// Proficiency gained by tool.
		/// </summary>
		private const int Proficiency = 30;

		/// <summary>
		/// Starts production, finished in Complete.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		/// <returns></returns>
		/// <example>
		/// 001 [............271D] Short  : 10013
		/// 002 [..............02] Byte   : 2
		/// 003 [........00000006] Int    : 6
		/// 004 [............0001] Short  : 1
		/// 005 [............0006] Short  : 6
		/// 006 [............0001] Short  : 1
		/// 007 [..............01] Byte   : 1
		/// 008 [005000CC7F17E280] Long   : 22518876442452608
		/// 009 [............0003] Short  : 3
		/// </example>
		public bool Prepare(Creature creature, Skill skill, Packet packet)
		{
			if (creature.RightHand == null || !creature.RightHand.HasTag("/handicraft_kit/"))
			{
				Send.MsgBox(creature, Localization.Get("You're going to need a Handicraft Kit for that."));
				return false;
			}

			var unkByte1 = packet.GetByte();
			var productId = packet.GetInt();
			var unkShort1 = packet.GetShort();
			var category = (ProductionCategory)packet.GetShort();
			var amountToProduce = packet.GetShort();
			var count = packet.GetByte();
			var materials = new List<ProductionMaterial>(count);
			for (int i = 0; i < count; ++i)
			{
				var entityId = packet.GetLong();
				var amount = packet.GetShort();

				// Check item
				var item = creature.Inventory.GetItem(entityId);
				if (item == null)
				{
					Log.Warning("Handicraft.Prepare: Creature '{0:X16}' tried to use non-existent item as material.", creature.EntityId);
					return false;
				}

				materials.Add(new ProductionMaterial(item, amount));
			}

			Send.UseMotion(creature, 11, 3); // Handicraft motion
			Send.SkillUse(creature, skill.Info.Id, unkByte1, productId, unkShort1, category, amountToProduce, materials);
			skill.State = SkillState.Used;

			return true;
		}

		/// <summary>
		/// Completes production.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="packet"></param>
		public void Complete(Creature creature, Skill skill, Packet packet)
		{
			var unkByte = packet.GetByte();
			var productId = packet.GetInt();
			var unkShort = packet.GetShort();
			var category = (ProductionCategory)packet.GetShort();
			var amountToProduce = packet.GetShort();
			var count = packet.GetByte();
			var materials = new List<ProductionMaterial>(count);
			for (int i = 0; i < count; ++i)
			{
				var entityId = packet.GetLong();
				var amount = packet.GetShort();

				// Check item
				var item = creature.Inventory.GetItem(entityId);
				if (item == null)
				{
					Log.Warning("Handicraft.Prepare: Creature '{0:X16}' tried to use non-existent item as material.", creature.EntityId);
					return;
				}

				materials.Add(new ProductionMaterial(item, amount));
			}

			// Check product
			var productData = AuraData.ProductionDb.Find(category, productId);
			if (productData == null)
			{
				Send.ServerMessage(creature, "Unknown product.");
				goto L_Fail;
			}

			var productItemData = AuraData.ItemDb.Find(productData.ItemId);
			if (productItemData == null)
			{
				Send.ServerMessage(creature, "Unknown product item.");
				goto L_Fail;
			}

			// Check tool
			// TODO: Check durability? What happens if tool is unusable?
			if (creature.RightHand == null || !creature.RightHand.HasTag(productData.Tool))
			{
				Log.Warning("Handicraft.Complete: Creature '{0:X16}' tried to produce without the appropriate tool.", creature.EntityId);
				goto L_Fail;
			}

			// Check materials
			var requiredMaterials = productData.GetMaterialList();
			var toReduce = new List<ProductionMaterial>();
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
							Send.ServerMessage(creature, Localization.Get("Unable to handle request, please report, with this information: ({0}/{1})."), material.Item.Info.Id, productData.Id);
							Log.Warning("Handicraft.Complete: Item '{0}' matches multiple materials for product '{1}'.", material.Item.Info.Id, productData.Id);
							goto L_Fail;
						}

						var reduce = Math.Min(reqMat.Amount, material.Item.Amount);
						reqMat.Amount -= reduce;
						toReduce.Add(new ProductionMaterial(material.Item, reduce));
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
				goto L_Fail;
			}

			// Check success
			var rank = skill.Info.Rank <= SkillRank.R1 ? skill.Info.Rank : SkillRank.R1;
			var chance = creature.GetProductionSuccessChance(productData.SuccessRates[rank], productData.RainBonus);
			var rnd = RandomProvider.Get();
			var success = (rnd.Next(100) < chance);

			// Update tool's durability and proficiency
			creature.Inventory.ReduceDurability(creature.RightHand, productData.Durability);
			creature.Inventory.AddProficiency(creature.RightHand, Proficiency);

			// Skill training
			this.SkillTraining(creature, skill, productData, success);

			// Reduce mats
			foreach (var material in toReduce)
				creature.Inventory.Decrement(material.Item, (ushort)material.Amount);

			if (success)
			{
				// Create product
				var productItem = new Item(productData.ItemId);
				productItem.Amount = productData.Amount;
				creature.Inventory.Insert(productItem, true);

				// Success
				Send.UseMotion(creature, 14, 0); // Success motion
				Send.Notice(creature, Localization.Get("{0} created successfully!"), productItemData.Name);
				Send.SkillComplete(creature, skill.Info.Id, unkByte, productId, unkShort, category, amountToProduce, materials);

				return;
			}

		L_Fail:
			// Unofficial
			Send.UseMotion(creature, 14, 3); // Fail motion
			Send.SkillComplete(creature, skill.Info.Id, unkByte, productId, unkShort, category, amountToProduce, materials);
		}

		/// <summary>
		/// Handles skill training.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="data"></param>
		/// <param name="success"></param>
		private void SkillTraining(Creature creature, Skill skill, ProductionData data, bool success)
		{
			if (skill.Info.Rank == SkillRank.RF)
			{
				if (success)
				{
					if (data.Rank >= SkillRank.RD)
						skill.Train(1); // Make a rank D or higher item.
					else
						skill.Train(2); // Make a rank F or E item.
				}
				else
				{
					if (data.Rank >= SkillRank.RD)
						skill.Train(3); // Fail at making a rank D or higher item.
					else
						skill.Train(4); // Fail at making a rank F or E item.
				}

				return;
			}

			if (skill.Info.Rank == SkillRank.RE)
			{
				if (success)
				{
					if (data.Rank >= SkillRank.RC)
						skill.Train(1); // Make a rank C or higher item.
					else
						skill.Train(2); // Make a rank F, E, or D item. 
				}
				else
				{
					if (data.Rank >= SkillRank.RC)
						skill.Train(3); // Fail at making a rank C or higher item.
					else
						skill.Train(4); // Fail at making a rank F, E, or D item.
				}

				return;
			}

			if (skill.Info.Rank >= SkillRank.RD && skill.Info.Rank <= SkillRank.R3)
			{
				var high = skill.Info.Rank + 2;
				var mid = high - 1;
				var low = mid - 2;

				if (success)
				{
					if (data.Rank >= high)
						skill.Train(1); // Make a rank X or higher item.
					else if (data.Rank >= low && data.Rank <= mid)
						skill.Train(2); // Make a rank X, X, or X item.
					else
						skill.Train(3); // Make a rank X (or lower) item.
				}
				else
				{
					if (data.Rank >= high)
						skill.Train(4); // Fail at making a rank X or higher item.
					else if (data.Rank >= low && data.Rank <= mid)
						skill.Train(5); // Fail at making a rank X, X, or X item.
					else
						skill.Train(6); // Fail at making a rank X (or lower) item.
				}

				return;
			}

			if (skill.Info.Rank == SkillRank.R2)
			{
				if (success)
				{
					if (data.Rank >= SkillRank.R3)
						skill.Train(1); // Make a rank 3, 2, or 1 item.
					else
						skill.Train(2); // Make a rank 4 or lower item.
				}
				else
				{
					if (data.Rank >= SkillRank.R3)
						skill.Train(3); // Fail at making a rank 2, or 1 item.
					else
						skill.Train(4); // Fail at making a rank 3 or lower item.
				}

				return;
			}

			if (skill.Info.Rank == SkillRank.R1)
			{
				if (success)
				{
					if (data.Rank >= SkillRank.R2)
						skill.Train(1); // Make a rank 3, 2, or 1 item.
					else
						skill.Train(2); // Make a rank 4 or lower item.
				}
				else
				{
					if (data.Rank >= SkillRank.R2)
						skill.Train(3); // Fail at making a rank 2, or 1 item.
					else
						skill.Train(4); // Fail at making a rank 3 or lower item.
				}

				return;
			}
		}
	}

	public class ProductionMaterial
	{
		public Item Item { get; private set; }
		public int Amount { get; private set; }

		public ProductionMaterial(Item item, int amount)
		{
			this.Item = item;
			this.Amount = amount;
		}
	}
}
