//--- Aura Script -----------------------------------------------------------
// Duncan's Praise
//--- Description -----------------------------------------------------------
// One of 4 quests is given automatically once Malcolm's Ring is completed.
// The 4 quests all involve talking to one of 4 NPCs, which will then give
// a friend title to the player. You can only get one of the 4 quests,
// and likewise only one of the titles.
//---------------------------------------------------------------------------

public abstract class DuncansPraiseQuestScript : QuestScript
{
	// 202036 - Duncan's Praise (Malcolm)
	// 202037 - Duncan's Praise (Nora)
	// 202038 - Duncan's Praise (Deian)
	// 202039 - Duncan's Praise (Trefor)
	public static int[] Quests = { 202036, 202037, 202038, 2020369 };

	protected abstract int QuestId { get; }
	protected abstract string FriendName { get; }
	protected abstract string FriendIdent { get; }
	protected abstract ushort FriendTitle { get; }
	protected abstract ushort FriendRegion { get; }
	protected abstract ushort FriendX { get; }
	protected abstract ushort FriendY { get; }

	public override void Load()
	{
		SetId(QuestId);
		SetName("Duncan's Praise");
		SetDescription("I heard you worked hard and made a difference helping the town residents. Can you visit me for a second? I will recommend a friend to you. - Duncan -");

		AddObjective("talk_duncan", "Talk with Chief Duncan", 1, 15409, 38310, Talk("duncan"));
		AddObjective("talk_friend", "A Talk with " + FriendName, FriendRegion, FriendX, FriendY, Talk(FriendIdent));

		AddReward(Exp(300));
		AddReward(Gold(1200));

		AddHook("_duncan", "after_intro", TalkDuncan);
		AddHook("_" + FriendIdent, "after_intro", TalkFriend);
	}

	public async Task<HookResult> TalkDuncan(NpcScript npc, params object[] args)
	{
		if (!npc.QuestActive(this.Id, "talk_duncan"))
			return HookResult.Continue;
		npc.FinishQuest(this.Id, "talk_duncan");

		// Unofficial
		npc.Msg("Hello <username/>.<br/>I heard you worked hard and made a difference helping the town residents.<br/>Why don't you pay " + FriendName + " a visit?<br/>I think you could become good friends.");

		return HookResult.Break;
	}

	public async Task<HookResult> TalkFriend(NpcScript npc, params object[] args)
	{
		if (!npc.QuestActive(this.Id, "talk_friend"))
			return HookResult.Continue;
		npc.FinishQuest(this.Id, "talk_friend");

		npc.Player.Titles.Enable(FriendTitle); // is a friend of ...
		await FriendDialog(npc);

		return HookResult.Break;
	}

	protected virtual async Task FriendDialog(NpcScript npc)
	{
		// Unofficial
		npc.Msg("Hello <username/>, did Duncan send you?<br/>I'd be glad to call you my friend.");
	}
}

public class DuncansPraiseMalcolmQuestScript : DuncansPraiseQuestScript
{
	protected override int QuestId { get { return 202036; } }
	protected override string FriendName { get { return "Malcolm"; } }
	protected override string FriendIdent { get { return "malcolm"; } }
	protected override ushort FriendTitle { get { return 10061; } } // is a friend of Malcolm
	protected override ushort FriendRegion { get { return 8; } }
	protected override ushort FriendX { get { return 1238; } }
	protected override ushort FriendY { get { return 1655; } }
}

public class DuncansPraiseNoraQuestScript : DuncansPraiseQuestScript
{
	protected override int QuestId { get { return 202037; } }
	protected override string FriendName { get { return "Nora"; } }
	protected override string FriendIdent { get { return "nora"; } }
	protected override ushort FriendTitle { get { return 10062; } } // is a friend of Nora
	protected override ushort FriendRegion { get { return 1; } }
	protected override ushort FriendX { get { return 15933; } }
	protected override ushort FriendY { get { return 33363; } }
}

public class DuncansPraiseDeianQuestScript : DuncansPraiseQuestScript
{
	protected override int QuestId { get { return 202038; } }
	protected override string FriendName { get { return "Deian"; } }
	protected override string FriendIdent { get { return "deian"; } }
	protected override ushort FriendTitle { get { return 10060; } } // is a friend of Deian
	protected override ushort FriendRegion { get { return 1; } }
	protected override ushort FriendX { get { return 27953; } }
	protected override ushort FriendY { get { return 42287; } }
}

public class DuncansPraiseTreforQuestScript : DuncansPraiseQuestScript
{
	protected override int QuestId { get { return 202039; } }
	protected override string FriendName { get { return "Trefor"; } }
	protected override string FriendIdent { get { return "trefor"; } }
	protected override ushort FriendTitle { get { return 10059; } } // is a friend of Trefor
	protected override ushort FriendRegion { get { return 1; } }
	protected override ushort FriendX { get { return 8692; } }
	protected override ushort FriendY { get { return 52637; } }
}

public class DuncansPraiseStarterScript : GeneralScript
{
	[On("PlayerCompletesQuest")]
	public void OnPlayerCompletesQuest(Creature creature, int questId)
	{
		// Cancel if finished quest isn't right
		if (questId != 202004) // Malcolm's Ring
			return;

		// Start one of the praise quests randomly if player doesn't have
		// one yet, completed or not.
		if (!creature.Quests.HasAny(DuncansPraiseQuestScript.Quests))
			creature.Quests.SendOwl(Rnd(DuncansPraiseQuestScript.Quests));
	}

	[On("PlayerLoggedIn")]
	public void OnPlayerLoggedIn(Creature creature)
	{
		// Cancel if quest hasn't been done yet
		// We only need this logged in event for the people who might've
		// already done Malcolm's Ring, where we won't get the Complete
		// event anymore. It could be removed if we wipe all progress.
		if (!creature.Quests.IsComplete(202004)) // Malcolm's Ring
			return;

		// Start one of the praise quests randomly if player doesn't have
		// one yet, completed or not.
		if (!creature.Quests.HasAny(DuncansPraiseQuestScript.Quests))
			creature.Quests.SendOwl(Rnd(DuncansPraiseQuestScript.Quests));
	}
}
