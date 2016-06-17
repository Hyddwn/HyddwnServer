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
		SetName(L("Make a Steamed Potato"));
		SetDescription(L("Do you need a Steamed Potato? If so, then please gather up [Potatoes, water, salt]. The ingredients will be exchanged for some  [Steamed Potato]."));
		SetType(QuestType.Collect);

		SetIcon(QuestIcon.Collect);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("collect1", L("Gather 1 Potato"), 0, 0, 0, Collect(50010, 1));
		AddObjective("collect2", L("Gather 1 cup of water"), 0, 0, 0, Collect(50118, 1));
		AddObjective("collect3", L("Gather 1 Salt"), 0, 0, 0, Collect(50132, 1));

		AddReward(Item(50125)); // Steamed Potato
		AddReward(Item(63020), RewardOptions.Hidden); // Empty Bottle
	}
}
