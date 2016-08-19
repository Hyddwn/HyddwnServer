#region TEMP_NOTES
// If you see this region in a pull request, 
// **I contain undefined values and should not be merged into master!**

// Class name resolver - go go gadget intellisense
using Aura.Mabi;
using Aura.Mabi.Const;
using System.Threading.Tasks;
using Aura.Channel.Scripting.Scripts;
// Temporarily place this file under ChannelServer.csproj to resolve rest of dependencies

// PR Readiness Check: Run following regex searches for invalid Localization:
/* /LN?\((?!")/             - Non-string Localization call
 * /(?<!(LN?\(|,\s?))".*?"/ - Unlocalised string //warn: You will get a lot of false-positives.
 * tring\.Format            - Check for proper string parametrisation. 
 *                            All parts of the string must be exposed to Poedit.
 */
#endregion

//--- Aura Script -----------------------------------------------------------
// Aeira's Bookstore Part-Time Job
//--- Description -----------------------------------------------------------
// All quests used by the PTJ, and a script to handle the PTJ via hooks.
//--- Notes -----------------------------------------------------------------
// The following dialogue is missing:
// * 1-star PTJ turn in response
// * 3-star PTJ turn in response
//---------------------------------------------------------------------------

public class AeiraPtjScript : GeneralScript
{
	const PtjType JobType = PtjType.Bookstore;

	const int Start = 13;
	const int Report = 15;
	const int Deadline = 20;
	const int PerDay = 10;

	int remaining = PerDay;

	readonly int[] QuestIds = new int[]
	{
		?
	};

	public override void Load()
	{
		AddHook("_aeira", "after_intro", AfterIntro);
		AddHook("_aeira", "before_keywords", BeforeKeywords);
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
			npc.Msg(L("You seem to be working on something else already....<br/>It's probably a good idea to finish what you're working on first."));
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
					npc.Msg(L("I'm a little busy right now. Would you mind coming back later?"));
				}
				else
				{
					npc.Msg(L("How's the work going? You'll be fine."));
				}
				return;
			}

			// Report?
			npc.Msg(L("Are you done with the task I gave you?<br/>If not, you can come back later."),
				npc.Button(L("Report Now"), "@report"),
				npc.Button(L("Report Later"), "@later")
				);

			if (await npc.Select() != "@report")
			{
				npc.Msg(L("Then please come back later."));
				return;
			}

			// Nothing done
			if (result == QuestResult.None)
			{
				npc.GiveUpPtj();

				npc.Msg(npc.FavorExpression(), L("You didn't want to work, did you?<br/>I can't pay you, then."));
				npc.ModifyRelation(0, -Random(3), 0);
			}
			// Low~Perfect result
			else
			{
				npc.Msg(L("So... How shall I put this?<br/>This is a way of saying thank you for all the hard work you've done for me...<br/>Please pick something that you'll find useful."),
					npc.Button(L("Report Later"), "@later"),
					npc.PtjReport(result)
					);
				var reply = await npc.Select();

				// Report later
				if (!reply.StartsWith("@reward:"))
				{
					npc.Msg(npc.FavorExpression(), L("Yes, you can do it next time.<br/>I'll see you later."));
					return;
				}

				// Complete
				npc.CompletePtj(reply);
				remaining--;

				// Result msg
				if (result == QuestResult.Perfect)
				{
					npc.Msg(npc.FavorExpression(), L("Wow, thank you!<br/>Can you help me out again tomorrow?"));
					npc.ModifyRelation(0, Random(3), 0);
				}
				else if (result == QuestResult.Mid)
				{
					npc.Msg(npc.FavorExpression(), L("(missing): 3 star response"));
					npc.ModifyRelation(0, Random(1), 0);
				}
				else if (result == QuestResult.Low)
				{
					npc.Msg(npc.FavorExpression(), L("(missing): 1 star response"));
					npc.ModifyRelation(0, -Random(2), 0);
				}
			}
			return;
		}

		// Check if PTJ time
		if (!npc.ErinnHour(Start, Deadline))
		{
			npc.Msg(L("Oh no. It's not time for a part-time job, yet.<br/>Please come back later."));
			return;
		}

		// Check if not done today and if there are jobs remaining
		if (!npc.CanDoPtj(JobType, remaining))
		{
			npc.Msg(L("I'm done for today!<br/>Please come again tomorrow."));
			return;
		}

		// Offer PTJ
		var randomPtj = npc.RandomPtj(JobType, QuestIds);
		var msg = "";

		if (npc.GetPtjDoneCount(JobType) == 0)
			msg = L("It must be your first time working at a bookstore.<br/>It's actually more of my personal business than work. Hehe.<br/>Will you do it?");
		else
			msg = L("Oh, can you help me today, too?");

		npc.Msg(msg, npc.PtjDesc(randomPtj,
			L("Aeira's Bookstore Part-time Job"),
			L("Looking for help with delivery of goods in Bookstore."),
			PerDay, remaining, npc.GetPtjDoneCount(JobType)));

		if (await npc.Select() == "@accept")
		{
			if (npc.GetPtjDoneCount(JobType) == 0)
				npc.Msg(L("Thank you. Please be on time for the deadline.<br/>Even if you don't get to finish everything, please come back and report.<br/>I'll pay you for the amount of work you've accomplished."));
			else
				npc.Msg(L("I believe in you."));

			npc.StartPtj(randomPtj);
		}
		else
		{
			if (npc.GetPtjDoneCount(JobType) == 0)
				npc.Msg(L("Well, if you don't want to, then I can't force you."));
			else
				npc.Msg(L("You must be pretty busy, aren't you?"));
		}
	}
}

Unimplemented: QuestScripts