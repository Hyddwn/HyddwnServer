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
		SetName("[PQ] Hunt Down the Skeletons");
		SetDescription("Have you ever seen a Skeleton? The Skeletons living in Rabbie Dungeon do not have their own will or consciousness. They do not belong in this world. Please hunt [30 Skeletons].");
		SetType(QuestType.Collect);

		AddObjective("obj", "Hunt 30 Skeletons", 0, 0, 0, Kill(30, "/normalskeleton/"));

		AddReward(Exp(882));
		AddReward(Gold(2352));
	}
}
