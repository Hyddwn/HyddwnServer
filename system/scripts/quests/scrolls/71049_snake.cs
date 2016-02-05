//--- Aura Script -----------------------------------------------------------
// Collect the Snake's Fomor Scrolls
//--- Description -----------------------------------------------------------
// Scroll collection quest, purchasable from shops.
//---------------------------------------------------------------------------

public class SnakeScrollQuest : QuestScript
{
	public override void Load()
	{
		SetId(71049);
		SetScrollId(70123);
		SetName("Collect the Snake's Fomor Scrolls");
		SetDescription("The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Snake Fomor Scrolls].");
		SetType(QuestType.Collect);

		AddObjective("collect", "Collect 10 Snake Fomor Scrolls", 0, 0, 0, Collect(71049, 10));

		AddReward(Gold(2300));
	}
}
