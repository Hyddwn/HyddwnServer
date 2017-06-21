//--- Aura Script -----------------------------------------------------------
// Smash Skill Training (Humans)
//--- Description -----------------------------------------------------------
// Skill rank up quest received upon earning Rank A Smash
//---------------------------------------------------------------------------

public class SmashTrainingQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(200014);
		SetName(L("Smash Skill Training"));
		SetDescription(L("It's me. Instructor Ranald of Tir Chonaill. I heard you were training for the Smash skill. Don't you think the Smash is a wonderful skill? If you reach [Smash skill rank 8] I will give you something that will help with your training. - Ranald -"));

		SetIcon(QuestIcon.CloseCombat);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Skill);

		SetReceive(Receive.Automatically);
		AddPrerequisite(ReachedRank(SkillId.Smash, SkillRank.RA));

		AddObjective("learn_smash_r8", L("Learn Smash Skill Rank 8"), 0, 0, 0, ReachRank(SkillId.Smash, SkillRank.R8));

		AddReward(AP(1));
		AddReward(Exp(500));
	}
}

