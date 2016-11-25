public class TestEventScript : GameEventScript
{
	public override void Load()
	{
		SetId("aura_test_event");
		SetName(L("Test"));
	}

	public override void AfterLoad()
	{
		ScheduleEvent(Id, DateTime.Parse("2016-11-25 13:00"), DateTime.Parse("2016-11-25 13:05"));
	}

	protected override void OnStart()
	{
		Send.Notice(NoticeType.Middle, "test event started");
		AddGlobalBonus(GlobalBonusStat.CombatExp, 2);
	}

	protected override void OnEnd()
	{
		Send.Notice(NoticeType.Middle, "test event stopped");
	}
}
