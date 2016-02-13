//--- Aura Script -----------------------------------------------------------
// Collect the Wild Boar's Fomor Scrolls
//--- Description -----------------------------------------------------------
// Scroll collection quest, purchasable from shops.
//---------------------------------------------------------------------------

public class WildBoarScrollQuest : QuestScript
{
	public override void Load()
	{
		SetId(71062);
		SetScrollId(70135);
		SetName("Collect the Wild Boar's Fomor Scrolls");
		SetDescription("The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Wild Boar Fomor Scrolls].");
		SetType(QuestType.Collect);

		AddObjective("collect", "Collect 10 Wild Boar Fomor Scrolls", 0, 0, 0, Collect(71062, 10));

		AddReward(Gold(1700));
	}
}
