//--- Aura Script -----------------------------------------------------------
// Dagon
//--- Description -----------------------------------------------------------
// Grocery Store Manager
//---------------------------------------------------------------------------

public class DagonScript : NpcScript
{
	public override void Load()
	{
		SetRace(10002);
		SetName("_<mini>NPC</mini> Dagon");
		SetFace(skinColor: 26, eyeType: 7, eyeColor: 0, mouthType: 24);
		SetLocation(600, 102646, 98103, 121);
		SetGiftWeights(beauty: 0, individuality: 2, luxury: 1, toughness: 1, utility: 0, rarity: 1, meaning: 2, adult: 1, maniac: 1, anime: 1, sexy: 0);

		EquipItem(Pocket.Face, 4909, 0x007C345B, 0x000F3681, 0x000069A9);
		EquipItem(Pocket.Hair, 4955, 0xFF999900, 0xFF999900, 0xFF999900);
		EquipItem(Pocket.Armor, 15044, 0x004859F7, 0xFFFFFFFF, 0x00000000);
		EquipItem(Pocket.Shoe, 17021, 0xFF003399, 0x00000000, 0x00000000);
		EquipItem(Pocket.Head, 18132, 0x00000000, 0x00000000, 0x00000000);
		EquipItem(Pocket.RightHand1, 40045, 0x00000000, 0x00000000, 0x00000000);

		AddPhrase("Should I try making new dishes?");
		AddPhrase("I wonder if I'll ever see a giant carnivorous fish from Uladh's streams one day?");
		AddPhrase("This dish seems better than usual!");
	}

	protected override async Task Talk()
	{
		Msg(SetDefaultName("Dagon"));

		await Intro(L("He has an small old-fashioned straw hat and a fishing rod with a familiar presence.<br/>He always seems to have a smile on his face that is full of happiness<br/>and with the heart of a kind and gentle soul."));

		Msg("What would you like?", Button("Start a Conversation", "@talk"), Button("Shop", "@shop"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());
				await Conversation();
				break;

			case "@shop":
				Msg("I've prepared some simple ingredients for dishes you can cook.<br/>It should be very easy, but don't mess them up.");
				OpenShop("DagonShop");
				return;
		}

		End("Goodbye, <npcname/>. I'll see you later!");
	}

	private void Greet()
	{
		if (Memory <= 0)
		{
			Msg(FavorExpression(), L("Hello.<br/>Your name is... <username/>?<br/>I'll try hard to remember it. Hahaha."));
		}
		else if (Memory == 1)
		{
			Msg(FavorExpression(), L("Oh, <username/> you came back?<br/>Hahaha. I'm not too surprised that I remember such a unique name and face."));
		}
		else if (Memory == 2)
		{
			Msg(FavorExpression(), L("Oh, you're back.<br/>What can I do for you today?"));
		}
		else if (Memory <= 6)
		{
			Msg(FavorExpression(), L("Just like Bastet, you're interested in how I cook aren't you?<br/>Well... No matter how interested you are, its difficult to explain. Ufufufu..."));
		}
		else
		{
			Msg(FavorExpression(), L("You're quite the familiar face aren't you?<br/>It can be easy to mistake you as an inhabitant of this island."));
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
					RndFavorMsg(
						"The fish that we can catch on our island are quite rare elsewhere,<br/>but on the contrary I'm interested in fish that can be caught elsewhere.<br/>Especially what seems to be a very rare fish dubbed as the giant carnivorous fish of the river!",
						"Everyone really seems to like my fish dishes, but I don't do anything special.<br/>I just make it with good ingredients and a lot of heart.<br/>Well, yes, everyone does seem pleased with my cooking, so thats probably why<br/>I can put so much heart into it. Ufufufu.",
						"I probably would not have chosen to become a fisherman<br/>if I had not been born here,<br/>but I thought about it and I probably would have gone on journey and found work at sea,<br/>and I'd probably be doing pretty much the same job I'm doing now.<p/>You know, it feels like fish exist in order to be caught by me.<br/>Just kidding!"
					);
					ModifyRelation(Random(2), 0, Random(2));
				}
				else if (Memory >= 15 && Favor >= 30 && Stress <= 5)
				{
					RndFavorMsg(
						"The fish that we can catch on our island are quite rare elsewhere,<br/>but on the contrary I'm interested in fish that can be caught elsewhere.<br/>Especially what seems to be a very rare fish dubbed as the giant carnivorous fish of the river!",
						"Everyone really seems to like my fish dishes, but I don't do anything special.<br/>I just make it with good ingredients and a lot of heart.<br/>Well, yes, everyone does seem pleased with my cooking, so thats probably why<br/>I can put so much heart into it. Ufufufu.",
						"I probably would not have chosen to become a fisherman<br/>if I had not been born here,<br/>but I thought about it and I probably would have gone on journey and found work at sea,<br/>and I'd probably be doing pretty much the same job I'm doing now.<p/>You know, it feels like fish exist in order to be caught by me.<br/>Just kidding!"
					);
					ModifyRelation(Random(2), 0, Random(2));
				}
				else if (Favor >= 10 && Stress <= 10)
				{
					RndFavorMsg(
						"The fish that we can catch on our island are quite rare elsewhere,<br/>but on the contrary I'm interested in fish that can be caught elsewhere.<br/>Especially what seems to be a very rare fish dubbed as the giant carnivorous fish of the river!",
						"Everyone really seems to like my fish dishes, but I don't do anything special.<br/>I just make it with good ingredients and a lot of heart.<br/>Well, yes, everyone does seem pleased with my cooking, so thats probably why<br/>I can put so much heart into it. Ufufufu.",
						"I probably would not have chosen to become a fisherman<br/>if I had not been born here,<br/>but I thought about it and I probably would have gone on journey and found work at sea,<br/>and I'd probably be doing pretty much the same job I'm doing now.<p/>You know, it feels like fish exist in order to be caught by me.<br/>Just kidding!"
					);
					ModifyRelation(Random(2), Random(2), Random(2));
				}
				else if (Favor <= -10)
				{
					Msg(FavorExpression(), "Fishing is not just about catching fish.<br/>Without the pleasure and affection for fishing,<br/>the idea of just catching fish seems wrong.");
					ModifyRelation(Random(2), 0, Random(1, 3));
				}
				else if (Favor <= -30 && Stress <= 10)
				{
					Msg(FavorExpression(), "You already know about me, what is there to talk about.");
					ModifyRelation(Random(2), 0, Random(1, 3));
				}
				else if (Favor <= -30 && Stress > 10)
				{
					Msg(FavorExpression(), "You already know about me, what is there to talk about.");
					ModifyRelation(Random(2), -Random(2), Random(1, 4));
				}
				else
				{
					Msg(FavorExpression(), "It seems that fishermen are sort of like sea warriors<br/>with the strong waves we confront.<br/>But, I how can I say that I understand the sea if I don't become familiar with it?<br/>...I think sometimes its important to just go with the flow of things.");
					ModifyRelation(Random(2), 0, Random(3));
				}
				break;

			case "rumor":
				if (Memory >= 15 && Favor >= 50 && Stress <= 5)
				{
					RndFavorMsg(
						"Many people tell me that my tone emulates our village chief's tone.<br/>Haha, I think my tone suits him perfectly.<br/>I mean, wouldn't it be more strange to say the guy over there has the same tone as the elder?<br/>Hahaha.",
						"Have you gone to see Omikesama before?<br/>The stair-way leading to him is pretty steep, but I don't regret going up.<br/>I'm constantly trying to think of ways to do something for Omikesama.",
						"Everyone on this island just loves silvervine fruit,<br/>but Mentum's obsession is especially bad.<br/>Now I'm just as obsessed with the fruits as Mentum.<br/>If you doubt me, go give a silvervine fruit a try."
					);
					ModifyRelation(Random(2), 0, Random(2));
				}
				else if (Memory >= 15 && Favor >= 30 && Stress <= 5)
				{
					RndFavorMsg(
						"Many people tell me that my tone emulates our village chief's tone.<br/>Haha, I think my tone suits him perfectly.<br/>I mean, wouldn't it be more strange to say the guy over there has the same tone as the elder?<br/>Hahaha.",
						"Have you gone to see Omikesama before?<br/>The stair-way leading to him is pretty steep, but I don't regret going up.<br/>I'm constantly trying to think of ways to do something for Omikesama.",
						"Everyone on this island just loves silvervine fruit,<br/>but Mentum's obsession is especially bad.<br/>Now I'm just as obsessed with the fruits as Mentum.<br/>If you doubt me, go give a silvervine fruit a try."
					);
					ModifyRelation(Random(2), 0, Random(2));
				}
				else if (Favor >= 10 && Stress <= 10)
				{
					RndFavorMsg(
						"Many people tell me that my tone emulates our village's chief's tone.<br/>Haha, I think my tone suits him perfectly.<br/>I mean, wouldn't it be more strange to say the guy over there has the same tone as the elder?<br/>Hahaha.",
						"Have you gone to see Omikesama before?<br/>The stair-way leading to him is pretty steep, but I don't regret going up.<br/>I'm constantly trying to think of ways to do something for Omikesama.",
						"Everyone on this island just loves silvervine fruit,<br/>but Mentum's obsession is especially bad.<br/>Now I'm just as obsessed with the fruits as Mentum.<br/>If you doubt me, go give a silvervine fruit a try."
					);
					ModifyRelation(Random(2), Random(2), Random(2));
				}
				else if (Favor <= -10)
				{
					RndFavorMsg(
						"People from across the sea began visiting this place fairly often.<br/>I don't know how they found out about this place...<br/>...Well, I guess I am thankful that they help eliminate the rats.",
						"The fishing and metallurgy spots are just important for us as the silvervine fields.<br/>Recently, travelers from other places starting gathering our resources,<br/>but our resources are abundant, so there are no worries. Hahaha."
					);
					ModifyRelation(Random(2), 0, Random(1, 3));
				}
				else if (Favor <= -30 && Stress <= 10)
				{
					Msg(FavorExpression(), "Well, I don't have much to talk about other than fish.");
					ModifyRelation(Random(2), 0, Random(1, 3));
				}
				else if (Favor <= -30 && Stress > 10)
				{
					Msg(FavorExpression(), "Well, I don't have much to talk about other than fish.");
					ModifyRelation(Random(2), -Random(2), Random(1, 4));
				}
				else
				{
					RndFavorMsg(
						"People from across the sea began visiting this place fairly often.<br/>I don't know how they found out about this place...<br/>...Well, I guess I am thankful that they help eliminate the rats.",
						"The fishing and metallurgy spots are just important for us as the silvervine fields.<br/>Recently, travelers from other places starting gathering our resources,<br/>but our resources are abundant, so there are no worries. Hahaha."
					);
					ModifyRelation(Random(2), 0, Random(3));
				}
				break;

			case "about_arbeit":
				Msg("Not implemented.");
				break;

			// Implement Quest id 601121 Help Dagon
			//case "Recommendation_Nekojima":
			//	if (QuestActive(601121) && QuestCompleted(601121))
			//	{
			//		Msg("Did you not already get my letter of recommendation?<br/>My memory is not what it used to be...");
			//	}
			//	else
			//	{
			//		Msg("Hahahaha, I don't know if you like cats,<br/>but I can't just write you a letter of recommendation.");
			//		Msg("What did you say? You really need my recommendation??<br/>Well... then, can you help me out? Don't worry, it won't be a difficult task.<br/>I just need help with my daily routine of catching the cats' favorite food.<br/>What do you say, want to try it out once?");
			//		Msg("If you just go to the fishing spot beyond the coast,<br/>you can catch the cats' favorite fish.<br/>Well its thanks to the cats that we have fertilizer for the silvervine fruits.<br/>Go and fish up some bluefin tuna and give it to the cats.<br/>Haha, I believe there is nothing wasteful about this, after all this is why the fish were called to this world.");
			//		StartQuest(601121);
			//	}
			//	break;

			case "shop_misc":
				Msg("Bastet over there handles general goods.<br/>Bastet's personality may be a bit tedious to handle,<br/>but it certainly has a cute aspect to it as well~.");
				Msg("Oh right... please don't keep talking to me about this.<br/>Bastet will end up misunderstanding again. Haha.");
				break;

			case "shop_grocery":
				GiveKeyword("shop_restaurant");
				Msg("I sell some ingredients that can be used to cook with caught fish,<br/>but I don't have a large variety.<br/>They still make good dishes though, so I can afford to sell just these.");
				break;

			case "shop_restaurant":
				Msg("There is no real grocery store or restaurant on this island.<br/>I do occasionally make dishes with my caught fish for the local residents,<br/>but I'm not going to put those dishes on sale.");
				break;

			case "shop_healing":
				Msg("Go visit the village elder's place.");
				break;

			case "shop_armory":
				Msg("I think its nice to go to Mentum.<br/>You see, Mentum often repairs my fishing rods.");
				break;

			default:
				if (Memory >= 15 && Favor >= 30 && Stress <= 5)
				{
					RndFavorMsg(
						"You are quite knowledgeable<br/>However, I'm sorry, I don't know that story...",
						"I'm sorry.</br>I'm only familiar with fishing stories.",
						"I don't have the answer you want <username/>.<br/>I don't quite understand.<br/>I'm sorry."
					);
					ModifyRelation(0, 0, Random(2));
				}
				else if (Favor >= 10 && Stress <= 10)
				{
					RndFavorMsg(
						"You are quite knowledgeable<br/>However, I'm sorry, I don't know that story...",
						"I'm sorry.</br>I'm only familiar with fishing stories.",
						"I don't have the answer you want <username/>.<br/>I don't quite understand.<br/>I'm sorry."
					);
					ModifyRelation(0, 0, Random(2));
				}
				else if (Favor <= -10)
				{
					RndFavorMsg(
						"Right now I'm thinking about various kinds of fishing. I'm sorry can you come back later?",
						"Oh, I need to go to the ocean soon...",
						"That's bad, and also a strange story."
					);
					ModifyRelation(0, 0, Random(4));
				}
				else if (Favor <= -30)
				{
					RndFavorMsg(
						"Right now I'm thinking about various kinds of fishing. I'm sorry can you come back later?",
						"Oh, I need to go to the ocean soon...",
						"That's bad, and also a strange story."
					);
					ModifyRelation(0, 0, Random(5));
				}
				else
				{
					RndFavorMsg(
						"Actually, I love to learn while listening to stories I don't know.<br/>But, if you only listen to me its boring. Haha.",
						"Well, lets put that story aside and talk about a different one."
					);
					ModifyRelation(0, 0, Random(3));
				}
				break;
		}
	}

	protected override async Task Gift(Item item, GiftReaction reaction)
	{
		if (item.Info.Id == 50085 || item.Info.Id >= 64057 && item.Info.Id <= 64066) // Silvervine fruit and Nekojima Gems
		{
			RndMsg(
				"Are you really giving me this?<br/>You went this far to think about me!<br/>Thank you so very much.",
				"This is my favorite! How did you know?<br/>Thank you very much.",
				"Only receiving is bad...<br/>But, thank you so much."
			);

		}
		else
		{
			switch (reaction)
			{
				case GiftReaction.Love:
					RndMsg(
						"Huh, you'll really give me this?<br/>Thank you very much.",
						"You have an open mind to give this to me.<br/>I appreciate the gift.",
						"Are you serious?<br/>I don't know if I really deserve to receive this...<br/>I will be grateful."
					);
					break;

				case GiftReaction.Like:
					RndMsg(
						"Oh, I can have such a present... Thank you.",
						"Did you mean to give this to Bastet?<br/>Haha, relax it was a joke.<br/>Thank you."
					);
					break;

				case GiftReaction.Dislike:
					RndMsg(
						"Oh, it's a present. Thank you.",
						"You prepared this for me, didn't you?<br/>I will not hesitate to accept it."
					);
					break;

				default: // GiftReaction.Neutral
					RndMsg(
						"I will appreciate it.",
						"I can sense your sincerity. Thank you."
					);
					break;
			}
		}
	}
}

public class DagonShop : NpcShopScript
{
	public override void Setup()
	{
		Add("Ingredients", 50219); // Basil
		Add("Ingredients", 50131); // Sugar
		Add("Ingredients", 50132); // Salt
		Add("Ingredients", 50156); // Pepper
		Add("Ingredients", 50217); // Celery
		Add("Ingredients", 50186); // Red Pepper Powder
	}
}
