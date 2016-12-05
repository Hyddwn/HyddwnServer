//--- Aura Script -----------------------------------------------------------
// Soldier Gachapon
//--- Description -----------------------------------------------------------
// Gives a random item from a collection of rewards.
// Reference: http://wiki.mabinogiworld.com/view/Soldier_Gachapon_(2009)
//---------------------------------------------------------------------------

[ItemScript(91036)]
public class SoldierGachapon91036ItemScript : ItemScript
{
	public override void OnUse(Creature creature, Item item, string parameter)
	{
		var rnd = RandomProvider.Get();
		var rndItem = Item.GetRandomDrop(rnd, items);

		creature.AcquireItem(rndItem);
	}

	private static List<DropData> items = new List<DropData>
	{
		// The chance is the probability here, if you have 3 items with the
		// chances 1, 10, and 20, they have the actual chances of 3.2%,
		// 32.3%, and 64.5% to drop. ( 100/(1+10+20)*X )

		// Soldier Gachapon-Only Items
		new DropData(itemId: 40171, chance: 1), // Masamune
		new DropData(itemId: 40193, chance: 1), // Muramasa
		new DropData(itemId: 16506, chance: 1), // Ring Gloves
		new DropData(itemId: 18523, chance: 1), // Swan Wing Cap
		new DropData(itemId: 40194, chance: 1), // Tanto
		new DropData(itemId: 40195, chance: 1), // Yoshimitsu

		// Enchanted Items
		new DropData(itemId: 13033, chance: 1, suffix: 30901), // (Pitch Black) Valencia's Cross Line Plate Armor (M)
		new DropData(itemId: 40080, chance: 1, suffix: 30702), // (Raven) Gladius

		// Rare-Color Items
		new DropData(itemId: 40080, chance: 1, color1: 0x1000000d), // !Pink! Gladius
		new DropData(itemId: 40010, chance: 1, color1: 0x1000000e), // !Light-Blue! Longsword

		// Weapons
		new DropData(itemId: 40012, chance: 4), // Bastard Sword
		new DropData(itemId: 40078, chance: 4), // Bipennis
		new DropData(itemId: 40033, chance: 4), // Claymore
		new DropData(itemId: 40041, chance: 4), // Combat Wand
		new DropData(itemId: 40014, chance: 4), // Composite Bow
		new DropData(itemId: 40031, chance: 4), // Crossbow
		new DropData(itemId: 40006, chance: 4), // Dagger
		new DropData(itemId: 40040, chance: 4), // Fire Wand
		new DropData(itemId: 40015, chance: 4), // Fluted Short Sword
		new DropData(itemId: 40080, chance: 4), // Gladius
		new DropData(itemId: 40090, chance: 4), // Healing Wand
		new DropData(itemId: 40083, chance: 4), // Hooked Cutlass
		new DropData(itemId: 40039, chance: 4), // Ice Wand
		new DropData(itemId: 40081, chance: 4), // Leather Long Bow
		new DropData(itemId: 40010, chance: 4), // Longsword
		new DropData(itemId: 40038, chance: 4), // Lightning Wand
		new DropData(itemId: 40079, chance: 4), // Mace
		new DropData(itemId: 40005, chance: 4), // Short Sword
		new DropData(itemId: 40192, chance: 4), // Wakizashi
		new DropData(itemId: 40016, chance: 4), // Warhammer

		// Shields
		new DropData(itemId: 46006, chance: 4), // Kite Shield
		new DropData(itemId: 46008, chance: 4), // Light Hetero Kite Shield

		// Equipment
		new DropData(itemId: 13046, chance: 4), // Arish Ashuvain Armor (F)
		new DropData(itemId: 13045, chance: 4), // Arish Ashuvain Armor (M)
		new DropData(itemId: 17519, chance: 4), // Arish Ashuvain Boots (F)
		new DropData(itemId: 17518, chance: 4), // Arish Ashuvain Boots (M)
		new DropData(itemId: 18509, chance: 4), // Bascinet
		new DropData(itemId: 18544, chance: 4), // Big Pelican Protector
		new DropData(itemId: 18547, chance: 4), // Big Panache Head Protector
		new DropData(itemId: 18514, chance: 4), // Bird Face Cap
		new DropData(itemId: 17019, chance: 4), // Blacksmith Shoes
		new DropData(itemId: 18502, chance: 4), // Bone Helm
		new DropData(itemId: 14023, chance: 4), // Bone Marine Armor (M)
		new DropData(itemId: 14024, chance: 4), // Bone Marine Armor (F)
		new DropData(itemId: 17064, chance: 4), // Camelle Spirit Boots
		new DropData(itemId: 16008, chance: 4), // Cores' Thief Gloves
		new DropData(itemId: 18504, chance: 4), // Cross Full Helm
		new DropData(itemId: 18503, chance: 4), // Cuirassier Helm
		new DropData(itemId: 18518, chance: 4), // Dragon Crest
		new DropData(itemId: 14005, chance: 4), // Drandos Leather Mail (F)
		new DropData(itemId: 13038, chance: 4), // Dustin Silver Knight Armor
		new DropData(itemId: 18521, chance: 4), // European Comb
		new DropData(itemId: 18516, chance: 4), // Evil Dying Crown
		new DropData(itemId: 17016, chance: 4), // Field Combat Shoes
		new DropData(itemId: 18511, chance: 4), // Fluted Full Helm
		new DropData(itemId: 16505, chance: 4), // Fluted Gauntlet
		new DropData(itemId: 17503, chance: 4), // Greaves
		new DropData(itemId: 18501, chance: 4), // Guardian Helm
		new DropData(itemId: 17005, chance: 4), // Hunter Boots
		new DropData(itemId: 18517, chance: 4), // Iron Mask Headgear
		new DropData(itemId: 13048, chance: 4), // Kirinusjin's Half-plate Armor (F)
		new DropData(itemId: 13047, chance: 4), // Kirinusjin's Half-plate Armor (M)
		new DropData(itemId: 17017, chance: 4), // Leather Coat Shoes
		new DropData(itemId: 16000, chance: 4), // Leather Gloves
		new DropData(itemId: 17012, chance: 4), // Leather Shoes (Type 1)
		new DropData(itemId: 17003, chance: 4), // Leather Shoes (Type 4)
		new DropData(itemId: 13044, chance: 4), // Leminia's Holy Moon Armor (F)
		new DropData(itemId: 13043, chance: 4), // Leminia's Holy Moon Armor (M)
		new DropData(itemId: 14006, chance: 4), // Linen Cuirass (F)
		new DropData(itemId: 16002, chance: 4), // Linen Gloves
		new DropData(itemId: 17021, chance: 4), // Lorica Sandals
		new DropData(itemId: 17004, chance: 4), // Magic School Shoes
		new DropData(itemId: 18519, chance: 4), // Panache Head Protector
		new DropData(itemId: 17505, chance: 4), // Plate Boots
		new DropData(itemId: 16001, chance: 4), // Quilting Gloves
		new DropData(itemId: 18500, chance: 4), // Ring Mail Helm
		new DropData(itemId: 17504, chance: 4), // Round Polean Plate Boots
		new DropData(itemId: 18508, chance: 4), // Slit Full Helm
		new DropData(itemId: 16004, chance: 4), // Studded Bracelet
		new DropData(itemId: 18513, chance: 4), // Spiked Cap
		new DropData(itemId: 16013, chance: 4), // Swordsman Gloves
		new DropData(itemId: 17018, chance: 4), // Swordsman Shoes
		new DropData(itemId: 16012, chance: 4), // Swordswoman Gloves
		new DropData(itemId: 17002, chance: 4), // Swordswoman Shoes
		new DropData(itemId: 17020, chance: 4), // Thief Shoes
		new DropData(itemId: 18515, chance: 4), // Twin Horn Cap
		new DropData(itemId: 16500, chance: 4), // Ulna Protector Gloves
		new DropData(itemId: 13033, chance: 4), // Valencia's Cross Line Plate Armor (M)
		new DropData(itemId: 18525, chance: 4), // Waterdrop Cap
		new DropData(itemId: 18506, chance: 4), // Wing Half Helm

		// Enchant Scrolls
		new DropData(itemId: 62005, chance: 40, prefix: 7),     // Prefix, Rank E - Fox Hunter's
		new DropData(itemId: 62005, chance: 40, prefix: 8),     // Prefix, Rank C - Wolf Hunter's
		new DropData(itemId: 62005, chance: 40, prefix: 207),   // Prefix, Rank B - Fox
		new DropData(itemId: 62005, chance: 40, prefix: 20714), // Prefix, Rank 9 - Silver Fox
		new DropData(itemId: 62005, chance: 40, suffix: 30302), // Suffix, Rank D - Kobold
		new DropData(itemId: 62005, chance: 40, suffix: 30501), // Suffix, Rank B - Giant
		new DropData(itemId: 62005, chance: 40, suffix: 30702), // Suffix, Rank 9 - Raven
		new DropData(itemId: 62005, chance: 40, suffix: 31002), // Suffix, Rank 6 - Knight

		// Advanced Play Items
		//new DropData(itemId: 91006, chance: 50), // Age Potion: 10 Year Old - 30 Days
		//new DropData(itemId: 91010, chance: 50), // Age Potion: 11 Year Old - 30 Days
		//new DropData(itemId: 91011, chance: 50), // Age Potion: 12 Year Old - 30 Days
		//new DropData(itemId: 91014, chance: 50), // Age Potion: 15 Year Old - 30 Days
		//new DropData(itemId: 91015, chance: 50), // Age Potion: 16 Year Old - 30 Days
		//new DropData(itemId: 91007, chance: 50), // Age Potion: 17 Year Old - 30 Days
		new DropData(itemId: 63025, chance: 50), // Massive Holy Water of Lymilark
		//new DropData(itemId: 63027, chance: 50), // Waxen Wing of Goddess

		// Potions
		new DropData(itemId: 51028, chance: 60, amountMin: 3, amountMax: 5), // HP & Stamina 50 Potion
		new DropData(itemId: 51004, chance: 60, amountMin: 3, amountMax: 5), // HP 100 Potion
		new DropData(itemId: 51008, chance: 60, amountMin: 3, amountMax: 5), // MP 50 Potion
		new DropData(itemId: 51013, chance: 60, amountMin: 3, amountMax: 5), // Stamina 50 Potion
		new DropData(itemId: 51014, chance: 60, amountMin: 3, amountMax: 5), // Stamina 100 Potion
	};
}
