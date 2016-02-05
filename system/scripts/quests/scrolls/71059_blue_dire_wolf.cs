//--- Aura Script -----------------------------------------------------------
// Collect the Blue Dire Wolf's Fomor Scrolls
//--- Description -----------------------------------------------------------
// Scroll collection quest, purchasable from shops.
//---------------------------------------------------------------------------

public class BlueDireWolfScrollQuest : QuestScript
{
	public override void Load()
	{
		SetId(71059);
		SetScrollId(70132);
		SetName("Collect the Blue Dire Wolf's Fomor Scrolls");
		SetDescription("The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Blue Dire Wolf Fomor Scrolls].");
		SetType(QuestType.Collect);

		AddObjective("collect", "Collect 10 Blue Dire Wolf Fomor Scrolls", 0, 0, 0, Collect(71059, 10));

		AddReward(Gold(3500));
	}
}
