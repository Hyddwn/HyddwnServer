//--- Aura Script -----------------------------------------------------------
// Ciar Dungeon
//--- Description -----------------------------------------------------------
// Ciar router and script for Ciar Normal.
//---------------------------------------------------------------------------

[DungeonScript("tircho_ciar_dungeon")]
public class CiarDungeonScript : DungeonScript
{
	public override bool Route(Creature creature, Item item, ref string dungeonName)
	{
		// Ciar Beginner
		if (item.Info.Id == 63139) // Ciar Beginner Pass
		{
			dungeonName = "tircho_ciar_beginner_1_dungeon";
			return true;
		}

		// Ciar Basic
		if (item.Info.Id == 63104) // Ciar Basic Fomor Pass
		{
			dungeonName = "tircho_ciar_low_dungeon";
			return true;
		}

		// Ciar Int 1
		if (item.Info.Id == 63123) // Ciar Intermediate Fomor Pass for One
		{
			if (creature.Party.MemberCount == 1)
			{
				dungeonName = "tircho_ciar_middle_1_dungeon";
				return true;
			}
			else
			{
				Send.Notice(creature, L("You can only enter this dungeon alone."));
				return false;
			}
		}

		// Ciar Int 2
		if (item.Info.Id == 63124) // Ciar Intermediate Fomor Pass for Two
		{
			if (creature.Party.MemberCount == 2)
			{
				dungeonName = "tircho_ciar_middle_2_dungeon";
				return true;
			}
			else
			{
				Send.Notice(creature, L("To enter this dungeon, you need a party with 2 members."));
				return false;
			}
		}

		// Ciar Int 4
		if (item.Info.Id == 63125) // Ciar Intermediate Fomor Pass for Four
		{
			if (creature.Party.MemberCount == 4)
			{
				dungeonName = "tircho_ciar_middle_4_dungeon";
				return true;
			}
			else
			{
				Send.Notice(creature, L("To enter this dungeon, you need a party with 4 members."));
				return false;
			}
		}

		// Wizard's Note (G1)
		if (item.Info.Id == 73024)
		{
			if (!creature.Party.Leader.Keywords.Has("g1_25"))
			{
				Send.Notice(creature, L("You can't enter this dungeon right now."));
				return false;
			}

			dungeonName = "g1_21_tircho_ciar_dungeon";
			return true;
		}

		// Fall back for unknown passes
		if (item.IsDungeonPass)
		{
			Send.Notice(creature, L("This dungeon hasn't been implemented yet."));
			return false;
		}

		// tircho_ciar_dungeon
		return true;
	}

	public override void OnCreation(Dungeon dungeon)
	{
	}

	public override void OnBoss(Dungeon dungeon)
	{
		dungeon.AddBoss(130001, 1); // Golem
		dungeon.AddBoss(11003, 6); // Metal Skeleton

		dungeon.PlayCutscene("bossroom_Metalskeleton_Golem");
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
				// Broad Stick
				int prefix = 0, suffix = 0;
				switch (rnd.Next(4))
				{
					case 0: prefix = 207; break; // Fox
					case 1: prefix = 306; break; // Sharp
					case 2: prefix = 303; break; // Rusty
					case 3: prefix = 7; break; // Fox Hunter's
				}
				switch (rnd.Next(3))
				{
					case 0: suffix = 11106; break; // Blood
					case 1: suffix = 10806; break; // Understanding
					case 2: suffix = 10704; break; // Slug
				}
				treasureChest.Add(Item.CreateEnchanted(40019, prefix, suffix));
			}

			treasureChest.AddGold(rnd.Next(979, 2400)); // Gold
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
			drops.Add(new DropData(itemId: 62004, chance: 15, amountMin: 1, amountMax: 2)); // Magic Powder
			drops.Add(new DropData(itemId: 51102, chance: 15, amountMin: 1, amountMax: 2)); // Mana Herb
			drops.Add(new DropData(itemId: 51003, chance: 15, amountMin: 1, amountMax: 2)); // HP 50 Potion
			drops.Add(new DropData(itemId: 51008, chance: 15, amountMin: 1, amountMax: 2)); // MP 50 Potion
			drops.Add(new DropData(itemId: 51013, chance: 15, amountMin: 1, amountMax: 2)); // Stamina 50 Potion
			drops.Add(new DropData(itemId: 71037, chance: 4, amountMin: 2, amountMax: 4)); // Goblin Fomor Scroll
			drops.Add(new DropData(itemId: 71035, chance: 4, amountMin: 3, amountMax: 5)); // Gray Town Rat Fomor Scroll
			drops.Add(new DropData(itemId: 63104, chance: 3, amount: 1, expires: 480)); // Ciar Basic Fomor Pass
			drops.Add(new DropData(itemId: 63123, chance: 2, amount: 1, expires: 480)); // Ciar Intermediate Fomor Pass for One
			drops.Add(new DropData(itemId: 63124, chance: 2, amount: 1, expires: 480)); // Ciar Intermediate Fomor Pass for Two
			drops.Add(new DropData(itemId: 63125, chance: 2, amount: 1, expires: 480)); // Ciar Intermediate Fomor Pass for Four
			drops.Add(new DropData(itemId: 40006, chance: 2, amount: 1, color1: 0xFFDB60, durability: 0)); // Dagger (gold)

			if (IsEnabled("CiarAdvanced"))
			{
				drops.Add(new DropData(itemId: 63136, chance: 2, amount: 1, expires: 360)); // Ciar Adv. Fomor Pass for 2
				drops.Add(new DropData(itemId: 63137, chance: 2, amount: 1, expires: 360)); // Ciar Adv. Fomor Pass for 3
				drops.Add(new DropData(itemId: 63138, chance: 2, amount: 1, expires: 360)); // Ciar Adv. Fomor Pass
			}
		}

		return Item.GetRandomDrop(rnd, drops);
	}
}
