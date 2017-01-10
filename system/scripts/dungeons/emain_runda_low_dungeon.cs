//--- Aura Script -----------------------------------------------------------
// Rundal Basic Dungeon
//--- Description -----------------------------------------------------------
// Dungeon script for Rundal Basic.
//---------------------------------------------------------------------------

[DungeonScript("emain_runda_low_dungeon")]
public class RundalBasicDungeonScript : DungeonScript
{
	public override void OnBoss(Dungeon dungeon)
	{
		dungeon.AddBoss(17001, 1); // Cat Sith Knights
		dungeon.AddBoss(17002, 1); // Cat Sith Knights
		dungeon.AddBoss(17003, 1); // Cat Sith Knights
		dungeon.AddBoss(17004, 3); // Cat Sith Knights

		dungeon.PlayCutscene("bossroom_Runda_Catsith");
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
				// Cores' Angel wing
				int prefix = 0, suffix = 0;
				switch (rnd.Next(3))
				{
					case 0: prefix = 20601; break; // Blessed
					case 1: prefix = 20406; break; // Convenient
					case 2: prefix = 30307; break; // Red Bear
				}
				treasureChest.Add(Item.CreateEnchanted(18052, prefix, suffix));
			}

			treasureChest.AddGold(rnd.Next(3906, 6390)); // Gold
			treasureChest.Add(GetRandomTreasureItem(rnd)); // Random item

			dungeon.AddChest(treasureChest);

			member.GiveItemWithEffect(Item.CreateKey(70028, "chest"));
		}
	}

	public Item GetRandomTreasureItem(Random rnd)
	{
		var drops = new List<DropData>();

		drops.Add(new DropData(itemId: 62004, chance: 17, amountMin: 1, amountMax: 2)); // Magic Powder
		drops.Add(new DropData(itemId: 51102, chance: 17, amountMin: 1, amountMax: 2)); // Mana Herb
		drops.Add(new DropData(itemId: 51003, chance: 17, amountMin: 1, amountMax: 2)); // HP 50 Potion
		drops.Add(new DropData(itemId: 51008, chance: 17, amountMin: 1, amountMax: 2)); // MP 50 Potion
		drops.Add(new DropData(itemId: 51013, chance: 17, amountMin: 1, amountMax: 2)); // Stamina 50 Potion

		if (IsEnabled("RundalDungeon"))
		{
			drops.Add(new DropData(itemId: 63126, chance: 2, amount: 1, expires: 360)); // Rundal Adv. Fomor Pass for 2
			drops.Add(new DropData(itemId: 63127, chance: 2, amount: 1, expires: 300)); // Rundal Adv. Fomor Pass for 3
			drops.Add(new DropData(itemId: 63128, chance: 2, amount: 1, expires: 360)); // Rundal Adv. Fomor Pass
			drops.Add(new DropData(itemId: 63105, chance: 3, amount: 1, expires: 480)); // Rundal Basic Fomor Pass
		}

		if (IsEnabled("RundalSirenDungeon"))
		{
			drops.Add(new DropData(itemId: 63103, chance: 3, amount: 1)); // Suspicious Fomor Pass
		}

		if (IsEnabled("NaoDressUp"))
		{
			var dressId = 80003; // Black Coat
			if (ErinnTime.Now.Month == ErinnMonth.Imbolic)
				dressId = 80002; // Pink Coat

			drops.Add(new DropData(itemId: dressId, chance: 3, amount: 1));
		}

		return Item.GetRandomDrop(rnd, drops);
	}
}
