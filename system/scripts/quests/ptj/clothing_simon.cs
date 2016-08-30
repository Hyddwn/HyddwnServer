#region TEMP_NOTES
// If you see this region in a pull request, 
// **I contain undefined values and should not be merged into master!**

// Class name resolver - go go gadget intellisense
using Aura.Mabi;
using Aura.Mabi.Const;
using System.Threading.Tasks;
using Aura.Channel.Scripting.Scripts;
using Aura.Channel.World.Entities;
using Aura.Channel.World.Quests;
using Aura.Shared.Util;
using System;
// Temporarily place this file under ChannelServer.csproj to resolve rest of dependencies

// PR Readiness Check: 
/* Run following regex searches for possibly improper Localization:
	* /LX?N?\((?!")/
		Non-string Localization call
	* /(?<!LX?N?\()".*?"(?!\w)/
		Unlocalised string //warn: You will get a lot of false-positives.
	* ".Format"
		Check for proper string parametrisation.
		All parts of the string must be exposed to Poedit.
	* /LX?N?\((.*?$\n.*?LX?N?\()+/m
		Consecutive Msg() calls?
		Join `L("a") ... L("b")` like so: `L("a<p/>b")`
 * Ensure rewards are in counting order with non-conflicting group IDs, per PTJ Quest ID.
 * Search ".Log" and ensure displayed messages are sufficiently descriptive (particularly, do specify the source of the message).
 */
#endregion

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