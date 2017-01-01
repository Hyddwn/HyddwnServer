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
		514602, // Basic  Mine  5 Iron Ore
		514632, // Int    Mine  7 Iron Ore
		514662, // Adv    Mine 10 Iron Ore
	};

	public override void Load()
	{
		AddHook("_sion", "after_intro", AfterIntro);
		AddHook("_sion", "before_keywords", BeforeKeywords);
	}

	public async Task<HookResult> AfterIntro(NpcScript npc, params object[] args)
	{
		// Handle delivery of Iron Ore from some of his PTJ quests
		int id, itemCount = -1;
		if (npc.QuestActive(id = 514602, "ptj2"))
			itemCount = 5;
		else if (npc.QuestActive(id = 514632, "ptj2"))
			itemCount = 7;
		else if (npc.QuestActive(id = 514662, "ptj2"))
			itemCount = 10;

		if (itemCount != -1)
		{
			if (!npc.Player.Inventory.Has(64002, itemCount)) // Iron Ore
				return HookResult.Continue;

			npc.FinishQuest(id, "ptj2");

			npc.Player.Inventory.Remove(64002, itemCount);
			npc.Notice(L("You have given Iron Ore to Sion."));
			npc.Msg(string.Format(LN("(Gave Sion {0} Iron Ore)", "(Gave Sion {0} Iron Ore)", itemCount), itemCount));
		}

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

		// Offer PTJ
		var randomPtj = npc.RandomPtj(JobType, QuestIds);
		var msg = "";

		if (npc.GetPtjDoneCount(JobType) == 0)
			msg = L("Alright, if you've heard about my family,<br/>then I take it that you know what kind of work you'll be doing?<br/>You're going to need a good grip on that Pickaxe.");
		else
			msg = L("You brought your Pickaxe, right?");

		npc.Msg(msg, npc.PtjDesc(randomPtj,
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
		}
	}
}

public abstract class SionMinePtjBaseScript : QuestScript
{
	protected abstract int QuestId { get; }
	protected abstract int ItemCount { get; }
	protected abstract QuestLevel QuestLevel { get; }

	protected abstract void AddRewards();

	public override void Load()
	{
		SetId(QuestId);
		SetName(L("Iron Ore-Mining Part-time Job"));
		SetDescription(string.Format(LN(
			"This task involves going into the mines to mine out Iron Ore. Today, the task is mining out [{0} Iron Ore]. Mining out the ore will require a Pickaxe, so make sure to bring that before heading over to the mines.",
			"This task involves going into the mines to mine out Iron Ore. Today, the task is mining out [{0} Iron Ore]. Mining out the ore will require a Pickaxe, so make sure to bring that before heading over to the mines.",
			ItemCount), ItemCount
			));

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.ById);

		SetType(QuestType.Deliver);
		SetPtjType(PtjType.Stope);
		SetLevel(QuestLevel);
		SetHours(start: 10, report: 15, deadline: 23);

		AddObjective("ptj1", string.Format(LN("Mine {0} Iron Ore", "Mine {0} Iron Ore", ItemCount), ItemCount), 0, 0, 0, Gather(64002, ItemCount));
		AddObjective("ptj2", string.Format(LN("Deliver {0} Iron Ore to Sion", "Deliver {0} Iron Ore to Sion", ItemCount), ItemCount), 0, 0, 0, Deliver(64002, "_sion"));

		// Sion's AfterIntro is handled above, as we have to deliver the ore to him
		// before he asks us about the PTJ.

		AddRewards();
	}
}

public class SionMineBasicPtjScript : SionMinePtjBaseScript
{
	protected override int QuestId { get { return 514602; } }
	protected override int ItemCount { get { return 5; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Basic; } }

	protected override void AddRewards()
	{
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(220));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(420));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(110));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(210));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(44));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(84));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(125));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(490));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(62));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(245));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(25));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(98));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(620));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(120));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(310));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(60));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(124));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(24));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(40025)); // Pickaxe

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(51011, 5)); // Stamina 10 Potion

		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(63170)); // Pass to the Hidden Mine
	}
}

public class SionMineIntPtjScript : SionMinePtjBaseScript
{
	protected override int QuestId { get { return 514632; } }
	protected override int ItemCount { get { return 7; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Int; } }

	protected override void AddRewards()
	{
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(300));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(720));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(150));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(360));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(60));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(144));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(220));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(780));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(110));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(390));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(44));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(156));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(1000));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(195));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(500));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(97));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(200));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(39));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(40025)); // Pickaxe

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(51011, 10)); // Stamina 10 Potion

		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(63170)); // Pass to the Hidden Mine
	}
}

public class SionMineAdvPtjScript : SionMinePtjBaseScript
{
	protected override int QuestId { get { return 514662; } }
	protected override int ItemCount { get { return 10; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Adv; } }

	protected override void AddRewards()
	{
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(500));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(1200));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(250));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(600));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(100));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(240));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(370));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(1300));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(185));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(650));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(74));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(260));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(1680));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(315));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(840));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(157));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(336));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(63));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(40025)); // Pickaxe

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(51012, 5)); // Stamina 30 Potion

		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(63170)); // Pass to the Hidden Mine

		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, QuestScroll(40012)); // Big Order of Iron Ore
	}
}
