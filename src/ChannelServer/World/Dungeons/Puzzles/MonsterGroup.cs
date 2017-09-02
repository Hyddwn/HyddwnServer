// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System.Collections.Generic;
using System.Threading;
using Aura.Channel.World.Entities;
using Aura.Data.Database;
using Aura.Mabi;
using Aura.Mabi.Const;
using Aura.Shared.Util;

namespace Aura.Channel.World.Dungeons.Puzzles
{
    public class MonsterGroup
    {
        private readonly List<NPC> _monsters;
        private int _remaining;

        private readonly Placement _spawnPosition;

        /// <summary>
        ///     Creates new monster group.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="puzzle"></param>
        /// <param name="place"></param>
        /// <param name="spawnPosition"></param>
        public MonsterGroup(string name, Puzzle puzzle, PuzzlePlace place, Placement spawnPosition = Placement.Random)
        {
            _monsters = new List<NPC>();

            Name = name;
            Puzzle = puzzle;
            Place = place;
            _spawnPosition = spawnPosition;
        }

        /// <summary>
        ///     Name of the group.
        /// </summary>
        public string Name { get; }

        /// <summary>
        ///     Puzzle this group is a part of.
        /// </summary>
        public Puzzle Puzzle { get; }

        /// <summary>
        ///     Place this group is a part of.
        /// </summary>
        public PuzzlePlace Place { get; }

        /// <summary>
        ///     Amount of monsters in this group.
        /// </summary>
        public int Count => _monsters.Count;

        /// <summary>
        ///     Amount of alive monsters in this group.
        /// </summary>
        public int Remaining => _remaining;

        /// <summary>
        ///     Creates monsters from group data and adds them to internal list.
        /// </summary>
        /// <param name="groupData"></param>
        public void Allocate(DungeonMonsterGroupData groupData)
        {
            foreach (var monsterData in groupData)
                for (var i = 0; i < monsterData.Amount; ++i)
                {
                    var monster = new NPC(monsterData.RaceId);
                    monster.State |= CreatureStates.Spawned | CreatureStates.InstantNpc;
                    monster.Death += OnDeath;

                    _monsters.Add(monster);
                }

            _remaining = Count;

            Puzzle.Script.OnMobAllocated(Puzzle, this);
        }

        /// <summary>
        ///     Spawns monsters from internal list.
        /// </summary>
        public void Spawn()
        {
            var rnd = RandomProvider.Get();

            var region = Puzzle.Region;
            var worldPos = Place.GetWorldPosition();

            foreach (var monster in _monsters)
            {
                var pos = Place.GetPosition(_spawnPosition);
                monster.Direction = MabiMath.DegreeToByte(pos[2]);
                monster.Spawn(region.Id, pos[0], pos[1]);

                if (monster.AI != null)
                    monster.AI.Activate(1000);
            }
        }

        /// <summary>
        ///     Raised when one of the monsters dies.
        /// </summary>
        /// <param name="creature"></param>
        /// <param name="killer"></param>
        private void OnDeath(Creature creature, Creature killer)
        {
            Interlocked.Decrement(ref _remaining);
            Puzzle.Script.OnMonsterDead(Puzzle, this);
        }

        /// <summary>
        ///     Adds key for lock place to a random monster of this group as a drop.
        /// </summary>
        /// <param name="lockPlace"></param>
        public void AddKeyForLock(PuzzlePlace lockPlace)
        {
            var place = lockPlace;
            if (!place.IsLock)
            {
                Log.Warning("PuzzleChest.AddKeyForLock: This place isn't a Lock. ({0})", Puzzle.Name);
                return;
            }

            if (Count == 0)
            {
                Log.Warning("MonsterGroup.AddKeyForLock: No monsters in group.");
                return;
            }

            if (place.Key == null)
            {
                Log.Warning("MonsterGroup.AddKeyForLock: Place's key is null.");
                return;
            }

            AddDrop(place.Key);
        }

        /// <summary>
        ///     Adds item to the drops of one random monster in this group.
        /// </summary>
        /// <param name="item"></param>
        public void AddDrop(Item item)
        {
            var rnd = RandomProvider.Get();
            var rndMonster = _monsters[rnd.Next(_monsters.Count)];
            rndMonster.Drops.Add(item);
        }
    }
}