//--- Aura Script -----------------------------------------------------------
// Cook a Hard-Boiled Egg
//--- Description -----------------------------------------------------------
// Collection quest, trading ingredients for food.
//---------------------------------------------------------------------------

public class CookHardBoiledEggQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(60025);
		SetScrollId(70070);
		SetName("Cook a Hard-Boiled Egg");
		SetDescription("Do you need a Hard-Boiled Egg? If so, then please gather up [Egg, water, salt]. The ingredients will be exchanged for some [Hard-Boiled Egg].");
		SetType(QuestType.Collect);

		AddObjective("collect1", "Gather 1 Egg", 0, 0, 0, Collect(50009, 1));
		AddObjective("collect2", "Gather 1 Bottle of water", 0, 0, 0, Collect(50118, 1));
		AddObjective("collect3", "Gather 1 Salt", 0, 0, 0, Collect(50132, 1));

		AddReward(Item(50126)); // Hard-Boiled Egg
		AddReward(Item(63020), RewardOptions.Hidden); // Empty Bottle
	}
}
