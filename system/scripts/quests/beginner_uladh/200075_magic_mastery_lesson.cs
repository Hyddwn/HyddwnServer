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
		if (npc.QuestActive(this.Id, "obj1"))
		{
			npc.FinishQuest(this.Id, "obj1");

			npc.Msg(L("Umm... Are you <username/> by any chance?"));
			npc.Msg(L("Hahaha! You look exactly like the way Bebhinn described.<br/>I'm sorry, I apologize for laughing.<br/>Nice to meet you, I am Lassar."));
			npc.Msg(L("Ah... I suppose you came to learn about Magic Mastery.<br/>Welcome, <username/>. I will explain this so that you can understand pretty easily."));
			npc.Msg(L("The term 'Magic Mastery' refers to<br/>the common principle of magic.<br/>Magic mastery consists of learning how to use magic and<br/>with understanding the theory behind magic."));
			npc.Msg(L("The more you train Magic Mastery,<br/>The more you'll increase your intelligence and your mana limit."));
			npc.Msg(L("But even if your Magic Mastery is very high,<br/>It doesn't necessarily mean you're<br/>more familiar with other magic."));
			npc.Msg(L("Magic mastery<br/>is only a passive magic boost<br/>that allows you use other magic more easily."));
			npc.Msg(L("The minute you end the quest<br/>you will be able to understand entirely."));
			npc.Msg(L("...Through the Magic Mastery<br/>I hope you may get close<br/>to the main principle<br/>which forms this world, <username/>..."));
			npc.Msg(L("...Well, how about taking magic classes<br/>since you're already here at school?<br/>You can learn 3 kinds of magic<br/>on dealing with elements."));

			return HookResult.Break;
		}

		return HookResult.Continue;
	}
}
