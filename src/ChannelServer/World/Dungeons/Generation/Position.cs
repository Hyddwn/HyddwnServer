// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

namespace Aura.Channel.World.Dungeons.Generation
{
    public class Position
    {
        public Position(Position pos)
            : this(pos.X, pos.Y)
        {
        }

        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        public int X { get; set; }
        public int Y { get; set; }

        public Position GetBias(int dir)
        {
            if (dir == Direction.Up)
                return new Position(0, 1);
            if (dir == Direction.Right)
                return new Position(1, 0);
            if (dir == Direction.Down)
                return new Position(0, -1);
            if (dir == Direction.Left)
                return new Position(-1, 0);
            return new Position(0, 0);
        }

        public Position GetBiasedPosition(int direction)
        {
            var bias = GetBias(direction);
            return new Position(X + bias.X, Y + bias.Y);
        }
    }
}