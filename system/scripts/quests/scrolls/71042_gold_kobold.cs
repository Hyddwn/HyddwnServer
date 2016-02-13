//--- Aura Script -----------------------------------------------------------
// Collect the Gold Kobold's Fomor Scrolls
//--- Description -----------------------------------------------------------
// Scroll collection quest, purchasable from shops.
//---------------------------------------------------------------------------

public class GoldKoboldScrollQuest : QuestScript
{
	public override void Load()
	{
		SetId(71042);
		SetScrollId(70116);
		SetName("Collect the Gold Kobold's Fomor Scrolls");
		SetDescription("The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Gold Kobold Fomor Scrolls].");
		SetType(QuestType.Collect);

		AddObjective("collect", "Collect 10 Gold Kobold Fomor Scrolls", 0, 0, 0, Collect(71042, 10));

		AddReward(Gold(6800));
	}
}
