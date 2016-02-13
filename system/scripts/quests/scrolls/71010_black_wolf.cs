//--- Aura Script -----------------------------------------------------------
// Collect the Black Wolf's Fomor Scrolls
//--- Description -----------------------------------------------------------
// Scroll collection quest, purchasable from shops.
//---------------------------------------------------------------------------

public class BlackWolfScrollQuest : QuestScript
{
	public override void Load()
	{
		SetId(71010);
		SetScrollId(70084);
		SetName("Collect the Black Wolf's Fomor Scrolls");
		SetDescription("The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Black Wolf Fomor Scrolls].");
		SetType(QuestType.Collect);

		AddObjective("collect", "Collect 10 Black Wolf Fomor Scrolls", 0, 0, 0, Collect(71010, 10));

		AddReward(Gold(4100));
	}
}
