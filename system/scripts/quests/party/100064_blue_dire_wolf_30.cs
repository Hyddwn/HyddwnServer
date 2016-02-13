//--- Aura Script -----------------------------------------------------------
// [PQ] Hunt Down the Blue Dire Wolves (30)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class BlueDireWolf30PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100064);
		SetScrollId(70025);
		SetName("[PQ] Hunt Down the Blue Dire Wolves");
		SetDescription("Blue dire wolves under the control of an evil power are attacking travelers. Please [hunt 30 blue dire wolves].");
		SetType(QuestType.Collect);

		AddObjective("obj", "Hunt 30 Blue Dire Wolves", 0, 0, 0, Kill(30, "/bluedirewolf/"));

		AddReward(Exp(1350));
		AddReward(Gold(2676));
	}
}
