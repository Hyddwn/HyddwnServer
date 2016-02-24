//--- Aura Script -----------------------------------------------------------
// Collect the Wisp's Fomor Scrolls
//--- Description -----------------------------------------------------------
// Scroll collection quest, purchasable from shops.
//---------------------------------------------------------------------------

public class WispScrollQuest : QuestScript
{
	public override void Load()
	{
		SetId(71045);
		SetScrollId(70119);
		SetName("Collect the Wisp's Fomor Scrolls");
		SetDescription("The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Wisp Fomor Scrolls].");
		SetType(QuestType.Collect);

		AddObjective("collect", "Collect 10 Wisp Fomor Scrolls", 0, 0, 0, Collect(71045, 10));

		AddReward(Gold(4800));
	}
}
