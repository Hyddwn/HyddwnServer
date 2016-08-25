//--- Aura Script -----------------------------------------------------------
// Glenis's Grocery Store Part-Time Job
//--- Description -----------------------------------------------------------
// All quests used by the PTJ, and a script to handle the PTJ via hooks.
//--- Notes -----------------------------------------------------------------
// This script depends on ./grocery_caitin.cs for berry collection PTJs.
// Please ensure this script loads afterward.
//
// Apparently some rewards have been increased, as the Wiki now lists
// higher ones than in the past. We'll use the G1 rewards for now.
// http://wiki.mabinogiworld.com/index.php?title=Glenis&oldid=272001#Part-time_Jobs
// In addition, homestead seeds are not rewarded nor are they mentioned for now.
//
// Definitions for the following base scripts have been improvised,
// update with official script whenever possible:
// * GlenisExtDeliveryAeiraStewartPtjBaseScript
// * GlenisExtDeliveryAranwenStewartPtjBaseScript
//---------------------------------------------------------------------------

public class GlenisPtjScript : GeneralScript
{
	const PtjType JobType = PtjType.GroceryStore;

	const int Start = 12;
	const int Report = 14;
	const int Deadline = 21;
	const int PerDay = 10;

	int remaining = PerDay;

	readonly int[] QuestIds = new int[]
	{
		501101, // Basic  Gather  5 Berries
		501131, // Int    Gather  7 Berries
		501161, // Adv    Gather 10 Berries
		501102, // Basic  Gather  5 Milk Bottles
		501132, // Int    Gather  7 Milk Bottles
		501162, // Adv    Gather 10 Milk Bottles

		501406, // Basic  Item delivery (Aeira)
		501436, // Int    Item delivery (Aeira)
		501466, // Adv    Item delivery (Aeira)
		501407, // Basic  Item delivery (Nerys)
		501437, // Int    Item delivery (Nerys)
		501467, // Adv    Item delivery (Nerys)
		501408, // Basic  Item delivery (Manus)
		501438, // Int    Item delivery (Manus)
		501468, // Adv    Item delivery (Manus)
		501439, // Int    Item delivery (Aeira -> Stewart)
		501469, // Adv    Item delivery (Aeira -> Stewart)
		501440, // Int    Item delivery (Aranwen -> Stewart)
		501470, // Adv    Item delivery (Aranwen -> Stewart)
	};

	public override void Load()
	{
		AddHook("_glenis", "after_intro", AfterIntro);
		AddHook("_glenis", "before_keywords", BeforeKeywords);
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
			npc.Msg(L("Well, you seem to be involved in another part-time job right now.<br/>Is that right?"));
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
					npc.Msg(L("My, someone's in a hurry.<br/>It's not the deadline yet, so why don't you come back later?<p/>Now, I'll see you back here by the deadline."));
				else
					npc.Msg(L("How's the work going?"));
				return;
			}

			// Report?
			npc.Msg(L("Do you want to call it a day?<br/>You can report later if you're not finished yet."),
				npc.Button(L("Report Now"), "@report"),
				npc.Button(L("Report Later"), "@later")
				);

			if (await npc.Select() != "@report")
			{
				npc.Msg(L("Still have some work left to do, do you?<br/>Either way, please make sure to report by the deadline."));
				return;
			}

			// Nothing done
			if (result == QuestResult.None)
			{
				npc.GiveUpPtj();

				npc.Msg(npc.FavorExpression(), L("You know what? It's ok.<br/>If you didn't do your work, I won't need to pay you. Simple as that."));
				npc.ModifyRelation(0, -Random(3), 0);
			}
			// Low~Perfect result
			else
			{
				npc.Msg(L("Mmm? I didn't know you would do such a good job.<br/>You are a very meticulous worker, <username/>.<br/>I know this doesn't do justice for the excellent work you've done, but<br/>I've prepared a few things as a token of my gratitude. Take your pick."),
					npc.Button(L("Report Later"), "@later"),
					npc.PtjReport(result)
					);
				var reply = await npc.Select();

				// Report later
				if (!reply.StartsWith("@reward:"))
				{
					npc.Msg(npc.FavorExpression(), L("Hahaha. It must be a difficult choice."));
					return;
				}

				// Complete
				npc.CompletePtj(reply);
				remaining--;

				// Result msg
				if (result == QuestResult.Perfect)
				{
					npc.Msg(npc.FavorExpression(), L("Great! 10 out of 10!<br/>Please keep up the good work next time, too."));
					npc.ModifyRelation(0, Random(3), 0);
				}
				else if (result == QuestResult.Mid)
				{
					npc.Msg(npc.FavorExpression(), L("I'd say... 6 out of 10.<br/>Your pay will only be that much, too."));
					npc.ModifyRelation(0, Random(1), 0);
				}
				else if (result == QuestResult.Low)
				{
					npc.Msg(npc.FavorExpression(), L("Did you put in any effort at all?<br/>My, my!"));
					npc.ModifyRelation(0, -Random(2), 0);
				}
			}
			return;
		}

		// Check if PTJ time
		if (!npc.ErinnHour(Start, Deadline))
		{
			npc.Msg(L("Are you here for work?<br/>Sorry, but it's not business hours yet."));
			return;
		}

		// Check if not done today and if there are jobs remaining
		if (!npc.CanDoPtj(JobType, remaining))
		{
			npc.Msg(L("Didn't you just work here?<br/>You have to take care of yourself. Don't overwork yourself."));
			return;
		}

		// Offer PTJ
		var randomPtj = npc.RandomPtj(JobType, QuestIds);
		var msg = "";

		if (npc.GetPtjDoneCount(JobType) == 0)
			msg = L("Are you looking for work?<br/>You don't seem to have any experience in this line of work. Are you sure you can handle it?<br/>Well, why don't you get started on this now?<br/>Any type of work is difficult at first, but you get used to it as you gain more experience, you see.<p/>Well, how about it?");
		else
			msg = L("Are you looking for work?");//<br/>You can get a seed for your Homestead.");

		npc.Msg(msg, npc.PtjDesc(randomPtj,
			L("Glenis's Grocery Store Part-Time Job"),
			L("Looking for help with delivering goods to Grocery Store."),
			PerDay, remaining, npc.GetPtjDoneCount(JobType)));

		if (await npc.Select() == "@accept")
		{
			if (npc.GetPtjDoneCount(JobType) == 0)
				npc.Msg(L("Yes, yes. Good idea.<br/>Now, I'll see you by the deadline.<br/>Even if you don't finish everything, at least come file a report."));
			else
				npc.Msg(L("I'll be waiting for you."));

			npc.StartPtj(randomPtj);
		}
		else
		{
			if (npc.GetPtjDoneCount(JobType) == 0)
				npc.Msg(L("Ha ha. You don't have to get so intimidated.<br/>All right. Come by next time."));
			else
				npc.Msg(L("If today's not a good day, I'm sure we'll do this some other time."));
		}
	}
}

// The following quest IDs are already defined in ./grocery_caitin.cs:
/* 501101, // Basic  Gather  5 Berries
 * 501131, // Int    Gather  7 Berries
 * 501161, // Adv    Gather 10 Berries
 */

public abstract class GlenisMilkPtjBaseScript : QuestScript
{
	protected abstract int QuestId { get; }
	protected abstract int ItemCount { get; }
	protected abstract QuestLevel QuestLevel { get; }

	protected abstract void AddRewards();

	public override void OnReceive(Creature creature)
	{
		creature.Inventory.InsertStacks(63022, ItemCount);
	}

	public override void Load()
	{
		SetId(QuestId);
		SetName(L("Grocery Store Part-Time Job"));
		SetDescription(string.Format(LN(
			"This job is to gather food ingredients. Today's ingredient is [{0} Milk Bottle].  Use the empty bottles to collect milk from the Cows.",
			"This job is to gather food ingredients. Today's ingredient is [{0} Milk Bottles].  Use the empty bottles to collect milk from the Cows.",
			ItemCount), ItemCount));

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.ById);

		SetType(QuestType.Deliver);
		SetPtjType(PtjType.GroceryStore);
		SetLevel(QuestLevel);
		SetHours(start: 12, report: 14, deadline: 21);

		AddObjective("ptj", string.Format(LN("Gather {0} Milk Bottle", "Gather {0} Milk Bottles", ItemCount), ItemCount), 0, 0, 0, Collect(63023, ItemCount));

		AddRewards();
	}
}

public class GlenisMilkBasicPtjScript : GlenisMilkPtjBaseScript
{
	protected override int QuestId { get { return 501102; } }
	protected override int ItemCount { get { return 5; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Basic; } }

	protected override void AddRewards()
	{
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(200));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(520));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(100));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(260));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(40));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(104));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(155));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(555));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(77));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(277));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(31));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(111));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(705));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(140));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(352));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(70));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(141));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(28));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(50006)); // Slice of Meat
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(70));
	}
}

public class GlenisMilkIntPtjScript : GlenisMilkPtjBaseScript
{
	protected override int QuestId { get { return 501132; } }
	protected override int ItemCount { get { return 7; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Int; } }

	protected override void AddRewards()
	{
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(300));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(800));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(150));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(400));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(60));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(160));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(250));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(840));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(125));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(420));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(50));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(168));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(1090));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(210));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(545));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(105));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(218));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(42));
	}
}

public class GlenisMilkAdvPtjScript : GlenisMilkPtjBaseScript
{
	protected override int QuestId { get { return 501162; } }
	protected override int ItemCount { get { return 10; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Adv; } }

	protected override void AddRewards()
	{
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(500));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(1100));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(250));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(550));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(100));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(220));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(365));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(1200));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(182));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(600));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(73));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(240));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(1565));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(300));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(782));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(150));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(313));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(60));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(46005)); // Cooking Table

		AddReward(5, RewardGroupType.Scroll, QuestResult.Perfect, QuestScroll(40004)); // Collecting Quest - Big Order of Milk
	}
}

public abstract class GlenisDeliveryPtjBaseScript : QuestScript
{
	/// <summary>
	/// This property determines which reward set to use.
	/// <para>Default is false. Override to specify true.</para>
	/// </summary>
	protected virtual bool IsExtended { get { return false; } }

	protected abstract int QuestId { get; }
	protected abstract string LQuestDescription { get; }
	protected abstract QuestLevel QuestLevel { get; }
	protected abstract string LObjectiveDescription { get; }
	protected abstract int ItemId { get; }
	protected abstract string LItemNotice { get; }
	protected abstract string NpcIdent { get; }

	protected abstract void AfterIntroDialogue(NpcScript npc);

	public override void Load()
	{
		SetId(QuestId);
		SetName(L("Grocery Store Part-Time Job"));
		SetDescription(LQuestDescription);

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.ById);

		SetType(QuestType.Deliver);
		SetPtjType(PtjType.GroceryStore);
		SetLevel(QuestLevel);
		SetHours(start: 12, report: 14, deadline: 21);

		AddObjective("ptj1", LObjectiveDescription, 0, 0, 0, Deliver(ItemId, NpcIdent));

		AddHook(NpcIdent, "after_intro", ClientAfterIntro);

		AddRewards(QuestLevel);
	}

	public virtual async Task<HookResult> ClientAfterIntro(NpcScript npc, params object[] args)
	{
		if (!npc.QuestActive(this.Id, "ptj1"))
			return HookResult.Continue;

		npc.FinishQuest(this.Id, "ptj1");

		npc.Player.RemoveItem(ItemId, 1);
		npc.Notice(LItemNotice);

		AfterIntroDialogue(npc);

		return HookResult.Break;
	}

	/// <remarks>Rewards are common among all non/extended item delivery PTJs, separately.</remarks>
	private void AddRewards(QuestLevel questLevel)
	{
		if (!IsExtended)
		{
			switch (questLevel)
			{
				case QuestLevel.Basic:
					AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(200));
					AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(150));
					AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(100));
					AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(75));
					AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(40));
					AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(30));

					AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(65));
					AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(260));
					AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(32));
					AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(130));
					AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(13));
					AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(52));

					AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(315));
					AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(65));
					AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(157));
					AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(32));
					AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(63));
					AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(13));

					AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(40027)); // Weeding Hoe

					AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(51001, 6)); // HP 10 Potion
					AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(25));
					return;

				case QuestLevel.Int:
					AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(300));
					AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(250));
					AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(150));
					AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(125));
					AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(60));
					AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(50));

					AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(100));
					AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(400));
					AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(50));
					AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(200));
					AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(20));
					AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(80));

					AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(500));
					AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(100));
					AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(250));
					AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(50));
					AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(100));
					AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(20));

					AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(50021)); // Milk
					AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(80));

					AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(40027)); // Weeding Hoe
					AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(180));
					return;

				case QuestLevel.Adv:
					AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(400));
					AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(320));
					AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(200));
					AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(160));
					AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(80));
					AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(64));

					AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(140));
					AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(515));
					AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(70));
					AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(257));
					AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(28));
					AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(103));

					AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(655));
					AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(130));
					AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(327));
					AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(65));
					AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(131));
					AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(26));

					AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(50006, 1)); // Slice of Meat
					AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(20));

					AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(63016, 1)); // Holy Water of Lymilark
					return;
			}
		}
		else
		{
			switch (questLevel)
			{
				case QuestLevel.Basic: // Copied from basic non-extended reward in the event of fallback.
					AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(200));
					AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(150));
					AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(100));
					AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(75));
					AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(40));
					AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(30));

					AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(65));
					AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(260));
					AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(32));
					AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(130));
					AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(13));
					AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(52));

					AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(315));
					AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(65));
					AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(157));
					AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(32));
					AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(63));
					AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(13));

					AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(40027)); // Weeding Hoe

					AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(51001, 6)); // HP 10 Potion
					AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(25));
					return;

				case QuestLevel.Int:
					AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(360));
					AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(260));
					AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(180));
					AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(130));
					AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(72));
					AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(52));

					AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(110));
					AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(450));
					AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(55));
					AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(225));
					AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(22));
					AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(90));

					AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(575));
					AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(100));
					AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(287));
					AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(50));
					AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(115));
					AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(20));

					AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(50021)); // Milk
					AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(135));
					return;

				case QuestLevel.Adv:
					AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(490));
					AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(390));
					AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(245));
					AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(195));
					AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(98));
					AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(78));

					AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(175));
					AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(625));
					AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(87));
					AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(312));
					AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(35));
					AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(125));

					AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(800));
					AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(155));
					AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(400));
					AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(77));
					AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(160));
					AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(31));

					AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(50006, 1)); // Slice of Meat
					AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(157));

					AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(63016, 1)); // Holy Water of Lymilark
					AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(154));
					return;
			}
		}

		Log.Warning(
			"GlenisDeliveryPtjBaseScript: Unspecified quest level for derived class {0}. Fell back to {1} basic rewards.",
			this.GetType(),
			IsExtended ? "extended" : "non-extended"
			);
		AddRewards(QuestLevel.Basic); // Repeat method.
	}
}

public abstract class GlenisDeliveryAeiraPtjBaseScript : GlenisDeliveryPtjBaseScript
{
	protected override string LQuestDescription { get { return L("I need to deliver a [Piece of Cake] to Aeira at the bookstore, but I'm really busy right now. Could you deliver this for me? - Glenis -"); } }
	protected override string LObjectiveDescription { get { return L("Deliver a [Piece of Cake] to Aeira."); } }
	protected override int ItemId { get { return 70018; } }
	protected override string LItemNotice { get { return L("You have given Piece of Cake to be Delivered to Aeira."); } }
	protected override string NpcIdent { get { return "_aeira"; } }

	protected override void AfterIntroDialogue(NpcScript npc)
	{
		npc.Msg(L("This piece of cake looks delicious!<br/>Everybody knows how good Glenis' cake is.<br/>Thank you very much for the cake!"));
		npc.Msg(Hide.Name, L("(Delivered a Piece of Cake to Aeira.)"));
	}
}

public class GlenisDeliveryAeiraBasicPtjScript : GlenisDeliveryAeiraPtjBaseScript
{
	protected override int QuestId { get { return 501406; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Basic; } }
}

public class GlenisDeliveryAeiraIntPtjScript : GlenisDeliveryAeiraPtjBaseScript
{
	protected override int QuestId { get { return 501436; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Int; } }
}

public class GlenisDeliveryAeiraAdvPtjScript : GlenisDeliveryAeiraPtjBaseScript
{
	protected override int QuestId { get { return 501466; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Adv; } }
}

public abstract class GlenisDeliveryNerysPtjBaseScript : GlenisDeliveryPtjBaseScript
{
	protected override string LQuestDescription { get { return L("I need to deliver a [Lump of Cheese] to Nerys at the Weapons Shop, but I'm a little busy right now. Could you deliver this for me? - Glenis -"); } }
	protected override string LObjectiveDescription { get { return L("Deliver a [Lump of Cheese] to Nerys."); } }
	protected override int ItemId { get { return 70034; } }
	protected override string LItemNotice { get { return L("You have given Lump of Cheese to be Delivered to Nerys."); } }
	protected override string NpcIdent { get { return "_nerys"; } }

	protected override void AfterIntroDialogue(NpcScript npc)
	{
		npc.Msg(L("It's from Glenis, right?<br/>Thanks for the delivery."));
		npc.Msg(Hide.Name, L("(Delivered a Lump of Cheese to Nerys.)"));
	}
}

public class GlenisDeliveryNerysBasicPtjScript : GlenisDeliveryNerysPtjBaseScript
{
	protected override int QuestId { get { return 501407; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Basic; } }
}

public class GlenisDeliveryNerysIntPtjScript : GlenisDeliveryNerysPtjBaseScript
{
	protected override int QuestId { get { return 501437; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Int; } }
}

public class GlenisDeliveryNerysAdvPtjScript : GlenisDeliveryAeiraPtjBaseScript
{
	protected override int QuestId { get { return 501467; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Adv; } }
}

public abstract class GlenisDeliveryManusPtjBaseScript : GlenisDeliveryPtjBaseScript
{
	protected override string LQuestDescription { get { return L("I need to deliver a [Chunk of Meat] to Manus at the Healer's House, but I'm too busy right now. Could you deliver this for me? - Glenis -"); } }
	protected override string LObjectiveDescription { get { return L("Deliver a [Chunk of Meat] to Manus."); } }
	protected override int ItemId { get { return 70035; } }
	protected override string LItemNotice { get { return L("You have given Chunk of Meat to be Delivered to Manus."); } }
	protected override string NpcIdent { get { return "_manus"; } }

	protected override void AfterIntroDialogue(NpcScript npc)
	{
		npc.Msg(L("Yes! Finally, it's here!<br/>I've been starving!<br/>Thanks!!"));
		npc.Msg(Hide.Name, L("(Delivered a Chunk of Meat to Manus.)"));
	}
}

public class GlenisDeliveryManusBasicPtjScript : GlenisDeliveryManusPtjBaseScript
{
	protected override int QuestId { get { return 501408; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Basic; } }
}

public class GlenisDeliveryManusIntPtjScript : GlenisDeliveryManusPtjBaseScript
{
	protected override int QuestId { get { return 501438; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Int; } }
}

public class GlenisDeliveryManusAdvPtjScript : GlenisDeliveryAeiraPtjBaseScript
{
	protected override int QuestId { get { return 501468; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Adv; } }
}

// Script improvised. Update with official details whenever possible.
public abstract class GlenisExtDeliveryAeiraStewartPtjBaseScript : GlenisDeliveryPtjBaseScript
{
	protected override bool IsExtended { get { return true; } }

	protected override string LQuestDescription { get { return L("I need to deliver a [Fresh-baked Bread] to Aeira at the bookstore, but I'm too busy right now. Could you deliver this for me? - Glenis -"); } }
	protected override string LObjectiveDescription { get { return L("Deliver a [Fresh-baked Bread] to Aeira."); } }
	protected override int ItemId { get { return 70027; } }
	protected override string NpcIdent { get { return "_aeira"; } }

	protected override string LItemNotice { get { return L("You have given Fresh-baked Bread to be Delivered to Stewart."); } }

	public override async Task<HookResult> ClientAfterIntro(NpcScript npc, params object[] args)
	{
		if (!npc.QuestActive(this.Id, "ptj1"))
			return HookResult.Continue;

		npc.FinishQuest(this.Id, "ptj1");

		AfterIntroDialogue(npc);

		return HookResult.Break;
	}

	protected override void AfterIntroDialogue(NpcScript npc)
	{
		npc.Msg(L("Oh... that bread looks delicious.<br/>Only to be expected from Glenis.<p/>But... I'm full...<br/>Oh, I know. Would you please give that to Stewart?<br/>Sorry for troubling you."));
	}

	public override void Load()
	{
		base.Load(); // Allow Aeira's objective to go first.

		AddObjective("ptj2", L("Deliver a [Fresh-baked Bread] to Stewart."), 0, 0, 0, Deliver(ItemId, "_stewart"));

		AddHook("_stewart", "after_intro", RecipientAfterIntro);
	}

	private async Task<HookResult> RecipientAfterIntro(NpcScript npc, params object[] args)
	{
		if (!npc.QuestActive(this.Id, "ptj2"))
			return HookResult.Continue;

		npc.FinishQuest(this.Id, "ptj2");

		npc.Player.RemoveItem(ItemId, 1);
		npc.Notice(LItemNotice);

		npc.Msg(L("Whose bread is that?<br/>Ah Aeira's, is it?<br/>It looks very appetizing. Thank you very much for bringing this."));
		npc.Msg(Hide.Name, L("(Delivered a Fresh-baked Bread to Stewart.)"));

		return HookResult.Break;
	}
}

public class GlenisExtDeliveryAeiraStewartIntPtjScript : GlenisExtDeliveryAeiraStewartPtjBaseScript
{
	protected override int QuestId { get { return 501439; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Int; } }
}

public class GlenisExtDeliveryAeiraStewartAdvPtjScript : GlenisExtDeliveryAeiraStewartPtjBaseScript
{
	protected override int QuestId { get { return 501469; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Adv; } }
}

// Script improvised. Update with official details whenever possible.
public abstract class GlenisExtDeliveryAranwenStewartPtjBaseScript : GlenisDeliveryPtjBaseScript
{
	protected override bool IsExtended { get { return true; } }

	protected override string LQuestDescription { get { return L("I need to deliver [Fresh-baked Bread] to the school, but I'm too busy right now. Could you deliver this for me? - Glenis -"); } }
	protected override string LObjectiveDescription { get { return L("Deliver a [Fresh-baked Bread] to Aranwen."); } }
	protected override int ItemId { get { return 70027; } }
	protected override string NpcIdent { get { return "_aranwen"; } }

	protected override string LItemNotice { get { return L("You have given Fresh-baked Bread to be Delivered to Aranwen."); } }

	public override void OnReceive(Creature creature)
	{
		// Give player an extra bread to deliver
		// in addition to the one given by the first Deliver objective.
		creature.GiveItem(70027);
	}

	protected override void AfterIntroDialogue(NpcScript npc)
	{
		npc.Msg(L("From Glenis?<br/>...Thank you."));
		npc.Msg(Hide.Name, L("(Delivered a Fresh-baked Bread to Aranwen.)"));
	}

	public override void Load()
	{
		base.Load();

		AddObjective("ptj2", L("Deliver a [Fresh-baked Bread] to Stewart."), 0, 0, 0, Deliver(ItemId, "_stewart"));

		AddHook("_stewart", "after_intro", RecipientAfterIntro);
	}

	private async Task<HookResult> RecipientAfterIntro(NpcScript npc, params object[] args)
	{
		if (!npc.QuestActive(this.Id, "ptj2"))
			return HookResult.Continue;

		npc.FinishQuest(this.Id, "ptj2");

		npc.Player.RemoveItem(ItemId, 1);
		npc.Notice(L("You have given Fresh-baked Bread to be Delivered to Stewart."));

		npc.Msg(L("Bread from Glenis?<br/>Thank you very much."));
		npc.Msg(Hide.Name, L("(Delivered a Fresh-baked Bread to Stewart.)"));

		return HookResult.Break;
	}
}

public class GlenisExtDeliveryAranwenStewartIntPtjScript : GlenisExtDeliveryAranwenStewartPtjBaseScript
{
	protected override int QuestId { get { return 501440; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Int; } }
}

public class GlenisExtDeliveryAranwenStewartAdvPtjScript : GlenisExtDeliveryAeiraStewartPtjBaseScript
{
	protected override int QuestId { get { return 501470; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Adv; } }
}