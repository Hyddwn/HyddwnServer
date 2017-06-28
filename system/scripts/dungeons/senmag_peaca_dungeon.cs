//--- Aura Script -----------------------------------------------------------
// Peaca Dungeon
//--- Description -----------------------------------------------------------
// Peaca router and script for Peaca Normal.
//---------------------------------------------------------------------------

[DungeonScript("senmag_peaca_dungeon")]
public class PeacaDungeonScript : DungeonScript
{
	public override bool Route(Creature creature, Item item, ref string dungeonName)
	{
		// Peaca Beginner Dungeon
		if (item.Info.Id == 63144) // Peaca Beginner Pass
		{
			dungeonName = "senmag_peaca_beginner_dungeon";
			return true;
		}

		// Fall back for unknown passes
		if (item.IsDungeonPass)
		{
			Send.Notice(creature, L("This dungeon hasn't been implemented yet."));
			return false;
		}

		// Peaca Normal (senmag_peaca_dungeon)
		if (!IsEnabled("G17S2"))
		{
			if (creature.Party.MemberCount < 4)
			{
				Send.Notice(creature, L("You must have a party of at least 4 members to enter."));
				return false;
			}
		}

		return true;
	}

	public override void OnCreation(Dungeon dungeon)
	{
	}

	public override void OnBoss(Dungeon dungeon)
	{
		dungeon.AddBoss(120010, 1); // Black Ship Rat
		dungeon.AddBoss(17501, 1); // Demi Lich

		dungeon.PlayCutscene("bossroom_demi_lich");
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
				switch (rnd.Next(6))
				{
					case 0: enchant = 20501; break; // Simple (Prefix)
					case 1: enchant = 20502; break; // Scrupulous (Prefix)
					case 2: enchant = 30809; break; // Dark Cross (Suffix)
					case 3: enchant = 31201; break; // Prudent (Suffix)
					case 4: enchant = 31202; break; // Jackal (Suffix)
					case 5: enchant = 31102; break; // Viper (Suffix)
				}
				treasureChest.Add(Item.CreateEnchant(enchant));
			}

			treasureChest.AddGold(rnd.Next(14440, 24000)); // Gold
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
			drops.Add(new DropData(itemId: 62004, chance: 18, amountMin: 2, amountMax: 4)); // Magic Powder
			drops.Add(new DropData(itemId: 51102, chance: 18, amountMin: 2, amountMax: 4)); // Mana Herb
			drops.Add(new DropData(itemId: 51003, chance: 18, amountMin: 2, amountMax: 4)); // Hp 50 Potion
			drops.Add(new DropData(itemId: 51008, chance: 18, amountMin: 2, amountMax: 4)); // Mp 50 Potion
			drops.Add(new DropData(itemId: 51013, chance: 18, amountMin: 2, amountMax: 4)); // Stamina 50 Potion

			if (IsEnabled("PeacaBasic"))
			{
				drops.Add(new DropData(itemId: 63145, chance: 7, amount: 1, expires: 600)); // Peaca Basic Fomor Pass
			}

			if (IsEnabled("PeacaInt"))
			{
				drops.Add(new DropData(itemId: 63227, chance: 3, amount: 1, expires: 600)); // Peaca Intermediate Fomor Pass
			}
		}

		return Item.GetRandomDrop(rnd, drops);
	}
}
