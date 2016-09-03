#region TEMP_NOTES
// If you see this region in a pull request, 
// **I contain undefined values and should not be merged into master!**

// Class name resolver - go go gadget intellisense
using Aura.Mabi;
using Aura.Mabi.Const;
using System.Threading.Tasks;
using Aura.Channel.Scripting.Scripts;
// Temporarily place this file under ChannelServer.csproj to resolve rest of dependencies

// PR Readiness Check: 
/* Run following regex searches for possibly improper Localization:
	* /LX?N?\((?!")/
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
 */
#endregion

//--- Aura Script -----------------------------------------------------------
// Stewart's Library Delivery Part-Time Job
//--- Description -----------------------------------------------------------
// All quests used by the PTJ, and a script to handle the PTJ via hooks.
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
		?
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

Unimplemented: QuestScripts