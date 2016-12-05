//--- Aura Script -----------------------------------------------------------
// [PQ] Hunt Down the Black Dire Wolves (30)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class BlackDireWolf30PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100037);
		SetScrollId(70025);
		SetName(L("[PQ] Hunt Down the Black Dire Wolves"));
		SetDescription(L("Dire wolves are totally under the control of evil spirits, even more than ordinary wolves. Please do us a favor and [hunt 30 black dire wolves]."));
		SetType(QuestType.Collect);
		SetCancelable(true);

		SetIcon(QuestIcon.Party);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("obj1", L("Hunt 10 Black Dire Wolves"), 0, 0, 0, Kill(10, "/blackdirewolf/"));
		AddObjective("obj2", L("Hunt 20 Black Dire Wolf Cubs"), 0, 0, 0, Kill(20, "/black/direwolfkid/"));

		AddReward(Exp(888));
		AddReward(Gold(1566));
	}
}
