//--- Aura Script -----------------------------------------------------------
// Collect the Gray Town Rat's Fomor Scrolls
//--- Description -----------------------------------------------------------
// Scroll collection quest, purchasable from shops.
//---------------------------------------------------------------------------

public class GrayTownRatScrollQuest : QuestScript
{
	public override void Load()
	{
		SetId(71035);
		SetScrollId(70109);
		SetName("Collect the Gray Town Rat's Fomor Scrolls");
		SetDescription("The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Gray Town Rat Fomor Scrolls].");
		SetType(QuestType.Collect);

		AddObjective("collect", "Collect 10 Gray Town Rat Fomor Scrolls", 0, 0, 0, Collect(71035, 10));

		AddReward(Gold(1740));
	}
}
