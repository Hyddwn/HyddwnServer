//--- Aura Script -----------------------------------------------------------
// Collect the Goblin's Fomor Scrolls
//--- Description -----------------------------------------------------------
// Scroll collection quest, purchasable from shops.
//---------------------------------------------------------------------------

public class GoblinScrollQuest : QuestScript
{
	public override void Load()
	{
		SetId(71037);
		SetScrollId(70111);
		SetName(L("Collect the Goblin's Fomor Scrolls"));
		SetDescription(L("The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Goblin Fomor Scrolls]."));
		SetType(QuestType.Collect);
		SetCancelable(true);

		SetIcon(QuestIcon.Collect);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("collect", L("Collect 10 Goblin Fomor Scrolls"), 0, 0, 0, Collect(71037, 10));

		AddReward(Gold(3100));
	}
}
