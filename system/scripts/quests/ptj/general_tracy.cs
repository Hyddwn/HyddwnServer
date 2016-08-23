//--- Aura Script -----------------------------------------------------------
// Tracy's Firewood-Chopping (General) Part-Time Job
//--- Description -----------------------------------------------------------
// All quests used by the PTJ, and a script to handle the PTJ via hooks.
//---------------------------------------------------------------------------

public class TracyPtjScript : GeneralScript
{
	const PtjType JobType = PtjType.General;

	const int Start = 7;
	const int Report = 9;
	const int Deadline = 19;
	const int PerDay = 10;

	int remaining = PerDay;

	readonly int[] QuestIds = new int[]
	{
		500102, // Basic  Gather 10 Pieces of Firewood
		500132, // Int    Gather 20 Pieces of Firewood
		500162, // Adv    Gather 30 Pieces of Firewood
	};

	public override void Load()
	{
		AddHook("_tracy", "after_intro", AfterIntro);
		AddHook("_tracy", "before_keywords", BeforeKeywords);
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
			npc.Msg(L("You can do only one part-time job per day.<br/>You're done for the day, so find something else to do."));
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
					npc.Msg(L("What? Already finished everything?<br/>Man, that's the spirit.<br/>But there's still time left before the deadline.<br/>Come back a little later. Haha..."));
				else
					npc.Msg(L("Chopping enough firewood?"));
				return;
			}

			// Report?
			npc.Msg(L("Got enough firewood?<br/>Report to me later if you need more time.<br/>If you want to finish it now, I can pay for what you did so far."),
				npc.Button(L("Report Now"), "@report"),
				npc.Button(L("Report Later"), "@later")
				);

			if (await npc.Select() != "@report")
			{
				npc.Msg(L("Right. That's it.<br/>Things done by halves are never done right!<br/>Just finish it before the deadline."));
				return;
			}

			// Nothing done
			if (result == QuestResult.None)
			{
				npc.GiveUpPtj();

				npc.Msg(npc.FavorExpression(), L("What is this? A joke?<br/>Don't even think about a payment!"));
				npc.ModifyRelation(0, -Random(3), 0);
			}
			// Low~Perfect result
			else
			{
				npc.Msg(L("Hear, Hear! <username/> just did another wonderful job!<br/>Outstanding, you are a lucky guy today. Just take anything you want from here. Haha!"),
					npc.Button(L("Report Later"), "@later"),
					npc.PtjReport(result)
					);
				var reply = await npc.Select();

				// Report later
				if (!reply.StartsWith("@reward:"))
				{
					npc.Msg(npc.FavorExpression(), L("What's this? Are you saying you don't like the reward<br/>Tracy the Lumberjack prepared for you?"));
					return;
				}

				// Complete
				npc.CompletePtj(reply);
				remaining--;

				// Result msg
				if (result == QuestResult.Perfect)
				{
					npc.Msg(npc.FavorExpression(), L("Hmm...<br/>Good! That's good enough."));
					npc.ModifyRelation(0, Random(3), 0);
				}
				else if (result == QuestResult.Mid)
				{
					npc.Msg(npc.FavorExpression(), L("Well, not bad.<br/>Put a little more effort next time."));
					npc.ModifyRelation(0, Random(1), 0);
				}
				else if (result == QuestResult.Low)
				{
					npc.Msg(npc.FavorExpression(), L("What... You think this is a joke?<br/>That's all I can pay.<br/>And I won't accept results like this next time."));
					npc.ModifyRelation(0, -Random(2), 0);
				}
			}
			return;
		}

		// Check if PTJ time
		if (!npc.ErinnHour(Start, Deadline))
		{
			npc.Msg(L("You want a logging job?<br/>This is not the right time. Come back later.<br/>When the shadow's in the northwest... I think 7 o'clock in the morning will do."));
			return;
		}

		// Check if not done today and if there are jobs remaining
		if (!npc.CanDoPtj(JobType, remaining))
		{
			npc.Msg(L("You can do only one part-time job per day.<br/>You're done for the day, so find something else to do."));
			return;
		}

		// Offer PTJ
		var randomPtj = npc.RandomPtj(JobType, QuestIds);
		var msg = "";

		if (npc.GetPtjDoneCount(JobType) == 0)
		{
			npc.Msg(L("Good. You want a logging job?<br/>I was actually a little bored working alone.<br/>I can use some help. If you're good enough, I can pay you more."));
			msg = L("Want to give it a try?");
		}
		else
		{
			msg = L("Want to play lumberjack again?");
		}

		npc.Msg(msg, npc.PtjDesc(randomPtj,
			L("Tracy's Firewood-Chopping PTJ"),
			L("Looking for material collectors."),
			PerDay, remaining, npc.GetPtjDoneCount(JobType)));

		if (await npc.Select() == "@accept")
		{
			if (npc.GetPtjDoneCount(JobType) == 0)
				npc.Msg(L("(missing): first time accepting PTJ offer"));
			else
				npc.Msg(L("Outstanding! Go on, man."));

			npc.StartPtj(randomPtj);
		}
		else
		{
			if (npc.GetPtjDoneCount(JobType) == 0)
				npc.Msg(L("(missing): first time declining PTJ offer"));
			else
				npc.Msg(L("Well... if you're not interested..."));
		}
	}
}

public abstract class TracyFirewoodPtjBaseScript : QuestScript
{
	protected abstract int QuestId { get; }
	protected abstract int ItemCount { get; }
	protected abstract QuestLevel QuestLevel { get; }
	protected abstract void AddRewards();

	public override void Load()
	{
		SetId(QuestId);
		SetName(L("Firewood-Chopping PTJ"));
		SetDescription(string.Format(LN(
			"A part-time job to chop firewood from the trees in the forest. Collect [{0} piece of firewood]. First, you need an axe to chop firewood.",
			"A part-time job to chop firewood from the trees in the forest. Collect [{0} pieces of firewood]. First, you need an axe to chop firewood.",
			ItemCount), ItemCount));

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.ById);

		SetType(QuestType.Deliver);
		SetPtjType(PtjType.General);
		SetLevel(QuestLevel);
		SetHours(start: 7, report: 9, deadline: 19);

		AddObjective("ptj", string.Format(LN("Collect {0} Piece of Firewood", "Collect {0} Pieces of Firewood", ItemCount), ItemCount), 0, 0, 0, Collect(63002, ItemCount));

		AddRewards();
	}
}

public class TracyFirewoodBasicPtjScript : TracyFirewoodPtjBaseScript
{
	protected override int QuestId { get { return 500102; } }
	protected override int ItemCount { get { return 10; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Basic; } }

	protected override void AddRewards()
	{
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(250));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(225));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(125));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(112));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(50));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(45));

		AddReward(2, RewardGroupType.Exp, QuestResult.Perfect, Exp(440));
		AddReward(2, RewardGroupType.Exp, QuestResult.Perfect, Gold(90));
		AddReward(2, RewardGroupType.Exp, QuestResult.Mid, Exp(220));
		AddReward(2, RewardGroupType.Exp, QuestResult.Mid, Gold(45));
		AddReward(2, RewardGroupType.Exp, QuestResult.Low, Exp(88));
		AddReward(2, RewardGroupType.Exp, QuestResult.Low, Gold(18));

		AddReward(3, RewardGroupType.Item, QuestResult.Perfect, Item(60005, 10)); // Bandage
		AddReward(3, RewardGroupType.Item, QuestResult.Perfect, Gold(237));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(40022)); // Gathering Axe
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(267));
	}
}

public class TracyFirewoodIntPtjScript : TracyFirewoodPtjBaseScript
{
	protected override int QuestId { get { return 500132; } }
	protected override int ItemCount { get { return 20; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Int; } }

	protected override void AddRewards()
	{
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(480));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(450));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(240));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(225));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(96));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(90));

		AddReward(2, RewardGroupType.Item, QuestResult.Perfect, Item(50006, 1)); // Slice of Meat
		AddReward(2, RewardGroupType.Item, QuestResult.Perfect, Gold(90));
	}
}

public class TracyFirewoodAdvPtjScript : TracyFirewoodPtjBaseScript
{
	protected override int QuestId { get { return 500162; } }
	protected override int ItemCount { get { return 30; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Adv; } }

	protected override void AddRewards()
	{
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(840));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(840));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(420));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(420));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(168));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(168));

		AddReward(2, RewardGroupType.Item, QuestResult.Perfect, Item(40042)); // Cooking Knife
		AddReward(2, RewardGroupType.Item, QuestResult.Perfect, Gold(504));

		AddReward(3, RewardGroupType.Scroll, QuestResult.Perfect, QuestScroll(40005)); // Collecting Quest - Big Order of Firewood
	}
}