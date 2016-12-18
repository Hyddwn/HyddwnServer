//--- Aura Script -----------------------------------------------------------
// Collect the Burgundy Spider's Fomor Scrolls
//--- Description -----------------------------------------------------------
// Scroll collection quest, purchasable from shops.
//---------------------------------------------------------------------------

public class BurgundySpiderScrollQuest : QuestScript
{
	public override void Load()
	{
		SetId(71058);
		SetScrollId(70131);
		SetName(L("Collect the Burgundy Spider's Fomor Scrolls"));
		SetDescription(L("The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Burgundy Spider Fomor Scrolls]."));
		SetType(QuestType.Collect);
		SetCancelable(true);

		SetIcon(QuestIcon.Collect);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("collect", L("Collect 10 Burgundy Spider Fomor Scrolls"), 0, 0, 0, Collect(71058, 10));

		AddReward(Gold(2000));
	}
}
