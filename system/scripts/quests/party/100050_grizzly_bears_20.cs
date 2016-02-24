//--- Aura Script -----------------------------------------------------------
// [PQ] Hunt Down the Grizzly Bears (20)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class GrizzlyBears20_2PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100050);
		SetScrollId(70025);
		SetName("[PQ] Hunt Down the Grizzly Bears");
		SetDescription("The Grizzly Bears roaming in the plains are under a mighty evil spell which can be seen in their eyes. Please [Hunt 10 Red Grizzly Bears, and Hunt 10 Brown Grizzly Bears].");
		SetType(QuestType.Collect);

		AddObjective("obj1", "Hunt 10 Red Grizzly Bears", 0, 0, 0, Kill(10, "/redgrizzlybear/"));
		AddObjective("obj2", "Hunt 10 Brown Grizzly Bears", 0, 0, 0, Kill(10, "/browngrizzlybear/"));

		AddReward(Exp(549));
		AddReward(Gold(1146));
	}
}
