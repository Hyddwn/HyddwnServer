//--- Aura Script -----------------------------------------------------------
// Walter's General Shop Part-Time Job
//--- Description -----------------------------------------------------------
// All quests used by the PTJ, and a script to handle the PTJ via hooks.
//--- Notes -----------------------------------------------------------------
// Definitions for the following base scripts have been improvised,
// update with official script whenever possible:
// * WalterExtDeliveryGlenisAeiraPtjBaseScript
// * WalterExtDeliveryNerysStewartPtjBaseScript
//---------------------------------------------------------------------------

public class WalterPtjScript : GeneralScript
{
	const PtjType JobType = PtjType.GeneralShop;

	const int Start = 7;
	const int Report = 9;
	const int Deadline = 19;
	const int PerDay = 10;

	int remaining = PerDay;

	readonly int[] QuestIds = new int[]
	{
		508406, // Basic  Item delivery (Aeira)
		508436, // Int    Item delivery (Aeira)
		508466, // Adv    Item delivery (Aeira)
		508407, // Basic  Item delivery (Manus)
		508437, // Int    Item delivery (Manus)
		508467, // Adv    Item delivery (Manus)
		508438, // Int    Item delivery (Glenis -> Aeira)
		508468, // Adv    Item delivery (Glenis -> Aeira)
		508439, // Int    Item delivery (Nerys  -> Stewart)
		508469, // Adv    Item delivery (Nerys  -> Stewart)

		508503, // Basic  Weave 2 Braids
		508504, // Basic  Weave 5 Cheap Leather Straps
		508533, // Int    Weave 5 Common Leather Straps
		508534, // Int    Weave 5 Fine Leather Straps
		508535, // Int    Weave 3 Braids
		508564, // Adv    Weave 5 Finest Leather Straps
		508565, // Adv    Weave 5 Braids
	};

	public override void Load()
	{
		AddHook("_walter", "after_intro", AfterIntro);
		AddHook("_walter", "before_keywords", BeforeKeywords);
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
			npc.Msg(L("Just finish what you're doing."));
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
					npc.Msg(L("It's not time yet."));
				}
				else
				{
					npc.Msg(L("Is everything alright?"));
				}
				return;
			}

			// Report?
			npc.Msg(L("Have you finished your work?"), npc.Button(L("Report Now"), "@report"), npc.Button(L("Report Later"), "@later"));

			if (await npc.Select() != "@report")
			{
				npc.Msg(L("Come when you're finished."));
				return;
			}

			// Nothing done
			if (result == QuestResult.None)
			{
				npc.GiveUpPtj();

				npc.Msg(npc.FavorExpression(), L("Don't ever come here again!"));
				npc.ModifyRelation(0, -Random(3), 0);
			}
			// Low~Perfect result
			else
			{
				npc.Msg(L("Phew... Well done.<br/>Take your pick."),
					npc.Button(L("Report Later"), "@later"),
					npc.PtjReport(result)
					);
				var reply = await npc.Select();

				// Report later
				if (!reply.StartsWith("@reward:"))
				{
					npc.Msg(npc.FavorExpression(), L("Hah..."));
					return;
				}

				// Complete
				npc.CompletePtj(reply);
				remaining--;

				// Result msg
				if (result == QuestResult.Perfect)
				{
					npc.Msg(npc.FavorExpression(), L("Good!"));
					npc.ModifyRelation(0, Random(3), 0);
				}
				else if (result == QuestResult.Mid)
				{
					npc.Msg(npc.FavorExpression(), L("This is not enough..."));
					npc.ModifyRelation(0, Random(1), 0);
				}
				else if (result == QuestResult.Low)
				{
					npc.Msg(npc.FavorExpression(), L("What is this!"));
					npc.ModifyRelation(0, -Random(2), 0);
				}
			}
			return;
		}

		// Check if PTJ time
		if (!npc.ErinnHour(Start, Deadline))
		{
			npc.Msg(L("It's not the right time."));
			return;
		}

		// Check if not done today and if there are jobs remaining
		if (!npc.CanDoPtj(JobType, remaining))
		{
			npc.Msg(L("That's it for today."));
			return;
		}

		// Offer PTJ
		var randomPtj = npc.RandomPtj(JobType, QuestIds);
		var msg = "";

		if (npc.GetPtjDoneCount(JobType) == 0)
			msg = L("Is this your first time? Good luck.");
		else
			msg = L("I'm counting on you as usual.");

		var ptjTitle = "";
		if ((randomPtj / 100) % 10 == 5) // Poll third least significant digit
			ptjTitle = L("Looking for weavers.");
		else
			ptjTitle = L("Looking for help with delivery of goods in General Shop.");

		npc.Msg(msg, npc.PtjDesc(randomPtj, L("Walter's General Shop Part-Time Job"), L(ptjTitle), PerDay, remaining, npc.GetPtjDoneCount(JobType)));

		if (await npc.Select() == "@accept")
		{
			if (npc.GetPtjDoneCount(JobType) == 0)
				npc.Msg(L("You have to finish your work before the deadline and come back to me.<br/>"));
			else
				npc.Msg(L("Well, then."));

			npc.StartPtj(randomPtj);
		}
		else
		{
			npc.Msg(L("I got it."));
		}
	}
}

public abstract class WalterDeliveryPtjBaseScript : QuestScript
{
	protected abstract int QuestId { get; }
	protected abstract string LQuestDescription { get; }
	protected abstract QuestLevel QuestLevel { get; }
	protected abstract string LObjectiveDescription { get; }
	protected abstract string LItemNotice { get; }
	protected abstract int ItemId { get; }
	protected abstract string NpcIdent { get; }

	public override void Load()
	{
		SetId(QuestId);
		SetName(L("General Shop Part-Time Job"));
		SetDescription(LQuestDescription);

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.ById);

		SetType(QuestType.Deliver);
		SetPtjType(PtjType.GeneralShop);
		SetLevel(QuestLevel);
		SetHours(start: 7, report: 9, deadline: 19);

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

		npc.Player.RemoveItem(ItemId);
		npc.FinishQuest(this.Id, "ptj");
		npc.Notice(LItemNotice);

		await this.OnFinish(npc);

		return HookResult.Break;
	}

	protected virtual async Task OnFinish(NpcScript npc)
	{
		await Task.Yield();
	}

	/// <remarks>
	/// Rewards common among single-objective deliveries.
	/// </remarks>
	private void AddRewards()
	{
		switch (QuestLevel)
		{
			case QuestLevel.Basic:
				AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(250));
				AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(150));
				AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(125));
				AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(75));
				AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(50));
				AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(30));

				AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(60));
				AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(300));
				AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(30));
				AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(150));
				AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(12));
				AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(60));

				AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(370));
				AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(60));
				AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(185));
				AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(30));
				AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(74));
				AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(12));

				AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(50004)); // Bread
				AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(306));

				AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(40023)); // Gathering Knife
				AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(312));

				AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(51001, 7)); // HP 10 Potion
				AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(12));

				AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(51011, 5)); // Stamina 10 Potion
				AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(312));
				break;

			case QuestLevel.Int:
				AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(320));
				AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(220));
				AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(160));
				AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(110));
				AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(64));
				AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(44));

				AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(90));
				AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(375));
				AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(45));
				AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(187));
				AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(18));
				AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(75));

				AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(480));
				AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(80));
				AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(240));
				AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(40));
				AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(96));
				AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(16));

				AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(63020)); // Empty Bottle
				AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(65));

				AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(50004)); // Bread
				AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(306));

				AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(51001, 9)); // HP 10 Potion
				AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(15));

				AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(51011, 5)); // Stamina 10 Potion
				AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(415));
				break;

			case QuestLevel.Adv:
				AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(430));
				AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(300));
				AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(215));
				AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(150));
				AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(86));
				AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(60));

				AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(135));
				AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(520));
				AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(87));
				AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(260));
				AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(27));
				AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(104));

				AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(665));
				AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(125));
				AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(332));
				AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(62));
				AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(133));
				AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(25));

				AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(18024)); // Hairband
				AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(48));

				AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(18012)); // Tork Merchant Cap
				AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(48));

				AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(19001)); // Robe
				AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(148));

				AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(19002)); // Slender Robe
				AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(148));
				break;

			default:
				Log.Warning("Walter PTJ: No reward set for quest ID {0}. Fell back to basic rewards.", QuestId);
				goto case QuestLevel.Basic;
		}
	}
}

public abstract class WalterDeliveryAeiraPtjBaseScript : WalterDeliveryPtjBaseScript
{
	protected override string LQuestDescription { get { return L("I need to give this cubic puzzle to my daughter at the bookstore. Can you help me out? - Walter -"); } }
	protected override string LObjectiveDescription { get { return L("Deliver the [Cubic Puzzle] to Aeira."); } }
	protected override string LItemNotice { get { return L("You have given Cubic Puzzle to be Delivered to Aeira."); } }
	protected override int ItemId { get { return 70006; } }
	protected override string NpcIdent { get { return "_aeira"; } }

	protected override async Task OnFinish(NpcScript npc)
	{
		npc.Msg(L("Is this from my dad? Cool!<br/>Thanks for bringing it to me!!"));
		npc.Msg(Hide.Name, L("(Delivered the Cubic Puzzle to Aeira.)"));
	}
}

public class WalterDeliveryAeiraBasicPtjScript : WalterDeliveryAeiraPtjBaseScript
{
	protected override int QuestId { get { return 508406; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Basic; } }
}

public class WalterDeliveryAeiraIntPtjScript : WalterDeliveryAeiraPtjBaseScript
{
	protected override int QuestId { get { return 508436; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Int; } }
}

public class WalterDeliveryAeiraAdvPtjScript : WalterDeliveryAeiraPtjBaseScript
{
	protected override int QuestId { get { return 508466; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Adv; } }
}

public abstract class WalterDeliveryManusPtjBaseScript : WalterDeliveryPtjBaseScript
{
	protected override string LQuestDescription { get { return L("I need to deliver this glass bottle to Manus at the Healer's House. Can you help me? - Walter -"); } }
	protected override string LObjectiveDescription { get { return L("Deliver the [Glass Bottle] to Manus."); } }
	protected override string LItemNotice { get { return L("You have given Empty Bottle to be Delivered to Manus."); } }
	protected override int ItemId { get { return 70037; } }
	protected override string NpcIdent { get { return "_manus"; } }

	protected override async Task OnFinish(NpcScript npc)
	{
		npc.Msg(L("Ah ha, so you're delivering for Walter?<br/>I was getting worried because I was running short on glass bottles. Thanks!"));
		npc.Msg(Hide.Name, L("(Delivered the Glass Bottle to Manus.)"));
	}
}

public class WalterDeliveryManusBasicPtjScript : WalterDeliveryManusPtjBaseScript
{
	protected override int QuestId { get { return 508407; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Basic; } }
}

public class WalterDeliveryManusIntPtjScript : WalterDeliveryManusPtjBaseScript
{
	protected override int QuestId { get { return 508437; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Int; } }
}

public class WalterDeliveryManusAdvPtjScript : WalterDeliveryManusPtjBaseScript
{
	protected override int QuestId { get { return 508467; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Adv; } }
}

public abstract class WalterExtDeliveryPtjBaseScript : QuestScript
{
	protected abstract int QuestId { get; }
	protected abstract string LQuestDescription { get; }
	protected abstract QuestLevel QuestLevel { get; }
	protected abstract string LGetObjectiveDescription { get; }
	protected abstract string LGiveObjectiveDescription { get; }
	protected abstract string LGetItemNotice { get; }
	protected abstract string LGiveItemNotice { get; }
	protected abstract int ItemId { get; }
	protected abstract string GetNpcIdent { get; }
	protected abstract string GiveNpcIdent { get; }

	public override void Load()
	{
		SetId(QuestId);
		SetName(L("General Shop Part-Time Job"));
		SetDescription(LQuestDescription);

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.ById);

		SetType(QuestType.Deliver);
		SetPtjType(PtjType.GeneralShop);
		SetLevel(QuestLevel.Basic);
		SetHours(start: 7, report: 9, deadline: 19);

		AddObjective("ptj1", LGetObjectiveDescription, 0, 0, 0, Talk(GetNpcIdent));
		AddObjective("ptj2", LGiveObjectiveDescription, 0, 0, 0, Deliver(ItemId, GiveNpcIdent));
		AddHook(GetNpcIdent, "after_intro", GetAfterIntro);
		AddHook(GiveNpcIdent, "after_intro", GiveAfterIntro);

		AddRewards();
	}

	public async Task<HookResult> GetAfterIntro(NpcScript npc, params object[] args)
	{
		if (!npc.QuestActive(this.Id, "ptj1"))
			return HookResult.Continue;

		npc.Player.GiveItem(ItemId);
		npc.FinishQuest(this.Id, "ptj1");
		npc.Notice(LGetItemNotice);

		await this.GetOnFinish(npc);

		return HookResult.Break;
	}

	public async Task<HookResult> GiveAfterIntro(NpcScript npc, params object[] args)
	{
		if (!npc.QuestActive(this.Id, "ptj2"))
			return HookResult.Continue;

		if (!npc.Player.Inventory.Has(ItemId))
			return HookResult.Continue;

		npc.FinishQuest(this.Id, "ptj2");
		npc.Player.RemoveItem(ItemId);
		npc.Notice(LGiveItemNotice);

		await this.GiveOnFinish(npc);

		return HookResult.Break;
	}

	protected virtual async Task GetOnFinish(NpcScript npc)
	{
		await Task.Yield();
	}

	protected virtual async Task GiveOnFinish(NpcScript npc)
	{
		await Task.Yield();
	}

	/// <remarks>
	/// Rewards common among double-objective deliveries.
	/// </remarks>
	private void AddRewards()
	{
		switch (QuestLevel)
		{
			case QuestLevel.Basic:
				Log.Warning("Walter PTJ: Quest ID {0} set to undefined basic rewards. Fell back to int rewards.", QuestId);
				goto case QuestLevel.Int;

			case QuestLevel.Int:
				AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(380));
				AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(250));
				AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(190));
				AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(125));
				AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(76));
				AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(50));

				AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(115));
				AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(450));
				AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(57));
				AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(225));
				AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(23));
				AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(90));

				AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(580));
				AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(100));
				AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(290));
				AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(50));
				AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(116));
				AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(20));

				AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(19001)); // Robe
				AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(60));

				AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(19002)); // Slender Robe
				AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(60));

				AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(63020)); // Empty Bottle
				AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(160));

				AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(50004)); // Bread
				AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(507));
				break;

			case QuestLevel.Adv:
				AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(520));
				AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(350));
				AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(260));
				AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(175));
				AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(104));
				AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(70));

				AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(160));
				AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(620));
				AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(80));
				AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(315));
				AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(32));
				AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(124));

				AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(800));
				AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(140));
				AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(400));
				AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(70));
				AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(160));
				AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(28));

				AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(18024)); // Hairband
				AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(165));

				AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(40025)); // Pickaxe
				AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(15));

				AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(18012)); // Tork Merchant Cap
				AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(165));

				AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(19001)); // Robe
				AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(265));
				break;

			default:
				Log.Warning("Walter PTJ: No reward set for quest ID {0}. Fell back to int rewards.", QuestId);
				goto case QuestLevel.Int;
		}
	}
}

// Quest strings improvised
public abstract class WalterExtDeliveryGlenisAeiraPtjBaseScript : WalterExtDeliveryPtjBaseScript
{
	protected override string LQuestDescription { get { return L("I want you to deliver food to my daughter working at the bookstore. Go visit Glenis at [the grocery store] to pick up the cheese I ordered. - Walter -"); } }
	protected override string LGetObjectiveDescription { get { return L("Receive the cheese from Glenis."); } }
	protected override string LGiveObjectiveDescription { get { return L("Deliver the cheese to Aeira."); } }
	protected override string LGetItemNotice { get { return L("You have received Lump of Cheese to be Delivered from Glenis"); } }
	protected override string LGiveItemNotice { get { return L("You have given Lump of Cheese to be Delivered to Aeira"); } }
	protected override int ItemId { get { return 70034; } }
	protected override string GetNpcIdent { get { return "_glenis"; } }
	protected override string GiveNpcIdent { get { return "_aeira"; } }

	protected override async Task GetOnFinish(NpcScript npc)
	{
		npc.Msg(L("Even with the way Walter is, he sure dotes on his daughter, doesn't he? Haha.<br/>Here's the cheese."));
		npc.Msg(Hide.Name, L("(Received the cheese from Glenis.)"));
	}

	protected override async Task GiveOnFinish(NpcScript npc)
	{
		npc.Msg(L("Wow! Thank you!<br/>Is this from my dad?<br/>Looks delicious!"));
		npc.Msg(Hide.Name, L("(Delivered the cheese to Aeira.)"));
	}
}

public class WalterExtDeliveryGlenisAeiraIntPtjScript : WalterExtDeliveryGlenisAeiraPtjBaseScript
{
	protected override int QuestId { get { return 508438; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Int; } }
}

public class WalterExtDeliveryGlenisAeiraAdvPtjScript : WalterExtDeliveryGlenisAeiraPtjBaseScript
{
	protected override int QuestId { get { return 508468; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Adv; } }
}

// Quest strings improvised
public abstract class WalterExtDeliveryNerysStewartPtjBaseScript : WalterExtDeliveryPtjBaseScript
{
	protected override string LQuestDescription { get { return L("I accidentally sent a candlestick to Nerys at the weapon shop. It was meant to go to Stewart. Can you help me? - Walter -"); } }
	protected override string LGetObjectiveDescription { get { return L("Receive the candlestick from Nerys."); } }
	protected override string LGiveObjectiveDescription { get { return L("Deliver the candlestick to Stewart."); } }
	protected override string LGetItemNotice { get { return L("You have received Candlestick to be Delivered from Nerys"); } }
	protected override string LGiveItemNotice { get { return L("You have given Candlestick to be Delivered to Stewart"); } }
	protected override int ItemId { get { return 70009; } }
	protected override string GetNpcIdent { get { return "_nerys"; } }
	protected override string GiveNpcIdent { get { return "_stewart"; } }

	protected override async Task GetOnFinish(NpcScript npc)
	{
		npc.Msg(L("Here for Walter's candlestick?<br/>Here you go."));
		npc.Msg(Hide.Name, L("(Received the candlestick from Nerys.)"));
	}

	protected override async Task GiveOnFinish(NpcScript npc)
	{
		npc.Msg(L("Oh, thank you. What a great candlestick."));
		npc.Msg(Hide.Name, L("(Delivered the candlestick to Stewart.)"));
	}
}

public class WalterExtDeliveryNerysStewartIntPtjScript : WalterExtDeliveryNerysStewartPtjBaseScript
{
	protected override int QuestId { get { return 508439; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Int; } }
}

public class WalterExtDeliveryNerysStewartAdvPtjScript : WalterExtDeliveryNerysStewartPtjBaseScript
{
	protected override int QuestId { get { return 508469; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Adv; } }
}

public abstract class WalterWeavePtjBaseScript : QuestScript
{
	protected abstract int QuestId { get; }
	protected abstract string LQuestDescription { get; }
	protected abstract int ItemId { get; }
	protected abstract int ItemCount { get; }
	protected abstract string LCreateObjectiveDescription { get; }
	protected abstract string LCollectObjectiveDescription { get; }
	protected abstract QuestLevel QuestLevel { get; }

	protected abstract void AddRewards();

	public override void Load()
	{
		SetId(QuestId);
		SetName(L("General Shop Part-Time Job"));
		SetDescription(LQuestDescription);

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.ById);

		SetType(QuestType.Deliver);
		SetPtjType(PtjType.GeneralShop);
		SetLevel(QuestLevel);
		SetHours(start: 7, report: 9, deadline: 19);

		AddObjective("ptj1", LCreateObjectiveDescription, 0, 0, 0, Create(ItemId, ItemCount, SkillId.Weaving));
		AddObjective("ptj2", LCollectObjectiveDescription, 0, 0, 0, Collect(ItemId, ItemCount));

		AddRewards();
	}
}

public class WalterWeaveBraidBasicPtjScript : WalterWeavePtjBaseScript
{
	protected override int QuestId { get { return 508503; } }
	protected override string LQuestDescription { get { return L("This job is to make braid. Use the thread that was supplied to make them. Deadline is this evening, so don't come to me before then."); } }
	protected override int ItemId { get { return 60404; } }
	protected override int ItemCount { get { return 2; } }
	protected override string LCreateObjectiveDescription { get { return L("Make 2 Braids (Part-time job)"); } }
	protected override string LCollectObjectiveDescription { get { return L("2 Braids (Part-time job)"); } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Basic; } }

	public override void OnReceive(Creature creature)
	{
		// Thick Thread Ball (Part-Time Job)
		creature.GiveItem(60406, 5);
		creature.GiveItem(60406, 5);
		creature.GiveItem(60406, 5);

		// Thin Thread Ball (Part-Time Job)
		creature.GiveItem(60407, 5);
		creature.GiveItem(60407, 5);
		creature.GiveItem(60407, 5);
	}

	protected override void AddRewards()
	{
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(600));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(900));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(300));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(450));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(120));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(180));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(333));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(1100));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(166));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(550));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(66));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(220));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(1440));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(270));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(720));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(135));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(288));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(54));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(40004)); // Lute
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(375));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(60034, 300)); // Bait Tin
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(375));

		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(17025)); // Sandal
		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(475));

		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(40044)); // Ladle
		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(501));
	}
}

public class WalterWeaveCheapLeatherStrapBasicPtjScript : WalterWeavePtjBaseScript
{
	protected override int QuestId { get { return 508504; } }
	protected override string LQuestDescription { get { return L("This job is to make a cheap leather strap. Use the cheap leather that was supplied to make them. Deadline is this evening, so don't come to me before then."); } }
	protected override int ItemId { get { return 60427; } }
	protected override int ItemCount { get { return 5; } }
	protected override string LCreateObjectiveDescription { get { return L("Make 5 Cheap Leather Straps (Part-time job)"); } }
	protected override string LCollectObjectiveDescription { get { return L("5 Cheap Leather Straps (Part-time job)"); } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Basic; } }

	public override void OnReceive(Creature creature)
	{
		// Cheap Leather (Part-Time Job)
		creature.GiveItem(60423, 10);
		creature.GiveItem(60423, 10);
		creature.GiveItem(60423, 10);
	}

	protected override void AddRewards()
	{
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(400));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(600));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(200));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(300));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(80));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(120));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(200));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(760));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(100));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(380));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(40));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(152));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(960));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(180));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(480));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(90));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(192));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(36));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(17025)); // Sandal
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(25));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(40042)); // Cooking Knife
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(95));

		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(40044)); // Ladle
		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(51));

		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(60034, 300)); // Bait Tin
		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(25));
	}
}

public class WalterWeaveCommonLeatherStrapIntPtjScript : WalterWeavePtjBaseScript
{
	protected override int QuestId { get { return 508533; } }
	protected override string LQuestDescription { get { return L("This job is to make a common leather strap. Use the common leather that was supplied to make them. Deadline is this evening, so don't come to me before then."); } }
	protected override int ItemId { get { return 60428; } }
	protected override int ItemCount { get { return 5; } }
	protected override string LCreateObjectiveDescription { get { return L("Make 5 Common Leather Straps (Part-time job)"); } }
	protected override string LCollectObjectiveDescription { get { return L("5 Common Leather Straps (Part-time job)"); } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Int; } }

	public override void OnReceive(Creature creature)
	{
		// Common Leather (Part-Time Job)
		creature.GiveItem(60424, 10);
		creature.GiveItem(60424, 10);
		creature.GiveItem(60424, 10);
	}

	protected override void AddRewards()
	{
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(900));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(1300));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(450));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(650));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(180));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(260));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(500));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(1600));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(250));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(800));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(100));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(320));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(2100));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(400));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(1050));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(200));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(420));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(80));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(46005)); // Cooking Table
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(179));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(2001)); // Gold Pouch
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(240));

		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(18016)); // Hat
		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(300));

		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(40004)); // Lute
		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(1000));
	}
}

public class WalterWeaveFineLeatherStrapIntPtjScript : WalterWeavePtjBaseScript
{
	protected override int QuestId { get { return 508534; } }
	protected override string LQuestDescription { get { return L("This job is to make a fine leather strap. Use the fine leather that was supplied to make them. Deadline is this evening, so don't come to me before then."); } }
	protected override int ItemId { get { return 60429; } }
	protected override int ItemCount { get { return 5; } }
	protected override string LCreateObjectiveDescription { get { return L("Make 5 Fine Leather Straps (Part-time job)"); } }
	protected override string LCollectObjectiveDescription { get { return L("5 Fine Leather Straps (Part-time job)"); } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Int; } }

	public override void OnReceive(Creature creature)
	{
		// Fine Leather (Part-Time Job)
		creature.GiveItem(60425, 10);
		creature.GiveItem(60425, 10);
		creature.GiveItem(60425, 10);
	}

	protected override void AddRewards()
	{
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(1100));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(1700));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(550));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(850));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(220));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(340));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(635));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(2050));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(317));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(1025));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(127));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(410));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(2700));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(500));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(1350));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(250));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(540));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(100));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(46005)); // Cooking Table
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(729));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(2001)); // Gold Pouch
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(790));

		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(18016)); // Hat
		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(850));

		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(40045)); // Fishing Rod
		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(550));
	}
}

public class WalterWeaveBraidIntPtjScript : WalterWeavePtjBaseScript
{
	protected override int QuestId { get { return 508535; } }
	protected override string LQuestDescription { get { return L("This job is to make braid. Use the thread that was supplied to make them. Deadline is this evening, so don't come to me before then."); } }
	protected override int ItemId { get { return 60404; } }
	protected override int ItemCount { get { return 3; } }
	protected override string LCreateObjectiveDescription { get { return L("Make 3 Braids (Part-time job)"); } }
	protected override string LCollectObjectiveDescription { get { return L("3 Braids (Part-time job)"); } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Int; } }

	public override void OnReceive(Creature creature)
	{
		// Thick Thread Ball (Part-Time Job)
		creature.GiveItem(60406, 5);
		creature.GiveItem(60406, 5);
		creature.GiveItem(60406, 5);

		// Thin Thread Ball (Part-Time Job)
		creature.GiveItem(60407, 5);
		creature.GiveItem(60407, 5);
		creature.GiveItem(60407, 5);
	}

	protected override void AddRewards()
	{
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(900));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(1600));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(450));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(800));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(180));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(320));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(580));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(1840));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(290));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(920));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(116));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(368));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(2420));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(460));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(1210));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(230));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(484));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(92));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(46005)); // Cooking Table
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(479));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(2001)); // Gold Pouch
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(540));

		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(18016)); // Hat
		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(600));

		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(40045)); // Fishing Rod
		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(300));
	}
}

public class WalterWeaveFinestLeatherStrapAdvPtjScript : WalterWeavePtjBaseScript
{
	protected override int QuestId { get { return 508564; } }
	protected override string LQuestDescription { get { return L("This job is to make the finest leather strap. Use the finest leather that was supplied to make them. Deadline is this evening, so don't come to me before then."); } }
	protected override int ItemId { get { return 60430; } }
	protected override int ItemCount { get { return 5; } }
	protected override string LCreateObjectiveDescription { get { return L("Make 5 Finest Leather Straps (Part-time job)"); } }
	protected override string LCollectObjectiveDescription { get { return L("5 Finest Leather Straps (Part-time job)"); } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Adv; } }

	public override void OnReceive(Creature creature)
	{
		// Finest Leather (Part-Time Job)
		creature.GiveItem(60426, 10);
		creature.GiveItem(60426, 10);
		creature.GiveItem(60426, 10);
	}

	protected override void AddRewards()
	{
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(1600));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(3200));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(800));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(1600));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(320));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(640));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(1150));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(3540));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(575));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(1770));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(230));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(708));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(4700));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(875));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(2350));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(437));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(940));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(175));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(40045));      // Fishing Rod
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(60034, 300)); // Bait Tin
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(1525));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(2001)); // Gold Pouch
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(2665));

		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(18016)); // Hat
		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(2725));
	}
}

public class WalterWeaveBraidAdvPtjScript : WalterWeavePtjBaseScript
{
	protected override int QuestId { get { return 508565; } }
	protected override string LQuestDescription { get { return L("This job is to make braid. Use the thread that was supplied to make them. Deadline is this evening, so don't come to me before then."); } }
	protected override int ItemId { get { return 60404; } }
	protected override int ItemCount { get { return 5; } }
	protected override string LCreateObjectiveDescription { get { return L("Make 5 Braids (Part-time job)"); } }
	protected override string LCollectObjectiveDescription { get { return L("5 Braids (Part-time job)"); } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Adv; } }

	public override void OnReceive(Creature creature)
	{
		// Thick Thread Ball (Part-Time Job)
		creature.GiveItem(60406, 5);
		creature.GiveItem(60406, 5);
		creature.GiveItem(60406, 5);
		creature.GiveItem(60406, 5);

		// Thin Thread Ball (Part-Time Job)
		creature.GiveItem(60407, 5);
		creature.GiveItem(60407, 5);
		creature.GiveItem(60407, 5);
		creature.GiveItem(60407, 5);
	}

	protected override void AddRewards()
	{
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(1300));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(2700));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(650));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(1350));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(260));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(540));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(950));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(2960));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(475));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(1480));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(190));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(592));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(3925));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(730));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(1962));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(365));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(785));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(146));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(46005)); // Cooking Table
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(479));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(2001)); // Gold Pouch
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(540));

		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(18016)); // Hat
		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(600));

		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(40045)); // Fishing Rod
		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(300));

		AddReward(8, RewardGroupType.Item, QuestResult.Perfect, Pattern(64500, 20103, 10)); // Blacksmith Manual - Round Shield
	}
}
