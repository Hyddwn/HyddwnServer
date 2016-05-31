//--- Aura Script -----------------------------------------------------------
// Ciar Beginner Dungeon
//--- Description -----------------------------------------------------------
// Script for Ciar Beginner.
//---------------------------------------------------------------------------

[DungeonScript("tircho_ciar_beginner_1_dungeon")]
public class CiarBeginnerDungeonScript : DungeonScript
{
	public override void OnBoss(Dungeon dungeon)
	{
		dungeon.AddBoss(130014, 1); // Small Golem

		dungeon.PlayCutscene("bossroom_small_golem_Ciar");
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
				// Enchant
				var enchant = 0;
				switch (rnd.Next(6))
				{
					case 0: enchant = 11105; break; // Health (Suffix)
					case 1: enchant = 11106; break; // Blood (Suffix)
					case 2: enchant = 11205; break; // Water (Suffix)
					case 3: enchant = 11206; break; // Fountain (Suffix)
					case 4: enchant = 11304; break; // Patience (Suffix)
					case 5: enchant = 11305; break; // Sustainer (Suffix)
				}
				treasureChest.Add(Item.CreateEnchant(enchant));
			}

			treasureChest.AddGold(rnd.Next(640, 960)); // Gold
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
			drops.Add(new DropData(itemId: 62004, chance: 17, amountMin: 1, amountMax: 2)); // Magic Powder
			drops.Add(new DropData(itemId: 51102, chance: 17, amountMin: 1, amountMax: 2)); // Mana Herb
			drops.Add(new DropData(itemId: 51003, chance: 15, amountMin: 1, amountMax: 2)); // HP 50 Potion
			drops.Add(new DropData(itemId: 51008, chance: 15, amountMin: 1, amountMax: 2)); // MP 50 Potion
			drops.Add(new DropData(itemId: 51013, chance: 15, amountMin: 1, amountMax: 2)); // Stamina 50 Potion
			drops.Add(new DropData(itemId: 71037, chance: 4, amountMin: 2, amountMax: 4)); // Goblin Fomor Scroll
			drops.Add(new DropData(itemId: 71035, chance: 4, amountMin: 3, amountMax: 5)); // Gray Town Rat Fomor Scroll
			drops.Add(new DropData(itemId: 63104, chance: 3, amount: 1, expires: 480)); // Ciar Basic Fomor Pass
			drops.Add(new DropData(itemId: 63123, chance: 2, amount: 1, expires: 480)); // Ciar Intermediate Fomor Pass for One
			drops.Add(new DropData(itemId: 63124, chance: 2, amount: 1, expires: 480)); // Ciar Intermediate Fomor Pass for Two
			drops.Add(new DropData(itemId: 63125, chance: 2, amount: 1, expires: 480)); // Ciar Intermediate Fomor Pass for Four
			drops.Add(new DropData(itemId: 40006, chance: 2, amount: 1, color1: 0xFFDB60, durability: 0)); // Dagger (gold)
			// advanced passes gX
		}

		return Item.GetRandomDrop(rnd, drops);
	}
}
