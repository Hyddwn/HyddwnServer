//--- Aura Script -----------------------------------------------------------
// Make Mayonnaise
//--- Description -----------------------------------------------------------
// Collection quest, trading ingredients for food.
//---------------------------------------------------------------------------

public class MakeMayonnaiseQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(60022);
		SetScrollId(70070);
		SetName(L("Make Mayonnaise"));
		SetDescription(L("Do you need Mayo? If so, then please gather up [Eggs and Olive Oil]. The ingredients will be exchanged for some [Mayonnaise]."));
		SetType(QuestType.Collect);

		SetIcon(QuestIcon.Collect);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("collect1", L("Gather 1 Egg"), 0, 0, 0, Collect(50009, 1));
		AddObjective("collect2", L("Gather 1 Olive Oil"), 0, 0, 0, Collect(50145, 1));

		AddReward(Item(50116)); // Mayonnaise
	}
}
