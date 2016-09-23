//--- Aura Script -----------------------------------------------------------
// Gilmore
//--- Description -----------------------------------------------------------
// General Store Worker
//---------------------------------------------------------------------------

public class GilmoreScript : NpcScript
{
	public override void Load()
	{
		SetRace(10002);
		SetName("_gilmore");
		SetBody(height: 0.8f, weight: 0.4f);
		SetFace(skinColor: 17, eyeType: 7, eyeColor: 76, mouthType: 1);
		SetStand("human/male/anim/male_natural_stand_npc_gilmore");
		SetLocation(31, 10383, 10055, 224);
		SetGiftWeights(beauty: 1, individuality: 2, luxury: -1, toughness: 2, utility: 2, rarity: 0, meaning: -1, adult: 2, maniac: -1, anime: 2, sexy: 0);

		EquipItem(Pocket.Face, 4903, 0x00005479, 0x00442064, 0x00240059);
		EquipItem(Pocket.Hair, 4026, 0x00896D43, 0x00896D43, 0x00896D43);
		EquipItem(Pocket.Armor, 15003, 0x00B6CAAA, 0x00584232, 0x00100C0A);
		EquipItem(Pocket.Shoe, 17009, 0x00000000, 0x00A68DC3, 0x0001B24B);
		EquipItem(Pocket.Head, 18028, 0x00000000, 0x00C8C6C4, 0x00DFE9A7);

		AddPhrase("Business is slow nowadays. Perhaps I should raise the rent.");
		AddPhrase("Cheap stuff means cheap quality.");
		AddPhrase("Get lost unless you are going to buy something!");
		AddPhrase("I have plenty of goods. As long as you have the Gold.");
		AddPhrase("If you don't like me, you can buy goods somewhere else.");
		AddPhrase("My goods don't just grow on trees.");
		AddPhrase("So you think you can buy goods somewhere else?");
		AddPhrase("They are so much trouble. Those thieving jerks...");
		AddPhrase("What a pain. More kids just keep coming to the store.");
		AddPhrase("Why should I put up with criticism from people who are not even my customers?");
	}

	protected override async Task Talk()
	{
		SetBgm("NPC_Gilmore.mp3");

		await Intro(L("This wiry man with a slight hump has his hands folded behind his back, and light brown hair hangs over his wide, wrinkled forehead.<br/>The reading glasses, so thick that you can't see what's behind them, rest on the wrinkles of his nose and flash every time he turns his face.<br/>Over his firmly sealed, stubborn-looking lips, he has a light-brown mustache.<br/>Frowning, he tilts down his head and stares at you over his reading glasses with grumpy brown eyes."));

		Msg("What brings you here?", Button("Start Conversation", "@talk"), Button("Shop", "@shop"), Button("Modify Item", "@upgrade"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());

				if (Title == 11001)
				{
					Msg("...");
					Msg("...You're quite a good liar.<br/>I don't know what to say to that.");
				}
				else if (Title == 11002)
				{
					Msg("...Guardian? Ha!");
				}

				await Conversation();
				break;

			case "@shop":
				Msg("I don't know what it is you're looking for,<br/>but you'd better stop now if all you are going to do is look around.");
				OpenShop("GilmoreShop");
				return;

			case "@upgrade":
				Msg("...<br/>Is there something you need to upgrade?<br/>Sigh... Fine, let's see it.<upgrade />");

				while (true)
				{
					var reply = await Select();

					if (!reply.StartsWith("@upgrade:"))
						break;

					var result = Upgrade(reply);
					if (result.Success)
						Msg("...There. All done.<br/>...Again?");
					else
						Msg("(Error)");
				}

				Msg("Is that it? Well then...<upgrade hide='true'/>");
				break;
		}

		End("Thank you, <npcname/>. I'll see you later!");
	}

	private void Greet()
	{
		if (Memory <= 0)
		{
			Msg(FavorExpression(), L("Welcome. I haven't seen you before. I assume you have Gold?<br/>Because, you have no business with me unless you have Gold."));
		}
		else if (Memory == 1)
		{
			Msg(FavorExpression(), L("You are the stingy guy from before. Are you feeling more generous today?"));
		}
		else if (Memory == 2)
		{
			Msg(FavorExpression(), L("I was just trying to remember your name... <username/>..."));
		}
		else if (Memory <= 6)
		{
			Msg(FavorExpression(), L("It's you again. If you are here again just to browse! I don't want you here, <username/>."));
		}
		else
		{
			Msg(FavorExpression(), L("Bah! It's you, <username/>... What's the use of coming so often if you rarely buy anything?"));
		}

		UpdateRelationAfterGreet();
	}

	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				if (Title == 33)
				{
					Msg(FavorExpression(), "Wait, <username/> the Diligent?<br/>Hmm. Not so bad for a youngster.");
					Msg("You got it. You have to live diligently like that to save money. Yep.<br/>Now, don't get complacent!");
				}
				else
				{
					Msg(FavorExpression(), "...<br/>Kids these days have no manners.");
					Msg("Don't you think you should introduce yourself first before you ask the name of your elders?!<br/>What are they teaching the kids at home these days? Hopeless...");
				}
				ModifyRelation(Random(2), 0, Random(3));
				break;

			case "rumor":
				Msg(FavorExpression(), "That Sion... He seems to have taken interest in Bryce's daughter.");
				Msg("Hopeless.");
				Msg("How hopeless.<br/>A girl like Ibbie would never take interest in that poor kid.");
				Msg("Trying to make money off of a water mill...<br/>He's not going to go anywhere in life. Bah!");
				Msg("Like father, like son. No mystery there. Ahem!");
				ModifyRelation(Random(2), 0, Random(3));
				break;

			case "about_arbeit":
				Msg("What is this?<br/>So, you're going to rob this old man, are ya? How rude!");
				break;

			case "shop_misc":
				Msg("What? Are you going to go somewhere else?");
				Msg("You think you'll find what you're looking for elsewhere?");
				break;

			case "shop_grocery":
				Msg("You came to the wrong place. Bah!");
				Msg("Try Jennifer's Pub..");
				break;

			case "shop_healing":
				Msg("This town has no need for a Healer's House.");
				Msg("How are you so weak at such a young age?!<br/>I, in my old age, am still vigorous as ever!");
				break;

			case "shop_bank":
				Msg("A bank, huh? You're looking for Bryce. That jerk?");
				Msg("Ha ha! He should be around here somewhere.<br/>He tries to avoid me, but no dice!");
				break;

			case "shop_smith":
				Msg("Bah! You mean that ol' Edern over there.<br/>That self-assured, miserable jerk.");
				Msg("What? Don't tell me you're asking me for directions?");
				break;

			case "skill_rest":
				Msg("Kids these days are so hopeless.<br/>All they want to do is rest.");
				Msg("If you're going to rest, just rest.<br/>What skill is there in resting, anyway?");
				break;

			case "skill_range":
				Msg("See? Kids these days.<br/>All they want is free stuff.");
				Msg("You are essentially saying that you want to attack without any loss on your part!<br/>It's something only cowards would take interest in.");
				Msg("That attitude of wanting only to gain<br/>without risking any danger is hopeless!");
				break;

			case "skill_instrument":
				Msg("Well, we do carry lutes but...<br/>Why would you want to learn such a useless skill?");
				Msg("Don't tell me you think it will help win<br/>the interest of a lover.");
				Msg("Quit dreaming.<br/>Those things require<br/>Gold and good looks. Bah!");
				break;

			case "skill_composing":
				Msg("What, you think making songs puts food on the table?");
				break;

			case "skill_tailoring":
				Msg("So you know some useful skills, do you?<br/>Bring some clothes you made, and I'll pay you a fair price.");
				break;

			case "skill_magnum_shot":
				Msg("Ohhhh, so you want stronger long range attack, huhhhh?<br/>What kind of a cheating scheme is that?");
				Msg("Bah! Shame on you! Kids these days...<br/>You gain only as much danger as you risk!");
				break;

			case "skill_counter_attack":
				Msg("So, what are your intentions behind asking<br/>me about combat skills?");
				Msg("Kids these days don't have any foresight at all.");
				break;

			case "skill_smash":
				Msg("How would I know about something like that?!");
				break;

			case "skill_gathering":
				Msg("You want work?<br/>Well, if you feel up for it, come by when I recruit part-timers.");
				break;

			case "square":
				Msg("What kind of nonsense is that?");
				break;

			case "pool":
				Msg("Does it look like this town would have something like that?<br/>You really are new here, aren't you?");
				break;

			case "farmland":
				Msg("If you're looking for food, just go to Jennifer's Pub.");
				Msg("...");
				Msg("Hmm... Perhaps I should start selling food too...");
				break;

			case "shop_headman":
				Msg("What, do I not look like the Chief?");
				Msg("Hey! Take your eyes off of the Blacksmith's Shop!");
				break;

			case "temple":
				Msg("Ha! That kid priest?");
				Msg("He came last time asking for an offering. Bah!");
				Msg("That kid tried to take money from me<br/>using the Church as an excuse.<br/>What a joke.");
				break;

			case "school":
				Msg("That Sion kid, he's simply ecstatic now that he doesn't have to go to school.");
				Msg("He has no regard for education but only focuses<br/>on making money to buy gifts for that girl.");
				Msg("Hopeless. So hopeless.");
				break;

			case "skill_campfire":
				Msg("Bah! If you had firewood for something<br/>like that, you should put it towards refining more metal.");
				Msg("Kids these days... All they want to do is play<br/>and don't know what's truly valuable! UGH!");
				break;

			case "shop_restaurant":
				Msg("So you think you can sneak away like that<br/>without buying anything after just browsing, do you?");
				Msg("You have no manners at all!");
				break;

			case "shop_armory":
				Msg("Bah! We don't carry any weapons here!");
				Msg("Move along to the Blacksmith's Shop.");
				break;

			case "shop_cloth":
				Msg("This would be the place for it.<br/>Click 'Shop' after finishing the conversation.");
				Msg("If you are going to complain about prices or items<br/>that aren't available, I don't want to hear it.<br/>My goods don't grow on trees, you know?");
				break;

			case "shop_bookstore":
				Msg("We don't have that in this town.");
				Msg("Hmm... You need books, do you?<br/>There are quite a few people looking for them these days.");
				Msg("Hmm, I might get some to sell if I feel like it.");
				break;

			case "shop_goverment_office":
				Msg("So would you enjoy having<br/>something like that in this town?");
				break;

			case "graveyard":
				Msg("I get it... You want ol' me<br/>buried, do ya?");
				Msg("Bah! How rude of you!");
				break;

			case "bow":
				Msg("If it is bows you are looking for, go see Edern at his Blacksmith's Shop.");
				Msg("What's that look on your face?<br/>In my General Shop, we don't carry such<br/>barbaric items as bows in the first place!<br/>No thanks! Denied!");
				break;

			case "lute":
				Msg("Stop asking and start buying!");
				Msg("What's with all these folks asking about things that are obviously for sale here?!");
				break;

			case "tir_na_nog":
				Msg("You don't actually believe that such a place exists, do you?");
				Msg("It's all a money-making scheme.<br/>You'd better be careful, too. Got it?");
				break;

			case "mabinogi":
				Msg("Hmph. Take that nonsense to some other old folks who are ready to die.");
				break;

			case "musicsheet":
				Msg("Music Scores?<br/>You haven't even clicked the 'Shop' button yet?");
				Msg("What are you doing in a shop then?");
				Msg("Kids these days...");
				break;

			default:
				RndFavorMsg(
					"How should I know about something like that?",
					"Stop asking me and go to the Pub over there.",
					"You won't buy anything and you won't shut up.",
					"And what obligation do I have to answer that question?",
					"Quit bothering me. I don't know anything about such things.",
					"Wait a minute... You're asking me questions without even buying anything? How ridiculous!"
				);
				ModifyRelation(0, 0, Random(3));
				break;
		}
	}
}

public class GilmoreShop : NpcShopScript
{
	public override void Setup()
	{
		Add("General Goods", 1053);       // What We Call 'Iron'
		Add("General Goods", 1124);       // An Easy Guide to Taking Up Residence in a Home
		Add("General Goods", 2001);       // Gold Pouch
		Add("General Goods", 2005);       // Item Bag (7x5)
		Add("General Goods", 2006);       // Big Gold Pouch
		Add("General Goods", 2024);       // Item Bag (7x6)
		Add("General Goods", 2029);       // Item Bag (8x6)
		Add("General Goods", 2038);       // Item Bag (8X10)
		Add("General Goods", 40004);      // Lute
		Add("General Goods", 40017);      // Mandolin
		Add("General Goods", 40017);      // Mandolin
		Add("General Goods", 40018);      // Ukulele
		Add("General Goods", 40216);      // Cymbals
		Add("General Goods", 60045);      // Handicraft Kit
		Add("General Goods", 61001);      // Score Scroll
		Add("General Goods", 61001);      // Score Scroll
		Add("General Goods", 62021, 100); // Six-sided Die x100
		Add("General Goods", 63020);      // Empty Bottle
		Add("General Goods", 64018, 10);  // Paper x10
		Add("General Goods", 64018, 100); // Paper x100

		Add("Blacksmith", 64500, "FORMID:4:20101;", 300);    // Blacksmith Manual - Weeding Hoe
		Add("Blacksmith", 64500, "FORMID:4:20102;", 2000);   // Blacksmith Manual - Dagger
		Add("Blacksmith", 64500, "FORMID:4:20103;", 9000);   // Blacksmith Manual - Round Shield
		Add("Blacksmith", 64500, "FORMID:4:20104;", 1000);   // Blacksmith Manual - Pickaxe
		Add("Blacksmith", 64500, "FORMID:4:20105;", 3000);   // Blacksmith Manual - Longsword
		Add("Blacksmith", 64500, "FORMID:4:20106;", 600);    // Blacksmith Manual - Sickle
		Add("Blacksmith", 64500, "FORMID:4:20107;", 4000);   // Blacksmith Manual - Cuirassier Helm
		Add("Blacksmith", 64500, "FORMID:4:20108;", 5000);   // Blacksmith Manual - Spiked Cap
		Add("Blacksmith", 64500, "FORMID:4:20109;", 10000);  // Blacksmith Manual - Ulna Protector Gloves
		Add("Blacksmith", 64500, "FORMID:4:20110;", 15000);  // Blacksmith Manual - Waterdrop Cap
		Add("Blacksmith", 64500, "FORMID:4:20112;", 35000);  // Blacksmith Manual - Evil Dying Crown
		Add("Blacksmith", 64500, "FORMID:4:20113;", 35000);  // Blacksmith Manual - Dragon Crest
		Add("Blacksmith", 64500, "FORMID:4:20114;", 41000);  // Blacksmith Manual - Iron Mask Headgear
		Add("Blacksmith", 64500, "FORMID:4:20117;", 18000);  // Blacksmith Manual - Greaves
		Add("Blacksmith", 64500, "FORMID:4:20130;", 2000);   // Blacksmith Manual - Blacksmith Hammer
		Add("Blacksmith", 64500, "FORMID:4:20131;", 3000);   // Blacksmith Manual - Short Sword
		Add("Blacksmith", 64500, "FORMID:4:20132;", 10000);  // Blacksmith Manual - Hatchet
		Add("Blacksmith", 64500, "FORMID:4:20134;", 22000);  // Blacksmith Manual - Warhammer
		Add("Blacksmith", 64500, "FORMID:4:20135;", 27000);  // Blacksmith Manual - Broadsword
		Add("Blacksmith", 64500, "FORMID:4:20137;", 42000);  // Blacksmith Manual - Bastard Sword
		Add("Blacksmith", 64500, "FORMID:4:20139;", 48000);  // Blacksmith Manual - Gladius
		Add("Blacksmith", 64500, "FORMID:4:20140;", 66000);  // Blacksmith Manual - Two-handed Sword
		Add("Blacksmith", 64500, "FORMID:4:20141;", 61000);  // Blacksmith Manual - Claymore
		Add("Blacksmith", 64581, "FORMID:4:20119;", 32000);  // Blacksmith Manual - Plate Gauntlets
		Add("Blacksmith", 64581, "FORMID:4:20121;", 42000);  // Blacksmith Manual - Spaika's Silver Plate Boots
		Add("Blacksmith", 64581, "FORMID:4:20124;", 200);    // Blacksmith Manual - Bundle of Arrowheads
		Add("Blacksmith", 64581, "FORMID:4:20125;", 1200);   // Blacksmith Manual - Bundle of Fine Arrowheads
		Add("Blacksmith", 64581, "FORMID:4:20127;", 250);    // Blacksmith Manual - Bundle of Bolt Heads
		Add("Blacksmith", 64581, "FORMID:4:20142;", 3920);   // Blacksmith Manual - Mythril Cable
		Add("Blacksmith", 64581, "FORMID:4:20145;", 1000);   // Blacksmith Manual - Iron Bar
		Add("Blacksmith", 64581, "FORMID:4:20146;", 3000);   // Blacksmith Manual - Copper Bar
		Add("Blacksmith", 64581, "FORMID:4:20147;", 5000);   // Blacksmith Manual - Silver Bar
		Add("Blacksmith", 64581, "FORMID:4:20148;", 7000);   // Blacksmith Manual - Gold Bar
		Add("Blacksmith", 64581, "FORMID:4:20149;", 9000);   // Blacksmith Manual - Mythril Bar
		Add("Blacksmith", 64581, "FORMID:4:20150;", 32000);  // Blacksmith Manual - Light Melka Chainmail
		Add("Blacksmith", 64581, "FORMID:4:20151;", 58000);  // Blacksmith Manual - Pelican Protector
		Add("Blacksmith", 64581, "FORMID:4:20202;", 25000);  // Blacksmith Manual - Battle Short Sword
		Add("Blacksmith", 64581, "FORMID:4:20128;", 1300);   // Blacksmith Manual - Bundle of Fine Bolt Heads
		Add("Blacksmith", 64581, "FORMID:4:20126;", 4600);   // Blacksmith Manual - Bundle of Finest Arrowheads
		Add("Blacksmith", 64581, "FORMID:4:20129;", 5000);   // Blacksmith Manual - Bundle of the Finest Bolt Heads
		Add("Blacksmith", 64581, "FORMID:4:20201;", 45000);  // Blacksmith Manual - Battle Sword

		// This feature is used by the client for the manuals.
		// TODO: Sub-feature?
		if (IsEnabled("TalentRenovationCloseCombat"))
		{
			Add("Blacksmith", 64581, "FORMID:4:20290;", 76500);  // Blacksmith Manual - Celtic War Hammer
			Add("Blacksmith", 64581, "FORMID:4:20291;", 76500);  // Blacksmith Manual - Celtic Warrior Hammer
			Add("Blacksmith", 64581, "FORMID:4:20292;", 76500);  // Blacksmith Manual - Celtic Battle Axe
			Add("Blacksmith", 64581, "FORMID:4:20293;", 76500);  // Blacksmith Manual - Celtic Warrior Axe
			Add("Blacksmith", 64581, "FORMID:4:20294;", 117200); // Blacksmith Manual - Celtic Royal Knight Sword
			Add("Blacksmith", 64581, "FORMID:4:20295;", 117200); // Blacksmith Manual - Celtic Royal War Hammer
			Add("Blacksmith", 64581, "FORMID:4:20296;", 117200); // Blacksmith Manual - Celtic Royal Warrior Hammer
			Add("Blacksmith", 64581, "FORMID:4:20297;", 117200); // Blacksmith Manual - Celtic Royal Battle Axe
			Add("Blacksmith", 64581, "FORMID:4:20298;", 117200); // Blacksmith Manual - Celtic Royal Warrior Axe
		}

		Add("Clothing", 15044); // Carpenter Clothes
		Add("Clothing", 15062); // Zigzag Tunic
		Add("Clothing", 15063); // Layered Frilled Dress
		Add("Clothing", 16029); // Leather Stitched Glove
		Add("Clothing", 16033); // Adolph Glove
		Add("Clothing", 17031); // Outdoor Ankle Boots
		Add("Clothing", 17035); // Four-line Boots
		Add("Clothing", 17068); // Dotted Stitch Boots
		Add("Clothing", 17071); // Knee-high Boots
		Add("Clothing", 18039); // Archer Feather Cap
		Add("Clothing", 18045); // Starry Wizard Hat
		Add("Clothing", 18047); // Cores' Felt Hat

		Add("Gift", 52008); // Anthology
		Add("Gift", 52009); // Cubic Puzzle
		Add("Gift", 52011); // Socks
		Add("Gift", 52017); // Underwear Set
		Add("Gift", 52018); // Hammer

		Add("Event"); // Empty
		if (IsEnabled("PetBirds"))
		{
			Add("General Goods", 16024); // Pet Instructor Glove
			Add("General Goods", 40093); // Pet Instructor Stick
		}

		if (IsEnabled("ItemSeal2"))
		{
			Add("General Goods", 91364, 1);  // Seal Scroll (1-day) x1
			Add("General Goods", 91364, 10); // Seal Scroll (1-day) x10
			Add("General Goods", 91365, 1);  // Seal Scroll (7-day) x1
			Add("General Goods", 91365, 10); // Seal Scroll (7-day) x10
			Add("General Goods", 91366, 1);  // Seal Scroll (30-day) x1
			Add("General Goods", 91366, 10); // Seal Scroll (30-day) x10
		}

		if (IsEnabled("Singing"))
		{
			Add("General Goods", 41124); // Standing Microphone
			Add("General Goods", 41125); // Wireless Microphone
		}

		if (IsEnabled("PropInstruments"))
			Add("General Goods", 41123); // Cello

		if (IsEnabled("Reforges"))
			Add("General Goods", 85571); // Reforging Tool

		if (IsEnabled("TalentRenovationArchery"))
			Add("General Goods", 45130, 10); // Spider Trap
	}
}