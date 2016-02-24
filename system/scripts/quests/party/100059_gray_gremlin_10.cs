//--- Aura Script -----------------------------------------------------------
// [PQ] Hunt Down the Gray Gremlins (10)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class GrayGremlin10PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100059);
		SetScrollId(70025);
		SetName("[PQ] Hunt Down the Gray Gremlins");
		SetDescription("Gray Gremlins are under an evil spell and are attacking travelers. Please [Hunt 10 Gray Gremlins].");
		SetType(QuestType.Collect);

		AddObjective("obj", "Hunt 10 Gray Gremlins", 0, 0, 0, Kill(10, "/graygremlin/"));

		AddReward(Exp(1200));
		AddReward(Gold(840));
	}
}
