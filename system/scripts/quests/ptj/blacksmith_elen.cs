//--- Aura Script -----------------------------------------------------------
// Elen's Iron Ingot-Refining Part-Time Job
//--- Description -----------------------------------------------------------
// All quests used by the PTJ, and a script to handle the PTJ via hooks.
//--- Notes -----------------------------------------------------------------
// The following dialogue is missing:
// * 1-star PTJ turn in response
// * 3-star PTJ turn in response
//---------------------------------------------------------------------------

public class ElenPtjScript : GeneralScript
{
	const PtjType JobType = PtjType.BlacksmithShop;

	const int Start = 12;
	const int Report = 13;
	const int Deadline = 19;
	const int PerDay = 8;

	int remaining = PerDay;

	readonly int[] QuestIds = new int[]
	{
		507701, // Basic  Refine 1 Iron Ingot
		507731, // Int    Refine 2 Iron Ingots, Deliver 1
		507761, // Adv    Refine 4 Iron Ingots, Deliver 1
	};

	public override void Load()
	{
		AddHook("_elen", "after_intro", AfterIntro);
		AddHook("_elen", "before_keywords", BeforeKeywords);
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
			npc.Msg(L("Are you doing what you're supposed to be doing?<p/>Don't get lazy now. Make sure you take care of your work."));
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
					npc.Msg(L("Are you done already?<br/>It's not the deadline yet. Please come back later."));
				else
					npc.Msg(L("You'd better be doing the work I asked you to do!<br/>Please finish it before the deadline."));

				return;
			}

			// Report?
			npc.Msg(L("Did you finish today's tasks?<br/>If not, you can report to me later."),
				npc.Button(L("Report Now"), "@report"),
				npc.Button(L("Report Later"), "@later")
				);

			if (await npc.Select() != "@report")
			{
				npc.Msg(L("Alright then, please report later."));
				return;
			}

			// Nothing done
			if (result == QuestResult.None)
			{
				npc.GiveUpPtj();

				npc.Msg(npc.FavorExpression(), L("All you did was waste time<br/>and you got nothing done.<br/>I can't pay you for this."));
				npc.ModifyRelation(0, -Random(3), 0);
			}
			// Low~Perfect result
			else
			{
				npc.Msg(L("Wow. You've done a meticulous job, <username/>.<br/>I've prepared a few things to thank you.<br/>And, thought I might as well give you some choices.<br/>Make sure to pick something you really need."),
					npc.Button(L("Report Later"), "@later"),
					npc.PtjReport(result)
					);
				var reply = await npc.Select();

				// Report later
				if (!reply.StartsWith("@reward:"))
				{
					npc.Msg(npc.FavorExpression(), L("Huh? Do you have an emergency?<br/>Well, what can you do. Please come back later."));
					return;
				}

				// Complete
				npc.CompletePtj(reply);
				remaining--;

				// Result msg
				if (result == QuestResult.Perfect)
				{
					npc.Msg(npc.FavorExpression(), L("Thank you.<br/>Please keep up this good work."));
					npc.ModifyRelation(0, Random(3), 0);
				}
				else if (result == QuestResult.Mid)
				{
					npc.Msg(npc.FavorExpression(), L("(missing): 3 star response"));
					npc.ModifyRelation(0, Random(1), 0);
				}
				else if (result == QuestResult.Low)
				{
					npc.Msg(npc.FavorExpression(), L("(missing): 1 star response"));
					npc.ModifyRelation(0, -Random(2), 0);
				}
			}
			return;
		}

		// Check if PTJ time
		if (!npc.ErinnHour(Start, Deadline))
		{
			npc.Msg(L("I'm busy with my own work right now. Would you come back later?"));
			return;
		}

		// Check if not done today and if there are jobs remaining
		if (!npc.CanDoPtj(JobType, remaining))
		{
			npc.Msg(L("Today's work has been finished.<br/>Please come again tomorrow."));
			return;
		}

		// Offer PTJ
		var randomPtj = npc.RandomPtj(JobType, QuestIds);
		var msg = "";

		if (npc.GetPtjDoneCount(JobType) == 0)
			msg = L("Are you interested in some part-time work at the Bangor Blacksmith's Shop?<br/>If you complete the work before<br/>the deadline, I'll pay you.");
		else
			msg = L("Would you like to see today's work agenda?");

		npc.Msg(msg, npc.PtjDesc(randomPtj,
			L("Elen's Iron Ingot-Refining Part-Time Job"),
			L("Looking for refiners."),
			PerDay, remaining, npc.GetPtjDoneCount(JobType)));

		if (await npc.Select() == "@accept")
		{
			if (npc.GetPtjDoneCount(JobType) == 0)
				npc.Msg(L("Even if you finish the work early,<br/>you can't report until the deadline. Don't forget."));
			else
				npc.Msg(L("Alright. Good luck with your work."));

			npc.StartPtj(randomPtj);
		}
		else
		{
			if (npc.GetPtjDoneCount(JobType) == 0)
				npc.Msg(L("Don't underestimate blacksmith work<br/>or you might come to regret it."));
			else
				npc.Msg(L("I see.<br/>Then I'll assign this task to someone else.."));
		}
	}
}

public class ElenRefineBasicPtjScript : QuestScript
{
	public override void Load()
	{
		SetId(507701);
		SetName(L("Iron Ingot-Refining Part-Time Job"));
		SetDescription(L("This job is to refine iron ore into iron ingots. Today, refine [1 Lump of Iron Ingot] but you don't need to bring the finished ingot. Use the furnace to refine iron ore into iron ingots."));

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.ById);

		SetType(QuestType.Deliver);
		SetPtjType(PtjType.BlacksmithShop);
		SetLevel(QuestLevel.Basic);
		SetHours(start: 12, report: 13, deadline: 19);

		AddObjective("ptj", L("Refine 1 lump of iron ingot."), 0, 0, 0, Create(64001, 1, SkillId.Refining));

		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(220));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(360));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(110));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(180));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(44));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(72));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(115));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(440));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(57));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(220));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(23));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(88));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(565));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(100));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(282));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(50));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(113));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(20));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(45002, 200)); // Bolt
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(150));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(45002, 100)); // Bolt
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(350));

		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(51001, 10)); // HP 10 Potion
		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(50));

		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(51011, 10)); // Stamina 10 Potion
		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(450));
	}
}

public class ElenRefineIntPtjScript : QuestScript
{
	public override void Load()
	{
		SetId(507731);
		SetName(L("Iron Ingot-Refining Part-Time Job"));
		SetDescription(L("This job is to refine iron ore into iron ingots. Today, refine [2 Lumps of Iron Ingots] and bring me one of them. Use the furnace to refine iron ore into iron ingots."));

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.ById);

		SetType(QuestType.Deliver);
		SetPtjType(PtjType.BlacksmithShop);
		SetLevel(QuestLevel.Int);
		SetHours(start: 12, report: 13, deadline: 19);

		AddObjective("ptj1", L("Refine 2 lumps of iron ingots."), 0, 0, 0, Create(64001, 2, SkillId.Refining));
		AddObjective("ptj2", L("Deliver 1 iron ingot."), 0, 0, 0, Collect(64001, 1));

		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(300));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(900));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(150));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(450));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(60));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(180));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(265));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(925));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(132));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(462));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(53));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(185));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(1200));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(225));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(600));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(112));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(240));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(45));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(16008)); // Cores' Thief Gloves
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(250));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(16004)); // Studded Bracelet
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(350));

		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(40025)); // Pickaxe
		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(400));

		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(45002, 200)); // Bolt
		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(750));
	}
}

public class ElenRefineAdvPtjScript : QuestScript
{
	public override void Load()
	{
		SetId(507761);
		SetName(L("Iron Ingot-Refining Part-Time Job"));
		SetDescription(L("This job is to refine iron ore into iron ingots. Today, refine [4 Lumps of Iron Ingots] and bring me one of them. Use the furnace to refine iron ore into iron ingots."));

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.ById);

		SetType(QuestType.Deliver);
		SetPtjType(PtjType.BlacksmithShop);
		SetLevel(QuestLevel.Adv);
		SetHours(start: 12, report: 13, deadline: 19);

		AddObjective("ptj1", L("Refine 4 lumps of iron ingots."), 0, 0, 0, Create(64001, 4, SkillId.Refining));
		AddObjective("ptj2", L("Deliver 1 iron ingot."), 0, 0, 0, Collect(64001, 1));

		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(500));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(1800));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(250));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(900));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(100));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(360));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(550));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(1760));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(275));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(880));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(110));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(352));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(2320));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(435));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(1160));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(217));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(464));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(87));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(16008)); // Cores' Thief Gloves
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(1300));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(16004)); // Studded Bracelet
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(1400));

		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(40024)); // Blacksmith Hammer
		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(200));

		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(17019)); // Blacksmith Shoes
		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(600));

		AddReward(8, RewardGroupType.Item, QuestResult.Perfect, Pattern(64500, 20102, 30)); // Blacksmith Manual - Dagger

		AddReward(9, RewardGroupType.Item, QuestResult.Perfect, QuestScroll(40020)); // Big Order of Iron Ingots
	}
}
