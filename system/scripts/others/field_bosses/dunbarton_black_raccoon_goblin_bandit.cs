//--- Aura Script -----------------------------------------------------------
// Dunbarton Field Bosses
//--- Description -----------------------------------------------------------
// Handles spawning and NPC hooks for Goblin Bandit and Black Raccoon.
//---------------------------------------------------------------------------

public class DunbartonFieldBossScript : FieldBossBaseScript
{
	bool goblin;

	protected override SpawnInfo GetNextSpawn()
	{
		var spawn = new SpawnInfo();

		if (Random(100) < 50)
		{
			goblin = true;
			spawn.BossName = L("Goblin Bandit");
		}
		else
		{
			goblin = false;
			spawn.BossName = L("Black Raccoon");
		}

		switch (Random(4))
		{
			case 0:
				spawn.LocationName = L("North Eastern Fields of Dunbarton");
				spawn.Location = new Location("Uladh_Dunbarton/field_Dunbarton_02/dunbmon33");
				break;
			case 1:
				spawn.LocationName = L("Southern Fields of Dunbarton");
				spawn.Location = new Location("Uladh_Dunbarton/field_Dunbarton_p3/dunbmon40");
				break;
			case 2:
				spawn.LocationName = L("North Eastern Fields of Dunbarton");
				spawn.Location = new Location("Uladh_Dunbarton/field_Dunbarton_02/dunbmon47");
				break;
			case 3:
				spawn.LocationName = L("Eastern Fields of Dunbarton");
				spawn.Location = new Location("Uladh_Dunbarton/field_Dunbarton_05/dunbmon35");
				break;
		}

		spawn.Time = DateTime.Now.AddHours(Random(3, 6));
		spawn.LifeSpan = TimeSpan.FromMinutes(20);

		return spawn;
	}

	protected override void OnSpawnBosses()
	{
		if (goblin)
		{
			// Goblin Bandits
			SpawnBoss(10179, 0, 0);
			SpawnBoss(10179, -250, 0);
			SpawnBoss(10179, 250, 0);
			SpawnBoss(10179, 0, -250);
			SpawnBoss(10179, 0, 250);
			SpawnBoss(10179, -150, -150);
			SpawnBoss(10179, 150, -150);
			SpawnBoss(10179, -150, 150);
			SpawnBoss(10179, 150, 150);
		}
		else
		{
			// Black Raccoon
			SpawnBoss(50107, 0, 0);

			// Young Black Raccoon
			SpawnMinion(50105, -250, 0);
			SpawnMinion(50105, 250, 0);
			SpawnMinion(50105, 0, -250);
			SpawnMinion(50105, 0, 250);
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
