//--- Aura Script -----------------------------------------------------------
// Goro
//--- Description -----------------------------------------------------------
// The Alby Arena Manager in Tir Chonaill
//---------------------------------------------------------------------------

public class GoroScript : NpcScript
{
	public override void Load()
	{
		SetRace(10105);
		SetName("_goro");
		SetBody(height: 0.3f);
		SetFace(skinColor: 32, eyeType: 3, eyeColor: 7, mouthType: 2);
		SetLocation(28, 1283, 3485, 198);
		SetGiftWeights(beauty: 1, individuality: 2, luxury: -1, toughness: 2, utility: 2, rarity: 0, meaning: -1, adult: 2, maniac: -1, anime: 2, sexy: 0);

		EquipItem(Pocket.Shoe, 17005, 0x00441A19, 0x00695C66, 0x0000BADB);
		EquipItem(Pocket.RightHand1, 40007, 0x006A6A6A, 0x00745D2F, 0x00737270);
		EquipItem(Pocket.LeftHand1, 46001, 0x00858585, 0x00746C54, 0x0005003C);

		AddPhrase("Here, you may enter Alby Arena.");
		AddPhrase("Test your strength here, in Alby Arena!");
		AddPhrase("Wait, do not attack.");
		AddPhrase("We exchange Stars with Arena Coins.");
	}

	protected override async Task Talk()
	{
		await Intro(L("With his rough skin, menacing face, and his constant hard-breathing,<br/>he has the sure look of a Goblin.<br/>Yet, there is something different about this one.<br/>Strangely, it appears to have a sense of noble demeanor that does not match its rugged looks."));

		Msg("How can I help you?", Button("Start a Conversation", "@talk"), Button("Shop", "@shop"));

		switch (await Select())
		{
			case "@talk":
				Greet();

				if (Title == 11001)
				{
					Msg("...Did you finally succeed?<br/>That's incredible. Haha.");
					Msg("But... Honestly, I believe all this is just the beginning.<br/>Puhahahaha...");
				}
				else if (Title == 11002)
				{
					Msg("The power of humans is truly amazing...<br/>You're already strong enough to protect Erinn...");
					Msg("...<npcname/> is quite curious<br/>where humans<br/>get such strength...");
				}

				await StartConversation();
				break;

			case "@shop":
				Msg("What do you need?<br/>You must be fully prepared if you wish to enter the Battle Arena.");
				OpenShop("GoroShop");
				return;
		}

		End("Goodbye, <npcname/>. I'll see you later!");
	}

	private void Greet()
	{
		if (Memory <= 0)
		{
			Msg(FavorExpression(), L("Welcome, what a smart " + (Player.IsMale ? "man" : "woman") + " you are.<br/>My name is Goro.<br/>Ah, do not be surprised. I have no intention of hurting you."));
		}
		else if (Memory == 1)
		{
			Msg(FavorExpression(), L("I'm so glad to see you again.<br/>I am <npcname/>, the goblin who can speak the language of humans."));
		}
		else if (Memory == 2)
		{
			Msg(FavorExpression(), L("Ah, <username/>. Thank you for coming.")); // Translated
		}
		else if (Memory <= 6)
		{
			Msg(FavorExpression(), L("Have you come to test your abilities at the Arena again today?"));
		}
		else
		{
			Msg(FavorExpression(), L("Nice to see you, <username/>. I think it's worthwhile to mention that I'm hearing many stories about you from other people."));
		}

		UpdateRelationAfterGreet();
	}

	public override async Task Conversation()
	{
		while (true)
		{
			this.ShowKeywords();

			Msg("You wish to speak to me?<br/>Please ask, and I will do my best to answer your questions.", Button("End Conversation", "@end"), Button("Battle Arena", "@reply1"), Button("Arena Coin", "@reply2"), Button("Talking Goblin", "@reply3"));

			var keyword = await Select();
			switch (keyword)
			{
				case "@reply1":
					RndMsg(
						"The Arenas are located in every dungeon across Erinn and each have its own rules.<br/>Here, in Alby Battle Arena, you will participate in the Single Deathmatch.",
						"The people of Erinn, who value peace,<br/>are known to dislike hurting each other.<br/>However, the true meaning of competition<br/>is something all races have been doing during uncountable millenia.",
						"This is the place to test and train your strength, wisdom, and skills.<br/>Even if you get hurt and lose consciousness in the Arena,<br/>you will not lose any EXP like you did in the outside world."
					);
					break;

				case "@reply2":
					Msg("The Arena Coin is quite different from the common Gold used in Erinn.<br/>It's a currency used only in the Arena, as well as a ticket to enter it.<br/>Quite different, yes?");
					Msg("As you know, General Shops do not accept Arena Coins.<br/>However, here, there is not much you can do with ordinary Erinn Gold.<br/>You have used Arena Coins to get to me,<br/>and you will also need them to enter the Arena.");
					Msg("And, some of my Goblin brothers hiding throughout the dungeons of Erinn<br/>sell items that cannot be found in ordinary shops,<br/>but they only accept Arena Coins as payment.");
					break;

				case "@reply3":
					Msg("There are Goblins spread throughout the many dungeons of Erinn<br/>who can speak the language of humans.<br/>They are my brothers, and we have all learned from the same father.");
					Msg("My father is a human.<br/>To tell the truth, I am a Goblin raised by the humans.");
					Msg("My father took care of me since I was young, and took me all over the world<br/>teaching me the language, culture, manners, and many other aspects of humankind.<br/>My dream is to become the greatest merchant in Erinn, just like my father.");
					break;

				case "@end":
					return;

				default:
					await Hook("before_keywords", keyword);

					await this.Keywords(keyword);
					break;
			}
		}
	}

	protected override async Task Keywords(string kw)
	{
		RndMsg(
			"Well...",
			"Excuse me, what did you say?",
			"Hmm...I believe I have heard about it...",
			"I do not know anything about that kind of story."
		);
		ModifyRelation(0, 0, Random(3));
	}
}

public class GoroShop : NpcShopScript
{
	public override void Setup()
	{
		Add("Arena", 63019, 10);  // Alby Battle Arena Coin x10
		Add("Arena", 63019, 20);  // Alby Battle Arena Coin x20
		Add("Arena", 63019, 50);  // Alby Battle Arena Coin x50
		Add("Arena", 63019, 100); // Alby Battle Arena Coin x100

		Add("Potions", 51002, 1);  // HP 30 Potion x1
		Add("Potions", 51002, 20); // HP 30 Potion x20
		Add("Potions", 51007, 1);  // MP 30 Potion x1
		Add("Potions", 51007, 20); // MP 30 Potion x20
		Add("Potions", 51012, 1);  // Stamina 30 Potion x1
		Add("Potions", 51012, 20); // Stamina 30 Potion x20
		Add("Potions", 60005, 10); // Bandage x10
		Add("Potions", 60005, 20); // Bandage x20
		Add("Potions", 63000, 10); // Phoenix Feather x10
		Add("Potions", 63000, 20); // Phoenix Feather x20

	}
}
