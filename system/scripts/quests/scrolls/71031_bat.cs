//--- Aura Script -----------------------------------------------------------
// Collect the Bat's Fomor Scrolls
//--- Description -----------------------------------------------------------
// Scroll collection quest, purchasable from shops.
//---------------------------------------------------------------------------

public class BatScrollQuest : QuestScript
{
	public override void Load()
	{
		SetId(71031);
		SetScrollId(70105);
		SetName("Collect the Bat's Fomor Scrolls");
		SetDescription("The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Bat Fomor Scrolls].");
		SetType(QuestType.Collect);

		AddObjective("collect", "Collect 10 Bat Fomor Scrolls", 0, 0, 0, Collect(71031, 10));

		AddReward(Gold(900));
	}
}
