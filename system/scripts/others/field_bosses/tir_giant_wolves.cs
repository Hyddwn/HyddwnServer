using Aura.Channel.World;
using Aura.Channel.World.Entities;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
}
