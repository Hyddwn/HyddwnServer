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
// Comgan's Monster-Hunting Part-Time Job
//--- Description -----------------------------------------------------------
// All quests used by the PTJ, and a script to handle the PTJ via hooks.
//--- Notes -----------------------------------------------------------------
// The following dialogue is missing:
// * first time worker PTJ inquiry
// * first time accepting PTJ offer
// * first time declining PTJ offer
//---------------------------------------------------------------------------

public class ComganPtjScript : GeneralScript
{
	const PtjType JobType = PtjType.Church;

	const int Start = 12;
	const int Report = 16;
	const int Deadline = 21;
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
		AddHook("_comgan", "after_intro", AfterIntro);
		AddHook("_comgan", "before_keywords", BeforeKeywords);

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
			npc.Msg(L("Do you need holy water?<br/>If you come after you have finished the work that you are doing now, I will give you a task that's related to holy water."));
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
					npc.Msg(L("You will have to wait a little longer for the deadline today."));
				else
					npc.Msg(L("It is not the deadline yet.<br/>Anyway, are you doing the work I have asked you to do?"));

				return;
			}

			// Report?
			npc.Msg(L("It is the deadline.<br/>Shall we see whether you have completed your tasks?"),
				npc.Button(L("Report Now"), "@report"),
				npc.Button(L("Report Later"), "@later")
				);

			if (await npc.Select() != "@report")
			{
				npc.Msg(L("Very well.<br/>But, please do report to me before the deadline."));
				return;
			}

			// Nothing done
			if (result == QuestResult.None)
			{
				npc.GiveUpPtj();

				npc.Msg(npc.FavorExpression(), L("If you ignore the tasks you promised to carry out,<br/>I cannot help you much, either."));
				npc.ModifyRelation(0, -Random(3), 0);
			}
			// Low~Perfect result
			else
			{
				npc.Msg(L("Thank you for a job well done.<br/>For that, I have prepared a few things.<br/>I cannot part with all of them, but<br/>why don't you pick one that you like?"),
					npc.Button(L("Report Later"), "@later"),
					npc.PtjReport(result)
					);
				var reply = await npc.Select();

				// Report later
				if (!reply.StartsWith("@reward:"))
				{
					npc.Msg(npc.FavorExpression(), L("Hmm? Would you rather do it next time?<br/>Whatever works for you, then."));
					return;
				}

				// Complete
				npc.CompletePtj(reply);
				remaining--;

				// Result msg
				if (result == QuestResult.Perfect)
				{
					npc.Msg(npc.FavorExpression(), L("Thank you.<br/>You have not disappointed me, as expected."));
					npc.ModifyRelation(0, Random(3), 0);
				}
				else if (result == QuestResult.Mid)
				{
					npc.Msg(npc.FavorExpression(), L("It seems that you have not completely finished the given tasks.<br/>But, you also seem to have done your best, <username/>,<br/>so I will give you a reward for that."));
					npc.ModifyRelation(0, Random(1), 0);
				}
				else if (result == QuestResult.Low)
				{
					npc.Msg(npc.FavorExpression(), L("Pardon my criticism,<br/>but you seem to have neglected your tasks and focused on something else.<br/>I will give you a partial compensation."));
					npc.ModifyRelation(0, -Random(2), 0);
				}
			}
			return;
		}

		// Check if PTJ time
		if (!npc.ErinnHour(Start, Deadline))
		{
			npc.Msg(L("I appreciate your willingness to help,<br/>but it is not time yet for me to assign tasks."));
			return;
		}

		// Check if not done today and if there are jobs remaining
		if (!npc.CanDoPtj(JobType, remaining))
		{
			npc.Msg(L("There doesn't seem to be much else you can help with today.<br/>Let's talk again tomorrow."));
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
			msg = L("Do you need holy water again today?");

		npc.Msg("debug: Decline PTJ to receive go to the next PTJ to test.<br/>" + msg, npc.PtjDesc(randomPtj,
			L("Comgan's Monster-Hunting Part-Time Job"),
			L("Looking for monster hunters in Church."),
			PerDay, remaining, npc.GetPtjDoneCount(JobType)));

		if (await npc.Select() == "@accept")
		{
			if (npc.GetPtjDoneCount(JobType) == 0)
				npc.Msg(L("(missing): first time accepting PTJ offer"));
			else
				npc.Msg(L("Please be careful not to miss the deadline."));

			npc.StartPtj(randomPtj);
		}
		else
		{
			if (npc.GetPtjDoneCount(JobType) == 0)
				npc.Msg(L("(missing): first time declining PTJ offer"));
			else
				npc.Msg(L("I am sorry, but if you won't help with the tasks,<br/>I cannot really help you, either."));
			Log.Debug("Removed quest ID {0} from test stack.", dbg_questTestStack.Pop());
		}
	}
}

Unimplemented: QuestScripts