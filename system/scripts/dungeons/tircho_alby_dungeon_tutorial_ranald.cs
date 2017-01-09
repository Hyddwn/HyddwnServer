//--- Aura Script -----------------------------------------------------------
// Rescue Resident quest dungeon
//--- Description -----------------------------------------------------------
// Player has to make it to the boss room and fight a Giant Spiderling
// alongside a Tir Chonaill resident.
//---------------------------------------------------------------------------

[DungeonScript("tircho_alby_dungeon_tutorial_ranald")]
public class AlbyTutorialDungeonScript : DungeonScript
{
	public override void OnCreation(Dungeon dungeon)
	{
	}

	public override void OnBoss(Dungeon dungeon)
	{
		dungeon.AddBoss(30022, 1); // Giant Spiderling
		dungeon.AddBoss(1002, 1);  // Lost Resident

		dungeon.PlayCutscene("bossroom_tutorial_giantspider_kid");
	}

	public override void OnBossDeath(Dungeon dungeon, Creature boss, Creature killer)
	{
		if (boss.RaceId == 30022)
		{
			dungeon.Complete();
		}
		else
		{
			// Lost resident, as in "lost" lost
		}
	}

	public override void OnCleared(Dungeon dungeon)
	{
		var rnd = RandomProvider.Get();
		var creators = dungeon.GetCreators();

		for (int i = 0; i < creators.Count; ++i)
		{
			var member = creators[i];
			member.TalkToNpc("_dungeonlostresident", "Lost Resident");

			var treasureChest = new TreasureChest();

			// Enchant
			var enchant = 0;
			switch (rnd.Next(3))
			{
				case 0: enchant = 1506; break; // Swan Summoner's (Prefix)
				case 1: enchant = 1706; break; // Good (Prefix)
				case 2: enchant = 305; break;  // Fine (Prefix)
			}
			treasureChest.Add(Item.CreateEnchant(enchant));

			treasureChest.AddGold(rnd.Next(153, 768)); // Gold
			treasureChest.Add(GetRandomTreasureItem(rnd)); // Random item

			dungeon.AddChest(treasureChest);

			member.GiveItemWithEffect(Item.CreateKey(70028, "chest"));
		}
	}

	public override void OnLeftEarly(Dungeon dungeon, Creature creature)
	{
		// Give pass again if lost
		if (!creature.HasKeyword("Clear_Tutorial_Alby_Dungeon"))
			creature.GiveItem(63180); // Trefor's Pass
	}

	List<DropData> drops;
	public Item GetRandomTreasureItem(Random rnd)
	{
		if (drops == null)
		{
			drops = new List<DropData>();
			drops.Add(new DropData(itemId: 51001, chance: 54, amountMin: 1, amountMax: 4)); // HP 10 Potion
			drops.Add(new DropData(itemId: 51102, chance: 20, amountMin: 1, amountMax: 2)); // Mana Herb
			drops.Add(new DropData(itemId: 71017, chance: 15, amountMin: 1, amountMax: 2)); // White Spider Fomor Scroll
			drops.Add(new DropData(itemId: 71019, chance: 10, amount: 1)); // Red Spider Fomor Scroll
			drops.Add(new DropData(itemId: 40002, chance: 1, amount: 1, color1: 0x000000, durability: 0)); // Wooden Blade (black)
		}

		return Item.GetRandomDrop(rnd, drops);
	}
}
