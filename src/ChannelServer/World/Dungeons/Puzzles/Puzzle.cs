// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Collections.Generic;
using System.Linq;
using Aura.Channel.Scripting.Scripts;
using Aura.Channel.World.Dungeons.Generation;
using Aura.Channel.World.Dungeons.Props;
using Aura.Channel.World.Entities;
using Aura.Data.Database;
using Aura.Mabi;

namespace Aura.Channel.World.Dungeons.Puzzles
{
    /// <summary>
    ///     Dungeon puzzle
    /// </summary>
    /// <remarks>
    ///     Aka something that may or may not spawn props and monsters and may or
    ///     may not create rooms.
    /// </remarks>
    public class Puzzle
    {
        private readonly Dictionary<string, DungeonMonsterGroupData> _monsterGroupData;
        private readonly Dictionary<string, MonsterGroup> _monsterGroups;
        private readonly Dictionary<string, PuzzlePlace> _places = new Dictionary<string, PuzzlePlace>();
        private readonly DungeonFloorSection _section;
        private readonly Dictionary<string, object> _variables;

        /// <summary>
        ///     Creates new puzzle.
        /// </summary>
        /// <param name="dungeon"></param>
        /// <param name="section"></param>
        /// <param name="floorData"></param>
        /// <param name="puzzleData"></param>
        /// <param name="puzzleScript"></param>
        public Puzzle(Dungeon dungeon, DungeonFloorSection section, DungeonFloorData floorData,
            DungeonPuzzleData puzzleData, PuzzleScript puzzleScript)
        {
            _variables = new Dictionary<string, object>();
            _monsterGroups = new Dictionary<string, MonsterGroup>();
            _monsterGroupData = new Dictionary<string, DungeonMonsterGroupData>();
            Props = new Dictionary<string, Prop>();
            Keys = new Dictionary<string, Item>();

            _section = section;
            Name = puzzleScript.Name;
            Data = puzzleData;
            Dungeon = dungeon;
            Script = puzzleScript;
            FloorData = floorData;

            for (var i = 1; i <= puzzleData.Groups.Count; ++i)
                _monsterGroupData["Mob" + i] = puzzleData.Groups[i - 1].Copy();
        }

        /// <summary>
        ///     Name of the puzzle.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Puzzle's data from db.
        /// </summary>
        public DungeonPuzzleData Data { get; }

        /// <summary>
        ///     Data of the floor this puzzle is spawned on.
        /// </summary>
        public DungeonFloorData FloorData { get; }

        /// <summary>
        ///     Script that controls this puzzle.
        /// </summary>
        public PuzzleScript Script { get; }

        /// <summary>
        ///     Dungeon this puzzle is part of.
        /// </summary>
        public Dungeon Dungeon { get; }

        /// <summary>
        ///     Region this puzzle is in.
        /// </summary>
        public Region Region { get; private set; }

        /// <summary>
        ///     List of props spawned by this puzzle.
        /// </summary>
        public Dictionary<string, Prop> Props { get; }

        /// <summary>
        ///     List of keys created for this puzzle.
        /// </summary>
        public Dictionary<string, Item> Keys { get; }

        /// <summary>
        ///     Returns true if all locked places have been unlocked.
        /// </summary>
        /// <returns></returns>
        public bool HasBeenSolved
        {
            get
            {
                // Check for != closed, so boss doors that don't open count
                return _places.Values.All(a => !a.IsLock || a.GetLockDoor().State != "closed");
            }
        }

        /// <summary>
        ///     Creates doors for puzzle and calls OnPuzzleCreate.
        /// </summary>
        /// <param name="region"></param>
        public void OnCreate(Region region)
        {
            Region = region;
            foreach (var place in _places)
            foreach (var door in place.Value.Doors.Where(x => x != null))
            {
                // Beware, some doors are shared between puzzles
                if (door.EntityId != 0)
                    continue;
                door.Info.Region = region.Id;
                region.AddProp(door);
            }

            Script.OnPuzzleCreate(this);
        }

        /// <summary>
        ///     Creates a new place for the puzzle to use.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public PuzzlePlace NewPlace(string name)
        {
            _places[name] = new PuzzlePlace(_section, this, name);
            return _places[name];
        }

        /// <summary>
        ///     Returns the place with the given name, or null if it doesn't exist.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public PuzzlePlace GetPlace(string name)
        {
            if (_places.ContainsKey(name))
                return _places[name];
            return null;
        }

        /// <summary>
        ///     Returns prop with the given name, or null if it doesn't exist.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public Prop FindProp(string name)
        {
            if (Props.ContainsKey(name))
                return Props[name];
            return null;
        }

        /// <summary>
        ///     Sets temporary variable.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="value"></param>
        public void Set(string name, object value)
        {
            _variables[name] = value;
        }

        /// <summary>
        ///     Gets value of temporary variable, returns null if variable doesn't exist.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public dynamic Get(string name)
        {
            if (_variables.ContainsKey(name))
                return _variables[name];
            return null;
        }

        /// <summary>
        ///     Locks place and creates and returns a key for it.
        /// </summary>
        /// <param name="place"></param>
        /// <param name="keyName"></param>
        /// <returns></returns>
        public Item LockPlace(PuzzlePlace place, string keyName)
        {
            if (!place.IsLock)
                throw new PuzzleException("Tried to lock a place that isn't a Lock");

            var doorName = place.GetLockDoor().Name;

            Item key;
            if (place.IsBossLock)
                key = Item.CreateKey(70030, 0xFF0000, doorName); // Boss Room Key
            else
                key = Item.CreateKey(70029, place.LockColor, doorName); // Dungeon Room Key

            place.LockPlace(key);
            Keys[keyName] = key;

            return key;
        }

        /// <summary>
        ///     Locks place without a key.
        /// </summary>
        /// <param name="place"></param>
        /// <returns></returns>
        public void LockPlace(PuzzlePlace place)
        {
            if (!place.IsLock)
                throw new PuzzleException("Tried to lock a place that isn't a Lock");

            place.LockPlace();
        }

        /// <summary>
        ///     Adds prop to puzzle in place.
        /// </summary>
        /// <param name="place"></param>
        /// <param name="prop"></param>
        /// <param name="positionType"></param>
        public void AddProp(PuzzlePlace place, DungeonProp prop, Placement positionType)
        {
            if (Region == null)
                throw new PuzzleException("AddProp outside of OnPuzzleCreate.");

            var pos = place.GetPosition(positionType);

            prop.RegionId = Region.Id;
            prop.Info.X = pos[0];
            prop.Info.Y = pos[1];
            prop.UpdateShapes();
            prop.Info.Direction = MabiMath.DegreeToRadian(pos[2]);
            prop.Behavior += PuzzleEvent;

            Region.AddProp(prop);
            Props[prop.Name] = prop;
        }

        /// <summary>
        ///     Calls OnPropEvent.
        /// </summary>
        /// <param name="creature"></param>
        /// <param name="prop"></param>
        public void PuzzleEvent(Creature creature, Prop prop)
        {
            Script.OnPropEvent(this, prop);
        }

        /// <summary>
        ///     Spawns mob in place.
        /// </summary>
        /// <param name="place"></param>
        /// <param name="name"></param>
        /// <param name="group"></param>
        /// <param name="spawnPosition"></param>
        public void AllocateAndSpawnMob(PuzzlePlace place, string name, DungeonMonsterGroupData group,
            Placement spawnPosition)
        {
            var mob = new MonsterGroup(name, this, place, spawnPosition);
            _monsterGroups.Add(name, mob);

            mob.Allocate(group);
            mob.Spawn();
        }

        /// <summary>
        ///     Returns monster group, or null if it doesn't exist.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public MonsterGroup GetMonsterGroup(string name)
        {
            MonsterGroup result;
            _monsterGroups.TryGetValue(name, out result);
            return result;
        }

        /// <summary>
        ///     Returns monster data, or null if it doesn't exist.
        /// </summary>
        /// <param name="name"></param>
        /// <returns></returns>
        public DungeonMonsterGroupData GetMonsterData(string name)
        {
            DungeonMonsterGroupData result;
            _monsterGroupData.TryGetValue(name, out result);
            return result;
        }
    }
}