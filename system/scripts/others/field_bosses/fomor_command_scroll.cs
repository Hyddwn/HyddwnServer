//--- Aura Script -----------------------------------------------------------
// Fomor Command Scroll
//--- Description -----------------------------------------------------------
// Sends region-wide notice about when and where the next Field Boss
// will appear.
//---------------------------------------------------------------------------

[ItemScript(63021)]
public class FomorCommandScrollItemScript : ItemScript
{
	public override void OnUse(Creature creature, Item item, string parameter)
	{
		var erinnNow = ErinnTime.Now;
		var spawnTime = item.MetaData1.GetDateTime("BSGRTM");
		if (spawnTime < erinnNow.DateTime)
		{
			Send.MsgBox(creature, Localization.Get("This Fomor Command Scroll is no longer available."));
			return;
		}

		var erinnSpawn = new ErinnTime(spawnTime);
		var bossName = item.MetaData1.GetString("BSGRNM");
		var locationName = item.MetaData1.GetString("BSGRPS");
		var announcerName = creature.Name;
		var time = GetTimeSpanString(erinnNow, erinnSpawn);
		var msg = string.Format(Localization.Get("{0} has informed that {1} will appear in {2} at {3}."), announcerName, bossName, locationName, time);

		Send.Notice(creature.Region, NoticeType.Top, 20000, msg);
	}
}
