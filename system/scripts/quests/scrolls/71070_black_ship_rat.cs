//--- Aura Script -----------------------------------------------------------
// Collect the Black Ship Rat's Fomor Scrolls
//--- Description -----------------------------------------------------------
// Scroll collection quest, purchasable from shops.
//---------------------------------------------------------------------------

public class BlackShipRatScrollQuest : QuestScript
{
	public override void Load()
	{
		SetId(71070);
		SetScrollId(70143);
		SetName("Collect the Black Ship Rat's Fomor Scrolls");
		SetDescription("The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Black Ship Rat Fomor Scrolls].");
		SetType(QuestType.Collect);

		AddObjective("collect", "Collect 10 Black Ship Rat Fomor Scrolls", 0, 0, 0, Collect(71070, 10));

		AddReward(Gold(2230));
	}
}
