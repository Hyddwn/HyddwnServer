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
		SetName(L("[PQ] Hunt Down the Kobold Bandits"));
		SetDescription(L("Kobold Bandits are a type of Kobold that steal. These thieves are stealing from the travelers passing through Gairech. Please [Hunt 30 Kobold Bandits]."));
		SetType(QuestType.Collect);
		SetCancelable(true);

		SetIcon(QuestIcon.Party);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("obj", L("Hunt 30 Kobold Bandits"), 0, 0, 0, Kill(30, "/koboldbandit/"));

		AddReward(Exp(774));
		AddReward(Gold(1800));
	}
}
