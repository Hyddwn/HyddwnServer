//--- Aura Script -----------------------------------------------------------
// [PQ] Hunt Down the Brown Gremlins (10)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class BrownGremlin10PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100060);
		SetScrollId(70025);
		SetName("[PQ] Hunt Down the Brown Gremlins");
		SetDescription("Brown Gremlins are under an evil spell and are attacking travelers. Please [Hunt 10 Brown Gremlins].");
		SetType(QuestType.Collect);

		AddObjective("obj", "Hunt 10 Brown Gremlins", 0, 0, 0, Kill(10, "/browngremlin/"));

		AddReward(Exp(900));
		AddReward(Gold(1098));
	}
}
