using Aura.Channel.Network.Sending;
using Aura.Channel.Scripting.Scripts;
using Aura.Channel.World.Entities;
using Aura.Mabi;
using Aura.Mabi.Const;
using Aura.Shared.Util;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

		// Calculate days
		var m1 = (erinnNow.Year * 7 * 40 * 24 * 60) + (erinnNow.Month * 40 * 24 * 60) + (erinnNow.Day * 24 * 60) + (erinnNow.Hour * 60) + erinnNow.Minute;
		var m2 = (erinnSpawn.Year * 7 * 40 * 24 * 60) + (erinnSpawn.Month * 40 * 24 * 60) + (erinnSpawn.Day * 24 * 60) + (erinnSpawn.Hour * 60) + erinnSpawn.Minute;
		var days = (m2 - m1) / 60.0 / 24.0;

		if (erinnSpawn.Hour * 100 + erinnSpawn.Minute > erinnNow.Hour * 100 + erinnNow.Minute)
			days = Math.Floor(days);
		else
			days = Math.Ceiling(days);

		// Get days part
		var time = "";
		if (erinnSpawn.DateTimeStamp == erinnNow.DateTimeStamp)
			time = L("Today");
		else if (erinnSpawn.DateTimeStamp == erinnNow.DateTimeStamp + 1)
			time = L("Tomorrow");
		else if (erinnSpawn.DateTimeStamp == erinnNow.DateTimeStamp + 2)
			time = L("the day after tomorrow");
		else
			time = string.Format(LN("{0} day", "{0} days", (int)days), (int)days);

		// Get time of day part
		var hour = erinnSpawn.Hour;
		if (hour >= 20)
			time += L(" night");
		else if (hour >= 12)
			time += L(" afternoon");
		else if (hour >= 6)
			time += L(" morning");
		else if (hour >= 0)
			time += L(" dawn");

		// Send message
		var msg = string.Format(Localization.Get("{0} has informed that {1} will appear in {2} at {3}."), announcerName, bossName, locationName, time);

		Send.Notice(creature.Region, NoticeType.Top, 20000, msg);
	}
}
