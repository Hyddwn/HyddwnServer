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
		SetName(L("[PQ] Hunt Down the Werewolves"));
		SetDescription(L("Werewolves are under an evil spell and are attacking travelers. Please [Hunt 10 Werewolves]."));
		SetType(QuestType.Collect);
		SetCancelable(true);

		SetIcon(QuestIcon.Party);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("obj", L("Hunt 10 Werewolves"), 0, 0, 0, Kill(10, "/werewolf/"));

		AddReward(Exp(2250));
		AddReward(Gold(1539));
	}
}
