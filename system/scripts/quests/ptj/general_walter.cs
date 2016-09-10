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
	* /(?<=\W)LX?N?\((?!")/
		Non-string Localization call
	* /(?<!LX?N?\()".*?"(?!\w)/
		Unlocalised string //warn: You will get a lot of false-positives.
	* ".Format"
		Check for proper string parametrisation.
		All parts of the string must be exposed to Poedit.
	* /LX?N?\((.*?$\n.*?LX?N?\()+/m
		Consecutive Msg() calls?
		Join `L("a") ... L("b")` like so: `L("a<p/>b")`
 * Ensure rewards are in counting order with non-conflicting group IDs, per PTJ Quest ID.
 * Search "Log." and ensure displayed messages are sufficiently descriptive (particularly, do specify the source of the message).
 * Remove dbg_questTestStack and all other "debug:" text.
 */
#endregion

//--- Aura Script -----------------------------------------------------------
// Walter's General Shop Part-Time Job
//--- Description -----------------------------------------------------------
// All quests used by the PTJ, and a script to handle the PTJ via hooks.
//--- Notes -----------------------------------------------------------------
// The following dialogue is missing:
// * first time worker PTJ inquiry
// * first time accepting PTJ offer
// * first time declining PTJ offer
//---------------------------------------------------------------------------

public class WalterPtjScript : GeneralScript
{
	const PtjType JobType = PtjType.GeneralShop;

	const int Start = 7;
	const int Report = 9;
	const int Deadline = 19;
	const int PerDay = 10;

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
		AddHook("_walter", "after_intro", AfterIntro);
		AddHook("_walter", "before_keywords", BeforeKeywords);

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
			npc.Msg(L("Just finish what you're doing."));
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
					npc.Msg(L("It's not time yet."));
				}
				else
				{
					npc.Msg(L("Is everything alright?"));
				}
				return;
			}

			// Report?
			npc.Msg(L("Have you finished your work?"),
				npc.Button(L("Report Now"), "@report"),
				npc.Button(L("Report Later"), "@later")
				);

			if (await npc.Select() != "@report")
			{
				npc.Msg(L("Come when you're finished."));
				return;
			}

			// Nothing done
			if (result == QuestResult.None)
			{
				npc.GiveUpPtj();

				npc.Msg(npc.FavorExpression(), L("Don't ever come here again!"));
				npc.ModifyRelation(0, -Random(3), 0);
			}
			// Low~Perfect result
			else
			{
				npc.Msg(L("Phew... Well done.<br/>Take your pick."),
					npc.Button(L("Report Later"), "@later"),
					npc.PtjReport(result)
					);
				var reply = await npc.Select();

				// Report later
				if (!reply.StartsWith("@reward:"))
				{
					npc.Msg(npc.FavorExpression(), L("Hah..."));
					return;
				}

				// Complete
				npc.CompletePtj(reply);
				remaining--;

				// Result msg
				if (result == QuestResult.Perfect)
				{
					npc.Msg(npc.FavorExpression(), L("Good!"));
					npc.ModifyRelation(0, Random(3), 0);
				}
				else if (result == QuestResult.Mid)
				{
					npc.Msg(npc.FavorExpression(), L("This is not enough..."));
					npc.ModifyRelation(0, Random(1), 0);
				}
				else if (result == QuestResult.Low)
				{
					npc.Msg(npc.FavorExpression(), L("What is this!"));
					npc.ModifyRelation(0, -Random(2), 0);
				}
			}
			return;
		}

		// Check if PTJ time
		if (!npc.ErinnHour(Start, Deadline))
		{
			npc.Msg(L("It's not the right time."));
			return;
		}

		// Check if not done today and if there are jobs remaining
		if (!npc.CanDoPtj(JobType, remaining))
		{
			npc.Msg(L("That's it for today."));
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
			msg = L("I'm counting on you as usual.");

		// Possible PTJ titles:
		/* Looking for weavers.
		 * Looking for help with delivery of goods in General Shop.
		 */

		npc.Msg("debug: Decline PTJ to receive go to the next PTJ to test.<br/>" + msg, npc.PtjDesc(randomPtj,
			L("Walter's General Shop Part-Time Job"),
			L("(missing): PTJ title"),
			PerDay, remaining, npc.GetPtjDoneCount(JobType)));

		if (await npc.Select() == "@accept")
		{
			if (npc.GetPtjDoneCount(JobType) == 0)
				npc.Msg(L("(missing): first time accepting PTJ offer"));
			else
				npc.Msg(L("Well, then."));

			npc.StartPtj(randomPtj);
		}
		else
		{
			if (npc.GetPtjDoneCount(JobType) == 0)
				npc.Msg(L("(missing): first time declining PTJ offer"));
			else
				npc.Msg(L("I got it."));
			Log.Debug("Removed quest ID {0} from test stack.", dbg_questTestStack.Pop());
		}
	}
}

Unimplemented: QuestScripts