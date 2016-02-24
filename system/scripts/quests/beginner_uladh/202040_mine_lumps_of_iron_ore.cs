//--- Aura Script -----------------------------------------------------------
// Mine Lumps of Iron Ore
//--- Description -----------------------------------------------------------
// Elen asks player to mine Iron Ore.
//---------------------------------------------------------------------------

public class ElenIronOreMiningQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(202040);
		SetName("Mine Lumps of Iron Ore");
		SetDescription("Hello, I am Elen from the Bangor Blacksmith Shop. I'm low on blacksmith material because no one wants to mine iron ore these days. Perhaps you could mine 10 Lumps of Iron Ore for me? - Elen -");

		SetReceive(Receive.Automatically);
		AddPrerequisite(Completed(202045)); // Visiting Bangor

		AddObjective("talk", "Deliver 10 Lumps of Iron Ore to Elen", 31, 11353, 12960, Talk("elen"));

		AddReward(Exp(8000));
		AddReward(Gold(1150));
		AddReward(Item(40024)); // Blacksmith Hammer

		AddHook("_elen", "after_intro", AfterIntro);
	}

	public async Task<HookResult> AfterIntro(NpcScript npc, params object[] args)
	{
		if (npc.QuestActive(this.Id, "talk") && npc.HasItem(64002, 5))
		{
			npc.FinishQuest(this.Id, "talk");
			npc.RemoveItem(64002, 5); // Iron Ore
			npc.Msg("(Missing dialog: Mine Lumps of Iron Ore)");

			return HookResult.Break;
		}

		return HookResult.Continue;
	}
}
