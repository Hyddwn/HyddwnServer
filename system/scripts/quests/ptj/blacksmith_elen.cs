#region TEMP_NOTES
// If you see this region in a pull request, 
// **I contain undefined values and should not be merged into master!**

// Class name resolver - go go gadget intellisense
using Aura.Mabi;
using Aura.Mabi.Const;
using System.Threading.Tasks;
using Aura.Channel.Scripting.Scripts;
using System;
using Aura.Shared.Util;
using Aura.Channel.Scripting;
using System.Collections.Generic;
// Temporarily place this file under ChannelServer.csproj to resolve rest of dependencies

// PR Readiness Check: 
/* Run following regex searches for possibly improper Localization:
	* /(?<=\W)LX?N?\((?!\s*")/g
		Non-string Localization call
	* /(?<!(Log\.\w*|LX?N?)\(\s*)"(?!([@_]|ptj|after_intro|before_keywords|about_arbeit|ErinnMidnightTick|QuestViewRenewal|CollectionBooks))\w.*?"(?!\w)/g
		Unlocalised string //warn: You will get a lot of false-positives.
	* /\.Format/g
		Check for proper string parametrisation.
		All parts of the string must be exposed to Poedit.
	* /LX?N?\((.*?$\n.*?LX?N?\()+/gm
		Consecutive Msg() calls?
		Join `L("a") ... L("b")` like so: `L("a<p/>b")`
 * Ensure rewards are in counting order with non-conflicting group IDs, per PTJ Quest ID.
 * Search "Log." and ensure displayed messages are sufficiently descriptive (particularly, do specify the source of the message).
 * Remove dbg_questTestStack and all other "debug:" text.
 */
#endregion

//--- Aura Script -----------------------------------------------------------
// Elen's Iron Ingot-Refining Part-Time Job
//--- Description -----------------------------------------------------------
// All quests used by the PTJ, and a script to handle the PTJ via hooks.
//--- Notes -----------------------------------------------------------------
// The following dialogue is missing:
// * first time worker PTJ inquiry
// * first time accepting PTJ offer
// * first time declining PTJ offer
// * 1-star PTJ turn in response
// * 3-star PTJ turn in response
//---------------------------------------------------------------------------

public class ElenPtjScript : GeneralScript
{
	const PtjType JobType = PtjType.BlacksmithShop;

	const int Start = 12;
	const int Report = 13;
	const int Deadline = 19;
	const int PerDay = 8;

	int remaining = PerDay;

	readonly int[] QuestIds = new int[]
	{
		?
	};

	Stack<int> dbg_questTestStack = new Stack<int>(new int[]
	{

	});

	public override void Load()
	{
		AddHook("_elen", "after_intro", AfterIntro);
		AddHook("_elen", "before_keywords", BeforeKeywords);

		Log.Debug("Quest test stack populated with {0} IDs.", dbg_questTestStack.Count);
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
			npc.Msg(L("Are you doing what you're supposed to be doing?<p/>Don't get lazy now. Make sure you take care of your work."));
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
					npc.Msg(L("Are you done already?<br/>It's not the deadline yet. Please come back later."));
				}
				else
				{
					npc.Msg(L("You'd better be doing the work I asked you to do!<br/>Please finish it before the deadline."));
				}
				return;
			}

			// Report?
			npc.Msg(L("Did you finish today's tasks?<br/>If not, you can report to me later."),
				npc.Button(L("Report Now"), "@report"),
				npc.Button(L("Report Later"), "@later")
				);

			if (await npc.Select() != "@report")
			{
				npc.Msg(L("Alright then, please report later."));
				return;
			}

			// Nothing done
			if (result == QuestResult.None)
			{
				npc.GiveUpPtj();

				npc.Msg(npc.FavorExpression(), L("All you did was waste time<br/>and you got nothing done.<br/>I can't pay you for this."));
				npc.ModifyRelation(0, -Random(3), 0);
			}
			// Low~Perfect result
			else
			{
				npc.Msg(L("Wow. You've done a meticulous job, <username/>.<br/>I've prepared a few things to thank you.<br/>And, thought I might as well give you some choices.<br/>Make sure to pick something you really need."),
					npc.Button(L("Report Later"), "@later"),
					npc.PtjReport(result)
					);
				var reply = await npc.Select();

				// Report later
				if (!reply.StartsWith("@reward:"))
				{
					npc.Msg(npc.FavorExpression(), L("Huh? Do you have an emergency?<br/>Well, what can you do. Please come back later."));
					return;
				}

				// Complete
				npc.CompletePtj(reply);
				remaining--;

				// Result msg
				if (result == QuestResult.Perfect)
				{
					npc.Msg(npc.FavorExpression(), L("Thank you.<br/>Please keep up this good work."));
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
			npc.Msg(L("I'm busy with my own work right now. Would you come back later?"));
			return;
		}

		// Check if not done today and if there are jobs remaining
		if (!npc.CanDoPtj(JobType, remaining))
		{
			npc.Msg(L("Today's work has been finished.<br/>Please come again tomorrow."));
			return;
		}

		if (dbg_questTestStack.Count <= 0)
		{
			npc.Msg("debug: Quest test stack exhausted.");
			return;
		}

		// Offer PTJ
		//var randomPtj = npc.RandomPtj(JobType, QuestIds);
		var randomPtj = dbg_questTestStack.Peek();
		var msg = "";

		if (npc.GetPtjDoneCount(JobType) == 0)
			msg = L("(missing): first time worker PTJ inquiry");
		else
			msg = L("Would you like to see today's work agenda?");

		npc.Msg("debug: Decline PTJ to receive go to the next PTJ to test.<br/>" + msg, npc.PtjDesc(randomPtj,
			L("Elen's Iron Ingot-Refining Part-Time Job"),
			L("Looking for refiners."),
			PerDay, remaining, npc.GetPtjDoneCount(JobType)));

		if (await npc.Select() == "@accept")
		{
			if (npc.GetPtjDoneCount(JobType) == 0)
				npc.Msg(L("(missing): first time accepting PTJ offer"));
			else
				npc.Msg(L("Alright. Good luck with your work."));

			npc.StartPtj(randomPtj);
		}
		else
		{
			if (npc.GetPtjDoneCount(JobType) == 0)
				npc.Msg(L("(missing): first time declining PTJ offer"));
			else
				npc.Msg(L("I see.<br/>Then I'll assign this task to someone else.."));
			Log.Debug("Removed quest ID {0} from test stack.", dbg_questTestStack.Pop());
		}
	}
}

Unimplemented: QuestScripts