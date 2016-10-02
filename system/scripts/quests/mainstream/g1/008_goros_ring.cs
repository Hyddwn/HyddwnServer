//--- Aura Script -----------------------------------------------------------
// G1 008: Goro's Ring
//--- Description -----------------------------------------------------------
// (Not an actual quest.)
// 
// Talk to Comgan, Kristell, Endelyon, and Meven about the Fomor Medal,
// to be directed to Goro.
// 
// See "tircho_ciar_low_dungeon" for the ring drop.
// 
// Wiki:
// - Requirement: One must drop their own Basic pass as the party's leader
//                and have every other member leave the dungeon and party
//                before opening the boss door. Pets may be used to assist
//                in the Boss Room.
// - Instruction: Clear Ciar Basic Dungeon.
//---------------------------------------------------------------------------

public class GorosRingQuest : QuestScript
{
	private const int FomorMedal = 73021;
	private const int GorosRing = 73060;

	public override void Load()
	{
		SetId(210020);
		SetName(L("Goro's Ring"));
		SetDescription(L("Goro lost a ring in the depths of [Ciar Basic Dungeon] and asked you to retrieve it in exchange for his help."));

		SetIcon(QuestIcon.AdventOfTheGoddess);
		SetCategory(QuestCategory.AdventOfTheGoddess);

		AddObjective("get_ring", L("Bring Goro his ring."), 28, 1283, 3485, Talk("goro"));

		AddReward(Keyword("g1_dulbrau1"));

		AddHook("_goro", "after_intro", GoroAfterIntro);
		AddHook("_goro", "before_keywords", GoroBeforeKeywords);
		AddHook("_tarlach", "before_keywords", TarlachBeforeKeywords);
	}

	public async Task<HookResult> GoroAfterIntro(NpcScript npc, params object[] args)
	{
		if (!npc.QuestActive(this.Id) || !npc.HasItem(GorosRing))
			return HookResult.Continue;

		npc.CompleteQuest(this.Id);
		npc.RemoveItem(GorosRing);

		npc.Msg(Hide.Name, L("(You give Goro his ring.)"));
		npc.Msg(L("Thank you, I'll now read it to you..."));
		npc.Msg(L("'Dul Brau Dairam Shanon' means 'Goddess, lend me the moonlight.'"));

		return HookResult.Break;
	}

	public async Task<HookResult> GoroBeforeKeywords(NpcScript npc, params object[] args)
	{
		var keyword = args[0] as string;

		if (keyword == "g1_voucher_of_priest")
		{
			if (npc.QuestActive(this.Id))
			{
				npc.Msg(L("If you get Goro's Ring back, I'll read it to you."));
			}
			else if (npc.HasItem(FomorMedal))
			{
				npc.RemoveKeyword("g1_voucher_of_priest");
				npc.RemoveKeyword("g1_12");
				npc.GiveKeyword("g1_13");
				npc.StartQuest(this.Id);

				npc.Msg(L("Priest's Token...? This...?<br/>This...is an amulet that belongs to a high ranking Fomor...let's see...<br/>Just what I'd thought...'Dul Brau Dairam Shanon.' It surely belongs to a Fomor. Heh."), npc.Image("g1_ch11_12_fomormedal02"));
				npc.Msg(L("It's been a while since I came across such Fomor writings. Heheh."));
				npc.Msg(L("I learned the human language as a child<br/>so I'm a little rusty...<br/>but since Goro is smart, this is no problem."));
				npc.Msg(L("As a sign of respect for your courage and<br/>since I'm such a nice goblin,<br/>I'll read it to you..."));
				npc.Msg(L("If you help Goro that is. Heheh."));
				npc.Msg(L("I've lost a ring in Ciar Dungeon, if you get Goro's Ring back, I'll read it to you."));
			}
			else
			{
				npc.Msg(L("Priest's Token...?"));
			}

			return HookResult.Break;
		}
		else if (keyword == "g1_dulbrau2")
		{
			npc.Msg(L("...Oh... that's what it means?<br/>Wow...I'm amazed."));
			npc.Msg(L("Ha...! You truly live up to the hype as someone who's obtained the Fomor Medal...<br/>To be honest, because I was raised in the Human world since I was young,<br/>my Fomor language skills aren't that good...hehe...<br/>I hope you understand...hehe..."));
			npc.Msg(L("...In order to make up for this embarrassment,<br/>I'll tell you who can decipher the Fomor writing."));
			npc.Msg(L("...<p/>...<p/>I'm sorry, but I can't seem to remember the name."));
			npc.Msg(L("...Wait, please don't get angry now!"));
			npc.Msg(L("Um... right!<br/>Goro had heard this story once...The Black Roses of the dungeons,<br/>known as Succubus to Humans,<br/>said there was someone who had betrayed them...and is now living in Dunbarton..."));
			npc.Msg(L("They said they were waiting to get revenge on that person if they ever ran into her...<br/>Well, I mean it's just a rumor...but still... Hehe..."));
			npc.Msg(L("The Succubus is a Fomor also...<br/>so they might know more about the Fomor language...or that world...no?<br/>But it won't be easy to find them...ha...haha..."));

			return HookResult.Break;
		}

		return HookResult.Continue;
	}

	public async Task<HookResult> TarlachBeforeKeywords(NpcScript npc, params object[] args)
	{
		var keyword = args[0] as string;

		if (keyword == "g1_dulbrau1")
		{
			if (npc.HasItem(FomorMedal))
			{
				if (npc.HasKeyword("g1_13"))
				{
					npc.RemoveKeyword("g1_13");
					npc.GiveKeyword("g1_14");

					npc.Msg(L("'Goddess, lend me the moonlight...'<br/>That's what Goro said it means?<br/>The line 'Dul Brau Dairam Shanon?'"));
					npc.Msg(L("I can't say for sure that's it's a wrong interpretation but...<br/>it's slightly different from what I remember."));
				}

				npc.Msg(L("Could I take a look at this medal?"), npc.Button(L("Yes"), "@yes"), npc.Button(L("No"), "@no"));
				if (await npc.Select() != "@yes")
				{
					npc.Msg(L("I won't be able to teach you the meaning of this line without seeing the medal."));
					return HookResult.Break;
				}

				npc.RemoveItem(FomorMedal);
				npc.RemoveKeyword("g1_dulbrau1");
				npc.GiveKeyword("g1_dulbrau2");

				npc.Msg(L("Thank you, let's take a look.<br/>Hm... I see."));
				npc.Msg(L("The reason I am human during the night,<br/>is because of Eweca's moonlight and its magic power..."));
				npc.Msg(L("'Dul Brau Dairam Shanon' means,<br/>'Oh, Goddess. Please bestow me with your powers.'"));
				npc.Msg(L("This story...<br/>is straight from a Fomor who was practicing magic,<br/>so I am sure of it.<br/>..."));
				npc.Msg(L("Anyway, now you should know what this means.<br/>Fomors are coming to Erinn with the help of the Goddess.<br/>Yes, with the very power of the Goddess."));
				npc.Msg(L("Morrighan is assisting Fomors with their infiltration.<br/>Which is an unforgivable act.<br/>If you don't believe me, why don't you go ask Goro again?"));
			}
			else
			{
				npc.Msg(L("'Goddess, lend me the moonlight...'<br/>That's what Goro said it means?<br/>The line 'Dul Brau Dairam Shanon?'"));
				npc.Msg(L("I can't say for sure that's it's a wrong interpretation but...<br/>it's slightly different from what I remember."));
			}

			return HookResult.Break;
		}
		else if (keyword == "g1_dulbrau2")
		{
			npc.Msg(L("If you don't believe me, why don't you go ask Goro again?"));

			return HookResult.Break;
		}

		return HookResult.Continue;
	}
}
