//--- Aura Script -----------------------------------------------------------
// Do you know Production Mastery skill?
//--- Description -----------------------------------------------------------
// Alissa tells the player about the Production Mastery skill.
// Starts automatically after Duncan's Praise.
//---------------------------------------------------------------------------

public class ProductionMasteryHumanQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(200079);
		SetName("Do you know Production Mastery skill?");
		SetDescription("How are you doing? I'm Alissa. I help the milling in Tir Chonaill. Aren't you curious about the Production Mastery skill? If you are, meet me in front of the windmill in Tir Chonaill~");

		SetReceive(Receive.Automatically);
		AddPrerequisite(Or(Completed(202036), Completed(202037), Completed(202038), Completed(2020369))); // Duncan's Praise

		AddObjective("talk", "Talk with Alissa", 1, 15765, 31015, Talk("alissa"));

		AddReward(Exp(50));
		AddReward(Skill(SkillId.ProductionMastery, SkillRank.Novice, 1));

		AddHook("_alissa", "after_intro", AfterIntro);
	}

	public async Task<HookResult> AfterIntro(NpcScript npc, params object[] args)
	{
		if (npc.QuestActive(this.Id, "talk"))
		{
			npc.FinishQuest(this.Id, "talk");

			npc.Msg("Hey, it's you, <username/>!<br/>I was wondering why you didn't come to see me.");
			npc.Msg("Have you heard of the Production Mastery skill?<br/>It's a skill that's needed for both gathering and crafting.<br/>You can make things more easily if you know this skill.");
			npc.Msg("You will have more Stamina,<br/>and you will have a better success rate at gathering and crafting.<br/>Your dexterity will increase as well.");
			npc.Msg("Note that this is not a separate skill.<br/>The skill will activate automatically whenever you gather things or do some crafting,<br/>so you don't need to worry about learning it.");
			npc.Msg("Now that I told you about the skill,<br/>you will be able to get the Production Mastery skill when you complete the quest.");

			return HookResult.Break;
		}

		return HookResult.Continue;
	}
}
