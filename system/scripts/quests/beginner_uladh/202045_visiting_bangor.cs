//--- Aura Script -----------------------------------------------------------
// Visiting Bangor
//--- Description -----------------------------------------------------------
// ?
//---------------------------------------------------------------------------

public class VisitingBangorQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(202045);
		SetName("Visiting Bangor");
		SetDescription("I'm Comgan, the priest of Bangor. Lately, it's been difficult extracting ores from the mines because of the sudden rush of Fomors occupying dungeons in Bangor. If you receive this scroll, I'd appreciate it a great deal if you can make your way to Bangor. Thank you. - Comgan -");

		SetReceive(Receive.Automatically);
		AddPrerequisite(Completed(202046)); // Talk with Eavan

		AddObjective("talk", "Talk with Comgan of Bangor", 31, 15329, 12122, Talk("comgan"));

		AddReward(Exp(10000));
		AddReward(Item(1611)); // Item Upgrade Guidebook

		AddHook("_comgan", "after_intro", AfterIntro);
	}

	public async Task<HookResult> AfterIntro(NpcScript npc, params object[] args)
	{
		if (npc.QuestActive(this.Id, "talk"))
		{
			npc.FinishQuest(this.Id, "talk");
			npc.Msg("(Missing dialog: Visiting Bangor)");

			return HookResult.Break;
		}

		return HookResult.Continue;
	}
}
