//--- Aura Script -----------------------------------------------------------
// Rundal Dungeon
//--- Description -----------------------------------------------------------
// Rundal router and script for Rundal Normal.
//---------------------------------------------------------------------------

[DungeonScript("emain_runda_dungeon")]
public class RundalDungeonScript : DungeonScript
{
	public override bool Route(Creature creature, Item item, ref string dungeonName)
	{
		// Rundal Basic Pass
		if (item.Info.Id == 63105)
		{
			dungeonName = "emain_runda_low_dungeon";
			return true;
		}

		// Rundal Adv. Fomor Pass for 2
		if (item.Info.Id == 63126)
		{
			if (creature.Party.MemberCount != 2)
			{
				Send.Notice(creature, L("To enter this dungeon, you need a party with 2 members."));
				return false;
			}

			dungeonName = "emain_runda_high_2_dungeon";
			return true;
		}

		// Rundal Adv. Fomor Pass for 3
		if (item.Info.Id == 63127)
		{
			if (creature.Party.MemberCount != 3)
			{
				Send.Notice(creature, L("To enter this dungeon, you need a party with 3 members."));
				return false;
			}

			dungeonName = "emain_runda_high_3_dungeon";
			return true;
		}

		// Rundal Adv. Fomor Pass
		if (item.Info.Id == 63128)
		{
			dungeonName = "emain_runda_high_dungeon";
			return true;
		}

		// Fall back for unknown passes
		if (item.IsDungeonPass)
		{
			Send.Notice(creature, L("This dungeon hasn't been implemented yet."));
			return false;
		}

		// emain_runda_dungeon
		return true;
	}

	public override void OnBoss(Dungeon dungeon)
	{
		dungeon.AddBoss(100201, 1); // Cyclops

		dungeon.PlayCutscene("bossroom_Runda_Cyclops1");
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
				// Tail Cap
				int prefix = 0, suffix = 0;
				switch (rnd.Next(3))
				{
					case 0: prefix = 20701; break; // Stiff
					case 1: prefix = 20406; break; // Convenient
					case 2: prefix = 20104; break; // Green
				}
				treasureChest.Add(Item.CreateEnchanted(18017, prefix, suffix));
			}

			treasureChest.AddGold(rnd.Next(1386, 2772)); // Gold
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
			drops.Add(new DropData(itemId: 62004, chance: 30, amountMin: 1, amountMax: 2)); // Magic Powder
			drops.Add(new DropData(itemId: 51102, chance: 30, amountMin: 1, amountMax: 2)); // Mana Herb
			drops.Add(new DropData(itemId: 51003, chance: 30, amountMin: 1, amountMax: 2)); // HP 50 Potion
			drops.Add(new DropData(itemId: 51008, chance: 30, amountMin: 1, amountMax: 2)); // MP 50 Potion
			drops.Add(new DropData(itemId: 51013, chance: 25, amountMin: 1, amountMax: 2)); // Stamina 50 Potion
			drops.Add(new DropData(itemId: 71032, chance: 11, amountMin: 1, amountMax: 2)); // Mimic Fomor Scroll
			drops.Add(new DropData(itemId: 71018, chance: 15, amountMin: 1, amountMax: 1)); // Black Spider Fomor Scroll
			drops.Add(new DropData(itemId: 71019, chance: 15, amountMin: 1, amountMax: 1)); // Red Spider Fomor Scroll (officially Black Spider duplicate #officialFix)

			if (IsEnabled("RundalDungeon"))
			{
				drops.Add(new DropData(itemId: 63126, chance: 2, amount: 1, expires: 360)); // Rundal Adv. Fomor Pass for 2
				drops.Add(new DropData(itemId: 63127, chance: 2, amount: 1, expires: 360)); // Rundal Adv. Fomor Pass for 3
				drops.Add(new DropData(itemId: 63128, chance: 2, amount: 1, expires: 360)); // Rundal Adv. Fomor Pass
				drops.Add(new DropData(itemId: 63105, chance: 5, amount: 1, expires: 600)); // Rundal Basic Fomor Pass
			}

			if (IsEnabled("RundalSirenDungeon"))
			{
				drops.Add(new DropData(itemId: 63103, chance: 3, amount: 1)); // Suspicious Fomor Pass
			}

			// Japan
			// drops.Add(new DropData(itemId: 40195, chance: 2, amount: 1)); // Yoshimitsu
		}

		return Item.GetRandomDrop(rnd, drops);
	}
}
