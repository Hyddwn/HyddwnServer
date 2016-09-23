//--- Aura Script -----------------------------------------------------------
// G1 023: Kristell's Request
//--- Description -----------------------------------------------------------
// Killing some monsters for Kristell and obtaining the Black Fomor Pass.
// 
// Wiki:
// - Requirement: npc.Titles stated below while level 25 or higher
// - Instruction: Slay monsters threatening Kristell.
//---------------------------------------------------------------------------

public class KristellsRequestQuest : QuestScript
{
	private const int MinLevel = 25;
	private const int BlackFomorPass = 73012;

	public override void Load()
	{
		SetId(210022);
		SetName(L("Kristell's Request"));
		SetDescription(L("I'm nervous. It feels like evil spirits are watching me. I saw the Field Boss Infor Scroll and I hink I am their target. Can you help so that they won't come near me? - Kristell -"));

		SetIcon(QuestIcon.AdventOfTheGoddess);
		SetCategory(QuestCategory.AdventOfTheGoddess);

		AddObjective("kill1", L("Hunt down the Black Dire Wolves"), 0, 0, 0, Kill(1, "/blackdirewolf/"));
		AddObjective("kill2", L("Hunt down the Werewolves"), 0, 0, 0, Kill(1, "/werewolf/"));
		AddObjective("kill3", L("Hunt down the Ogres"), 0, 0, 0, Kill(1, "/ogre/"));
		AddObjective("talk", L("Talk to Kristell"), 14, 34657, 42808, Talk("kristell"));

		AddReward(Item(BlackFomorPass));
		AddReward(WarpScroll(63009, "barri_dungeon"));

		AddHook("_duncan", "before_keywords", DuncanBeforeKeywords);
		AddHook("_kristell", "before_keywords", KristellBeforeKeywords);
		AddHook("_kristell", "after_intro", KristellAfterIntro);
	}

	public async Task<HookResult> DuncanBeforeKeywords(NpcScript npc, params object[] args)
	{
		var keyword = args[0] as string;

		if (keyword == "g1_way_to_tirnanog1")
		{
			npc.Msg(L("Hurry up and find out how to get to Tir Na Nog!"));

			return HookResult.Break;
		}

		return HookResult.Continue;
	}

	// Dialog pieced together from vague translations
	// and snippets of official dialog.
	public async Task<HookResult> KristellBeforeKeywords(NpcScript npc, params object[] args)
	{
		var keyword = args[0] as string;

		if (keyword == "g1_cichol")
		{
			npc.Msg(L("He's amongst one of the evil Gods that lead the Fomors.<br/>Yes, Fomors do worship Gods."));
			npc.Msg(L("I don't particularly feel up to talking about him.<br/>But, I'll tell you now, you should hurry if you think Cichol<br/>is somehow involved with GOddess Morrighan."));
			npc.Msg(L("He is cunning and meticulous, so he will<br/>be very difficult to stop by ourselves."));
		}
		else if (keyword == "g1_way_to_tirnanog1")
		{
			if (npc.Player.Level < 25)
			{
				npc.Msg(L("...<p/>I have heard people call your name...<br/>And I know what kind of person you are."));
				npc.Msg(L("...But, <username/>... Tir Na Nog is a bad place to be.<br/>In that place even your connection<br/>to the Soul Stream won't help you..."));
				npc.Msg(L("You don't have the experience to survive there."));
				npc.Msg(L("You should at least reach level 25...<br/>Otherwise, going there would be suicide."));

				return HookResult.Break;
			}

			var hasSlayerTitle = (npc.Title == 79 || npc.Title == 80 || npc.Title == 81 || npc.Title == 82 || npc.Title == 83 || npc.Title == 85 || npc.Title == 53);
			if (!hasSlayerTitle)
			{
				npc.Msg(L("...<br/>If...<br/>If Tir Na Nog is..."));
				npc.Msg(L("If it really is the land of the Fomors, like Tarlach said..."));
				npc.Msg(L("I've already betrayed the Fomors. I supposed it doesn't hurt to tell you.<br/>The land of the Fomors is a dangerous place.<br/>There are no friendly faces or safe places...<br/>You have no one but yourself and your party members to rely on."));
				npc.Msg(L("Do you... think you have the strength to bear the pain...?"));
				npc.Msg(L("I can tell you how to reach Tir Na Nog."));
				npc.Msg(L("But... please show me that you're capable<br/>of fighting Erinn's powerful monsters."));
				npc.Msg(L("I can't tell you how to reach Tir Na Nog before that."));

				return HookResult.Break;
			}

			if (npc.HasKeyword("g1_33"))
			{
				if (!npc.HasQuest(this.Id))
					npc.StartQuest(this.Id);

				npc.Msg(L("That title above your head<br/>tells me that you're confident<br/>with your strength."));
				npc.Msg(L("If you're so strong,<br/>could I ask you for one favor?"));
				npc.Msg(L("I've betrayed the Fomors to become a Human.<br/>Fomors are always waiting for the chance to hurt me."));
				npc.Msg(L("Ever since I translated the book for Tarlach,<br/>I sensed someone stalking me."));
				npc.Msg(L("I'm really insecure because I feel the Fomors looking for me.<br/>I saw the Fomor Command Scroll and think I'm supposed to be the target.<br/>Can you help me so they can't come anywhere near here?"));
				npc.Msg(L("If you do that,<br/>I'll feel a bit more at ease."));
			}
			else
			{
				if (!npc.HasItem(BlackFomorPass))
				{
					npc.GiveItem(BlackFomorPass);
					npc.Msg(L("This is a Fomor Pass used only by high ranking Fomors.<br/>I pray that you won't lose it."));
				}
				else
				{
					npc.Msg(L("Yes, offer the pass I gave you in Barri Dungeon.<br/>...<br/>Please be careful."));
				}
			}

			return HookResult.Break;
		}

		return HookResult.Continue;
	}

	public async Task<HookResult> KristellAfterIntro(NpcScript npc, params object[] args)
	{
		if (npc.QuestActive(this.Id, "talk"))
		{
			npc.CompleteQuest(this.Id);

			npc.RemoveKeyword("g1_33");
			npc.GiveKeyword("g1_34");

			npc.Msg(L("Thank you for your help, I feel much safer now.<br/>I'll tell you how to get to the place Tarlach mentioned.<br/>This Fomor Pass is used by high-ranking Fomors to travel there.<br/>This wing will take you to the dungeon. I pray that you don't lose it."));

			// Newer versions of G1 place the third Morrighan cutscene after
			// she gave the pass to the player, possibly because of g1_34
			// becoming active.

			return HookResult.Break;
		}

		return HookResult.Continue;
	}

	[On("PlayerLoggedIn")]
	public void PlayerLoggedIn(Creature creature)
	{
		if (creature.Keywords.Has("g1_34") && creature.Keywords.Has("g1_34_2") && creature.Keywords.Has("g1_cichol"))
		{
			Cutscene.Play("G1_33_a_Morrighan", creature, cutscene =>
			{
				creature.Keywords.Remove("g1_34");
				creature.Keywords.Remove("g1_34_2");
				creature.Keywords.Give("g1_36"); // Dunno what happened to 35.
				creature.Keywords.Remove("g1_cichol");
				creature.Keywords.Give("g1_tirnanog_seal_breaker");
			});
		}
	}
}
