//--- Aura Script -----------------------------------------------------------
// Make Some Flour Dough
//--- Description -----------------------------------------------------------
// Collection quest, trading ingredients for food.
//---------------------------------------------------------------------------

public class MakeSomeFlourDoughQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(60026);
		SetScrollId(70070);
		SetName(L("Make Some Flour Dough"));
		SetDescription(L("Do you need a Flour Dough? If so, then please gather up [wheat flour, yeast, and water]. The ingredients will be exchanged for some  [Flour Dough]."));
		SetType(QuestType.Collect);

		SetIcon(QuestIcon.Collect);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("collect1", L("Gather 1 bag of wheat flour"), 0, 0, 0, Collect(50022, 1));
		AddObjective("collect2", L("1 Yeast"), 0, 0, 0, Collect(50148, 1));
		AddObjective("collect3", L("1 Water"), 0, 0, 0, Collect(50118, 1));

		AddReward(Item(50154)); // Fry Batter
		AddReward(Item(63020), RewardOptions.Hidden); // Empty Bottle
	}
}
