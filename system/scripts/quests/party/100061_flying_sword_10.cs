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
		SetName("[PQ] Hunt Down the Flying Swords");
		SetDescription("Flying Swords are under an evil spell and are attacking travelers. Please [Hunt 10 Flying Swords].");
		SetType(QuestType.Collect);

		AddObjective("obj", "Hunt 10 Flying Swords", 0, 0, 0, Kill(10, "/flyingsword/"));

		AddReward(Exp(1500));
		AddReward(Gold(2298));
	}
}
