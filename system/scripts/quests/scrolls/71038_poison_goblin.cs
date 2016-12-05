//--- Aura Script -----------------------------------------------------------
// Collect the Poison Goblin's Fomor Scrolls
//--- Description -----------------------------------------------------------
// Scroll collection quest, purchasable from shops.
//---------------------------------------------------------------------------

public class PoisonGoblinScrollQuest : QuestScript
{
	public override void Load()
	{
		SetId(71038);
		SetScrollId(70112);
		SetName(L("Collect the Poison Goblin's Fomor Scrolls"));
		SetDescription(L("The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Poison Goblin Fomor Scrolls]."));
		SetType(QuestType.Collect);
		SetCancelable(true);

		SetIcon(QuestIcon.Collect);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("collect", L("Collect 10 Poison Goblin Fomor Scrolls"), 0, 0, 0, Collect(71038, 10));

		AddReward(Gold(3240));
	}
}
