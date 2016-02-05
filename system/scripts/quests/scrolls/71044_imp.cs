//--- Aura Script -----------------------------------------------------------
// Collect the Imp's Fomor Scrolls
//--- Description -----------------------------------------------------------
// Scroll collection quest, purchasable from shops.
//---------------------------------------------------------------------------

public class ImpScrollQuest : QuestScript
{
	public override void Load()
	{
		SetId(71044);
		SetScrollId(70118);
		SetName("Collect the Imp's Fomor Scrolls");
		SetDescription("The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Imp Fomor Scrolls].");
		SetType(QuestType.Collect);

		AddObjective("collect", "Collect 10 Imp Fomor Scrolls", 0, 0, 0, Collect(71044, 10));

		AddReward(Gold(2700));
	}
}
