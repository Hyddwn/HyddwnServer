//--- Aura Script -----------------------------------------------------------
// Wings for Teh Win! Event
//--- Description -----------------------------------------------------------
// Based on the 2015 April Fool's Day event
// http://wiki.mabinogiworld.com/view/Wings_for_Teh_Win!_Event
//---------------------------------------------------------------------------

public class AprilFoolsEventScript : GameEventScript
{
	public override void Load()
	{
		SetId("aura_april_fools_event");
		SetName(L("Ferghus's Amazing Wings"));
	}

	public override void AfterLoad()
	{
		ScheduleEvent(DateTime.Parse("2015-04-01 00:00"), DateTime.Parse("2015-04-01 23:59"));
	}
}

public class FerghusRequestQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(400026); //Unofficial Quest ID
		SetName("Ferghus's Request");
		SetDescription("Greetings! I'm working on something really special. Could you spare some time to help? If you decide to help, come to the Blacksmith's Shop in Tir Chonaill today! Just make it here before midnight. I've got plans tonight... - Ferghus");
		SetCancelable(true);

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Event);

		SetReceive(Receive.Automatically);
		AddPrerequisite(EventActive("aura_april_fools_event"));

		AddObjective("talk_ferghus", "Talk to Ferghus at Tir Chonaill's Blacksmith's Shop.", 1, 18075, 29960, Talk("ferghus"));

		AddReward(Exp(401));

		AddHook("_ferghus", "after_intro", TalkFerghus);
	}

	public async Task<HookResult> TalkFerghus(NpcScript npc, params object[] args)
	{
		if (npc.Player.QuestActive(this.Id, "talk_ferghus"))
		{
			npc.Player.FinishQuestObjective(this.Id, "talk_ferghus");

			npc.Msg("<npcportrait name='ferghus_fake'/>Hey there. I'm working on something... I think it might be<br/>my masterpiece. I'll show you soon.");

			return HookResult.End;
		}

		return HookResult.Continue;
	}
}

public class FerghusMasterpieceQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(400027); //Unofficial Quest ID
		SetName("Ferghus's Masterpiece");
		SetDescription("Ferghus is creating a masterpiece for the ages. You won't want to miss the show.");
		SetCancelable(true);

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Event);

		SetReceive(Receive.Automatically);
		AddPrerequisite(And(EventActive("aura_april_fools_event"), Completed(400026)));

		AddObjective("talk_ferghus", "Talk to Ferghus.", 1, 18075, 29960, Talk("ferghus"));
		AddObjective("talk_ferghus2", "Deliver 50 Papers to Ferghus.", 1, 18075, 29960, Talk("ferghus"));
		AddObjective("talk_ferghus3", "Complain to Ferghus while wearing Ferghus's Amazing Wings.", 1, 18075, 29960, Talk("ferghus"));

		AddReward(Exp(222222));

		AddHook("_ferghus", "after_intro", TalkFerghus);
	}

	public async Task<HookResult> TalkFerghus(NpcScript npc, params object[] args)
	{
		if (npc.Player.QuestActive(this.Id, "talk_ferghus"))
		{
			npc.Player.FinishQuestObjective(this.Id, "talk_ferghus");

			npc.Msg("<npcportrait name='ferghus_fake'/>They're gonna start calling me Ferghus the Trend King. These<br/>wings are going to change the world, But that's just a shake of<br/>the hammer for old Ferghus.");
			npc.Msg("<npcportrait name='ferghus_fake'/>I deserve a long night of drinking for this one.");
			npc.Msg("<npcportrait name='ferghus_fake'/>I call these Ferghus's Amazing Wings. I like names to be simple,<br/>you know? These things are going to be flying off the shelves.<br/>That is, if I can get the materials I need to get them made...");
			npc.Msg("<npcportrait name='ferghus_fake'/>Do you think you could help an old blacksmith out? I've got my<br/>hands full here.");
			npc.Msg(Hide.Both, "(Ask him what he needs.)");
			npc.Msg("<npcportrait name='ferghus_fake'/>Paper! Can you believe it? I swear I'd bought about 100 reams<br/>of the stuff, but I woke up a couple of days ago in a haze and I<br/>have no idea what I did with it.");
			npc.Msg("<npcportrait name='ferghus_fake'/>Hopefully Malcolm still has some left over at the General Store...");
			npc.Msg(Hide.Both, "(You try to figure out why he would need that much paper for<br/>wings, but he interrupts your train of thought.");
			npc.Msg("<npcportrait name='ferghus_fake'/>Oh, I see the burning passion in your eyes, You can see that we're<br/>on the cusp of something great here! Just go get me 50 sheets<br/>of paper, and I'll make you something worth talking about.");

			return HookResult.End;
		}

		else if (npc.QuestActive(this.Id, "talk_ferghus2") && npc.HasItem(64018, 50))
		{
			npc.FinishQuest(this.Id, "talk_ferghus2");
			npc.RemoveItem(64018, 50); // Paper
			npc.Msg("<npcportrait name='ferghus_fake'/>It's happening! My masterpiece is coming right up!");
			npc.Msg(Hide.Both, "...");
			npc.Msg("<npcportrait name='ferghus_fake'/>All done! Put those on!");

			npc.Player.GiveItem(19208); // Ferghus's Amazing Wings (Expired after 12 hours in official)
			npc.Player.Notice("You received Ferghus's Amazing Wings.");

			return HookResult.End;
		}

		else if (npc.QuestActive(this.Id, "talk_ferghus3") && npc.Player.HasEquipped("/action_butterfly_big_type/cloth/not_enchantable/not_dyeable/expiring/"))
		{
			npc.FinishQuest(this.Id, "talk_ferghus3");
			npc.Msg(Hide.Both, "(The wings look like they'd fall apart in an instant and the<br/>harness is downright painful, Ferghus notices you shifting<br/>uncomfortably).");
			npc.Msg("<npcportrait name='ferghus_fake'/>Don't you like them? It's just a prototype, you know? I know<br/>they aren't the sturdiest things in the world, but I think they<br/>have a certain charm.");
			npc.Msg("<npcportrait name='ferghus_fake'/>I guess I'll have to go back to the drawing board for now...<br/>Why don't you take a look around, see if there's anything you<br/>want to buy? Maybe I can do some repairs for you.");
			npc.Msg(Hide.Both, "(The wings seem to be simultaneously wrenching your neck out<br/>of place and compressing your spine. You'd better get out of here.)");

			return HookResult.End;
		}

		return HookResult.Continue;
	}
}
