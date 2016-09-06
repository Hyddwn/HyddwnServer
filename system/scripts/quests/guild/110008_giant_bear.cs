//--- Aura Script -----------------------------------------------------------
// Eliminate Giant Bear
//--- Description -----------------------------------------------------------
// Guild quest to kill a Giant Bear.
//---------------------------------------------------------------------------

public class GiantBearGuildQuest : QuestScript
{
	public override void Load()
	{
		SetId(110008);
		SetScrollId(70152);
		SetName(L("Eliminate Giant Bear"));
		SetDescription(L("Defeat [Giant Bear] that sometimes appears in Dugald Aisle."));
		SetType(QuestType.Collect);

		SetIcon(QuestIcon.Party);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("obj", L("Kill 1 Giant Bear"), 0, 0, 0, Kill(1, "/giantbear/"));

		AddReward(Exp(19600));
		AddReward(Gold(6700));
	}
}
