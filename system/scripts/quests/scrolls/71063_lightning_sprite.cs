//--- Aura Script -----------------------------------------------------------
// Collect the Lightning Sprite's Fomor Scrolls
//--- Description -----------------------------------------------------------
// Scroll collection quest, purchasable from shops.
//---------------------------------------------------------------------------

public class LightningSpriteBoarScrollQuest : QuestScript
{
	public override void Load()
	{
		SetId(71063);
		SetScrollId(70136);
		SetName("Collect the Lightning Sprite's Fomor Scrolls");
		SetDescription("The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Lightning Sprite Fomor Scrolls].");
		SetType(QuestType.Collect);

		AddObjective("collect", "Collect 10 Lightning Sprite Fomor Scrolls", 0, 0, 0, Collect(71063, 10));

		AddReward(Gold(3500));
	}
}
