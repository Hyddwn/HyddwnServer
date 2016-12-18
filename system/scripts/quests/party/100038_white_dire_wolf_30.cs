//--- Aura Script -----------------------------------------------------------
// [PQ] Hunt Down the White Dire Wolves (30)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class WhiteDireWolf30PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100038);
		SetScrollId(70025);
		SetName(L("[PQ] Hunt Down the White Dire Wolves"));
		SetDescription(L("Dire wolves are totally under the control of evil spirits, even more than ordinary wolves. Please do us a favor and [hunt 30 white dire wolves]."));
		SetType(QuestType.Collect);
		SetCancelable(true);

		SetIcon(QuestIcon.Party);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("obj1", L("Hunt 10 White Dire Wolves"), 0, 0, 0, Kill(10, "/whitedirewolf/"));
		AddObjective("obj2", L("Hunt 20 White Dire Wolf Cubs"), 0, 0, 0, Kill(20, "/white/direwolfkid/"));

		AddReward(Exp(1005));
		AddReward(Gold(1815));
	}
}
