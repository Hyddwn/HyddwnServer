//--- Aura Script -----------------------------------------------------------
// Ferghus's Blacksmith Shop Part-Time Job
//--- Description -----------------------------------------------------------
// All quests used by the PTJ, and a script to handle the PTJ via hooks.
//--- Notes -----------------------------------------------------------------
// Apparently the rewards have been increased, as the Wiki now lists
// higher ones than in the past. We'll use the G1 rewards for now.
// Due to limited G1 data, current rewards are downscaled to 50% gold, 33% exp.
// Ref.: http://wiki.mabinogiworld.com/index.php?title=Ferghus&diff=prev&oldid=172331
//---------------------------------------------------------------------------

public class FerghusPtjScript : GeneralScript
{
	const PtjType JobType = PtjType.BlacksmithShop;

	const int Start = 12;
	const int Report = 13;
	const int Deadline = 19;
	const int PerDay = 10;

	int remaining = PerDay;


	readonly int[] QuestIds = new int[]
	{
		507401, // Basic  Armor Delivery (Ranald)
		507431, // Int    Armor Delivery (Ranald)
		507461, // Adv    Armor Delivery (Ranald)
		507402, // Basic  Armor Delivery (Nora)
		507432, // Int    Armor Delivery (Nora)
		507462, // Adv    Armor Delivery (Nora)
		507403, // Basic  Armor Delivery (Malcolm)
		507433, // Int    Armor Delivery (Malcolm)
		507463, // Adv    Armor Delivery (Malcolm)
		507404, // Basic  Armor Delivery (Trefor)
		507434, // Int    Armor Delivery (Trefor)
		507464, // Adv    Armor Delivery (Trefor)
	};


	public override void Load()
	{
		AddHook("_ferghus", "after_intro", AfterIntro);
		AddHook("_ferghus", "before_keywords", BeforeKeywords);
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
			npc.Msg(L("Are you doing a part-time job?<br/>I guess you can help me next time."));
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
					npc.Msg(L("What? Did you finish the job?<br/>I'm busy now. Come back to me closer to the deadline."));
				}
				else
				{
					npc.Msg(L("How's it going?<br/>"));
					npc.Msg(L("Make sure to report back to me before the deadline."));
				}
				return;
			}

			// Report?
			npc.Msg(L("Let's see how you did.<br/>If you're not finished, you can report later.<br/>What are you going to do?"),
				npc.Button(L("Report Now"), "@report"),
				npc.Button(L("Report Later"), "@later")
				);

			if (await npc.Select() != "@report")
			{
				npc.Msg(L("Haha.<br/>If you don't finish the work by the deadline, you will be in big trouble."));
				return;
			}

			// Nothing done
			if (result == QuestResult.None)
			{
				npc.GiveUpPtj();

				npc.Msg(npc.FavorExpression(), L("......"));
				npc.Msg(L("What!"));
				npc.Msg(L("Are you joking with me?<br/>Once you take my offer, you have to do it right!<br/>If you're not interested, then don't even start!"));

				npc.ModifyRelation(0, -Random(3), 0);
			}
			// Low~Perfect result
			else
			{
				npc.Msg(L("Haha. You are quite diligent.<br/>Alright, I'll let you pick one of these.<br/>Just get whatever you want. It's all because you've worked very hard for me. Go ahead."),
					npc.Button(L("Report Later"), "@later"),
					npc.PtjReport(result)
					);
				var reply = await npc.Select();

				// Report later
				if (!reply.StartsWith("@reward:"))
				{
					npc.Msg(npc.FavorExpression(), L("It's alright if you have other things to do.<br/>Go ahead. I'll see you later."));
					return;
				}

				// Complete
				npc.CompletePtj(reply);
				remaining--;

				// Result msg
				if (result == QuestResult.Perfect)
				{
					npc.Msg(npc.FavorExpression(), L("You may not look the part, but you're quite dilligent in your work.<br/>Here's your pay for the day."));
					npc.ModifyRelation(0, Random(3), 0);
				}
				else if (result == QuestResult.Mid)
				{
					npc.Msg(npc.FavorExpression(), L("(missing): 3 star response"));
					npc.ModifyRelation(0, Random(1), 0);
				}
				else if (result == QuestResult.Low)
				{
					npc.Msg(npc.FavorExpression(), L("Is this all you did?<br/>It's far from enough. This is all I can give you.<br/>Remember this. It's not easy to earn money out of others' pockets."));
					npc.ModifyRelation(0, -Random(2), 0);
				}
			}
			return;
		}

		// Check if PTJ time
		if (!npc.ErinnHour(Start, Deadline))
		{
			npc.Msg(L("What? Part-time job?<br/>There's nothing. You can come back later."));
			return;
		}

		// Check if not done today and if there are jobs remaining
		if (!npc.CanDoPtj(JobType, remaining))
		{
			npc.Msg(L("Hey, they're all taken for the day.<br/>And don't come here tomorrow. I don't want to work with you any more."));
			npc.Msg(L("..."));
			npc.Msg(L("Haha. I'm joking, I'm joking.<br/>Of course you can come back tomorrow."));
			return;
		}

		// Offer PTJ
		var randomPtj = npc.RandomPtj(JobType, QuestIds);
		var msg = "";

		if (npc.GetPtjDoneCount(JobType) == 0)
		{
			npc.Msg(L("Are you looking for a job?<br/>You'd get sweaty, hot and tired working at the Blacksmith's Shop.<br/>I guess you are not really up to it.<br/>How about doing some simple part-time work?"));
			msg = L("I'll see how much I can pay you depending on how you do.");
		}
		else
			msg = L("Let's see, you want to work at the Blacksmith's Shop for a day?");

		npc.Msg(msg, npc.PtjDesc(randomPtj,
			L("Ferghus's Blacksmith Shop Part-Time Job"),
			L("Looking for help with delivery of goods in Blacksmith Shop."),
			PerDay, remaining, npc.GetPtjDoneCount(JobType)));

		if (await npc.Select() == "@accept")
		{
			if (npc.GetPtjDoneCount(JobType) == 0)
				npc.Msg(L("Oh, good then.<br/>I'll keep watching you.<br/>You're not a lazy kid who doesn't even bother<br/>to work or report before the deadline, are you?"));
			else
				npc.Msg(L("Alright. Good idea."));
			npc.StartPtj(randomPtj);
		}
		else
		{
			if (npc.GetPtjDoneCount(JobType) == 0)
				npc.Msg(L("If you don't want it, then forget it.<br/>Young people these days don't even bother to think of doing anything difficult."));
			else
				npc.Msg(L("You can't really hire someone who doesn't want to work for you."));
		}
	}
}

public abstract class FerghusPtjBaseScript : QuestScript
{
	protected const int ArmorToDeliver = 70002; // Full Ring Mail to be Delivered

	public override void Load()
	{
		SetName(L("Blacksmith Shop Part-Time Job"));
		SetDescription(L("This armor has been repaired, but the owner has not shown up yet. Can you [deliver this Ring Mail] for me? - Ferghus -"));

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.ById);

		SetType(QuestType.Deliver);
		SetPtjType(PtjType.BlacksmithShop);
		SetHours(start: 12, report: 13, deadline: 19);
	}

	// All deliveries seem to share common rewards...
	protected void SetBasicLevelAndRewards()
	{
		SetLevel(QuestLevel.Basic);

		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(225));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(180));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(113));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(90));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(45));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(36));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(75));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(300));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(38));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(150));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(15));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(60));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(375));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(75));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(188));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(38));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(75));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(15));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(51001, 7)); // HP 10 Potion
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(25));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(51011, 10)); // Stamina 10 Potion
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(140));
	}

	protected void SetIntLevelAndRewards()
	{
		SetLevel(QuestLevel.Int);

		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(350));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(280));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(175));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(140));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(70));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(56));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(120));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(450));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(60));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(225));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(24));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(90));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(570));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(115));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(285));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(58));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(114));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(23));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(51003, 10)); // HP 50 Potion
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(70));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(51013, 10)); // Stamina 50 Potion
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(210));
	}

	protected void SetAdvLevelAndRewards()
	{
		SetLevel(QuestLevel.Adv);

		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(480));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(380));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(240));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(190));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(96));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(76));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(170));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(600));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(85));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(300));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(34));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(120));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(800));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(150));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(400));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(75));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(160));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(30));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(51003, 10)); // HP 50 Potion
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(600));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(17019)); // Blacksmith Shoes

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(40025)); // Pickaxe
	}
}

public abstract class FerghusDeliveryPtjBaseScript : FerghusPtjBaseScript
{
	protected abstract int QuestId { get; }
	protected abstract string NpcIdent { get; }
	protected abstract string Objective { get; }

	public override void Load()
	{
		SetId(QuestId);

		AddObjective("ptj", this.Objective, 0, 0, 0, Deliver(ArmorToDeliver, NpcIdent));
		AddHook(NpcIdent, "after_intro", AfterIntro);

		base.Load();
	}

	public async Task<HookResult> AfterIntro(NpcScript npc, params object[] args)
	{
		if (!npc.QuestActive(this.Id, "ptj"))
			return HookResult.Continue;

		if (!npc.Player.Inventory.Has(ArmorToDeliver))
			return HookResult.Continue;

		npc.Player.Inventory.Remove(ArmorToDeliver, 1);
		npc.FinishQuest(this.Id, "ptj");

		await this.OnFinish(npc);

		return HookResult.Break;
	}

	protected virtual async Task OnFinish(NpcScript npc)
	{
		await Task.Yield();
	}
}

public class FerghusDeliveryRanaldBasicPtjScript : FerghusDeliveryPtjBaseScript
{
	protected override int QuestId { get { return 507401; } }
	protected override string NpcIdent { get { return "_ranald"; } }
	protected override string Objective { get { return L("Deliver Armor to Ranald"); } }

	public override void Load()
	{
		SetBasicLevelAndRewards();

		base.Load();
	}

	protected override async Task OnFinish(NpcScript npc)
	{
		npc.Msg(L("Thank you.<br/>I completely forgot about picking up my armor."));
		npc.Msg(Hide.Name, L("(Delivered the armor to Ranald.)"));
		npc.Msg(L("Hmm, perfect. Ferghus is the best blacksmith in town,<br/>don't you think?<br/>Anyway, thanks again for bringing it to me."));
	}
}

public class FerghusDeliveryRanaldIntPtjScript : FerghusDeliveryPtjBaseScript
{
	protected override int QuestId { get { return 507431; } }
	protected override string NpcIdent { get { return "_ranald"; } }
	protected override string Objective { get { return L("Deliver Armor to Ranald"); } }

	public override void Load()
	{
		SetIntLevelAndRewards();

		base.Load();
	}

	protected override async Task OnFinish(NpcScript npc)
	{
		npc.Msg(L("Thank you.<br/>I completely forgot about picking up my armor."));
		npc.Msg(Hide.Name, L("(Delivered the armor to Ranald.)"));
		npc.Msg(L("Hmm, perfect. Ferghus is the best blacksmith in town,<br/>don't you think?<br/>Anyway, thanks again for bringing it to me."));
	}
}

public class FerghusDeliveryRanaldAdvPtjScript : FerghusDeliveryPtjBaseScript
{
	protected override int QuestId { get { return 507461; } }
	protected override string NpcIdent { get { return "_ranald"; } }
	protected override string Objective { get { return L("Deliver Armor to Ranald"); } }

	public override void Load()
	{
		SetAdvLevelAndRewards();

		base.Load();
	}

	protected override async Task OnFinish(NpcScript npc)
	{
		npc.Msg(L("Thank you.<br/>I completely forgot about picking up my armor."));
		npc.Msg(Hide.Name, L("(Delivered the armor to Ranald.)"));
		npc.Msg(L("Hmm, perfect. Ferghus is the best blacksmith in town,<br/>don't you think?<br/>Anyway, thanks again for bringing it to me."));
	}
}

public class FerghusDeliveryNoraBasicPtjScript : FerghusDeliveryPtjBaseScript
{
	protected override int QuestId { get { return 507402; } }
	protected override string NpcIdent { get { return "_nora"; } }
	protected override string Objective { get { return L("Deliver Armor to Nora"); } }

	public override void Load()
	{
		SetBasicLevelAndRewards();

		base.Load();
	}

	protected override async Task OnFinish(NpcScript npc)
	{
		npc.Msg(L("Wow, that was fast. I'm glad to have my armor back."));
		npc.Msg(Hide.Name, L("(Delivered the armor to Nora.)"));
		npc.Msg(L("Yes? You're asking if this armor is for me?<br/>Ha ha, no way. I just like how this ring mail looks."));
	}
}

public class FerghusDeliveryNoraIntPtjScript : FerghusDeliveryPtjBaseScript
{
	protected override int QuestId { get { return 507432; } }
	protected override string NpcIdent { get { return "_nora"; } }
	protected override string Objective { get { return L("Deliver Armor to Nora"); } }

	public override void Load()
	{
		SetIntLevelAndRewards();

		base.Load();
	}

	protected override async Task OnFinish(NpcScript npc)
	{
		npc.Msg(L("Wow, that was fast. I'm glad to have my armor back."));
		npc.Msg(Hide.Name, L("(Delivered the armor to Nora.)"));
		npc.Msg(L("Yes? You're asking if this armor is for me?<br/>Ha ha, no way. I just like how this ring mail looks."));
	}
}

public class FerghusDeliveryNoraAdvPtjScript : FerghusDeliveryPtjBaseScript
{
	protected override int QuestId { get { return 507462; } }
	protected override string NpcIdent { get { return "_nora"; } }
	protected override string Objective { get { return L("Deliver Armor to Nora"); } }

	public override void Load()
	{
		SetAdvLevelAndRewards();

		base.Load();
	}

	protected override async Task OnFinish(NpcScript npc)
	{
		npc.Msg(L("Wow, that was fast. I'm glad to have my armor back."));
		npc.Msg(Hide.Name, L("(Delivered the armor to Nora.)"));
		npc.Msg(L("Yes? You're asking if this armor is for me?<br/>Ha ha, no way. I just like how this ring mail looks."));
	}
}

public class FerghusDeliveryMalcolmBasicPtjScript : FerghusDeliveryPtjBaseScript
{
	protected override int QuestId { get { return 507403; } }
	protected override string NpcIdent { get { return "_malcolm"; } }
	protected override string Objective { get { return L("Deliver Armor to Malcolm"); } }

	public override void Load()
	{
		SetBasicLevelAndRewards();

		base.Load();
	}

	protected override async Task OnFinish(NpcScript npc)
	{
		npc.Msg(L("Ah, my new armor has finally arrived.<br/>In fact, I was about to get it myself.<br/>Thank you for the delivery."));
		npc.Msg(Hide.Name, L("(Delivered the armor to Malcolm.)"));
		npc.Msg(L("You didn't put it on before coming here, did you?<br/>"));
	}
}

public class FerghusDeliveryMalcolmIntPtjScript : FerghusDeliveryPtjBaseScript
{
	protected override int QuestId { get { return 507433; } }
	protected override string NpcIdent { get { return "_malcolm"; } }
	protected override string Objective { get { return L("Deliver Armor to Malcolm"); } }

	public override void Load()
	{
		SetIntLevelAndRewards();

		base.Load();
	}

	protected override async Task OnFinish(NpcScript npc)
	{
		npc.Msg(L("Ah, my new armor has finally arrived.<br/>In fact, I was about to get it myself.<br/>Thank you for the delivery."));
		npc.Msg(Hide.Name, L("(Delivered the armor to Malcolm.)"));
		npc.Msg(L("You didn't put it on before coming here, did you?<br/>"));
	}
}

public class FerghusDeliveryMalcolmAdvPtjScript : FerghusDeliveryPtjBaseScript
{
	protected override int QuestId { get { return 507463; } }
	protected override string NpcIdent { get { return "_malcolm"; } }
	protected override string Objective { get { return L("Deliver Armor to Malcolm"); } }

	public override void Load()
	{
		SetAdvLevelAndRewards();

		base.Load();
	}

	protected override async Task OnFinish(NpcScript npc)
	{
		npc.Msg(L("Ah, my new armor has finally arrived.<br/>In fact, I was about to get it myself.<br/>Thank you for the delivery."));
		npc.Msg(Hide.Name, L("(Delivered the armor to Malcolm.)"));
		npc.Msg(L("You didn't put it on before coming here, did you?<br/>"));
	}
}

// Extended delivery quest - Trefor
public abstract class FerghusExtDeliveryPtjBaseScript : FerghusPtjBaseScript
{
	public override void Load()
	{
		AddObjective("ptj1", L("Deliver Armor to Trefor"), 0, 0, 0, Deliver(ArmorToDeliver, "_trefor"));
		AddObjective("ptj2", L("Leave the Armor at the Healer's House"), 0, 0, 0, Deliver(ArmorToDeliver, "_dilys"));

		AddHook("_trefor", "after_intro", TreforAfterIntro);
		AddHook("_dilys", "after_intro", DilysAfterIntro);

		base.Load();
	}

	public async Task<HookResult> TreforAfterIntro(NpcScript npc, params object[] args)
	{
		if (!npc.QuestActive(this.Id, "ptj1"))
			return HookResult.Continue;

		if (!npc.Player.Inventory.Has(ArmorToDeliver))
			return HookResult.Continue;

		npc.FinishQuest(this.Id, "ptj1");

		npc.Msg(L("Is that the armor I asked to be repaired?<br/>The work is finally done?"));
		npc.Msg(Hide.Name, L("(Delivered the armor to Trefor.)"));
		npc.Msg(L("But I've got a slight problem.<br/>As you can see, I'm on duty right now and have no place to store it."));
		npc.Msg(L("Could you do me a favor and leave it at the Healer's House?<br/>I'll grab it after my shift is over."));
		npc.Msg(Hide.Name, L("(Received the armor.)"));

		return HookResult.Break;
	}

	public async Task<HookResult> DilysAfterIntro(NpcScript npc, params object[] args)
	{
		if (!npc.QuestActive(this.Id, "ptj2"))
			return HookResult.Continue;

		if (!npc.Player.Inventory.Has(ArmorToDeliver))
			return HookResult.Continue;

		npc.Player.Inventory.Remove(ArmorToDeliver, 1);
		npc.FinishQuest(this.Id, "ptj2");

		npc.Msg(L("Not again!<br/>Did Trefor ask you to leave that armor here?"));
		npc.Msg(Hide.Name, L("(Gave the armor to Dilys.)"));
		npc.Msg(L("That guy! Does he think this is a warehouse or something?<br/>Well, fine. I'll hold on to it for him."));
		npc.Msg(L("As you know, being a guard is not easy."));

		return HookResult.Break;
	}
}

public class FerghusExtDeliveryBasicPtjScript : FerghusExtDeliveryPtjBaseScript
{
	public override void Load()
	{
		SetId(507404);
		SetBasicLevelAndRewards();

		base.Load();
	}
}

public class FerghusExtDeliveryIntPtjScript : FerghusExtDeliveryPtjBaseScript
{
	public override void Load()
	{
		SetId(507434);
		SetIntLevelAndRewards();

		base.Load();
	}
}

public class FerghusExtDeliveryAdvPtjScript : FerghusExtDeliveryPtjBaseScript
{
	public override void Load()
	{
		SetId(507464);
		SetAdvLevelAndRewards();

		base.Load();
	}
}