//--- Aura Script -----------------------------------------------------------
// [PQ] Hunt Down the Kobolds (30)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class Kobolds30PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100042);
		SetScrollId(70025);
		SetName(L("[PQ] Hunt Down the Kobolds"));
		SetDescription(L("A Kobold's dog-like legs make them physically stronger than Goblins but their intellect is far below that of humans. Please [Hunt 30 Kobolds]."));
		SetType(QuestType.Collect);
		SetCancelable(true);

		SetIcon(QuestIcon.Party);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("obj", L("Hunt 30 Kobolds"), 0, 0, 0, Kill(30, "/kobold/"));

		AddReward(Exp(726));
		AddReward(Gold(1635));
	}
}
