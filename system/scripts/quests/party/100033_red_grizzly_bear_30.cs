//--- Aura Script -----------------------------------------------------------
// [PQ] Hunt Down the Red Grizzly Bears (30)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class RedGrizzlyBear30PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100033);
		SetScrollId(70025);
		SetName(L("[PQ] Hunt Down the Red Grizzly Bears"));
		SetDescription(L("The Grizzly Bears roaming in the plains are under a mighty evil spell which can be seen in their eyes. Please [Hunt 30 Red Grizzly Bears]."));
		SetType(QuestType.Collect);
		SetCancelable(true);

		SetIcon(QuestIcon.Party);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("obj1", L("Hunt 10 Red Grizzly Bears"), 0, 0, 0, Kill(10, "/redgrizzlybear/"));
		AddObjective("obj2", L("Hunt 20 Red Grizzly Bear Cubs"), 0, 0, 0, Kill(20, "/red/grizzlybearkid/"));

		AddReward(Exp(966));
		AddReward(Gold(1911));
	}
}
