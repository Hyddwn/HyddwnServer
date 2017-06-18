//--- Aura Script -----------------------------------------------------------
// Rundal Siren Dungeon
//--- Description -----------------------------------------------------------
// Dungeon script for Rundal Siren.
//---------------------------------------------------------------------------

[DungeonScript("emain_runda_middle_siren_dungeon")]
public class RundalSirenDungeonScript : DungeonScript
{
	public override void OnBoss(Dungeon dungeon)
	{
		dungeon.AddBoss(10701, 1); // Siren
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
				switch (rnd.Next(3))
				{
					case 0: enchant = 30613; break; // Imp (Suffix)
					case 1: enchant = 30402; break; // Stone (Suffix)
					case 2: enchant = 30701; break;  // Homunculus (Suffix)
				}
				treasureChest.Add(Item.CreateEnchant(enchant));
			}
			
			treasureChest.AddGold(rnd.Next(6144, 10032)); // Gold
			treasureChest.Add(GetRandomTreasureItem(rnd)); // Random item

			dungeon.AddChest(treasureChest);

			member.GiveItemWithEffect(Item.CreateKey(70028, "chest"));
		}
		
		foreach (var member in dungeon.GetCreators())
		{
			if (!member.HasKeyword("mini_aer_killsiren"))
				member.Keywords.Give("mini_aer_killsiren");	
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
		drops.Add(new DropData(itemId: 60043, chance: 10, amountMin: 1, amountMax: 5)); // Fog Silk

		if (IsEnabled("RundalDungeon"))
		{
			drops.Add(new DropData(itemId: 63126, chance: 2, amount: 1, expires: 360)); // Rundal Adv. Fomor Pass for 2
			drops.Add(new DropData(itemId: 63127, chance: 2, amount: 1, expires: 300)); // Rundal Adv. Fomor Pass for 3
			drops.Add(new DropData(itemId: 63128, chance: 2, amount: 1, expires: 360)); // Rundal Adv. Fomor Pass
			drops.Add(new DropData(itemId: 63105, chance: 3, amount: 1, expires: 480)); // Rundal Basic Fomor Pass
		}

		if (IsEnabled("RundalSirenDungeon"))
		{
			drops.Add(new DropData(itemId: 63103, chance: 5, amount: 1)); // Suspicious Fomor Pass
		}

		if (IsEnabled("NaoDressUp"))
		{
			var dressId = 80005; // Nao's White Spring Dress
			if (ErinnTime.Now.Month == ErinnMonth.Imbolic)
				dressId = 80004; // Nao's Yellow Spring Dress
			else if (ErinnTime.Now.Month == ErinnMonth.Samhain)
				dressId = 80006; // Nao's Pink Spring Dress

			drops.Add(new DropData(itemId: dressId, chance: 3, amount: 1));
		}

		return Item.GetRandomDrop(rnd, drops);
	}
}
