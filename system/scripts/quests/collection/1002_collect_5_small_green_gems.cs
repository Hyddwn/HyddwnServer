//--- Aura Script -----------------------------------------------------------
// Collect 5 Small Green Gems
//--- Description -----------------------------------------------------------
// Collection quest for Small Gems.
//---------------------------------------------------------------------------

public class Collect5SmallGreenGems1QuestScript : QuestScript
{
	public override void Load()
	{
		SetId(1002);
		SetScrollId(70023);
		SetName(L("Collect Small Green Gems"));
		SetDescription(L("Please [collect 5 Small Green Gems]. The Imps have hidden them all over town. You'll find these gems if you [check suspicious items] around town, but you can also trade other gems to get Green Gems."));
		SetType(QuestType.Collect);

		SetIcon(QuestIcon.Collect);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("collect1", L("Collect 5 Green Gems"), 0, 0, 0, Collect(52004, 5));

		AddReward(Exp(10));
		AddReward(Item(52005)); // Small Blue Gem
	}
}
