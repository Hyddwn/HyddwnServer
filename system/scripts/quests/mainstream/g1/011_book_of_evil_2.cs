//--- Aura Script -----------------------------------------------------------
// G1 011: Book Of Evil Part 2 - Reloaded
//--- Description -----------------------------------------------------------
// Getting Black Rose from Lassar and bringing it to Tarlach and then to
// Kristell.
// 
// Wiki:
// - Give the Black Rose to Kristell.
//---------------------------------------------------------------------------

public class BookOfEvilPart2Quest : QuestScript
{
	private const int BookOfRevenge = 73053;

	public override void Load()
	{
		SetId(210023);
		SetName(L("Reveive the Reqested Object"));
		SetDescription(L("The favor you have asked for before went well. I will give it to you when you visit. - Lassar -"));

		SetIcon(QuestIcon.AdventOfTheGoddess);
		SetCategory(QuestCategory.AdventOfTheGoddess);

		AddObjective("get_item", L("Receive the item from Lassar."), 9, 2020, 1537, Talk("lassar"));
		AddObjective("talk_tarlach", L("Talk to Tarlach."), 48, 11100, 30400, Talk("tarlach"));
		AddObjective("deliver_item", L("Pass the item to Kristell."), 14, 34657, 42808, Talk("kristell"));

		AddReward(Exp(265));
		AddReward(Gold(250));
		AddReward(Item(BookOfRevenge));

		AddHook("_lassar", "after_intro", LassarAfterIntro);
		AddHook("_tarlach", "after_intro", TarlachAfterIntro);
		AddHook("_kristell", "after_intro", KristellAfterIntro);
	}

	public async Task<HookResult> LassarAfterIntro(NpcScript npc, params object[] args)
	{
		if (!npc.QuestActive(this.Id, "get_item"))
			return HookResult.Continue;

		npc.FinishQuest(this.Id, "get_item");

		npc.Msg(L("Here... This is the item Priest Meven told me to give to you."));
		npc.Msg(L("Isn't it beautiful? It's a black rose.<br/>The overwhelming redness of the flower itself turned the flower pitch black...<br/>I am happy it grew so beautifully compared to other flowers..."), npc.Image("g1_ch16_blackrose"));
		npc.Msg(L("But... what's the reason you're looking for such a rare flower?<br/>Are you... going to give it to your lover? Ha ha..."));

		return HookResult.Break;
	}

	public async Task<HookResult> TarlachAfterIntro(NpcScript npc, params object[] args)
	{
		if (!npc.QuestActive(this.Id, "talk_tarlach"))
			return HookResult.Continue;

		npc.FinishQuest(this.Id, "talk_tarlach");

		npc.Msg(L("...!<br/>Yes... that's it.<br/>The black rose I have been looking for..."));
		npc.Msg(L("No... it's different. This...<br/>is a new flower..."));
		npc.Msg(L("Thank you... <username/>...<br/>for helping me..."));
		npc.Msg(L("Then... please do me one more favor...<br/>Can you... deliver this rose to Kristell of Dunbarton?"));
		npc.Msg(L("...That would be all... thanks."));

		return HookResult.Break;
	}

	public async Task<HookResult> KristellAfterIntro(NpcScript npc, params object[] args)
	{
		if (!npc.QuestActive(this.Id, "deliver_item"))
			return HookResult.Continue;

		npc.CompleteQuest(this.Id);
		npc.RemoveKeyword("g1_17_5");
		npc.GiveKeyword("g1_18");

		npc.Msg(L("This is!"));
		npc.Msg(L("Tarlach......"));
		npc.Msg(Hide.Name, L("(Tears flow from Kristell's eyes.)"));
		npc.Msg(L("Tarlach..."));
		npc.Msg(L("He still remembers the song...<br/>The song of the black rose... the song I sang for him...<br/>Does this mean all this time, Tarlach had been struggling just like me?"));
		npc.Msg(L("Thank you. <username/>...<br/>...Thank you..."));
		npc.Msg(L("If it weren't for you,<br/>I'd still be wondering why Tarlach would be like that..."));
		npc.Msg(L("Here is the translated book... <username/>...<br/>I apologize for my rudeness..."));
		npc.Msg(L("There were some Fomor text writting in the back of the book which<br/>seems like it has no connection to the contents of the book...<br/>But I translated it just in case."));
		npc.Msg(L("I hope this helps..."));

		return HookResult.Break;
	}
}
