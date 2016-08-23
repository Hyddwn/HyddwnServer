//--- Aura Script -----------------------------------------------------------
// Tracy
//--- Description -----------------------------------------------------------
// The Lumberjack in Ulaid Logging Camp
//---------------------------------------------------------------------------

public class TracyScript : NpcScript
{
	public override void Load()
	{
		SetRace(10002);
		SetName("_tracy");
		SetBody(height: 1.2f, weight: 1.5f, upper: 2f);
		SetFace(skinColor: 19, eyeType: 9, eyeColor: 27);
		SetLocation(16, 22900, 59500, 56);
		SetGiftWeights(beauty: 0, individuality: 2, luxury: 0, toughness: 0, utility: 2, rarity: 2, meaning: 1, adult: 0, maniac: 0, anime: 1, sexy: 1);

		EquipItem(Pocket.Face, 4904, 0x004B4C64, 0x007C1B83, 0x00CE8970);
		EquipItem(Pocket.Hair, 4025, 0x00754C2A, 0x00754C2A, 0x00754C2A);
		EquipItem(Pocket.Armor, 15005, 0x00744D3C, 0x00DDB372, 0x00D6BDA3);
		EquipItem(Pocket.Glove, 16010, 0x00755744, 0x00005B40, 0x009E086C);
		EquipItem(Pocket.Shoe, 17010, 0x00371E00, 0x00000047, 0x00747374);
		EquipItem(Pocket.Head, 18017, 0x00744D3C, 0x00F79622, 0x00BE7781);
		EquipItem(Pocket.RightHand1, 40007, 0x00A7A894, 0x00625F44, 0x00872F92);

		AddPhrase("Gee, it's hot...");
		AddPhrase("I tire out so easily these days...");
		AddPhrase("It's so dull here...");
		AddPhrase("Man, I'm so sweaty...");
		AddPhrase("Oh, my arm...");
		AddPhrase("Oh, my leg...");
		AddPhrase("Oww, my muscles are sore all over.");
		AddPhrase("Phew. Alright. Time to rest!");
		AddPhrase("Should I take a break now?");
		AddPhrase("*Yawn*");
	}

	protected override async Task Talk()
	{
		SetBgm("NPC_Tracy.mp3");

		await Intro(L("This broad-shouldered man holding a wood-cutting axe in his right hand must have gone through a lot of rough times.<br/>He's wearing a cap backwards and his bronzed face is covered with a heavy beard.<br/>Between the wavy strands of his bushy dark brown hair are a pair of bright, playful eyes full of benevolent mischief."));

		Msg("What is it? Come on, spit it out!<br/>I'm a busy man!", Button("Start Conversation", "@talk"), Button("Shop", "@shop"), Button("Modify Item", "@upgrade"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());

				if (Title == 11001)
				{
					Msg("Wow! You rescued the Goddess? You?<br/>Oh, man... That's awesome!");
					Msg("...Not!");
					Msg("Did you think I, Mr. Tracy,<br/>would actually be impressed<br/>if you just showed up with a weird little title");
					Msg("attached to your head?!");
					Msg("I doubt you can even take care of yourself.<br/>Yet, you tell me you rescued the Goddess?<br/>You suck at lying, you know that?");
				}

				await Conversation();
				break;

			case "@shop":
				Msg("You need something?");
				OpenShop("TracyShop");
				return;

			case "@upgrade":
				Msg("Somebody told you that modified items are good, right?<br/>Well, if <username/> needs a favor, I guess I must help.<br/>Show me what you want to modify.<upgrade />");

				while (true)
				{
					var reply = await Select();

					if (!reply.StartsWith("@upgrade:"))
						break;

					var result = Upgrade(reply);
					if (result.Success)
						Msg("Haha... I did it myself, but I have to tell you, man!<br/>This is an excellent modification! It just feels so good!<br/>You got something else to modify?");
					else
						Msg("(Error)");
				}

				Msg("Just ask me if you want something modified, man! Anytime, haha!<upgrade hide='true'/>");
				break;
		}

		End("Goodbye, <npcname/>. I'll see you later!");
	}

	private void Greet()
	{
		if (DoingPtjForOtherNpc())
		{
			Msg(L("You... A part-timer?"));
			Msg(L("You're kidding right?"));
			Msg(L("You like part-time jobs? Are they fun? Well?<br/>Are they fun? I can't believe this!"));
		}
		else if (DoingPtjForNpc())
		{
			Msg(FavorExpression(), L("Hey, what happened to my logs?<br/>Remember, no firewood, no reward. OK?"));
		}
		else if (Memory <= 0)
		{
			Msg(FavorExpression(), L("Hey... I'm <npcname/>, the lumberjack.<br/>When I say hi, I say it right!"));
			Msg(L("What, you have something to say?"));
		}
		else if (Memory == 1)
		{
			Msg(FavorExpression(), L("What was your name?<br/>I think... you were the one snickering at my face before..."));
		}
		else if (Memory == 2)
		{
			Msg(FavorExpression(), L("Ah! It's you! <username/>!<br/>What brings you here?"));
		}
		else if (Memory <= 6)
		{
			Msg(FavorExpression(), L("You must be interested in lumberjacks like me, right?"));
		}
		else
		{
			Msg(FavorExpression(), L("You like this place? I see you around here often."));
		}

		UpdateRelationAfterGreet();
	}

	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				if (Memory == 1)
				{
					Msg("Hey, hey. You're thinking about my name again?<br/>I don't like it myself.");
					Msg("Stop grinning. Don't give me that look any more. It's really disturbing.");
					ModifyRelation(1, 0, 0);
				}
				else
				{
					Msg(FavorExpression(), "Yes, <npcname/> is my name. The lumberjack...<br/>Hey! Why are you giggling while you ask?");
					Msg("...Ha! <username/>... Your name sounds no better than mine.<br/>Actually, yours is even worse!");
					ModifyRelation(Random(2), 0, Random(3));
				}
				break;

			case "rumor":
				Msg("Looks like the people who talked to me before<br/>don't really like my speaking style...<br/>Let me say something. These people just show up and start pestering me with all these silly questions,<br/>and I'm supposed to be polite to them all the time?");
				Msg("What do you think? What?<br/>You don't like how I speak either?");
				ModifyRelation(Random(2), 0, Random(3));
				break;

			case "about_skill":
				if (IsEnabled("Carpentry") && !HasSkill(SkillId.Carpentry) && !HasQuest(20113))
				{
					// Learn skill Carpentry Mastery
					// StartQuest(20113);
					Msg("Interested in Carpentry? It's a skill that lets you<br/>make lumber and bows.<br/>Buy a Lumber Axe, chop a piece of Average Firewood<br/>using that chopping block over there, and talk to me.<br/>I'll teach the secrets to carpentry then.");
				}
				else
				{
					Msg("Why do you continue to pester me about that?<br/>I have nothing more to tell you.");
				}
				break;

			case "shop_misc":
				Msg("Haha... Looking for a general shop in a place like this...<br/>What is it? An emergency?");
				Msg("I guess you need to go to a town to find one.<br/>That gentlemanly Malcolm's to the north,<br/>and the General Shop in Dunbarton is to the south...");
				Msg("It will probably take the same time either way...<br/>If I were you, I'd walk down the slope to the south.");
				break;

			case "shop_grocery":
				Msg("How come people look for a grocery store in a place like this?<br/>What do they look for when they are in town?");
				Msg("If you are really hungry, I've got my lunchbox here...<br/>What do you think? I can give it to you if you want.");
				Msg("...");
				Msg("But I'm not saying it's free.<br/>Since I'm giving you my lunch,<br/>I need to get paid something for it...");
				Msg("So, are you interested? Then press 'Shop'.");
				break;

			case "shop_healing":
				Msg("You can find a Healer's House in any decent town.");
				Msg("But, you know what? If I got hurt,<br/>I wouldn't mind walking a little more<br/>to go to Tir Chonaill to get treated.");
				Msg("The healer girl there, she's really pretty, isn't she? Hehe...");
				break;

			case "shop_inn":
				Msg("Want to go to the Inn?<br/>You don't look that tired...<br/>You've got some business with Nora or Piaras?");
				Msg("Tir Chonaill is in the north. Follow the road up.");
				break;

			case "shop_smith":
				Msg("Blacksmith's Shop?<br/>Walk along the path up to Tir Chonaill.<br/>And say hi to Ferghus for me.");
				Msg("Be careful of the wolves around here...<br/>Ah... nah, forget what I said. You're ugly enough to scare off any monsters.");
				break;

			case "skill_range":
				Msg("You know what, I have a hunter friend...<br/>And this guy never stops bragging about his archery skills...<br/>He says that he can hit the target from afar with a bow or something.");
				Msg("Details are not my business. Hey, I'm an environmentalist!");
				break;

			case "skill_instrument":
				Msg("haha... You are not a complete idiot, after all!<br/>Now I chop down trees here,<br/>but I once used to dream of becoming a musician.");
				Msg("What? You want me to play music with this axe or something?<br/>You have to at least show me an instrument before asking me.");
				break;

			case "skill_composing":
				Msg("That's a skill to make songs.<br/>Any traveler who knows the joys of life<br/>must learn it...");
				Msg("Me?<br/>Hahaha ... Why don't you ask Helene about it?<br/>She's the one who wrote 'Aspiring to a Higher Level of Composition'.<br/>I taught her everything she wrote in that book, you know. Hahaha!");
				break;

			case "skill_tailoring":
				Msg("Tailoring skill?<br/>It's a good skill.");
				Msg("...");
				Msg("What more did you expect?");
				break;

			case "skill_magnum_shot":
				Msg("Magnum Shot skill...<br/>Trefor is quite good at it, you know...");
				Msg("Hmm... Using Magnum Shot with an axe<br/>might be fun.");
				break;

			case "skill_counter_attack":
				Msg("Haha... Ranald told you that?<br/>That you will know when you get hit?");
				Msg("It's... not totally wrong, I guess...<br/>But that's true only for talented people.<br/>If you are no better than mediocre, you'd learn nothing even after getting beat up...<br/>It's a matter of risking your life...");
				Msg("To be honest, a long time ago, Ranald once beat the crap out of me<br/>saying he would teach me a lesson about<br/>something called Counterattack skill...");
				Msg("Anyway, I might have used some rough words against him at the time but...<br/>You'd better watch your language in front of Ranald... Haha...");
				break;

			case "skill_smash":
				Msg("Smash? Go and ask Ranald about it...<br/>Ranald, that creep...<br/>Thinks he's some kind of a famous warrior...<br/>Wearing the same clothes as mine... Man, I don't like him...");
				break;

			case "skill_gathering":
				Msg("Hahaha...<br/>If you have an axe,<br/>that's good enough to enjoy the life of a lumberjack. Just like me!<br/>What do you think? Very tempting, right?");
				Msg("If you actually start to realize the beauty of being a lumberjack,<br/>come and talk to me using the 'Part-Time Jobs' keyword<br/>at the right time.");
				break;

			case "pool":
				Msg("You want to jump into the water?<br/>I see... You are quite dirty and stained. A quick wash won't hurt you at all.");
				Msg("Ah, are you challenging me with your scowl?");
				break;

			case "farmland":
				Msg("Are you interested in farming? More so than in trees?<br/>I won't stop you if you insist...");
				Msg("Yes...<br/>Most of the towns in Erinn<br/>have farmlands nearby.<br/>I guess helping farmers would be good too.");
				break;

			case "windmill":
				Msg("Hehe...<br/>The Windmill up in Tir Chonaill, you know.<br/>Malcolm didn't listen to me<br/>and paid the price.");
				Msg("He made the Windmill frame against the woodgrain<br/>and the frame just broke in half....<br/>The broken Windmill blades flew so far away that<br/>one of them nearly fell here.");
				Msg("Do you get the lesson of my story?<br/>You might be really good at making something,<br/>but you should always listen to the one providing the materials.");
				Msg("I wonder how that cocky kid is doing... Hehe.");
				break;

			case "brook":
				Msg("Curious about Adelia Stream?");
				Msg("You want to play in the water or something?");
				Msg("Nah, first you need... a bath... Man, why don't you go clean yourself first.");
				break;

			case "shop_headman":
				Msg("The Chief's House?<br/>Ah, that stubborn old man.<br/>You mean Duncan, right?<br/>He lives at the highest place in Tir Chonaill... just like a monkey.");
				Msg("...");
				Msg("You came all the way out here just to ask me that?");
				break;

			case "temple":
				Msg("Hmm... You're looking for a church?<br/>Do you have anything to confess?<br/>Let me give you some comfort. Cheer up.");
				Msg("What? What's up with that look? I'm trying to help you, you know?");
				Msg("Humans tend to look for God when they are in trouble,<br/>but once they are OK, they don't even look back, man.<br/>Am I right or wrong?");
				break;

			case "school":
				Msg("School? Well...<br/>It all depends, you know...");
				Msg("Tir Chonaill's School is in the north,<br/>and the one in Dunbarton is in the south.<br/>Just follow the path.");
				Msg("Which reminds me... It's been some time since I last saw that man-beater Ranald...");
				break;

			case "skill_windmill":
				Msg("The Windmill skill is useful<br/>when you are surrounded by enemies,<br/>but it's really distracting...");
				Msg("Once you use it, you don't even know where you are...");
				break;

			case "skill_campfire":
				Msg("Haha...<br/>Most of the people coming to Ulaid Logging Camp<br/>actually show up here to build a campfire,<br/>not to get wood.");
				Msg("After spending a day or two here,<br/>even I get confused about where I am. A logging camp or a holiday campsite?");
				Msg("Haha... I don't mind it as long as there are some good views...");
				break;

			case "shop_restaurant":
				Msg("Restaurant? Are you hungry?<br/>Well, I'm sorry but... it's far from here...<br/>You need to go to a town to find one.");
				Msg("If you need some bread,<br/>I can give you mine for free.<br/>Want to hear about it?", Button("Sure", "@yes"), Button("Not interested", "@no"));

				switch (await Select())
				{
					case "@yes":
						Msg("Awesome! Repeat after me!<br/>[Tough Guy Tracy!]<br/>Say it 100 times and I'll give you a piece of bread.", Button("Tough Guy Tracy!", "@reply1"), Button("Crazy Dude", "@reply2"));

						switch (await Select())
						{
							case "reply1":
								Msg("Wow, did you actually do that?<br/>Your pride is worth... a measly piece of bread? Hahahahaha...");
								break;

							case "reply2":
								Msg("...<p/>Right back at ya!");
								break;
						}
						break;

					case "@no":
						Msg("Haha... Don't bother if you don't like it.<br/>Ah, this bread looks great!");
						break;
				}
				break;

			case "shop_armory":
				Msg("You need to go to the nearest town to find a Weapons Shop...<br/>Who would want to buy a weapon here, so far away from town?");
				Msg("Hmm... Now that you say so,<br/>it could be a good idea to open one here.");
				break;

			case "shop_cloth":
				Msg("A clothing shop?<br/>Of all the clothing shops nearby,<br/>Simon's Boutique in Dunbarton is quite famous.<br/>In fact, the clothes I am wearing right now were made by Simon.");
				Msg("You know what? The clothes Ranald is wearing,<br/>they may look similar to mine. But his are fake.<br/>...");
				Msg("... Why don't you go to Dunbarton?<br/>Tell him I sent you there. He will be really nice to you. Haha...");
				break;

			case "shop_bookstore":
				Msg("A bookstore?<br/>haha... You are interested in books?<br/>But you really don't look like one of those bookworms I know...");
				Msg("The nearest bookstore from here...<br/>Go to Dunbarton.<br/>Check out where it is for yourself.");
				break;

			case "shop_government_office":
				Msg("The Town Office? You are asking about the Town Office in Dunbarton, right?<br/>Then you should ask about it in Dunbarton!");
				Msg("You know how to get to Dunbarton, don't you?<br/>You can't read a map?<br/>Then what's the point of working so hard to make a Minimap?<br/>Just walk straight to the south.");
				Msg("Eavan at the Town Office is pretty...<br/>But she's so cold, man. Too cold...");
				break;

			case "graveyard":
				Msg("The graveyard... I believe other people have told you<br/>about it in detail...<br/>I'll tell you what, there's an interesting story about the graveyard...");
				Msg("You saw the dead tree in the back of the graveyard?<br/>You know why it died?");
				Msg("A lumberjack once walked close to the tree, you know.<br/>Very slowly, just one little step at a time.<br/>And the tree got so scared watching him that it shivered to death.");
				Msg("...");
				Msg("You're not entertained?");
				break;

			case "bow":
				Msg("Talking about a bow, its body is the most important part.<br/>It's called the rim. You usually use oak to make it.<br/>A good bow is made of oak<br/>heartwood from the center of the tree, you know...");
				Msg("That part of the tree is supposed to be stronger,<br/>sturdier, and glossier...<br/>That's how I can tell whether a bow is good or not.");
				Msg("Haha... Surprised? Any lumberjack knows that.<br/>I mean, you know what beef you like, right?<br/>Sirloin, tenderloin, flank, ribeye, blah blah.");
				Msg("Then you'd better know at least what trees you need to make a bow out of, right?");
				break;

			case "lute":
				Msg("You know a Lute is made of wood, right?<br/>You must carefully carve the wood thin<br/>and bend it slowly to adjust the sound.");
				Msg("That's why you need the outer rim of a tree<br/>to make a Lute.<br/>This part is soft and easy to bend.");
				Msg("But it can also change and break very easily.<br/>You must make sure to take care of it...");
				Msg("I saw some kids using Lutes as weapons.<br/>Man, that will definitely ruin the instrument.");
				break;

			case "tir_na_nog":
				Msg("Tir Na Nog? Hahaha...<br/>You can tell it's full of baloney, can't you?");
				Msg("...<br/>It may not be a complete waste though.<br/>Kids can listen to that<br/>and have all the colorful dreams they wish.");
				break;

			case "musicsheet":
				Msg("Music Scores are sold at the General Shop.<br/>If you have one with you,<br/>you can play the music written on it.");
				Msg("But what's wrong with those morons<br/>complaining they can't play music with Music Scores<br/>when they don't have any instruments!");
				break;

			default:
				RndMsg(
					"You think that's funny?",
					"Man! You want me to talk about it?",
					"What? You want to be a know-it-all?",
					"Why do you care about all that weird stuff?",
					"Just don't waste my time with that junk, OK?",
					"I don't think other people know about that...",
					"That's what you want to tell me? Childish....",
					"I don't know anything about that. Try to make some sense."
				);
				ModifyRelation(0, 0, Random(3));
				break;
		}
	}
}

public class TracyShop : NpcShopScript
{
	public override void Setup()
	{
		AddQuest("Quest", 71015, 30); // Collect the Black Dire Wolf's Fomor Scrolls
		AddQuest("Quest", 71016, 30); // Collect the White Dire Wolf's Fomor Scrolls
		AddQuest("Quest", 71021, 30); // Collect the Brown Fox's Fomor Scrolls
		AddQuest("Quest", 71025, 30); // Collect the Brown Bear's Fomor Scrolls
		AddQuest("Quest", 71026, 30); // Collect the Red Bear's Fomor Scrolls
		AddQuest("Quest", 71043, 30); // Collect the Raccoon's Fomor Scrolls
		AddQuest("Quest", 71045, 30); // Collect the Wisp's Fomor Scrolls

		AddQuest("Party Quest", 100019, 5);  // [PQ] Hunt Down the Brown Bears (10)
		AddQuest("Party Quest", 100020, 30); // [PQ] Hunt Down the Brown Bears (30)
		AddQuest("Party Quest", 100021, 5);  // [PQ] The Hunt for Red Bears (10)
		AddQuest("Party Quest", 100022, 30); // [PQ] The Hunt for Red Bears (30)
		AddQuest("Party Quest", 100037, 30); // [PQ] Hunt Down the Black Dire Wolves (30)
		AddQuest("Party Quest", 100038, 30); // [PQ] Hunt Down the White Dire Wolves (30)

		Add("Food", 50004); // Bread

		if (IsEnabled("Carpentry"))
		{
			Add("Carpentry Tool", 40022); // Gathering Axe
			Add("Carpentry Tool", 63223); // Woodworking Plane
			Add("Carpentry Tool", 63222); // Lumber Axe
		}
	}
}
