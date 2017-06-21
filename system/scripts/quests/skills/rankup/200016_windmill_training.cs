//--- Aura Script -----------------------------------------------------------
// Stylish Attack (Human)
//--- Description -----------------------------------------------------------
// Skill rank up quest received upon earning Rank D Windmill
//---------------------------------------------------------------------------

public class WindmillTrainingQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(200016);
		SetName(L("Stylish Attack"));
		SetDescription(L("I'm Aranwen from Dunbarton. The Windmill skill is a difficult skill, but don't you think it's a beautiful skill? It's a skill only for stylish and elegant warriors. If you manage to [Train the Windmill Skill to Rank 9], I will give you a gift that will help in your training. -Aranwen-"));

		SetIcon(QuestIcon.CloseCombat);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Skill);

		SetReceive(Receive.Automatically);
		AddPrerequisite(ReachedRank(SkillId.Windmill, SkillRank.RD));

		AddObjective("learn_windmill_r9", L("Learn Windmill Skill Rank 9"), 0, 0, 0, ReachRank(SkillId.Windmill, SkillRank.R9));

		AddReward(AP(3));
		AddReward(Exp(1000));
	}
}

