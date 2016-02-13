//--- Aura Script -----------------------------------------------------------
// Collect the Jackal's Fomor Scrolls
//--- Description -----------------------------------------------------------
// Scroll collection quest, purchasable from shops.
//---------------------------------------------------------------------------

public class JackalScrollQuest : QuestScript
{
	public override void Load()
	{
		SetId(71052);
		SetScrollId(70126);
		SetName("Collect the Jackal's Fomor Scrolls");
		SetDescription("The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Jackal Fomor Scrolls].");
		SetType(QuestType.Collect);

		AddObjective("collect", "Collect 10 Jackal Fomor Scrolls", 0, 0, 0, Collect(71052, 10));

		AddReward(Gold(2100));
	}
}
