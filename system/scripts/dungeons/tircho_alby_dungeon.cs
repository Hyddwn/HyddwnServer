//--- Aura Script -----------------------------------------------------------
// Alby Dungeon
//--- Description -----------------------------------------------------------
// Alby router and script for Alby Normal.
//---------------------------------------------------------------------------

[DungeonScript("tircho_alby_dungeon")]
public class AlbyDungeonScript : DungeonScript
{
	public override bool Route(Creature creature, Item item, ref string dungeonName)
	{
		// Access to bunny dungeon with a check worth at least 1m
		if (item.Info.Id == 2004 && item.MetaData1.GetInt("EVALUE") >= 1000000)
		{
			dungeonName = "tircho_alby_whiteday_dungeon";
			return true;
		}

		// Rescue Resident quest dungeon
		if (item.Info.Id == 63180) // Trefor's Pass
		{
			dungeonName = "tircho_alby_dungeon_tutorial_ranald";
			return true;
		}

		// Malcolm's Ring quest dungeon
		if (item.Info.Id == 63181) // Malcolm's Pass
		{
			dungeonName = "tircho_alby_dungeon_tutorial_malcolm";
			return true;
		}

		// Alby Beginner
		if (item.Info.Id == 63140) // Alby Beginner Pass
		{
			dungeonName = "tircho_alby_beginner_1_dungeon";
			return true;
		}

		// Alby Basic
		if (item.Info.Id == 63101) // Alby Basic Fomor Pass
		{
			dungeonName = "tircho_alby_low_dungeon";
			return true;
		}

		// Alby Int 1
		if (item.Info.Id == 63116) // Alby Intermediate Fomor Pass for One
		{
			if (creature.Party.MemberCount == 1)
			{
				dungeonName = "tircho_alby_middle_1_dungeon";
				return true;
			}
			else
			{
				Send.Notice(creature, L("You can only enter this dungeon alone."));
				return false;
			}
		}

		// Alby Int 2
		if (item.Info.Id == 63117) // Alby Intermediate Fomor Pass for Two
		{
			if (creature.Party.MemberCount == 2)
			{
				dungeonName = "tircho_alby_middle_2_dungeon";
				return true;
			}
			else
			{
				Send.Notice(creature, L("To enter this dungeon, you need a party with 2 members."));
				return false;
			}
		}

		// Alby Int 4
		if (item.Info.Id == 63118) // Alby Intermediate Fomor Pass for Four
		{
			if (creature.Party.MemberCount == 4)
			{
				dungeonName = "tircho_alby_middle_4_dungeon";
				return true;
			}
			else
			{
				Send.Notice(creature, L("To enter this dungeon, you need a party with 4 members."));
				return false;
			}
		}

		// Tarlach's Locket (G1 RP)
		if (item.Info.Id == 73002)
		{
			if (!creature.Party.Leader.Keywords.Has("g1_03"))
			{
				Send.Notice(creature, L("You can't enter this dungeon right now."));
				return false;
			}

			if (creature.Party.MemberCount != 3 && !IsEnabled("SoloRP"))
			{
				Send.Notice(creature, L("To enter this dungeon, you need a party with 3 members."));
				return false;
			}

			dungeonName = "g1rp_05_tircho_alby_dungeon";
			return true;
		}

		// Fall back for unknown passes
		if (item.IsDungeonPass)
		{
			Send.Notice(creature, L("This dungeon hasn't been implemented yet."));
			return false;
		}

		// tircho_alby_dungeon
		return true;
	}

	public override void OnCreation(Dungeon dungeon)
	{
	}

	public override void OnBoss(Dungeon dungeon)
	{
		dungeon.AddBoss(30004, 1); // Giant Spider
		dungeon.AddBoss(30003, 6); // Red Spider

		dungeon.PlayCutscene("bossroom_GiantSpider");
	}

	public override void OnCleared(Dungeon dungeon)
	{
		var rnd = RandomProvider.Get();
		var creators = dungeon.GetCreators();

		for (int i = 0; i < creators.Count; ++i)
		{
			var member = creators[i];
			var treasureChest = new TreasureChest();

			if (i == 0)
			{
				// Enchant
				var enchant = 0;
				switch (rnd.Next(3))
				{
					case 0: enchant = 1506; break; // Swan Summoner's (Prefix)
					case 1: enchant = 1706; break; // Good (Prefix)
					case 2: enchant = 305; break;  // Fine (Prefix)
				}
				treasureChest.Add(Item.CreateEnchant(enchant));
			}

			treasureChest.AddGold(rnd.Next(153, 768)); // Gold
			treasureChest.Add(GetRandomTreasureItem(rnd)); // Random item

			dungeon.AddChest(treasureChest);

			member.GiveItemWithEffect(Item.CreateKey(70028, "chest"));
		}
	}

	List<DropData> drops;
	public Item GetRandomTreasureItem(Random rnd)
	{
		if (drops == null)
		{
			drops = new List<DropData>();
			drops.Add(new DropData(itemId: 62004, chance: 44, amountMin: 1, amountMax: 2)); // Magic Powder
			drops.Add(new DropData(itemId: 51102, chance: 44, amountMin: 1, amountMax: 2)); // Mana Herb
			drops.Add(new DropData(itemId: 71017, chance: 2, amountMin: 1, amountMax: 2));  // White Spider Fomor Scroll
			drops.Add(new DropData(itemId: 71019, chance: 2, amountMin: 1, amountMax: 1)); // Red Spider Fomor Scroll
			drops.Add(new DropData(itemId: 63116, chance: 1, amount: 1, expires: 480)); // Alby Int 1
			drops.Add(new DropData(itemId: 63117, chance: 1, amount: 1, expires: 480)); // Alby Int 2
			drops.Add(new DropData(itemId: 63118, chance: 1, amount: 1, expires: 480)); // Alby Int 4
			drops.Add(new DropData(itemId: 63101, chance: 2, amount: 1, expires: 480)); // Alby Basic
			drops.Add(new DropData(itemId: 40002, chance: 1, amount: 1, color1: 0x000000, durability: 0)); // Wooden Blade (black)

			if (IsEnabled("AlbyAdvanced"))
			{
				drops.Add(new DropData(itemId: 63160, chance: 1, amount: 1, expires: 360)); // Alby Advanced 3-person Fomor Pass
				drops.Add(new DropData(itemId: 63161, chance: 1, amount: 1, expires: 360)); // Alby Advanced Fomor Pass
			}
		}

		return Item.GetRandomDrop(rnd, drops);
	}
}
