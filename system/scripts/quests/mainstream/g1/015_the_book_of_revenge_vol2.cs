//--- Aura Script -----------------------------------------------------------
// G1 015: The Book of Revenge, Volume II
//--- Description -----------------------------------------------------------
// Giving the translated book to Duncan.
// 
// Wiki:
// - Translate The Book of Revenge, Volume II.
//---------------------------------------------------------------------------

public class BookOfRevengeVol2Quest : QuestScript
{
	private const int BookOfRevenge2 = 73054;
	private const int BookOfRevenge2Translated = 73055;
	private const int OwlDelay1 = 5 * 60;
	private const int OwlDelay2 = 5 * 60;

	public override void Load()
	{
		SetId(210007);
		SetName(L("Receive the Book of Revenge, Vol II"));
		SetDescription(L("I finished the Book if Revenge, Vol. II. I'll be waiting at Dunbarton Church. - Kristell -"));

		SetIcon(QuestIcon.AdventOfTheGoddess);
		SetCategory(QuestCategory.AdventOfTheGoddess);

		AddObjective("get_book", L("Receive the translated copy from Kristell at the Dunbarton Church."), 14, 34657, 42808, Talk("kristell"));
		AddObjective("give_book", L("Show Duncan the translated copy."), 1, 15409, 38310, Talk("duncan"));
		AddObjective("talk_aeira", L("Perhaps Aeira knows where the third volume is."), 14, 44978, 43143, Talk("aeira"));

		AddReward(Exp(450));
		AddReward(Gold(170));

		AddHook("_kristell", "after_intro", KristellAfterIntro);
		AddHook("_duncan", "after_intro", DuncanAfterIntro);
		AddHook("_aeira", "after_intro", AeiraAfterIntro);
	}

	public async Task<HookResult> KristellAfterIntro(NpcScript npc, params object[] args)
	{
		if (npc.HasKeyword("g1_25") && npc.HasItem(BookOfRevenge2))
		{
			npc.RemoveItem(BookOfRevenge2);
			npc.SendOwl(this.Id, OwlDelay1);

			npc.RemoveKeyword("g1_memo_of_parcelman");
			npc.RemoveKeyword("g1_25");
			npc.GiveKeyword("g1_26");

			npc.Msg(L("So there really was another volume.<br/>I'm impressed. I didn't think you'd be able to find it."));
			npc.Msg(L("I'll translate this book, as promised.<br/>I'll let you know as soon as I'm finished."));

			return HookResult.Break;
		}
		else if (npc.QuestActive(this.Id, "get_book"))
		{
			npc.FinishQuest(this.Id, "get_book");

			npc.GiveItem(BookOfRevenge2Translated);
			npc.Notice(L("You have received the Book of Revenge, Vol. 2 (Translated) from Kristell."));

			npc.Msg(L("You must be here for your translated copy of the book.<br/>Here, I think you should read it yourself.<br/>It's better than having me summarize it for you."));
			npc.Msg(L("That said, I can't believe what's written in this book..."));

			return HookResult.Break;
		}

		return HookResult.Continue;
	}

	public async Task<HookResult> DuncanAfterIntro(NpcScript npc, params object[] args)
	{
		if (npc.QuestActive(this.Id, "give_book"))
		{
			npc.FinishQuest(this.Id, "give_book");

			npc.RemoveItem(BookOfRevenge2Translated);
			npc.Notice(L("You have given the Book of Revenge, Vol. 2 (Translated) to Duncan."));

			npc.Msg(L("Good job.<br/>You not only found the book but even got it translated...<br/>Here, let me see it."));
			npc.Msg(Hide.Name, L("(Duncan starts reading the book.)"));
			npc.Msg(L("Hmm... Hah..."));
			npc.Msg(L("So, it's true.<br/>So that's why the Fomor Scroll was made."));
			npc.Msg(L("The Fomors...recognized that humans and nature<br/>were becoming separated..."));
			npc.Msg(L("At this point, I think you'll have to find the last volume of the book.<br/>If what I think is true, all of their plans will be in that book.<br/>Ever since the Mag Tuireadh war,<br/>Fomors have been preparing for another war."));
			npc.Msg(L("We don't know anything for sure yet, but<br/>if we don't find their plan and prevent it,<br/>Erinn might be destroyed..."));
			npc.Msg(L("We have to find the 3rd volume.<br/>Please... You have to find the last book of the Fomors."));
			npc.Msg(L("And don't forget that<br/>you must not tell anyone<br/>about these things regarding the Goddess."));

			return HookResult.Break;
		}

		return HookResult.Continue;
	}

	public async Task<HookResult> AeiraAfterIntro(NpcScript npc, params object[] args)
	{
		if (npc.QuestActive(this.Id, "talk_aeira"))
		{
			npc.CompleteQuest(this.Id);
			npc.SendOwl(210008, OwlDelay2); // Find the Book of Revenge, Vol. III

			npc.Msg(L("Since you're looking for volume 3, I'm guessing you already<br/>found volume 2? I'm afraid I haven't heard anything about volume 3 just yet."));
			npc.Msg(L("It's turning out to be a lot harder to find the last volume.<br/>My distributor has been absolutely useless, and Eavan says<br/>she hasn't come across such a book. Even Leslie says she<br/>doesn't know anything. Same with Stewart."));
			npc.Msg(L("I've asked everywhere, but I don't know how long it will take.<br/>I'm afraid you're going to have to be patient. But even if<br/>you're patient, I just don't have any confidence that I'll<br/>be able to find it..."));

			return HookResult.Break;
		}

		return HookResult.Continue;
	}
}
