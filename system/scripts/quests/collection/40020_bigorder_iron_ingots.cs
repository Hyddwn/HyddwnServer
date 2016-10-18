//--- Aura Script -----------------------------------------------------------
// Big Order of Iron Ingots
//--- Description -----------------------------------------------------------
// PTJ reward quest to refine a lot of iron ingots.
//--- Notes -----------------------------------------------------------------
// Quest details improvised (based off other collection quests).
// Update with official data whenever possible.
//---------------------------------------------------------------------------

public class BigOrderIronIngotsQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(40020);
		SetScrollId(70023);
		SetName(L("Big Order of Iron Ingots"));
		SetDescription(L("This job is to refine iron ore into iron ingots. Today, refine [30 Lumps of Iron Ingots] and deliver them to Elen at the blacksmith shop."));
		SetType(QuestType.Collect);

		SetIcon(QuestIcon.Collect);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("create1", L("Refine 30 lumps of iron ingots."), 0, 0, 0, Create(64001, 30, SkillId.Refining));
		AddObjective("deliver1", L("Deliver 30 lumps of iron ingots to Elen."), 0, 0, 0, Talk("_elen"));

		AddHook("_elen", "after_intro", AfterIntro);

		AddReward(Exp(1200));
		AddReward(Gold(15000));
	}

	public async Task<HookResult> AfterIntro(NpcScript npc, params object[] args)
	{
		if (!npc.QuestActive(this.Id, "deliver1"))
			return HookResult.Continue;

		if (!npc.Player.Inventory.Has(64001, 30))
			return HookResult.Continue;

		npc.FinishQuest(this.Id, "deliver1");

		npc.Player.RemoveItem(64001, 30);

		npc.Msg(L("Thanks for the hard work."));

		return HookResult.Break;
	}
}
