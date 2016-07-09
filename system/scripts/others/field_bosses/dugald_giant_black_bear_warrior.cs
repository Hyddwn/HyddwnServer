//--- Aura Script -----------------------------------------------------------
// Dugald Aisle Field Bosses
//--- Description -----------------------------------------------------------
// Handles spawning and NPC hooks for Giant Bear and Black Warrior.
//---------------------------------------------------------------------------

public class DugaldFieldBossScript : FieldBossBaseScript
{
	bool bear;

	protected override SpawnInfo GetNextSpawn()
	{
		var spawn = new SpawnInfo();

		if (Random(100) < 50)
		{
			bear = true;
			spawn.BossName = L("Giant Bear");
		}
		else
		{
			bear = false;
			spawn.BossName = L("Black Warrior");
		}

		switch (Random(4))
		{
			case 0:
				spawn.LocationName = L("South Eastern Fields of Dugald Aisle");
				spawn.Location = new Location("Uladh_Dun_to_Tircho/Field_DunTir_06/dugaldmon25a");
				break;
			case 1:
				spawn.LocationName = L("South Western Fields of Dugald Aisle");
				spawn.Location = new Location("Uladh_Dun_to_Tircho/Field_DunTir_01/dugaldmon23");
				break;
			case 2:
				spawn.LocationName = L("Southern Path of Dugald Aisle");
				spawn.Location = new Location("Uladh_Dun_to_Tircho/Field_DunTir_07/dugaldmon22");
				break;
			case 3:
				spawn.LocationName = L("Ulaid Logging Camp");
				spawn.Location = new Location("Uladh_Dun_to_Tircho/Field_DunTir_11/dugaldmon27");
				break;
		}

		spawn.Time = DateTime.Now.AddHours(Random(3, 6));
		spawn.LifeSpan = TimeSpan.FromMinutes(20);

		return spawn;
	}

	protected override void OnSpawnBosses()
	{
		if (bear)
		{
			// Giant Bear
			SpawnBoss(70016, 0, 0);

			// Brown Grizzly Bears
			SpawnMinion(70004, -800, 0);
			SpawnMinion(70004, 800, 0);
			SpawnMinion(70004, 0, -800);
			SpawnMinion(70004, 0, 800);
		}
		else
		{
			// Black Warrior
			SpawnBoss(10095, 0, 0);

			// Black Soldiers
			SpawnMinion(10005, -600, 0);
			SpawnMinion(10005, 600, 0);
			SpawnMinion(10005, 0, -600);
			SpawnMinion(10005, 0, 600);
		}

		BossNotice(L("{0} has appeared at {1}!!"), Spawn.BossName, Spawn.LocationName);
	}

	protected override void OnBossDied(Creature boss, Creature killer)
	{
		BossNotice(L("{0} has defeated {1} that appeared at {2}!"), killer.Name, Spawn.BossName, Spawn.LocationName);
	}

	//public override void Load()
	//{
	//	AddHook("_tracy", "before_keywords", TracyKeywords);
	//}

	//private async Task<HookResult> TracyKeywords(NpcScript npc, params object[] args)
	//{
	//	var keyword = args[0] as string;
	//	if (keyword == "rumor")
	//	{
	//		var spawnTime = GetTimeUntilSpawn();
	//		if (spawnTime.Ticks == 0)
	//		{
	//			npc.Msg(npc.FavorExpression(), string.Format(L(""), Spawn.BossName, Spawn.LocationName));
	//		}
	//		else if (spawnTime.TotalMinutes < 100)
	//		{
	//			var time = GetTimeSpanString(ErinnTime.Now, new ErinnTime(Spawn.Time));
	//			npc.Msg(npc.FavorExpression(), string.Format(L(""), Spawn.BossName, Spawn.LocationName, time));
	//		}
	//	}

	//	return HookResult.Continue;
	//}
}
