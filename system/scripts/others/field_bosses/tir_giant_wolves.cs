//--- Aura Script -----------------------------------------------------------
// Tir Chonaill Field Bosses
//--- Description -----------------------------------------------------------
// Handles spawning and NPC hooks for Giant Black Wolf and Giant White Wolf.
//---------------------------------------------------------------------------

public class GiantWolvesFieldBossScript : FieldBossBaseScript
{
	bool white;

	protected override SpawnInfo GetNextSpawn()
	{
		var spawn = new SpawnInfo();

		if (Random(100) < 50)
		{
			white = true;
			spawn.BossName = L("Giant White Wolf");
		}
		else
		{
			white = false;
			spawn.BossName = L("Giant Black Wolf");
		}

		if (Random(100) < 50)
		{
			spawn.LocationName = L("Southern Fields of Tir Chonaill");
			spawn.Location = new Location("Uladh_main/field_Tir_S_aa/TirChonaill_monster3");
		}
		else
		{
			spawn.LocationName = L("Eastern Fields of Tir Chonaill");
			spawn.Location = new Location("Uladh_main/field_Tir_E_ba/TirChonaill_monster6");
		}

		spawn.Time = DateTime.Now.AddHours(Random(3, 6));
		spawn.LifeSpan = TimeSpan.FromMinutes(20);

		return spawn;
	}

	protected override void OnSpawnBosses()
	{
		if (white)
		{
			// Giant White Wolves
			SpawnBoss(20040, 200, 0);
			SpawnBoss(20040, -200, 0);

			// White Wolves
			SpawnMinion(20003, -550, 0);
			SpawnMinion(20003, 550, 0);
			SpawnMinion(20003, -250, -400);
			SpawnMinion(20003, 250, -400);
			SpawnMinion(20003, -250, 400);
			SpawnMinion(20003, 250, 400);
		}
		else
		{
			// Giant Black Wolves
			SpawnBoss(20041, 250, -125);
			SpawnBoss(20041, -250, -125);
			SpawnBoss(20041, 0, 250);

			// Black Wolves
			SpawnMinion(20002, -550, 0);
			SpawnMinion(20002, 550, 0);
			SpawnMinion(20002, 0, -550);
			SpawnMinion(20002, 0, 550);
			SpawnMinion(20002, -400, -400);
			SpawnMinion(20002, 400, -400);
			SpawnMinion(20002, -400, 400);
			SpawnMinion(20002, 400, 400);
		}

		BossNotice(L("{0} has appeared at {1}!!"), Spawn.BossName, Spawn.LocationName);
	}

	protected override void OnBossDied(Creature boss, Creature killer)
	{
		BossNotice(L("{0} has defeated {1} that appeared at {2}!"), killer.Name, Spawn.BossName, Spawn.LocationName);
	}

	public override void Load()
	{
		AddHook("_duncan", "before_keywords", DuncanKeywords);
		AddHook("_deian", "before_keywords", DeianKeywords);
		AddHook("_dilys", "before_keywords", DilysKeywords);
	}

	private async Task<HookResult> DuncanKeywords(NpcScript npc, params object[] args)
	{
		var keyword = args[0] as string;
		if (keyword == "rumor")
		{
			var spawnTime = GetTimeUntilSpawn();
			if (spawnTime.Ticks == 0)
			{
				npc.Msg(npc.FavorExpression(), string.Format(L("Why are you here?<br/>I saw people running to {1}.<br/>They were running to save their friends in peril after {0} showed up."), Spawn.BossName, Spawn.LocationName));
			}
			else if (spawnTime.TotalMinutes < 100)
			{
				var time = GetTimeSpanString(ErinnTime.Now, new ErinnTime(Spawn.Time));
				npc.Msg(npc.FavorExpression(), string.Format(L("I have something to tell you.<br/>Can you feel the evil presence of {0} spreading around {1}?<br/>I think something bad will happen in around {2}..."), Spawn.BossName, Spawn.LocationName, time));
			}
		}

		return HookResult.Continue;
	}

	private async Task<HookResult> DeianKeywords(NpcScript npc, params object[] args)
	{
		var keyword = args[0] as string;
		if (keyword == "rumor")
		{
			var spawnTime = GetTimeUntilSpawn();
			if (spawnTime.Ticks == 0)
			{
				// ?
			}
			else if (spawnTime.TotalMinutes < 100)
			{
				var time = GetTimeSpanString(ErinnTime.Now, new ErinnTime(Spawn.Time));
				npc.Msg(npc.FavorExpression(), string.Format(L("A monster will show up in {1} at {2}!<br/>{0} will show up!<br/>Hey, I said I'm not lying!"), Spawn.BossName, Spawn.LocationName, time));
			}
		}

		return HookResult.Continue;
	}

	private async Task<HookResult> DilysKeywords(NpcScript npc, params object[] args)
	{
		var keyword = args[0] as string;
		if (keyword == "rumor")
		{
			var spawnTime = GetTimeUntilSpawn();
			if (spawnTime.Ticks == 0)
			{
				npc.Msg(npc.FavorExpression(), string.Format(L("Head to {1} right away!<br/>Trefor made a fuss because of {0}'s attack."), Spawn.BossName, Spawn.LocationName));
			}
			else if (spawnTime.TotalMinutes < 100)
			{
				// ?
			}
		}

		return HookResult.Continue;
	}
}
