//--- Aura Script -----------------------------------------------------------
// Collect the Skeleton Wolf's Fomor Scrolls
//--- Description -----------------------------------------------------------
// Scroll collection quest, purchasable from shops.
//---------------------------------------------------------------------------

public class SkeletonWolfScrollQuest : QuestScript
{
	public override void Load()
	{
		SetId(71012);
		SetScrollId(70086);
		SetName(L("Collect the Skeleton Wolf's Fomor Scrolls"));
		SetDescription(L("The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Skeleton Wolf Fomor Scrolls]."));
		SetType(QuestType.Collect);
		SetCancelable(true);

		SetIcon(QuestIcon.Collect);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("collect", L("Collect 10 Skeleton Wolf Fomor Scrolls"), 0, 0, 0, Collect(71012, 10));

		AddReward(Gold(10800));
	}
}
