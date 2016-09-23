//--- Aura Script -----------------------------------------------------------
// Piaras's Inn Part-Time Job
//--- Description -----------------------------------------------------------
// All quests used by the PTJ, and a script to handle the PTJ via hooks.
//--- Notes -----------------------------------------------------------------
// Apparently the rewards have been increased, as the Wiki now lists
// higher ones than in the past. We'll use the G1 rewards for now.
// http://wiki.mabinogiworld.com/index.php?title=Part-time_Jobs&oldid=30265
//---------------------------------------------------------------------------

public class PiarasPtjScript : GeneralScript
{
	const PtjType JobType = PtjType.Inn;

	const int Start = 7;
	const int Report = 9;
	const int Deadline = 19;
	const int PerDay = 15;

	int remaining = PerDay;

	readonly int[] QuestIds = new int[]
	{
		513103, // Basic  Gather 10 Pieces of Firewood
		513133, // Int    Gather 20 Pieces of Firewood
		513163, // Adv    Gather 30 Pieces of Firewood
	};

	[On("ErinnMidnightTick")]
	public void OnErinnMidnightTick(ErinnTime time)
	{
		// Reset available jobs
		remaining = PerDay;
	}

	public override void Load()
	{
		AddHook("_piaras", "after_intro", AfterIntro);
		AddHook("_piaras", "before_keywords", BeforeKeywords);
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
			npc.Msg(L("Are you working on a different part-time job?<br/>Well, then. Please help me in the future when you have a chance."));
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
					npc.Msg(L("Ah, you are here already?<br/>It's a little bit too early. Can you come back around the deadline?"));
				else
					npc.Msg(L("I hope you didn't forget what I asked you to do.<p/>Please have it done by the deadline."));
				return;
			}

			// Report?
			npc.Msg(L("Did you complete the task I requested?<br/>You can report now and finish it up,<br/>or you may report it later if you're not done yet."), npc.Button(L("Report Now"), "@report"), npc.Button(L("Report Later"), "@later"));

			if (await npc.Select() != "@report")
			{
				npc.Msg(L("Please report before the deadline is over.<br/>Even if the work is not done, you should still report.<br/>Then I can pay you for what you've completed."));
				return;
			}

			// Nothing done
			if (result == QuestResult.None)
			{
				npc.GiveUpPtj();

				npc.Msg(npc.FavorExpression(), L("Ha ha. This is a little disappointing.<br/>I don't think I can pay you for this."));
				npc.ModifyRelation(0, -Random(3), 0);
			}
			// Low~Perfect result
			else
			{
				npc.Msg(L("You are quite skillful, <username/>.<br/>Now there's nothing to worry about even if I get too much work. Ha ha.<br/>Please choose what you want. You deserve it.<br/>I'd like to give it to you as a compensation for your hard work."), npc.Button(L("Report Later"), "@later"), npc.PtjReport(result));
				var reply = await npc.Select();

				// Report later
				if (!reply.StartsWith("@reward:"))
				{
					npc.Msg(L("Please report before the deadline is over.<br/>Even if the work is not done, you should still report.<br/>Then I can pay you for what you've completed."));
					return;
				}

				// Complete
				npc.CompletePtj(reply);
				remaining--;

				// Result msg
				if (result == QuestResult.Perfect)
				{
					npc.Msg(npc.FavorExpression(), L("Great! You have done well as I requested.<br/>I hope you can help me again next time."));
					npc.ModifyRelation(0, Random(3), 0);
				}
				else if (result == QuestResult.Mid)
				{
					npc.Msg(npc.FavorExpression(), L("Thank you. Although you didn't complete the job, you've done enough so far.<br/>But I'm sorry to tell you I must deduct a little from your pay."));
					npc.ModifyRelation(0, Random(1), 0);
				}
				else if (result == QuestResult.Low)
				{
					npc.Msg(npc.FavorExpression(), L("Hmm... It's not exactly what I expected, but thank you.<br/>I'm afraid this is all I can pay you."));
					npc.ModifyRelation(0, -Random(2), 0);
				}
			}
			return;
		}

		// Check if PTJ time
		if (!npc.ErinnHour(Start, Deadline))
		{
			npc.Msg(L("Hmm... It's not a good time for this.<br/>Can you come back when it is time for part-time jobs?"));
			return;
		}

		// Check if not done today and if there are jobs remaining
		if (!npc.CanDoPtj(JobType, remaining))
		{
			npc.Msg(L("I'm all set for today.<br/>Will you come back tomorrow?"));
			return;
		}

		// Offer PTJ
		var randomPtj = npc.RandomPtj(JobType, QuestIds);

		// Msg is kinda unofficial, she currently says the following, and then
		// tells you you'd get Homestead seeds.
		npc.Msg(L("Are you here for a part-time job at my Inn again?"), npc.PtjDesc(randomPtj, L("Piaras's Inn Part-time Job"), L("Looking for help with delivering goods to Inn."), PerDay, remaining, npc.GetPtjDoneCount(JobType)));

		if (await npc.Select() == "@accept")
		{
			npc.Msg(L("I'll be counting on you as usual."));
			npc.StartPtj(randomPtj);
		}
		else
		{
			npc.Msg(L("You want to sleep on it?<br/>Alright, then.<br/>But report on time please."));
		}
	}
}

public class PiarasFirewoodBasicPtjScript : QuestScript
{
	public override void Load()
	{
		SetId(513103);
		SetName(L("Inn Part-time Job"));
		SetDescription(L("There is a shortage of firewood at the Inn. Please collect [10 pieces of firewood]. First of all, an axe is required to chop firewood."));

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.ById);

		SetType(QuestType.Deliver);
		SetPtjType(PtjType.Inn);
		SetLevel(QuestLevel.Basic);
		SetHours(start: 7, report: 9, deadline: 19);

		AddObjective("ptj", L("Gather 10 Pieces of Firewood"), 0, 0, 0, Collect(63002, 10));

		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(250));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(225));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(125));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(112));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(50));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(45));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(87));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(350));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(46));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(185));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(19));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(76));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(433));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(87));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(213));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(46));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(76));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(19));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(50021)); // Milk
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(17));
	}
}

public class PiarasFirewoodIntPtjScript : QuestScript
{
	public override void Load()
	{
		SetId(513133);
		SetName(L("Inn Part-time Job"));
		SetDescription(L("There is a shortage of firewood at the Inn. Please collect [20 pieces of firewood]. First of all, an axe is required to chop firewood."));

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.ById);

		SetType(QuestType.Deliver);
		SetPtjType(PtjType.Inn);
		SetLevel(QuestLevel.Int);
		SetHours(start: 7, report: 9, deadline: 19);

		AddObjective("ptj", L("Gather 20 Pieces of Firewood"), 0, 0, 0, Collect(63002, 20));

		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(400));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(375));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(200));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(188));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(80));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(75));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(153));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(560));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(73));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(290));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(31));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(124));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(713));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(140));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(354));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(73));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(132));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(31));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(50021)); // Milk
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(280));
	}
}

public class PiarasFirewoodAdvPtjScript : QuestScript
{
	public override void Load()
	{
		SetId(513163);
		SetName(L("Inn Part-time Job"));
		SetDescription(L("There is a shortage of firewood at the Inn. Please collect [30 pieces of firewood]. First of all, an axe is required to chop firewood."));

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.ById);

		SetType(QuestType.Deliver);
		SetPtjType(PtjType.Inn);
		SetLevel(QuestLevel.Adv);
		SetHours(start: 7, report: 9, deadline: 19);

		AddObjective("ptj", L("Gather 30 Pieces of Firewood"), 0, 0, 0, Collect(63002, 30));

		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(700));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(700));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(350));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(350));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(140));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(140));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(300));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(1000));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(137));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(510));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(54));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(216));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(1300));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(250));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(647));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(128));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(255));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(54));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(63000, 10)); // Phoenix Feather
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(250));
	}
}
