//--- Aura Script -----------------------------------------------------------
// Collect the Rat Man's Fomor Scrolls
//--- Description -----------------------------------------------------------
// Scroll collection quest, purchasable from shops.
//---------------------------------------------------------------------------

public class RatManScrollQuest : QuestScript
{
	public override void Load()
	{
		SetId(71067);
		SetScrollId(70140);
		SetName("Collect the Rat Man's Fomor Scrolls");
		SetDescription("The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Rat Man Fomor Scrolls].");
		SetType(QuestType.Collect);

		AddObjective("collect", "Collect 10 Rat Man Fomor Scrolls", 0, 0, 0, Collect(71067, 10));

		AddReward(Gold(3120));
	}
}
