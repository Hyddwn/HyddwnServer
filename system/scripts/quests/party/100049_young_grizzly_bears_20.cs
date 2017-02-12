//--- Aura Script -----------------------------------------------------------
// [PQ] Hunt Down the Young Grizzly Bears (30)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class YoungGrizzlyBears30_1PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100049);
		SetScrollId(70025);
		SetName(L("[PQ] Hunt Down the Young Grizzly Bears"));
		SetDescription(L("Grizzly cubs are small but they are still a threat to humans. Please [Hunt 15 Black Grizzly Bear Cubs, and Brown Grizzly Bear Cubs.]"));
		SetType(QuestType.Collect);
		SetCancelable(true);

		SetIcon(QuestIcon.Party);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("obj1", L("Hunt 15 Black Grizzly Bear Cubs"), 0, 0, 0, Kill(15, "/black/grizzlybearkid/"));
		AddObjective("obj2", L("Hunt 15 Brown Grizzly Bear Cubs"), 0, 0, 0, Kill(15, "/brown/grizzlybearkid/"));

		AddReward(Exp(765));
		AddReward(Gold(1569));
	}
}
