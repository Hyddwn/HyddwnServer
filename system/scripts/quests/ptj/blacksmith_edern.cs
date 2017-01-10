//--- Aura Script -----------------------------------------------------------
// Edern's Blacksmith's Shop Part-Time Job
//--- Description -----------------------------------------------------------
// All quests used by the PTJ, and a script to handle the PTJ via hooks.
//--- Notes -----------------------------------------------------------------
// Warning: Following dialogue is missing:
// * first time worker PTJ inquiry
// * first time accepting PTJ offer
// * first time declining PTJ offer
//---------------------------------------------------------------------------

using ItemEntity = Aura.Channel.World.Entities.Item; // Conflicts with QuestScript.Item(...)

public class EdernPtjScript : GeneralScript
{
	const PtjType JobType = PtjType.BlacksmithShop;

	const int Start = 7;
	const int Report = 12;
	const int Deadline = 19;
	const int PerDay = 5;

	int remaining = PerDay;

	private class QuestIdSkillRankPairs : List<Tuple<int, SkillRank>>
	{
		public void Add(int questId, SkillRank skillRank)
		{
			Add(new Tuple<int, SkillRank>(questId, skillRank));
		}
	}

	readonly QuestIdSkillRankPairs QuestIdSkillRankList = new QuestIdSkillRankPairs
	{
		{507201, SkillRank.RF}, // Basic  Smith 2 Weeding Hoes
		{507231, SkillRank.RF}, // Int    Smith 2 Weeding Hoes
		{507261, SkillRank.RF}, // Adv    Smith 2 Weeding Hoes
		{507202, SkillRank.RF}, // Basic  Smith 2 Sickles
		{507232, SkillRank.RF}, // Int    Smith 2 Sickles
		{507262, SkillRank.RF}, // Adv    Smith 2 Sickles
		{507203, SkillRank.RC}, // Basic  Smith 2 Round Shields
		{507204, SkillRank.RB}, // Basic  Smith 2 Evil Dying Crowns
		{507264, SkillRank.RB}, // Adv    Smith 2 Evil Dying Crowns
		{507205, SkillRank.RC}, // Basic  Smith 2 Cuirassier Helms
		{507236, SkillRank.RB}, // Int    Smith 2 Arish Ashuvain Gauntlets
		{507237, SkillRank.RA}, // Int    Smith 2 Plate Gauntlets
		{507238, SkillRank.RB}, // Int    Smith 2 Vito Crux Greaves
		{507268, SkillRank.RB}, // Adv    Smith 2 Vito Crux Greaves
		{507239, SkillRank.RA}, // Int    Smith 2 Arish Ashuvain Boots (M)
		{507269, SkillRank.RA}, // Adv    Smith 2 Arish Ashuvain Boots (M)
		{507270, SkillRank.RA}, // Adv    Smith 1 Scale Armor
		{507271, SkillRank.RA}, // Adv    Smith 1 Giant Half Guard Leather Armor (F)
	};

	public override void Load()
	{
		AddHook("_edern", "after_intro", AfterIntro);
		AddHook("_edern", "before_keywords", BeforeKeywords);
	}

	public async Task<HookResult> AfterIntro(NpcScript npc, params object[] args)
	{
		// Call PTJ method after intro if it's time to report
		if (npc.Player.IsDoingPtjFor(npc.NPC) && ErinnHour(Report, Deadline))
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
		if (keyword == "about_arbeit" && IsEnabled("EdernPtj"))
		{
			await AboutArbeit(npc);
			await npc.Conversation();
			npc.End();

			return HookResult.End;
		}

		return HookResult.Continue;
	}

	/// <summary>
	/// Returns a random quest ID from QuestIdSkillRankList,
	/// based on the current Erinn day and the player's blacksmithing
	/// PTJ level/skill rank (via <paramref name="npc"/>).
	/// </summary>
	/// <param name="npc"></param>
	/// <returns>
	/// Will always return a quest ID of the given <paramref name="level"/>.
	/// However, if no job of the matching rank can be found,
	/// a job of next-highest rank will be returned.
	/// </returns>
	/// <remarks>
	/// As there are (at the time of writing) only jobs of rank F, C, B, and A,
	/// the ranks that are supplied to and returned from this method are as follows:
	/// <list type="table">
	/// 	<listheader> <term>Supplied Rank</term> <term>Returned Rank</term> </listheader>
	/// 	<item>       <term>(Unlearned)</term>   <term>rF</term>                </item>
	/// 	<item>       <term>rN ~ rD</term>       <term>rF</term>                </item>
	/// 	<item>       <term>rC ~ rA</term>       <term>[Same as supplied]</term></item>
	/// 	<item>       <term>r9 and up</term>     <term>rA</term>                </item>
	/// </list>
	///
	/// See also: http://wiki.mabinogiworld.com/view/Thread:Talk:Edern/Part-time_job_requests
	/// </remarks>
	public int RandomPtj(NpcScript npc)
	{
		// Determine player's Blacksmithing skill level
		var playerSkills = npc.Player.Skills;
		var skillRank = playerSkills.Has(SkillId.Blacksmithing)
			? playerSkills.Get(SkillId.Blacksmithing).Info.Rank
			: SkillRank.RF; // Default to RF jobs if player does not know Blacksmithing.
		var ptjQuestLevel = npc.Player.GetPtjQuestLevel(JobType);

		Func<SkillRank, IEnumerable<int>> GetSameRankQuests = r => QuestIdSkillRankList
			.Where(pair => pair.Item2 == r) // Filter on skill rank
			.Select(pair => pair.Item1); // Get resulting quest IDs
		Func<IEnumerable<int>, int> GetRandomIdOfTheDay = ids => ids.ElementAt(new Random(ErinnTime.Now.DateTimeStamp).Next(ids.Count()));

		var rankProbe = skillRank;
		var level = npc.Player.GetPtjQuestLevel(JobType);
		var sameLevelQuestIds = GetLevelMatchingQuestIds(level, JobType, QuestIdSkillRankList.Select(pair => pair.Item1).ToArray());

		IEnumerable<int> matchingQuestIds;
		// Clamp on rank A, the most difficult job available.
		if (rankProbe > SkillRank.RA)
			rankProbe = SkillRank.RA;
		// Filter on skill rank, retrying on a lower rank if no results.
		while (rankProbe >= SkillRank.RC)
		{
			// Merge with filter on sameLevelQuestIds.
			matchingQuestIds = GetSameRankQuests(rankProbe).Where(id => sameLevelQuestIds.Contains(id));

			if (matchingQuestIds.Any())
				return GetRandomIdOfTheDay(matchingQuestIds);
			else
				--rankProbe; // Retry on lower rank.
		}
		// Else no matching jobs at A, B, or C.

		// Try rank F jobs?
		matchingQuestIds = GetSameRankQuests(SkillRank.RF).Where(id => sameLevelQuestIds.Contains(id));
		if (matchingQuestIds.Any())
			return GetRandomIdOfTheDay(matchingQuestIds);
		// Else no matching jobs at F.
		// If this point is reached,
		// we were not able to find a quest for the player.
		throw new Exception(string.Format("EdernPtjScript.RandomPtj: Unable to provide a quest for level:{0}, rank:{1}", ptjQuestLevel, skillRank));
	}

	public async Task AboutArbeit(NpcScript npc)
	{
		// Check if already doing another PTJ
		if (npc.Player.IsDoingPtjNotFor(npc.NPC))
		{
			npc.Msg(L("Tasks at the Blacksmith's Shop aren't as easy as you think.<br/>Come back after you finish what you're doing."));
			return;
		}

		// Check if PTJ is in progress
		if (npc.Player.IsDoingPtjFor(npc.NPC))
		{
			var result = npc.Player.GetPtjResult();

			// Check if report time
			if (!ErinnHour(Report, Deadline))
			{
				if (result == QuestResult.Perfect)
				{
					npc.Msg(L("Did you finish early?<br/>I'm busy right now. Come see me after the deadline."));
				}
				else
				{
					npc.Msg(L("Are you doing what you're supposed to be doing?<p/>Don't get lazy now. Make sure you take care of your work."));
				}
				return;
			}

			// Report?
			npc.Msg(L("Did you finish today's work?<br/>Yeah? Then give me a report and wrap it up.<br/>Let's see how well you did."),
				npc.Button(L("Report Now"), "@report"),
				npc.Button(L("Report Later"), "@later")
				);

			if (await npc.Select() != "@report")
			{
				npc.Msg(L("If you have time to waste talking to me,<br/>hurry up and finish your work."));
				return;
			}

			// Nothing done
			if (result == QuestResult.None)
			{
				npc.Player.GiveUpPtj();

				npc.Msg(npc.FavorExpression(), L("Leave.<br/>Don't ever come work for me again."));
				npc.ModifyRelation(0, -Random(3), 0);
			}
			// Low~Perfect result
			else
			{
				npc.Msg(L("Wow, I'm impressed. Well done.<br/>Good work deserves a reward. Here, pick the item you want."),
					npc.Button(L("Report Later"), "@later"),
					npc.PtjReport(result)
					);
				var reply = await npc.Select();

				// Report later
				if (!reply.StartsWith("@reward:"))
				{
					npc.Msg(npc.FavorExpression(), L("I'm working. Don't bother me if you've got nothing to say."));
					return;
				}

				// Complete
				npc.Player.CompletePtj(reply);
				remaining--;

				// Result msg
				if (result == QuestResult.Perfect)
				{
					npc.Msg(npc.FavorExpression(), L("This'll do.<br/>Good job."));
					npc.ModifyRelation(0, Random(3), 0);
				}
				else if (result == QuestResult.Mid)
				{
					npc.Msg(npc.FavorExpression(), L("Not great...<br/>But that's probably the best someone of your age could do.<br/>You did okay, but I can't pay you the entire amount I promised."));
					npc.ModifyRelation(0, Random(1), 0);
				}
				else if (result == QuestResult.Low)
				{
					npc.Msg(npc.FavorExpression(), L("They say you can tell a lot about a person by the type of armor they wear.<br/>I guess they're right in this case."));
					npc.ModifyRelation(0, -Random(2), 0);
				}
			}
			return;
		}

		// Check if PTJ time
		if (!ErinnHour(Start, Deadline))
		{
			npc.Msg(L("Come back at the deadline."));
			return;
		}

		// Check if not done today and if there are jobs remaining
		if (!npc.Player.CanDoPtj(JobType, remaining))
		{
			npc.Msg(L("That's enough for today.<br/>Come back tomorrow."));
			return;
		}

		// Offer PTJ
		var randomPtj = RandomPtj(npc);
		var msg = "";

		if (npc.Player.GetPtjDoneCount(JobType) == 0)
			msg = L("(missing): first time worker PTJ inquiry");
		else
			msg = L("Are you looking for work? I just happen to have the perfect job for you.");

		npc.Msg(msg, npc.PtjDesc(randomPtj,
			L("Edern's Blacksmith's Shop Part-Time Job"),
			L("Looking for help with crafting items needed for Blacksmith Shop."),
			PerDay, remaining, npc.Player.GetPtjDoneCount(JobType)));

		if (await npc.Select() == "@accept")
		{
			if (npc.Player.GetPtjDoneCount(JobType) == 0)
				npc.Msg(L("(missing): first time accepting PTJ offer"));
			else
				npc.Msg(L("Do it right."));

			npc.Player.StartPtj(randomPtj, npc.NPC.Name);
		}
		else
		{
			if (npc.Player.GetPtjDoneCount(JobType) == 0)
				npc.Msg(L("(missing): first time declining PTJ offer"));
			else
				npc.Msg(L("Don't bother me. I'm a busy person."));
		}
	}
}

public abstract class EdernSmithPtjBaseScript : QuestScript
{
	protected abstract int QuestId { get; }
	protected abstract string LQuestDescription { get; }
	protected abstract int ItemId { get; }
	protected virtual int ItemCount { get { return 2; } }
	protected abstract string LCreateObjectiveDescription { get; }
	protected abstract string LCollectObjectiveDescription { get; }
	protected abstract QuestLevel QuestLevel { get; }
	protected abstract void AddRewards();

	public override void Load()
	{
		SetId(QuestId);
		SetName(L("Blacksmith's Shop Part-Time Job"));
		SetDescription(LQuestDescription);

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.ById);

		SetType(QuestType.Deliver);
		SetPtjType(PtjType.BlacksmithShop);
		SetLevel(QuestLevel);
		SetHours(start: 7, report: 12, deadline: 19);

		AddObjective("ptj1", LCreateObjectiveDescription, 0, 0, 0, Create(ItemId, ItemCount, SkillId.Blacksmithing));
		AddObjective("ptj2", LCollectObjectiveDescription, 0, 0, 0, Collect(ItemId, ItemCount));

		AddRewards();
	}
}

public abstract class EdernSmithBasicPtjBaseScript : EdernSmithPtjBaseScript
{
	protected override QuestLevel QuestLevel { get { return QuestLevel.Basic; } }

	protected override void AddRewards()
	{
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

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(64036, 10)); // Iron Ore Fragment
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(315));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(64038, 5)); // Silver Ore Fragment
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(565));

		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(64039, 1)); // Gold Ore Fragment
		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(765));
	}
}

public abstract class EdernSmithIntPtjBaseScript : EdernSmithPtjBaseScript
{
	protected override QuestLevel QuestLevel { get { return QuestLevel.Int; } }

	protected override void AddRewards()
	{
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(300));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(1000));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(150));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(500));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(60));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(200));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(100));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(1100));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(87));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(327));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(35));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(131));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(1300));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(250));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(650));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(125));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(260));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(50));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(40024)); // Blacksmith Hammer
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(250));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(64036, 10)); // Iron Ore Fragment
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(750));

		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(64038, 5)); // Silver Ore Fragment
		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(1000));
	}
}

public abstract class EdernSmithAdvPtjBaseScript : EdernSmithPtjBaseScript
{
	protected override QuestLevel QuestLevel { get { return QuestLevel.Adv; } }

	protected override void AddRewards()
	{
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Exp(400));
		AddReward(1, RewardGroupType.Gold, QuestResult.Perfect, Gold(1200));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Exp(200));
		AddReward(1, RewardGroupType.Gold, QuestResult.Mid, Gold(600));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Exp(80));
		AddReward(1, RewardGroupType.Gold, QuestResult.Low, Gold(240));

		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Exp(365));
		AddReward(2, RewardGroupType.Gold, QuestResult.Perfect, Gold(1225));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Exp(182));
		AddReward(2, RewardGroupType.Gold, QuestResult.Mid, Gold(612));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Exp(73));
		AddReward(2, RewardGroupType.Gold, QuestResult.Low, Gold(245));

		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Exp(1600));
		AddReward(3, RewardGroupType.Exp, QuestResult.Perfect, Gold(300));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Exp(800));
		AddReward(3, RewardGroupType.Exp, QuestResult.Mid, Gold(150));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Exp(320));
		AddReward(3, RewardGroupType.Exp, QuestResult.Low, Gold(60));

		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Item(40024)); // Blacksmith Hammer
		AddReward(4, RewardGroupType.Item, QuestResult.Perfect, Gold(525));

		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Item(64036, 10)); // Iron Ore Fragment
		AddReward(5, RewardGroupType.Item, QuestResult.Perfect, Gold(1025));

		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Item(64038, 5)); // Silver Ore Fragment
		AddReward(6, RewardGroupType.Item, QuestResult.Perfect, Gold(1025));
	}
}

public class EdernSmithWeedingHoeBasicPtjScript : EdernSmithBasicPtjBaseScript
{
	protected override int QuestId { get { return 507201; } }
	protected override string LQuestDescription { get { return L("This job involves creating equipment to supply the Blacksmith's Shop. Today's task is creating the [Weeding Hoe], using the materials given for this part-time job. Deadline starts at noon. Be careful not to deliver them before the deadline since the final work doesn't begin until then."); } }
	protected override int ItemId { get { return 60801; } }
	protected override string LCreateObjectiveDescription { get { return L("Make 2 Weeding Hoes (Part-Time Job)"); } }
	protected override string LCollectObjectiveDescription { get { return L("2 Weeding Hoes (Part-Time Job)"); } }

	public override void OnReceive(Creature creature)
	{
		creature.GiveItem(ItemEntity.CreatePattern(60800, 30001, 10)); // Part-Time Job Blacksmithing Manual - Weeding Hoe
		creature.GiveItem(60812, 2); // Firewood (Part-Time Job)
		creature.GiveItem(60815, 5); // Iron Ingot (Part-Time Job)
	}
}

public class EdernSmithSickleBasicPtjScript : EdernSmithBasicPtjBaseScript
{
	protected override int QuestId { get { return 507202; } }
	protected override string LQuestDescription { get { return L("This job involves creating equipment to supply the Blacksmith's Shop. Today's task is creating the [Sickle], using the materials given for this part-time job. Deadline starts at noon. Be careful not to deliver them before the deadline since the final work doesn't begin until then."); } }
	protected override int ItemId { get { return 60802; } }
	protected override string LCreateObjectiveDescription { get { return L("Make 2 Sickles (Part-Time Job)"); } }
	protected override string LCollectObjectiveDescription { get { return L("2 Sickles (Part-Time Job)"); } }

	public override void OnReceive(Creature creature)
	{
		creature.GiveItem(ItemEntity.CreatePattern(60800, 30002, 10)); // Part-Time Job Blacksmithing Manual - Sickle
		creature.GiveItem(60812, 5);  // Firewood (Part-Time Job)
		creature.GiveItem(60815, 10); // Iron Ingot (Part-Time Job)
	}
}

public class EdernSmithRoundShieldBasicPtjScript : EdernSmithBasicPtjBaseScript
{
	protected override int QuestId { get { return 507203; } }
	protected override string LQuestDescription { get { return L("This job involves creating equipment to supply the Blacksmith's Shop. Today's task is creating the [Round Shield], using the materials given for this part-time job. Deadline starts at noon. Be careful not to deliver them before the deadline since the final work doesn't begin until then."); } }
	protected override int ItemId { get { return 60803; } }
	protected override string LCreateObjectiveDescription { get { return L("Make 2 Round Shields (Part-Time Job)"); } }
	protected override string LCollectObjectiveDescription { get { return L("2 Round Shields (Part-Time Job)"); } }

	public override void OnReceive(Creature creature)
	{
		creature.GiveItem(ItemEntity.CreatePattern(60800, 30003, 15)); // Part-Time Job Blacksmithing Manual - Round Shield
		creature.GiveItem(60812, 4);  // Firewood (Part-Time Job)
		creature.GiveItem(60428, 2);  // Common Leather Strap (Part-Time Job)
		creature.GiveItem(60815, 20); // Iron Ingot (Part-Time Job)
		creature.GiveItem(60815, 20); // Iron Ingot (Part-Time Job)
		creature.GiveItem(60815, 20); // Iron Ingot (Part-Time Job)
	}
}

public class EdernSmithEvilDyingCrownBasicPtjScript : EdernSmithBasicPtjBaseScript
{
	protected override int QuestId { get { return 507204; } }
	protected override string LQuestDescription { get { return L("This job involves creating equipment to supply the Blacksmith's Shop. Today's task is creating the [Evil Dying Crown], using the materials given for this part-time job. Deadline starts at noon. Be careful not to deliver them before the deadline since the final work doesn't begin until then."); } }
	protected override int ItemId { get { return 60804; } }
	protected override string LCreateObjectiveDescription { get { return L("Make 2 Evil Dying Crowns (Part-Time Job)"); } }
	protected override string LCollectObjectiveDescription { get { return L("2 Evil Dying Crowns (Part-Time Job)"); } }

	public override void OnReceive(Creature creature)
	{
		creature.GiveItem(ItemEntity.CreatePattern(60800, 30004, 20)); // Part-Time Job Blacksmithing Manual - Evil Dying Crown
		creature.GiveItem(60420, 2);  // Common Fabric (Part-Time Job)
		creature.GiveItem(60412, 2);  // Common Silk (Part-Time Job)
		creature.GiveItem(60428, 10); // Common Leather Strap (Part-Time Job)
		creature.GiveItem(60428, 10); // Common Leather Strap (Part-Time Job)
		creature.GiveItem(60815, 20); // Iron Ingot (Part-Time Job)
		creature.GiveItem(60815, 20); // Iron Ingot (Part-Time Job)
		creature.GiveItem(60815, 20); // Iron Ingot (Part-Time Job)
		creature.GiveItem(60815, 20); // Iron Ingot (Part-Time Job)
		creature.GiveItem(60815, 10); // Iron Ingot (Part-Time Job)
	}
}

public class EdernSmithCuirassierHelmBasicPtjScript : EdernSmithBasicPtjBaseScript
{
	protected override int QuestId { get { return 507205; } }
	protected override string LQuestDescription { get { return L("This job involves creating equipment to supply the Blacksmith's Shop. Today's task is creating the [Cuirassier Helm], using the materials given for this part-time job. Deadline starts at noon. Be careful not to deliver them before the deadline since the final work doesn't begin until then."); } }
	protected override int ItemId { get { return 60805; } }
	protected override string LCreateObjectiveDescription { get { return L("Make 2 Cuirassier Helms (Part-Time Job)"); } }
	protected override string LCollectObjectiveDescription { get { return L("2 Cuirassier Helms (Part-Time Job)"); } }

	public override void OnReceive(Creature creature)
	{
		creature.GiveItem(ItemEntity.CreatePattern(60800, 30005, 15)); // Part-Time Job Blacksmithing Manual - Cuirassier Helm
		creature.GiveItem(60424, 4);  // Common Leather (Part-Time Job)
		creature.GiveItem(60815, 20); // Iron Ingot (Part-Time Job)
		creature.GiveItem(60815, 20); // Iron Ingot (Part-Time Job)
	}
}
public class EdernSmithWeedingHoeIntPtjScript : EdernSmithIntPtjBaseScript
{
	protected override int QuestId { get { return 507231; } }
	protected override string LQuestDescription { get { return L("This job involves creating equipment to supply the Blacksmith's Shop. Today's task is creating the [Weeding Hoe], using the materials given for this part-time job. Deadline starts at noon. Be careful not to deliver them before the deadline since the final work doesn't begin until then."); } }
	protected override int ItemId { get { return 60801; } }
	protected override string LCreateObjectiveDescription { get { return L("Make 2 Weeding Hoes (Part-Time Job)"); } }
	protected override string LCollectObjectiveDescription { get { return L("2 Weeding Hoes (Part-Time Job)"); } }

	public override void OnReceive(Creature creature)
	{
		creature.GiveItem(ItemEntity.CreatePattern(60800, 30001, 10)); // Part-Time Job Blacksmithing Manual - Weeding Hoe
		creature.GiveItem(60812, 2); // Firewood (Part-Time Job)
		creature.GiveItem(60815, 5); // Iron Ingot (Part-Time Job)
	}
}

public class EdernSmithSickleIntPtjScript : EdernSmithIntPtjBaseScript
{
	protected override int QuestId { get { return 507232; } }
	protected override string LQuestDescription { get { return L("This job involves creating equipment to supply the Blacksmith's Shop. Today's task is creating the [Sickle], using the materials given for this part-time job. Deadline starts at noon. Be careful not to deliver them before the deadline since the final work doesn't begin until then."); } }
	protected override int ItemId { get { return 60802; } }
	protected override string LCreateObjectiveDescription { get { return L("Make 2 Sickles (Part-Time Job)"); } }
	protected override string LCollectObjectiveDescription { get { return L("2 Sickles (Part-Time Job)"); } }

	public override void OnReceive(Creature creature)
	{
		creature.GiveItem(ItemEntity.CreatePattern(60800, 30002, 10)); // Part-Time Job Blacksmithing Manual - Sickle
		creature.GiveItem(60812, 5);  // Firewood (Part-Time Job)
		creature.GiveItem(60815, 10); // Iron Ingot (Part-Time Job)
	}
}

public class EdernSmithArishAshuvainGauntletsIntPtjScript : EdernSmithIntPtjBaseScript
{
	protected override int QuestId { get { return 507236; } }
	protected override string LQuestDescription { get { return L("This job involves creating equipment to supply the Blacksmith's Shop. Today's task is creating [Arish Ashuvain Gauntlets], using the materials given for this part-time job. Deadline starts at noon. Be careful not to deliver them before the deadline since the final work doesn't begin until then."); } }
	protected override int ItemId { get { return 60806; } }
	protected override string LCreateObjectiveDescription { get { return L("Make 2 Arish Ashuvain Gauntlets (Part-Time Job)"); } }
	protected override string LCollectObjectiveDescription { get { return L("2 Arish Ashuvain Gauntlets (Part-Time Job)"); } }

	public override void OnReceive(Creature creature)
	{
		creature.GiveItem(ItemEntity.CreatePattern(60800, 30006, 10)); // Part-Time Job Blacksmithing Manual - Arish Ashuvain Gauntlets
		creature.GiveItem(60411, 2);  // Cheap Silk (Part-Time Job)
		creature.GiveItem(60815, 20); // Iron Ingot (Part-Time Job)
		creature.GiveItem(60815, 5);  // Iron Ingot (Part-Time Job)
		creature.GiveItem(60428, 10); // Common Leather Strap (Part-Time Job)
		creature.GiveItem(60428, 10); // Common Leather Strap (Part-Time Job)
		creature.GiveItem(60428, 5);  // Common Leather Strap (Part-Time Job)
	}
}

public class EdernSmithPlateGauntletsIntPtjScript : EdernSmithIntPtjBaseScript
{
	protected override int QuestId { get { return 507237; } }
	protected override string LQuestDescription { get { return L("This job involves creating equipment to supply the Blacksmith's Shop. Today's task is creating [Plate Gauntlets], using the materials given for this part-time job. Deadline starts at noon. Be careful not to deliver them before the deadline since the final work doesn't begin until then."); } }
	protected override int ItemId { get { return 60807; } }
	protected override string LCreateObjectiveDescription { get { return L("Make 2 Plate Gauntlets (Part-Time Job)"); } }
	protected override string LCollectObjectiveDescription { get { return L("2 Plate Gauntlets (Part-Time Job)"); } }

	public override void OnReceive(Creature creature)
	{
		creature.GiveItem(ItemEntity.CreatePattern(60800, 30007, 20)); // Part-Time Job Blacksmithing Manual - Plate Gauntlets
		creature.GiveItem(60425, 2);  // Fine Leather (Part-Time Job)
		creature.GiveItem(60406, 2);  // Thick Thread Ball (Part-Time Job)
		creature.GiveItem(60815, 20); // Iron Ingot (Part-Time Job)
		creature.GiveItem(60815, 20); // Iron Ingot (Part-Time Job)
		creature.GiveItem(60815, 20); // Iron Ingot (Part-Time Job)
		creature.GiveItem(60815, 20); // Iron Ingot (Part-Time Job)
		creature.GiveItem(60815, 20); // Iron Ingot (Part-Time Job)
	}
}

public class EdernSmithVitoCruxGreavesIntPtjScript : EdernSmithIntPtjBaseScript
{
	protected override int QuestId { get { return 507238; } }
	protected override string LQuestDescription { get { return L("This job involves creating equipment to supply the Blacksmith's Shop. Today's task is creating [Vito Crux Greaves], using the materials given for this part-time job. Deadline starts at noon. Be careful not to deliver them before the deadline since the final work doesn't begin until then."); } }
	protected override int ItemId { get { return 60808; } }
	protected override string LCreateObjectiveDescription { get { return L("Make 2 Vito Crux Greaves (Part-Time Job)"); } }
	protected override string LCollectObjectiveDescription { get { return L("2 Vito Crux Greaves (Part-Time Job)"); } }

	public override void OnReceive(Creature creature)
	{
		creature.GiveItem(ItemEntity.CreatePattern(60800, 30008, 15)); // Part-Time Job Blacksmithing Manual - Vito Crux Greaves
		creature.GiveItem(60425, 2);  // Fine Leather (Part-Time Job)
		creature.GiveItem(60404, 5);  // Braid (Part-Time Job)
		creature.GiveItem(60404, 5);  // Braid (Part-Time Job)
		creature.GiveItem(60815, 20); // Iron Ingot (Part-Time Job)
		creature.GiveItem(60815, 5);  // Iron Ingot (Part-Time Job)
		creature.GiveItem(60816, 20); // Copper Ingot (Part-Time Job)
		creature.GiveItem(60816, 20); // Copper Ingot (Part-Time Job)
		creature.GiveItem(60816, 10); // Copper Ingot (Part-Time Job)
	}
}

public class EdernSmithArishAshuvainBootsMIntPtjScript : EdernSmithIntPtjBaseScript
{
	protected override int QuestId { get { return 507239; } }
	protected override string LQuestDescription { get { return L("This job involves creating equipment to supply the Blacksmith's Shop. Today's task is creating [Arish Ashuvain Boots (M)], using the materials given for this part-time job. Deadline starts at noon. Be careful not to deliver them before the deadline since the final work doesn't begin until then."); } }
	protected override int ItemId { get { return 60809; } }
	protected override string LCreateObjectiveDescription { get { return L("Make 2 Arish Ashuvain Boots (M) (Part-Time Job)"); } }
	protected override string LCollectObjectiveDescription { get { return L("2 Arish Ashuvain Boots (M) (Part-Time Job)"); } }

	public override void OnReceive(Creature creature)
	{
		creature.GiveItem(ItemEntity.CreatePattern(60800, 30009, 10)); // Part-Time Job Blacksmithing Manual - Arish Ashuvain Boots (M)
		creature.GiveItem(60413, 2);  // Fine Silk (Part-Time Job)
		creature.GiveItem(60815, 20); // Iron Ingot (Part-Time Job)
		creature.GiveItem(60815, 20); // Iron Ingot (Part-Time Job)
		creature.GiveItem(60428, 10); // Common Leather Strap (Part-Time Job)
		creature.GiveItem(60428, 10); // Common Leather Strap (Part-Time Job)
		creature.GiveItem(60428, 10); // Common Leather Strap (Part-Time Job)
		creature.GiveItem(60428, 10); // Common Leather Strap (Part-Time Job)
	}
}

public class EdernSmithWeedingHoeAdvPtjScript : EdernSmithAdvPtjBaseScript
{
	protected override int QuestId { get { return 507261; } }
	protected override string LQuestDescription { get { return L("This job involves creating equipment to supply the Blacksmith's Shop. Today's task is creating the [Weeding Hoe], using the materials given for this part-time job. Deadline starts at noon. Be careful not to deliver them before the deadline since the final work doesn't begin until then."); } }
	protected override int ItemId { get { return 60801; } }
	protected override string LCreateObjectiveDescription { get { return L("Make 2 Weeding Hoes (Part-Time Job)"); } }
	protected override string LCollectObjectiveDescription { get { return L("2 Weeding Hoes (Part-Time Job)"); } }

	public override void OnReceive(Creature creature)
	{
		creature.GiveItem(ItemEntity.CreatePattern(60800, 30001, 10)); // Part-Time Job Blacksmithing Manual - Weeding Hoe
		creature.GiveItem(60812, 2); // Firewood (Part-Time Job)
		creature.GiveItem(60815, 5); // Iron Ingot (Part-Time Job)
	}
}

public class EdernSmithSickleAdvPtjScript : EdernSmithAdvPtjBaseScript
{
	protected override int QuestId { get { return 507262; } }
	protected override string LQuestDescription { get { return L("This job involves creating equipment to supply the Blacksmith's Shop. Today's task is creating the [Sickle], using the materials given for this part-time job. Deadline starts at noon. Be careful not to deliver them before the deadline since the final work doesn't begin until then."); } }
	protected override int ItemId { get { return 60802; } }
	protected override string LCreateObjectiveDescription { get { return L("Make 2 Sickles (Part-Time Job)"); } }
	protected override string LCollectObjectiveDescription { get { return L("2 Sickles (Part-Time Job)"); } }

	public override void OnReceive(Creature creature)
	{
		creature.GiveItem(ItemEntity.CreatePattern(60800, 30002, 10)); // Part-Time Job Blacksmithing Manual - Sickle
		creature.GiveItem(60812, 5);  // Firewood (Part-Time Job)
		creature.GiveItem(60815, 10); // Iron Ingot (Part-Time Job)
	}
}

public class EdernSmithEvilDyingCrownAdvPtjScript : EdernSmithAdvPtjBaseScript
{
	protected override int QuestId { get { return 507264; } }
	protected override string LQuestDescription { get { return L("This job involves creating equipment to supply the Blacksmith's Shop. Today's task is creating the [Evil Dying Crown], using the materials given for this part-time job. Deadline starts at noon. Be careful not to deliver them before the deadline since the final work doesn't begin until then."); } }
	protected override int ItemId { get { return 60804; } }
	protected override string LCreateObjectiveDescription { get { return L("Make 2 Evil Dying Crowns (Part-Time Job)"); } }
	protected override string LCollectObjectiveDescription { get { return L("2 Evil Dying Crowns (Part-Time Job)"); } }

	public override void OnReceive(Creature creature)
	{
		creature.GiveItem(ItemEntity.CreatePattern(60800, 30004, 20)); // Part-Time Job Blacksmithing Manual - Evil Dying Crown
		creature.GiveItem(60420, 2);  // Common Fabric (Part-Time Job)
		creature.GiveItem(60412, 2);  // Common Silk (Part-Time Job)
		creature.GiveItem(60428, 10); // Common Leather Strap (Part-Time Job)
		creature.GiveItem(60428, 10); // Common Leather Strap (Part-Time Job)
		creature.GiveItem(60815, 20); // Iron Ingot (Part-Time Job)
		creature.GiveItem(60815, 20); // Iron Ingot (Part-Time Job)
		creature.GiveItem(60815, 20); // Iron Ingot (Part-Time Job)
		creature.GiveItem(60815, 20); // Iron Ingot (Part-Time Job)
		creature.GiveItem(60815, 10); // Iron Ingot (Part-Time Job)
	}
}

public class EdernSmithVitoCruxGreavesAdvPtjScript : EdernSmithAdvPtjBaseScript
{
	protected override int QuestId { get { return 507268; } }
	protected override string LQuestDescription { get { return L("This job involves creating equipment to supply the Blacksmith's Shop. Today's task is creating [Vito Crux Greaves], using the materials given for this part-time job. Deadline starts at noon. Be careful not to deliver them before the deadline since the final work doesn't begin until then."); } }
	protected override int ItemId { get { return 60808; } }
	protected override string LCreateObjectiveDescription { get { return L("Make 2 Vito Crux Greaves (Part-Time Job)"); } }
	protected override string LCollectObjectiveDescription { get { return L("2 Vito Crux Greaves (Part-Time Job)"); } }

	public override void OnReceive(Creature creature)
	{
		creature.GiveItem(ItemEntity.CreatePattern(60800, 30008, 15)); // Part-Time Job Blacksmithing Manual - Vito Crux Greaves
		creature.GiveItem(60425, 2);  // Fine Leather (Part-Time Job)
		creature.GiveItem(60404, 5);  // Braid (Part-Time Job)
		creature.GiveItem(60404, 5);  // Braid (Part-Time Job)
		creature.GiveItem(60815, 20); // Iron Ingot (Part-Time Job)
		creature.GiveItem(60815, 5);  // Iron Ingot (Part-Time Job)
		creature.GiveItem(60816, 20); // Copper Ingot (Part-Time Job)
		creature.GiveItem(60816, 20); // Copper Ingot (Part-Time Job)
		creature.GiveItem(60816, 10); // Copper Ingot (Part-Time Job)
	}
}

public class EdernSmithArishAshuvainBootsMAdvPtjScript : EdernSmithAdvPtjBaseScript
{
	protected override int QuestId { get { return 507269; } }
	protected override string LQuestDescription { get { return L("This job involves creating equipment to supply the Blacksmith's Shop. Today's task is creating [Arish Ashuvain Boots (M)], using the materials given for this part-time job. Deadline starts at noon. Be careful not to deliver them before the deadline since the final work doesn't begin until then."); } }
	protected override int ItemId { get { return 60809; } }
	protected override string LCreateObjectiveDescription { get { return L("Make 2 Arish Ashuvain Boots (M) (Part-Time Job)"); } }
	protected override string LCollectObjectiveDescription { get { return L("2 Arish Ashuvain Boots (M) (Part-Time Job)"); } }

	public override void OnReceive(Creature creature)
	{
		creature.GiveItem(ItemEntity.CreatePattern(60800, 30009, 10)); // Part-Time Job Blacksmithing Manual - Arish Ashuvain Boots (M)
		creature.GiveItem(60413, 2);  // Fine Silk (Part-Time Job)
		creature.GiveItem(60815, 20); // Iron Ingot (Part-Time Job)
		creature.GiveItem(60815, 20); // Iron Ingot (Part-Time Job)
		creature.GiveItem(60428, 10); // Common Leather Strap (Part-Time Job)
		creature.GiveItem(60428, 10); // Common Leather Strap (Part-Time Job)
		creature.GiveItem(60428, 10); // Common Leather Strap (Part-Time Job)
		creature.GiveItem(60428, 10); // Common Leather Strap (Part-Time Job)
	}
}

public class EdernSmithScaleArmorAdvPtjScript : EdernSmithAdvPtjBaseScript
{
	protected override int QuestId { get { return 507270; } }
	protected override string LQuestDescription { get { return L("This job involves creating equipment to supply the Blacksmith's Shop. Today's task is creating the [Scale Armor], using the materials given for this part-time job. Deadline starts at noon. Be careful not to deliver it before the deadline since the final work doesn't begin until then."); } }
	protected override int ItemId { get { return 60810; } }
	protected override int ItemCount { get { return 1; } }
	protected override string LCreateObjectiveDescription { get { return L("Make 1 Scale Armor (Part-Time Job)"); } }
	protected override string LCollectObjectiveDescription { get { return L("1 Scale Armor (Part-Time Job)"); } }

	public override void OnReceive(Creature creature)
	{
		creature.GiveItem(ItemEntity.CreatePattern(60800, 30010, 15)); // Part-Time Job Blacksmithing Manual - Scale Armor
		creature.GiveItem(60420, 1);  // Common Fabric (Part-Time Job)
		creature.GiveItem(60813, 1);  // Fomor Token (Part-Time Job)
		creature.GiveItem(60815, 20); // Iron Ingot (Part-Time Job)
		creature.GiveItem(60815, 20); // Iron Ingot (Part-Time Job)
		creature.GiveItem(60429, 10); // Fine Leather Strap (Part-Time Job)
		creature.GiveItem(60429, 10); // Fine Leather Strap (Part-Time Job)
		creature.GiveItem(60429, 10); // Fine Leather Strap (Part-Time Job)
	}
}

public class EdernSmithGiantHalfGuardLeatherArmorFAdvPtjScript : EdernSmithAdvPtjBaseScript
{
	protected override int QuestId { get { return 507271; } }
	protected override string LQuestDescription { get { return L("This job involves creating equipment to supply the Blacksmith's Shop. Today's task is creating the [Giant Half Guard Leather Armor (F)], using the materials given for this part-time job. Deadline starts at noon. Be careful not to deliver it before the deadline since the final work doesn't begin until then."); } }
	protected override int ItemId { get { return 60811; } }
	protected override int ItemCount { get { return 1; } }
	protected override string LCreateObjectiveDescription { get { return L("Make 1 Giant Half Guard Leather Armor (F) (Part-Time Job)"); } }
	protected override string LCollectObjectiveDescription { get { return L("1 Giant Half Guard Leather Armor (F) (Part-Time Job)"); } }

	public override void OnReceive(Creature creature)
	{
		creature.GiveItem(ItemEntity.CreatePattern(60800, 30011, 10)); // Part-Time Job Blacksmithing Manual - Giant Half Guard Leather Armor (F)
		creature.GiveItem(60426, 1);  // Finest Leather (Part-Time Job)
		creature.GiveItem(60814, 2);  // Tough String (Part-Time Job)
		creature.GiveItem(60815, 10); // Iron Ingot (Part-Time Job)
		creature.GiveItem(60424, 10); // Common Leather (Part-Time Job)
		creature.GiveItem(60404, 5);  // Braid (Part-Time Job)
		creature.GiveItem(60404, 5);  // Braid (Part-Time Job)
	}
}
