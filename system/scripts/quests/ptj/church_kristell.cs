#region TEMP_NOTES
// If you see this region in a pull request, 
// **I contain undefined values and should not be merged into master!**

// Class name resolver - go go gadget intellisense
using Aura.Mabi;
using Aura.Mabi.Const;
using System.Threading.Tasks;
using Aura.Channel.Scripting.Scripts;
// Temporarily place this file under ChannelServer.csproj to resolve rest of dependencies
#endregion

//--- Aura Script -----------------------------------------------------------
// Kristell's Church Part-Time Job
//--- Description -----------------------------------------------------------
// All quests used by the PTJ, and a script to handle the PTJ via hooks.
//---------------------------------------------------------------------------

public class KristellPtjScript : GeneralScript
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

	public override void Load()
	{
		AddHook("_kristell", "after_intro", AfterIntro);
		AddHook("_kristell", "before_keywords", BeforeKeywords);
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
			npc.Msg(L("The Church also needs workers.<br/>Please pay a visit here once you finish the current task."));
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
					npc.Msg(L("You have finished already?<br/>It is a little too early, so would you mind returning later?"));
				}
				else
				{
					npc.Msg(L("I trust that the assigned task is going well?<br/>The deadline is not past yet, so please do your best."));
				}
				return;
			}

			// Report?
			npc.Msg(L("It seems that you have completed the given task.<br/>If you would like, we can call it a day here."),
				npc.Button(L("Report Now"), "@report"),
				npc.Button(L("Report Later"), "@later")
				);

			if (await npc.Select() != "@report")
			{
				npc.Msg(L("If not right now, please make sure to report to me before the deadline.<br/>You have to at least report back to me even if you do not completely finish your task."));
				return;
			}

			// Nothing done
			if (result == QuestResult.None)
			{
				npc.GiveUpPtj();

				npc.Msg(npc.FavorExpression(), L("Oh, good heavens!<br/><username/>, I trusted you with this and this is all you have done. How disappointing.<br/>I cannot pay you for this."));
				npc.ModifyRelation(0, -Random(3), 0);
			}
			// Low~Perfect result
			else
			{
				npc.Msg(L("Mmm? I did not know you would do such a good job.<br/>You are a very meticulous worker, <username/>.<br/>I know this does not do justice for the excellent work you have done, but<br/>I have prepared a few things as a token of my gratitude. Please, take your pick."),
					npc.Button(L("Report Later"), "@later"),
					npc.PtjReport(result)
					);
				var reply = await npc.Select();

				// Report later
				if (!reply.StartsWith("@reward:"))
				{
					npc.Msg(npc.FavorExpression(), L("You seem to be busy all the time, <username/>."));
					return;
				}

				// Complete
				npc.CompletePtj(reply);
				remaining--;

				// Result msg
				if (result == QuestResult.Perfect)
				{
					npc.Msg(npc.FavorExpression(), L("Great! You have done very well.<br/>Here is the Holy Water as promised."));
					npc.ModifyRelation(0, Random(3), 0);
				}
				else if (result == QuestResult.Mid)
				{
					npc.Msg(npc.FavorExpression(), L("Thank you for your help.<br/>But, it is a little less than what was asked for.<br/>Anyway, I will pay you for what has been completed."));
					npc.ModifyRelation(0, Random(1), 0);
				}
				else if (result == QuestResult.Low)
				{
					npc.Msg(npc.FavorExpression(), L("Hmm. You have not adequately completed your work.<br/>Did you not have enough time?<br/>I cannot give you the Holy Water, then."));
					npc.ModifyRelation(0, -Random(2), 0);
				}
			}
			return;
		}

		// Check if PTJ time
		if (!npc.ErinnHour(Start, Deadline))
		{
			npc.Msg(L("Oh, no. It is not time for Church duties yet.<br/>Would you return later?"));
			return;
		}

		// Check if not done today and if there are jobs remaining
		if (!npc.CanDoPtj(JobType, remaining))
		{
			npc.Msg(L("Today's work has been completed.<br/>Only one task is given to one person per day.<br/>Please return tomorrow."));
			return;
		}

		// Offer PTJ
		var randomPtj = npc.RandomPtj(JobType, QuestIds);
		var msg = "";

		if (npc.GetPtjDoneCount(JobType) == 0)
			msg = L("(missing): first time worker PTJ inquiry");
		else
			msg = L("Do you want to work at the Church again today?<br/>Please take a look at the work details before you decide.");

		npc.Msg(msg, npc.PtjDesc(randomPtj,
			L("Kristell's Church Part-Time Job"),
			L("Looking for help with delivering goods to Church."),
			PerDay, remaining, npc.GetPtjDoneCount(JobType)));

		if (await npc.Select() == "@accept")
		{
			if (npc.GetPtjDoneCount(JobType) == 0)
				msg = L("(missing): first time accepting PTJ offer");
			else
				msg = L("Thank you.<br/>I hope you finish it in time.");

			npc.StartPtj(randomPtj);
		}
		else
		{
			if (npc.GetPtjDoneCount(JobType) == 0)
				msg = L("(missing): first time declining PTJ offer");
			else
				msg = L("Are you busy with something else?<br/>If not today, please give me a hand later.");
		}
	}
}

Unimplemented: QuestScripts