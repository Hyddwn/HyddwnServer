//--- Aura Script -----------------------------------------------------------
// Rabbie Dungeon
//--- Description -----------------------------------------------------------
// Rabbie router and script for Rabbie Normal.
//---------------------------------------------------------------------------

[DungeonScript("dunbarton_rabbie_dungeon")]
public class RabbieDungeonScript : DungeonScript
{
	public override bool Route(Creature creature, Item item, ref string dungeonName)
	{
		// Rabbie Basic
		if (item.Info.Id == 63110) // Rabbie Basic Fomor Pass
		{
			dungeonName = "dunbarton_rabbie_low_dungeon";
			return true;
		}

		// Tarlach's Glasses Pouch (G1 RP)
		if (item.Info.Id == 73022)
		{
			if (!creature.Party.Leader.Keywords.Has("g1_15"))
			{
				Send.Notice(creature, L("You can't enter this dungeon right now."));
				return false;
			}

			if (creature.Party.MemberCount != 1)
			{
				Send.Notice(creature, L("You need to enter this dungeon alone."));
				return false;
			}

			dungeonName = "g1rp_15_dunbarton_rabbie_dungeon";
			return true;
		}

		// Tarlach's Preserved Broken Glasses (G1 RP)
		if (item.Info.Id == 73004)
		{
			if (!creature.Party.Leader.Keywords.Has("g1_29"))
			{
				Send.Notice(creature, L("You can't enter this dungeon right now."));
				return false;
			}

			if (creature.Party.MemberCount != 3 && !IsEnabled("SoloRP"))
			{
				Send.Notice(creature, L("To enter this dungeon, you need a party with 3 members."));
				return false;
			}

			dungeonName = "g1rp_25_dunbarton_rabbie_dungeon";
			return true;
		}

		// Fall back for unknown passes
		if (item.IsDungeonPass)
		{
			Send.Notice(creature, L("This dungeon hasn't been implemented yet."));
			return false;
		}

		// dunbarton_rabbie_dungeon
		return true;
	}

	public override void OnBoss(Dungeon dungeon)
	{
		if (dungeon.CountPlayers() == 1)
		{
			dungeon.AddBoss(10301, 1); // Black Succubus
		}
		else
		{
			dungeon.AddBoss(10101, 1); // Goblin

			dungeon.PlayCutscene("bossroom_GoldGoblin");
		}
	}

	public override void OnBossDeath(Dungeon dungeon, Creature boss, Creature killer)
	{
		if (boss.RaceId != 10101)
			return;

		dungeon.AddBoss(10104, 12); // Gold Goblin
		dungeon.AddBoss(10103, 6); // Goblin Archer
	}

	public override void OnCleared(Dungeon dungeon)
	{
		var rnd = RandomProvider.Get();
		var creators = dungeon.GetCreators();

		if (dungeon.CountPlayers() == 1)
		{
			var member = creators[0];
			var treasureChest = new TreasureChest();

			// Bracelet
			int prefix = 0, suffix = 0;
			switch (rnd.Next(3))
			{
				case 0: prefix = 206; break; // Snake
				case 1: prefix = 305; break; // Fine
				case 2: prefix = 303; break; // Rusty
			}
			switch (rnd.Next(3))
			{
				case 0: suffix = 10504; break; // Topaz
				case 1: suffix = 10605; break; // Soldier
				case 2: suffix = 11206; break; // Fountain
			}
			treasureChest.Add(Item.CreateEnchanted(16015, prefix, suffix));

			treasureChest.Add(Item.Create(id: 2000, amountMin: 570, amountMax: 2520)); // Gold
			treasureChest.Add(GetRandomTreasureItem(rnd)); // Random item

			dungeon.AddChest(treasureChest);

			member.GiveItemWithEffect(Item.CreateKey(70028, "chest"));
		}
		else
		{
			for (int i = 0; i < creators.Count; ++i)
			{
				var member = creators[i];
				var treasureChest = new TreasureChest();

				if (i == 0)
				{
					// Bracelet
					int prefix = 0, suffix = 0;
					switch (rnd.Next(3))
					{
						case 0: suffix = 10504; break; // Topaz
						case 1: suffix = 10605; break; // Soldier
						case 2: suffix = 11205; break; // Water
					}
					treasureChest.Add(Item.CreateEnchanted(16015, prefix, suffix));
				}

				treasureChest.Add(Item.Create(id: 2000, amountMin: 896, amountMax: 3600)); // Gold
				treasureChest.Add(GetRandomTreasureItem(rnd)); // Random item

				dungeon.AddChest(treasureChest);

				member.GiveItemWithEffect(Item.CreateKey(70028, "chest"));
			}
		}
	}

	List<DropData> drops;
	public Item GetRandomTreasureItem(Random rnd)
	{
		if (drops == null)
		{
			drops = new List<DropData>();
			drops.Add(new DropData(itemId: 62004, chance: 15, amountMin: 1, amountMax: 2)); // Magic Powder
			drops.Add(new DropData(itemId: 51102, chance: 15, amountMin: 1, amountMax: 2)); // Mana Herb
			drops.Add(new DropData(itemId: 51003, chance: 15, amountMin: 1, amountMax: 2)); // HP 50 Potion
			drops.Add(new DropData(itemId: 51008, chance: 15, amountMin: 1, amountMax: 2)); // MP 50 Potion
			drops.Add(new DropData(itemId: 51013, chance: 15, amountMin: 1, amountMax: 2)); // Stamina 50 Potion
			drops.Add(new DropData(itemId: 71006, chance: 1, amountMin: 1, amountMax: 2)); // Skeleton Fomor Scroll
			drops.Add(new DropData(itemId: 71007, chance: 1, amountMin: 1, amountMax: 1)); // Red Skeleton Fomor Scroll
			drops.Add(new DropData(itemId: 71008, chance: 1, amountMin: 1, amountMax: 1)); // Metal Skeleton Fomor Scroll
			drops.Add(new DropData(itemId: 71035, chance: 3, amountMin: 3, amountMax: 5)); // Gray Town Rat Fomor Scroll
			drops.Add(new DropData(itemId: 63110, chance: 8, amount: 1, expires: 600)); // Rabbie Basic Fomor Pass
			drops.Add(new DropData(itemId: 40005, chance: 1, amount: 1, color1: 0xFFE760, durability: 0)); // Short Sword (gold)

			if (IsEnabled("RabbieAdvanced"))
			{
				drops.Add(new DropData(itemId: 63141, chance: 3, amount: 1, expires: 360)); // Rabbie Adv. Fomor Pass for 2
				drops.Add(new DropData(itemId: 63142, chance: 5, amount: 1, expires: 360)); // Rabbie Adv. Fomor Pass for 3
				drops.Add(new DropData(itemId: 63143, chance: 2, amount: 1, expires: 360)); // Rabbie Adv. Fomor Pass
			}
		}

		return Item.GetRandomDrop(rnd, drops);
	}
}
