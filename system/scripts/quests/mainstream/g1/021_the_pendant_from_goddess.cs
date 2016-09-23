//--- Aura Script -----------------------------------------------------------
// G1 021: The Pendant from Goddess
//--- Description -----------------------------------------------------------
// Talking to Duncan and Tarlach and getting next cutscene from Morrighan.
// 
// Wiki:
// - Investigation of the goddess & obtain the Pendant from Goddess.
//---------------------------------------------------------------------------

public class ThePendantFromGoddessQuest : QuestScript
{
	private const int Pendant = 73026;
	private const int Torque = 73005;

	public override void Load()
	{
		SetId(210010);
		SetName(L("Duncan's Call"));
		SetDescription(L("Come visit me when you're free, I've found something that might be of interest to you. - Duncan -"));

		SetIcon(QuestIcon.AdventOfTheGoddess);
		SetCategory(QuestCategory.AdventOfTheGoddess);

		AddObjective("talk", L("Talk to Duncan."), 1, 15409, 38310, Talk("duncan"));

		AddReward(Exp(370));
		AddReward(WarpScroll(63009, "math_dungeon"));

		AddHook("_duncan", "before_keywords", DuncanBeforeKeywords);
		AddHook("_duncan", "after_intro", DuncanAfterIntro);
		AddHook("_tarlach", "before_keywords", TarlachBeforeKeywords);
	}

	public async Task<HookResult> DuncanBeforeKeywords(NpcScript npc, params object[] args)
	{
		var keyword = args[0] as string;

		if (keyword == "g1_goddess_morrighan2")
		{
			if (npc.HasKeyword("g1_30"))
			{
				npc.RemoveKeyword("g1_30");
				npc.GiveKeyword("g1_31");

				npc.Msg(Hide.Name, L("(You tell Duncan about the experience Tarlach had in Tir Na Nog.)"));
				npc.Msg(L("Is that what happened to Tarlach?<br/>That's unbelievable.<br/>But I suppose there's no choice but to believe him. Hmmm."));
				npc.Msg(L("There is one more thing I'd like to tell you.<br/>What I'm about to tell you is a secret I've kept all my life,<br/>so promise me you won't tell anyone else, okay?"));
				npc.Msg(L("You said Mores tried to save<br/>Tarlach and his friends at the last minute when they went to Tir Na Nog, right?"));
				npc.Msg(L("I'm sure that wasn't because of Tarlach."));
				npc.Msg(L("Mari. It was because of Mari.<br/>Mari was Mores's only daughter.<br/>Mores recognized that fact."), npc.Image("g1_ch27_mari"));
				npc.Msg(L("I knew his wife Shiela before she passed away.<br/>She was killed by humans instigated by nobles,<br/>but Mari was sent to me with the help of the wolves and deer."));
				npc.Msg(L("Mari's memory of her parents<br/>was personally erased by her mother."));
				npc.Msg(L("It would mean death for her to<br/>remember her parents."));
				npc.Msg(L("So I raised Mari myself."));
				npc.Msg(L("Yes, I think I've told you enought about this story."));
				npc.Msg(L("Hmm. Everything else fits into the story except<br/>the Goddess.<br/>You don't aactually agree with Tarlach<br/>about the Goddess, do you?"));
				npc.Msg(L("Ask the other people.<br/>Don't tell them what you think, but just listen<br/>to see if the Goddess is someone who'd betray humankind."));
			}

			npc.Msg(L("I'll remind you one more time.<br/>Please keep what I told you about Mari a secret."));

			return HookResult.Break;
		}
		else if (keyword == "g1_request_from_goddess")
		{
			var owlDelay = 36 * 60; // 1 Erinn day
			if (IsEnabled("ShorterWaitTimesChapter1"))
				owlDelay = 4 * 60;

			npc.SendOwl(this.Id, owlDelay);

			npc.RemoveKeyword("g1_32");
			npc.GiveKeyword("g1_33");
			npc.RemoveKeyword("g1_request_from_goddess");
			npc.GiveKeyword("g1_way_to_tirnanog1");

			npc.Msg(L("You dreamt of the Goddess again?<br/>The Goddess said that the day of Glas Ghaibhleann's resurrection is approaching?"));
			npc.Msg(L("It's just as I'd thought... Is Mores<br/>behind the resurrection of Glas Ghaibhleann?<br/>We don't even have the slightest clue what to do about it yet.<br/>This is a big problem!"));
			npc.Msg(L("I think I forgot to tell you this,<br/>Magic doesn't work on Adamantium,<br/>so it's difficult to fuse to a magical creature."));
			npc.Msg(L("That's why special ingredients are required.<br/>They say that the soul of a brave human is needed as an ingredient."));
			npc.Msg(L("It's possible that they may already have the soul<br/>of one of the Three Missing Warriors.<br/>Then this is truly a big problem.<br/>We have to stop them somehow."));
			npc.Msg(L("Now hurry up and find out how to get there!"));

			return HookResult.Break;
		}

		return HookResult.Continue;
	}

	public async Task<HookResult> TarlachBeforeKeywords(NpcScript npc, params object[] args)
	{
		var keyword = args[0] as string;

		if (keyword == "g1_goddess_morrighan2")
		{
			npc.RemoveKeyword("g1_30");
			npc.GiveKeyword("g1_31");

			npc.Msg(L("Now you know what I've been through.<br/>There was no sight of Mari and Ruiairi.<br/>The master barely managed to get me out.<br/>We made another attempt to find them, but it was no good..."));
			npc.Msg(L("Since that day, I can no longer use any advanced magic<br/>and I have to stay in bear form during the day.<br/>Isn't that a strange turn of events?"));
			npc.Msg(L("Though I still possess all my old arcane knowledge,<br/>my life as a wizard ended that day,<br/>vanished along with my dear friends."));
			npc.Msg(L("That's why I can never forgive the Goddess.<br/>I was so consumed by hatred that even<br/>Kristell's devotion couldn't soothe me."));
			npc.Msg(L("It occured to me to leave one day.<br/>I've been nothing but a burden on Kristell.<br/>She betrayed her own people to learn about love...<br/>I can only give her more pain."));
			npc.Msg(L("...<br/>So now you know why I live like this.<br/>I will remove the mask of hypocrisy from the Goddess.<br/>And I will make my master pay for this betrayal."));

			return HookResult.Break;
		}

		return HookResult.Continue;
	}

	[On("PlayerLoggedIn")]
	public void PlayerLoggedIn(Creature creature)
	{
		if (creature.Keywords.Has("g1_31"))
		{
			Cutscene.Play("G1_28_a_Morrighan", creature, cutscene =>
			{
				creature.Keywords.Remove("g1_31");
				creature.Keywords.Give("g1_32");
				creature.Keywords.Remove("g1_goddess_morrighan2");
				creature.Keywords.Give("g1_request_from_goddess");

				creature.GiveItem(Pendant);
			});
		}
	}

	public async Task<HookResult> DuncanAfterIntro(NpcScript npc, params object[] args)
	{
		if (npc.QuestActive(this.Id, "talk"))
		{
			npc.CompleteQuest(this.Id);

			npc.GiveKeyword("g1_memorial4");
			npc.GiveKeyword("g1_34_1");

			npc.GiveItem(Torque);
			npc.Notice(L("You have received Broken Torque from Duncan."));

			npc.Msg(L("Welcome, <username/>. I'm glad you came...<br/>First, let me give you this.<br/>I found it while I was cleaning the house."));
			npc.Msg(L("Yes... This is the reason I called you<br/>Do you remember...? How I told you I raised Mari..."));
			npc.Msg(L("When Mari first came to me...<br/>She was delivered by a pair of white and brown deer, wrapped tightly in a blanket..."), npc.Image("g1_ch31_baby"));
			npc.Msg(L("Inside the blanket, there was a letter from Shiela,<br/>asking me to watch over Mari, and the item I just gave you..."), npc.Image("g1_ch31_baby"));
			npc.Msg(L("This is what the letter said...<br/>When Mari becomes an adult...<br/>give her this memento so she can find her past...<br/>and...tell her go to Math Dungeon..."), npc.Image("g1_ch31_baby"));
			npc.Msg(L("Shiela said...when she dies<br/>she'll attach her memories to this item..."));
			npc.Msg(L("...<br/>But...now...since Mari...<br/>doesn't need it anymore...<br/>...I'm giving it to you."));
			npc.Msg(L("It seems like an item related to Mores...<br/>I hope it will be helpful in some way<br/>in your search to find Tir Na Nog..."));
			npc.Msg(L("I'm giving you a Red Wing of the Goddess as well...<br/>so hurry up and go to Math Dungeon.<br/>I wish you the best of luck."));

			return HookResult.Break;
		}

		return HookResult.Continue;
	}
}
