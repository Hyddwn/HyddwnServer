//--- Aura Script -----------------------------------------------------------
// Hunt 5 Kobold Bandits
//--- Description -----------------------------------------------------------
// Eleventh hunting quest beginner quest series, started automatically
// after completing Hunt 1 Wisp.
//---------------------------------------------------------------------------

public class KoboldBanditsQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(1000044);
		SetName(L("Hunt 5 Kobold Bandits"));
		SetDescription(L("I am Comgan, serving as a priest at Bangor. Evil creatures near Bangor are threatening town residents. Can you please hunt 5 kobold bandits? - Comgan -"));

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Basic);

		SetReceive(Receive.Automatically);
		AddPrerequisite(Completed(1000043));

		AddObjective("kill_bandits", L("Hunt 5 Kobold Bandits"), 30, 38713, 23402, Kill(5, "/koboldbandit/"));

		AddReward(Exp(4000));
		AddReward(Item(51007, 3));
	}
}

