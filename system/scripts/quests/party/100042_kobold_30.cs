//--- Aura Script -----------------------------------------------------------
// [PQ] Hunt Down the Kobolds (30)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class Kobolds30PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100042);
		SetScrollId(70025);
		SetName("[PQ] Hunt Down the Kobolds");
		SetDescription("A Kobold's dog-like legs make them physically stronger than Goblins but their intellect is far below that of humans. Please [Hunt 30 Kobolds].");
		SetType(QuestType.Collect);

		AddObjective("obj", "Hunt 30 Kobolds", 0, 0, 0, Kill(30, "/kobold/"));

		AddReward(Exp(726));
		AddReward(Gold(1635));
	}
}
