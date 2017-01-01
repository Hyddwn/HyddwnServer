//--- Aura Script -----------------------------------------------------------
// [PQ] Hunt White Wolves (30)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class WhiteWolf30PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100012);
		SetScrollId(70025);
		SetName(L("[PQ] Hunt White Wolves"));
		SetDescription(L("The wolves that started to appear recently, are natural enemies of the sheep. To protect the sheep, please hunt [30 white wolves] roaming the plains."));
		SetType(QuestType.Collect);
		SetCancelable(true);

		SetIcon(QuestIcon.Party);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("obj", L("Hunt 30 White Wolves"), 0, 0, 0, Kill(30, "/whitewolf/"));

		AddReward(Exp(207));
		AddReward(Gold(381));
	}
}
