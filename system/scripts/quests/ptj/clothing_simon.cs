#region TEMP_NOTES
// If you see this region in a pull request, 
// **I contain undefined values and should not be merged into master!**

// Class name resolver - go go gadget intellisense
using Aura.Mabi;
using Aura.Mabi.Const;
using System.Threading.Tasks;
using Aura.Channel.Scripting.Scripts;
using Aura.Channel.World.Entities;
using Aura.Channel.World.Quests;
using Aura.Shared.Util;
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
 * Search ".Log" and ensure displayed messages are sufficiently descriptive (particularly, do specify the source of the message).
 */
#endregion

//--- Aura Script -----------------------------------------------------------
// Simon's Clothing Shop Part-Time Job
//--- Description -----------------------------------------------------------
// All quests used by the PTJ, and a script to handle the PTJ via hooks.
//---------------------------------------------------------------------------

public class SimonPtjScript : GeneralScript
{
	const PtjType JobType = PtjType.ClothingShop;

	const int Start = 7;
	//int Report; // Variable - Extracted from player's current PTJ quest data.
	const int Deadline = 19;
	const int PerDay = 5;

	int remaining = PerDay;

	readonly int[] QuestIds = new int[]
	{
		?
	};

	/// <summary>
	/// Precondition: <paramref name="player"/> is already working for Simon.
	/// <para>Returns the <paramref name="player"/>'s report time for their PTJ, or 1 AM if unable to for some reason.</para>
	/// </summary>
	/// <returns>
	/// This <paramref name="player"/>'s report time for their PTJ.
	/// <para>Precondition has not been met if unable to obtain a PTJ. A fallback of 1 AM will be returned.</para>
	/// </returns>
	/// <remarks>
	/// Report time changes depending on the task Simon gives to the player:
	/// 5 PM - Non-basic tailoring or weaving jobs
	/// Noon - Everything else
	/// </remarks>
	private int GetPersonalReportTime(Creature player)
	{
		Quest quest = player.Quests.GetPtjQuest();
		if (quest == null)
		{ // This should not normally happen.
			Log.Error("SimonPtjScript: Player {0} does not have a PTJ report time for Simon. Used fallback of 1 AM.", player.Name);
			return 1; // Fallback
		}
		else return quest.Data.ReportHour;
	}

	public override void Load()
	{
		AddHook("_simon", "after_intro", AfterIntro);
		AddHook("_simon", "before_keywords", BeforeKeywords);
	}

	public async Task<HookResult> AfterIntro(NpcScript npc, params object[] args)
	{
		// Call PTJ method after intro if it's time to report
		if (npc.DoingPtjForNpc() && npc.ErinnHour(GetPersonalReportTime(npc.Player), Deadline))
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
			npc.Msg(L("You seem pretty busy yourself.<br/>You should worry about finishing the work first."));
			return;
		}

		// Check if PTJ is in progress
		if (npc.DoingPtjForNpc())
		{
			var result = npc.GetPtjResult();

			// Check if report time
			if (!npc.ErinnHour(GetPersonalReportTime(npc.Player), Deadline))
			{
				if (result == QuestResult.Perfect)
					npc.Msg(L("You are done already?<br/>I'm a little busy right now. Come see me closer to the deadline, OK?"));
				else
					npc.Msg(L("I trust that the work is going well?<p/>Don't let me down!"));

				return;
			}

			// Report?
			npc.Msg(L("Did you finish today's work?<br/>Then report now and let's wrap it up.<br/>I trust that the results live up to my name."),
				npc.Button(L("Report Now"), "@report"),
				npc.Button(L("Report Later"), "@later")
				);

			if (await npc.Select() != "@report")
			{
				npc.Msg(L("All right. I look forward to your work."));
				return;
			}

			// Nothing done
			if (result == QuestResult.None)
			{
				npc.GiveUpPtj();

				npc.Msg(npc.FavorExpression(), L("Oh my...<br/>If you're going to be like this, don't even think about working for me again."));
				npc.ModifyRelation(0, -Random(3), 0);
			}
			// Low~Perfect result
			else
			{
				npc.Msg(L("You've done a good job with the task I gave you. Thanks.<br/>Well done. Now choose what you need,<br/>and I'll give it to you."),
					npc.Button(L("Report Later"), "@later"),
					npc.PtjReport(result)
					);
				var reply = await npc.Select();

				// Report later
				if (!reply.StartsWith("@reward:"))
				{
					npc.Msg(npc.FavorExpression(), L("Well, it's up to you."));
					return;
				}

				// Complete
				npc.CompletePtj(reply);
				remaining--;

				// Result msg
				if (result == QuestResult.Perfect)
				{
					npc.Msg(npc.FavorExpression(), L("Yes, perfect.<br/>Well done."));
					npc.ModifyRelation(0, Random(3), 0);
				}
				else if (result == QuestResult.Mid)
				{
					npc.Msg(npc.FavorExpression(), L("Hmmm...<br/>Oh well. I'll take it this time.<br/>I don't want to hear you complain about the compensation, though."));
					npc.ModifyRelation(0, Random(1), 0);
				}
				else if (result == QuestResult.Low)
				{
					npc.Msg(npc.FavorExpression(), L("Are you mocking me? Anyone can do THIS!"));
					npc.ModifyRelation(0, -Random(2), 0);
				}
			}
			return;
		}

		// Check if PTJ time
		if (!npc.ErinnHour(Start, Deadline))
		{
			npc.Msg(L("No, no, no. There is no work before or after the designated time."));
			return;
		}

		// Check if not done today and if there are jobs remaining
		if (!npc.CanDoPtj(JobType, remaining))
		{
			npc.Msg(L("I'm done for today.<br/>Come back tomorrow!"));
			return;
		}

		// Offer PTJ
		var randomPtj = npc.RandomPtj(JobType, QuestIds);
		var msg = "";

		if (npc.GetPtjDoneCount(JobType) == 0)
			msg = L("Do you have any experience in this line of work?<br/>The path of a designer is long and challenging.<br/>If you're feeling confident enough, though, I can entrust you with the work.");
		else
			msg = L("Here to work again?");

		var ptjDescTitle = "";
		// Poll third least-significant digit
		if ((randomPtj / 100) % 10 == 2) // 2 -> Tailoring job
			ptjDescTitle = L("Looking for help with crafting items needed for Clothing Shop.");
		else // not 2, fallback -> Delivery job
			ptjDescTitle = L("Looking for help with delivery of goods in Clothing Shop.");

		npc.Msg(msg, npc.PtjDesc(randomPtj,
			L("Simon's Clothing Shop Part-Time Job"),
			L("ptjDescTitle"),
			PerDay, remaining, npc.GetPtjDoneCount(JobType)));

		if (await npc.Select() == "@accept")
		{
			if (npc.GetPtjDoneCount(JobType) == 0)
				npc.Msg(L("Alright. Finish the work and report back to me before the deadline."));
			else
				npc.Msg(L("All right. I'll see you before the deadline. OK?"));

			npc.StartPtj(randomPtj);
		}
		else
		{
			if (npc.GetPtjDoneCount(JobType) == 0)
				npc.Msg(L("Huh? Are you giving up that easily?"));
			else
				npc.Msg(L("Oh well, then. Maybe next time."));
		}
	}
}

Unimplemented: QuestScripts