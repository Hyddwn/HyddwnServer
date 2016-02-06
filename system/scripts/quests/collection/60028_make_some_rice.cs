//--- Aura Script -----------------------------------------------------------
// Make Some Rice
//--- Description -----------------------------------------------------------
// Collection quest, trading ingredients for food.
//---------------------------------------------------------------------------

public class MakeSomeRiceQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(60028);
		SetScrollId(70070);
		SetName("Make Some Rice");
		SetDescription("Do you need to make rice? If so, then please gather up [rice, water]. The ingredients will be exchanged for some [Rice].");
		SetType(QuestType.Collect);

		AddObjective("collect1", "Gather 1 bag of rice", 0, 0, 0, Collect(50135, 1));
		AddObjective("collect2", "Gather 1 Bottle of water", 0, 0, 0, Collect(50118, 1));

		AddReward(Item(50120)); // Steamed Rice
		AddReward(Item(63020), RewardOptions.Hidden); // Empty Bottle
	}
}
