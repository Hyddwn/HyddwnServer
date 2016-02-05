//--- Aura Script -----------------------------------------------------------
// Collect the Gray Fox's Fomor Scrolls
//--- Description -----------------------------------------------------------
// Scroll collection quest, purchasable from shops.
//---------------------------------------------------------------------------

public class GrayFoxScrollQuest : QuestScript
{
	public override void Load()
	{
		SetId(71023);
		SetScrollId(70097);
		SetName("Collect the Gray Fox's Fomor Scrolls");
		SetDescription("The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Gray Fox Fomor Scrolls].");
		SetType(QuestType.Collect);

		AddObjective("collect", "Collect 10 Gray Fox Fomor Scrolls", 0, 0, 0, Collect(71023, 10));

		AddReward(Gold(1050));
	}
}
