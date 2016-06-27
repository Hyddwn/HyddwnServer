//--- Aura Script -----------------------------------------------------------
// Gather Cobweb
//--- Description -----------------------------------------------------------
// Malcolm asks the player to bring him Cobweb.
//---------------------------------------------------------------------------

public class GatherCobwebQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(202034);
		SetName("Gather Cobweb");
		SetDescription("Hello. This is Malcolm in the General Shop of Tir Chonaill. I need some Cobwebs to work with. Anyone willing to get some for me? Just take 5 pieces of Cobweb from the Graveyard, please. Do not forget to use the ALT key in picking them up. - Malcolm -");

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Tutorial);

		SetReceive(Receive.Automatically);
		AddPrerequisite(Completed(202035)); // Sheep-shearing

		AddObjective("talk", "Convey 5 pieces of Cobweb to Malcolm", 8, 1238, 1655, Talk("malcolm"));

		AddReward(Exp(1500));
		AddReward(Gold(550));
		AddReward(Item(1604)); // Tailoring Guidebook

		AddHook("_malcolm", "after_intro", AfterIntro);
	}

	public async Task<HookResult> AfterIntro(NpcScript npc, params object[] args)
	{
		if (npc.QuestActive(this.Id) && npc.HasItem(60008, 5))
		{
			npc.RemoveItem(60008, 5); // Cobweb
			npc.CompleteQuest(this.Id);

			npc.Msg(L("You brought the cobwebs! Thank you."));
			npc.Msg(L("Cobwebs are used to make Fine Yarn.<br/>The cobwebs dropped by the spiders in TirChonaill are sturdier than usual, so they're great for making strings."));
			npc.Msg(L("If you're interested in making thread,<br/>come and talk to me with the keyword 'Skill' after gathering Cobwebs.<br/>I'll tell you about the Weaving skill."));

			return HookResult.Break;
		}

		return HookResult.Continue;
	}
}
