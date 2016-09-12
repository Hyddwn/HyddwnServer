//--- Aura Script -----------------------------------------------------------
// G1 017: Book of the Revenge, Volume III
//--- Description -----------------------------------------------------------
// (Not an actual quest.)
// 
// Learning about Glas Ghaibhleann from the book, Duncan, and Bryce.
// 
// Wiki:
// - Investigate the content of the book.
//---------------------------------------------------------------------------

public class BookOfRevengeVol3_1Quest : GeneralScript
{
	private const int BookOfRevenge3Translated = 73057;

	public override void Load()
	{
		AddHook("_duncan", "before_keywords", DuncanBeforeKeywords);
		AddHook("_bryce", "before_keywords", BryceBeforeKeywords);
	}

	public async Task<HookResult> DuncanBeforeKeywords(NpcScript npc, params object[] args)
	{
		var keyword = args[0] as string;

		if (keyword == "g1_glasgavelen")
		{
			if (npc.HasKeyword("g1_26"))
			{
				npc.RemoveKeyword("g1_26");
				npc.GiveKeyword("g1_27");

				npc.Msg(L("What? What did you just say?"));
				npc.Msg(L("G-Glas Ghaibhleann?<br/>This can't be... Where did you hear that name?"));
				npc.Msg(L("Does the third book of Fomors cover that?<br/>I'd like to read it. If you have it on you, please let me see it."), npc.Button(L("Here it is."), "@yes"), npc.Button(L("No"), "@no"));
			}
			else if (npc.HasKeyword("g1_27"))
			{
				npc.Msg(L("I'd like to read the third book of Fomors. If you have it on you, please let me see it."), npc.Button(L("Here it is."), "@yes"), npc.Button(L("No"), "@no"));
			}

			if (await npc.Select() != "@yes" || !npc.HasItem(BookOfRevenge3Translated))
			{
				npc.Msg(L("Please come back immediately once you have the book on you."));
				return HookResult.Break;
			}

			npc.RemoveKeyword("g1_glasgavelen");
			npc.GiveKeyword("g1_book_of_glasgavelen");

			npc.RemoveItem(BookOfRevenge3Translated);
			npc.Notice(L("You have given the Book of Revenge, Vol. 3 (Translated) to Duncan."));

			npc.Msg(Hide.Name, L("(Duncan receives the book and carefully starts reading.)"));
			npc.Msg(L("This can't be!<br/>Things are a lot more serious than I'd thought.<br/>This is a problem. What should we do?"));
			npc.Msg(L("Glas Ghaibhleann is a legendary giant<br/>that destroyed everything within it's sight with hatred and anger.<br/>It's impossible to describe the fear this monster generates."));
			npc.Msg(L("Once, Glas Ghaibhleann was commanded by an evil god<br/>to rampage all across Erinn.<br/>There was simply nothing we could do about it."), npc.Image("g1_ch24_glasgavelen"));
			npc.Msg(L("That's when our ancestor race, the Partholons, was decimated<br/>and sought refuge in this small mountainous area."));
			npc.Msg(L("I'm sorry. I'm a bit dazed and it's hard to talk.<br/>It'd be more helpful for you to read a book written by our ancestors<br/>who fought against this monster.<br/>Let's see."));
			npc.Msg(L("...<p/>...!!<p/>It's not here!<p/>Ah, don't give up now.<br/>I just forgot that<br/>I'd lent it to someone.<br/>He lives in Bangor and...what was his name?"));
			npc.Msg(L("B-Bri... That's right!<br/>Bryce is the person who borrowed it from me."));
			npc.Msg(L("Tell Bryce about me to get the book and read it.<br/>There is so much more there than I can ever tell you<br/>that is written in that book."));

			return HookResult.Break;
		}
		else if (keyword == "g1_book_of_glasgavelen")
		{
			npc.Msg(L("Tell Bryce about me to get the book and read it.<br/>There is so much more there than I can ever tell you<br/>that is written in that book."));

			return HookResult.Break;
		}

		return HookResult.Continue;
	}

	public async Task<HookResult> BryceBeforeKeywords(NpcScript npc, params object[] args)
	{
		var keyword = args[0] as string;

		if (keyword == "g1_book_of_glasgavelen")
		{
			npc.RemoveKeyword("g1_27");
			npc.GiveKeyword("g1_28");
			npc.RemoveKeyword("g1_book_of_glasgavelen");
			npc.GiveKeyword("g1_bone_of_glasgavelen");

			npc.Msg(L("...So you must be the one who spoke to Duncan in Tir Chonaill.<br/>I was roughly brought up to speed by the owl he'd sent me."));
			npc.Msg(L("I've always had an interest in old stories, so<br/>when I happened to meet the Chief, I was able to read this book.<br/>I am done with this book now, so you can take it if you want to."));
			npc.Msg(L("By the way, I found something very interesting in this book.<br/>Resurrecting that monster apparently requires this mineral called Adamantium.<br/>Adamantium is a mystical metal that's resistant to magic powers.<br/>It's a type of metal that is only found in the mines of Bangor."), npc.Image("g1_ch24_adaman"));
			npc.Msg(L("The problem is... Adamantium is no longer<br/>being produced in the mines of Bangor.<br/>And apparently, it's been like that for a long time."));
			npc.Msg(L("When this happens, it's either on of those things."));
			npc.Msg(L("Either this region has run out of Adamantium deposits...<br/>Or..."));
			npc.Msg(L("Someone else is mining them all."));

			return HookResult.Break;
		}

		return HookResult.Continue;
	}
}
