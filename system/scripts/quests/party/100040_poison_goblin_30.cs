//--- Aura Script -----------------------------------------------------------
// [PQ] Hunt Down the Poison Goblins (30)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class PoisonGoblins30PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100040);
		SetScrollId(70025);
		SetName("[PQ] Hunt Down the Poison Goblins");
		SetDescription("Poison goblins are different from ordinary goblins with their poisonous purple skin. This threatening creature is from the lower class of Fomors. Please do us a favor and [hunt 30 poison goblins].");
		SetType(QuestType.Collect);

		AddObjective("obj", "Hunt 30 Poison Goblins", 0, 0, 0, Kill(30, "/goblin/poison/"));

		AddReward(Exp(447));
		AddReward(Gold(1005));
	}
}
