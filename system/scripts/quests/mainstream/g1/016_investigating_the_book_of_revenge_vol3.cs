//--- Aura Script -----------------------------------------------------------
// G1 016: Investigation of the third volume of the Book of Revenge
//--- Description -----------------------------------------------------------
// Asking Seumas about the third volume and getting it after making a
// delivery. Some messages are improvised, since the originals couldn't
// be found anymore. The second quest in particular, as it was removed
// later on.
// 
// Wiki:
// - Look for The Book of Revenge, Volume III.
//---------------------------------------------------------------------------

public class InvestigatingBookOfRevengeVol3_1Quest : QuestScript
{
	private const int FathersGift = 73025;

	public override void Load()
	{
		SetId(210008);
		SetName(L("Find the Book of Revenge, Vol. III"));
		SetDescription(L("I heard about the Book of Revenge, Vol. III. Come to the Dunbarton Bookstore immediately. - Aeira -"));

		SetIcon(QuestIcon.AdventOfTheGoddess);
		SetCategory(QuestCategory.AdventOfTheGoddess);

		AddObjective("talk_aeira", L("Find out what Aeira of the Dunbarton Bookstore discovered."), 14, 44978, 43143, Talk("aeira"));
		AddObjective("talk_lassar", L("Ask Lassar in Tir Chonaill for the book."), 9, 2020, 1537, Talk("lassar"));
		AddObjective("talk_seumas", L("Meet Seumas at the DragonRuins and ask him about the book."), 30, 38334, 48677, Talk("seumas"));

		AddReward(Exp(1500));

		AddHook("_aeira", "after_intro", AeiraAfterIntro);
		AddHook("_lassar", "after_intro", LassarAfterIntro);
		AddHook("_seumas", "after_intro", SeumasAfterIntro);
	}

	public async Task<HookResult> AeiraAfterIntro(NpcScript npc, params object[] args)
	{
		if (npc.QuestActive(this.Id, "talk_aeira"))
		{
			npc.FinishQuest(this.Id, "talk_aeira");

			npc.Msg(L("Ah! I've been waiting!<br/>I have good news."));
			npc.Msg(L("I found out where you can find volume 3<br/>of that Fomor book you've been looking for,<br/>Lassar, the magic instructor at the school in<br/>Tir Chonaill has it. You know her, don't you?"));
			npc.Msg(L("She came by looking for a particular book recently<br/>and while we were chatting, I snuck in a question<br/>about your own search. She says she has the third<br/>colume of 'The Book of Revenge!' An original copy<br/>in the Fomor language, no less!"));
			npc.Msg(L("You should hurry up and see here. Good luck!"));

			return HookResult.Break;
		}

		return HookResult.Continue;
	}

	public async Task<HookResult> LassarAfterIntro(NpcScript npc, params object[] args)
	{
		if (npc.QuestActive(this.Id, "talk_lassar"))
		{
			npc.FinishQuest(this.Id, "talk_lassar");

			npc.Msg(L("Welcome. You must be... <username/>?"));
			npc.Msg(L("...So it was you who was looking for the book.<br/>Aeira had asked me about the book before.<br/>Did she tell you what I told her...?"));
			npc.Msg(L("Yes, the book isn't actually mine.<br/>It belongs to Seumas, who works at the ruins excavation site in Gairech."), npc.Image("g1_ch23_seumas"));
			npc.Msg(L("He said he found it not too long ago while excavating the site.<br/>But he could not read the writing<br/>and asked around and came to me to interpret it."), npc.Image("g1_ch23_seumas"));
			npc.Msg(L("But... it was too difficult for me as well.<br/>Forget translating...I couldn't even read one sentence of it.<br/>I was barely able to figure out the title."));
			npc.Msg(L("When I told Seumas, he said it was okay, and asked me to return his book, so<br/>as a matter of fact, I just gave the book back to him."));
			npc.Msg(L("I don't know if you'll be able to read it because it's such a difficult book to read...<br/>but if you want to take a look at it, stop by the...<br/>ruins excavation site in Gairech."));
			npc.Msg(L("I'm sure Seumas will be happy to find out<br/>if there is indeed a way the book can be translated."));

			return HookResult.Break;
		}

		return HookResult.Continue;
	}

	public async Task<HookResult> SeumasAfterIntro(NpcScript npc, params object[] args)
	{
		if (npc.QuestActive(this.Id, "talk_seumas"))
		{
			npc.CompleteQuest(this.Id);
			npc.StartQuest(210012);

			npc.GiveItem(FathersGift);
			npc.Notice(L("You have received Father's Gift from Seumas."));

			npc.Msg(L("(He gasps for breath as he speaks.)<br/>Ah... I suppose you're the one Lassar mentioned...?<br/>You need the...book of Fomors...?"));
			npc.Msg(L("Not too long ago...I asked Lassar...<br/>to translate it<br/>but she said she couldn't do it..."));
			npc.Msg(L("I probably won't be able to figure it out myself anyway..."));
			npc.Msg(L("Could you do me a little favor... before I give it to you?<br/>I can't leave here right now,<br/>Please deliver this present to my son Sion in Bangor."));

			return HookResult.Break;
		}

		return HookResult.Continue;
	}
}

public class InvestigatingBookOfRevengeVol3_2Quest : QuestScript
{
	private const int FathersGift = 73025;
	private const int BookOfRevenge3 = 73056;
	private const int OwlDelay = 5 * 60;

	public override void Load()
	{
		SetId(210012);
		SetName(L("Get the Book of Revenge, Vol. III"));
		SetDescription(L("Please deliver Father's Gift to my son Sion in Bangor, in exchange I will give you the Book of Revenge, Vol. III. - Seumas -"));

		SetIcon(QuestIcon.AdventOfTheGoddess);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.AdventOfTheGoddess);

		AddObjective("talk_sion", L("Deliver gift to Sion in Bangor."), 31, 12093, 15062, Talk("sion"));
		AddObjective("talk_seumas", L("Get the book from Seumas."), 30, 38334, 48677, Talk("seumas"));
		AddObjective("talk_kristell", L("Ask Kristell to translate the Book of Revenge, Vol. III."), 14, 34657, 42808, Talk("kristell"));

		AddReward(Exp(1500));
		AddReward(Gold(470));

		AddHook("_sion", "after_intro", SionAfterIntro);
		AddHook("_seumas", "after_intro", SeumasAfterIntro);
		AddHook("_kristell", "after_intro", KristellAfterIntro);
	}

	public async Task<HookResult> SionAfterIntro(NpcScript npc, params object[] args)
	{
		if (npc.QuestActive(this.Id, "talk_sion"))
		{
			npc.FinishQuest(this.Id, "talk_sion");

			npc.RemoveItem(FathersGift);
			npc.Notice(L("You have given Father's Gift to Sion."));

			npc.Msg(L("... What's this?<br/>Eh, a present from dad?<br/>Wow, thank you!"));

			return HookResult.Break;
		}

		return HookResult.Continue;
	}

	public async Task<HookResult> SeumasAfterIntro(NpcScript npc, params object[] args)
	{
		if (npc.QuestActive(this.Id, "talk_seumas"))
		{
			npc.FinishQuest(this.Id, "talk_seumas");

			npc.GiveItem(BookOfRevenge3);
			npc.Notice(L("You have received The Book of Revenge, Vol. III from Seumas."));

			npc.Msg(L("Oh, that was fast, <username/>...<br/>Did Sion like it? Hah... Thank you...<br/>Here...is the...book...<br/>Hope it would be helpful..."));

			return HookResult.Break;
		}

		return HookResult.Continue;
	}

	public async Task<HookResult> KristellAfterIntro(NpcScript npc, params object[] args)
	{
		if (npc.QuestActive(this.Id, "talk_kristell"))
		{
			npc.CompleteQuest(this.Id);
			npc.SendOwl(210009, OwlDelay); // Receive the Book of Revenge, Vol. III.

			npc.RemoveItem(BookOfRevenge3);
			npc.Notice(L("You have given the Book of Revenge, Vol. III to Kristell."));

			npc.Msg(L("You really did bring the Book of Fomors...<br/>Volume 3 is indeed the last in the series."));
			npc.Msg(L("I knew you could get the job done.<br/>I'll let you know when my translation is complete."));

			return HookResult.Break;
		}

		return HookResult.Continue;
	}
}


public class InvestigatingBookOfRevengeVol3_3Quest : QuestScript
{
	private const int BookOfRevenge3Translated = 73057;

	public override void Load()
	{
		SetId(210009);
		SetName(L("Receive the Book of Revenge, Vol. III."));
		SetDescription(L("I finished the Book if Revenge, Vol. III. I'll be waiting at Dunbarton Church. - Kristell -"));

		SetIcon(QuestIcon.AdventOfTheGoddess);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.AdventOfTheGoddess);

		AddObjective("talk_kristell", L("Receive the translated copy from Kristell at the Dunbarton Church."), 14, 34657, 42808, Talk("kristell"));

		AddReward(Exp(350));
		AddReward(Gold(67));

		AddHook("_kristell", "after_intro", KristellAfterIntro);
	}

	public async Task<HookResult> KristellAfterIntro(NpcScript npc, params object[] args)
	{
		if (npc.QuestActive(this.Id, "talk_kristell"))
		{
			npc.CompleteQuest(this.Id);

			npc.GiveItem(BookOfRevenge3Translated);
			npc.Notice(L("You have received The Book of Revenge, Vol. III (Translated) from Kristell."));

			npc.Msg(L("Here is the translated copy you've been waiting for.<br/>With that, I've finished translating all three volumes of the Book of Revenge."));
			npc.Msg(L("...<br/>I must warn you...<br/>This last volume contains some...disturbing passages."));

			return HookResult.Break;
		}

		return HookResult.Continue;
	}
}
