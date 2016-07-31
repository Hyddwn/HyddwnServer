// Note: Weaving and tailoring jobs have a different report time. 
// Reset (int) Report based on quest ID in OnErinnMidnightTick()?
// With the way QuestIDs are set up, one could check digits with integer division and/or modulus.
/* Report times (according to mabiwiki):
 * Advanced tailoring jobs report at 17
 * Other weaving/tailoring jobs report at 12
 * Anything else reports at 9
 */
// Update: Could there be a race condition if multiple people of different report times speak to the NPC? This is not major, though: everyone has the same deadline, at least.

// When inquiring for a PTJ, the <title> field changes depending on the job type.

// MISSINGNO prevents this script from compiling.
// The following MISSINGNO have unknown values at the time of writing.
// (Tip: Perform a find and replace all operation to define them.)
/* MISSINGNO000  QuestID: Basic  Garment Delivery (Nora)
 * MISSINGNO002  QuestID: Basic  Garment Delivery (Caitin)
 * MISSINGNO003  QuestID: Basic  Item Delivery (Dilys)
 * MISSINGNO005  QuestID: Basic  Tailor 2 Wizard Hats
 * MISSINGNO006  QuestID: Basic  Tailor 2 Headbands
 * MISSINGNO030  QuestID: Int    Garment Delivery (Bebhinn)
 * MISSINGNO032  QuestID: Int    Garment Delivery (Lassar)
 * MISSINGNO033  QuestID: Int    Garment Delivery (Caitin)
 * MISSINGNO034  QuestID: Int    Weave Thin Thread Ball
 * MISSINGNO035  QuestID: Int    Weave Thick Thread Ball
 * MISSINGNO036  QuestID: Int    Weave 2 Thin Thread Balls
 * MISSINGNO037  QuestID: Int    Weave 2 Thick Thread Balls
 * MISSINGNO038  QuestID: Int    Tailor 2 Cores Ninja Suits (M)
 * MISSINGNO039  QuestID: Int    Tailor 2 Magic School Uniforms (M)
 * MISSINGNO040  QuestID: Int    Tailor 2 Guardian Gloves
 * MISSINGNO041  QuestID: Int    Tailor 2 Cores' Healer Suits
 * MISSINGNO060  QuestID: Adv    Garment Delivery (Lassar)
 * MISSINGNO061  QuestID: Adv    Weave 2 Thin Thread Balls
 * MISSINGNO062  QuestID: Adv    Weave 2 Thick Thread Balls
 * MISSINGNO063  QuestID: Adv    Tailor 2 Lirina's Long Skirts
 * MISSINGNO064  QuestID: Adv    Tailor 2 Mongo's Hats
 * MISSINGNO065  QuestID: Adv    Tailor 2 Cloth Mails
 * MISSINGNO066  QuestID: Adv    Tailor 2 Light Leather Mails (M)
 */

// Class name resolution - go go gadget intellisense
using Aura.Mabi;
using Aura.Mabi.Const;
using System.Threading.Tasks;
using Aura.Channel.Scripting.Scripts;

//--- Aura Script -----------------------------------------------------------
// Malcolm's General Store Part-Time Job
//--- Description -----------------------------------------------------------
// All quests used by the PTJ, and a script to handle the PTJ via hooks.
//--- Notes -----------------------------------------------------------------
// Apparently the rewards have been increased, as the Wiki now lists
// higher ones than in the past. We'll use the G1 rewards for now.
// Due to limited G1 data, some current rewards are downscaled to 50% gold, 33% exp.
// Ref: http://wiki.mabinogiworld.com/index.php?title=Malcolm&oldid=145857#Part-time_Jobs
//---------------------------------------------------------------------------

public class MalcolmPtjScript : GeneralScript
{
	const PtjType JobType = PtjType.GeneralShop;

	const int Start = 7;
	int Report = 9;
	const int Deadline = 19;
	const int PerDay = 8;

	int remaining = PerDay;

	readonly int[] QuestIds = new int[]
	{
		MISSINGNO005, // Basic  Tailor 2 Wizard Hats
		MISSINGNO006, // Basic  Tailor 2 Headbands
		508207, // Basic  Tailor 2 Popo's Skirts (F)
		508210, // Basic  Tailor 2 Mongo Traveler Suits (F)
		508212, // Basic  Tailor 2 Leather Bandanas
		MISSINGNO003, // Basic  Garment Delivery (Dilys)
		MISSINGNO000, // Basic  Garment Delivery (Nora) // Appears twice on mabiwiki?
		508401, // Basic  Garment Delivery (Caitin) // Confirm?
		508402, // Int    Garment Delivery (Nora)
		508403, // Basic  Garment Delivery (Lassar)
		508405, // Basic  Garment Delivery (Bebhinn)
		508501, // Basic  Weave Thick Thread Ball
		508502, // Basic  Weave Thin Thread Ball
		MISSINGNO030, // Int    Garment Delivery (Bebhinn)
		MISSINGNO032, // Int    Garment Delivery (Lassar)
		MISSINGNO033, // Int    Garment Delivery (Caitin)
		MISSINGNO034, // Int    Weave Thin Thread Ball
		MISSINGNO035, // Int    Weave Thick Thread Ball
		MISSINGNO036, // Int    Weave 2 Thin Thread Balls
		MISSINGNO037, // Int    Weave 2 Thick Thread Balls
		MISSINGNO038, // Int    Tailor 2 Cores Ninja Suits (M)
		MISSINGNO039, // Int    Tailor 2 Magic School Uniforms (M)
		MISSINGNO040, // Int    Tailor 2 Guardian Gloves
		MISSINGNO041, // Int    Tailor 2 Cores' Healer Suits
		MISSINGNO060, // Adv    Garment Delivery (Lassar)
		MISSINGNO061, // Adv    Weave 2 Thin Thread Balls
		MISSINGNO062, // Adv    Weave 2 Thick Thread Balls
		MISSINGNO063, // Adv    Tailor 2 Lirina's Long Skirts
		MISSINGNO064, // Adv    Tailor 2 Mongo's Hats
		MISSINGNO065, // Adv    Tailor 2 Cloth Mails
		MISSINGNO066, // Adv    Tailor 2 Light Leather Mails (M)
	};

	public override void Load()
	{
		AddHook("_malcolm", "after_intro", AfterIntro);
		AddHook("_malcolm", "before_keywords", BeforeKeywords);

		// Pre-populate report time and job description variables (Is this necessary?)
		//OnErinnMidnightTick(ErinnTime.Now);
	}

	public async Task<HookResult> AfterIntro(NpcScript npc, params object[] args)
	{
		// Handle receiving of Flowerpot from Bebhinn
		const int Flowerpot = 70010;
		int id;
		if (npc.QuestActive(id = 508405, "ptj3") || npc.QuestActive(id = MISSINGNO030, "ptj3"))
		{
			if (!npc.Player.Inventory.Has(Flowerpot))
				return HookResult.Continue;

			npc.Player.Inventory.Remove(Flowerpot, 1);
			npc.FinishQuest(id, "ptj3");

			npc.Notice(L("You have given Flowerpot to be Delivered to Malcolm."));

			npc.Msg(L("Oh, no.<br/>That Flowerpot, did Bebhinn give it to you instead of the money for the clothes?"));
			npc.Msg(Hide.Name, L("(Delivered the small Flowerpot to Malcolm.)"));
			npc.Msg(L("I have no idea why everyone brings me flowerpots instead of paying me in cash.<br/>Anyway, thank you.<br/>Hmm, it's weird, but I think I kind of like this flowerpot."));
			npc.Msg(Hide.Name, L("(Another Flowerpot is added to Malcolm's collection.)"));
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
			npc.Msg(L("You seem to be on another job.<br/>You should finish it first."));
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
					npc.Msg(L("Oh...<br/>Would you come after the deadline starts?"));
					npc.Msg(L("Thanks for your help."));
				}
				else
				{
					npc.Msg(L("How's it going?<br/>"));
					npc.Msg(L("I'm expecting good things from you."));
				}
				return;
			}

			// Report?
			npc.Msg(L("Ah, did you finish the work?<br/>Would you like to report now?<br/>If you haven't finished the job, you can report later."),
				npc.Button(L("Report Now"), "@report"),
				npc.Button(L("Report Later"), "@later")
				);

			if (await npc.Select() != "@report")
			{
				npc.Msg(L("You can report any time before the deadline ends."));
				return;
			}

			// Nothing done
			if (result == QuestResult.None)
			{
				npc.GiveUpPtj();

				npc.Msg(npc.FavorExpression(), L("Did I ask too much of you?<br/>Sorry, but I can't pay you because you didn't work at all. Please understand."));
				npc.ModifyRelation(0, -Random(3), 0);
			}
			// Low~Perfect result
			else
			{
				npc.Msg(L("To tell you the truth, I was worried.<br/>But... <username/>, I think I can count on you from here on out.<br/>This is just a token of my gratitude.<br/>Choose whatever you like."),
					npc.Button(L("Report Later"), "@later"),
					npc.PtjReport(result)
					);
				var reply = await npc.Select();

				// Report later
				if (!reply.StartsWith("@reward:"))
				{
					npc.Msg(npc.FavorExpression(), L("OK then. Come again later.<br/>... But don't be late."));
					return;
				}

				// Complete
				npc.CompletePtj(reply);
				remaining--;

				// Result msg
				if (result == QuestResult.Perfect)
				{
					npc.Msg(npc.FavorExpression(), L("Oh... Thank you.<br/>I appreciate your help."));
					npc.ModifyRelation(0, Random(3), 0);
				}
				else if (result == QuestResult.Mid)
				{
					npc.Msg(npc.FavorExpression(), L("Well... It is not enough,<br/>but I'm grateful for your help. However, I have to reduce your pay. I hope you'll understand."));
					npc.ModifyRelation(0, Random(1), 0);
				}
				else if (result == QuestResult.Low)
				{
					npc.Msg(npc.FavorExpression(), L("I guess you were busy with something else...<br/>It's not much, but I'll pay you for what you've done."));
					npc.ModifyRelation(0, -Random(2), 0);
				}
			}
			return;
		}

		// Check if PTJ time
		if (!npc.ErinnHour(Start, Deadline))
		{
			npc.Msg(L("Sorry, but it is not time for part-time jobs.<br/>Would you come later?"));
			return;
		}

		// Check if not done today and if there are jobs remaining
		if (!npc.CanDoPtj(JobType, remaining))
		{
			npc.Msg(L("I don't have anymore work to give you today.<br/>Would you come back tomorrow?"));
			return;
		}

		// Offer PTJ
		var randomPtj = npc.RandomPtj(JobType, QuestIds);

		var msg = "";
		if (npc.GetPtjDoneCount(JobType) == 0)
			msg = L("Our town may be small, but running the General Shop<br/>can really get hectic since I'm running this all by myself.<br/>Fortunately, many people are helping me out, so it's a lot easier for me to handle.<br/>Are you also interested in working here, <username/>?<p/>I'll pay you if you can help me.");
		else
			msg = L("Are you here to work at the General Shop?");

		var ptjDescTitle = "";
		switch (/* Pattern against QuestId to determine job type */) // Pending: QuestIDs (How should it be matched?)
		{
			case "WeavingJob":
				ptjDescTitle = L("Looking for weavers."); 
				break;
			case "TailoringJob":
				ptjDescTitle = L("Looking for help with crafting items needed for General Shop."); 
				break;
			default:
				ptjDescTitle = L("Looking for help with delivery of goods in General Shop.");
				break;
		}

		npc.Msg(msg, npc.PtjDesc(randomPtj,
			L("Malcolm's General Shop Part-Time Job"),
			ptjDescTitle,
			PerDay, remaining, npc.GetPtjDoneCount(JobType)));

		if (await npc.Select() == "@accept")
		{
			if (npc.GetPtjDoneCount(JobType) == 0)
				npc.Msg(L("Thank you.<br/>Then I'll see you when the deadline starts.<br/>Please report your work even if you couldn't finish it on time. That way, I can get on with other jobs without worry."));
			else
				npc.Msg(L("Thanks, and good luck!"));

			npc.StartPtj(randomPtj);
		}
		else
		{
			if (npc.GetPtjDoneCount(JobType) == 0)
				npc.Msg(L("Oh, well.<br/>If you change your mind, let me know."));
			else
				npc.Msg(L("Oh, I misunderstood.<br/>I'm sorry."));
		}
	}
}

// Delivery base script
public abstract class MalcolmDeliveryPtjBaseScript : QuestScript
{
	protected const int Garment = 70003;

	protected abstract int QuestId { get; }
	protected abstract string NpcIdent { get; }
	protected abstract string NpcName { get; }
	protected abstract string Objective { get; }

	public override void Load()
	{
		SetId(QuestId);
		SetName("General Shop Part-Time Job");
		SetDescription("I need to [deliver the items] that arrived today, but I can't afford to leave the shop unattended. Could you deliver the goods for me? - Malcolm -");

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.ById);

		SetType(QuestType.Deliver);
		SetPtjType(PtjType.GeneralShop);
		SetHours(start: 7, report: 9, deadline: 19);

		AddObjective("ptj", this.Objective, 0, 0, 0, Deliver(Garment, NpcIdent));
		AddHook(NpcIdent, "after_intro", AfterIntro);
	}

	public async Task<HookResult> AfterIntro(NpcScript npc, params object[] args)
	{
		if (!npc.QuestActive(this.Id, "ptj"))
			return HookResult.Continue;

		if (!npc.Player.Inventory.Has(Garment))
			return HookResult.Continue;

		npc.Player.Inventory.Remove(Garment, 1);
		npc.Notice("You have given Garment to be Delivered to {0}.", NpcName);
		npc.FinishQuest(this.Id, "ptj");

		await this.OnFinish(npc);

		return HookResult.Break;
	}

	protected virtual async Task OnFinish(NpcScript npc)
	{
		await Task.Yield();
	}

	protected void SetBasicLevelAndRewards()
	{
		SetLevel(QuestLevel.Basic);

		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(200));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(250));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(100));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(125));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(40));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(50));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(65));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(260));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(33));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(130));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(13));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(52));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(320));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(65));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(160));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(33));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(64));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(13));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(50004, 1)); // Bread
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(600));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(51001, 6)); // HP 10 Potion
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(25));

		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(51011, 5)); // Stamina 10 Potion
		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(275));
	}

	protected void SetIntLevelAndRewards()
	{
		SetLevel(QuestLevel.Int);

		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(300));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(250));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(150));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(125));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(60));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(50));

		AddReward(2, RewardGroupType.Item, QuestResult.Perfect, Item(50004, 1)); // Bread
		AddReward(2, RewardGroupType.Item, QuestResult.Perfect, Gold(444));

		AddReward(3, RewardGroupType.Item, QuestResult.Perfect, Item(51001, 6)); // HP 10 Potion
		AddReward(3, RewardGroupType.Item, QuestResult.Perfect, Gold(400));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(51011, 5)); // Stamina 10 Potion
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(450));
	}

	protected void SetAdvLevelAndRewards()
	{
		SetLevel(QuestLevel.Adv);

		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(350));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(525));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(175));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(263));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(70));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(105));

		AddReward(2, RewardGroupType.Item, QuestResult.Perfect, Item(19001, 1)); // Robe
		AddReward(2, RewardGroupType.Item, QuestResult.Perfect, Gold(410));

		AddReward(3, RewardGroupType.Item, QuestResult.Perfect, Item(19002, 6)); // Slender Robe
		AddReward(3, RewardGroupType.Item, QuestResult.Perfect, Gold(410));
	}
}

public class MalcolmDeliveryCaitinBasicPtjScript : MalcolmDeliveryPtjBaseScript
{
	protected override int QuestId { get { return 508401; } }
	protected override string NpcIdent { get { return "_caitin"; } }
	protected override string NpcName { get { return "Caitin"; } }
	protected override string Objective { get { return L("Deliver Garment to Caitin at the Grocery Store"); } }

	public override void Load()
	{
		SetBasicLevelAndRewards();

		base.Load();
	}

	protected override async Task OnFinish(NpcScript npc)
	{
		// (Caitin has no dialogue.)
	}
}

public class MalcolmDeliveryCaitinIntPtjScript : MalcolmDeliveryPtjBaseScript
{
	protected override int QuestId { get { return MISSINGNO033; } }
	protected override string NpcIdent { get { return "_caitin"; } }
	protected override string NpcName { get { return "Caitin"; } }
	protected override string Objective { get { return L("Deliver Garment to Caitin at the Grocery Store"); } }

	public override void Load()
	{
		SetIntLevelAndRewards();

		base.Load();
	}

	protected override async Task OnFinish(NpcScript npc)
	{
		// (Caitin has no dialogue.)
	}
}

public class MalcolmDeliveryNoraBasicPtjScript : MalcolmDeliveryPtjBaseScript
{
	protected override int QuestId { get { return 508402; } }
	protected override string NpcIdent { get { return "_nora"; } }
	protected override string NpcName { get { return "Nora"; } }
	protected override string Objective { get { return L("Deliver Garment to Nora at the Inn"); } }

	public override void Load()
	{
		SetBasicLevelAndRewards();

		base.Load();
	}

	protected override async Task OnFinish(NpcScript npc)
	{
		npc.Msg(L("Malcolm sent you here, right?<br/>I cannot understand this guy!"));
		npc.Msg(Hide.Name, L("(Delivered the clothes to Nora.)"));
		npc.Msg(L("Sorry, I'm not mad at you.<br/>The thing is, I never ordered this.<br/>I really don't understand why he keeps sending me stuff I didn't ask for."));
		npc.Msg(L("I might be tempted to take it, but it's not really my taste anyway.<br/>I guess I will just return it to him... again."));
		npc.Msg(Hide.Name, L("(It seems pretty obvious why Malcolm is doing this.)"));
	}
}

public class MalcolmDeliveryLassarBasicPtjScript : MalcolmDeliveryPtjBaseScript
{
	protected override int QuestId { get { return 508403; } }
	protected override string NpcIdent { get { return "_lassar"; } }
	protected override string NpcName { get { return "Lassar"; } }
	protected override string Objective { get { return L("Deliver Clothes to Lassar at the School"); } }

	public override void Load()
	{
		SetBasicLevelAndRewards();

		base.Load();
	}

	protected override async Task OnFinish(NpcScript npc)
	{
		npc.Msg(L("Oh, this is for me?<br/>You're so sweet. How did you know my favorite style?"));
		npc.Msg(Hide.Name, L("(Delivered the clothes to Lassar.)"));
		npc.Msg(L("Oh, I see now... This is what I ordered from Malcolm.<br/>Ha ha, sorry for my mistake.<br/>I didn't mean to say that in hopes of getting free clothes. Don't worry about that."));
		npc.Msg(L("Anyway, look at it. This is the style that I like.<br/>I thought you would like to remember it, just in case. That's all.<br/>Really."));
		npc.Msg(Hide.Name, L("(The pressure becomes unbearable.)"));
	}
}

public class MalcolmDeliveryLassarIntPtjScript : MalcolmDeliveryPtjBaseScript
{
	protected override int QuestId { get { return MISSINGNO032; } }
	protected override string NpcIdent { get { return "_lassar"; } }
	protected override string NpcName { get { return "Lassar"; } }
	protected override string Objective { get { return L("Deliver Clothes to Lassar at the School"); } }

	public override void Load()
	{
		SetIntLevelAndRewards();

		base.Load();
	}

	protected override async Task OnFinish(NpcScript npc)
	{
		npc.Msg(L("Oh, this is for me?<br/>You're so sweet. How did you know my favorite style?"));
		npc.Msg(Hide.Name, L("(Delivered the clothes to Lassar.)"));
		npc.Msg(L("Oh, I see now... This is what I ordered from Malcolm.<br/>Ha ha, sorry for my mistake.<br/>I didn't mean to say that in hopes of getting free clothes. Don't worry about that."));
		npc.Msg(L("Anyway, look at it. This is the style that I like.<br/>I thought you would like to remember it, just in case. That's all.<br/>Really."));
		npc.Msg(Hide.Name, L("(The pressure becomes unbearable.)"));
	}
}

public class MalcolmDeliveryLassarIntPtjScript : MalcolmDeliveryPtjBaseScript
{
	protected override int QuestId { get { return MISSINGNO060; } }
	protected override string NpcIdent { get { return "_lassar"; } }
	protected override string NpcName { get { return "Lassar"; } }
	protected override string Objective { get { return L("Deliver Clothes to Lassar at the School"); } }

	public override void Load()
	{
		SetAdvLevelAndRewards();

		base.Load();
	}

	protected override async Task OnFinish(NpcScript npc)
	{
		npc.Msg(L("Oh, this is for me?<br/>You're so sweet. How did you know my favorite style?"));
		npc.Msg(Hide.Name, L("(Delivered the clothes to Lassar.)"));
		npc.Msg(L("Oh, I see now... This is what I ordered from Malcolm.<br/>Ha ha, sorry for my mistake.<br/>I didn't mean to say that in hopes of getting free clothes. Don't worry about that."));
		npc.Msg(L("Anyway, look at it. This is the style that I like.<br/>I thought you would like to remember it, just in case. That's all.<br/>Really."));
		npc.Msg(Hide.Name, L("(The pressure becomes unbearable.)"));
	}
}

// Extended delivery quest - Bebhinn
public abstract class MalcolmExtDeliveryBebhinnPtjBaseScript : QuestScript
{
	protected const int Garment = 70003;
	protected const int Flowerpot = 70010;

	public override void Load()
	{
		SetName("General Shop Part-Time Job");
		SetDescription("I need to [deliver the items] that arrived today, but I can't afford to leave the shop unattended. Could you deliver the goods for me? - Malcolm -");

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.ById);

		SetType(QuestType.Deliver);
		SetPtjType(PtjType.GeneralShop);
		SetHours(start: 7, report: 9, deadline: 19);

		AddObjective("ptj1", L("Deliver Garment to Bebhinn at the Bank"), 0, 0, 0, Deliver(Garment, "_bebhinn"));
		AddObjective("ptj2", L("Keep talking to Bebhinn about payment"), 0, 0, 0, Talk("_bebhinn"));
		AddObjective("ptj3", L("Deliver Flowerpot to Malcolm at the General Shop."), 0, 0, 0, Deliver(Flowerpot, "_malcolm"));

		AddHook("_bebhinn", "after_intro", BebhinnAfterIntro);
		// Malcolm is handled above, as we have to deliver the item to him 
		// before he asks us about the PTJ.
	}

	public async Task<HookResult> BebhinnAfterIntro(NpcScript npc, params object[] args)
	{
		if (npc.QuestActive(this.Id, "ptj1"))
		{
			if (!npc.Player.Inventory.Has(Garment))
				return HookResult.Continue;

			npc.FinishQuest(this.Id, "ptj1");

			npc.Player.RemoveItem(Garment, 1);
			npc.Notice("You have given Garment to be Delivered to Bebhinn.");

			npc.Msg(L("Wow, so the clothes I ordered have finally arrived.<br/>Thank you so much! Wow, I really like this style!"));
			npc.Msg(Hide.Name, L("(Delivered the clothes to Bebhinn.)"));
			npc.Msg(L("Yes? Payment?<br/>What? 1500G???!!!!!<br/>So, Malcolm's done it again. That guy always relies on others to do his dirty work!"));
			npc.Msg(L("Anyway, I can't pay you. I don't have it! It's the bank that has money, not the banker!<br/>Tell him to come get it himself!"));
			npc.Msg(Hide.Name, L("(Intimidated by Bebhinn's rant, you failed to receive any payment for the clothes.)"));

			return HookResult.Break;
		}
		else if (npc.QuestActive(this.Id, "ptj2"))
		{
			npc.FinishQuest(this.Id, "ptj2");

			npc.Player.GiveItem(Flowerpot, 1);
			npc.Notice("You have received Flowerpot to be Delivered from Bebhinn.");

			npc.Msg(L("Oh, give me a break! Go tell Malcolm to put it on my bill and I'll pay him later."));
			npc.Msg(Hide.Name, L("(Keep asking Bebhinn for payment, saying you won't be able to get a reward otherwise.)"));
			npc.Msg(L("OK, OK, I know it's not your fault after all.<br/>Stupid Malcolm, he should have come himself."));
			npc.Msg(L("But, I don't have the money with me now.<br/>So, can you take this to him instead and make sure to tell him this?<br/>I'll definitely pay the bill later."));
			npc.Msg(Hide.Name, L("(Received a small Flowerpot from Bebhinn.)"));

			return HookResult.Break;
		}
		else return HookResult.Continue;
	}
}

private class MalcolmUnimplementedPtjScript : QuestScript
{
	private void Placeholder(NpcScript npc)
	{
		// 508207  Tailor 2 Popo's Skirts (F)
		SetName("General Shop Part-Time Job");
		SetDescription("This job is tailoring and supplying clothes to the General Shop. Today's order is tailoring [2 Popo's skirts (F)], using the materials given for this part-time job. Make sure to bring it to me no earlier than noon. Keep that in mind when delivering the goods, since I can't use them before then.");
		// Objective 1: Make 2 Popo's Skirts (F) (Part-time job)
		// Quest objective metadata: TARGETCOUNT:4:2;TARGETITEM:4:60606;TARGETQUALITY:4:-1000;TGTSKL:2:10001;
		{ // OnQuestStart
			// Grant crafting materials.
			// Should we have QuestObjectiveCraft?
			npc.Player.GiveItem(60419, 2); // Cheap Fabric (Part-Time Job)
			npc.Player.GiveItem(60415, 2); // Cheap Finishing Thread (Part-Time Job)
			npc.Player.GiveItem(60419, 2); // Cheap Fabric (Part-Time Job)
		}
		Collect(60606, 2); // Objective 2: 2 Popo's Skirts (F) (Part-time job)

		// 508210  Tailor 2 Mongo Traveler Suits (F)
		SetName("General Shop Part-Time Job");
		SetDescription("This job is tailoring and supplying clothes to the General Shop. Today's order is tailoring [2 Mongo traveler suits (F)], using the materials given for this part-time job. Make sure to bring it to me no earlier than noon. Keep that in mind when delivering the goods, since I can't use them before then.");
		// Quest objective metadata:  TARGETCOUNT:4:2;TARGETITEM:4:60607;TARGETQUALITY:4:-1000;TGTSKL:2:10001;
		{ // OnQuestStart
			// Grant crafting materials. 
			// Should we have QuestObjectiveCraft?
			npc.Player.GiveItem(60411, 10); // Cheap Silk (Part-Time Job)
			npc.Player.GiveItem(60407, 5);  // Thin Thread Ball (Part-Time Job)
			npc.Player.GiveItem(60407, 5);  // Thin Thread Ball (Part-Time Job)
			npc.Player.GiveItem(60416, 2);  // Common Finishing Thread (Part-Time Job)
			npc.Player.GiveItem(60411, 10); // Cheap Silk (Part-Time Job)
			npc.Player.GiveItem(60407, 5);  // Thin Thread Ball (Part-Time Job)
			npc.Player.GiveItem(60407, 5);  // Thin Thread Ball (Part-Time Job)
		}

		// 508501  Weave Thick Thread Ball
		SetName("General Shop Part-Time Job");
		SetDescription("This job is to make and supply a ball of thick thread to the General Shop. Make [1 ball of thick thread] today and then deliver it. Use wool to spin thick thread with a spinning wheel.");
		// Objective 1: Make 1 Ball of Thick Thread.
		// Quest objective metadata:  TARGETCOUNT:4:2;TARGETITEM:4:60607;TARGETQUALITY:4:-1000;TGTSKL:2:10001;
		Collect(60006, 1); // Objective 2: Collect 1 Ball of Thick Thread.
	}
}