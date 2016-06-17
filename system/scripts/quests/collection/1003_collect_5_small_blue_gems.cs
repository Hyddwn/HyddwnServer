//--- Aura Script -----------------------------------------------------------
// Collect 5 Small Blue Gems
//--- Description -----------------------------------------------------------
// Collection quest for Small Gems.
//---------------------------------------------------------------------------

public class Collect5SmallBlueGems1QuestScript : QuestScript
{
	public override void Load()
	{
		SetId(1003);
		SetScrollId(70023);
		SetName(L("Collect Small Blue Gems"));
		SetDescription(L("Please [collect 5 Small Blue Gems]. The Imps have hidden them all over town. You'll find these gems if you [check suspicious items] around town, but you can also trade other gems to get Blue Gems."));
		SetType(QuestType.Collect);

		SetIcon(QuestIcon.Collect);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("collect1", L("Collect 5 Blue Gems"), 0, 0, 0, Collect(52005, 5));

		AddReward(Exp(20));
		AddReward(Item(52006)); // Small Red Gem
	}
}
