//--- Aura Script -----------------------------------------------------------
// Math Dungeon
//--- Description -----------------------------------------------------------
// Dungeon script for Math Valentine's Day Event Dungeon
//---------------------------------------------------------------------------

[DungeonScript("dunbarton_math_valentine_dungeon")]
public class MathValentineDungeonScript : DungeonScript
{
	public override void OnBoss(Dungeon dungeon)
	{
		dungeon.AddBoss(20201); // Hellhound
		dungeon.AddBoss(10236, 6); // Valentine Kobold
		dungeon.AddBoss(10237, 5); // Valentine Poison Kobold
		dungeon.AddBoss(10238, 4); // Valentine Kobold Archer
	}

	public override void OnCleared(Dungeon dungeon)
	{
		var rnd = RandomProvider.Get();
		var creators = dungeon.GetCreators();

		for (int i = 0; i < creators.Count; ++i)
		{
			var member = creators[i];
			var treasureChest = new TreasureChest();

			treasureChest.AddGold(rnd.Next(800, 1200)); // Gold
			treasureChest.Add(GetRandomTreasureItem(rnd)); // Random item
			treasureChest.Add(GetRandomChocolate(rnd)); // Random chocolate

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
			drops.Add(new DropData(itemId: 64042, chance: 10, scale: 1)); // 10cm Topaz
			drops.Add(new DropData(itemId: 64043, chance: 10, scale: 1)); // 10cm Star Sapphire
			drops.Add(new DropData(itemId: 64044, chance: 10, scale: 1)); // 10cm Emerald
			drops.Add(new DropData(itemId: 64045, chance: 10, scale: 1)); // 10cm Aquamarine
			drops.Add(new DropData(itemId: 64046, chance: 10, scale: 1)); // 10cm Garnet
			drops.Add(new DropData(itemId: 64047, chance: 10, scale: 1)); // 10cm Jasper
			drops.Add(new DropData(itemId: 64048, chance: 10, scale: 1)); // 10cm Ruby
			drops.Add(new DropData(itemId: 64049, chance: 10, scale: 1)); // 10cm Spinel
			drops.Add(new DropData(itemId: 64049, chance: 10, scale: 1)); // 10cm Diamond
			drops.Add(new DropData(itemId: 75497, chance: 10)); // I Love You! Gesture Coupon
			drops.Add(new DropData(itemId: 51003, chance: 10, amount: 10)); // HP 50 potion x10
			drops.Add(new DropData(itemId: 51004, chance: 10, amount: 10)); // HP 100 potion x10
			drops.Add(new DropData(itemId: 51013, chance: 10, amount: 10)); // Stamina 50 potion x10
			drops.Add(new DropData(itemId: 51014, chance: 10, amount: 10)); // Stamina 100 potion x10
			drops.Add(new DropData(itemId: 51008, chance: 10, amount: 10)); // MP 50 potion x10
			drops.Add(new DropData(itemId: 51009, chance: 10, amount: 10)); // MP 100 potion x10
		}

		return Item.GetRandomDrop(rnd, drops);
	}
	
	List<DropData> chocoDrops;
	public Item GetRandomChocolate(Random rnd)
	{
		if (chocoDrops == null)
		{
			chocoDrops = new List<DropData>();
			chocoDrops.Add(new DropData(itemId: 50708, chance: 20)); // Soy Chocolate
			chocoDrops.Add(new DropData(itemId: 50709, chance: 20)); // Mustard Chocolate
			chocoDrops.Add(new DropData(itemId: 50710, chance: 20)); // Wasabi Chocolate
			chocoDrops.Add(new DropData(itemId: 50711, chance: 20)); // Herbal Chocolate
			chocoDrops.Add(new DropData(itemId: 50712, chance: 20)); // Vinegar Chocolate
		}

		return Item.GetRandomDrop(rnd, chocoDrops);
	}
}
