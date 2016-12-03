//--- Aura Script -----------------------------------------------------------
// [PQ] Hunt Down the Skeletons (30)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class Skeletons30PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100026);
		SetScrollId(70025);
		SetName(L("[PQ] Hunt Down the Skeletons"));
		SetDescription(L("Have you ever seen a Skeleton? The Skeletons living in Rabbie Dungeon do not have their own will or consciousness. They do not belong in this world. Please hunt [30 Skeletons]."));
		SetType(QuestType.Collect);
		SetCancelable(true);

		SetIcon(QuestIcon.Party);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("obj", L("Hunt 30 Skeletons"), 0, 0, 0, Kill(30, "/normalskeleton/"));

		AddReward(Exp(882));
		AddReward(Gold(2352));
	}
}
