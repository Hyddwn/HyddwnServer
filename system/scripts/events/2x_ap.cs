//--- Aura Script -----------------------------------------------------------
// 2x AP Event
//--- Description -----------------------------------------------------------
// Doubles AP received by leveling up.
// Reference: http://wiki.mabinogiworld.com/view/2x_AP_Event
//---------------------------------------------------------------------------

public class DoubleRainbowEventScript : GameEventScript
{
	public override void Load()
	{
		SetId("aura_2x_ap");
		SetName(L("2x AP"));
	}

	public override void AfterLoad()
	{
		ScheduleEvent(DateTime.Parse("2013-08-31 00:00"), TimeSpan.FromHours(36));
		ScheduleEvent(DateTime.Parse("2013-09-07 00:00"), TimeSpan.FromHours(36));
		ScheduleEvent(DateTime.Parse("2013-09-14 00:00"), TimeSpan.FromHours(36));
	}

	protected override void OnStart()
	{
		AddGlobalBonus(GlobalBonusStat.LevelUpAp, 2);

		Send.Notice(NoticeType.Middle, L("The {0} Event is now in progress."), Name);
	}

	protected override void OnEnd()
	{
		RemoveGlobalBonuses();

		Send.Notice(NoticeType.Middle, L("The {0} Event has ended."), Name);
	}
}
