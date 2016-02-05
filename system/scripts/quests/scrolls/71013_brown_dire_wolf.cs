//--- Aura Script -----------------------------------------------------------
// Collect the Brown Dire Wolf's Fomor Scrolls
//--- Description -----------------------------------------------------------
// Scroll collection quest, purchasable from shops.
//---------------------------------------------------------------------------

public class BrownDireWolfScrollQuest : QuestScript
{
	public override void Load()
	{
		SetId(71013);
		SetScrollId(70087);
		SetName("Collect the Brown Dire Wolf's Fomor Scrolls");
		SetDescription("The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Brown Dire Wolf Fomor Scrolls].");
		SetType(QuestType.Collect);

		AddObjective("collect", "Collect 10 Brown Dire Wolf Fomor Scrolls", 0, 0, 0, Collect(71013, 10));

		AddReward(Gold(5000));
	}
}
