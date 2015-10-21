//--- Aura Script -----------------------------------------------------------
// Malcolm
//--- Description -----------------------------------------------------------
// General store manager
//---------------------------------------------------------------------------

public class MalcolmBaseScript : NpcScript
{
	public override void Load()
	{
		SetName("_malcolm");
		SetRace(10002);
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
				//Msg("Welcome to the General Shop. This must be your first visit here.");
				Msg("Thank you for coming again.");
				//Msg("Thank you for visiting again, <username/>.<br/>If you come and shop here regularly,<br/>I will treat you as a VIP customer. Ha ha.");
				//Msg("Ah, my VIP customer, <username/>! Welcome.");
				//Alternate messages as you talk to him more? Apparently it logs your visits at least to this shop
				await StartConversation();
				break;

			case "@shop":
				Msg("Welcome to Malcolm's General Shop.<br/>Look around as much as you wish. Clothes, accessories and other goods are in stock.");
				OpenShop("MalcolmShop");
				return;

			case "@repair":
				Msg("What item do you want to repair?<br/>You can repair various items such as Music Instruments and Glasses.");
				Msg("(Unimplemented)");
				//Next message happens after you close repair window
				//Msg("Let me give you a tip.<br/>If you bless your item with Holy Water of Lymilark,<br/>you can reduce abrasion which means your item will wear off more slowly over time.");
				break;
		}

		End("Goodbye, Malcolm. I'll see you later!");
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
				Msg("Tir Chonaill is a peaceful town.<br/>So when something happens, everyone in the town knows right away.<br/>I warn you, some were humiliated because of that...<br/>Nothing is as important as being responsible for your own actions.<p/>If you behave like Tracy, you'll be in big trouble.");
				ModifyRelation(Random(2), 0, Random(2));
				break;

			case "about_skill":
				Msg("If you are interested in music skills,<br/>why don't you buy the 'Introduction to Composing Tunes' in my shop?<p/>I try to have as many as possible in stock,<br/>but it's not easy to bring books to a rural town.<p/>There is a Bookstore in Dunbarton.<br/>So if you're looking for books on music, go there.");
				//Msg("Have you heard of the Weaving skill?<br/>It is a skill of spinning yarn from natural materials and making fabric.<p/>Do you want to learn the Weaving skill?<br/>Actually, I'm out of thick yarn and can't meet all the orders for fabric...<br/>If you get me some wool, I'll teach you the Weaving skill in return.<br/>An owl will deliver you a note on how to find wool if you wait outside.");
				//Event Flag for quest "Gathering Wool" is activated after weaving msg
				break;

			case "about_arbeit":
				Msg("Sorry, but it is not time for part-time jobs.<br/>Would you come later?");
				//Msg("Our town may be small, but running the General Shop<br/>can really get hectic since I'm running this all by myself.<br/>Fortunately, many people are helping me out, so it's a lot easier for me to handle.<br/>Are you also interested in working here, <username/>?<p/>I'll pay you if you can help me.");
				break;

			case "shop_misc":
				Msg("Yes, this is Malcolm's General Shop.");
				break;

			case "shop_grocery":
				Msg("The Grocery Store is right across from here.<br/>Caitin is such an honest, meticulous person<br/>that she only sells fresh goods. You should absolutely pay her a visit.<p/>A one-sided diet is bad for your health<br/>so pick your food wisely.");
				break;

			case "shop_bank":
				Msg("That's the place where Bebhinn works. I know she's an excellent worker,<br/>but I don't trust her...<p/>She gossips about other people too much<br/>and often doesn't tell you when you forget about your deposited items.");
				break;

			case "shop_smith":
				Msg("Many people seem to be confusing the General Shop with the Blacksmith's Shop.<br/>I sell general goods, and Ferghus usually sells<br/>weapons or armor made from iron.<br/>His shop is near the Adelia Stream at the entrance of town.");
				break;

			case "skill_counter_attack":
				Msg("Um... I don't know what it is,<br/>but it sounds like a combat skill.<br/>How about asking Ranald at the School?<p/>I'm sure he'll be a lot more helpful than me.");
				break;

			case "square":
				Msg("...<br/>Are you joking?<br/>You ask me where the Square is when it's just out the door?<p/>Um... Do I look so naive?<br/>Maybe I should change my hairstyle...");
				break;

			case "pool":
				GiveKeyword("shop_grocery");
				Msg("Go down the road behind Caitin's Grocery Store and you'll find it soon.<br/>If it weren't for the reservoir,<br/>the crops wouldn't grow.<br/>It sure does play a vital role in our town's agriculture.");
				break;

			case "farmland":
				Msg("The farmland is near the School.<br/>How come so many travelers are interested in it?<br/>There's nothing special about it.<p/>What's more, their careless strolls through the farmland<br/>are damaging the crops...");
				break;

			case "shop_headman":
				Msg("The Chief's House? It's on the hill over there.<br/>If your eyesight is good, you can see it from here.<p/>If you can't remember it, think of it this way...<br/>A person of a high position lives in a high location.");
				break;

			case "temple":
				Msg("So, you want to go to the Church?<br/>Let's see... Go down a bit from the Bank over there,<br/>and you can't miss it.<p/>Could you tell Priest Meven that I have lots of high-quality candlesticks when you get there?<br/>You can tell Priestess Endelyon instead<br/>if he's not there.");
				break;

			case "school":
				GiveKeyword("pool");
				Msg("The School?<br/>You can get there by going down the road towards the Bank and to the reservoir.<br/>If you still can't find it, right-click your mouse and look around.<br/>Scrolling the mouse wheel would help too.<p/>By the way, are you a student?");
				break;

			case "skill_campfire":
				Msg("Beats me. Why would someone<br/>build a campfire<br/>when they could just stay inside a house?<p/>Things could go wrong and you could burn down the entire forest, you know.");
				break;

			case "shop_restaurant":
				Msg("Have you visited the Grocery Store next door?<br/>There aren't any restaurants in town, but any food can be bought at the Grocery Store.<br/>So we don't feel it's an inconvenience.<p/>And, after all,<br/>Caitin is an excellent cook.");
				break;

			case "shop_armory":
				Msg("Weapons Shop? Well...<br/>If you're looking for weapons, try the Blacksmith's Shop.<br/>There aren't any Weapon Shops in this town.<p/>Is it just me? Or are you trying to boast about<br/>having come from a city?");
				break;

			case "shop_goverment_office":
				Msg("Um... you can't find such a thing in a country town like this. Expect to find it in a big city.<br/>And this is an autonomous district<br/>protected by Ulaid, descendants of Partholon.<p/>What was it...<br/>Some people said they have to go to the Town Office to find their lost items.<br/>If that's the case, you can go ask Chief Duncan.<p/>He greets new adventurers,<br/>takes care of a weird cat,<br/>and returns lost items.<br/>He lives a busy life indeed.");
				break;

			default:
				RndMsg(
					"I don't know.",
					"Hm... Beats me.",
					"Well... I don't have much to say about it.",
					"I think I heard about it but... I can't remember.",
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
		Add("General Goods", 40093);      // Pet Instructor Stick
		Add("General Goods", 61001);      // Score Scroll
		Add("General Goods", 64018, 10);  // Paper x10
		Add("General Goods", 64018, 100); // Paper x100
		Add("General Goods", 62021, 100); // Six-sided Die x100
		Add("General Goods", 63020);      // Empty Bottle
		Add("General Goods", 19001);   // Robe
		Add("General Goods", 1006);    // Introduction to Music Composition
		Add("General Goods", 60045);      // Handicraft Kit
		Add("General Goods", 60034);    // Bait Tin x300
		Add("General Goods", 40004);      // Lute
		Add("General Goods", 19002);    // Slender Robe
		Add("General Goods", 51227, 1);   // Ticking Quiz Bomb x1
		Add("General Goods", 2001);       // Gold Pouch
		Add("General Goods", 40045);    // Fishing Rod
		Add("General Goods", 40018);    // Ukulele
		Add("General Goods", 40214);    // Bass Drum
		Add("General Goods", 2006);       // Big Gold Pouch
		Add("General Goods", 2005);       // Item Bag (7x5)
		Add("General Goods", 2029);       // Item Bag (8x6)
		Add("General Goods", 2024);       // Item Bag (7x6)
		Add("General Goods", 18029);    // Wood-rimmed Glasses
		Add("General Goods", 91364, 1);   // Seal Scroll (1-day) x1
		Add("General Goods", 91364, 10);  // Seal Scroll (1-day) x10
		Add("General Goods", 91365, 1);   // Seal Scroll (7-day) x1
		Add("General Goods", 91365, 10);  // Seal Scroll (7-day) x10
		Add("General Goods", 91366, 1);   // Seal Scroll (30-day) x1
		Add("General Goods", 91366, 10);  // Seal Scroll (30-day) x10
		Add("General Goods", 85571);      // Reforging Tool

		Add("Hats", 18024);    //  Hairband
		Add("Hats", 18017);    //  Tail Cap
		Add("Hats", 18023);    //  Mongo's Thief Cap
		Add("Hats", 18025);    //  Popo's Merchant Cap
		Add("Hats", 18016);    //  Hat
		Add("Hats", 18015);    //  Leather Hat
		Add("Hats", 18019);    //  Lirina's Feather Cap
		Add("Hats", 18027);    //  Lirina's Merchant Cap
		Add("Hats", 18020);    //  Mongo's Feather Cap
		Add("Hats", 18018);    //  Leather Tail Cap
		Add("Hats", 18021);    //  Merchant Cap
		Add("Hats", 18026);    //  Mongo's Merchant Cap

		Add("Shoes & Gloves", 17012);    //  Leather Shoes (Type 1)
		Add("Shoes & Gloves", 16024);    //  Pet Instructor Glove
		Add("Shoes & Gloves", 17025);    //  Sandal
		Add("Shoes & Gloves", 16029);    //  Leather Stitched Glove
		Add("Shoes & Gloves", 16002);    //  Linen Gloves
		Add("Shoes & Gloves", 17066);    //  One-button Ankle Shoes
		Add("Shoes & Gloves", 16001);    //  Quilting Gloves
		Add("Shoes & Gloves", 16030);    //  Big Band Glove
		Add("Shoes & Gloves", 17006);    //  Cloth Shoes
		Add("Shoes & Gloves", 17000);    //  Women's Flats
		Add("Shoes & Gloves", 17014);    //  Leather Shoes (Type 3)
		Add("Shoes & Gloves", 17002);    //  Swordswoman Shoes
		Add("Shoes & Gloves", 17008);    //  Cores' Boots (F)
		Add("Shoes & Gloves", 17027);    //  Long Sandals
		Add("Shoes & Gloves", 16003);    //  Sesamoid Gloves
		Add("Shoes & Gloves", 16011);    //  Cores' Healer Gloves
		Add("Shoes & Gloves", 16012);    //  Swordswoman Gloves
		Add("Shoes & Gloves", 17036);    //  Spika Two-piece Boots
		Add("Shoes & Gloves", 16034);    //  Two-lined Belt Glove
		Add("Shoes & Gloves", 17071);    //  Knee-high Boots
		Add("Shoes & Gloves", 17038);    //  Mini Ribbon Sandals

		Add("Casual", 15000);    //  Popo's Shirt and Pants
		Add("Casual", 15003);    //  Vest and Pants Set
		Add("Casual", 15021);    //  Elementary School Uniform
		Add("Casual", 15018);    //  Mongo's Traveler Suit (F)
		Add("Casual", 15043);    //  Track Suit Set
		Add("Casual", 15031);    //  Magic School Uniform
		Add("Casual", 15024);    //  Popo's Dress
		Add("Casual", 15012);    //  Ceremonial Dress

		Add("Formal", 15025);    //  Magic School Uniform (F)
		Add("Formal", 15061);    //  Wave-print Side-slit Tunic
		Add("Formal", 15059);    //  Terks' Tank Top and Shorts
		Add("Formal", 15005);    //  Adventurer's Suit
		Add("Formal", 15007);    //  Traditional Tir Chonaill Costume
		Add("Formal", 15028);    //  Cores' Thief Suit (F)
		Add("Formal", 15011);    //  Sleeveless and Bell-Bottoms
		Add("Formal", 15013);    //  China Dress

		Add("Event"); // Empty
	}
}
