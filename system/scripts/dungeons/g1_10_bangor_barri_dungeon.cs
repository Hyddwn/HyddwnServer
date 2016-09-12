//--- Aura Script -----------------------------------------------------------
// Brown Fomor Pass dungeon
//--- Description -----------------------------------------------------------
// Second dungeon in G1 mainstream, to get Fomor Medal keyword and item.
//---------------------------------------------------------------------------

[DungeonScript("g1_10_bangor_barri_dungeon")]
public class BrownFomorPassDungeonScript : DungeonScript
{
	public override void OnBoss(Dungeon dungeon)
	{
		dungeon.AddBoss(10008, 1); // Black Wizard
		dungeon.AddBoss(80101, 8); // Wisp

		dungeon.PlayCutscene("G1_10_a_BlackWizard_solo", cutscene =>
		{
			var creators = dungeon.GetCreators();
			foreach (var member in creators)
			{
				if (member.Keywords.Has("g1_10"))
				{
					member.Keywords.Remove("g1_10");
					member.Keywords.Give("g1_11");
					member.Keywords.Give("g1_medal_of_fomor");
				}
			}
		});
	}

	public override void OnLeftEarly(Dungeon dungeon, Creature creature)
	{
		dungeon.PlayCutscene("G1_LeaveDungeon");
	}

	public override void OnCleared(Dungeon dungeon)
	{
		var rnd = RandomProvider.Get();
		var creators = dungeon.GetCreators();

		for (int i = 0; i < creators.Count; ++i)
		{
			var member = creators[i];
			var treasureChest = new TreasureChest();

			// Fomor Medal
			if (member.Keywords.Has("g1_medal_of_fomor"))
				treasureChest.Add(Item.Create(73021));

			treasureChest.AddGold(rnd.Next(576, 1440)); // Gold
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
			drops.Add(new DropData(itemId: 62004, chance: 25, amountMin: 1, amountMax: 2)); // Magic Powder
			drops.Add(new DropData(itemId: 51102, chance: 25, amountMin: 1, amountMax: 2)); // Mana Herb
			drops.Add(new DropData(itemId: 51003, chance: 25, amountMin: 1, amountMax: 2)); // HP 50 Potion
			drops.Add(new DropData(itemId: 51008, chance: 25, amountMin: 1, amountMax: 2)); // MP 50 Potion
		}

		return Item.GetRandomDrop(rnd, drops);
	}
}
