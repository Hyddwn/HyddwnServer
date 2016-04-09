// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see licence.txt in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using Aura.Channel.Network.Sending;
using Aura.Channel.Util;
using Aura.Channel.World.Entities;
using Aura.Data.Database;
using Aura.Mabi.Const;
using Aura.Shared.Util;
using Aura.Shared.Network;
using Aura.Channel.World.Entities.Creatures;
using Aura.Mabi.Structs;
using Aura.Mabi;
using Aura.Channel.Skills;
using Aura.Channel.World.Quests;

namespace Aura.Channel.World.Inventory
{
	/// <summary>
	/// Inventory for players
	/// </summary>
	public class CreatureInventory
	{
		/// <summary>
		/// Default inventory width.
		/// </summary>
		/// <remarks>
		/// Equal to a player's normal inventory width.
		/// </remarks>
		private const int DefaultWidth = 6;

		/// <summary>
		/// Default inventory height.
		/// </summary>
		/// <remarks>
		/// Equal to a player's normal inventory height.
		/// </remarks>
		private const int DefaultHeight = 10;

		/// <summary>
		/// Maximum inventory width.
		/// </summary>
		/// <remarks>
		/// Maximum width supported by the client.
		/// </remarks>
		private const int MaxWidth = 32;

		/// <summary>
		/// Maximum inventory height.
		/// </summary>
		/// <remarks>
		/// Maximum height supported by the client.
		/// </remarks>
		private const int MaxHeight = 32;

		/// <summary>
		/// Item id for gold.
		/// </summary>
		private const int GoldItemId = 2000;

		/// <summary>
		/// Item id for stars.
		/// </summary>
		private const int StarItemId = 2071;

		/// <summary>
		/// Max amount of gold that fit into one stack.
		/// </summary>
		private const int GoldStackMax = 1000;

		private Creature _creature;
		private Dictionary<Pocket, InventoryPocket> _pockets;

		private object _upgradeEffectSyncLock = new object();
		private Dictionary<UpgradeCheckType, int> _upgradeCheckTypeCache = new Dictionary<UpgradeCheckType, int>();

		private bool _liveUpdateStarted;

		/// <summary>
		/// Initializes static information.
		/// </summary>
		static CreatureInventory()
		{
			// Set pockets directly modifiable by creatures.
			AccessiblePockets = new HashSet<Pocket>()
			{
				Pocket.Accessory1,
				Pocket.Accessory2,
				Pocket.Armor,
				Pocket.ArmorStyle,
				Pocket.BattleReward,
				Pocket.ComboCard,
				Pocket.Cursor,
				Pocket.EnchantReward,
				Pocket.Falias1,
				Pocket.Falias2,
				Pocket.Falias3,
				Pocket.Falias4,
				Pocket.Glove,
				Pocket.GloveStyle,
				Pocket.Head,
				Pocket.HeadStyle,
				Pocket.Inventory,
				Pocket.LeftHand1,
				Pocket.LeftHand2,
				Pocket.Magazine1,
				Pocket.Magazine2,
				Pocket.ManaCrystalReward,
				Pocket.PersonalInventory,
				Pocket.RightHand1,
				Pocket.RightHand2,
				Pocket.Robe,
				Pocket.RobeStyle,
				Pocket.Shoe,
				Pocket.ShoeStyle,
				Pocket.Temporary,
				Pocket.Trade,
				Pocket.VIPInventory,
			};

			// Add bags to the list of modifiable pockets.
			for (var i = Pocket.ItemBags; i <= Pocket.ItemBagsMax; i++)
				AccessiblePockets.Add(i);
		}

		/// <summary>
		/// These pockets aren't checked by the Count() method
		/// </summary>
		public static readonly IEnumerable<Pocket> InvisiblePockets = new[]
		{
			Pocket.Temporary
		};

		/// <summary>
		/// These pockets are checked by Safe methoods.
		/// </summary>
		public static ISet<Pocket> AccessiblePockets { get; private set; }

		/// <summary>
		/// The selected weapon set.
		/// </summary>
		public WeaponSet WeaponSet { get; private set; }

		/// <summary>
		/// The currently active right hand pocket (main weapon hand).
		/// </summary>
		public Pocket RightHandPocket { get { return (this.WeaponSet == WeaponSet.First ? Pocket.RightHand1 : Pocket.RightHand2); } }

		/// <summary>
		/// The currently active left hand pocket (off hand).
		/// </summary>
		public Pocket LeftHandPocket { get { return (this.WeaponSet == WeaponSet.First ? Pocket.LeftHand1 : Pocket.LeftHand2); } }

		/// <summary>
		/// The currently active magazine pocket (ammunition).
		/// </summary>
		public Pocket MagazinePocket { get { return (this.WeaponSet == WeaponSet.First ? Pocket.Magazine1 : Pocket.Magazine2); } }

		/// <summary>
		/// Reference to the item currently equipped in the right hand.
		/// </summary>
		public Item RightHand { get; protected set; }

		/// <summary>
		/// Reference to the item currently equipped in the left hand.
		/// </summary>
		public Item LeftHand { get; protected set; }

		/// <summary>
		/// Reference to the item currently equipped in the magazine (eg arrows).
		/// </summary>
		public Item Magazine { get; protected set; }

		/// <summary>
		/// Gets or sets the amount of gold, by modifying the inventory.
		/// </summary>
		public int Gold
		{
			get { return this.Count(GoldItemId); }
			set
			{
				var itemId = GoldItemId;
				var curAmount = this.Gold;
				var newAmount = Math.Max(0, value);

				if (newAmount < curAmount)
					this.Remove(itemId, curAmount - newAmount);
				else if (newAmount > curAmount)
					this.InsertStacks(itemId, newAmount - curAmount);
			}
		}

		/// <summary>
		/// Gets or sets the amount of stars (rafting reward), by modifying
		/// the inventory.
		/// </summary>
		public int Stars
		{
			get { return this.Count(StarItemId); }
			set
			{
				var itemId = StarItemId;
				var curAmount = this.Stars;
				var newAmount = Math.Max(0, value);

				if (newAmount < curAmount)
					this.Remove(itemId, curAmount - newAmount);
				else if (newAmount > curAmount)
					this.InsertStacks(itemId, newAmount - curAmount);
			}
		}

		/// <summary>
		/// Creates new creature inventory instance for creature.
		/// </summary>
		/// <param name="creature"></param>
		public CreatureInventory(Creature creature)
		{
			_creature = creature;

			_pockets = new Dictionary<Pocket, InventoryPocket>();

			// Cursor, Temp, Quests
			this.Add(new InventoryPocketStack(Pocket.Temporary));
			this.Add(new InventoryPocketStack(Pocket.Quests));
			this.Add(new InventoryPocketSingle(Pocket.Cursor));

			// Equipment
			for (var i = Pocket.Face; i <= Pocket.Accessory2; ++i)
				this.Add(new InventoryPocketSingle(i));

			// Style
			for (var i = Pocket.ArmorStyle; i <= Pocket.RobeStyle; ++i)
				this.Add(new InventoryPocketSingle(i));
		}

		/// <summary>
		/// Adds pocket to inventory.
		/// </summary>
		/// <param name="inventoryPocket"></param>
		public void Add(InventoryPocket inventoryPocket)
		{
			lock (_pockets)
			{
				if (_pockets.ContainsKey(inventoryPocket.Pocket))
					Log.Warning("Replacing pocket '{0}' in '{1}'s inventory.", inventoryPocket.Pocket, _creature);

				_pockets[inventoryPocket.Pocket] = inventoryPocket;
			}
		}

		/// <summary>
		/// Adds main inventories (inv, personal, VIP). Call after creature's
		/// defaults (RaceInfo) have been loaded.
		/// </summary>
		public void AddMainInventory()
		{
			if (_creature.RaceData == null)
				Log.Warning("Race for creature '{0}' ({1:X16}) not loaded before initializing main inventory.", _creature.Name, _creature.EntityId);

			var width = (_creature.RaceData != null ? _creature.InventoryWidth : DefaultWidth);
			if (width > MaxWidth)
			{
				width = MaxWidth;
				Log.Warning("AddMainInventory: Width exceeds max, using {0} instead.", MaxWidth);
			}

			var height = (_creature.RaceData != null ? _creature.InventoryHeight : DefaultHeight);
			if (height > MaxHeight)
			{
				height = MaxHeight;
				Log.Warning("AddMainInventory: Height exceeds max, using {0} instead.", MaxHeight);
			}

			// TODO: Race check
			this.Add(new InventoryPocketNormal(Pocket.Inventory, width, height));
			this.Add(new InventoryPocketNormal(Pocket.PersonalInventory, width, height));
			this.Add(new InventoryPocketNormal(Pocket.VIPInventory, width, height));
		}

		/// <summary>
		/// Subscribes inventory to events necessary for upgrade effect live updates.
		/// </summary>
		public void StartLiveUpdate()
		{
			if (_liveUpdateStarted)
				return;
			_liveUpdateStarted = true;

			_creature.LeveledUp += this.OnCreatureLeveledUp;
			_creature.Titles.Changed += this.OnCreatureChangedTitles;
			_creature.Skills.RankChanged += this.OnCreatureSkillRankChanged;
			_creature.Conditions.Changed += this.OnCreatureConditionsChanged;
			ChannelServer.Instance.Events.HoursTimeTick += this.OnHoursTimeTick;
		}

		/// <summary>
		/// Returns true if pocket exists in this inventory.
		/// </summary>
		/// <param name="pocket"></param>
		/// <returns></returns>
		public bool Has(Pocket pocket)
		{
			lock (_pockets)
				return _pockets.ContainsKey(pocket);
		}

		/// <summary>
		/// Returns item with the id, or null.
		/// </summary>
		/// <param name="entityId"></param>
		/// <returns></returns>
		public Item GetItem(long entityId)
		{
			lock (_pockets)
				return _pockets.Values.Select(pocket => pocket.GetItem(entityId)).FirstOrDefault(item => item != null);
		}

		/// <summary>
		/// Returns first item to match predicate, or null.
		/// </summary>
		/// <param name="predicate"></param>
		/// <param name="startAt">
		/// Affects the order of the returned items, based on their position in
		/// the inventory.
		/// </param>
		/// <returns></returns>
		public Item GetItem(Func<Item, bool> predicate, StartAt startAt = StartAt.Random)
		{
			lock (_pockets)
			{
				foreach (var pocket in _pockets.Values)
				{
					var item = pocket.GetItem(predicate, startAt);
					if (item != null)
						return item;
				}

				return null;
			}
		}

		/// <summary>
		/// Returns  a new list of all items that match the predicate.
		/// </summary>
		/// <param name="predicate"></param>
		/// <param name="startAt">
		/// Affects the order of the returned items, based on their position in
		/// the inventory.
		/// </param>
		/// <returns></returns>
		public List<Item> GetItems(Func<Item, bool> predicate, StartAt startAt = StartAt.Random)
		{
			var result = new List<Item>();

			lock (_pockets)
			{
				foreach (var pocket in _pockets.Values)
					result.AddRange(pocket.GetItems(predicate, startAt));
			}

			return result;
		}

		/// <summary>
		/// Returns a new list of all items in the inventory.
		/// </summary>
		/// <returns></returns>
		public Item[] GetItems()
		{
			Item[] result;

			lock (_pockets)
				result = _pockets.Values.SelectMany(pocket => pocket.Items.Where(a => a != null)).ToArray();

			return result;
		}

		/// <summary>
		/// Returns a new list of all items in all equipment pockets.
		/// </summary>
		/// <returns></returns>
		public Item[] GetAllEquipment()
		{
			Item[] result;

			lock (_pockets)
				result = _pockets.Values
					.Where(a => a.Pocket.IsEquip())
					.SelectMany(pocket => pocket.Items.Where(a => a != null))
					.ToArray();

			return result;
		}

		/// <summary>
		/// Returns a new list of all items that match the predicate,
		/// in all equipment pockets.
		/// </summary>
		/// <returns></returns>
		public Item[] GetAllEquipment(Func<Item, bool> predicate)
		{
			Item[] result;

			lock (_pockets)
				result = _pockets.Values
					.Where(a => a.Pocket.IsEquip())
					.SelectMany(pocket => pocket.Items.Where(a => a != null && predicate(a)))
					.ToArray();

			return result;
		}

		/// <summary>
		/// Returns a new list of all items in all equipment pockets
		/// that aren't hair or face.
		/// </summary>
		/// <returns></returns>
		public Item[] GetEquipment()
		{
			Item[] result;

			lock (_pockets)
				result = _pockets.Values
					.Where(a => a.Pocket.IsEquip() && a.Pocket != Pocket.Hair && a.Pocket != Pocket.Face)
					.SelectMany(pocket => pocket.Items.Where(a => a != null))
					.ToArray();

			return result;
		}

		/// <summary>
		/// Returns a new list of all items that match the predicate,
		/// in all equipment pockets that aren't hair or face.
		/// </summary>
		/// <returns></returns>
		public Item[] GetEquipment(Func<Item, bool> predicate)
		{
			Item[] result;

			lock (_pockets)
				result = _pockets.Values
					.Where(a => a.Pocket.IsEquip() && a.Pocket != Pocket.Hair && a.Pocket != Pocket.Face)
					.SelectMany(pocket => pocket.Items.Where(a => a != null && predicate(a)))
					.ToArray();

			return result;
		}

		/// <summary>
		/// Returns a new list of all items in all main equipment pockets,
		/// meaning no style or inactive weapon pockets.
		/// </summary>
		/// <returns></returns>
		public Item[] GetMainEquipment()
		{
			Item[] result;

			lock (_pockets)
				result = _pockets.Values
					.Where(a => a.Pocket.IsMainEquip(this.WeaponSet))
					.SelectMany(pocket => pocket.Items.Where(a => a != null))
					.ToArray();

			return result;
		}

		/// <summary>
		/// Returns a new list of all items in all main equipment pockets,
		/// meaning no style or inactive weapon pockets, that match
		/// the predicate.
		/// </summary>
		/// <returns></returns>
		public Item[] GetMainEquipment(Func<Item, bool> predicate)
		{
			Item[] result;

			lock (_pockets)
				result = _pockets.Values
					.Where(a => a.Pocket.IsMainEquip(this.WeaponSet))
					.SelectMany(pocket => pocket.Items.Where(a => a != null && predicate(a)))
					.ToArray();

			return result;
		}

		/// <summary>
		/// Returns item or throws security violation exception,
		/// if item didn't exist or isn't allowed to be accessed.
		/// </summary>
		/// <param name="entityId"></param>
		/// <returns></returns>
		public Item GetItemSafe(long entityId)
		{
			var result = this.GetItem(entityId);

			if (result == null)
				throw new SevereViolation("Creature does not have an item with entity id 0x{0:X16}", entityId);

			if (!AccessiblePockets.Contains(result.Info.Pocket))
				throw new SevereViolation("Item 0x{0:X16} is located in inaccessible pocket {1}", entityId, result.Info.Pocket);

			return result;
		}

		/// <summary>
		/// Returns item at the location, or null.
		/// </summary>
		/// <param name="pocket"></param>
		/// <param name="x"></param>
		/// <param name="y"></param>
		/// <returns></returns>
		public Item GetItemAt(Pocket pocket, int x, int y)
		{
			if (!this.Has(pocket))
				return null;

			lock (_pockets)
				return _pockets[pocket].GetItemAt(x, y);
		}

		/// <summary>
		/// Returns a free pocket id to be used for item bags.
		/// </summary>
		/// <returns></returns>
		public Pocket GetFreePocketId()
		{
			lock (_pockets)
			{
				for (var i = Pocket.ItemBags; i < Pocket.ItemBagsMax; ++i)
				{
					if (!_pockets.ContainsKey(i))
						return i;
				}
			}

			return Pocket.None;
		}

		/// <summary>
		/// Adds pocket for item and updates item's linked pocket.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool AddBagPocket(Item item)
		{
			var freePocket = this.GetFreePocketId();
			if (freePocket == Pocket.None)
				return false;

			item.OptionInfo.LinkedPocketId = freePocket;

			this.Add(new InventoryPocketNormal(freePocket, item.Data.BagWidth, item.Data.BagHeight));

			return true;
		}

		/// <summary>
		/// Returns list of all items in pocket. Returns null if the pocket
		/// doesn't exist.
		/// </summary>
		/// <param name="pocket"></param>
		/// <returns></returns>
		public List<Item> GetAllItemsFrom(Pocket pocket)
		{
			lock (_pockets)
			{
				if (!_pockets.ContainsKey(pocket))
					return null;

				return _pockets[pocket].Items.Where(a => a != null).ToList();
			}
		}

		/// <summary>
		/// Removes pocket from inventory.
		/// </summary>
		/// <param name="pocket"></param>
		/// <returns></returns>
		public bool Remove(Pocket pocket)
		{
			lock (_pockets)
			{
				if (pocket == Pocket.None || !_pockets.ContainsKey(pocket))
					return false;

				_pockets.Remove(pocket);
			}

			return true;
		}

		// Handlers
		// ------------------------------------------------------------------

		/// <summary>
		/// Used from MoveItem handler.
		/// </summary>
		/// <remarks>
		/// The item is the one that's interacted with, the one picked up
		/// when taking it, the one being put into a packet when it's one
		/// the cursor. Colliding items switch places with it.
		/// </remarks>
		/// <param name="item">Item to move</param>
		/// <param name="target">Pocket to move it to</param>
		/// <param name="targetX"></param>
		/// <param name="targetY"></param>
		/// <returns></returns>
		public bool Move(Item item, Pocket target, byte targetX, byte targetY)
		{
			if (!this.Has(target))
				return false;

			var source = item.Info.Pocket;
			var amount = item.Info.Amount;

			Item collidingItem = null;
			var collidingItemTarget = source;

			lock (_pockets)
			{
				// Hotfix for #200, ctrl+click-equipping.
				// If an item is moved from the inventory to a filled equip
				// slot, but there's not enough space in the source pocket
				// for the colliding item, it would vanish, because the Add
				// failed. ("Toss it in, it should be the cursor.")
				//
				// The following code tries to prevent that, by explicitly
				// checking if this is a ctrl+click-equip move, and whether
				// the potentially colliding item fits into the inventory.
				// 
				// Is there a better way to solve this? Maybe a more
				// generalized one? *Without* reverting the move on fail?
				if (source != Pocket.Cursor && target.IsEquip())
				{
					//Log.Debug("Inv2EqMove: {0} -> {1}", source, target);

					if ((collidingItem = _pockets[target].GetItemAt(0, 0)) != null)
					{
						var success = false;

						// Cursor will work by default, as it will be
						// empty after moving the new item out of it.
						if (source == Pocket.Cursor)
						{
							success = true;
							collidingItemTarget = Pocket.Cursor;
						}

						// Try main inv
						if (!success)
						{
							if (_pockets.ContainsKey(Pocket.Inventory))
							{
								success = _pockets[Pocket.Inventory].HasSpace(collidingItem);
								collidingItemTarget = Pocket.Inventory;
							}
						}

						// VIP inv
						if (!success)
						{
							if (_pockets.ContainsKey(Pocket.VIPInventory))
							{
								success = _pockets[Pocket.VIPInventory].HasSpace(collidingItem);
								collidingItemTarget = Pocket.VIPInventory;
							}
						}

						// Try bags
						if (_creature.Client.Account.PremiumServices.CanUseBags)
						{
							for (var i = Pocket.ItemBags; i <= Pocket.ItemBagsMax && !success; ++i)
							{
								if (_pockets.ContainsKey(i))
								{
									success = _pockets[i].HasSpace(collidingItem);
									collidingItemTarget = i;
								}
							}
						}

						if (!success)
						{
							Send.Notice(_creature, Localization.Get("There is no room in your inventory."));
							return false;
						}
					}
				}

				if (!_pockets[target].TryAdd(item, targetX, targetY, out collidingItem))
					return false;

				// If amount differs (item was added to stack)
				if (collidingItem != null && (item.Info.Amount != amount || (item.Info.Amount == 0 && item.Data.Type != ItemType.Sac)))
				{
					Send.ItemAmount(_creature, collidingItem);

					// Left overs or sac, update
					if (item.Info.Amount > 0 || item.Data.Type == ItemType.Sac)
					{
						Send.ItemAmount(_creature, item);
					}
					// All in, remove from cursor.
					else
					{
						_pockets[item.Info.Pocket].Remove(item);
						Send.ItemRemove(_creature, item);
					}
				}
				else
				{
					// Remove the item from the source pocket
					_pockets[source].Remove(item);

					if (collidingItem != null)
					{
						// Move colliding item into the pocket ascertained to
						// be free in the beginning.
						if (!_pockets[collidingItemTarget].Add(collidingItem))
						{
							// Should never happen, as it was checked above.
							Log.Error("CreatureInventory: Inv2EqMove error? Please report. {0} -> {1}", source, target);
						}
					}

					Send.ItemMoveInfo(_creature, item, source, collidingItem);
				}
			}

			// Inform about temp moves (items in temp don't count for quest objectives?)
			if (source == Pocket.Temporary && target == Pocket.Cursor)
			{
				this.OnItemEntersInventory(item);
				ChannelServer.Instance.Events.OnPlayerReceivesItem(_creature, item.Info.Id, item.Info.Amount);
			}

			// Check movement
			this.CheckLeftHand(item, source, target);
			this.CheckRightHand(item, source, target);

			// Equip handling
			if (target.IsEquip())
			{
				this.UpdateEquipReferences();
				this.OnEquip(item);
				if (collidingItem != null)
					this.OnUnequip(collidingItem);
				this.UpdateEquipStats();

				Send.EquipmentChanged(_creature, item);
			}
			else if (source.IsEquip())
			{
				this.UpdateEquipReferences();
				this.OnUnequip(item);
				this.UpdateEquipStats();

				Send.EquipmentMoved(_creature, source);
			}

			return true;
		}

		/// <summary>
		/// Moving item between char and pet, used from handler.
		/// </summary>
		/// <param name="pet">Always the pet</param>
		/// <param name="item"></param>
		/// <param name="other">The "other" creature, player when taking out, pet when putting in.</param>
		/// <param name="target"></param>
		/// <param name="targetX"></param>
		/// <param name="targetY"></param>
		/// <returns></returns>
		public bool MovePet(Creature pet, Item item, Creature other, Pocket target, int targetX, int targetY)
		{
			if (!this.Has(target) || !other.Inventory.Has(target))
				return false;

			var source = item.Info.Pocket;
			var amount = item.Info.Amount;

			// We have to copy the item to get a new id, otherwise there could
			// be collisions when saving, because the moved item is still in
			// the inventory of the pet/character (from the pov of the db).
			// http://dev.mabinoger.com/forum/index.php/topic/804-pet-inventory/
			var newItem = new Item(item);

			Item collidingItem = null;
			lock (_pockets)
			{
				if (!other.Inventory._pockets[target].TryAdd(newItem, (byte)targetX, (byte)targetY, out collidingItem))
					return false;

				// If amount differs (item was added to stack)
				if (collidingItem != null && newItem.Info.Amount != amount)
				{
					Send.ItemAmount(other, collidingItem);

					// Left overs, update
					if (newItem.Info.Amount > 0)
					{
						Send.ItemAmount(_creature, item);
					}
					// All in, remove from cursor.
					else
					{
						_pockets[item.Info.Pocket].Remove(item);
						Send.ItemRemove(_creature, item);
					}
				}
				else
				{
					// Remove the item from the source pocket
					_pockets[source].Remove(item);
					Send.ItemRemove(_creature, item, source);

					if (collidingItem != null)
					{
						// Remove colliding item
						Send.ItemRemove(other, collidingItem, target);

						// Toss it in, it should be the cursor.
						_pockets[source].Add(collidingItem);
						Send.ItemNew(_creature, collidingItem);
					}

					Send.ItemNew(other, newItem);

					Send.ItemMoveInfo(_creature, item, source, collidingItem);
				}
			}

			// Check movement
			pet.Inventory.CheckLeftHand(item, source, target);
			pet.Inventory.CheckRightHand(item, source, target);

			// Equip handling
			if (target.IsEquip())
			{
				pet.Inventory.UpdateEquipReferences();
				pet.Inventory.OnEquip(item);
				if (collidingItem != null)
					pet.Inventory.OnUnequip(collidingItem);
				pet.Inventory.UpdateEquipStats();

				Send.EquipmentChanged(pet, newItem);
			}
			else if (source.IsEquip())
			{
				pet.Inventory.UpdateEquipReferences();
				pet.Inventory.OnUnequip(item);
				pet.Inventory.UpdateEquipStats();

				Send.EquipmentMoved(pet, source);
			}

			return true;
		}

		/// <summary>
		/// Tries to put item into the inventory, by filling stacks and adding it.
		/// If it was completely added to the inventory, it's removed
		/// from the region the inventory's creature is in.
		/// </summary>
		/// <param name="item"></param>
		/// <returns>Returns true if item was picked up completely.</returns>
		public bool PickUp(Item item)
		{
			var originalAmount = item.Info.Amount;

			// Making a copy of the item and generating a new temp id
			// ensures that we can still remove the item from the ground
			// after moving it (region, x, y) to the pocket.
			// (Remove takes an id parameter, maybe this can be solved
			//   properly, see pet invs.)
			// We also need the new id to prevent conflicts in the db
			// (SVN r67).
			// If this is changed, the item's owner and protection limit
			// should be reset on pick up.

			var newItem = new Item(item);
			newItem.IsNew = true;

			var success = this.Insert(newItem, false);

			// Remove item from floor if it was completely added to
			// the inventory, into existing or new stacks.
			if (success)
			{
				_creature.Region.RemoveItem(item);
			}
			// Update original item's amount if it wasn't added completely.
			else if (newItem.Info.Amount != originalAmount)
			{
				item.Info.Amount = newItem.Info.Amount;
				// TODO: We need an update packet for items on the floor.
			}

			return success;
		}

		/// <summary>
		/// Changes weapon set, if necessary, and updates clients.
		/// </summary>
		/// <param name="set"></param>
		public void ChangeWeaponSet(WeaponSet set)
		{
			var unequipRightHand = this.RightHand;
			var unequipLeftHand = this.LeftHand;
			var unequipMagazine = this.Magazine;

			this.WeaponSet = set;
			this.UpdateEquipReferences();

			if (unequipRightHand != null) this.OnUnequip(unequipRightHand);
			if (unequipLeftHand != null) this.OnUnequip(unequipLeftHand);
			if (unequipMagazine != null) this.OnUnequip(unequipMagazine);

			if (this.RightHand != null) this.OnEquip(this.RightHand);
			if (this.LeftHand != null) this.OnEquip(this.LeftHand);
			if (this.Magazine != null) this.OnEquip(this.Magazine);

			this.UpdateEquipStats();

			// Make sure the creature is logged in
			if (_creature.Region != Region.Limbo)
				Send.UpdateWeaponSet(_creature);
		}

		// Adding
		// ------------------------------------------------------------------

		/// <summary>
		/// Adds item to pocket at the position it currently has.
		/// Returns false if pocket doesn't exist.
		/// </summary>
		public bool InitAdd(Item item)
		{
			lock (_pockets)
			{
				if (!_pockets.ContainsKey(item.Info.Pocket))
					return false;

				_pockets[item.Info.Pocket].AddUnsafe(item);
			}

			if (item.Info.Pocket.IsEquip())
			{
				this.UpdateEquipReferences();
				this.OnEquip(item);
				//this.UpdateEquipStats();
			}

			return true;
		}

		// TODO: Add central "Add" method that all others use, for central stuff
		//   like adding bag pockets. This wil require a GetFreePosition
		//   method in the pockets.

		/// <summary>
		/// Tries to add item to pocket. Returns false if the pocket
		/// doesn't exist or there was no space.
		/// </summary>
		public bool Add(Item item, Pocket pocket)
		{
			var success = false;

			lock (_pockets)
			{
				if (!_pockets.ContainsKey(pocket))
					return success;

				success = _pockets[pocket].Add(item);
			}

			if (success)
			{
				Send.ItemNew(_creature, item);

				// Add bag pocket if it doesn't already exist.
				if (item.OptionInfo.LinkedPocketId != Pocket.None && !this.Has(item.OptionInfo.LinkedPocketId))
					this.AddBagPocket(item);

				if (_creature.IsPlayer && pocket != Pocket.Temporary)
				{
					this.OnItemEntersInventory(item);

					// Notify everybody about receiving the item.
					ChannelServer.Instance.Events.OnPlayerReceivesItem(_creature, item.Info.Id, item.Amount);

					// If item was a sac, we have to notify the server about
					// receiving its *contents* as well.
					if (item.Data.StackType == StackType.Sac)
						ChannelServer.Instance.Events.OnPlayerReceivesItem(_creature, item.Data.StackItemId, item.Info.Amount);
				}

				if (pocket.IsEquip())
				{
					this.UpdateEquipReferences();
					this.OnEquip(item);
					this.UpdateEquipStats();

					if (_creature.Region != Region.Limbo)
						Send.EquipmentChanged(_creature, item);
				}
			}

			return success;
		}

		/// <summary>
		/// Tries to add item to one of the main inventories, using the temp
		/// inv as fallback (if specified to do so). Returns false if
		/// there was no space.
		/// </summary>
		public bool Add(Item item, bool tempFallback)
		{
			var success = this.TryAutoAdd(item, tempFallback);

			// Inform about new item
			if (success)
			{
				Send.ItemNew(_creature, item);

				// Add bag pocket if it doesn't already exist.
				if (item.OptionInfo.LinkedPocketId != Pocket.None && !this.Has(item.OptionInfo.LinkedPocketId))
					this.AddBagPocket(item);
			}

			return success;
		}

		/// <summary>
		/// Tries to add item to one of the main inventories,
		/// using temp as fallback. Unlike "Add" the item will be filled
		/// into stacks first, if possible, before calling Add.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="tempFallback">Add to temp inv when all other pockets are full?</param>
		/// <returns>Returns true if item was added to the inventory completely.</returns>
		public bool Insert(Item item, bool tempFallback)
		{
			List<Item> changed;
			return this.Insert(item, tempFallback, out changed);
		}

		/// <summary>
		/// Tries to add item to one of the main inventories,
		/// using temp as fallback. Unlike "Add" the item will be filled
		/// into stacks first, if possible, before calling Add.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="tempFallback">Add to temp inv when all other pockets are full?</param>
		/// <param name="changed">List of stacks that items were inserted into.</param>
		/// <returns>Returns true if item was added to the inventory completely.</returns>
		public bool Insert(Item item, bool tempFallback, out List<Item> changed)
		{
			changed = null;

			var originalAmount = item.Amount;

			// TODO: Maybe it would be cleaner to ask the pockets for a list
			//   of certain items, that we can fill into, instead of them
			//   returning lists of changed items via out.
			if (item.Data.StackType == StackType.Stackable)
			{
				// Try stacks/sacs first
				lock (_pockets)
				{
					// Main inv
					_pockets[Pocket.Inventory].FillStacks(item, out changed);
					this.UpdateChangedItems(changed);

					// VIP inv
					// TODO: Add and check inv locks
					if (item.Info.Amount != 0)
					{
						_pockets[Pocket.VIPInventory].FillStacks(item, out changed);
						this.UpdateChangedItems(changed);
					}

					// Bags
					if (_creature.Client.Account.PremiumServices.CanUseBags)
					{
						for (var i = Pocket.ItemBags; i <= Pocket.ItemBagsMax; ++i)
						{
							if (item.Info.Amount == 0)
								break;

							if (_pockets.ContainsKey(i))
							{
								_pockets[i].FillStacks(item, out changed);
								this.UpdateChangedItems(changed);
							}
						}
					}
				}

				// Notify everybody about receiving the item, amount being
				// the amount of items filled into stacks.
				var inserted = (originalAmount - item.Info.Amount);
				if (inserted > 0 && _creature.IsPlayer)
					ChannelServer.Instance.Events.OnPlayerReceivesItem(_creature, item.Info.Id, inserted);

				if (item.Info.Amount == 0)
					return true;
			}

			return this.Add(item, tempFallback);
		}

		/// <summary>
		/// Adds new gold stacks to the inventory until the amount was added,
		/// using temp as fallback. Returns false if something went wrong,
		/// and not everything could be added.
		/// </summary>
		/// <param name="amount"></param>
		/// <returns></returns>
		public bool AddGold(int amount)
		{
			return this.InsertStacks(GoldItemId, amount);
		}

		/// <summary>
		/// Adds new stacks to the inventory until the amount was added,
		/// using temp as fallback. Returns false if something went wrong,
		/// and not everything could be added.
		/// </summary>
		/// <param name="itemId">Id of the item to add.</param>
		/// <param name="amount">Amount to add, value affects the amount of stacks.</param>
		/// <returns></returns>
		/// <example>
		/// InsertStacks(2000, 500);
		/// Add 500g to inventory, by either adding them to existing stacks,
		/// or creating a new one.
		/// 
		/// InsertStacks(2000, 10000);
		/// Add 10,000g to inventory, by either adding them to existing stacks,
		/// or creating up to 10 new ones.
		/// </example>
		public bool InsertStacks(int itemId, int amount)
		{
			if (amount < 0)
				return false;

			// Insert new stacks till amount is 0.
			do
			{
				var stackItem = new Item(itemId);
				var stackAmount = Math.Min(stackItem.Data.StackMax, amount);

				stackItem.Amount = stackAmount;
				amount -= stackAmount;

				if (!this.Insert(stackItem, true))
					return false;
			}
			while (amount > 0);

			return true;
		}

		/// <summary>
		/// Tries to add item to one of the main invs or bags,
		/// wherever free space is available. Returns whether it was successful.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="tempFallback">Use temp inventory if all others are full?</param>
		/// <returns></returns>
		private bool TryAutoAdd(Item item, bool tempFallback)
		{
			var success = false;
			var inTemp = false;

			lock (_pockets)
			{
				// Try main inv
				if (_pockets.ContainsKey(Pocket.Inventory))
					success = _pockets[Pocket.Inventory].Add(item);

				// VIP inv
				// TODO: Add and check inv locks
				if (!success)
				{
					if (_pockets.ContainsKey(Pocket.VIPInventory))
						success = _pockets[Pocket.VIPInventory].Add(item);
				}

				// Try bags
				if (_creature.Client.Account.PremiumServices.CanUseBags)
				{
					for (var i = Pocket.ItemBags; i <= Pocket.ItemBagsMax; ++i)
					{
						if (success)
							break;

						if (_pockets.ContainsKey(i))
							success = _pockets[i].Add(item);
					}
				}

				// Try temp
				if (!success && tempFallback)
				{
					success = _pockets[Pocket.Temporary].Add(item);
					inTemp = true;
				}
			}

			if (success && _creature.IsPlayer && !inTemp)
			{
				this.OnItemEntersInventory(item);

				// Notify everybody about receiving the item.
				ChannelServer.Instance.Events.OnPlayerReceivesItem(_creature, item.Info.Id, item.Amount);

				// If item was a sac, we have to notify the server about
				// receiving its *contents* as well.
				if (item.Data.StackType == StackType.Sac)
					ChannelServer.Instance.Events.OnPlayerReceivesItem(_creature, item.Data.StackItemId, item.Info.Amount);
			}

			return success;
		}

		// Removing
		// ------------------------------------------------------------------

		/// <summary>
		/// Removes item from inventory, if it is in it, and sends update packets.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool Remove(Item item)
		{
			lock (_pockets)
			{
				if (_pockets.Values.Any(pocket => pocket.Remove(item)))
				{
					Send.ItemRemove(_creature, item);

					// Remove bag pocket
					if (item.OptionInfo.LinkedPocketId != Pocket.None)
					{
						this.Remove(item.OptionInfo.LinkedPocketId);
						item.OptionInfo.LinkedPocketId = Pocket.None;
					}

					this.OnItemLeavesInventory(item);
					ChannelServer.Instance.Events.OnPlayerRemovesItem(_creature, item.Info.Id, item.Info.Amount);

					if (item.Info.Pocket.IsEquip())
					{
						this.CheckLeftHand(item, item.Info.Pocket, Pocket.None);
						this.CheckRightHand(item, item.Info.Pocket, Pocket.None);

						this.UpdateEquipReferences();
						this.OnUnequip(item);
						this.UpdateEquipStats();

						if (_creature.Region != Region.Limbo)
							Send.EquipmentMoved(_creature, item.Info.Pocket);
					}

					return true;
				}
			}

			return false;
		}

		/// <summary>
		/// Removes the amount of items with the id from the inventory.
		/// Returns true if the specified amount was removed.
		/// </summary>
		/// <remarks>
		/// Does not check amount before removing.
		/// </remarks>
		/// <param name="itemId"></param>
		/// <param name="amount"></param>
		/// <returns></returns>
		public bool Remove(int itemId, int amount = 1)
		{
			if (amount < 0)
				amount = 0;

			var changed = new List<Item>();
			var removed = 0;

			lock (_pockets)
			{
				foreach (var pocket in _pockets.Values)
				{
					var r = pocket.Remove(itemId, amount, ref changed);
					amount -= r;
					removed += r;

					if (amount == 0)
						break;
				}
			}

			this.UpdateChangedItems(changed);

			ChannelServer.Instance.Events.OnPlayerRemovesItem(_creature, itemId, removed);

			return (amount == 0);
		}

		/// <summary>
		/// Removes the amount of gold from the inventory.
		/// Returns true if the specified amount was removed.
		/// </summary>
		/// <param name="amount"></param>
		/// <returns></returns>
		public bool RemoveGold(int amount)
		{
			return this.Remove(GoldItemId, amount);
		}

		/// <summary>
		/// Reduces item's amount and sends the necessary update packets.
		/// Also removes the item, if it's not a sack and its amount reaches 0.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="amount"></param>
		/// <returns></returns>
		public bool Decrement(Item item, ushort amount = 1)
		{
			if (!this.Has(item) || item.Info.Amount == 0 || item.Info.Amount < amount)
				return false;

			item.Info.Amount -= amount;

			if (item.Info.Amount > 0 || item.Data.StackType == StackType.Sac)
			{
				ChannelServer.Instance.Events.OnPlayerRemovesItem(_creature, item.Info.Id, amount);
				Send.ItemAmount(_creature, item);
			}
			else
			{
				this.Remove(item);
				Send.ItemRemove(_creature, item);
			}

			return true;
		}

		// Checks
		// ------------------------------------------------------------------

		/// <summary>
		/// Returns true uf the item exists in this inventory.
		/// </summary>
		/// <param name="item"></param>
		/// <returns></returns>
		public bool Has(Item item)
		{
			lock (_pockets)
				return _pockets.Values.Any(pocket => pocket.Has(item));
		}

		/// <summary>
		/// Returns the amount of items with this id in the inventory.
		/// </summary>
		/// <param name="itemId"></param>
		/// <returns></returns>
		public int Count(int itemId)
		{
			lock (_pockets)
				return _pockets.Values.Where(a => !InvisiblePockets.Contains(a.Pocket))
					.Sum(pocket => pocket.CountItem(itemId));
		}

		/// <summary>
		/// Returns the amount of items in the inventory that match the
		/// given tag.
		/// </summary>
		/// <param name="tag"></param>
		/// <returns></returns>
		public int Count(string tag)
		{
			lock (_pockets)
				return _pockets.Values.Where(a => !InvisiblePockets.Contains(a.Pocket))
					.Sum(pocket => pocket.CountItem(tag));
		}

		/// <summary>
		/// Returns the number of items in the given pocket.
		/// Returns -1 if the pocket doesn't exist.
		/// </summary>
		/// <param name="pocket"></param>
		/// <returns></returns>
		public int CountItemsInPocket(Pocket pocket)
		{
			lock (_pockets)
			{
				if (!_pockets.ContainsKey(pocket))
					return -1;

				return _pockets[pocket].Count;
			}
		}

		/// <summary>
		/// Returns whether inventory contains the item in this amount.
		/// </summary>
		/// <param name="itemId"></param>
		/// <param name="amount"></param>
		/// <returns></returns>
		public bool Has(int itemId, int amount = 1)
		{
			return (this.Count(itemId) >= amount);
		}

		/// <summary>
		/// Returns whether inventory contains gold in this amount.
		/// </summary>
		/// <param name="amount"></param>
		/// <returns></returns>
		public bool HasGold(int amount)
		{
			return this.Has(GoldItemId, amount);
		}

		// Helpers
		// ------------------------------------------------------------------

		/// <summary>
		/// Called when an item is equipped, or becomes "active".
		/// Calls and sets the appropriate events and stat bonuses.
		/// Does not update the client.
		/// </summary>
		/// <remarks>
		/// Before this is called, the references should be updated,
		/// so the subscribers see the character as he is *after*
		/// equipping the item. For example, to check the equipment
		/// for custom set bonuses.
		/// 
		/// Afterwards the equip stats should be updated, so the client
		/// displays the changes.
		/// 
		/// Both of these things aren't done inside this method, because
		/// there are cases where we have to equip/unequip multiple items,
		/// and we don't want to spam the client.
		/// 
		/// You also have to take care of the visual equipment updates,
		/// for similar reasons.
		/// </remarks>
		/// <param name="item"></param>
		private void OnEquip(Item item)
		{
			// For *players* who went through ChannelLogin...
			if (_creature.IsPlayer && _creature.Client.State == ClientState.LoggedIn)
			{
				// Raise event
				ChannelServer.Instance.Events.OnPlayerEquipsItem(_creature, item);

				// Execute script
				var itemScript = ChannelServer.Instance.ScriptManager.ItemScripts.Get(item.Info.Id);
				if (itemScript != null)
					itemScript.OnEquip(_creature, item);
			}

			// Apply bonuses if item is in a main equip pocket,
			// i.e. no style, hair, face, or second weapon set.
			if (item.Info.Pocket.IsMainEquip(this.WeaponSet))
			{
				this.ApplyDefenseBonuses(item);
				this.ApplyUpgradeEffects(item);
			}
		}

		/// <summary>
		/// Called when an item is unequipped, or becomes "inactive".
		/// Calls and removes the appropriate events and stat bonuses.
		/// Does not update the client.
		/// </summary>
		/// <remarks>
		/// Before this is called, the references should be updated,
		/// so the subscribers see the character as he is *after*
		/// unequipping the item. For example, to check the equipment
		/// for custom set bonuses.
		/// 
		/// Afterwards the equip stats should be updated, so the client
		/// displays the changes.
		/// 
		/// Both of these things aren't done inside this method, because
		/// there are cases where we have to equip/unequip multiple items,
		/// and we don't want to spam the client.
		/// 
		/// You also have to take care of the visual equipment updates,
		/// for similar reasons.
		/// </remarks>
		/// <param name="item"></param>
		private void OnUnequip(Item item)
		{
			// For *players* who went through ChannelLogin...
			if (_creature.IsPlayer && _creature.Client.State == ClientState.LoggedIn)
			{
				// Raise event
				ChannelServer.Instance.Events.OnPlayerUnequipsItem(_creature, item);

				// Execute script
				var itemScript = ChannelServer.Instance.ScriptManager.ItemScripts.Get(item.Info.Id);
				if (itemScript != null)
					itemScript.OnUnequip(_creature, item);
			}

			_creature.StatMods.Remove(StatModSource.Equipment, item.EntityId);
		}

		/// <summary>
		/// Raised when inventory's creature leveled up.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="levelBefore"></param>
		private void OnCreatureLeveledUp(Creature creature, int levelBefore)
		{
			// Would it be more efficient to check if updating is actually
			// necessary? Or maybe even to filter out the items that need it?
			// but then we'd be iterating over all items and upgrade effects
			// *multiple* times...

			this.UpdateStatBonuses();
		}

		/// <summary>
		/// Raised when inventory's creature changes titles.
		/// </summary>
		/// <param name="creature"></param>
		private void OnCreatureChangedTitles(Creature creature)
		{
			this.UpdateStatBonuses();
		}

		/// <summary>
		/// Raised when one of the inventory's creature's skill's rank changes.
		/// </summary>
		/// <param name="creature"></param>
		/// <param name="skill"></param>
		private void OnCreatureSkillRankChanged(Creature creature, Skill skill)
		{
			this.UpdateStatBonuses();
		}

		/// <summary>
		/// Raised when inventory's creature's conditions change.
		/// </summary>
		/// <param name="creature"></param>
		private void OnCreatureConditionsChanged(Creature creature)
		{
			this.UpdateStatBonuses();
		}

		/// <summary>
		/// Raised once ever RL hour.
		/// </summary>
		/// <param name="now"></param>
		private void OnHoursTimeTick(ErinnTime now)
		{
			// Update on midnight, for Erinn month checks
			if (now.DateTime.Hour == 0)
				this.UpdateStatBonuses();
		}

		/// <summary>
		/// Removes all upgrade effect bonuses from current equipment and
		/// reapplies them.
		/// </summary>
		public void UpdateStatBonuses()
		{
			lock (_upgradeEffectSyncLock)
			{
				// Go through all main equipment items, remove their effects
				// and reapply them.
				foreach (var item in this.GetMainEquipment())
				{
					_creature.StatMods.Remove(StatModSource.Equipment, item.EntityId);
					this.ApplyDefenseBonuses(item);
					this.ApplyUpgradeEffects(item);
				}

				this.UpdateEquipStats();
			}
		}

		/// <summary>
		/// Applies non-upgrade Defense and Protection bonuses from the item.
		/// </summary>
		/// <param name="item"></param>
		private void ApplyDefenseBonuses(Item item)
		{
			if (item.OptionInfo.Defense != 0)
				_creature.StatMods.Add(Stat.DefenseBaseMod, item.OptionInfo.Defense, StatModSource.Equipment, item.EntityId);
			if (item.OptionInfo.Protection != 0)
				_creature.StatMods.Add(Stat.ProtectionBaseMod, item.OptionInfo.Protection, StatModSource.Equipment, item.EntityId);
		}

		/// <summary>
		/// Applies upgrade effects from item.
		/// </summary>
		/// <param name="item"></param>
		private void ApplyUpgradeEffects(Item item)
		{
			foreach (var effect in item.GetUpgradeEffects())
			{
				var stat = effect.Stat.ToStat();
				var value = effect.Value;

				// Check stat
				if (stat == Stat.None)
				{
					Log.Warning("ApplyUpgradeEffects: Unknown/unhandled stat '{0}'.", effect.Stat);
					continue;
				}

				// Check requirements
				var fulfilled = false;

				// None
				if (effect.CheckType == UpgradeCheckType.None)
				{
					fulfilled = true;
				}
				// Stat ==, >, >=, <, <=
				else if (effect.CheckType >= UpgradeCheckType.GreaterThan && effect.CheckType <= UpgradeCheckType.Equal)
				{
					// Check upgrade stat and get value
					var valueToCheck = 0;
					switch (effect.CheckStat)
					{
						case UpgradeStat.Level: valueToCheck = _creature.Level; break;
						case UpgradeStat.TotalLevel: valueToCheck = _creature.TotalLevel; break;
						case UpgradeStat.ExplorationLevel: valueToCheck = 0; break; // TODO: Set once we have exploration levels.
						case UpgradeStat.Age: valueToCheck = _creature.Age; break;

						default:
							Log.Warning("ApplyUpgradeEffects: Unknown/unhandled check stat '{0}'.", effect.CheckStat);
							continue;
					}

					// Check value
					switch (effect.CheckType)
					{
						case UpgradeCheckType.Equal: fulfilled = (valueToCheck == effect.CheckValue); break;
						case UpgradeCheckType.GreaterThan: fulfilled = (valueToCheck > effect.CheckValue); break;
						case UpgradeCheckType.GreaterEqualThan: fulfilled = (valueToCheck >= effect.CheckValue); break;
						case UpgradeCheckType.LowerThan: fulfilled = (valueToCheck < effect.CheckValue); break;
						case UpgradeCheckType.LowerEqualThan: fulfilled = (valueToCheck <= effect.CheckValue); break;
					}
				}
				// Skill rank >, <, ==
				else if (effect.CheckType >= UpgradeCheckType.SkillRankEqual && effect.CheckType >= UpgradeCheckType.SkillRankLowerThan)
				{
					var skillId = effect.CheckSkillId;
					var skillRank = effect.CheckSkillRank;

					var skill = _creature.Skills.Get(effect.CheckSkillId);
					if (skill != null)
					{
						switch (effect.CheckType)
						{
							case UpgradeCheckType.SkillRankEqual: fulfilled = (skill.Info.Rank == effect.CheckSkillRank); break;
							case UpgradeCheckType.SkillRankGreaterThan: fulfilled = (skill.Info.Rank >= effect.CheckSkillRank); break;
							case UpgradeCheckType.SkillRankLowerThan: fulfilled = (skill.Info.Rank < effect.CheckSkillRank); break;
						}
					}
				}
				// Broken
				else if (effect.CheckType == UpgradeCheckType.WhenBroken)
				{
					fulfilled = (effect.CheckBroken && item.Durability == 0) || (!effect.CheckBroken && item.Durability != 0);
				}
				// Title
				else if (effect.CheckType == UpgradeCheckType.HoldingTitle)
				{
					fulfilled = (_creature.Titles.SelectedTitle == effect.CheckTitleId);
				}
				// Condition
				else if (effect.CheckType == UpgradeCheckType.InAStateOf)
				{
					fulfilled = _creature.Conditions.Has(effect.CheckCondition);
				}
				// PTJ
				else if (effect.CheckType == UpgradeCheckType.IfPtjCompletedMoreThan)
				{
					var trackRecord = _creature.Quests.GetPtjTrackRecord(effect.CheckPtj);
					fulfilled = (trackRecord.Done >= effect.CheckValue);
				}
				// Month
				else if (effect.CheckType == UpgradeCheckType.WhileBeing)
				{
					fulfilled = (ErinnTime.Now.Month == (int)effect.CheckMonth);
				}
				// Summon
				else if (effect.CheckType == UpgradeCheckType.WhileSummoned)
				{
					switch (effect.CheckStat)
					{
						case UpgradeStat.Pet: fulfilled = (_creature.Pet != null); break;
						case UpgradeStat.Golem: fulfilled = false; break; // TODO: Set once we have golems.
						case UpgradeStat.BarrierSpikes: fulfilled = false; break; // TODO: Set once we have barrier spikes.

						default:
							Log.Warning("ApplyUpgradeEffects: Unknown/unhandled check summon '{0}'.", effect.CheckStat);
							continue;
					}
				}
				else
				{
					Log.Warning("ApplyUpgradeEffects: Unknown/unhandled check type '{0}'.", effect.CheckType);
					continue;
				}

				// Apply if requirements are fulfilled
				if (fulfilled)
					_creature.StatMods.Add(stat, value, StatModSource.Equipment, item.EntityId);
			}
		}

		/// <summary>
		/// Makes sure you can't combine invalid equipment, like 2H and shields.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="source"></param>
		/// <param name="target"></param>
		private void CheckRightHand(Item item, Pocket source, Pocket target)
		{
			var rightItem = this.RightHand;

			// Move 2H weapon if shield is euipped
			if (target == this.LeftHandPocket && item.IsShieldLike && (rightItem != null && rightItem.IsTwoHand))
			{
				lock (_pockets)
				{
					// Switch item
					var success = _pockets[source].Add(rightItem);

					// Fallback, temp inv
					if (!success)
						success = _pockets[Pocket.Temporary].Add(rightItem);

					if (success)
					{
						_pockets[this.RightHandPocket].Remove(rightItem);

						Send.ItemMoveInfo(_creature, rightItem, this.RightHandPocket, null);
						if (_creature.Region != Region.Limbo)
							Send.EquipmentMoved(_creature, this.RightHandPocket);

						this.OnUnequip(rightItem);
					}
				}
			}
		}

		/// <summary>
		/// Unequips item in left hand/magazine, if item in right hand is moved.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="source"></param>
		/// <param name="target"></param>
		private void CheckLeftHand(Item item, Pocket source, Pocket target)
		{
			var pocketOfInterest = Pocket.None;

			if (source == Pocket.RightHand1 || source == Pocket.RightHand2)
				pocketOfInterest = source;
			if (target == Pocket.RightHand1 || target == Pocket.RightHand2)
				pocketOfInterest = target;

			if (pocketOfInterest == Pocket.None)
				return;

			// Check LeftHand first, switch to Magazine if it's empty
			lock (_pockets)
			{
				var leftPocket = pocketOfInterest + 2; // Left Hand 1/2
				var leftItem = _pockets[leftPocket].GetItemAt(0, 0);
				if (leftItem == null)
				{
					leftPocket += 2; // Magazine 1/2
					leftItem = _pockets[leftPocket].GetItemAt(0, 0);

					// Nothing to remove
					if (leftItem == null)
						return;
				}

				// Special handling of shield-likes (shields, books, etc)
				if (leftItem.IsShieldLike)
				{
					// If right hand item is something that can be combined with
					// a shield, the unequipping must be canceled. Things that
					// don't go with shields include bows and 2H weapons,
					// possibly more.
					// TODO: Is there a better way to check this?
					if (!item.IsBow && !item.IsTwoHand)
						return;
				}

				// Try inventory first.
				// TODO: List of pockets stuff can be auto-moved to.
				var success = _pockets[Pocket.Inventory].Add(leftItem);

				// Fallback, temp inv
				if (!success)
					success = _pockets[Pocket.Temporary].Add(leftItem);

				if (success)
				{
					_pockets[leftPocket].Remove(leftItem);

					Send.ItemMoveInfo(_creature, leftItem, leftPocket, null);
					if (_creature.Region != Region.Limbo)
						Send.EquipmentMoved(_creature, leftPocket);

					this.OnUnequip(leftItem);
				}
			}
		}

		/// <summary>
		/// Sends amount update or remove packets for all items, depending on
		/// their amount.
		/// </summary>
		/// <param name="items"></param>
		private void UpdateChangedItems(IEnumerable<Item> items)
		{
			if (items == null)
				return;

			foreach (var item in items)
			{
				if (item.Info.Amount > 0 || item.Data.StackType == StackType.Sac)
					Send.ItemAmount(_creature, item);
				else
					Send.ItemRemove(_creature, item);
			}
		}

		/// <summary>
		/// Updates quick access equipment refernces.
		/// </summary>
		/// <param name="toCheck"></param>
		private void UpdateEquipReferences()
		{
			lock (_pockets)
			{
				this.RightHand = _pockets[this.RightHandPocket].GetItemAt(0, 0);
				this.LeftHand = _pockets[this.LeftHandPocket].GetItemAt(0, 0);
				this.Magazine = _pockets[this.MagazinePocket].GetItemAt(0, 0);
			}
		}

		/// <summary>
		/// Sends private stat update for all equipment relevant stats.
		/// </summary>
		private void UpdateEquipStats()
		{
			Send.StatUpdate(_creature, StatUpdateType.Private,
				Stat.AttackMinBase, Stat.AttackMaxBase,
				Stat.AttackMinBaseMod, Stat.AttackMaxBaseMod,
				Stat.RightAttackMinMod, Stat.RightAttackMaxMod,
				Stat.LeftAttackMinMod, Stat.LeftAttackMaxMod,
				Stat.InjuryMinBaseMod, Stat.InjuryMaxBaseMod,
				Stat.RightInjuryMinMod, Stat.RightInjuryMaxMod,
				Stat.LeftInjuryMinMod, Stat.LeftInjuryMaxMod,
				Stat.CriticalBase, Stat.CriticalBaseMod,
				Stat.LeftCriticalMod, Stat.RightCriticalMod,
				Stat.BalanceBase, Stat.BalanceBaseMod,
				Stat.LeftBalanceMod, Stat.RightBalanceMod,
				Stat.DefenseBaseMod, Stat.ProtectionBaseMod,

				Stat.AttackMinMod, Stat.AttackMaxMod,
				Stat.InjuryMinMod, Stat.InjuryMaxMod,
				Stat.CriticalMod, Stat.BalanceMod,
				Stat.DefenseMod, Stat.ProtectionMod,
				Stat.StrMod, Stat.DexMod, Stat.IntMod, Stat.WillMod, Stat.LuckMod,
				Stat.LifeMaxMod, Stat.ManaMaxMod, Stat.StaminaMaxMod,
				Stat.MagicAttackMod, Stat.MagicDefenseMod,
				Stat.CombatPower, Stat.PoisonImmuneMod, Stat.ArmorPierceMod
			);
		}

		/// <summary>
		/// Handles events that need to happen when an item "enters" the
		/// inventory.
		/// </summary>
		/// <remarks>
		/// Only called when an item is actually new to the inventory.
		/// </remarks>
		/// <param name="item"></param>
		private void OnItemEntersInventory(Item item)
		{
			// Add quest to quest manager
			if (item.Quest != null)
			{
				var quest = item.Quest;

				// Add
				_creature.Quests.Add(quest);
			}
		}

		/// <summary>
		/// Handles events that need to happen when an item "leaves" the
		/// inventory.
		/// </summary>
		/// <remarks>
		/// Only called when an item is completely removed from the inventory.
		/// </remarks>
		/// <param name="item"></param>
		private void OnItemLeavesInventory(Item item)
		{
			// Remove quest from quest manager
			if (item.Quest != null)
			{
				var quest = item.Quest;

				// Only give up quest if it's incomplete, otherwise the
				// completed quest would be removed from the quest manager,
				// and the player would receive auto quests again.
				if (quest.State != QuestState.Complete)
					_creature.Quests.GiveUp(item.Quest);
			}
		}

		// Functions
		// ------------------------------------------------------------------

		/// <summary>
		/// Reduces durability and updates client.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="amount"></param>
		public void ReduceDurability(Item item, int amount)
		{
			if (!this.Has(item))
				return;

			item.OptionInfo.Durability = Math.Max(0, item.OptionInfo.Durability - amount);
			Send.ItemDurabilityUpdate(_creature, item);
		}

		/// <summary>
		/// Reduces max durability and updates client.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="amount"></param>
		public void ReduceMaxDurability(Item item, int amount)
		{
			if (!this.Has(item))
				return;

			item.OptionInfo.DurabilityMax = Math.Max(1000, item.OptionInfo.DurabilityMax - amount);
			if (item.OptionInfo.DurabilityMax < item.OptionInfo.Durability)
				item.Durability = item.OptionInfo.DurabilityMax;

			Send.ItemDurabilityUpdate(_creature, item);
			Send.ItemMaxDurabilityUpdate(_creature, item);
		}

		/// <summary>
		/// Increases item's proficiency and updates client.
		/// </summary>
		/// <param name="item"></param>
		/// <param name="amount"></param>
		public void AddProficiency(Item item, int amount)
		{
			item.Proficiency += amount;

			Send.ItemExpUpdate(_creature, item);
		}
	}
}
