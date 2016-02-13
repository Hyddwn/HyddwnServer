//--- Aura Script -----------------------------------------------------------
// [PQ] Hunt Down the Young Grizzly Bears (30)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class YoungGrizzlyBears30_2PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100051);
		SetScrollId(70025);
		SetName("[PQ] Hunt Down the Young Grizzly Bears");
		SetDescription("Grizzly cubs are small but they are still a threat to humans. Please [Hunt 15 Red Grizzly Bear Cubs, and Brown Grizzly Bear Cubs.]");
		SetType(QuestType.Collect);

		AddObjective("obj1", "Hunt 15 Red Grizzly Bear Cubs", 0, 0, 0, Kill(15, "/red/grizzlybearkid/"));
		AddObjective("obj2", "Hunt 15 Brown Grizzly Bear Cubs", 0, 0, 0, Kill(15, "/brown/grizzlybearkid/"));

		AddReward(Exp(393));
		AddReward(Gold(843));
	}
}
