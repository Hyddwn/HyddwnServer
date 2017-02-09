//--- Aura Script -----------------------------------------------------------
// Osna Sail Field Boss
//--- Description -----------------------------------------------------------
// Handles spawning and NPC hooks for the Mini Bear field boss
//---------------------------------------------------------------------------

public class MiniBearFieldBossScript : FieldBossBaseScript
{
	protected override SpawnInfo GetNextSpawn()
	{
		var spawn = new SpawnInfo();

		spawn.BossName = L("Mini Bear");

		if (Random(100) < 50)
		{
			spawn.LocationName = L("East Osna Sail");
			spawn.Location = new Location("Ula_Osna_Sail/_Ula_Osna_Sail_C/mon_184");
		}
		else
		{
			spawn.LocationName = L("West Osna Sail");
			spawn.Location = new Location("Ula_Osna_Sail/_Ula_Osna_Sail_C/mon_188");
		}

		spawn.Time = DateTime.Now.AddHours(Random(3, 6));
		spawn.LifeSpan = TimeSpan.FromMinutes(30);

		return spawn;
	}

	protected virtual bool ShouldSpawn()
	{
		return IsEnabled("G2FieldBosses");
	}

	protected override void OnSpawnBosses()
	{
		// Brown Bear (Mini)
		SpawnBoss(70102, 0, 0, 30002);

		// Sheep Wolves
		SpawnMinion(20020, 0, 300, 30001);
		SpawnMinion(20020, 0, -300, 30004);
		SpawnMinion(20020, 300, 0, 30005);
		SpawnMinion(20020, -300, 0, 30007);

		BossNotice(L("{0} has appeared at {1}!!"), Spawn.BossName, Spawn.LocationName);
	}

	protected override void OnBossDied(Creature boss, Creature killer)
	{
		BossNotice(L("{0} has defeated {1} that appeared at {2}!"), killer.Name, Spawn.BossName, Spawn.LocationName);
	}

	//TODO: Add NPC hooks for Mini Bear Field Boss 
}
