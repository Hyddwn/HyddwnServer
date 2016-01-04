//--- Aura Script -----------------------------------------------------------
// Treasure Chest Room Puzzle
//--- Description -----------------------------------------------------------
// A room with 9 chests, that can be opened with keys dropped from mobs.
//---------------------------------------------------------------------------

public abstract class Keychest9ChestScript : PuzzleScript
{
	public override void OnPrepare(Puzzle puzzle)
	{
		var place = puzzle.NewPlace("Place");
		place.ReservePlace();
		place.ReserveDoors();

		puzzle.Set("MonsterI", 1);
	}

	public override void OnPuzzleCreate(Puzzle puzzle)
	{
		var place = puzzle.GetPlace("Place");

		// Get 4 random numbers between 1 and 9
		var specialChests = UniqueRnd(4, 1, 2, 3, 4, 5, 6, 7, 8, 9);

		// Spawn 9 chests, with monsters in 3, and an enchant in one of them
		for (int i = 1; i <= 9; ++i)
		{
			// First 3 special chests are monsters, last is enchant
			var isMonsterChest = specialChests.Take(3).Contains(i);
			var isEnchantChest = specialChests[3] == i;

			var chest = new LockedChest(puzzle, "Chest" + i, "lock1");
			AddChestDrops(chest, i, isMonsterChest, isEnchantChest);
			place.AddProp(chest, Placement.Center9);

			puzzle.Set("Chest" + i + "Open", false);
			puzzle.Set("Chest" + i + "Monster", isMonsterChest);
		}
	}

	protected abstract void AddChestDrops(Chest chest, int chestNum, bool isMonsterChest, bool isEnchantChest);

	public override void OnPropEvent(Puzzle puzzle, Prop prop)
	{
		var chest = prop as LockedChest;
		if (chest == null || puzzle.Get(chest.Name + "Open") || !chest.IsOpen)
			return;

		puzzle.Set(chest.Name + "Open", true);

		if (puzzle.Get(chest.Name + "Monster"))
		{
			var place = puzzle.GetPlace("Place");
			place.CloseAllDoors();
			place.SpawnSingleMob(chest.Name + "Mob", "Mob" + puzzle.Get("MonsterI"));
			puzzle.Set("MonsterI", puzzle.Get("MonsterI") + 1);
		}
	}

	public override void OnMonsterDead(Puzzle puzzle, MonsterGroup group)
	{
		if (group.Remaining != 0)
			return;

		puzzle.GetPlace("Place").OpenAllDoors();
	}
}

[PuzzleScript("keychest_9chest")]
public class Keychest9ChestNormalScript : Keychest9ChestScript
{
	protected override void AddChestDrops(Chest chest, int chestNum, bool isMonsterChest, bool isEnchantChest)
	{
		// Gold
		if (!isMonsterChest)
			chest.Add(Item.Create(id: 2000, amountMin: 250, amountMax: 1000));
	}
}

[PuzzleScript("keychest_9chest_ciar")]
public class Keychest9ChestCiarScript : Keychest9ChestScript
{
	protected override void AddChestDrops(Chest chest, int chestNum, bool isMonsterChest, bool isEnchantChest)
	{
		// Gold
		if (!isMonsterChest)
			chest.Add(Item.Create(id: 2000, amountMin: 100, amountMax: 250));

		// Enchant
		if (isEnchantChest)
		{
			var enchant = 0;
			switch (Random(10))
			{
				case 0:
				case 1:
				case 2:
				case 3: enchant = 20205; break; // Restfull (Prefix)
				case 4:
				case 5:
				case 6: enchant = 20204; break; // Foggy (Prefix)
				case 7:
				case 8: enchant = 30501; break; // Giant (Suffix)
				case 9: enchant = 30602; break; // Healer (Suffix)
			}

			chest.Add(Item.CreateEnchant(enchant));
		}
	}
}

[PuzzleScript("keychest_9chest_barri")]
public class Keychest9ChestBarriScript : Keychest9ChestScript
{
	protected override void AddChestDrops(Chest chest, int chestNum, bool isMonsterChest, bool isEnchantChest)
	{
		// Gold
		if (!isMonsterChest)
			chest.Add(Item.Create(id: 2000, amountMin: 250, amountMax: 400));

		// Enchant
		if (isEnchantChest)
		{
			var enchant = 0;
			switch (Random(10))
			{
				case 0:
				case 1:
				case 2: enchant = 20401; break; // Smart (Prefix)
				case 3:
				case 4:
				case 5: enchant = 20402; break; // Strong (Prefix)
				case 6:
				case 7: enchant = 30702; break; // Raven (Suffix)
				case 8: enchant = 30704; break; // Deadly (Suffix)
				case 9: enchant = 30805; break; // Falcon (Suffix)
			}

			chest.Add(Item.CreateEnchant(enchant));
		}
	}
}

[PuzzleScript("keychest_9chest_rabbie")]
public class Keychest9ChestRabbieScript : Keychest9ChestScript
{
	protected override void AddChestDrops(Chest chest, int chestNum, bool isMonsterChest, bool isEnchantChest)
	{
		// Gold
		if (!isMonsterChest)
			chest.Add(Item.Create(id: 2000, amountMin: 150, amountMax: 350));

		// Enchant
		if (isEnchantChest)
		{
			var enchant = 0;
			switch (Random(10))
			{
				case 0:
				case 1:
				case 2:
				case 3: enchant = 20205; break; // Restfull (Prefix)
				case 4:
				case 5:
				case 6: enchant = 20204; break; // Mist (Prefix)
				case 7:
				case 8: enchant = 30501; break; // Giant (Suffix)
				case 9: enchant = 30602; break; // Healer (Suffix)
			}

			chest.Add(Item.CreateEnchant(enchant));
		}
	}
}

[PuzzleScript("keychest_9chest_mid_supply")]
public class Keychest9ChestMidSupplyScript : Keychest9ChestScript
{
	protected override void AddChestDrops(Chest chest, int chestNum, bool isMonsterChest, bool isEnchantChest)
	{
		// Gold
		if (!isMonsterChest)
			chest.Add(Item.Create(id: 2000, amountMin: 250, amountMax: 1000));

		// Enchant
		if (isEnchantChest)
		{
			var enchant = 0;
			switch (Random(10))
			{
				case 0:
				case 1:
				case 2:
				case 3: enchant = 20205; break; // Restfull (Prefix)
				case 4:
				case 5:
				case 6: enchant = 20204; break; // Mist (Prefix)
				case 7:
				case 8: enchant = 30501; break; // Giant (Suffix)
				case 9: enchant = 30602; break; // Healer (Suffix)
			}

			chest.Add(Item.CreateEnchant(enchant));
		}
	}
}
