//--- Aura Script -----------------------------------------------------------
// Collect the Skeleton's Fomor Scrolls
//--- Description -----------------------------------------------------------
// Scroll collection quest, purchasable from shops.
//---------------------------------------------------------------------------

public class SkeletonScrollQuest : QuestScript
{
	public override void Load()
	{
		SetId(71006);
		SetScrollId(70080);
		SetName("Collect the Skeleton's Fomor Scrolls");
		SetDescription("The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Skeleton Fomor Scrolls].");
		SetType(QuestType.Collect);

		AddObjective("collect", "Collect 10 Skeleton Fomor Scrolls", 0, 0, 0, Collect(71006, 10));

		AddReward(Gold(7800));
	}
}
