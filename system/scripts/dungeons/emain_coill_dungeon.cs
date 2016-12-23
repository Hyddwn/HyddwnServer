//--- Aura Script -----------------------------------------------------------
// Coill Dungeon
//--- Description -----------------------------------------------------------
// Router and script for Coill.
//---------------------------------------------------------------------------

[DungeonScript("emain_coill_dungeon")]
public class CoillDungeonScript : DungeonScript
{
	public override bool Route(Creature creature, Item item, ref string dungeonName)
	{
		// Fall back for unknown passes
		if (item.IsDungeonPass)
		{
			Send.Notice(creature, L("This dungeon hasn't been implemented yet."));
			return false;
		}

		// emain_coill_dungeon
		return true;
	}

	public override void OnBoss(Dungeon dungeon)
	{
		dungeon.AddBoss(100101, 1); // Giant Headless
		dungeon.AddBoss(80205, 1);  // Giant Fire Sprite

		dungeon.PlayCutscene("bossroom_GiantHeadless");
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
				// Wizard Hat
				int prefix = 0, suffix = 0;
				switch (rnd.Next(3))
				{
					case 0: prefix = 20802; break; // Chaotic
					case 1: prefix = 20601; break; // Blessed
					case 2: prefix = 20202; break; // Wild Dog
				}
				switch (rnd.Next(3))
				{
					case 0: suffix = 30805; break; // Falcon
					case 1: suffix = 30307; break; // Red Bear
					case 2: suffix = 30503; break; // White Spider
				}
				treasureChest.Add(Item.CreateEnchanted(18006, prefix, suffix));
			}

			treasureChest.AddGold(rnd.Next(5250, 10500)); // Gold
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
			drops.Add(new DropData(itemId: 62004, chance: 17, amountMin: 1, amountMax: 2)); // Magic Powder
			drops.Add(new DropData(itemId: 51102, chance: 17, amountMin: 1, amountMax: 2)); // Mana Herb
			drops.Add(new DropData(itemId: 51003, chance: 17, amountMin: 1, amountMax: 2)); // HP 50 Potion
			drops.Add(new DropData(itemId: 51008, chance: 17, amountMin: 1, amountMax: 2)); // MP 50 Potion
			drops.Add(new DropData(itemId: 51013, chance: 17, amountMin: 1, amountMax: 2)); // Stamina 50 Potion
			drops.Add(new DropData(itemId: 71049, chance: 5, amountMin: 2, amountMax: 5)); // Snake Fomor Scroll
			drops.Add(new DropData(itemId: 71018, chance: 5, amountMin: 3, amountMax: 5)); // Black Spider Fomor Scroll
			drops.Add(new DropData(itemId: 71019, chance: 1, amountMin: 3, amountMax: 5)); // Red Spider Fomor Scroll (officially Black Spider duplicate #officialFix)
			drops.Add(new DropData(itemId: 71052, chance: 4, amount: 5)); // Jackal Fomor Scroll
		}

		return Item.GetRandomDrop(rnd, drops);
	}
}
