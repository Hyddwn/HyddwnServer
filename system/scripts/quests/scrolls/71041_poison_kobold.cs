//--- Aura Script -----------------------------------------------------------
// Collect the Poison Kobold's Fomor Scrolls
//--- Description -----------------------------------------------------------
// Scroll collection quest, purchasable from shops.
//---------------------------------------------------------------------------

public class PoisonKoboldScrollQuest : QuestScript
{
	public override void Load()
	{
		SetId(71041);
		SetScrollId(70115);
		SetName("Collect the Poison Kobold's Fomor Scrolls");
		SetDescription("The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Poison Kobold Fomor Scrolls].");
		SetType(QuestType.Collect);

		AddObjective("collect", "Collect 10 Poison Kobold Fomor Scrolls", 0, 0, 0, Collect(71041, 10));

		AddReward(Gold(5100));
	}
}
