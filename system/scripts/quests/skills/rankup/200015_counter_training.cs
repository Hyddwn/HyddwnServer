//--- Aura Script -----------------------------------------------------------
// Counterattack Training (Human)
//--- Description -----------------------------------------------------------
// Skill rank up quest received upon earning Rank B Counterattack
//---------------------------------------------------------------------------

public class CounterTrainingQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(200015);
		SetName(L("Counterattack Training"));
		SetDescription(L("I am instructor Ranald. There was a rumor that you were training for the Counter skill. If you reach [Counter skill rank 9] I will give you something that will help you with your training. - Ranald -"));

		SetIcon(QuestIcon.CloseCombat);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Skill);

		SetReceive(Receive.Automatically);
		AddPrerequisite(ReachedRank(SkillId.Counterattack, SkillRank.RB));

		AddObjective("learn_counter_r9", L("Learn Counterattack Skill Rank 9"), 0, 0, 0, ReachRank(SkillId.Counterattack, SkillRank.R9));

		AddReward(AP(1));
		AddReward(Exp(1000));
	}
}

