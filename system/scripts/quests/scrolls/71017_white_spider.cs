//--- Aura Script -----------------------------------------------------------
// Collect the White Spider's Fomor Scrolls
//--- Description -----------------------------------------------------------
// Scroll collection quest, purchasable from shops.
//---------------------------------------------------------------------------

public class WhiteSpiderScrollQuest : QuestScript
{
	public override void Load()
	{
		SetId(71017);
		SetScrollId(70091);
		SetName(L("Collect the White Spider's Fomor Scrolls"));
		SetDescription(L("The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 White Spider Fomor Scrolls]."));
		SetType(QuestType.Collect);
		SetCancelable(true);

		SetIcon(QuestIcon.Collect);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("collect", L("Collect 10 White Spider Fomor Scrolls"), 0, 0, 0, Collect(71017, 10));

		AddReward(Gold(520));
	}
}
