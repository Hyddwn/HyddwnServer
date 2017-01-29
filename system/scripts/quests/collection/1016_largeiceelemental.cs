//--- Aura Script -----------------------------------------------------------
//Produce a Massive Ice Elemental
//--- Description -----------------------------------------------------------
// Collection quest to create a large ice elemental
//---------------------------------------------------------------------------

public class LargeIceElementalQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(1016);
		SetScrollId(70023);
		SetName(L("Produce a Massive Ice Elemental"));
		SetDescription(L("Gather 10 Ice Elementals from the Dungeon to transform into a Massive Elemental"));
		SetType(QuestType.Collect);
		SetCancelable(true);

		SetIcon(QuestIcon.Collect);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("collect1", L("10 Ice Elementals"), 0, 0, 0, Collect(62007, 10));

		AddReward(Exp(120));
		AddReward(Item(62010)); // Massive Ice Elemental
	}
}
