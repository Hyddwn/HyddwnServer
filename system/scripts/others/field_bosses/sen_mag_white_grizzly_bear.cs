//--- Aura Script -----------------------------------------------------------
// Sen Mag Field Boss
//--- Description -----------------------------------------------------------
// Handles spawning and NPC hooks for the White Grizzly Bear field boss
//---------------------------------------------------------------------------

public class WhiteGrizzlyBearFieldBossScript : FieldBossBaseScript
{
	protected override SpawnInfo GetNextSpawn()
	{
		var spawn = new SpawnInfo();

		spawn.BossName = L("White Grizzly Bear");

		switch (Random(5))
		{
			case 0:
				spawn.LocationName = L("South Sen Mag Plains");
				spawn.Location = new Location("Ula_Sen_Mag/_Ula_Sen_Mag_S_p/mon134");
				break;
			case 1:
				spawn.LocationName = L("South Sen Mag Plains");
				spawn.Location = new Location("Ula_Sen_Mag/_Ula_Sen_Mag_S_p/mon137");
				break;
			case 2:
				spawn.LocationName = L("West Sen Mag Plains");
				spawn.Location = new Location("Ula_Sen_Mag/_Ula_Sen_Mag_C2/mon132");
				break;
			case 3:
				spawn.LocationName = L("North-West Sen Mag Plains");
				spawn.Location = new Location("Ula_Sen_Mag/_Ula_Sen_Mag_C2/mon130");
				break;
			case 4:
				spawn.LocationName = L("North-East Sen Mag Plains");
				spawn.Location = new Location("Ula_Sen_Mag/_Ula_Sen_Mag_C1/mon131");
				break;
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
		// White Grizzly Bear
		SpawnBoss(70017, 0, 0, 30002);

		// Black Grizzly Bears and Black Grizzly Bear Cubs
		SpawnMinion(70006, 0, 1000, 30001);
		SpawnMinion(70006, 0, -1000, 30004);
		SpawnMinion(70006, 1000, 0, 30005);
		SpawnMinion(70006, -1000, 0, 30007);
		SpawnMinion(70009, 500, 500, 30001);
		SpawnMinion(70009, 500, -500, 30001);

		BossNotice(L("{0} has appeared at {1}!!"), Spawn.BossName, Spawn.LocationName);
	}

	protected override void OnBossDied(Creature boss, Creature killer)
	{
		BossNotice(L("{0} has defeated {1} that appeared at {2}!"), killer.Name, Spawn.BossName, Spawn.LocationName);
	}

	//TODO: Add NPC hooks for White Grizzly Bear Field Boss 
}
