//--- Aura Script -----------------------------------------------------------
// Kristell's Church Part-Time Job
//--- Description -----------------------------------------------------------
// All quests used by the PTJ, and a script to handle the PTJ via hooks.
//--- Notes -----------------------------------------------------------------
// This script depends on ./church_endelyon.cs for egg collection PTJs.
// Please ensure this script loads afterward.
//
// The following dialogue is missing:
// * first time worker PTJ inquiry
// * first time accepting PTJ offer
// * first time declining PTJ offer
//---------------------------------------------------------------------------

public class KristellPtjScript : GeneralScript
{
	const PtjType JobType = PtjType.Church;

	const int Start = 12;
	const int Report = 16;
	const int Deadline = 21;
	const int PerDay = 10;

	int remaining = PerDay;

	readonly int[] QuestIds = new int[]
	{
		502101, // Basic  10 Potatoes
		502131, // Int    15 Potatoes
		502161, // Adv    20 Potatoes
		502104, // Basic   3 Apples
		502134, // Int     6 Apples
		502164, // Adv    10 Apples
		502105, // Basic  15 Eggs
		502135, // Int    20 Eggs
		502165, // Adv    30 Eggs
	};

	public override void Load()
	{
		AddHook("_kristell", "after_intro", AfterIntro);
		AddHook("_kristell", "before_keywords", BeforeKeywords);
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
			npc.Msg(L("The Church also needs workers.<br/>Please pay a visit here once you finish the current task."));
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
					npc.Msg(L("You have finished already?<br/>It is a little too early, so would you mind returning later?"));
				}
				else
				{
					npc.Msg(L("I trust that the assigned task is going well?<br/>The deadline is not past yet, so please do your best."));
				}
				return;
			}

			// Report?
			npc.Msg(L("It seems that you have completed the given task.<br/>If you would like, we can call it a day here."),
				npc.Button(L("Report Now"), "@report"),
				npc.Button(L("Report Later"), "@later")
				);

			if (await npc.Select() != "@report")
			{
				npc.Msg(L("If not right now, please make sure to report to me before the deadline.<br/>You have to at least report back to me even if you do not completely finish your task."));
				return;
			}

			// Nothing done
			if (result == QuestResult.None)
			{
				npc.GiveUpPtj();

				npc.Msg(npc.FavorExpression(), L("Oh, good heavens!<br/><username/>, I trusted you with this and this is all you have done. How disappointing.<br/>I cannot pay you for this."));
				npc.ModifyRelation(0, -Random(3), 0);
			}
			// Low~Perfect result
			else
			{
				npc.Msg(L("Mmm? I did not know you would do such a good job.<br/>You are a very meticulous worker, <username/>.<br/>I know this does not do justice for the excellent work you have done, but<br/>I have prepared a few things as a token of my gratitude. Please, take your pick."),
					npc.Button(L("Report Later"), "@later"),
					npc.PtjReport(result)
					);
				var reply = await npc.Select();

				// Report later
				if (!reply.StartsWith("@reward:"))
				{
					npc.Msg(npc.FavorExpression(), L("You seem to be busy all the time, <username/>."));
					return;
				}

				// Complete
				npc.CompletePtj(reply);
				remaining--;

				// Result msg
				if (result == QuestResult.Perfect)
				{
					npc.Msg(npc.FavorExpression(), L("Great! You have done very well.<br/>Here is the Holy Water as promised."));
					npc.ModifyRelation(0, Random(3), 0);
				}
				else if (result == QuestResult.Mid)
				{
					npc.Msg(npc.FavorExpression(), L("Thank you for your help.<br/>But, it is a little less than what was asked for.<br/>Anyway, I will pay you for what has been completed."));
					npc.ModifyRelation(0, Random(1), 0);
				}
				else if (result == QuestResult.Low)
				{
					npc.Msg(npc.FavorExpression(), L("Hmm. You have not adequately completed your work.<br/>Did you not have enough time?<br/>I cannot give you the Holy Water, then."));
					npc.ModifyRelation(0, -Random(2), 0);
				}
			}
			return;
		}

		// Check if PTJ time
		if (!npc.ErinnHour(Start, Deadline))
		{
			npc.Msg(L("Oh, no. It is not time for Church duties yet.<br/>Would you return later?"));
			return;
		}

		// Check if not done today and if there are jobs remaining
		if (!npc.CanDoPtj(JobType, remaining))
		{
			npc.Msg(L("Today's work has been completed.<br/>Only one task is given to one person per day.<br/>Please return tomorrow."));
			return;
		}

		// Offer PTJ
		var randomPtj = npc.RandomPtj(JobType, QuestIds);
		var msg = "";

		if (npc.GetPtjDoneCount(JobType) == 0)
			msg = L("(missing): first time worker PTJ inquiry");
		else
			msg = L("Do you want to work at the Church again today?<br/>Please take a look at the work details before you decide.");

		npc.Msg(msg, npc.PtjDesc(randomPtj,
			L("Kristell's Church Part-Time Job"),
			L("Looking for help with delivering goods to the Church."),
			PerDay, remaining, npc.GetPtjDoneCount(JobType)));

		if (await npc.Select() == "@accept")
		{
			if (npc.GetPtjDoneCount(JobType) == 0)
				npc.Msg(L("(missing): first time accepting PTJ offer"));
			else
				npc.Msg(L("Thank you.<br/>I hope you finish it in time."));

			npc.StartPtj(randomPtj);
		}
		else
		{
			if (npc.GetPtjDoneCount(JobType) == 0)
				npc.Msg(L("(missing): first time declining PTJ offer"));
			else
				npc.Msg(L("Are you busy with something else?<br/>If not today, please give me a hand later."));
		}
	}
}

public abstract class KristellPtjBaseScript : QuestScript
{
	protected abstract int QuestId { get; }
	protected abstract string QuestDescription { get; }
	protected abstract QuestLevel QuestLevel { get; }
	protected abstract string ObjectiveDescription { get; }
	protected abstract int ItemId { get; }
	protected abstract int ItemCount { get; }

	public override void Load()
	{
		SetId(QuestId);
		SetName(L("Church Part-Time Job"));
		SetDescription(QuestDescription);

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.ById);

		SetType(QuestType.Deliver);
		SetPtjType(PtjType.Church);
		SetLevel(QuestLevel);
		SetHours(start: 12, report: 16, deadline: 21);

		AddObjective("ptj", ObjectiveDescription, 0, 0, 0, Collect(ItemId, ItemCount));

		// Rewards common among all PTJs
		switch (QuestLevel)
		{
			case QuestLevel.Basic:
				AddReward(1, RewardGroupType.Item, QuestResult.Perfect, Item(63016, 4)); // Holy Water of Lymilark
				AddReward(1, RewardGroupType.Item, QuestResult.Perfect, Exp(200));
				AddReward(1, RewardGroupType.Item, QuestResult.Mid, Item(63016, 2)); // Holy Water of Lymilark
				AddReward(1, RewardGroupType.Item, QuestResult.Mid, Exp(100));
				AddReward(1, RewardGroupType.Item, QuestResult.Low, Item(63016, 1)); // Holy Water of Lymilark
				AddReward(1, RewardGroupType.Item, QuestResult.Low, Exp(40));
				break;

			case QuestLevel.Int:
				AddReward(1, RewardGroupType.Item, QuestResult.Perfect, Item(63016, 6)); // Holy Water of Lymilark
				AddReward(1, RewardGroupType.Item, QuestResult.Perfect, Exp(300));
				AddReward(1, RewardGroupType.Item, QuestResult.Mid, Item(63016, 3)); // Holy Water of Lymilark
				AddReward(1, RewardGroupType.Item, QuestResult.Mid, Exp(150));
				AddReward(1, RewardGroupType.Item, QuestResult.Low, Item(63016, 1)); // Holy Water of Lymilark
				AddReward(1, RewardGroupType.Item, QuestResult.Low, Exp(60));
				break;

			case QuestLevel.Adv:
				AddReward(1, RewardGroupType.Item, QuestResult.Perfect, Item(63016, 10)); // Holy Water of Lymilark
				AddReward(1, RewardGroupType.Item, QuestResult.Perfect, Exp(500));
				AddReward(1, RewardGroupType.Item, QuestResult.Mid, Item(63016, 5)); // Holy Water of Lymilark
				AddReward(1, RewardGroupType.Item, QuestResult.Mid, Exp(250));
				AddReward(1, RewardGroupType.Item, QuestResult.Low, Item(63016, 2)); // Holy Water of Lymilark
				AddReward(1, RewardGroupType.Item, QuestResult.Low, Exp(100));

				AddReward(2, RewardGroupType.Item, QuestResult.Perfect, Item(40004)); // Lute
				AddReward(2, RewardGroupType.Item, QuestResult.Perfect, Item(19001)); // Robe
				break;

			default: // Fallback
				Log.Error("Quest ID {0} has no set quest level. Fell back to basic rewards.", QuestId);
				goto case QuestLevel.Basic;
		}
	}
}

public abstract class KristellPotatoPtjBaseScript : KristellPtjBaseScript
{
	protected override string QuestDescription
	{
		get
		{
			return string.Format(LN(
				"This job is to harvest vegetables from the farmland. Today, dig up [{0} Potato]. Use a weeding hoe to gather potatoes from the fields around town.",
				"This job is to harvest vegetables from the farmland. Today, dig up [{0} Potatoes]. Use a weeding hoe to gather potatoes from the fields around town.",
				ItemCount), ItemCount);
		}
	}
	protected override string ObjectiveDescription { get { return string.Format(LN("Harvest {0} Potato", "Harvest {0} Potatoes", ItemCount), ItemCount); } }
	protected override int ItemId { get { return 50010; } }
}

public class KristellPotatoBasicPtjScript : KristellPotatoPtjBaseScript
{
	protected override int QuestId { get { return 502101; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Basic; } }
	protected override int ItemCount { get { return 10; } }
}

public class KristellPotatoIntPtjScript : KristellPotatoPtjBaseScript
{
	protected override int QuestId { get { return 502131; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Int; } }
	protected override int ItemCount { get { return 15; } }
}

public class KristellPotatoAdvPtjScript : KristellPotatoPtjBaseScript
{
	protected override int QuestId { get { return 502161; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Adv; } }
	protected override int ItemCount { get { return 20; } }
}

public abstract class KristellApplePtjBaseScript : KristellPtjBaseScript
{
	protected override string QuestDescription
	{
		get
		{
			return string.Format(LN(
				"This job is to gather fruit from the outskirts of the town. Today, gather [{0} Apple]. Gather apples from apple trees on the outskirts of the town.",
				"This job is to gather fruit from the outskirts of the town. Today, gather [{0} Apples]. Gather apples from apple trees on the outskirts of the town.",
				ItemCount), ItemCount);
		}
	}
	protected override string ObjectiveDescription { get { return string.Format(LN("Harvest {0} Apple", "Harvest {0} Apples", ItemCount), ItemCount); } }
	protected override int ItemId { get { return 50003; } }
}

public class KristellAppleBasicPtjScript : KristellApplePtjBaseScript
{
	protected override int QuestId { get { return 502104; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Basic; } }
	protected override int ItemCount { get { return 3; } }
}

public class KristellAppleIntPtjScript : KristellApplePtjBaseScript
{
	protected override int QuestId { get { return 502134; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Int; } }
	protected override int ItemCount { get { return 6; } }
}

public class KristellAppleAdvPtjScript : KristellApplePtjBaseScript
{
	protected override int QuestId { get { return 502164; } }
	protected override QuestLevel QuestLevel { get { return QuestLevel.Adv; } }
	protected override int ItemCount { get { return 10; } }
}

// The following quest IDs are already defined in ./church_endelyon.cs:
/* 502105, // Basic  15 Eggs
 * 502135, // Int    20 Eggs
 * 502165, // Adv    30 Eggs
 */