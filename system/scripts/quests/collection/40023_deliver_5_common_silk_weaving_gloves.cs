//--- Aura Script -----------------------------------------------------------
// Deliver 5 Common Silk Weaving Gloves
//--- Description -----------------------------------------------------------
// PTJ reward quest to tailor and deliver Common Silk Weaving Gloves to Simon.
//--- Notes -----------------------------------------------------------------
// Quest details improvised (based off other collection quests).
// Update with official data whenever possible.
//---------------------------------------------------------------------------

public class Deliver5CommonSilkWeavingGlovesQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(40023);
		SetScrollId(70023);
		SetName(L("Deliver 5 Common Silk Weaving Gloves"));
		SetDescription(L("Would you make me [5 Common Silk Weaving Gloves]? When you are finished, come see me for your payment. - Simon -"));
		SetType(QuestType.Collect);

		SetIcon(QuestIcon.Collect);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("create1", L("Make 5 Common Silk Weaving Gloves"), 0, 0, 0, Create(60032, 5, SkillId.Tailoring));
		AddObjective("deliver1", L("Deliver 5 Common Silk Weaving Gloves to Simon"), 0, 0, 0, Deliver(60032, "_simon"));

		AddHook("_simon", "after_intro", AfterIntro);

		AddReward(Exp(750));
		AddReward(Gold(9500));
	}

	public async Task<HookResult> AfterIntro(NpcScript npc, params object[] args)
	{
		if (!npc.QuestActive(this.Id, "deliver1"))
			return HookResult.Continue;

		npc.FinishQuest(this.Id, "deliver1");

		npc.Player.RemoveItem(60032, 5);

		npc.Msg(L("Ah, thank you."));

		return HookResult.Break;
	}
}
