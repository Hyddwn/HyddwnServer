// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;

namespace Aura.Channel.World.Dungeons
{
    public class DungeonBoss
    {
        public DungeonBoss(int raceId, int amount)
        {
            RaceId = raceId;
            Amount = Math.Max(1, amount);
        }

        public int RaceId { get; set; }
        public int Amount { get; set; }
    }
}