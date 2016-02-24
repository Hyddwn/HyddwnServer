//--- Aura Script -----------------------------------------------------------
// Collect the Dark Blue Spider's Fomor Scrolls
//--- Description -----------------------------------------------------------
// Scroll collection quest, purchasable from shops.
//---------------------------------------------------------------------------

public class DarkBlueSpiderScrollQuest : QuestScript
{
	public override void Load()
	{
		SetId(71069);
		SetScrollId(70142);
		SetName("Collect the Dark Blue Spider's Fomor Scrolls");
		SetDescription("The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Dark Blue Spider Fomor Scrolls].");
		SetType(QuestType.Collect);

		AddObjective("collect", "Collect 10 Dark Blue Spider Fomor Scrolls", 0, 0, 0, Collect(71069, 10));

		AddReward(Gold(1930));
	}
}
