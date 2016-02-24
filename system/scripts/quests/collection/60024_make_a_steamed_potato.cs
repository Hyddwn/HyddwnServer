//--- Aura Script -----------------------------------------------------------
// Make a Steamed Potato
//--- Description -----------------------------------------------------------
// Collection quest, trading ingredients for food.
//---------------------------------------------------------------------------

public class MakeSteamedPotatoQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(60024);
		SetScrollId(70070);
		SetName("Make a Steamed Potato");
		SetDescription("Do you need a Steamed Potato? If so, then please gather up [Potatoes, water, salt]. The ingredients will be exchanged for some  [Steamed Potato].");
		SetType(QuestType.Collect);

		AddObjective("collect1", "Gather 1 Potato", 0, 0, 0, Collect(50010, 1));
		AddObjective("collect2", "Gather 1 cup of water", 0, 0, 0, Collect(50118, 1));
		AddObjective("collect3", "Gather 1 Salt", 0, 0, 0, Collect(50132, 1));

		AddReward(Item(50125)); // Steamed Potato
		AddReward(Item(63020), RewardOptions.Hidden); // Empty Bottle
	}
}
