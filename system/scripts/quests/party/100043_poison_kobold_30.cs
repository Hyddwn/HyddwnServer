//--- Aura Script -----------------------------------------------------------
// [PQ] Hunt Down the Poison Kobolds (30)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class PoisonKobolds30PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100043);
		SetScrollId(70025);
		SetName("[PQ] Hunt Down the Poison Kobolds");
		SetDescription("The Poison Kobold, unlike regular Kobolds, threatens its enemies with its poisonous attacks. Please [Hunt 30 Poison Kobolds].");
		SetType(QuestType.Collect);

		AddObjective("obj", "Hunt 30 Poison Kobolds", 0, 0, 0, Kill(30, "/poisonkobold/"));

		AddReward(Exp(786));
		AddReward(Gold(1767));
	}
}
