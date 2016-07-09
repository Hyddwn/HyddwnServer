//--- Aura Script -----------------------------------------------------------
// Gairech Field Bosses
//--- Description -----------------------------------------------------------
// Handles spawning and NPC hooks for Giant Ogre and Black Wizard.
//---------------------------------------------------------------------------

public class GairechFieldBossScript : FieldBossBaseScript
{
	bool ogre;

	protected override SpawnInfo GetNextSpawn()
	{
		var spawn = new SpawnInfo();

		if (Random(100) < 50)
		{
			ogre = true;
			spawn.BossName = L("Giant Ogre");
		}
		else
		{
			ogre = false;
			spawn.BossName = L("Black Wizard");
		}

		switch (Random(6))
		{
			case 0:
				spawn.LocationName = L("Reighinalt");
				spawn.Location = new Location("Ula_Dun_to_Bangor/Field_DunBan_S2/mon64");
				break;
			case 1:
				spawn.LocationName = L("Dragon Statue Excavation Site");
				spawn.Location = new Location("Ula_Dun_to_Bangor/Field_DunBan_Center/mon57");
				break;
			case 2:
				spawn.LocationName = L("Path to Dunbarton");
				spawn.Location = new Location("Ula_Dun_to_Bangor/Field_DunBan_N4/mon52");
				break;
			case 3:
				spawn.LocationName = L("Eastern Fields of Gairech");
				spawn.Location = new Location("Ula_Dun_to_Bangor/Field_DunBan_N3/mon53");
				break;
			case 4:
				spawn.LocationName = L("Eastern Fields of Gairech");
				spawn.Location = new Location("Ula_Dun_to_Bangor/Field_DunBan_N3/mon54");
				break;
			case 5:
				spawn.LocationName = L("Eastern Fields of Gairech");
				spawn.Location = new Location("Ula_Dun_to_Bangor/Field_DunBan_N3/mon55");
				break;
		}

		spawn.Time = DateTime.Now.AddHours(Random(3, 6));
		spawn.LifeSpan = TimeSpan.FromMinutes(20);

		return spawn;
	}

	protected override void OnSpawnBosses()
	{
		if (ogre)
		{
			// Giant Ogre
			SpawnBoss(100023, 0, 0);

			// Skeleton Soldier
			SpawnMinion(11004, -550, 0);
			SpawnMinion(11004, 550, 0);
			SpawnMinion(11004, 0, -550);
			SpawnMinion(11004, 0, 550);
			SpawnMinion(11004, -400, -400);
			SpawnMinion(11004, 400, -400);
			SpawnMinion(11004, -400, 400);
			SpawnMinion(11004, 400, 400);
		}
		else
		{
			// Black Wizard
			SpawnBoss(10096, 0, 0);

			// Wisps
			SpawnMinion(80101, -550, 0);
			SpawnMinion(80101, 550, 0);
			SpawnMinion(80101, 0, -550);
			SpawnMinion(80101, 0, 550);
			SpawnMinion(80101, -400, -400);
			SpawnMinion(80101, 400, -400);
			SpawnMinion(80101, -400, 400);
			SpawnMinion(80101, 400, 400);
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
