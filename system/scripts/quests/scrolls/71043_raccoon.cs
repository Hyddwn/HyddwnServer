//--- Aura Script -----------------------------------------------------------
// Collect the Raccoon's Fomor Scrolls
//--- Description -----------------------------------------------------------
// Scroll collection quest, purchasable from shops.
//---------------------------------------------------------------------------

public class RaccoonScrollQuest : QuestScript
{
	public override void Load()
	{
		SetId(71043);
		SetScrollId(70117);
		SetName("Collect the Raccoon's Fomor Scrolls");
		SetDescription("The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Raccoon Fomor Scrolls].");
		SetType(QuestType.Collect);

		AddObjective("collect", "Collect 10 Raccoon Fomor Scrolls", 0, 0, 0, Collect(71043, 10));

		AddReward(Gold(700));
	}
}
