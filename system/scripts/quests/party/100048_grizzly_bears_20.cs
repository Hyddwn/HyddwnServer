//--- Aura Script -----------------------------------------------------------
// [PQ] Hunt Down the Grizzly Bears (20)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class GrizzlyBears20_1PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100048);
		SetScrollId(70025);
		SetName("[PQ] Hunt Down the Grizzly Bears");
		SetDescription("The Grizzly Bears roaming in the plains are under a mighty evil spell which can be seen in their eyes. Please [Hunt 10 Black Grizzly Bears, and Hunt 10 Brown Grizzly Bears].");
		SetType(QuestType.Collect);

		AddObjective("obj1", "Hunt 10 Black Grizzly Bears", 0, 0, 0, Kill(10, "/blackgrizzlybear/"));
		AddObjective("obj2", "Hunt 10 Brown Grizzly Bears", 0, 0, 0, Kill(10, "/browngrizzlybear/"));

		AddReward(Exp(993));
		AddReward(Gold(1950));
	}
}
