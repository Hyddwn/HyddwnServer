//--- Aura Script -----------------------------------------------------------
// Galvin
//--- Description -----------------------------------------------------------
// Observatory Owner, runs a General Shop and manages the lost and found
//---------------------------------------------------------------------------

public class GalvinScript : NpcScript
{
	public override void Load()
	{
		SetRace(10002);
		SetName("_galvin");
		SetBody(height: 1.1f, upper: 1.1f);
		SetFace(skinColor: 25, eyeType: 6, eyeColor: 29);
		SetStand("human/male/anim/male_natural_stand_npc_Piaras");
		SetLocation(52, 43924, 37003, 61);
		SetGiftWeights(beauty: 2, individuality: 1, luxury: 0, toughness: 1, utility: 0, rarity: 2, meaning: 0, adult: 0, maniac: 1, anime: -1, sexy: 2);

		EquipItem(Pocket.Face, 4900, 0x00FABB43, 0x00F49C37, 0x00771B1B);
		EquipItem(Pocket.Hair, 4036, 0x00F2BB7D, 0x00F2BB7D, 0x00F2BB7D);
		EquipItem(Pocket.Armor, 15070, 0x003E3D22, 0x0016322C, 0x00F5D29E);
		EquipItem(Pocket.Shoe, 17044, 0x00000000, 0x004C1458, 0x00AF760B);
		EquipItem(Pocket.Head, 18047, 0x007C2703, 0x00E7A95F, 0x008B5340);


		AddPhrase("Hey there, cool guy!");
		AddPhrase("Welcome to the world famous Observatory!");
		AddPhrase("Hey there, handsome guy!");
		AddPhrase("Hee! Ha! Hee!");
		AddPhrase("Hey there, pretty lady!");
		AddPhrase("Hey there, beautiful!");
		AddPhrase("Hahahaha!");
		AddPhrase("What's the price you're looking for?");
		AddPhrase("Hello, sir! Right here!");
		AddPhrase("Nooo! I don't think you know what you're talking about, Sir...");
		AddPhrase("Hello, there!");
		AddPhrase("Great! Have a great day!");
		AddPhrase("How are you doing, miss?");
		AddPhrase("Hey there, fly guy!");
		AddPhrase("Lalala. Lalala.");
		AddPhrase("Business is pumping today!");
		AddPhrase("What's up girl! Check this out!");
		AddPhrase("Haha!");
		AddPhrase("Yo bro! Over here!");
		AddPhrase("(*Scratch*)");
		AddPhrase("Here you are again!");
		AddPhrase("How can I help you?");
		AddPhrase("Hey! How you doing there?");
	}

	protected override async Task Talk()
	{
		SetBgm("NPC_Galvin.mp3");

		await Intro(L("Here stands a young man with dirty brown hair, almost completely hidden under an even dirtier hat.<br/>Constantly leaning over to his right, he attempts to flag down passersby to his store.<br/>His sleeveless jacket is decorated in embroidered metalic patterns. His dark blue eyes wandering around while he talks, always<br/>seeking more customers.<br/>He keeps mumbling something to himself, but it is rather difficult to make out what he's saying as he seems to be talking a<br/>mile a minute."));

		Msg("What do you need? I can get you anything!", Button("End Conversation", "@cancel"), Button("Start a Conversation", "@talk"), Button("Shop", "@shop"), Button("Repair Item", "@repair"), Button("Retrieve lost item", "@lostandfound"), Button("Upgrade Item", "@upgrade"));

		switch (await Select())
		{
			case "@cancel":
				break;

			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());

				if (Player.IsUsingTitle(11001))
				{
					Msg("<username/>, the one who saved the Goddess!<br/>You are my VIP!<br/>How can I help you!<br/>Tell me anything! Haha!");
				}
				if (Player.IsUsingTitle(11002))
				{
					Msg("Ooh, it's you, <username/>!!<br/>The Guardian of Erinn is<br/>always welcomed here! Welcome! Haha!");
					Msg("...On that note, I will give you<br/>a special discount on all my items...");
					Msg("And actually sell them to you at a reasonable price!<br/>Hahaha!");
				}

				await Conversation();
				break;

			case "@shop":
				Msg("Oh! Are you looking for something?<br/>Seriously, I have everything here! Hahahaha!<br/>I'll bet that you'll find just what you're looking for!");
				OpenShop("GalvinShop");
				return;

			case "@repair":
				Msg("Oh! You want to repair your stuff!<br/>You won't find a better person than me, Galvin!<br/>Well, show me the item that you want repaired.<br/>I'll fix it like new! Just trust me on this!<repair rate='93' stringid='(*/misc_repairable/*)|(*/cashchair/*)' />");

				while (true)
				{
					var repair = await Select();

					if (!repair.StartsWith("@repair"))
						break;

					var result = Repair(repair, 93, "/misc_repairable/", "/cashchair/");
					if (!result.HadGold)
					{
						RndMsg(
							"Excuse me...<br/>no IOU's!<br/>It's cash only.",
							"If you can't pay the fee,<br/>then come back later when you have the money to pay in advance!",
							"Hey, you need to pay!<br/>We may be good friends, but you need to pay up first!"
						);
					}
					else if (result.Points == 1)
					{
						if (result.Fails == 0)
							RndMsg(
								"Just like new!<br/>Well, how do you like it?",
								"There! Done! Haha!<br/>I'm pretty talented at repairing, too. Hahahahaha!",
								"Done!<br/>Wow! I only repaired 1 point, and it looks like new!<br/>Do you like it? Just like new, right?"
							);
						else
							Msg("This... is touger than it looks.<br/>I made a mistake, but it's not because of my lack of skill.<br/>There's nothing you or anybody can do about it, so<br/>just deal with it.");
						Msg("Arrgh... another one of these...<br/>Sorry, but I think it was a defect to begin with.<br/>It is not my fault. My guess is, you bought a defective product from the beginning.<br/>I don't think there's anything you can do.");
					}
					else if (result.Points > 1)
					{
						if (result.Fails == 0)
							RndMsg(
								"Done! This wasn't a normal repair, it's almost recreation. Haha!<br/>...This must mean something to you...? Hahaha.",
								"This was a piece of cake to me! Hahaha...<br/>There, just like new. Hahaha!<br/>Bring some of your friends.<br/>I'll give them a special discount!"
							);
						else
							Msg(string.Format(L("Hey, this one is designed not to be fixed above {1} point(s).<br/>Think about it, if people are only selling perfect products, then how would we ever make any money?<br/>So please calm down and just blame the one who made this faulty product. Alright?"), result.Fails, result.Successes));
					}
				}

				Msg("Well, well! I'll see you later then! Haha!<repair hide='true'/>");
				break;

			case "@lostandfound":
				Msg("The Castle's administrators have finally acknowledged my abilities<br/>and assigned me to be in charge of<br/>all lost items here. Hahaha...");
				Msg("If you happen to lose anything, <username/><br/>just come and visit me here.<br/>Just as long as your item was lost during battle,<br/>I'll be able to retrieve it with all its enchants intact.");
				Msg("I'll have to charge you a small fee...<br/>but a fee would still be a lot cheaper than buying a new one outright, don't you think?<br/>Haha...");
				Msg("Oh, by the way, I can only hold up to 20 of your items<br/>since I have limited space here.<br/>Remember this: if you don't have space to hold your most recent lost items,<br/>then I'll have to destroy your earliest stored items, yep.");
				Msg("Unimplemented");
				break;

			case "@upgrade":
				Msg("What would you like to upgrade?<br/>Let me know!<upgrade />");

				while (true)
				{
					var reply = await Select();

					if (!reply.StartsWith("@upgrade:"))
						break;

					var result = Upgrade(reply);
					if (result.Success)
						Msg("Here you go!<br/>Is there anything else you'd like to upgrade?");
					else
						Msg("(Error)");
				}

				Msg("Come again anytime!<upgrade hide='true'/>");
				break;
		}

		End("Thank you, <npcname/>. I'll see you later!");
	}

	private void Greet()
	{
		if (Player.IsDoingPtjFor(NPC))
		{
			Msg(FavorExpression(), L("Missing Dialogue, talking to npc while doing ptj."));
		}
		else if (Memory <= 0)
		{
			Msg(FavorExpression(), L("Oh, is this your first time here?<br/>What would you like to know?"));
		}
		else if (Memory == 1)
		{
			Msg(FavorExpression(), L("Haha, believe it or not, I have a photographic memory.<br/>You were here before, weren't you?"));
		}
		else if (Memory == 2)
		{
			Msg(FavorExpression(), L("Your name is, <username/>...Right?<br/>Haha, I told you! I do have a photographic memory!"));
		}
		else if (Memory <= 6)
		{
			Msg(FavorExpression(), L("Welcome, <username/>!<br/>How's everything going?"));
		}
		else
		{
			Msg(FavorExpression(), L("It is none other than my favorite customer!<br/>How are ya?!"));
		}

		UpdateRelationAfterGreet();
	}

	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				Msg(FavorExpression(), "Where are you from, <username/>?<br/>I'm from the mining city renowned for Adamantium!<br/>I'm from B-A-N-G-O-R!");
				ModifyRelation(1, 0, 0);
				break;

			case "rumor":
				Msg(FavorExpression(), "Haha! Lucas is my savior...<br/>When I first came to this town, I was completely lost.<br/>That's when Lucas approached me and helped me grow into the person that I am now. All thanks to Lucas!<br/>Understand? Haha!");
				ModifyRelation(Random(2), 0, Random(3));
				break;

			case "shop_misc":
				Msg("Looking for a General Shop?<br/>Hey! So here you are!<br/>I have quite a few things available.");
				Msg("I am the reason why there is no General Shop in this town! Hahaha!");
				break;

			case "shop_grocery":
				Msg("A Grocery store?<br/>Well, it's kind of far from here...<br/>First, you need to go to the Town Square, then follow the road towards the west.");
				Msg("...You'll find a nice restaurant near the lake.<br/>It's called Loch Lios, and it's very famous around here. Haha!");
				break;

			case "shop_healing":
				Msg("Are you looking for the Healer's House?<br/>Oh, oohhh, so you are going there to see Agnes.<br/>Ahh... Well, I'm not saying this because I'm good friends with her,<br/>but she is really a wonderful lady. Haha!");
				Msg("Do you want me to introduce you to her? Hehe!");
				break;

			case "shop_inn":
				Msg("...What?<br/>You're looking for an inn?<br/>There's no such thing as an inn in this town...");
				Msg("...Hey! How about this?<br/>Why don't you sleep over at my place...? It's pretty close by.<br/>Don't worry, I kept that place organized.<br/>A couple of bugs here and there, but who cares?");
				Msg("...Uh, you really want to go?<br/>I was only kidding... hehe.");
				break;

			case "shop_bank":
				Msg("A Bank? Wow, no wonder you seem to be at ease!<br/>I could sense it the moment you stepped in here...<br/>Well, you'll first need to head to the alley near Osla's Weapons Shop. Go another block from there, and you'll find the Bank.");
				Msg("You'll find an old lady there.<br/>Be nice to her. She's Jocelin, the branch manager of the bank.");
				Msg("She can be a bit anal about certain things, but<br/>that also means she's very careful with your money.<br/>You can definitely trust her. I guarantee it!");
				Msg("Um...I probably shouldn't have said that last line.");
				break;

			case "shop_smith":
				Msg("You're looking for a Blacksmith's Shop?<br/>My oh my, this is a big town, but<br/>we don't have one here...");
				Msg("If you are going to fix your weapons,<br/>try Osla's Weapons Shop.<br/>She may be a bit ditzy, but<br/>she looks pretty cute, you know... Haha!");
				break;

			case "skill_range":
				Msg("Ah, <username/>, so you have an interest in that area.<br/>Hahaha!<br/>Why didn't you tell me so...");
				Msg("If you buy a bow at the Weapons Shop and practice with it a bit,<br/>you'll learn that skill in no time. Haha! <br/>...I highly recommend Osla's Weapons Shop, if you are planning to buy one.");
				break;

			case "skill_rest":
				Msg("The Resting skill?<br/>Hah... the ones that need the skill the most may be<br/>people like me who are on their feet all day!");
				Msg("...You won't believe how busy this place gets at times...<br/>You don't even have a minute to sit down.");
				break;

			case "skill_instrument":
				Msg("Well, for the Instrument playing skill... Hmm, let's see.<br/>Oh, you can ask Nele about that!");
				Msg("You'll see him at the Town Square right there.<br/>If he likes you, he might teach you<br/>a thing or two about playing an instrument.");
				Msg("Well, honestly, I am not interested in it, so...<br/>Plus, I don't know how to play one at all.");
				break;

			case "skill_tailoring":
				Msg("The Tailoring skill?<br/>Uh, I don't know anything about that.");
				break;

			case "skill_magnum_shot":
				Msg("Shooting really strong arrows, maybe...");
				break;

			case "skill_smash":
				Msg("Are you some kind of a thug...?");
				break;

			case "skill_gathering":
				Msg("So you are intered in the gathering skill?<br/>I have some tools for that, so take a look!<br/>What do you think?");
				Msg("You know that you need the right set of tools for gathering.");
				break;

			case "square":
				Msg("You don't know where the Town Square is?<br/>Hahaha! Seriously, that was too funny!<br/>Hahahaha!");
				break;

			case "pool":
				Msg("Re...ser...voir? Re...ser...voir!<br/>Oh, I think you're not used to this place.<br/>I understand... you're new here, and people make mistakes here and there.");
				Msg("That body of water you see there is a lake, although I can't remember its name...<br/>But hey, don't just go in there and take a bath or something dumb like that, okay?");
				break;

			case "farmland":
				Msg("Hmm... You definitely don't look the part, but you seem to have some country in you.<br/>If you want to harvest something...<br/>Why don't you go all the way up north?<br/>You can find corn and wheat there!");
				break;

			case "shop_headman":
				Msg("A town Chief? Haha!<br/>Wait, your name is <username/>, right?<br/>Wow, you're definitely from the country!<br/>Okay, I don't deal with people from the yee-haw lands where your gardening chore takes hours to complete!");
				Msg("Hey, are you angry with me? I was just kidding!<br/>There's no way I would say that to you, <username/>. Hahaha!");
				break;

			case "temple":
				Msg("Church...?<br/>Hmm... Why would you go to a boring place like that...?");
				Msg("I'll give you a piece of advice for your own sake.<br/>Forget about wasting your time in such boring places like church.<br/>You should go to some more exciting places... A place like the Observatory!<br/>What about it?");
				break;

			case "skill_windmill":
				Msg("Who told you that<br/>you need skills to run the windmill?");
				break;

			case "skill_campfire":
				Msg("Haha, I see some people light up at the top of the Observatory every once in a while...<br/>Please don't do that!");
				Msg("...No messing around with fire at the Observatory! Please!");
				break;

			case "shop_restaurant":
				Msg("A restaurant...?<br/>You look like a sophisticated classy individual,<br/>so I can definitely see you going to a fancy restaurant for a nice meal...");
				Msg("If price is not an issue, I highly recommend<br/>the world famous Loch Lios!");
				Msg("It's not far from here, and the food is excellent, so<br/>drop by one of these days.");
				break;

			case "shop_armory":
				Msg("A Weapons Shop? Oh! Osla's Weapons Shop!<br/>Well, it's a little difficult to explain,<br/>so why don't we go up the Observatory and I'll try to explain while we're looking at the Weapons Shop...<br/>You should be able to find it easily from up there.");
				Msg("You can even see Osla picking her nose from up there. Puhahahaha!");
				break;

			case "shop_cloth":
				Msg("A Clothing Shop?<br/>Oh. You certainly do seem like a person with a lot of interest in fashion! Hahaha!<br/>Head up north, and you'll find a clothing Shop named Tre'imhse Cairde. I know It's a rough name to pronounce.");
				Msg("If you happen to visit there,<br/>please let the gorgeous Ailionoa know that I sent you! Haha!");
				Msg("...If you are not sure where it is,<br/>why don't you go up the Observatory and see where it is?");
				Msg("Just a few hours ago,<br/>a person who didn't know where the Weapons Shop was, went up the Observatory<br/>and found the excat location of it!");
				break;

			case "shop_goverment_office":
				Msg("So, you are looking for the town office, <username/>? Haha...");
				Msg("...We don't have a town office here.<br/>instead, we have a castle...<br/>The castle is not too faraway from here.<br/>Do you need the directions to the Castle?");
				Msg("The Castle's guards look pretty mean...<br/>but they are actually nice guys.");
				Msg("Oh, if you want to retrieve any lost items,<br/>don't bother going all the way to the castle.<br/>Because of some concerns about the castle's security they've moved their lost item service off-location, outside of the castle<br/>and I, Galvin, is in charge of all the lost items in this town. Haha...");
				break;

			case "graveyard":
				Msg("The Graveyard is on the westside of the town.<br/>...Is... someone you know buried over there?<br/>...Well then, why don't you bring some flowers?");
				Msg("You can find Del and Delen at the Square.<br/>Those ladies will pick the right flowers for you.");
				break;

			case "bow":
				Msg("Oh, you need a bow?<br/>How much are you willing to spend?");
				Msg("Haha, just kidding. I have most of the items you'll need,<br/>but if you're talking about a bow,<br/>you need to buy one at Osla's Weapons Shop.<br/>Osla would kill me if I were to sell stuff like that!");
				break;

			case "lute":
				Msg("Haha, you must be really interested in instruments!<br/>In that case, I may have to show you all the instruments I have for you!");
				Msg("Check this out!<br/>There are two kinds of instruments. The wind instrument, and the string instrument.<br/>We'll talk about the wind instrument later.<br/>This is the Lute, the most popular string instrument.");
				Msg("The best Lutes are made in Emain Macha! You knew that, right?<br/>You seem to be a really nice person, so<br/>I'll give you a special discount, Just choose the one you'd like to buy!");
				Msg("...Oh no... we have a slight problem. I am out of stock.<br/>If you really need one,<br/>ask Nele at the Square. He should be selling them, too!");
				break;

			case "complicity":
				Msg("...How could you twist my hospitality like that...<br/>That makes this Galvin really sad.");
				break;

			case "tir_na_nog":
				Msg("Haha! You must mean 'Heaven'!<br/>If you make your way up to the top the Observatory, you will be that much closer to heaven.<br/>What do you say? Don't you want to go up there?");
				Msg("...You'll have the best view of the area. I, Galvin, will guarantee it on my own name!");
				break;

			case "nao_friend":
				Msg("Are you a friend of Nao's?<br/>Really, you? No?<br/>Well, it's always good to have alot of friends.");
				Msg("If you happen to see Lucas,<br/>please say hi to him for me, okay?<br/>Hahahaha!");
				break;

			case "nao_owl":
				Msg("...So Nao has an owl for a pet?<br/>I heard owls are not easy to tame.<br/>She must have struggled for a long time.<br/>I don't know how she did it...");
				break;

			case "nao_blacksuit":
				Msg("So you are looking for Nao...<br/>And she's wearing a black dress, am I correct?<br/>Oh man! Everyone wears black around here.");
				Msg("...For some reason, people seem to be obsessed with black and white clothes,<br/>so you need more details to describe her.");
				Msg("I wish she would wear more colorful and fancy clothes like me.");
				break;

			case "nao_owlscroll":
				Msg("Are you saying that you haven't received a single scroll from an owl, ever?<br/>Lucas told me some of the scrolls here are getting snatched by owls...");
				Msg("...Oh, I shouldn't be telling you this.<br/>I'm sorry! Oh anyway, that's that.");
				break;

			case "breast":
				Msg("Well... is it my heart?");
				Msg("Wow...<br/>Agnes will have to do a consultation of my mental state...");
				break;

			case "EG_C2_Unknown_Continet":
				Msg("Ahh. Thank you for asking!<br/>I specialize in that area!<br/>Who else would know more aobut that than I, who have<br/>talked to so many adventurers?");
				Msg("Uh... wait...what did you just say?<br/>An uncharted continent?");
				Msg("That is...well, hahaha.<br/>I have to try to remember<br/>so please hold. Hahaha...");
				break;

			case "EG_C2_HowTo_Shipboard":
				Msg("Come to think of it, I think there might have been a harbor<br/>on the south end of Bangor, where I grew up.<br/>But grownups forbade me from<br/>going there, if I remember correctly...");
				Msg("Well, not that a few fomors appearing would scare<br/>someone like you anyway, <username/>,<br/>right? Hehehe.");
				break;

			default:
				RndFavorMsg(
					"Haha!<br/>I don't know about stuff like that!",
					"...I have no idea...<br/>I'll definitely let you know if I find out the answer! I promise!",
					"I have no idea!",
					"Thank you for asking!<br/>But I have no idea what that is...",
					"Haha! How am I supposed to know about something like that?",
					"Th... That is...<br/>...oh, right.<br/>Lucas should know!<br/>Why don't you ask Lucas? Haha!"
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
					"Haha! This will be a great addition to my collection!",
					"Oh, did you win some lottery, <username/>?<br/>I can't believe this...<br/>Of course I'll be more than glad to accept this! Haha!",
					"huh! A gift from you, <username/>?<br/>Wow! What's going on! This is great!",
					"Whoa! A gift!<br/>For me? Hahaha!<br/>Thanks! Thanks!"
				);
				break;
		}
	}
}

public class GalvinShop : NpcShopScript
{
	public override void Setup()
	{
		Add("General Goods", 2001);       // Gold Pouch
		Add("General Goods", 2005);       // Item Bag (7x5)
		Add("General Goods", 2006);       // Big Gold Pouch
		Add("General Goods", 2024);       // Item Bag (7x6)
		Add("General Goods", 2025);       // Item Bag (5x9)
		Add("General Goods", 2029);       // Item Bag (8x6)
		Add("General Goods", 18159);      // Plastic Frame Glasses
		Add("General Goods", 18029);      // Wood-rimmed Glasses
		Add("General Goods", 18160);      // Trudy Metal Glasses
		Add("General Goods", 62021, 100); // Six-sided Die x100
		Add("General Goods", 63020);      // Empty Bottle
		Add("General Goods", 64018, 10);  // Paper x10
		Add("General Goods", 64018, 100); // Paper x100
		Add("General Goods", 63001);      // Wings of a Goddess
		Add("General Goods", 63001, 5);   // Wings of a Goddess x5
		Add("General Goods", 62012);      // Elemental Remover
		Add("General Goods", 62012, 5);   // Elemental Remover x5
		Add("General Goods", 40045);      // Fishing Rod
		Add("General Goods", 60034, 300); // Bait Tin

		//China Only
		//Add("General Goods", 2030);     // Asian Big Pouch

		Add("Tailoring", 60001);    // Tailoring Kit
		Add("Tailoring", 60015, 1); // Cheap Finishing Thread x1
		Add("Tailoring", 60015, 5); // Cheap Finishing Thread x5
		Add("Tailoring", 60016, 1); // Common Finishing Thread x1
		Add("Tailoring", 60016, 5); // Common Finishing Thread x5
		Add("Tailoring", 60017, 1); // Fine Finishing Thread x1
		Add("Tailoring", 60017, 5); // Fine Finishing Thread x5
		Add("Tailoring", 60018, 1); // Finest Finishing Thread x1
		Add("Tailoring", 60018, 5); // Finest Finishing Thread x5
		Add("Tailoring", 60019, 1); // Cheap Fabric Pouch x1
		Add("Tailoring", 60019, 5); // Cheap Fabric Pouch x5
		Add("Tailoring", 60020, 1); // Common Fabric Pouch x1
		Add("Tailoring", 60020, 5); // Common Fabric Pouch x5
		Add("Tailoring", 60031);    // Regular Silk Weaving Gloves
		Add("Tailoring", 60046);    // Finest Silk Weaving Gloves
		Add("Tailoring", 60055);    // Fine Silk Weaving Gloves
		Add("Tailoring", 60056);    // Finest Fabric Weaving Gloves
		Add("Tailoring", 60057);    // Fine Fabric Weaving Gloves

		Add("Sewing Patterns", 60000, "FORMID:4:101;", 3000);   // Sewing Pattern - Cores' Healer Dress (F)
		Add("Sewing Patterns", 60000, "FORMID:4:102;", 1700);   // Sewing Pattern - Magic School Uniform (M)
		Add("Sewing Patterns", 60000, "FORMID:4:104;", 4600);   // Sewing Pattern - Cores' Healer Gloves
		Add("Sewing Patterns", 60000, "FORMID:4:106;", 400);    // Sewing Pattern - Popo's Skirt (F)
		Add("Sewing Patterns", 60000, "FORMID:4:107;", 1250);   // Sewing Pattern - Mongo's Traveler Suit (F)
		Add("Sewing Patterns", 60000, "FORMID:4:110;", 4000);   // Sewing Pattern - Cores' Healer Suit (M)
		Add("Sewing Patterns", 60000, "FORMID:4:111;", 4600);   // Sewing Pattern - Guardian Gloves
		Add("Sewing Patterns", 60000, "FORMID:4:113;", 1250);   // Sewing Pattern - Leather Bandana
		Add("Sewing Patterns", 60000, "FORMID:4:114;", 400);    // Sewing Pattern - Hairband
		Add("Sewing Patterns", 60000, "FORMID:4:122;", 5000);   // Sewing Pattern - Professional Silk Weaving Gloves
		Add("Sewing Patterns", 60000, "FORMID:4:123;", 125000); // Sewing Pattern - Ring-Type Mini Leather Dress (F)
		Add("Sewing Patterns", 60000, "FORMID:4:126;", 77000);  // Sewing Pattern - Lueys' Vest Wear
		Add("Sewing Patterns", 60000, "FORMID:4:127;", 55000);  // Sewing Pattern - Broad-brimmed Feather Hat
		Add("Sewing Patterns", 60000, "FORMID:4:128;", 11000);  // Sewing Pattern - Tork's Little-brimmed Hat
		Add("Sewing Patterns", 60044, "FORMID:4:171;", 172000); // Sewing Pattern - High-class Leather Armor
		Add("Sewing Patterns", 60044, "FORMID:4:172;", 86400);  // Sewing Pattern - Middle-class Leather Armor
		Add("Sewing Patterns", 60044, "FORMID:4:173;", 32500);  // Sewing Pattern - Basic Leather Armor
		Add("Sewing Patterns", 60044, "FORMID:4:180;", 73000);  // Sewing Pattern - Riding Suit
		Add("Sewing Patterns", 60044, "FORMID:4:182;", 72200);  // Sewing Pattern - Terra Diamond-shaped Leather Boots
		Add("Sewing Patterns", 60044, "FORMID:4:185;", 121000); // Sewing Pattern - Spark Leather Armor

		Add("Gift", 52008); // Anthology
		Add("Gift", 52009); // Cubic Puzzle
		Add("Gift", 52011); // Socks
		Add("Gift", 52017); // Underwear Set
		Add("Gift", 52018); // Hammer

		Add("Event"); // Empty

		if (IsEnabled("Handicraft"))
			Add("General Goods", 60045); // Handicraft Kit

		if (IsEnabled("PetBirds"))
			Add("General Goods", 40093); // Pet Instructor Stick

		if (IsEnabled("AntiMacroCaptcha"))
		{
			Add("General Goods", 51227, 1);  // Ticking Quiz Bomb x1
			Add("General Goods", 51227, 20); // Ticking Quiz Bomb x20
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

		if (IsEnabled("PremiumBags"))
			Add("General Goods", 2038); // Item Bag (8X10)

		if (IsEnabled("Reforges"))
			Add("General Goods", 85571); // Reforging Tool
	}
}