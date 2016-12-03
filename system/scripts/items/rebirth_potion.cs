//--- Aura Script -----------------------------------------------------------
// Rebirth Potion
//--- Description -----------------------------------------------------------
// Changes player's rebirth time, so they can rebirth again immediately.
//---------------------------------------------------------------------------

[ItemScript("/usable/rebirth_reset/")]
public class RebirthPotionItemScript : ItemScript
{
	public override void OnUse(Creature creature, Item item, string parameter)
	{
		var minRebirth = ChannelServer.Instance.Conf.World.RebirthTime;
		creature.LastRebirth = DateTime.Now.Add(-minRebirth);

		Send.Notice(creature, L("You're now able to be reborn."));
	}
}
