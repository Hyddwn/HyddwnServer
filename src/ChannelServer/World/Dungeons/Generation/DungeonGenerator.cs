// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Collections.Generic;
using Aura.Data;
using Aura.Data.Database;
using Aura.Mabi.Util;

namespace Aura.Channel.World.Dungeons.Generation
{
    public class DungeonGenerator
    {
        public DungeonGenerator(string dungeonName, int itemId, int seed, int floorPlan, string option)
        {
            Name = dungeonName.ToLower();
            Data = AuraData.DungeonDb.Find(Name);

            Seed = seed;
            FloorPlan = floorPlan;
            Option = (option ?? "").ToLower();
            RngMaze = new MTRandom(Data.BaseSeed + itemId + floorPlan);
            RngPuzzles = new MTRandom(seed);
            Floors = new List<DungeonFloor>();

            DungeonFloor prev = null;
            for (var i = 0; i < Data.Floors.Count; i++)
            {
                var isLastFloor = i == Data.Floors.Count - 1;

                var floor = new DungeonFloor(this, Data.Floors[i], isLastFloor, prev);
                Floors.Add(floor);

                prev = floor;
            }
        }

        public string Name { get; }
        public int ItemId { get; private set; }
        public int Seed { get; }
        public int FloorPlan { get; }
        public string Option { get; }
        public MTRandom RngMaze { get; }
        public MTRandom RngPuzzles { get; }
        public List<DungeonFloor> Floors { get; }
        public DungeonData Data { get; }
    }
}