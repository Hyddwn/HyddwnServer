//--- Aura Script -----------------------------------------------------------
//The Way of the Healer (Human)
//--- Description -----------------------------------------------------------
// Skill rank up quest received upon earning Rank E Healing
//---------------------------------------------------------------------------

public class HealingTrainingQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(200019);
		SetName(L("The Way of the Healer"));
		SetDescription(L("I am Healer Dilys. How's your Healing skill training going? The way to becoming a healer needs patience and effort. Don't get disappointed and keep on training. Someday you will become a good healer. If you [keep training the Healing skill to rank 9], I will give you something that will help with your training. -Dilys-"));

		SetIcon(QuestIcon.Magic);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Skill);

		SetReceive(Receive.Automatically);
		AddPrerequisite(ReachedRank(SkillId.Healing, SkillRank.RD));

		AddObjective("learn_heal_r9", L("Learn Healing Skill Rank 9"), 0, 0, 0, ReachRank(SkillId.Healing, SkillRank.R9));

		AddReward(AP(5));
		AddReward(Exp(1000));
	}
}

