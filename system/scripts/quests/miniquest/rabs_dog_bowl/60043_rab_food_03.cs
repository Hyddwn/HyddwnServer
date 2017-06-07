//--- Aura Script -----------------------------------------------------------
// Rab's Dog Food [Bacon and Potato Dog Food]
//--- Description -----------------------------------------------------------
// Deliver Dog food to Rab. Obtained by giving Fleta Rab's empty plate
// Rab's empty plate is earned by talking to Rab while logged on as a pet
//---------------------------------------------------------------------------

public class RabFood03QuestScript : QuestScript
{
	public override void Load()
	{
		SetId(60043);
		SetName("Rab's Dog Food");
		SetDescription("Rab looks like he wants some Bacon and Potato Dog Food. For a dog, he has quite a taste... Bacon and Potato Dog Food is literally a mix of bacon and potato. The ingredients should be available around Dunbarton, and you should gather up the potato yourself, right? Anyway, whatever you do, whether you cook it, buy it or anything else, it doesn't matter. Just get it for me and I'd appreciate it. - Fleta -");
		SetCancelable(true);

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Basic);

		AddObjective("talk_rab", "Deliver Dog Food to Rab", 0, 0, 0, Talk("rab"));
		AddObjective("talk_fleta", "Talk to Fleta.", 53, 104616, 110159, Talk("fleta"));

		AddReward(Item(52038)); // Fleta Upgrade Coupon for Heavy Armor for 1

		AddHook("_rab", "give_food", RabIntro);
		AddHook("_fleta", "after_intro", FletaIntro);
	}

	public async Task<HookResult> RabIntro(NpcScript npc, params object[] args)
	{
		if (npc.QuestActive(this.Id, "talk_rab") && npc.HasItem(50214))
		{
			Send.Notice(npc.Player, "You have given Bacon and Potato Dog Food to Fleta's Rab.");
			npc.RemoveItem(50214); // Bacon and Potato Dog Food
			npc.Msg(Hide.Both, "(Rab seems happy)");
			npc.Msg("Bark! Bark! Bark!");
			npc.Msg(Hide.Both, "(Rab's devouring the food)");
			npc.Msg("Slurp slurp...");
			npc.Msg(Hide.Both, "(His dish is totally clearned out)");
			npc.Msg("Ruff Ruff!");
			npc.FinishQuest(this.Id, "talk_rab");

			return HookResult.End;
		}

		return HookResult.Continue;
	}
	
	public async Task<HookResult> FletaIntro(NpcScript npc, params object[] args)
	{
		if (!npc.Player.QuestActive(this.Id))
			return HookResult.Continue;

		npc.Player.FinishQuestObjective(this.Id, "talk_fleta");

		return HookResult.Break;
	}
}
