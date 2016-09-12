//--- Aura Script -----------------------------------------------------------
// G1 012: The Book of Revenge
//--- Description -----------------------------------------------------------
// Talking to some NPCs about Mores and getting the item for his RP dungeon.
// 
// Wiki:
// - N/A
//---------------------------------------------------------------------------

public class TheBookOfRevengeQuest : GeneralScript
{
	private const int BookOfRevenge = 73053;
	private const int Torque = 73003;

	public override void Load()
	{
		AddHook("_duncan", "before_keywords", DuncanBeforeKeywords);
		AddHook("_tarlach", "before_keywords", TarlachBeforeKeywords);
		AddHook("_eavan", "before_keywords", EavanBeforeKeywords);
		AddHook("_kristell", "before_keywords", KristellBeforeKeywords);
	}

	public async Task<HookResult> DuncanBeforeKeywords(NpcScript npc, params object[] args)
	{
		var keyword = args[0] as string;

		if (keyword == "g1_mores")
		{
			npc.RemoveKeyword("g1_18");
			npc.GiveKeyword("g1_19");
			npc.RemoveKeyword("g1_mores");
			npc.GiveKeyword("g1_mores_gwydion");

			npc.Msg(L("Mores...? That's the author of the book you have?<br/>Hmm... You're right..."));
			npc.Msg(L("Really...? The one and only hero who rescued this world...<br/>Mores Gwydion?<br/>...No, it can't be. They probably just have the same name."));
			npc.Msg(L("... I know who Mores Gwydion is...<br/>I saw him a few times when I was young..."));
			npc.Msg(L("In the Second War at Mag Tuireadh Plains,<br/>he'd infiltrated deep into the Fomors' camp<br/>and stopped the Fomors from casting their ultimate spell...<br/>...all the while sacrificing his own life."), npc.Image("g1_ch17_magicfight"));
			npc.Msg(L("If it wasn't for him...<br/>Erinn would have turned into <br/>a wasteland by the evil Wizard<br/>Jabchiel."));
			npc.Msg(L("People were devestated when his friends returned<br/>to report of this death...<br/>Many people speak very highly of him even now."));
			npc.Msg(L("And quite a few parents wanted to name their newborn children<br/>after him."));
			npc.Msg(L("Come to think of it... By now, enough time has probably passed for a<br/>child with his name to grow up and write a book on Fomors...<br/>...Hah..."));

			return HookResult.Break;
		}

		return HookResult.Continue;
	}

	public async Task<HookResult> TarlachBeforeKeywords(NpcScript npc, params object[] args)
	{
		var keyword = args[0] as string;

		if (keyword == "g1_mores_gwydion")
		{
			if (npc.HasKeyword("g1_19"))
			{
				npc.RemoveKeyword("g1_19");
				npc.GiveKeyword("g1_20");

				npc.Msg(L("Mores Gwydion..."));
				npc.Msg(L("...Yes, he's the author of that book.<br/>He's my mentor who's taught me<br/>...And also a Wizard who'd saved the world."));
				npc.Msg(L("I had always thought that he had passed away but...<br/>he's apparently alive and has sided with the Fomors."));
				npc.Msg(L("...Anyhow, give me the translated book.<br/>I would like to take a look at it."), npc.Button(L("Sure"), "@yes"), npc.Button(L("No"), "@no"));
			}
			else if (npc.HasKeyword("g1_20"))
			{
				npc.Msg(L("Give me the translated book.<br/>I would like to take a look at it."), npc.Button(L("Sure"), "@yes"), npc.Button(L("No"), "@no"));
			}

			if (await npc.Select() != "@yes" || !npc.HasItem(BookOfRevenge))
			{
				npc.Msg(L("Don't you have it with you?"));
				return HookResult.Break;
			}

			npc.RemoveItem(BookOfRevenge);
			npc.RemoveKeyword("g1_mores_gwydion");
			npc.GiveKeyword("g1_memo_of_lost_thing");

			npc.Msg(Hide.Name, L("(Tarlach flipped through the book.)"));
			npc.Msg(L("Just what I'd tought...<br/>Master had faked his own death<br/>for some reason and now he's helping the Fomors."));
			npc.Msg(L("This must be what Kristell was talking about."));
			npc.Msg(L("'...Okay, I admit it.<br/>Perhaps I didn't lose the token but<br/>I wanted to throw it away..."), npc.Image("g1_ch18_book"));
			npc.Msg(L("Hmm... apparently, Master was mulling over something<br/>after losing some item...<p/>I wonder what it was..."));
			npc.Msg(L("...!<p/>Dunbarton's Town Office<br/>collects lost items and returns them to their rightful owners.<br/>You might be able to find a clue if you<br/>can find what it is that Mores had lost."));

			return HookResult.Break;
		}

		return HookResult.Continue;
	}

	public async Task<HookResult> EavanBeforeKeywords(NpcScript npc, params object[] args)
	{
		var keyword = args[0] as string;

		if (keyword == "g1_memo_of_lost_thing")
		{
			npc.Msg(L("Hmm? Who picked it up just now?<br/>Priestess Kristel was looking for such an item, so I gave it to her."));
			npc.Msg(L("A Priestess wouldn't have any reason to lie<br/>Is there a problem?"));
			npc.Msg(L("Why don't you speak to her at the Temple?"));

			return HookResult.Break;
		}

		return HookResult.Continue;
	}

	public async Task<HookResult> KristellBeforeKeywords(NpcScript npc, params object[] args)
	{
		var keyword = args[0] as string;

		if (keyword == "g1_memo_of_lost_thing")
		{
			if (npc.HasKeyword("g1_20"))
			{
				npc.RemoveKeyword("g1_20");
				npc.GiveKeyword("g1_21");

				npc.GiveWarpScroll(63009, "math_dungeon");

				npc.Msg(L("You're back. I knew you would be back.<br/>Here's the item you're looking for."));
				npc.Msg(L("I only remembered it later on too.<br/>I figured the person who wrote that can<br/>find the lost item at the Town Office."), npc.Image("g1_ch18_torque01"));
				npc.Msg(L("Broken Torque.<br/>It looks like a memorial item. Try using it in Math Dungeon.<br/>I gave you the Red Wing of the Goddess too, just in case you need it."), npc.Image("g1_ch18_torque01"));
			}
			else if (npc.HasKeyword("g1_21"))
			{
				npc.Msg(L("Try using the Broken Torque in Math Dungeon."));
			}

			if (!npc.HasItem(Torque))
				npc.GiveItem(Torque);

			return HookResult.Break;
		}

		return HookResult.Continue;
	}
}
