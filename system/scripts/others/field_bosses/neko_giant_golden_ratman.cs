//--- Aura Script -----------------------------------------------------------
// Nekojima Field Boss
//--- Description -----------------------------------------------------------
// Handles spawning and NPC hooks for Nekojima's Golden Ratman field boss
//---------------------------------------------------------------------------

public class GiantGoldenRatmanFieldBossScript : FieldBossBaseScript
{
	protected override SpawnInfo GetNextSpawn()
	{
		var spawn = new SpawnInfo();

		spawn.BossName = L("Giant Golden Ratman");

		spawn.LocationName = L("Southern Coast of Nekojima");
		spawn.Location = new Location("JP_Nekojima_islet/_JP_Nekojima_islet003/mon_Nekojima_islet_003_001");

		spawn.Time = DateTime.Now.AddHours(Random(3, 6));
		spawn.LifeSpan = TimeSpan.FromMinutes(30);

		return spawn;
	}

	protected virtual bool ShouldSpawn()
	{
		return IsEnabled("Nekojima");
	}

	protected override void OnSpawnBosses()
	{
		// Giant Golden Ratman
		SpawnBoss(900022, 0, 0);

		// Dark Ratmen
		SpawnMinion(170303, -600, 0);
		SpawnMinion(170303, -300, 300);
		SpawnMinion(170303, -300, -300);
		SpawnMinion(170303, 0, 600);
		SpawnMinion(170303, 0, -600);
		SpawnMinion(170303, -300, 300);
		SpawnMinion(170303, 300, 300);

		BossNotice(L("{0} has appeared at {1}!!"), Spawn.BossName, Spawn.LocationName);
	}

	protected override void OnBossDied(Creature boss, Creature killer)
	{
		BossNotice(L("{0} has defeated {1} that appeared at {2}!"), killer.Name, Spawn.BossName, Spawn.LocationName);
		ContributorDrops(boss, GetContributorDrops());
	}

	List<DropData> drops;
	public List<DropData> GetContributorDrops()
	{
		if (drops == null)
		{
			drops = new List<DropData>();

			drops.Add(new DropData(itemId: 51003, chance: 20, amount: 6)); // HP 50 Potion
			drops.Add(new DropData(itemId: 51004, chance: 10, amount: 6)); // HP 100 Potion
			drops.Add(new DropData(itemId: 51008, chance: 20, amount: 6)); // MP 50 Potion
			drops.Add(new DropData(itemId: 51009, chance: 10, amount: 6)); // MP 100 Potion
			drops.Add(new DropData(itemId: 51013, chance: 20, amount: 3)); // Stamina 50 Potion
			drops.Add(new DropData(itemId: 51014, chance: 10, amount: 3)); // Stamina 100 Potion
			drops.Add(new DropData(itemId: 51016, chance: 20, amount: 3)); // Wound Remedy 10 Potion
			drops.Add(new DropData(itemId: 51017, chance: 10, amount: 3)); // Wound Remedy 30 Potion
			drops.Add(new DropData(itemId: 91108, chance: 80, amount: 5)); // Cat Bell
		}

		return drops;
	}
}
