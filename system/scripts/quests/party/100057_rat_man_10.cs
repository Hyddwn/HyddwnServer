//--- Aura Script -----------------------------------------------------------
// [PQ] Hunt Down the Rat Men (10)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class RatMan10PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100057);
		SetScrollId(70025);
		SetName("[PQ] Hunt Down the Rat Men");
		SetDescription("Rat Men under the control of an evil power are attacking travelers. Please [hunt 10 rat men].");
		SetType(QuestType.Collect);

		AddObjective("obj", "Hunt 10 Rat Men", 0, 0, 0, Kill(10, "/ratman/"));

		AddReward(Exp(1500));
		AddReward(Gold(1362));
	}
}
