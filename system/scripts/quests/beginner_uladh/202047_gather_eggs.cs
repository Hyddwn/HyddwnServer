//--- Aura Script -----------------------------------------------------------
// Gather Eggs
//--- Description -----------------------------------------------------------
// Glenis asks player to get her eggs.
//---------------------------------------------------------------------------

public class GatherEggsQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(202047);
		SetName("Gather Eggs");
		SetDescription("This is Glenis from the Restaurant. I'm short on ingredients for dough, so can you get me 5 eggs?");

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Tutorial);

		SetReceive(Receive.Automatically);
		AddPrerequisite(Completed(202043)); // Talk with Stewart

		AddObjective("talk", "Deliver 5 eggs to Glenis", 14, 37566, 41605, Talk("glenis"));

		AddReward(Exp(4000));
		AddReward(Item(51013, 5)); // Stamina 50 Potion

		AddHook("_glenis", "after_intro", AfterIntro);
	}

	public async Task<HookResult> AfterIntro(NpcScript npc, params object[] args)
	{
		if (npc.QuestActive(this.Id, "talk") && npc.HasItem(50009, 5))
		{
			npc.FinishQuest(this.Id, "talk");
			npc.RemoveItem(50009, 5); // Eggs

			npc.Msg(L("Oh, thank you. That was fast.<br/>If you get hungry later on, feel free to drop by.<br/>I've got plenty of delicious food in store for you!"));

			return HookResult.Break;
		}

		return HookResult.Continue;
	}
}
