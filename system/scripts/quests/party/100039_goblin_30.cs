//--- Aura Script -----------------------------------------------------------
// [PQ] Hunt Down the Goblins (30)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class Goblins30PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100039);
		SetScrollId(70025);
		SetName("[PQ] Hunt Down the Goblins");
		SetDescription("Goblins are small ugly monsters with dull green skin and red eyes. These small bogeyman-like creatures are from the lower class of Fomors. Please do us a favor and [hunt 30 goblins].");
		SetType(QuestType.Collect);

		AddObjective("obj", "Hunt 30 Goblins", 0, 0, 0, Kill(30, "/goblin/"));

		AddReward(Exp(426));
		AddReward(Gold(957));
	}
}
