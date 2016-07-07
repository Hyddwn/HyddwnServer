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
	protected override TimeSpan GetSpawnDelay()
	{
		return TimeSpan.FromMinutes(1);
	}

	protected override TimeSpan GetLifeSpan()
	{
		return TimeSpan.FromMinutes(1);
	}

	protected override void OnSpawnBosses()
	{
		var rnd = RandomProvider.Get();
		var pos = new Position(12800, 38100);

		SpawnBoss(20041, 1, pos.GetRandomInRange(750, rnd));
		SpawnBoss(20041, 1, pos.GetRandomInRange(750, rnd));

		BossNotice(1, L("Giant Black Wolf has appeared at Tir Chonaill Square!!"));
	}

	protected override void OnBossDied(Creature boss, Creature killer)
	{
		BossNotice(1, L("{0} has defeated Giant Black Wolf that appeared at Tir Chonaill Square!"), killer.Name);
	}
}
