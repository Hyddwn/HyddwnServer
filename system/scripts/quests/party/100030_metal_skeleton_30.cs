//--- Aura Script -----------------------------------------------------------
// [PQ] Hunt Down the Metal Skeletons (30)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class MetalSkeletons30PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100030);
		SetScrollId(70025);
		SetName(L("[PQ] Hunt Down the Metal Skeletons"));
		SetDescription(L("Have you ever seen a Metal Skeleton? The Metal Skeletons living in Rabbie Dungeon are controlled by a more powerful evil force than regular Skeletons. Please hunt [30 Metal Skeletons]."));
		SetType(QuestType.Collect);
		SetCancelable(true);

		SetIcon(QuestIcon.Party);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("obj", L("Hunt 30 Metal Skeletons"), 0, 0, 0, Kill(30, "/metalskeleton/"));

		AddReward(Exp(1107));
		AddReward(Gold(2949));
	}
}
