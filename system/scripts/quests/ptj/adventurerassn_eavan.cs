//--- Aura Script -----------------------------------------------------------
// Eavan's Adventurer's Association Part-Time Job
//--- Description -----------------------------------------------------------
// All quests used by the PTJ, and a script to handle the PTJ via hooks.
//--- Notes -----------------------------------------------------------------
// Definitions for the following base scripts have been improvised,
// update with official script whenever possible:
// * EavanExtDeliveryManusAusteynPtjBaseScript
// * EavanDeliveryTracyPtjBaseScript
// * EavanExtDeliveryNerysSimonPtjBaseScript
//---------------------------------------------------------------------------

public class EavanPtjScript : GeneralScript
{
	const PtjType JobType = PtjType.AdventurersAssociation;

	const int Start = 7;
	//int Report, Deadline; // Variable - Extracted from today's PTJ quest data.
	const int PerDay = 10;

	int remaining = PerDay;

	readonly int[] QuestIds = new int[]
	{
		509401, // Basic  Item Delivery (Manus)
		509431, // Int    Item Delivery (Manus)
		509461, // Adv    Item Delivery (Manus)
		509402, // Basic  Item Delivery (Nerys)
		509432, // Int    Item Delivery (Nerys)
		509462, // Adv    Item Delivery (Nerys)
		509433, // Int    Item Delivery (Manus -> Austeyn)
		509463, // Adv    Item Delivery (Manus -> Austeyn)
		509434, // Int    Item Delivery (Tracy)
		509464, // Adv    Item Delivery (Tracy)
		509435, // Int    Item Delivery (Nerys -> Simon)
		509465, // Adv    Item Delivery (Nerys -> Simon)
	};

	/// <summary>
	/// Using supplied <paramref name="npc"/> context to determine today's random PTJ, 
	/// returns personal reporting and deadline hours.
	/// </summary>
	private void GetPersonalReportAndDeadline(NpcScript npc, out int report, out int deadline)
	{
		int ptjid = npc.RandomPtj(JobType, QuestIds);
		switch ((ptjid / 10) % 10) // Extract second least significant digit
		{
			case 0:
				report = 9;
				deadline = 16;
				return;
			case 3:
			case 6:
				report = 16;
				deadline = 20;
				return;
			default:
				Log.Warning("EavanPtjScript: Encountered unexpected PTJ QuestId {0} for player {1} when retrieving report hours. Fell back to basic.", ptjid, npc.Player.Name);
				goto case 0;
		}
	}

	public override void Load()
	{
		AddHook("_eavan", "after_intro", AfterIntro);
		AddHook("_eavan", "before_keywords", BeforeKeywords);
	}

	public async Task<HookResult> AfterIntro(NpcScript npc, params object[] args)
	{
		// Call PTJ method after intro if it's time to report
		int report, deadline;
		GetPersonalReportAndDeadline(npc, out report, out deadline);
		if (npc.DoingPtjForNpc() && npc.ErinnHour(report, deadline))
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
		int report, deadline;

		// Check if already doing another PTJ
		if (npc.DoingPtjForOtherNpc())
		{
			npc.Msg(L("Are you working for someone else?<br/>Can you help me later with this job?"));
			return;
		}

		// Check if PTJ is in progress
		if (npc.DoingPtjForNpc())
		{
			var result = npc.GetPtjResult();

			// Check if report time
			GetPersonalReportAndDeadline(npc, out report, out deadline);
			if (!npc.ErinnHour(report, deadline))
			{
				if (result == QuestResult.Perfect)
				{
					npc.Msg(L("Hmm... Can you come back closer to the deadline?<br/>I will pay you then."));
				}
				else
				{
					npc.Msg(L("How's the work going?"));
				}
				return;
			}

			// Report?
			npc.Msg(L("Did you complete what I've asked of you?<br/>You can report to me even if it's not complete<br/>and I will pay you for the work you've done."),
				npc.Button(L("Report Now"), "@report"),
				npc.Button(L("Report Later"), "@later")
				);

			if (await npc.Select() != "@report")
			{
				npc.Msg(L("Good, I trust your work.<br/>Please make sure to report back to me before the deadline."));
				return;
			}

			// Nothing done
			if (result == QuestResult.None)
			{
				npc.GiveUpPtj();

				npc.Msg(npc.FavorExpression(), L("Are you feeling sick?<br/>You should rest instead of overworking yourself.<br/>But a promise is a promise. I am sorry, but I can't pay you this time."));
				npc.ModifyRelation(0, -Random(3), 0);
			}
			// Low~Perfect result
			else
			{
				npc.Msg(L("Well done. <username/>.<br/>I hope that you will keep up the good work.<br/>I've prepared these items as a reward for the job.<br/>Pick what you need.."),
					npc.Button(L("Report Later"), "@later"),
					npc.PtjReport(result)
					);
				var reply = await npc.Select();

				// Report later
				if (!reply.StartsWith("@reward:"))
				{
					npc.Msg(npc.FavorExpression(), L("You can think about it a little more. Excuse me..."));
					return;
				}

				// Complete
				npc.CompletePtj(reply);
				remaining--;

				// Result msg
				if (result == QuestResult.Perfect)
				{
					npc.Msg(npc.FavorExpression(), L("Good. Just as I asked you to do.<br/>Thank you very much."));
					npc.ModifyRelation(0, Random(3), 0);
				}
				else
				{
					// Eeavan doesn't have any PTJ quests that could yield
					// mid/low results.
				}
			}
			return;
		}

		// Check if PTJ time
		GetPersonalReportAndDeadline(npc, out report, out deadline);
		if (!npc.ErinnHour(Start, deadline))
		{
			npc.Msg(L("It's not time to start work yet.<br/>Can you come back and ask for a job later?"));
			return;
		}

		// Check if not done today and if there are jobs remaining
		if (!npc.CanDoPtj(JobType, remaining))
		{
			npc.Msg(L("There are no more jobs today.<br/>I will give you another job tomorrow."));
			return;
		}

		// Offer PTJ
		var randomPtj = npc.RandomPtj(JobType, QuestIds);
		var msg = "";

		if (npc.GetPtjDoneCount(JobType) == 0)
			msg = L("Do you need some work to do?<br/>If you want, you can help me here.<br/>The pay is not that great, but I will definitely pay you for your work.<br/>The pay will be adjusted depending on how long you've worked for me.<p/>Would you like to try?");
		else
			msg = L("Ah, <username/>. Can you help me today?");

		npc.Msg(msg, npc.PtjDesc(randomPtj,
			L("Eavan's Adven. Assoc. Part-Time Job"),
			L("Looking for help with delivery of goods in Adventurers' Association."),
			PerDay, remaining, npc.GetPtjDoneCount(JobType)));

		if (await npc.Select() == "@accept")
		{
			if (npc.GetPtjDoneCount(JobType) == 0)
				npc.Msg(L("Thank you. I know you will be of great help to me.<br/>I want to make one thing clear before we start, though.<br/>You must report to me before the deadline whether or not your job is complete."));
			else
				npc.Msg(L("Thank you for your help in advance."));

			npc.StartPtj(randomPtj);
		}
		else
		{
			if (npc.GetPtjDoneCount(JobType) == 0)
				npc.Msg(L("Do you have something else to do?<br/>Then, I will find someone else."));
			else
				npc.Msg(L("You seem busy today."));
		}
	}
}

public abstract class EavanDeliveryPtjBaseScript : QuestScript
{
	protected abstract int QuestId { get; }
	protected abstract string LQuestDescription { get; }
	protected abstract QuestLevel QuestLevel { get; }
	protected abstract string LObjectiveDescription { get; }
	protected abstract string LGivenNotice { get; }
	protected abstract int ItemId { get; }
	protected abstract string NpcIdent { get; }

	protected abstract void AddRewards();
	protected abstract void RecipientAfterIntroDialogue(NpcScript npc);

	public override void Load()
	{
		SetId(QuestId);
		SetName(L("Adven. Assoc. Part-Time Job"));
		SetDescription(LQuestDescription);

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.ById);

		SetType(QuestType.Deliver);
		SetPtjType(PtjType.AdventurersAssociation);
		SetLevel(QuestLevel);
		SetHours(
			start: 7,
			report: QuestLevel == QuestLevel.Basic ? 9 : 16,
			deadline: QuestLevel == QuestLevel.Basic ? 16 : 20
			);

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

		npc.FinishQuest(this.Id, "ptj");

		npc.Player.Inventory.Remove(ItemId, 1);
		npc.Notice(LGivenNotice);

		RecipientAfterIntroDialogue(npc);

		return HookResult.Break;
	}
}

// Extended item delivery quests
public abstract class EavanExtDeliveryPtjBaseScript : QuestScript
{
	protected abstract int QuestId { get; }
	protected abstract string LQuestDescription { get; }
	protected abstract QuestLevel QuestLevel { get; } // All extended item delivery quests either Int or Adv.
	protected abstract string LTalkObjectiveDescription { get; }
	protected abstract string LDeliverObjectiveDescription { get; }
	protected abstract int ItemId { get; }
	protected abstract string LReceivedNotice { get; }
	protected abstract string ClientNpcIdent { get; }
	protected abstract string LGivenNotice { get; }
	protected abstract string RecipientNpcIdent { get; }

	protected abstract void AddRewards();
	protected abstract void ClientAfterIntroDialogue(NpcScript npc);
	protected abstract void RecipientAfterIntroDialogue(NpcScript npc);

	public override void Load()
	{
		SetId(QuestId);
		SetName(L("Adven. Assoc. Part-Time Job"));
		SetDescription(LQuestDescription);

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.ById);

		SetType(QuestType.Deliver);
		SetPtjType(PtjType.AdventurersAssociation);
		SetLevel(QuestLevel);
		SetHours(start: 7, report: 16, deadline: 20);

		AddObjective("ptj1", LTalkObjectiveDescription, 0, 0, 0, Talk(ClientNpcIdent));
		AddObjective("ptj2", LDeliverObjectiveDescription, 0, 0, 0, Deliver(ItemId, RecipientNpcIdent));

		AddHook(ClientNpcIdent, "after_intro", ClientAfterIntro);
		AddHook(RecipientNpcIdent, "after_intro", RecipientAfterIntro);

		AddRewards();
	}

	public async Task<HookResult> ClientAfterIntro(NpcScript npc, params object[] args)
	{
		if (!npc.QuestActive(this.Id, "ptj1"))
			return HookResult.Continue;

		npc.FinishQuest(this.Id, "ptj1");

		npc.Player.GiveItem(ItemId);
		npc.Notice(LReceivedNotice);

		ClientAfterIntroDialogue(npc);

		return HookResult.Break;
	}

	public async Task<HookResult> RecipientAfterIntro(NpcScript npc, params object[] args)
	{
		if (!npc.QuestActive(this.Id, "ptj2"))
			return HookResult.Continue;

		if (!npc.Player.Inventory.Has(ItemId))
			return HookResult.Continue;

		npc.FinishQuest(this.Id, "ptj2");

		npc.Player.RemoveItem(ItemId, 1);
		npc.Notice(LGivenNotice);

		RecipientAfterIntroDialogue(npc);

		return HookResult.Break;
	}
}

public abstract class EavanDeliveryManusPtjBaseScript : EavanDeliveryPtjBaseScript
{
	protected override string LQuestDescription { get { return L("I need to send [1 wild ginseng] to Manus at the Healer's House, but I'm really busy right now. Can you help me deliver this? - Eavan -"); } }
	protected override string LObjectiveDescription { get { return L("Deliver [1 Wild Ginseng] to Manus."); } }
	protected override string LGivenNotice { get { return L("You have given Wild Ginseng to be Delivered to Manus."); } }
	protected override int ItemId { get { return 70020; } }
	protected override string NpcIdent { get { return "_manus"; } }

	protected override void RecipientAfterIntroDialogue(NpcScript npc)
	{
		npc.Msg(L("Hey! This is the Wild Ginseng I lost a while ago!<br/>I thought I'd never see this again. I can't thank you enough!"));
		npc.Msg(Hide.Name, L("(Delivered 1 Wild Ginseng to Manus.)"));
	}
}

public class EavanDeliveryManusBasicPtjScript : EavanDeliveryManusPtjBaseScript
{
	protected override int QuestId { get { return 509401; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Basic; } }

	protected override void AddRewards() // Reward set 1
	{
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(180));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(180));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(90));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(90));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(36));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(36));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(70));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(270));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(35));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(135));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(14));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(54));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(325));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(70));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(163));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(35));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(65));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(14));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(51001, 6)); // HP 10 Potion
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(40));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(51011, 10)); // Stamina 10 Potion
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(240));

		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(50004)); // Bread
		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(284));

		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(40023)); // Gathering Knife
		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(290));
	}
}

public class EavanDeliveryManusIntPtjScript : EavanDeliveryManusPtjBaseScript
{
	protected override int QuestId { get { return 509431; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Int; } }

	protected override void AddRewards() // Reward set 2
	{
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(260));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(260));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(130));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(130));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(52));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(52));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(100));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(380));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(50));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(190));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(20));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(76));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(480));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(95));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(240));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(38));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(96));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(19));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(51001, 9)); // HP 10 Potion
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(30));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(51011, 10)); // Stamina 10 Potion
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(380));

		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(50004)); // Bread
		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(424));

		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(40023)); // Gathering Knife
		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(430));
	}
}

public class EavanDeliveryManusAdvPtjScript : EavanDeliveryManusPtjBaseScript
{
	protected override int QuestId { get { return 509461; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Adv; } }

	protected override void AddRewards() // Reward set 3
	{
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(340));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(340));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(170));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(170));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(68));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(68));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(130));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(500));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(65));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(250));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(26));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(100));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(625));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(125));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(312));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(62));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(125));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(25));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(51001, 9)); // HP 10 Potion
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(120));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(51011, 10)); // Stamina 10 Potion
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(520));

		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(50004)); // Bread
		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(564));

		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(60015, 10)); // Cheap Finishing Thread
		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(120));
	}
}

public abstract class EavanDeliveryNerysPtjBaseScript : EavanDeliveryPtjBaseScript
{
	protected override string LQuestDescription { get { return L("I need to send a [pearl necklace] to Nerys at the Weapons Shop, but I'm really busy right now. Can you help me deliver this? - Eavan -"); } }
	protected override string LObjectiveDescription { get { return L("Deliver the [Pearl Necklace] to Nerys."); } }
	protected override string LGivenNotice { get { return L("You have given Pearl Necklace to be Delivered to Nerys."); } }
	protected override int ItemId { get { return 70012; } }
	protected override string NpcIdent { get { return "_nerys"; } }

	protected override void RecipientAfterIntroDialogue(NpcScript npc)
	{
		npc.Msg(L("It's my lost pearl necklace!<br/>I was so worried after losing this! Thank you so much!"));
		npc.Msg(Hide.Name, L("(Delivered the Pearl Necklace to Nerys.)"));
	}
}

public class EavanDeliveryNerysBasicPtjScript : EavanDeliveryNerysPtjBaseScript
{
	protected override int QuestId { get { return 509402; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Basic; } }

	protected override void AddRewards() // Reward set 1
	{
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(180));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(180));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(90));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(90));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(36));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(36));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(70));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(270));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(35));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(135));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(14));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(54));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(325));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(70));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(163));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(35));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(65));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(14));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(51001, 6)); // HP 10 Potion
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(40));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(51011, 10)); // Stamina 10 Potion
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(240));

		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(50004)); // Bread
		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(284));

		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(40023)); // Gathering Knife
		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(290));
	}
}

public class EavanDeliveryNerysIntPtjScript : EavanDeliveryNerysPtjBaseScript
{
	protected override int QuestId { get { return 509432; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Int; } }

	protected override void AddRewards() // Reward set 2
	{
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(260));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(260));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(130));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(130));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(52));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(52));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(100));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(380));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(50));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(190));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(20));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(76));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(480));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(95));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(240));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(38));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(96));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(19));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(51001, 9)); // HP 10 Potion
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(30));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(51011, 10)); // Stamina 10 Potion
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(380));

		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(50004)); // Bread
		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(424));

		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(40023)); // Gathering Knife
		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(430));
	}
}

public class EavanDeliveryNerysAdvPtjScript : EavanDeliveryNerysPtjBaseScript
{
	protected override int QuestId { get { return 509462; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Adv; } }

	protected override void AddRewards() // Reward set 3
	{
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(340));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(340));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(170));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(170));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(68));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(68));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(130));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(500));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(65));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(250));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(26));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(100));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(625));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(125));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(312));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(62));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(125));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(25));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(51001, 9)); // HP 10 Potion
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(120));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(51011, 10)); // Stamina 10 Potion
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(520));

		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(50004)); // Bread
		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(564));

		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(60015, 10)); // Cheap Finishing Thread
		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(120));
	}
}

// Script improvised. Update with official details whenever possible.
public abstract class EavanExtDeliveryManusAusteynPtjBaseScript : EavanExtDeliveryPtjBaseScript
{
	protected override string LQuestDescription { get { return L("Austeyn seems to have forgotten something at the [Healer's House]. Can you deliver it to him for me? You can find him at the [Bank]. - Eavan -"); } }
	protected override string LTalkObjectiveDescription { get { return L("Receive a book from Manus."); } }
	protected override string LDeliverObjectiveDescription { get { return L("Deliver the book to Austeyn."); } }
	protected override int ItemId { get { return 70032; } }
	protected override string LReceivedNotice { get { return L("You have received Book to be Delivered from Manus"); } }
	protected override string ClientNpcIdent { get { return "_manus"; } }
	protected override string LGivenNotice { get { return L("You have given Book to be Delivered to Austeyn."); } }
	protected override string RecipientNpcIdent { get { return "_austeyn"; } }

	protected override void ClientAfterIntroDialogue(NpcScript npc)
	{
		npc.Msg(L("What a boring book I just read...<br/>Oh, <username/>. I heard from Eavan. This is Austeyn's, right?<br/>Say hello to the old man for me. Haha!"));
		npc.Msg(L("(Received a book from Manus.)"));
	}

	protected override void RecipientAfterIntroDialogue(NpcScript npc)
	{
		npc.Msg(L("I was wondering where I lost that book.<br/>Thank you."));
		npc.Msg(L("(Delivered the book to Austeyn.)"));
	}
}

public class EavanExtDeliveryManusAusteynIntPtjScript : EavanExtDeliveryManusAusteynPtjBaseScript
{
	protected override int QuestId { get { return 509433; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Int; } }

	protected override void AddRewards() // Reward set 4
	{
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(300));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(320));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(150));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(160));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(60));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(64));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(125));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(450));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(62));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(225));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(25));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(90));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(575));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(115));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(287));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(57));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(115));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(23));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(51001, 10)); // HP 10 Potion
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(70));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(51011, 10)); // Stamina 10 Potion
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(470));

		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(50004)); // Bread
		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(514));

		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(60015, 10)); // Cheap Finishing Thread
		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(70));
	}
}

public class EavanExtDeliveryManusAusteynAdvPtjScript : EavanExtDeliveryManusAusteynPtjBaseScript
{
	protected override int QuestId { get { return 509463; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Adv; } }

	protected override void AddRewards() // Reward set 5
	{
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(400));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(450));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(200));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(225));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(80));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(90));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(180));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(615));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(90));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(307));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(36));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(123));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(800));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(150));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(400));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(75));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(160));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(30));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(51001, 10)); // HP 10 Potion
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(275));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(51011, 10)); // Stamina 10 Potion
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(675));

		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(15003)); // Vest and Pants Set
		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(75));

		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(60015, 10)); // Cheap Finishing Thread
		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(275));
	}
}

// Script improvised. Update with official details whenever possible.
public abstract class EavanDeliveryTracyPtjBaseScript : EavanDeliveryPtjBaseScript
{
	protected override string LQuestDescription { get { return L("Tracy has been troubled since losing his hammer. Can you deliver it to him for me? You can find him at [Dugald Aisle]. - Eavan -"); } }
	protected override string LObjectiveDescription { get { return L("Deliver the [hammer] to Tracy."); } }
	protected override string LGivenNotice { get { return L("You have given Hammer to be Delivered to Tracy."); } }
	protected override int ItemId { get { return 70015; } }
	protected override string NpcIdent { get { return "_tracy"; } }

	protected override void RecipientAfterIntroDialogue(NpcScript npc)
	{
		npc.Msg(L("Hey! Isn't that my hammer?<br/>Thanks, man."));
		npc.Msg(Hide.Name, L("(Delivered the hammer to Tracy.)"));
	}
}

public class EavanDeliveryTracyIntPtjScript : EavanDeliveryTracyPtjBaseScript
{
	protected override int QuestId { get { return 509434; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Int; } }

	protected override void AddRewards() // Reward set 6
	{
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(500));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(360));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(250));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(180));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(100));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(72));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(170));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(610));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(85));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(305));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(34));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(122));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(780));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(150));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(390));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(75));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(156));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(30));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(51001, 10)); // HP 10 Potion
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(260));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(51011, 10)); // Stamina 10 Potion
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(660));

		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(15003)); // Vest and Pants Set
		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(60));

		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(60015, 10)); // Cheap Finishing Thread
		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(260));
	}
}

public class EavanDeliveryTracyAdvPtjScript : EavanDeliveryTracyPtjBaseScript
{
	protected override int QuestId { get { return 509464; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Adv; } }

	protected override void AddRewards() // Reward set 7
	{
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(700));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(520));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(350));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(260));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(140));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(104));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(260));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(850));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(130));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(425));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(52));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(170));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(1100));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(210));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(550));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(105));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(220));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(42));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(63000, 10)); // Phoenix Feather
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(70));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(40002)); // Wooden Blade
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(70));

		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(15003)); // Vest and Pants Set
		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(370));

		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(60015, 10)); // Cheap Finishing Thread
		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(570));
	}
}

// Script improvised. Update with official details whenever possible.
public abstract class EavanExtDeliveryNerysSimonPtjBaseScript : EavanExtDeliveryPtjBaseScript
{
	protected override string LQuestDescription { get { return L("Simon seems to have forgotten a [sewing kit] at the [Weapon Shop]. Can you deliver it to him for me? You can find him at the [Clothing Shop]. - Eavan -"); } }
	protected override string LTalkObjectiveDescription { get { return L("Receive the sewing kit from Nerys."); } }
	protected override string LDeliverObjectiveDescription { get { return L("Deliver the sewing kit to Simon."); } }
	protected override int ItemId { get { return 70033; } }
	protected override string LReceivedNotice { get { return L("You have received Sewing Kit to be Delivered from Nerys"); } }
	protected override string ClientNpcIdent { get { return "_nerys"; } }
	protected override string LGivenNotice { get { return L("You have given Sewing Kit to be Delivered to Simon."); } }
	protected override string RecipientNpcIdent { get { return "_simon"; } }

	protected override void ClientAfterIntroDialogue(NpcScript npc)
	{
		npc.Msg(L("...just pick this up himself...?"));
		npc.Msg(L("Oh, sorry. Was just talking to myself.<br/>Here you go."));
		npc.Msg(L("(Received the sewing kit from Nerys.)"));
	}

	protected override void RecipientAfterIntroDialogue(NpcScript npc)
	{
		npc.Msg(L("Ah, there's my sewing kit.<br/>So you must be sent by Eavan?"));
		npc.Msg(L("(Delivered the sewing kit to Simon.)"));
	}
}

public class EavanExtDeliveryNerysSimonIntPtjScript : EavanExtDeliveryNerysSimonPtjBaseScript
{
	protected override int QuestId { get { return 509435; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Int; } }

	protected override void AddRewards() // Reward set 4
	{
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(300));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(320));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(150));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(160));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(60));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(64));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(125));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(450));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(62));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(225));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(25));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(90));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(575));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(115));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(287));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(57));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(115));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(23));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(51001, 10)); // HP 10 Potion
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(70));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(51011, 10)); // Stamina 10 Potion
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(470));

		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(50004)); // Bread
		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(514));

		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(60015, 10)); // Cheap Finishing Thread
		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(70));
	}
}

public class EavanExtDeliveryNerysSimonAdvPtjScript : EavanExtDeliveryNerysSimonPtjBaseScript
{
	protected override int QuestId { get { return 509465; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Adv; } }

	protected override void AddRewards() // Reward set 5
	{
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(400));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(450));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(200));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(225));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(80));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(90));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(180));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(615));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(90));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(307));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(36));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(123));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(800));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(150));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(400));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(75));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(160));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(30));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(51001, 10)); // HP 10 Potion
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(275));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(51011, 10)); // Stamina 10 Potion
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(675));

		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(15003)); // Vest and Pants Set
		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(75));

		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Item(60015, 10)); // Cheap Finishing Thread
		AddReward(7, RewardGroupType.Item, QuestResult.Perfect, Gold(275));
	}
}