//--- Aura Script -----------------------------------------------------------
// Jocelin
//--- Description -----------------------------------------------------------
// Emain Macha Banker
//---------------------------------------------------------------------------

public class JocelinScript : NpcScript
{
	public override void Load()
	{
		SetRace(10001);
		SetName("_jocelin");
		SetFace(skinColor: 17, eyeColor: 47);
		SetLocation(62, 2125, 1871, 121);
		SetGiftWeights(beauty: 1, individuality: 1, luxury: 1, toughness: 1, utility: 2, rarity: 2, meaning: 0, adult: 1, maniac: -1, anime: -1, sexy: 0);

		EquipItem(Pocket.Face, 3900, 0x00DFAF1E, 0x00293A98, 0x002F3294);
		EquipItem(Pocket.Hair, 3021, 0x00491622, 0x00491622, 0x00491622);
		EquipItem(Pocket.Armor, 15041, 0x0075554A, 0x0075554A, 0x0069554F);
		EquipItem(Pocket.Shoe, 17008, 0x00000000, 0x00D3AB64, 0x00F29D38);

		AddPhrase("That's just wrong... How come nobody talked to me about that signboard...?");
		AddPhrase("I can't believe that Sign Board...");
		AddPhrase("Maybe everyone's busy...");
		AddPhrase("Everyone's busy...");
		AddPhrase("I'm waiting...");
		AddPhrase("I cannot let this one get away...");
		AddPhrase("...");
		AddPhrase("So how come there's no report?");
		AddPhrase("What's going on?");
		AddPhrase("We can't waste any time...");
		AddPhrase("Nope. Not yet...");
		AddPhrase("Good. Done.");
		AddPhrase("Maybe I need to double check...");
		AddPhrase("This should be it.");
		AddPhrase("*Sigh*...");
		AddPhrase("Maybe another time.");
	}

	protected override async Task Talk()
	{
		await Intro(L("Her hair is combed all nice and neat, and<br/>you can tell that she is not as young as she looks, but you can't guess her exact age because of her lively, spirited aura.<br/>Her expression seems to be smiling, but when she articulates her words as she speaks it is very obvious she means serious business."));

		Msg("Please feel free to ask any question you wish.", Button("Start a Conversation", "@talk"), Button("Open Account", "@bank"), Button("Redeem Coupon", "@coupon"), Button("Shop", "@shop"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());

				if (Player.IsUsingTitle(11001))
				{
					Msg("Ah, it's you, the one who saved the Goddess <username/>...<br/>I heard from Mr. Bryce about you.<br/>That you are a very cool person...");
					Msg("...Nice to see you.");
				}
				else if (Player.IsUsingTitle(11002))
				{
					Msg("I believe the reason you've become<br/>the Guardian of Erinn is<br/>because you displayed courage and sacrifice fitting for a title such as that.");
					Msg("...You know, I have been secretly observing you. Hehe...");
				}

				await Conversation();
				break;

			case "@bank":
				OpenBank("EmainMachBank");
				return;

			case "@coupon":
				Msg("Would you like to redeem your coupon?<br/>Please enter the number from your coupon.", Input("Redeem Coupon", "Enter Coupon Number"));
				var input = await Select();

				if (input == "@cancel")
					break;

				if (!RedeemCoupon(input))
				{
					Msg("Please double-check your coupon number.<br/>I can't seem to find your number on this list.");
				}
				else
				{
					// Unofficial response.
					Msg("There you go, have a nice day.");
				}
				break;

			case "@shop":
				Msg("Ah, you came for a license for a Personal Shop.<br/>Of course, you need one<br/>in order to open up a personal Shop around here.");
				OpenShop("JocelinShop");
				return;
		}

		End("Thank you, <npcname/>. I'll see you later!");
	}

	private void Greet()
	{
		if (Memory <= 0)
		{
			Msg(FavorExpression(), L("This is the Emain Macha branch of Erskin Bank. How can I help you?"));
		}
		else if (Memory == 1)
		{
			Msg(FavorExpression(), L("Welcome, <username/>. Can I help you with anything?"));
		}
		else if (Memory == 2)
		{
			Msg(FavorExpression(), L("I see you often these days, <username/>.<br/>If you need me for anything, please feel free to drop by."));
		}
		else if (Memory <= 6)
		{
			Msg(FavorExpression(), L("Hello, again. I was just thinking that you'd be here right about now."));
		}
		else
		{
			Msg(FavorExpression(), L("This Bank is flourishing, thanks to your help, <username/>. Hahaha..."));
		}

		UpdateRelationAfterGreet();
	}

	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				Msg(FavorExpression(), "You'd rather talk about your personal matter instead of business...<br/>You are a random person, <username/>. Hahaha...");
				ModifyRelation(Random(2), 0, Random(3));
				break;

			case "rumor":
				Msg(FavorExpression(), "Well! I didn't think you'd need to ask me, of all people, about rumors floating around...");
				ModifyRelation(Random(2), 0, Random(3));
				break;

			case "shop_misc":
				Msg("Are you looking for the General Shop?<br/>Hmm...I think you saw me store items in the account, and assumed there was one.<br/>It happens all the time.");
				Msg("...We don't have a General Shop here,<br/>but if you're looking for items that are usually found in a General Shop,<br/>you should go visit Galvin<br/>at the Observatory over across from the Square...");
				break;

			case "shop_grocery":
				Msg("You are looking for a grocery store, <username/>...?<br/>We don't have a grocery store,<br/>but if you need to buy ingredients,<br/>you might want to go to the lake.");
				Msg("...There's a restaurant.<br/>Loch Lios...<br/>They seem to sell some grocery items<br/>you can buy what you want if they have what you are looking for.");
				break;

			case "shop_healing":
				Msg("A Healer's House?<br/>Oh, are you looking for Agnes? Haha...");
				Msg("...She is a lovely lady.<br/>She reminds me of myself back when I was her age...");
				Msg("...Are you not feeling well?<br/>Because you seem quite healthy, <username/>...");
				break;

			case "shop_bank":
				Msg("Yes, this is the Emain Macha branch of Erskin Bank.<br/>This branch used to be the main branch,<br/>but they moved the main office to Tara...");
				Msg("...If you want me to help you with Bank related issues,<br/>please click on the Bank tab, then ask for assistance.<br/>I'd be more than happy to help you.");
				break;

			case "shop_smith":
				Msg("...A Blacksmith's Shop?<br/>This is a very crowded town,<br/>so a Blacksmith's shop wouldn't be able to survive a day without getting a complaint about its noise around here.");
				Msg("...If you are looking for weapons and armors,<br/>why don't you try the Weapons Shop?");
				break;

			case "skill_rest":
				Msg("Resting is as important as your work.<br/>If you can't rest, you won't have enough energy to work.<br/>But it is a little weird to use a 'Skill' in order to rest.<br/>It doesn't sound quite right. Hahaha...");
				break;

			case "skill_tailoring":
				Msg("You mean the tailoring skill, right?<br/>I know a little about tailoring,<br/>but I think you should ask somebody else.");
				Msg("Have you seen Ail? Yes, Ailionoa.<br/>She is the head designer at Tre'imhse Cairde,<br/>so nobody knows more<br/>about tailoring than she does.");
				break;

			case "skill_gathering":
				Msg("You mean the collecting skill, no?<br/>Yes, our Bank can take care of all of your items you've gathered using the collecting skill,<br/>and we provide a perfect storage environment for your goods.");
				Msg("...You can trust us.");
				break;

			case "square":
				Msg("You mean the Square...?");
				Msg("Hmm, your question is a little surprising.<br/>The Square is just south of this Bank...");
				Msg("Well, I can explain to you the history of our square, but I'm not exactly being paid to be a tour guide here...<br/>Why don't you go visit the square and talk to some of the people around there? Be sure to take a look at our monuments too.");
				Msg("I guarantee you will have a good time there. Haha...");
				break;

			case "pool":
				Msg("A Reservoir?<br/>Maybe you've seen our lake.<br/>That isn't a reservoir, that is a lake.<br/>You can tell if you go a bit close to it.");
				break;

			case "farmland":
				Msg("This town is not exactly known for its farmland...<br/>If you follow the road toward north,<br/>there's a corn field.");
				Msg("...Do you have business around there?");
				break;

			case "windmill":
				Msg("You must be talking about the windmill.<br/>No, there aren't any in this town.");
				break;

			case "shop_headman":
				Msg("A town Chief?<br/>Hmm... Looking for a Chief in a town with a castle...<br/><username/>, where you come from there might be a Chief,<br/>but there's not one here!");
				Msg("We have a Lord instead.<br/>You might want to go visit the castle.");
				break;

			case "temple":
				Msg("Are you looking for the church?<br/>You must have taken a wrong turn. The church is on the other side of town! If you take the road south, you'll see a big bridge.<br/>On the other side of the bridge is the church.");
				Msg("Just go across the bridge and it will be right in front of you.<br/>Why don't you have a conversation with the Priests there?");
				break;

			case "school":
				Msg("There used to be a school, once...<br/>Then the number of the student body began to shrink, and eventually...<br/>disappeared.");
				Msg("...I'm thinking, maybe it was because<br/>the school curriculum didn't address enough real life issues or had little to do with useful learning.");
				break;

			case "shop_restaurant":
				Msg("There's a café over there.<br/>Well...it is more like a restaurant than a café.<br/>By the way... Haven't you heard of Loch Lios?");
				Msg("That's right, it's the name of the restaurant.");
				break;

			case "shop_armory":
				Msg("A Weapons Shop?<br/>If you came from the Square, you might have passed by one...<br/>Osla's Weapons Shop is on the way from the Square to this Bank.");
				Msg("...Sorry... You're going to have to go all the way back. Hahaha...<br/>Osla spent most of her time outside of the Shop, so<br/>take a look around.");
				break;

			case "shop_cloth":
				Msg("Are you talking about Tre'imhse Cairde?<br/>Yes, the best in Emain Macha.<br/>They sell a variety of clothes<br/>so take a look, you might find something you like there.");
				Msg("The head designer Ailionoa is<br/>a very intelligent and detail-oriented designer.<br/>You might have fun spending time with her.");
				break;

			case "shop_bookstore":
				Msg("Hmm... There aren't any bookstores around here!<br/>I heard if you travel East of Emain Macha, there's a small bookstore in Dunbarton.");
				Msg("...I'm sorry... Seems like you really like to read.");
				break;

			case "shop_goverment_office":
				Msg("Town Office...?<br/>I don't know what you are looking for...<br/>Anyways, why don't you go visit the castle<br/>where there is our Lord, it might be better to ask someone who works for the castle.");
				Msg("Ah... If you are looking for your lost items,<br/>Go visit Galvin.<br/>He was assigned from the castle to help adventurers like you.");
				break;

			case "graveyard":
				Msg("You're talking about graveyards...<br/>You have to go to the Square first.<br/>Try going east.");
				Msg("...Do you have someone buried in there?");
				break;

			case "bow":
				Msg("I've seen people storing their bows at the Bank.<br/>Do you mean you want those bows from me...?");
				Msg("...Asking for items that are not yours could be<br/>considered attempted larceny.<br/>Just letting you know.");
				break;

			case "lute":
				Msg("Yes. There are quite a few people who like to keep their lutes at the Bank.<br/>The Bank's storage area is blocking light from Palala<br/>as well as humidity from the environment completely.<br/>No cases of affected musical instruments has been reported.");
				Msg("...Our Bank's storage environment is quite excellent, if I may say so myself.<br/>You can count on us indeed. Hahaha...");
				break;

			case "complicity":
				Msg("It is a pretty common trick among merchants.<br/>Two people acting according to a scheme and stirring up the crowd,<br/>all to increase their sales...");
				Msg("...But, that is an old school method,<br/>and it doesn't really work in the long run.");
				Msg("If you are a real merchant or a business man...");
				Msg("...You need to use your brain, not your head,<br/>don't you think so? Hahaha...");
				break;

			case "tir_na_nog":
				Msg("Yes, Tir Na Nog...<br/>People believe deeply in the stories about this ideal world.");
				Msg("...It gives reason to live on,<br/>especially for people who have little else to hold onto.<br/>This world would otherwise be in a state of chaos.");
				Msg("The whole tale about Tir Na Nog is<br/>more likely a defensive mechanism<br/>that is preventing a perhaps inevitable meltdown.");
				Msg("...Is that too difficult for you to understand? Hahaha...");
				break;

			case "mabinogi":
				Msg("There are many different ways to pass on<br/>knowledge about one's life experiences from one generation to another.<br/>The most effective way of all would be through tales indeed.");
				Msg("...As short stories are aligned against each other in some kind of order,<br/>everything begins to gain significance and start to mean something.<br/>Older generations pass down their life experiences to the following generations in this way.<br/>People who have listed to these stories, added their own experiences and then<br/>passed it on again to the next generation...");
				Msg("There are many people<br/>who overlook the effectiveness of such old tales as an educational tool,<br/>and my guess is that they'd never heard any<br/>bedtime stories when they were young and growing up... Hahaha...");
				Msg("...Am I boring you to death?<br/>Well, anyways, still, old tales are for kids.<br/>If you are an adult and still hold on to these kinds of stories<br/>then perhaps that is a problem, don't you think?");
				break;

			case "EG_C2_Unknown_Continet":
				Msg("I don't know where you heard about this<br/>but I would like to caution that<br/>you really must be careful not to waste precious time<br/>on fairy tale stories.");
				Msg("Even if you're spending your own time on it<br/>you should do so with<br/>the utmost caution.");
				Msg("People forget about one's own preciousness when they are young<br/>and tend to just waste away everything they have...<br/>If I had known this while I was young,<br/>I would have lived a much more reasonable lifestyle.");
				break;

			case "EG_C2_HowTo_Shipboard":
				Msg("Please don't tell me that you are going to board any old ship<br/>just to explore some uncharted, mysterious land?");
				Msg("I hope that you are able to distinguish the difference between recklessness and courage.<br/>I'd advise you to give it some more careful thought.");
				break;

			case "breast":
				Msg("Men really like chests.<br/>I don't mean to complain, but I'm thinking only about my own breasts.");
				break;

			default:
				RndFavorMsg(
					"Sounds intersting,<br/>but let's talk about it some other time.",
					"You don't seem to have any interesting topics, don't you?",
					"Please, stop.<br/>Please be more cautious when you choose a story to talk to someone else.",
					"I'm not interested in that... That's not really a good ice breaker.<br/>Let's talk about something else.",
					"I can't believe you want me to talk with you about that...<br/>Let's talk about something else, shall we?<br/>But honestly, I don't see the importance of that topic.",
					"I would like to talk about something else, <username/>.",
					"That's not the most interesting topic, you know...<br/>Let's talk about something else."
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
					"Thank you. It is a very nice gift.",
					"Haha... I'll gladly accept this.<br/>Where did you get this anyways?",
					"A gift? Thank you very much.",
					"Is this for me?<br/>I'm a little surprised, <username/>... a gift from you.",
					"Thank you for your kindness.<br/>I won't forget your kindness...",
					"Thank you for your gift. It is really nice...",
					"What a gift. It isn't easy to give something to someone else...<br/>Thank you. <username/>."
				);
				break;
		}
	}
}

public class JocelinShop : NpcShopScript
{
	public override void Setup()
	{
		if (IsEnabled("PersonalShop"))
		{
			Add("License", 60104); // Emain Macha Merchant License
			Add("License", 81010); // Purple Personal Shop Brownie Work-For-Hire Contract
			Add("License", 81011); // Pink Personal Shop Brownie Work-For-Hire Contract
			Add("License", 81012); // Green Personal Shop Brownie Work-For-Hire Contract
		}
	}
}
