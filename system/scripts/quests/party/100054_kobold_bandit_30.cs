//--- Aura Script -----------------------------------------------------------
// [PQ] Hunt Down the Kobold Bandits (30)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class KoboldBandit30PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100054);
		SetScrollId(70025);
		SetName("[PQ] Hunt Down the Kobold Bandits");
		SetDescription("Kobold Bandits are a type of Kobold that steal. These thieves are stealing from the travelers passing through Gairech. Please [Hunt 30 Kobold Bandits].");
		SetType(QuestType.Collect);

		AddObjective("obj", "Hunt 30 Kobold Bandits", 0, 0, 0, Kill(30, "/koboldbandit/"));

		AddReward(Exp(774));
		AddReward(Gold(1800));
	}
}
