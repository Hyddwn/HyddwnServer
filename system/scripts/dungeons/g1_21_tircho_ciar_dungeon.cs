//--- Aura Script -----------------------------------------------------------
// Ciar Dungeon (Wizard's Note)
//--- Description -----------------------------------------------------------
// A normal Ciar, with the Book of Revenge, Vol II in the end chest.
//---------------------------------------------------------------------------

[DungeonScript("g1_21_tircho_ciar_dungeon")]
public class WizardsNoteCiarDungeonScript : DungeonScript
{
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

			if (member.Keywords.Has("g1_25"))
				treasureChest.Add(Item.Create(73054)); // Book of Revenge Vol 2

			treasureChest.AddGold(rnd.Next(326, 800)); // Gold
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
			drops.Add(new DropData(itemId: 51013, chance: 15, amountMin: 1, amountMax: 2)); // Stamina 50 Potion
		}

		return Item.GetRandomDrop(rnd, drops);
	}
}
