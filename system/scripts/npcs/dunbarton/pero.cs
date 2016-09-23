//--- Aura Script -----------------------------------------------------------
// Pero
//--- Description -----------------------------------------------------------
// The Rabbie Arena Manager in Dunbarton
//---------------------------------------------------------------------------

public class PeroScript : NpcScript
{
	public override void Load()
	{
		SetRace(10105);
		SetName("_pero");
		SetBody(height: 0.3f);
		SetFace(skinColor: 3, eyeType: 3, eyeColor: 7, mouthType: 2);
		SetLocation(98, 1980, 4400, 185);
		SetGiftWeights(beauty: 1, individuality: 2, luxury: -1, toughness: 2, utility: 2, rarity: 0, meaning: -1, adult: 2, maniac: -1, anime: 2, sexy: 0);

		EquipItem(Pocket.Shoe, 17005, 0x00441A19, 0x00695C66, 0x0000BADB);
		EquipItem(Pocket.RightHand1, 40007, 0x0090A4B6, 0x00745D2F, 0x00EBE94A);
		EquipItem(Pocket.LeftHand1, 46001, 0x005B6F82, 0x00745D2F, 0x00F6887F);

		AddPhrase("Rabbie Arena, Paladin, Dark Knight, ready to duel.");
		AddPhrase("Me, Arena, Goblin, nice Goblin.");
		AddPhrase("Here. Rabbie Arena. Enter.");
		AddPhrase("I trade. Star, Arena coin.");
		AddPhrase("Me. Goblin. Nice Goblin. I like humans.");
	}

	protected override async Task Talk()
	{
		await Intro(L("He may look like a typical Goblin, with the dry skin, intimidating looks,<br/>and the heavy breathing that garbles most of his speech, but he seems somehow different from other Goblins.<br/>His piercing eyes betray a level of intelligence that puts you at ease, and<br/>the corners of his mouth are forced a bit upwards, which closely resembles a human smile."));

		Msg("Dark Knight. Knight of Darkness. Is coming.<br/>Rabbie Arena. Paladin. Dark Knight. Duel.<br/>Fight and fight. Transform. Continue fighting. Only inside.<br/>Fight. See who's stronger. Fun fun.", Button("Start a Conversation", "@talk"), Button("Shop", "@shop"));

		switch (await Select())
		{
			case "@talk":
				Greet();

				if (Title == 11001)
				{
					Msg("Rescue. Goddess? Wow. Amazing. Great. Strong. <username/>. Really. Strong.");
				}
				else if (Title == 11002)
				{
					Msg("The. Savior. Of. Erinn? <username/>. Really. Really. Powerful.<br/>Pero. Shocked. Really. Shocked.");
				}

				await StartConversation();
				break;

			case "@shop":
				Msg("What do you need?<br/>You must be fully prepared if you wish to enter the Battle Arena.");
				OpenShop("PeroShop");
				return;
		}

		End("Goodbye, <npcname/>. I'll see you later!");
	}

	private void Greet()
	{
		if (Memory <= 0)
		{
			Msg(FavorExpression(), L("I, <npcname/>, meet, humans. I. Protect. Arenas.<br/>My. Father. Teach. Me. Human. Language.<br/>Not. Good. Now. Human. I. Like.<br/>Pero, help, Arena work."));
		}
		else if (Memory == 1)
		{
			Msg(FavorExpression(), L("Welcome. <username/>. Two. Times. We. Meet.<br/>Arena. You. Like? I. Too."));
		}
		else if (Memory == 2)
		{
			Msg(FavorExpression(), L("<username/>. Is. Good. Meet. Many. Times.<br/>Arena. Fun. <username/>. Go. To. Arena."));
		}
		else if (Memory <= 6)
		{
			Msg(FavorExpression(), L("Paladin. Dark Knight. Test. Ability. Duel. At. Arena.<br/><username/>. You. Always. Welcome. Always."));
		}
		else
		{
			Msg(FavorExpression(), L("<username/>. Pero. Like. Very. Much.<br/>Pero. See. You. Many. Times. Friend. Friend.<br/>Arena. Friend. <username/>.<br/>Arena. Always. Fun."));
		}

		UpdateRelationAfterGreet();
	}

	public override async Task Conversation()
	{
		while (true)
		{
			this.ShowKeywords();

			Msg("<username/>. Talk. To. <npcname/>.", Button("End Conversation", "@end"), Button("Battle Arena", "@reply1"), Button("Arena Coin", "@reply2"), Button("Paladin vs Dark Knight", "@reply3"), Button("The Talking Goblin", "@reply4"));

			var keyword = await Select();
			switch (keyword)
			{
				case "@reply1":
					Msg("Fight. For Power. Inside. Arena.<br/>Win. You. Get. Star. On. Head. Lose. No. Star.<br/>EXP. No Loss. When. Die.<br/>Click. On. Tower. Next. Me. Enter. Arena. Arena Coin. Important.");
					break;

				case "@reply2":
					Msg("Arena Coin. Not. Money.<br/>Arena Coin. Important. Enter. Arena.<br/>Arena Coin. Important. To. Fight. Paladin. Dark Knight.");
					Msg("Those. Coins. I. Sell. Regular. Coins. I. Sell.<br/><username/>. Important. Items. I. Sell. And. Buy.");
					break;

				case "@reply3":
					Msg("Rabbie Arena. Special. Arena.<br/>Paladin. Dark Knight. Fight. Inside. Arena.");
					Msg("Fight. Free. Transform. Every. Hour.<br/>Fight. For. Power. I. Like.<br/>Special. Coins. Regular. Coins. I. Sell. Coins.<br/>Many. People. Buy. Coins. Fighting. People. Buy. Coins.");
					break;

				case "@reply4":
					Msg("My. Father. Teach. Me. Human. Language.<br/>Not. Good. Now. Human. I. Like. I. Learn. Human. Language.<br/>I. Still. Practice. Everyday.<br/>Pero. Try. Liking. Humans.");
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
			"<npcname/>. Don't. Know. Learn. Now.",
			"<npcname/>. Don't. Know. Learn. Now. Important.",
			"<username/>. Smart. Human. Pero. Listening. Now."
		);
		ModifyRelation(0, 0, Random(3));
	}
}

public class PeroShop : NpcShopScript
{
	public override void Setup()
	{
		Add("Arena", 63050, 10);  // Rabbie Battle Arena Coin x10
		Add("Arena", 63050, 20);  // Rabbie Battle Arena Coin x20
		Add("Arena", 63050, 50);  // Rabbie Battle Arena Coin x50
		Add("Arena", 63050, 100); // Rabbie Battle Arena Coin x100

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

		Add("Fomor Scroll", 71072, 1);  // Black Fomor Scroll x1
		Add("Fomor Scroll", 71072, 10); // Black Fomor Scroll x10
	}
}