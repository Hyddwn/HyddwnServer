//--- Aura Script -----------------------------------------------------------
// G1 010: Book Of Evil Part 1
//--- Description -----------------------------------------------------------
// Bringing a book to Kristell for her to translate it, waiting for the
// translation, and then talking to Tarlach, Meven, and Lassar.
// 
// Wiki:
// - Requirement: Holy Water of Lymilark x 1 
// - Instruction: Obtain Book of Evil.
//---------------------------------------------------------------------------

public class BookOfEvilPart1_1Quest : QuestScript
{
	private const int TarlachsGlassesPouch = 73022;
	private const int BookOfFomor = 73052;
	private const int OwlDelay = 5 * 60;

	public override void Load()
	{
		SetId(210003);
		SetName(L("Translating the Book of Fomors"));
		SetDescription(L("I got this book a long time ago, but I haven't even read a single page because it's written in the Fomor language. Could you ask Kristell to translate the book? Please tell her it's important... -Tarlach-"));

		SetIcon(QuestIcon.AdventOfTheGoddess);
		SetCategory(QuestCategory.AdventOfTheGoddess);

		AddObjective("deliver_book", L("Ask Kristell to translate the [Book of Fomors]."), 14, 34657, 42808, Talk("kristell"));

		AddReward(Exp(65));
		AddReward(Gold(65));

		AddHook("_kristell", "after_intro", KristellAfterIntro);
	}

	public async Task<HookResult> KristellAfterIntro(NpcScript npc, params object[] args)
	{
		if (!npc.QuestActive(this.Id) || !npc.HasItem(BookOfFomor))
			return HookResult.Continue;

		npc.CompleteQuest(this.Id);
		npc.SendOwl(210004, OwlDelay); // [Book of Fomors] Translation Completed

		npc.RemoveItem(BookOfFomor);
		npc.Notice(L("You have given Book of Fomors to Kristell."));

		npc.Msg(L("...Tarlach asked me...?<br/>To translate...this book for him...?"));
		npc.Msg(L("I see... This is definitely Tarlach's book...<br/>... ...Is he still living as a Druid...with his injured body and all...?<br/>Poor guy..."));
		npc.Msg(L("Okay...<br/>I will translate it..."));
		npc.Msg(L("I will contact you once the translation is completed."));

		return HookResult.Break;
	}
}

public class BookOfEvilPart1_2Quest : QuestScript
{
	public override void Load()
	{
		SetId(210004);
		SetName(L("[Book of Fomors] Translation Completed"));
		SetDescription(L("The translation of the [Book of Fomors] has been finished. Please stop by so I can give you the translated book. - Kristell -"));

		SetIcon(QuestIcon.AdventOfTheGoddess);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.AdventOfTheGoddess);

		AddObjective("talk_kristell", L("Receive the translated [Book of Fomors] from Kristell."), 14, 34657, 42808, Talk("kristell"));
		AddObjective("talk_tarlach", L("Tell Tarlach about the book."), 48, 11100, 30400, Talk("tarlach"));
		AddObjective("talk_meven", L("Ask Meven for Tarlach's item."), 4, 954, 2271, Talk("meven"));

		AddReward(Exp(250));

		AddHook("_kristell", "after_intro", KristellAfterIntro);
		AddHook("_tarlach", "after_intro", TarlachAfterIntro);
		AddHook("_meven", "after_intro", MevenAfterIntro);
	}

	public async Task<HookResult> KristellAfterIntro(NpcScript npc, params object[] args)
	{
		if (!npc.QuestActive(this.Id, "talk_kristell"))
			return HookResult.Continue;

		npc.FinishQuest(this.Id, "talk_kristell");

		npc.Msg(L("...Hello... <username/>, welcome...<br/>...And Tarlach... he didn't come...did he?"));
		npc.Msg(L("...I see. Of course...<br/>I'm not surprised...<br/>He doesn't let anyone enter in his life..."));
		npc.Msg(L("He said he couldn't accept me<br/>because he was living the life of a Druid..."));
		npc.Msg(L("...<p/>...Am I nothing more than a tool who understands the Fomor language to him...?"));
		npc.Msg(L("...I'm sorry, but you will need to go back to where he is..."));
		npc.Msg(L("... When Tarlach comes...<br/>...I would like to give the book to him myself.<br/>Please understand why I can't give this book to you."));

		return HookResult.Break;
	}

	public async Task<HookResult> TarlachAfterIntro(NpcScript npc, params object[] args)
	{
		if (!npc.QuestActive(this.Id, "talk_tarlach"))
			return HookResult.Continue;

		npc.FinishQuest(this.Id, "talk_tarlach");

		npc.Msg(Hide.Name, L("(Tarlach listens intently as I relayed Kristell's message to him."));
		npc.Msg(L("...<br/>Is that what she said...?<br/>But... I can't leave this place."));
		npc.Msg(L("I understand...how she feels, since<br/>she gave up her life as a Fomor...for love...<br/>but I'm not a man who deserves that kind of love..."));
		npc.Msg(L("...although it was too late when I realized it...and ended up breaking her heart."));
		npc.Msg(L("...<p/>Could I ask you for a favor...?<br/>I left something with Priest Meven of Tir Chonail a long time ago.<br/>Would you please retrive it for me...?"));
		npc.Msg(L("Just go and tell him what I told you<br/>...and he will know what you're talking about."));

		return HookResult.Break;
	}

	public async Task<HookResult> MevenAfterIntro(NpcScript npc, params object[] args)
	{
		if (!npc.QuestActive(this.Id, "talk_meven"))
			return HookResult.Continue;

		npc.CompleteQuest(this.Id);
		npc.RemoveKeyword("g1_17_1");
		npc.GiveKeyword("g1_17_2");
		npc.GiveKeyword("g1_black_rose");

		npc.Msg(L("Oh, it's you <username/>... Welcome."));
		npc.Msg(Hide.Name, L("(Told priest Meven about the item Tarlach left with him.)"));
		npc.Msg(L("...He...wants that...back?<br/>Nonsense...!<br/>Do you know how long it's been? What, was he expecting it<br/>to be preserved by magic or something?"));
		npc.Msg(L("... Hmm...I'm afraid that's not possible.<br/>Granted it's the magic of a Druid...<br/>but...the best thing to do will be to grow it again."));
		npc.Msg(L("I'll let Lassar know,<br/>so would you go talk to her about it...?<br/>Lassar is quite the expert on that field."));
		npc.Msg(L("Go see her and tell her that I sent you..."));
		npc.Msg(L("...Anyway...Tarlach still must have not forgotten about her...<br/>if he's asking for that item back...It's a shame..."));

		return HookResult.Break;
	}
}

public class BookOfEvilPart1_3Quest : GeneralScript
{
	private const int HolyWater = 63016;
	private const int OwlDelay = 5 * 60;

	public override void Load()
	{
		AddHook("_lassar", "before_keywords", LassarBeforeKeyword);
	}

	public async Task<HookResult> LassarBeforeKeyword(NpcScript npc, params object[] args)
	{
		var keyword = args[0] as string;

		if (keyword == "g1_black_rose")
		{
			if (npc.HasKeyword("g1_17_2"))
			{
				npc.RemoveKeyword("g1_17_2");
				npc.GiveKeyword("g1_17_3");

				npc.Msg(L("Hmm. Priest Meven's favor?<br/>Why would he need something like that? Hehehe."));
				npc.Msg(L("Hmm, I don't know how this will sound<br/>but I need some Holy Water of Lymilark to grow this.<br/>Priest Meven used to supply me with it up until recently."));
				npc.Msg(L("I guess he got caught by Priestess Endelyon.<br/>He hasn't given me any for the past few days."));
				npc.Msg(L("If you have any Holy Water of Lymilark, could you give me a bottle?"), npc.Button(L("Here."), "@yes"), npc.Button(L("No"), "@no"));
				if (await npc.Select() != "@yes" || !npc.HasItem(HolyWater))
				{
					npc.Msg(L("I need some Holy Water of Lymilark to grow this, please come back once you have some."));
					return HookResult.Break;
				}

				LassarFinish(npc);

				return HookResult.Break;
			}
			else if (npc.HasKeyword("g1_17_3"))
			{
				npc.Msg(L("Did you get a bottle of Holy Water of Lymilark?"), npc.Button(L("Here."), "@yes"), npc.Button(L("No"), "@no"));
				if (await npc.Select() != "@yes" || !npc.HasItem(HolyWater))
				{
					npc.Msg(L("I need some Holy Water of Lymilark to grow this, please come back once you have some."));
					return HookResult.Break;
				}

				LassarFinish(npc);

				return HookResult.Break;
			}
		}

		return HookResult.Continue;
	}

	private void LassarFinish(NpcScript npc)
	{
		npc.RemoveKeyword("g1_black_rose");
		npc.RemoveKeyword("g1_17_3");
		npc.RemoveKeyword("g1_17_4");
		npc.GiveKeyword("g1_17_5");

		npc.RemoveItem(HolyWater);
		npc.Notice(L("You have given Holy Water of Lymilark to Lassar."));

		npc.SendOwl(210023, OwlDelay); // Receive the Requested Object

		npc.Msg(L("Yes, this should be enough.<br/>It's almost ready. Once it forms the proper shape, I'll let you know via an owl. Hahaha.<br/>Owls don't fly into buildings so<br/>don't forget to check the sky outside."));
		npc.Msg(L("Now, will you excuse me?"));
	}
}
