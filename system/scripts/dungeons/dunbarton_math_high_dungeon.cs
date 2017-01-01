//--- Aura Script -----------------------------------------------------------
// Math Advanced Dungeon
//--- Description -----------------------------------------------------------
// Dungeon script for Math Advanced.
//---------------------------------------------------------------------------

[DungeonScript("dunbarton_math_high_dungeon")]
public class MathAdvDungeonScript : DungeonScript
{
	public override void OnBoss(Dungeon dungeon)
	{
		dungeon.AddBoss(20019, 2); // Giant Skeleton Hellhound
		dungeon.AddBoss(11019, 2); // Bard Skeleton

		dungeon.PlayCutscene("bossroom_Math_BardSkeleton_GiantSkeletonHellhound");
	}

	public override void OnBossDeath(Dungeon dungeon, Creature boss, Creature killer)
	{
		// Transform Bard Skeletons when their hounds were killed.
		if (boss.RaceId == 20019 && dungeon.RemainingBosses == 2)
		{
			dungeon.RemoveAllMonsters();
			dungeon.AddBoss(11020, 2); // Metal Bard Skeleton
			dungeon.PlayCutscene("bossroom_Math_BardSkeleton_change");
		}
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
				var prefix = 0;
				switch (rnd.Next(3))
				{
					case 0: prefix = 20711; break; // Famous
					case 1: prefix = 20712; break; // Ornamented
					case 2: prefix = 20402; break; // Strong
				}

				treasureChest.Add(Item.CreateEnchant(prefix));
			}

			treasureChest.AddGold(rnd.Next(1980, 3630)); // Gold
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
			drops.Add(new DropData(itemId: 51003, chance: 17, amountMin: 1, amountMax: 2)); // HP 50 Potion
			drops.Add(new DropData(itemId: 51008, chance: 17, amountMin: 1, amountMax: 2)); // MP 50 Potion
			drops.Add(new DropData(itemId: 51013, chance: 17, amountMin: 1, amountMax: 2)); // Stamina 50 Potion

			if (IsEnabled("MathAdvanced"))
			{
				drops.Add(new DropData(itemId: 63129, chance: 5, amount: 1, expires: 360)); // Math Adv. Fomor Pass for 2
				drops.Add(new DropData(itemId: 63130, chance: 5, amount: 1, expires: 360)); // Math Adv. Fomor Pass for 3
				drops.Add(new DropData(itemId: 63131, chance: 5, amount: 1, expires: 360)); // Math Adv. Fomor Pass
			}
		}

		return Item.GetRandomDrop(rnd, drops);
	}
}
