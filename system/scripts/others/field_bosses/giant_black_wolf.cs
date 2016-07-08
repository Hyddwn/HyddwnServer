using Aura.Channel.World;
using Aura.Channel.World.Entities;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public class GiantBlackWolfScript : FieldBossBaseScript
{
	protected override SpawnInfo GetNextSpawn()
	{
		var spawn = new SpawnInfo();

		spawn.BossName = L("Giant Black Wolf");
		spawn.LocationName = L("Tir Chonaill Square");
		spawn.Location = new Location(1, 12800, 38100);
		spawn.Time = DateTime.Now.AddMinutes(Random(1, 1));
		spawn.LifeSpan = TimeSpan.FromMinutes(1);

		return spawn;
	}

	protected override void OnSpawnBosses()
	{
		SpawnBoss(20041, 300, -150);
		SpawnBoss(20041, -300, -150);
		SpawnBoss(20041, 0, 300);

		BossNotice(L("{0} has appeared at {1}!!"), Spawn.BossName, Spawn.LocationName);
	}

	protected override void OnBossDied(Creature boss, Creature killer)
	{
		BossNotice(L("{0} has defeated {1} that appeared at {2}!"), killer.Name, Spawn.BossName, Spawn.LocationName);
	}
}
