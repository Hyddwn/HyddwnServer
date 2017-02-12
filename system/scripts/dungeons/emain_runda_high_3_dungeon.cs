//--- Aura Script -----------------------------------------------------------
// Rundal Advanced for 3 Dungeon
//--- Description -----------------------------------------------------------
// Dungeon script for Rundal Advanced for 3.
//---------------------------------------------------------------------------

[DungeonScript("emain_runda_high_3_dungeon")]
public class RundalAdv3DungeonScript : DungeonScript
{
	public override void OnBoss(Dungeon dungeon)
	{
		dungeon.AddBoss(100202, 1); // Cyclops
		dungeon.AddBoss(190003, 2); // Flying Sword
		dungeon.AddBoss(11015, 4); // Captain Skeleton

		dungeon.PlayCutscene("bossroom_Runda_Cyclops2");
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
				int suffix = 0;
				switch (rnd.Next(3))
				{
					case 0: suffix = 30808; break; // Captain's
					case 1: suffix = 30307; break; // Red Bear
					case 2: suffix = 30515; break; // Pirate
				}
				treasureChest.Add(Item.CreateEnchant(suffix));
			}

			treasureChest.AddGold(rnd.Next(6480, 10800)); // Gold
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
			drops.Add(new DropData(itemId: 62004, chance: 13, amountMin: 1, amountMax: 2)); // Magic Powder
			drops.Add(new DropData(itemId: 51102, chance: 12, amountMin: 1, amountMax: 2)); // Mana Herb
			drops.Add(new DropData(itemId: 51003, chance: 15, amountMin: 1, amountMax: 2)); // HP 50 Potion
			drops.Add(new DropData(itemId: 51008, chance: 15, amountMin: 1, amountMax: 2)); // MP 50 Potion
			drops.Add(new DropData(itemId: 51013, chance: 15, amountMin: 1, amountMax: 2)); // Stamina 50 Potion
			drops.Add(new DropData(itemId: 71007, chance: 5, amountMin: 5, amountMax: 5)); // Red Skeleton Fomor Scroll

			if (IsEnabled("RundalDungeon"))
			{
				drops.Add(new DropData(itemId: 63126, chance: 5, amount: 1, expires: 360)); // Rundal Adv. Fomor Pass for 2
				drops.Add(new DropData(itemId: 63127, chance: 5, amount: 1, expires: 360)); // Rundal Adv. Fomor Pass for 3
				drops.Add(new DropData(itemId: 63128, chance: 5, amount: 1, expires: 360)); // Rundal Adv. Fomor Pass
				drops.Add(new DropData(itemId: 63105, chance: 5, amount: 1, expires: 600)); // Rundal Basic Fomor Pass
			}

			if (IsEnabled("RundalSirenDungeon"))
			{
				drops.Add(new DropData(itemId: 63103, chance: 3, amount: 1)); // Suspicious Fomor Pass
			}
		}

		return Item.GetRandomDrop(rnd, drops);
	}
}
