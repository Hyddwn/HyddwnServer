//--- Aura Script -----------------------------------------------------------
// Collect the Fire Sprite's Fomor Scrolls
//--- Description -----------------------------------------------------------
// Scroll collection quest, purchasable from shops.
//---------------------------------------------------------------------------

public class FireSpriteBoarScrollQuest : QuestScript
{
	public override void Load()
	{
		SetId(71065);
		SetScrollId(70138);
		SetName("Collect the Fire Sprite's Fomor Scrolls");
		SetDescription("The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Fire Sprite Fomor Scrolls].");
		SetType(QuestType.Collect);

		AddObjective("collect", "Collect 10 Fire Sprite Fomor Scrolls", 0, 0, 0, Collect(71065, 10));

		AddReward(Gold(3500));
	}
}
