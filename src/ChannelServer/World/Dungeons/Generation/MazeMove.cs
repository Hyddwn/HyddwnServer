// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

namespace Aura.Channel.World.Dungeons.Generation
{
    public class MazeMove
    {
        public MazeMove(Position from, Position to, int direction)
        {
            PosFrom = new Position(from);
            PosTo = new Position(to);
            Direction = direction;
        }

        public Position PosFrom { get; }
        public Position PosTo { get; }

        public int Direction { get; set; }
    }
}