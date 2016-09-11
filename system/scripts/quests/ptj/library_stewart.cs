//--- Aura Script -----------------------------------------------------------
// Stewart's Library Delivery Part-Time Job
//--- Description -----------------------------------------------------------
// All quests used by the PTJ, and a script to handle the PTJ via hooks.
//--- Notes -----------------------------------------------------------------
// All receive & shelve PTJs are not fully tested.
// At the time of writing, the library is unavailable to shelve the books in.
//
// Some Talk objective descriptions have been shortened
// to account for potential textbox overflow:
// https://files.gitter.im/aura-project/aura/4QYo/blob
//---------------------------------------------------------------------------

public class StewartPtjScript : GeneralScript
{
	const PtjType JobType = PtjType.Library;

	const int Start = 9;
	const int Report = 11;
	const int Deadline = 19;
	const int PerDay = 10;

	int remaining = PerDay;

	readonly int[] QuestIds = new int[]
	{
		519401, // Receive book from Aeira (Type 1)
		519402, // Receive book from Aeira (Type 2)
		519403, // Receive book from Aeira (Type 3)
		519404, // Receive book from Simon (Type 1)
		519405, // Receive book from Simon (Type 2)
		519406, // Receive book from Simon (Type 3)
		519407, // Receive book from Austeyn (Type 1)
		519408, // Receive book from Austeyn (Type 2)
		519409, // Receive book from Austeyn (Type 3)
		519410, // Receive book from Kristell (Type 1)
		519411, // Receive book from Kristell (Type 2)
		519412, // Receive book from Glenis (Type 1)
		519413, // Receive book from Glenis (Type 2)
		519414, // Receive book from Glenis (Type 3)
		519415, // Receive book from Eavan (Type 1)
		519416, // Receive book from Eavan (Type 2)
		519417, // Receive book from Manus (Type 1)
		519418, // Receive book from Manus (Type 2)
		519419, // Receive book from Walter (Type 1)
		519420, // Receive book from Walter (Type 2)
		519421, // Receive book from Walter (Type 3)
		519422, // Receive book from Nerys (Type 1)
		519423, // Receive book from Nerys (Type 2)
		519424, // Receive book from Nerys (Type 3)
		519431, // Receive book from Aeira/Eavan (Type 1)
		519432, // Receive book from Aeira/Eavan (Type 2)
		519433, // Receive book from Aeira/Eavan (Type 3)
		519434, // Receive & shelve book from Walter/Austeyn
		519435, // Receive book from Walter/Austeyn (Type 1)
		519436, // Receive book from Walter/Austeyn (Type 2)
		519437, // Receive & shelve book from Simon/Glenis
		519438, // Receive book from Simon/Glenis (Type 1)
		519439, // Receive book from Simon/Glenis (Type 2)
		519440, // Receive & shelve book from Nerys/Manus (Type 1)
		519441, // Receive book from Nerys/Manus
		519442, // Receive & shelve book from Nerys/Manus (Type 2)
		519443, // Receive book from Tracy (Type 1)
		519444, // Receive book from Tracy (Type 2)
		519445, // Receive & shelve book from Tracy
		519461, // Receive book from Aeira/Walter/Simon (Type 1)
		519462, // Receive & shelve book from Aeira/Walter/Simon
		519463, // Receive book from Aeira/Walter/Simon (Type 2)
		519464, // Receive & shelve book from Kristell/Glenis/Austeyn
		519465, // Receive book from Kristell/Glenis/Austeyn (Type 1)
		519466, // Receive book from Kristell/Glenis/Austeyn (Type 2)
		519467, // Receive & shelve book from Austeyn/Manus/Nerys
		519468, // Receive book from Austeyn/Manus/Nerys (Type 1)
		519469, // Receive book from Austeyn/Manus/Nerys (Type 2)
		519470, // Receive & shelve book from Eavan/Tracy
		519471, // Receive book from Eavan/Tracy (Type 1)
		519472, // Receive book from Eavan/Tracy (Type 2)
	};

	public override void Load()
	{
		AddHook("_stewart", "after_intro", AfterIntro);
		AddHook("_stewart", "before_keywords", BeforeKeywords);
	}

	public async Task<HookResult> AfterIntro(NpcScript npc, params object[] args)
	{
		// Call PTJ method after intro if it's time to report
		if (npc.DoingPtjForNpc() && npc.ErinnHour(Report, Deadline))
		{
			await AboutArbeit(npc);
			return HookResult.Break;
		}

		return HookResult.Continue;
	}

	[On("ErinnMidnightTick")]
	private void OnErinnMidnightTick(ErinnTime time)
	{
		// Reset available jobs
		remaining = PerDay;
	}

	public async Task<HookResult> BeforeKeywords(NpcScript npc, params object[] args)
	{
		var keyword = args[0] as string;

		// Hook PTJ keyword
		if (keyword == "about_arbeit")
		{
			await AboutArbeit(npc);
			await npc.Conversation();
			npc.End();

			return HookResult.End;
		}

		return HookResult.Continue;
	}

	public async Task AboutArbeit(NpcScript npc)
	{
		// Check if already doing another PTJ
		if (npc.DoingPtjForOtherNpc())
		{
			npc.Msg(L("Are you trying to do this while working on another part-time job?<br/>Hahaha, that's greedy!"));
			return;
		}

		// Check if PTJ is in progress
		if (npc.DoingPtjForNpc())
		{
			var result = npc.GetPtjResult();

			// Check if report time
			if (!npc.ErinnHour(Report, Deadline))
			{
				if (result == QuestResult.Perfect)
				{
					npc.Msg(L("I'm sorry, but I'm really busy right now...<br/>Can you please come back in a bit?"));
				}
				else
				{
					npc.Msg(L("How's work going along so far?<br/>You haven't found anyone that claims to have lost the book, though, right?"));
				}
				return;
			}

			// Report?
			npc.Msg(L("Did you get all the books?"),
				npc.Button(L("Report Now"), "@report"),
				npc.Button(L("Report Later"), "@later")
				);

			if (await npc.Select() != "@report")
			{
				npc.Msg(L("Please come back after you're done collecting books."));
				return;
			}

			// Nothing done
			if (result == QuestResult.None)
			{
				npc.GiveUpPtj();

				npc.Msg(npc.FavorExpression(), L("This is not good...<br/>Hope you'll get the job done another time."));
				npc.ModifyRelation(0, -Random(3), 0);
			}
			// Low~Perfect result
			else
			{
				npc.Msg(L("Thank you for a job well done.<br/>Would you like to choose your reward?"),
					npc.Button(L("Report Later"), "@later"),
					npc.PtjReport(result)
					);
				var reply = await npc.Select();

				// Report later
				if (!reply.StartsWith("@reward:"))
				{
					npc.Msg(npc.FavorExpression(), L("Having a hard time choosing?<br/>Please take your time and think carefully about your reward..."));
					return;
				}

				// Complete
				npc.CompletePtj(reply);
				remaining--;

				// Result msg
				if (result == QuestResult.Perfect)
				{
					npc.Msg(npc.FavorExpression(), L("Great Job. Thank you very much."));
					npc.ModifyRelation(0, Random(3), 0);
				}
				else if (result == QuestResult.Mid)
				{
					npc.Msg(npc.FavorExpression(), L("I don't think you got all of them.<br/>Oh well...<br/>Hopefully you can do better another time."));
					npc.ModifyRelation(0, Random(1), 0);
				}
				else if (result == QuestResult.Low)
				{
					npc.Msg(npc.FavorExpression(), L("Hmmm... did you just skip the whole thing? This isn't good..."));
					npc.ModifyRelation(0, -Random(2), 0);
				}
			}
			return;
		}

		// Check if PTJ time
		if (!npc.ErinnHour(Start, Deadline))
		{
			npc.Msg(L("I'm sorry, but I can't give you work right now."));
			return;
		}

		// Check if not done today and if there are jobs remaining
		if (!npc.CanDoPtj(JobType, remaining))
		{
			npc.Msg(L("You've already done this today...<br/>Did you forget that? Haha..."));
			return;
		}

		// Offer PTJ
		var randomPtj = npc.RandomPtj(JobType, QuestIds);
		var msg = "";

		if (npc.GetPtjDoneCount(JobType) == 0)
			msg = L("You must be here for the first time...<br/>the job is pretty simple, actually.<br/>All you have to do is retrieve books that have been borrowwed, but haven't been returned yet.");
		else
			msg = L("Thanks, and good luck.<br/>It would have been so much easier if people retured books on time.");

		npc.Msg(msg, npc.PtjDesc(randomPtj,
			L("Stewart's Library Delivery Part-time Job"),
			L("Looking for help with delivery of goods in Library."),
			PerDay, remaining, npc.GetPtjDoneCount(JobType)));

		if (await npc.Select() == "@accept")
		{
			if (npc.GetPtjDoneCount(JobType) == 0)
				npc.Msg(L("All you have to do is retrieve the books before deadline, and return them to me. You'll have plenty of time."));
			else
				npc.Msg(L("Okay, now please take care of it. Don't forget it just because it's an easy task!"));

			npc.StartPtj(randomPtj);
		}
		else
		{
			if (npc.GetPtjDoneCount(JobType) == 0)
				npc.Msg(L("Ah... That's too bad.<br/>It could have been really nice if you could help me."));
			else
				npc.Msg(L("It's an easy task... well, no can do."));
		}
	}
}

public abstract class StewartVarLibraryPtjBaseScript : QuestScript
{
	protected struct NpcBookPair
	{
		public string NpcIdent { get; set; }
		public int ItemId { get; set; }
	}

	/// <summary>
	/// true: Also shelve retrieved books in the proper bookcase.
	/// <para>false: Collect books to deliver to Stewart.</para>
	/// </summary>
	protected abstract bool DoShelving { get; }

	protected abstract int QuestId { get; }
	protected abstract string LQuestDescription { get; }
	protected abstract NpcBookPair[] NpcBookPairs { get; }
	protected abstract int RewardSetId { get; }

	public override void Load()
	{
		SetId(QuestId);
		SetName(L("Library Delivery Part-time Job"));
		SetDescription(LQuestDescription);

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.ById);

		SetType(QuestType.Deliver);
		SetPtjType(PtjType.Library);
		SetLevel(QuestLevel.Basic);
		SetHours(start: 9, report: 11, deadline: 19);

		// Add objectives depending on specified NpcBookPairs and whether or not to DoShelving.
		var bookCount = NpcBookPairs.Length;

		if (DoShelving)
		{
			// Generates the following objectives and respective hooks:
			/* "Receive [{BookTitle1}] from {NpcName1} at {Location1} for return."
			 * ...
			 * "Receive [{BookTitleN}] from {NpcNameN} at {LocationN} for return."
			 * "Return the book to the [{BookcaseName1}] section."
			 * ...
			 * "Return the book to the [{BookcaseNameN}] section."
			 */

			for (int i = 0; i < bookCount; ++i)
			{
				var nbp = NpcBookPairs[i];
				var objectiveIdent = string.Concat("ptjTalk", i.ToString());

				AddObjective(objectiveIdent, GetLTalkDescription(nbp), 0, 0, 0, Talk(nbp.NpcIdent));
				AddHook(nbp.NpcIdent, "after_intro", GenerateNpcAfterIntroHook(objectiveIdent, nbp));
			}

			for (int i = 0; i < bookCount; ++i)
			{
				var nbp = NpcBookPairs[i];

				// Deliver objectives to bookshelves before reporting to Stewart
				var objectiveIdent = string.Concat("ptjDeliver", i.ToString());
				var bookcaseNpcIdent = GetMatchingBookcaseNpcIdent(nbp.ItemId);

				AddObjective(objectiveIdent, GetLDeliverDescription(bookcaseNpcIdent), 0, 0, 0, Deliver(nbp.ItemId, bookcaseNpcIdent));
				AddHook(bookcaseNpcIdent, "after_intro", GenerateBookcaseAfterIntroHook(objectiveIdent, nbp.ItemId));
			}
		}
		else
		{
			// Generates the following objectives and respective hooks:
			/* "Talk to {NpcName1} at {Location1}."
			 * "Receive [{BookTitle1}] for return." / if one book: "Receive the overdue [Book]."
			 * ...
			 * "Talk to {NpcNameN} at {LocationN}."
			 * "Receive [{BookTitleN}] for return."
			 */

			// Specially handle first objective pair due to check for single book.
			var nbp = NpcBookPairs[0];

			AddObjective("ptjTalk0", GetLTalkDescription(nbp.NpcIdent), 0, 0, 0, Talk(nbp.NpcIdent));
			AddHook(nbp.NpcIdent, "after_intro", GenerateNpcAfterIntroHook("ptjTalk0", nbp));

			AddObjective("ptjCollect0", bookCount == 1 ? L("Receive the overdue [Book].") : GetLCollectDescription(nbp.ItemId), 0, 0, 0, Collect(nbp.ItemId, 1));

			for (int i = 1; i < bookCount; ++i) // Skip first NpcBookPair; already handled.
			{
				nbp = NpcBookPairs[i];
				var objectiveTalkIdent = string.Concat("ptjTalk", i.ToString());

				AddObjective(objectiveTalkIdent, GetLTalkDescription(nbp.NpcIdent), 0, 0, 0, Talk(nbp.NpcIdent));
				AddHook(nbp.NpcIdent, "after_intro", GenerateNpcAfterIntroHook(objectiveTalkIdent, nbp));

				AddObjective(string.Concat("ptjCollect", i.ToString()), GetLCollectDescription(nbp.ItemId), 0, 0, 0, Collect(nbp.ItemId, 1));
			}
		}

		AddRewards();
	}

	/// <summary>
	/// Removes other quest items for PTJs involving more than one book.
	/// </summary>
	/// <param name="creature"></param>
	/// <remarks>
	/// Fix #414: Lingering books on quest completion for Stewart PTJ
	/// This might be removed when "any order" quest objective completion is implemented.
	/// </remarks>
	public override void OnComplete(Creature creature)
	{
		// Remove all books except the last one;
		// the last book is already taken care of automatically by CreatureQuests.Complete(Quest, int, bool) .
		for (int i = NpcBookPairs.Length - 2; i >= 0; --i)
			creature.RemoveItem(NpcBookPairs[i].ItemId);
	}

	private NpcScriptHook GenerateBookcaseAfterIntroHook(string objectiveIdent, int bookItemId)
	{
		return async (NpcScript npc, object[] args) =>
		{
			if (!npc.QuestActive(this.Id, objectiveIdent))
				return HookResult.Continue;

			if (!npc.Player.Inventory.Has(bookItemId))
				return HookResult.Continue;

			npc.Player.RemoveItem(bookItemId);
			npc.FinishQuest(this.Id, objectiveIdent);

			npc.End(L("(Placed the book in the bookshelf)"));

			return HookResult.End;
		};
	}

	private NpcScriptHook GenerateNpcAfterIntroHook(string objectiveIdent, NpcBookPair nbp)
	{
		return async (NpcScript npc, object[] args) =>
		{
			if (!npc.QuestActive(this.Id, objectiveIdent))
				return HookResult.Continue;

			npc.FinishQuest(this.Id, objectiveIdent);
			npc.Player.GiveItem(nbp.ItemId);
			npc.Notice(GetLItemReceivedNotice(nbp));

			await this.OnFinish(npc, nbp.NpcIdent);

			return HookResult.Break;
		};
	}

	protected async Task OnFinish(NpcScript npc, string npcIdent)
	{
		switch (npcIdent)
		{
			case "_aeira":
				npc.Msg(L("You're here to get the book that I borrowed from the library?<br/>Oh wow, I totally forgot about it!<p/>Umm... well... but then...<br/>I thought Stewart was going to come pick it up himself...<br/><username/>, you're here instead...<p/>What can I do...<br/>Please tell him I'm sorry!"));
				break;

			case "_austeyn": await Task.Yield(); break; // No response

			case "_eavan":
				npc.Msg(L("You are from the library, correct?<br/>Has the due date passed already? I hadn't checked the date, and...<br/>Here it is. Thank you for making your way here to get the book."));
				break;

			case "_glenis": await Task.Yield(); break; // No response

			case "_kristell":
				npc.Msg(L("Ahh... You're here because of the book I borrowed from the library?<p/>I am sorry... I was just thinking of reading it over one more time, and...<br/>I went past the due date.<br/>I'll borrow it again some other time, so here it is.<p/>Please tell Stewart I feel bad about this."));
				break;

			case "_manus":
				npc.Msg(L("Oh, you mean the book I borrowed? Here it is!!<br/>I had been so busy with work lately, that I forgot about it.<br/>I wasn't even able to read half of it... I'll borrow it some other time..<p/>Hopefully Stewart didn't get mad at me...<br/>Haha, anyway, thanks for returning this book for me.<br/>"));
				break;

			case "_nerys":
				npc.Msg(L("Hey, I was just about to return this... and you showed up!<br/>It'd be nice if you could come by and deliver some books sometimes,<br/>instead of just collecting...<br/>Well, tell Stewart I enjoyed reading it."));
				break;

			case "_simon":
				npc.Msg(L("Hmmm? You're from the library?<br/>Oh no...I completely forgot about it.<br/>I was too busy working with all the customers...you know what I mean?<p/>Here it is, please return it to Stewart.<br/>See you!"));
				break;

			case "_tracy":
				npc.Msg(L("What? He wants it back already?<br/>Geez... can't he just let me borrow it for a little bit longer?<br/>Oh well... tell him I'm going to check it out again soon.<br/>Oh, and also tell him to have the next volume ready!"));
				break;

			case "_walter": await Task.Yield(); break; // No response

			default:
				Log.Error("Stewart PTJ Script: No defined OnFinish dialogue when picking up book from NPC {1}", npcIdent);
				npc.Msg("(missing): dialogue undefined");
				break;
		}
	}

	private void AddRewards()
	{
		switch (RewardSetId)
		{
			case 1:
				AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(160));
				AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(200));
				AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(80));
				AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(100));
				AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(32));
				AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(40));

				AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(65));
				AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(280));
				AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(32));
				AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(140));
				AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(13));
				AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(56));

				AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(335));
				AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(70));
				AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(167));
				AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(35));
				AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(67));
				AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(14));

				AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(51006, 10)); // MP 10 Potion
				AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(195));

				AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(51001, 6)); // HP 10 Potion
				AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(45));

				AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(51011, 10)); // Stamina 10 Potion
				AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(245));
				break;

			case 2:
				AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(200));
				AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(200));
				AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(100));
				AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(100));
				AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(40));
				AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(40));

				AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(75));
				AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(300));
				AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(37));
				AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(150));
				AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(15));
				AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(60));

				AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(375));
				AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(70));
				AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(187));
				AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(35));
				AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(75));
				AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(14));

				AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(51006, 10)); // MP 10 Potion
				AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(225));

				AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(51001, 7)); // HP 10 Potion
				AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(25));

				AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(51011, 10)); // Stamina 10 Potion
				AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(275));
				break;

			case 3:
				AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(240));
				AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(240));
				AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(120));
				AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(120));
				AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(48));
				AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(48));

				AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(85));
				AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(360));
				AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(42));
				AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(180));
				AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(17));
				AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(72));

				AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(440));
				AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(90));
				AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(220));
				AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(45));
				AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(88));
				AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(18));

				AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(51006, 10)); // MP 10 Potion
				AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(295));

				AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(51001, 8)); // HP 10 Potion
				AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(45));

				AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(51011, 10)); // Stamina 10 Potion
				AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(345));

				AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(18006)); // Wizard Hat
				AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(45));
				break;

			case 4:
				AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(200));
				AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(200));
				AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(100));
				AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(100));
				AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(40));
				AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(40));

				AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(75));
				AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(300));
				AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(37));
				AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(150));
				AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(15));
				AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(60));

				AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(375));
				AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(70));
				AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(187));
				AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(35));
				AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(75));
				AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(14));

				AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(51006, 10)); // MP 10 Potion
				AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(225));

				AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(51001, 7)); // HP 10 Potion
				AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(25));

				AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(51011, 10)); // Stamina 10 Potion
				AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(275));

				AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(18006)); // Wizard Hat
				AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(45));
				break;

			case 5:
				AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(160));
				AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(200));
				AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(80));
				AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(100));
				AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(32));
				AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(40));

				AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(65));
				AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(280));
				AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(32));
				AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(140));
				AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(13));
				AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(56));

				AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(335));
				AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(70));
				AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(167));
				AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(35));
				AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(67));
				AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(14));

				AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(51006, 10)); // MP 10 Potion
				AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(195));

				AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(51001, 6)); // HP 10 Potion
				AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(45));

				AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(51011, 10)); // Stamina 10 Potion
				AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(245));

				AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(18006)); // Wizard Hat
				AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(45));
				break;

			case 6:
				AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(340));
				AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(460));
				AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(170));
				AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(230));
				AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(68));
				AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(92));

				AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(160));
				AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(600));
				AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(80));
				AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(300));
				AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(32));
				AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(120));

				AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(760));
				AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(140));
				AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(380));
				AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(70));
				AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(152));
				AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(28));

				AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(51006, 10)); // MP 10 Potion
				AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(590));

				AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(51001, 6)); // HP 10 Potion
				AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(320));

				AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(51011, 10)); // Stamina 10 Potion
				AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(640));

				AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(63000, 5)); // Phoenix Feather
				AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(240));
				break;

			case 7:
				AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(300));
				AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(400));
				AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(150));
				AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(200));
				AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(60));
				AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(80));

				AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(140));
				AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(520));
				AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(70));
				AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(260));
				AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(28));
				AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(104));

				AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(660));
				AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(130));
				AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(330));
				AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(65));
				AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(132));
				AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(26));

				AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(51006, 10)); // MP 10 Potion
				AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(500));

				AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(51001, 6)); // HP 10 Potion
				AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(150));

				AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(51011, 10)); // Stamina 10 Potion
				AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(550));

				AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(63000, 5)); // Phoenix Feather
				AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(150));
				break;

			case 8:
				AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(400));
				AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(480));
				AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(200));
				AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(240));
				AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(80));
				AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(96));

				AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(175));
				AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(650));
				AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(87));
				AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(325));
				AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(35));
				AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(130));

				AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(825));
				AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(160));
				AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(412));
				AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(80));
				AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(165));
				AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(32));

				AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(51006, 10)); // MP 10 Potion
				AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(655));

				AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(51001, 6)); // HP 10 Potion
				AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(352));

				AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(51011, 10)); // Stamina 10 Potion
				AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(705));

				AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(63000, 5)); // Phoenix Feather
				AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(305));
				break;

			case 9:
				AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(420));
				AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(500));
				AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(210));
				AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(250));
				AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(84));
				AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(100));

				AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(185));
				AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(675));
				AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(92));
				AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(337));
				AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(37));
				AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(135));

				AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(875));
				AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(160));
				AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(437));
				AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(80));
				AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(175));
				AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(32));

				AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(51006, 10)); // MP 10 Potion
				AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(690));

				AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(51001, 6)); // HP 10 Potion
				AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(370));

				AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(51011, 10)); // Stamina 10 Potion
				AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(740));

				AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(63000, 5)); // Phoenix Feather
				AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(340));
				break;

			case 10:
				AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(400));
				AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(500));
				AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(200));
				AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(250));
				AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(80));
				AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(100));

				AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(180));
				AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(665));
				AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(90));
				AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(332));
				AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(36));
				AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(133));

				AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(855));
				AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(160));
				AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(427));
				AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(80));
				AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(171));
				AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(32));

				AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(51006, 10)); // MP 10 Potion
				AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(675));

				AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(51001, 6)); // HP 10 Potion
				AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(325));

				AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(51011, 10)); // Stamina 10 Potion
				AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(725));

				AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(63000, 5)); // Phoenix Feather
				AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(325));
				break;

			case 11:
				AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(420));
				AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(510));
				AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(210));
				AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(255));
				AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(84));
				AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(102));

				AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(185));
				AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(685));
				AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(92));
				AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(342));
				AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(37));
				AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(137));

				AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(880));
				AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(165));
				AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(440));
				AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(82));
				AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(176));
				AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(33));

				AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(51006, 10)); // MP 10 Potion
				AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(700));

				AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(51001, 10)); // HP 10 Potion
				AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(350));

				AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(51011, 10)); // Stamina 10 Potion
				AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(750));

				AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(63000, 5)); // Phoenix Feather
				AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(350));
				break;

			case 12:
				AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(450));
				AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(530));
				AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(225));
				AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(265));
				AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(90));
				AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(104));

				AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(200));
				AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(720));
				AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(100));
				AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(360));
				AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(40));
				AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(144));

				AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(920));
				AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(175));
				AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(460));
				AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(87));
				AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(184));
				AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(35));

				AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(51006, 10)); // MP 10 Potion
				AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(742));

				AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(51001, 10)); // HP 10 Potion
				AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(396));

				AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(51011, 10)); // Stamina 10 Potion
				AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(792));

				AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(63000, 5)); // Phoenix Feather
				AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(392));
				break;

			case 13:
				AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(520));
				AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(620));
				AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(260));
				AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(310));
				AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(104));
				AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(124));

				AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(240));
				AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(830));
				AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(120));
				AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(415));
				AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(48));
				AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(166));

				AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(1080));
				AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(200));
				AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(540));
				AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(100));
				AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(216));
				AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(40));

				AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(51006, 10)); // MP 10 Potion
				AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(885));

				AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(51001, 10)); // HP 10 Potion
				AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(467));

				AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(51011, 10)); // Stamina 10 Potion
				AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(935));

				AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(63000, 5)); // Phoenix Feather
				AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(535));
				break;

			case 14:
				AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(500));
				AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(600));
				AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(250));
				AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(300));
				AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(100));
				AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(120));

				AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(230));
				AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(800));
				AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(115));
				AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(400));
				AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(46));
				AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(160));

				AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(1030));
				AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(200));
				AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(515));
				AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(100));
				AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(206));
				AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(40));

				AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(51006, 10)); // MP 10 Potion
				AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(850));

				AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(51001, 10)); // HP 10 Potion
				AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(500));

				AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(51011, 10)); // Stamina 10 Potion
				AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(900));

				AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(63000, 5)); // Phoenix Feather
				AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(500));
				break;

			default:
				Log.Warning("Stewart PTJ Script: Reward set ID {0} is not defined. Fell back to reward set 1.");
				goto case 1;
		}
	}

	/// <summary>
	/// Returns the NPC identifier of the bookcase corresponding to the specified <paramref name="bookItemId"/>.
	/// <para>If no matching bookcase NPC identifier is found, returns fallback _bookcase05, [Specialties] section.</para>
	/// </summary>
	/// <param name="bookItemId"></param>
	/// <returns></returns>
	/// <remarks>Not all books need to be shelved.</remarks>
	private string GetMatchingBookcaseNpcIdent(int bookItemId)
	{
		switch (bookItemId)
		{
			case 70051: // Family Meals by the Experts (3)
			case 70053: // Crossing the Brifne Peninsula
			case 70054: // Your Guide to Organizing Information
			case 70057: // 10 Things You Should Do for Your Health
				return "_bookcase01"; // [Self Improvement] section

			case 70040: // Aliech Poetry Book
			case 70042: // Wolves in the Snowfield
			case 70066: // Braden's Big Adventure (2)
			case 70068: // Braden's Big Adventure (4)
				return "_bookcase02"; // [Art/Literature] section

			case 70044: // Swept Away by Trends
			case 70046: // Business Made Easy
			case 70056: // Plants from Ulaid
			case 70062: // Ores in Bangor
				return "_bookcase03"; // [Literature/Philosophy] section

			case 70043: // History of Clothes in Aliech (Pt. 1)
			case 70049: // History of Lymilark: the Religion
				return "_bookcase04"; // [History] section

			case 70047: // Business Rules for Maximizing Profit
			case 70058: // Best Wood in Devenish Forest
			case 70060: // How to Make a String Instrument
			case 70061: // The ABCs of Swordsmanship
				return "_bookcase05"; // [Specialties] section

			case 70063: // Latest Issue of Erinn Walker
				return "_bookcase06"; // [Magazine] section

			default:
				Log.Warning("Stewart PTJ Script: No matching bookcase NPC identifier matched with given book item ID {0}. Fell back to _bookcase05, [Specialties] section.", bookItemId);
				return "_bookcase05"; // [Specialties] section
		}
	}

	/// <summary>
	/// Returns localised notice text in the forme of 
	/// <para>"You have received {BookTitle} requested for delivery from {NpcName}."</para>
	/// <para>But if there is no definition for the specified NPC and book item ID, a log warning is issued and a blank string is returned.</para>
	/// </summary>
	/// <param name="nbp">NPC and book item ID to fetch notice text for</param>
	/// <returns></returns>
	private string GetLItemReceivedNotice(NpcBookPair nbp)
	{
		switch (nbp.NpcIdent)
		{
			case "_aeira":
				switch (nbp.ItemId)
				{
					case 70040: return L("You have received Aliech Poetry Book requested for delivery from Aeira.");
					case 70041: return L("You have received A Study on the Rules of Sustaining Mana Caused by Erg requested for delivery from Aeira.");
					case 70042: return L("You have received Wolves in the Snowfield requested for delivery from Aeira.");
				} break;

			case "_austeyn":
				switch (nbp.ItemId)
				{
					case 70046: return L("You have received Business Made Easy requested for delivery from Austeyn.");
					case 70047: return L("You have received Business Rules for Maximizing Profit requested for delivery from Austeyn.");
					case 70048: return L("You have received Book of Statistics requested for delivery from Austeyn.");
				} break;

			case "_eavan":
				switch (nbp.ItemId)
				{
					case 70054: return L("You have received Your Guide to Organizing Information requested for delivery from Eavan.");
					case 70055: return L("You have received Devenish Love Song requested for delivery from Eavan.");
				} break;

			case "_glenis":
				switch (nbp.ItemId)
				{
					case 70051: return L("You have received Family Meals by the Experts (3) requested for delivery from Glenis.");
					case 70052: return L("You have received Easy Guide to World History requested for delivery from Glenis.");
					case 70053: return L("You have received Crossing the Brifne Peninsula requested for delivery from Glenis.");
				} break;

			case "_kristell":
				switch (nbp.ItemId)
				{
					case 70049: return L("You have received History of Lymilark: the Religion requested for delivery from Kristell.");
					case 70050: return L("You have received Great Poetry in Motion requested for delivery from Kristell.");
				} break;

			case "_manus":
				switch (nbp.ItemId)
				{
					case 70056: return L("You have received Plants from Ulaid requested for delivery from Manus.");
					case 70057: return L("You have received 10 Things You Should Do for Your Health requested for delivery from Manus.");
				} break;

			case "_nerys":
				switch (nbp.ItemId)
				{
					case 70061: return L("You have received The ABCs of Swordsmanship requested for delivery from Nerys.");
					case 70062: return L("You have received Ores in Bangor requested for delivery from Nerys.");
					case 70063: return L("You have received Latest Issue of Erinn Walker requested for delivery from Nerys.");
				} break;

			case "_simon":
				switch (nbp.ItemId)
				{
					case 70043: return L("You have received History of Clothes in Aliech (Pt. 1) requested for delivery from Simon.");
					case 70044: return L("You have received Swept Away by Trends requested for delivery from Simon.");
					case 70045: return L("You have received Gorgeous Colors requested for delivery from Simon.");
				} break;

			case "_tracy":
				switch (nbp.ItemId)
				{
					case 70066: return L("You have received Braden's Big Adventure (2) requested for delivery from Tracy.");
					case 70067: return L("You have received Braden's Big Adventure (3) requested for delivery from Tracy.");
					case 70068: return L("You have received Braden's Big Adventure (4) requested for delivery from Tracy.");
				} break;

			case "_walter":
				switch (nbp.ItemId)
				{
					case 70058: return L("You have received Best Wood in Devenish Forest requested for delivery from Walter.");
					case 70059: return L("You have received Household Goods Everyone Can Make requested for delivery from Walter.");
					case 70060: return L("You have received How to Make a String Instrument requested for delivery from Walter.");
				} break;
		}

		Log.Warning("Stewart PTJ Script: No defined objective description for picking up item ID {0} from NPC {1}", nbp.ItemId, nbp.NpcIdent);
		return "";
	}

	/// <summary>
	/// Returns a localised objective description in the forme of 
	/// <para>"Receive [{BookTitle}] from {NpcName} at {Location} for return."</para>
	/// <para>But if there is no definition for the specified NPC and book item ID, issues log warning and</para>
	/// <para>returns "Receive a book from {NpcIdent} for return."</para>
	/// <para>Intended to be used when shelving books.</para>
	/// </summary>
	/// <param name="nbp">NPC and book item ID to fetch an objective description for</param>
	/// <returns></returns>
	/// <remarks>
	/// Some texts have been shortened to account for textbox overflow:
	/// https://files.gitter.im/aura-project/aura/4QYo/blob
	/// </remarks>
	private string GetLTalkDescription(NpcBookPair nbp)
	{
		switch (nbp.NpcIdent)
		{
			case "_aeira":
				switch (nbp.ItemId)
				{
					case 70040: return L("Receive [Aliech Poetry Book] from Aeira at the bookstore for return.");
					case 70041: return L("Receive [A Study on the Rules of Sustaining Mana Caused by Erg] from Aeira at the bookstore for return.");
					case 70042: return L("Receive [Wolves in the Snowfield] from Aeira at the bookstore for return.");
				} break;

			case "_austeyn":
				switch (nbp.ItemId)
				{
					case 70046: return L("Receive [Business Made Easy] from Austeyn at the bank for return.");
					//case 70047: return L("Receive [Business Rules for Maximizing Profit] from Austeyn at the bank for return.");
					case 70047: return L("Receive [Business Rules for Max. Profit] from Austeyn at the bank for return.");
					case 70048: return L("Receive [Book of Statistics] from Austeyn at the bank for return.");
				} break;

			case "_eavan":
				switch (nbp.ItemId)
				{
					//case 70054: return L("Receive [Your Guide to Organizing Information] from Eavan at town hall for return.");
					case 70054: return L("Receive [Your Guide to Organizing Info.] from Eavan at town hall for return.");
					case 70055: return L("Receive [Devenish Love Song] from Eavan at town hall for return.");
				} break;

			case "_glenis":
				switch (nbp.ItemId)
				{
					//case 70051: return L("Receive [Family Meals by the Experts (3)] from Glenis at the grocery store for return.");
					case 70051: return L("Receive [Family Meals by Experts] from Glenis at the grocery store for return.");
					case 70052: return L("Receive [Easy Guide to World History] from Glenis at the grocery store for return.");
					//case 70053: return L("Receive [Crossing the Brifne Peninsula] from Glenis at the grocery store for return.");
					case 70053: return L("Receive [Crossing the Brifne Peninsula] from Glenis at the grocery store.");
				} break;

			case "_kristell":
				switch (nbp.ItemId)
				{
					//case 70049: return L("Receive [History of Lymilark: the Religion] from Kristell at the church for return.");
					case 70049: return L("Receive [History of Lymilark] from Kristell at the church for return.");
					case 70050: return L("Receive [Great Poetry in Motion] from Kristell at the church for return.");
				} break;

			case "_manus":
				switch (nbp.ItemId)
				{
					case 70056: return L("Receive [Plants from Ulaid] from Manus at the healer's house for return.");
					//case 70057: return L("Receive [10 Things You Should Do for Your Health] from Manus at the healer's house for return.");
					case 70057: return L("Receive [10 Things for Your Health] from Manus at the healer's house.");
				} break;

			case "_nerys":
				switch (nbp.ItemId)
				{
					case 70061: return L("Receive [The ABCs of Swordsmanship] from Nerys at the weapon shop for return.");
					case 70062: return L("Receive [Ores in Bangor] from Nerys at the weapon shop for return.");
					case 70063: return L("Receive [Latest Issue of Erinn Walker] from Nerys at the weapon shop for return.");
				} break;

			case "_simon":
				switch (nbp.ItemId)
				{
					//case 70043: return L("Receive [History of Clothes in Aliech (Pt. 1)] from Simon at the clothing store for return.");
					case 70043: return L("Receive [History of Clothes in Aliech] from Simon at the clothing store for return.");
					case 70044: return L("Receive [Swept Away by Trends] from Simon at the clothing store for return.");
					case 70045: return L("Receive [Gorgeous Colors] from Simon at the clothing store for return.");
				} break;

			case "_tracy":
				switch (nbp.ItemId)
				{
					case 70066: return L("Receive [Braden's Big Adventure (2)] from Tracy at the logging camp for return.");
					case 70067: return L("Receive [Braden's Big Adventure (3)] from Tracy at the logging camp for return.");
					case 70068: return L("Receive [Braden's Big Adventure (4)] from Tracy at the logging camp for return.");
				} break;

			case "_walter":
				switch (nbp.ItemId)
				{
					//case 70058: return L("Receive [Best Wood in Devenish Forest] from Walter at the general store for return.");
					case 70058: return L("Receive [Best Wood in Devenish] from Walter at the general store for return.");
					case 70059: return L("Receive [Household Goods Everyone Can Make] from Walter at the general store for return.");
					//case 70060: return L("Receive [How to Make a String Instrument] from Walter at the general store for return.");
					case 70060: return L("Receive [Making a String Instrument] from Walter at the general store for return.");
				} break;
		}

		Log.Warning("Stewart PTJ Script: No defined objective description for picking up item ID {0} from NPC {1}", nbp.ItemId, nbp.NpcIdent);
		return string.Format(L("Receive a book from {0} for return."), nbp.NpcIdent);
	}

	/// <summary>
	/// Returns a localised objective description in the forme of 
	/// <para>"Talk to {NpcName} at {Location}."</para>
	/// <para>But if there is no definition for the specified NPC identifier, issues log warning and</para>
	/// <para>returns "Talk to {NpcIdent}."</para>
	/// <para>Intended to be used when not shelving books.</para>
	/// </summary>
	/// <param name="npcIdent">NPC identifier to fetch an objective description for</param>
	/// <returns></returns>
	private string GetLTalkDescription(string npcIdent)
	{
		switch (npcIdent)
		{
			case "_aeira": return L("Talk to Aeira at the bookstore.");
			case "_austeyn": return L("Talk to Austeyn at the bank.");
			case "_eavan": return L("Talk to Eavan at town hall.");
			case "_glenis": return L("Talk to Glenis at the grocery store.");
			case "_kristell": return L("Talk to Kristell at the church.");
			case "_manus": return L("Talk to Manus at the healer's house.");
			case "_nerys": return L("Talk to Nerys at the weapon shop.");
			case "_simon": return L("Talk to Simon at the clothing store.");
			case "_tracy": return L("Talk to Tracy at the logging camp.");
			case "_walter": return L("Talk to Walter at the general store.");
		}

		Log.Warning("Stewart PTJ Script: No defined objective description for talking to NPC {1}", npcIdent);
		return string.Format(L("Talk to {0}."), npcIdent);
	}

	/// <summary>
	/// Returns a localised objective description in the forme of 
	/// <para>"Return the book to the [{BookcaseName}] section."</para>
	/// <para>Intended to be used when shelving books.</para>
	/// </summary>
	/// <param name="bookcaseNpcIdent">Bookcase NPC identifier to fetch an objective description for</param>
	/// <returns></returns>
	/// <exception cref="System.ArgumentException">Thrown if there is no definition for the specified bookcase NPC identifier. There are only bookcases named /"_bookcase0[1-6]"/ .</exception>
	private string GetLDeliverDescription(string bookcaseNpcIdent)
	{
		switch (bookcaseNpcIdent)
		{
			case "_bookcase01": return L("Return the book to the [Self Improvement] section.");
			case "_bookcase02": return L("Return the book to the [Art/Literature] section.");
			case "_bookcase03": return L("Return the book to the [Literature/Philosophy] section.");
			case "_bookcase04": return L("Return the book to the [History] section.");
			case "_bookcase05": return L("Return the book to the [Specialties] section.");
			case "_bookcase06": return L("Return the book to the [Magazine] section.");

			default: throw new ArgumentException(string.Format("Stewart PTJ Script: No defined objective description for delivering to NPC {0}; no such bookcase NPC.", bookcaseNpcIdent));
		}
	}

	/// <summary>
	/// Returns a localised objective description in the forme of 
	/// <para>"Receive [{BookTitle}] for return."</para>
	/// <para>But if there is no definition for the specified book item ID, issues log warning and</para>
	/// <para>returns "Receive the overdue [Book]."</para>
	/// <para>Intended to be used when not shelving books.</para>
	/// </summary>
	/// <param name="itemId">Book item ID to fetch an objective description for</param>
	/// <returns></returns>
	private string GetLCollectDescription(int itemId)
	{
		switch (itemId)
		{
			case 70040: return L("Receive [Aliech Poetry Book] for return.");
			case 70041: return L("Receive [A Study on the Rules of Sustaining Mana Caused by Erg] for return.");
			case 70042: return L("Receive [Wolves in the Snowfield] for return.");
			case 70043: return L("Receive [History of Clothes in Aliech (Pt. 1)] for return.");
			case 70044: return L("Receive [Swept Away by Trends] for return.");
			case 70045: return L("Receive [Gorgeous Colors] for return.");
			case 70046: return L("Receive [Business Made Easy] for return.");
			case 70047: return L("Receive [Business Rules for Maximizing Profit] for return.");
			case 70048: return L("Receive [Book of Statistics] for return.");
			case 70049: return L("Receive [History of Lymilark: the Religion] for return.");
			case 70050: return L("Receive [Great Poetry in Motion] for return.");
			case 70051: return L("Receive [Family Meals by the Experts (3)] for return.");
			case 70052: return L("Receive [Easy Guide to World History] for return.");
			case 70053: return L("Receive [Crossing the Brifne Peninsula] for return.");
			case 70054: return L("Receive [Your Guide to Organizing Information] for return.");
			case 70055: return L("Receive [Devenish Love Song] for return.");
			case 70056: return L("Receive [Plants from Ulaid] for return.");
			case 70057: return L("Receive [10 Things You Should Do for Your Health] for return.");
			case 70058: return L("Receive [Best Wood in Devenish Forest] for return.");
			case 70059: return L("Receive [Household Goods Everyone Can Make] for return.");
			case 70060: return L("Receive [How to Make a String Instrument] for return.");
			case 70061: return L("Receive [The ABCs of Swordsmanship] for return.");
			case 70062: return L("Receive [Ores in Bangor] for return.");
			case 70063: return L("Receive [Latest Issue of Erinn Walker] for return.");
			case 70066: return L("Receive [Braden's Big Adventure (2)] for return.");
			case 70067: return L("Receive [Braden's Big Adventure (3)] for return.");
			case 70068: return L("Receive [Braden's Big Adventure (4)] for return.");

			default:
				Log.Warning("Stewart PTJ Script: No defined objective description for collecting book item ID {0}. Fell back to generic \"Receive the overdue [Book].\"", itemId);
				return L("Receive the overdue [Book].");
		}
	}
}

public class StewartLibraryAeira1PtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return false; } }

	protected override int QuestId { get { return 519401; } }
	protected override string LQuestDescription { get { return L("Aeira from the bookstore hasn't returned [Aliech Poetry Book] in a long time. Can you please get it for me? - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs { get { return new NpcBookPair[] { new NpcBookPair { NpcIdent = "_aeira", ItemId = 70040 } }; } }
	protected override int RewardSetId { get { return 1; } }
}

public class StewartLibraryAeira2PtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return false; } }

	protected override int QuestId { get { return 519402; } }
	protected override string LQuestDescription { get { return L("Aeira from the bookstore hasn't returned [A Study on the Rules of Sustaining Mana Caused by Erg] in a long time. Can you please get it for me? - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs { get { return new NpcBookPair[] { new NpcBookPair { NpcIdent = "_aeira", ItemId = 70041 } }; } }
	protected override int RewardSetId { get { return 1; } }
}

public class StewartLibraryAeira3PtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return false; } }

	protected override int QuestId { get { return 519403; } }
	protected override string LQuestDescription { get { return L("Aeira from the bookstore hasn't returned [Wolves in the Snowfield] in a long time. Can you please get it for me? - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs { get { return new NpcBookPair[] { new NpcBookPair { NpcIdent = "_aeira", ItemId = 70042 } }; } }
	protected override int RewardSetId { get { return 1; } }
}

public class StewartLibrarySimon1PtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return false; } }

	protected override int QuestId { get { return 519404; } }
	protected override string LQuestDescription { get { return L("Simon from the clothing shop hasn't returned [History of Clothes in Aliech (Pt. 1)] in a long time. Can you please get it for me? - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs { get { return new NpcBookPair[] { new NpcBookPair { NpcIdent = "_simon", ItemId = 70043 } }; } }
	protected override int RewardSetId { get { return 2; } }
}

public class StewartLibrarySimon2PtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return false; } }

	protected override int QuestId { get { return 519405; } }
	protected override string LQuestDescription { get { return L("Simon from the clothing shop hasn't returned [Swept Away by Trends] in a long time. Can you please get it for me? - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs { get { return new NpcBookPair[] { new NpcBookPair { NpcIdent = "_simon", ItemId = 70044 } }; } }
	protected override int RewardSetId { get { return 2; } }
}

public class StewartLibrarySimon3PtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return false; } }

	protected override int QuestId { get { return 519406; } }
	protected override string LQuestDescription { get { return L("Simon from the clothing shop hasn't returned [Gorgeous Colors] in a long time. Can you please get it for me? - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs { get { return new NpcBookPair[] { new NpcBookPair { NpcIdent = "_simon", ItemId = 70045 } }; } }
	protected override int RewardSetId { get { return 2; } }
}

public class StewartLibraryAusteyn1PtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return false; } }

	protected override int QuestId { get { return 519407; } }
	protected override string LQuestDescription { get { return L("Austeyn from the bank hasn't returned [Business Made Easy] in a long time. Can you please get it for me? - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs { get { return new NpcBookPair[] { new NpcBookPair { NpcIdent = "_austeyn", ItemId = 70046 } }; } }
	protected override int RewardSetId { get { return 2; } }
}

public class StewartLibraryAusteyn2PtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return false; } }

	protected override int QuestId { get { return 519408; } }
	protected override string LQuestDescription { get { return L("Austeyn from the bank hasn't returned [Business Rules for Maximizing Profit] in a long time. Can you please get it for me? - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs { get { return new NpcBookPair[] { new NpcBookPair { NpcIdent = "_austeyn", ItemId = 70047 } }; } }
	protected override int RewardSetId { get { return 2; } }
}

public class StewartLibraryAusteyn3PtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return false; } }

	protected override int QuestId { get { return 519409; } }
	protected override string LQuestDescription { get { return L("Austeyn from the bank hasn't returned [Book of Statistics] in a long time. Can you please get it for me? - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs { get { return new NpcBookPair[] { new NpcBookPair { NpcIdent = "_austeyn", ItemId = 70048 } }; } }
	protected override int RewardSetId { get { return 2; } }
}

public class StewartLibraryKristell1PtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return false; } }

	protected override int QuestId { get { return 519410; } }
	protected override string LQuestDescription { get { return L("Kristell from the church hasn't returned [History of Lymilark: the Religion] in a long time. Can you please get it for me? - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs { get { return new NpcBookPair[] { new NpcBookPair { NpcIdent = "_kristell", ItemId = 70049 } }; } }
	protected override int RewardSetId { get { return 3; } }
}

public class StewartLibraryKristell2PtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return false; } }

	protected override int QuestId { get { return 519411; } }
	protected override string LQuestDescription { get { return L("Kristell from the church hasn't returned [Great Poetry in Motion] in a long time. Can you please get it for me? - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs { get { return new NpcBookPair[] { new NpcBookPair { NpcIdent = "_kristell", ItemId = 70050 } }; } }
	protected override int RewardSetId { get { return 3; } }
}

public class StewartLibraryGlenis1PtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return false; } }

	protected override int QuestId { get { return 519412; } }
	protected override string LQuestDescription { get { return L("Glenis from the grocery store hasn't returned [Family Meals by the Experts (3)] in a long time. Can you please get it for me? - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs { get { return new NpcBookPair[] { new NpcBookPair { NpcIdent = "_glenis", ItemId = 70051 } }; } }
	protected override int RewardSetId { get { return 4; } }
}

public class StewartLibraryGlenis2PtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return false; } }

	protected override int QuestId { get { return 519413; } }
	protected override string LQuestDescription { get { return L("Glenis from the grocery store hasn't returned [Easy Guide to World History] in a long time. Can you please get it for me? - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs { get { return new NpcBookPair[] { new NpcBookPair { NpcIdent = "_glenis", ItemId = 70052 } }; } }
	protected override int RewardSetId { get { return 4; } }
}

public class StewartLibraryGlenis3PtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return false; } }

	protected override int QuestId { get { return 519414; } }
	protected override string LQuestDescription { get { return L("Glenis from the grocery store hasn't returned [Crossing the Brifne Peninsula] in a long time. Can you please get it for me? - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs { get { return new NpcBookPair[] { new NpcBookPair { NpcIdent = "_glenis", ItemId = 70053 } }; } }
	protected override int RewardSetId { get { return 4; } }
}

public class StewartLibraryEavan1PtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return false; } }

	protected override int QuestId { get { return 519415; } }
	protected override string LQuestDescription { get { return L("Eavan from the town office hasn't returned [Your Guide to Organizing Information] in a long time. Can you please get it for me? - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs { get { return new NpcBookPair[] { new NpcBookPair { NpcIdent = "_eavan", ItemId = 70054 } }; } }
	protected override int RewardSetId { get { return 5; } }
}

public class StewartLibraryEavan2PtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return false; } }

	protected override int QuestId { get { return 519416; } }
	protected override string LQuestDescription { get { return L("Eavan from the town office hasn't returned [Devenish Love Song] in a long time. Can you please get it for me? - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs { get { return new NpcBookPair[] { new NpcBookPair { NpcIdent = "_eavan", ItemId = 70055 } }; } }
	protected override int RewardSetId { get { return 5; } }
}

public class StewartLibraryManus1PtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return false; } }

	protected override int QuestId { get { return 519417; } }
	protected override string LQuestDescription { get { return L("Manus from the healer's house hasn't returned [Plants from Ulaid] in a long time. Can you please get it for me? - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs { get { return new NpcBookPair[] { new NpcBookPair { NpcIdent = "_manus", ItemId = 70056 } }; } }
	protected override int RewardSetId { get { return 5; } }
}

public class StewartLibraryManus2PtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return false; } }

	protected override int QuestId { get { return 519418; } }
	protected override string LQuestDescription { get { return L("Manus from the healer's house hasn't returned [10 Things You Should Do for Your Health] in a long time. Can you please get it for me? - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs { get { return new NpcBookPair[] { new NpcBookPair { NpcIdent = "_manus", ItemId = 70057 } }; } }
	protected override int RewardSetId { get { return 5; } }
}

public class StewartLibraryWalter1PtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return false; } }

	protected override int QuestId { get { return 519419; } }
	protected override string LQuestDescription { get { return L("Walter from the general store hasn't returned [Best Wood in Devenish Forest] in a long time. Can you please get it for me? - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs { get { return new NpcBookPair[] { new NpcBookPair { NpcIdent = "_walter", ItemId = 70058 } }; } }
	protected override int RewardSetId { get { return 4; } }
}

public class StewartLibraryWalter2PtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return false; } }

	protected override int QuestId { get { return 519420; } }
	protected override string LQuestDescription { get { return L("Walter from the general store hasn't returned [Household Goods Everyone Can Make] in a long time. Can you please get it for me? - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs { get { return new NpcBookPair[] { new NpcBookPair { NpcIdent = "_walter", ItemId = 70059 } }; } }
	protected override int RewardSetId { get { return 4; } }
}

public class StewartLibraryWalter3PtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return false; } }

	protected override int QuestId { get { return 519421; } }
	protected override string LQuestDescription { get { return L("Walter from the general store hasn't returned [How to Make a String Instrument] in a long time. Can you please get it for me? - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs { get { return new NpcBookPair[] { new NpcBookPair { NpcIdent = "_walter", ItemId = 70060 } }; } }
	protected override int RewardSetId { get { return 4; } }
}

public class StewartLibraryNerys1PtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return false; } }

	protected override int QuestId { get { return 519422; } }
	protected override string LQuestDescription { get { return L("Nerys from the weapon shop hasn't returned [The ABCs of Swordsmanship] in a long time. Can you please get it for me? - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs { get { return new NpcBookPair[] { new NpcBookPair { NpcIdent = "_nerys", ItemId = 70061 } }; } }
	protected override int RewardSetId { get { return 5; } }
}

public class StewartLibraryNerys2PtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return false; } }

	protected override int QuestId { get { return 519423; } }
	protected override string LQuestDescription { get { return L("Nerys from the weapon shop hasn't returned [Ores in Bangor] in a long time. Can you please get it for me? - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs { get { return new NpcBookPair[] { new NpcBookPair { NpcIdent = "_nerys", ItemId = 70062 } }; } }
	protected override int RewardSetId { get { return 5; } }
}

public class StewartLibraryNerys3PtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return false; } }

	protected override int QuestId { get { return 519424; } }
	protected override string LQuestDescription { get { return L("Nerys from the weapon shop hasn't returned [Latest Issue of Erinn Walker] in a long time. Can you please get it for me? - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs { get { return new NpcBookPair[] { new NpcBookPair { NpcIdent = "_nerys", ItemId = 70063 } }; } }
	protected override int RewardSetId { get { return 5; } }
}

public class StewartExtLibraryAeiraEavanPtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return true; } }

	protected override int QuestId { get { return 519431; } }
	protected override string LQuestDescription { get { return L("Aeira from the bookstore and Eavan from the town office haven't returned their [Borrowed Book from the Library], so can you get the books from them for me? I'm really busy right now, so I'd also appreciate it if you could put the books back on the shelves for me. - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs
	{
		get
		{
			return new NpcBookPair[] {
				new NpcBookPair { NpcIdent = "_aeira", ItemId = 70040 },
				new NpcBookPair { NpcIdent = "_eavan", ItemId = 70054 },
			};
		}
	}
	protected override int RewardSetId { get { return 6; } }
}

public class StewartLibraryAeiraEavan1PtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return false; } }

	protected override int QuestId { get { return 519432; } }
	protected override string LQuestDescription { get { return L("Aeira from the bookstore and Eavan from the town office haven't returned their [Borrowed Book from the Library]. Can you please get them for me? - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs
	{
		get
		{
			return new NpcBookPair[] {
				new NpcBookPair { NpcIdent = "_aeira", ItemId = 70042 },
				new NpcBookPair { NpcIdent = "_eavan", ItemId = 70055 },
			};
		}
	}
	protected override int RewardSetId { get { return 7; } }
}

public class StewartLibraryAeiraEavan2PtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return false; } }

	protected override int QuestId { get { return 519433; } }
	protected override string LQuestDescription { get { return L("Aeira from the bookstore and Eavan from the town office haven't returned their [Borrowed Book from the Library]. Can you please get them for me? - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs
	{
		get
		{
			return new NpcBookPair[] {
				new NpcBookPair { NpcIdent = "_aeira", ItemId = 70041 },
				new NpcBookPair { NpcIdent = "_eavan", ItemId = 70055 },
			};
		}
	}
	protected override int RewardSetId { get { return 7; } }
}

public class StewartExtLibraryWalterAusteynPtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return true; } }

	protected override int QuestId { get { return 519434; } }
	protected override string LQuestDescription { get { return L("Walter from the general store and Austeyn from the bank haven't returned their [Borrowed Book from the Library], so can you get the books from them for me? I'm really busy right now, so I'd also appreciate it if you could put the books back on the shelves for me. - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs
	{
		get
		{
			return new NpcBookPair[] {
				new NpcBookPair { NpcIdent = "_walter", ItemId = 70058 },
				new NpcBookPair { NpcIdent = "_austeyn", ItemId = 70047 },
			};
		}
	}
	protected override int RewardSetId { get { return 8; } }
}

public class StewartLibraryWalterAusteyn1PtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return false; } }

	protected override int QuestId { get { return 519435; } }
	protected override string LQuestDescription { get { return L("Walter from the general store and Austeyn from the bank haven't returned their [Borrowed Book from the Library]. Can you please get them for me? - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs
	{
		get
		{
			return new NpcBookPair[] {
				new NpcBookPair { NpcIdent = "_walter", ItemId = 70060 },
				new NpcBookPair { NpcIdent = "_austeyn", ItemId = 70048 },
			};
		}
	}
	protected override int RewardSetId { get { return 6; } }
}

public class StewartLibraryWalterAusteyn2PtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return false; } }

	protected override int QuestId { get { return 519436; } }
	protected override string LQuestDescription { get { return L("Walter from the general store and Austeyn from the bank haven't returned their [Borrowed Book from the Library]. Can you please get them for me? - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs
	{
		get
		{
			return new NpcBookPair[] {
				new NpcBookPair { NpcIdent = "_walter", ItemId = 70058 },
				new NpcBookPair { NpcIdent = "_austeyn", ItemId = 70046 },
			};
		}
	}
	protected override int RewardSetId { get { return 6; } }
}

public class StewartExtLibrarySimonGlenisPtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return true; } }

	protected override int QuestId { get { return 519437; } }
	protected override string LQuestDescription { get { return L("Simon from the clothing shop and Glenis from the grocery store haven't returned their [Borrowed Book from the Library], so can you get the books from them for me? I'm really busy right now, so I'd also appreciate it if you could put the books back on the shelves for me. - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs
	{
		get
		{
			return new NpcBookPair[] {
				new NpcBookPair { NpcIdent = "_simon", ItemId = 70043 },
				new NpcBookPair { NpcIdent = "_glenis", ItemId = 70053 },
			};
		}
	}
	protected override int RewardSetId { get { return 8; } }
}

public class StewartLibrarySimonGlenis1PtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return false; } }

	protected override int QuestId { get { return 519438; } }
	protected override string LQuestDescription { get { return L("Simon from the clothing shop and Glenis from the grocery store haven't returned their [Borrowed Book from the Library]. Can you please get them for me? - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs
	{
		get
		{
			return new NpcBookPair[] {
				new NpcBookPair { NpcIdent = "_simon", ItemId = 70044 },
				new NpcBookPair { NpcIdent = "_glenis", ItemId = 70051 },
			};
		}
	}
	protected override int RewardSetId { get { return 6; } }
}

public class StewartLibrarySimonGlenis2PtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return false; } }

	protected override int QuestId { get { return 519439; } }
	protected override string LQuestDescription { get { return L("Simon from the clothing shop and Glenis from the grocery store haven't returned their [Borrowed Book from the Library]. Can you please get them for me? - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs
	{
		get
		{
			return new NpcBookPair[] {
				new NpcBookPair { NpcIdent = "_simon", ItemId = 70045 },
				new NpcBookPair { NpcIdent = "_glenis", ItemId = 70052 },
			};
		}
	}
	protected override int RewardSetId { get { return 6; } }
}

public class StewartExtLibraryNerysManus1PtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return true; } }

	protected override int QuestId { get { return 519440; } }
	protected override string LQuestDescription { get { return L("Nerys at the weapon shop and Manus at the healer's house haven't returned their [Borrowed Book from the Library], so can you get the books from them for me? I'm really busy right now, so I'd also appreciate it if you could put the books back on the shelves for me. - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs
	{
		get
		{
			return new NpcBookPair[] {
				new NpcBookPair { NpcIdent = "_nerys", ItemId = 70061 },
				new NpcBookPair { NpcIdent = "_manus", ItemId = 70056 },
			};
		}
	}
	protected override int RewardSetId { get { return 6; } }
}

public class StewartLibraryNerysManusPtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return false; } }

	protected override int QuestId { get { return 519441; } }
	protected override string LQuestDescription { get { return L("Nerys at the weapon shop and Manus at the healer's house haven't returned their [Borrowed Book from the Library]. Can you please get them for me? - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs
	{
		get
		{
			return new NpcBookPair[] {
				new NpcBookPair { NpcIdent = "_nerys", ItemId = 70062 },
				new NpcBookPair { NpcIdent = "_manus", ItemId = 70056 },
			};
		}
	}
	protected override int RewardSetId { get { return 7; } }
}

public class StewartExtLibraryNerysManus2PtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return true; } }

	protected override int QuestId { get { return 519442; } }
	protected override string LQuestDescription { get { return L("Nerys at the weapon shop and Manus at the healer's house haven't returned their [Borrowed Book from the Library], so can you get the books from them for me? I'm really busy right now, so I'd also appreciate it if you could put the books back on the shelves for me. - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs
	{
		get
		{
			return new NpcBookPair[] {
				new NpcBookPair { NpcIdent = "_nerys", ItemId = 70063 },
				new NpcBookPair { NpcIdent = "_manus", ItemId = 70057 },
			};
		}
	}
	protected override int RewardSetId { get { return 6; } }
}

public class StewartLibraryTracy1PtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return false; } }

	protected override int QuestId { get { return 519443; } }
	protected override string LQuestDescription { get { return L("Tracy from the logging camp hasn't returned [Braden's Big Adventure (2)] in a long time. Can you please get it for me? - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs { get { return new NpcBookPair[] { new NpcBookPair { NpcIdent = "_tracy", ItemId = 70066 } }; } }
	protected override int RewardSetId { get { return 8; } }
}

public class StewartLibraryTracy2PtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return false; } }

	protected override int QuestId { get { return 519444; } }
	protected override string LQuestDescription { get { return L("Tracy from the logging camp hasn't returned [Braden's Big Adventure (3)] in a long time. Can you please get it for me? - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs { get { return new NpcBookPair[] { new NpcBookPair { NpcIdent = "_tracy", ItemId = 70067 } }; } }
	protected override int RewardSetId { get { return 8; } }
}

public class StewartExtLibraryTracyPtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return true; } }

	protected override int QuestId { get { return 519445; } }
	protected override string LQuestDescription { get { return L("Tracy from the logging camp hasn't returned [Braden's Big Adventure (4)] in a long time, so can you get it from him for me? I'm really busy right now, so I'd also appreciate it if you could put the book back on the shelves for me. - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs { get { return new NpcBookPair[] { new NpcBookPair { NpcIdent = "_tracy", ItemId = 70068 } }; } }
	protected override int RewardSetId { get { return 9; } }
}

public class StewartLibraryAeiraWalterSimon1PtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return false; } }

	protected override int QuestId { get { return 519461; } }
	protected override string LQuestDescription { get { return L("Aeira at the bookstore, Walter at the general store, and Simon at the clothing shop haven't returned their [Borrowed Book from the Library]. Can you please get them for me? - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs
	{
		get
		{
			return new NpcBookPair[] {
				new NpcBookPair { NpcIdent = "_aeira", ItemId = 70040 },
				new NpcBookPair { NpcIdent = "_walter", ItemId = 70059 },
				new NpcBookPair { NpcIdent = "_simon", ItemId = 70045 },
			};
		}
	}
	protected override int RewardSetId { get { return 10; } }
}

public class StewartExtLibraryAeiraWalterSimonPtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return true; } }

	protected override int QuestId { get { return 519462; } }
	protected override string LQuestDescription { get { return L("Aeira at the bookstore, Walter at the general store, and Simon at the clothing shop haven't returned their [Borrowed Book from the Library], so can you get the books from them for me? I'm really busy right now, so I'd also appreciate it if you could put the books back on the shelves for me. - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs
	{
		get
		{
			return new NpcBookPair[] {
				new NpcBookPair { NpcIdent = "_aeira", ItemId = 70042 },
				new NpcBookPair { NpcIdent = "_walter", ItemId = 70060 },
				new NpcBookPair { NpcIdent = "_simon", ItemId = 70044 },
			};
		}
	}
	protected override int RewardSetId { get { return 11; } }
}

public class StewartLibraryAeiraWalterSimon2PtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return false; } }

	protected override int QuestId { get { return 519463; } }
	protected override string LQuestDescription { get { return L("Aeira at the bookstore, Walter at the general store, and Simon at the clothing shop haven't returned their [Borrowed Book from the Library]. Can you please get them for me? - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs
	{
		get
		{
			return new NpcBookPair[] {
				new NpcBookPair { NpcIdent = "_aeira", ItemId = 70041 },
				new NpcBookPair { NpcIdent = "_walter", ItemId = 70058 },
				new NpcBookPair { NpcIdent = "_simon", ItemId = 70043 },
			};
		}
	}
	protected override int RewardSetId { get { return 10; } }
}

public class StewartExtLibraryKristellGlenisAusteynPtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return true; } }

	protected override int QuestId { get { return 519464; } }
	protected override string LQuestDescription { get { return L("Kristell at the church, Glenis at the grocery store, and Austeyn at the bank haven't returned their [Borrowed Book from the Library], so can you get the books from them for me? I'm really busy right now, so I'd also appreciate it if you could put the books back on the shelves for me. - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs
	{
		get
		{
			return new NpcBookPair[] {
				new NpcBookPair { NpcIdent = "_kristell", ItemId = 70049 },
				new NpcBookPair { NpcIdent = "_glenis", ItemId = 70051 },
				new NpcBookPair { NpcIdent = "_austeyn", ItemId = 70046 },
			};
		}
	}
	protected override int RewardSetId { get { return 12; } }
}

public class StewartLibraryKristellGlenisAusteyn1PtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return false; } }

	protected override int QuestId { get { return 519465; } }
	protected override string LQuestDescription { get { return L("Kristell at the church, Glenis at the grocery store, and Austeyn at the bank haven't returned their [Borrowed Book from the Library]. Can you please get them for me? - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs
	{
		get
		{
			return new NpcBookPair[] {
				new NpcBookPair { NpcIdent = "_kristell", ItemId = 70050 },
				new NpcBookPair { NpcIdent = "_glenis", ItemId = 70052 },
				new NpcBookPair { NpcIdent = "_austeyn", ItemId = 70047 },
			};
		}
	}
	protected override int RewardSetId { get { return 11; } }
}

public class StewartLibraryKristellGlenisAusteyn2PtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return false; } }

	protected override int QuestId { get { return 519466; } }
	protected override string LQuestDescription { get { return L("Kristell at the church, Glenis at the grocery store, and Austeyn at the bank haven't returned their [Borrowed Book from the Library]. Can you please get them for me? - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs
	{
		get
		{
			return new NpcBookPair[] {
				new NpcBookPair { NpcIdent = "_kristell", ItemId = 70050 },
				new NpcBookPair { NpcIdent = "_glenis", ItemId = 70053 },
				new NpcBookPair { NpcIdent = "_austeyn", ItemId = 70048 },
			};
		}
	}
	protected override int RewardSetId { get { return 11; } }
}

public class StewartExtLibraryAusteynManusNerysPtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return true; } }

	protected override int QuestId { get { return 519467; } }
	protected override string LQuestDescription { get { return L("Austeyn at the bank, Manus at the healer's house, and Nerys at the weapon shop haven't returned their [Borrowed Book from the Library], so can you get the books from them for me? I'm really busy right now, so I'd also appreciate it if you could put the books back on the shelves for me. - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs
	{
		get
		{
			return new NpcBookPair[] {
				new NpcBookPair { NpcIdent = "_austeyn", ItemId = 70046 },
				new NpcBookPair { NpcIdent = "_manus", ItemId = 70056 },
				new NpcBookPair { NpcIdent = "_nerys", ItemId = 70062 },
			};
		}
	}
	protected override int RewardSetId { get { return 11; } }
}

public class StewartLibraryAusteynManusNerys1PtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return false; } }

	protected override int QuestId { get { return 519468; } }
	protected override string LQuestDescription { get { return L("Austeyn at the bank, Manus at the healer's house, and Nerys at the weapon shop haven't returned their [Borrowed Book from the Library]. Can you please get them for me? - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs
	{
		get
		{
			return new NpcBookPair[] {
				new NpcBookPair { NpcIdent = "_austeyn", ItemId = 70047 },
				new NpcBookPair { NpcIdent = "_manus", ItemId = 70057 },
				new NpcBookPair { NpcIdent = "_nerys", ItemId = 70061 },
			};
		}
	}
	protected override int RewardSetId { get { return 10; } }
}

public class StewartLibraryAusteynManusNerys2PtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return false; } }

	protected override int QuestId { get { return 519469; } }
	protected override string LQuestDescription { get { return L("Austeyn at the bank, Manus at the healer's house, and Nerys at the weapon shop haven't returned their [Borrowed Book from the Library]. Can you please get them for me? - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs
	{
		get
		{
			return new NpcBookPair[] {
				new NpcBookPair { NpcIdent = "_austeyn", ItemId = 70048 },
				new NpcBookPair { NpcIdent = "_manus", ItemId = 70057 },
				new NpcBookPair { NpcIdent = "_nerys", ItemId = 70063 },
			};
		}
	}
	protected override int RewardSetId { get { return 10; } }
}

public class StewartExtLibraryEavanTracyPtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return true; } }

	protected override int QuestId { get { return 519470; } }
	protected override string LQuestDescription { get { return L("Eavan from the town office and Tracy from the logging camp haven't returned their [Borrowed Book from the Library], so can you get the books from them for me? I'm really busy right now, so I'd also appreciate it if you could put the books back on the shelves for me. - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs
	{
		get
		{
			return new NpcBookPair[] {
				new NpcBookPair { NpcIdent = "_eavan", ItemId = 70054 },
				new NpcBookPair { NpcIdent = "_tracy", ItemId = 70066 },
			};
		}
	}
	protected override int RewardSetId { get { return 13; } }
}

public class StewartLibraryEavanTracy1PtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return false; } }

	protected override int QuestId { get { return 519471; } }
	protected override string LQuestDescription { get { return L("Eavan from the town office and Tracy from the logging camp haven't returned their [Borrowed Book from the Library]. Can you please get them for me? - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs
	{
		get
		{
			return new NpcBookPair[] {
				new NpcBookPair { NpcIdent = "_eavan", ItemId = 70054 },
				new NpcBookPair { NpcIdent = "_tracy", ItemId = 70068 },
			};
		}
	}
	protected override int RewardSetId { get { return 14; } }
}

public class StewartLibraryEavanTracy2PtjScript : StewartVarLibraryPtjBaseScript
{
	protected override bool DoShelving { get { return false; } }

	protected override int QuestId { get { return 519472; } }
	protected override string LQuestDescription { get { return L("Eavan from the town office and Tracy from the logging camp haven't returned their [Borrowed Book from the Library]. Can you please get them for me? - Stewart -"); } }
	protected override NpcBookPair[] NpcBookPairs
	{
		get
		{
			return new NpcBookPair[] {
				new NpcBookPair { NpcIdent = "_eavan", ItemId = 70055 },
				new NpcBookPair { NpcIdent = "_tracy", ItemId = 70067 },
			};
		}
	}
	protected override int RewardSetId { get { return 14; } }
}