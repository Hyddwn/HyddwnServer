//--- Aura Script -----------------------------------------------------------
// Eliminate Goblin Bandits
//--- Description -----------------------------------------------------------
// Guild quest to kill a Goblin Bandits.
//---------------------------------------------------------------------------

public class GoblinBanditsGuildQuest : QuestScript
{
	public override void Load()
	{
		SetId(110006);
		SetScrollId(70152);
		SetName(L("Eliminate Goblin Bandits"));
		SetDescription(L("Defeat [Goblin Bandits] that sometimes appear near Dunbarton."));
		SetType(QuestType.Collect);

		SetIcon(QuestIcon.Party);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("obj", L("Kill 6 Goblin Bandits"), 0, 0, 0, Kill(6, "/goblinbandit/"));

		AddReward(Exp(7100));
		AddReward(Gold(15700));
	}
}
