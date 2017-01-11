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
	}
}
