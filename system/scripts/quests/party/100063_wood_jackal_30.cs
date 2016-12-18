//--- Aura Script -----------------------------------------------------------
// [PQ] Hunt Down the Wood Jackals (30)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class WoodJackal30PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100063);
		SetScrollId(70025);
		SetName(L("[PQ] Hunt Down the Wood Jackals"));
		SetDescription(L("Wood Jackals are under an evil spell and are attacking travelers. Please [hunt 30 Wood Jackals]."));
		SetType(QuestType.Collect);
		SetCancelable(true);

		SetIcon(QuestIcon.Party);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("obj", L("Hunt 30 Wood Jackals"), 0, 0, 0, Kill(30, "/woodjackal/"));

		AddReward(Exp(1350));
		AddReward(Gold(2250));
	}
}
