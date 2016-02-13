//--- Aura Script -----------------------------------------------------------
// Collect the Kobold's Fomor Scrolls
//--- Description -----------------------------------------------------------
// Scroll collection quest, purchasable from shops.
//---------------------------------------------------------------------------

public class KoboldScrollQuest : QuestScript
{
	public override void Load()
	{
		SetId(71040);
		SetScrollId(70114);
		SetName("Collect the Kobold's Fomor Scrolls");
		SetDescription("The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Kobold Fomor Scrolls].");
		SetType(QuestType.Collect);

		AddObjective("collect", "Collect 10 Kobold Fomor Scrolls", 0, 0, 0, Collect(71040, 10));

		AddReward(Gold(4100));
	}
}
