//--- Aura Script -----------------------------------------------------------
// Collect the Black Spider's Fomor Scrolls
//--- Description -----------------------------------------------------------
// Scroll collection quest, purchasable from shops.
//---------------------------------------------------------------------------

public class BlackSpiderScrollQuest : QuestScript
{
	public override void Load()
	{
		SetId(71018);
		SetScrollId(70092);
		SetName("Collect the Black Spider's Fomor Scrolls");
		SetDescription("The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Black Spider Fomor Scrolls].");
		SetType(QuestType.Collect);

		AddObjective("collect", "Collect 10 Black Spider Fomor Scrolls", 0, 0, 0, Collect(71018, 10));

		AddReward(Gold(1900));
	}
}
