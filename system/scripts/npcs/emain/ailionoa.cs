//--- Aura Script -----------------------------------------------------------
// Ailionoa
//--- Description -----------------------------------------------------------
// Clothing Store Owner
//---------------------------------------------------------------------------

public class AilionoaScript : NpcScript
{
	public override void Load()
	{
		SetRace(10001);
		SetName("_ailionoa");
		SetFace(skinColor: 16, eyeType: 3, eyeColor: 114, mouthType: 14);
		SetStand("human/female/anim/female_natural_stand_npc_03");
		SetLocation(52, 48618, 44955, 131);
		SetGiftWeights(beauty: 2, individuality: 2, luxury: 2, toughness: -1, utility: 0, rarity: 2, meaning: -1, adult: -1, maniac: 0, anime: 0, sexy: 0);

		EquipItem(Pocket.Face, 3900, 0x00600058, 0x0093250D, 0x00F482A0);
		EquipItem(Pocket.Hair, 3048, 0x00FFE2AC, 0x00FFE2AC, 0x00FFE2AC);
		EquipItem(Pocket.Armor, 15079, 0x00DD6C66, 0x00450C1D, 0x00FFF2D2);
		EquipItem(Pocket.Shoe, 17007, 0x00200B04, 0x00576D8D, 0x00FFFFFF);

		AddPhrase("Am I missing something...?");
		AddPhrase("Why...");
		AddPhrase("I think I've forgot something...");
		AddPhrase("No way...");
		AddPhrase("What's wrong with them?");
		AddPhrase("But...");
		AddPhrase("...Annoying.");
		AddPhrase("...No...Nothing.");
		AddPhrase("There's no point in talking like that...");
		AddPhrase("Not yet...");
		AddPhrase("It doesn't look good...");
		AddPhrase("That's your problem...");
	}

	protected override async Task Talk()
	{
		SetBgm("NPC_Ailionoa.mp3");

		await Intro(L("She wears a dark dress lined with gorgeous lace<br/>over which her long pale blonde hair flows like a river.<br/>Her dark brown eyes glow in a mysterious way as if to wholly consume anyone who approaches her."));

		Msg("Yes. Go ahead.", Button("Start a Conversation", "@talk"), Button("Shop", "@shop"), Button("Repair Item", "@repair"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());

				if (Player.IsUsingTitle(11001))
				{
					Msg("...If what you're saying is true, <username/>...<br/>you must at least have the strength<br/>to protect one person...");
					Msg("...Only if there are more people like you...<br/>this world wouldn't be such a rough place to live...");
				}
				else if (Player.IsUsingTitle(11002))
				{
					Msg("Erinn's...Guardian?<br/>...You mean, you risked your life<br/>just so you can get those few words<br/>tagged in front of your name...?");
					Msg("......");
					Msg("...If that's how you live your life,<br/>what can I say to that...");
				}

				await Conversation();
				break;

			case "@shop":
				Msg("Is there any particular style you are looking for...?<br/>Feel free to look around and let me know if you see something you like.");
				OpenShop("AilionoaShop");
				return;

			case "@repair":
				Msg("Do you want to repair your clothes?<br/>Let me take a look at it...<repair rate='98' stringid='(*/cloth/*)|(*/glove/*)|(*/bracelet/*)|(*/shoes/*)|(*/headgear/*)|(*/robe/*)|(*/headband/*)' />");

				while (true)
				{
					var repair = await Select();

					if (!repair.StartsWith("@repair"))
						break;

					var result = Repair(repair, 98, "/cloth/", "/glove/", "/bracelet/", "/shoes/", "/headgear/", "/robe/", "/headband/");
					if (!result.HadGold)
					{
						RndMsg(
							"I can't do that...<br/>Please check your wallet again.",
							"Umm...How could you request that without any money to pay for it..."
						);
					}
					else if (result.Points == 1)
					{
						if (result.Fails == 0)
							RndMsg(
								"It is as good as new.",
								"I finished repairing 1 point.",
								"I repaired 1 point."
							);
						else
							Msg("Sorry... I made a little mistake.<br/>I'm sorry."); // Should be 2
					}
					else if (result.Points > 1)
					{
						if (result.Fails == 0)
						{
							RndMsg(
								"I've repaired it flawlessly.",
								"It is as good as new.",
								"That was a piece of cake."
							);
						}
						else
						{
							// TODO: Placeholder dialouge, gather Ailionoa's Dialogue
							Msg(string.Format(L("There, it's done.<br/>But I made {0} mistake(s), unfortunately.<br/>I could restore only {1} point(s)."), result.Fails, result.Successes));
						}
					}
				}

				Msg("Is there anything else I can help you with?<br/>...Please be more careful with your clothes.<repair hide='true'/>");
				break;
		}

		End("Thank you, <npcname/>. I'll see you later!");
	}

	private void Greet()
	{
		if (Player.IsDoingPtjFor(NPC))
		{
			Msg(FavorExpression(), L("Hmm? A part-timer at my shop?<br/>Keep in mind that being indecisive is very unprofessional."));
		}
		else if (Memory <= 0)
		{
			Msg(FavorExpression(), L("Hello. Welcome to Emain Macha's Clothing Shop."));
		}
		else if (Memory == 1)
		{
			Msg(FavorExpression(), L("Hello there. how can I help you?"));
		}
		else if (Memory == 2)
		{
			Msg(FavorExpression(), L("You look familiar...You must be one of my regulars...right?"));
		}
		else if (Memory <= 6)
		{
			Msg(FavorExpression(), L("It's you again. Do you need a consultation regarding clothes?"));
		}
		else
		{
			Msg(FavorExpression(), L("Now I remember.<br/>Let me see... <username/>, was it?"));
		}

		UpdateRelationAfterGreet();
	}

	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				Msg(FavorExpression(), "I run this Clothing Shop.<br/>Please, feel free to take a look around.");
				ModifyRelation(1, 0, 0);
				break;

			case "rumor":
				Msg(FavorExpression(), "Are you asking me about nearby rumors?");
				ModifyRelation(Random(2), 0, Random(3));
				break;

			case "shop_misc":
				Msg("Pardon me? The General Shop?<br/><username/>, didn't you meet Galvin on your way over here?<br/>Yes, the boy that watches the Observatory.<br/>He sells General Shop products.<br/>I think Lucas helps him out too...");
				Msg("...I don't like the fact that Galvin associates himself with that kind of a guy though...");
				break;

			case "shop_grocery":
				Msg("Groceries? Why do you need to buy groceries?<br/>Don't tell me that you haven't been to<br/>Loch Lios, the restaurant by the Lake?");
				Msg("...Yes, they sell ingredients there as well.<br/>But once you try their dishes,<br/>I doubt that you'll ever<br/>want to cook again.");
				break;

			case "shop_healing":
				Msg("If you are talking about the Healer's House,<br/>you must be looking for Agnes.<br/>She is a renowned Healer in this town.");
				Msg("Go to the crossroad in front of here then walk along the Square.<br/>That will lead you to the Healer's House.");
				Msg("...Anyways, a lot of people seem to be asking me for directions to<br/>the Healer's House as of late. I wonder what's going on...");
				break;

			case "shop_bank":
				Msg("Are you looking for a Bank? Do you have business with Jocelin?<br/>To meet Jocelin, walk along the Square at the crossroad you see right over here.<br/>Go all the way around the park.");
				Msg("...I know it's a bit far, but talking to Jocelin will be beneficial<br/>even if it's not something related to banking...");
				break;

			case "skill_rest":
				Msg("My legs are always hurting because I have to stand out here all the time...<br/>Well, I don't think I need a special skill or anything.<br/>...The only reason I am here is<br/>because of my mentor's advice.");
				Msg("...He's told me that in order to make the finest clothes,<br/>you need to observe the way people look when they are wearing your clothes...<br/>Their facial expressions, while in your clothes, tell you everything about the clothes that they are wearing...");
				Msg("...Anyways, I heard he's in Dunbarton now,<br/>but I haven't heard from him at all.<br/>...So cold.");
				break;

			case "skill_instrument":
				Msg("I think a Bard would know better, don't you think...?<br/>Go visit Nele, the Bard, at the Square.<br/>He would be able to give you some useful information...");
				Msg("...Sorry, I'm just not that interested.<br/>Life as a musician...is full of trials... and hunger.<br/>Wouldn't you agree?");
				break;

			case "skill_tailoring":
				Msg("<username/>, you seem to have a lot interest in clothes.<br/>Tailoring clothes requires a lot of attention,<br/>from the design, choosing the right fabric and thread, cutting and tailoring it.");
				Msg("Two of the exact same clothes in the same color<br/>can have a different feel just by being made with a different type of thead...");
				Msg("...Well, not that everyone pays this much attention to the details...<br/>I do have the tendency to go over the top...");
				break;

			case "skill_gathering":
				Msg("Hmm... Can you collect some Cobwebs or some Threads for me?<br/>I would ask you to weave some cloth for me<br/>but I don't think I could expect as much...");
				Msg("If you just bring me some Threads, I'll take it from there.<br/>...Are you offended by my comment?<br/>Then why don't you prove me wrong?<br/>Take a job from me and bring me some quality fabric that I can use.");
				break;

			case "square":
				Msg("If you go south from here, you will be able to get to the Square...<br/>Didn't you pass it on the way here?<br/>I'm surprised... how could you miss something that you just passed by...");
				break;

			case "pool":
				Msg("Just follow the road down south, and you will see the lake.<br/>No, a reservoir is different from a lake.<br/>Reservoirs are artificially created bodies of water, but lakes are not...");
				break;

			case "farmland":
				Msg("If you follow the road north, you will see it.<br/>A Corn Field should be fine, right...?");
				break;

			case "shop_headman":
				Msg("Hmm... You're looking for a town Chief in a city...?<br/>Are you fromt hat town called Tir Chonaill up north or something...?");
				Msg("Well...you certainly don't look like a person from the countryside.<br/>Well, I did think you dressed a little funny...<br/>But I still didn't expect that...");
				break;

			case "temple":
				Msg("The church? Just go straight down south.<br/>...People don't usually go to church,<br/>but I guess you're a little different, <username/>...");
				Msg("...Did you come all the way over here<br/>just to go visit a Church?");
				break;

			case "school":
				Msg("There used to be one...<br/>But it disappeared one day<br/>as the number of students gradually decreased.");
				break;

			case "shop_restaurant":
				Msg("Are you talking about the Restaurant by the lake...?<br/>You must be talking about Loch Lios.<br/>You've come the wrong way then.<br/>There's no lake around here. Why don't you try going in the opposite direction?");
				Msg("And if you get a chance, can you say hello to Chef Gordon for me?");
				break;

			case "shop_armory":
				Msg("Are you looking for a Weapons Shop...?<br/>Hmmm...");
				Msg("Why don't you first go to the Square,<br/>then go toward the western part of town.<br/>It'll be near the edge of the Square.");
				Msg("If you can't find it<br/>then just ask the people hanging around the Square...");
				break;

			case "shop_cloth":
				Msg("Yes. This is the Clothing Store.<br/>I'm not trying to show off, but...<br/>Tre'imhse Cairde is a very well-known place, not only around Emain Macha,<br/>but also throughout all of the Uladh continent.");
				break;

			case "shop_goverment_office":
				Msg("Are you looking for the Town Hall...?<br/>Why don't you visit the Castle instead?<br/>...It is where the Lord of the manor lives.");
				Msg("If not...<br/>Ask Galvin.<br/>I heard that he is doing some business<br/>that's associated with the Castle.");
				break;

			case "graveyard":
				Msg("Are you looking for the Graveyard...?<br/>Let me see...<br/>Take the southern road all the way down,<br/>then make a left when you see another road.");
				Msg("If you get lost,<br/>you can always ask someone that's nearby.");
				break;

			case "lute":
				Msg("Well, it's an instrument. Don't tell me you were planning to buy one here?");
				Msg("...I'm not too sure, so you should just ask someone else.<br/>It'll probably save you some time.");
				break;

			case "complicity":
				Msg("...What do you want me to say?<br/>I have to say, I'm offended.<br/>I thought I knew you, <username/>.");
				break;

			case "tir_na_nog":
				Msg("Tir Na Nog...?<br/>...If there wasn't even a fairy tale like that<br/>there would be no hope for the weak and poor to cling to.<br/>People would lose their desire to live...");
				Msg("...No matter who you are,<br/>everyone has the right to live in this world.");
				break;

			case "mabinogi":
				Msg("...You seem to be interested in old tales.<br/>Those stories are good when you're a child,<br>but getting into it even as you age could become a problem.");
				Msg("...Nele at the Square is a good example.");
				break;

			case "EG_C2_Unknown_Continet":
				Msg("It's good to have a dream.<br/>I have nothing against dreaming.");
				Msg("But you must not forget to put in that much more effort<br/>in order to make your dreams into reality.");
				break;

			case "breast":
				Msg("Ahh... You are a much ruder person than I thought.");
				break;

			default:
				RndFavorMsg(
					"I'm not interested in a story like that.",
					"That is not at all interesting to talk about.<br/>I would love to talk about something else.",
					"I feel like I need to know more about that story.",
					"...I don't know anything about that topic, and I have no interest either.",
					"Are you expecting me to answer that?",
					"I have no clue.",
					"Are you asking me a question?<br/>I'm not too familiar with that...",
					"If you are having a hard time choosing a topic of conversation,<br/>we might as well just end our conversation right now."
				);
				ModifyRelation(0, 0, Random(3));
				break;
		}
	}

	protected override async Task Gift(Item item, GiftReaction reaction)
	{
		switch (reaction)
		{
			case GiftReaction.Love:
				Msg(L("You seem to have a very good sense for choosing the right gift.<br/><username/>, right? Thanks."));
				break;

			case GiftReaction.Like:
				RndMsg(
						L("...Is this for me?<br/>I wasn't expecting it...<br/>Thank you very much."),
						L("I cannot even imagine what has caused you to want to give me a gift like this...<br/>But I will be more than happy to accept this gift.<br/>Thank you."),
						L("I was feeling a little down today, but<br/>this is shining a bright light on my sad day, <username/>."),
						L("You surely know how to impress a lady.<br/>Thank you.")
					);
				break;

			case GiftReaction.Dislike:
				Msg(L("I won't think about why.<br/>Hope this is not just a...<br/>re-gift of something that you'd received from someone else."));
				break;

			default: // GiftReaction.Neutral
				RndMsg(
						L("This is very pretty.<br/>Thanks. I appreciate your kindness."),
						L("Oh.... it is a gift.<br/>I'm not sure if I can take this, but...<br/>Thank you very much."),
						L("I cannot even imagine what has caused you to want to give me a gift like this...<br/>But I will be more than happy to accept this gift.<br/>Thank you."),
						L("I can sense your kind heart in this gift, <username/>.<br/>Thank you, really."),
						L("It is always nice to receive something so nice.<br/>Thank you."),
						L("This must be a gift for me.<br/>...Thanks."),
						L("I truly appreciate your kindness."),
						L("I'm getting lots of gifts lately...<br/>Anyway, thanks very much.")
					);
				break;
		}
	}
}

public class AilionoaShop : NpcShopScript
{
	public override void Setup()
	{
		Add("Robes", 19001); // Robe
		Add("Robes", 19001); // Robe
		Add("Robes", 19002); // Slender Robe
		Add("Robes", 19002); // Slender Robe
		Add("Robes", 19003); // Tri-color Robe
		Add("Robes", 19003); // Tri-color Robe
		Add("Robes", 19003); // Tri-color Robe
		Add("Robes", 19003); // Tri-color Robe
		Add("Robes", 19036); // Cruces Robe

		Add("Hats", 18024); // Hairband
		Add("Hats", 18016); // Hat
		Add("Hats", 18019); // Lirina's Feather Cap
		Add("Hats", 18020); // Mongo's Feather Cap
		Add("Hats", 18026); // Mongo's Merchant Cap
		Add("Hats", 18027); // Lirina's Merchant Cap
		Add("Hats", 18004); // Mongo's Fashion Cap
		Add("Hats", 18007); // Popo Cap
		Add("Hats", 18013); // Cores' Cap
		Add("Hats", 18014); // Mongo's Hat
		Add("Hats", 18014); // Cores' Ribbon Hat
		Add("Hats", 18123); // Kirin's Winter Angel Cap

		// Taiwan Server only items
		//Add("Hats", 18248); // Emerald's Classic Celtic Hat
		//Add("Hats", 18249); // Emerald's Classic Celtic Hairband

		Add("Shoes && Gloves", 17006); // Cloth Shoes
		Add("Shoes && Gloves", 17007); // Leather Shoes
		Add("Shoes && Gloves", 17025); // Sandal
		Add("Shoes && Gloves", 17027); // Long Sandals
		Add("Shoes && Gloves", 17023); // Enamel Shoes
		Add("Shoes && Gloves", 17024); // Open-toe Platform Sandal
		Add("Shoes && Gloves", 17010); // Cores' Boots (M)
		Add("Shoes && Gloves", 16001); // Quilting Gloves
		Add("Shoes && Gloves", 16002); // Linen Gloves
		Add("Shoes && Gloves", 16013); // Swordsman Gloves
		Add("Shoes && Gloves", 16016); // Light Gloves
		Add("Shoes && Gloves", 16020); // Tiger Skin Gloves
		Add("Shoes && Gloves", 17044); // Twin Buckle Boots
		Add("Shoes && Gloves", 17055); // Trudy Moccasin Boots
		Add("Shoes && Gloves", 16025); // Kirin's Winter Angel Gloves
		Add("Shoes && Gloves", 16025); // Kirin's Winter Angel Boots
		Add("Shoes && Gloves", 16030); // Big Band Glove
		Add("Shoes && Gloves", 16033); // Adolph Glove
		Add("Shoes && Gloves", 17069); // Leo Shoes
		Add("Shoes && Gloves", 17070); // Leo Tie Shoes
		Add("Shoes && Gloves", 17098); // Odelia Wizard Boots

		// Japan Server event only items
		//Add("Shoes && Gloves", 17054); // Japanese Sandals
		//Add("Shoes && Gloves", 40206); // Japanese Fan

		// Taiwan Server only items
		//Add("Shoes && Gloves", 17162); // Emerald's Classic Celtic Shoes
		//Add("Shoes && Gloves", 17163); // Emerald's Classic Celtic Boots

		if (IsEnabled("PetBirds"))
			Add("Shoes && Gloves", 16024); // Pet Instructor Glove

		Add("Clothes", 15003); // Vest and Pants Set
		Add("Clothes", 15031); // Magic School Uniform (M)
		Add("Clothes", 15021); // Elementary School Uniform
		Add("Clothes", 15035); // Tork's Hunter Suit (M)
		Add("Clothes", 15044); // Carpenter Clothes
		Add("Clothes", 15022); // Popo's Skirt
		Add("Clothes", 15027); // Mongo's Long Skirt
		Add("Clothes", 15041); // Female Business Suit
		Add("Clothes", 15073); // Martial Arts Suit
		Add("Clothes", 15107); // Mongo's Duffle Coat
		Add("Clothes", 15132); // Natural Vest Wear
		Add("Clothes", 15131); // Pinned Apron Skirt
		Add("Clothes", 15137); // Selina Half Jacket Coat
		Add("Clothes", 15138); // Trudy Hunting Suit
		Add("Clothes", 15262); // Tera Adventurer Wear

		// Japan Server event only items
		//Add("Clothes", 15129); // Yukata (F)
		//Add("Clothes", 15130); // Yukata (M)

		Add("Fine Clothes", 15032); // Lirina's Shorts
		Add("Fine Clothes", 15005); // Adventurer's Suit
		Add("Fine Clothes", 15007); // Traditional Tir Chonaill Costume
		Add("Fine Clothes", 15013); // China Dress
		Add("Fine Clothes", 15025); // Magic School Uniform (F)
		Add("Fine Clothes", 15028); // Cores Thief Suit (F)
		Add("Fine Clothes", 15019); // Cores Ninja Suit (F)
		Add("Fine Clothes", 15026); // Lirina's Long Skirt
		Add("Fine Clothes", 15029); // Tork's Blacksmith Suit
		Add("Fine Clothes", 15042); // High Neck One-piece Dress
		Add("Fine Clothes", 15070); // Stand Collar Sleeveless
		Add("Fine Clothes", 15074); // Lueys' Cleric Coat
		Add("Fine Clothes", 15080); // Ailionoa's Healer Dress
		Add("Fine Clothes", 15078); // Ailionoa's Cute Ruffled Skirt
		Add("Fine Clothes", 15149); // Kirin's Winter Angel Coat (M)
		Add("Fine Clothes", 15150); // Kirin's Winter Angel Coat (F)
		Add("Fine Clothes", 15155); // Edekai's Priest Robe (M)
		Add("Fine Clothes", 15156); // Edekai's Priest Robe (F)
		Add("Fine Clothes", 15259); // Odelia Wizard Suit

		// Taiwan Server only items
		//Add("Fine Clothes", 15419); // Emerald's Classic Celtic Ensemble (M)
		//Add("Fine Clothes", 15420); // Emerald's Classic Celtic Ensemble (F)

		Add("Event"); // Empty
	}
}
