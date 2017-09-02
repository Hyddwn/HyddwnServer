// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using Aura.Channel.World.Dungeons.Props;
using Aura.Mabi.Const;

namespace Aura.Channel.World.Dungeons.Generation
{
    public class RoomTrait
    {
        /// <summary>
        ///     This room is locked, don't try to put UnlockPlace in here
        /// </summary>
        public bool isLocked;

        /// <summary>
        ///     Is this room should be walked though to get puzzle done
        /// </summary>
        public bool isOnPath;

        /// <summary>
        ///     Is this room reserved for a puzzle
        /// </summary>
        public bool isReserved;

        /// <summary>
        ///     Index of this room in DungeonFloorSection.Places list
        /// </summary>
        public int RoomIndex;

        //public int ShapeType { get; private set; }
        //public int ShapeRotationCount { get; private set; }

        public RoomTrait(int x, int y)
        {
            Neighbor = new RoomTrait[4];
            Links = new[] {LinkType.None, LinkType.None, LinkType.None, LinkType.None};
            DoorType = new[] {0, 0, 0, 0};

            X = x;
            Y = y;

            isOnPath = false;
            isReserved = false;
            PuzzleDoors = new Door[] {null, null, null, null};
            ReservedDoor = new[] {false, false, false, false};
            RoomType = RoomType.None;
            RoomIndex = -1;
        }

        public RoomTrait[] Neighbor { get; }

        /// <summary>
        ///     Doors for puzzles
        /// </summary>
        public Door[] PuzzleDoors { get; }

        /// <summary>
        ///     Is door in direction reserved for a puzzle
        /// </summary>
        public bool[] ReservedDoor { get; }

        /// <summary>
        ///     Paths
        /// </summary>
        public LinkType[] Links { get; }

        /// <summary>
        ///     Types of the room's doors (up/down).
        /// </summary>
        public int[] DoorType { get; }

        public RoomType RoomType { get; set; }

        public int X { get; }
        public int Y { get; }

        public void SetNeighbor(int direction, RoomTrait room)
        {
            Neighbor[direction] = room;
        }

        public bool IsLinked(int direction)
        {
            if (direction > 3)
                throw new ArgumentException("Direction out of bounds.");

            return Links[direction] != LinkType.None;
        }

        public int GetDoorType(int direction)
        {
            if (direction > 3)
                throw new ArgumentException("Direction out of bounds.");

            return DoorType[direction];
        }

        public void SetPuzzleDoor(Door door, int direction)
        {
            PuzzleDoors[direction] = door;

            var opposite_direction = Direction.GetOppositeDirection(direction);

            var room = Neighbor[direction];
            if (room != null)
                room.PuzzleDoors[opposite_direction] = door;
        }

        public Door GetPuzzleDoor(int direction)
        {
            return PuzzleDoors[direction];
        }

        public void ReserveDoors()
        {
            if (RoomType != RoomType.End || RoomType != RoomType.Start)
                RoomType = RoomType.Room;
            for (var dir = 0; dir < 4; ++dir)
            {
                ReserveDoor(dir);
                var doorType = GetDoorType(dir);
                if (doorType != (int) DungeonBlockType.BossDoor && doorType != (int) DungeonBlockType.Door)
                    SetDoorType(dir, (int) DungeonBlockType.Door);
            }
        }

        public void ReserveDoor(int direction)
        {
            if (direction > 3)
                throw new ArgumentException("Direction out of bounds.");

            ReservedDoor[direction] = true;

            var opposite_direction = Direction.GetOppositeDirection(direction);

            var room = Neighbor[direction];
            if (room != null)
                room.ReservedDoor[opposite_direction] = true;
        }

        public void Link(int direction, LinkType linkType)
        {
            if (direction > 3)
                throw new ArgumentException("Direction out of bounds.");

            Links[direction] = linkType;

            if (Neighbor[direction] != null)
            {
                var opposite_direction = Direction.GetOppositeDirection(direction);
                if (linkType == LinkType.From)
                    Neighbor[direction].Links[opposite_direction] = LinkType.To;
                else if (linkType == LinkType.To)
                    Neighbor[direction].Links[opposite_direction] = LinkType.From;
                else
                    Neighbor[direction].Links[opposite_direction] = LinkType.None;
            }
        }

        public void SetDoorType(int direction, int doorType)
        {
            if (direction > 3)
                throw new ArgumentException("Direction out of bounds.");

            DoorType[direction] = doorType;

            var opposite_direction = Direction.GetOppositeDirection(direction);

            var room = Neighbor[direction];
            if (room != null)
                room.DoorType[opposite_direction] = doorType;
        }

        public int GetIncomingDirection()
        {
            for (var dir = 0; dir < 4; ++dir)
                if (Links[dir] == LinkType.From) return dir;
            return 0;
        }
    }

    public enum RoomType
    {
        None,
        Alley,
        Start,
        End,
        Room
    }

    public enum LinkType
    {
        None,
        From,
        To
    }
}