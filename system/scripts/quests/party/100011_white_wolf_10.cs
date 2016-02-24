//--- Aura Script -----------------------------------------------------------
// [PQ] Hunt White Wolves (10)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class WhiteWolf10PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100011);
		SetScrollId(70025);
		SetName("[PQ] Hunt White Wolves");
		SetDescription("The wolves that started to appear recently, are natural enemies of the sheep. To protect the sheep, please hunt [10 white wolves] roaming the plains.");
		SetType(QuestType.Collect);

		AddObjective("obj", "Hunt 10 White Wolves", 0, 0, 0, Kill(10, "/whitewolf/"));

		AddReward(Exp(207));
		AddReward(Gold(381));
		AddReward(QuestScroll(100012)); // [PQ] Hunt White Wolves (30)
	}
}
