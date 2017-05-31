//--- Aura Script -----------------------------------------------------------
// Lucas
//--- Description -----------------------------------------------------------
// Club Owner of the Bean Rua in Emain Macha
//---------------------------------------------------------------------------

public class LucasScript : NpcScript
{
	public override void Load()
	{
		SetRace(10002);
		SetName("_lucas");
		SetBody(height: 1.5f, upper: 1.8f, lower: 1.3f);
		SetFace(skinColor: 20, eyeType: 17, eyeColor: 0, mouthType: 4);
		SetStand("human/male/anim/male_natural_stand_npc_Ranald");
		SetLocation(57, 6900, 6090, 165);
		SetGiftWeights(beauty: 0, individuality: 0, luxury: 2, toughness: 1, utility: 1, rarity: 2, meaning: 0, adult: 0, maniac: -1, anime: -1, sexy: 1);

		EquipItem(Pocket.Face, 4904, 0x00A3DAD5, 0x00B6005C, 0x0000B592);
		EquipItem(Pocket.Hair, 4083, 0x0022180F, 0x0022180F, 0x0022180F);
		EquipItem(Pocket.Armor, 15007, 0x00100701, 0x000C0501, 0x006D3C12);
		EquipItem(Pocket.Shoe, 17031, 0x00210F0E, 0x006D0036, 0x004B0007);
		EquipItem(Pocket.RightHand2, 40011, 0x00FFFFFF, 0x000B0909, 0x00FFFFFF);


		AddPhrase("*Cough*... This is what I'm talking about.");
		AddPhrase("Hey! Behave...");
		AddPhrase("If you want something special, let me know. *Chuckle*...");
		AddPhrase("Interesting... I wonder what's going to happen today...");
		AddPhrase("This shop is as good as any of the ones in Tara, if not better.");
		AddPhrase("You smell nice.");
		AddPhrase("We don't deal with any old fogies around here though.");
		AddPhrase("This is your lucky day. This round's on me.");
		AddPhrase("No problem. Have fun!");
		AddPhrase("Drink and be merry!");
		AddPhrase("That is not funny at all. Get lost.");
	}

	protected override async Task Talk()
	{
		await Intro(L("With his dark hair, dark clothes, and very distinctive beard.<br/>he always wears a cynical look on his face.<br/>Truth be told, he looks ten times more dangerous when he smiles.<br/>A tall guy, he regularly looks down at people with his confident set eyes."));

		Msg("Welcome to Bean Rua, the paradise of red-headed ladies.", Button("Start a Conversation", "@talk"), Button("Shop", "@shop"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());

				if (Player.IsUsingTitle(11001))
				{
					Msg("Whoa... I'm sorry I didn't recognize someone as great as you.<br/>Well, there's no coward in the bar, and no hero standing before a dentist.<br/>*chuckle*...!");
				}
				else if (Player.IsUsingTitle(11002))
				{
					Msg("Hm? <username/>, Guardian of Erinn?<br/>Not bad.");
					Msg("...But you know what? You shouldn't<br/>wear that title<br/>in a large crowd.");
					Msg("If you brag about yourself just anywhere...<br/>...You'll find yourself tangled in a unnecessary mess...<br/>...Hehe.");
				}

				await Conversation();
				break;

			case "@shop":
				if (HasItem(73110))
					Msg("Oh, you're a club member!<br/>Do you want me to show you something special?");
				else
					Msg("Order anything you want; drinks, food, anything.");
				OpenShop("LucasShop");
				return;
		}

		End("Thank you, <npcname/>. I'll see you later!");
	}

	private void Greet()
	{
		if (Memory <= 0)
		{
			Msg(FavorExpression(), L("Oh, I don't think I've seen you before. What brings you here?"));
		}
		else if (Memory == 1)
		{
			Msg(FavorExpression(), L("Hello there. Enjoy yourself to your heart's content."));
		}
		else if (Memory == 2)
		{
			Msg(FavorExpression(), L("Welcome. What would you like to order? Some alcohol? Food? Feel free to order anything you want."));
		}
		else if (Memory <= 6)
		{
			Msg(FavorExpression(), L("I'm seeing you a lot these days, <username/>. You like my place?"));
		}
		else
		{
			Msg(FavorExpression(), L("It's you again, <username/>. You seem to come here more often than I do."));
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
					if (!HasItem(73110))
					{
						Msg("Oh, welcome, <username/>.<br/>Okay... I'll give you a useful pointer today.<br/>Do you know why Bean Rua is considered a club?<br/>That's because thhe club operates on membership.<br/>You know that a club equals members, right?");
						Msg("Most people aren't aware of this fact, though. That's because I only give out memberships<br/>to people that take the time to get to know me.");
						Msg("I won't give you all the details, but<br/>it doesn't take a genius to find out that<br/>there are some special privileges that are reserved just for club members.");
						Msg("If you want to become a member, it's actually very simple.<br/>You need to pay 10,000 Gold for your annual membership fee.<br/>However, you know a year can pass by in the blink of an eye,<br/>so I'll accept a 5-year membership fee in advance, a total of 50,000 Gold.");
						Msg("Some people ask me whether it is 5 years in Erinn time or in other time zones...<br/>Of course it is 5 Erinn years. Is there any other time around?<br/>50,000 Gold for a secret club membership... or something like it anyways.");
						Msg("So, what do you say?", Button("Join", "@join"), Button("Too Expensive!", "@exit"));
						switch (await Select())
						{
							case "@join":
								if (Player.Inventory.Gold >= 50000)
								{
									Player.Inventory.Gold -= 50000;
									Send.Notice(Player, ("Received the Bean Rua Brooch from Lucas."));
									Msg("Good thinking. You won't find a better club than mine...");
									GiveItem(73110);
								}
								else
								{
									//unoffical
									Msg("Looks like you don't have enough gold.<br/>Come back when you have enough.");
								}
								break;
							case "@exit":
								Msg("You are the one losing out for not joining the club, anyway.<br/>If you change your mind, then let me know... *chuckle*.");
								break;
						}
					}
					else
					{
						Msg(FavorExpression(), "One day, I'm going to make my club<br/>the biggest in this continent. Even bigger than the one in Tara.<br/>Well, I could theoretically make easy money if I worked for someone else,<br/>but if you are a real man, don't you think you would have to take control of your own destiny?<br/>I'm not interested in kissing up to others to make money...haha.");
						ModifyRelation(Random(2), 0, Random(2));
					}
				}
				else if (Memory >= 15 && Favor >= 30 && Stress <= 5)
				{
					Msg(FavorExpression(), "If we build our friendship, something good will happen to you.<br/>What is it that I'm saying?");
					Msg("You'll know what I mean. Just keep trying... *chuckle*....");
					ModifyRelation(Random(2), 0, Random(2));
				}
				else
				{
					Msg(FavorExpression(), "How do you like this place? I'd like to hear your honest opinion on it.<br/>Well, I think you are smart enough to know what I'd like to hear... *chuckle*.");
					ModifyRelation(Random(2), 0, Random(3));
				}
				break;

			case "rumor":
				if (Memory >= 15 && Favor >= 50 && Stress <= 5)
				{
					Msg(FavorExpression(), "This place used to be famous for grapes<br/>as well as corn.<br/>For some reason,<br/>every grape farm seems to have migrated to the other side of the lake.");
					Msg("That side of the lake is owned by some Druid.<br/>I wonder if Druids actually have a need for private property.<br/>Hehe...");
					ModifyRelation(Random(2), 0, Random(2));
				}
				else if (Memory >= 15 && Favor >= 30 && Stress <= 5)
				{
					Msg(FavorExpression(), "This dungeon, which was closed until recently, apparently has water running inside.<br/>That created a structural damage to the dungeon, which caused<br/>rocks to fall all over the dungeon, in the end, blocking the entrance.");
					Msg("However, as fierce complaints were launched by adventurers and scholars alike,<br/>the dungeon eventually re-opened after some repairs.");
					Msg("I don't understand why people like that dungeon so much<br/>when it is filled with monsters.");
					ModifyRelation(Random(2), 0, Random(2));
				}
				else
				{
					Msg(FavorExpression(), "I don't know what's wrong with the security captain of this town.<br/>Whenever he sees me, he seems like he wants to show who's the boss around here.<br/>That is very rude for an innocent store owner like me... Don't you think so? *chuckle*...");
					ModifyRelation(Random(2), 0, Random(3));
				}
				break;

			case "about_study":
				Msg("You are no here to learn anything from me, right?<br/>It's not like I won't teach you, but if I do, then<br/>you'll need to be determined to learn from me, as well as pay me right.");
				break;

			case "shop_misc":
				Msg("If you are looking for general goods, Galvin is selling some at the Observatory.<br/>Just letting you know, Galvin will be VERY persistent in making you buy something from him,<br/>so just be careful...haha.");
				break;

			case "shop_grocery":
				Msg("Groceries?<br/>Does that look like something you can buy here?<br/>Go somewhere else, and don't make the same mistake anywhere else.");
				break;

			case "shop_healing":
				Msg("The Healer's House? You must be talking about Agnes.<br/>*chuckle* She's a cute one. It would have been nice if she were just a tad more easygoing.<br/>She's a bit stuck up.");
				break;

			case "shop_inn":
				Msg("Who needs an inn after a long day when you can have a drink instead?<br/>I'm telling you, that's the bestway to relax.");
				break;

			case "shop_bank":
				Msg("You must be talking about that picky lady from the bank...<br/>Are you sure you want her to handle your money?<br/>All you'll get for doing that is her constant nagging. *chuckle*...");
				break;

			case "shop_smith":
				Msg("Blacksmith's Shop...? You might as well just scream out loud that you're a county bumpkin!<br/>Go to the Weapons Shop. It's on the west side of the Square.");
				break;

			case "skill_rest":
				Msg("What, you want to rest?<br/>Well, if you want to rest...in peace...<br/>Hahaha, I'm just kidding.");
				break;

			case "skill_range":
				Msg("You are smarter that I'd thought, kid,<br/>for asking a question about that skill.<br/>The dumbest thing you can do is just rush towards the<br/>action without knowing where the enemy is.<br/>Don't you think so?");
				break;

			case "skill_tailoring":
				Msg("Why don't you ask Ailionoa at the Clothing Shop?<br/>Well, everyone can learn,<br/>but not everyone can really do it. *chuckle*...");
				break;

			case "skill_magnum_shot":
				Msg("Huh? Why are you asking me?<br/>Who told you to ask me<br/>I'd like to know who that was.");
				break;

			case "skill_counter_attack":
				Msg("An eye for an eye.<br/>That's one of my favorite phrases.");
				break;

			case "skill_smash":
				Msg("The dumbest thing you can do is just rush towards the<br/>action without knowing where the enemy is.<br/>Don't you think so?");
				break;

			case "skill_windmill":
				Msg("Don't you think you've already made a stupid decision<br/>if and when you find yourself surrounded by multiple enemies at once?<br/>If you ask me,<br/>I wouldn't put myself in that situation at all.");
				break;

			case "skill_gathering":
				Msg("Wherever you go, you'll more than likely find yourself using your hands and feet instead of your head.<br/>The ones that are desperate and without money are the ones that move,<br/>while the employer, the one with money like me, use our heads and know how to relax.<br/>That's the golden rule.");
				break;

			case "square":
				Msg("I pity you, friend.<br/>Are you really bad with directions?");
				Msg("If you really want to find the square, then<br/>you may want to first get out of here.");
				break;

			case "pool":
				Msg("Why would we need that when we already have a lake?");
				break;

			case "shop_headman":
				Msg("You are such a country bumpkin.<br/>Hello! You're in a city now! C.I.T.Y!!");
				break;

			case "temple":
				Msg("A place where underneath the roof of mercy,<br/>freedom is strangled in the name of love,<br/>and justice is nowhere to be found.");
				Msg("Hundreds of years of dust are gone with a sigh<br/>leaving only trails of a sickening scent behind.");
				Msg("That's a passage from \"Following Heroes' Footsteps\",<br/>by Nicholas Chonaill. That excerpt always reminds me of the church for some reason...");
				break;

			case "school":
				Msg("I believe all that learning you do in school is<br/>completely useless in real life.<br/>Nothing teaches you better than<br/>real life experiences which you learn with your body.<br/>Wouldn't you agree?");
				break;

			case "skill_campfire":
				Msg("Some travelers seem to prefer sleeping outside on the ground<br/>over a decent nightclub like mine.<br/>Are they nuts, or what?");
				break;

			case "shop_restaurant":
				Msg("Oh, so you say food here isn't good enough?");
				Msg("Haha, just kidding. Don't be scared.<br/>Well, if you want a really decent meal, visit Gordon's restaurant.<br/>It's out west along the lake.");
				break;

			case "shop_armory":
				Msg("There is such a thing called a 'Map'. Just letting you know.");
				break;

			case "shop_cloth":
				Msg("Well, you need to dress to impress.<br/>Especially you...<br/>Don't you think you should? Haha...");
				break;

			case "shop_bookstore":
				Msg("It seems like this town only attracts big-time book traders.<br/>Well, they weren't needed before because there used to be a school library,<br/>but I don't think they can continue if they keep operating like that.<br/>I don't know what the Lord must be thinking...");
				Msg("Ah, by the way, don't tell anybody what I just said.<br/>You might get in trouble, too.");
				break;

			case "shop_goverment_office":
				Msg("The castle takes care of most of the matters here.<br/>Well, the guy who issued the license was really...um...flexible with regulations.<br/>I had an easy time getting it. *chuckle*...");
				break;

			case "bow":
				Msg("Having a reliable bow is as important as having a reliable sword.<br/>Well, if you don't know how to use it,<br/>then what good is it anyway?");
				break;

			case "lute":
				Msg("I think Nele is selling that.<br/>If he has all those things to sell,<br/>then he might want to change his old lute first...");
				break;

			case "complicity":
				Msg("There are some who say that Galvin is an instigator and such,<br/>but he's just doing his job.<br/>He's a very enthusiastic... salesman...hahaha...");
				break;

			case "tir_na_nog":
				Msg("I was going to name my club that,<br/>but I realized the people at church wouldn't get the joke.<br/>Priest James would have a sense of humor about it,<br/>but the guys above him... *Sigh*...");
				break;

			case "mabinogi":
				Msg("Not interested.<br/>Might as well use a love song, don't you think so?");
				break;

			case "musicsheet":
				Msg("Music scores? Ask Nele.");
				break;

			case "breast":
				Msg("E cup is the best.");
				Msg("You don't know what that is?<br/>What are you, a child? *chuckle*.");
				break;

			case "EG_C2_Unknown_Continet":
				Msg("An uncharted territory?<br/>Ha...I guess you're finally growing up.<br/>You might say that there are lots of<br/>mysterious, undiscovered lands in this world.");
				Msg("Sure seems that way, especially when you're thinking of<br/>beautiful, mysterious beings of the opposite<br/>gender...kkk.");
				break;

			default:
				RndFavorMsg(
					"I'm not sure what you are talking about.",
					"So what?",
					"Why are you asking me that kind of question?",
					"I don't know.",
					"Well, well, well.",
					"Why do you care?"
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
				RndMsg(
					"Thanks for your kindness.",
					"<username/>, are you hitting on me? *chuckle*...",
					"How should I interpret this? What do you want from me?"
				);
				break;

			case GiftReaction.Like:
				RndMsg(
					"Ah, this is troubling... You look even prettier now.",
					"I didn't know you had this in you. Not Bad!"
				);
				break;

			case GiftReaction.Neutral:
				RndMsg(
					"It isn't what I was expecting, but I'll take it.",
					"You came to say hi? That's very nice of you.",
					"Well, I'll take it since you're giving this to me for free."
				);
				break;
		}
	}
}

public class LucasShop : NpcShopScript
{
	public override void Setup()
	{
		Add("Liquor", 50181, "QUAL:4:64;"); // Leighean Gin
		Add("Liquor", 50171, "QUAL:4:65;"); // Emain Macha Wine
		Add("Liquor", 50182, "QUAL:4:66;"); // Brifne Whiskey
		Add("Liquor", 50183, "QUAL:4:65;"); // Devenish Black Beer
		Add("Liquor", 50184); // Ice

		Add("Food", 50109, "QUAL:4:62;"); // Roasted Chicken Wing
		Add("Food", 50107, "QUAL:4:64;"); // Fruit Salad

		Add("Suspicious Item", (creature, npc) => creature.HasItem(73110));
		Add("Suspicious Item", 18089); // Purple Rose Decoration
		if (Random(100) > 50)
			Add("Suspicious Item", 16519); // Glove of Extravaganza
		else
			Add("Suspicious Item", 16518); // Couple Ring Glove
		if ((ErinnTime.Now.Month == ErinnMonth.Imbolic) || (ErinnTime.Now.Month == ErinnMonth.AlbanHeruin))
			Add("Suspicious Item", 40053); // Purple Rose Bouquet
		if (Random(30) >= 20)
		{
			if ((ErinnTime.Now.Month == ErinnMonth.AlbanEiler) || (ErinnTime.Now.Month == ErinnMonth.Samhain))
				Add("Suspicious Item", 40051); // Single Purple Rose
			if (ErinnTime.Now.Month == ErinnMonth.Imbolic)
				Add("Suspicious Item", 40057); // 0 Sign

			//Special Equipment
			if (ErinnTime.Now.Month == ErinnMonth.Imbolic)
				Add("Suspicious Item", 14005, 0x00202020, 0x00202020, 0x00202020, 129800, 1); // Drandos Leather Mail (F)
			else if (ErinnTime.Now.Month == ErinnMonth.AlbanEiler)
				Add("Suspicious Item", 40011, 0x00202020, 0x00202020, 0x00202020, 3500, 4); // Broadsword
			else if (ErinnTime.Now.Month == ErinnMonth.Baltane)
				Add("Suspicious Item", 18521, 0x00202020, 0x00202020, 0x00202020, 43800, 3); // European Comb
			else if (ErinnTime.Now.Month == ErinnMonth.AlbanHeruin)
				Add("Suspicious Item", 13015, 0x00202020, 0x00202020, 0x00202020, 242750, 1); // Brigandine
			else if (ErinnTime.Now.Month == ErinnMonth.Lughnasadh)
				Add("Suspicious Item", 40016, 0x00202020, 0x00202020, 0x00202020, 2900, 2); // Warhammer
			else if (ErinnTime.Now.Month == ErinnMonth.AlbanElved)
				Add("Suspicious Item", 18519, 0x00202020, 0x00202020, 0x00202020, 47850, 3); // Panache Head Protector
			else if (ErinnTime.Now.Month == ErinnMonth.Samhain)
				Add("Suspicious Item", 18515, 0x00202020, 0x00202020, 0x00202020, 47000, 3); // Twin Horn Cap
		}
	}
}