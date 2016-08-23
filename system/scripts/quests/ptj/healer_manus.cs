//--- Aura Script -----------------------------------------------------------
// Manus's Healer's House Part-Time Job
//--- Description -----------------------------------------------------------
// All quests used by the PTJ, and a script to handle the PTJ via hooks.
//--- Notes -----------------------------------------------------------------
// This script depends on ./healer_dilys.cs for wool collection PTJs.
// Please ensure this script loads afterward.
//
// As of G16S2(?), Manus began providing PTJs that reward holy water.
// For now, said PTJs will remain undefined and out of circulation.
//---------------------------------------------------------------------------

public class ManusPtjScript : GeneralScript
{
	const PtjType JobType = PtjType.HealersHouse;

	const int Start = 6;
	const int Report = 9;
	const int Deadline = 15;
	const int PerDay = 10;

	int remaining = PerDay;

	readonly int[] QuestIds = new int[]
	{
		505101, // Basic  Gather 10 Wool
		505131, // Int    Gather 20 Wool
		505161, // Adv    Gather 30 Wool
	};

	public override void Load()
	{
		AddHook("_manus", "after_intro", AfterIntro);
		AddHook("_manus", "before_keywords", BeforeKeywords);
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
			npc.Msg(L("So, you want to work at a Healer's House, do you?<br/>I appreciate your enthusiasm, but finish the work you're doing first."));
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
					npc.Msg(L("It's not the deadline yet. Come back later."));
					npc.Msg(L("Alright. I'll see you later!"));
				}
				else
				{
					npc.Msg(L("I trust that your work is going well?"));
				}
				return;
			}

			// Report?
			npc.Msg(L("So, do you want to wrap up today's work here?<br/>You can report and finish the work without completing it,<br/>but it's good to finish something you've started."),
				npc.Button(L("Report Now"), "@report"),
				npc.Button(L("Report Later"), "@later")
				);

			if (await npc.Select() != "@report")
			{
				npc.Msg(L("Alright! Go for it!<br/>I look forward to your report!"));
				return;
			}

			// Nothing done
			if (result == QuestResult.None)
			{
				npc.GiveUpPtj();

				npc.Msg(npc.FavorExpression(), L("Weak.<br/>If you keep this up,<br/>you'll never get paid."));
				npc.ModifyRelation(0, -Random(3), 0);
			}
			// Low~Perfect result
			else
			{
				npc.Msg(L("Ha ha. I knew I was an excellent judge of character!<br/>Now, I should pay you for the work you have done, hmm? Take your pick here.<br/>I have to give to other workers, too, so don't even think about taking them all! Hahaha."),
					npc.Button(L("Report Later"), "@later"),
					npc.PtjReport(result)
					);
				var reply = await npc.Select();

				// Report later
				if (!reply.StartsWith("@reward:"))
				{
					npc.Msg(npc.FavorExpression(), L("Eh? What, are you going to just leave?"));
					return;
				}

				// Complete
				npc.CompletePtj(reply);
				remaining--;

				// Result msg
				if (result == QuestResult.Perfect)
				{
					npc.Msg(npc.FavorExpression(), L("Alright! Well done!<br/>You've worked hard."));
					npc.ModifyRelation(0, Random(3), 0);
				}
				else if (result == QuestResult.Mid)
				{
					npc.Msg(npc.FavorExpression(), L("Hmm. That's not quite enough.<br/>I'll deduct it from your pay."));
					npc.ModifyRelation(0, Random(1), 0);
				}
				else if (result == QuestResult.Low)
				{
					npc.Msg(npc.FavorExpression(), L("Is this the best you can do?<br/>Then I have no choice but to pay only a little."));
					npc.ModifyRelation(0, -Random(2), 0);
				}
			}
			return;
		}

		// Check if PTJ time
		if (!npc.ErinnHour(Start, Deadline))
		{
			npc.Msg(L("Try again later.<br/>It's not time for work yet."));
			return;
		}

		// Check if not done today and if there are jobs remaining
		if (!npc.CanDoPtj(JobType, remaining))
		{
			npc.Msg(L("Come back tomorrow."));
			return;
		}

		// Offer PTJ
		var randomPtj = npc.RandomPtj(JobType, QuestIds);
		var msg = "";

		if (npc.GetPtjDoneCount(JobType) == 0)
			msg = L("Looking for some work, are you?<br/>I happen to be in need of extra hands here,<br/>so help me out here and I'll pay you adequately.<br/>It's not much, though...<p/>So, are you interested?");
		else
			msg = L("So, you're here to help out again.");

		npc.Msg(msg, npc.PtjDesc(randomPtj,
			L("Manus's Healer's House part-time job"),
			L("Looking for help with delivering goods to Healer's House."),
			PerDay, remaining, npc.GetPtjDoneCount(JobType)));

		if (await npc.Select() == "@accept")
		{
			if (npc.GetPtjDoneCount(JobType) == 0)
				npc.Msg(L("Great. Good choice.<br/>Remember to report back to me before the deadline.<br/>"));
			else
				npc.Msg(L("I look forward to your job well done."));

			npc.StartPtj(randomPtj);
		}
		else
		{
			if (npc.GetPtjDoneCount(JobType) == 0)
				npc.Msg(L("If you don't want to report, though, I won't force you."));
			else
				npc.Msg(L("You must be feeling off today."));
		}
	}
}