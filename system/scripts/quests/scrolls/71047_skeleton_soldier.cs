//--- Aura Script -----------------------------------------------------------
// Collect the Skeleton Solider's Fomor Scrolls
//--- Description -----------------------------------------------------------
// Scroll collection quest, purchasable from shops.
//---------------------------------------------------------------------------

public class SkeletonSoliderScrollQuest : QuestScript
{
	public override void Load()
	{
		SetId(71047);
		SetScrollId(70121);
		SetName("Collect the Skeleton Solider's Fomor Scrolls");
		SetDescription("The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Skeleton Solider Fomor Scrolls].");
		SetType(QuestType.Collect);

		AddObjective("collect", "Collect 10 Skeleton Solider Fomor Scrolls", 0, 0, 0, Collect(71047, 10));

		AddReward(Gold(4500));
	}
}
