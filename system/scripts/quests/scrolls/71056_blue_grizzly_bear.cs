//--- Aura Script -----------------------------------------------------------
// Collect the Blue Grizzly Bear's Fomor Scrolls
//--- Description -----------------------------------------------------------
// Scroll collection quest, purchasable from shops.
//---------------------------------------------------------------------------

public class BlueGrizzlyBearScrollQuest : QuestScript
{
	public override void Load()
	{
		SetId(71056);
		SetScrollId(70129);
		SetName(L("Collect the Blue Grizzly Bear's Fomor Scrolls"));
		SetDescription(L("The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Blue Grizzly Bear Fomor Scrolls]."));
		SetType(QuestType.Collect);
		SetCancelable(true);

		SetIcon(QuestIcon.Collect);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("collect", L("Collect 10 Blue Grizzly Bear Fomor Scrolls"), 0, 0, 0, Collect(71056, 10));

		AddReward(Gold(12200));
	}
}
