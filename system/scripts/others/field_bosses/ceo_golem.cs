//--- Aura Script -----------------------------------------------------------
// Ceo Island Field Boss
//--- Description -----------------------------------------------------------
// Handles spawning and NPC hooks for Ceo Island Golem Boss
//---------------------------------------------------------------------------

public class GolemFieldBossScript : FieldBossBaseScript
{
	protected override SpawnInfo GetNextSpawn()
	{
		var spawn = new SpawnInfo();

		spawn.BossName = L("Golem");

		spawn.LocationName = L("Southeast Ceo Island");
		spawn.Location = new Location("Ula_Emainmacha_Ceo/_Ula_Emainmacha_Ceo_E2/mon121");

		spawn.Time = DateTime.Now.AddHours(Random(3, 6));
		spawn.LifeSpan = TimeSpan.FromMinutes(30);

		return spawn;
	}

	protected virtual bool ShouldSpawn()
	{
		// Ceo's availability is tied to Emain Macha right now
		return IsEnabled("EmainMacha");
	}

	protected override void OnSpawnBosses()
	{
		// Golem (Black)
		SpawnBoss(130046, 0, 0);

		// Golems (Red)
		SpawnMinion(130006, 0, 1000);
		SpawnMinion(130006, 0, -1000);
		SpawnMinion(130006, 1000, 0);
		SpawnMinion(130006, -1000, 0);

		BossNotice(L("{0} has appeared at {1}!!"), Spawn.BossName, Spawn.LocationName);
	}

	protected override void OnBossDied(Creature boss, Creature killer)
	{
		BossNotice(L("{0} has defeated {1} that appeared at {2}!"), killer.Name, Spawn.BossName, Spawn.LocationName);
	}

	//TODO: Add NPC hooks for Golem Field Boss 
}
