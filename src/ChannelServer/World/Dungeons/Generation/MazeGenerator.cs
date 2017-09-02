// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Collections.Generic;
using System.Linq;
using Aura.Mabi.Util;

namespace Aura.Channel.World.Dungeons.Generation
{
    public class MazeGenerator
    {
        private int _counter;

        //private int _critPathMinResult;
        private int _critPathMaxResult;

        private Position _currentPos;
        private bool isCriticalPathGenerated;
        private bool isSubPathGenerated;

        public MazeGenerator()
        {
            _currentPos = new Position(0, 0);
            StartPos = new Position(0, 0);
            EndPos = new Position(0, 0);
            Rooms = new List<List<MazeRoomInternal>>();
            CriticalPath = new List<MazeMove>();
        }

        public int Width { get; private set; }
        public int Height { get; private set; }

        public Position StartPos { get; private set; }
        public Position EndPos { get; private set; }
        public int StartDirection { get; set; }

        public List<List<MazeRoomInternal>> Rooms { get; private set; } // [width][height] array of maze_room_internal
        public List<MazeMove> CriticalPath { get; private set; }

        public void SetSize(int width, int height)
        {
            Width = width;
            Height = height;
            Rooms = new List<List<MazeRoomInternal>>();
            CriticalPath = new List<MazeMove>();
            for (var h = 0; h < Width; ++h)
            {
                var row = new List<MazeRoomInternal>();
                for (var w = 0; w < Height; ++w)
                    row.Add(new MazeRoomInternal());
                Rooms.Add(row);
            }
            EndPos = new Position(width - 1, height - 1);
        }

        public bool GenerateCriticalPath(MTRandom rnd, int critPathMin, int critPathMax)
        {
            if (isCriticalPathGenerated)
                return true;

            if (critPathMin > critPathMax)
            {
                var min = critPathMin;
                critPathMin = critPathMax;
                critPathMax = min;
            }

            //this._critPathMinResult = 0;
            _critPathMaxResult = 0;
            isCriticalPathGenerated = GenerateCriticalPathRecursive(0, critPathMin, critPathMax, -1, rnd);

            return isCriticalPathGenerated;
        }

        public bool GenerateSubPath(MTRandom rnd, int coverageFactor, int branchProbability)
        {
            if (!isCriticalPathGenerated)
                return false;

            if (isSubPathGenerated)
                return true;

            if (coverageFactor > 100)
                coverageFactor = 100;

            if (branchProbability > 100)
                branchProbability = 100;

            var freeRooms = 0;
            for (var y = 0; y < Height; ++y)
            for (var x = 0; x < Width; ++x)
                if (!Rooms[x][y].Occupied)
                    freeRooms += 1;

            var coverage = freeRooms * coverageFactor / 100;
            var toVector = new List<Position>();

            if (CriticalPath.Count > 0)
            {
                foreach (var move in CriticalPath)
                    toVector.Add(move.PosTo);

                toVector.RemoveAt(toVector.Count - 1);
            }

            toVector = GenerateSubPathSub1(toVector);

            if (coverage <= 0)
                return true;

            var tempVector = new List<Position>();
            for (var i = 0; i < coverage; ++i)
            {
                var vect = toVector;
                var flag = false;

                if (tempVector.Count == 0)
                {
                    if (toVector.Count == 0)
                        break;

                    flag = true;
                }
                else
                {
                    if (toVector.Count == 0)
                    {
                        flag = false;
                        vect = tempVector;
                    }
                    else
                    {
                        var rndNum = rnd.GetUInt32() % 100;
                        flag = branchProbability >= rndNum;
                        if (!flag)
                            vect = tempVector;
                    }
                }

                var rndIndex = (int) (rnd.GetUInt32() % (uint) vect.Count());
                var pos = vect[rndIndex];
                var room = GetRoom(pos);
                var directions = new[] {0, 0, 0, 0};
                var rndDirection = -1;
                var direction = 0;

                while (true)
                {
                    rndDirection = GenerateSubPathRandomDir(rnd, directions);
                    if (room.GetPassageType(rndDirection) == 0)
                        if (IsRoomInDirectionFree(pos, rndDirection))
                            break;

                    direction += 1;

                    if (direction >= 4)
                        break;
                }

                if (direction >= 4)
                {
                    tempVector = GenerateSubPathSub3(tempVector, toVector);
                    toVector = GenerateSubPathSub1(toVector);
                    continue;
                }

                var biasedPos = pos.GetBiasedPosition(rndDirection);
                var room2 = GetRoom(biasedPos);

                room.Directions[rndDirection] = 2;
                room2.Directions[Direction.GetOppositeDirection(rndDirection)] = 1;

                _counter += 1;

                room2.VisitedCount = _counter;
                tempVector.Add(biasedPos);

                if (!flag)
                {
                    tempVector.RemoveAt(rndIndex);
                    toVector.Add(pos);
                }

                tempVector = GenerateSubPathSub3(tempVector, toVector);
                toVector = GenerateSubPathSub1(toVector);
            }

            isSubPathGenerated = true;

            return true;
        }

        private int GenerateSubPathRandomDir(MTRandom rnd, int[] directions)
        {
            for (var i = 0; i < 4; i++)
                if (directions[i] == 0)
                    while (true)
                    {
                        var random_dir = (int) (rnd.GetUInt32() & 3);
                        if (directions[random_dir] == 0)
                        {
                            directions[random_dir] = 1;
                            return random_dir;
                        }
                    }

            return -1;
        }

        private List<Position> GenerateSubPathSub3(List<Position> tempVector, List<Position> to_vector)
        {
            var result = new List<Position>();

            foreach (var pos in tempVector)
                if (GenerateSubPathSub2(pos))
                {
                    var room = GetRoom(pos);
                    var vect = true;

                    for (var i = 0; i < 4; i++)
                        if (room.Directions[i] == 2)
                        {
                            vect = false;
                            to_vector.Add(pos);
                            break;
                        }

                    if (vect)
                        result.Add(pos);
                }

            return result;
        }

        private bool GenerateSubPathSub2(Position pos)
        {
            var room = GetRoom(pos);

            if (room != null)
                for (var i = 0; i < 4; i++)
                    if (room.GetPassageType(i) == 0)
                        if (IsRoomInDirectionFree(pos, i))
                            return true;

            return false;
        }

        private bool GenerateCriticalPathRecursive(int critPathPos, int critPathMin, int critPathMax, int direction,
            MTRandom rnd)
        {
            var directions = new int[4];
            _critPathMaxResult += 1;

            if (_critPathMaxResult <= 10 * critPathMax)
                if (critPathMin <= critPathPos && critPathPos <= critPathMax &&
                    IsRoomInDirectionFree(_currentPos, StartDirection))
                {
                    StartPos = _currentPos;

                    foreach (var move in CriticalPath)
                    {
                        var temp = move.PosFrom.X;
                        move.PosFrom.X = move.PosTo.X;
                        move.PosTo.X = temp;

                        temp = move.PosFrom.Y;
                        move.PosFrom.Y = move.PosTo.Y;
                        move.PosTo.Y = temp;

                        move.Direction = Direction.GetOppositeDirection(move.Direction);
                    }

                    CriticalPath.Reverse();

                    return true;
                }
                else
                {
                    critPathPos += 1;
                    var count = 0;

                    if (critPathPos <= critPathMax)
                    {
                        if (direction != -1)
                            direction = Direction.GetOppositeDirection(direction);

                        for (var i = 0; i < 4; i++)
                            if (i == direction)
                            {
                                directions[i] = 0;
                            }
                            else
                            {
                                var next_pos = _currentPos.GetBiasedPosition(i);
                                directions[i] = Sub(next_pos);
                                count += directions[i];
                            }

                        while (count > 0)
                        {
                            var rndNum = rnd.GetUInt32() % count + 1;
                            var cnt2 = 0;

                            var iDir = 0;
                            while (iDir < 4)
                            {
                                cnt2 += directions[iDir];
                                if (cnt2 >= rndNum)
                                    break;

                                iDir++;
                            }

                            count -= directions[iDir];
                            directions[iDir] = 0;

                            if (MakeMove(iDir))
                            {
                                if (GenerateCriticalPathRecursive(critPathPos, critPathMin, critPathMax, iDir, rnd))
                                    return true;

                                UndoMove();
                            }
                        }
                    }
                }

            return false;
        }

        private int Sub(Position pos)
        {
            if (GetRoom(pos) != null)
            {
                var count = 1;

                for (var iDir = 0; iDir < 4; iDir++)
                {
                    var room = GetRoom(pos.GetBiasedPosition(iDir));
                    if (room != null && !room.Occupied)
                        count += 1;
                }

                return count;
            }

            return 0;
        }

        private bool MakeMove(int direction)
        {
            if (IsRoomInDirectionFree(_currentPos, direction))
            {
                var nextPos = _currentPos.GetBiasedPosition(direction);
                var currentRoom = GetRoom(_currentPos);
                var nextRoom = GetRoom(nextPos);
                var move = new MazeMove(_currentPos, nextPos, direction);

                CriticalPath.Add(move);

                _counter++;

                nextRoom.VisitedCount = _counter;
                nextRoom.IsOnCriticalPath = true;

                currentRoom.Directions[direction] = 1;
                nextRoom.Directions[Direction.GetOppositeDirection(direction)] = 2;

                _currentPos = nextPos;

                return true;
            }

            return false;
        }

        private void UndoMove(int count = 1)
        {
            for (var i = 0; i < count; i++)
            {
                var move = CriticalPath[CriticalPath.Count - 1];

                CriticalPath.Remove(CriticalPath[CriticalPath.Count - 1]);

                var current_room = GetRoom(move.PosFrom);
                var next_room = GetRoom(move.PosTo);

                var oppositeDirection = Direction.GetOppositeDirection(move.Direction);

                if (next_room.Visited)
                {
                    current_room.Directions[move.Direction] = 0;
                    next_room.Directions[oppositeDirection] = 0;

                    next_room.VisitedCount = 0;
                    next_room.IsOnCriticalPath = false;

                    _counter -= 1;
                }

                _currentPos = _currentPos.GetBiasedPosition(oppositeDirection);
            }
        }

        public MazeRoomInternal GetRoom(Position pos)
        {
            return GetRoom(pos.X, pos.Y);
        }

        public MazeRoomInternal GetRoom(int x, int y)
        {
            if (0 <= x && x < Width && 0 <= y && y < Height)
                return Rooms[x][y];

            return null;
        }

        public bool IsFree(Position pos)
        {
            return !Rooms[pos.X][pos.Y].Occupied;
        }

        public bool IsRoomInDirectionFree(Position pos, int direction)
        {
            var dirPos = pos.GetBiasedPosition(direction);

            if (0 <= dirPos.X && dirPos.X < Width && 0 <= dirPos.Y && dirPos.Y < Height)
                return !Rooms[dirPos.X][dirPos.Y].Occupied;

            return false;
        }

        public void MarkReservedPosition(Position pos)
        {
            var room = Rooms[pos.X][pos.Y];
            if (!room.Visited)
                room.IsReserved = true;
        }

        public void SetPathPosition(Position pos)
        {
            if (Width > pos.X && Height > pos.Y)
            {
                MazeRoomInternal room;

                for (var y = 0; y < Height; y++)
                for (var x = 0; x < Width; x++)
                {
                    room = Rooms[x][y];
                    room.Directions = new int[4];
                    room.VisitedCount = 0;
                }

                CriticalPath = new List<MazeMove>();
                StartPos = new Position(0, 0);
                _currentPos = new Position(0, 0);

                EndPos.X = pos.X;
                EndPos.Y = pos.Y;

                _currentPos.X = pos.X;
                _currentPos.Y = pos.Y;

                _counter = 1;

                room = Rooms[pos.X][pos.Y];
                room.VisitedCount = _counter;
                room.IsOnCriticalPath = true;
            }
        }

        private List<Position> GenerateSubPathSub1(List<Position> to_vector)
        {
            var result = new List<Position>();

            foreach (var pos in to_vector)
                if (GenerateSubPathSub2(pos))
                    result.Add(pos);

            return result;
        }
    }
}