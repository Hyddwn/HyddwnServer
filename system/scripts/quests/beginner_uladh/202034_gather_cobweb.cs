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

			npc.Msg("(Missing dialog: Appreciation for getting the cobweb)");

			return HookResult.Break;
		}

		return HookResult.Continue;
	}
}
