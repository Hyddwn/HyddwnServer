//--- Aura Script -----------------------------------------------------------
// Caitin's Grocery Store Part-Time Job
//--- Description -----------------------------------------------------------
// All quests used by the PTJ, and a script to handle the PTJ via hooks.
//--- Notes -----------------------------------------------------------------
// Apparently the rewards have been increased, as the Wiki now lists
// higher ones than in the past. We'll use the G1 rewards for now.
// Ref.: http://wiki.mabinogiworld.com/index.php?title=Dilys&oldid=143928
//---------------------------------------------------------------------------

public class CaitinPtjScript : GeneralScript
{
	const PtjType JobType = PtjType.GroceryStore;

	const int Start = 12;
	const int Report = 14;
	const int Deadline = 21;
	const int PerDay = 10;

	const int Anthology = 70005;

	int remaining = PerDay;

	readonly int[] QuestIds = new int[]
	{
		501101, // Basic  Gather 5 Berries
		501131, // Int    Gather 7 Berries
		501161, // Adv    Gather 10 Berries
		501103, // Basic  Make a Sack of Wheat Flour
		501133, // Int    Make 2 Sacks of Wheat Flour
		501163, // Adv    Make 4 Sacks of Wheat Flour
		501104, // Basic  Make a Sack of Barley Flour
		501134, // Int    Make 2 Sacks of Barley Flour
		501164, // Adv    Make 4 Sacks of Barley Flour
		501401, // Basic  Bread Delivery to Meven
		501431, // Int    Bread Delivery to Meven
		501461, // Adv    Bread Delivery to Meven
		501402, // Basic  Bread Delivery to Duncan
		501432, // Int    Bread Delivery to Duncan
		501462, // Adv    Bread Delivery to Duncan
		501403, // Basic  Bread Delivery to Dilys
		501433, // Int    Bread Delivery to Dilys
		501463, // Adv    Bread Delivery to Dilys
		501404, // Basic  Bread Delivery to Piaras
		501434, // Int    Bread Delivery to Piaras
		501464, // Adv    Bread Delivery to Piaras
		501405, // Basic  Deliver Bread to Lassar, Talk To Ranald, Deliver Anthology
		501435, // Int    Deliver Bread to Lassar, Talk To Ranald, Deliver Anthology
		501465, // Adv    Deliver Bread to Lassar, Talk To Ranald, Deliver Anthology

		// These existed at some point, but were probably removed,
		// as they don't seem to be available on NA anymore.
		//501107, // Basic  Fish up a Brifne Carp at least 10 cm big
		//501136, // Int    Fish up a Brifne Carp at least 30 cm big
		//501166, // Adv    Fish up a Brifne Carp at least 60 cm big
	};

	public override void Load()
	{
		AddHook("_caitin", "after_intro", AfterIntro);
		AddHook("_caitin", "before_keywords", BeforeKeywords);
	}

	public async Task<HookResult> AfterIntro(NpcScript npc, params object[] args)
	{
		// Handle receiving of Anthology from some of her PTJ quests
		int id;
		if (npc.QuestActive(id = 501405, "ptj3") || npc.QuestActive(id = 501435, "ptj3") || npc.QuestActive(id = 501465, "ptj3"))
		{
			if (!npc.Player.Inventory.Has(Anthology))
				return HookResult.Continue;

			npc.Player.Inventory.Remove(Anthology, 1);
			npc.FinishQuest(id, "ptj3");

			npc.Notice(L("You have given Anthology to be Delivered to Caitin."));
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
	public void OnErinnMidnightTick(ErinnTime time)
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
			npc.Msg(L("Hmm... You seem to be on a different job.<br/>Why don't you finish that first and come back later?"));
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
					npc.Msg(L("Did you finish all the work I requested?<br/>It's a bit early for that now.<br/>Please come back at the deadline."));
				else
					npc.Msg(L("How's it going?<br/>"));
				return;
			}

			// Report?
			npc.Msg(L("Are you ready to show me what you've got?<br/>Or if you haven't finished it yet, you can report later."), npc.Button(L("Report Now"), "@report"), npc.Button(L("Report Later"), "@later"));

			if (await npc.Select() != "@report")
			{
				npc.Msg(L("Okay then, I'll see you later.<br/>You know you have to report back even if you don't finish the job, right?"));
				return;
			}

			// Nothing done
			if (result == QuestResult.None)
			{
				npc.GiveUpPtj();

				npc.Msg(npc.FavorExpression(), L("Are you here to work or what? Why did you even ask for the job in the first place?<br/>Sorry, but I can't pay you anything."));
				npc.ModifyRelation(0, -Random(3), 0);
			}
			// Low~Perfect result
			else
			{
				npc.Msg(L("Wow! What a fine job! Thank you so much.<br/>This is a token of my gratitude.<br/>Take any one of these.<br/>Ahem! Taking more than one would be greedy..."), npc.Button(L("Report Later"), "@later"), npc.PtjReport(result));
				var reply = await npc.Select();

				// Report later
				if (!reply.StartsWith("@reward:"))
				{
					npc.Msg(L("Okay then, I'll see you later.<br/>You know you have to report back even if you don't finish the job, right?"));
					return;
				}

				// Complete
				npc.CompletePtj(reply);
				remaining--;

				// Result msg
				if (result == QuestResult.Perfect)
				{
					npc.Msg(npc.FavorExpression(), L("Oh, thank you so much! You're really good!<br/>Can you help me again tomorrow?"));
					npc.ModifyRelation(0, Random(3), 0);
				}
				else if (result == QuestResult.Mid)
				{
					npc.Msg(npc.FavorExpression(), L("Oh... A bit short... But thanks anyway.<br/>I'll have to pay you a little less, though."));
					npc.ModifyRelation(0, Random(1), 0);
				}
				else if (result == QuestResult.Low)
				{
					npc.Msg(npc.FavorExpression(), L("Huh?! This is less than I asked for! Well...<br/>I can only pay you this much."));
					npc.ModifyRelation(0, -Random(2), 0);
				}
			}
			return;
		}

		// Check if PTJ time
		if (!npc.ErinnHour(Start, Deadline))
		{
			npc.Msg(L("I'm sorry... This isn't the right time for a part-time job.<br/>Please come back later."));
			return;
		}

		// Check if not done today and if there are jobs remaining
		if (!npc.CanDoPtj(JobType, remaining))
		{
			npc.Msg(L("That's it for today's Grocery Store work.<br/>I'll give you a new task when you come tomorrow."));
			return;
		}

		// Offer PTJ
		var randomPtj = npc.RandomPtj(JobType, QuestIds);

		// Msg is kinda unofficial, she currently says the following, and then
		// tells you you'd get Homestead seeds.
		npc.Msg(L("Ah, <username/>! Are you here for part-time work as usual?"), npc.PtjDesc(randomPtj, L("Caitin's Grocery Store Part-Time Job"), L("Looking for help with delivery of goods in Grocery Store."), PerDay, remaining, npc.GetPtjDoneCount(JobType)));

		if (await npc.Select() == "@accept")
		{
			npc.Msg(L("I'm counting on you."));
			npc.StartPtj(randomPtj);
		}
		else
		{
			npc.Msg(L("Do you have something else to do?"));
		}
	}
}

// Berry quests
public class CaitinBerryBasicPtjScript : QuestScript
{
	public override void Load()
	{
		SetId(501101);
		SetName(L("Grocery Store Part-Time Job"));
		SetDescription(L("This task is to gather food ingredients. Today's ingredients are [5 berries]. Hit or shake trees to get berries."));

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.ById);

		SetType(QuestType.Deliver);
		SetPtjType(PtjType.GroceryStore);
		SetLevel(QuestLevel.Basic);
		SetHours(start: 12, report: 14, deadline: 21);

		AddObjective("ptj", L("Collect 5 Berries"), 0, 0, 0, Collect(50007, 5));

		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(200));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(400));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(100));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(200));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(40));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(80));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(120));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(460));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(60));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(240));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(24));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(96));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(580));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(115));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(287));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(60));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(96));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(24));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(50021)); // Milk
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(155));
	}
}

public class CaitinBerryIntPtjScript : QuestScript
{
	public override void Load()
	{
		SetId(501131);
		SetName(L("Grocery Store Part-Time Job"));
		SetDescription(L("This task is to gather food ingredients. Today's ingredients are [7 berries]. Hit or shake trees to get berries."));

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.ById);

		SetType(QuestType.Deliver);
		SetPtjType(PtjType.GroceryStore);
		SetLevel(QuestLevel.Int);
		SetHours(start: 12, report: 14, deadline: 21);

		AddObjective("ptj", L("Collect 7 Berries"), 0, 0, 0, Collect(50007, 7));

		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(300));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(700));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(150));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(350));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(60));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(140));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(220));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(760));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(98));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(390));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(40));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(160));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(980));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(190));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(487));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(98));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(180));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(40));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(40044)); // Ladle
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(76));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(40042)); // Cooking Knife
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(120));
	}
}

public class CaitinBerryAdvPtjScript : QuestScript
{
	public override void Load()
	{
		SetId(501161);
		SetName(L("Grocery Store Part-Time Job"));
		SetDescription(L("This task is to gather food ingredients. Today's ingredients are [10 berries]. Hit or shake trees to get berries."));

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.ById);

		SetType(QuestType.Deliver);
		SetPtjType(PtjType.GroceryStore);
		SetLevel(QuestLevel.Adv);
		SetHours(start: 12, report: 14, deadline: 21);

		AddObjective("ptj", L("Collect 10 Berries"), 0, 0, 0, Collect(50007, 10));

		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(500));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(1300));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(180));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(440));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(1100));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(260));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(420));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(1360));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(127));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(480));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(72));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(288));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(1780));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(340));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(607));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(120));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(351));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(72));
	}
}

// Wheat Flour quests
public class CaitinWheatFlourBasicPtjScript : QuestScript
{
	public override void Load()
	{
		SetId(501103);
		SetName(L("Grocery Store Part-Time Job"));
		SetDescription(L("This task is to gather food ingredients. Today's ingredient is [1 Sack of Wheat Flour]. Grind wheat to make Wheat Flour."));

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.ById);

		SetType(QuestType.Deliver);
		SetPtjType(PtjType.GroceryStore);
		SetLevel(QuestLevel.Basic);
		SetHours(start: 12, report: 14, deadline: 21);

		AddObjective("ptj", L("Deliver 1 Sack of Wheat Flour"), 0, 0, 0, Collect(50022, 1));

		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(300));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(400));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(150));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(200));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(60));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(80));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(140));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(520));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(68));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(270));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(28));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(112));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(660));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(130));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(327));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(68));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(116));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(28));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(50006)); // Sliced Meat
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(25));
	}
}

public class CaitinWheatFlourIntPtjScript : QuestScript
{
	public override void Load()
	{
		SetId(501133);
		SetName(L("Grocery Store Part-Time Job"));
		SetDescription(L("This task is to gather food ingredients. Today's ingredient is [2 Sacks of Wheat Flour]. Grind wheat to make Wheat Flour."));

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.ById);

		SetType(QuestType.Deliver);
		SetPtjType(PtjType.GroceryStore);
		SetLevel(QuestLevel.Int);
		SetHours(start: 12, report: 14, deadline: 21);

		AddObjective("ptj", L("Deliver 2 Sacks of Wheat Flour"), 0, 0, 0, Collect(50022, 2));

		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(500));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(700));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(250));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(350));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(100));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(140));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(260));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(880));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(117));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(450));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(48));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(192));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(1140));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(220));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(566));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(112));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(223));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(48));
	}
}

public class CaitinWheatFlourAdvPtjScript : QuestScript
{
	public override void Load()
	{
		SetId(501163);
		SetName(L("Grocery Store Part-Time Job"));
		SetDescription(L("This task is to gather food ingredients. Today's ingredient is [4 Sacks of Wheat Flour]. Grind wheat to make Wheat Flour."));

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.ById);

		SetType(QuestType.Deliver);
		SetPtjType(PtjType.GroceryStore);
		SetLevel(QuestLevel.Adv);
		SetHours(start: 12, report: 14, deadline: 21);

		AddObjective("ptj", L("Deliver 4 Sacks of Wheat Flour"), 0, 0, 0, Collect(50022, 4));

		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(900));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(1200));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(310));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(440));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(180));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(240));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(473));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(1520));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(153));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(558));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(80));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(320));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(1993));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(380));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(711));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(140));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(393));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(80));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(46005)); // Cooking Table
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(79));
	}
}

// Barley Flour quests
public class CaitinBarleyFlourBasicPtjScript : QuestScript
{
	public override void Load()
	{
		SetId(501104);
		SetName(L("Grocery Store Part-Time Job"));
		SetDescription(L("This task is to gather food ingredients. Today's ingredient is [1 Sack of Barley Flour]. Grind barley to make Barley Flour."));

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.ById);

		SetType(QuestType.Deliver);
		SetPtjType(PtjType.GroceryStore);
		SetLevel(QuestLevel.Basic);
		SetHours(start: 12, report: 14, deadline: 21);

		AddObjective("ptj", L("Deliver 1 Sack of Barley Flour"), 0, 0, 0, Collect(50023, 1));

		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(300));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(400));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(150));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(200));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(60));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(80));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(140));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(520));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(68));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(270));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(28));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(112));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(660));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(130));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(327));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(68));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(116));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(28));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(50006)); // Sliced Meat
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(25));
	}
}

public class CaitinBarleyFlourIntPtjScript : QuestScript
{
	public override void Load()
	{
		SetId(501134);
		SetName(L("Grocery Store Part-Time Job"));
		SetDescription(L("This task is to gather food ingredients. Today's ingredient is [2 Sacks of Barley Flour]. Grind barley to make Barley Flour."));

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.ById);

		SetType(QuestType.Deliver);
		SetPtjType(PtjType.GroceryStore);
		SetLevel(QuestLevel.Int);
		SetHours(start: 12, report: 14, deadline: 21);

		AddObjective("ptj", L("Deliver 2 Sacks of Barley Flour"), 0, 0, 0, Collect(50023, 2));

		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(500));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(700));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(250));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(350));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(100));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(140));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(260));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(880));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(117));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(450));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(48));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(192));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(1140));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(220));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(566));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(112));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(223));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(48));
	}
}

public class CaitinBarleyFlourAdvPtjScript : QuestScript
{
	public override void Load()
	{
		SetId(501164);
		SetName(L("Grocery Store Part-Time Job"));
		SetDescription(L("This task is to gather food ingredients. Today's ingredient is [4 Sacks of Barley Flour]. Grind barley to make Barley Flour."));

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.ById);

		SetType(QuestType.Deliver);
		SetPtjType(PtjType.GroceryStore);
		SetLevel(QuestLevel.Adv);
		SetHours(start: 12, report: 14, deadline: 21);

		AddObjective("ptj", L("Deliver 4 Sacks of Barley Flour"), 0, 0, 0, Collect(50023, 4));

		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(900));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(1200));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(310));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(440));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(180));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(240));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(473));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(1520));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(153));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(558));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(80));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(320));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(1993));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(380));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(711));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(140));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(393));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(80));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(46005)); // Cooking Table
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(79));
	}
}

// Bread Delivery quests
public abstract class CaitinBreadPtjBaseScript : QuestScript
{
	protected const int ItemId = 70027; // Bread

	protected abstract int QuestId { get; }
	protected abstract string NpcIdent { get; }
	protected abstract string Objective { get; }

	public override void Load()
	{
		SetId(QuestId);
		SetName(L("Grocery Store Part-Time Job"));
		SetDescription(L("I need to [deliver fresh baked bread], but I'm busy. Can you deliver the bread for me? - Caitin -"));

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.ById);

		SetType(QuestType.Deliver);
		SetPtjType(PtjType.GroceryStore);
		SetHours(start: 12, report: 14, deadline: 21);

		AddObjective("ptj", Objective, 0, 0, 0, Deliver(ItemId, NpcIdent));
		AddHook(NpcIdent, "after_intro", AfterIntro);
	}

	public async Task<HookResult> AfterIntro(NpcScript npc, params object[] args)
	{
		if (!npc.QuestActive(this.Id, "ptj"))
			return HookResult.Continue;

		if (!npc.Player.Inventory.Has(ItemId))
			return HookResult.Continue;

		npc.Player.Inventory.Remove(ItemId, 1);
		npc.FinishQuest(this.Id, "ptj");

		await this.OnFinish(npc);

		return HookResult.Break;
	}

	protected virtual async Task OnFinish(NpcScript npc)
	{
		await Task.Yield();
	}
}

// Bread Delivery Basic
public abstract class CaitinBreadBasicPtjBaseScript : CaitinBreadPtjBaseScript
{
	public override void Load()
	{
		SetLevel(QuestLevel.Basic);

		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(200));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(200));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(100));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(100));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(40));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(40));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(75));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(300));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(40));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(160));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(16));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(64));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(367));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(75));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(180));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(40));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(64));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(16));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(40027)); // Weeding Hoe
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(55));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(51001, 7)); // HP 10 Potion
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(25));

		base.Load();
	}
}

public class CaitinBreadMevenBasicPtjScript : CaitinBreadBasicPtjBaseScript
{
	protected override int QuestId { get { return 501401; } }
	protected override string NpcIdent { get { return "_meven"; } }
	protected override string Objective { get { return L("Deliver Bread to Meven at Church"); } }

	protected override async Task OnFinish(NpcScript npc)
	{
		npc.Msg(L("Oh, this Bread looks lovely.<br/>Another blessing from our God, Lymilark.<br/>Thank you."));
		npc.Msg(Hide.None, L("(Delivered the Bread to the Priest.)"));
	}
}

public class CaitinBreadDuncanBasicPtjScript : CaitinBreadBasicPtjBaseScript
{
	protected override int QuestId { get { return 501402; } }
	protected override string NpcIdent { get { return "_duncan"; } }
	protected override string Objective { get { return L("Deliver Bread to Duncan at the Chief's House"); } }

	protected override async Task OnFinish(NpcScript npc)
	{
		npc.Msg(L("Ah, so you're delivering bread now?<br/>Keep up the good work. Thank you."));
		npc.Msg(Hide.Name, L("(Delivered the Bread to the Chief.)"));
	}
}

public class CaitinBreadDilysBasicPtjScript : CaitinBreadBasicPtjBaseScript
{
	protected override int QuestId { get { return 501403; } }
	protected override string NpcIdent { get { return "_dilys"; } }
	protected override string Objective { get { return L("Deliver Bread to Healer Dilys"); } }

	protected override async Task OnFinish(NpcScript npc)
	{
		npc.Msg(L("Is this bread from Caitin?<br/>Thank you."));
		npc.Msg(Hide.Name, L("(Delivered the Bread to Dilys.)"));
	}
}

public class CaitinBreadPiarasBasicPtjScript : CaitinBreadBasicPtjBaseScript
{
	protected override int QuestId { get { return 501404; } }
	protected override string NpcIdent { get { return "_piaras"; } }
	protected override string Objective { get { return L("Deliver Bread to Piaras at the Inn"); } }

	protected override async Task OnFinish(NpcScript npc)
	{
		npc.Msg(L("You are delivering the bread on Caitin's behalf?<br/>I can't thank you enough."));
		npc.Msg(Hide.Name, L("(Delivered the Bread to Piaras.)"));
	}
}

// Bread Delivery Int
public abstract class CaitinBreadIntPtjBaseScript : CaitinBreadPtjBaseScript
{
	public override void Load()
	{
		SetLevel(QuestLevel.Int);

		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(300));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(300));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(150));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(150));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(60));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(60));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(113));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(440));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(58));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(230));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(24));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(96));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(553));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(110));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(274));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(58));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(96));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(24));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(50021)); // Milk
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(130));

		base.Load();
	}
}

public class CaitinBreadMevenIntPtjScript : CaitinBreadIntPtjBaseScript
{
	protected override int QuestId { get { return 501431; } }
	protected override string NpcIdent { get { return "_meven"; } }
	protected override string Objective { get { return L("Deliver Bread to Meven at Church"); } }

	protected override async Task OnFinish(NpcScript npc)
	{
		npc.Msg(L("Oh, this Bread looks lovely.<br/>Another blessing from our God, Lymilark.<br/>Thank you."));
		npc.Msg(Hide.None, L("(Delivered the Bread to the Priest.)"));
	}
}

public class CaitinBreadDuncanIntPtjScript : CaitinBreadIntPtjBaseScript
{
	protected override int QuestId { get { return 501432; } }
	protected override string NpcIdent { get { return "_duncan"; } }
	protected override string Objective { get { return L("Deliver Bread to Duncan at the Chief's House"); } }

	protected override async Task OnFinish(NpcScript npc)
	{
		npc.Msg(L("Ah, so you're delivering bread now?<br/>Keep up the good work. Thank you."));
		npc.Msg(Hide.Name, L("(Delivered the Bread to the Chief.)"));
	}
}

public class CaitinBreadDilysIntPtjScript : CaitinBreadIntPtjBaseScript
{
	protected override int QuestId { get { return 501433; } }
	protected override string NpcIdent { get { return "_dilys"; } }
	protected override string Objective { get { return L("Deliver Bread to Healer Dilys"); } }

	protected override async Task OnFinish(NpcScript npc)
	{
		npc.Msg(L("Is this bread from Caitin?<br/>Thank you."));
		npc.Msg(Hide.Name, L("(Delivered the Bread to Dilys.)"));
	}
}

public class CaitinBreadPiarasIntPtjScript : CaitinBreadIntPtjBaseScript
{
	protected override int QuestId { get { return 501434; } }
	protected override string NpcIdent { get { return "_piaras"; } }
	protected override string Objective { get { return L("Deliver Bread to Piaras at the Inn"); } }

	protected override async Task OnFinish(NpcScript npc)
	{
		npc.Msg(L("You are delivering the bread on Caitin's behalf?<br/>I can't thank you enough."));
		npc.Msg(Hide.Name, L("(Delivered the Bread to Piaras.)"));
	}
}

// Bread Delivery Adv
public abstract class CaitinBreadAdvPtjBaseScript : CaitinBreadPtjBaseScript
{
	public override void Load()
	{
		SetLevel(QuestLevel.Adv);

		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(400));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(500));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(200));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(250));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(80));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(100));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(187));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(660));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(85));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(340));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(36));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(144));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(847));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(165));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(420));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(85));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(159));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(36));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(50006)); // Sliced Meat
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(200));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(63016)); // Holy Water
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Exp(215));

		base.Load();
	}
}

public class CaitinBreadMevenAdvPtjScript : CaitinBreadAdvPtjBaseScript
{
	protected override int QuestId { get { return 501461; } }
	protected override string NpcIdent { get { return "_meven"; } }
	protected override string Objective { get { return L("Deliver Bread to Meven at Church"); } }

	protected override async Task OnFinish(NpcScript npc)
	{
		npc.Msg(L("Oh, this Bread looks lovely.<br/>Another blessing from our God, Lymilark.<br/>Thank you."));
		npc.Msg(Hide.None, L("(Delivered the Bread to the Priest.)"));
	}
}

public class CaitinBreadDuncanAdvPtjScript : CaitinBreadAdvPtjBaseScript
{
	protected override int QuestId { get { return 501462; } }
	protected override string NpcIdent { get { return "_duncan"; } }
	protected override string Objective { get { return L("Deliver Bread to Duncan at the Chief's House"); } }

	protected override async Task OnFinish(NpcScript npc)
	{
		npc.Msg(L("Ah, so you're delivering bread now?<br/>Keep up the good work. Thank you."));
		npc.Msg(Hide.Name, L("(Delivered the Bread to the Chief.)"));
	}
}

public class CaitinBreadDilysAdvPtjScript : CaitinBreadAdvPtjBaseScript
{
	protected override int QuestId { get { return 501463; } }
	protected override string NpcIdent { get { return "_dilys"; } }
	protected override string Objective { get { return L("Deliver Bread to Healer Dilys"); } }

	protected override async Task OnFinish(NpcScript npc)
	{
		npc.Msg(L("Is this bread from Caitin?<br/>Thank you."));
		npc.Msg(Hide.Name, L("(Delivered the Bread to Dilys.)"));
	}
}

public class CaitinBreadPiarasAdvPtjScript : CaitinBreadAdvPtjBaseScript
{
	protected override int QuestId { get { return 501464; } }
	protected override string NpcIdent { get { return "_piaras"; } }
	protected override string Objective { get { return L("Deliver Bread to Piaras at the Inn"); } }

	protected override async Task OnFinish(NpcScript npc)
	{
		npc.Msg(L("You are delivering the bread on Caitin's behalf?<br/>I can't thank you enough."));
		npc.Msg(Hide.Name, L("(Delivered the Bread to Piaras.)"));
	}
}

// Extended bread delivery quests
public abstract class CaitinExtBreadPtjBaseScript : QuestScript
{
	protected const int Bread = 70027;
	protected const int Anthology = 70005;

	public override void Load()
	{
		SetName(L("Grocery Store Part-Time Job"));
		SetDescription(L("I need to [deliver fresh baked bread], but I'm busy. Can you deliver the bread for me? - Caitin -"));

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.ById);

		SetType(QuestType.Deliver);
		SetPtjType(PtjType.GroceryStore);
		SetHours(start: 12, report: 14, deadline: 21);

		AddObjective("ptj1", L("Deliver Bread to Lassar at the School"), 0, 0, 0, Deliver(Bread, "_lassar"));
		AddObjective("ptj2", L("Meet Ranald at the School."), 0, 0, 0, Talk("_ranald"));
		AddObjective("ptj3", L("Deliver Anthology to Caitin at the Grocery Store"), 0, 0, 0, Deliver(Anthology, "_caitin"));

		AddHook("_lassar", "after_intro", LassarAfterIntro);
		AddHook("_ranald", "after_intro", RanaldAfterIntro);
		// Caitin is handled above, as we have to deliver the item to her
		// before she asks us about the PTJ.
	}

	public async Task<HookResult> LassarAfterIntro(NpcScript npc, params object[] args)
	{
		if (!npc.QuestActive(this.Id, "ptj1"))
			return HookResult.Continue;

		if (!npc.Player.Inventory.Has(Bread))
			return HookResult.Continue;

		npc.Player.Inventory.Remove(Bread, 1);
		npc.FinishQuest(this.Id, "ptj1");

		npc.Msg(L("Oh, so Caitin asked you to deliver this Bread to me?<br/>Thanks."));
		npc.Msg(Hide.Name, L("(Delivered the Bread to Lassar.)"));
		npc.Msg(L("Ah, by the way, teacher Ranald said<br/>he had something to give to Caitin...<br/>Can you go talk to him?"));

		return HookResult.Break;
	}

	public async Task<HookResult> RanaldAfterIntro(NpcScript npc, params object[] args)
	{
		if (!npc.QuestActive(this.Id, "ptj2"))
			return HookResult.Continue;

		npc.FinishQuest(this.Id, "ptj2");
		npc.GiveItem(Anthology);

		npc.Msg(L("You are working for Caitin now?<br/>Then, can you do me a favor? Please give this to her for me."));
		npc.Msg(Hide.Name, L("(Received an Anthology.)"));
		npc.Msg(L("I borrowed this from her,<br/>but don't have time to return it right now."));

		return HookResult.Break;
	}
}

public class CaitinExtBreadBasicPtjScript : CaitinExtBreadPtjBaseScript
{
	public override void Load()
	{
		SetId(501405);
		SetLevel(QuestLevel.Basic);

		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(200));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(200));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(100));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(100));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(40));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(40));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(75));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(300));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(40));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(160));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(16));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(64));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(367));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(75));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(180));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(40));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(64));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(16));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(40027)); // Weeding Hoe
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(55));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(51001, 7)); // HP 10 Potion
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(25));

		base.Load();
	}
}

public class CaitinExtBreadIntPtjScript : CaitinExtBreadPtjBaseScript
{
	public override void Load()
	{
		SetId(501435);
		SetLevel(QuestLevel.Int);

		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(300));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(300));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(150));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(150));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(60));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(60));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(113));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(440));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(58));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(230));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(24));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(96));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(553));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(110));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(274));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(58));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(96));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(24));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(50021)); // Milk
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(130));

		base.Load();
	}
}

public class CaitinExtBreadAdvPtjScript : CaitinExtBreadPtjBaseScript
{
	public override void Load()
	{
		SetId(501465);
		SetLevel(QuestLevel.Adv);

		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(400));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(500));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(200));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(250));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(80));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(100));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(187));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(660));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(85));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(340));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(36));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(114));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(847));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(165));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(420));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(85));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(159));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(36));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(50006)); // Sliced Meat
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(200));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(63016)); // Holy Water
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Exp(224));

		base.Load();
	}
}
