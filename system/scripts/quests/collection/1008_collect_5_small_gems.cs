//--- Aura Script -----------------------------------------------------------
// Collect Small Gems
//--- Description -----------------------------------------------------------
// Collection quest for Small Gems.
//---------------------------------------------------------------------------

public class Collect5SmallGems2QuestScript : QuestScript
{
	public override void Load()
	{
		SetId(1008);
		SetScrollId(70023);
		SetName(L("Collect Small Gems"));
		SetDescription(L("Please [collect 5 Small Gems]. The Imps have hidden them all over town. This gem is not very valuable, but rats just love them. They gnaw at everything to get to the gems, and it's become quite a serious problem. You'll find the the gems if you [check suspicious items] around town."));
		SetType(QuestType.Collect);

		SetIcon(QuestIcon.Collect);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("collect1", L("Collect 5 Small Gems"), 0, 0, 0, Collect(52002, 5));

		AddReward(Exp(5));
		AddReward(Gold(10));
	}
}
