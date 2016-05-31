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
		// Gem dungeons...

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
