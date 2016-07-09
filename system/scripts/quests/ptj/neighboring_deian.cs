//--- Aura Script -----------------------------------------------------------
// Deian's Neighboring Part-Time Job
//--- Description -----------------------------------------------------------
// All quests used by the PTJ, and a script to handle the PTJ via hooks.
//--- Notes -----------------------------------------------------------------
// Apparently the rewards have been increased, as the Wiki now lists
// higher ones than in the past. We'll use the G1 rewards for now.
// http://wiki.mabinogiworld.com/index.php?title=Part-time_Jobs&oldid=30265
//---------------------------------------------------------------------------

public class DeianPtjScript : GeneralScript
{
	const PtjType JobType = PtjType.General;

	const int Start = 7;
	const int Report = 9;
	const int Deadline = 19;
	const int PerDay = 15;

	int remaining = PerDay;

	readonly int[] QuestIds = new int[]
	{
		500101, // Basic  Gather 10 Wool
		500131, // Int    Gather 20 Wool
		500161, // Adv    Gather 30 Wool
	};

	[On("ErinnMidnightTick")]
	public void OnErinnMidnightTick(ErinnTime time)
	{
		// Reset available jobs
		remaining = PerDay;
	}

	public override void Load()
	{
		AddHook("_deian", "after_intro", AfterIntro);
		AddHook("_deian", "before_keywords", BeforeKeywords);
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
			npc.Msg(L("You have some other job?<br/>Shearing is the best part-time job.<br/>You'll understand once you try it."));
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
					npc.Msg(L("You still have time left.<br/>Come back later."));
				else
					npc.Msg(L("How's everything going?<br/>"));
				return;
			}

			// Report?
			npc.Msg(L("Did you finish shearing the sheep?<br/>Let me see if you're done.<br/>If not, you can report it to me later."), npc.Button(L("Report Now"), "@report"), npc.Button(L("Report Later"), "@later"));

			if (await npc.Select() != "@report")
			{
				npc.Msg(L("You can show me later if you're not done.<br/>But even if you are not done, don't forget to report it before the deadline.<br/>I'll just pay for what you have."));
				return;
			}

			// Nothing done
			if (result == QuestResult.None)
			{
				npc.GiveUpPtj();

				npc.Msg(npc.FavorExpression(), L("Hey, this isn't enough...<br/>I won't pay you a penny."));
				npc.ModifyRelation(0, -Random(3), 0);
			}
			// Low~Perfect result
			else
			{
				npc.Msg(L("Wow, <username/>, you are quite a worker. Nicely done.<br/>I will give you anything you want from here as a bonus.<br/>But you'd better not complain about the selections!"), npc.Button(L("Report Later"), "@later"), npc.PtjReport(result));
				var reply = await npc.Select();

				// Report later
				if (!reply.StartsWith("@reward:"))
				{
					npc.Msg(L("You can show me later if you're not done.<br/>But even if you are not done, don't forget to report it before the deadline.<br/>I'll just pay for what you have."));
					return;
				}

				// Complete
				npc.CompletePtj(reply);
				remaining--;

				// Result msg
				if (result == QuestResult.Perfect)
				{
					npc.Msg(npc.FavorExpression(), L("Nice!<br/>Keep up the good work."));
					npc.ModifyRelation(0, Random(3), 0);
				}
				else if (result == QuestResult.Mid)
				{
					npc.Msg(npc.FavorExpression(), L("It's a little less than what you promised,<br/>but I guess it's not that bad. Good enough."));
					npc.ModifyRelation(0, Random(1), 0);
				}
				else if (result == QuestResult.Low)
				{
					npc.Msg(npc.FavorExpression(), L("Hey, this is it?<br/>There's no more? Then this is all I can give you."));
					npc.ModifyRelation(0, -Random(2), 0);
				}
			}
			return;
		}

		// Check if PTJ time
		if (!npc.ErinnHour(Start, Deadline))
		{
			npc.Msg(L("It's not time for part-time jobs yet.<br/>Come back later."));
			return;
		}

		// Check if not done today and if there are jobs remaining
		if (!npc.CanDoPtj(JobType, remaining))
		{
			npc.Msg(L("Ah, I have enough wool for the day.<br/>Want to try again tomorrow?"));
			return;
		}

		// Offer PTJ
		var randomPtj = npc.RandomPtj(JobType, QuestIds);

		// Msg is kinda unofficial, she currently says the following, and then
		// tells you you'd get Homestead seeds.
		npc.Msg(L("Do you want a part-time shearing job?"), npc.PtjDesc(randomPtj, L("Shepherd Boy Deian's Shearing Part-Time Job"), L("Looking for material collectors."), PerDay, remaining, npc.GetPtjDoneCount(JobType)));

		if (await npc.Select() == "@accept")
		{
			npc.Msg(L("Let's see what you've got."));
			npc.StartPtj(randomPtj);
		}
		else
		{
			npc.Msg(L("Forget it if you don't want to do it."));
		}
	}
}

public class DeianWoolBasicPtjScript : QuestScript
{
	public override void Load()
	{
		SetId(500101);
		SetName(L("Shearing Part-Time Job"));
		SetDescription(L("This task is to gather wool that is used to make bandages. I got an order for [10 bundles of wool] that I need you to help me fill. Wool can be obtained from sheep."));

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.ById);

		SetType(QuestType.Deliver);
		SetPtjType(PtjType.General);
		SetLevel(QuestLevel.Basic);
		SetHours(start: 7, report: 9, deadline: 19);

		AddObjective("ptj", L("Gather 10 Bundles of Wool"), 0, 0, 0, Collect(60009, 10));

		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(200));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(300));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(100));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(150));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(40));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(60));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(95));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(380));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(50));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(200));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(20));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(80));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(473));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(95));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(233));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(50));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(80));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(20));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(50021)); // Milk
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(55));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(60005, 10)); // Bandage
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(10));

		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(63000)); // Phoenix Feather
		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(375));
	}
}

public class DeianWoolIntPtjScript : QuestScript
{
	public override void Load()
	{
		SetId(500131);
		SetName(L("Shearing Part-Time Job"));
		SetDescription(L("This task is to gather wool that is used to make bandages. I got an order for [20 bundles of wool] that I need you to help me fill. Wool can be obtained from sheep."));

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.ById);

		SetType(QuestType.Deliver);
		SetPtjType(PtjType.General);
		SetLevel(QuestLevel.Int);
		SetHours(start: 7, report: 9, deadline: 19);

		AddObjective("ptj", L("Gather 20 Bundles of Wool"), 0, 0, 0, Collect(60009, 20));

		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(300));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(575));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(150));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(188));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(60));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(75));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(187));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(660));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(65));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(261));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(27));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(108));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(633));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(125));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(314));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(65));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(111));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(27));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(50021)); // Milk
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(205));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(60005, 10)); // Bandage
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(425));

		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(63000, 5)); // Phoenix Feather
		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(125));

		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(40022)); // Gathering Axe
		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(455));
	}
}

public class DeianWoolAdvPtjScript : QuestScript
{
	public override void Load()
	{
		SetId(500161);
		SetName(L("Shearing Part-Time Job"));
		SetDescription(L("This task is to gather wool that is used to make bandages. I got an order for [30 bundles of wool] that I need you to help me fill. Wool can be obtained from sheep."));

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.ById);

		SetType(QuestType.Deliver);
		SetPtjType(PtjType.General);
		SetLevel(QuestLevel.Adv);
		SetHours(start: 7, report: 9, deadline: 19);

		AddObjective("ptj", L("Gather 30 Bundles of Wool"), 0, 0, 0, Collect(60009, 30));

		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(500));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(950));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(250));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(475));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(100));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(190));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(327));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(1080));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(150));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(550));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(58));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(232));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(1407));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(270));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(1699));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(137));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(276));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(58));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(63000, 10)); // Phoenix Feather
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(350));
	}
}
