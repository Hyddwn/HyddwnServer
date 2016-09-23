//--- Aura Script -----------------------------------------------------------
// Nerys's Weapon Shop Part-Time Job
//--- Description -----------------------------------------------------------
// All quests used by the PTJ, and a script to handle the PTJ via hooks.
//---------------------------------------------------------------------------

public class NerysPtjScript : GeneralScript
{
	const PtjType JobType = PtjType.WeaponsShop;

	const int Start = 15;
	const int Report = 17;
	const int Deadline = 22;
	const int PerDay = 10;

	int remaining = PerDay;

	readonly int[] QuestIds = new int[]
	{
		512401, // Basic  Item delivery (Kristell)
		512431, // Int    Item delivery (Kristell)
		512461, // Adv    Item delivery (Kristell)
		512402, // Basic  Item delivery (Glenis)
		512432, // Int    Item delivery (Glenis)
		512462, // Adv    Item delivery (Glenis)
		512403, // Basic  Item delivery (Walter)
		512433, // Int    Item delivery (Walter)
		512463, // Adv    Item delivery (Walter)
		512404, // Basic  Item delivery (Simon)
		512434, // Int    Item delivery (Simon)
		512464, // Adv    Item delivery (Simon)
	};

	public override void Load()
	{
		AddHook("_nerys", "after_intro", AfterIntro);
		AddHook("_nerys", "before_keywords", BeforeKeywords);
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
			npc.Msg(L("You seem to have another job. Don't you think you should finish that first?"));
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
					npc.Msg(L("Now is not the time. Want to come back later?"));
				}
				else
				{
					npc.Msg(L("I trust that the work is going well?<p/>I'm getting worried for no reason."));
				}
				return;
			}

			// Report?
			npc.Msg(L("Did you finish today's work?<br/>If so, would you like to report now and wrap it up?"),
				npc.Button(L("Report Now"), "@report"),
				npc.Button(L("Report Later"), "@later")
				);

			if (await npc.Select() != "@report")
			{
				npc.Msg(L("I look forward to your work."));
				return;
			}

			// Nothing done
			if (result == QuestResult.None)
			{
				npc.GiveUpPtj();

				npc.Msg(npc.FavorExpression(), L("*Sigh*<br/>If you're going to be like this, don't even start working next time."));
				npc.ModifyRelation(0, -Random(3), 0);
			}
			// Low~Perfect result
			else
			{
				npc.Msg(L("Wow, you're not bad at all. I thought it would be rather difficult for you.<br/>Ha. Then take your pick among these items.<br/>Thank you for the hard work, <username/>."),
					npc.Button(L("Report Later"), "@later"),
					npc.PtjReport(result)
					);
				var reply = await npc.Select();

				// Report later
				if (!reply.StartsWith("@reward:"))
				{
					npc.Msg(npc.FavorExpression(), L("Well, then I guess I'll see you next time.<br/>Someone else might take away all the good stuff in the meantime, though."));
					return;
				}

				// Complete
				npc.CompletePtj(reply);
				remaining--;

				// Result msg
				if (result == QuestResult.Perfect)
				{
					npc.Msg(npc.FavorExpression(), L("Wow, it's perfect!<br/>Thanks for the help."));
					npc.ModifyRelation(0, Random(3), 0);
				}
				else
				{
					// Nerys doesn't have any PTJ quests that could yield
					// mid/low results.
				}
			}
			return;
		}

		// Check if PTJ time
		if (!npc.ErinnHour(Start, Deadline))
		{
			npc.Msg(L("Come back during the business hours."));
			return;
		}

		// Check if not done today and if there are jobs remaining
		if (!npc.CanDoPtj(JobType, remaining))
		{
			npc.Msg(L("Today's work is done. Come back tomorrow."));
			return;
		}

		// Offer PTJ
		var randomPtj = npc.RandomPtj(JobType, QuestIds);
		var msg = "";

		if (npc.GetPtjDoneCount(JobType) == 0)
			msg = L("Need work, do you?<br/>Would you like to give me a hand? I'll pay you, too.<br/>Interested?");
		else
			msg = L("Here to help out again?");

		npc.Msg(msg, npc.PtjDesc(randomPtj,
			L("Nerys's Weapons Shop Part-time Job"),
			L("Looking for help with delivery of goods in Weapons Shop."),
			PerDay, remaining, npc.GetPtjDoneCount(JobType)));

		if (await npc.Select() == "@accept")
		{
			if (npc.GetPtjDoneCount(JobType) == 0)
				npc.Msg(L("Alright. Finish the work and report back to me before the deadline."));
			else
				npc.Msg(L("Alright. I'll see you before the deadline."));

			npc.StartPtj(randomPtj);
		}
		else
		{
			if (npc.GetPtjDoneCount(JobType) == 0)
				npc.Msg(L("Mmm? Are you giving up?"));
			else
				npc.Msg(L("Oh well, then. Maybe next time."));
		}
	}
}

public abstract class NerysDeliveryPtjBaseScript : QuestScript
{
	protected abstract int QuestId { get; }
	protected abstract string LQuestDescription { get; }
	protected abstract QuestLevel QuestLevel { get; }
	protected abstract string LObjectiveDescription { get; }
	protected abstract int ItemId { get; }
	protected abstract string NpcIdent { get; }
	protected abstract string LGivenNotice { get; }

	public override void Load()
	{
		SetId(QuestId);
		SetName(L("Weapons Shop Part-time Job"));
		SetDescription(LQuestDescription);

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.ById);

		SetType(QuestType.Deliver);
		SetPtjType(PtjType.WeaponsShop);
		SetLevel(QuestLevel);
		SetHours(start: 15, report: 17, deadline: 22);

		AddObjective("ptj", LObjectiveDescription, 0, 0, 0, Deliver(ItemId, NpcIdent));
		AddHook(NpcIdent, "after_intro", AfterIntro);

		AddRewards();
	}

	public async Task<HookResult> AfterIntro(NpcScript npc, params object[] args)
	{
		if (!npc.QuestActive(this.Id, "ptj"))
			return HookResult.Continue;

		if (!npc.Player.Inventory.Has(ItemId))
			return HookResult.Continue;

		npc.Player.Inventory.Remove(ItemId, 1);
		npc.Notice(LGivenNotice);
		npc.FinishQuest(this.Id, "ptj");

		await this.OnFinish(npc);

		return HookResult.Break;
	}

	protected virtual async Task OnFinish(NpcScript npc)
	{
		await Task.Yield();
	}

	private void AddRewards()
	{
		// Rewards are common among all of Nerys's PTJs.
		switch (QuestLevel)
		{
			case QuestLevel.Basic:
				AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(300));
				AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(220));
				AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(150));
				AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(110));
				AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(60));
				AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(44));

				AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(90));
				AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(380));
				AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(45));
				AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(190));
				AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(18));
				AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(76));

				AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(475));
				AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(90));
				AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(237));
				AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(45));
				AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(95));
				AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(18));

				AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(18006)); // Wizard Hat
				AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(70));

				AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(51001, 8)); // HP 10 Potion
				AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(40));

				AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(51011, 10)); // Stamina 10 Potion
				AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(370));

				AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(40001)); // Wooden Stick
				AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(370));
				break;

			case QuestLevel.Int:
				AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(400));
				AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(320));
				AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(200));
				AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(160));
				AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(80));
				AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(64));

				AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(135));
				AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(520));
				AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(77));
				AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(260));
				AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(27));
				AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(104));

				AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(655));
				AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(130));
				AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(327));
				AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(65));
				AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(131));
				AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(26));

				AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(40026)); // Sickle
				AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(45));

				AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(18006)); // Wizard Hat
				AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(245));

				AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(51001, 10)); // HP 10 Potion
				AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(145));

				AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(51011, 10)); // Stamina 10 Potion
				AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(545));
				break;

			case QuestLevel.Adv:
				AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(500));
				AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(400));
				AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(250));
				AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(200));
				AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(100));
				AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(80));

				AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(180));
				AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(640));
				AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(90));
				AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(320));
				AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(36));
				AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(128));

				AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(820));
				AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(160));
				AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(410));
				AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(80));
				AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(164));
				AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(32));

				AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(40025)); // Pickaxe
				AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(50));

				AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(40026)); // Sickle
				AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(200));

				AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(18006)); // Wizard Hat
				AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(400));

				AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(51001, 10)); // HP 10 Potion
				AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(300));
				break;

			default:
				Log.Warning("NerysPtjBaseScript: Invalid PTJ quest level for quest ID {0}. Falling back to Basic to determine rewards.", QuestId);
				goto case QuestLevel.Basic;
		}
	}
}

public abstract class NerysDeliveryKristellPtjBaseScript : NerysDeliveryPtjBaseScript
{
	protected override string LQuestDescription { get { return L("I need to deliver a [candlestick] to the priestess at Church. Can you help me? - Nerys -"); } }
	protected override string LObjectiveDescription { get { return L("Deliver the [Candlestick] to Kristell."); } }
	protected override int ItemId { get { return 70009; } }
	protected override string NpcIdent { get { return "_kristell"; } }
	protected override string LGivenNotice { get { return L("You have given Candlestick to be Delivered to Kristell."); } }

	protected override async Task OnFinish(NpcScript npc)
	{
		npc.Msg(L("It must be the candlestick from Nerys.<br/>Thank you, I can really put this to good use. Hehe."));
		npc.Msg(Hide.Name, L("(Delivered the Candlestick to Kristell.)"));
	}
}

public class NerysDeliveryKristellBasicPtjScript : NerysDeliveryKristellPtjBaseScript
{
	protected override int QuestId { get { return 512401; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Basic; } }
}

public class NerysDeliveryKristellIntPtjScript : NerysDeliveryKristellPtjBaseScript
{
	protected override int QuestId { get { return 512431; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Int; } }
}

public class NerysDeliveryKristellAdvPtjScript : NerysDeliveryKristellPtjBaseScript
{
	protected override int QuestId { get { return 512461; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Adv; } }
}

public abstract class NerysDeliveryGlenisPtjBaseScript : NerysDeliveryPtjBaseScript
{
	protected override string LQuestDescription { get { return L("I need to send this [ring] to Glenis at the Grocery Store. Can you help me? - Nerys -"); } }
	protected override string LObjectiveDescription { get { return L("Deliver the [Ring] to Glenis."); } }
	protected override int ItemId { get { return 70022; } }
	protected override string NpcIdent { get { return "_glenis"; } }
	protected override string LGivenNotice { get { return L("You have given Gift Ring to be Delivered to Glenis."); } }

	protected override async Task OnFinish(NpcScript npc)
	{
		// Glenis says nothing.
	}
}

public class NerysDeliveryGlenisBasicPtjScript : NerysDeliveryGlenisPtjBaseScript
{
	protected override int QuestId { get { return 512402; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Basic; } }
}

public class NerysDeliveryGlenisIntPtjScript : NerysDeliveryGlenisPtjBaseScript
{
	protected override int QuestId { get { return 512432; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Int; } }
}

public class NerysDeliveryGlenisAdvPtjScript : NerysDeliveryGlenisPtjBaseScript
{
	protected override int QuestId { get { return 512462; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Adv; } }
}

public abstract class NerysDeliveryWalterPtjBaseScript : NerysDeliveryPtjBaseScript
{
	protected override string LQuestDescription { get { return L("I need to deliver a [Full Ring Mail] to Walter at the General Shop. Can you help me? - Nerys -"); } }
	protected override string LObjectiveDescription { get { return L("Deliver a [Full Ring Mail] to Walter."); } }
	protected override int ItemId { get { return 70002; } }
	protected override string NpcIdent { get { return "_walter"; } }
	protected override string LGivenNotice { get { return L("You have given Full Ring Mail to be Delivered to Walter."); } }

	protected override async Task OnFinish(NpcScript npc)
	{
		// Walter says nothing.
	}
}

public class NerysDeliveryWalterBasicPtjScript : NerysDeliveryWalterPtjBaseScript
{
	protected override int QuestId { get { return 512403; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Basic; } }
}

public class NerysDeliveryWalterIntPtjScript : NerysDeliveryWalterPtjBaseScript
{
	protected override int QuestId { get { return 512433; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Int; } }
}

public class NerysDeliveryWalterAdvPtjScript : NerysDeliveryWalterPtjBaseScript
{
	protected override int QuestId { get { return 512463; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Adv; } }
}

public abstract class NerysDeliverySimonPtjBaseScript : NerysDeliveryPtjBaseScript
{
	protected override string LQuestDescription { get { return L("I need to deliver a [Tailoring Kit] to Simon at the Clothing Shop. Can you help me? - Nerys -"); } }
	protected override string LObjectiveDescription { get { return L("Deliver a [Tailoring Kit] to Simon."); } }
	protected override int ItemId { get { return 70033; } }
	protected override string NpcIdent { get { return "_simon"; } }
	protected override string LGivenNotice { get { return L("You have given Tailoring Kit to be Delivered to Simon."); } }

	protected override async Task OnFinish(NpcScript npc)
	{
		npc.Msg(L("From Nerys, right?<br/>Ho ho, thank you."));
		npc.Msg(Hide.Name, L("(Delivered the Tailoring Kit to Simon.)"));
	}
}

public class NerysDeliverySimonBasicPtjScript : NerysDeliverySimonPtjBaseScript
{
	protected override int QuestId { get { return 512404; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Basic; } }
}

public class NerysDeliverySimonIntPtjScript : NerysDeliverySimonPtjBaseScript
{
	protected override int QuestId { get { return 512434; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Int; } }
}

public class NerysDeliverySimonAdvPtjScript : NerysDeliverySimonPtjBaseScript
{
	protected override int QuestId { get { return 512464; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Adv; } }
}