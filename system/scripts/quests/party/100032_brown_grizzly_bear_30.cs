//--- Aura Script -----------------------------------------------------------
// [PQ] Hunt Down the Brown Grizzly Bears (30)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class BrownGrizzlyBear30PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100032);
		SetScrollId(70025);
		SetName(L("[PQ] Hunt Down the Brown Grizzly Bears"));
		SetDescription(L("The Grizzly Bears roaming in the plains are under a mighty evil spell which can be seen in their eyes. Please [Hunt 30 Brown Grizzly Bears]."));
		SetType(QuestType.Collect);
		SetCancelable(true);

		SetIcon(QuestIcon.Party);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("obj1", L("Hunt 10 Brown Grizzly Bears"), 0, 0, 0, Kill(10, "/browngrizzlybear/"));
		AddObjective("obj2", L("Hunt 20 Brown Grizzly Bear Cubs"), 0, 0, 0, Kill(20, "/brown/grizzlybearkid/"));

		AddReward(Exp(849));
		AddReward(Gold(1668));
	}
}
