//--- Aura Script -----------------------------------------------------------
// [PQ] Hunt Down the Brown Bears (10)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class BrownBear10PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100019);
		SetScrollId(70025);
		SetName("[PQ] Hunt Down the Brown Bears");
		SetDescription("They say that there is a [brown bear in the plains far away from town]. Supposedly, the Brown Bears have tremendous power. Will you please hunt [10 brown bears]?");
		SetType(QuestType.Collect);

		AddObjective("obj", "Hunt 10 Brown Bears", 0, 0, 0, Kill(10, "/brownbear/"));

		AddReward(Exp(255));
		AddReward(Gold(564));
		AddReward(QuestScroll(100020)); // [PQ] Hunt Down the Brown Bears (30)
	}
}
