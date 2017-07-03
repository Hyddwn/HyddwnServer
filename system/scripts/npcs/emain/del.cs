//--- Aura Script -----------------------------------------------------------
// Del
//--- Description -----------------------------------------------------------
// Florist, older sister of Delen.
//---------------------------------------------------------------------------

public class DelScript : NpcScript
{
	public override void Load()
	{
		SetRace(10001);
		SetName("_del");
		SetBody(height: 0.8f, upper: 1.1f);
		SetFace(skinColor: 16, eyeType: 3, eyeColor: 113);
		SetStand("human/female/anim/female_natural_stand_npc_01");
		SetLocation(52, 40406, 38978, 38);
		SetGiftWeights(beauty: 2, individuality: 0, luxury: 0, toughness: 1, utility: 2, rarity: 1, meaning: 1, adult: 2, maniac: -1, anime: -1, sexy: 2);

		EquipItem(Pocket.Face, 3900, 0x00F59C2F, 0x009D0082, 0x00AF469B);
		EquipItem(Pocket.Hair, 3031, 0x00EC7766, 0x00EC7766, 0x00EC7766);
		EquipItem(Pocket.Armor, 15082, 0x00FA8D8D, 0x00F7B4AC, 0x00202020);
		EquipItem(Pocket.Shoe, 17040, 0x005E2922, 0x0041002C, 0x00683D0D);

		AddPhrase("Hey! I got flowers.");
		AddPhrase("Flowers make a perfect gift for a romantic date.");
		AddPhrase("Smell these fresh flowers.");
		AddPhrase("Hi. Take a look at these gorgeous flowers!");
		AddPhrase("How are you doing?");
		AddPhrase("Do you need flowers?");
		AddPhrase("Fresh flowers!");
		AddPhrase("Look at these beautiful flowers. Choose one!");
		AddPhrase("What's wrong with today's customers...?");
		AddPhrase("What kinds of flowers are you looking for?");
	}

	protected override async Task Talk()
	{
		SetBgm("NPC_Del.mp3");

		await Intro(L("She holds a flower basket in one hand and the smile on her face never seems to fade.<br/>She is wearing a dress that is embroidered with flowers and lace patterns.<br/>Her long shiny hair flows over her shoulder<br/>and she approaches me as soon as she feels my stare on her, with which she starts a conversation."));

		Msg("Take a look at these flowers.<br/>Aren't they gorgeous?", Button("Start a Conversation", "@talk"), Button("Shop", "@shop"), Button("Upgrade", "@upgrade"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());

				if (Player.IsUsingTitle(11001))
				{
					Msg("I see... I didn't know you were such an amazing person, <username/>...");
					Msg("A person of your stature...<br/>would surely be able to afford to buy a flower to give to the Goddess...no?");
				}
				else if (Player.IsUsingTitle(11002))
				{
					Msg("Guardian of Erinn... <username/>...?<br/>Oh... You're that person<br/>who asked me all those questsions regarding Macha before, right...?");
					Msg("...I didn't expect to see you again like this...<br/>Wait... Did you take care of the danger...<br/>that involved Goddess Macha...?");
					Msg("...When did you<br/>do all that...? <username/>, you...");
					Msg("...You are... something special.");
				}

				await Conversation();
				break;

			case "@shop":
				Msg("How about a beautiful flower?");
				OpenShop("DelShop");
				return;

			case "@upgrade":
				Msg("Are you here because you need a better weeding hoe?<upgrade />");

				while (true)
				{
					var reply = await Select();

					if (!reply.StartsWith("@upgrade:"))
						break;

					var result = Upgrade(reply);
					if (result.Success)
						Msg("Yes, the modification is finished.<br/>I'll be happy if you like it.<br/>Is there anything else you need?");
					else
						Msg("(Error)");
				}
				Msg("Come see me again<br/>for anything, especially if you need to upgrade anything...<upgrade hide='true'/>");
				break;
		}

		End("Thank you, <npcname/>. I'll see you later!");
	}

	private void Greet()
	{
		if (DoingPtjForNpc())
		{
			Msg(FavorExpression(), L("(Missing Dialogue, talking to npc during PTJ.)"));
		}
		else if (Memory <= 0)
		{
			Msg(FavorExpression(), L("Can I help you with something?"));
		}
		else if (Memory == 1)
		{
			Msg(FavorExpression(), L("<username/>... <username/>, right?"));
		}
		else if (Memory == 2)
		{
			Msg(FavorExpression(), L("It's you again, <username/>. What brings you here today?"));
		}
		else if (Memory <= 6)
		{
			Msg(FavorExpression(), L("You must really love flowers, <username/>.<br/>You can buy as many as you'd like!"));
		}
		else
		{
			Msg(FavorExpression(), L("<username/>. Everyone in town has been talking about how close we are. Did you know that?"));
		}

		UpdateRelationAfterGreet();
	}

	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				Msg(FavorExpression(), "Ah, people keep confusing me and Delen...<br/>and Delen seems to enjoy it...<br/>Last time, she was flirting with Aodhan pretending to be me.<br/>I was so embarrassed...");
				ModifyRelation(Random(2), 0, Random(3));
				break;

			case "rumor":
				Msg(FavorExpression(), "I heard that Lucas sells special items...<br/>to those who are close to him...<br/>I wonder what he is selling...?");
				Msg("...I want to see too...");
				ModifyRelation(Random(2), 0, Random(3));
				break;

			case "shop_misc":
				Msg("I don't think we have something like that around here...<br/>If you really need to find it, go to the Observatory.<br/>Galvin sells random items.");
				break;

			case "shop_grocery":
				Msg("Grocery Store? Haha...<br/>We have a restaurant, but no Grocery Store...<br/>Why don't you visit the Restaurant?<br/>It is near the lake, so you can't miss it.");
				break;

			case "shop_healing":
				Msg("Huh... Are you alright?<br/>Agnes is the Healer of this town.<br/>You can find her if you head<br/>northeast from the center of the city.");
				break;

			case "shop_inn":
				Msg("Hmm... We don't really have an Inn...");
				break;

			case "shop_bank":
				Msg("The Bank is in front of the castle.<br/>Say hello to Jocelin for me.");
				break;

			case "shop_smith":
				Msg("Blacksmith's Shop...? Hmm... It sounds familiar...<br/>Are you sure you don't mean the Weapons Shop?");
				break;

			case "skill_range":
				Msg("Hmm... Aodhan might know something about that.<br/>Come to think of it, I think I heard<br/>that Lucas knows a lot about that topic.");
				break;

			case "skill_instrument":
				Msg("You should ask Nele about that.<br/>He is a really good performer.<br/>He once performed for both of us and<br/>it was so beautiful.");
				break;

			case "skill_composing":
				Msg("I heard about that book somewhere...<br/>Sorry, I don't remember exactly.<br/>Go ask Nele who is at the bridge near the lake.<br/>He might know about it...");
				break;

			case "skill_tailoring":
				Msg("You should go talk to Ailionoa.<br/>If it's anything dealing with clothes, she's the person to go to in this city.");
				break;

			case "skill_gathering":
				Msg("Picking flowers requires careful attention.<br/>It matters whether it's for a pot... or an arrangement...<br/>You have to distinguish them carefully when you collect them.");
				Msg("If you happen to take a part-time job<br/>don't forget about that.");
				break;

			case "square":
				Msg("Come on... <username/>...<br/>This is the Square. hahaha...<br/>Don't tell me you didn't know that?");
				break;

			case "pool":
				Msg("Reservoir? Haha...<br/>We have a big lake right in front of our eyes.<br/>Why would we need a reservoir?");
				break;

			case "temple":
				Msg("The Church is located at the interior of the lake south from here.<br/>It is a very large and beautiful building.<br/>The Priest is very kind, too.<br/>If you have the chance, you should stop by and visit.");
				break;

			case "shop_restaurant":
				Msg("Have you been to the Restaurant owned by Mr. Gordon?<br/>The food is absolutely amazing there.<br/>Plus, Fraser works there. He is Delen and my childhood friend.<br/>I wonder how he's doing. Haha...");
				break;

			case "shop_armory":
				Msg("Are you talking about Osla?<br/>You can find her if you go a little bit west from the Square.<br/>She daydreams a lot, so even if she doesn't answer,<br/>don't think that she's ignoring you on purpose. Hehe....");
				break;

			case "shop_cloth":
				Msg("Ailionoa's Clothing Shop is...<br/>at the east end of the city.<br/>She has a great selection of beauitul clothes<br/>so you should stop by and check it out.");
				break;

			case "bow":
				Msg("I think Fraser mentioned it before...<br/>Lucas, the Club owner,<br/>supposedly has a hobby of collecting rare and expensive bows.<br/>It is kind of scary, but... <br/>on the other hand, it's a very cool hobby.");
				break;

			case "lute":
				Msg("If I remember correctly, I think Nele sells them.<br/>Are you planning to buy one? Haha...");
				break;

			case "mabinogi":
				Msg("A hymn for heroes...<br/>I heard it is very beautiful and romantic.<br/>I wish I could hear something like that one day...");
				break;

			case "breast":
				Msg("Well, what are you talking about...?");
				break;

			case "errand_of_fleta":
				if (!HasItem(70074))
				{
					Send.Notice(Player, ("Received St. John's Wort from Del."));
					Msg("Flowers that shine even during the night?<br/>Ohh... One comes to mind.<br/>It's called the Hypericum Ascyron, a beautiful yellow flower.");
					Msg("The dye that is made with this flower has a bit of glow to it<br/>so it is used often to make special items.<br/>However, you need to be careful because there is poison within the dye.");
					Msg("Mm... If you really, really need one, I can maybe secretly get you one...<br/>Would you like that?");
					Msg("Hehe, here it is.<br/>Don't just waste it on anything,<br/>but take precious care of it.");
					GiveItem(70074);
				}
				else
					Msg("Hey, <username/>!<br/>You already have the flower!<br/>Are you trying to trick me or something?");
				break;

			case "EG_C2_Unknown_Continet":
				Msg("Um... I really don't know.<br/>I think Lucas would know more<br/>about things like that...");
				Msg("Oh, but don't tell him I said<br/>that, ok?");
				break;

			default:
				RndFavorMsg(
					"...On second thought,<br/>Delen might be able<br/>to give you a better answer on that.",
					"...Why does everyone love talking about such difficult matters...?<br/>Maybe I should've paid more attention in school. Hehe...",
					"You can understand...<br/>that I don't really know about that, right...? Hehe...",
					"You're still bothered by that?<br/>...Why don't you just let it go already?",
					"Have you ever gone completely blank<br/>while taking a test?<br/>That's exactly how I feel right now. Hehe...",
					"Stop staring at me like that.<br/>I don't know!",
					"Don't give me that look just because I don't know.<br/>...I'm trying my best here.",
					"...You should be more gentle<br/>when you ask people questions.<br/>...It's not just because I don't know what you're talking about...",
					"Huh? What did you just say?",
					"If I were you, <username/>,<br/>I wouldn't waste my time asking people about things like that.",
					"Why would you ask me something you don't even know...?<br/>That's not nice, hehe...",
					"I've heard it a few times, but I'm not sure."
				);
				ModifyRelation(0, 0, Random(3));
				break;
		}
	}

	protected override async Task Gift(Item item, GiftReaction reaction)
	{
		switch (reaction)
		{
			default: // GiftReaction.Neutral
				RndMsg(
					"Thanks for the gift, <username/>.<br/>You are very kind.",
					"...Have you talked to Delen yet?<br/>...Please keep this a secret from Delen!<br/>Thanks!",
					"Thanks.<br/>This isn't for...Delen, is it?",
					"Is this from you, <username/>...?<br/>Haha... you don't need to buy me such nice things...<br/>The thought is what counts... really.",
					"You're not confusing me for my sister...are you?<br/>If it's for me, I'll gladly take it."
				);
				break;
		}
	}
}

public class DelShop : NpcShopScript
{
	public override void Setup()
	{
		Add(L("Floral Decoration"));

		Add(L("Floral Coronet"), 18088); // Floral Coronet
		Add(L("Floral Coronet"), 18088); // Floral Coronet
		Add(L("Floral Coronet"), 18088); // Floral Coronet

		OnErinnMidnightTick(ErinnTime.Now);
	}
	
	protected override void OnErinnMidnightTick(ErinnTime time)
	{
		// Run base (color randomization)
		base.OnErinnMidnightTick(time);

		ClearTab(L("Floral Decoration"));

		Add(L("Floral Decoration"), 18086); // Daisy Decoration
		Add(L("Floral Decoration"), 18086); // Daisy Decoration
		Add(L("Floral Decoration"), 18086); // Daisy Decoration
		Add(L("Floral Decoration"), 18087); // Rose Decoration
		Add(L("Floral Decoration"), 18087); // Rose Decoration
		Add(L("Floral Decoration"), 18087); // Rose Decoration
		if (time.Month == ErinnMonth.Imbolic) // Sunday
			Add(L("Floral Decoration"), 18090); // Blue Rose Decoration
	}
}
