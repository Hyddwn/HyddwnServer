//--- Aura Script -----------------------------------------------------------
// Loggin Camp at Dugald Aisle
//--- Description -----------------------------------------------------------
// Started automatically after finishing the quest Repairing an Item.
// Tracy asks the player to get him some firewood.
//---------------------------------------------------------------------------

public class TracyLoggingCampQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(202041);
		SetName("Loggin Camp at Dugald Aisle");
		SetDescription("Hey~ I'm Tracy, the sexiest guy in Tir Chonaill~ I'm short on hands here, so if you can, please drop by the Logging Camp of Dugald Aisle! All you need to do is cut down a liil bit of trees. Pleeeease help me~ Just head Southwest from Tir Chonaill. - Tracy -");

		SetReceive(Receive.Automatically);
		AddPrerequisite(Completed(202042)); // Repairing an Item

		AddObjective("talk", "Talk with Tracy", 16, 22900, 59500, Talk("tracy"));
		AddObjective("deliver", "Deliver 6 Pieces of Firewood to Tracy", 16, 22900, 59500, Talk("tracy"));

		AddReward(Exp(3500));
		AddReward(Gold(500));
		AddReward(Item(1603)); // Part-Time Jobs, Moon Gate Guidebook
		AddReward(QuestScroll(71021)); // Collect the Brown Fox's Fomor Scrolls

		AddHook("_tracy", "after_intro", AfterIntro);
	}

	public async Task<HookResult> AfterIntro(NpcScript npc, params object[] args)
	{
		if (npc.QuestActive(this.Id, "talk"))
		{
			npc.FinishQuest(this.Id, "talk");

			npc.AcquireItem(40022); // Gathering Axe
			npc.Msg("(Missing dialog: Explanation on getting firewood");

			return HookResult.Break;
		}
		else if (npc.QuestActive(this.Id, "deliver") && npc.HasItem(63002, 6))
		{
			npc.FinishQuest(this.Id, "deliver");

			npc.RemoveItem(63002, 6);
			npc.Msg("(Missing dialog: Appreciation for getting firewood");

			return HookResult.Break;
		}

		return HookResult.Continue;
	}
}
