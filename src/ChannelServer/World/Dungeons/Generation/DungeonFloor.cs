// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Linq;
using Aura.Data.Database;
using Aura.Mabi.Const;

namespace Aura.Channel.World.Dungeons.Generation
{
    public class DungeonFloor
    {
        private readonly int _branchProbability;
        private readonly int _coverageFactor;
        private readonly DungeonGenerator _dungeonGenerator;

        private readonly Position _pos;

        private readonly DungeonFloor _prevFloor;
        //private DungeonFloor next_floor_structure;

        private List<List<RoomTrait>> _rooms;
        private int _startDirection;
        private Position _startPos;

        public DungeonFloor(DungeonGenerator dungeonGenerator, DungeonFloorData floorData, bool isLastFloor,
            DungeonFloor prevFloor)
        {
            _pos = new Position(0, 0);
            _startPos = new Position(0, 0);
            _startDirection = Direction.Down;
            Width = 1;
            Height = 1;
            MazeGenerator = new MazeGenerator();
            Sections = new List<DungeonFloorSection>();

            _dungeonGenerator = dungeonGenerator;
            _branchProbability = floorData.Branch;
            _coverageFactor = floorData.Coverage;
            IsLastFloor = isLastFloor;
            _prevFloor = prevFloor;

            HasBossRoom = floorData.HasBoss;
            Statue = floorData.Statue;

            CalculateSize(floorData);
            InitRoomtraits();
            GenerateMaze(floorData);
            GenerateRooms(floorData);
            InitSections(floorData);
        }

        public List<DungeonFloorSection> Sections { get; }

        public bool HasBossRoom { get; }
        public bool Statue { get; }
        public MazeGenerator MazeGenerator { get; private set; }
        public int Width { get; private set; }
        public int Height { get; private set; }
        public bool IsLastFloor { get; }

        /// <summary>
        ///     Split this floor into sections.
        /// </summary>
        /// <param name="floorData"></param>
        private void InitSections(DungeonFloorData floorData)
        {
            var criticalPathLength = MazeGenerator.CriticalPath.Count - 1;
            var criticalPathLeft = criticalPathLength;
            var sectionCount = floorData.Sections.Count;
            if (sectionCount == 0) return;
            var sectionStart = 0;
            var puzzleCountSum = 0f;
            var weightsList = CalculateWeights();
            //Log.Debug("Floor weightsList: " + string.Join(",", weightsList));
            var pathWeight = weightsList.Sum();
            floorData.Sections.ForEach(x => puzzleCountSum += x.Max);
            for (var i = 0; i < sectionCount; ++i)
            {
                List<MazeMove> sectionPath;
                var haveBossDoor = false;
                var sectionLength = (int) Math.Round(floorData.Sections[i].Max / puzzleCountSum * pathWeight);
                var sectionEnd = sectionStart;
                var currentWeight = 0;
                for (; sectionEnd < weightsList.Length; ++sectionEnd)
                {
                    currentWeight += weightsList[sectionEnd];
                    if (currentWeight >= sectionLength)
                        break;
                }
                if (currentWeight > sectionLength)
                    if (currentWeight - weightsList[sectionEnd] >=
                        (int) Math.Round(floorData.Sections[i].Max / puzzleCountSum * criticalPathLeft))
                    {
                        currentWeight -= weightsList[sectionEnd];
                        --sectionEnd;
                    }

                pathWeight -= currentWeight;
                criticalPathLeft -= sectionEnd - sectionStart + 1;
                puzzleCountSum -= floorData.Sections[i].Max;

                // if last section
                if (i == sectionCount - 1)
                {
                    sectionPath =
                        MazeGenerator.CriticalPath.GetRange(sectionStart + 1, criticalPathLength - sectionStart);
                    haveBossDoor = HasBossRoom;
                }
                else
                {
                    sectionPath = MazeGenerator.CriticalPath.GetRange(sectionStart + 1, sectionEnd - sectionStart + 1);
                }
                Sections.Add(new DungeonFloorSection(GetRoom(sectionPath[0].PosFrom), sectionPath, haveBossDoor,
                    _dungeonGenerator.RngPuzzles));
                var weightsListSegment = i == sectionCount - 1
                    ? new ArraySegment<int>(weightsList, sectionStart, criticalPathLength - sectionStart)
                    : new ArraySegment<int>(weightsList, sectionStart, sectionEnd - sectionStart + 1);
                //Log.Debug("section weightsList: " + string.Join(",", weightsListSegment));
                //Log.Debug(string.Format("section {0}: max puzzles: {1}, wanted length: {2}, length: {3}", i, floorData.Sections[i].Max, sectionLength, weightsListSegment.Sum()));
                sectionStart = sectionEnd + 1;
            }
        }

        /// <summary>
        ///     InitSections helper method.
        ///     Walks critical path and for each room calculates number of subpath rooms.
        /// </summary>
        /// <returns></returns>
        private int[] CalculateWeights()
        {
            var criticalPathLength = MazeGenerator.CriticalPath.Count - 1;
            var weightsList = new int[criticalPathLength];

            for (var i = 0; i < criticalPathLength; ++i)
            {
                var move = MazeGenerator.CriticalPath[i + 1];
                var room = GetRoom(move.PosFrom);
                weightsList[i] = 1;
                for (var direction = 0; direction < 4; direction++)
                    if (move.Direction != direction && room.Links[direction] == LinkType.To)
                    {
                        var nextRoom = room.Neighbor[direction];
                        if (nextRoom != null)
                            weightsList[i] += CalculateSubPathWeightRecursive(nextRoom, 1);
                    }
            }
            return weightsList;
        }

        /// <summary>
        ///     CalculateWeights helper method.
        /// </summary>
        /// <param name="room"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        private int CalculateSubPathWeightRecursive(RoomTrait room, int count)
        {
            for (var direction = 0; direction < 4; direction++)
            {
                if (room.Links[direction] != LinkType.To) continue;
                var nextRoom = room.Neighbor[direction];
                if (nextRoom != null)
                    count = CalculateSubPathWeightRecursive(nextRoom, count + 1);
            }
            return count;
        }

        private void GenerateRooms(DungeonFloorData floorData)
        {
            if (HasBossRoom)
            {
                var endPos = MazeGenerator.EndPos;

                var preEndRoom = GetRoom(endPos.GetBiasedPosition(Direction.Down));
                preEndRoom.RoomType = RoomType.Room;
                preEndRoom.SetDoorType(Direction.Up, (int) DungeonBlockType.BossDoor);
            }
        }

        private void CalculateSize(DungeonFloorData floorData)
        {
            var width = floorData.Width;
            var height = floorData.Height;

            if (floorData.Width < 6)
                width = 6;
            else if (floorData.Width > 18)
                width = 18;

            if (floorData.Height < 6)
                height = 6;
            else if (floorData.Height > 18)
                height = 18;

            var rndNum = _dungeonGenerator.RngMaze.GetUInt32();
            Width = (int) (width - rndNum % (int) (width / 5.0));

            rndNum = _dungeonGenerator.RngMaze.GetUInt32();
            Height = (int) (height - rndNum % (int) (height / 5.0));
        }

        private void InitRoomtraits()
        {
            _rooms = new List<List<RoomTrait>>();
            for (var x = 0; x < Width; x++)
            {
                var row = new List<RoomTrait>();
                for (var y = 0; y < Height; y++)
                    row.Add(new RoomTrait(x, y));

                _rooms.Add(row);
            }

            for (var y = 0; y < Height; y++)
            for (var x = 0; x < Width; x++)
            for (var direction = 0; direction < 4; direction++)
            {
                var biased_pos = new Position(x, y).GetBiasedPosition(direction);
                if (biased_pos.X >= 0 && biased_pos.Y >= 0)
                    if (biased_pos.X < Width && biased_pos.Y < Height)
                        _rooms[x][y].SetNeighbor(direction, _rooms[biased_pos.X][biased_pos.Y]);
            }
        }

        public List<RoomTrait> GetRooms()
        {
            var result = new List<RoomTrait>();

            for (var x = 0; x < Width; x++)
            for (var y = 0; y < Height; y++)
            {
                var room = GetRoom(x, y);
                if (room.RoomType == RoomType.Room)
                    result.Add(room);
            }

            return result;
        }

        public RoomTrait GetRoom(Position pos)
        {
            return GetRoom(pos.X, pos.Y);
        }

        public RoomTrait GetRoom(int x, int y)
        {
            if (x < 0 || y < 0 || x >= Width || y >= Height)
                throw new ArgumentException("Position out of bounds.");

            return _rooms[x][y];
        }

        private bool SetTraits(Position pos, int direction, int doorType)
        {
            var biased_pos = pos.GetBiasedPosition(direction);
            if (biased_pos.X >= 0 && biased_pos.Y >= 0)
                if (biased_pos.X < Width && biased_pos.Y < Height)
                {
                    if (!MazeGenerator.IsFree(biased_pos))
                        return false;

                    MazeGenerator.MarkReservedPosition(biased_pos);
                }

            var room = GetRoom(pos);
            if (room.IsLinked(direction))
                throw new Exception("Room in direction isn't linked");

            if (room.GetDoorType(direction) != 0)
                throw new Exception();

            LinkType linkType;
            if (doorType == (int) DungeonBlockType.StairsDown)
                linkType = LinkType.To;
            else if (doorType == (int) DungeonBlockType.StairsUp)
                linkType = LinkType.From;
            else
                throw new Exception("Invalid door_type");

            room.Link(direction, linkType);
            room.SetDoorType(direction, doorType);

            return true;
        }

        private void GenerateMaze(DungeonFloorData floorDesc)
        {
            var critPathMin = Math.Max(1, floorDesc.CritPathMin);
            var critPathMax = Math.Max(1, floorDesc.CritPathMax);

            if (critPathMin > critPathMax)
            {
                var temp = critPathMax;
                critPathMax = critPathMin;
                critPathMin = temp;
            }

            CreateCriticalPath(critPathMin, critPathMax);
            CreateSubPath(_coverageFactor, _branchProbability);

            SetRoomTypes();
        }

        private void SetRoomTypes()
        {
            for (var y = 0; y < MazeGenerator.Height; ++y)
            for (var x = 0; x < MazeGenerator.Width; ++x)
            {
                var pos = new Position(x, y);
                var room = MazeGenerator.GetRoom(pos);
                var roomTrait = GetRoom(pos);

                if (MazeGenerator.StartPos.X == x && MazeGenerator.StartPos.Y == y)
                    roomTrait.RoomType = RoomType.Start;
                else if (MazeGenerator.EndPos.X == x && MazeGenerator.EndPos.Y == y)
                    roomTrait.RoomType = RoomType.End;
                else if (room.Visited)
                    roomTrait.RoomType = RoomType.Alley;
            }
        }

        private List<MazeMove> CreateCriticalPath(int crit_path_min, int crit_path_max)
        {
            while (true)
            {
                MazeGenerator.SetSize(Width, Height);
                SetRandomPathPosition();

                if (MazeGenerator.GenerateCriticalPath(_dungeonGenerator.RngMaze, crit_path_min, crit_path_max))
                {
                    _startPos = MazeGenerator.StartPos;
                    if (SetTraits(_startPos, MazeGenerator.StartDirection, (int) DungeonBlockType.StairsUp))
                        break;
                }

                MazeGenerator = new MazeGenerator();
                InitRoomtraits();
            }

            return MazeGenerator.CriticalPath;
        }

        private bool CreateSubPath(int coverageFactor, int branchProbability)
        {
            MazeGenerator.GenerateSubPath(_dungeonGenerator.RngMaze, coverageFactor, branchProbability);
            return CreateSubPathRecursive(_startPos);
        }

        private bool CreateSubPathRecursive(Position pos)
        {
            var room = GetRoom(pos);
            var maze_room = MazeGenerator.GetRoom(pos);

            for (var direction = 0; direction < 4; direction++)
                if (maze_room.GetPassageType(direction) == 2)
                {
                    var biased_pos = pos.GetBiasedPosition(direction);
                    if (room != null)
                        room.Link(direction, LinkType.To);

                    CreateSubPathRecursive(biased_pos);
                }
            return true;
        }

        private void SetRandomPathPosition()
        {
            MazeGenerator.StartDirection = _startDirection = _prevFloor == null
                ? Direction.Down
                : Direction.GetOppositeDirection(_prevFloor._startDirection);

            var mt = _dungeonGenerator.RngMaze;
            if (HasBossRoom)
            {
                if (_dungeonGenerator.Option.Contains("largebossroom=" + '"' + "true")
                ) // <option largebossroom="true" />
                {
                    while (true)
                    {
                        _pos.X = (int) (mt.GetUInt32() % (Width - 2) + 1);
                        _pos.Y = (int) (mt.GetUInt32() % (Height - 3) + 1);
                        if (MazeGenerator.IsFree(_pos))
                            if (MazeGenerator.IsFree(new Position(_pos.X - 1, _pos.Y)))
                                if (MazeGenerator.IsFree(new Position(_pos.X + 1, _pos.Y)))
                                    if (MazeGenerator.IsFree(new Position(_pos.X, _pos.Y + 1)))
                                        if (MazeGenerator.IsFree(new Position(_pos.X - 1, _pos.Y + 1)))
                                            if (MazeGenerator.IsFree(new Position(_pos.X + 1, _pos.Y + 1)))
                                                if (MazeGenerator.IsFree(new Position(_pos.X, _pos.Y + 2)))
                                                    if (MazeGenerator.IsFree(new Position(_pos.X - 1, _pos.Y + 2)))
                                                        if (MazeGenerator.IsFree(new Position(_pos.X + 1, _pos.Y + 2)))
                                                            break;
                    }

                    MazeGenerator.MarkReservedPosition(new Position(_pos.X - 1, _pos.Y));
                    MazeGenerator.MarkReservedPosition(new Position(_pos.X + 1, _pos.Y));
                    MazeGenerator.MarkReservedPosition(new Position(_pos.X, _pos.Y + 1));
                    MazeGenerator.MarkReservedPosition(new Position(_pos.X - 1, _pos.Y + 1));
                    MazeGenerator.MarkReservedPosition(new Position(_pos.X + 1, _pos.Y + 1));
                    MazeGenerator.MarkReservedPosition(new Position(_pos.X, _pos.Y + 2));
                    MazeGenerator.MarkReservedPosition(new Position(_pos.X - 1, _pos.Y + 2));
                    MazeGenerator.MarkReservedPosition(new Position(_pos.X + 1, _pos.Y + 2));
                }
                else
                {
                    while (true)
                    {
                        _pos.X = (int) (mt.GetUInt32() % (Width - 2) + 1);
                        _pos.Y = (int) (mt.GetUInt32() % (Height - 3) + 1);
                        if (MazeGenerator.IsFree(_pos))
                            if (MazeGenerator.IsFree(new Position(_pos.X - 1, _pos.Y)))
                                if (MazeGenerator.IsFree(new Position(_pos.X + 1, _pos.Y)))
                                    if (MazeGenerator.IsFree(new Position(_pos.X, _pos.Y + 1)))
                                        if (MazeGenerator.IsFree(new Position(_pos.X - 1, _pos.Y + 1)))
                                            if (MazeGenerator.IsFree(new Position(_pos.X + 1, _pos.Y + 1)))
                                                if (MazeGenerator.IsFree(new Position(_pos.X, _pos.Y + 2)))
                                                    break;
                    }

                    MazeGenerator.MarkReservedPosition(new Position(_pos.X - 1, _pos.Y));
                    MazeGenerator.MarkReservedPosition(new Position(_pos.X + 1, _pos.Y));
                    MazeGenerator.MarkReservedPosition(new Position(_pos.X, _pos.Y + 1));
                    MazeGenerator.MarkReservedPosition(new Position(_pos.X - 1, _pos.Y + 1));
                    MazeGenerator.MarkReservedPosition(new Position(_pos.X + 1, _pos.Y + 1));
                    MazeGenerator.MarkReservedPosition(new Position(_pos.X, _pos.Y + 2));
                }
            }
            else
            {
                var free = false;
                while (!free)
                {
                    _pos.X = (int) (mt.GetUInt32() % Width);
                    _pos.Y = (int) (mt.GetUInt32() % Height);

                    free = MazeGenerator.IsFree(_pos);
                }
            }

            if (!IsLastFloor && !HasBossRoom)
            {
                var rndDir = new RandomDirection();
                while (true)
                {
                    var direction = rndDir.GetDirection(mt);
                    if (SetTraits(_pos, direction, (int) DungeonBlockType.StairsDown))
                    {
                        _startDirection = direction;
                        break;
                    }
                }
            }

            MazeGenerator.SetPathPosition(_pos);
        }
    }
}