//--- Aura Script -----------------------------------------------------------
// [PQ] Hunt Down the Burgundy Dire Wolves (30)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class BurgundyDireWolf30PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100065);
		SetScrollId(70025);
		SetName(L("[PQ] Hunt Down the Burgundy Dire Wolves"));
		SetDescription(L("Burgundy dire wolves under the control of an evil power are attacking travelers. Please [hunt 30 burgundy dire wolves]."));
		SetType(QuestType.Collect);
		SetCancelable(true);

		SetIcon(QuestIcon.Party);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("obj", L("Hunt 30 Burgundy Dire Wolves"), 0, 0, 0, Kill(30, "/darkreddirewolf/"));

		AddReward(Exp(1350));
		AddReward(Gold(2676));
	}
}
