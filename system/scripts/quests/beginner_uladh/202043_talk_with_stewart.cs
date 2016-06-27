//--- Aura Script -----------------------------------------------------------
// Talk with Stewart
//--- Description -----------------------------------------------------------
// Introduction to Mana with Stewart?
//---------------------------------------------------------------------------

public class TalkWithStewartQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(202043);
		SetName("Talk with Stewart");
		SetDescription("I am Stewart, teaching magic at Dunbarton school. Therefore... I'm a teacher. Come visit me without any pressure. I will give you something that will be helpful. - Stewart -");

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Tutorial);

		SetReceive(Receive.Automatically);
		AddPrerequisite(Completed(202041)); // Loggin Camp at Dugald Aisle

		AddObjective("talk", "Talk with Stewart", 18, 2671, 1771, Talk("stewart"));

		AddReward(Exp(4000));
		AddReward(Item(51007, 5)); // MP 30 Potion

		AddHook("_stewart", "after_intro", AfterIntro);
	}

	public async Task<HookResult> AfterIntro(NpcScript npc, params object[] args)
	{
		if (npc.QuestActive(this.Id, "talk"))
		{
			npc.FinishQuest(this.Id, "talk");

			npc.Msg(L("Welcome. I am Stewart.<br/>I will give you a mana potion that will help you with your magic training studies.<br/>I think you have talent in magic."));
			npc.Msg(L("I'd be thankful if you had interest in magic."));

			return HookResult.Break;
		}

		return HookResult.Continue;
	}
}
