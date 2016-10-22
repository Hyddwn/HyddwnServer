//--- Aura Script -----------------------------------------------------------
// Comgan's Monster-Hunting Part-Time Job
//--- Description -----------------------------------------------------------
// All quests used by the PTJ, and a script to handle the PTJ via hooks.
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
		502301, // Basic  Hunt 4 Goblins
		502331, // Int    Hunt 4 Goblins, 1 Imp
		502361, // Adv    Hunt 3 Goblins, 3 Imps
	};

	public override void Load()
	{
		AddHook("_comgan", "after_intro", AfterIntro);
		AddHook("_comgan", "before_keywords", BeforeKeywords);
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

		// Offer PTJ
		var randomPtj = npc.RandomPtj(JobType, QuestIds);
		var msg = "";

		if (npc.GetPtjDoneCount(JobType) == 0)
			msg = L("As you can see, I am in need of financial assistance,<br/>not in a position to help financially.<br/>But if you help with the tasks, I can at least give you some holy water.");
		else
			msg = L("Do you need holy water again today?");

		npc.Msg(msg, npc.PtjDesc(randomPtj,
			L("Comgan's Monster-Hunting Part-Time Job"),
			L("Looking for monster hunters in Church."),
			PerDay, remaining, npc.GetPtjDoneCount(JobType)));

		if (await npc.Select() == "@accept")
		{
			if (npc.GetPtjDoneCount(JobType) == 0)
				npc.Msg(L("Please do come back to report before the deadline."));
			else
				npc.Msg(L("Please be careful not to miss the deadline."));

			npc.StartPtj(randomPtj);
		}
		else
		{
			if (npc.GetPtjDoneCount(JobType) == 0)
				npc.Msg(L("Don't like the given work, do you?"));
			else
				npc.Msg(L("I am sorry, but if you won't help with the tasks,<br/>I cannot really help you, either."));
		}
	}
}

public class ComganHuntBasicPtjScript : QuestScript
{
	public override void Load()
	{
		SetId(502301);
		SetName(L("Monster-Hunting Part-Time Job"));
		SetDescription(L("Evil creatures are invading our homes! If you hunt [4 goblins] I'll give you some holy water."));

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.ById);

		SetType(QuestType.Deliver);
		SetPtjType(PtjType.Church);
		SetLevel(QuestLevel.Basic);
		SetHours(start: 12, report: 16, deadline: 21);

		AddObjective("ptj", L("Hunt 4 Goblins"), 0, 0, 0, Kill(4, "/goblin/"));

		AddReward(1, RewardGroupType.Item, QuestResult.Perfect, Item(63016, 4)); // Holy Water of Lymilark
		AddReward(1, RewardGroupType.Item, QuestResult.Perfect, Exp(220));
		AddReward(1, RewardGroupType.Item, QuestResult.Mid, Item(63016, 2)); // Holy Water of Lymilark
		AddReward(1, RewardGroupType.Item, QuestResult.Mid, Exp(110));
		AddReward(1, RewardGroupType.Item, QuestResult.Low, Item(63016, 1)); // Holy Water of Lymilark
		AddReward(1, RewardGroupType.Item, QuestResult.Low, Exp(44));
	}
}

public class ComganHuntIntPtjScript : QuestScript
{
	public override void Load()
	{
		SetId(502331);
		SetName(L("Monster-Hunting Part-Time Job"));
		SetDescription(L("Evil creatures are invading our homes! If you hunt [4 goblins and 1 imp] I'll give you some holy water."));

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.ById);

		SetType(QuestType.Deliver);
		SetPtjType(PtjType.Church);
		SetLevel(QuestLevel.Int);
		SetHours(start: 12, report: 16, deadline: 21);

		AddObjective("ptj1", L("Hunt 4 Goblins"), 0, 0, 0, Kill(4, "/goblin/"));
		AddObjective("ptj2", L("Hunt 1 Imp"), 0, 0, 0, Kill(1, "/imp/"));

		AddReward(1, RewardGroupType.Item, QuestResult.Perfect, Item(63016, 6)); // Holy Water of Lymilark
		AddReward(1, RewardGroupType.Item, QuestResult.Perfect, Exp(300));
		AddReward(1, RewardGroupType.Item, QuestResult.Mid, Item(63016, 3)); // Holy Water of Lymilark
		AddReward(1, RewardGroupType.Item, QuestResult.Mid, Exp(150));
		AddReward(1, RewardGroupType.Item, QuestResult.Low, Item(63016, 1)); // Holy Water of Lymilark
		AddReward(1, RewardGroupType.Item, QuestResult.Low, Exp(60));
	}
}

public class ComganHuntAdvPtjScript : QuestScript
{
	public override void Load()
	{
		SetId(502361);
		SetName(L("Monster-Hunting Part-Time Job"));
		SetDescription(L("Evil creatures are invading our homes! If you hunt [3 goblins and 3 imps] I'll give you some holy water."));

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.ById);

		SetType(QuestType.Deliver);
		SetPtjType(PtjType.Church);
		SetLevel(QuestLevel.Adv);
		SetHours(start: 12, report: 16, deadline: 21);

		AddObjective("ptj1", L("Hunt 3 Goblins"), 0, 0, 0, Kill(4, "/goblin/"));
		AddObjective("ptj2", L("Hunt 3 Imps"), 0, 0, 0, Kill(3, "/imp/"));

		AddReward(1, RewardGroupType.Item, QuestResult.Perfect, Item(63016, 10)); // Holy Water of Lymilark
		AddReward(1, RewardGroupType.Item, QuestResult.Perfect, Exp(500));
		AddReward(1, RewardGroupType.Item, QuestResult.Mid, Item(63016, 5)); // Holy Water of Lymilark
		AddReward(1, RewardGroupType.Item, QuestResult.Mid, Exp(250));
		AddReward(1, RewardGroupType.Item, QuestResult.Low, Item(63016, 2)); // Holy Water of Lymilark
		AddReward(1, RewardGroupType.Item, QuestResult.Low, Exp(100));

		AddReward(2, RewardGroupType.Item, QuestResult.Perfect, Item(40004)); // Lute
		AddReward(2, RewardGroupType.Item, QuestResult.Perfect, Item(19001)); // Robe
	}
}
