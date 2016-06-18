//--- Aura Script -----------------------------------------------------------
// Hunt 1 Black Succubus
//--- Description -----------------------------------------------------------
// Fourth hunting quest Boss quest series, started automatically
// after Completing Hunt 1 Hellhound.
//---------------------------------------------------------------------------

public class BlackSuccubusQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(1000058);
		SetName(L("Hunt 1 Black Succubus"));
		SetDescription(L("If you go to the end of Rabbie Dungeon alone the Succubus will attack. Be careful not to be fooled by the monster. It tempts humans with its tongue and its looks. - Aranwen -"));

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Basic);

		SetReceive(Receive.Automatically);
		AddPrerequisite(Completed(1000057));

		AddObjective("kill_succubus", L("Hunt 1 Black Succubus"), 24, 3198, 3427, Kill(1, "/black_succubus/"));

		AddReward(Exp(30000));
		AddReward(Item(18506, 1));
	}
}

