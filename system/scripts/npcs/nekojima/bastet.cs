//--- Aura Script -----------------------------------------------------------
// Bastet
//--- Description -----------------------------------------------------------
// General store manager
//---------------------------------------------------------------------------

public class BastetScript : NpcScript
{
	public override void Load()
	{
		SetRace(10001);
		SetName("_<mini>NPC</mini> Bastet");
		SetBody(height: 0.8f);
		SetFace(skinColor: 21, eyeType: 24, eyeColor: 8, mouthType: 23);
		SetLocation(600, 102115, 97502, 20);
		SetGiftWeights(beauty: 0, individuality: 2, luxury: 1, toughness: 1, utility: 0, rarity: 1, meaning: 2, adult: 1, maniac: 1, anime: 1, sexy: 0);

		EquipItem(Pocket.Face, 3908, 0x00F79723, 0x0057686A, 0x0005004F);
		EquipItem(Pocket.Hair, 3041, 0xFCC66666, 0xFCC66666, 0xFCC66666);
		EquipItem(Pocket.Armor, 15018, 0xFF339933, 0xFF669900, 0xFFFFFFCC);
		EquipItem(Pocket.Shoe, 17021, 0xFFFFFFCC, 0x00000000, 0x00000000);

		AddPhrase("I'd like some new clothes to wear~!");
		AddPhrase("I wonder if I look young~?");
		AddPhrase("I don't feel like I need a diet yet... Probably...");
	}

	protected override async Task Talk()
	{
		Msg(SetDefaultName("Bastet"));

		await Intro(L("It seems that her harsh appearance is representative of her vibrant character.<br/>Her curious eyes dart around indicating a high interest in her surroundings,<br/>but her talkative mouth is proof of that interest."));

		Msg("Hello!<br/>I may be small, but I'm a super important person on this island!!<br/>I'm Bastet!", Button("Start a Conversation", "@talk"), Button("Shop", "@shop"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());
				await Conversation();
				break;

			case "@shop":
				Msg("My products may not be very good.<br/>However, I think there is no inconvenience for <username/> to stay here for awhile.");
				OpenShop("BastetShop");
				return;
		}

		End("Goodbye, <npcname/>. I'll see you later!");
	}

	private void Greet()
	{
		if (Memory <= 0)
		{
			Msg(FavorExpression(), L("This seems to be the first time I'm meeting you... Is that right?<br/>I am happy to meet you! Nyahaha♪"));
		}
		else if (Memory == 1)
		{
			Msg(FavorExpression(), L("Oh, are you the one in front of me?<br/>You came again after all! Nyahaha♪"));
		}
		else if (Memory == 2)
		{
			Msg(FavorExpression(), L("Oh, oh, <username/> is that you? You came back.<br/>Everyone seems to like Bastet!<br/>Did you even make a fan club? Nyahaha."));
		}
		else if (Memory <= 6)
		{
			Msg(FavorExpression(), L("<username/>,<br/>I already memorized the name, <username/>, perfectly! Heehee~"));
		}
		else
		{
			Msg(FavorExpression(), L("Hello.<br/>Even when I see you everyday, it seems fresh, just like meeting you for the first time.<br/>Nyahaha♪"));
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
						"I love the people in this village!<br/>And everyone likes me!<br/>This is my greatest pleasure.",
						"The village elder doesn't like to talk much about himself.<br/>That's why I want to talk...<br/>Do I really want to hide the important story that you want to hear?",
						"I'm very concerned about other people. Do you want to know about that person...<br/>because you feel its better to know each other well?<br/>That way you can have a better sense of affinity... Am I not wrong?"
					);
					ModifyRelation(Random(2), 0, Random(2));
				}
				else if (Memory >= 15 && Favor >= 30 && Stress <= 5)
				{
					RndFavorMsg(
						"I love the people in this village!<br/>And everyone likes me!<br/>This is my greatest pleasure.",
						"The village elder doesn't like to talk much about himself.<br/>That's why I want to talk...<br/>Do I really want to hide the important story that you want to hear?",
						"I'm very concerned about other people. Do you want to know about that person...<br/>because you feel its better to know each other well?<br/>That way you can have a better sense of affinity... Am I not wrong?"
					);
					ModifyRelation(Random(2), 0, Random(2));
				}
				else if (Favor >= 10 && Stress <= 10)
				{
					RndFavorMsg(
						"I love the people in this village!<br/>And everyone likes me!<br/>This is my greatest pleasure.",
						"The village elder doesn't like to talk much about himself.<br/>That's why I want to talk...<br/>Do I really want to hide the important story that you want to hear?",
						"I'm very concerned about other people. Do you want to know about that person...<br/>because you feel its better to know each other well?<br/>That way you can have a better sense of affinity... Am I not wrong?"
					);
					ModifyRelation(Random(2), Random(2), Random(2));
				}
				else if (Favor <= -10)
				{
					Msg(FavorExpression(), "<username/> is curious too, are you not?");
					ModifyRelation(Random(2), 0, Random(1, 3));
				}
				else if (Favor <= -30 && Stress <= 10)
				{
					Msg(FavorExpression(), "<username/> is curious too, are you not?");
					ModifyRelation(Random(2), 0, Random(1, 3));
				}
				else if (Favor <= -30 && Stress > 10)
				{
					Msg(FavorExpression(), "I am tired now...<br/>I also can become tired and there are times when everything becomes too troublesome...");
					ModifyRelation(Random(2), -Random(2), Random(1, 4));
				}
				else
				{
					RndFavorMsg(
						"Its so cute, I often hear pleasant tales...<br/>...That everyone likes me!<br/>They say I'm like a flower on this island. Hehe... why did I say that?",
						"I have some clothes that I am making right now,<br/>but I'm worried about who should get it...<br/>I'm torn between Mentam and the village chief, but I'd also like to ask<br/>Uncle Dagon to tell me the secret of making fish dishes...<br/>Hnnngg I don't know what to do."
					);
					ModifyRelation(Random(2), 0, Random(3));
				}
				break;

			case "rumor":
				if (Memory >= 15 && Favor >= 50 && Stress <= 5)
				{
					RndFavorMsg(
						"I've never been to the place where Omikesama is,<br/>but I hear the spirits inside are powerful and that its wise not to go in.<br/>Did you get that <username/>?",
						"I think the silvervine field is really well managed by the village elder.<br/>...If Mentum managed it, I don't think there would be any<br/>silvervine fruits around.<br/>...I wouldn't want that because I really love silvervine fruits.",
						"Once upon a time the southern island<br/>was a peaceful place with a good atmosphere,<br/>but then the rats came and ruined that peace...<br/>The rats came and robbed it..."
					);
					ModifyRelation(Random(2), 0, Random(2));
				}
				else if (Memory >= 15 && Favor >= 30 && Stress <= 5)
				{
					RndFavorMsg(
						"I've never been to the place where Omikesama is,<br/>but I hear the spirits inside are powerful and that its wise not to go in.<br/>Did you get that <username/>?",
						"I think the silvervine field is really well managed by the village elder.<br/>...If Mentum managed it, I don't think there would be any<br/>silvervine fruits around.<br/>...I wouldn't want that because I really love silvervine fruits.",
						"Once upon a time the southern island<br/>was a peaceful place with a good atmosphere,<br/>but then the rats came and ruined that peace...<br/>The rats came and robbed it..."
					);
					ModifyRelation(Random(2), 0, Random(2));
				}
				else if (Favor >= 10 && Stress <= 10)
				{
					RndFavorMsg(
						"I've never been to the place where Omikesama is,<br/>but I hear the spirits inside are powerful and that its wise not to go in.<br/>Did you get that <username/>?",
						"I think the silvervine field is really well managed by the village elder.<br/>...If Mentum managed it, I don't think there would be any<br/>silvervine fruits around.<br/>...I wouldn't want that because I really love silvervine fruits.",
						"Once upon a time the southern island<br/>was a peaceful place with a good atmosphere,<br/>but then the rats came and ruined that peace...<br/>The rats came and robbed it..."
					);
					ModifyRelation(Random(2), Random(2), Random(2));
				}
				else if (Favor <= -10)
				{
					Msg(FavorExpression(), "Have you seen the rats on the island beyond the water bridge?<br/>I went for a stroll without knowing,<br/>anything and then a rat came out...<br/>I thought my heart was going to stop! After that I will not go there by myself.<br/>Sheesh... I'm horrified just thinking about it.");
					ModifyRelation(Random(2), 0, Random(1, 3));
				}
				else if (Favor <= -30 && Stress <= 10)
				{
					Msg(FavorExpression(), "If you go and exterminate the rats, do you think everyone will be delighted?");
					ModifyRelation(Random(2), 0, Random(1, 3));
				}
				else if (Favor <= -30 && Stress > 10)
				{
					Msg(FavorExpression(), "If you go and exterminate the rats, do you think everyone will be delighted?");
					ModifyRelation(Random(2), -Random(2), Random(1, 4));
				}
				else
				{
					Msg(FavorExpression(), "Have you seen the rats on the island beyond the water bridge?<br/>I went for a stroll without knowing,<br/>anything and then a rat came out...<br/>I thought my heart was going to stop! After that I will not go there by myself.<br/>Sheesh... I'm horrified just thinking about it.");
					ModifyRelation(Random(2), 0, Random(3));
				}
				break;

			case "about_arbeit":
				Msg("Not implemented.");
				break;

			// TODO: Implement Quest id 601111 Help Bastet
			//case "Recommendation_Nekojima":
			//	if (QuestActive(601111) && QuestCompleted(601111))
			//	{
			//		Msg("Do you still need my letter of recommendation?<br/>Are you trying to collect signatures? Nyahaha!");
			//	}
			//	else
			//	{
			//		Msg("Oh, you want something to give to the cats?<br/>This will be just right~. I'm making silvervine perfume for the cats,<br/>but its hard for me to make it alone...");
			//		Msg("Do you know how to make the perfume? You can make it easily with water and silvervine fruits.<br/>Don't worry too much! It's really easy. I'll even give you a letter of recommendation.<br/>Please give perfume to the cats on the island~.<br/>Well then, I have no time to spare... but it was nice to meet you!");
			//		StartQuest(601111);
			//	}
			//	break;

			case "shop_misc":
				Msg("I sell some miscellaneous goods.<br/>Oh, please do not look too much~.<br/>Although the selection is small, <br/>it lets me make enough to eat on this island.<br/>It's true! Mumumu, what is that face you're making??");
				break;

			case "shop_grocery":
				GiveKeyword("shop_restaurant");
				Msg("Uncle Dagon sells some ingredients.<br/>But I feel that there is only one really important ingredient...<br/>Should I go hide it and use it myself...?");
				Msg("Even if its not the only important one, it is unlikely<br/>that Uncle Dagon's dish will be delicious without it!");
				break;

			case "shop_restaurant":
				Msg("Uncle Dagon's fish dishes are really delicious!<br/>But... sometimes I want to try dishes from the<br/>Uladh continent as well~.");
				break;

			case "shop_healing":
				Msg("Our village elder can take care of your ailments.<br/>It's difficult to get medicinal herbs, potions, and first aid materials here...<br/>The village elder really can do anything.<br/>Isn't he cool?");
				break;

			case "shop_inn":
				Msg("An inn is...<br/>The place where tourists stay?<br/>Oh! Please stay at my house if necessary!");
				Msg("Do you think I'd welcome everyone?<br/>I probably would, because my heart is like clean silk!");
				break;

			case "shop_armory":
				Msg("Mentum sells only weapons that are absolutely necessary.<br/>Most are tools that are necessary for daily life.<br/>I help train the knights and I prefer using the life tools...<br/>Does Mentum have some sort of secret in mind?<br/>It's Worrisome, worrisome~!");
				break;

			case "shop_cloth":
				Msg("To tell you the truth, while I do make my own clothes and clothes for the villagers,<br/>I am not the best tailor, so I won't make clothing like you find<br/>in a clothing store on one of the big continents.<br/>If the clothes I make are at least of a passable quality,<br/>I'll be satisfied with that. Nyahaha~.");
				break;

			default:
				if (Memory >= 15 && Favor >= 30 && Stress <= 5)
				{
					RndFavorMsg(
						"Oh... it's a very worrisome story though...<br/>I'm sorry, <username/>...",
						"Let's talk about other things. Nyahaha.",
						"<username/> is very knowledgeable...<br/>... That's so cool!!"
					);
					ModifyRelation(0, 0, Random(2));
				}
				else if (Favor >= 10 && Stress <= 10)
				{
					RndFavorMsg(
						"Oh... it's a very worrisome story though...<br/>I'm sorry, <username/>...",
						"Let's talk about other things. Nyahaha.",
						"<username/> is very knowledgeable...<br/>... That's so cool!!"
					);
					ModifyRelation(0, 0, Random(2));
				}
				else if (Favor <= -10)
				{
					RndFavorMsg(
					"...Umm...",
						"Huh? Did I hear that wrong?",
						"I don't understand to well..."
					);
					ModifyRelation(0, 0, Random(4));
				}
				else if (Favor <= -30)
				{
					RndFavorMsg(
					 "...Umm...",
					 "Huh? Did I hear that wrong?",
					 "I don't understand to well..."
					);
					ModifyRelation(0, 0, Random(5));
				}
				else
				{
					RndFavorMsg(
						"Here we go...<br/>Maybe the village elder may know that...?",
						"Well... I do not know much about it.",
						"Well... Why don't you talk about other things?"
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
				"Oh, where did you get this?<br/><username/>, Cool cool~!",
				"Wow, thank you so much!!<br/>Who do I get to boast to? Nyahaha!",
				"Thank you! I'll take care of it.<br/>I feel really good now!"
			);
			return;
		}

		switch (reaction)
		{
			case GiftReaction.Love:
				RndMsg(
					"Wow! Are you really giving this to me?",
					"<username/> is the best!! Thank you!",
					"Oh, I love this!<br/>I'm impressed.<br/><username/> has outstanding sense~."
				);
				break;

			case GiftReaction.Like:
				RndMsg(
					"Can I have this, please?",
					"You bothered to prepare this for me?<br/>Thank you!<br/>Wow ~ this is nice! Thank you!"
				);
				break;

			case GiftReaction.Dislike:
				RndMsg(
					"Oh, yes... Thanks...",
					"Well, if you give me this..."
				);
				break;

			default: // GiftReaction.Neutral
				RndMsg(
						"A present...? Thank you so much.",
						"I can accept this with a light-hearted feeling, right?",
						"I don't have anything prepared...<br/>... Thank you so very much~."
					);
				break;
		}
	}
}

public class BastetShop : NpcShopScript
{
	public override void Setup()
	{
		Add("General Goods", 2005);       // Item Bag (7x5)
		Add("General Goods", 2006);       // Big Gold Pouch
		Add("General Goods", 40045);      // Fishing Rod
		Add("General Goods", 60034, 300); // Bait Tin x300
		Add("General Goods", 63020);      // Empty Bottle

		if (IsEnabled("Handicraft"))
			Add("General Goods", 60045);  // Handicraft Kit

		if (IsEnabled("Metallurgy"))
			Add("General Goods", 40211);  // Metallurgy Sieve
	}
}
