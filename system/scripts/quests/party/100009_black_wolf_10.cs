//--- Aura Script -----------------------------------------------------------
// [PQ] Hunt Black Wolves (10)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class BlackWolf10PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100009);
		SetScrollId(70025);
		SetName("[PQ] Hunt Black Wolves");
		SetDescription("The wolves that started to appear recently, are natural enemies of the sheep. To protect the sheep, please hunt [10 black wolves] roaming the plains.");
		SetType(QuestType.Collect);

		AddObjective("obj", "Hunt 10 Black Wolves", 0, 0, 0, Kill(10, "/blackwolf/"));

		AddReward(Exp(171));
		AddReward(Gold(327));
		AddReward(QuestScroll(100010)); // [PQ] Hunt Black Wolves (30)
	}
}
