//--- Aura Script -----------------------------------------------------------
// [PQ] Hunt Gray Wolves (10)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class GrayWolf10PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100007);
		SetScrollId(70025);
		SetName("[PQ] Hunt Gray Wolves");
		SetDescription("The wolves that started to appear recently, are natural enemies of the sheep. To protect the sheep, please hunt [10 gray wolves] roaming the plains.");
		SetType(QuestType.Collect);

		AddObjective("obj", "Hunt 10 Gray Wolves", 0, 0, 0, Kill(10, "/graywolf/"));

		AddReward(Exp(126));
		AddReward(Gold(234));
		AddReward(QuestScroll(100008)); // [PQ] Hunt Gray Wolves (30)
	}
}
