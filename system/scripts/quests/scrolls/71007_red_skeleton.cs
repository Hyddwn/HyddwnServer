//--- Aura Script -----------------------------------------------------------
// Collect the Red Skeleton's Fomor Scrolls
//--- Description -----------------------------------------------------------
// Scroll collection quest, purchasable from shops.
//---------------------------------------------------------------------------

public class RedSkeletonScrollQuest : QuestScript
{
	public override void Load()
	{
		SetId(71007);
		SetScrollId(70081);
		SetName("Collect the Red Skeleton's Fomor Scrolls");
		SetDescription("The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Red Skeleton Fomor Scrolls].");
		SetType(QuestType.Collect);

		AddObjective("collect", "Collect 10 Red Skeleton Fomor Scrolls", 0, 0, 0, Collect(71007, 10));

		AddReward(Gold(11100));
	}
}
