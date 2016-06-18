//--- Aura Script -----------------------------------------------------------
// Magic Mastery Lesson
//--- Description -----------------------------------------------------------
// Lassar teaches Magic Mastery to the player.
//---------------------------------------------------------------------------

public class MagicMasteryLessonQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(200075);
		SetName(L("Magic Mastery Lesson"));
		SetDescription(L("I am Lassar from Tir Chonaill and I teach magic. Have you ever heard about Magic Mastery? If you are interested in understanding about magic, please visit me. I will explain magic so you can understand easily. - Lassar -"));

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Tutorial);

		SetReceive(Receive.Automatically);
		AddPrerequisite(Completed(202045)); // Visiting Bangor
		AddPrerequisite(NotSkill(SkillId.MagicMastery));

		AddObjective("obj1", L("Talk with Lassar about taking Magic Lessons"), 9, 2020, 1537, Talk("lassar"));

		AddReward(Exp(800));
		AddReward(Skill(SkillId.MagicMastery, SkillRank.Novice));

		AddHook("_lassar", "after_intro", AfterIntro);
	}

	public async Task<HookResult> AfterIntro(NpcScript npc, params object[] args)
	{
		if (npc.QuestActive(this.Id,"obj1"))
		{
			npc.FinishQuest(this.Id, "obj1");
			npc.Msg(L("(Missing dialog: Magic Mastery Lesson"));

			return HookResult.Break;
		}

		return HookResult.Continue;
	}
}
