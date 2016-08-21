#region TEMP_NOTES
// If you see this region in a pull request, 
// **I contain uncompilable undefined values and should not be merged into master!**

// Class name resolver - go go gadget intellisense
using Aura.Mabi;
using Aura.Mabi.Const;
using System.Threading.Tasks;
using Aura.Channel.Scripting.Scripts;
// Temporarily place this file under ChannelServer.csproj to resolve rest of dependencies

// PR Readiness Check: 
// * Run following regex searches for invalid Localization:
    /* /LN?\((?!")/            - Non-string Localization call
     * /(?<!LN?\()".*?"(?!\w)/ - Unlocalised string //warn: You will get a lot of false-positives.
     * /\.Format/              - Check for proper string parametrisation. 
                                 All parts of the string must be exposed to Poedit.
     */
// * Ensure rewards are in counting order with non-conflicting group IDs, per PTJ Quest ID.
#endregion

//--- Aura Script -----------------------------------------------------------
// Glenis's Grocery Store Part-Time Job
//--- Description -----------------------------------------------------------
// All quests used by the PTJ, and a script to handle the PTJ via hooks.
//--- Notes -----------------------------------------------------------------
// The following dialogue is missing:
// * first time worker PTJ inquiry
// * first time accepting PTJ offer
// * first time declining PTJ offer
//
// Apparently some rewards have been increased, as the Wiki now lists
// higher ones than in the past. We'll use the G1 rewards for now.
// http://wiki.mabinogiworld.com/index.php?title=Glenis&oldid=272001#Part-time_Jobs
// In addition, homestead seeds are not rewarded nor are they mentioned for now.
//---------------------------------------------------------------------------

public class GlenisPtjScript : GeneralScript
{
	const PtjType JobType = PtjType.GroceryStore;

	const int Start = 12;
	const int Report = 14;
	const int Deadline = 21;
	const int PerDay = 10;

	int remaining = PerDay;

	readonly int[] QuestIds = new int[]
	{
		?
	};

	public override void Load()
	{
		AddHook("_glenis", "after_intro", AfterIntro);
		AddHook("_glenis", "before_keywords", BeforeKeywords);
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
			npc.Msg(L("Well, you seem to be involved in another part-time job right now.<br/>Is that right?"));
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
					npc.Msg(L("My, someone's in a hurry.<br/>It's not the deadline yet, so why don't you come back later?"));
					npc.Msg(L("Now, I'll see you back here by the deadline."));
				}
				else
				{
					npc.Msg(L("How's the work going?"));
				}
				return;
			}

			// Report?
			npc.Msg(L("Do you want to call it a day?<br/>You can report later if you're not finished yet."),
				npc.Button(L("Report Now"), "@report"),
				npc.Button(L("Report Later"), "@later")
				);

			if (await npc.Select() != "@report")
			{
				npc.Msg(L("Still have some work left to do, do you?<br/>Either way, please make sure to report by the deadline."));
				return;
			}

			// Nothing done
			if (result == QuestResult.None)
			{
				npc.GiveUpPtj();

				npc.Msg(npc.FavorExpression(), L("You know what? It's ok.<br/>If you didn't do your work, I won't need to pay you. Simple as that."));
				npc.ModifyRelation(0, -Random(3), 0);
			}
			// Low~Perfect result
			else
			{
				npc.Msg(L("Mmm? I didn't know you would do such a good job.<br/>You are a very meticulous worker, <username/>.<br/>I know this doesn't do justice for the excellent work you've done, but<br/>I've prepared a few things as a token of my gratitude. Take your pick."),
					npc.Button(L("Report Later"), "@later"),
					npc.PtjReport(result)
					);
				var reply = await npc.Select();

				// Report later
				if (!reply.StartsWith("@reward:"))
				{
					npc.Msg(npc.FavorExpression(), L("Hahaha. It must be a difficult choice."));
					return;
				}

				// Complete
				npc.CompletePtj(reply);
				remaining--;

				// Result msg
				if (result == QuestResult.Perfect)
				{
					npc.Msg(npc.FavorExpression(), L("Great! 10 out of 10!<br/>Please keep up the good work next time, too."));
					npc.ModifyRelation(0, Random(3), 0);
				}
				else if (result == QuestResult.Mid)
				{
					npc.Msg(npc.FavorExpression(), L("I'd say... 6 out of 10.<br/>Your pay will only be that much, too."));
					npc.ModifyRelation(0, Random(1), 0);
				}
				else if (result == QuestResult.Low)
				{
					npc.Msg(npc.FavorExpression(), L("Did you put in any effort at all?<br/>My, my!"));
					npc.ModifyRelation(0, -Random(2), 0);
				}
			}
			return;
		}

		// Check if PTJ time
		if (!npc.ErinnHour(Start, Deadline))
		{
			npc.Msg(L("Are you here for work?<br/>Sorry, but it's not business hours yet."));
			return;
		}

		// Check if not done today and if there are jobs remaining
		if (!npc.CanDoPtj(JobType, remaining))
		{
			npc.Msg(L("Didn't you just work here?<br/>You have to take care of yourself. Don't overwork yourself."));
			return;
		}

		// Offer PTJ
		var randomPtj = npc.RandomPtj(JobType, QuestIds);
		var msg = "";

		if (npc.GetPtjDoneCount(JobType) == 0)
			msg = L("(missing): first time worker PTJ inquiry");
		else
			msg = L("Are you looking for work?");//<br/>You can get a seed for your Homestead.");

		npc.Msg(msg, npc.PtjDesc(randomPtj,
			L("Glenis's Grocery Store Part-Time Job"),
			L("Looking for help with delivering goods to Grocery Store."),
			PerDay, remaining, npc.GetPtjDoneCount(JobType)));

		if (await npc.Select() == "@accept")
		{
			if (npc.GetPtjDoneCount(JobType) == 0)
				npc.Msg(L("(missing): first time accepting PTJ offer"));
			else
				npc.Msg(L("I'll be waiting for you."));

			npc.StartPtj(randomPtj);
		}
		else
		{
			if (npc.GetPtjDoneCount(JobType) == 0)
				npc.Msg(L("(missing): first time declining PTJ offer"));
			else
				npc.Msg(L("If today's not a good day, I'm sure we'll do this some other time."));
		}
	}
}

Unimplemented: QuestScripts