//--- Aura Script -----------------------------------------------------------
// Collect the Mimic's Fomor Scrolls
//--- Description -----------------------------------------------------------
// Scroll collection quest, purchasable from shops.
//---------------------------------------------------------------------------

public class MimicScrollQuest : QuestScript
{
	public override void Load()
	{
		SetId(71032);
		SetScrollId(70106);
		SetName("Collect the Mimic's Fomor Scrolls");
		SetDescription("The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Mimic Fomor Scrolls].");
		SetType(QuestType.Collect);

		AddObjective("collect", "Collect 10 Mimic Fomor Scrolls", 0, 0, 0, Collect(71032, 10));

		AddReward(Gold(6000));
	}
}
