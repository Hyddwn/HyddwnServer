//--- Aura Script -----------------------------------------------------------
// Collect the Wood Jackal's Fomor Scrolls
//--- Description -----------------------------------------------------------
// Scroll collection quest, purchasable from shops.
//---------------------------------------------------------------------------

public class WoodJackalScrollQuest : QuestScript
{
	public override void Load()
	{
		SetId(71061);
		SetScrollId(70134);
		SetName("Collect the Wood Jackal's Fomor Scrolls");
		SetDescription("The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Wood Jackal Fomor Scrolls].");
		SetType(QuestType.Collect);

		AddObjective("collect", "Collect 10 Wood Jackal Fomor Scrolls", 0, 0, 0, Collect(71061, 10));

		AddReward(Gold(1500));
	}
}
