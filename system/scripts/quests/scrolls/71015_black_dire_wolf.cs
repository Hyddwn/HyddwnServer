//--- Aura Script -----------------------------------------------------------
// Collect the Black Dire Wolf's Fomor Scrolls
//--- Description -----------------------------------------------------------
// Scroll collection quest, purchasable from shops.
//---------------------------------------------------------------------------

public class BlackDireWolfScrollQuest : QuestScript
{
	public override void Load()
	{
		SetId(71015);
		SetScrollId(70089);
		SetName(L("Collect the Black Dire Wolf's Fomor Scrolls"));
		SetDescription(L("The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Black Dire Wolf Fomor Scrolls]."));
		SetType(QuestType.Collect);
		SetCancelable(true);

		SetIcon(QuestIcon.Collect);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("collect", L("Collect 10 Black Dire Wolf Fomor Scrolls"), 0, 0, 0, Collect(71015, 10));

		AddReward(Gold(7100));
	}
}
