//--- Aura Script -----------------------------------------------------------
// [PQ] Hunt Down the Brown Bears (30)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class BrownBear30PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100020);
		SetScrollId(70025);
		SetName("[PQ] Hunt Down the Brown Bears");
		SetDescription("They say that there is a [brown bear in the plains far away from town]. Supposedly, the Brown Bears have tremendous power. Will you please hunt [30 brown bears]?");
		SetType(QuestType.Collect);

		AddObjective("obj", "Hunt 30 Brown Bears", 0, 0, 0, Kill(30, "/brownbear/"));

		AddReward(Exp(693));
		AddReward(Gold(1614));
	}
}
