//--- Aura Script -----------------------------------------------------------
// Nele
//--- Description -----------------------------------------------------------
// Bard and music item shop-keeper.
//---------------------------------------------------------------------------

public class NeleScript : NpcScript
{
	public override void Load()
	{
		SetRace(10002);
		SetName("_nele");
		SetBody(height: 1.1f, upper: 1.2f, lower: 1.2f);
		SetFace(skinColor: 17, eyeType: 5, eyeColor: 0);
		SetLocation(52, 40527, 41230, 217);
		SetGiftWeights(beauty: 1, individuality: 2, luxury: 1, toughness: 0, utility: 0, rarity: -1, meaning: 2, adult: -1, maniac: -1, anime: -1, sexy: 1);

		EquipItem(Pocket.Face, 4900, 0x008A8D91, 0x00954C2C, 0x003B6922);
		EquipItem(Pocket.Hair, 4953, 0x00A97534, 0x00A97534, 0x00A97534);
		EquipItem(Pocket.Armor, 15002, 0x00004040, 0x00539540, 0x006A8C91);
		EquipItem(Pocket.Shoe, 17044, 0x00824D3E, 0x004B4B4B, 0x00B80026);
		EquipItem(Pocket.Head, 18045, 0x0045250C, 0x00000000, 0x00FEF1CB);
		EquipItem(Pocket.Robe, 19004, 0x00FDE8B7, 0x00539540, 0x006A8C91);
		EquipItem(Pocket.RightHand1, 40017, 0x00067974, 0x0031150F, 0x001D5567);
		SetHoodDown();

		AddPhrase("Mmm... nothing like a cool breeze...");
		AddPhrase("Nice weather...");
		AddPhrase("A great day to listen to music, don't you think...?");
		AddPhrase("The sky is the roof, and the land is my bed...");
		AddPhrase("I need to fix this lute...");
		AddPhrase("That water makes such a pleasant sound...");
		AddPhrase("Hmm... should I use this song... or that...");
		AddPhrase("Maybe I should write a new song...");
		AddPhrase("Everyone. Please hear my music.");
		AddPhrase("Hmm... I've just thought of something...");
		AddPhrase("Hmm... I'm hungry... Hahaha...");
	}

	protected override async Task Talk()
	{
		SetBgm("NPC_Nele.mp3");

		await Intro(L("He's got long brown hair, although you would never be able to guess the last time his hair had been combed.<br/>His hat is worn so low that you can barely make out his faintly smiling lips. His worn out hat and robe look like they have been<br/>through many travels with him.<br/>Although you cannot see his eyes, by the look of his expressions, you can tell he has a jovial temperament. Whenever someone<br/>talks to him, he greets them with a soft but cheerful voice, like a friendly puppy or an old friend."));

		Msg("Do you want to listen to a song...?", Button("Start a Conversation", "@talk"), Button("Shop", "@shop"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());

				if (Player.IsUsingTitle(11001))
				{
					Msg("This is great...<br/>You might hear a mabinogi named after you in Erinn, <username/>...");
				}
				else if (Player.IsUsingTitle(11002))
				{
					Msg("I can't believe you, the one who was researching about Macha earlier,<br/>has become the Guardian of Erinn...");
					Msg("...I'm assuming you've stopped Macha's curse...?<br/>Thank you. <username/>...");
				}

				await Conversation();
				break;

			case "@shop":
				if (Memory >= 15 && Favor >= 50 && Stress <= 5)
					Msg("Hey, are you here to buy something from me again?<br/>Since you are here so often, <username/>, I feel like I have to show you something very special...<br/>Haha, take a look...");
				else
					Msg("Do you want anything? Feel free to take a look...");
				OpenShop("NeleShop");
				return;
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
			Msg(FavorExpression(), L("Hello there, did you just follow the wind over this way? Haha..."));
		}
		else if (Memory == 1)
		{
			Msg(FavorExpression(), L("Oh, it's you again. This must be some kind of miracle. Hahaha..."));
		}
		else if (Memory == 2)
		{
			Msg(FavorExpression(), L("I'm seeing you a lot these days, <username/>. How do you like today's weather...?"));
		}
		else if (Memory <= 6)
		{
			Msg(FavorExpression(), L("Hello, <username/>. You look wonderful today...haha."));
		}
		else
		{
			Msg(FavorExpression(), L("I was wondering whether you would come or not...<br/>And here you are! Hahaha..."));
		}

		UpdateRelationAfterGreet();
	}

	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				if (Memory >= 15 && Favor >= 50 && Stress <= 5)
				{
					Msg(FavorExpression(), "Do you believe in destiny, <username/>?<br/>Well... I do.");
					Msg("I am sure you have a very unique destiny awaiting you.");
					Msg("How do I know this?<br/>I just do... I just do...");
					ModifyRelation(Random(2), 0, Random(2));
				}
				else if (Memory >= 15 && Favor >= 30 && Stress <= 5)
				{
					Msg(FavorExpression(), "For some reason, whenever I talk to you, <username/>,<br/>you remind me of this girl... who'd left me a long time ago.<br/>She was very nice and easygoing just like you.");
					Msg("Hmm...");
					Msg("Hehe, I think I just caught a cold.<br/>Why do I keep getting a lump in my throat?");
					ModifyRelation(Random(2), 0, Random(2));
				}
				else if (Favor >= 10 && Stress <= 10)
				{
					Msg(FavorExpression(), "Some people ask me why I don't sing for them...<br/>Hmm...");
					Msg("Well...");
					Msg("I don't know...");
					Msg("Maybe someday I'll sing again...<br/>Someday...");
					ModifyRelation(Random(2), Random(2), Random(2));
				}
				else
				{
					Msg(FavorExpression(), "My name is <npcname/>. It means \"Clouds\"...<br/>I go wherever the wind blows... wherever my feet lead me...<br/>With my name, don't you think it fits that I am sort of a wanderer?");
					ModifyRelation(Random(2), 0, Random(3));
				}

				break;

			case "rumor":
				if (Memory >= 15 && Favor >= 50 && Stress <= 5)
				{
					Msg(FavorExpression(), "The Sen Mag Plains is where countless Humans and Fomors perished<br/>during the time of war in Erinn...<br/>Maybe because of that, people claim to have seen<br/>ghosts of warriors around the area...");
					Msg("Hahaha, whatever...");
					ModifyRelation(Random(2), 0, Random(2));
				}
				else if (Memory >= 15 && Favor >= 30 && Stress <= 5)
				{
					Msg(FavorExpression(), "The father of the Lord that used to lead this place a long time ago<br/>had two sons.<br/>One of his sons, when he was very young,<br/>left home to see the world.");
					Msg("The current Lord is the son that remained...<br/>I wonder what happened to the one that had left...");
					Msg("Who knows, he might be back in this town,<br/>disguised in his identity, and working at a bar or something...");
					ModifyRelation(Random(2), 0, Random(2));
				}
				else if (Favor >= 10 && Stress <= 10)
				{
					Msg(FavorExpression(), "Emain Macha has been known as a liberal, tourist town<br/>with scores of tourists visiting this place everyday.<br/>That's what's brought me here, too... the free atmosphere I'd felt here...");
					Msg("Lately, however, it feels different around here.<br/>I wonder why that is...");
					ModifyRelation(Random(2), 2, Random(3));
				}
				else
				{
					Msg(FavorExpression(), "There are many legends about the water fountain at the Square.<br/>One of the tales says that if someone in love tosses a coin in and prays,<br/>then the prayer will be answered.<br/>What do you think? Do you want to give it a try...?");
					ModifyRelation(Random(2), 0, Random(3));
				}
				break;

			case "about_skill":
				Msg("Well...<br/>I don't think I have any information that will help you, <username/>.");
				break;

			case "shop_misc":
				Msg("Hmm... I think Galvin sells a lot of this and that in front of the Observatory.<br/>The Observatory is riiight over there, riiight over there...");
				break;

			case "shop_grocery":
				GiveKeyword("shop_restaurant");
				Msg("Are you going to buy me something? Hahaha...<br/>Come to think of it, I AM a little hungry...");
				break;

			case "shop_healing":
				Msg("You mean Agnes the beautiful...<br/>She's a really nice lady, although she always gives me an earful<br/>for sleeping anywhere on the ground. Hahaha...");
				Msg("Wait, we were talking about the Healer's House, right?<br/>Follow the northbound road in front of you.<br/>You should be able to find it pretty quickly...");
				break;

			case "shop_inn":
				Msg("The sky is the roof... and the land is my bed...<br/>If you are a bard like me, the whole world is your home.");
				break;

			case "shop_bank":
				Msg("I haven't had money in a long time...<br/>I think it's somewhere on the way to the castle.");
				break;

			case "shop_smith":
				Msg("There's no Blacksmith's Shop around Emain Macha, but there's a Weapons Shop...<br/>Head west from here and you'll find it.");
				break;

			case "skill_rest":
				Msg("I like it... It's my favorite skill out there... Haha.");
				break;

			case "skill_instrument":
				Msg("Easy. Anyone can learn it, as long as he or she has an instrument to play with...<br/>You don't even have to be awesome at it.<br/>You just need to be good enough to have fun with it.<br/>Music is supposed to be fun...");
				break;

			case "skill_tailoring":
				Msg("Hmm... So if I were to have that skill...<br/>would it be easier for me to sew my own clothes...?<br/>What do you think? Do you think I should learn it?");
				break;

			case "skill_gathering":
				Msg("If I'm hungry, I pick apples and other fruits from the tree...<br/>If I'm thirsty, then I drink water... That's pretty much how my life is...");
				break;

			case "square":
				Msg("You ARE at the Square...");
				break;

			case "pool":
				Msg("There is a lake here... maybe that's why there isn't one.");
				break;

			case "farmland":
				Msg("Corns around Emain Macha are very delicious.<br/>Eh... Well... I picked out a few from the field, but...<br/>please don't tell anyone. Hahaha...");
				break;

			case "brook":
				Msg("Beautiful name, isn't it?<br/>I am sure the stream is just as beautiful as the name suggests...");
				break;

			case "shop_headman":
				Msg("I grew up in a little town where there's a Chief.<br/>He liked having kids around, and<br/>I remember him giving me some food whenever I went over to his place...");
				Msg("It's probably the kind of thing you'd never experience in a town this big.");
				break;

			case "temple":
				Msg("Do you see the building on the inner part of the lake?<br/>That is the Church of Emain Macha.<br/>It's the second biggest church in the whole continent of Uladh.<br/>Compared to its massive size, though, I haven't seen too many people going there...");
				break;

			case "school":
				Msg("I think it's more important to have great teachers<br/>than it is to have a nice building called the school...");
				Msg("Hmm... I wonder how my teacher's doing these days...");
				break;

			case "skill_campfire":
				Msg("I like it... It's my second favorite skill...<br/>Hmm, you want to know my favorite skill...?<br/>Well... I don't know... should I tell you...? Haha...");
				break;

			case "shop_restaurant":
				Msg("You must be talking about Gordon's 'Loch Lios.'<br/>'Loch Lios' means a garden by the lake.<br/>If you look at his restaurant, and the name itself...<br/>Gordon certainly has good taste in general.");
				break;

			case "shop_armory":
				Msg("Ah, Osla's Weapons Shop.<br/>She is a very interesting person, actually. She's really good at scribbling, too...<br/>Fraser told me the other day that Osla and I talk the same,<br/>followed by a huge sigh... Do you think we really talk alike?");
				break;

			case "shop_cloth":
				Msg("Ailionoa at the Clothing Shop makes gorgeous dresses.<br/>She always seems to have that sad look, though.<br/>She would look a lot more beautiful if she would smile more often...");
				break;

			case "shop_bookstore":
				Msg("The bookstore? Well...<br/>I don't think I've seen one around here...");
				Msg("There's a book dealer around here called Buchanan,<br/>but I think he's out of town to see other book dealers...");
				break;

			case "shop_goverment_office":
				Msg("The Lord governs everything around Emain Macha.<br/>However, it seems that these days, someone else<br/>has more power than our Lord...<br/>Wait, I didn't say that! I didn't, I didn't... Ha...ha...");
				break;

			case "graveyard":
				Msg("Hmmm... I don't like to talk about sad stories...");
				break;

			case "fishing":
				Msg("Are you aware that Priest James enjoys fishing<br/>a lot more than he would like people to believe...?");
				break;

			case "lute":
				Msg("I hear some people tell me<br/>this lute looks old, and that I'll need it replaced...<br/>I've been playing this lute for so long, though,<br/>that this lute feels more like a friend to me than an instrument...");
				break;

			case "tir_na_nog":
				Msg("Tir Na Nog is the ideal land of utopia where<br/>eternal happiness and youth supposedly exist...");
				Msg("But...<br/>is there a thing such as eternal happiness...?");
				break;

			case "musicsheet":
				Msg("I have plenty of music scores... Would you like to buy some?<br/>If you do, then press 'Shop'...");
				break;

			case "g3_DarkKnight":
				Msg("...You're asking me<br/>about the Dark knight...?<br/>Now it's nout about Macha,<br/>but a new situation involving Dark Knights...?");
				Msg("...When will we ever have peace...");
				break;

			case "EG_C2_Unknown_Continet":
				Msg("I don't know... maybe there is...<br/>and maybe there isn't...<br/>Argh...");
				break;

			case "EG_C2_HowTo_Shipboard":
				Msg("Well, sailing on a ship certainly sounds very romantic...<br/>Being in the ocean is a little like giving up your body,<br/>perhaps even your life, to the flow of nature in a way... you know what I mean?<br/>You'll have to have a lot of courage... haha...");
				break;

			// keyword should be removed upon ranking the skill up rather than 
			// after receiving skill training
			case "musical_know_a_nele_loeiz":
				if (IsSkill(SkillId.MusicalKnowledge, SkillRank.RA))
				{
					Msg("What? You read my mentor's book?<br/>Erinn's Musicians...? I see.<br/>... I don't know how my mentor Loeiz is doing...");
					Msg("......<p/>...To be honest, I have actually been excommunicated from him.<br/>He is essentially the pillar of the old school of thought...<br/>It's like a slap in the face for him to have a student like me.<br/>You know, a student who is obsessed with putting a magical element in music...");
					Msg("That's right, I have quite an interest in<br/>using magic effects within music.");
					Msg("...But, I don't think my thinking is wrong.<br/>If a Bard cannot help those<br/>who really need help with their music<br/>because of their stubbornness for the purity of music...");
					Msg("...I believe that is wrong<br/>first and foremost as a human being.");
					Msg("Humans have such a limited lifespan...<br/>Isn't it meaningful in and of itself if<br/>you can help even one person<br/>with your gifts?");
					Msg("...They say magic effects take place<br/>in scores that are written by people with a high musical knowledge rank and composing rank.<br/>If you happen to be interested in<br/>musical knowledge, why don't you try composing...?", Button("Ok", "@ok"));
					await Select();

					RemoveKeyword("musical_know_a_nele_loeiz");
					TrainSkill(SkillId.MusicalKnowledge, 1);

					Msg("Yes.<br/>Thank you for showing interest in magical music.");
					Msg("If you want to compose magical songs, your Composing skill needs to be at rank 9<br/>and in order to perform it, you need to raise your Playing Instrument skill to at least 9 as well.<br/>Until then, I can't really help you.");
					Msg("May your life be filled with joy through music...");
				}
				break;

			default:
				if (Favor >= 10 && Stress <= 10)
				{
					RndFavorMsg(
						"I'm sorry I couldn't be of any help.",
						"Hahaha... Well... I don't know...",
						"Hmm... I don't know... If you ever find out, please let me know.",
						"Ah, it's something that I am not aware of...",
						"Should we find out about that together sometime?",
						"Hmm... I wish I knew what that was... Haha...",
						"Hmm, what is that...?"
					);
					ModifyRelation(0, 0, Random(2));
				}
				else
				{
					RndFavorMsg(
						"I see... well, I don't know much about it...",
						"Um... so... um...",
						"Well... My guess is as good as yours...",
						"Hmmm... Sorry...<br/>I don't know much about that... Hahaha...",
						"Sorry... I don't know."
					);
					ModifyRelation(0, 0, Random(3));
				}
				break;
		}
	}

	protected override async Task Gift(Item item, GiftReaction reaction)
	{
		switch (reaction)
		{
			case GiftReaction.Love:
				RndMsg(
					"Whoa... I've always wanted this... How did you know? It's not something I'd tell the whole world about or anything...",
					"<username/>, you must be an angel from heaven...",
					"Whoa... can I really have this...?",
					"Oh... Are you serious...? This is too much for me..."
				);
				break;

			case GiftReaction.Like:
				RndMsg(
					"For me? Thanks!",
					"I'm not sure I should accept this, but... thank you."
				);
				break;

			case GiftReaction.Dislike:
				RndMsg(
					"Most people shriek when they receive a gift like this...",
					"You know, I heard that tossing something in the water fountain brings good luck... can I throw this in there?",
					"What...",
					"This is not right..."
				);
				break;

			default: // GiftReaction.Neutral
				Msg(L("Thanks. I like it..."));
				break;
		}
	}
}

public class NeleShop : NpcShopScript
{
	public override void Setup()
	{
		Add("Instruments", 40004); // Lute
		Add("Instruments", 40004); // Lute
		Add("Instruments", 40004); // Lute
		Add("Instruments", 40017); // Mandolin
		Add("Instruments", 40017); // Mandolin
		Add("Instruments", 40017); // Mandolin
		Add("Instruments", 40018); // Ukulele
		Add("Instruments", 40018); // Ukulele
		Add("Instruments", 40018); // Ukulele		

		Add("Music Score", 61001); // Score Scroll
		Add("Music Score", 61001); // Score Scroll
		Add("Music Score", 61001); // Score Scroll
		Add("Music Score", 61001); // Score Scroll
		Add("Music Score", 61001); // Score Scroll
		//TODO add pre-made scores

		Add("Music Book", 1006); // Introduction to Music Composition
		Add("Music Book", 1011); // Improving Your Composing Skill
		Add("Music Book", 1018); // The History of Music in Erinn (1)
		Add("Music Book", 1019); // The History of Music in Erinn (2)
		Add("Music Book", 1020); // Composition Lessons with Helene (1)
		Add("Music Book", 1013, 1, 80000); // Music Theory
		Add("Music Book", 1087); // Wind Instruments: Melodies of the Soul

		// TODO: add items to the existing Music Book tab when favor is >= 50	
		Add("Special Music Books", (creature, npc) => npc.GetFavor(creature) >= 50); // Allow access with >= 50 favor	
		Add("Special Music Books", 1111); // The Path of Composing
		Add("Special Music Books", 1086); // What is an Ensemble?
		Add("Special Music Books", 1121); // Instrument Ranges
		Add("Special Music Books", 1122); // The Fundamentals of Becoming a Great Composer

		AddQuest("Quest", 71018, 30); // Collect the Black Spider's Fomor Scrolls
		AddQuest("Quest", 71032, 30); // Collect the Mimic's Fomor Scrolls
		AddQuest("Quest", 71035, 30); // Collect the Gray Town Rat's Fomor Scrolls
		AddQuest("Quest", 71049, 30); // Collect the Snake's Fomor Scrolls
		AddQuest("Quest", 71052, 30); // Collect the Jackal's Fomor Scrolls
		AddQuest("Quest", 1016, 350); // Produce Massive Ice Elemental
		AddQuest("Quest", 1017, 350); // Produce Massive Fire Elemental
		AddQuest("Quest", 1018, 350); // Produce Massive Lightning Elemental

		if (IsEnabled("PercussionInstruments"))
		{
			Add("Instruments", 40214); // Big Drum
			Add("Instruments", 40214); // Big Drum
			Add("Instruments", 40214); // Big Drum
			Add("Instruments", 40215); // Small Drum
			Add("Instruments", 40215); // Small Drum
			Add("Instruments", 40215); // Small Drum
			Add("Instruments", 40216); // Cymbols
			Add("Instruments", 40216); // Cymbols
			Add("Instruments", 40216); // Cymbols
		}

		// Dye-able versions of woodwind instruments were added at some point
		// Is this when they were added?
		if (IsEnabled("!G18S4"))
		{
			Add("Instruments", 40048); // Whistle
			Add("Instruments", 40048); // Whistle
			Add("Instruments", 40048); // Whistle
			Add("Instruments", 40049); // Flute
			Add("Instruments", 40049); // Flute
			Add("Instruments", 40049); // Flute
			Add("Instruments", 40050); // Chalumeau
			Add("Instruments", 40050); // Chalumeau
			Add("Instruments", 40050); // Chalumeau

		}

		if (IsEnabled("G18S4"))
		{
			Add("Instruments", 40658); // Whistle (dyeable)
			Add("Instruments", 40659); // Flute (dyeable)
			Add("Instruments", 40660); // Chalumeau (dyeable)
		}

		if (IsEnabled("PropInstruments"))
			Add("Instruments", 41123); // Cello
	}
}
