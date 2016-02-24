//--- Aura Script -----------------------------------------------------------
// Collect the Burgundy Bear's Fomor Scrolls
//--- Description -----------------------------------------------------------
// Scroll collection quest, purchasable from shops.
//---------------------------------------------------------------------------

public class BurgundyBearScrollQuest : QuestScript
{
	public override void Load()
	{
		SetId(71074);
		SetScrollId(70104);
		SetName("Collect the Burgundy Bear's Fomor Scrolls");
		SetDescription("The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Burgundy Bear Fomor Scrolls].");
		SetType(QuestType.Collect);

		AddObjective("collect", "Collect 10 Burgundy Bear Fomor Scrolls", 0, 0, 0, Collect(71074, 10));

		AddReward(Gold(32000));
	}
}
