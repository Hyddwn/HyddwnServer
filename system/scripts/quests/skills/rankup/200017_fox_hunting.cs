//--- Aura Script -----------------------------------------------------------
// Hunt Foxes
//--- Description -----------------------------------------------------------
// Skill rank up quest received upon earning Rank C Ranged Attack
//---------------------------------------------------------------------------

public class RangedAttackTrainingQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(200017);
		SetName(L("Hunt Foxes"));
		SetDescription(L("I'm Malcolm. Don't you think the fox population in the pasture has grown too big these days? It wouldn't be a problem if they didn't attack our chickens... but I do think we need to get rid of them to stay safe. Please hunt [30 red foxes]. - Malcolm -"));

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Tutorial);

		SetReceive(Receive.Automatically);
		AddPrerequisite(ReachedRank(SkillId.RangedAttack, SkillRank.RC));

		AddObjective("kill_fox", L("Hunt 30 Red Foxes"), 1, 32700, 42200, Kill(30, "/redfox/"));

		AddReward(AP(1));
		AddReward(Exp(500));
	}
}

