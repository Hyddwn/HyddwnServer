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
	private DateTime _lastSpawn = DateTime.Now;
	private DateTime _nextSpawn;
	private bool _disposed = false;

	protected bool AllBossesAreDead { get { lock (_syncLock) return _bosses.All(b => b.IsDead); } }

	protected override void CleanUp()
	{
		_disposed = true;
	}

	public override bool Init()
	{
		PrepareSpawn();

		return true;
	}

	private void PrepareSpawn()
	{
		var delay = GetSpawnDelay();
		Task.Delay(delay).ContinueWith(a =>
		{
			if (_disposed)
				return;

			SpawnAll();
		});

		_nextSpawn = DateTime.Now.Add(delay);
	}

	private void SpawnAll()
	{
		OnSpawnBosses();

		var lifeSpan = GetLifeSpan();
		Task.Delay(lifeSpan).ContinueWith(a =>
		{
			if (_disposed)
				return;

			var allDead = AllBossesAreDead;

			DespawnAll();

			if (!allDead)
				PrepareSpawn();
		});

		_lastSpawn = DateTime.Now;
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

	protected Creature SpawnBoss(int raceId, int regionId, Position pos)
	{
		var npc = new NPC(raceId);
		npc.Death += this.OnBossDeath;

		npc.Spawn(regionId, pos.X, pos.Y);
		Send.SpawnEffect(SpawnEffect.Monster, regionId, pos.X, pos.Y, npc, npc);

		lock (_syncLock)
			_bosses.Add(npc);

		return npc;
	}

	protected Creature SpawnMinion(int raceId, int regionId, Position pos)
	{
		var npc = new NPC(raceId);
		npc.Death += this.OnMinionDied;

		npc.Spawn(regionId, pos.X, pos.Y);
		Send.SpawnEffect(SpawnEffect.Monster, regionId, pos.X, pos.Y, npc, npc);

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

	protected void BossNotice(int regionId, string format, params object[] args)
	{
		var region = ChannelServer.Instance.World.GetRegion(regionId);
		if (region == null)
		{
			Log.Error("{0}.BossNotice: Region '{1}' not found.", GetType().Name, regionId);
			return;
		}

		Send.Notice(region, NoticeType.Top, 16000, format, args);
	}

	protected abstract TimeSpan GetSpawnDelay();

	protected abstract TimeSpan GetLifeSpan();

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
}
