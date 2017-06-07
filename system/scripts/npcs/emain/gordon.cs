//--- Aura Script -----------------------------------------------------------
// Gordon
//--- Description -----------------------------------------------------------
// Head Chef at Emain Macha's Restaraunt, Loch Lios
//---------------------------------------------------------------------------

public class GordonScript : NpcScript
{
	public override void Load()
	{
		SetRace(10002);
		SetName("_gordon");
		SetBody(height: 1.3f, upper: 1.5f, lower: 1.1f);
		SetFace(skinColor: 21, eyeType: 9, eyeColor: 154, mouthType: 9);
		SetStand("human/male/anim/male_natural_stand_npc_Ranald");
		SetLocation(52, 34890, 38980, 10);
		SetGiftWeights(beauty: 1, individuality: -1, luxury: 2, toughness: 1, utility: 1, rarity: 2, meaning: 2, adult: 1, maniac: -1, anime: -1, sexy: 0);

		EquipItem(Pocket.Face, 4903, 0x0067686D, 0x00FCB154, 0x00F8A360);
		EquipItem(Pocket.Hair, 4027, 0x004A2811, 0x004A2811, 0x004A2811);
		EquipItem(Pocket.Armor, 15077, 0x00FAFAFA, 0x00F5A929, 0x007D0217);
		EquipItem(Pocket.Shoe, 17009, 0x0060381E, 0x0082CA9C, 0x00FF7200);
		EquipItem(Pocket.Head, 18055, 0x00FFFFFF, 0x00657999, 0x00717F7B);
		EquipItem(Pocket.RightHand1, 40044, 0x00808080, 0x00FFFFFF, 0x00FFFFFF);

		AddPhrase("Do as I tell you!");
		AddPhrase("Cooking is an art! And art is spirit! Spirit is dedication and soul!");
		AddPhrase("Tsk Tsk...");
		AddPhrase("The customer is always right!");
		AddPhrase("You are hopeless...");
		AddPhrase("Measuring is everything!");
		AddPhrase("You moron! You call yourself a Chef?");
		AddPhrase("Fraser, you moron! What are you thinking!");
		AddPhrase("Do you know how to cook the kind of unforgettable dishes that impress the customers?");
	}

	protected override async Task Talk()
	{
		SetBgm("NPC_Gordon.mp3");

		await Intro(L("His thick eyebrows stand out amongst his bold facial features.<br/>His brow is always furrowed and his loud voice echoes throughout the restaurant.<br/>He cooks with grace and dexterity despite his rather large frame."));

		Msg("Welcome! What are you looking for?", Button("Start a Conversation", "@talk"), Button("Shop", "@shop"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());

				if (Player.IsUsingTitle(11001))
				{
					Msg("You rescued the Goddess? You wish!<br/>Well, there's nothing wrong with dreaming, though. Haha!");
				}
				else if (Player.IsUsingTitle(11002))
				{
					Msg("...Eh? <username/>, you're Erinn's Guardian?<br/>You mean... You prevented the threat of war<br/>with your own strength...?");
					Msg("...Wow, I should be grateful.<br/>But I'll tell you this...<br/>It's not over just yet...");
					Msg("I've fought before too...<br/>...And one thing I know,<br/>those Fomors, they are quite relentless...");
				}

				await Conversation();
				break;

			case "@shop":
				if (Memory >= 15 && Favor >= 50 && Stress <= 5)
					Msg("<username/>! Why don't you try this?");
				else
					Msg("Tell me anything you'd like to eat! I'll serve up heaven itself on your plate");
				OpenShop("GordonShop");
				return;
		}

		End("Thank you, <npcname/>. I'll see you later!");
	}

	private void Greet()
	{
		if (Memory <= 0)
		{
			Msg(FavorExpression(), L("My name is Gordon, the head Chef of this restaurant!"));
		}
		else if (Memory == 1)
		{
			Msg(FavorExpression(), L("Weren't you here before? Well, order anything you want!"));
		}
		else if (Memory == 2)
		{
			Msg(FavorExpression(), L("Oh it's you again <username/>! You're going to be happy with what I'm cooking for you today!"));
		}
		else if (Memory <= 6)
		{
			Msg(FavorExpression(), L("Seems that I am seeing you quite a lot these days, although you are not the only one I'm see recurrently, though. Hahaha!"));
		}
		else
		{
			Msg(FavorExpression(), L("Seems that I am seeing you quite a lot these days, although you are not the only one I'm see recurrently, though. Hahaha!"));
		}

		UpdateRelationAfterGreet();
	}

	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				Msg(FavorExpression(),"People say I'm obsessed with cooking.<br/>But, if you're not obsessed with somthing,<br/>you can't really achieve anything!");
				ModifyRelation(Random(2), 0, Random(3));
				break;

			case "rumor":
				Msg(FavorExpression(),"Our Lord is such a connoisseur of food that<br/>his Chef frequently asks me to assist him with the Lord's menu.<br/>No offence to him, but he'll need to really work on refining his skills.");
				ModifyRelation(Random(2), 0, Random(3));
				break;

			case "about_skill":
				Msg("I'm not interested in having more apprentices. Fraser's enough.");
				break;

			case "about_arbeit":
				Msg("If you want a part-time job, talk to Fraser.");
				break;

			case "shop_misc":
				Msg("Talk to Galvin.");
				break;

			case "shop_grocery":
				Msg("What's wrong with you?<br/>Why are you looking for groceries at a restaurant?<br/>Talk to Shena. She might have some.");
				break;

			case "shop_healing":
				Msg("Ask Shena.");
				break;

			case "shop_inn":
				Msg("Ask Shena...");
				break;

			case "shop_bank":
				Msg("You can find the bank in front of the gate to the castle.");
				break;

			case "shop_smith":
				Msg("As far as I know, there's no Blacksmith's Shop at Emain Macha.<br/>We do have a Weapons Shop, though.");
				break;

			case "skill_range":
				Msg("From time to time, I've seen people throwing pies and cakes<br/>during festivals. If I see them again, I'm going to smack<br/>each and every one of them silly.");
				Msg("There's nothing more disrespectful than playing around with food!<br/>Nothing!");
				break;

			case "skill_instrument":
				Msg("Go to Nele by the fountain at the Town Square.<br/>He's surprising good at it...");
				break;

			case "skill_tailoring":
				Msg("Ask Ailionoa.<br/>She's quite talented.");
				break;

			case "skill_magnum_shot":
				Msg("Ask Aodhan.");
				break;

			case "skill_smash":
				Msg("Cooking requires the strength of a real man.<br/>That's the similarity I see between cooking and that!");
				break;

			case "skill_windmill":
				Msg("Windmill?<br/>Are you talking about the combat skill?");
				break;

			case "skill_gathering":
				Msg("I truly believe that collecting ingredients is the starting point for all cooking!<br/>If you want to eat something<br/>learn to collect it first!");
				break;

			case "square":
				Msg("The Square? It's right in front of you!<br/>...<br/>...<br/>Um, do you have something else to say?");
				break;

			case "farmland":
				Msg("You wouldn't dare throw away a single grain of rice<br/>if you thought about all the hard work farmers put into harvesting that one grain of rice!<br/>Considering that, even the thought of ruining farmland is nothing less than a complete outrage!");
				break;

			case "shop_headman":
				Msg("What town Chief? This is a city.");
				break;

			case "temple":
				Msg("The church is located in the inner parts of the lake.");
				break;

			case "school":
				Msg("It's not there anymore.");
				break;

			case "skill_campfire":
				Msg("It's much better to have a loom or a stove for cooking,<br/>but since adventurers can't carry those around, they'll need to use the campfire.<br/>An important part of cooking is about<br/>efficiently utilizing the various tools around you.");
				break;

			case "shop_restaurant":
				Msg("Hungry?<br/>Then order something!");
				break;

			case "shop_armory":
				Msg("Osla's Weapons Shop? Ask Fraser.<br/>That fool doesn't pay any attention to his work,<br/>and spends all his time mastering the art of seducing girls... *Sigh*");
				break;

			case "shop_cloth":
				Msg("The Clothing Shop is located next to the Auditorium.<br/>The lady there seems to be starving herself.<br/>I can't believe how someone so thin can work so hard!");
				break;

			case "shop_goverment_office":
				Msg("The Lord's castle?<br/>It's not the same as the town office, but that's where the legislations of Emain Macha are taken care of.");
				Msg("You want to know about work regarding adventurers?<br/>Hmm... I heard that Galvin guy recently earned a permit on something...<br/>Why don't you ask him?");
				break;

			case "graveyard":
				Msg("Many friends and families are buried there.<br/>No matter which grave you visit, don't forget to pay your respects.");
				break;

			case "lute":
				Msg("Please ask Nele.");
				break;

			case "complicity":
				Msg("What are you talking about?<br/>What kind of a person would do that?");
				break;

			case "tir_na_nog":
				Msg("I always say it's better to aim for realistic goals<br/>rather than to unrealistically shoot for the moon.");
				break;

			case "Cooker_qualification_test":
				Msg("Are you asking me that, knowing that I am a judge in the competition?<br/>I certainly hope that your intentions in befriending me was not to lobby for some kind of an advantage.");
				break;

			case "breast":
				Msg("What do you mean by that?");
				break;

			case "errand_of_fleta":
				if (!HasItem(70077))
				{
					Send.Notice(Player, ("Received Peppermint from Gordon."));
					Msg("A spice that Imps like?<br/>Well, there is a spice that is known as that.");
					Msg("It's a spice called peppermint<br/>which is rare to find. It is known as that<br/>because it's found in the forest and has a clear and cool aroma to it.");
					Msg("I have some at the shop<br/>so if you need it, I guess I can give you some... Do you want it?");
					Msg("Here you go.<br/>It's not much so use it wisely.");
					GiveItem(70077);
				}
				else
					Msg("Mm? You already have some peppermint!<br/>Then I guess I don't need to give you some.");
				break;

			case "g3_DarkKnight":
				Msg("Dark Knight are the Fomor knights<br/>who cover themselves with black armor.<br/>You can even say...<br/>They are the antithesis of the Knight of Light...");
				Msg("...I pray that there won't be<br/>another invasion by Fomors led by Dark Knights again.<br/>Although, I'm not too worried<br/>since you're around...");
				break;

			case "EG_C2_HowTo_Shipboard":
				Msg("I don't think there are any harbors around here.");
				Msg("Oh, wait just a minute.<br/>Actually, I do remember there being a small<br/>harbor on the South end of Bangor...");
				Msg("It was quite a long time ago, though, so I don't know what it's like downt here now but...<br/>If you don't know of anywhere else, you might as well go down there and find out?");
				break;

			default:
				RndFavorMsg(
					"I don't know.",
					"What is that?",
					"It's none of my business.",
					"Nope, I know nothing about that."
				);
				ModifyRelation(0, 0, Random(3));
				break;
		}
	}

	protected override async Task Gift(Item item, GiftReaction reaction)
	{
		switch (reaction)
		{
			case GiftReaction.Love: //placeholder missing love messages
			case GiftReaction.Like:
				RndMsg(
					"Well, today is a good day.",
					"I'll gladly accept this.",
					"What's the occasion?"
				);
				break;		

			case GiftReaction.Dislike:
				RndMsg(
					"A gift? This wasn't necessary.",
					"Why did you bring something like this to me?",
					"What is this?"
				);
				break;
				
			default: // GiftReaction.Neutral
				RndMsg(
					"Thanks.",
					"A gift? This wasn't necessary.",
					"I don't know if I need this for the resaurant, but thanks."
				);
				break;
		}
	}
}

public class GordonShop : NpcShopScript
{
	public override void Setup()
	{
		Add("Cooking", 50164, "QUAL:4:85;"); // T-Bone Steak
		Add("Cooking", 50165, "QUAL:4:86;"); // Cheese Gratin
		Add("Cooking", 50166, "QUAL:4:87;"); // Curry and Rice
		Add("Cooking", 50120, "QUAL:4:88;"); // Steamed Rice
		Add("Cooking", 50167, "QUAL:4:87;"); // Coq Au Vin
		Add("Cooking", 50162, "QUAL:4:89;"); // Taitinn Carp Stew
		Add("Cooking", 50168, "QUAL:4:86;"); // Steamed Trout
		Add("Cooking", 50169, "QUAL:4:87;"); // Bouillabaisse
		Add("Cooking", 50101, "QUAL:4:88;"); // Potato & Egg Salad
		Add("Cooking", 50170, "QUAL:4:86;"); // Cheese Fondue

		Add("Ingredient", 50171); // Emain Macha Wine

		Add("Books", (creature, npc) => npc.GetFavor(creature) >= 50);
		Add("Books", 1064); // Master Chef's Cooking Class: Baking
		Add("Books", 1065); // Master Chef's Cooking Class: Simmering
		Add("Books", 1066); // About Kneading
		Add("Books", 1067); // Boiling: The Basics of Heat Cooking
		Add("Books", 1068); // About Noodle-Making
		Add("Books", 1069); // Master Chef's Cooking Class: Frying
		Add("Books", 1070); // Master Chef's Cooking Class: Stir-frying

	}
}