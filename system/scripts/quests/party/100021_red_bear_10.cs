//--- Aura Script -----------------------------------------------------------
// [PQ] Hunt Down the Red Bears (10)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class RedBear10PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100021);
		SetScrollId(70025);
		SetName(L("[PQ] Hunt Down the Red Bears"));
		SetDescription(L("It is uncommon to see red bear roaming around the plains. Supposedly, the Red Bears have amazing power. Will you please hunt [10 red bears]?"));
		SetType(QuestType.Collect);
		SetCancelable(true);

		SetIcon(QuestIcon.Party);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("obj", L("Hunt 10 Red Bears"), 0, 0, 0, Kill(10, "/redbear/"));

		AddReward(Exp(300));
		AddReward(Gold(630));
		AddReward(QuestScroll(100022)); // [PQ] Hunt Down the Red Bears (30)
	}
}
