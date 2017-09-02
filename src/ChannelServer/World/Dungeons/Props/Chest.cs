// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using Aura.Channel.Network.Sending;
using Aura.Channel.World.Dungeons.Puzzles;
using Aura.Channel.World.Entities;
using Aura.Channel.World.Entities.Props;
using Aura.Shared.Util;

namespace Aura.Channel.World.Dungeons.Props
{
    public class Chest : DungeonProp
    {
        protected List<Item> _items;

        public Chest(int propId, string name)
            : base(propId, name)
        {
            _items = new List<Item>();

            Name = name;
            Behavior = DefaultBehavior;
        }

        public Chest(Puzzle puzzle, string name)
            : this(puzzle.Dungeon.Data.ChestId, name)
        {
        }

        public bool IsOpen => State == "open";

        protected virtual void DefaultBehavior(Creature creature, Prop prop)
        {
            if (State == "open")
                return;

            SetState("open");
            DropItems(creature);
        }

        /// <summary>
        ///     Drops all items inside the chest to the floor.
        /// </summary>
        /// <param name="opener">If not null, creature becomes the owner of the items.</param>
        public void DropItems(Creature opener)
        {
            lock (_items)
            {
                foreach (var item in _items)
                    item.Drop(Region, GetPosition(), Item.DropRadius, opener, false);

                _items.Clear();
            }
        }

        /// <summary>
        ///     Adds item to chest.
        /// </summary>
        /// <param name="item"></param>
        public void Add(Item item)
        {
            lock (_items)
            {
                _items.Add(item);
            }
        }

        /// <summary>
        ///     Adds gold stacks based on amount to chest.
        /// </summary>
        /// <param name="amount"></param>
        public void AddGold(int amount)
        {
            while (amount > 0)
            {
                var n = Math.Min(1000, amount);
                amount -= n;

                var gold = Item.CreateGold(n);
                Add(gold);
            }
        }
    }

    public class LockedChest : Chest
    {
        public LockedChest(int propId, string name, string lockName)
            : base(propId, name)
        {
            LockName = lockName;
            State = "closed";
            Extensions.AddSilent(new ConfirmationPropExtension("", Localization.Get("Do you wish to open this chest?"),
                null, "haskey(" + lockName + ")"));
        }

        public LockedChest(Puzzle puzzle, string name, string key)
            : this(puzzle.Dungeon.Data.ChestId, name, key)
        {
        }

        public string LockName { get; protected set; }

        protected override void DefaultBehavior(Creature creature, Prop prop)
        {
            // Make sure the chest was still closed when it was clicked.
            // No security violation because it could be caused by lag.
            if (prop.State == "open")
                return;

            // Check key
            var key = creature.Inventory.GetItem(a =>
                a.Info.Id == 70028 && a.MetaData1.GetString("prop_to_unlock") == LockName);
            if (key == null)
            {
                Send.Notice(creature, Localization.Get("There is no matching key."));
                return;
            }

            // Remove key
            creature.Inventory.Remove(key);

            // Open and drop
            prop.SetState("open");
            DropItems(creature);
        }
    }

    public class TreasureChest : LockedChest
    {
        public TreasureChest()
            : base(10201, "TreasureChest", "chest")
        {
            State = "closed_identified";
        }
    }
}