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

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Tutorial);

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
			
			npc.Msg(L("Welcome to Dunbarton.<br/>My name is Eavan, the Town Office worker who takes care of all the business related to the Adventurer's Association."));
			npc.Msg(L("<username/>, your outstanding achievements are already well-known<br/>all around the Adventurers' Association."));
			npc.Msg(L("I'm certain that all the hardships you went through<br/>will help you during your stay<br/>here on Erinn."));
			npc.Msg(L("You've done very well."));

			return HookResult.Break;
		}

		return HookResult.Continue;
	}
}
