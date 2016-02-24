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
		SetName("[PQ] Hunt White Wolves");
		SetDescription("The wolves that started to appear recently, are natural enemies of the sheep. To protect the sheep, please hunt [30 white wolves] roaming the plains.");
		SetType(QuestType.Collect);

		AddObjective("obj", "Hunt 30 White Wolves", 0, 0, 0, Kill(30, "/whitewolf/"));

		AddReward(Exp(207));
		AddReward(Gold(381));
	}
}
