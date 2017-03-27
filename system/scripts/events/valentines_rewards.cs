//--- Aura Script -----------------------------------------------------------
// Valentines Day rewards script
//--- Description -----------------------------------------------------------
// Contains both Male and Female variants of the Valentine's Gift Boxes
//---------------------------------------------------------------------------

[ItemScript(91514)]
public class FemaleValentinesBox : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		List<DropData> list;

		list = new List<DropData>();

		// Hats
		list.Add(new DropData(itemId: 18955, chance: 30)); // Heart Eyepatch
		list.Add(new DropData(itemId: 18957, chance: 30)); // Lollipop Heart Eyepatch
		list.Add(new DropData(itemId: 18392, chance: 30)); // Heart-shaped Glasses
		list.Add(new DropData(itemId: 28610, chance: 20)); // Macaroon Mistress Hat
		list.Add(new DropData(itemId: 210363, chance: 20)); // Macaroon Mistress Wig
		list.Add(new DropData(itemId: 18834, chance: 20)); // Lady Waffle Cone Bow
		list.Add(new DropData(itemId: 28725, chance: 10)); // Waffle Witch Wig
		list.Add(new DropData(itemId: 28726, chance: 10)); // Waffle Witch Wig and Hat
		list.Add(new DropData(itemId: 28727, chance: 10)); // Waffle Witch Hat

		// Clothing
		list.Add(new DropData(itemId: 80736, chance: 20)); // Macaroon Mistress Dress
		list.Add(new DropData(itemId: 15785, chance: 20)); // Lady Waffle Cone Dress
		list.Add(new DropData(itemId: 80858, chance: 10)); // Waffle Witch Dress

		// Shoes
		list.Add(new DropData(itemId: 17429, chance: 20));  // Macaroon Mistress Shoes
		list.Add(new DropData(itemId: 17373, chance: 20));  // Lady Waffle Cone Ribbon Shoes
		list.Add(new DropData(itemId: 17826, chance: 10));  // Waffle Witch Shoes

		// Gloves
		list.Add(new DropData(itemId: 16200, chance: 20));  // Lady Waffle Cone Heart Ring

		// Weapons
		list.Add(new DropData(itemId: 40723, chance: 20)); // Heart Lightning Wand
		list.Add(new DropData(itemId: 40724, chance: 20)); // Heart Fire Wand
		list.Add(new DropData(itemId: 40725, chance: 20)); // Heart Ice Wand
		list.Add(new DropData(itemId: 41141, chance: 20)); // Lady Waffle Cone Heart Clutch
		list.Add(new DropData(itemId: 41275, chance: 15)); // Heart Glow Stick (red)

		// Wings
		list.Add(new DropData(itemId: 19194, chance: 5)); // Hot Pink Heart Wings

		// Useables
		list.Add(new DropData(itemId: 45118, chance: 30, amount: 5)); // Heart Shaped Fireworks Kit

		var rnd = RandomProvider.Get();
		var item = Item.GetRandomDrop(rnd, list);

		if (item != null)
		{
			cr.Inventory.Add(item, true);
		}
	}
}

[ItemScript(91515)]
public class MaleValentinesBox : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		List<DropData> list;

		list = new List<DropData>();

		// Hats
		list.Add(new DropData(itemId: 18955, chance: 30)); // Heart Eyepatch
		list.Add(new DropData(itemId: 18957, chance: 30)); // Lollipop Heart Eyepatch
		list.Add(new DropData(itemId: 18392, chance: 30)); // Heart-shaped Glasses
		list.Add(new DropData(itemId: 28609, chance: 20)); // Count Cookie Hat (M)
		list.Add(new DropData(itemId: 210361, chance: 20)); // Count Cookie Wig
		list.Add(new DropData(itemId: 18833, chance: 20)); // Lord Waffle Cone Hat
		list.Add(new DropData(itemId: 28722, chance: 10)); // Waffle Wizard Wig
		list.Add(new DropData(itemId: 28723, chance: 10)); // Waffle Wizard Wig and Hat
		list.Add(new DropData(itemId: 28724, chance: 10)); // Waffle Wizard Hat

		// Clothing
		list.Add(new DropData(itemId: 80735, chance: 20)); // Count Cookie Suit
		list.Add(new DropData(itemId: 15784, chance: 20)); // Lord Waffle Cone Suit
		list.Add(new DropData(itemId: 80857, chance: 10)); // Waffle Wizard Suit

		// Shoes
		list.Add(new DropData(itemId: 17428, chance: 20));  // Count Cookie Shoes
		list.Add(new DropData(itemId: 17372, chance: 20));  // Lord Waffle Cone Shoes
		list.Add(new DropData(itemId: 17825, chance: 10));  // Waffle Wizard Shoes

		// Gloves
		list.Add(new DropData(itemId: 16199, chance: 20));  // Lord Waffle Cone Bracelet

		// Weapons
		list.Add(new DropData(itemId: 40723, chance: 20)); // Heart Lightning Wand
		list.Add(new DropData(itemId: 40724, chance: 20)); // Heart Fire Wand
		list.Add(new DropData(itemId: 40725, chance: 20)); // Heart Ice Wand
		list.Add(new DropData(itemId: 41140, chance: 20)); // Lord Waffle Cone Heart Key
		list.Add(new DropData(itemId: 41275, chance: 15)); // Heart Glow Stick (red)

		// Wings
		list.Add(new DropData(itemId: 19194, chance: 5)); // Hot Pink Heart Wings

		// Useables
		list.Add(new DropData(itemId: 45118, chance: 30, amount: 5)); // Heart Shaped Fireworks Kit

		var rnd = RandomProvider.Get();
		var item = Item.GetRandomDrop(rnd, list);

		if (item != null)
		{
			cr.Inventory.Add(item, true);
		}
	}
}
