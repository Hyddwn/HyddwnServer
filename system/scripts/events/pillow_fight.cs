//--- Aura Script -----------------------------------------------------------
// 100% Repair Rate Event
//--- Description -----------------------------------------------------------
// Spawns field bosses that fight pillows and and can only be damaged by
// pillows as well.
// 
// Reference: http://wiki.mabinogiworld.com/view/Pillow_Fight_Event_(2013)
//---------------------------------------------------------------------------

public class PillowFightEventScript : GameEventScript
{
	public override void Load()
	{
		SetId("aura_pillow_fight");
		SetName(L("Pillow Fight"));
	}

	public override void AfterLoad()
	{
		ScheduleEvent(DateTime.Parse("2013-05-22 00:00"), DateTime.Parse("2016-06-12 00:00"));
	}

	protected override void OnStart()
	{
	}

	protected override void OnEnd()
	{
	}
}

public abstract class PillowFightFieldBaseBossScript : FieldBossBaseScript
{
	protected override SpawnInfo GetNextSpawn()
	{
		var spawn = new SpawnInfo();
		spawn.BossName = L("Pillow Fighters");
		spawn.Time = ErinnTime.GetNextTime(19, 0).DateTime;
		spawn.LifeSpan = TimeSpan.FromMinutes(6);

		return spawn;
	}

	protected override bool ShouldSpawn()
	{
		return IsEventActive("aura_pillow_fight");
	}

	protected override void OnSpawnBosses()
	{
		var s = 200; // space
		var hs = s / 2; // half space

		// Spawn them in a 6x6 grid, for 2x 18 available pillow monsters
		SpawnBoss(191101, -hs - s * 2, -hs - s * 2); SpawnBoss(191107, -hs - s * 1, -hs - s * 2); SpawnBoss(191113, -hs - s * 0, -hs - s * 2); SpawnBoss(191101, +hs + s * 0, -hs - s * 2); SpawnBoss(191107, +hs + s * 1, -hs - s * 2); SpawnBoss(191113, +hs + s * 2, -hs - s * 2);
		SpawnBoss(191102, -hs - s * 2, -hs - s * 1); SpawnBoss(191108, -hs - s * 1, -hs - s * 1); SpawnBoss(191114, -hs - s * 0, -hs - s * 1); SpawnBoss(191102, +hs + s * 0, -hs - s * 1); SpawnBoss(191108, +hs + s * 1, -hs - s * 1); SpawnBoss(191114, +hs + s * 2, -hs - s * 1);
		SpawnBoss(191103, -hs - s * 2, -hs - s * 0); SpawnBoss(191109, -hs - s * 1, -hs - s * 0); SpawnBoss(191115, -hs - s * 0, -hs - s * 0); SpawnBoss(191103, +hs + s * 0, -hs - s * 0); SpawnBoss(191109, +hs + s * 1, -hs - s * 0); SpawnBoss(191115, +hs + s * 2, -hs - s * 0);
		SpawnBoss(191104, -hs - s * 2, +hs + s * 0); SpawnBoss(191110, -hs - s * 1, +hs + s * 0); SpawnBoss(191116, -hs - s * 0, +hs + s * 0); SpawnBoss(191104, +hs + s * 0, +hs + s * 0); SpawnBoss(191110, +hs + s * 1, +hs + s * 0); SpawnBoss(191116, +hs + s * 2, +hs + s * 0);
		SpawnBoss(191105, -hs - s * 2, +hs + s * 1); SpawnBoss(191111, -hs - s * 1, +hs + s * 1); SpawnBoss(191117, -hs - s * 0, +hs + s * 1); SpawnBoss(191105, +hs + s * 0, +hs + s * 1); SpawnBoss(191111, +hs + s * 1, +hs + s * 1); SpawnBoss(191117, +hs + s * 2, +hs + s * 1);
		SpawnBoss(191106, -hs - s * 2, +hs + s * 2); SpawnBoss(191112, -hs - s * 1, +hs + s * 2); SpawnBoss(191118, -hs - s * 0, +hs + s * 2); SpawnBoss(191106, +hs + s * 0, +hs + s * 2); SpawnBoss(191112, +hs + s * 1, +hs + s * 2); SpawnBoss(191118, +hs + s * 2, +hs + s * 2);

		BossNotice(L("Pillow Fighters have appeared at {1}!!"), Spawn.BossName, Spawn.LocationName);
	}

	protected override void OnBossDied(Creature boss, Creature killer)
	{
		BossNotice(L("{0} has defeated Pillow Fighter that appeared at {2}!"), killer.Name, Spawn.BossName, Spawn.LocationName);
		ContributorDrops(boss, GetContributorDrops());
	}

	List<DropData> drops;
	public List<DropData> GetContributorDrops()
	{
		if (drops == null)
		{
			drops = new List<DropData>();

			drops.Add(new DropData(itemId: 2000, chance: 200, amount: 10)); // Gold
			drops.Add(new DropData(itemId: 75512, chance: 10)); // White Pillow Feather
			drops.Add(new DropData(itemId: 64018, chance: 5));  // Paper
			drops.Add(new DropData(itemId: 60009, chance: 5));  // Wool
			drops.Add(new DropData(itemId: 52002, chance: 4));  // Small Gem
			drops.Add(new DropData(itemId: 52004, chance: 4));  // Small Green Gem
			drops.Add(new DropData(itemId: 52005, chance: 4));  // Small Blue Gem
			drops.Add(new DropData(itemId: 52006, chance: 4));  // Small Red Gem
			drops.Add(new DropData(itemId: 52007, chance: 4));  // Small Silver Gem
			drops.Add(new DropData(itemId: 51101, chance: 3));  // Bloody Herb
			drops.Add(new DropData(itemId: 51102, chance: 3));  // Mana Herb
			drops.Add(new DropData(itemId: 51103, chance: 3));  // Sunlight Herb
			drops.Add(new DropData(itemId: 51104, chance: 3));  // Base Herb
			drops.Add(new DropData(itemId: 51105, chance: 3));  // Gold Herb
			drops.Add(new DropData(itemId: 51106, chance: 3));  // Garbage Herb
			drops.Add(new DropData(itemId: 51107, chance: 3));  // White Herb
			drops.Add(new DropData(itemId: 64025, chance: 2));  // Iron Plate

			if (IsEnabled("Sketching"))
				drops.Add(new DropData(itemId: 61501, chance: 5)); // Sketch Paper

			if (IsEnabled("Metallurgy"))
				drops.Add(new DropData(itemId: 64041, chance: 2)); // Unknown Ore Fragment
		}

		return drops;
	}
}

public class PillowFightTirFieldBossScript : PillowFightFieldBaseBossScript
{
	protected override SpawnInfo GetNextSpawn()
	{
		var spawn = base.GetNextSpawn();

		spawn.LocationName = L("Eastern Fields of Tir Chonaill");
		spawn.Location = new Location("Uladh_main/field_Tir_E_aa/TirChonaill_monster5");

		return spawn;
	}
}

public class PillowFightDunFieldBossScript : PillowFightFieldBaseBossScript
{
	protected override SpawnInfo GetNextSpawn()
	{
		var spawn = base.GetNextSpawn();

		spawn.LocationName = L("North Western Fields of Dunbarton");
		spawn.Location = new Location("Uladh_Dunbarton/field_Dunbarton_01/dunbmon46");

		return spawn;
	}
}

public class PillowFightEmainFieldBossScript : PillowFightFieldBaseBossScript
{
	protected override SpawnInfo GetNextSpawn()
	{
		var spawn = base.GetNextSpawn();

		spawn.LocationName = L("North Eastern Fields of Emain Macha");
		spawn.Location = new Location("Ula_Emainmacha/_Ula_Emainmacha_C/mon138");

		return spawn;
	}

	protected override bool ShouldSpawn()
	{
		return base.ShouldSpawn() && IsEnabled("EmainMacha");
	}
}

public class PillowFightTaillFieldBossScript : PillowFightFieldBaseBossScript
{
	protected override SpawnInfo GetNextSpawn()
	{
		var spawn = base.GetNextSpawn();

		spawn.LocationName = L("Western Fields of Taillteann");
		spawn.Location = new Location("taillteann_main_field/_taillteann_main_field_0024/mon1721");

		return spawn;
	}

	protected override bool ShouldSpawn()
	{
		return base.ShouldSpawn() && IsEnabled("Taillteann");
	}
}

public class PillowFightTaraFieldBossScript : PillowFightFieldBaseBossScript
{
	protected override SpawnInfo GetNextSpawn()
	{
		var spawn = base.GetNextSpawn();

		spawn.LocationName = L("South Eastern Fields of Tara");
		spawn.Location = new Location("Tara_main_field/_Tara_main_field_0016/mon2008");

		return spawn;
	}

	protected override bool ShouldSpawn()
	{
		return base.ShouldSpawn() && IsEnabled("Tara");
	}
}
