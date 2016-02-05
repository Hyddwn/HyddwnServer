//--- Aura Script -----------------------------------------------------------
// Collect the Brown Fox's Fomor Scrolls
//--- Description -----------------------------------------------------------
// Scroll collection quest, purchasable from shops.
//---------------------------------------------------------------------------

public class BrownFoxScrollQuest : QuestScript
{
	public override void Load()
	{
		SetId(71021);
		SetScrollId(70095);
		SetName("Collect the Brown Fox's Fomor Scrolls");
		SetDescription("The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Brown Fox Fomor Scrolls].");
		SetType(QuestType.Collect);

		AddObjective("collect", "Collect 10 Brown Fox Fomor Scrolls", 0, 0, 0, Collect(71021, 10));

		AddReward(Gold(760));
	}
}
