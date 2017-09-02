// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Drawing;
using Aura.Channel.World.Dungeons.Generation;
using Aura.Channel.World.Entities;
using Aura.Channel.World.Weather;
using Aura.Data;
using Aura.Data.Database;
using Aura.Mabi;
using Aura.Mabi.Const;

namespace Aura.Channel.World.Dungeons
{
    /// <summary>
    ///     Base class for dungeon regions.
    /// </summary>
    public abstract class DungeonRegion : Region
    {
        /// <summary>
        ///     Initializes region.
        /// </summary>
        /// <param name="regionId"></param>
        /// <param name="dungeon"></param>
        protected DungeonRegion(int regionId, Dungeon dungeon)
            : base(regionId)
        {
            Dungeon = dungeon;
            Name = dungeon.Name + "_" + regionId;

            InitWeather();
        }

        /// <summary>
        ///     Dungeon this region belongs to.
        /// </summary>
        public Dungeon Dungeon { get; }

        /// <summary>
        ///     Initiate weather for the new region.
        /// </summary>
        private void InitWeather()
        {
            var regionId = Id;
            var dungeon = Dungeon;

            // Set weather for Fiodh and Coill, which use the weather of
            // their respective outer regions.
            // We could add this information to the data, but officials
            // don't seem to have it either, which suggests that they hard-
            // coded it. Were we to add the info to the data, users could
            // quickly desynchonize the weather.

            // Fiodh = Gairech (type2)
            if (dungeon.Name.Contains("gairech_fiodh"))
                ChannelServer.Instance.Weather.SetProviderAndUpdate(regionId,
                    new WeatherProviderTable(regionId, "type2"));
            // Coill = Emain (type4)
            else if (dungeon.Name.Contains("emain_coill"))
                ChannelServer.Instance.Weather.SetProviderAndUpdate(regionId,
                    new WeatherProviderTable(regionId, "type4"));
        }

        /// <summary>
        ///     Returns the first prop with the given id or null if none were found.
        /// </summary>
        /// <param name="propId"></param>
        /// <returns></returns>
        public Prop GetPropById(int propId)
        {
            return GetProp(a => a.Info.Id == propId);
        }

        /// <summary>
        ///     Kills all monster NPCs in this region.
        /// </summary>
        public void RemoveAllMonsters()
        {
            foreach (var creature in GetCreatures(a => a is NPC && !a.Has(CreatureStates.GoodNpc)))
                RemoveCreature(creature);
        }

        /// <summary>
        ///     Removes creature from region, after making him drop all his dungeon keys.
        /// </summary>
        /// <param name="creature"></param>
        public override void RemoveCreature(Creature creature)
        {
            foreach (var item in creature.Inventory.GetItems(a => a.IsDungeonKey))
            {
                creature.Inventory.Remove(item);
                item.Drop(creature.Region, creature.GetPosition(), Item.DropRadius);
            }

            base.RemoveCreature(creature);
        }
    }

    /// <summary>
    ///     Floor (not lobby) region of a dungeon.
    /// </summary>
    public class DungeonFloorRegion : DungeonRegion
    {
        /// <summary>
        ///     Creates new floor region.
        /// </summary>
        /// <param name="regionId"></param>
        /// <param name="dungeon"></param>
        /// <param name="floorId"></param>
        public DungeonFloorRegion(int regionId, Dungeon dungeon, int floorId)
            : base(regionId, dungeon)
        {
            FloorId = floorId;
            Floor = dungeon.Generator.Floors[floorId];

            GenerateAreas();
        }

        /// <summary>
        ///     Id of the floor in the dungeon's generator.
        /// </summary>
        public int FloorId { get; }

        /// <summary>
        ///     The floor from the generator.
        /// </summary>
        public DungeonFloor Floor { get; }

        /// <summary>
        ///     Generates areas, incl (client) props and events.
        /// </summary>
        private void GenerateAreas()
        {
            Data = new RegionInfoData();

            var areaId = 2;
            var floor = Floor;

            for (var x = 0; x < floor.MazeGenerator.Width; ++x)
            for (var y = 0; y < floor.MazeGenerator.Height; ++y)
            {
                var room = floor.MazeGenerator.GetRoom(x, y);
                var roomTrait = floor.GetRoom(x, y);

                if (!room.Visited)
                    continue;

                var isStart = roomTrait.RoomType == RoomType.Start;
                var isEnd = roomTrait.RoomType == RoomType.End;
                var isRoom = roomTrait.RoomType >= RoomType.Start;
                var isBossRoom = floor.HasBossRoom && isEnd;
                var eventId = 0L;

                if (!isBossRoom)
                {
                    var areaData = new AreaData();
                    areaData.Id = areaId++;
                    areaData.Name = "Tile" + areaData.Id;

                    areaData.X1 = x * Dungeon.TileSize;
                    areaData.Y1 = y * Dungeon.TileSize;
                    areaData.X2 = x * Dungeon.TileSize + Dungeon.TileSize;
                    areaData.Y2 = y * Dungeon.TileSize + Dungeon.TileSize;

                    Data.Areas.Add(areaData);

                    var type = isRoom ? DungeonBlockType.Room : DungeonBlockType.Alley;

                    var propEntityId = MabiId.ClientProps | ((long) Id << 32) | ((long) areaData.Id << 16) | 1;
                    var block = Dungeon.Data.Style.Get(type, room.Directions);
                    var tileCenter = new Point(x * Dungeon.TileSize + Dungeon.TileSize / 2,
                        y * Dungeon.TileSize + Dungeon.TileSize / 2);

                    var prop = new Prop(propEntityId, block.PropId, Id, tileCenter.X, tileCenter.Y,
                        MabiMath.DegreeToRadian(block.Rotation), 1, 0, "", "", "");
                    AddProp(prop);

                    // Debug
                    //foreach (var points in prop.Shapes)
                    //{
                    //	foreach (var point in points)
                    //	{
                    //		var pole = new Prop(30, this.Id, point.X, point.Y, 0, 1, 0, "", "", "");
                    //		pole.Shapes.Clear();
                    //		this.AddProp(pole);
                    //	}
                    //}

                    // TODO: This region/data stuff is a mess... create
                    //   proper classes, put them in the regions and be
                    //   done with it.

                    if (isStart || isEnd)
                    {
                        var xp = tileCenter.X;
                        var yp = tileCenter.Y;

                        if (roomTrait.DoorType[Direction.Up] >= 3000)
                            yp += 400;
                        else if (roomTrait.DoorType[Direction.Right] >= 3000)
                            xp += 400;
                        else if (roomTrait.DoorType[Direction.Down] >= 3000)
                            yp -= 400;
                        else if (roomTrait.DoorType[Direction.Left] >= 3000)
                            xp -= 400;

                        var eventData = new EventData();
                        eventData.Id = MabiId.AreaEvents | ((long) Id << 32) | ((long) areaData.Id << 16) | eventId++;
                        eventData.Name = isStart ? "Indoor_RDungeon_SB" : "Indoor_RDungeon_EB";
                        eventData.X = xp;
                        eventData.Y = yp;

                        var shape = new ShapeData();
                        shape.DirX1 = 1;
                        shape.DirY2 = 1;
                        shape.LenX = 100;
                        shape.LenY = 100;
                        shape.PosX = xp;
                        shape.PosY = yp;
                        eventData.Shapes.Add(shape);

                        areaData.Events.Add(eventData.Id, eventData);
                        _clientEvents.Add(eventData.Id, new ClientEvent(eventData, Data.Name, areaData.Name));
                    }
                }
                else
                {
                    // Big main room
                    var areaData = new AreaData();
                    areaData.Id = areaId++;
                    areaData.Name = "Tile" + areaData.Id;

                    areaData.X1 = x * Dungeon.TileSize - Dungeon.TileSize;
                    areaData.Y1 = y * Dungeon.TileSize;
                    areaData.X2 = x * Dungeon.TileSize + Dungeon.TileSize * 2;
                    areaData.Y2 = y * Dungeon.TileSize + Dungeon.TileSize * 2;

                    Data.Areas.Add(areaData);

                    var block = Dungeon.Data.Style.Get(DungeonBlockType.BossRoom);
                    var propEntityId = MabiId.ClientProps | ((long) Id << 32) | ((long) areaData.Id << 16) | 1;
                    var tileCenter = new Point(x * Dungeon.TileSize + Dungeon.TileSize / 2,
                        y * Dungeon.TileSize + Dungeon.TileSize);

                    var prop = new Prop(propEntityId, block.PropId, Id, tileCenter.X, tileCenter.Y,
                        MabiMath.DegreeToRadian(block.Rotation), 1, 0, "", "", "");
                    AddProp(prop);

                    // Debug
                    //foreach (var points in prop.Shapes)
                    //{
                    //	foreach (var point in points)
                    //	{
                    //		var pole = new Prop(30, this.Id, point.X, point.Y, 0, 1, 0, "", "", "");
                    //		pole.Shapes.Clear();
                    //		this.AddProp(pole);
                    //	}
                    //}

                    // Treasure room
                    areaData = new AreaData();
                    areaData.Id = areaId++;
                    areaData.Name = "Tile" + areaData.Id;

                    areaData.X1 = x * Dungeon.TileSize;
                    areaData.Y1 = y * Dungeon.TileSize + Dungeon.TileSize * 2;
                    areaData.X2 = x * Dungeon.TileSize + Dungeon.TileSize;
                    areaData.Y2 = y * Dungeon.TileSize + Dungeon.TileSize * 2 + Dungeon.TileSize;

                    Data.Areas.Add(areaData);
                }
            }
        }
    }

    /// <summary>
    ///     Lobby region of a dungeon.
    /// </summary>
    public class DungeonLobbyRegion : DungeonRegion
    {
        /// <summary>
        ///     Creates new lobby region.
        /// </summary>
        /// <param name="regionId"></param>
        /// <param name="baseRegionId"></param>
        /// <param name="dungeon"></param>
        public DungeonLobbyRegion(int regionId, int baseRegionId, Dungeon dungeon)
            : base(regionId, dungeon)
        {
            var baseRegionInfoData = AuraData.RegionInfoDb.Find(baseRegionId);
            if (baseRegionInfoData == null)
                throw new Exception("DungeonLobbyRegion: No region info data found for '" + baseRegionId + "'.");

            IsIndoor = true;
            Data = baseRegionInfoData.Copy();
            FixIds(Data, Id);

            InitializeFromData();
        }

        /// <summary>
        ///     Fixes the region data's entity ids.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="regionId"></param>
        private static void FixIds(RegionInfoData data, int regionId)
        {
            var areaId = 1;

            foreach (var areaData in data.Areas)
            {
                areaData.Id = areaId++;

                foreach (var propData in areaData.Props.Values)
                {
                    var entityId = (propData.EntityId & ~0x0000FFFF00000000) | ((long) regionId << 32) |
                                   ((long) areaData.Id << 16);
                    propData.EntityId = entityId;
                }

                foreach (var eventData in areaData.Events.Values)
                {
                    var entityId = (eventData.Id & ~0x0000FFFF00000000) | ((long) regionId << 32) |
                                   ((long) areaData.Id << 16);
                    eventData.Id = entityId;
                    eventData.RegionId = regionId;
                }
            }
        }
    }
}