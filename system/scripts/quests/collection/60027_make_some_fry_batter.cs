//--- Aura Script -----------------------------------------------------------
// Make Some Fry Batter
//--- Description -----------------------------------------------------------
// Collection quest, trading ingredients for food.
//---------------------------------------------------------------------------

public class MakeSomeFryBatterQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(60027);
		SetScrollId(70070);
		SetName("Make Some Fry Batter");
		SetDescription("Do you need some Fry Batter? If so, then please gather up [frying powder, egg, and water]. The ingredients will be exchanged for some [Fry Batter].");
		SetType(QuestType.Collect);

		AddObjective("collect1", "Gather 1 bag of Frying Powder", 0, 0, 0, Collect(50153, 1));
		AddObjective("collect2", "Gather 1 Egg", 0, 0, 0, Collect(50009, 1));
		AddObjective("collect3", "Gather 1 Bottle of water", 0, 0, 0, Collect(50118, 1));

		AddReward(Item(50154)); // Fry Batter
		AddReward(Item(63020), RewardOptions.Hidden); // Empty Bottle
	}
}
