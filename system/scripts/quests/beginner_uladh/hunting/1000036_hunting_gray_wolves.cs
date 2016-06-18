//--- Aura Script -----------------------------------------------------------
// Hunt 5 Gray Wolves
//--- Description -----------------------------------------------------------
// Third hunting quest beginner quest series, started automatically
// after reaching level 14 or completing Hunt 5 Gray Foxes .
//---------------------------------------------------------------------------

public class GrayWolvesQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(1000036);
		SetName(L("Hunt 5 Gray Wolves"));
		SetDescription(L("I think it's time you fight the beasts. Have you ever heard about the gray wolves in the southern plains of Tir Chonaill? Try to hunt 5 gray wolves. Even without any reporting, I will pay you if you complete the mission - Ranald -"));

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Basic);

		SetReceive(Receive.Automatically);
		AddPrerequisite(Or(Completed(1000035), ReachedTotalLevel(14)));

		AddObjective("kill_wolves", L("Hunt 5 Gray Wolves"), 1, 11682, 21291, Kill(5, "/graywolf/"));

		AddReward(Exp(570));
		AddReward(Item(50021, 1));
	}
}

