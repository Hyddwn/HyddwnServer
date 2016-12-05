//--- Aura Script -----------------------------------------------------------
// Field Boss base class
//--- Description -----------------------------------------------------------
// Provides a base for Field Boss scripts, with common methods and
// automated handling of spawning and dropping Fomor Command Scrolls.
//---------------------------------------------------------------------------

public abstract class FieldBossBaseScript : GeneralScript
{
	private List<Creature> _bosses = new List<Creature>();
	private List<Creature> _minions = new List<Creature>();
	private object _syncLock = new object();
	private bool _disposed = false;
	private bool _droppedScroll = false;

	protected bool AllBossesAreDead { get { lock (_syncLock) return _bosses.All(b => b.IsDead); } }
	protected new SpawnInfo Spawn { get; private set; }

	protected override void CleanUp()
	{
		_disposed = true;
	}

	public override bool Init()
	{
		Spawn = new SpawnInfo();

		Load();
		PrepareSpawn();

		return true;
	}

	[On("CreatureKilled")]
	public void OnCreatureKilled(Creature creature, Creature killer)
	{
		// Don't drop anything if nothing should spawn
		if (!ShouldSpawn())
			return;

		// Only drop once, from NPCs in the region
		if (_droppedScroll || creature.RegionId != Spawn.Location.RegionId || !creature.Has(CreatureStates.Npc))
			return;

		// Start dropping 100 minutes before spawn and stop when spawned.
		var time = GetTimeUntilSpawn();
		if (time.TotalMinutes >= 100 || time.Ticks == 0)
			return;

		// Chance = (100 - remaining minutes) / 4
		// 90 minutes =  2.5% chance
		// 60 minutes = 10.0% chance
		// 20 minutes = 20.0% chance
		//  2 minutes = 24.5% chance
		var chance = (100 - time.TotalMinutes) / 4;

		// Don't lock until here, to save time
		lock (_syncLock)
		{
			// Check again, for race conditions
			if (_droppedScroll)
				return;

			if (Random(100) < chance)
			{
				creature.Drops.Add(CreateFomorCommandScroll());
				_droppedScroll = true;
			}
		}
	}

	private void PrepareSpawn()
	{
		Spawn = GetNextSpawn();
		_droppedScroll = false;

		var delay = GetTimeUntilSpawn();
		Task.Delay(delay).ContinueWith(a =>
		{
			if (_disposed)
				return;

			SpawnAll();
		});
	}

	private void SpawnAll()
	{
		if (ShouldSpawn())
			OnSpawnBosses();

		// Get number of bosses spawned, so we can check for it when it comes
		// to preparing the next spawn. If no bosses were spawned, they can't
		// die, and as such, the following tasks has to prepare the next one,
		// even if all bosses are technically dead, because there aren't any.
		var spawnCount = 0;
		lock (_syncLock)
			spawnCount = _bosses.Count;

		Task.Delay(Spawn.LifeSpan).ContinueWith(a =>
		{
			if (_disposed)
				return;

			var allDead = AllBossesAreDead;

			DespawnAll();

			if (!allDead || spawnCount == 0)
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

	protected Item CreateFomorCommandScroll()
	{
		var item = new Item(63021);
		item.MetaData1.SetString("BSGRNM", Spawn.BossName);
		item.MetaData1.SetString("BSGRPS", Spawn.LocationName);
		item.MetaData1.SetLong("BSGRTM", Spawn.Time);

		return item;
	}

	protected TimeSpan GetTimeUntilSpawn()
	{
		var now = DateTime.Now;
		var spawnTime = Spawn.Time;

		if (spawnTime < now)
			return new TimeSpan();

		return (spawnTime - now);
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

	protected virtual bool ShouldSpawn()
	{
		return true;
	}

	protected void ContributorDrops(Creature boss, List<DropData> drops)
	{
		var hitters = boss.GetAllHitters();
		var rnd = RandomProvider.Get();

		foreach (var hitter in hitters)
		{
			var pos = hitter.GetPosition();
			var item = Item.GetRandomDrop(rnd, drops);
			var region = hitter.Region;

			item.Drop(region, pos, 100);
		}
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
