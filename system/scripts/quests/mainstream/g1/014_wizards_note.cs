//--- Aura Script -----------------------------------------------------------
// G1 014: Wizard's Note
//--- Description -----------------------------------------------------------
// Receive The Book of Revenge Vol 2 by talking to various NPCs.
// 
// See "g1_21_tircho_ciar_dungeon" for the dungeon part of the quest.
// 
// Wiki:
// - Obtain The Book of Revenge, Vol. II
//---------------------------------------------------------------------------

public class WizardsNoteQuest : QuestScript
{
	private const int BookOfRevenge = 73062;
	private const int OwlDelay = 5 * 60;

	public override void Load()
	{
		SetId(210005);
		SetName(L("Get the Book of Revenge back"));
		SetDescription(L("I'll have to read the book Mores wrote. Can you get it from Tarlach for me? - Duncan -"));

		SetIcon(QuestIcon.AdventOfTheGoddess);
		SetCategory(QuestCategory.AdventOfTheGoddess);

		AddObjective("get_book", L("Get the Book of Revenge back from Tarlach."), 48, 11100, 30400, Talk("tarlach"));
		AddObjective("give_book", L("Show Duncan the Book of Revenge."), 1, 15409, 38310, Talk("duncan"));
		AddObjective("talk_kristell", L("Ask Kristell about the book that follows the Book of Revenge."), 14, 34657, 42808, Talk("kristell"));
		AddObjective("talk_aeira", L("Meet with Aeira."), 14, 44978, 43143, Talk("aeira"));

		AddReward(Exp(350));
		AddReward(Gold(126));

		AddHook("_duncan", "before_keywords", DuncanBeforeKeywords);
		AddHook("_tarlach", "after_intro", TarlachAfterIntro);
		AddHook("_duncan", "after_intro", DuncanAfterIntro);
		AddHook("_kristell", "after_intro", KristellAfterIntro);
		AddHook("_aeira", "after_intro", AeiraAfterIntro);
	}

	public async Task<HookResult> DuncanBeforeKeywords(NpcScript npc, params object[] args)
	{
		var keyword = args[0] as string;

		if (keyword == "g1_goddess_morrighan1")
		{
			npc.StartQuest(this.Id);

			npc.RemoveKeyword("g1_goddess_morrighan1");
			npc.RemoveKeyword("g1_22");
			npc.GiveKeyword("g1_23");

			npc.Msg(L("...<p/>It's hard to believe that such a thing could have happened<br/>to Mores. Is he still alive?"));
			npc.Msg(L("And...this is the first time I've heard<br/>how much he hated humans.<br/>Please, don't speak of this to anyone else.<br/>It's better he's remembered as a hero."));
			npc.Msg(L("Besides, we may have gotten something wrong.<br/>I'd like to investigate this more on my own.<br/>And I'll have to read the book Mores wrote.<br/>Can you get it from Tarlach for me?"));

			return HookResult.Break;
		}

		return HookResult.Continue;
	}

	public async Task<HookResult> TarlachAfterIntro(NpcScript npc, params object[] args)
	{
		if (npc.QuestActive(this.Id, "get_book"))
		{
			npc.FinishQuest(this.Id, "get_book");

			npc.GiveItem(BookOfRevenge);
			npc.Notice(L("You have received the Book of Revenge from Tarlach."));

			npc.Msg(L("Thank you for sharing the story of my mentor with me, <username/>..."));
			npc.Msg(L("...<p/>Um... are you telling me that Chief Duncan of Tir Chonaill<br/>wants to...borrow the master's book...?"));
			npc.Msg(L("...I guess it should be okay."));
			npc.Msg(L("He is a wise old man who has been through a lot, and<br/>he may be able to catch some things that we may have missed..."));
			npc.Msg(L("Please give this copy to the Chief for me..."));

			return HookResult.Break;
		}

		return HookResult.Continue;
	}

	public async Task<HookResult> DuncanAfterIntro(NpcScript npc, params object[] args)
	{
		if (npc.QuestActive(this.Id, "give_book"))
		{
			npc.FinishQuest(this.Id, "give_book");

			npc.RemoveItem(BookOfRevenge);
			npc.Notice(L("You have given the Book of Revenge to Duncan."));

			npc.Msg(L("So this is the Book of Revenge that Mores wrote...<br/>Let me take a look at it."));
			npc.Msg(L("Hmm...interesting.<br/>I see. Hmm.<br/>Mmm..."));
			npc.Msg(L("...? Is this the end...?<br/>That can't be..."));
			npc.Msg(L("<username/>, it doesn't seem like the book ends here.<br/>Judging by the book's writing style and structure, it seems like there should be more to it.<br/>Looking at the introduction of the book, it seems like a 3 part series..."));
			npc.Msg(L("It's hard to comment on the Goddess's intentions<br/>just from this part of the book.<br/>Could you find out if there is another book...?"));
			npc.Msg(L("If we can get a hold of the other volumes,<br/>we will be able to figure out how to best respond to Mores and the Goddess,<br/>and what exactly to say to the King of Aliech."));
			npc.Msg(L("I am sure you are aware of this, as well...<br/>that it's difficult to get the attention of the King and the lords<br/>with something as vague as this..."));
			npc.Msg(L("I don't know where you found the translation of this book...<br/>but please inquire about the existence of the next volume. Thank you."));

			return HookResult.Break;
		}

		return HookResult.Continue;
	}

	public async Task<HookResult> KristellAfterIntro(NpcScript npc, params object[] args)
	{
		if (npc.QuestActive(this.Id, "talk_kristell"))
		{
			npc.FinishQuest(this.Id, "talk_kristell");

			npc.Msg(L("Is there another volume to this book...?<br/>I'm not sure.<br/>Based on the introduction, it seems likely..."));
			npc.Msg(L("I've learned that this book is very popular amongst high-ranking Fomors.<br/>I only found that out while I was translating the book."));
			npc.Msg(L("Sorry I'm not much help<br/>Maybe Aeira knows something about this.<br/>When it comes to books, no one's more knowledgable than her."));

			return HookResult.Break;
		}

		return HookResult.Continue;
	}

	public async Task<HookResult> AeiraAfterIntro(NpcScript npc, params object[] args)
	{
		if (npc.QuestActive(this.Id, "talk_aeira"))
		{
			npc.CompleteQuest(this.Id);
			npc.SendOwl(210006, OwlDelay); // The Book of Revenge, Volume II

			npc.RemoveKeyword("g1_23");
			npc.GiveKeyword("g1_24");

			npc.Msg(L("'The Book of Revenge...?'<br/>I don't think we have a book by that title.<br/>I'm certain our distributor doesn't carry it, either.<br/>Trust me, I know all the books we carry here..."));
			npc.Msg(L("It's a 3-volume set? If you've already got the first<br/>volume, then I can't sell it to you as a set, either...<br/>I doubt they'll let me order each volume separately."));
			npc.Msg(L("It's written in the Fomor language?!<br/>I'm afraid you'll have to look elsewhere for that.<br/>It's going to be hard for a human bookseller to get such<br/>a book, don't you think?"));
			npc.Msg(L("<username/>, was it? You have quite the unique taste in books...<br/>I'll tell you what. You've piqued my interest, and I know<br/>a number of explorers and scholars. I'll ask around.<br/>If I hear anything, I'll get in touch with you."));

			return HookResult.Break;
		}

		return HookResult.Continue;
	}
}

public class WizardsNote2Quest : QuestScript
{
	private const int WizardsNote = 73024;

	public override void Load()
	{
		SetId(210006);
		SetName(L("The Book of Revenge, Volume II"));
		SetDescription(L("I found how to get the Book of Revenge Volume II. Please come by the Dunbarton Bookstore. - Aeira -"));

		SetIcon(QuestIcon.AdventOfTheGoddess);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.AdventOfTheGoddess);

		AddObjective("talk_aeira", L("Meet Aeira at the Dunbarton Bookstore."), 14, 44978, 43143, Talk("aeira"));

		AddReward(Exp(151));
		AddReward(Item(WizardsNote));

		AddHook("_aeira", "after_intro", AeiraAfterIntro);
		AddHook("_aeira", "before_keywords", AeiraBeforeKeywords);
	}

	public async Task<HookResult> AeiraAfterIntro(NpcScript npc, params object[] args)
	{
		if (npc.QuestActive(this.Id, "talk_aeira"))
		{
			npc.CompleteQuest(this.Id);

			npc.RemoveKeyword("g1_24");
			npc.GiveKeyword("g1_25");
			npc.GiveKeyword("g1_memo_of_parcelman");

			npc.GiveWarpScroll(63009, "ciar_dungeon");

			npc.Msg(L("Ah! <username/>! Welcome. Do you remember the book I gave you before, 'Land of Eternity, Tir Na Nog?'<br/>The author of that book, Leslie, called.<br/>You see, Leslie is a famous historian and an avide explorer."));
			npc.Msg(L("She sent me this note.<br/>It says that she once found 'The Book of Revenge' inside Ciar dungeon, written in Fomor language.<br/>She said if you offer this note on the Altar of Ciar Dungeon, you'll be transported to where you can find the book.<br/>...Apparently this note has some kind of magic on it or something..."), npc.Image("g1_ch21_memo"));
			npc.Msg(L("And...here, this is Aeira's special gift to you, The Red Wing of the Goddess!<br/>I will give you the note and the Red Wing of the Goddess!<br/>I wish you the best of luck."));

			return HookResult.Break;
		}

		return HookResult.Continue;
	}

	public async Task<HookResult> AeiraBeforeKeywords(NpcScript npc, params object[] args)
	{
		var keyword = args[0] as string;

		if (keyword == "g1_memo_of_parcelman")
		{
			if (npc.HasItem(WizardsNote))
				npc.GiveItem(WizardsNote);

			npc.Msg(L("If you offer the note on the Altar of Ciar Dungeon, you'll be transported to where you can find the book."));

			return HookResult.Break;
		}

		return HookResult.Continue;
	}
}
