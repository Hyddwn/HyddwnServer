// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Channel.Network.Sending;
using Aura.Channel.World.Entities;
using Aura.Channel.World.Entities.Props;
using Aura.Mabi;
using Aura.Mabi.Const;
using Aura.Shared.Util;

namespace Aura.Channel.World.Dungeons.Props
{
    /// <summary>
    ///     A door, as found in dungeons.
    /// </summary>
    public class Door : Prop
    {
        /// <summary>
        ///     Position of a room, from which this door was closed.
        /// </summary>
        private Position _closedFrom;

        /// <summary>
        ///     Is this door used for switch room (won't display notice about a key).
        /// </summary>
        private bool _isSwitchDoor;

        /// <summary>
        ///     Creates new door prop.
        /// </summary>
        /// <param name="propId"></param>
        /// <param name="regionId"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="direction">Direction the door faces in, in degree.</param>
        /// <param name="doorType"></param>
        /// <param name="name"></param>
        /// <param name="state"></param>
        public Door(int propId, int regionId, int x, int y, int direction, DungeonBlockType doorType, string name,
            string state = "open")
            : base(propId, regionId, x, y, direction, 1, 0, state, "", "")
        {
            Name = name;
            DoorType = doorType;
            BlockBoss = false;
            Behavior = DefaultBehavior;
            _isSwitchDoor = false;
            _closedFrom = new Position(x / Dungeon.TileSize, y / Dungeon.TileSize);

            // Set direction and adjust Y for boss doors
            if (doorType == DungeonBlockType.BossDoor)
            {
                Info.Direction = MabiMath.DirectionToRadian(0, 1);
                Info.Y += Dungeon.TileSize + Dungeon.TileSize / 2;
            }
            else
            {
                Info.Direction = MabiMath.DegreeToRadian(direction);
            }
        }

        /// <summary>
        ///     Name of the door, used from puzzles.
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        ///     Type of the door.
        /// </summary>
        public DungeonBlockType DoorType { get; protected set; }

        /// <summary>
        ///     Specifies whether the door is locked or not.
        /// </summary>
        public bool IsLocked { get; set; }

        /// <summary>
        ///     Boss door stays closed when unlocked.
        /// </summary>
        public bool BlockBoss { get; set; }

        /// <summary>
        ///     Door's behavior.
        /// </summary>
        /// <param name="creature"></param>
        /// <param name="prop"></param>
        private void DefaultBehavior(Creature creature, Prop prop)
        {
            // Open doors can't be interacted with
            if (State == "open")
                return;

            // If it's unlocked, warp inside
            if (!IsLocked)
            {
                WarpInside(creature, prop);
                return;
            }

            // Check if it's switch room door
            if (_isSwitchDoor)
                return;

            // Check dungeon doors for boss room doors
            // TODO: Mixing normal and boss doors like this seems wrong.
            if (DoorType == DungeonBlockType.BossDoor)
            {
                var dungeonRegion = Region as DungeonRegion;
                if (dungeonRegion != null)
                    if (!dungeonRegion.Dungeon.CheckDoors())
                    {
                        if (!creature.IsDev)
                        {
                            Send.Notice(creature,
                                Localization.Get(
                                    "Unable to enter the boss room. There must be a closed door somewhere in the dungeon."));
                            return;
                        }

                        Send.Notice(creature, NoticeType.MiddleSystem,
                            Localization.Get("Bypassed dungeon door check."));
                    }
            }

            // Check if character has the key
            if (!RemoveKey(creature))
            {
                Send.Notice(creature, NoticeType.MiddleSystem, Localization.Get("There is no matching key."));
                return;
            }

            // Unlock the door, but don't open it if it's supposed to block the bosses
            if (BlockBoss)
            {
                if (State != "unlocked")
                {
                    SetState("unlocked");
                    var wasLocked = IsLocked;
                    IsLocked = false;
                    _closedFrom =
                        new Position(_closedFrom.X, _closedFrom.Y + 1); // Fix closed from to be inside boss room.
                    AddConfirmation();

                    // Check sections if door was unlocked
                    // This has to be done here and in Open because some
                    // doors are opened without being touched.
                    if (wasLocked && !IsLocked)
                    {
                        var dungeonRegion = Region as DungeonRegion;
                        if (dungeonRegion != null)
                            dungeonRegion.Dungeon.CheckSectionClear();
                    }
                }
                WarpInside(creature, prop);
            }
            else
            {
                Open();
            }

            Send.Notice(creature, NoticeType.MiddleSystem, Localization.Get("You have opened the door with the key."));
        }

        /// <summary>
        ///     Door's behavior when it's not locked.
        /// </summary>
        /// <param name="creature"></param>
        /// <param name="prop"></param>
        private void WarpInside(Creature creature, Prop prop)
        {
            var creaturePos = creature.GetPosition();
            var cCoord = new Position(creaturePos.X / Dungeon.TileSize, creaturePos.Y / Dungeon.TileSize);

            if (cCoord == _closedFrom)
            {
                Send.Notice(creature, NoticeType.MiddleSystem,
                    Localization.Get(
                        "There is a monster still standing.\nYou must defeat all the monsters for the door to open."));
                return;
            }

            var x = _closedFrom.X * Dungeon.TileSize + Dungeon.TileSize / 2;
            var y = _closedFrom.Y * Dungeon.TileSize + Dungeon.TileSize / 2;

            if (cCoord.X < _closedFrom.X)
                x -= 1000;
            else if (cCoord.X > _closedFrom.X)
                x += 1000;
            else if (cCoord.Y < _closedFrom.Y)
                y -= 1000;
            else if (cCoord.Y > _closedFrom.Y)
                y += 1000;

            creature.SetPosition(x, y);
            Send.SetLocation(creature, x, y);
        }

        /// <summary>
        ///     Returns true if character has the key to unlock this door,
        ///     and removes it from his inventory.
        /// </summary>
        /// <param name="character"></param>
        /// <returns></returns>
        private bool RemoveKey(Creature character)
        {
            // Check key
            var key = character.Inventory.GetItem(item =>
                (item.Info.Id == 70029 || item.Info.Id == 70030) && item.MetaData1.GetString("prop_to_unlock") == Name);
            if (key == null)
                return false;

            // Remove key
            character.Inventory.Remove(key);
            return true;
        }

        /// <summary>
        ///     Closes door.
        /// </summary>
        public void Close(int x, int y)
        {
            _closedFrom = new Position(x, y);
            AddConfirmation();
            SetState("closed");
        }

        /// <summary>
        ///     Opens door.
        /// </summary>
        public void Open()
        {
            if (!IsLocked)
                Extensions.Clear();

            var wasLocked = IsLocked;
            IsLocked = false;
            SetState("open");

            // Check sections if door was unlocked
            if (wasLocked && !IsLocked)
            {
                var dungeonRegion = Region as DungeonRegion;
                if (dungeonRegion != null)
                    dungeonRegion.Dungeon.CheckSectionClear();
            }
        }

        /// <summary>
        ///     Locks the door.
        /// </summary>
        /// <param name="isSwitchDoor"></param>
        public void Lock(bool isSwitchDoor = false)
        {
            _isSwitchDoor = isSwitchDoor;
            IsLocked = true;
            SetState("closed");
        }

        /// <summary>
        ///     Adds confirmation to get into the room.
        /// </summary>
        public void AddConfirmation()
        {
            var extname = string.Format("directed_ask({0},{1})", _closedFrom.X, _closedFrom.Y);
            var condition = string.Format("notfrom({0},{1})", _closedFrom.X, _closedFrom.Y);
            var ext = new ConfirmationPropExtension(extname, Localization.Get("Do you wish to go inside the room?"),
                condition: condition);
            Extensions.Add(ext);
        }
    }
}