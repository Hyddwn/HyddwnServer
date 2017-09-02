// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

namespace Aura.Channel.World.Dungeons.Generation
{
    public static class Direction
    {
        public const int Up = 0;
        public const int Right = 1;
        public const int Down = 2;
        public const int Left = 3;

        public static int GetOppositeDirection(int dir)
        {
            if (dir == Up)
                return Down;
            if (dir == Right)
                return Left;
            if (dir == Down)
                return Up;
            if (dir == Left)
                return Right;
            return -1;
        }
    }
}