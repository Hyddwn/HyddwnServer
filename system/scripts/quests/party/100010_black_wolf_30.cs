//--- Aura Script -----------------------------------------------------------
// [PQ] Hunt Black Wolves (30)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class BlackWolf30PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100010);
		SetScrollId(70025);
		SetName(L("[PQ] Hunt Black Wolves"));
		SetDescription(L("The wolves that started to appear recently, are natural enemies of the sheep. To protect the sheep, please hunt [30 black wolves] roaming the plains."));
		SetType(QuestType.Collect);
		SetCancelable(true);

		SetIcon(QuestIcon.Party);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("obj", L("Hunt 30 Black Wolves"), 0, 0, 0, Kill(30, "/blackwolf/"));

		AddReward(Exp(490));
		AddReward(Gold(930));
	}
}
