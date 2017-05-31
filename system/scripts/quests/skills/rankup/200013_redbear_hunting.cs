//--- Aura Script -----------------------------------------------------------
// The Hunt for Red Bears
//--- Description -----------------------------------------------------------
// Skill rank up quest received upon earning Rank B Combat Mastery
//---------------------------------------------------------------------------

public class CombatMasteryTrainingQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(200013);
		SetName(L("The Hunt for Red Bears"));
		SetDescription(L("It's me. Tracy. The bears near the logging camp are a hazard to the charming trees around the camp. It would be so nice if you'd get rid of just [1 bear]. Please help. -Tracy-"));

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Tutorial);

		SetReceive(Receive.Automatically);
		AddPrerequisite(ReachedRank(SkillId.CombatMastery, SkillRank.RB));

		AddObjective("kill_bear", L("Hunt 1 Red Bear"), 16, 8400, 57200, Kill(1, "/redbear/"));
		AddObjective("talk", "Talk to Tracy", 16, 22900, 59500, Talk("tracy"));

		AddReward(AP(1));

		AddHook("_tracy", "after_intro", AfterIntro);
	}
	
	public async Task<HookResult> AfterIntro(NpcScript npc, params object[] args)
	{
		if (npc.Player.QuestActive(this.Id, "talk"))
		{
			npc.Player.FinishQuestObjective(this.Id, "talk");

			npc.Msg("Hey... I'm Tracy, the lumberjack.<br/>When I say hi, I say it right!");
			npc.Msg("What, you have something to say?");
			npc.Msg("Hahaha! Oh my! You really killed the bear, just like I asked!<br/>Those beasts have been scratching away at the trees with their dirty claws,<br/>and we were running out of useable wood.");
			npc.Msg("So, thank you, thank you!<br/>I love you! So, so much!");
			npc.Msg("Don't feel bad.<br/>I'm just doing my job. Hahaha!");

			return HookResult.Break;
		}

		return HookResult.Continue;
	}
}

