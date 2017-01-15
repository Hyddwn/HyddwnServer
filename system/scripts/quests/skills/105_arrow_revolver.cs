//--- Aura Script -----------------------------------------------------------
// Arrow Revolver skill quest
//--- Description -----------------------------------------------------------
// Learn Arrow Revolver from Aranwen by collecting the 10 Arrow Revolver
// pages.
//---------------------------------------------------------------------------

public class ArrowRevolverQuestScript : QuestScript
{
	private const int BookOnArrowRevolver = 63505;
	private const int BookOnArrowRevolverPage10 = 40060;

	public override void Load()
	{
		SetId(105);
		SetScrollId(70500);
		SetName("Learn Arrow Revolver");
		SetDescription("In order to learn the Arrow Revolver skill, you'll need to complete the [Book of Arrow Revolver]. Collect pages 1-10 and enchant them [in order]. - Aranwen -");
		SetCancelable(true);

		SetIcon(QuestIcon.Archery);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Skill);

		AddObjective("talk", "Bring the pages of 'Book of Arrow Revolver' to Aranwen.", 0, 0, 0, Talk("aranwen"));

		AddReward(Skill(SkillId.ArrowRevolver2, SkillRank.Novice, 1));

		AddHook("_aranwen", "before_keywords", BeforeKeywords);
	}

	public async Task<HookResult> BeforeKeywords(NpcScript npc, params object[] args)
	{
		var keyword = args[0] as string;
		if (keyword != "about_skill")
			return HookResult.Continue;

		// Arrow revolver can only be learned by humans
		if (!npc.Player.IsHuman)
		{
			npc.Msg(L("(Missing dialog: Aranwen about non-humans and bows? (Arrow Revolver)"));

			return HookResult.Break;
		}

		// Aranwen can't do anything for you before AR is enabled
		if (!IsEnabled("ArrowRevolver"))
		{
			npc.Msg(L("I know a little bit about bows,<br/>but it's not quite the right time."));
			npc.Msg(L("Please come back some other time."));

			return HookResult.Break;
		}

		// Check Fire Arrow title
		if (!npc.Player.IsUsingTitle(88))
		{
			npc.Msg(L("...I am sorry, but someone that has yet to master the skill<br/>should not be bluntly asking questions about skills like this."));
			npc.Msg(L("...if you are interested in high-leveled bowman skills, then<br/>you should at least master the Fire Arrow skill first."));

			return HookResult.Break;
		}

		// Check skill
		if (npc.Player.HasSkill(SkillId.ArrowRevolver2))
		{
			// Unofficial
			npc.Msg(L("...You've learned Arrow Revolver. I hope you keep learning<br/>until you master it."));

			return HookResult.Break;
		}

		// Start quest
		if (!npc.Player.QuestActive(this.Id))
		{
			if (!npc.Player.HasItem(this.ScrollId))
			{
				npc.Msg(L("Hmmm... <username/>, you knew how to shoot a Fire Arrow?<br/>It seems like you are interested in bows, and...<br/>it's nice to meet someone who's talent is only surpassed by their work ethic."));
				npc.Msg(L("I think it'll be very beneficial for you to learn this skill called the Arrow Revolver..."));
				npc.Msg(L("Arrow Revolver is a skill that lets you fire arrows consecutively.<br/>You'll fire multiple arrows in quick succession, not giving your enemy enough time to react..<br/>This skill will allow you to shoot up to 5 arrows at once."));
				npc.Msg(L("Arrow Revolver is very effective against powerful enemies by applying multiple<br/>damages and wounds in the blink of an eye. It's also effective against groups of enemies."));
				npc.Msg(L("...Are you interested in learning this skill?<br/>Yes, the Arrow Revolver...<br/>Please promise me that you will not quit on me midway through. Okay?"), npc.Button(L("Okay!"), "@yes"), npc.Button(L("No"), "@no"));

				if (await npc.Select() == "@yes")
				{
					npc.Player.GiveItem(Aura.Channel.World.Entities.Item.CreateQuestScroll(this.Id));
					npc.Player.GiveItem(BookOnArrowRevolver);

					npc.Msg(L("Great, <username/>.<br/>I'll give you this book and the quest.<br/>Technically speaking, Arrow Revolver<br/>relies somewhat on the power of Mana."));
					npc.Msg(L("Gather together all of the pages in this book.<br/>Our ancestors seperated them in hopes of preventing<br/>the reoccurrence of the same mistakes that had been made by inexperienced bowmen."));
					npc.Msg(L("Read the scroll carefully, and once you complete the book,<br/>you'll be able to use Arrow Revolver.<br/>...Good luck."));
				}
				else
				{
					npc.Msg(L("...I see.<br/>Please talk to me when you change your mind."));
				}

				return HookResult.Break;
			}
		}
		// Finish quest
		else
		{
			// Check if book is complete and finish quest if it is
			var book = npc.Player.Inventory.GetItem(a => a.Info.Id == BookOnArrowRevolver && a.OptionInfo.Suffix == BookOnArrowRevolverPage10);
			if (book != null)
			{
				npc.Player.FinishQuestObjective(this.Id, "talk");
				npc.Player.Inventory.Remove(book);

				npc.Msg(L("Yes? Please don't block my view."));
			}
			else
			{
				npc.Msg(L("...You're learning Arrow Revolver. I hope you keep trying<br/>until you master it."));
			}

			return HookResult.Break;
		}

		return HookResult.Continue;
	}
}
