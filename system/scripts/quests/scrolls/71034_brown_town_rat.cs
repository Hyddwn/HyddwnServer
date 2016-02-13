//--- Aura Script -----------------------------------------------------------
// Collect the Brown Town Rat's Fomor Scrolls
//--- Description -----------------------------------------------------------
// Scroll collection quest, purchasable from shops.
//---------------------------------------------------------------------------

public class BrownTownRatScrollQuest : QuestScript
{
	public override void Load()
	{
		SetId(71034);
		SetScrollId(70108);
		SetName("Collect the Brown Town Rat's Fomor Scrolls");
		SetDescription("The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Brown Town Rat Fomor Scrolls].");
		SetType(QuestType.Collect);

		AddObjective("collect", "Collect 10 Brown Town Rat Fomor Scrolls", 0, 0, 0, Collect(71034, 10));

		AddReward(Gold(1150));
	}
}
