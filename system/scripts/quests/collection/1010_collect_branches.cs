//--- Aura Script -----------------------------------------------------------
// Collect Branches
//--- Description -----------------------------------------------------------
// Collection quest for some branches.
//---------------------------------------------------------------------------

public class Collect10BranchesQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(1010);
		SetScrollId(70023);
		SetName(L("Collect Branches"));
		SetDescription(L("Branches are needed for an owl's nest. Please [collect 10 Branches]. If you see a tree around, [shaking the tree] can make its Branches fall."));
		SetType(QuestType.Collect);

		SetIcon(QuestIcon.Collect);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("collect1", L("Collect 10 Branches"), 0, 0, 0, Collect(52001, 10));

		AddReward(Exp(5));
		AddReward(Gold(15));
	}
}
