//--- Aura Script -----------------------------------------------------------
// Deliver 2 Cores' Healer Gloves
//--- Description -----------------------------------------------------------
// PTJ reward quest to tailor and deliver Cores' Healer Gloves to Simon.
//--- Notes -----------------------------------------------------------------
// Quest details improvised (based off other collection quests).
// Update with official data whenever possible.
//---------------------------------------------------------------------------

public class Deliver2CoresHealerGlovesQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(40024);
		SetScrollId(70023);
		SetName(L("Deliver 2 Cores' Healer Gloves"));
		SetDescription(L("Would you make me [2 Cores' Healer Gloves]? When you are finished, come see me for your payment. - Simon -"));
		SetType(QuestType.Collect);

		SetIcon(QuestIcon.Collect);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("create1", L("Make 2 Cores' Healer Gloves"), 0, 0, 0, Create(16011, 2, SkillId.Tailoring));
		AddObjective("deliver1", L("Deliver 2 Cores' Healer Gloves to Simon"), 0, 0, 0, Deliver(16011, "_simon"));

		AddHook("_simon", "after_intro", AfterIntro);

		AddReward(Exp(900));
		AddReward(Gold(13000));
	}

	public async Task<HookResult> AfterIntro(NpcScript npc, params object[] args)
	{
		if (!npc.QuestActive(this.Id, "deliver1"))
			return HookResult.Continue;

		npc.FinishQuest(this.Id, "deliver1");

		npc.Player.RemoveItem(16011, 2);

		npc.Msg(L("Ah, thank you."));

		return HookResult.Break;
	}
}
