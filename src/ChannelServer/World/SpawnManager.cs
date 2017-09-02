// Copyright (c) Aura development team - Licensed under GNU GPL
// For more information, see license file in the main folder

using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Aura.Channel.Network.Sending;
using Aura.Channel.World.Entities;
using Aura.Mabi.Const;
using Aura.Shared.Util;

namespace Aura.Channel.World
{
    public class SpawnManager
    {
        private readonly Dictionary<int, CreatureSpawner> _spawners;

        /// <summary>
        ///     Creates new spawn manager.
        /// </summary>
        public SpawnManager()
        {
            _spawners = new Dictionary<int, CreatureSpawner>();
        }

        /// <summary>
        ///     Adds spawner to manager.
        /// </summary>
        /// <param name="spawner"></param>
        public void Add(CreatureSpawner spawner)
        {
            lock (_spawners)
            {
                _spawners[spawner.Id] = spawner;
            }
        }

        /// <summary>
        ///     Returns spawn with given id or null if it doesn't exist.
        /// </summary>
        /// <param name="spawnerId"></param>
        /// <returns></returns>
        public CreatureSpawner Get(int spawnerId)
        {
            CreatureSpawner result;

            lock (_spawners)
            {
                _spawners.TryGetValue(spawnerId, out result);
            }

            return result;
        }

        /// <summary>
        ///     Removes spawner with given id, returns true if one was removed.
        /// </summary>
        /// <param name="spawnerId"></param>
        /// <returns></returns>
        public bool Remove(int spawnerId)
        {
            var spawner = Get(spawnerId);
            if (spawner == null)
                return false;

            // Remove spawner
            lock (_spawners)
            {
                _spawners.Remove(spawnerId);
            }

            // Dispose to remove spawned creatures
            spawner.Dispose();

            return true;
        }

        /// <summary>
        ///     Disposes and removes all spawners.
        /// </summary>
        public void Clear()
        {
            lock (_spawners)
            {
                foreach (var spawner in _spawners.Values)
                    spawner.Dispose();
                _spawners.Clear();
            }
        }

        /// <summary>
        ///     Calls Spawn on all of the manager's spawners.
        /// </summary>
        public void SpawnAll()
        {
            Log.Info("Spawning creatures...");

            foreach (var spawner in _spawners.Values)
                spawner.Spawn();

            Log.Info("  done spawning creatures.");
        }

        /// <summary>
        ///     Spawns a creature.
        /// </summary>
        /// <param name="raceId"></param>
        /// <param name="regionId"></param>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="spawnId"></param>
        /// <param name="active"></param>
        /// <param name="effect"></param>
        /// <returns></returns>
        public NPC Spawn(int raceId, int regionId, int x, int y, bool active, bool effect)
        {
            // Create NPC
            var npc = new NPC(raceId);

            // Warp to spawn point
            if (!npc.Warp(regionId, x, y))
            {
                Log.Error("Failed to spawn '{0}'s, region '{1}' doesn't exist.", raceId, regionId);
                return null;
            }

            // Save spawn location
            npc.SpawnLocation = new Location(regionId, x, y);

            // Activate AI at least once
            if (npc.AI != null && active)
                npc.AI.Activate(0);

            // Spawn effect
            if (effect)
                Send.SpawnEffect(SpawnEffect.Monster, npc.RegionId, x, y, npc, npc);

            return npc;
        }
    }

    /// <summary>
    ///     Defines area in which certain creatures are spawned, up to a certain amount.
    /// </summary>
    public class CreatureSpawner : IDisposable
    {
        private static int _id;

        private readonly List<NPC> _creatures;
        private readonly int _minX = int.MaxValue;
        private readonly int _minY = int.MaxValue;
        private readonly int _maxX;
        private readonly int _maxY;

        private readonly Point[] _points;

        private readonly int[] _titles;

        /// <summary>
        ///     Creates new CreatureSpawner
        /// </summary>
        /// <param name="raceId">Race to spawn</param>
        /// <param name="amount">Maximum amount to spawn</param>
        /// <param name="regionId">Region to spawn in</param>
        /// <param name="delay">Initial spawn delay in seconds</param>
        /// <param name="delayMin">Minimum respawn delay in seconds</param>
        /// <param name="delayMax">Maximum respawn delay in seconds</param>
        /// <param name="titles">List of random titles to apply to creatures</param>
        /// <param name="coordinates">Even number of coordinates, specifying the spawn area</param>
        public CreatureSpawner(int raceId, int amount, int regionId, int delay, int delayMin, int delayMax,
            int[] titles, int[] coordinates)
        {
            if (coordinates == null || coordinates.Length < 2 || coordinates.Length % 2 != 0)
                throw new ArgumentException("CreatureSpawner: Invalid amount of coordinates.");

            Id = Interlocked.Increment(ref _id);
            RaceId = raceId;
            Amount = (int) Math.Ceiling(amount / 2f);
            RegionId = regionId;
            Delay = delay * 1000;
            DelayMin = delayMin * 1000;
            DelayMax = delayMax * 1000;

            _titles = titles;

            _points = new Point[coordinates.Length / 2];
            for (int i = 0, j = 0; i < coordinates.Length; ++j, i += 2)
            {
                _points[j] = new Point(coordinates[i], coordinates[i + 1]);
                if (coordinates[i] < _minX) _minX = coordinates[i];
                if (coordinates[i] > _maxX) _maxX = coordinates[i];
                if (coordinates[i + 1] < _minY) _minY = coordinates[i + 1];
                if (coordinates[i + 1] > _maxY) _maxY = coordinates[i + 1];
            }

            _creatures = new List<NPC>();
        }

        /// <summary>
        ///     Unique id for the spawn.
        /// </summary>
        public int Id { get; }

        /// <summary>
        ///     Race spawned
        /// </summary>
        public int RaceId { get; }

        /// <summary>
        ///     Number of creatures spawned
        /// </summary>
        public int Amount { get; }

        /// <summary>
        ///     Region in which the creatures are spawned.
        /// </summary>
        public int RegionId { get; }

        /// <summary>
        ///     Initial spawn delay in ms
        /// </summary>
        public int Delay { get; }

        /// <summary>
        ///     Minimum respawn delay in ms
        /// </summary>
        public int DelayMin { get; }

        /// <summary>
        ///     Maximum respawn delay in ms
        /// </summary>
        public int DelayMax { get; }

        /// <summary>
        ///     Unsubscribes from spawned creature's disappear events.
        /// </summary>
        public void Dispose()
        {
            lock (_creatures)
            {
                foreach (var creature in _creatures)
                    creature.Disappears -= OnDisappears;
                _creatures.Clear();
            }
        }

        /// <summary>
        ///     Returns random spawn position.
        /// </summary>
        /// <returns></returns>
        private Point GetRandomPosition()
        {
            // Single position
            if (_points.Length == 1 || _points.All(a => a.X == _points[0].X) && _points.All(a => a.Y == _points[0].Y))
                return _points[0];

            var rnd = RandomProvider.Get();

            // Line
            if (_points.Length == 2)
            {
                var d = rnd.NextDouble();
                var x = _points[0].X + (_points[1].X - _points[0].X) * d;
                var y = _points[0].Y + (_points[1].Y - _points[0].Y) * d;
                return new Point((int) x, (int) y);
            }

            // Polygon
            var region = ChannelServer.Instance.World.GetRegion(RegionId);
            var result = new Point();
            for (var i = 0; i < 10; ++i)
            {
                while (!IsPointInside(result = new Point(rnd.Next(_minX, _maxX), rnd.Next(_minY, _maxY))))
                {
                }

                // Check if position is inside a prop.
                // Iterating over all props is not ideal, but a viable hot-fix.
                // The region's quadtree should take the entities instead,
                // so we can get only the props in a specific location.
                if (region.GetProp(a => a.IsCollision && a.IsInside(result.X, result.Y)) == null)
                    break;
            }

            return result;
        }

        /// <summary>
        ///     Returns true if point is within the spawn points.
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        private bool IsPointInside(Point point)
        {
            var result = false;

            for (int i = 0, j = _points.Length - 1; i < _points.Length; j = i++)
                if (_points[i].Y > point.Y != _points[j].Y > point.Y && point.X <
                    (_points[j].X - _points[i].X) * (point.Y - _points[i].Y) / (_points[j].Y - _points[i].Y) +
                    _points[i].X)
                    result = !result;

            return result;
        }

        /// <summary>
        ///     Spawns as many creatures as necessary to reach this spawn's amount.
        /// </summary>
        public void Spawn()
        {
            var spawnAmount = Math.Max(0, Amount - _creatures.Count);

            for (var i = 0; i < spawnAmount; ++i)
                if (Delay == 0)
                    SpawnOne();
                else
                    Task.Delay(Delay).ContinueWith(_ => SpawnOne());
        }

        /// <summary>
        ///     Spawns one creature based on this spawners settings.
        /// </summary>
        /// <returns></returns>
        private void SpawnOne()
        {
            // Create NPC
            var creature = new NPC(RaceId);

            // Warp to spawn point
            var pos = GetRandomPosition();
            if (!creature.Warp(RegionId, pos.X, pos.Y))
            {
                Log.Error("CreatureSpawner: Failed to spawn '{0}'s, region '{1}' doesn't exist.", RaceId, RegionId);
                return;
            }

            // Save spawn location
            creature.SpawnLocation = new Location(RegionId, pos.X, pos.Y);

            // Random title
            if (_titles != null && _titles.Length != 0)
            {
                var title = (ushort) _titles[RandomProvider.Get().Next(_titles.Length)];
                if (title != 0)
                {
                    creature.Titles.Enable(title);
                    creature.Titles.ChangeTitle(title, false);
                }
            }

            // Maintenance
            creature.SpawnId = Id;
            creature.Disappears += OnDisappears;

            // Add to list to keep track of all creatures
            lock (_creatures)
            {
                _creatures.Add(creature);
            }
        }

        /// <summary>
        ///     Called when an NPC created by this spawner disappears,
        ///     after it was killed.
        /// </summary>
        /// <param name="entity"></param>
        private void OnDisappears(Entity entity)
        {
            var npc = entity as NPC;
            if (npc == null)
                return;

            lock (_creatures)
            {
                _creatures.Remove(npc);
            }

            var delay = DelayMin >= 0 && DelayMax > 0 ? RandomProvider.Get().Next(DelayMin, DelayMax + 1) : 0;
            if (delay == 0)
                SpawnOne();
            else
                Task.Delay(delay).ContinueWith(_ => SpawnOne());
        }
    }
}