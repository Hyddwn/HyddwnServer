//--- Aura Script -----------------------------------------------------------
// Double Rainbow Event
//--- Description -----------------------------------------------------------
// Doubles various values while it's active, like AP and EXP reveiced,
// gold drop amount, skill training, and more.
// Reference: http://wiki.mabinogiworld.com/view/Double_Rainbow_Event
//---------------------------------------------------------------------------

public class DoubleRainbowEventScript : GameEventScript
{
	public override void Load()
	{
		SetId("aura_double_rainbow");
		SetName(L("Double Rainbow"));
	}

	public override void AfterLoad()
	{
		ScheduleEvent(DateTime.Parse("2013-11-01 00:00"), TimeSpan.FromHours(36));
		ScheduleEvent(DateTime.Parse("2016-03-10 00:00"), DateTime.Parse("2016-03-16 00:00"));
	}

	protected override void OnStart()
	{
		AddGlobalBonus(GlobalBonusStat.LevelUpAp, 2);
		AddGlobalBonus(GlobalBonusStat.CombatExp, 2);
		AddGlobalBonus(GlobalBonusStat.QuestExp, 2);
		AddGlobalBonus(GlobalBonusStat.SkillTraining, 2);
		AddGlobalBonus(GlobalBonusStat.ItemDropRate, 2);
		AddGlobalBonus(GlobalBonusStat.GoldDropRate, 2);
		AddGlobalBonus(GlobalBonusStat.GoldDropAmount, 2);
		AddGlobalBonus(GlobalBonusStat.LuckyFinishRate, 2);
	}

	protected override void OnEnd()
	{
		RemoveGlobalBonuses();
	}
}
