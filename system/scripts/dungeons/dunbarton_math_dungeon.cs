//--- Aura Script -----------------------------------------------------------
// Math Dungeon
//--- Description -----------------------------------------------------------
// Math router and script for Math Normal.
//---------------------------------------------------------------------------

[DungeonScript("dunbarton_math_dungeon")]
public class MathDungeonScript : DungeonScript
{
	public override bool Route(Creature creature, Item item, ref string dungeonName)
	{
		// Broken Torque (G1, Mores RP)
		if (item.Info.Id == 73003)
		{
			if (!creature.Keywords.Has("g1_21"))
			{
				Send.Notice(creature, L("You can't enter this dungeon right now."));
				return false;
			}

			if (creature.Party.MemberCount != 1)
			{
				Send.Notice(creature, L("You must enter this dungeon alone."));
				return false;
			}

			dungeonName = "g1rp_18_dunbarton_math_dungeon";
			return true;
		}

		// Broken Torque (G1, Shiela+Mores RP)
		if (item.Info.Id == 73005)
		{
			if (!creature.Keywords.Has("g1_34_1"))
			{
				Send.Notice(creature, L("You can't enter this dungeon right now."));
				return false;
			}

			if (creature.Party.MemberCount != 2 && !IsEnabled("SoloRP"))
			{
				Send.Notice(creature, L("To enter this dungeon, you need a party with 2 members."));
				return false;
			}

			dungeonName = "g1rp_31_dunbarton_math_dungeon";
			return true;
		}

		// Fall back for unknown passes
		if (item.IsDungeonPass)
		{
			Send.Notice(creature, L("This dungeon hasn't been implemented yet."));
			return false;
		}

		// dunbarton_math_dungeon
		return true;
	}

	public override void OnCreation(Dungeon dungeon)
	{
	}

	public override void OnBoss(Dungeon dungeon)
	{
		dungeon.AddBoss(20201, 3); // Hellhound

		dungeon.PlayCutscene("bossroom_HellHound");
	}

	public override void OnCleared(Dungeon dungeon)
	{
		var rnd = RandomProvider.Get();
		var creators = dungeon.GetCreators();

		for (int i = 0; i < creators.Count; ++i)
		{
			var member = creators[i];
			var treasureChest = new TreasureChest();

			// Lute
			int prefix = 0, suffix = 0;
			switch (rnd.Next(3))
			{
				case 0: prefix = 4; break; // Donkey Hunter's
				case 1: prefix = 1506; break; // Swan Summoner's
				case 2: prefix = 1707; break; // Sturdy
			}
			switch (rnd.Next(3))
			{
				case 0: suffix = 10806; break; // Understanding
				case 1: suffix = 10504; break; // Topaz
				case 2: suffix = 10706; break; // Wind
			}
			switch (rnd.Next(3))
			{
				case 0: treasureChest.Add(Item.CreateEnchanted(40042, prefix, suffix)); break;
				case 1: treasureChest.Add(Item.CreateEnchanted(40042, prefix, 0)); break;
				case 2: treasureChest.Add(Item.CreateEnchanted(40042, 0, suffix)); break;
			}

			treasureChest.AddGold(rnd.Next(576, 2880)); // Gold
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
			drops.Add(new DropData(itemId: 62004, chance: 18, amountMin: 1, amountMax: 2)); // Magic Powder
			drops.Add(new DropData(itemId: 51102, chance: 18, amountMin: 1, amountMax: 2)); // Mana Herb
			drops.Add(new DropData(itemId: 51003, chance: 18, amountMin: 1, amountMax: 2)); // HP 50 Potion
			drops.Add(new DropData(itemId: 51008, chance: 18, amountMin: 1, amountMax: 2)); // MP 50 Potion
			drops.Add(new DropData(itemId: 51013, chance: 18, amountMin: 1, amountMax: 2)); // Stamina 50 Potion
			drops.Add(new DropData(itemId: 71018, chance: 3, amountMin: 3, amountMax: 5));  // Black Spider Fomor Scroll
			drops.Add(new DropData(itemId: 71035, chance: 3, amountMin: 3, amountMax: 5)); // Gray Town Rat Fomor Scroll
			drops.Add(new DropData(itemId: 40003, chance: 1, amount: 1, color1: 0x12644A)); // Short Bow (green)
			drops.Add(new DropData(itemId: 46001, chance: 2, amount: 1, color1: 0xEBBE21, durability: 0)); // Round Shield (gold)

			if (IsEnabled("MathAdvanced"))
				drops.Add(new DropData(itemId: 63131, chance: 1, amount: 1, expires: 480)); // Math Advanced
		}

		return Item.GetRandomDrop(rnd, drops);
	}
}
