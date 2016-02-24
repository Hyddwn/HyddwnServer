//--- Aura Script -----------------------------------------------------------
// Collect the Brown Bear's Fomor Scrolls
//--- Description -----------------------------------------------------------
// Scroll collection quest, purchasable from shops.
//---------------------------------------------------------------------------

public class BrownBearScrollQuest : QuestScript
{
	public override void Load()
	{
		SetId(71025);
		SetScrollId(70099);
		SetName("Collect the Brown Bear's Fomor Scrolls");
		SetDescription("The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Brown Bear Fomor Scrolls].");
		SetType(QuestType.Collect);

		AddObjective("collect", "Collect 10 Brown Bear Fomor Scrolls", 0, 0, 0, Collect(71025, 10));

		AddReward(Gold(7400));
	}
}
