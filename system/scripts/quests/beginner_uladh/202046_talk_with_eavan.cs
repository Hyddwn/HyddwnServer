//--- Aura Script -----------------------------------------------------------
// Talk with Eavan
//--- Description -----------------------------------------------------------
// ?
//---------------------------------------------------------------------------

public class TalkEavanQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(202046);
		SetName("Talk with Eavan");
		SetDescription("I am Eavan. I work at the Town Office taking care of Adventures' Association business. You have turned into an excellent adventurer. I have something to tell you. Can you come see me? - Eavan -");

		SetReceive(Receive.Automatically);
		AddPrerequisite(Completed(202044)); // Harvest Potato

		AddObjective("talk", "Talk with Eavan", 14, 40024, 41041, Talk("eavan"));

		AddReward(Exp(4000));
		AddReward(Item(51003, 10)); // HP 50 Potion

		AddHook("_eavan", "after_intro", AfterIntro);
	}

	public async Task<HookResult> AfterIntro(NpcScript npc, params object[] args)
	{
		if (npc.QuestActive(this.Id, "talk"))
		{
			npc.FinishQuest(this.Id, "talk");
			npc.Msg("(Missing dialog: Talk with Eavan)");

			return HookResult.Break;
		}

		return HookResult.Continue;
	}
}
