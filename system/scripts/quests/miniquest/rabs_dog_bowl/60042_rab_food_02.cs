//--- Aura Script -----------------------------------------------------------
// Rab's Dog Food [Dog Biscuit]
//--- Description -----------------------------------------------------------
// Deliver Dog food to Rab. Obtained by giving Fleta Rab's empty plate
// Rab's empty plate is earned by talking to Rab while logged on as a pet
//---------------------------------------------------------------------------

public class RabFood02QuestScript : QuestScript
{
	public override void Load()
	{
		SetId(60042);
		SetName("Rab's Dog Food");
		SetDescription("Rab looks like he wants some Dog Biscuit. Seriously.. is he spoiled or what... Anyway, Dog Biscuit can be made by mixing butter biscuit and chocolatechip cookies. I think they sell those ingredients in Emain Macha. You can make it, or you can ask someone else to make it, or whatever. If you can just get it for me, I'd appreciate it. - Fleta -");
		SetCancelable(true);

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Basic);

		AddObjective("talk_rab", "Deliver Dog Biscuit to Rab", 0, 0, 0, Talk("rab"));
		AddObjective("talk_fleta", "Talk to Fleta.", 53, 104616, 110159, Talk("fleta"));

		AddReward(Item(52038)); // Fleta Upgrade Coupon for Heavy Armor for 1

		AddHook("_rab", "give_food", RabIntro);
		AddHook("_fleta", "after_intro", FletaIntro);
	}

	public async Task<HookResult> RabIntro(NpcScript npc, params object[] args)
	{
		if (npc.QuestActive(this.Id, "talk_rab") && npc.HasItem(50213))
		{
			Send.Notice(npc.Player, "You have given Dog Biscuit to Fleta's Rab.");
			npc.RemoveItem(50213); // Dog Biscuit
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
