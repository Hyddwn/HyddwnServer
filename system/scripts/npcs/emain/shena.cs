//--- Aura Script -----------------------------------------------------------
// Shena
//--- Description -----------------------------------------------------------
// Server at Emain Macha's Restaraunt, Loch Lios
//---------------------------------------------------------------------------

public class ShenaScript : NpcScript
{
	public override void Load()
	{
		SetRace(10001);
		SetName("_shena");
		SetBody(height: 0.8f, upper: 1.1f);
		SetFace(skinColor: 16, eyeColor: 39);
		SetStand("human/male/anim/male_natural_stand_npc_riocard");
		SetLocation(52, 36027, 39195, 48);
		SetGiftWeights(beauty: 1, individuality: 2, luxury: 1, toughness: -1, utility: 2, rarity: 1, meaning: 2, adult: 2, maniac: 2, anime: 1, sexy: -1);

		EquipItem(Pocket.Face, 3900, 0x00D45825, 0x00485F31, 0x00BDE3D9);
		EquipItem(Pocket.Hair, 3038, 0x00960C21, 0x00960C21, 0x00960C21);
		EquipItem(Pocket.Armor, 15078, 0x00FFF8EC, 0x001D100C, 0x00977855);
		EquipItem(Pocket.Shoe, 17007, 0x00000000, 0x00B4A67F, 0x00842160);

		AddPhrase("Can you wait for 5 minutes?");
		AddPhrase("Welcome to Loch Lios!!");
		AddPhrase("Hey, Right here!");
		AddPhrase("Hello hello!");
		AddPhrase("Taste the very best in this kingdom");
		AddPhrase("Alright, please file a single line from here!");
		AddPhrase("Did you like it?");
		AddPhrase("Thank you very much! I'll See you soon.");
		AddPhrase("Here is the special menu.");
		AddPhrase("Please wait for a bit.");
		AddPhrase("Welcome to Loch Lios, a restaurant located right on the lakeside!");
	}

	protected override async Task Talk()
	{
		SetBgm("NPC_Shena.mp3");

		await Intro(L("Shena is a young lady with an adorable dress sporting puffy yellow laces.<br/>She leans a bit to her right so she can look at customers with her large cherry-colored eyes.<br/>Her complexion is very smooth and clear. Each time she speaks, she enunciates her words in her perfect, clear voice, while her<br/>short bobbed hair bounces up and down."));

		Msg("Loch Lios, the best restaurant in Emain Macha! No, the whole kingdom!", Button("Start a Conversation", "@talk"), Button("Shop", "@shop"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());

				if (Player.IsUsingTitle(11001))
				{
					Msg("...You saved the goddess...? You...?<br/>Oh my gosh... such a person exists!!!");
				}
				else if (Player.IsUsingTitle(11002))
				{
					Msg("Hey, <username/>, it's you.<br/>You are quite famous around here now.<br/>They call you... Guardian of Erinn, right?");
					Msg("...So what exactly did you do<br/>that people are calling you that?");
				}

				await Conversation();
				break;

			case "@shop":
				Msg("Take your time looking around!");
				OpenShop("ShenaShop");
				return;
		}

		End("Thank you, <npcname/>. I'll see you later!");
	}

	private void Greet()
	{
		if (Memory <= 0)
		{
			Msg(FavorExpression(), L("<username/>, is this your first time at the restaurant?<br/>Then you've come to the right place."));
		}
		else if (Memory == 1)
		{
			Msg(FavorExpression(), L("Hello, <username/>.<br/>How can I help you?"));
		}
		else if (Memory == 2)
		{
			Msg(FavorExpression(), L("Ah. <username/>.<br/>I was wondering why you haven't come lately."));
		}
		else if (Memory <= 6)
		{
			Msg(FavorExpression(), L("Hello, familiar face.<br/>How can I help you today, <username/>?"));
		}
		else
		{
			Msg(FavorExpression(), L("You're a regular here, <username/>.<br/>Seems like people that came here once, keep coming back for more."));
		}

		UpdateRelationAfterGreet();
	}

	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				Msg(FavorExpression(), "Wait, are you trying to hit on me? Hahaha!");
				ModifyRelation(Random(2), 0, Random(3));
				break;

			// Rumor keywords may need different conditions, I'm unsure of how the cooking contest related messages work.
			case "rumor":
				if (!Player.HasKeyword("Cooker_qualification_test"))
				{
					Player.GiveKeyword("Cooker_qualification_test");
					Msg("Every Samhain, a weekly Chef Exam is held in the Emain Macha town square,<br/>hosted by the three greatest chefs of Erinn.");
					Msg("Gordon, who is the top executive judge, in the contest,<br/>has given me the authority to pick out<br/>whether someone should be considered qualified to enter the contest.");
					Msg("Argh!... Please use the keyword I just gave you<br/>and talk to the people in Emain Macha<br/>to find out more about<br/>the cooking contest.");
					Msg("But the Chef Exam is in progress right now...<br/>You can request to enter the Chef Exam<br/>after this one is over.");
				}
				else
					RndFavorMsg(
						"Someone flat out told me that Loch Lios has the best food<br/>in this kingdom. Hahaha...",
						"On this coming Samhain, the highest qualifying chefs with the highest scores<br/>from all previous cooking contests, will battle each other in a cook-off.<br/>It'll be a sort of a battle of the cooks!"
					);
					ModifyRelation(Random(2), 0, Random(3));
				break;

			case "about_arbeit":
				Msg("By the way, you can't earn the entry ticket for the monthly cook-off by completeing a part-time job.<br/>You have to have won the weekly cooking contests in order to qualify.");
				break;

			case "shop_misc":
				Msg("Are you looking for the General Shop?<br/><username/>, I think this is your first time in this town, right?<br/>...Otherwise, you wouldn't be asking where the general shop is.");
				Msg("Hehe... there are no such shops in this town.<br/>If you are really looking for something similar, though, then go see Galvin at the bottom of the Observatory.<br/>He'll have some items.");
				break;

			case "shop_grocery":
				Msg("Grocery store?<br/>Are you going to cook your own food?");
				Msg("...My master Chef will feel left out if he hears this.<br/>He'll feel bad that you'll want to cook on your own, when there's a fine restaurant like this place around...");
				Msg("...If you are really good at cooking, though, then can you help us out from time to time?<br/>Honestly, I can't really trust Fraser.");
				break;

			case "shop_healing":
				Msg("If you're looking for the Healer's House,<br/>then you should stop by Agnes's place!");
				Msg("From here, head over to the center of the town square, then follow the path that heads north.<br/>Go past the first block, and you'll see it on the right.");
				Msg("Lots of male customers here seem to fake their injuries just to see Agnes.<br/>Haoho...");
				break;

			case "shop_inn":
				Msg("What? An inn?<br/>I've lived here for a while, but<br/>I've never seen a place like that...");
				break;

			case "shop_bank":
				Msg("Do you have some items that you want to store somewhere?<br/>I mean, carrying around items that you don't need right now...<br/>they are nothing but space-eaters.");
				Msg("Hmmm... for that, you can go to the bank.<br/>When you talk to Jocelin, can you tell her I said hi,<br/>and that I'll stop by soon?");
				break;

			case "shop_smith":
				Msg("A Blacksmithin's Shop? Hehe...<br/>there's no such thing in this town.<br/>...It's loud, it's messy, it's scorching hot, and it's all smokey too...");
				Msg("It doesn't fit the image of a town like this.");
				break;

			case "skill_rest":
				Msg("Yes, one of complementary things to the rest skill<br/>is a well-made dish from a master Chef.");
				Msg("...Are you... ready to order?");
				break;

			case "skill_composing":
				Msg("Hmmm? You're more romantic that I'd thought.<br/>Hahaha...");
				Msg("The Chef told us the other day that there should be a<br/>signature song for this restaurant.<br/>Is it possible for you to write a song for us?");
				break;

			case "skill_instrument":
				Msg("Instrument playing skill?<br/>You should ask Nele at the town square about that.<br/>Have you seen him before? He wears a robe and a hat...<br/>He's not too hard to spot.");
				break;

			case "skill_tailoring":
				Msg("The Tailoring skill?<br/>Hmmm... For that, you may want to go to the Clothing Shop<br/>and ask Ailionoa.");
				Msg("Head straight in the direction of the Auditorium,<br/>then take the road that appears midway through.");
				Msg("...Don't forget, the shop is called<br/>Tre'imhse Cairde!!");
				break;

			case "skill_gathering":
				Msg("There are times where I run out of ingredients.<br/>That's when I wish someone would drop by<br/>and gather up some ingredients for us.");
				Msg("What do you think? If you have some time,<br/>how about doing some part-time work for our restaurant?");
				Msg("Sign up for one, and I'll look for some things to do here. Hehe.");
				break;

			case "square":
				Msg("The town square is right there.<br/>People seem to want to make that the meeting place,<br/>so the place is always crowded.<br/>You should check it out sometime.");
				break;

			case "pool":
				Msg("Please don't tell me you see the lake as a reservoir.<br/>Well, if it's your first time here, then that's understandable, I guess.");
				break;

			case "farmland":
				Msg("You may need to walk to the outer edges of this town.<br/>And for some reason, there's a lot of cornfield around the area...");
				break;

			case "windmill":
				Msg("...I think you got the wrong information...");
				break;

			case "shop_headman":
				Msg("Hmmm... a town this big<br/>usually just has an office.");
				Msg("...and this place is ruled by the Lord.");
				break;

			case "temple":
				Msg("Church...<br/>Church...<br/>You need to cross the bridge that heads to the lake...");
				Msg("If you get there please say hello to<br/>Senior Priest Wyllow and Priest James for me!!");
				break;

			case "school":
				Msg("A School...?<br/>There used to be one before, but it's gone now.");
				Msg("...For some reason, the number of students kept declining...");
				Msg("...Don't tell me, just because I work here,<br/>I look like someone that didn't study all that much.");
				break;

			case "skill_campfire":
				Msg("The Campfire skill?<br/>...I feel like you're picking on me because I'm just a kid...<br/>Honestly, that makes me pretty upset, <username/>!");
				break;

			case "shop_restaurant":
				Msg("Yes, it's right here.<br/>It's called Loch Lios, meaning<br/>The garden of the lake.<br/>Doesn't that sound cool?");
				break;

			case "shop_armory":
				Msg("The Weapons shop?<br/>Oh, you mean Osla's place?<br/>Are you going to buy a weapon there or have one of yours repaired?");
				Msg("That's a little surprising!<br/><username/>, you don't look like someone that'd wield a weapon...");
				break;

			case "shop_cloth":
				Msg("Yes, you see Tre'imhse Cairde? That's the Clothing Shop.<br/>It's located right behind the Auditorium.");
				Msg("If you are going there, then I recommend talking to Ailionoa.<br/>She's very sensitive,<br/>so you may want to choose your words carefully.");
				break;

			case "shop_goverment_office":
				Msg("Can't really call that a town office, but...<br/>head up north, and you'll find an island where the town Lord stays.");
				Msg("Yes, that big building you see from afar.<br/>You do recognize the castle when you see one, right?");
				Msg("Once you get there, you'll see Aodhan.<br/>He's in charge of the security in this town,<br/>so it'll help you if you remember his face.");
				Msg("Also... I can't really trust this person, but<br/>if you ever lose an item, then you should see Galvin at the Observatory.<br/>I hear that he sometimes loses a lost item...<br/>Oh well, hopefully that doesn't happen to your item.");
				break;

			case "complicity":
				Msg("...You don't think.. I'm like that, do you?");
				break;

			case "tir_na_nog":
				Msg("...You must be talking about the legendary paradise.<br/>But, if such a place really exists, then<br/>do our lives in this place right now<br/>have any meaning to it?");
				Msg("...Ah, I didn't come up with that.<br/>Jocelin did.");
				break;

			case "EG_C2_Unknown_Continet":
				Msg("Ahh. I was wondering what you were talking about.<br/>It sure is great discoving an uncharted land, but<br/>shouldn't you be making sure that you have completed all the tasks for today first?");
				Msg("I've been noticing that a lot of people are playing hooky on<br/>their part-time jobs.");
				break;

			case "EG_C2_HowTo_Shipboard":
				Msg("If there really is a harbor, I'd love to go see it myself...<br/>but aren't you scared?<br/>Well first and foremost, I don't even know how to swim.");
				break;

			case "breast":
				Msg("Mmm, if the \"chest\" that I thought of was correct,<br/><username/> is a very rude person...<br/>I'm disappointed!");
				break;

			default:
				RndFavorMsg(
					"...I think it's something you should talk about with someone else.",
					"Seriously, this may take up the whole working hours...<br/>Can we talk about this later?",
					"...",
					"...I think it's something you should talk about with someone else.",
					"I'm not really free right now...<br/>so if you keep asking me questions, then when can I work?",
					"That topic's just not that interesting to me.",
					"Hmmm? I don't know anything about that.",
					"...?",
					"I don't know anything about it...<br/>and I don't really want to know anything about it.",
					"I know nothing about it.<br/><username/>, if you know so much about it, then can you explain it to me?",
					"How about talking about something else, <username/>.",
					"...Since you're already at the restaurant, you should order some food!",
					"I can't talk about stuff I don't know.<br/><username/>, you understand, right?"
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
					"This... is from you, <username/>?<br/>Thanks!",
					"Since you're giving me this,<br/>are you saying that you're interested in me?<br/>Hahaha, I was just kidding, yet you're turning red.",
					"Wow, I must be popular too!<br/>Thank you!!!",
					"I may have to be nicer to you now.<br/>Thank you for the present!",
					"Hey, is that a present?<br/>Hahaha, thanks  for this!",
					"Wow, it's a present.<br/>That's for me? Thanks!!"
				);
				break;
		}
	}
}

public class ShenaShop : NpcShopScript
{
	public override void Setup()
	{
		Add("Cooking Ingredients", 50004);     // Bread
		Add("Cooking Ingredients", 50002);     // Slice of Cheese
		Add("Cooking Ingredients", 50220);     // Corn Powder
		Add("Cooking Ingredients", 50206);     // Chocolate
		Add("Cooking Ingredients", 50217);     // Celery
		Add("Cooking Ingredients", 50217);     // Basil x1
		Add("Cooking Ingredients", 50217, 5);  // Basil x5
		Add("Cooking Ingredients", 50217, 10); // Basil x10
		Add("Cooking Ingredients", 50217, 20); // Basil x20
		Add("Cooking Ingredients", 50218);     // Tomato x1
		Add("Cooking Ingredients", 50218, 5);  // Tomato x5
		Add("Cooking Ingredients", 50218, 10); // Tomato x10
		Add("Cooking Ingredients", 50218, 20); // Tomato x20
		Add("Cooking Ingredients", 50045);     // Pine Nut
		Add("Cooking Ingredients", 50047);     // Camellia Seeds
		Add("Cooking Ingredients", 50111);     // Carrot x1
		Add("Cooking Ingredients", 50111, 5);  // Carrot x5
		Add("Cooking Ingredients", 50111, 10); // Carrot x10
		Add("Cooking Ingredients", 50111, 20); // Carrot x20
		Add("Cooking Ingredients", 50018);     // Baking Chocolate x1
		Add("Cooking Ingredients", 50018, 5);  // Baking Chocolate x5
		Add("Cooking Ingredients", 50018, 10); // Baking Chocolate x10
		Add("Cooking Ingredients", 50018, 20); // Baking Chocolate x20
		Add("Cooking Ingredients", 50114);     // Garlic x1
		Add("Cooking Ingredients", 50114, 5);  // Garlic x5
		Add("Cooking Ingredients", 50114, 10); // Garlic x10
		Add("Cooking Ingredients", 50114, 20); // Garlic x20
		Add("Cooking Ingredients", 50127);     // Shrimp x1
		Add("Cooking Ingredients", 50127, 5);  // Shrimp x5
		Add("Cooking Ingredients", 50127, 10); // Shrimp x10
		Add("Cooking Ingredients", 50127, 20); // Shrimp x20
		Add("Cooking Ingredients", 50131);     // Sugar x1
		Add("Cooking Ingredients", 50131, 5);  // Sugar x5
		Add("Cooking Ingredients", 50131, 10); // Sugar x10
		Add("Cooking Ingredients", 50131, 20); // Sugar x20
		Add("Cooking Ingredients", 50132);     // Salt x1
		Add("Cooking Ingredients", 50132, 5);  // Salt x5
		Add("Cooking Ingredients", 50132, 10); // Salt x10
		Add("Cooking Ingredients", 50132, 20); // Salt x20
		Add("Cooking Ingredients", 50148);     // Yeast x1
		Add("Cooking Ingredients", 50148, 5);  // Yeast x5
		Add("Cooking Ingredients", 50148, 10); // Yeast x10
		Add("Cooking Ingredients", 50148, 20); // Yeast x20
		Add("Cooking Ingredients", 50153);     // Deep Fry Batter x1
		Add("Cooking Ingredients", 50153, 5);  // Deep Fry Batter x5
		Add("Cooking Ingredients", 50153, 10); // Deep Fry Batter x10
		Add("Cooking Ingredients", 50153, 20); // Deep Fry Batter x20
		Add("Cooking Ingredients", 50156);     // Pepper x1
		Add("Cooking Ingredients", 50156, 5);  // Pepper x5
		Add("Cooking Ingredients", 50156, 10); // Pepper x10
		Add("Cooking Ingredients", 50156, 20); // Pepper x20
		Add("Cooking Ingredients", 50046);     // Juniper Berry
		Add("Cooking Ingredients", 50112);     // Strawberry x1
		Add("Cooking Ingredients", 50112, 5);  // Strawberry x5
		Add("Cooking Ingredients", 50112, 10); // Strawberry x10
		Add("Cooking Ingredients", 50112, 20); // Strawberry x20
		Add("Cooking Ingredients", 50121);     // Butter x1
		Add("Cooking Ingredients", 50121, 5);  // Butter x5
		Add("Cooking Ingredients", 50121, 10); // Butter x10
		Add("Cooking Ingredients", 50121, 20); // Butter x20
		Add("Cooking Ingredients", 50142);     // Onion x1
		Add("Cooking Ingredients", 50142, 5);  // Onion x5
		Add("Cooking Ingredients", 50142, 10); // Onion x10
		Add("Cooking Ingredients", 50142, 20); // Onion x20
		Add("Cooking Ingredients", 50108);     // Chicken Wings x1
		Add("Cooking Ingredients", 50108, 5);  // Chicken Wings x5
		Add("Cooking Ingredients", 50108, 10); // Chicken Wings x10
		Add("Cooking Ingredients", 50108, 20); // Chicken Wings x20
		Add("Cooking Ingredients", 50130);     // Whipped Cream x1
		Add("Cooking Ingredients", 50130, 5);  // Whipped Cream x5
		Add("Cooking Ingredients", 50130, 10); // Whipped Cream x10
		Add("Cooking Ingredients", 50130, 20); // Whipped Cream x20
		Add("Cooking Ingredients", 50186);     // Red Pepper Powder x1
		Add("Cooking Ingredients", 50186, 5);  // Red Pepper Powder x5
		Add("Cooking Ingredients", 50186, 10); // Red Pepper Powder x10
		Add("Cooking Ingredients", 50186, 20); // Red Pepper Powder x20
		Add("Cooking Ingredients", 50005);     // Large Meat
		Add("Cooking Ingredients", 50001);     // Big Lump of Cheese
		Add("Cooking Ingredients", 50135);     // Rice x1
		Add("Cooking Ingredients", 50135, 5);  // Rice x5
		Add("Cooking Ingredients", 50135, 10); // Rice x10
		Add("Cooking Ingredients", 50135, 20); // Rice x20
		Add("Cooking Ingredients", 50138);     // Cabbage x1
		Add("Cooking Ingredients", 50138, 5);  // Cabbage x5
		Add("Cooking Ingredients", 50138, 10); // Cabbage x10
		Add("Cooking Ingredients", 50138, 20); // Cabbage x20
		Add("Cooking Ingredients", 50139);     // Button Mushroom x1
		Add("Cooking Ingredients", 50139, 5);  // Button Mushroom x5
		Add("Cooking Ingredients", 50139, 10); // Button Mushroom x10
		Add("Cooking Ingredients", 50139, 20); // Button Mushroom x20
		Add("Cooking Ingredients", 50145);     // Olive Oil x1
		Add("Cooking Ingredients", 50145, 5);  // Olive Oil x5
		Add("Cooking Ingredients", 50145, 10); // Olive Oil x10
		Add("Cooking Ingredients", 50145, 20); // Olive Oil x20
		Add("Cooking Ingredients", 50187);     // Lemon x1
		Add("Cooking Ingredients", 50187, 5);  // Lemon x5
		Add("Cooking Ingredients", 50187, 10); // Lemon x10
		Add("Cooking Ingredients", 50187, 20); // Lemon x20
		Add("Cooking Ingredients", 50118);     // Orange x1
		Add("Cooking Ingredients", 50118, 5);  // Orange x5
		Add("Cooking Ingredients", 50118, 10); // Orange x10
		Add("Cooking Ingredients", 50118, 20); // Orange x20
		Add("Cooking Ingredients", 50118);     // Thyme x1
		Add("Cooking Ingredients", 50118, 5);  // Thyme x5
		Add("Cooking Ingredients", 50118, 10); // Thyme x10
		Add("Cooking Ingredients", 50118, 20); // Thyme x20
		Add("Cooking Ingredients", 50421);     // Pecan x1
		Add("Cooking Ingredients", 50421, 5);  // Pecan x5
		Add("Cooking Ingredients", 50421, 10); // Pecan x10
		Add("Cooking Ingredients", 50421, 20); // Pecan x20
		Add("Cooking Ingredients", 50426);     // Peanuts x1
		Add("Cooking Ingredients", 50426, 5);  // Peanuts x5
		Add("Cooking Ingredients", 50426, 10); // Peanuts x10
		Add("Cooking Ingredients", 50426, 20); // Peanuts x20
		Add("Cooking Ingredients", 50123);     // Roasted Bacon
		Add("Cooking Ingredients", 50134);     // Sliced Bread
		Add("Cooking Ingredients", 50133);     // Beef
		Add("Cooking Ingredients", 50122);     // Bacon x1
		Add("Cooking Ingredients", 50122, 5);  // Bacon x5
		Add("Cooking Ingredients", 50122, 10); // Bacon x10
		Add("Cooking Ingredients", 50122, 20); // Bacon x20
		Add("Cooking Ingredients", 50430);     // Grapes x1
		Add("Cooking Ingredients", 50430, 5);  // Grapes x5
		Add("Cooking Ingredients", 50430, 10); // Grapes x10
		Add("Cooking Ingredients", 50430, 20); // Grapes x20
		Add("Cooking Ingredients", 50120);     // Steamed Rice
		Add("Cooking Ingredients", 50102);     // Potato Salad
		Add("Cooking Ingredients", 50431);     // Ripe Pumpkin x1
		Add("Cooking Ingredients", 50431, 5);  // Ripe Pumpkin x5
		Add("Cooking Ingredients", 50431, 10); // Ripe Pumpkin x10
		Add("Cooking Ingredients", 50431, 20); // Ripe Pumpkin x20
		Add("Cooking Ingredients", 50006);     // Sliced Meat x1
		Add("Cooking Ingredients", 50006, 5);  // Sliced Meat x5
		Add("Cooking Ingredients", 50006, 10); // Sliced Meat x10
		Add("Cooking Ingredients", 50006, 20); // Sliced Meat x2
		Add("Cooking Ingredients", 50104);     // Egg Salad
		Add("Cooking Ingredients", 50101);     // Potato Egg Salad

		Add("Event"); // Empty
	}
}