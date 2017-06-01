//--- Aura Script -----------------------------------------------------------
// Magnum Arrow (Human)
//--- Description -----------------------------------------------------------
// Skill rank up quest received upon earning Rank D Magnum Shot
//---------------------------------------------------------------------------

public class MagnumShotTrainingQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(200018);
		SetName(L("Magnum Arrow"));
		SetDescription(L("I am Ranald of the Tir Chonaill School. Your Magnum Shot was just fantastic. Do you know you can shoot faster if you keep training your [magnum shot]? I will reward you 2 AP if you [reach rank 9]. - Ranald -"));

		SetIcon(QuestIcon.CloseCombat);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Skill);

		SetReceive(Receive.Automatically);
		AddPrerequisite(ReachedRank(SkillId.MagnumShot, SkillRank.RD));

		AddObjective("learn_magnumshot_r9", L("Learn Magnum Shot Skill Rank 9"), 0, 0, 0, ReachRank(SkillId.MagnumShot, SkillRank.R9));

		AddReward(AP(2));
	}
}

