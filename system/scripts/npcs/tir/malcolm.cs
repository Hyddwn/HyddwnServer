//--- Aura Script -----------------------------------------------------------
// Malcolm
//--- Description -----------------------------------------------------------
// General store manager
//---------------------------------------------------------------------------

public class MalcolmScript : NpcScript
{
	public override void Load()
	{
		SetRace(10002);
		SetName("_malcolm");
		SetBody(height: 1.22f);
		SetFace(skinColor: 16, eyeType: 26, eyeColor: 162);
		SetStand("human/male/anim/male_natural_stand_npc_malcolm_retake", "human/male/anim/male_natural_stand_npc_malcolm_talk");
		SetLocation(8, 1238, 1655, 59);

		EquipItem(Pocket.Face, 4900, 0x00FFB859, 0x003C6274, 0x00505968);
		EquipItem(Pocket.Hair, 4155, 0x00ECBC58, 0x00ECBC58, 0x00ECBC58);
		EquipItem(Pocket.Armor, 15655, 0x00D8C9B7, 0x00112A13, 0x00131313);
		EquipItem(Pocket.Shoe, 17287, 0x00544838, 0x00000000, 0x00000000);
		EquipItem(Pocket.RightHand1, 40491, 0x00808080, 0x00000000, 0x00000000);
		EquipItem(Pocket.LeftHand1, 40017, 0x003F7246, 0x00C0B584, 0x003F4B40);

		AddGreeting(0, "Welcome to the General Shop. This must be your first visit here.");
		AddGreeting(1, "Thank you for coming again.");
		//AddGreeting(2, "Thank you for visiting again, <username/>.<br/>If you come and shop here regularly,<br/>I will treat you as a VIP customer. Ha ha.");
		//AddGreeting(3, "Ah, my VIP customer, <username/>! Welcome.");

		AddPhrase("Maybe I should wrap it up and call it a day...");
		AddPhrase("Aww! My legs hurt. My feet are all swollen from standing all day long.");
		AddPhrase("I wonder what Nora is doing now...");
		AddPhrase("These travelers will buy something sooner or later.");
	}

	protected override async Task Talk()
	{
		SetBgm("NPC_Malcolm.mp3");

		await Intro(
			"While his thin face makes him look weak,",
			"and his soft and delicate hands seem much too feminine,",
			"his cool long blonde hair gives him a suave look.",
			"He looks like he just came out of a workshop since he's wearing a heavy leather apron."
		);

		Msg("What can I do for you?", Button("Start a Conversation", "@talk"), Button("Shop", "@shop"), Button("Repair Item", "@repair"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());
				if (Player.Titles.SelectedTitle == 11002)
				{
					Msg("You're the... Guardian of Erinn?<br/>I don't know what you do exactly,<br/>but you seem to leave<br/>a really good impression on people.");
					Msg("...I'm a bit jealous...");
				}
				await Conversation();
				break;

			case "@shop":
				Msg("Welcome to <npcname/>'s General Shop.<br/>Look around as much as you wish. Clothes, accessories and other goods are in stock.");
				OpenShop("MalcolmShop");
				return;

			case "@repair":
				Msg("What item do you want to repair?<br/>You can repair various items such as Music Instruments and Glasses.<repair rate='95' stringid='(*/misc_repairable/*)|(*/pouch/bag/*)' />");

				while (true)
				{
					var repair = await Select();

					if (!repair.StartsWith("@repair"))
						break;

					var result = Repair(repair, 95, "/misc_repairable/", "/pouch/bag/");
					if (!result.HadGold)
					{
						RndMsg(
							"Sorry, you need more money.",
							"I'm sorry but you don't have enough money.",
							"Um... OK.<br/>If you don't have enough money at the moment, why don't you come back later?"
						);
					}
					else if (result.Points == 1)
					{
						if (result.Fails == 0)
							RndMsg(
								"I'm happy to say it's repaired.",
								"OK, 1 point repair is finished.",
								"It has been repaired well."
							);
						else
							RndMsg(
								"Oh no! I'm so sorry.",
								"Aw... I'm terribly sorry."
							);
					}
					else if (result.Points > 1)
					{
						if (result.Fails == 0)
							RndMsg(
								"The repairing is complete.",
								"Hahaha, it's perfectly repaired.",
								"Wow! It has been repaired perfectly.<br/>Yes, I'm surprised, too."
						);
						else
							// TODO: Use string format once we have XML dialogues.
							Msg(result.Successes + " point(s) repaired.<br/>Unfortunately, I have failed in repairing " + result.Fails + " point(s).");
					}
				}

				Msg("Let me give you a tip.<br/>If you bless your item with Holy Water of Lymilark,<br/>you can reduce abrasion which means<br/>your item will wear off more slowly over time.<repair hide='true'/>");
				break;
		}

		End("Goodbye, <npcname/>. I'll see you later!");
	}

	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				GiveKeyword("shop_misc");
				Msg("I run this General Shop. I sell various goods.");
				ModifyRelation(Random(2), 0, Random(2));
				break;

			case "rumor":
				Msg("Tir Chonaill is a peaceful town.<br/>So when something happens, everyone in the town knows it right away.<br/>I warn you, some were humiliated because of that...<br/>Nothing is as important as being responsible for your own actions.");
				Msg("If you behave like Tracy, you'll be in big trouble.");
				ModifyRelation(Random(2), 0, Random(2));
				break;

			case "about_skill":
				Msg("If you are interested in music skills,<br/>why don't you buy the 'Introduction to Composing Tunes' in my shop?");
				Msg("I try to have as many items as possible in stock,<br/>but it's not easy to bring books to a rural town.");
				Msg("There is a Bookstore in Dunbarton.<br/>So if you're looking for books on music, go there.");
				//Msg("Have you heard of the Weaving skill?<br/>It is a skill of spinning yarn from natural materials and making fabric.<p/>Do you want to learn the Weaving skill?<br/>Actually, I'm out of thick yarn and can't meet all the orders for fabric...<br/>If you get me some wool, I'll teach you the Weaving skill in return.<br/>An owl will deliver you a note on how to find wool if you wait outside.");
				//Event Flag for quest "Gathering Wool" is activated after weaving msg
				break;

			case "about_arbeit":
				Msg("Unimplemented");
				//Msg("Sorry, but it is not time for part-time jobs.<br/>Would you come later?");
				//Msg("Our town may be small, but running the General Shop<br/>can really get hectic since I'm running this all by myself.<br/>Fortunately, many people are helping me out, so it's a lot easier for me to handle.<br/>Are you also interested in working here, <username/>?<p/>I'll pay you if you can help me.");
				break;

			case "shop_misc":
				Msg("Yes, this is <npcname/>'s General Shop.");
				break;

			case "shop_grocery":
				Msg("The Grocery Store is right across from here.<br/>Caitin is such an honest, meticulous person<br/>that she only sells fresh goods. You should absolutely pay her a visit.");
				Msg("A one-sided diet is bad for your health<br/>so pick your food wisely.");
				break;

			case "shop_healing":
				GiveKeyword("graveyard");
				Msg("Health is the most valuable thing to have.<br/>What use can wealth and fame be<br/>when you're bedridden or buried dead?<br/>We should try to stay healthy all the time.");
				Msg("In my opinion, avoiding anything that can be dangerous<br/>is the top priority.<br/>A penny for your thoughts!");
				Msg("Oh, I'm sorry.<br/>I forgot to tell you where Dilys lives.<br/>Go up the road and you'll see the Healer's House in no time.");
				break;

			case "shop_inn":
				GiveKeyword("windmill");
				Msg("It's right next door. See the building down there?<br/>It's just down the road.<br/>The Windmill nearby is worth watching, so if you have time, go take a look.<br/>Oh, and... Don't forget to see Piaras at the Inn.");
				Msg("Um... Nora... You know, the girl standing in front of the Inn.<br/>Can you... Maybe... Umm... Say hello to her for me?");
				break;

			case "shop_bank":
				Msg("That's the place where Bebhinn works. I know she's an excellent worker,<br/>but I don't trust her...");
				Msg("She gossips about other people too much<br/>and often doesn't tell you when you forget about your deposited items.");
				break;

			case "shop_smith":
				GiveKeyword("brook");
				Msg("Many people seem to be confusing the General Shop with the Blacksmith's Shop.<br/>I sell general goods, and Ferghus usually sells<br/>weapons or armor made from iron.<br/>His shop is near the Adelia Stream at the entrance of town.");
				break;

			case "skill_range":
				Msg("I'm not interested in such things.<br/>Fighting is immature and stupid.");
				Msg("However, some see violence as an indication of masculinity.<br/>I can't say I like it.");
				break;

			case "skill_instrument":
				Msg("Are you interested in playing instruments?<br/>If you acquire the skill,<br/>please buy a lute or a ukulele from my shop.");
				break;

			case "skill_composing":
				Msg("You must be interested in composition.<br/>Then open the Shop window.<br/>There it is - Introduction to Composing Tunes! Everything you need to know about composing is written in it.<br/>Read it carefully and you'll learn the Composing skill in no time.");
				Msg("And, after acquiring the Composing Skill, you'll need Music Scores.<br/>You can write your own music on blank Music Scores!");
				Msg("Buy it! Come on, buy it!<br/>That's right, press 'Shop'!");
				break;

			case "skill_tailoring":
				GiveKeyword("shop_grocery");
				Msg("No one is as good as Caitin when it comes to tailoring.<br/>Have you talked to her?");
				Msg("She's at the Grocery Store just out the door.<br/>You should talk to her about it<br/>if you haven't already.");
				break;

			case "skill_magnum_shot":
				GiveKeyword("school");
				Msg("I'm sure I've heard about it before.<br/>It's a powerful attack you can shoot with a bow...<br/>Travelers mention it a lot.");
				Msg("They say Trefor up the road<br/>and Ranald at the School<br/>can teach it.");
				Msg("Huh? Are you asking me whether I have learned it?<br/>Frankly speaking, I'm not interested.<br/>Bows are not of much use in daily life, you know.<br/>But if you're interested, why not go learn it yourself?");
				break;

			case "skill_counter_attack":
				GiveKeyword("school");
				Msg("Um... I don't know what it is,<br/>but it sounds like a combat skill.<br/>How about asking Ranald at the School?");
				Msg("I'm sure he'll be a lot more helpful than me.");
				break;

			case "skill_smash":
				GiveKeyword("school");
				Msg("I did see people using the Smash skill.<br/>But that doesn't mean I know anything about it...<br/>Ask Ranald, the combat instructor at the School.<br/>He'd definitely know more about it.");
				break;

			case "skill_gathering":
				GiveKeyword("shop_smith");
				Msg("Sounds like you're trying to do some gathering.<br/>You can't do much with your bare hands. You need the right tools.<br/>I happen to sell some Gathering Knives. Would you like to buy one?");
				Msg("If you don't want any,<br/>there are other blade weapons at the Blacksmith's Shop...");
				break;

			case "square":
				Msg("...<br/>Are you joking?<br/>You ask me where the Square is when it's just out the door?");
				Msg("Um... Do I look so naive?<br/>Maybe I should change my hairstyle...");
				break;

			case "pool":
				GiveKeyword("shop_grocery");
				Msg("Go down the road behind Caitin's Grocery Store and you'll find it soon.<br/>If it weren't for the reservoir,<br/>the crops wouldn't grow.<br/>It sure does play a vital role in our town's agriculture.");
				break;

			case "farmland":
				Msg("The farmland is near the School.<br/>How come so many travelers are interested in it?<br/>There's nothing special about it.");
				Msg("What's more, their careless strolls through the farmland<br/>are damaging the crops...");
				break;

			case "windmill":
				Msg("You can find the Windmill by going down the side street from the Inn.<br/>I used to watch the Windmill's blades making shadows<br/>while the sun was setting... With Nora...");
				Msg("But these days she won't see me because she says she's busy.");
				break;

			case "brook":
				GiveKeyword("pool");
				Msg("Adelia Stream is like water of life<br/>to our town, Tir Chonaill.<br/>To begin with, the reservoir's water is mainly from the Adelia Stream...");
				Msg("The water from the stream plays an important role,<br/>providing drinking water for us and our sheep, but also water for housework too.<br/>My words can't fully describe the importance of it... Hmmph.");
				break;

			case "shop_headman":
				Msg("The Chief's House? It's on the hill over there.<br/>If your eyesight is good, you can see it from here.");
				Msg("If you can't remember it, think of it this way...<br/>A person of a high position lives in a high location.");
				break;

			case "temple":
				Msg("So, you want to go to Church?<br/>Let's see... Go down a bit from the Bank over there,<br/>and you can't miss it.");
				Msg("Could you tell Priest Meven that I have<br/>lots of high-quality candlesticks when you get there?<br/>You can tell Priestess Endelyon instead<br/>if he's not there.");
				break;

			case "school":
				GiveKeyword("pool");
				Msg("The School?<br/>You can get there by going down the road towards the Bank and to the reservoir.<br/>If you still can't find it, right-click your mouse and look around.<br/>Scrolling the mouse wheel would help too.");
				Msg("By the way, are you a student?");
				break;

			case "skill_campfire":
				Msg("Beats me. Why would someone<br/>build a campfire<br/>when they could just stay inside a house?");
				Msg("Things could go wrong and you could burn down the entire forest, you know.");
				break;

			case "shop_restaurant":
				Msg("Have you visited the Grocery Store next door?<br/>There aren't any restaurants in town,<br/>but any food can be bought at the Grocery Store.<br/>So we don't feel it's an inconvenience.");
				Msg("And, after all,<br/>Caitin is an excellent cook.");
				break;

			case "shop_armory":
				GiveKeyword("shop_smith");
				Msg("Weapons Shop? Well...<br/>If you're looking for weapons, try the Blacksmith's Shop.<br/>There aren't any Weapons Shops in this town.");
				Msg("Is it just me? Or are you trying to boast about<br/>having come from a city?");
				break;

			case "shop_cloth":
				GiveKeyword("shop_misc");
				Msg("If you're looking for clothes, you can buy them here.<br/>This may be a General Shop, but I do have some simple clothes in stock.<br/>We also have lots of clothes for ladies. Why don't you take a look around?");
				Msg("...");
				Msg("Um... Why do you look so doubtful?");
				break;

			case "shop_bookstore":
				GiveKeyword("shop_misc");
				Msg("Ah, are you looking for books?<br/>I once brought in a lot of books, but nobody bought them and I lost a lot of money.<br/>Since then, I haven't been stocking books.");
				Msg("At least some books on the Composing skill did sell pretty well.");
				break;

			case "shop_goverment_office":
				Msg("Um... you can't find such a thing in a country town like this. Expect to find it in a big city.<br/>And this is an autonomous district<br/>protected by Ulaid, descendants of Partholon.");
				Msg("What was it...<br/>Some people said they have to go to the Town Office to find their lost items.<br/>If that's the case, you can go ask Chief Duncan.");
				Msg("He greets new adventurers,<br/>takes care of a weird cat,<br/>and returns lost items.<br/>He lives a busy life indeed.");
				break;

			case "graveyard":
				Msg("Looking for the graveyard?<br/>...");
				Msg("...");
				Msg("Just go up there.<br/>You really are weird.");
				break;

			case "skill_fishing":
				Msg("The Fishing skill?<br/>There's nothing to learn.<br/>You just need a Fishing Rod and a Bait Tin.");
				Msg("You can buy them here,<br/>so take a look if you're interested in fishing.");
				break;

			default:
				RndMsg(
					"I don't know.",
					"Hm... Beats me.",
					"Well... I don't have much to say about it.",
					"I think I heard about it but... I can't remember.",
					"NPCs don't have a conversation book.<br/>So I won't remember the things you told me...",
					"Sorry, I don't know.<br/>Hm... Maybe I should have a travel diary to write things down."
				);
				ModifyRelation(0, 0, Random(2));
				break;
		}
	}
}

public class MalcolmShop : NpcShopScript
{
	public override void Setup()
	{
		Add("General Goods", 1006);       // Introduction to Music Composition
		Add("General Goods", 2001);       // Gold Pouch
		Add("General Goods", 2005);       // Item Bag (7x5)
		Add("General Goods", 2006);       // Big Gold Pouch
		Add("General Goods", 2024);       // Item Bag (7x6)
		Add("General Goods", 2029);       // Item Bag (8x6)
		Add("General Goods", 2038);       // Item Bag (8X10)
		Add("General Goods", 18029);      // Wood-rimmed Glasses
		Add("General Goods", 18029);      // Wood-rimmed Glasses
		Add("General Goods", 19001);      // Robe
		Add("General Goods", 19001);      // Robe
		Add("General Goods", 19001);      // Robe
		Add("General Goods", 19002);      // Slender Robe
		Add("General Goods", 19002);      // Slender Robe
		Add("General Goods", 19002);      // Slender Robe
		Add("General Goods", 40004);      // Lute
		Add("General Goods", 40004);      // Lute
		Add("General Goods", 40004);      // Lute
		Add("General Goods", 40018);      // Ukulele
		Add("General Goods", 40018);      // Ukulele
		Add("General Goods", 40018);      // Ukulele
		Add("General Goods", 40045);      // Fishing Rod
		Add("General Goods", 40214);      // Big Drum
		Add("General Goods", 40214);      // Big Drum
		Add("General Goods", 40214);      // Big Drum
		Add("General Goods", 60034, 300); // Bait Tin x300
		Add("General Goods", 60045);      // Handicraft Kit
		Add("General Goods", 61001);      // Score Scroll
		Add("General Goods", 61001);      // Score Scroll
		Add("General Goods", 61001);      // Score Scroll
		Add("General Goods", 61001);      // Score Scroll
		Add("General Goods", 61001);      // Score Scroll
		Add("General Goods", 62021, 100); // Six-sided Die x100
		Add("General Goods", 63020);      // Empty Bottle
		Add("General Goods", 64018, 10);  // Paper x10
		Add("General Goods", 64018, 100); // Paper x100

		Add("Hats", 18015); // Leather Hat
		Add("Hats", 18016); // Hat
		Add("Hats", 18017); // Tail Cap
		Add("Hats", 18018); // Leather Tail Cap
		Add("Hats", 18019); // Lirina's Feather Cap
		Add("Hats", 18020); // Mongo's Feather Cap
		Add("Hats", 18021); // Merchant Cap
		Add("Hats", 18023); // Mongo's Thief Cap
		Add("Hats", 18024); // Hairband
		Add("Hats", 18025); // Popo's Merchant Cap
		Add("Hats", 18026); // Mongo's Merchant Cap
		Add("Hats", 18027); // Lirina's Merchant Cap

		Add("Shoes && Gloves", 16001); // Quilting Gloves
		Add("Shoes && Gloves", 16002); // Linen Gloves
		Add("Shoes && Gloves", 16003); // Sesamoid Gloves
		Add("Shoes && Gloves", 16011); // Cores' Healer Gloves
		Add("Shoes && Gloves", 16012); // Swordswoman Gloves
		Add("Shoes && Gloves", 16029); // Leather Stitched Glove
		Add("Shoes && Gloves", 16030); // Big Band Glove
		Add("Shoes && Gloves", 16034); // Two-lined Belt Glove
		Add("Shoes && Gloves", 17000); // Women's Flats
		Add("Shoes && Gloves", 17002); // Swordswoman Shoes
		Add("Shoes && Gloves", 17006); // Cloth Shoes
		Add("Shoes && Gloves", 17007); // Leather Shoes
		Add("Shoes && Gloves", 17008); // Cores' Boots (F)
		Add("Shoes && Gloves", 17012); // Leather Shoes
		Add("Shoes && Gloves", 17025); // Sandal
		Add("Shoes && Gloves", 17027); // Long Sandals
		Add("Shoes && Gloves", 17036); // Spika Two-piece Boots
		Add("Shoes && Gloves", 17038); // Mini Ribbon Sandals
		Add("Shoes && Gloves", 17066); // One-button Ankle Shoes
		Add("Shoes && Gloves", 17071); // Knee-high Boots

		Add("Casual", 15000); // Popo's Shirt and Pants
		Add("Casual", 15003); // Vest and Pants Set
		Add("Casual", 15012); // Ceremonial Dress
		Add("Casual", 15018); // Mongo's Traveler Suit (F)
		Add("Casual", 15021); // Elementary School Uniform
		Add("Casual", 15024); // Popo's Dress
		Add("Casual", 15031); // Magic School Uniform (M)
		Add("Casual", 15043); // Track Suit Set

		Add("Formal", 15005); // Adventurer's Suit
		Add("Formal", 15007); // Traditional Tir Chonaill Costume
		Add("Formal", 15011); // Sleeveless and Bell-Bottoms
		Add("Formal", 15013); // China Dress
		Add("Formal", 15025); // Magic School Uniform (F)
		Add("Formal", 15028); // Cores Thief Suit (F)
		Add("Formal", 15059); // Terks' Tank Top and Shorts
		Add("Formal", 15061); // Wave-print Side-slit Tunic

		Add("Event");

		if (IsEnabled("PetBirds"))
		{
			Add("Shoes && Gloves", 16024); // Pet Instructor Glove
			Add("General Goods", 40093);   // Pet Instructor Stick
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
			Add("General Goods", 45130, 10); // Spider Trap x10
	}
}
