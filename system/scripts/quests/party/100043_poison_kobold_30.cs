//--- Aura Script -----------------------------------------------------------
// [PQ] Hunt Down the Poison Kobolds (30)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class PoisonKobolds30PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100043);
		SetScrollId(70025);
		SetName(L("[PQ] Hunt Down the Poison Kobolds"));
		SetDescription(L("The Poison Kobold, unlike regular Kobolds, threatens its enemies with its poisonous attacks. Please [Hunt 30 Poison Kobolds]."));
		SetType(QuestType.Collect);
		SetCancelable(true);

		SetIcon(QuestIcon.Party);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("obj", L("Hunt 30 Poison Kobolds"), 0, 0, 0, Kill(30, "/poisonkobold/"));

		AddReward(Exp(786));
		AddReward(Gold(1767));
	}
}
