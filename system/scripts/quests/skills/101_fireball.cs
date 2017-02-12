//--- Aura Script -----------------------------------------------------------
// Fireball skill quest
//--- Description -----------------------------------------------------------
// Learn Fireball from Stewart by collecting the 10 Fireball pages.
//---------------------------------------------------------------------------

public class FireballQuestScript : QuestScript
{
	private const int BookOnFireball = 63502;
	private const int BookOnFireballPage10 = 40030;
	private const int ElementalApprentice = 28;

	public override void Load()
	{
		SetId(101);
		SetScrollId(70500);
		SetName(L("Learn Fireball"));
		SetDescription(L("In order to learn the Fireball, you'll need to complete the  'Book of Fireball'. Collect pages 1-10 of the 'Book of Fireball' and enchant each page in order. -Stewart-"));
		SetCancelable(true);

		SetIcon(QuestIcon.Magic);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Skill);

		AddObjective("talk", L("Collect all 10 pages of the Book of Fireball and give them to Stewart."), 0, 0, 0, Talk("stewart"));

		AddReward(Skill(SkillId.Fireball, SkillRank.Novice));

		AddHook("_stewart", "before_keywords", BeforeKeywords);
	}

	public async Task<HookResult> BeforeKeywords(NpcScript npc, params object[] args)
	{
		var keyword = args[0] as string;
		if (keyword != "about_skill")
			return HookResult.Continue;

		// Continue if player has the skill already, so we reach other
		// hooks, for other skills.
		if (npc.Player.HasSkill(SkillId.Fireball))
			return HookResult.Continue;

		// Check prerequisites
		if (!IsEnabled("Fireball") || !npc.Player.IsUsingTitle(ElementalApprentice) || !npc.Player.HasEquipped("/fire_wand/"))
		{
			npc.Msg(L("Haha, <username/>, you seem to be a very curious person.<br/>This is not the right time, though...<br/>...I'll let you know when the time is right."));

			return HookResult.Break;
		}

		// Start quest
		if (!npc.Player.QuestActive(this.Id))
		{
			if (!npc.Player.HasItem(this.ScrollId))
			{
				npc.Msg(L("Hahaha... for an Elemental Master like you, <username/>, to<br/>ask someone like me for a skill..."));
				npc.Msg(L("...That must mean you're quite interested in<br/>the Fireball? Hahaha..."));
				npc.Msg(L("Ahhh, don't be surprised by my comments.<br/>A lot of people like you have been asking the same question lately.<br/>I mean, it's better for me to teach a spell like this to someone who's well-prepared for something like this, that is, someone like you."));
				npc.Msg(L("Besides, Fireball is a dangerous spell...<br/>Have you ever heard of it before?<br/>If the Firebolt is a bullet, then<br/>the Fireball is a cannonball."));
				npc.Msg(L("There's a big difference in the damage it can cause,<br/>and it's difficult to control that much ball of Mana energy, so...<br/>anyone wishing to learn the Fireball skill<br/>must first pass a test."));
				npc.Msg(L("...If  you, <username/>, are also interested in it, then<br/>you'll have to pass this test, too.<br/>You can't drop out in the middle, so you'll have to be really ready and committed to do this.<br/>Are you interested?"), npc.Button(L("Yes, I am!"), "@yes"), npc.Button(L("Maybe another time..."), "@no"));

				if (await npc.Select() == "@yes")
				{
					npc.Player.GiveItem(Aura.Channel.World.Entities.Item.CreateQuestScroll(this.Id));
					npc.Player.GiveItem(BookOnFireball);

					npc.Msg(L("I knew you'd do it, <username/>.<br/>Take this first..."));
					npc.Msg(L("...It's not a difficult task.<br/>All you have to do is make a book.<br/>The catch is, this isn't one of those ordinary books that Aeira stores in her bookstore."));
					npc.Msg(L("Follow the quest scroll and<br/>gather up each page of the Book of Fireball<br/>that's laden with magic power, and make a book out of it."));
				}
				else
				{
					npc.Msg(L("Yes, I understand... it can seem quite daunting.<br/>If you ever change your mind,<br/>just let me know and I'll give you the quest."));
				}

				return HookResult.Break;
			}
		}
		// Finish quest
		else
		{
			// Check if book is complete and finish quest if it is
			var book = npc.Player.Inventory.GetItem(a => a.Info.Id == BookOnFireball && a.OptionInfo.Suffix == BookOnFireballPage10);
			if (book != null)
			{
				npc.Player.FinishQuestObjective(this.Id, "talk");
				npc.Player.Inventory.Remove(book);

				npc.Msg(L("Wow, you found them all! Congratulations!<br/>Now, I suggest you step out and press 'Complete' to use the Fireball.<br/>Go ahead and try using it!"));
			}
			else
			{
				npc.Msg(L("Haha, <username/>, you seem to be a very curious person.<br/>This is not the right time, though...<br/>...I'll let you know when the time is right."));
			}

			return HookResult.Break;
		}

		return HookResult.Continue;
	}
}
