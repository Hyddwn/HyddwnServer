//--- Aura Script -----------------------------------------------------------
// Albey Dungeon
//--- Description -----------------------------------------------------------
// Albey router and script for Albey Normal.
//---------------------------------------------------------------------------

[DungeonScript("tirnanog_dungeon")]
public class AlbeyDungeonScript : DungeonScript
{
	public override bool Route(Creature creature, Item item, ref string dungeonName)
	{
		// Small Green Gem (G1)
		if (item.Info.Id == 52004)
		{
			dungeonName = "g1_37_green_tirnanog_dungeon";
			return true;
		}

		// Small Blue Gem (G1)
		if (item.Info.Id == 52005)
		{
			dungeonName = "g1_37_blue_tirnanog_dungeon";
			return true;
		}

		// Small Red Gem (G1)
		if (item.Info.Id == 52006)
		{
			dungeonName = "g1_37_red_tirnanog_dungeon";
			return true;
		}

		// Small Silver Gem (G1)
		if (item.Info.Id == 52007)
		{
			dungeonName = "g1_37_silver_tirnanog_dungeon";
			return true;
		}

		// Black Orb (G1)
		if (item.Info.Id == 73033)
		{
			if (!creature.Party.Leader.Keywords.Has("g1_36"))
			{
				Send.Notice(creature, L("You can't enter this dungeon right now."));
				return false;
			}

			if (creature.Party.MemberCount > 3)
			{
				Send.Notice(creature, L("To enter this dungeon, you need a party with 3 or less members."));
				return false;
			}

			dungeonName = "g1_37_black_tirnanog_dungeon";
			return true;
		}

		// Pendant of the Goddess (G1 Final)
		if (item.Info.Id == 73029)
		{
			if (!creature.Party.Leader.Keywords.Has("g1_38"))
			{
				Send.Notice(creature, L("You can't enter this dungeon right now."));
				return false;
			}

			if (creature.Party.MemberCount > 3)
			{
				Send.Notice(creature, L("To enter this dungeon, you need a party with 3 or less members."));
				return false;
			}

			dungeonName = "g1_39_tirnanog_dungeon";
			return true;
		}

		// Goddess Pass (G1 Final)
		if (item.Info.Id == 73034)
		{
			if (!creature.Party.Leader.Keywords.Has("g1_38"))
			{
				Send.Notice(creature, L("You can't enter this dungeon right now."));
				return false;
			}

			if (creature.Party.MemberCount > 3)
			{
				Send.Notice(creature, L("To enter this dungeon, you need a party with 3 or less members."));
				return false;
			}

			dungeonName = "g1_39_tirnanog_dungeon_again";
			return true;
		}

		// tirnanog_dungeon
		return true;
	}

	public override void OnBoss(Dungeon dungeon)
	{
		dungeon.AddBoss(160002, 6); // Light Gargolye

		dungeon.PlayCutscene("bossroom_LightGargoyle");
	}

	public override void OnCleared(Dungeon dungeon)
	{
		var rnd = RandomProvider.Get();
		var creators = dungeon.GetCreators();

		for (int i = 0; i < creators.Count; ++i)
		{
			var member = creators[i];
			var treasureChest = new TreasureChest();

			treasureChest.AddGold(rnd.Next(1440, 2560)); // Gold
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
			drops.Add(new DropData(itemId: 62004, chance: 20, amountMin: 1, amountMax: 2)); // Magic Powder
			drops.Add(new DropData(itemId: 51102, chance: 20, amountMin: 1, amountMax: 2)); // Mana Herb
			drops.Add(new DropData(itemId: 51003, chance: 20, amountMin: 1, amountMax: 2)); // HP 50 Potion
			drops.Add(new DropData(itemId: 51008, chance: 20, amountMin: 1, amountMax: 2)); // MP 50 Potion
			drops.Add(new DropData(itemId: 51013, chance: 20, amountMin: 1, amountMax: 2)); // Stamina 50 Potion
		}

		return Item.GetRandomDrop(rnd, drops);
	}
}
