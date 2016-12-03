//--- Aura Script -----------------------------------------------------------
// AP Potion
//--- Description -----------------------------------------------------------
// Gives the user the amount of AP written on it.
//--- Notes -----------------------------------------------------------------
// The "writing" on it is the meta data 1 short integer value "AP".
//---------------------------------------------------------------------------

[ItemScript(85564, 85619, 85763, 85776)]
public class ApPotionItemScript : ItemScript
{
	public override void OnUse(Creature creature, Item item, string parameter)
	{
		var ap = item.MetaData1.GetShort("AP");
		if (ap <= 0)
		{
			Send.MsgBox(creature, L("Invalid potion."));
			return;
		}

		creature.AbilityPoints += ap;

		Send.StatUpdate(creature, StatUpdateType.Private, Stat.AbilityPoints);
		Send.AcquireInfo(creature, "ap", ap);
	}
}
