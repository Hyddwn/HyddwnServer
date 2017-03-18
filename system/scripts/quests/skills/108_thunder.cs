//--- Aura Script -----------------------------------------------------------
// Thunder skill quest
//--- Description -----------------------------------------------------------
// Get the Thunder collection book from Stewart.
//---------------------------------------------------------------------------

public class ThunderQuestScript : QuestScript
{
	private const int CollectionBook = 1504;
	private const int StewartsBook = 70065;

	public override void Load()
	{
		SetId(108);
		SetScrollId(70500);
		SetName(L("Earn Thunder Skillbook"));
		SetDescription(L("Manus borrowed a book of mine, and hasn't returned it to me yet. If you can get the book from him, then I'll give you a book that should help you learn the Thunder spell. - Stewart -"));
		SetCancelable(true);

		SetIcon(QuestIcon.Magic);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Skill);

		AddObjective("talk_manus", L("Talk to Manus"), 0, 0, 0, Talk("manus"));
		AddObjective("talk_shelf", L("Find the book at the Specialties section"), 0, 0, 0, Talk("book"));
		AddObjective("talk_stewart", L("Deliver the book to Stewart"), 0, 0, 0, Talk("stewart"));

		AddReward(Item(CollectionBook));

		AddHook("_stewart", "before_keywords", StewartBeforeKeywords);
		AddHook("_manus", "after_intro", ManusAfterIntro);
		AddHook("_bookcase05", "after_intro", ShelfAfterIntro);
		AddHook("_stewart", "after_intro", StewartAfterIntro);
	}

	public async Task<HookResult> StewartBeforeKeywords(NpcScript npc, params object[] args)
	{
		var keyword = args[0] as string;
		if (keyword != "about_skill")
			return HookResult.Continue;

		// Continue if player has the skill already, so we reach other
		// hooks, for other skills.
		if (npc.Player.HasSkill(SkillId.Thunder))
			return HookResult.Continue;

		// Check prerequisites
		if (!IsEnabled("Thunder") || !npc.Player.HasSkill(SkillId.Lightningbolt) || !npc.Player.HasEquipped("/lightning_wand/"))
			return HookResult.Continue;

		// Start quest
		if (!npc.Player.HasQuest(this.Id))
		{
			npc.Msg(L("Ah, <username/>, you have the Lightning Wand.<br/>You seem to know of the Lightning Bolt magic...<br/>Do you also know the Thunder magic by any chance?"));
			npc.Msg(L("...Thunder is a very powerful, electric magic.<br/>It's a spell that is used by first freezing the enemy with a lightning bolt,<br/>followed by a huge thunder on the enemy's head.<br/>It's so much more powerful than the Lightning Bolt, that it's incomparable."));
			npc.Msg(L("If you can help me out here once,<br/>I'll help you acquire the Thunder magic.<br/>Are you interested?"), npc.Button(L("Yes I am!"), "@yes"), npc.Button(L("Maybe another time..."), "@no"));

			if (await npc.Select() == "@yes")
			{
				npc.Player.StartQuest(this.Id);
				npc.Msg(L("I'd appreciate it if you can get this person, who borrowed a book from me, to return it.<br/>I'll give you the quest scroll through the owl,<br/>and your job will be to get the book back from the person written on the scroll."));
			}
			else
			{
				npc.Msg(L("...if you ever change your mind,<br/>then can you please come see me with the Lightning Wand?"));
			}

			return HookResult.Break;
		}

		return HookResult.Continue;
	}

	public async Task<HookResult> ManusAfterIntro(NpcScript npc, params object[] args)
	{
		if (npc.Player.QuestActive(this.Id, "talk_manus"))
		{
			npc.Player.FinishQuestObjective(this.Id, "talk_manus");

			npc.Msg(L("Hey, <username/>, what's up?<br/>Are you here because you miss me? Hahahaha!"));
			npc.Msg(Hide.Both, L("(Told Manus that Stewart wants the book returned)"));
			npc.Msg(L("What? A book? The one I borrowed from Stewart?<br/>Do I look like a scumbag that just steals books?<br/>I returned that book a loooong time ago!!!!"));
			npc.Msg(L("..."));
			npc.Msg(L("...Wait a minute, now I remember,<br/>I didn't return the book to Stewart. I just put the book back on the library shelf myself..."));
			npc.Msg(L("Hahaha, it's the kind of mistake I'd make ever so often.<br/>I'll forgive you and Stewart for this gross oversight, so<br/>just find that book at the library."));
			npc.Msg(L("...Wait, which shelf did I put the book back on...?<br/>I think I placed it in the Specialty aisle. What can I do?<br/><username/>, I think you'll be able to find it."));

			return HookResult.Break;
		}

		return HookResult.Continue;
	}

	public async Task<HookResult> ShelfAfterIntro(NpcScript npc, params object[] args)
	{
		if (npc.Player.QuestActive(this.Id, "talk_shelf"))
		{
			npc.Msg(Hide.Name, L("(Manus said he placed Stewart's book here, so I'd better look for it.)"));
			npc.Msg(Hide.Name, L("(......)"), npc.Button(L("Continue"), "@continue"));
			await npc.Select();

			npc.Player.FinishQuestObjective(this.Id, "talk_shelf");
			npc.Player.GiveItem(StewartsBook);
			npc.Player.Notice(L("Found Stewart's book inside the bookshelf."));

			npc.Msg(Hide.Name, L("(Found Stewart's book.)"));

			return HookResult.Break;
		}

		return HookResult.Continue;
	}

	public async Task<HookResult> StewartAfterIntro(NpcScript npc, params object[] args)
	{
		if (npc.Player.QuestActive(this.Id, "talk_stewart"))
		{
			npc.Player.FinishQuestObjective(this.Id, "talk_stewart");
			npc.Player.RemoveItem(StewartsBook);
			npc.Player.Notice(L("You have given Stewart's Book to Stewart."));

			npc.Msg(L("Thank you, <username/>. I am glad you were able to find the book.<br/>As promised, I'll give you a book that'll help you<br/>obtain Thunder magic.<br/>You'll be able to get it after completing the quest that I will give you."));
			npc.Msg(L("The book I am giving you is in a series,<br/>so in order for you to obtain Thunder magic,<br/>you'll have to collect the books from those who have checked out the books, then read them."));
			npc.Msg(L("Truthfully, this series of books features very potent spells in great detail,<br/>that many people have been looking for.<br/>...Even the Fomors..."));

			return HookResult.Break;
		}

		return HookResult.Continue;
	}
}
