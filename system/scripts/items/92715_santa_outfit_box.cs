//--- Aura Script -----------------------------------------------------------
// Santa Outfit Box
//--- Description -----------------------------------------------------------
// Gives Old Saint Nick Outfit and Boots M or F, specific to the item
// user's gender.
//---------------------------------------------------------------------------

[ItemScript(92715)]
public class SantaOutfitBoxItemScript : ItemScript
{
	public override void OnUse(Creature creature, Item item, string parameter)
	{
		if (creature.IsMale)
		{
			creature.AcquireItem(Item.Create(15778)); // Old Saint Nick Outfit (M)
			creature.AcquireItem(Item.Create(17368)); // Old Saint Nick Boots (M)
		}
		else if (creature.IsFemale)
		{
			creature.AcquireItem(Item.Create(15779)); // Old Saint Nick Outfit (F)
			creature.AcquireItem(Item.Create(17369)); // Old Saint Nick Boots (F)
		}
	}
}
