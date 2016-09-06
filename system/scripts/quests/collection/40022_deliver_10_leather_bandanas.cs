//--- Aura Script -----------------------------------------------------------
// Deliver 10 Leather Bandanas
//--- Description -----------------------------------------------------------
// PTJ reward quest to tailor and deliver Leather Bandanas to Simon.
//--- Notes -----------------------------------------------------------------
// Quest details improvised (based off other collection quests).
// Update with official data whenever possible.
//---------------------------------------------------------------------------

public class Deliver10LeatherBandanasQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(40022);
		SetScrollId(70023);
		SetName(L("Deliver 10 Leather Bandanas"));
		SetDescription(L("Would you make me [10 Leather Bandanas]? When you are finished, come see me for your payment. - Simon -"));
		SetType(QuestType.Collect);

		SetIcon(QuestIcon.Collect);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("create1", L("Make 10 Leather Bandanas"), 0, 0, 0, Create(18022, 10, SkillId.Tailoring));
		AddObjective("deliver1", L("Deliver 10 Leather Bandanas to Simon"), 0, 0, 0, Deliver(18022, "_simon"));

		AddHook("_simon", "after_intro", AfterIntro);

		AddReward(Exp(600));
		AddReward(Gold(8000));
	}

	public async Task<HookResult> AfterIntro(NpcScript npc, params object[] args)
	{
		if (!npc.QuestActive(this.Id, "deliver1"))
			return HookResult.Continue;

		npc.FinishQuest(this.Id, "deliver1");

		npc.Player.RemoveItem(18022, 10);

		npc.Msg(L("Ah, thank you."));

		return HookResult.Break;
	}
}
