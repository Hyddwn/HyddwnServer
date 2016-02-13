//--- Aura Script -----------------------------------------------------------
// [PQ] Hunt Down the Werewolves (10)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class Werewolf10PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100062);
		SetScrollId(70025);
		SetName("[PQ] Hunt Down the Werewolves");
		SetDescription("Werewolves are under an evil spell and are attacking travelers. Please [Hunt 10 Werewolves].");
		SetType(QuestType.Collect);

		AddObjective("obj", "Hunt 10 Werewolves", 0, 0, 0, Kill(10, "/werewolf/"));

		AddReward(Exp(2250));
		AddReward(Gold(1539));
	}
}
