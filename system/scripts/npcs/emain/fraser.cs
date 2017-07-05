//--- Aura Script -----------------------------------------------------------
// Fraser
//--- Description -----------------------------------------------------------
// Chef at Emain Macha's Restaraunt, Loch Lios.
//---------------------------------------------------------------------------

public class FraserScript : NpcScript
{
	public override void Load()
	{
		SetRace(10002);
		SetName("_fraser");
		SetBody(height: 1.2f, upper: 1.3f);
		SetFace(skinColor: 17, eyeType: 3, eyeColor: 39, mouthType: 4);
		SetStand("human/male/anim/male_natural_stand_npc_Piaras");
		SetLocation(52, 35257, 39473, 232);
		SetGiftWeights(beauty: 2, individuality: 1, luxury: 1, toughness: 1, utility: 2, rarity: 2, meaning: 0, adult: 1, maniac: -1, anime: 2, sexy: 1);

		EquipItem(Pocket.Face, 4900, 0x0042B5B6, 0x0066C076, 0x00862C04);
		EquipItem(Pocket.Hair, 4032, 0x00DBB0B0, 0x00DBB0B0, 0x00DBB0B0);
		EquipItem(Pocket.Armor, 15077, 0x00FFFFFF, 0x00F5A929, 0x00F5A929);
		EquipItem(Pocket.Shoe, 17009, 0x00352411, 0x00696969, 0x00E17662);
		EquipItem(Pocket.Head, 18053, 0x00FFFFFF, 0x00824F58, 0x00745339);

		AddPhrase("I gotta make sure I keep track of the time on the oven.");
		AddPhrase("Ha! I'm a genius!");
		AddPhrase("This should be good enough for the whip cream...");
		AddPhrase("Ah, my legs are sore.");
		AddPhrase("Whats the next step?");
		AddPhrase("Mmm, smells good.");
		AddPhrase("Let's eat first, I'm hungry.");
	}

	protected override async Task Talk()
	{
		SetBgm("NPC_Fraser.mp3");

		await Intro(L("With spiky hair sticking out of his white Chef's hat,<br/>his eyes are full of curiosity.<br/>Though he doesn't have the confidence of a master Chef,<br/>he does show talent beyond his years."));

		Msg("Hello, there!", Button("Start a Conversation", "@talk"), Button("Shop", "@shop"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());

				if (Player.IsUsingTitle(11001))
				{
					Msg("You rescued the Goddess? Wow! You're awesome!");
				}
				else if (Player.IsUsingTitle(11002))
				{
					Msg("Whaa? Guardian of Erinn? You?");
					Msg("Haha... I sure wasn't expecting that...");
					Msg("No, no, I'm just saying,<br/>I didn't know you<br/>would do something that incredible.");
					Msg("...You definitely surprised me,<br/>which makes you truly remarkable in many ways.");
				}

				await Conversation();
				break;

			case "@shop":
				Msg("Choose whichever one you like, whatever looks good to you.");
				OpenShop("FraserShop");
				return;
		}

		End("Thank you, <npcname/>. I'll see you later!");
	}

	private void Greet()
	{
		if (Memory <= 0)
		{
			Msg(FavorExpression(), L("I am an Apprentice Chef...<p/>Ugh, sounds so boring.<br/>Welcome! I'm Fraser. I'm learning how to cook here."));
		}
		else if (Memory == 1)
		{
			Msg(FavorExpression(), L("We've met before, right? Are you looking for something?"));
		}
		else if (Memory == 2)
		{
			Msg(FavorExpression(), L("Hey, <username/>. Did you eat yet?"));
		}
		else if (Memory <= 6)
		{
			Msg(FavorExpression(), L("Hey, it's you, <username/>. Do you want me to tell Shena to open up a table for you?"));
		}
		else
		{
			Msg(FavorExpression(), L("Hey there, my dead loyal customer. What can I do for you today?"));
		}

		UpdateRelationAfterGreet();
	}

	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				Msg(FavorExpression(), "Me? I'm the second best Chef in Emain Macha!<br/>Although I'm still learning.");
				ModifyRelation(Random(2), 0, Random(3));
				break;

			case "rumor":
				Msg(FavorExpression(), "Have you met my boss, the Chef of this Restaurant?<br/>He's been cooking for 40 years...<br/>How old was he when he started then?");
				ModifyRelation(Random(2), 0, Random(3));
				break;

			case "shop_misc":
				Msg("Well, it's not exactly a General Shop, but go see Galvin.<br/>He's always in front of the Observatory, trying to attract customers<br/>so you can't miss him.");
				break;

			case "shop_grocery":
				Msg("Grocery Store? Not a Restaurant?<br/>If you're looking for a Restaurant, this is it.");
				break;

			case "shop_healing":
				Msg("Oh, Agnes?<br/>She kind of seems out of it at times, but she's still pretty.<br/>Follow this road in front of you, then head north.<br/>You'll find the Healer's House there.");
				break;

			case "shop_inn":
				Msg("I'm not too sure. Travelers visit the Club a lot,<br/>but I don't know if you would call that an Inn.");
				break;

			case "shop_bank":
				Msg("It is located directly across from the Castle.<br/>Just go up from here<br/>then follow any alley north and it'll be on your side.");
				break;

			case "shop_smith":
				Msg("A Blacksmith's Shop? You mean the Weapons Shop, no?<br/>Follow the ailse right up front and go up.<br/>You'll find Osla's Weapons Shop.");
				break;

			case "skill_rest":
				Msg("I want that for myself.");
				break;

			case "skill_range":
				Msg("I learned it briefly when I was young,<br/>but man, it's not that easy.");
				break;

			case "skill_instrument":
				Msg("Performing an instrument? Go ask Nele at the Square.<br/>He's the best in town.");
				break;

			case "skill_tailoring":
				Msg("You should ask about that at the Clothing Shop.<br/>I mean, really, how in the world would I know?");
				break;

			case "skill_magnum_shot":
				Msg("Ask Aodhan about that.<br/>He's the guy in front of the Castle wearing the shiny armor.");
				break;

			case "skill_counter_attack":
				Msg("Do you think if I learned that,<br/>I'd be able to use it on Gordon?");
				Msg("......");
				Msg("Forget it. If I tried, I'll probably get hit upside the head.");
				break;

			case "skill_smash":
				Msg("My boss is definitely a master of that skill...");
				break;

			case "skill_windmill":
				Msg("Is that a skill to make flour?");
				break;

			case "skill_gathering":
				Msg("Shoot!<br/>I'm sooo dead!<br/>I forgot he told me to get some milk!");
				break;

			case "square":
				Msg("Do you know where the large water fountain is right up front?<br/>That's the Square.");
				break;

			case "pool":
				Msg("A reservoir? You mean the lake?<br/>You can see it from right here. Isn't it beautiful?");
				break;

			case "shop_headman":
				Msg("There is no Chief in the city.");
				break;

			case "temple":
				Msg("It's on the inner part of the lake.<br/>Priest James is a very interesting man,<br/>but the Bishop is a little dull....<br/>Haha, don't tell him I said that, though!");
				break;

			case "school":
				Msg("Ah, my dad used to be a teacher there long ago.<br/>But after the School closed down, he switched over to gardening.");
				Msg("Why did it close down? I'm not sure...");
				break;

			case "skill_campfire":
				Msg("Aodhan seems like he's getting stressed out<br/>by the people that are starting campfires within the perimeters of the city.<br/>But I don't think there's anything to really worry about.<br/>I mean, we have a lake right next to us.");
				break;

			case "shop_restaurant":
				Msg("Hello? Is there something wrong with your eyes?<br/>You are standing in the middle of a Restaurant right now!");
				break;

			case "shop_armory":
				Msg("You mean Osla?<br/>Haha, she's probably dozing off right now.");
				break;

			case "shop_cloth":
				Msg("You must be talking about 'Tre'imhse Cairde'.<br/>Ailionoa does make excellent cloths,<br/>but she makes them too pricey.");
				Msg("If you don't have the moeny, you might as well forget about it.");
				break;

			case "shop_bookstore":
				Msg("The Bookstore? You mean Buchanan?<br/>He is probably at Tara right now.<br/>He was complaining a lot recently<br/>saying he received an order for over a few hundred books.");
				break;

			case "shop_goverment_office":
				Msg("The Castle is a bit too strict.<br/>Why don't you ask Aodhan or Galvin?<br/>They probably know more about jobs for adventurers.");
				break;

			case "skill_fishing":
				Msg("I love fishing,<br/>but I can never find the time to go.<br/>It's been such a long time since I last went.");
				break;

			case "bow":
				Msg("Ask Osla about bows.<br/>Oh, Lucas seems to have a lot of interest in bows as well.<br/>I wonder if he collects them.");
				break;

			case "complicity":
				Msg("An instigator?<br/>We don't need someone like that for our Restaurant.<br/>If my boss heard that, he would break your leg!");
				break;

			case "mabinogi":
				Msg("Ask Nele. You can see him at the Square.");
				break;

			case "musicsheet":
				Msg("A musical score? Ask Nele at the Square.<br/>He seems to have a bunch of used ones and blank ones as well.");
				break;

			case "nao_friend":
				Msg("Who's Nao?<br/>Sounds like a name that would belong to a really pretty girl!");
				break;

			case "nao_owl":
				Msg("Is it different from the owl that delivers letters?");
				break;

			case "nao_blacksuit":
				Msg("I looove women in black! Hehe.");
				break;

			case "breast":
				Msg("Huh?<br/>Being asked about something like that is embarrassing.");
				break;

			case "EG_C2_Unknown_Continet":
				Msg("If there was a place like that, I sure would love to go.<br/>The only thing is though... that perhaps it would be dangerous<br/>to go on vacation to a place like that? Hahaha.");
				break;

			case "EG_C2_HowTo_Shipboard":
				Msg("A ship? I wonder?<br/>Perhaps my master might know?");
				Msg("I've never left Emain Macha before<br/>so I really have no idea! Hahaha.");
				break;

			default:
				RndFavorMsg(
					"Well... I don't know!",
					"Ahh, you always ask me things I don't know.",
					"Ah, stop exposing my weaknesses.",
					"Hey, can we talk about something else?",
					"I don't really know anything about that..."
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
					"I don't know what's in here, but a gift is still a gift. Thanks!",
					"What is this? It's not anything weird is it?",
					"I'll get in so much trouble by my boss if he found out that I'd accepted this!",
					"A gift? Thanks!",
					"I can't turn down a gift, though. Haha, thanks!"
				);
				break;
		}
	}
}

public class FraserShop : NpcShopScript
{
	public override void Setup()
	{
		Add("Cooking", 50172, "QUAL:4:73;"); // Rare Cheesecake
		Add("Cooking", 50173, "QUAL:4:74;"); // Gateau Au Chocolat
		Add("Cooking", 50174, "QUAL:4:75;"); // Croque Monsieur
		Add("Cooking", 50175, "QUAL:4:74;"); // Brownie
		Add("Cooking", 50176, "QUAL:4:73;"); // Butter Biscuit
		Add("Cooking", 50177, "QUAL:4:74;"); // Chocolate Chip Cookie
		Add("Cooking", 50178, "QUAL:4:75;"); // Orange Juice
		Add("Cooking", 50179, "QUAL:4:74;"); // Lemonade
		Add("Cooking", 50113, "QUAL:4:75;"); // Strawberry Milk
		Add("Cooking", 50180, "QUAL:4:72;"); // Chocolate Milk

		Add("Event"); // Empty
	}
}
