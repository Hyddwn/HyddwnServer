//--- Aura Script -----------------------------------------------------------
// Simon's Clothing Shop Part-Time Job
//--- Description -----------------------------------------------------------
// All quests used by the PTJ, and a script to handle the PTJ via hooks.
//--- Notes -----------------------------------------------------------------
// Definitions for the following base scripts have been improvised,
// update with official script whenever possible:
// * SimonExtDeliveryAeiraEavanPtjBaseScript
// * SimonExtDeliveryWalterAusteynPtjBaseScript
//---------------------------------------------------------------------------

using ItemEntity = Aura.Channel.World.Entities.Item; // Conflicts with QuestScript.Item(...)

public class SimonPtjScript : GeneralScript
{
	const PtjType JobType = PtjType.ClothingShop;

	const int Start = 7;
	//int Report; // Variable - Extracted from player's current PTJ quest data.
	const int Deadline = 19;
	const int PerDay = 5;

	int remaining = PerDay;

	readonly int[] QuestIds = new int[]
	{
		510207, // Basic  Tailor 2 Popo's Skirts (F)
		510208, // Basic  Tailor 2 Wizard Hats
		510209, // Basic  Tailor 2 Hairbands
		510210, // Basic  Tailor 2 Mongo's Traveler Suits (F)
		510211, // Basic  Tailor 2 Mongo's Traveler Suits (M)
		510212, // Basic  Tailor 2 Leather Bandanas
		510238, // Int    Tailor 2 Cores' Healer Dresses
		510239, // Int    Tailor 2 Magic School Uniforms (M)
		510240, // Int    Tailor 2 Mongo's Long Skirts
		510241, // Int    Tailor 2 Cores Ninja Suits (M)
		510242, // Int    Tailor 2 Cores' Healer Gloves
		510243, // Int    Tailor 2 Cores' Healer Suits
		510244, // Int    Tailor 2 Guardian Gloves
		510269, // Adv    Tailor 2 Magic School Uniforms (F)
		510271, // Adv    Tailor 2 Cloth Mails
		510272, // Adv    Tailor 2 Light Leather Mails (F)
		510273, // Adv    Tailor 2 Light Leather Mails (M)
		510275, // Adv    Tailor 2 Lirina's Long Skirts
		510276, // Adv    Tailor 2 Mongo's Hats

		510401, // Basic  Garment Delivery (Nerys)
		510431, // Int    Garment Delivery (Nerys)
		510461, // Adv    Garment Delivery (Nerys)
		510402, // Basic  Garment Delivery (Stewart)
		510432, // Int    Garment Delivery (Stewart)
		510462, // Adv    Garment Delivery (Stewart)
		510403, // Basic  Garment Delivery (Manus)
		510433, // Int    Garment Delivery (Manus)
		510463, // Adv    Garment Delivery (Manus)
		510434, // Int    Garment Delivery (Aeira -> Eavan)
		510464, // Adv    Garment Delivery (Aeira -> Eavan)
		510435, // Int    Garment Delivery (Walter -> Austeyn)
		510465, // Adv    Garment Delivery (Walter -> Austeyn)
	};

	/// <summary>
	/// Precondition: <paramref name="player"/> is already working for Simon.
	/// <para>Returns the <paramref name="player"/>'s report time for their PTJ, or 1 AM if unable to for some reason.</para>
	/// </summary>
	/// <returns>
	/// This <paramref name="player"/>'s report time for their PTJ.
	/// <para>Precondition has not been met if unable to obtain a PTJ. A fallback of 1 AM will be returned.</para>
	/// </returns>
	/// <remarks>
	/// Report time changes depending on the task Simon gives to the player:
	/// 5 PM - Non-basic tailoring or weaving jobs
	/// Noon - Everything else
	/// </remarks>
	private int GetPersonalReportTime(Creature player)
	{
		Quest quest = player.Quests.GetPtjQuest();
		if (quest == null)
		{ // This should not normally happen.
			Log.Error("SimonPtjScript: Player {0} does not have a PTJ report time for Simon. Used fallback of 1 AM.", player.Name);
			return 1; // Fallback
		}
		else return quest.Data.ReportHour;
	}

	public override void Load()
	{
		AddHook("_simon", "after_intro", AfterIntro);
		AddHook("_simon", "before_keywords", BeforeKeywords);
	}

	public async Task<HookResult> AfterIntro(NpcScript npc, params object[] args)
	{
		// Call PTJ method after intro if it's time to report
		if (npc.DoingPtjForNpc() && npc.ErinnHour(GetPersonalReportTime(npc.Player), Deadline))
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
			npc.Msg(L("You seem pretty busy yourself.<br/>You should worry about finishing the work first."));
			return;
		}

		// Check if PTJ is in progress
		if (npc.DoingPtjForNpc())
		{
			var result = npc.GetPtjResult();

			// Check if report time
			if (!npc.ErinnHour(GetPersonalReportTime(npc.Player), Deadline))
			{
				if (result == QuestResult.Perfect)
					npc.Msg(L("You are done already?<br/>I'm a little busy right now. Come see me closer to the deadline, OK?"));
				else
					npc.Msg(L("I trust that the work is going well?<p/>Don't let me down!"));

				return;
			}

			// Report?
			npc.Msg(L("Did you finish today's work?<br/>Then report now and let's wrap it up.<br/>I trust that the results live up to my name."),
				npc.Button(L("Report Now"), "@report"),
				npc.Button(L("Report Later"), "@later")
				);

			if (await npc.Select() != "@report")
			{
				npc.Msg(L("All right. I look forward to your work."));
				return;
			}

			// Nothing done
			if (result == QuestResult.None)
			{
				npc.GiveUpPtj();

				npc.Msg(npc.FavorExpression(), L("Oh my...<br/>If you're going to be like this, don't even think about working for me again."));
				npc.ModifyRelation(0, -Random(3), 0);
			}
			// Low~Perfect result
			else
			{
				npc.Msg(L("You've done a good job with the task I gave you. Thanks.<br/>Well done. Now choose what you need,<br/>and I'll give it to you."),
					npc.Button(L("Report Later"), "@later"),
					npc.PtjReport(result)
					);
				var reply = await npc.Select();

				// Report later
				if (!reply.StartsWith("@reward:"))
				{
					npc.Msg(npc.FavorExpression(), L("Well, it's up to you."));
					return;
				}

				// Complete
				npc.CompletePtj(reply);
				remaining--;

				// Result msg
				if (result == QuestResult.Perfect)
				{
					npc.Msg(npc.FavorExpression(), L("Yes, perfect.<br/>Well done."));
					npc.ModifyRelation(0, Random(3), 0);
				}
				else if (result == QuestResult.Mid)
				{
					npc.Msg(npc.FavorExpression(), L("Hmmm...<br/>Oh well. I'll take it this time.<br/>I don't want to hear you complain about the compensation, though."));
					npc.ModifyRelation(0, Random(1), 0);
				}
				else if (result == QuestResult.Low)
				{
					npc.Msg(npc.FavorExpression(), L("Are you mocking me? Anyone can do THIS!"));
					npc.ModifyRelation(0, -Random(2), 0);
				}
			}
			return;
		}

		// Check if PTJ time
		if (!npc.ErinnHour(Start, Deadline))
		{
			npc.Msg(L("No, no, no. There is no work before or after the designated time."));
			return;
		}

		// Check if not done today and if there are jobs remaining
		if (!npc.CanDoPtj(JobType, remaining))
		{
			npc.Msg(L("I'm done for today.<br/>Come back tomorrow!"));
			return;
		}

		// Offer PTJ
		var randomPtj = npc.RandomPtj(JobType, QuestIds);
		var msg = "";

		if (npc.GetPtjDoneCount(JobType) == 0)
			msg = L("Do you have any experience in this line of work?<br/>The path of a designer is long and challenging.<br/>If you're feeling confident enough, though, I can entrust you with the work.");
		else
			msg = L("Here to work again?");

		var ptjDescTitle = "";
		// Poll third least-significant digit
		if ((randomPtj / 100) % 10 == 2) // 2 -> Tailoring job
			ptjDescTitle = L("Looking for help with crafting items needed for Clothing Shop.");
		else // not 2, fallback -> Delivery job
			ptjDescTitle = L("Looking for help with delivery of goods in Clothing Shop.");

		npc.Msg(msg, npc.PtjDesc(randomPtj,
			L("Simon's Clothing Shop Part-Time Job"),
			ptjDescTitle,
			PerDay, remaining, npc.GetPtjDoneCount(JobType)));

		if (await npc.Select() == "@accept")
		{
			if (npc.GetPtjDoneCount(JobType) == 0)
				npc.Msg(L("Alright. Finish the work and report back to me before the deadline."));
			else
				npc.Msg(L("All right. I'll see you before the deadline. OK?"));

			npc.StartPtj(randomPtj);
		}
		else
		{
			if (npc.GetPtjDoneCount(JobType) == 0)
				npc.Msg(L("Huh? Are you giving up that easily?"));
			else
				npc.Msg(L("Oh well, then. Maybe next time."));
		}
	}
}

public abstract class SimonTailorPtjBaseScript : QuestScript
{
	protected abstract int QuestId { get; }
	protected abstract string LQuestDescription { get; }
	protected abstract int ItemId { get; }
	protected abstract string LCreateObjectiveDescription { get; }
	protected abstract string LCollectObjectiveDescription { get; }
	protected abstract QuestLevel QuestLevel { get; }

	protected abstract int RewardSetId { get; }

	public override void Load()
	{
		SetId(QuestId);
		SetName(L("Clothing Shop Part-Time Job"));
		SetDescription(LQuestDescription);

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.ById);

		SetType(QuestType.Deliver);
		SetPtjType(PtjType.ClothingShop);
		SetLevel(QuestLevel);
		SetHours(start: 7, report: QuestLevel == QuestLevel.Basic ? 12 : 17, deadline: 19);

		AddObjective("ptj1", LCreateObjectiveDescription, 0, 0, 0, Create(ItemId, 2, SkillId.Tailoring));
		AddObjective("ptj2", LCollectObjectiveDescription, 0, 0, 0, Collect(ItemId, 2));

		AddRewards(QuestLevel, RewardSetId);
	}

	/// <remarks>Tailoring PTJs share reward sets to some extent.</remarks>
	private void AddRewards(QuestLevel questLevel, int rewardSetId)
	{
		switch (questLevel)
		{
			case QuestLevel.Basic:
				switch (rewardSetId)
				{
					case 1:
						AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(250));
						AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(600));
						AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(125));
						AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(300));
						AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(50));
						AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(120));

						AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(175));
						AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(655));
						AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(87));
						AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(327));
						AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(35));
						AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(131));

						AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(835));
						AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(160));
						AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(417));
						AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(80));
						AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(167));
						AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(32));

						AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(15003)); // Vest and Pants Set
						AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(112));

						AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(60015, 10)); // Cheap Finishing Thread
						AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(312));

						AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(60015, 5)); // Cheap Finishing Thread
						AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(562));

						AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(60015, 1)); // Cheap Finishing Thread
						AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(762));
						return;

					default:
						Log.Warning("SimonTailorPtjBaseScript: Derived class {0} is using an undefined reward set. Fell back to reward set Basic1.", this.GetType().Name);
						goto case 1;
				}

			case QuestLevel.Int:
				switch (rewardSetId)
				{
					case 1:
						AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(300));
						AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(900));
						AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(150));
						AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(450));
						AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(60));
						AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(180));

						AddReward(2, RewardGroupType.Exp, QuestResult.Perfect, Exp(1200));
						AddReward(2, RewardGroupType.Exp, QuestResult.Perfect, Gold(230));
						AddReward(2, RewardGroupType.Exp, QuestResult.Mid, Exp(600));
						AddReward(2, RewardGroupType.Exp, QuestResult.Mid, Gold(115));
						AddReward(2, RewardGroupType.Exp, QuestResult.Low, Exp(240));
						AddReward(2, RewardGroupType.Exp, QuestResult.Low, Gold(46));

						AddReward(3, RewardGroupType.Item, QuestResult.Perfect, Item(60031)); // Regular Silk Weaving Gloves
						AddReward(3, RewardGroupType.Item, QuestResult.Perfect, Gold(150));

						AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(15003)); // Vest and Pants Set
						AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(450));

						AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(60015, 10)); // Cheap Finishing Thread
						AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(650));

						AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(60015, 5)); // Cheap Finishing Thread
						AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(900));
						return;

					case 2:
						AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(300));
						AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(1000));
						AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(150));
						AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(500));
						AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(60));
						AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(200));

						AddReward(2, RewardGroupType.Exp, QuestResult.Perfect, Exp(1300));
						AddReward(2, RewardGroupType.Exp, QuestResult.Perfect, Gold(250));
						AddReward(2, RewardGroupType.Exp, QuestResult.Mid, Exp(650));
						AddReward(2, RewardGroupType.Exp, QuestResult.Mid, Gold(125));
						AddReward(2, RewardGroupType.Exp, QuestResult.Low, Exp(260));
						AddReward(2, RewardGroupType.Exp, QuestResult.Low, Gold(50));

						AddReward(3, RewardGroupType.Item, QuestResult.Perfect, Item(60031)); // Regular Silk Weaving Gloves
						AddReward(3, RewardGroupType.Item, QuestResult.Perfect, Gold(250));

						AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(15003)); // Vest and Pants Set
						AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(550));

						AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(60015, 10)); // Cheap Finishing Thread
						AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(750));

						AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(60015, 5)); // Cheap Finishing Thread
						AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(1000));
						return;

					default:
						Log.Warning("SimonTailorPtjBaseScript: Derived class {0} is using an undefined reward set. Fell back to reward set Int1.", this.GetType().Name);
						goto case 1;
				}

			case QuestLevel.Adv:
				switch (rewardSetId)
				{
					case 1:
						AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(400));
						AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(1300));
						AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(200));
						AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(650));
						AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(80));
						AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(260));

						AddReward(2, RewardGroupType.Exp, QuestResult.Perfect, Exp(1700));
						AddReward(2, RewardGroupType.Exp, QuestResult.Perfect, Gold(325));
						AddReward(2, RewardGroupType.Exp, QuestResult.Mid, Exp(850));
						AddReward(2, RewardGroupType.Exp, QuestResult.Mid, Gold(163));
						AddReward(2, RewardGroupType.Exp, QuestResult.Low, Exp(340));
						AddReward(2, RewardGroupType.Exp, QuestResult.Low, Gold(65));

						AddReward(3, RewardGroupType.Item, QuestResult.Perfect, Item(19003)); // Tricolor Robe
						AddReward(3, RewardGroupType.Item, QuestResult.Perfect, Gold(125));

						AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(60031)); // Regular Silk Weaving Gloves
						AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(625));

						AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(15003)); // Vest and Pants Set
						AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(925));

						AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(60015, 10)); // Cheap Finishing Thread
						AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(1125));
						return;

					case 2:
						AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(400));
						AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(1500));
						AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(200));
						AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(750));
						AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(80));
						AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(300));

						AddReward(2, RewardGroupType.Exp, QuestResult.Perfect, Exp(1920));
						AddReward(2, RewardGroupType.Exp, QuestResult.Perfect, Gold(360));
						AddReward(2, RewardGroupType.Exp, QuestResult.Mid, Exp(960));
						AddReward(2, RewardGroupType.Exp, QuestResult.Mid, Gold(180));
						AddReward(2, RewardGroupType.Exp, QuestResult.Low, Exp(384));
						AddReward(2, RewardGroupType.Exp, QuestResult.Low, Gold(72));

						AddReward(3, RewardGroupType.Item, QuestResult.Perfect, Item(15023)); // Tork's Hunter Suit (F)
						AddReward(3, RewardGroupType.Item, QuestResult.Perfect, Gold(25));

						AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(19003)); // Tricolor Robe
						AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(325));

						AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(60031)); // Regular Silk Weaving Gloves
						AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(825));

						AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(15003)); // Vest and Pants Set
						AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(1125));

						AddReward(7, RewardGroupType.Scroll, QuestResult.Perfect, QuestScroll(40022)); // Collecting Quest - Deliver 10 Leather Bandanas

						AddReward(8, RewardGroupType.Scroll, QuestResult.Perfect, QuestScroll(40023)); // Collecting Quest - Deliver 5 Common Silk Weaving Gloves

						AddReward(9, RewardGroupType.Scroll, QuestResult.Perfect, QuestScroll(40024)); // Collecting Quest - Deliver 2 Cores Healer Gloves

						AddReward(10, RewardGroupType.Scroll, QuestResult.Perfect, QuestScroll(40025)); // Collecting Quest - Deliver 1 Mongo Hat
						return;

					case 3:
						AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(400));
						AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(1200));
						AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(200));
						AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(600));
						AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(80));
						AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(240));

						AddReward(2, RewardGroupType.Exp, QuestResult.Perfect, Exp(1600));
						AddReward(2, RewardGroupType.Exp, QuestResult.Perfect, Gold(300));
						AddReward(2, RewardGroupType.Exp, QuestResult.Mid, Exp(800));
						AddReward(2, RewardGroupType.Exp, QuestResult.Mid, Gold(150));
						AddReward(2, RewardGroupType.Exp, QuestResult.Low, Exp(320));
						AddReward(2, RewardGroupType.Exp, QuestResult.Low, Gold(60));

						AddReward(3, RewardGroupType.Item, QuestResult.Perfect, Item(19003)); // Tricolor Robe
						AddReward(3, RewardGroupType.Item, QuestResult.Perfect, Gold(25));

						AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(60031)); // Regular Silk Weaving Gloves
						AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(525));

						AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(15003)); // Vest and Pants Set
						AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(825));

						AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(60015, 10)); // Cheap Finishing Thread
						AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(1025));

						AddReward(7, RewardGroupType.Scroll, QuestResult.Perfect, QuestScroll(40022)); // Collecting Quest - Deliver 10 Leather Bandanas

						AddReward(8, RewardGroupType.Scroll, QuestResult.Perfect, QuestScroll(40023)); // Collecting Quest - Deliver 5 Common Silk Weaving Gloves

						AddReward(9, RewardGroupType.Scroll, QuestResult.Perfect, QuestScroll(40024)); // Collecting Quest - Deliver 2 Cores Healer Gloves

						AddReward(10, RewardGroupType.Scroll, QuestResult.Perfect, QuestScroll(40025)); // Collecting Quest - Deliver 1 Mongo Hat

						if (IsEnabled("CollectionBooks"))
							AddReward(11, RewardGroupType.Item, QuestResult.Perfect, Item(1501)); // Collection Book - Fashion Item - Hat
						return;

					case 4:
						AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(400));
						AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(1300));
						AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(200));
						AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(650));
						AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(80));
						AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(260));

						AddReward(2, RewardGroupType.Exp, QuestResult.Perfect, Exp(1700));
						AddReward(2, RewardGroupType.Exp, QuestResult.Perfect, Gold(325));
						AddReward(2, RewardGroupType.Exp, QuestResult.Mid, Exp(850));
						AddReward(2, RewardGroupType.Exp, QuestResult.Mid, Gold(163));
						AddReward(2, RewardGroupType.Exp, QuestResult.Low, Exp(340));
						AddReward(2, RewardGroupType.Exp, QuestResult.Low, Gold(65));

						AddReward(3, RewardGroupType.Item, QuestResult.Perfect, Item(19003)); // Tricolor Robe
						AddReward(3, RewardGroupType.Item, QuestResult.Perfect, Gold(125));

						AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(60031)); // Regular Silk Weaving Gloves
						AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(625));

						AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(15003)); // Vest and Pants Set
						AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(925));

						AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(60015, 10)); // Cheap Finishing Thread
						AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(1125));

						AddReward(7, RewardGroupType.Scroll, QuestResult.Perfect, QuestScroll(40022)); // Collecting Quest - Deliver 10 Leather Bandanas

						AddReward(8, RewardGroupType.Scroll, QuestResult.Perfect, QuestScroll(40023)); // Collecting Quest - Deliver 5 Common Silk Weaving Gloves

						AddReward(9, RewardGroupType.Scroll, QuestResult.Perfect, QuestScroll(40024)); // Collecting Quest - Deliver 2 Cores Healer Gloves

						AddReward(10, RewardGroupType.Scroll, QuestResult.Perfect, QuestScroll(40025)); // Collecting Quest - Deliver 1 Mongo Hat

						if (IsEnabled("CollectionBooks"))
							AddReward(11, RewardGroupType.Item, QuestResult.Perfect, Item(1501)); // Collection Book - Fashion Item - Hat
						return;

					default:
						Log.Warning("SimonTailorPtjBaseScript: Derived class {0} is using an undefined reward set. Fell back to reward set Adv1.", this.GetType().Name);
						goto case 1;
				}

			default:
				Log.Warning("SimonTailorPtjBaseScript: Unspecified quest level for derived class {0}. Retrying as basic level.", this.GetType().Name);
				goto case QuestLevel.Basic;
		}
	}
}

public class SimonTailorPoposSkirtBasicPtjScript : SimonTailorPtjBaseScript
{
	protected override int QuestId { get { return 510207; } }
	protected override string LQuestDescription { get { return L("This job is tailoring and supplying clothes to the Clothing Shop. Today's order is tailoring [2 Popo's Skirts], using the materials given for this part-time job. Deadline kicks in at noon. Be careful not to deliver them before the deadline since the final work doesn't begin til then."); } }
	protected override int ItemId { get { return 60606; } }
	protected override string LCreateObjectiveDescription { get { return L("Make 2 Popo's Skirts (Part-Time Job)"); } }
	protected override string LCollectObjectiveDescription { get { return L("2 Popo's Skirts (Part-Time Job)"); } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Basic; } }

	protected override int RewardSetId { get { return 1; } }

	public override void OnReceive(Creature creature)
	{
		creature.GiveItem(ItemEntity.CreatePattern(60600, 10106, 5)); // Apprentice Sewing Pattern - Popo's Skirt
		creature.GiveItem(60419, 2); // Cheap Fabric (Part-Time Job)
		creature.GiveItem(60415, 2); // Cheap Finishing Thread (Part-Time Job)
		creature.GiveItem(60419, 2); // Cheap Fabric (Part-Time Job)
	}
}

public class SimonTailorWizardHatBasicPtjScript : SimonTailorPtjBaseScript
{
	protected override int QuestId { get { return 510208; } }
	protected override string LQuestDescription { get { return L("This job is tailoring and supplying clothes to the Clothing Shop. Today's order is tailoring [2 Wizard Hats], using the materials given for this part-time job. Deadline kicks in at noon. Be careful not to deliver them before the deadline since the final work doesn't begin til then."); } }
	protected override int ItemId { get { return 60612; } }
	protected override string LCreateObjectiveDescription { get { return L("Make 2 Wizard Hats (Part-Time Job)"); } }
	protected override string LCollectObjectiveDescription { get { return L("2 Wizard Hats (Part-Time Job)"); } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Basic; } }

	protected override int RewardSetId { get { return 1; } }

	public override void OnReceive(Creature creature)
	{
		creature.GiveItem(ItemEntity.CreatePattern(60600, 10112, 10)); // Apprentice Sewing Pattern - Wizard Hat
		creature.GiveItem(60424, 5); // Common Leather (Part-Time Job)
		creature.GiveItem(60415, 2); // Cheap Finishing Thread (Part-Time Job)
		creature.GiveItem(60424, 5); // Common Leather (Part-Time Job)
	}
}

public class SimonTailorHairbandBasicPtjScript : SimonTailorPtjBaseScript
{
	protected override int QuestId { get { return 510209; } }
	protected override string LQuestDescription { get { return L("This job is tailoring and supplying clothes to the Clothing Shop. Today's order is tailoring [2 Hairbands], using the materials given for this part-time job. Deadline kicks in at noon. Be careful not to deliver them before the deadline since the final work doesn't begin til then."); } }
	protected override int ItemId { get { return 60614; } }
	protected override string LCreateObjectiveDescription { get { return L("Make 2 Hairbands (Part-Time Job)"); } }
	protected override string LCollectObjectiveDescription { get { return L("2 Hairbands (Part-Time Job)"); } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Basic; } }

	protected override int RewardSetId { get { return 1; } }

	public override void OnReceive(Creature creature)
	{
		creature.GiveItem(ItemEntity.CreatePattern(60600, 10114, 5)); // Apprentice Sewing Pattern - Hairband
		creature.GiveItem(60415, 2); // Cheap Finishing Thread (Part-Time Job)
		creature.GiveItem(60419, 5); // Cheap Fabric (Part-Time Job)
	}
}

public class SimonTailorMongosTravelerSuitFBasicPtjScript : SimonTailorPtjBaseScript
{
	protected override int QuestId { get { return 510210; } }
	protected override string LQuestDescription { get { return L("This job is tailoring and supplying clothes to the Clothing Shop. Today's order is tailoring [2 Mongo's Traveler Suits (F)], using the materials given for this part-time job. Deadline kicks in at noon. Be careful not to deliver them before the deadline since the final work doesn't begin til then."); } }
	protected override int ItemId { get { return 60607; } }
	protected override string LCreateObjectiveDescription { get { return L("Make 2 Mongo's Traveler Suits (F) (Part-Time Job)"); } }
	protected override string LCollectObjectiveDescription { get { return L("2 Mongo's Traveler Suits (F) (Part-Time Job)"); } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Basic; } }

	protected override int RewardSetId { get { return 1; } }

	public override void OnReceive(Creature creature)
	{
		creature.GiveItem(ItemEntity.CreatePattern(60600, 10107, 40)); // Apprentice Sewing Pattern - Mongo Traveler Suit (F)
		creature.GiveItem(60411, 10); // Cheap Silk (Part-Time Job)
		creature.GiveItem(60407, 5);  // Thin Thread Ball (Part-Time Job)
		creature.GiveItem(60407, 5);  // Thin Thread Ball (Part-Time Job)
		creature.GiveItem(60416, 2);  // Common Finishing Thread (Part-Time Job)
		creature.GiveItem(60411, 10); // Cheap Silk (Part-Time Job)
		creature.GiveItem(60407, 5);  // Thin Thread Ball (Part-Time Job)
		creature.GiveItem(60407, 5);  // Thin Thread Ball (Part-Time Job)
	}
}

public class SimonTailorMongosTravelerSuitMBasicPtjScript : SimonTailorPtjBaseScript
{
	protected override int QuestId { get { return 510211; } }
	protected override string LQuestDescription { get { return L("This job is tailoring and supplying clothes to the Clothing Shop. Today's order is tailoring [2 Mongo's Traveler Suits (M)], using the materials given for this part-time job. Deadline kicks in at noon. Be careful not to deliver them before the deadline since the final work doesn't begin til then."); } }
	protected override int ItemId { get { return 60608; } }
	protected override string LCreateObjectiveDescription { get { return L("Make 2 Mongo's Traveler Suits (M) (Part-Time Job)"); } }
	protected override string LCollectObjectiveDescription { get { return L("2 Mongo's Traveler Suits (M) (Part-Time Job)"); } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Basic; } }

	protected override int RewardSetId { get { return 1; } }

	public override void OnReceive(Creature creature)
	{
		creature.GiveItem(ItemEntity.CreatePattern(60600, 10108, 40)); // Apprentice Sewing Pattern - Mongo Traveler Suit (M)
		creature.GiveItem(60411, 10); // Cheap Silk (Part-Time Job)
		creature.GiveItem(60407, 5);  // Thin Thread Ball (Part-Time Job)
		creature.GiveItem(60407, 5);  // Thin Thread Ball (Part-Time Job)
		creature.GiveItem(60416, 2);  // Common Finishing Thread (Part-Time Job)
		creature.GiveItem(60411, 10); // Cheap Silk (Part-Time Job)
		creature.GiveItem(60407, 5);  // Thin Thread Ball (Part-Time Job)
		creature.GiveItem(60407, 5);  // Thin Thread Ball (Part-Time Job)
	}
}

public class SimonTailorLeatherBandanaBasicPtjScript : SimonTailorPtjBaseScript
{
	protected override int QuestId { get { return 510212; } }
	protected override string LQuestDescription { get { return L("This job is tailoring and supplying clothes to the Clothing Shop. Today's order is tailoring [2 Leather Bandanas], using the materials given for this part-time job. Deadline kicks in at noon. Be careful not to deliver them before the deadline since the final work doesn't begin til then."); } }
	protected override int ItemId { get { return 60613; } }
	protected override string LCreateObjectiveDescription { get { return L("Make 2 Leather Bandanas (Part-Time Job)"); } }
	protected override string LCollectObjectiveDescription { get { return L("2 Leather Bandanas (Part-Time Job)"); } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Basic; } }

	protected override int RewardSetId { get { return 1; } }

	public override void OnReceive(Creature creature)
	{
		creature.GiveItem(ItemEntity.CreatePattern(60600, 10113, 15)); // Apprentice Sewing Pattern - Leather Bandana
		creature.GiveItem(60419, 3); // Cheap Fabric (Part-Time Job)
		creature.GiveItem(60424, 3); // Common Leather (Part-Time Job)
		creature.GiveItem(60416, 2); // Common Finishing Thread (Part-Time Job)
		creature.GiveItem(60419, 3); // Cheap Fabric (Part-Time Job)
		creature.GiveItem(60424, 3); // Common Leather (Part-Time Job)
	}
}

public class SimonTailorCoresHealerDressIntPtjScript : SimonTailorPtjBaseScript
{
	protected override int QuestId { get { return 510238; } }
	protected override string LQuestDescription { get { return L("This job is tailoring and supplying clothes to the Clothing Shop. Today's order is tailoring [2 Cores' Healer Dresses], using the materials given for this part-time job. Deadline kicks in at 5 PM. Be careful not to deliver them before the deadline since the final work doesn't begin til then."); } }
	protected override int ItemId { get { return 60601; } }
	protected override string LCreateObjectiveDescription { get { return L("Make 2 Cores' Healer Dresses (Part-Time Job)"); } }
	protected override string LCollectObjectiveDescription { get { return L("2 Cores' Healer Dresses (Part-Time Job)"); } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Int; } }

	protected override int RewardSetId { get { return 1; } }

	public override void OnReceive(Creature creature)
	{
		creature.GiveItem(ItemEntity.CreatePattern(60600, 10101, 30)); // Apprentice Sewing Pattern - Cores' Healer Dress
		creature.GiveItem(60419, 6); // Cheap Fabric (Part-Time Job)
		creature.GiveItem(60411, 6); // Cheap Silk (Part-Time Job)
		creature.GiveItem(60416, 2); // Common Finishing Thread (Part-Time Job)
		creature.GiveItem(60419, 6); // Cheap Fabric (Part-Time Job)
		creature.GiveItem(60411, 6); // Cheap Silk (Part-Time Job)
	}
}

public class SimonTailorMagicSchoolUniformMIntPtjScript : SimonTailorPtjBaseScript
{
	protected override int QuestId { get { return 510239; } }
	protected override string LQuestDescription { get { return L("This job is tailoring and supplying clothes to the Clothing Shop. Today's order is tailoring [2 Magic School Uniforms (M)], using the materials given for this part-time job. Deadline kicks in at 5 PM. Be careful not to deliver them before the deadline since the final work doesn't begin til then."); } }
	protected override int ItemId { get { return 60602; } }
	protected override string LCreateObjectiveDescription { get { return L("Make 2 Magic School Uniforms (M) (Part-Time Job)"); } }
	protected override string LCollectObjectiveDescription { get { return L("2 Magic School Uniforms (M) (Part-Time Job)"); } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Int; } }

	protected override int RewardSetId { get { return 1; } }

	public override void OnReceive(Creature creature)
	{
		creature.GiveItem(ItemEntity.CreatePattern(60600, 10102, 10)); // Apprentice Sewing Pattern - Magic School Uniform (M)
		creature.GiveItem(60419, 3); // Cheap Fabric (Part-Time Job)
		creature.GiveItem(60412, 3); // Common Silk (Part-Time Job)
		creature.GiveItem(60428, 3); // Common Leather Strap (Part-Time Job)
		creature.GiveItem(60417, 2); // Fine Finishing Thread (Part-Time Job)
	}
}

public class SimonTailorMongosLongSkirtIntPtjScript : SimonTailorPtjBaseScript
{
	protected override int QuestId { get { return 510240; } }
	protected override string LQuestDescription { get { return L("This job is tailoring and supplying clothes to the Clothing Shop. Today's order is tailoring [2 Mongo's Long Skirts], using the materials given for this part-time job. Deadline kicks in at 5 PM. Be careful not to deliver them before the deadline since the final work doesn't begin til then."); } }
	protected override int ItemId { get { return 60615; } }
	protected override string LCreateObjectiveDescription { get { return L("Make 2 Mongo's Long Skirts (Part-Time Job)"); } }
	protected override string LCollectObjectiveDescription { get { return L("2 Mongo's Long Skirts (Part-Time Job)"); } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Int; } }

	protected override int RewardSetId { get { return 1; } }

	public override void OnReceive(Creature creature)
	{
		creature.GiveItem(ItemEntity.CreatePattern(60600, 10115, 25)); // Apprentice Sewing Pattern - Mongo's Long Skirt
		creature.GiveItem(60419, 3); // Cheap Fabric (Part-Time Job)
		creature.GiveItem(60411, 3); // Cheap Silk (Part-Time Job)
		creature.GiveItem(60415, 2); // Cheap Finishing Thread (Part-Time Job)
		creature.GiveItem(60419, 3); // Cheap Fabric (Part-Time Job)
		creature.GiveItem(60411, 3); // Cheap Silk (Part-Time Job)
	}
}

public class SimonTailorCoresNinjaSuitMIntPtjScript : SimonTailorPtjBaseScript
{
	protected override int QuestId { get { return 510241; } }
	protected override string LQuestDescription { get { return L("This job is tailoring and supplying clothes to the Clothing Shop. Today's order is tailoring [2 Cores Ninja Suits (M)], using the materials given for this part-time job. Deadline kicks in at 5 PM. Be careful not to deliver them before the deadline since the final work doesn't begin til then."); } }
	protected override int ItemId { get { return 60618; } }
	protected override string LCreateObjectiveDescription { get { return L("Make 2 Cores Ninja Suits (M) (Part-Time Job)"); } }
	protected override string LCollectObjectiveDescription { get { return L("2 Cores Ninja Suits (M) (Part-Time Job)"); } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Int; } }

	protected override int RewardSetId { get { return 1; } }

	public override void OnReceive(Creature creature)
	{
		creature.GiveItem(ItemEntity.CreatePattern(60600, 10118, 25)); // Apprentice Sewing Pattern - Cores Ninja Suit (M)
		creature.GiveItem(60420, 7); // Common Fabric (Part-Time Job)
		creature.GiveItem(60412, 7); // Common Silk (Part-Time Job)
		creature.GiveItem(60406, 5); // Thick Thread Ball (Part-Time Job)
		creature.GiveItem(60406, 5); // Thick Thread Ball (Part-Time Job)
		creature.GiveItem(60406, 5); // Thick Thread Ball (Part-Time Job)
		creature.GiveItem(60406, 5); // Thick Thread Ball (Part-Time Job)
		creature.GiveItem(60415, 2); // Cheap Finishing Thread (Part-Time Job)
		creature.GiveItem(60420, 7); // Common Fabric (Part-Time Job)
		creature.GiveItem(60412, 7); // Common Silk (Part-Time Job)
		creature.GiveItem(60406, 5); // Thick Thread Ball (Part-Time Job)
		creature.GiveItem(60406, 5); // Thick Thread Ball (Part-Time Job)
		creature.GiveItem(60406, 5); // Thick Thread Ball (Part-Time Job)
		creature.GiveItem(60406, 5); // Thick Thread Ball (Part-Time Job)
		creature.GiveItem(60406, 2); // Thick Thread Ball (Part-Time Job)
	}
}

public class SimonTailorCoresHealerGlovesIntPtjScript : SimonTailorPtjBaseScript
{
	protected override int QuestId { get { return 510242; } }
	protected override string LQuestDescription { get { return L("This job is tailoring and supplying clothes to the Clothing Shop. Today's order is tailoring [2 Cores' Healer Gloves], using the materials given for this part-time job. Deadline kicks in at 5 PM. Be careful not to deliver them before the deadline since the final work doesn't begin til then."); } }
	protected override int ItemId { get { return 60604; } }
	protected override string LCreateObjectiveDescription { get { return L("Make 2 Cores' Healer Gloves (Part-Time Job)"); } }
	protected override string LCollectObjectiveDescription { get { return L("2 Cores' Healer Gloves (Part-Time Job)"); } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Int; } }

	protected override int RewardSetId { get { return 2; } }

	public override void OnReceive(Creature creature)
	{
		creature.GiveItem(ItemEntity.CreatePattern(60600, 10104, 20)); // Apprentice Sewing Pattern - Cores' Healer Gloves
		creature.GiveItem(60420, 6); // Common Fabric (Part-Time Job)
		creature.GiveItem(60412, 6); // Common Silk (Part-Time Job)
		creature.GiveItem(60404, 3); // Braid (Part-Time Job)
		creature.GiveItem(60404, 3); // Braid (Part-Time Job)
		creature.GiveItem(60417, 2); // Fine Finishing Thread (Part-Time Job)
		creature.GiveItem(60420, 6); // Common Fabric (Part-Time Job)
		creature.GiveItem(60412, 6); // Common Silk (Part-Time Job)
		creature.GiveItem(60404, 3); // Braid (Part-Time Job)
		creature.GiveItem(60404, 3); // Braid (Part-Time Job)
	}
}

public class SimonTailorCoresHealerSuitIntPtjScript : SimonTailorPtjBaseScript
{
	protected override int QuestId { get { return 510243; } }
	protected override string LQuestDescription { get { return L("This job is tailoring and supplying clothes to the Clothing Shop. Today's order is tailoring [2 Cores' Healer Suits], using the materials given for this part-time job. Deadline kicks in at 5 PM. Be careful not to deliver them before the deadline since the final work doesn't begin til then."); } }
	protected override int ItemId { get { return 60610; } }
	protected override string LCreateObjectiveDescription { get { return L("Make 2 Cores' Healer Suits (Part-Time Job)"); } }
	protected override string LCollectObjectiveDescription { get { return L("2 Cores' Healer Suits (Part-Time Job)"); } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Int; } }

	protected override int RewardSetId { get { return 2; } }

	public override void OnReceive(Creature creature)
	{
		creature.GiveItem(ItemEntity.CreatePattern(60600, 10110, 30)); // Apprentice Sewing Pattern - Cores' Healer Suit
		creature.GiveItem(60420, 5); // Common Fabric (Part-Time Job)
		creature.GiveItem(60419, 5); // Cheap Fabric (Part-Time Job)
		creature.GiveItem(60427, 5); // Cheap Leather Strap (Part-Time Job)
		creature.GiveItem(60418, 2); // Finest Finishing Thread (Part-Time Job)
		creature.GiveItem(60420, 5); // Common Fabric (Part-Time Job)
		creature.GiveItem(60419, 5); // Cheap Fabric (Part-Time Job)
		creature.GiveItem(60427, 5); // Cheap Leather Strap (Part-Time Job)
	}
}

public class SimonTailorGuardianGloveIntPtjScript : SimonTailorPtjBaseScript
{
	protected override int QuestId { get { return 510244; } }
	protected override string LQuestDescription { get { return L("This job is tailoring and supplying clothes to the Clothing Shop. Today's order is tailoring [2 Guardian Gloves], using the materials given for this part-time job. Deadline kicks in at 5 PM. Be careful not to deliver them before the deadline since the final work doesn't begin til then."); } }
	protected override int ItemId { get { return 60611; } }
	protected override string LCreateObjectiveDescription { get { return L("Make 2 Guardian Gloves (Part-Time Job)"); } }
	protected override string LCollectObjectiveDescription { get { return L("2 Guardian Gloves (Part-Time Job)"); } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Int; } }

	protected override int RewardSetId { get { return 2; } }

	public override void OnReceive(Creature creature)
	{
		creature.GiveItem(ItemEntity.CreatePattern(60600, 10111, 40)); // Apprentice Sewing Pattern - Guardian Gloves
		creature.GiveItem(60424, 10); // Common Leather (Part-Time Job)
		creature.GiveItem(60406, 5);  // Thick Thread Ball (Part-Time Job)
		creature.GiveItem(60406, 5);  // Thick Thread Ball (Part-Time Job)
		creature.GiveItem(60417, 2);  // Fine Finishing Thread (Part-Time Job)
		creature.GiveItem(60418, 2);  // Finest Finishing Thread (Part-Time Job)
		creature.GiveItem(60424, 10); // Common Leather (Part-Time Job)
		creature.GiveItem(60406, 5);  // Thick Thread Ball (Part-Time Job)
		creature.GiveItem(60406, 5);  // Thick Thread Ball (Part-Time Job)
	}
}

public class SimonTailorMagicSchoolUniformFAdvPtjScript : SimonTailorPtjBaseScript
{
	protected override int QuestId { get { return 510269; } }
	protected override string LQuestDescription { get { return L("This job is tailoring and supplying clothes to the Clothing Shop. Today's order is tailoring [2 Magic School Uniforms (F)], using the materials given for this part-time job. Deadline kicks in at 5 PM. Be careful not to deliver them before the deadline since the final work doesn't begin til then."); } }
	protected override int ItemId { get { return 60603; } }
	protected override string LCreateObjectiveDescription { get { return L("Make 2 Magic School Uniforms (F) (Part-Time Job)"); } }
	protected override string LCollectObjectiveDescription { get { return L("2 Magic School Uniforms (F) (Part-Time Job)"); } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Adv; } }

	protected override int RewardSetId { get { return 1; } }

	public override void OnReceive(Creature creature)
	{
		creature.GiveItem(ItemEntity.CreatePattern(60600, 10103, 20)); // Apprentice Sewing Pattern - Magic School Uniform (F)
		creature.GiveItem(60422, 10); // Finest Fabric (Part-Time Job)
		creature.GiveItem(60422, 6);  // Finest Fabric (Part-Time Job)
		creature.GiveItem(60412, 8);  // Common Silk (Part-Time Job)
		creature.GiveItem(60417, 2);  // Fine Finishing Thread (Part-Time Job)
	}
}

public class SimonTailorClothMailsAdvPtjScript : SimonTailorPtjBaseScript
{
	protected override int QuestId { get { return 510271; } }
	protected override string LQuestDescription { get { return L("This job is tailoring and supplying clothes to the Clothing Shop. Today's order is tailoring [2 Cloth Mails], using the materials given for this part-time job. Deadline kicks in at 5 PM. Be careful not to deliver them before the deadline since the final work doesn't begin til then."); } }
	protected override int ItemId { get { return 60609; } }
	protected override string LCreateObjectiveDescription { get { return L("Make 2 Cloth Mails (Part-Time Job)"); } }
	protected override string LCollectObjectiveDescription { get { return L("2 Cloth Mails (Part-Time Job)"); } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Adv; } }

	protected override int RewardSetId { get { return 2; } }

	public override void OnReceive(Creature creature)
	{
		creature.GiveItem(ItemEntity.CreatePattern(60600, 10109, 40)); // Apprentice Sewing Pattern - Cloth Mail
		creature.GiveItem(60422, 10); // Finest Fabric (Part-Time Job)
		creature.GiveItem(60422, 10); // Finest Fabric (Part-Time Job)
		creature.GiveItem(60407, 10); // Thin Thread Ball (Part-Time Job)
		creature.GiveItem(60404, 5);  // Braid (Part-Time Job)
		creature.GiveItem(60404, 5);  // Braid (Part-Time Job)
		creature.GiveItem(60418, 2);  // Finest Finishing Thread (Part-Time Job)
		creature.GiveItem(60422, 10); // Finest Fabric (Part-Time Job)
		creature.GiveItem(60422, 10); // Finest Fabric (Part-Time Job)
		creature.GiveItem(60407, 10); // Thin Thread Ball (Part-Time Job)
		creature.GiveItem(60404, 5);  // Braid (Part-Time Job)
		creature.GiveItem(60404, 5);  // Braid (Part-Time Job)
	}
}

public class SimonTailorLightLeatherMailFAdvPtjScript : SimonTailorPtjBaseScript
{
	protected override int QuestId { get { return 510272; } }
	protected override string LQuestDescription { get { return L("This job is tailoring and supplying clothes to the Clothing Shop. Today's order is tailoring [2 Light Leather Mails (F)], using the materials given for this part-time job. Deadline kicks in at 5 PM. Be careful not to deliver them before the deadline since the final work doesn't begin til then."); } }
	protected override int ItemId { get { return 60616; } }
	protected override string LCreateObjectiveDescription { get { return L("Make 2 Light Leather Mails (F) (Part-Time Job)"); } }
	protected override string LCollectObjectiveDescription { get { return L("2 Light Leather Mails (F) (Part-Time Job)"); } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Adv; } }

	protected override int RewardSetId { get { return 2; } }

	public override void OnReceive(Creature creature)
	{
		creature.GiveItem(ItemEntity.CreatePattern(60600, 10116, 30)); // Apprentice Sewing Pattern - Light Leather Mail (F)
		creature.GiveItem(60425, 9);  // Fine Leather (Part-Time Job)
		creature.GiveItem(60413, 9);  // Fine Silk (Part-Time Job)
		creature.GiveItem(60428, 9);  // Common Leather Strap (Part-Time Job)
		creature.GiveItem(60417, 2);  // Fine Finishing Thread (Part-Time Job)
		creature.GiveItem(60404, 2);  // Braid (Part-Time Job)
		creature.GiveItem(60425, 9);  // Fine Leather (Part-Time Job)
		creature.GiveItem(60413, 9);  // Fine Silk (Part-Time Job)
		creature.GiveItem(60428, 9);  // Common Leather Strap (Part-Time Job)
	}
}

public class SimonTailorLightLeatherMailMAdvPtjScript : SimonTailorPtjBaseScript
{
	protected override int QuestId { get { return 510273; } }
	protected override string LQuestDescription { get { return L("This job is tailoring and supplying clothes to the Clothing Shop. Today's order is tailoring [2 Light Leather Mails (M)], using the materials given for this part-time job. Deadline kicks in at 5 PM. Be careful not to deliver them before the deadline since the final work doesn't begin til then."); } }
	protected override int ItemId { get { return 60620; } }
	protected override string LCreateObjectiveDescription { get { return L("Make 2 Light Leather Mails (M) (Part-Time Job)"); } }
	protected override string LCollectObjectiveDescription { get { return L("2 Light Leather Mails (M) (Part-Time Job)"); } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Adv; } }

	protected override int RewardSetId { get { return 2; } }

	public override void OnReceive(Creature creature)
	{
		creature.GiveItem(ItemEntity.CreatePattern(60600, 10120, 30)); // Apprentice Sewing Pattern - Light Leather Mail (M)
		creature.GiveItem(60425, 10); // Fine Leather (Part-Time Job)
		creature.GiveItem(60412, 10); // Common Silk (Part-Time Job)
		creature.GiveItem(60428, 10); // Common Leather Strap (Part-Time Job)
		creature.GiveItem(60417, 2);  // Fine Finishing Thread (Part-Time Job)
		creature.GiveItem(60404, 2);  // Braid (Part-Time Job)
		creature.GiveItem(60425, 10); // Fine Leather (Part-Time Job)
		creature.GiveItem(60412, 10); // Common Silk (Part-Time Job)
		creature.GiveItem(60428, 10); // Common Leather Strap (Part-Time Job)
	}
}

public class SimonTailorLirinaLongSkirtAdvPtjScript : SimonTailorPtjBaseScript
{
	protected override int QuestId { get { return 510275; } }
	protected override string LQuestDescription { get { return L("This job is tailoring and supplying clothes to the Clothing Shop. Today's order is tailoring [2 Lirina's Long Skirts], using the materials given for this part-time job. Deadline kicks in at 5 PM. Be careful not to deliver them before the deadline since the final work doesn't begin til then."); } }
	protected override int ItemId { get { return 60617; } }
	protected override string LCreateObjectiveDescription { get { return L("Make 2 Lirina's Long Skirts (Part-Time Job)"); } }
	protected override string LCollectObjectiveDescription { get { return L("2 Lirina's Long Skirts (Part-Time Job)"); } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Adv; } }

	protected override int RewardSetId { get { return 3; } }

	public override void OnReceive(Creature creature)
	{
		creature.GiveItem(ItemEntity.CreatePattern(60600, 10117, 20)); // Apprentice Sewing Pattern - Lirina's Long Skirt
		creature.GiveItem(60413, 5);  // Fine Silk (Part-Time Job)
		creature.GiveItem(60407, 10); // Thin Thread Ball (Part-Time Job)
		creature.GiveItem(60417, 2);  // Fine Finishing Thread (Part-Time Job)
		creature.GiveItem(60413, 5);  // Fine Silk (Part-Time Job)
		creature.GiveItem(60407, 10); // Thin Thread Ball (Part-Time Job)
	}
}

public class SimonTailorMongoHatsAdvPtjScript : SimonTailorPtjBaseScript
{
	protected override int QuestId { get { return 510276; } }
	protected override string LQuestDescription { get { return L("This job is tailoring and supplying clothes to the Clothing Shop. Today's order is tailoring [2 Mongo's Hats], using the materials given for this part-time job. Deadline kicks in at 5 PM. Be careful not to deliver them before the deadline since the final work doesn't begin til then."); } }
	protected override int ItemId { get { return 60605; } }
	protected override string LCreateObjectiveDescription { get { return L("Make 2 Mongo's Hats (Part-Time Job)"); } }
	protected override string LCollectObjectiveDescription { get { return L("2 Mongo's Hats (Part-Time Job)"); } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Adv; } }

	protected override int RewardSetId { get { return 4; } }

	public override void OnReceive(Creature creature)
	{
		creature.GiveItem(ItemEntity.CreatePattern(60600, 10105, 15)); // Apprentice Sewing Pattern - Mongo's Hat
		creature.GiveItem(60422, 5); // Finest Fabric (Part-Time Job)
		creature.GiveItem(60412, 5); // Common Silk (Part-Time Job)
		creature.GiveItem(60418, 2); // Finest Finishing Thread (Part-Time Job)
		creature.GiveItem(60422, 5); // Finest Fabric (Part-Time Job)
		creature.GiveItem(60412, 5); // Common Silk (Part-Time Job)
	}
}

public abstract class SimonDeliveryPtjBaseScript : QuestScript
{
	protected abstract int QuestId { get; }
	protected abstract string LQuestDescription { get; }
	protected abstract QuestLevel QuestLevel { get; }
	protected abstract string LObjectiveDescription { get; }
	protected abstract int ItemId { get; }
	protected abstract string LItemNotice { get; }
	protected abstract string NpcIdent { get; }

	protected abstract Task OnFinish(NpcScript npc);

	public override void Load()
	{
		SetId(QuestId);
		SetName(L("Clothing Shop Part-Time Job"));
		SetDescription(LQuestDescription);

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.ById);

		SetType(QuestType.Deliver);
		SetPtjType(PtjType.ClothingShop);
		SetLevel(QuestLevel);
		SetHours(start: 7, report: 12, deadline: 19);

		AddObjective("ptj1", LObjectiveDescription, 0, 0, 0, Deliver(ItemId, NpcIdent));

		AddHook(NpcIdent, "after_intro", AfterIntro);

		AddRewards(QuestLevel);
	}

	public virtual async Task<HookResult> AfterIntro(NpcScript npc, params object[] args)
	{
		if (!npc.QuestActive(this.Id, "ptj1"))
			return HookResult.Continue;

		npc.FinishQuest(this.Id, "ptj1");

		npc.Player.RemoveItem(ItemId, 1);
		npc.Notice(LItemNotice);

		await OnFinish(npc);

		return HookResult.Break;
	}

	/// <remarks>Rewards are common among all non-extended item delivery PTJs.</remarks>
	protected virtual void AddRewards(QuestLevel questLevel)
	{
		switch (questLevel)
		{
			case QuestLevel.Basic:
				AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(190));
				AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(150));
				AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(95));
				AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(75));
				AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(38));
				AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(30));

				AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(65));
				AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(260));
				AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(32));
				AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(130));
				AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(13));
				AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(52));

				AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(310));
				AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(60));
				AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(155));
				AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(30));
				AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(62));
				AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(12));

				AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(60015, 5)); // Cheap Finishing Thread
				AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(68));

				AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(60015, 1)); // Cheap Finishing Thread
				AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(268));

				AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(60006, 5)); // Thick Thread Ball
				AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(288));
				return;

			case QuestLevel.Int:
				AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(280));
				AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(250));
				AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(140));
				AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(125));
				AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(56));
				AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(50));

				AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(95));
				AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(390));
				AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(47));
				AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(195));
				AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(19));
				AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(78));

				AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(490));
				AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(90));
				AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(245));
				AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(45));
				AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(98));
				AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(18));

				AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(60015, 5)); // Cheap Finishing Thread
				AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(235));

				AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(60015, 1)); // Cheap Finishing Thread
				AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(435));

				AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(60006, 5)); // Thick Thread Ball
				AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(455));
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

			default:
				Log.Warning("SimonDeliveryPtjBaseScript: Unspecified quest level for derived class {0}. Fell back to non-extended basic rewards.", this.GetType().Name);
				goto case QuestLevel.Basic;
		}
	}
}

public abstract class SimonDeliveryNerysPtjBaseScript : SimonDeliveryPtjBaseScript
{
	protected override string LQuestDescription { get { return L("I'd like to give some [clothes] to Nerys at the Weapons Shop as a present. Can you give me a hand? - Simon -"); } }
	protected override string LObjectiveDescription { get { return L("Deliver new [Clothes] to Nerys."); } }
	protected override int ItemId { get { return 70039; } }
	protected override string LItemNotice { get { return L("You have given Healer Dress to be Delivered to Nerys."); } }
	protected override string NpcIdent { get { return "_nerys"; } }

	protected override async Task OnFinish(NpcScript npc)
	{
		npc.Msg(L("This is from Simon as a gift?<br/>Thank you very much!"));
		npc.Msg(Hide.Name, L("(Delivered the Clothes to Nerys.)"));
	}
}

public class SimonDeliveryNerysBasicPtjScript : SimonDeliveryNerysPtjBaseScript
{
	protected override int QuestId { get { return 510401; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Basic; } }
}

public class SimonDeliveryNerysIntPtjScript : SimonDeliveryNerysPtjBaseScript
{
	protected override int QuestId { get { return 510431; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Int; } }
}

public class SimonDeliveryNerysAdvPtjScript : SimonDeliveryNerysPtjBaseScript
{
	protected override int QuestId { get { return 510461; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Adv; } }
}

public abstract class SimonDeliveryStewartPtjBaseScript : SimonDeliveryPtjBaseScript
{
	protected override string LQuestDescription { get { return L("I'd like to give some [clothes] to Stewart at the School as a present. Can you give me a hand? - Simon -"); } }
	protected override string LObjectiveDescription { get { return L("Deliver new [Clothes] to Stewart."); } }
	protected override int ItemId { get { return 70038; } }
	protected override string LItemNotice { get { return L("You have given Ruffled Tuxedo Ensemble to be Delivered to Stewart."); } }
	protected override string NpcIdent { get { return "_stewart"; } }

	protected override async Task OnFinish(NpcScript npc)
	{
		npc.Msg(L("Simon's giving me new clothes?<br/>I would never refuse such a gift, so thank you!"));
		npc.Msg(Hide.Name, L("(Delivered the Clothes to Stewart.)"));
	}
}

public class SimonDeliveryStewartBasicPtjScript : SimonDeliveryStewartPtjBaseScript
{
	protected override int QuestId { get { return 510402; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Basic; } }
}

public class SimonDeliveryStewartIntPtjScript : SimonDeliveryStewartPtjBaseScript
{
	protected override int QuestId { get { return 510432; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Int; } }
}

public class SimonDeliveryStewartAdvPtjScript : SimonDeliveryStewartPtjBaseScript
{
	protected override int QuestId { get { return 510462; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Adv; } }
}

public abstract class SimonDeliveryManusPtjBaseScript : SimonDeliveryPtjBaseScript
{
	protected override string LQuestDescription { get { return L("I'd like to give some [clothes] to Manus at the Healer's House as a present. Can you give me a hand? - Simon -"); } }
	protected override string LObjectiveDescription { get { return L("Deliver new [Clothes] to Manus."); } }
	protected override int ItemId { get { return 70038; } }
	protected override string LItemNotice { get { return L("You have given Ruffled Tuxedo Ensemble to be Delivered to Manus."); } }
	protected override string NpcIdent { get { return "_manus"; } }

	protected override async Task OnFinish(NpcScript npc)
	{
		npc.Msg(L("Simon's giving me new clothes?<br/>Haha, I wonder what kind of suit it will be this time."));
		npc.Msg(Hide.Name, L("(Delivered the Clothes to Manus.)"));
	}
}

public class SimonDeliveryManusBasicPtjScript : SimonDeliveryManusPtjBaseScript
{
	protected override int QuestId { get { return 510403; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Basic; } }
}

public class SimonDeliveryManusIntPtjScript : SimonDeliveryManusPtjBaseScript
{
	protected override int QuestId { get { return 510433; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Int; } }
}

public class SimonDeliveryManusAdvPtjScript : SimonDeliveryManusPtjBaseScript
{
	protected override int QuestId { get { return 510463; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Adv; } }
}

public abstract class SimonExtDeliveryPtjBaseScript : SimonDeliveryPtjBaseScript
{
	protected abstract string LObjectiveDescription2 { get; }
	protected abstract string NpcIdent2 { get; }

	protected abstract Task OnFinish2(NpcScript npc);

	public override void Load()
	{
		base.Load();

		AddObjective("ptj2", LObjectiveDescription2, 0, 0, 0, Deliver(ItemId, NpcIdent2));

		AddHook(NpcIdent2, "after_intro", AfterIntro2);
	}

	/// <remarks>Overriden to not remove item on intro finish.</remarks>
	public override async Task<HookResult> AfterIntro(NpcScript npc, params object[] args)
	{
		if (!npc.QuestActive(this.Id, "ptj1"))
			return HookResult.Continue;

		npc.FinishQuest(this.Id, "ptj1");

		await OnFinish(npc);

		return HookResult.Break;
	}

	public async Task<HookResult> AfterIntro2(NpcScript npc, params object[] args)
	{
		if (!npc.QuestActive(this.Id, "ptj2"))
			return HookResult.Continue;

		npc.FinishQuest(this.Id, "ptj2");

		npc.Player.RemoveItem(ItemId, 1);
		npc.Notice(LItemNotice);

		await OnFinish2(npc);

		return HookResult.Break;
	}

	/// <remarks>
	/// Int rewards are common among extended item delivery PTJs.
	/// However, they differ in adv rewards and will override this with their own implementation.
	/// </remarks>
	protected override void AddRewards(QuestLevel questLevel)
	{
		if (questLevel != QuestLevel.Int)
			Log.Warning("SimonExtDeliveryPtjBaseScript: Unexpected quest level in derived class {0} (expected Int, got {1}). Please override AddRewards(QuestLevel). Fell back to extended Int rewards.",
				this.GetType().Name,
				questLevel.ToString()
				);

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

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(60015, 5)); // Cheap Finishing Thread
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(250));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(60015, 1)); // Cheap Finishing Thread
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(450));

		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(60006, 5)); // Thick Thread Ball
		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(470));
	}
}

// Script improvised. Update with official details whenever possible.
public abstract class SimonExtDeliveryAeiraEavanPtjBaseScript : SimonExtDeliveryPtjBaseScript
{
	protected override string LQuestDescription { get { return L("I'd like to give some [clothes] to Aeira at the bookstore as a present. Can you give me a hand? - Simon -"); } }
	protected override string LObjectiveDescription { get { return L("Deliver new [Clothes] to Aeira."); } }
	protected override string LObjectiveDescription2 { get { return L("Deliver new [Clothes] to Eavan."); } }
	protected override int ItemId { get { return 70039; } }
	protected override string LItemNotice { get { return L("You have given Healer Dress to be Delivered to Eavan."); } }
	protected override string NpcIdent { get { return "_aeira"; } }
	protected override string NpcIdent2 { get { return "_eavan"; } }

	protected override async Task OnFinish(NpcScript npc)
	{
		npc.Msg(L("Is that a gift from Simon?<br/>But the clothes I'm wearing right now also came from him...<p/>...Oh!<br/>What if we gave it to Eavan? Could you bring that to her?"));
	}

	protected override async Task OnFinish2(NpcScript npc)
	{
		npc.Msg(L("What's the matter?<br/>Aeira would like to give this to me?<p/>...I see. I'll hold on to it for now, then."));
		npc.Msg(Hide.Name, L("(Delivered the Clothes to Eavan.)"));
	}
}

public class SimonExtDeliveryAeiraEavanIntPtjScript : SimonExtDeliveryAeiraEavanPtjBaseScript
{
	protected override int QuestId { get { return 510434; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Int; } }
}

public class SimonExtDeliveryAeiraEavanAdvPtjScript : SimonExtDeliveryAeiraEavanPtjBaseScript
{
	protected override int QuestId { get { return 510464; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Adv; } }

	protected override void AddRewards(QuestLevel questLevel)
	{
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(400));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(350));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(200));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(175));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(80));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(70));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(140));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(545));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(70));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(272));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(28));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(109));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(700));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(125));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(350));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(62));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(140));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(25));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(60015, 10)); // Cheap Finishing Thread
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(175));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(60015, 5)); // Cheap Finishing Thread
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(425));

		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(60015, 1)); // Cheap Finishing Thread
		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(625));

		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(60006, 5)); // Thick Thread Ball
		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(645));
	}
}

// Script improvised. Update with official details whenever possible.
public abstract class SimonExtDeliveryWalterAusteynPtjBaseScript : SimonExtDeliveryPtjBaseScript
{
	protected override string LQuestDescription { get { return L("I'd like to give some [clothes] to Walter at the General Shop as a present. Can you give me a hand? - Simon -"); } }
	protected override string LObjectiveDescription { get { return L("Deliver new [Clothes] to Walter."); } }
	protected override string LObjectiveDescription2 { get { return L("Deliver new [Clothes] to Austeyn."); } }
	protected override int ItemId { get { return 70038; } }
	protected override string LItemNotice { get { return L("You have given Ruffled Tuxedo Ensemble to be Delivered to Austeyn."); } }
	protected override string NpcIdent { get { return "_walter"; } }
	protected override string NpcIdent2 { get { return "_austeyn"; } }

	protected override async Task OnFinish(NpcScript npc)
	{
		npc.Msg(L("A gift from Simon? Hmph, I didn't ask for this. Take it to Austeyn."));
	}

	protected override async Task OnFinish2(NpcScript npc)
	{
		npc.Msg(L("So, this is from Walter.<br/>Not surprising, considering he doesn't like Simon.<br/>I'll take it, though I don't really think it suits me."));
		npc.Msg(Hide.Name, L("(Delivered the Clothes to Austeyn.)"));
	}
}

public class SimonExtDeliveryWalterAusteynIntPtjScript : SimonExtDeliveryWalterAusteynPtjBaseScript
{
	protected override int QuestId { get { return 510435; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Int; } }
}

public class SimonExtDeliveryWalterAusteynAdvPtjScript : SimonExtDeliveryWalterAusteynPtjBaseScript
{
	protected override int QuestId { get { return 510465; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Adv; } }

	protected override void AddRewards(QuestLevel questLevel)
	{
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(400));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(360));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(200));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(180));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(80));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(72));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(145));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(550));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(72));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(275));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(29));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(110));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(700));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(135));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(350));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(67));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(140));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(27));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(60015, 10)); // Cheap Finishing Thread
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(185));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(60015, 5)); // Cheap Finishing Thread
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(435));

		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(60015, 1)); // Cheap Finishing Thread
		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(635));

		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(60006, 5)); // Thick Thread Ball
		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(655));
	}
}