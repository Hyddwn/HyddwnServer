using Aura.Channel;
using Aura.Channel.Network.Sending;
using Aura.Channel.Scripting.Scripts;
using Aura.Channel.World;
using Aura.Channel.World.Entities;
using Aura.Mabi.Const;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

public abstract class FieldBossBaseScript : GeneralScript
{
	private List<Creature> _bosses = new List<Creature>();
	private List<Creature> _minions = new List<Creature>();
	private object _syncLock = new object();
	private bool _disposed = false;

	protected bool AllBossesAreDead { get { lock (_syncLock) return _bosses.All(b => b.IsDead); } }
	protected new SpawnInfo Spawn { get; private set; }

	protected override void CleanUp()
	{
		_disposed = true;
	}

	public override bool Init()
	{
		Spawn = new SpawnInfo();

		PrepareSpawn();

		return true;
	}

	private void PrepareSpawn()
	{
		Spawn = GetNextSpawn();

		var delay = Spawn.Time - DateTime.Now;
		Task.Delay(delay).ContinueWith(a =>
		{
			if (_disposed)
				return;

			SpawnAll();
		});
	}

	private void SpawnAll()
	{
		OnSpawnBosses();

		Task.Delay(Spawn.LifeSpan).ContinueWith(a =>
		{
			if (_disposed)
				return;

			var allDead = AllBossesAreDead;

			DespawnAll();

			if (!allDead)
				PrepareSpawn();
		});
	}

	private void DespawnAll()
	{
		lock (_syncLock)
		{
			foreach (var creature in _bosses.Union(_minions).Where(a => !a.IsDead))
			{
				var loc = creature.GetLocation();
				Send.SpawnEffect(SpawnEffect.MonsterDespawn, loc.RegionId, loc.X, loc.Y, creature, creature);
				creature.Disappear();
			}

			_bosses.Clear();
			_minions.Clear();
		}
	}

	protected Creature SpawnBoss(int raceId, int xOffset, int yOffset)
	{
		var regionId = Spawn.Location.RegionId;
		var x = Spawn.Location.X + xOffset;
		var y = Spawn.Location.Y + yOffset;

		var npc = new NPC(raceId);
		npc.Death += this.OnBossDeath;

		npc.Spawn(regionId, x, y);
		Send.SpawnEffect(SpawnEffect.Monster, regionId, x, y, npc, npc);

		lock (_syncLock)
			_bosses.Add(npc);

		return npc;
	}

	protected Creature SpawnMinion(int raceId)
	{
		var xOffset = Random(-750, 750);
		var yOffset = Random(-750, 750);

		return SpawnMinion(raceId, xOffset, yOffset);
	}

	protected Creature SpawnMinion(int raceId, int xOffset, int yOffset)
	{
		var regionId = Spawn.Location.RegionId;
		var x = Spawn.Location.X + xOffset;
		var y = Spawn.Location.Y + yOffset;

		var npc = new NPC(raceId);
		npc.Death += this.OnMinionDied;

		npc.Spawn(regionId, x, y);
		Send.SpawnEffect(SpawnEffect.Monster, regionId, x, y, npc, npc);

		lock (_syncLock)
			_minions.Add(npc);

		return npc;
	}

	private void OnBossDeath(Creature boss, Creature killer)
	{
		lock (_syncLock)
		{
			OnBossDied(boss, killer);

			if (AllBossesAreDead)
			{
				DespawnAll();
				PrepareSpawn();
				OnAllBossesDied();
			}
		}
	}

	protected void BossNotice(string format, params object[] args)
	{
		var region = ChannelServer.Instance.World.GetRegion(Spawn.Location.RegionId);
		if (region == null)
		{
			Log.Error("{0}.BossNotice: Region '{1}' not found.", GetType().Name, Spawn.Location.RegionId);
			return;
		}

		Send.Notice(region, NoticeType.Top, 16000, format, args);
	}

	protected abstract SpawnInfo GetNextSpawn();

	protected abstract void OnSpawnBosses();

	protected virtual void OnBossDied(Creature boss, Creature killer)
	{
	}

	protected virtual void OnAllBossesDied()
	{
	}

	protected virtual void OnMinionDied(Creature boss, Creature killer)
	{
	}

	protected class SpawnInfo
	{
		public string BossName { get; set; }
		public string LocationName { get; set; }
		public Location Location { get; set; }
		public DateTime Time { get; set; }
		public TimeSpan LifeSpan { get; set; }

		public SpawnInfo()
		{
			// Dummy values
			BossName = "";
			LocationName = "";
			Location = new Location(1, 12800, 38100);
			Time = DateTime.Now.AddMinutes(1);
			LifeSpan = TimeSpan.FromMinutes(1);
		}
	}
}
