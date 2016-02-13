//--- Aura Script -----------------------------------------------------------
// Make Assorted Fruits
//--- Description -----------------------------------------------------------
// Collection quest, trading ingredients for food.
//---------------------------------------------------------------------------

public class MakeAssortedFruitsQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(60021);
		SetScrollId(70070);
		SetName("Make Assorted Fruits");
		SetDescription("Do you need an Assorted Fruits Dish? If so, then please gather up some [Strawberries, Apples, and Berries]. The ingredients will be exchanged for a plate of [Assorted Fruits].");
		SetType(QuestType.Collect);

		AddObjective("collect1", "Gather 1 Strawberry", 0, 0, 0, Collect(50112, 1));
		AddObjective("collect2", "Gather 1 Apple", 0, 0, 0, Collect(50003, 1));
		AddObjective("collect3", "Gather 1 Berry", 0, 0, 0, Collect(50007, 1));

		AddReward(Item(50106)); // Assorted Fruits
	}
}
