//--- Aura Script -----------------------------------------------------------
// Simon
//--- Description -----------------------------------------------------------
// Clothing Store Owner
//---------------------------------------------------------------------------

public class SimonScript : NpcScript
{
	public override void Load()
	{
		SetRace(10002);
		SetName("_simon");
		SetBody(height: 1.10f, weight: 0.8f, upper: 0.8f, lower: 0.8f);
		SetFace(skinColor: 15, eyeType: 8, eyeColor: 25, mouthType: 0);
		SetStand("human/male/anim/male_natural_stand_npc_Simon");
		SetLocation(17, 1314, 921, 24);
		SetGiftWeights(beauty: 2, individuality: 2, luxury: 2, toughness: -1, utility: 0, rarity: 2, meaning: 1, adult: 0, maniac: 1, anime: 0, sexy: 1);

		EquipItem(Pocket.Face, 4902, 0x00F28427, 0x00844203, 0x0079C36D);
		EquipItem(Pocket.Hair, 4024, 0x00998866, 0x00998866, 0x00998866);
		EquipItem(Pocket.Armor, 15045, 0x00D6D8DE, 0x0031208E, 0x00FF9B3B);
		EquipItem(Pocket.Shoe, 17013, 0x009C7B6B, 0x00F79825, 0x00007335);

		AddPhrase("The fabric I ordered should be coming in any day now...");
		AddPhrase("Time just flies today. Heh.");
		AddPhrase("This... is too last-minute.");
		AddPhrase("That man over there... What he's wearing is so 20 minutes ago.");
		AddPhrase("Let's see... Which ones do I have to finish by today?");
		AddPhrase("Ugh! This world is so devoid of beauty.");
		AddPhrase("Travelers... How are they so careless about their appearance?");
		AddPhrase("Heeheehee... She's got some fashion sense.");
		AddPhrase("Oops! I was supposed to do that tomorrow, wasn't I? Heehee...");
	}

	protected override async Task Talk()
	{
		SetBgm("NPC_Simon.mp3");

		await Intro(L("With a long face, narrow shoulders, and a pale complexion, this man crosses his delicate hands in front of the chest and sways left and right.<br/>His demeanor is exaggerated and the voice nasal. He seems to have a habit of glancing sideways with those light brown eyes.<br/>His fashionable shirt has an intricate pattern and was made with great care."));

		Msg("What do you want?", Button("Start a Conversation", "@talk"), Button("Shop", "@shop"), Button("Repair Item", "@repair"), Button("Modify Item", "@upgrade"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());

				if (Title == 11001)
				{
					Msg("<username/>, the one who saved the Goddess...?");
					Msg("...Wait, if you're so great as to be saving the Goddess,<br/>shouldn't you know to be humble too?");
					Msg("...Soon, all the rumors of your self-aggrandizing behavior will start catching up with you.");
					Msg("Even so, I have to admit that what you did was pretty fabulous.");
				}
				else if (Title == 11002)
				{
					Msg("...Doesn't a title like that overwhelm you at all?<br/>Well... Judging by your confident look,<br/>I guess you have the skills to back it up.");
				}

				await Conversation();
				break;

			case "@shop":
				Msg("Hmm. Take your time to look around.");
				OpenShop("SimonShop");
				return;

			case "@repair":
				Msg("Want to mend your clothes?<br/>Rest assured, I am the best this kingdom has to offer. I never make mistakes.<br/>Because of that, I charge a higher repair fee.<br/>If you can stomach a cheap repair, go find someone else. I only work with top quality.<repair rate='98' stringid='(*/cloth/*)|(*/glove/*)|(*/bracelet/*)|(*/shoes/*)|(*/headgear/*)|(*/robe/*)|(*/headband/*)' />");

				while (true)
				{
					var repair = await Select();

					if (!repair.StartsWith("@repair"))
						break;

					var result = Repair(repair, 98, "/cloth/", "/glove/", "/bracelet/", "/shoes/", "/headgear/", "/robe/", "/headband/");
					if (!result.HadGold)
					{
						RndMsg(
							"You don't have any money, do you?",
							"You probably won't have enough money for the repair job.",
							"Hey, hey. Take a look at your wallet before you ask."
						);
					}
					else if (result.Points == 1)
					{
						if (result.Fails == 0)
							RndMsg(
								"It's a perfect repair job.",
								"There. 1 point, just like that.",
								"Repairing 1 point is nothing."
							);
						else
							Msg("Hmm... Sorry, I think I've failed the repair job."); // Should be 2
					}
					else if (result.Points > 1)
					{
						if (result.Fails == 0)
							RndMsg(
								"\"A supernatural needlework\" would describe it.",
								"The repair was perfect, but the quality of the clothing is rather cheap.",
								"The clothes I repair are just like brand new."
							);
						else
							// TODO: Use string format once we have XML dialogues.
							Msg("There, it's done.<br/>But I made " + result.Fails + " mistake(s), unfortunately.<br/>I could restore only " + result.Successes + " point(s).");
					}
				}

				Msg("No more?<br/>Then, bye!<repair hide='true'/>");
				break;

			case "@upgrade":
				Msg("Hmm... You want to modify your clothes? Like custom-made?<br/>Well, show me what you want modified. I'll make sure it fits you like a glove.<br/>But, you know that once I modify it, no one else can wear it anymore, right?<upgrade />");

				while (true)
				{
					var reply = await Select();

					if (!reply.StartsWith("@upgrade:"))
						break;

					var result = Upgrade(reply);
					if (result.Success)
						Msg("Modification done.<br/>Anything else you want modified?");
					else
						Msg("(Error)");
				}

				Msg("Come see me again next time if you have something else to upgrade.<upgrade hide='true'/>");
				break;
		}

		End("Thank you, <npcname/>. I'll see you later!");
	}

	private void Greet()
	{
		if (DoingPtjForNpc())
		{
			Msg(FavorExpression(), L("Hmm? A part-timer at my shop?<br/>Keep in mind that being indecisive is very unprofessional."));
		}
		else if (Memory <= 0)
		{
			Msg(FavorExpression(), L("Is this your first time here?"));
		}
		else if (Memory == 1)
		{
			Msg(FavorExpression(), L("Haha. That's right.<br/>You have to come often to be recognized."));
		}
		else if (Memory == 2)
		{
			Msg(FavorExpression(), L("Your name is... <username/>, right?"));
		}
		else if (Memory <= 6)
		{
			Msg(FavorExpression(), L("You are back, <username/>."));
		}
		else
		{
			Msg(FavorExpression(), L("Of course, I would like for you to visit often.<br/>Take your time and look around, <username/>."));
		}

		UpdateRelationAfterGreet();
	}

	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				if (Memory == 1)
				{
					Msg(FavorExpression(), "I don't think we've met. I'm <npcname/>.");
					ModifyRelation(1, 0, 0);
				}
				else
				{
					GiveKeyword("shop_cloth");
					Msg(FavorExpression(), "So, are you saying that you don't know the only<br/>Clothing Shop in Dunbarton that happens to be mine?<br/>You're denser than you look.");
					ModifyRelation(Random(2), 0, Random(3));
				}
				break;

			case "rumor":
				GiveKeyword("shop_bookstore");
				Msg(FavorExpression(), "I don't like to talk about people behind their backs.<br/>It's not a very good habit and you should get rid of it, too.<br/>Oh... You didn't mean that? Oh, I am so sorry.");
				Msg("Aeira at the Bookstore seems to be very interested in music.<br/>If you happen to be interested in music, be nice to her.<br/>She'll give you something good if you become friends.");
				ModifyRelation(Random(2), 0, Random(3));
				break;

			case "shop_misc":
				Msg("It's right next door. Didn't you see it?<br/>Well, the owner does look intimidating.<br/>That look on his face drives away potential customers.");
				Msg("But Walter is still a good man.<br/>Just try to get to know him a little.");
				break;

			case "shop_grocery":
				GiveKeyword("shop_restaurant");
				Msg("Are you interested in buying cooking ingredients?<br/>Go to the Restaurant.<br/>The Restaurant carries them, too.");
				break;

			case "shop_healing":
				Msg("Oh, yeah. You're looking for Manus' place.<br/>His house is near the south entrance.<br/>Keep following the main road next to the Square.");
				Msg("That building is L-shaped, so once you see the healer sign,<br/>you should have no trouble finding it.");
				break;

			case "shop_inn":
				Msg("An inn? Hmm?<br/>I didn't know there was one in this town.");
				break;

			case "shop_bank":
				Msg("Ha! It's right next door.<br/>You can't just keep asking questions about places<br/>you haven't even been to.");
				Msg("It doesn't hurt to actually go to those places.");
				break;

			case "shop_smith":
				GiveKeyword("shop_armory");
				Msg("Mmm? You are definitely not from here, are you?<br/>The villagers here knows not to ask ME<br/>where the Blacksmith's Shop is in this town.");
				Msg("If you must know,<br/>go talk to Nerys at the Weapons Shop.<br/>She's rather nice to outsiders.<br/>After all, she once was a traveler, too.");
				break;

			case "skill_range":
				Msg("Well! What's the point of asking me that question?");
				break;

			case "skill_instrument":
				Msg("Hmm. You are deceptively classy.<br/>I didn't know you'd be interested in that kind of a skill.<br/>I've misjudged you.");
				Msg("Unfortunately, I don't really know much about it. Sorry.");
				break;

			case "skill_composing":
				Msg("Hmm. I doubt there's<br/>anyone in this town who knows something like that.<br/>Sorry. I don't really know either.");
				break;

			case "skill_tailoring":
				GiveKeyword("shop_misc");
				Msg("Ha! You want to make clothes?<br/>Why don't you stop by the General Shop first?");
				Msg("You can simply buy the cheap tailoring kit<br/>and equip it to learn the skill.<br/>Did you ask me about the Tailoring skill<br/>so you can get it from me for free?");
				Msg("Don't be so cheap!");
				break;

			case "skill_magnum_shot":
				Msg("Well, I can't stand that sort of barbaric skill.");
				break;

			case "skill_counter_attack":
				Msg("Can't you talk about a more civilized<br/>topic with me?");
				break;

			case "skill_gathering":
				Msg("To make clothes, you need fabric.<br/>There's silk fabric made from thread produced by spiders or cocoon, and<br/>woolen fabric made from wool.");
				Msg("Either way, if you have the Gathering skill and the proper tools,<br/>it will help you when making fabric.<br/>If you make some, bring it to me and I'll pay you generously for it.");
				break;

			case "square":
				Msg("The Square is right there.<br/>You have no sense of direction, do you?");
				break;

			case "farmland":
				Msg("It's all farmlands around this town.<br/>I guess you haven't been there yet, have you?<br/>There has been a crisis because of rats.");
				break;

			case "shop_headman":
				Msg("There's a chief in this town?<br/>I've lived here for a while now,<br/>but that's news to me.");
				break;

			case "temple":
				Msg("The Church? Follow this alley and keep going up.<br/>You'll see the Church soon.<br/>If you need anything,<br/>Priestess Kristell will help you.");
				break;

			case "school":
				Msg("The School is over there.<br/>Go to the opposite side of the town.<br/>Talk to Stewart or Aranwen and you'll pick up some useful information.");
				break;

			case "skill_windmill":
				Msg("Even the name of that skill sounds barbaric.<br/>Ask someone else about it.");
				break;

			case "shop_restaurant":
				Msg("The Restaurant? You mean Glenis' place?<br/>It's just over there, too.<br/>Well, I don't have anything to say about that place in particular.");
				break;

			case "shop_armory":
				Msg("The Weapons Shop? Are you looking to buy an armor?<br/>Then what are you doing at my Clothing Shop?<br/>How irritating...");
				break;

			case "shop_cloth":
				Msg("Is this supposed to be a trick question?<br/>Doesn't this look like a Clothing Shop to you?");
				Msg("Hmm, if it really doesn't, though, then that's a problem...<br/>Perhaps I should remodel the interior.");
				break;

			case "shop_bookstore":
				Msg("The Bookstore? You mean that cute little Aeira's Bookstore?<br/>Heehee... She's up the north alley.<br/>If you see Aeira, tell her I said hi.");
				break;

			case "shop_goverment_office":
				Msg("Oh! You mean where that elegant lady Eavan works?<br/>The Town Office is just over there.<br/>Go left a little, then out to the edge of the Square.<br/>You'll see a large building there. That's the one.");
				break;

			case "bow":
				Msg("We don't sell bows here.<br/>Go to the Weapons Shop for that.");
				break;

			case "lute":
				GiveKeyword("shop_misc");
				Msg("A lute? Hmm...<br/>Ask Walter, the guy with that intimidating look over there.<br/>It's all right. He won't bite. Don't be so scared and go check out the General Shop.");
				break;

			case "tir_na_nog":
				Msg("Tir Na Nog...<br/>It's the eternal Utopia that everyone in Tuatha de Danann<br/>dreams of.");
				Msg("Have you ever imagined a world where there is no death?<br/>They say there's no pain there, only full of joy and happiness.");
				Msg("Well, that's all a work of imagination.<br/>That kind of place does not exist.<br/>Don't fall for that fairy tale.");
				break;

			case "musicsheet":
				GiveKeyword("shop_misc");
				Msg("Music Scores? I thought Walter sold them next door.<br/>Have you checked with him?");
				Msg("You know you can't play music without<br/>a music score, don't you?");
				break;

			case "jewel":
				Msg("Sometimes I use various gem accessories<br/>to accentuate my outfit.");
				Msg("If I have to choose my most favorite gem,<br/>I won't hesitate to choose the bloody red Ruby.");
				Msg("I like looking elegant,<br/>but sometimes I want to express my burning passion<br/>through that red colored gem. Hoho.");
				break;

			default:
				RndFavorMsg(
					"Go ask someone else.",
					"Hmm. What a boring topic.",
					"I don't know anything about that.",
					"How should I know anything about that?",
					"Should I know about something like that?",
					"So... what do you want me to do about that?",
					"Don't you think you've had enough? Let's talk about something else."
				);
				ModifyRelation(0, 0, Random(3));
				break;
		}
	}
}

public class SimonShop : NpcShopScript
{
	public override void Setup()
	{
		Add("Robes", 19003); // Tri-color Robe
		Add("Robes", 19003); // Tri-color Robe
		Add("Robes", 19003); // Tri-color Robe
		Add("Robes", 19003); // Tri-color Robe

		Add("Hats", 18012); // Tork's Merchant Hat
		Add("Hats", 18003); // Lirina's Cap
		Add("Hats", 18011); // Mongo Jester Cap
		Add("Hats", 18007); // Popo Cap
		Add("Hats", 18002); // Stripe Cap
		Add("Hats", 18051); // Cores' Ribbon Hat
		Add("Hats", 18013); // Cores' Cap
		Add("Hats", 18014); // Mongo's Hat
		Add("Hats", 18004); // Mongo's Fashion Cap
		Add("Hats", 18124); // Sandra's Sniper Suit Cap
		Add("Hats", 18046); // Tiara
		Add("Hats", 18010); // Mongo's Smart Cap
		Add("Hats", 18000); // Tork's Cap
		Add("Hats", 18042); // Cores' Oriental Hat
		Add("Hats", 18009); // Mongo's Archer Cap

		Add("Shoes && Gloves", 16015); // Bracelet
		Add("Shoes && Gloves", 17017); // Leather Shoes
		Add("Shoes && Gloves", 16006); // Guardian Gloves
		Add("Shoes && Gloves", 16017); // Standard Gloves
		Add("Shoes && Gloves", 16024); // Pet Instructor Glove
		Add("Shoes && Gloves", 17060); // Sandra's Sniper Suit Boots (M)
		Add("Shoes && Gloves", 17029); // Belt Buckle Boots
		Add("Shoes && Gloves", 17019); // Blacksmith Shoes
		Add("Shoes && Gloves", 17061); // Sandra's Sniper Suit Boots (F)
		Add("Shoes && Gloves", 16013); // Swordsman Gloves
		Add("Shoes && Gloves", 17067); // X Tie-Up Shoes
		Add("Shoes && Gloves", 17010); // Cores' Boots (M)
		Add("Shoes && Gloves", 17023); // Enamel Shoes
		Add("Shoes && Gloves", 16019); // Lirina Striped Gloves
		Add("Shoes && Gloves", 16031); // Tight Tri-lined Gloves
		Add("Shoes && Gloves", 16026); // Sandra's Sniper Suit Gloves
		Add("Shoes && Gloves", 16016); // Light Gloves
		Add("Shoes && Gloves", 17040); // Ella's Strap Boots
		Add("Shoes && Gloves", 17003); // Leather Shoes
		Add("Shoes && Gloves", 17024); // Open-toe Platform Sandal
		Add("Shoes && Gloves", 17013); // Thick Sandals
		Add("Shoes && Gloves", 17069); // Leo Shoes
		Add("Shoes && Gloves", 16032); // Elven Glove
		Add("Shoes && Gloves", 17041); // Vine-print Hunting Boots

		Add("Clothes", 15022); // Popo's Skirt
		Add("Clothes", 15044); // Carpenter's Clothes
		Add("Clothes", 15023); // Tork's Hunter Suit (F)
		Add("Clothes", 15022); // Belted Casual Wear
		Add("Clothes", 15051); // Popo's Skirt
		Add("Clothes", 15035); // Tork's Hunter Suit (M)
		Add("Clothes", 15052); // Terks' Two-Tone Tunic
		Add("Clothes", 15033); // Mongo's Traveler Suit (M)
		Add("Clothes", 15020); // Cores' Healer Dress
		Add("Clothes", 15027); // Mongo's Long Skirt
		Add("Clothes", 15041); // Female Business Suit

		Add("Fine Clothes", 15016); // Ceremonial Stocking
		Add("Fine Clothes", 15154); // Sandra's Sniper Suit (F)
		Add("Fine Clothes", 15026); // Lirina's Long Skirt
		Add("Fine Clothes", 15151); // Mario NY Modern Vintage Ensemble (M)
		Add("Fine Clothes", 15042); // High Neck One-piece Dress
		Add("Fine Clothes", 15152); // Mario NY Modern Vintage Ensemble (F)
		Add("Fine Clothes", 15045); // Ruffled Tuxedo Ensemble
		Add("Fine Clothes", 15053); // Flat Collar One-Piece Dress
		Add("Fine Clothes", 15032); // Lirina's Shorrts
		Add("Fine Clothes", 15014); // Messenger Wear
		Add("Fine Clothes", 15153); // Sandra's Sniper Suit (M)
		Add("Fine Clothes", 15029); // Tork's Blacksmith Suit
		Add("Fine Clothes", 15019); // Cores' Ninja Suit (F)
		Add("Fine Clothes", 15017); // Chinese Dress
		Add("Fine Clothes", 15067); // Oriental Warrior Suit
		Add("Fine Clothes", 15064); // Idol Ribbon Dress

		Add("Event"); // Empty
	}
}
