//--- Aura Script -----------------------------------------------------------
// Make Mayonnaise
//--- Description -----------------------------------------------------------
// Collection quest, trading ingredients for food.
//---------------------------------------------------------------------------

public class MakeMayonnaiseQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(60022);
		SetScrollId(70070);
		SetName("Make Mayonnaise");
		SetDescription("Do you need Mayo? If so, then please gather up [Eggs and Olive Oil]. The ingredients will be exchanged for some [Mayonnaise].");
		SetType(QuestType.Collect);

		AddObjective("collect1", "Gather 1 Egg", 0, 0, 0, Collect(50009, 1));
		AddObjective("collect2", "Gather 1 Olive Oil", 0, 0, 0, Collect(50145, 1));

		AddReward(Item(50116)); // Mayonnaise
	}
}
