//--- Aura Script -----------------------------------------------------------
// Collect Large Nails
//--- Description -----------------------------------------------------------
// Collection quest for some nails.
//---------------------------------------------------------------------------

public class Collect10LargeNailsQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(1012);
		SetScrollId(70023);
		SetName(L("Collect Large Nails"));
		SetDescription(L("If you shake objects in town, sometimes old nails fall out. Please [collect 10 Large Nails]. The collected nails will be used for repairs around town."));
		SetType(QuestType.Collect);

		SetIcon(QuestIcon.Collect);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("collect1", L("Collect 10 Large Nails"), 0, 0, 0, Collect(52003, 10));

		AddReward(Exp(5));
		AddReward(Gold(20));
	}
}
