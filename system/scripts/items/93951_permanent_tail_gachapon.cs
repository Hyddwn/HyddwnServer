//--- Aura Script -----------------------------------------------------------
// Permanent Tail Gachapon
//--- Description -----------------------------------------------------------
// Gives one of 5 random permanent tails.
// Reference: http://wiki.mabinogiworld.com/view/Wishbone_Exchange_Event#Wishbone_Shop
//---------------------------------------------------------------------------

[ItemScript(93951)]
public class PermanentTailGachapon93951ItemScript : ItemScript
{
	public override void OnUse(Creature creature, Item item, string parameter)
	{
		var rnd = RandomProvider.Get();
		var rndItem = Item.GetRandomDrop(rnd, items);

		creature.AcquireItem(rndItem);
	}

	private static List<DropData> items = new List<DropData>
	{
		new DropData(itemId: 9500, chance: 20), // Fluffy Puppy Tail (Permanent Version)
		new DropData(itemId: 9501, chance: 20), // Fluffy Kitty Tail (Permanent Version)
		new DropData(itemId: 9502, chance: 20), // Fluffy Fox Tail (Permanent Version)
		new DropData(itemId: 9503, chance: 20), // Fluffy Squirrel Tail (Permanent Version)
		new DropData(itemId: 9504, chance: 20), // Fluffy Tiger Tail (Permanent Version)
	};
}
