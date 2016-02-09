//--- Aura Script -----------------------------------------------------------
// [PQ] Hunt Kobolds (30)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class VariousKobolds30PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100046);
		SetScrollId(70025);
		SetName("[PQ] Hunt Kobolds");
		SetDescription("Recently, various Kobolds have been showing up. Although Kobold's are not as intelligent as humans, they are becoming a threat. I will give you a reward if you [Hunt 10 Poison Kobolds and 20 Kobold Archers].");
		SetType(QuestType.Collect);

		AddObjective("obj1", "Hunt 10 Poison Kobolds", 0, 0, 0, Kill(10, "/poisonkobold/"));
		AddObjective("obj2", "Hunt 20 Kobold Archers", 0, 0, 0, Kill(20, "/koboldarcher/"));

		AddReward(Exp(696));
		AddReward(Gold(1596));
	}
}
