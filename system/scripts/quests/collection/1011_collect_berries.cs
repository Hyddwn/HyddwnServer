//--- Aura Script -----------------------------------------------------------
// Collect Branches
//--- Description -----------------------------------------------------------
// Collection quest for some berries.
//---------------------------------------------------------------------------

public class Collect10BerriesQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(1011);
		SetScrollId(70023);
		SetName(L("Collect Berries"));
		SetDescription(L("Berries grow on the trees that can be found all over town. Please [collect 10 Berries]. The berries are edible, but no one can exactly describe how they taste."));
		SetType(QuestType.Collect);

		SetIcon(QuestIcon.Collect);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("collect1", L("Collect 10 Berries"), 0, 0, 0, Collect(50007, 10));

		AddReward(Exp(7));
		AddReward(Gold(60));
	}
}
