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
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Aura.Channel.Skills.Base
{
	/// <summary>
	/// Base class for production skills, like Handicraft and Weaving.
	/// </summary>
	/// <remarks>
	/// Starting production calls Prepare, once the creation process is done,
	/// Complete is called. There is no way to cancel the skill once Prepare
	/// was called.
	/// 
	/// While the client tells us how many items are gonna be produced,
	/// it Prepares the skill again and again, so we must only create
	/// one product at a time.
	/// </remarks>
	public abstract class ProductionSkill : IPreparable, ICompletable
	{
		/// <summary>
		/// Proficiency gained by tool.
		/// </summary>
		protected virtual int Proficiency { get { return 30; } }

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
		/// 
		/// 001 [............271B] Short  : 10011
		/// 002 [..............01] Byte   : 1
		/// 003 [00A188D000050041] Long   : 45467898185318465
		/// 004 [........00000000] Int    : 0
		/// 005 [........00000002] Int    : 2
		/// 006 [............0001] Short  : 1
		/// 007 [............0001] Short  : 1
		/// 008 [............0002] Short  : 2
		/// 009 [..............01] Byte   : 1
		/// 010 [005000CC80BA58CD] Long   : 22518876469876941
		/// 011 [............000A] Short  : 10
		/// </example>
		public bool Prepare(Creature creature, Skill skill, Packet packet)
		{
			var unkByte = packet.GetByte();
			var propEntityId = 0L;
			var unkInt = 0;
			if (packet.Peek() == PacketElementType.Long) // Rule unknown
			{
				propEntityId = packet.GetLong();
				unkInt = packet.GetInt();
			}
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
					Log.Warning("ProductionSkill.Prepare: Creature '{0:X16}' tried to use non-existent item as material.", creature.EntityId);
					return false;
				}

				materials.Add(new ProductionMaterial(item, amount));
			}

			// Get product data
			var potentialProducts = AuraData.ProductionDb.Find(category, productId);
			if (potentialProducts.Length == 0)
			{
				Send.ServerMessage(creature, "Unknown product.");
				return false;
			}
			var productData = potentialProducts[0];

			// Check tools
			if (!this.CheckTools(creature, skill, productData))
				return false;

			// Check prop
			if (!this.CheckProp(creature, propEntityId))
				return false;

			// Check mana
			if (!this.CheckMana(creature, productData))
				return false;

			// Give skills the ability to use motions and other things.
			this.OnUse(creature, skill);

			// Response
			Send.Echo(creature, Op.SkillUse, packet);
			skill.State = SkillState.Used;

			return true;
		}

		/// <summary>
		/// Called from successful Prepare.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		protected virtual void OnUse(Creature creature, Skill skill)
		{
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
			var propEntityId = 0L;
			var unkInt = 0;
			if (packet.Peek() == PacketElementType.Long) // Rule unknown
			{
				propEntityId = packet.GetLong();
				unkInt = packet.GetInt();
			}
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
					Log.Warning("ProductionSkill.Prepare: Creature '{0:X16}' tried to use non-existent item as material.", creature.EntityId);
					return;
				}

				materials.Add(new ProductionMaterial(item, amount));
			}

			// Check prop
			if (!this.CheckProp(creature, propEntityId))
				goto L_Fail;

			// Check category
			if (!this.CheckCategory(creature, category))
			{
				Log.Warning("ProductionSkill.Complete: Creature '{0:X16}' tried to use category '{1}' with skill '{2}'.", creature.EntityId, category, this.GetType().Name);
				goto L_Fail;
			}

			// Get potential products
			// Some productions can produce items of varying quality (cheap,
			// common, fine, finest)
			var potentialProducts = AuraData.ProductionDb.Find(category, productId);
			if (potentialProducts.Length == 0)
			{
				Send.ServerMessage(creature, "Unknown product.");
				goto L_Fail;
			}

			// Get reference product for checks and mats
			var productData = potentialProducts[0];

			// Check tools
			if (!this.CheckTools(creature, skill, productData))
				goto L_Fail;

			// Check mana
			if (!this.CheckMana(creature, productData))
				goto L_Fail;

			if (productData.Mana > 0)
			{
				creature.Mana -= productData.Mana;
				Send.StatUpdate(creature, StatUpdateType.Private, Stat.Mana);
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
					// Check item and stack item for tag, pouches can be put
					// into the window, reducing the contained items.
					var match =
						material.Item.HasTag(reqMat.Tag) ||
						(material.Item.IsGatheringPouch && material.Item.Data.StackItem != null && material.Item.Data.StackItem.HasTag(reqMat.Tag));

					// Satisfy requirement with item, up to the max amount
					// needed or available
					if (match)
					{
						// Cancel if one item matches multiple materials.
						// It's unknown how this would be handled, can it even
						// happen? Can one item maybe only be used as one material?
						if (inUse.Contains(material.Item.EntityId))
						{
							Send.ServerMessage(creature, Localization.Get("Unable to handle request, please report, with this information: ({0}/{1})."), material.Item.Info.Id, productData.Id);
							Log.Warning("ProductionSkill.Complete: Item '{0}' matches multiple materials for product '{1}'.", material.Item.Info.Id, productData.Id);
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

			// Adjust success rates
			// Some weaving gloves increase the chance for a certain item
			// to be product.
			var rank = skill.Info.Rank <= SkillRank.R1 ? skill.Info.Rank : SkillRank.R1;
			var successRates = new int[potentialProducts.Length];
			var successRatesTotal = 0;
			var gloves = creature.Inventory.GetItemAt(Pocket.Glove, 0, 0);
			for (int i = 0; i < potentialProducts.Length; ++i)
			{
				var multiplier = 1f;
				if (gloves != null && gloves.Data.ProductionBonus.Matches(skill.Info.Id, potentialProducts[i].ItemId))
					multiplier = gloves.Data.ProductionBonus.Rate;

				// The official formula is unknown, I'm gonna assume the rate
				// is a multiplier, since a bonus of 5% wouldn't mean much.
				// 500% *might* seem excessive, but half of that, 250%, would
				// only put the beneficiary at about the same rate as cheap
				// and common.
				var rate = potentialProducts[i].SuccessRates[rank];
				successRates[i] = (int)(rate * multiplier);
				successRatesTotal += successRates[i];
			}

			// Check success
			var rainBonus = productData.RainBonus;
			var chance = creature.GetProductionSuccessChance(skill, category, successRatesTotal, rainBonus);
			var rnd = RandomProvider.Get();
			var success = (rnd.Next(100) < chance);

			// Debug
			if (creature.IsDev)
				Send.ServerMessage(creature, "Debug: Chance {0}%", chance);

			// Select random product
			// Do this here, so we have the data for skill training,
			// no matter the outcome.
			if (potentialProducts.Length > 1)
			{
				var itemId = 0;
				var num = rnd.NextDouble() * successRatesTotal;
				var n = 0.0;

				for (int i = 0; i < potentialProducts.Length; ++i)
				{
					n += successRates[i];
					if (num <= n)
					{
						itemId = potentialProducts[i].ItemId;
						productData = potentialProducts[i];
						break;
					}
				}

				// Sanity check
				if (itemId == 0)
				{
					Log.Error("ProductionSkill.Complete: Failed to select random product item for {0}/{1}, num: {2}.", category, productId, num);
					Send.ServerMessage(creature, "Failed to generate product.");
					goto L_Fail;
				}
			}

			// Update tool's durability and proficiency
			this.UpdateTool(creature, productData);

			// Skill training
			this.SkillTraining(creature, skill, productData, success);

			// Event
			ChannelServer.Instance.Events.OnCreatureFinishedProductionOrCollection(creature, success);

			// Reduce mats
			foreach (var material in toReduce)
			{
				// On fail of non-queued productions you lose 1~amount of
				// materials randomly
				var reduce = success ? material.Amount : rnd.Next(1, material.Amount + 1);
				if (reduce > 0)
					creature.Inventory.Decrement(material.Item, (ushort)reduce);
			}

			if (success)
			{
				// Check item
				var productItemData = AuraData.ItemDb.Find(productData.ItemId);
				if (productItemData == null)
				{
					Log.Error("ProductionSkill.Complete: Unknown product item '{0}'.", productData.ItemId);
					Send.ServerMessage(creature, "Unknown product item.");
					goto L_Fail;
				}

				// Create product
				var productItem = new Item(productData.ItemId);
				productItem.Amount = productData.Amount;

				// Add product to inventory
				creature.Inventory.Insert(productItem, true);

				// Material creation event
				ChannelServer.Instance.Events.OnCreatureProducedItem(new ProductionEventArgs(creature, productData, true, productItem));

				// Success
				Send.UseMotion(creature, 14, 0); // Success motion
				Send.Notice(creature, Localization.Get("{0} created successfully!"), productItem.Data.Name);
				Send.Echo(creature, Op.SkillComplete, packet);

				return;
			}

			// Material creation event
			ChannelServer.Instance.Events.OnCreatureProducedItem(new ProductionEventArgs(creature, productData, false, null));

		L_Fail:
			// Unofficial
			Send.UseMotion(creature, 14, 3); // Fail motion
			Send.Echo(creature, Op.SkillComplete, packet);
		}

		/// <summary>
		/// Updates tool's durability and proficiency.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="productData"></param>
		protected virtual void UpdateTool(Creature creature, ProductionData productData)
		{
			if (productData.Tool == null || productData.Tool == "/barehand/")
				return;

			creature.Inventory.ReduceDurability(creature.RightHand, productData.Durability);
			creature.Inventory.AddProficiency(creature.RightHand, Proficiency);
		}

		/// <summary>
		/// Checks tools from Prepare, to maybe cancel skill.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="productData"></param>
		/// <returns></returns>
		protected virtual bool CheckTools(Creature creature, Skill skill, ProductionData productData)
		{
			// Sanity checks, the client should be handling this.

			// If null, anything can be equipped
			if (productData.Tool == null)
				return true;

			if (productData.Tool == "/barehand/")
			{
				// Fail if bare hand required but an item is equipped
				if (creature.RightHand != null)
				{
					Log.Warning("ProductionSkill.Complete: User '{0}' tried to produce without empty hands.", creature.Client.Account.Id);
					return false;
				}
			}
			else
			{
				// Fail if no item or the wrong one is equipped
				if (creature.RightHand == null || !creature.RightHand.HasTag(productData.Tool))
				{
					Log.Warning("ProductionSkill.Complete: Creature '{0:X16}' tried to produce without the appropriate tool.", creature.EntityId);
					return false;
				}
			}

			// TODO: Check durability? What happens if tool is unusable?

			return true;
		}

		/// <summary>
		/// Checks if category can be handled by this skill, returns false if not.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="category"></param>
		/// <returns></returns>
		protected abstract bool CheckCategory(Creature creature, ProductionCategory category);

		/// <summary>
		/// Checks if prop is valid and in range, returns false if not.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="propId"></param>
		/// <returns></returns>
		protected virtual bool CheckProp(Creature creature, long propEntityId)
		{
			return true;
		}

		/// <summary>
		/// Checks if creature has enough mana to produce product,
		/// returns false if not. Handles notices.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="productData"></param>
		/// <returns></returns>
		private bool CheckMana(Creature creature, ProductionData productData)
		{
			// Sanity check, client checks this as well.
			if (creature.Mana < productData.Mana)
			{
				Send.Notice(creature, Localization.Get("You do not have enough MP to make that many at once."));
				return false;
			}

			return true;
		}

		/// <summary>
		/// Handles skill training.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		/// <param name="data"></param>
		/// <param name="success"></param>
		protected abstract void SkillTraining(Creature creature, Skill skill, ProductionData data, bool success);
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
