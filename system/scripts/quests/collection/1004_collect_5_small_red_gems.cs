//--- Aura Script -----------------------------------------------------------
// Collect 5 Small Red Gems
//--- Description -----------------------------------------------------------
// Collection quest for Small Gems.
//---------------------------------------------------------------------------

public class Collect5SmallRedGems1QuestScript : QuestScript
{
	public override void Load()
	{
		SetId(1004);
		SetScrollId(70023);
		SetName(L("Collect Small Red Gems"));
		SetDescription(L("Please [collect 5 Small Red Gems]. The Imps have hidden them all over town. You'll find these gems if you [check suspicious items] around town, but you can also trade other gems to get Red Gems."));
		SetType(QuestType.Collect);

		SetIcon(QuestIcon.Collect);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("collect1", L("Collect 5 Red Gems"), 0, 0, 0, Collect(52006, 5));

		AddReward(Exp(50));
		AddReward(Item(52007)); // Small Silver Gem
	}
}
