//--- Aura Script -----------------------------------------------------------
//Produce a Massive Lightning Elemental
//--- Description -----------------------------------------------------------
// Collection quest to create a large lightning elemental
//---------------------------------------------------------------------------

public class LargeLightningElementalQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(1018);
		SetScrollId(70023);
		SetName(L("Produce a Massive Lightning Elemental"));
		SetDescription(L("Gather 10 Lightning Elementals from the Dungeon to transform into a Massive Elemental"));
		SetType(QuestType.Collect);
		SetCancelable(true);

		SetIcon(QuestIcon.Collect);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("collect1", L("10 Lightning Elementals"), 0, 0, 0, Collect(62008, 10));

		AddReward(Exp(120));
		AddReward(Item(62011)); // Massive Lightning Elemental
	}
}
