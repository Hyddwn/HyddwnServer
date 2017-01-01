//--- Aura Script -----------------------------------------------------------
// [PQ] Hunt Down the Dire Wolves and the Grizzly Bears (40)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class DireWolvesGrizzlyBears40PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100053);
		SetScrollId(70025);
		SetName(L("[PQ] Hunt Down the Dire Wolves and the Grizzly Bears"));
		SetDescription(L("Recently, Dire Wolves and Grizzly Bears, which are under a powerful evil spell, are threatening travelers passing through north of Gairech. Please [Hunt 20 Brown Dire Wolves and 20 Grizzly Bears]."));
		SetType(QuestType.Collect);
		SetCancelable(true);

		SetIcon(QuestIcon.Party);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("obj1", L("Hunt 10 Brown Dire Wolves"), 0, 0, 0, Kill(10, "/browndirewolf/"));
		AddObjective("obj2", L("Hunt 10 White Dire Wolves"), 0, 0, 0, Kill(10, "/whitedirewolf/"));
		AddObjective("obj3", L("Hunt 10 Black Grizzly Bears"), 0, 0, 0, Kill(10, "/blackgrizzlybear/"));
		AddObjective("obj4", L("Hunt 10 Brown Grizzly Bears"), 0, 0, 0, Kill(10, "/browngrizzlybear/"));

		AddReward(Exp(1929));
		AddReward(Gold(3573));
	}
}
