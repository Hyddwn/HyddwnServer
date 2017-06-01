//--- Aura Script -----------------------------------------------------------
// Free Music Score (Human)
//--- Description -----------------------------------------------------------
// Skill rank up quest received upon earning Rank F Composing
//---------------------------------------------------------------------------

public class ComposingTrainingQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(200022);
		SetName(L("Free Music Score"));
		SetDescription(L("I am Dilys of Tir Chonaill. I heard you were studying composition. I have an [empty music score] that I used to use when I was studying. If you want, I will give it to you as a gift. Except, you have to go over [Compose skill rank E]. If you don't study, I'm not going to give it to you. - Dilys -"));

		SetIcon(QuestIcon.Music);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Skill);

		SetReceive(Receive.Automatically);
		AddPrerequisite(ReachedRank(SkillId.Composing, SkillRank.RF));

		AddObjective("learn_composing_rE", L("Learn Compose Skill Rank E"), 0, 0, 0, ReachRank(SkillId.Composing, SkillRank.RE));
		AddObjective("talk", "Talk with Dilys", 6, 1107, 1050, Talk("dilys"));

		AddReward(AP(1));
		AddReward(Item(61001, 10)); // Score Scroll x 10

		AddHook("_dilys", "after_intro", AfterIntro);
	}
	
	public async Task<HookResult> AfterIntro(NpcScript npc, params object[] args)
	{
		if (npc.Player.QuestActive(this.Id, "talk"))
		{
			npc.Player.FinishQuestObjective(this.Id, "talk");

			npc.Msg("Ah, it's <username/>.<br/>Your Compose rank is already at E... You're pretty good.");
			npc.Msg("Open the Quest window and press Mission Complete!<br/>The owl will deliver the book to you.");
			npc.Msg("Keep practicing!<br/>Write a song about me, too!");

			return HookResult.Break;
		}

		return HookResult.Continue;
	}
}

