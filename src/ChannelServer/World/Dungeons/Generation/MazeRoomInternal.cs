// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

namespace Aura.Channel.World.Dungeons.Generation
{
    public class MazeRoomInternal
    {
        public MazeRoomInternal()
        {
            Directions = new[] {0, 0, 0, 0};
        }

        public int[] Directions { get; set; }
        public bool IsOnCriticalPath { get; set; }
        public int VisitedCount { get; set; }
        public bool IsReserved { get; set; }

        public bool Visited => VisitedCount != 0;
        public bool Occupied => Visited || IsReserved;

        public int GetPassageType(int direction)
        {
            return Directions[direction];
        }
    }
}