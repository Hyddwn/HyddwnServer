// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using Aura.Channel.World.Dungeons.Generation;
using Aura.Channel.World.Dungeons.Props;
using Aura.Channel.World.Entities;
using Aura.Data.Database;
using Aura.Mabi.Const;

namespace Aura.Channel.World.Dungeons.Puzzles
{
    /// <summary>
    ///     Place on a tile in a dungeon that contains a part of a puzzle.
    /// </summary>
    public class PuzzlePlace
    {
        private readonly string _name;
        private readonly Dictionary<Placement, PlacementProvider> _placementProviders;
        private RoomTrait _room;
        private readonly DungeonFloorSection _section;

        /// <summary>
        ///     Creates new puzzle place.
        /// </summary>
        /// <param name="section"></param>
        /// <param name="puzzle"></param>
        /// <param name="name"></param>
        public PuzzlePlace(DungeonFloorSection section, Puzzle puzzle, string name)
        {
            _placementProviders = new Dictionary<Placement, PlacementProvider>();
            Doors = new Door[] {null, null, null, null};

            _section = section;
            _name = name;
            PlaceIndex = -1;
            Puzzle = puzzle;
        }

        /// <summary>
        ///     Index of this place in DungeonFloorSection.Places list
        /// </summary>
        public int PlaceIndex { get; private set; }

        /// <summary>
        ///     X world coordinate of this place.
        /// </summary>
        public int X { get; private set; }

        /// <summary>
        ///     Y world coordinate of this place.
        /// </summary>
        public int Y { get; private set; }

        /// <summary>
        ///     Direction in which the place is locked.
        /// </summary>
        public int DoorDirection { get; private set; }

        /// <summary>
        ///     Returns true if place contains a locked door.
        /// </summary>
        public bool IsLock { get; private set; }

        /// <summary>
        ///     Color of the lock.
        /// </summary>
        public uint LockColor { get; private set; }

        /// <summary>
        ///     Returns true if place contains measures to unlock a place.
        /// </summary>
        public bool IsUnlock { get; private set; }

        /// <summary>
        ///     Returns true if this place contains the boss lock.
        /// </summary>
        public bool IsBossLock { get; private set; }

        /// <summary>
        ///     The key this place's door is locked with.
        /// </summary>
        public Item Key { get; private set; }

        /// <summary>
        ///     Doors between this place, alleys, and other rooms.
        /// </summary>
        public Door[] Doors { get; }

        /// <summary>
        ///     The puzzle this place is a part of.
        /// </summary>
        public Puzzle Puzzle { get; }

        /// <summary>
        ///     Updates world position of place.
        /// </summary>
        private void UpdatePosition()
        {
            X = _room.X * Dungeon.TileSize + Dungeon.TileSize / 2;
            Y = _room.Y * Dungeon.TileSize + Dungeon.TileSize / 2;
        }

        /// <summary>
        ///     Adds door to place.
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="doorType"></param>
        private void AddDoor(int direction, DungeonBlockType doorType)
        {
            var door = _room.GetPuzzleDoor(direction);
            // We'll create new door and replace if we want locked door instead of normal one here
            if (door != null && doorType == DungeonBlockType.Door)
            {
                Doors[direction] = door;
                return;
            }

            // Create new door
            var floorData = Puzzle.FloorData;
            var doorBlock = Puzzle.Dungeon.Data.Style.Get(doorType, direction);
            var doorName = string.Format("{0}_door_{1}{2}_{3}", _name, X, Y, direction);

            door = new Door(doorBlock.PropId, 0, X, Y, doorBlock.Rotation, doorType, doorName);
            door.Info.Color1 = floorData.Color1;
            door.Info.Color2 = floorData.Color2;
            door.Info.Color3 = LockColor;

            if (doorType == DungeonBlockType.BossDoor)
            {
                if (Puzzle.Dungeon.Data.BlockBoss)
                    door.BlockBoss = true;
                door.Behavior += Puzzle.Dungeon.BossDoorBehavior;
            }
            door.Behavior += Puzzle.PuzzleEvent;

            Doors[direction] = door;
            Puzzle.Props[doorName] = door;
            _room.SetPuzzleDoor(door, direction);
            _room.SetDoorType(direction, (int) doorType);
        }

        /// <summary>
        ///     Adds prop to place.
        /// </summary>
        /// <param name="prop"></param>
        /// <param name="positionType"></param>
        public void AddProp(DungeonProp prop, Placement positionType)
        {
            Puzzle.AddProp(this, prop, positionType);
        }

        /// <summary>
        ///     Makes it a locked place with a locked door.
        /// </summary>
        public void DeclareLock(bool lockSelf = false)
        {
            var doorElement = _section.GetLock(lockSelf);
            if (doorElement == null)
                return;

            PlaceIndex = doorElement.PlaceIndex;
            _room = doorElement.Room;
            UpdatePosition();
            IsLock = true;
            LockColor = _section.GetLockColor();
            DoorDirection = doorElement.Direction;

            _room.ReserveDoor(DoorDirection);
            _room.isLocked = true;
            if (_room.RoomType != RoomType.End || _room.RoomType != RoomType.Start)
                _room.RoomType = RoomType.Room;

            // Boss door - special case
            if ((DungeonBlockType) _room.DoorType[DoorDirection] == DungeonBlockType.BossDoor)
            {
                IsBossLock = true;
                AddDoor(DoorDirection, DungeonBlockType.BossDoor);
            }
            else
            {
                AddDoor(DoorDirection, DungeonBlockType.DoorWithLock);
            }

            if (lockSelf)
                GetLockDoor().IsLocked = true;
        }

        /// <summary>
        ///     Makes it a locked place with a locked door that doesn't need an unlock place.
        /// </summary>
        public void DeclareLockSelf()
        {
            DeclareLock(true);
        }

        /// <summary>
        ///     This place will precede locked place and contain some means to unlock it.
        /// </summary>
        /// <param name="lockPlace"></param>
        public void DeclareUnlock(PuzzlePlace lockPlace)
        {
            var place = lockPlace;
            if (place == null || place.PlaceIndex == -1)
                throw new PuzzleException("We can't declare unlock");

            PlaceIndex = _section.GetUnlock(place);
            _room = _section.Places[PlaceIndex].Room;
            IsUnlock = true;

            UpdatePosition();
        }

        /// <summary>
        ///     Declares that this place is not to be used by any other puzzles.
        ///     If we didn't declare this place to be something, reserve random place.
        /// </summary>
        public void ReservePlace()
        {
            if (IsUnlock || IsLock || IsBossLock)
            {
                _section.ReservePlace(PlaceIndex);
            }
            else
            {
                PlaceIndex = _section.ReservePlace();
                _room = _section.Places[PlaceIndex].Room;

                UpdatePosition();
            }
        }

        /// <summary>
        ///     Declares this place to be a room.
        ///     Doors of this place won't be locked with a key.
        /// </summary>
        public void ReserveDoors()
        {
            _room.ReserveDoors();

            _section.CleanLockedDoorCandidates();

            for (var dir = 0; dir < 4; ++dir)
                if (_room.Links[dir] == LinkType.From || _room.Links[dir] == LinkType.To)
                    if ((DungeonBlockType) _room.DoorType[dir] != DungeonBlockType.BossDoor)
                        AddDoor(dir, DungeonBlockType.Door);

            OpenAllDoors();
        }

        /// <summary>
        ///     Closes all doors at this place.
        /// </summary>
        public void CloseAllDoors()
        {
            foreach (var door in Doors)
                if (door != null)
                    door.Close(_room.X, _room.Y);
        }

        /// <summary>
        ///     Opens all doors at this place.
        /// </summary>
        public void OpenAllDoors()
        {
            foreach (var door in Doors)
                if (door != null)
                    door.Open();
        }

        /// <summary>
        ///     Locks this place with the given key.
        /// </summary>
        /// <param name="key"></param>
        public void LockPlace(Item key)
        {
            var door = GetLockDoor();
            door.Lock();
            Key = key;
        }

        /// <summary>
        ///     Locks this place.
        /// </summary>
        public void LockPlace()
        {
            GetLockDoor().Lock(true);
        }

        /// <summary>
        ///     Returns locked door of this place.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="PuzzleException">Thrown if there is no lock or no door.</exception>
        public Door GetLockDoor()
        {
            var door = Doors[DoorDirection];

            if (!IsLock)
                throw new PuzzleException("Place isn't a lock.");
            if (door == null)
                throw new PuzzleException("No door found.");

            return door;
        }

        /// <summary>
        ///     Opens locked place.
        /// </summary>
        public void Open()
        {
            if (!IsLock)
                return;

            Doors[DoorDirection].IsLocked = false;
            Doors[DoorDirection].Open();

            if (_room.DoorType[DoorDirection] == (int) DungeonBlockType.BossDoor)
                Puzzle.Dungeon.BossDoorBehavior(null, Doors[DoorDirection]);
        }

        /// <summary>
        ///     Returns position and direction for placement.
        /// </summary>
        /// <param name="placement"></param>
        /// <param name="border"></param>
        /// <returns>3 values, X, Y, and Direction (in degree).</returns>
        public int[] GetPosition(Placement placement, int border = -1)
        {
            if (PlaceIndex == -1)
                throw new PuzzleException("Place hasn't been declared anything or it wasn't reserved.");

            // todo: check those values
            var radius = 0;
            if (border >= 0)
            {
                radius = _room.RoomType == RoomType.Alley ? 200 - border : 800 - border;
                if (radius < 0)
                    radius = 0;
            }
            else
            {
                radius = _room.RoomType == RoomType.Alley ? 200 : 800;
            }

            if (!_placementProviders.ContainsKey(placement))
                _placementProviders[placement] = new PlacementProvider(placement, radius);

            var pos = _placementProviders[placement].GetPosition();
            if (pos == null)
            {
                if (!_placementProviders.ContainsKey(Placement.Random))
                    _placementProviders[Placement.Random] = new PlacementProvider(Placement.Random, radius);
                pos = _placementProviders[Placement.Random].GetPosition();
            }

            pos[0] += X;
            pos[1] += Y;

            return pos;
        }

        /// <summary>
        ///     Returns the room's position.
        /// </summary>
        /// <returns></returns>
        public Position GetRoomPosition()
        {
            return new Position(_room.X, _room.Y);
        }

        /// <summary>
        ///     Returns the position of the place in world coordinates.
        /// </summary>
        /// <returns></returns>
        public Position GetWorldPosition()
        {
            return new Position(X, Y);
        }

        /// <summary>
        ///     Creates mob in puzzle, in this place.
        /// </summary>
        /// <param name="mobGroupName">Name of the mob, for reference.</param>
        /// <param name="mobToSpawn">Mob to spawn (Mob1-3), leave as null for auto select.</param>
        /// <param name="placement"></param>
        public void SpawnSingleMob(string mobGroupName, string mobToSpawn = null,
            Placement placement = Placement.Random)
        {
            DungeonMonsterGroupData data;
            if (mobToSpawn == null)
                data = Puzzle.GetMonsterData("Mob3") ?? Puzzle.GetMonsterData("Mob2") ?? Puzzle.GetMonsterData("Mob1");
            else
                data = Puzzle.GetMonsterData(mobToSpawn);

            if (data == null)
                throw new Exception("No monster data found.");

            Puzzle.AllocateAndSpawnMob(this, mobGroupName, data, placement);
        }

        /// <summary>
        ///     Creates mob in puzzle, in place.
        /// </summary>
        /// <param name="mobGroupName">Name of the mob, for reference.</param>
        /// <param name="raceId">Race to spawn.</param>
        /// <param name="amount">Number of monsters to spawn.</param>
        /// <param name="placement"></param>
        public void SpawnSingleMob(string mobGroupName, int raceId, int amount, Placement placement = Placement.Random)
        {
            if (amount < 1)
                amount = 1;

            var group = new DungeonMonsterGroupData();
            group.Add(new DungeonMonsterData {RaceId = raceId, Amount = amount});

            Puzzle.AllocateAndSpawnMob(this, mobGroupName, group, placement);
        }
    }
}