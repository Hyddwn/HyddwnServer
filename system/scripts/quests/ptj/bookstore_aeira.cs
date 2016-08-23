//--- Aura Script -----------------------------------------------------------
// Aeira's Bookstore Part-Time Job
//--- Description -----------------------------------------------------------
// All quests used by the PTJ, and a script to handle the PTJ via hooks.
//--- Notes -----------------------------------------------------------------
// Definitions for the following base scripts have been improvised,
// update with official script whenever possible:
// * AeiraExtDeliveryNerysStewartPtjBaseScript
// * AeiraExtDeliveryManusWalterPtjBaseScript
//---------------------------------------------------------------------------

public class AeiraPtjScript : GeneralScript
{
	const PtjType JobType = PtjType.Bookstore;

	const int Start = 13;
	const int Report = 15;
	const int Deadline = 20;
	const int PerDay = 10;

	int remaining = PerDay;

	readonly int[] QuestIds = new int[]
	{
		511401, // Item delivery (Glenis)
		511431, // Item delivery (Glenis)
		511461, // Item delivery (Glenis)
		511402, // Item delivery (Kristell)
		511432, // Item delivery (Kristell)
		511462, // Item delivery (Kristell)
		511433, // Item delivery (Nerys -> Stewart)
		511463, // Item delivery (Nerys -> Stewart)
		511434, // Item delivery (Manus -> Walter)
		511464, // Item delivery (Manus -> Walter)
	};

	public override void Load()
	{
		AddHook("_aeira", "after_intro", AfterIntro);
		AddHook("_aeira", "before_keywords", BeforeKeywords);
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
			npc.Msg(L("You seem to be working on something else already....<br/>It's probably a good idea to finish what you're working on first."));
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
					npc.Msg(L("I'm a little busy right now. Would you mind coming back later?"));
				}
				else
				{
					npc.Msg(L("How's the work going? You'll be fine."));
				}
				return;
			}

			// Report?
			npc.Msg(L("Are you done with the task I gave you?<br/>If not, you can come back later."),
				npc.Button(L("Report Now"), "@report"),
				npc.Button(L("Report Later"), "@later")
				);

			if (await npc.Select() != "@report")
			{
				npc.Msg(L("Then please come back later."));
				return;
			}

			// Nothing done
			if (result == QuestResult.None)
			{
				npc.GiveUpPtj();

				npc.Msg(npc.FavorExpression(), L("You didn't want to work, did you?<br/>I can't pay you, then."));
				npc.ModifyRelation(0, -Random(3), 0);
			}
			// Low~Perfect result
			else
			{
				npc.Msg(L("So... How shall I put this?<br/>This is a way of saying thank you for all the hard work you've done for me...<br/>Please pick something that you'll find useful."),
					npc.Button(L("Report Later"), "@later"),
					npc.PtjReport(result)
					);
				var reply = await npc.Select();

				// Report later
				if (!reply.StartsWith("@reward:"))
				{
					npc.Msg(npc.FavorExpression(), L("Yes, you can do it next time.<br/>I'll see you later."));
					return;
				}

				// Complete
				npc.CompletePtj(reply);
				remaining--;

				// Result msg
				if (result == QuestResult.Perfect)
				{
					npc.Msg(npc.FavorExpression(), L("Wow, thank you!<br/>Can you help me out again tomorrow?"));
					npc.ModifyRelation(0, Random(3), 0);
				}
				else
				{
					// Aeira doesn't have any PTJ quests that could yield
					// mid/low results.
				}
			}
			return;
		}

		// Check if PTJ time
		if (!npc.ErinnHour(Start, Deadline))
		{
			npc.Msg(L("Oh no. It's not time for a part-time job, yet.<br/>Please come back later."));
			return;
		}

		// Check if not done today and if there are jobs remaining
		if (!npc.CanDoPtj(JobType, remaining))
		{
			npc.Msg(L("I'm done for today!<br/>Please come again tomorrow."));
			return;
		}

		// Offer PTJ
		var randomPtj = npc.RandomPtj(JobType, QuestIds);
		var msg = "";

		if (npc.GetPtjDoneCount(JobType) == 0)
			msg = L("It must be your first time working at a bookstore.<br/>It's actually more of my personal business than work. Hehe.<br/>Will you do it?");
		else
			msg = L("Oh, can you help me today, too?");

		npc.Msg(msg, npc.PtjDesc(randomPtj,
			L("Aeira's Bookstore Part-time Job"),
			L("Looking for help with delivery of goods in Bookstore."),
			PerDay, remaining, npc.GetPtjDoneCount(JobType)));

		if (await npc.Select() == "@accept")
		{
			if (npc.GetPtjDoneCount(JobType) == 0)
				npc.Msg(L("Thank you. Please be on time for the deadline.<br/>Even if you don't get to finish everything, please come back and report.<br/>I'll pay you for the amount of work you've accomplished."));
			else
				npc.Msg(L("I believe in you."));

			npc.StartPtj(randomPtj);
		}
		else
		{
			if (npc.GetPtjDoneCount(JobType) == 0)
				npc.Msg(L("Well, if you don't want to, then I can't force you."));
			else
				npc.Msg(L("You must be pretty busy, aren't you?"));
		}
	}
}

// Variable item delivery quests
public abstract class AeiraVarDeliveryPtjBaseScript : QuestScript
{
	/// <summary>
	/// true: Picking up item to give to someone else.
	/// <para>false: Given item at quest start to give to someone else.</para>
	/// </summary>
	protected abstract bool IsTwoPart { get; }

	protected abstract int QuestId { get; }
	protected abstract string LQuestDescription { get; }
	protected abstract QuestLevel QuestLevel { get; }
	protected abstract string[] LObjectiveDescription { get; }
	protected abstract int ItemId { get; }
	protected abstract string[] LItemNotice { get; }
	protected abstract string[] NpcIdent { get; }

	protected abstract void AddRewards();

	protected abstract Action<NpcScript>[] AfterIntroDialogue { get; }

	public override void Load()
	{
		SetId(QuestId);
		SetName(L("Adven. Assoc. Part-Time Job"));
		SetDescription(LQuestDescription);

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.ById);

		SetType(QuestType.Deliver);
		SetPtjType(PtjType.Bookstore);
		SetLevel(QuestLevel);
		SetHours(start: 13, report: 15, deadline: 20);

		if (!IsTwoPart)
		{
			AddObjective("ptj1", LObjectiveDescription[0], 0, 0, 0, Deliver(ItemId, NpcIdent[0]));
		}
		else
		{
			AddObjective("ptj1", LObjectiveDescription[0], 0, 0, 0, Talk(NpcIdent[0]));
			AddObjective("ptj2", LObjectiveDescription[1], 0, 0, 0, Deliver(ItemId, NpcIdent[1]));
		}

		AddHook(NpcIdent[0], "after_intro", ClientAfterIntro);
		if (IsTwoPart)
			AddHook(NpcIdent[1], "after_intro", RecipientAfterIntro);

		AddRewards();
	}

	public async Task<HookResult> ClientAfterIntro(NpcScript npc, params object[] args)
	{
		if (!npc.QuestActive(this.Id, "ptj1"))
			return HookResult.Continue;

		npc.FinishQuest(this.Id, "ptj1");

		if (!IsTwoPart)
			npc.Player.RemoveItem(ItemId, 1);
		else
			npc.Player.GiveItem(ItemId);
		npc.Notice(LItemNotice[0]);

		AfterIntroDialogue[0](npc);

		return HookResult.Break;
	}

	/// <summary>Precondition: All protected array properties declared in this class has a size greater than 1.</summary>
	/// <exception cref="System.IndexOutOfRangeException">Thrown if precondition is not met.</exception>
	public async Task<HookResult> RecipientAfterIntro(NpcScript npc, params object[] args)
	{
		if (!npc.QuestActive(this.Id, "ptj2"))
			return HookResult.Continue;

		if (!npc.Player.Inventory.Has(ItemId))
			return HookResult.Continue;

		npc.FinishQuest(this.Id, "ptj2");

		npc.Player.RemoveItem(ItemId, 1);
		npc.Notice(LItemNotice[1]);

		AfterIntroDialogue[1](npc);

		return HookResult.Break;
	}
}

public abstract class AeiraDeliveryGlenisPtjBaseScript : AeiraVarDeliveryPtjBaseScript
{
	protected override bool IsTwoPart { get { return false; } }

	protected override string LQuestDescription { get { return L("Can you take this [ring] and give it to Glenis at the Grocery Store? I don't really need it... - Aeira -"); } }
	protected override string[] LObjectiveDescription { get { return new string[] { L("Deliver the [Ring] to Glenis.") }; } }
	protected override int ItemId { get { return 70022; } }
	protected override string[] LItemNotice { get { return new string[] { L("You have given Gift Ring to be Delivered to Glenis.") }; } }
	protected override string[] NpcIdent { get { return new string[] { "_glenis" }; } }

	protected override Action<NpcScript>[] AfterIntroDialogue { get { return new Action<NpcScript>[] { (npc) => {/* Glenis says nothing. */} }; } }
}

public class AeiraDeliveryGlenisBasicPtjScript : AeiraDeliveryGlenisPtjBaseScript
{
	protected override int QuestId { get { return 511401; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Basic; } }

	protected override void AddRewards() // Reward Set Basic-1
	{
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(180));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(150));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(90));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(75));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(36));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(30));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(60));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(250));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(30));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(125));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(12));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(50));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(300));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(60));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(150));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(30));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(60));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(12));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(51011, 10)); // Stamina 10 Potion
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(210));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(51001, 5)); // HP 10 Potion
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(60));

		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(51011, 5)); // Stamina 10 Potion
		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(260));
	}
}

public class AeiraDeliveryGlenisIntPtjScript : AeiraDeliveryGlenisPtjBaseScript
{
	protected override int QuestId { get { return 511431; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Int; } }

	protected override void AddRewards() // Reward Set Int-1
	{
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(260));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(250));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(130));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(125));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(52));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(50));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(95));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(375));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(47));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(187));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(19));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(75));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(460));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(100));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(230));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(50));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(92));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(20));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(63020)); // Empty Bottle
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(70));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(18006)); // Wizard Hat
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(70));

		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(51001, 9)); // HP 10 Potion
		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(20));

		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(51011, 10)); // Stamina 10 Potion
		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(370));
	}
}

public class AeiraDeliveryGlenisAdvPtjScript : AeiraDeliveryGlenisPtjBaseScript
{
	protected override int QuestId { get { return 511461; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Adv; } }

	protected override void AddRewards() // Reward Set Adv-1
	{
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(450));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(420));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(225));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(210));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(90));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(84));

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

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(40025)); // Pickaxe
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(32));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(40026)); // Sickle
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(182));

		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(63020)); // Empty Bottle
		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(382));

		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(18006)); // Wizard Hat
		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(382));
	}
}

public abstract class AeiraDeliveryKristellPtjBaseScript : AeiraVarDeliveryPtjBaseScript
{
	protected override bool IsTwoPart { get { return false; } }

	protected override string LQuestDescription { get { return L("Can you take this [Anthology] to priestess Kristell at Church? - Aeira -"); } }
	protected override string[] LObjectiveDescription { get { return new string[] { L("Deliver the [Anthology] to Kristell.") }; } }
	protected override int ItemId { get { return 70005; } }
	protected override string[] LItemNotice { get { return new string[] { L("You have given Anthology to be Delivered to Kristell.") }; } }
	protected override string[] NpcIdent { get { return new string[] { "_kristell" }; } }

	protected override Action<NpcScript>[] AfterIntroDialogue
	{
		get
		{
			return new Action<NpcScript>[] { (npc) => {
				npc.Msg(L("So, this is from Aeira?<br/>She's really a good girl, isn't she? Hehe."));
				npc.Msg(Hide.Name, L("(Delivered the Anthology to Kristell.)"));
			} };
		}
	}
}

public class AeiraDeliveryKristellBasicPtjScript : AeiraDeliveryKristellPtjBaseScript
{
	protected override int QuestId { get { return 511402; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Basic; } }

	protected override void AddRewards() // Reward Set Basic-1
	{
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(180));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(150));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(90));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(75));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(36));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(30));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(60));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(250));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(30));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(125));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(12));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(50));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(300));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(60));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(150));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(30));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(60));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(12));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(51011, 10)); // Stamina 10 Potion
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(210));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(51001, 5)); // HP 10 Potion
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(60));

		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(51011, 5)); // Stamina 10 Potion
		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(260));
	}
}

public class AeiraDeliveryKristellIntPtjScript : AeiraDeliveryKristellPtjBaseScript
{
	protected override int QuestId { get { return 511432; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Int; } }

	protected override void AddRewards() // Reward Set Int-2
	{
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(260));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(250));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(130));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(125));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(52));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(50));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(120));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(500));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(60));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(250));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(24));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(100));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(620));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(125));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(310));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(62));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(124));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(25));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(40026)); // Sickle
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(15));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(63020)); // Empty Bottle
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(215));

		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(18006)); // Wizard Hat
		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(215));

		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(51001, 10)); // HP 10 Potion
		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(115));
	}
}

public class AeiraDeliveryKristellAdvPtjScript : AeiraDeliveryKristellPtjBaseScript
{
	protected override int QuestId { get { return 511462; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Adv; } }

	protected override void AddRewards() // Reward Set Adv-1
	{
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(450));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(420));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(225));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(210));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(90));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(84));

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

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(40025)); // Pickaxe
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(32));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(40026)); // Sickle
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(182));

		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(63020)); // Empty Bottle
		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(382));

		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(18006)); // Wizard Hat
		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(382));
	}
}

// Quest strings improvised
public abstract class AeiraExtDeliveryNerysStewartPtjBaseScript : AeiraVarDeliveryPtjBaseScript
{
	protected override bool IsTwoPart { get { return true; } }

	protected override string LQuestDescription { get { return L("I have been asked to bring a hammer to Stewart, however... I need to run errands for my father. I'm sorry for asking, but could you help me take care of this? I ordered a hammer from Nerys's [Weapon Shop] that you can pick up to deliver to Stewart at the [school]. - Aeira -"); } }
	protected override string[] LObjectiveDescription
	{
		get
		{
			return new string[] {
				L("Receive the hammer from Nerys."),
				L("Deliver the hammer to Stewart.")
			};
		}
	}
	protected override int ItemId { get { return 70015; } }
	protected override string[] LItemNotice
	{
		get
		{
			return new string[] {
				L("You have received Hammer to be Delivered from Nerys"),
				L("You have given Hammer to be Delivered to Stewart")
			};
		}
	}
	protected override string[] NpcIdent { get { return new string[] { "_nerys", "_stewart" }; } }

	protected override Action<NpcScript>[] AfterIntroDialogue
	{
		get
		{
			return new Action<NpcScript>[]
			{ (npc) => {
				npc.Msg(L("Picking up the hammer for Aeira?<br/>Here you go."));
				npc.Msg(Hide.Name, L("(Received the hammer from Nerys.)"));
			}, (npc) => {
				npc.Msg(L("Did Aeira send you?<br/>Thank you for bringing this to me."));
				npc.Msg(Hide.Name, L("(Delivered the hammer to Stewart.)"));
			} };
		}
	}
}

public class AeiraExtDeliveryNerysStewartIntPtjScript : AeiraExtDeliveryNerysStewartPtjBaseScript
{
	protected override int QuestId { get { return 511433; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Int; } }

	protected override void AddRewards() // Reward Set Int-2
	{
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(260));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(250));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(130));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(125));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(52));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(50));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(120));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(500));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(60));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(250));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(24));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(100));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(620));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(125));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(310));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(62));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(124));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(25));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(40026)); // Sickle
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(15));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(63020)); // Empty Bottle
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(215));

		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(18006)); // Wizard Hat
		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(215));

		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(51001, 10)); // HP 10 Potion
		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(115));
	}
}

public class AeiraExtDeliveryNerysStewartAdvPtjScript : AeiraExtDeliveryNerysStewartPtjBaseScript
{
	protected override int QuestId { get { return 511463; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Adv; } }

	protected override void AddRewards() // Reward Set Adv-1
	{
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(450));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(420));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(225));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(210));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(90));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(84));

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

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(40025)); // Pickaxe
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(32));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(40026)); // Sickle
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(182));

		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(63020)); // Empty Bottle
		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(382));

		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(18006)); // Wizard Hat
		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(382));
	}
}

// Quest strings improvised
public abstract class AeiraExtDeliveryManusWalterPtjBaseScript : AeiraVarDeliveryPtjBaseScript
{
	protected override bool IsTwoPart { get { return true; } }

	protected override string LQuestDescription { get { return L("My father seems to have fallen ill. Would you kindly get a potion from the Healer's House to deliver to him? - Aeira -"); } }
	protected override string[] LObjectiveDescription
	{
		get
		{
			return new string[] {
				L("Receive the potion from Manus."),
				L("Deliver the potion to Walter.")
			};
		}
	}
	protected override int ItemId { get { return 70004; } }
	protected override string[] LItemNotice
	{
		get
		{
			return new string[] {
				L("You have received Unidentified Potion from Manus"),
				L("You have given Unidentified Potion to Walter")
			};
		}
	}
	protected override string[] NpcIdent { get { return new string[] { "_manus", "_walter" }; } }

	protected override Action<NpcScript>[] AfterIntroDialogue
	{
		get
		{
			return new Action<NpcScript>[]
			{ (npc) => {
				npc.Msg(L("Hm, I see.<br/>Here, take this potion.<br/>Don't worry about payment."));
				npc.Msg(Hide.Name, L("(Received the potion from Manus.)"));
			}, (npc) => {
				npc.Msg(L("What? Aeira's potion?<br/>How much did she pay to have this delivered?"));
				npc.Msg(Hide.Name, L("(Walter's in a bad mood...)"));
			} };
		}
	}
}

public class AeiraExtDeliveryManusWalterIntPtjScript : AeiraExtDeliveryManusWalterPtjBaseScript
{
	protected override int QuestId { get { return 511434; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Int; } }

	protected override void AddRewards() // Reward Set Int-3
	{
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
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(130));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(26));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(40026)); // Sickle
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(45));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(63020)); // Empty Bottle
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(245));

		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(18006)); // Wizard Hat
		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(245));

		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(51001, 10)); // HP 10 Potion
		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(145));
	}
}

public class AeiraExtDeliveryManusWalterAdvPtjScript : AeiraExtDeliveryManusWalterPtjBaseScript
{
	protected override int QuestId { get { return 511464; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Adv; } }

	protected override void AddRewards() // Reward Set Adv-2
	{
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

		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(63020)); // Empty Bottle
		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(400));

		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(18006)); // Wizard Hat
		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(400));
	}
}