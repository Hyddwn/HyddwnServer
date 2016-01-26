//--- Aura Script -----------------------------------------------------------
// Caution against Fire
//--- Description -----------------------------------------------------------
// Started automatically after getting rF Campfire and finishing the quest
// Save my Sheep.
//---------------------------------------------------------------------------

public class CautionAgainstFireQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(200024);
		SetName("Caution against Fire");
		SetDescription("Hello? I am Trefor. Isn't it fun to have a campfire? Don't forget to take care of fire safety. It will be a big problem if the forest gets burned while having a campfire in the deep woods. If you come to me and take my class on fire safety. I will give you a book that will raise your Campfire skill rank. - Trefor -");

		SetReceive(Receive.Automatically);
		AddPrerequisite(Completed(202003)); // Save my Sheep
		AddPrerequisite(ReachedRank(SkillId.Campfire, SkillRank.RF));

		AddObjective("talk", "Talk with Trefor of Tir Chonaill", 1, 8692, 52637, Talk("trefor"));

		AddReward(Item(1029)); // A Campfire Memory

		AddHook("_trefor", "after_intro", AfterIntro);
	}

	public async Task<HookResult> AfterIntro(NpcScript npc, params object[] args)
	{
		if (!npc.QuestActive(this.Id))
			return HookResult.Continue;
		npc.FinishQuest(this.Id, "talk");

		npc.Msg("(Missing dialog: Advice about Campfire)");

		return HookResult.Break;
	}
}
