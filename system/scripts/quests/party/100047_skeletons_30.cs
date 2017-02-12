//--- Aura Script -----------------------------------------------------------
// [PQ] Hunt Skeletons (30)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class VariousSkeletons30PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100047);
		SetScrollId(70025);
		SetName(L("[PQ] Hunt Skeletons"));
		SetDescription(L("Various Skeletons under the evil spell are emerging as of late. I will reward you if you [Hunt 10 Metal Skeletons and 20 Red Skeletons]."));
		SetType(QuestType.Collect);
		SetCancelable(true);

		SetIcon(QuestIcon.Party);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("obj1", L("Hunt 10 Metal Skeletons"), 0, 0, 0, Kill(10, "/metalskeleton/"));
		AddObjective("obj2", L("Hunt 20 Red Skeletons"), 0, 0, 0, Kill(20, "/redskeleton/"));

		AddReward(Exp(762));
		AddReward(Gold(1890));
	}
}
