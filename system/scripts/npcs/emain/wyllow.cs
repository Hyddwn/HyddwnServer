//--- Aura Script -----------------------------------------------------------
// Wyllow
//--- Description -----------------------------------------------------------
// Head Bishop inside of Emain Macha Cathedral.
//---------------------------------------------------------------------------

public class WyllowScript : NpcScript
{
	public override void Load()
	{
		SetRace(10002);
		SetName("_wyllow");
		SetBody(weight: 0.1f, upper: 0.94f, lower: 0.83f);
		SetFace(skinColor: 17, eyeType: 0, eyeColor: 31, mouthType: 0);
		SetLocation(61, 6880, 5030, 76);
		SetGiftWeights(beauty: 1, individuality: -1, luxury: 1, toughness: -1, utility: 0, rarity: 1, meaning: 2, adult: 1, maniac: 0, anime: 0, sexy: 0);

		EquipItem(Pocket.Face, 4951, 0x00006246, 0x005F0051, 0x00F8984B);
		EquipItem(Pocket.Hair, 4952, 0x00916B39, 0x00916B39, 0x00916B39);
		EquipItem(Pocket.Armor, 15083, 0x00808080, 0x00808080, 0x00808080);
		EquipItem(Pocket.Shoe, 17009, 0x007B6642, 0x00B4A67F, 0x00842160);
		EquipItem(Pocket.Head, 18092, 0x00808080, 0x00FFFFFF, 0x00FFFFFF);

		AddPhrase("Bow down to the Lord...");
		AddPhrase("Lord Lymilark...");
		AddPhrase("Let us pray...");
		AddPhrase("Hmmm...");
		AddPhrase("These days, people just don't have that much faith...");
	}

	protected override async Task Talk()
	{
		await Intro(L("Wyllow's round face is smiling out from his ornate Priest's clothing.<br/>His white eyebrows, and everything about him tells you that he is the head Priest.<br/>The seemingly eternal smile on his face shines brightly with compassion, and his palms are facing outward,<br/>as if he is getting ready to greet you with them."));

		Msg("Welcome. Seek and you will find hope here.", Button("Start a Conversation", "@talk"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());

				if (Player.IsUsingTitle(11001))
				{
					Msg("You rescued the Goddess? Incredible...<br/>We need more people like you<br/>to make this world a better place<br/>in accordance with the will of Lymilark...");
				}
				else if (Player.IsUsingTitle(11002))
				{
					Msg("Well done, <username/>...<br/>So you protected Erinn after all...");
					Msg("The duty of protecting something is not easy,<br/>let alone protecting this world<br/>from the attacks of Fomors...");
					Msg("...The people here will<br/>forever be grateful,<br/>and honor you with endless respect.");
					Msg("...I would also like to thank you personally.<br/>Thank you, <username/>...");
				}

				await Conversation();
				break;
		}

		End("Thank you, <npcname/>. I'll see you later!");
	}

	private void Greet()
	{
		if (Memory <= 0)
		{
			Msg(FavorExpression(), L("Blessings to you for finding your way back to the Church!"));
		}
		else if (Memory == 1)
		{
			Msg(FavorExpression(), L("Blessings to you for finding your way back to the Church!"));
		}
		else if (Memory == 2)
		{
			Msg(FavorExpression(), L("You're here again, <username/>. Now, let's pray."));
		}
		else if (Memory <= 6)
		{
			Msg(FavorExpression(), L("Oh Lymilark, our lamb, <username/>, has come to see you once more.<br/>Please watch over <username/>."));
		}
		else
		{
			Msg(FavorExpression(), L("<username/>. Have you prayed today?"));
		}

		UpdateRelationAfterGreet();
	}

	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				Msg(FavorExpression(), "I am merely a Priest who spreads the knowledge and words of Lymilark.<br/>If you, too, give your all to him, then you won't need to be scared or struggle with your current quests.<br/>Plus, your eyes will be opened to the truths of this world...");
				ModifyRelation(Random(2), 0, Random(3));
				break;

			case "rumor":
				Msg(FavorExpression(), "Well... I'm not really intered in rumors or in gossiping about people around here...<br/>That's where Priest James comes in. He handles everything...<br/>I'm so glad he's here.");
				ModifyRelation(Random(2), 0, Random(3));
				break;

			case "shop_healing":
				Msg("The Healer's House...<br/>Isn't it near the northern gate...?<br/>Ask James.");
				break;

			case "shop_inn":
				Msg("Your body isn't the only thing that needs regular rest. So does your mind.<br/>Think about how your spirit will be saved<br/>if you spend some time praying whenever you sit down to rest.");
				break;

			case "skill_range":
				Msg("There's a saying in the words of Lymilark, who commands,<br/> 'Do not attack others.<br/>Repent your sins for unknowingly attacking someone, causing great distress.");
				Msg("That being is loved by Lymilark just the same, so why would you,<br/>a mere creation of Lymilark, fight your brethren and hurt them?<br/>Try to understand Lymilark's intentions through the eyes of a parent... '");
				Msg("That's when you'll realize how terrible it is<br/>to take away another being's life.<br/>I advise you to pray about it and to really think about it.");
				break;

			case "skill_instrument":
				Msg("Lymilark also supports and believes in anything that soothes one's mind and soul...");
				break;

			case "skill_magnum_shot":
				Msg("There's a saying in the words of Lymilark, who commands,<br/> 'Do not attack others.<br/>Repent your sins for unknowingly attacking someone, causing great distress.");
				Msg("That being is loved by Lymilark just the same, so why would you,<br/>a mere creation of Lymilark, fight your brethren and hurt them?<br/>Try to understand Lymilark's intentions through the eyes of a parent... '");
				Msg("That's when you'll realize how terrible it is<br/>to take away another being's life.<br/>I advise you to pray about it and to really think about it.");
				break;

			case "skill_counter_attack":
				Msg("There's a saying in the words of Lymilark, who commands,<br/> 'Do not attack others.<br/>Repent your sins for unknowingly attacking someone, causing great distress.");
				Msg("That being is loved by Lymilark just the same, so why would you,<br/>a mere creation of Lymilark, fight your brethren and hurt them?<br/>Try to understand Lymilark's intentions through the eyes of a parent... '");
				Msg("That's when you'll realize how terrible it is<br/>to take away another being's life.<br/>I advise you to pray about it and to really think about it.");
				break;

			case "skill_smash":
				Msg("There's a saying in the words of Lymilark, who commands,<br/> 'Do not attack others.<br/>Repent your sins for unknowingly attacking someone, causing great distress.");
				Msg("That being is loved by Lymilark just the same, so why would you,<br/>a mere creation of Lymilark, fight your brethren and hurt them?<br/>Try to understand Lymilark's intentions through the eyes of a parent... '");
				Msg("That's when you'll realize how terrible it is<br/>to take away another being's life.<br/>I advise you to pray about it and to really think about it.");
				break;

			case "pool":
				Msg("There's a scripture from Lymilark that goes as follows:<br/>'Human love is like water ina lake; if it floods, then it also overflows.<br/>So when in a drought, it can become dried out...");
				Msg("Lymilark's love for us, though, is like a deep ocean;<br/>unchanging over time.");
				Msg("We need to learn how to thank him for his undying love,<br/>and learn to love other people, just like he loves us.");
				break;

			case "temple":
				Msg("This is the sanctuary of Lymilark, the Emain Macha Church.");
				break;

			case "shop_restaurant":
				Msg("'Loch Lios'?<br/>Gordon, the head Chef there, is a brave, dependable person<br/>who participated in a war.<br/>He has also secretly been supporting the Church.");
				Msg("Shena, the waitress, seems very smart and strong in her own right.");
				Msg("Fraser is the only one I don't care for as much.<br/>If he could have been even half like Gordon...");
				break;

			case "shop_armory":
				Msg("A Weapons Shop...<br/>Well, a lady by the name of Osla runs it...<br/>I can't believe a young lady like her would have no fear selling weapons<br/>that encourages slaying other brethrens....Aigh...");
				Msg("Hopefully she'll one day come across the path of Lymilark's teachings,<br/>and really learn from it...");
				break;

			case "shop_government_office":
				Msg("Emain Macha is ruled by the Lord of this land, and<br/>all important administrative proceedings take place inside the castle.<br/>Of course, not just anyone can enter the castle and meet him...");
				break;

			case "graveyard":
				Msg("Graves are where you pay your respects to the dead with a heavy heart.<br/>Recently, there have been some foolish souls fighting at the graves....");
				Msg("It's an idiotic act, one that requires a combination of no fear of death<br/>as well as no respect for the dead.<br/>Please don't do that...");
				break;

			case "bow":
				Msg("Are you asking a Priest about weapons..?");
				break;

			case "tir_na_nog":
				Msg("It's a paradise that was seperated<br/>from this world because of our sins.");
				Msg("It is our duty to follow Lymilark's lead and to apply his teachings to our everyday life<br/>so that one day, Tir Na Nog will descend upon this world.");
				break;

			case "mabinogi":
				Msg("It's a praise song that praises the heroes of the past.<br/>Being a war hero, however, also means the hero has<br/>slayed enough lives to become a hero.");
				Msg("Of course, anyone who became a hero by battling against the Fomors<br/>needs to be commended for the sacrifices the hero has made,<br/>but among those heroes, are some who have had their hands in<br/>a secret, silent feud against Humans... and that worries me...");
				break;

			case "g3_DarkKnight":
				Msg("Dark Knights are the ones who lead the Dorca Feadhains.<br/>When the Army of Darkness<br/>invades Erinn, Dark Knights<br/>will be the ones in the frontlines leading them...");
				Msg("I wonder who will be able to<br/>stand up against these savage monsters...?<br/>So many lives will be lost...");
				Msg("...All I can do is<br/>pray that<br/>that day never comes again...");
				break;

			default:
				RndFavorMsg(
					"I haven't seen that in the scriptures...",
					"Hmmm... I don't really know, so...",
					"Is there such a thing?",
					"...ask Priest James.",
					"I don't think I know what you're talking about...",
					"Is there such a thing?"
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
					"Lymilark will be happy to know that you have given a donation....",
					"Thank you.",
					"Hmm... I'll take it.",
					"You're a faithful believer."
				);
				break;
		}
	}
}
