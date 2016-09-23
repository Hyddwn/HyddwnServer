//--- Aura Script -----------------------------------------------------------
// G1 005: The Land of Eternity, Tir Na Nog
//--- Description -----------------------------------------------------------
// Getting the book from Aeira and the Brown Evil Pass from Tarlach.
// 
// Part of Aeira's dialog structure was guessed, as I wasn't able to find
// and old enough video of this quest. It's not based on current dialog and
// how the process should be according to the Wiki.
// 
// Wiki:
// - Instruction: Obtain the book The Land of Eternity, Tir Na Nog.
//---------------------------------------------------------------------------

public class TheLandOfEternityQuest : QuestScript
{
	private const int Book = 73051;
	private const int BrownFomorPass = 73011;
	private const int OwlDelay = 5 * 60;

	public override void Load()
	{
		SetId(210002);
		SetName(L("The Land of Eternity, Tir Na Nog"));
		SetDescription(L("Ask Aeira in the Dunbarton Bookstore about the book."));

		SetIcon(QuestIcon.AdventOfTheGoddess);
		SetCategory(QuestCategory.AdventOfTheGoddess);

		AddObjective("get_book", L("Speak to Aeira at the Dunbarton Bookstore."), 14, 44978, 43143, Talk("aeira"));

		AddReward(Exp(220));

		AddHook("_aeira", "before_keywords", AeiraBeforeKeywords);
		AddHook("_aeira", "after_intro", AeiraAfterIntro);
		AddHook("_tarlach", "before_keywords", TarlachBeforeKeywords);
	}

	public async Task<HookResult> AeiraBeforeKeywords(NpcScript npc, params object[] args)
	{
		var keyword = args[0] as string;
		if (keyword != "g1_book1")
			return HookResult.Continue;

		if (npc.HasKeyword("g1_06"))
		{
			npc.RemoveKeyword("g1_06");
			npc.GiveKeyword("g1_07");

			npc.Msg(L("'The Land of Eternity, Tir Na Nog'...?"));
			npc.Msg(L("Oh no.<br/>That book wasn't selling at all, so I returned all of them.<br/>Haha! Where did you hear about that book?"));
		}
		else if (npc.HasKeyword("g1_07"))
		{
			npc.RemoveKeyword("g1_07");
			npc.GiveKeyword("g1_08");

			npc.Msg(L("Let's see... I can order one for you,<br/>but it'll take some time to arrive.<br/>I hope that's okay."));
		}
		else if (npc.HasKeyword("g1_08"))
		{
			npc.RemoveKeyword("g1_book1");
			npc.SendOwl(this.Id, OwlDelay);

			npc.Msg(L("I'm sorry!<br/>The book still isn't in stock."));
			npc.Msg(L("Hmm, how about this. I'll send you an owl when the book arrives.<br/>That would be better, right?"));
		}

		return HookResult.Break;
	}

	public async Task<HookResult> AeiraAfterIntro(NpcScript npc, params object[] args)
	{
		if (!npc.QuestActive(this.Id, "get_book"))
			return HookResult.Continue;

		npc.CompleteQuest(this.Id);
		npc.GiveItem(Book);

		npc.RemoveKeyword("g1_06");
		npc.RemoveKeyword("g1_07");
		npc.RemoveKeyword("g1_08");
		npc.GiveKeyword("g1_09");

		npc.Msg(L("Hey~! You came, <username/>.<br/>Here it is. The book you have been looking for."));
		npc.Msg(L("Sorry it took so long~<br/>To make it up to you, I'll give you the book for free~"));
		npc.Msg(L("Instead, just come and visit often, okay?"));

		return HookResult.Break;
	}

	public async Task<HookResult> TarlachBeforeKeywords(NpcScript npc, params object[] args)
	{
		var keyword = args[0] as string;
		if (keyword != "g1_paradise")
			return HookResult.Continue;

		if (npc.HasKeyword("g1_09"))
		{
			npc.RemoveKeyword("g1_09");
			npc.GiveKeyword("g1_10");

			npc.GiveItem(BrownFomorPass);
			npc.GiveWarpScroll(63009, "barri_dungeon");

			npc.Msg(L("...Did you actually read the book?<br/>You still want to go...?<br/>Just as I'd thought...<br/>..."));
			npc.Msg(L("...Honestly, I was hoping that<br/>reading the book would be enough to satisfy your curiosity and that you would forget all about it...<br/>But, you can't help that you're an adventurer..."));
			npc.Msg(L("...<p/>...<br/>Tir Na Nog... is not paradise.<br/>What's in that book is actually a lie.<br/>...Tir Na Nog is a real place, but it's not anything like paradise."));
			npc.Msg(L("...<p/>Listen carefully... Tir Na Nog is the land of the Fomors.<br/>And... the Goddess,<br/>who seems so loving, draws zealous adventurers like yourself to manipulate and use you."));
			npc.Msg(L("...<p/>...You don't believe me, do you...? Hah...<br/>The pass I just gave you is what the Fomors use to enter dungeons...<br/>It's an item they use to avoid the barriers set up by the Goddess."), npc.Image("g1_ch09_brownpass"));
			npc.Msg(L("If you offer that to the statue of the Goddess in Barri dungeon,<br/>you will witness everything I just told you with your own eyes.<br/>Here's a Red Wing of the Goddess, so use it if you need it...<br/>You can go there alone, but taking one or two friends with you is probably a better idea."));
			npc.Msg(L("...The best thing to do would be to forget<br/>trying to go to Tir Na Nog.<p/>...Heed my warning..."));
		}
		else if (npc.HasKeyword("g1_10"))
		{
			if (!npc.HasItem(BrownFomorPass))
				npc.GiveItem(BrownFomorPass);

			npc.Msg(L("...The best thing to do would be to forget<br/>trying to go to Tir Na Nog.<p/>...Heed my warning..."));
		}

		return HookResult.Break;
	}
}
