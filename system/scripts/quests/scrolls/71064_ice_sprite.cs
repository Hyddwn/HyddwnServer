//--- Aura Script -----------------------------------------------------------
// Collect the Ice Sprite's Fomor Scrolls
//--- Description -----------------------------------------------------------
// Scroll collection quest, purchasable from shops.
//---------------------------------------------------------------------------

public class IceSpriteBoarScrollQuest : QuestScript
{
	public override void Load()
	{
		SetId(71064);
		SetScrollId(70137);
		SetName("Collect the Ice Sprite's Fomor Scrolls");
		SetDescription("The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Ice Sprite Fomor Scrolls].");
		SetType(QuestType.Collect);

		AddObjective("collect", "Collect 10 Ice Sprite Fomor Scrolls", 0, 0, 0, Collect(71064, 10));

		AddReward(Gold(3500));
	}
}
