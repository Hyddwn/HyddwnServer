//--- Aura Script -----------------------------------------------------------
// Collect 5 Small Silver Gems
//--- Description -----------------------------------------------------------
// Collection quest for Small Gems.
//---------------------------------------------------------------------------

public class Collect5SmallSilverGemsQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(1009);
		SetScrollId(70023);
		SetName(L("Collect Small Silver Gems"));
		SetDescription(L("Please [collect 5 Small Silver Gems]. Even though they're not real silver, Imps value these gems but they are so rare. Rumor has it that there's a way to trade different colored gems for Silver Gems."));
		SetType(QuestType.Collect);

		SetIcon(QuestIcon.Collect);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("collect1", L("Collect 5 Silver Gems"), 0, 0, 0, Collect(52007, 5));

		AddReward(Exp(100));
		AddReward(Gold(1200));
	}
}
