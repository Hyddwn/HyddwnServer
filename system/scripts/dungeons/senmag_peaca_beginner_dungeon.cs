//--- Aura Script -----------------------------------------------------------
// Peaca Beginner Dungeon
//--- Description -----------------------------------------------------------
// Script for  Peaca Beginner Dungeon
// Peaca Beginner seems to use a duplicate of Barri Normal's rewards
// with different rates and with no enchanted equipment.
//---------------------------------------------------------------------------

[DungeonScript("senmag_peaca_beginner_dungeon")]
public class PeacaBeginnerDungeonScript : DungeonScript
{
	public override void OnBoss(Dungeon dungeon)
	{
		dungeon.AddBoss(120010, 1); // Black Ship Rat
	}

	public override void OnCleared(Dungeon dungeon)
	{
		var rnd = RandomProvider.Get();
		var creators = dungeon.GetCreators();

		for (int i = 0; i < creators.Count; ++i)
		{
			var member = creators[i];
			var treasureChest = new TreasureChest();

			treasureChest.AddGold(rnd.Next(1800, 3000)); // Gold
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
			drops.Add(new DropData(itemId: 60039, chance: 10, amountMin: 3, amountMax: 5));  // Fomor Token
			drops.Add(new DropData(itemId: 71044, chance: 4, amountMin: 2, amountMax: 5));  // Imp Fomor Scroll
			drops.Add(new DropData(itemId: 71037, chance: 3, amountMin: 2, amountMax: 4));  // Goblin Fomor Scroll (officially Imp duplicate #officialFix)
			drops.Add(new DropData(itemId: 40025, chance: 1, amount: 1, color1: 0xC00010, durability: 0)); // Pickaxe (bronze)
			drops.Add(new DropData(itemId: 63113, chance: 2, amount: 1, expires: 600)); // Barri Basic
		}

		return Item.GetRandomDrop(rnd, drops);
	}
}