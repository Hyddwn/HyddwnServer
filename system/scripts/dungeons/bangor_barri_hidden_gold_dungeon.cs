//--- Aura Script -----------------------------------------------------------
// Barri Hidden Mine
//--- Description -----------------------------------------------------------
// Script for Barri Hidden Mine
//---------------------------------------------------------------------------

[DungeonScript("bangor_barri_hidden_gold_dungeon")]
public class BarriHiddenGoldDungeonScript : DungeonScript
{
	public override void OnBoss(Dungeon dungeon)
	{
		dungeon.AddBoss(100003, 1); // Ogre Warrior
		dungeon.AddBoss(100004, 1); // Ogre Warrior
		dungeon.AddBoss(100005, 1); // Ogre Warrior
		dungeon.AddBoss(100006, 1); // Ogre Warrior
		dungeon.AddBoss(100007, 1); // Ogre Warrior

		dungeon.PlayCutscene("bossroom_OgreBros");
	}

	public override void OnCleared(Dungeon dungeon)
	{
		var rnd = RandomProvider.Get();
		var creators = dungeon.GetCreators();

		for (int i = 0; i < creators.Count; ++i)
		{
			var member = creators[i];
			var treasureChest = new TreasureChest();

			// Warhammer
			int prefix = 0, suffix = 0;
			switch (rnd.Next(4))
			{
				case 0: prefix = 1704; break; // Imitation
				case 1: prefix = 1705; break; // Cheap
				case 2: prefix = 1706; break; // Good
				case 3: prefix = 305; break;  // Fine
			}
			treasureChest.Add(Item.CreateEnchanted(40016, prefix, suffix));

			treasureChest.AddGold(rnd.Next(2140, 2859)); // Gold
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
			drops.Add(new DropData(itemId: 51003, chance: 10, amountMin: 1, amountMax: 2)); // HP 50 Potion
			drops.Add(new DropData(itemId: 51008, chance: 10, amountMin: 1, amountMax: 2)); // MP 50 Potion
			drops.Add(new DropData(itemId: 51013, chance: 10, amountMin: 1, amountMax: 2)); // Stamina 50 Potion
			drops.Add(new DropData(itemId: 60039, chance: 10, amountMin: 3, amountMax: 5));  // Fomor Token
			drops.Add(new DropData(itemId: 71044, chance: 5, amountMin: 2, amountMax: 3));  // Imp Fomor Scroll
			drops.Add(new DropData(itemId: 62005, chance: 1, prefix: 21102)); // Solid
			drops.Add(new DropData(itemId: 40025, chance: 1, amount: 1, color1: 0xC00010, durability: 0)); // Pickaxe (bronze)
		}

		return Item.GetRandomDrop(rnd, drops);
	}
}