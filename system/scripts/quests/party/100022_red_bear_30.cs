//--- Aura Script -----------------------------------------------------------
// [PQ] Hunt Down the Red Bears (30)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class RedBear30PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100022);
		SetScrollId(70025);
		SetName("[PQ] Hunt Down the Red Bears");
		SetDescription("It is uncommon to see red bear roaming around the plains. Supposedly, the Red Bears have amazing power. Will you please hunt [30 red bears]?");
		SetType(QuestType.Collect);

		AddObjective("obj", "Hunt 30 Red Bears", 0, 0, 0, Kill(30, "/redbear/"));

		AddReward(Exp(777));
		AddReward(Gold(1845));
	}
}
