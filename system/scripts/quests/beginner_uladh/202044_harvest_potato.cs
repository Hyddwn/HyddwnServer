//--- Aura Script -----------------------------------------------------------
// Harvest Potato
//--- Description -----------------------------------------------------------
// Kristell asks player to get her potatoes.
//---------------------------------------------------------------------------

public class HarvestPotatoQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(202044);
		SetName("Harvest Potato");
		SetDescription("Hello? I am Kristell in Dunbarton Church. There's a potato patch near Dunbarton where you can dig up some potatoes. Would you mind digging some with a weeding hoe? I will tell you about Holy Water of Lymilark as well as some rewards if you give me 5 Potatoes. Also, you will be rewarded. - Kristell -");

		SetReceive(Receive.Automatically);
		AddPrerequisite(Completed(202047)); // Gather Egg

		AddObjective("talk", "Deliver 5 Potatoes to Kristell in Dunbarton Church", 14, 34657, 42808, Talk("kristell"));

		AddReward(Exp(4000));
		AddReward(Item(1610)); // Blessing Guidebook
		AddReward(Item(63016, 2)); // Holy Water of Lymilark

		AddHook("_kristell", "after_intro", AfterIntro);
	}

	public async Task<HookResult> AfterIntro(NpcScript npc, params object[] args)
	{
		if (npc.QuestActive(this.Id, "talk") && npc.HasItem(50010, 5))
		{
			npc.FinishQuest(this.Id, "talk");
			npc.RemoveItem(50010, 5); // Potatoes
			npc.Msg("(Missing dialog: Harvest Potato)");

			return HookResult.Break;
		}

		return HookResult.Continue;
	}
}
