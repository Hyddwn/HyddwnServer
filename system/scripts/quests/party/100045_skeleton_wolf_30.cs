//--- Aura Script -----------------------------------------------------------
// [PQ] Hunt Down the Skeleton Wolves (30)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class SkeletonWolves30PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100045);
		SetScrollId(70025);
		SetName("[PQ] Hunt Down the Skeleton Wolves");
		SetDescription("Recently, there has been an emergence of Skeleton Wolves. Skeleton Wolves are under a stronger evil spell than regular Wolves. I will give you a reward if you [Hunt 30 Skeleton Wolves].");
		SetType(QuestType.Collect);

		AddObjective("obj", "Hunt 30 Skeleton Wolves", 0, 0, 0, Kill(30, "/skeletonwolf/"));

		AddReward(Exp(624));
		AddReward(Gold(1560));
	}
}
