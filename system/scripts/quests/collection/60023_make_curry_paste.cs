//--- Aura Script -----------------------------------------------------------
// Make Curry Paste
//--- Description -----------------------------------------------------------
// Collection quest, trading ingredients for food.
//---------------------------------------------------------------------------

public class MakeCurryPasteQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(60023);
		SetScrollId(70070);
		SetName("Make Curry Paste");
		SetDescription("Do you need Curry Paste? If so, then please gather up [Potatoes, Meat, Curry Powder]. The ingredients will be exchanged for some [Curry Paste].");
		SetType(QuestType.Collect);

		AddObjective("collect1", "Gather 1 Potato", 0, 0, 0, Collect(50010, 1));
		AddObjective("collect2", "Gather 1 Pieces of Meat", 0, 0, 0, Collect(50006, 1));
		AddObjective("collect3", "Gather 1 Curry Powder", 0, 0, 0, Collect(50185, 1));

		AddReward(Item(50192)); // Curry Paste
	}
}
