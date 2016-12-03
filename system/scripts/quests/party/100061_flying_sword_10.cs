//--- Aura Script -----------------------------------------------------------
// [PQ] Hunt Down the Flying Swords (10)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class FlyingSword10PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100061);
		SetScrollId(70025);
		SetName(L("[PQ] Hunt Down the Flying Swords"));
		SetDescription(L("Flying Swords are under an evil spell and are attacking travelers. Please [Hunt 10 Flying Swords]."));
		SetType(QuestType.Collect);
		SetCancelable(true);

		SetIcon(QuestIcon.Party);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("obj", L("Hunt 10 Flying Swords"), 0, 0, 0, Kill(10, "/flyingsword/"));

		AddReward(Exp(1500));
		AddReward(Gold(2298));
	}
}
