//--- Aura Script -----------------------------------------------------------
// Caitin
//--- Description -----------------------------------------------------------
// Grocer - manages the Tir Chonaill grocery shop
//---------------------------------------------------------------------------

public class CaitinBaseScript : NpcScript
{
	public override void Load()
	{
		SetRace(10001);
		SetName("_caitin");
		SetBody(height: 1.03f);
		SetFace(skinColor: 15, eyeType: 82, eyeColor: 27, mouthType: 43);
		SetStand("human/female/anim/female_natural_stand_npc_Caitin_new");
		SetLocation(5, 1831, 1801, 59);

		EquipItem(Pocket.Face, 3900, 0x00F3B14E, 0x00FBB8AC, 0x00BF921E);
		EquipItem(Pocket.Hair, 3142, 0x00723A2B, 0x00723A2B, 0x00723A2B);
		EquipItem(Pocket.Armor, 15654, 0x006A9050, 0x00F4D6A9, 0x002A2A2A);
		EquipItem(Pocket.Shoe, 17284, 0x002A2A2A, 0x00000000, 0x00000000);

		AddGreeting(0, "I think this is the first time we've met. Nice to meet you!");
		AddGreeting(1, "We've met before, right? Nice to meet you.");
		//AddGreeting(2, "Good to see you again, <username/>."); // Not 100% sure

		AddPhrase("*Yawn*");
		AddPhrase("Hmm... Sales are low today... That isn't good.");
		AddPhrase("I am a little tired.");
		AddPhrase("I have to finish these bills... I'm already behind schedule.");
		AddPhrase("I must have had a bad dream.");
		AddPhrase("It's about time for customers to start coming in.");
		AddPhrase("My body feels stiff all over.");
		AddPhrase("These vegetables are spoiling...");
	}

	protected override async Task Talk()
	{
		await Intro(
			"A young lady pouring flour into a bowl smiles at you as you enter.",
			"Her round face is adorably plump and her eyes shine brightly.",
			"As she wipes her hands and walks toward you, you detect the faint scent of cookie dough and flowers."
		);

		Msg("What can I do for you?", Button("Start a Conversation", "@talk"), Button("Shop", "@shop"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());
				if (Player.Titles.SelectedTitle == 11002)
					Msg("Wow. You're the Guardian of Erinn? My, what an honor!<br/>I still remember the first day you came here...<br/>Feels just like yesterday!<br/>People will remember your name years to come...");
				await Conversation();
				break;

			case "@shop":
				Msg("Welcome to the Grocery Store.<br/>There is a variety of fresh food and ingredients for you to choose from.");
				OpenShop("CaitinShop");
				return;
		}

		End();
	}

	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				GiveKeyword("shop_grocery");
				Msg("My grandmother named me.<br/>I work here at the Grocery Store, so I know one important thing.<br/>You have to eat to survive!<br/>Food helps you regain your Stamina.");
				Msg("That doesn't mean you can eat just anything.<br/>You shouldn't have too much greasy food<br/>because you could gain a lot of weight.");
				Msg("Huh? You have food with you but don't know how to eat it?<br/>Okay, open the Inventory and right-click on the food.<br/>Then, click \"Use\" to eat.<br/>If you have bread in your Inventory, and your Stamina is low,<br/>try eating it now.");
				ModifyRelation(Random(2), 0, Random(2));
				break;

			case "rumor":
				GiveKeyword("brook");
				Msg("Do you know anything about the Adelia Stream?<br/>The river near the Windmill is the Adelia Stream.");
				ModifyRelation(Random(2), 0, Random(2));
				break;

			case "about_skill":
				if (!HasSkill(SkillId.Cooking, SkillRank.RF))
				{
					Msg("Hehe, skills?<br/>Well, how about cooking? Do you enjoy cooking?<br/>Oh, but that doesn't mean I can teach you the Cooking skill.<br/>I'm not good enough to teach you...<br/>Though I can share my experiences of cooking if you like.", Button("OK", "@skilleffect01"), Button("No, thanks", "@skilleffect02"));
					switch (await Select())
					{
						case "@skilleffect01":
							Msg("Cooking isn't just for filling up your stomach.<br/>A well-made meal can make you happy, even if it's just temporary.<br/>You know, you can enhance your HP, or increase your Intelligence or Dexterity,<br/>by eating a Fruit Salad, which isn't hard to make.");
							Msg("Drinking Strawberry Milk enhances your HP or your Intelligence.<br/>These effects change according to what food you eat.<br/>The better the quality of the food, the better its effects.<br/>Also, the effects of certain delicacies last longer.");
							Msg("A high rank in the Cooking skill is needed to make such marvelous delicacies.<br/>The meals here have excellent effects because I do the cooking myself.<br/>Customers are always welcome! Hehe...");
							break;

						case "@skilleffect02":
							Msg("Sorry, <username/>, but I can't.<br/>If I share my secret recipes with you,<br/>then all the other travelers around here will know them too...<br/>That means my sales will drop.");
							Msg("Then I won't have money to pay for my mother's medicine.<br/>I hope you understand.");
							break;
					}
				}
				else
				{
					Msg("I want to bake some bread for Duncan but I'm out of wheat flour.<br/>Could you lend me a hand? In return, I'll tell you how to make wheat flour.<br/>Wait outside and an owl will deliver a message with details on making wheat flour.");
					// Quest Info: http://pastebin.com/jNaSzq5L
					// Quest Item: http://pastebin.com/pnGXni41
				}
				break;

			case "about_arbeit":
				Msg("I'm sorry... This isn't the right time for a part-time job.<br/>Please come back later.");
				break;

			case "shop_misc":
				Msg("The General Shop? It's the building in front of here. You must have missed the shop's sign.<br/>Hehe.... Nothing to be embarrassed about. Happens all the time.");
				break;

			case "shop_grocery":
				Msg("Yes, this is the Grocery Store. How many times are you going to ask?<br/>Are you implying that I look like a Grocery Store owner?  That I'm...fat?");
				Msg("...<br/>I'm sorry... That wasn't fair.<br/>Please. Come by anytime you need something to eat.<br/>Open the Trade window to take a look around.");
				break;

			case "shop_healing":
				Msg("Eat well. Stay healthy. Then you won't need the Healer's help.<br/>A nutritious, home-cooked meal beats medicine any day!<br/>Would Dilys be upset about what I just said? Hehe.");
				break;

			case "shop_inn":
				Msg("Yes, I cook for guests at the Inn.<br/>You should order food from the Inn<br/>instead of ordering it here, since there are hardly any seats here.");
				break;

			case "shop_bank":
				Msg("Sometimes people forget what they deposited at the Bank.<br/>Please keep that in mind!<br/>I say this because It often happens to me... Hehe.<br/>Bebhinn never reminds you of the items either... Sigh...");
				break;

			case "shop_smith":
				GiveKeyword("school");
				Msg("Ferghus at the Blacksmith's Shop loves to drink.<br/>He often hangs out with Ranald from the school for a drink.");
				break;

			case "skill_range":
				Msg("That's used for long-ranged attacks, right?<br/>That's about all I know.");
				break;

			case "skill_instrument":
				Msg("Playing an instrument?<br/>I saw Priestess Endelyon play an organ at the Church before.<br/>Why don't you go and talk to her?");
				break;

			case "skill_composing":
				GiveKeyword("shop_bank");
				Msg("I'm not sure...<br/>But Bebhinn is the one you should ask.<br/>She knows all the rumors.<br/>She'll definitely know who's the best at composing around here.");
				Msg("Bebhinn works at the Bank.<br/>It's the building to the left.");
				break;

			case "skill_tailoring":
				GiveKeyword("shop_inn");
				Msg("You just need a Tailoring Kit to use the Tailoring skill.<br/>Anyone can use the skill as long as they have a kit with them.<br/>It's not difficult at all.");
				Msg("You will, however, need various materials,<br/>such as fabric and sewing patterns to make anything.<br/>Ask Nora at the Inn for materials.");
				break;

			case "skill_magnum_shot":
				GiveKeyword("school");
				Msg("Sounds like a combat skill to me.<br/>Why don't you ask Ranald at the School?<br/>He teaches combat skills.");
				break;

			case "skill_counter_attack":
				GiveKeyword("school");
				Msg("Melee Counterattack? Sounds like a combat skill to me.<br/>Why don't you ask Ranald at the School?<br/>He teaches combat skills.");
				break;

			case "skill_smash":
				GiveKeyword("school");
				Msg("The Smash skill... Sounds like a combat skill to me.<br/>Why don't you ask Ranald at the School?<br/>He teaches combat skills.");
				break;

			case "skill_gathering":
				Msg("Sometimes, I have to venture outside town to pick wild plants and fruit.<br/>To be honest, it's quite scary to go alone.");
				Msg("The most important thing about the Gathering skill is the tool that you use.<br/>You can't gather anything with your bare hands.<br/>Without a tool, you'll only be able to pick up branches, berries, and nails.");
				Msg("You need a Gathering Axe for firewood, a Gathering Knife for wool,<br/>and a Sickle for barley or wheat.<br/>Make sense?");
				break;

			case "square":
				Msg("The Square? It's right in front of you!<br/>...<br/>...<br/>Um, do you have something else to say?");
				break;

			case "pool":
				Msg("The reservoir is located just behind this building.<br/>Please be careful not to fall into it!");
				break;

			case "farmland":
				Msg("The farmland lies in front of the School.<br/>It seems that many mice and birds<br/>are ruining the crops in the farmland.<br/>Residents are complaining about the quality of the groceries.");
				break;

			case "windmill":
				Msg("Isn't Alissa cute? Hehe...<br/>If you need to grind anything, you should go to the Windmill.<br/>I go there sometimes to grind my own grain.<br/>Maybe we can go together sometime.");
				Msg("Grinding grain requires strong shoulders... Hehe.<br/>What? I should go alone because I need the exercise?<br/>I know you're joking...but you crossed the line.  How rude!");
				break;

			case "brook":
				Msg("The Adelia Stream?<br/>Go straight down the road just out front.<br/>It's easy to find because it's near the Inn.<br/>I used to take baths there when I was young.<br/>Hey!  What kind of thoughts are you having?!");
				break;

			case "shop_headman":
				Msg("Chief Duncan's house is just over the hill.<br/>Please give him my best regards.");
				Msg("You should go visit him<br/>especially if you are a newcomer.");
				break;

			case "temple":
				Msg("You can get to the Church by taking the downhill road on the right.<br/>You will find Priest Meven and Priestess Endelyon there.<br/>Priestess Endelyon is looking for people who are willing to work at the Church.<br/>If you need a job, please go and ask her.");
				break;

			case "school":
				Msg("You'll find the School a bit further down from the Church.<br/>You can't miss it because the building is quite big<br/>and there is a strange plant growing near it.");
				break;

			case "shop_restaurant":
				Msg("A Restaurant? Hmm. We don't have one in this town.<br/>Some people dine at the Inn...<br/>But most just buy food here and find somewhere comfortable to eat it.<br/>Why don't you try that?");
				break;

			case "shop_armory":
				GiveKeyword("shop_smith");
				Msg("A Weapons Shop?<br/>I don't think there is a shop just for weapons...<br/>Oh yea!  Go to Ferghus's Blacksmith.<br/>I saw him making all sorts of swords and hammers recently.");
				break;

			case "shop_cloth":
				GiveKeyword("shop_misc");
				Msg("I usually make my own clothes<br/>but sometimes I purchase them at Malcolm's General Shop.<br/>Do you need new clothes?");
				break;

			case "shop_bookstore":
				Msg("Everyone is so busy with work,<br/>that it's almost impossible to sit down and just read.<br/>The only books you can buy around here are probably<br/>the magic books that Lassar sells at the School.");
				Msg("Some villagers give books as gifts to travelers passing through this village.<br/>Could be tempting if you're a bookworm.");
				break;

			case "shop_goverment_office":
				GiveKeyword("shop_headman");
				Msg("The Town Office? Huh?<br/>Er, if you are looking for someone who takes care of town affairs,<br/>go and see the Chief.");
				break;

			case "graveyard":
				Msg("The graveyard is over that hill behind Chief Duncan's House.<br/>Don't be surprised to see big spiders there.");
				Msg("Oh! Be careful because those spiders attack sometimes.");
				break;

			case "skill_fishing":
				Msg("Oh, fishing?<br/>The fish you catch can be sold in stores.<br/>Even it doesn't go well,<br/>you ought to be able to earn enough to pay for the bait at least.");
				break;

			case "lute":
				Msg("Lute...? Do you mean that small stringed instrument?<br/>I saw Malcolm selling them at the General Shop.<br/>If you plan to buy one, the General Shop is the place to go.");
				break;

			default:
				RndMsg(
					"Can we change the subject?",
					"I don't have much to say about that.",
					"Never heard of that before.",
					"Well, I really don't know.",
					"Did you ask everyone else the same question?"
				);
				ModifyRelation(0, 0, Random(2));
				break;
		}
	}
}

public class CaitinShop : NpcShopScript
{
	public override void Setup()
	{
		Add("Grocery", 50001);     // Big Lump of Cheese
		Add("Grocery", 50002);     // Slice of Cheese
		Add("Grocery", 50004);     // Bread
		Add("Grocery", 50005);     // Large Meat
		Add("Grocery", 50006, 1);  // Sliced Meat x1
		Add("Grocery", 50006, 5);  // Sliced Meat x5
		Add("Grocery", 50006, 10); // Sliced Meat x10
		Add("Grocery", 50006, 20); // Sliced Meat x20
		Add("Grocery", 50018, 1);  // Baking Chocolate x1
		Add("Grocery", 50018, 5);  // Baking Chocolate x5
		Add("Grocery", 50018, 10); // Baking Chocolate x10
		Add("Grocery", 50018, 20); // Baking Chocolate x20
		Add("Grocery", 50045);     // Pine Nut
		Add("Grocery", 50046);     // Juniper Berry
		Add("Grocery", 50047);     // Camellia Seeds
		Add("Grocery", 50101);     // Potato & Egg Salad
		Add("Grocery", 50102);     // Potato Salad
		Add("Grocery", 50104);     // Egg Salad
		Add("Grocery", 50108, 1);  // Chicken Wings x1
		Add("Grocery", 50108, 5);  // Chicken Wings x5
		Add("Grocery", 50108, 10); // Chicken Wings x10
		Add("Grocery", 50108, 20); // Chicken Wings x20
		Add("Grocery", 50111, 1);  // Carrot x1
		Add("Grocery", 50111, 5);  // Carrot x5
		Add("Grocery", 50111, 10); // Carrot x10
		Add("Grocery", 50111, 20); // Carrot x20
		Add("Grocery", 50112, 1);  // Strawberry x1
		Add("Grocery", 50112, 5);  // Strawberry x5
		Add("Grocery", 50112, 10); // Strawberry x10
		Add("Grocery", 50112, 20); // Strawberry x20
		Add("Grocery", 50114, 1);  // Garlic x1
		Add("Grocery", 50114, 5);  // Garlic x5
		Add("Grocery", 50114, 10); // Garlic x10
		Add("Grocery", 50114, 20); // Garlic x20
		Add("Grocery", 50120);     // Steamed Rice
		Add("Grocery", 50121, 1);  // Butter x1
		Add("Grocery", 50121, 5);  // Butter x5
		Add("Grocery", 50121, 10); // Butter x10
		Add("Grocery", 50121, 20); // Butter x20
		Add("Grocery", 50122, 1);  // Bacon x1
		Add("Grocery", 50122, 5);  // Bacon x5
		Add("Grocery", 50122, 10); // Bacon x10
		Add("Grocery", 50122, 20); // Bacon x20
		Add("Grocery", 50123);     // Roasted Bacon
		Add("Grocery", 50127, 1);  // Shrimp x1
		Add("Grocery", 50127, 5);  // Shrimp x5
		Add("Grocery", 50127, 10); // Shrimp x10
		Add("Grocery", 50127, 20); // Shrimp x20
		Add("Grocery", 50130, 1);  // Whipped Cream x1
		Add("Grocery", 50130, 5);  // Whipped Cream x5
		Add("Grocery", 50130, 10); // Whipped Cream x10
		Add("Grocery", 50130, 20); // Whipped Cream x20
		Add("Grocery", 50131, 1);  // Sugar x1
		Add("Grocery", 50131, 5);  // Sugar x5
		Add("Grocery", 50131, 10); // Sugar x10
		Add("Grocery", 50131, 20); // Sugar x20
		Add("Grocery", 50132, 1);  // Salt x1
		Add("Grocery", 50132, 5);  // Salt x5
		Add("Grocery", 50132, 10); // Salt x10
		Add("Grocery", 50132, 20); // Salt x20
		Add("Grocery", 50133);     // Beef
		Add("Grocery", 50134);     // Sliced Bread
		Add("Grocery", 50135, 1);  // Rice x1
		Add("Grocery", 50135, 5);  // Rice x5
		Add("Grocery", 50135, 10); // Rice x10
		Add("Grocery", 50135, 20); // Rice x20
		Add("Grocery", 50138, 1);  // Cabbage x1
		Add("Grocery", 50138, 5);  // Cabbage x5
		Add("Grocery", 50138, 10); // Cabbage x10
		Add("Grocery", 50138, 20); // Cabbage x20
		Add("Grocery", 50139, 1);  // Button Mushroom x1
		Add("Grocery", 50139, 5);  // Button Mushroom x5
		Add("Grocery", 50139, 10); // Button Mushroom x10
		Add("Grocery", 50139, 20); // Button Mushroom x20
		Add("Grocery", 50142, 1);  // Onion x1
		Add("Grocery", 50142, 5);  // Onion x5
		Add("Grocery", 50142, 10); // Onion x10
		Add("Grocery", 50142, 20); // Onion x20
		Add("Grocery", 50145, 1);  // Olive Oil x1
		Add("Grocery", 50145, 5);  // Olive Oil x5
		Add("Grocery", 50145, 10); // Olive Oil x10
		Add("Grocery", 50145, 20); // Olive Oil x20
		Add("Grocery", 50148, 1);  // Yeast x1
		Add("Grocery", 50148, 5);  // Yeast x5
		Add("Grocery", 50148, 10); // Yeast x10
		Add("Grocery", 50148, 20); // Yeast x20
		Add("Grocery", 50153, 1);  // Deep Fry Batter x1
		Add("Grocery", 50153, 5);  // Deep Fry Batter x5
		Add("Grocery", 50153, 10); // Deep Fry Batter x10
		Add("Grocery", 50153, 20); // Deep Fry Batter x20
		Add("Grocery", 50156, 1);  // Pepper x1
		Add("Grocery", 50156, 5);  // Pepper x5
		Add("Grocery", 50156, 10); // Pepper x10
		Add("Grocery", 50156, 20); // Pepper x20
		Add("Grocery", 50185, 1);  // Curry Powder x1
		Add("Grocery", 50185, 5);  // Curry Powder x5
		Add("Grocery", 50185, 10); // Curry Powder x10
		Add("Grocery", 50185, 20); // Curry Powder x20
		Add("Grocery", 50186, 1);  // Red Pepper Powder x1
		Add("Grocery", 50186, 5);  // Red Pepper Powder x5
		Add("Grocery", 50186, 10); // Red Pepper Powder x10
		Add("Grocery", 50186, 20); // Red Pepper Powder x20
		Add("Grocery", 50187, 1);  // Lemon x1
		Add("Grocery", 50187, 5);  // Lemon x5
		Add("Grocery", 50187, 10); // Lemon x10
		Add("Grocery", 50187, 20); // Lemon x20
		Add("Grocery", 50188, 1);  // Orange x1
		Add("Grocery", 50188, 5);  // Orange x5
		Add("Grocery", 50188, 10); // Orange x10
		Add("Grocery", 50188, 20); // Orange x20
		Add("Grocery", 50189, 1);  // Thyme x1
		Add("Grocery", 50189, 5);  // Thyme x5
		Add("Grocery", 50189, 10); // Thyme x10
		Add("Grocery", 50189, 20); // Thyme x20
		Add("Grocery", 50206);     // Chocolate
		Add("Grocery", 50217);     // Celery
		Add("Grocery", 50218, 1);  // Tomato x1
		Add("Grocery", 50218, 5);  // Tomato x5
		Add("Grocery", 50218, 10); // Tomato x10
		Add("Grocery", 50218, 20); // Tomato x20
		Add("Grocery", 50219, 1);  // Basil x1
		Add("Grocery", 50219, 5);  // Basil x5
		Add("Grocery", 50219, 10); // Basil x10
		Add("Grocery", 50219, 20); // Basil x20
		Add("Grocery", 50220);     // Corn Powder
		Add("Grocery", 50421, 1);  // Pecan x1
		Add("Grocery", 50421, 5);  // Pecan x5
		Add("Grocery", 50421, 10); // Pecan x10
		Add("Grocery", 50421, 20); // Pecan x20
		Add("Grocery", 50426, 1);  // Peanuts x1
		Add("Grocery", 50426, 5);  // Peanuts x5
		Add("Grocery", 50426, 10); // Peanuts x10
		Add("Grocery", 50426, 20); // Peanuts x20
		Add("Grocery", 50430, 1);  // Grapes x1
		Add("Grocery", 50430, 5);  // Grapes x5
		Add("Grocery", 50430, 10); // Grapes x10
		Add("Grocery", 50430, 20); // Grapes x20
		Add("Grocery", 50431, 1);  // Ripe Pumpkin x1
		Add("Grocery", 50431, 5);  // Ripe Pumpkin x5
		Add("Grocery", 50431, 10); // Ripe Pumpkin x10
		Add("Grocery", 50431, 20); // Ripe Pumpkin x20

		Add("Gift", 52010); // Ramen
		Add("Gift", 52019); // Heart Cake
		Add("Gift", 52021); // Slice of Cake
		Add("Gift", 52022); // Wine
		Add("Gift", 52023); // Wild Ginseng

		//AddQuest("Quest", InsertQuestId, 0);   // Collecting Quest [5 Small Gems]
		//AddQuest("Quest", InsertQuestId, 1);   // Collecting Quest [5 Small Green Gems (1 Small Blue Gem reward)]
		//AddQuest("Quest", InsertQuestId, 1);   // Collecting Quest [5 Small Green Gems (20g reward)]
		//AddQuest("Quest", InsertQuestId, 5);   // Collecting Quest [5 Small Blue Gems (1 Small Red Gem reward)]
		//AddQuest("Quest", InsertQuestId, 5);   // Collecting Quest [5 Small Blue Gems (50g reward)]
		//AddQuest("Quest", InsertQuestId, 25);  // Collecting Quest [5 Small Red Gems (1 Small Silver Gem reward)]
		//AddQuest("Quest", InsertQuestId, 25);  // Collecting Quest [5 Small Red Gems](200g reward)]
		//AddQuest("Quest", InsertQuestId, 100); // Collecting Quest [5 Small Silver Gems]

		Add("Event"); // Empty
	}
}