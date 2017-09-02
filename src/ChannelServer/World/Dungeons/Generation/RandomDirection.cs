// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using Aura.Mabi.Util;

namespace Aura.Channel.World.Dungeons.Generation
{
    public class RandomDirection
    {
        public RandomDirection()
        {
            Directions = new[] {0, 0, 0, 0};
        }

        public int[] Directions { get; }

        public int GetDirection(MTRandom rnd)
        {
            var visited = true;
            var direction = 0;

            while (visited)
            {
                direction = (int) rnd.GetUInt32() & 3;
                visited = Directions[direction] != 0;
            }

            Directions[direction] = 1;

            return direction;
        }
    }
}