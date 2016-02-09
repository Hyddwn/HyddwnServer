//--- Aura Script -----------------------------------------------------------
// [PQ] Hunt Down the Coyotes
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class Coyote100PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100055);
		SetScrollId(70025);
		SetName("[PQ] Hunt Down the Coyotes");
		SetDescription("Coyotes have fallen under an evil spell and are attacking travelers. Please [Hunt 100 Coyotes].");
		SetType(QuestType.Collect);

		AddObjective("obj", "Hunt 100 Coyotes", 0, 0, 0, Kill(100, "/coyote/"));

		AddReward(Exp(2100));
		AddReward(Gold(3810));
	}
}
