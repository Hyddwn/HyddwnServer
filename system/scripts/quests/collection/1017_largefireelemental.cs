//--- Aura Script -----------------------------------------------------------
//Produce a Massive Fire Elemental
//--- Description -----------------------------------------------------------
// Collection quest to create a large fire elemental
//---------------------------------------------------------------------------

public class LargeFireElementalQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(1017);
		SetScrollId(70023);
		SetName(L("Produce a Massive Fire Elemental"));
		SetDescription(L("Gather 10 Fire Elementals from the Dungeon to transform into a Massive Elemental"));
		SetType(QuestType.Collect);
		SetCancelable(true);

		SetIcon(QuestIcon.Collect);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("collect1", L("10 Fire Elementals"), 0, 0, 0, Collect(62006, 10));

		AddReward(Exp(120));
		AddReward(Item(62009)); // Massive Fire Elemental
	}
}
