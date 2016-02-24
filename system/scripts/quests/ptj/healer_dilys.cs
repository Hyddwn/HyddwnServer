//--- Aura Script -----------------------------------------------------------
// Dilys' Healer Part-Time Job
//--- Description -----------------------------------------------------------
// All quests used by the PTJ, and a script to handle the PTJ via hooks.
//--- Notes -----------------------------------------------------------------
// Apparently the rewards have been increased, as the Wiki now lists
// higher ones than in the past. We'll use the G1 rewards for now.
// Ref.: http://wiki.mabinogiworld.com/index.php?title=Dilys&oldid=143928
//---------------------------------------------------------------------------

public class DilysPtjScript : GeneralScript
{
	const PtjType JobType = PtjType.HealersHouse;

	const int Start = 6;
	const int Report = 9;
	const int Deadline = 15;
	const int PerDay = 20;

	int remaining = PerDay;

	readonly int[] QuestIds = new int[]
	{
		505101, // Basic  Gather 10 Wool
		505131, // Int    Gather 20 Wool
		505161, // Adv    Gather 30 Wool
		505401, // Basic  Potion Delivery (Piaras)
		505431, // Int    Potion Delivery (Piaras)
		505461, // Adv    Potion Delivery (Piaras)
		505402, // Basic  Potion Delivery (Ranald)
		505432, // Int    Potion Delivery (Ranald)
		505462, // Adv    Potion Delivery (Ranald)
		505403, // Basic  Potion Delivery (Deian)
		505433, // Int    Potion Delivery (Deian)
		505463, // Adv    Potion Delivery (Deian)
		505404, // Basic  Potion Delivery (Endelyon)
		505434, // Int    Potion Delivery (Endelyon)
		505464, // Adv    Potion Delivery (Endelyon)
		505405, // Basic  Potion Delivery (Duncan)
		505435, // Int    Potion Delivery (Duncan)
		505465, // Adv    Potion Delivery (Duncan)
	};

	public override void Load()
	{
		AddHook("_dilys", "after_intro", AfterIntro);
		AddHook("_dilys", "before_keywords", BeforeKeywords);
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
			npc.Msg("Are you working for someone else?<br/>Can you help me after you're finished?");
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
					npc.Msg("You're a little early.<br/>Report to me when it's closer to the deadline.");
				else
					npc.Msg("How's it going?");
				return;
			}

			// Report?
			npc.Msg("Did you complete the job I asked you to do?<br/>You can report to me even if you have not finished it<br/>and I will pay you for what you have done.<button title='Report Now' keyword='@report' /><button title='Report Later' keyword='@later' />");

			if (await npc.Select() != "@report")
			{
				npc.Msg("Good, I trust you.<br/>Please make sure to report before the deadline.");
				return;
			}

			// Nothing done
			if (result == QuestResult.None)
			{
				npc.GiveUpPtj();

				npc.Msg(npc.FavorExpression(), "Are you feeling sick?<br/>You should rest instead of working so hard.<br/>But, a promise is a promise. I am sorry, but I can't pay you this time.");
				npc.ModifyRelation(0, -Random(3), 0);
			}
			// Low~Perfect result
			else
			{
				npc.Msg("Nice job, <username/>. You did great.<br/>For now, this is all I can give you as a token of my gratitude.<br/>Please choose one. <button title='Report Later' keyword='@later' /><arbeit_report result=\"0\"/>");
				var reply = await npc.Select();

				// Report later
				if (!reply.StartsWith("@reward:"))
				{
					npc.Msg("Yes, <username/>.<br/>even if you come back later, I will hold on to your pay.<br/>But don't be too late.");
					return;
				}

				// Complete
				npc.CompletePtj(reply);
				remaining--;

				// Result msg
				if (result == QuestResult.Perfect)
				{
					npc.Msg(npc.FavorExpression(), "Fine job. Just what I asked!<br/>Thank you very much.");
					npc.ModifyRelation(0, Random(3), 0);
				}
				else if (result == QuestResult.Mid)
				{
					npc.Msg(npc.FavorExpression(), "You didn't bring me enough this time.<br/>I am sorry, but I will have to deduct it from your pay.");
					npc.ModifyRelation(0, Random(1), 0);
				}
				else if (result == QuestResult.Low)
				{
					npc.Msg(npc.FavorExpression(), "You don't seem to be at the top of your game today.<br/>Sorry, I can only pay you for what you've completed.");
					npc.ModifyRelation(0, -Random(2), 0);
				}

				// Herbalism quest
				if (npc.GetPtjSuccessCount(JobType) >= 10 && !npc.HasSkill(SkillId.Herbalism) && !npc.HasQuest(200042) && !npc.HasQuest(200063))
				{
					npc.Msg("Say, <username/>.  Do you have any interest in learning Herbalism?<br/>You've been such a great help to me here, I thought you might be interested in becoming a healer.<br/>If you're interested in Herbalism, I have a favor to ask you.<br/>If you do it, then I'll teach you.<button title='I will do it' keyword='@yes' /><button title='No, thanks' keyword='@no' />");
					if (await npc.Select() == "@yes")
					{
						npc.StartQuest(200063); // Gather Base Herb (Dilys)
						npc.Msg("You sound really interested in becoming a healer...<br/>If you step outside, an owl will deliver my request to you.");
					}
					else
					{
						npc.Msg("Really?<br/>Then, I will see you next time when you need another part-time job.");
					}
				}
			}
			return;
		}

		// Check if PTJ time
		if (!npc.ErinnHour(Start, Deadline))
		{
			npc.Msg("It's not time to start work yet.<br/>Can you come back and ask for a job later?");
			return;
		}

		// Check if not done today and if there are jobs remaining
		if (!npc.CanDoPtj(JobType, remaining))
		{
			npc.Msg("There are no more jobs today.<br/>I will give you another job tomorrow.");
			return;
		}

		// Offer PTJ
		var randomPtj = npc.RandomPtj(JobType, QuestIds);
		var ptjXml = npc.GetPtjXml(randomPtj, "Dilys's Healer's House Part-Time Job", "Looking for help with delivering goods in Healer's House.", PerDay, remaining);
		var msg = "";

		if (npc.GetPtjDoneCount(JobType) == 0)
			msg = "Do you need some work to do?<br/>If you want, you can help me here.<br/>The pay is not great, but I will definitely pay you for your work.<br/>The pay also depends on how long you've worked for me.<br/>Would you like to try?";
		else
			msg = "Ah, <username/>. Can you help me today?";

		npc.Msg(msg + ptjXml);

		if (await npc.Select() == "@accept")
		{
			npc.Msg("Thank you for your help in advance.");
			npc.StartPtj(randomPtj);
		}
		else
		{
			npc.Msg("You seem busy today.");
		}
	}
}

// Basic Wool quest
public class DilysWoolBasicPtjScript : QuestScript
{
	public override void Load()
	{
		SetId(505101);
		SetName("Healer's House Part-Time Job");
		SetDescription("This task is to gather wool that is used to make bandages. I got an order for [10 bundles of wool] today. Wool can be obtained from sheep.");

		SetType(QuestType.Deliver);
		SetPtjType(PtjType.HealersHouse);
		SetLevel(QuestLevel.Basic);
		SetHours(start: 6, report: 9, deadline: 15);

		AddObjective("ptj", "Gather 10 Wool", 0, 0, 0, Collect(60009, 10));

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

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(51012, 10)); // Stamina 30 Potion
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(175));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(51007, 5)); // MP 30 Potion
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(250));
	}
}

// Int Wool quest
public class DilysWoolIntPtjScript : QuestScript
{
	public override void Load()
	{
		SetId(505131);
		SetName("Healer's House Part-Time Job");
		SetDescription("This task is to gather wool that is used to make bandages. I got an order for [20 bundles of wool] today. Wool can be obtained from sheep.");

		SetType(QuestType.Deliver);
		SetPtjType(PtjType.HealersHouse);
		SetLevel(QuestLevel.Int);
		SetHours(start: 6, report: 9, deadline: 15);

		AddObjective("ptj", "Gather 20 Wool", 0, 0, 0, Collect(60009, 20));

		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(300));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(500));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(150));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(250));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(60));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(100));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(167));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(600));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(78));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(310));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(32));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(128));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(767));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(150));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(381));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(78));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(137));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(32));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(51012, 10)); // Stamina 30 Potion
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(450));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(51007, 10)); // MP 30 Potion
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(300));
	}
}

// Adv Wool quest
public class DilysWoolAdvPtjScript : QuestScript
{
	public override void Load()
	{
		SetId(505161);
		SetName("Healer's House Part-Time Job");
		SetDescription("This task is to gather wool that is used to make bandages. I got an order for [30 bundles of wool] today. Wool can be obtained from sheep.");

		SetType(QuestType.Deliver);
		SetPtjType(PtjType.HealersHouse);
		SetLevel(QuestLevel.Adv);
		SetHours(start: 6, report: 9, deadline: 15);

		AddObjective("ptj", "Gather 30 Wool", 0, 0, 0, Collect(60009, 30));

		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(500));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(1000));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(250));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(500));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(100));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(200));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(340));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(1120));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(157));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(570));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(60));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(240));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(1460));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(280));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(726));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(142));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(287));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(60));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(63000, 10)); // Phoenix Feather
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(400));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(51002, 10)); // HP 30 Potion
	}
}

// Delivery base script
public abstract class DilysDeliveryPtjBaseScript : QuestScript
{
	protected const int ItemId = 70004; // Unidentified Potion

	protected abstract int QuestId { get; }
	protected abstract string NpcName { get; }
	protected abstract string NpcIdent { get; }

	public override void Load()
	{
		SetId(QuestId);
		SetName("Potion Delivery");
		SetDescription("Please help me [deliver the potions] I made today. - Dilys -");

		SetType(QuestType.Deliver);
		SetPtjType(PtjType.HealersHouse);
		SetHours(start: 6, report: 9, deadline: 15);

		AddObjective("ptj", "Deliver Potion to " + NpcName, 0, 0, 0, Deliver(ItemId, NpcName));
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

		npc.Msg("Oh, great! A new potion from Dilys.<br/>And just in time.");
		npc.Msg(Hide.Name, "(Delivered the Potion to " + NpcName + ".)");
		npc.Msg("You didn't taste this potion, did you?");

		return HookResult.Break;
	}
}

// Basic delivery base quest
public abstract class DilysDeliveryBasicPtjBaseScript : DilysDeliveryPtjBaseScript
{
	public override void Load()
	{
		SetLevel(QuestLevel.Basic);

		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(200));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(150));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(100));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(75));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(40));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(30));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(65));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(260));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(35));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(140));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(14));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(56));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(313));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(65));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(153));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(35));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(56));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(14));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(51002, 2)); // HP 30 Potion
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(20));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(51012, 10)); // Stamina 30 Potion
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(25));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(60005, 10)); // Bandage
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(125));

		base.Load();
	}
}

// Int delivery base quest
public abstract class DilysDeliveryIntPtjBaseScript : DilysDeliveryPtjBaseScript
{
	public override void Load()
	{
		SetLevel(QuestLevel.Int);

		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(300));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(250));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(150));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(125));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(60));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(50));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(100));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(400));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(53));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(210));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(22));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(88));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(500));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(100));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(247));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(53));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(88));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(22));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(51002, 3)); // HP 30 Potion
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(80));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(51007, 3)); // MP 30 Potion
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(275));

		base.Load();
	}
}

// Adv delivery base quest
public abstract class DilysDeliveryAdvPtjBaseScript : DilysDeliveryPtjBaseScript
{
	public override void Load()
	{
		SetLevel(QuestLevel.Adv);

		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(400));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(350));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(200));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(175));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(80));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(70));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(147));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(540));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(70));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(280));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(30));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(120));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(687));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(135));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(340));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(70));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(127));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(30));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(51002, 4)); // HP 30 Potion
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(115));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(51007, 10)); // MP 30 Potion
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(225));

		base.Load();
	}
}

// Basic Piaras delivery quest
public class DilysPiarasBasicPtjScript : DilysDeliveryBasicPtjBaseScript
{
	protected override int QuestId { get { return 505401; } }
	protected override string NpcName { get { return "Piaras"; } }
	protected override string NpcIdent { get { return "_piaras"; } }
}

// Int Piaras delivery quest
public class DilysPiarasIntPtjScript : DilysDeliveryIntPtjBaseScript
{
	protected override int QuestId { get { return 505431; } }
	protected override string NpcName { get { return "Piaras"; } }
	protected override string NpcIdent { get { return "_piaras"; } }
}

// Adv Piaras delivery quest
public class DilysPiarasAdvPtjScript : DilysDeliveryAdvPtjBaseScript
{
	protected override int QuestId { get { return 505461; } }
	protected override string NpcName { get { return "Piaras"; } }
	protected override string NpcIdent { get { return "_piaras"; } }
}

// Basic Ranald delivery quest
public class DilysRanaldBasicPtjScript : DilysDeliveryBasicPtjBaseScript
{
	protected override int QuestId { get { return 505402; } }
	protected override string NpcName { get { return "Ranald"; } }
	protected override string NpcIdent { get { return "_ranald"; } }

	// OK, now slow down and take a breath.<br/>You must be here to deliver Dilys's Potion, right?
	// <title name='NONE' />(Delivered the Potion to Ranald.)
	// This potion is so fresh!<br/>Thank you very much.
}

// Int Ranald delivery quest
public class DilysRanaldIntPtjScript : DilysDeliveryIntPtjBaseScript
{
	protected override int QuestId { get { return 505432; } }
	protected override string NpcName { get { return "Ranald"; } }
	protected override string NpcIdent { get { return "_ranald"; } }
}

// Adv Ranald delivery quest
public class DilysRanaldAdvPtjScript : DilysDeliveryAdvPtjBaseScript
{
	protected override int QuestId { get { return 505462; } }
	protected override string NpcName { get { return "Ranald"; } }
	protected override string NpcIdent { get { return "_ranald"; } }
}

// Basic Deian delivery quest
public class DilysDeianBasicPtjScript : DilysDeliveryBasicPtjBaseScript
{
	protected override int QuestId { get { return 505403; } }
	protected override string NpcName { get { return "Deian"; } }
	protected override string NpcIdent { get { return "_deian"; } }
}

// Int Deian delivery quest
public class DilysDeianIntPtjScript : DilysDeliveryIntPtjBaseScript
{
	protected override int QuestId { get { return 505433; } }
	protected override string NpcName { get { return "Deian"; } }
	protected override string NpcIdent { get { return "_deian"; } }
}

// Adv Deian delivery quest
public class DilysDeianAdvPtjScript : DilysDeliveryAdvPtjBaseScript
{
	protected override int QuestId { get { return 505463; } }
	protected override string NpcName { get { return "Deian"; } }
	protected override string NpcIdent { get { return "_deian"; } }
}

// Basic Endelyon delivery quest
public class DilysEndelyonBasicPtjScript : DilysDeliveryBasicPtjBaseScript
{
	protected override int QuestId { get { return 505404; } }
	protected override string NpcName { get { return "Endelyon"; } }
	protected override string NpcIdent { get { return "_endelyon"; } }
}

// Int Endelyon delivery quest
public class DilysEndelyonIntPtjScript : DilysDeliveryIntPtjBaseScript
{
	protected override int QuestId { get { return 505434; } }
	protected override string NpcName { get { return "Endelyon"; } }
	protected override string NpcIdent { get { return "_endelyon"; } }
}

// Adv Endelyon delivery quest
public class DilysEndelyonAdvPtjScript : DilysDeliveryAdvPtjBaseScript
{
	protected override int QuestId { get { return 505464; } }
	protected override string NpcName { get { return "Endelyon"; } }
	protected override string NpcIdent { get { return "_endelyon"; } }
}

// Basic Duncan delivery quest
public class DilysDuncanBasicPtjScript : DilysDeliveryBasicPtjBaseScript
{
	protected override int QuestId { get { return 505405; } }
	protected override string NpcName { get { return "Duncan"; } }
	protected override string NpcIdent { get { return "_duncan"; } }

	// Oh, this must be Dilys' potion.<br/>Well done.
	// <title name='NONE' />(Delivered the potion to the Chief.)
	// You have been a great help to our town, and I really appreciate it.<br/>You have my unwavering trust. Please continue your work.
}

// Int Duncan delivery quest
public class DilysDuncanIntPtjScript : DilysDeliveryIntPtjBaseScript
{
	protected override int QuestId { get { return 505435; } }
	protected override string NpcName { get { return "Duncan"; } }
	protected override string NpcIdent { get { return "_duncan"; } }
}

// Adv Duncan delivery quest
public class DilysDuncanAdvPtjScript : DilysDeliveryAdvPtjBaseScript
{
	protected override int QuestId { get { return 505465; } }
	protected override string NpcName { get { return "Duncan"; } }
	protected override string NpcIdent { get { return "_duncan"; } }
}
