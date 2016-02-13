//--- Aura Script -----------------------------------------------------------
// Collect the White Wolf's Fomor Scrolls
//--- Description -----------------------------------------------------------
// Scroll collection quest, purchasable from shops.
//---------------------------------------------------------------------------

public class WhiteWolfScrollQuest : QuestScript
{
	public override void Load()
	{
		SetId(71011);
		SetScrollId(70085);
		SetName("Collect the White Wolf's Fomor Scrolls");
		SetDescription("The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 White Wolf Fomor Scrolls].");
		SetType(QuestType.Collect);

		AddObjective("collect", "Collect 10 White Wolf Fomor Scrolls", 0, 0, 0, Collect(71011, 10));

		AddReward(Gold(5900));
	}
}
