//--- Aura Script -----------------------------------------------------------
// Rab's Dog Food [Shrimp Dog Food]
//--- Description -----------------------------------------------------------
// Deliver Dog food to Rab. Obtained by giving Fleta Rab's empty plate
// Rab's empty plate is earned by talking to Rab while logged on as a pet
//---------------------------------------------------------------------------

public class RabFood01QuestScript : QuestScript
{
	public override void Load()
	{
		SetId(60041);
		SetName("Rab's Dog Food");
		SetDescription("Rab looks like he wants some Shrimp Dog Food. Yea, a dog that likes shrimp... Shrimp Dog Food is all about just mixing in shrimp, rice, and a bit of salt. You can buy the ingredients in Dunbarton. You don't really have to make it, so just get me the dish if you can. - Fleta -");
		SetCancelable(true);

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Basic);

		AddObjective("talk_rab", "Deliver Shrimp Dog Food to Rab", 0, 0, 0, Talk("rab"));
		AddObjective("talk_fleta", "Talk to Fleta.", 53, 104616, 110159, Talk("fleta"));

		AddReward(Item(52038)); // Fleta Upgrade Coupon for Heavy Armor for 1

		AddHook("_rab", "give_food", RabIntro);
		AddHook("_fleta", "after_intro", FletaIntro);
	}

	public async Task<HookResult> RabIntro(NpcScript npc, params object[] args)
	{
		if (npc.QuestActive(this.Id, "talk_rab") && npc.HasItem(50215))
		{
			Send.Notice(npc.Player, "You have given Shrimp Dog Food to Fleta's Rab.");
			npc.RemoveItem(50215); // Shrimp dog food
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
		if (!npc.QuestActive(this.Id, "talk_fleta"))
			return HookResult.Continue;

		npc.FinishQuest(this.Id, "talk_fleta");

		return HookResult.Break;
	}
}
