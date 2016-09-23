//--- Aura Script -----------------------------------------------------------
// Deliver 1 Mongo's Hat
//--- Description -----------------------------------------------------------
// PTJ reward quest to tailor and deliver a Mongo's Hat to Simon.
//--- Notes -----------------------------------------------------------------
// Quest details improvised (based off other collection quests).
// Update with official data whenever possible.
//---------------------------------------------------------------------------

public class Deliver1MongosHatQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(40025);
		SetScrollId(70023);
		SetName(L("Deliver 1 Mongo's Hat"));
		SetDescription(L("Would you make me [1 Mongo's Hat]? When you are finished, come see me for your payment. - Simon -"));
		SetType(QuestType.Collect);

		SetIcon(QuestIcon.Collect);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("create1", L("Make 1 Mongo's Hat"), 0, 0, 0, Create(18014, 1, SkillId.Tailoring));
		AddObjective("deliver1", L("Deliver 1 Mongo's Hat to Simon"), 0, 0, 0, Deliver(18014, "_simon"));

		AddHook("_simon", "after_intro", AfterIntro);

		AddReward(Exp(1200));
		AddReward(Gold(14000));
	}

	public async Task<HookResult> AfterIntro(NpcScript npc, params object[] args)
	{
		if (!npc.QuestActive(this.Id, "deliver1"))
			return HookResult.Continue;

		npc.FinishQuest(this.Id, "deliver1");

		npc.Player.RemoveItem(18014, 1);

		npc.Msg(L("Ah, thank you."));

		return HookResult.Break;
	}
}
