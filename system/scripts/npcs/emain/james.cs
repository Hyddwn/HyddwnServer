//--- Aura Script -----------------------------------------------------------
// James
//--- Description -----------------------------------------------------------
// Priest in front of Emain Macha Cathedral
//---------------------------------------------------------------------------

public class JamesScript : NpcScript
{
	public override void Load()
	{
		SetRace(10002);
		SetName("_james");
		SetBody(height: 1.1f, upper: 1.3f);
		SetFace(skinColor: 17, eyeType: 7, eyeColor: 97, mouthType: 2);
		SetLocation(52, 40966, 29741, 74);
		SetGiftWeights(beauty: 1, individuality: 0, luxury: 0, toughness: 1, utility: 2, rarity: 0, meaning: 2, adult: 1, maniac: 0, anime: 0, sexy: 0);

		EquipItem(Pocket.Face, 4901, 0x00315266, 0x00F98E38, 0x00E3BB5B);
		EquipItem(Pocket.Hair, 4042, 0x00E0B056, 0x00E0B056, 0x00E0B056);
		EquipItem(Pocket.Armor, 15081, 0x00594933, 0x00696969, 0x00FFFFFF);
		EquipItem(Pocket.Shoe, 17012, 0x00505832, 0x00FFFFFF, 0x004B4B4B);
		EquipItem(Pocket.Head, 18091, 0x00747474, 0x00FFFFFF, 0x00FFFFFF);


		AddPhrase("Lord, I pray for another meaningful day with you.");
		AddPhrase("Who should I write to today...");
		AddPhrase("Try to accomplish small things first. It'll make everything easier.");
		AddPhrase("I need to contact Dunbarton...");
		AddPhrase("I wonder if Comgan is doing alright.");
		AddPhrase("I need to order new candles.");
		AddPhrase("I better clean this place up!");
		AddPhrase("Oh dear, I need to wash my clothes.");
		AddPhrase("Everyone, church is not boring!");
		AddPhrase("Would you like some tea...?");
		AddPhrase("May every adventurer be blessed...");
	}

	protected override async Task Talk()
	{
		SetBgm("NPC_James.mp3");

		await Intro(L("He has pale blonde hair and calm blue eyes,<br/>and his expression seems to give visitors a warm sense of welcome.<br/>You can see his friendly greetings might confuse some people, who expect a clergyman to be much more serious.<br/>If it weren't for his Priest Token, visitors might not even be able to tell that he is a Priest.<br/>His Priest's medallion and stole around his shoulders, however, affirm his position as a Priest."));

		Msg("Welcome. Welcome to the place where you may find peace.", Button("Start a Conversation", "@talk"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());

				if (Player.IsUsingTitle(11001))
				{
					Msg("I don't know how to say this...");
					Msg("But I cannot say anything to you, <username/>,<br/>especially looking into your eyes.<br/>I trust that you must have done a great job indeed.");
				}
				else if (Player.IsUsingTitle(11002))
				{
					Msg("...So, you're the one who did it...<br/>You saved Erinn<br/>from the evil ploy of the Fomors...<br/>...I will always be grateful to you.");
					Msg("......");
					Msg("...I can only hope that this war is over for good,<br/>and that Erinn would truly become a peaceful world...<br/>...But we remain uncertain...");
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
			Msg(FavorExpression(), L("Nice to meet you. I'm Priest James."));
		}
		else if (Memory == 1)
		{
			Msg(FavorExpression(), L("<username/>, right? Great to see you again."));
		}
		else if (Memory == 2)
		{
			Msg(FavorExpression(), L("You look very well today, <username/>."));
		}
		else if (Memory <= 6)
		{
			Msg(FavorExpression(), L("What a day, isn't it?"));
		}
		else
		{
			Msg(FavorExpression(), L("Good to see you often these days, <username/>."));
		}

		UpdateRelationAfterGreet();
	}

	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				Msg(FavorExpression(), "The Church here has dominion over affairs of all the other churches in central Uladh.<br/>There is much more work here as a result, but I have to say that it is definitely more fulfilling too.<br/>I am very happy in my position here.");
				ModifyRelation(Random(2), 0, Random(3));
				break;

			case "rumor":
				Msg(FavorExpression(), "Emain Macha is a very beautiful city!<br/>I especially enjoy taking strolls past the water fountain, and spending time in the Auditorium.<br/>Emain Macha seems immaculate!");
				Msg("Why don't you take a visit?<br/>You will be able to find many interesting people.");
				ModifyRelation(Random(2), 0, Random(3));
				break;

			case "shop_misc":
				Msg("I don't know about a \"General Shop,\"<br/>but Galvin sells general goods at the Observatory.<br/>He sells souvenirs, too, so why don't you pay a visit?");
				break;

			case "shop_grocery":
				Msg("We don't have a grocery store around the area, but we do have the best restaurant in town.<br/>If you follow along the lake towards the west, you will find 'Loch Lios'. You should really check it out.<br/>The waitress is lovely, and the Chefs are amazing.");
				Msg("It is definitely one of the landmarks of Emain Macha.<br/>The food tastes so good..that even Lymilark would be speechless. Haha...");
				break;

			case "shop_healing":
				Msg("The Healer's House is located on the opposite end of the Church.<br/>Just head north, and you'll find it.<br/>Agnes is not only skilled, but she is also very beautiful and polite.<br/>It just makes that much more appealing.");
				break;

			case "shop_inn":
				Msg("From time to time, travelers ask for a stay through the night,<br/>but... Unfortunately, this church isn't like a normal church.<br/>We are too big compared to other churches. So it's difficult for us to keep track of all the visiting travelers.<br/>That is the reason why we do not accept any travelers to this church. It's kind of sad.");
				break;

			case "shop_bank":
				Msg("Are you talking about the Erskin Bank?<br/>Jocelin is a fascinating Banker.<br/>If you seem to have trouble managing your account,<br/>she might give you a lecture. Haha.");
				break;

			case "shop_smith":
				Msg("A Blacksmith's Shop?<br/>There's none around Emain Macha.<br/>Instead, if you are interested in weapons, visit Osla.<br/>Her Weapons Shop is placed at the west of the Square.");
				break;

			case "skill_range":
				Msg("The technique behind hurting your opponent is more than just a physical formula.<br/>Besides, skills for hunting are actually helpful to people.");
				Msg("So don't worry about the skills that you have been learning.");
				break;

			case "skill_instrument":
				Msg("It's a great skill that fills people's hearts with joy.<br/>Have you met Nele at the Square? If you ask him,<br/>he will play you truly beautiful music.");
				Msg("However, for some reason, he never sings.<br/>Seeing how knowledgeable he is,<br/>I bet he knows a lot of songs...<br/>It's very unfortunate.");
				break;

			case "skill_composing":
				Msg("Nele might give you the best answer to that question.<br/>You will see a guy playing a lute around the Square of Emain Macha.");
				break;

			case "skill_tailoring":
				Msg("Why don't you ask Ailionoa at the Clothing Shop?<br/>Even Tara's nobles were amazed by her<br/>talent in making gorgous dresses.");
				Msg("Of course, I'm not guaranteeing that<br/>she is as talented in teaching as well. Haha.");
				break;

			case "skill_magnum_shot":
				Msg("Aodhan seems to know much about that.<br/>He looks kind of unsociable, but he is a very well-mannered, classy young man.<br/>He just needs a nice lady that loves him...");
				Msg("Haha, am I being too meddling of his personal affairs?");
				break;

			case "skill_counter_attack":
				Msg("It is just Human nature to have the urge to get revenge<br/>on someone who's done something to you...<br/>But why don't we start thinking about the reason God gave us intelligence?");
				Msg("If we were unable to decipher the cause of a conflict,<br/>then we would be repeating the conflict with the other party forever.<br/>The conflict would never be resolved unless one side<br/>consciously decided to let things go.<br/>It wouldn't matter just how foolish that might seem to be.");
				Msg("But being able to cut those types of hangups...<br/>That's precisely what we call the power of intelligence in Human beings?");
				break;

			case "skill_gathering":
				Msg("Collecting is the foundation of all economic activity.<br/>Even churches are not exempt to that...<br/>Instead of gathering food ourselves, we are asking people to<br/>help us gather the things that we need.");
				Msg("Of course we payt hem back<br/>with some Holy Water of Lymilark.");
				break;

			case "square":
				Msg("There's a square just a little north of here, where there's a big water fountain.<br/>A lot of people are there, so it won't be difficult to find.");
				break;

			case "pool":
				Msg("This town's water supply is solely dependent on the lake.<br/>We've never had a serious drought that erquired us to build a reservoir.");
				break;

			case "brook":
				Msg("The river across the Sen Mag Plains starts from<br/>a small stream called Adelia.<br/>The stream had a different name before, but because of an incident that occurred there,<br/>the name was apparently changed to commemorate the name of the Nun who was related to that incident.");
				break;

			case "shop_headman":
				Msg("Since we have our Lord, we don't need a Chief.");
				break;

			case "temple":
				Msg("This is the Emain Macha Cathedral.<br/>It's kind of big, I know...");
				Msg("Haha, you're looking at me wondering how this could be seen as \"kind of\" big.<br/>Other than the Cathedral of Tara, the Cathedral of Emain Macha<br/>is indeed the biggest church in Uladh.");
				Msg("This is the main headquarters of the Lymilark church<br/>in Central Uladh.");
				break;

			case "school":
				Msg("It is too bad that the magic school had to close down.<br/>It was where all the great scholars gathered for<br/>passing down knowledge...<br/>But I heard there were real reasons that lead to the school shutting down.");
				break;

			case "skill_campfire":
				Msg("Tired adventurers sit down together,<br/>sharing each other's food, and exchange information and stories...<br/>In that sense, it is a skill that<br/>practices the Lymilark doctrine at its best.");
				break;

			case "shop_restaurant":
				Msg("Have you been to 'Loch Lios' on the westside of the lake?<br/>It is the most famous resstaurant in Emain Macha.<br/>The restaurant is very nicely decorated, and Chef Gordon's cooking<br/>is truly out of this world. It's amazing!");
				break;

			case "shop_armory":
				Msg("Why don't you visit Osla.<br/>She is a very intering person. Once in a while, there are people who get annoyed<br/>by her scribbling habit but...");
				break;

			case "shop_cloth":
				Msg("Ailionoa's clothing Shop. It is located on the eastern end<br/>across from the Square.<br/>It is the building with beautiful curtains and hanging clothes,<br/>which should make it easy to find.");
				Msg("To tell you the truth, Ailionoa made the outfit that bishop Wyllow is wearing.<br/>Don't you find something fancier about his gown compared to the others'?<br/>Hahaha...");
				break;

			case "shop_bookstore":
				Msg("It's going to be difficult to buy regular books<br/>especially because the book store is closed.");
				Msg("The Emain Macha church has a big library, but<br/>sadly, visitors cannot get in.<br/>Only apprentice Priests who study here, and<br/>regular Priests can get into our library.");
				break;

			case "shop_government_office":
				Msg("Town administration is handled inside the castle.<br/>So, there's no town office outside of the castle.<br/>If you are looking for lost items,<br/>then please visit Galvin.");
				break;

			case "graveyard":
				Msg("There's no graveyard inside the town of Emain Macha.<br/>During the War, Emain Macha was right near the biggest battlefield<br/>which left Emain Macha with the greatest death toll compared to other cities.");
				Msg("Most of the casualties were buried as is,<br/>leaving Emain Macha to be a gravesite without any tombstones.<br/>In place of tombstones for those who had perished a memorial tower was built much later.");
				Msg("We shouldn't forget that<br/>this beautiful and peaceful town<br/>owes its prosperity to the sacrifice of all those forgotten warriors.");
				break;

			case "skill_fishing":
				Msg("Fishing, I love it. You get peace of mind.<br/>From time to time, I sneak out during the night<br/>and enjoy fishing by the lake. Haha...");
				break;

			case "lute":
				Msg("I think I've seen Nele selling one of them.<br/>I don't know if he makes them himself,<br/>but he certainly knows how to handle them very well.");
				break;

			case "complicity":
				Msg("Sometimes people say that I don't look like a Priest,<br/>and say that I look more like an instigator instead...<br/>Haha, do you think so, too? <br/>Well, if everyone were to approach the god of Lymilark that way,<br/>with such familiarity, then I wouldn't mind being seen as an instigator afterall.<br/>It could even be a compliment for me.");
				break;

			case "tir_na_nog":
				Msg("It is a land where limitless happiness and youth exist.<br/>It is written that Erinn and Tir Na Nog were the same place,<br/>but that Tir Na Nog was seperated because of our sins.");
				Msg("People commited various sins throughout their lives, but<br/>the greatest sin of all was<br/>that we didn't even know that we were committing sins.");
				Msg("When we are able to recognize our sins and repent,<br/>that will be when Tir Na Nog will descend upon our world.");
				break;

			case "mabinogi":
				Msg("Mabinogis are narrative songs about legendary heroes.<br/>The only song I know is about King Nuadha, the Golden Arm...<br/>Haha, don't ask me to sing that song. Ask a Bard.");
				break;

			case "breast":
				Msg("For what purpose are you asking? Haha...");
				break;

			case "errand_of_fleta":
				if (!HasItem(70075))
				{
					Send.Notice(Player, ("Received Angelic Herb from James."));
					Msg("Herb grown by the angels?<br/>Haha... Actually I do have something by that name.<br/>It's an herb called Angelica.");
					Msg("Just as the word 'angel' is part of its name,<br/>its aroma is quite heavenly.<br/>It is used to sweeten the smell of alcohol or even for cake decorations.");
					Msg("I happen to have some,<br/>so I'll give you some if you need it.");
					Msg("Here you go.<br/>Don't just waste it anywhere... Use it wisely.");
					GiveItem(70075);
				}
				else
					Msg("Wait a minute, you already have some Angelica.<br/>Did I give you some earlier?<br/>Haha, I must be losing my memory.<br/>Well, use it wisely.");
				break;

			case "g3_DarkKnight":
				Msg("Although you've successfully<br/>prevented Macha's rebirth...");
				Msg("All these rumors of<br/>Dark Knights I'm hearing lately<br/>give me a bad feeling about what's going on...");
				break;

			case "EG_C2_Unknown_Continet":
				Msg("There is not a whole lot of interchange between Aliech and other nations, it's true but...<br/>I have yet to hear anything about any uncharted continents in existence.");
				Msg("I've even worked as a missionary by the harbor for a short period of time<br/>but most of our interchanges involved other vessels<br/>that belonged to nations that were already known to us.");
				Msg("I may be able to ask some of the people we'd met at that time<br/>and find out something.<br/>Let me write a letter... it's been a while since I've written one.");
				break;

			default:
				RndFavorMsg(
					"That is an interesting story. Hmm...",
					"That... I don't know much about.",
					"Well, well... I'm sorry, I don't know.",
					"Huh... That... Sorry I don't have the answer.",
					"Is that right? Interesting..."
				);
				ModifyRelation(0, 0, Random(3));
				break;
		}
	}

	protected override async Task Gift(Item item, GiftReaction reaction)
	{
		switch (reaction)
		{
			case GiftReaction.Neutral:
				RndMsg(
					"Are you donating this? Of course I'm more than happy to receive this.",
					"Thanks. I appreciate your kindness.",
					"This is... Wow, thank you very much.",
					"This will be very helpful for our church.",
					"This isn't left over, isn't it? Haha, just kidding."
				);
				break;
		}
	}
}