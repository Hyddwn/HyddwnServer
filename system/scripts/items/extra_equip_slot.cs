//--- Aura Script -----------------------------------------------------------
// Extra Equipment Slot (Expansion) Coupon
//--- Description -----------------------------------------------------------
// Add additional extra equipment slots and extend the time in which
// players can use them.
//---------------------------------------------------------------------------

// Extra Equipment Slot Coupon (30 days)
[ItemScript(94144)]
public class ExtraEquipmentSlotCouponScript : ItemScript
{
	public override void OnUse(Creature creature, Item item, string parameter)
	{
		creature.ExtendExtraEquipmentSetsTime(TimeSpan.FromDays(30));

		// Add first kit if there are none yet
		if (creature.ExtraEquipmentSetsCount == 0)
			creature.AddExtraEquipmentSet();

		creature.Inventory.Decrement(item);
		creature.Notice(L("You've used Extra Equipment Slot Coupon (30 days) item(s)."));
	}
}

// Extra Equipment Slot Expansion Coupon
[ItemScript(94145)]
public class ExtraEquipmentSlotExpansionCouponScript : ItemScript
{
	public override void OnUse(Creature creature, Item item, string parameter)
	{
		// Add first kit if there are none yet
		if (creature.ExtraEquipmentSetsCount == 0)
			creature.AddExtraEquipmentSet();

		// Try to add additional set
		if (!creature.AddExtraEquipmentSet())
		{
			// Unofficial
			creature.Notice(L("You have reached the maximum number of extra equipment slots."));
			return;
		}

		creature.Inventory.Decrement(item);
		creature.Notice(L("Added an extra equipment slot."));
	}
}
