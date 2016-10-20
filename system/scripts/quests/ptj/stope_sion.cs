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
// Sion's Iron Ore-Mining Part-time Job
//--- Description -----------------------------------------------------------
// All quests used by the PTJ, and a script to handle the PTJ via hooks.
//---------------------------------------------------------------------------

public class SionPtjScript : GeneralScript
{
	const PtjType JobType = PtjType.Stope;

	const int Start = 10;
	const int Report = 15;
	const int Deadline = 23;
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
		AddHook("_sion", "after_intro", AfterIntro);
		AddHook("_sion", "before_keywords", BeforeKeywords);

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
			npc.Msg(L("Are you working for someone else right now?<br/>If you want to help me, go finish that job first, then come back."));
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
					npc.Msg(L("You have to wait until the deadline.<br/>You're gonna have to wait a little more."));
				else
					npc.Msg(L("It's not the deadline yet.<br/>By the way, are you doing the work that I asked you to do?<br/>I need you to do a good job."));

				return;
			}

			// Report?
			npc.Msg(L("I hope you finished your work.<br/>Let's see how you did."),
				npc.Button(L("Report Now"), "@report"),
				npc.Button(L("Report Later"), "@later")
				);

			if (await npc.Select() != "@report")
			{
				npc.Msg(L("Let me know when you're done, Ok?"));
				return;
			}

			// Nothing done
			if (result == QuestResult.None)
			{
				npc.GiveUpPtj();

				npc.Msg(npc.FavorExpression(), L("What is this! If you agreed to work, then you should keep your promise!<br/>What do you take me for?"));
				npc.ModifyRelation(0, -Random(3), 0);
			}
			// Low~Perfect result
			else
			{
				npc.Msg(L("Hehe... Thank you for your hard work..<br/>I have a few things I want to show you...<br/>Why don't you pick something you like?"),
					npc.Button(L("Report Later"), "@later"),
					npc.PtjReport(result)
					);
				var reply = await npc.Select();

				// Report later
				if (!reply.StartsWith("@reward:"))
				{
					npc.Msg(npc.FavorExpression(), L("Don't tell me you don't like what I prepared for you?<br/>Hehe... I'm kidding."));
					return;
				}

				// Complete
				npc.CompletePtj(reply);
				remaining--;

				// Result msg
				if (result == QuestResult.Perfect)
				{
					npc.Msg(npc.FavorExpression(), L("Oh, thanks.<br/>I'll give you the reward that I promised."));
					npc.ModifyRelation(0, Random(3), 0);
				}
				else
				{
					npc.Msg(npc.FavorExpression(), L("Hmm...<br/>Even a kid like me could do better than this.<br/>You should be ashamed of yourself."));
					npc.ModifyRelation(0, -Random(2), 0);
				}
			}
			return;
		}

		// Check if PTJ time
		if (!npc.ErinnHour(Start, Deadline))
		{
			npc.Msg(L("It's too early for work."));
			return;
		}

		// Check if not done today and if there are jobs remaining
		if (!npc.CanDoPtj(JobType, remaining))
		{
			npc.Msg(L("Hmm... That's enough for today.<br/>Can you come back tomorrow?"));
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
			msg = L("Alright, if you've heard about my family,<br/>then I take it that you know what kind of work you'll be doing?<br/>You're going to need a good grip on that Pickaxe.");
		else
			msg = L("You brought your Pickaxe, right?");

		npc.Msg("debug: Decline PTJ to receive go to the next PTJ to test.<br/>" + msg, npc.PtjDesc(randomPtj,
			L("Sion's Iron Ore-Mining Part-time Job"),
			L("Looking for miners."),
			PerDay, remaining, npc.GetPtjDoneCount(JobType)));

		if (await npc.Select() == "@accept")
		{
			if (npc.GetPtjDoneCount(JobType) == 0)
				npc.Msg(L("Please come back to report before the deadline ends."));
			else
				npc.Msg(L("Alright! Don't be late for the deadline!"));

			npc.StartPtj(randomPtj);
		}
		else
		{
			if (npc.GetPtjDoneCount(JobType) == 0)
				npc.Msg(L("Hmm. Are you scared?"));
			else
				npc.Msg(L("I guess you don't feel like working today.<br/>Well, you can just help me next time then."));
			Log.Debug("Removed quest ID {0} from test stack.", dbg_questTestStack.Pop());
		}
	}
}

Unimplemented: QuestScripts