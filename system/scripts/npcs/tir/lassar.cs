//--- Aura Script -----------------------------------------------------------
// Lassar
//--- Description -----------------------------------------------------------
// The Magic Instructor in Tir Chonaill School
//---------------------------------------------------------------------------

public class LassarScript : NpcScript
{
	public override void Load()
	{
		SetRace(10001);
		SetName("_lassar");
		SetBody(height: 1.1f);
		SetFace(skinColor: 15, eyeType: 153, eyeColor: 25, mouthType: 2);
		SetStand("human/female/anim/female_natural_stand_npc_lassar02");
		SetLocation(9, 2020, 1537, 202);
		SetGiftWeights(beauty: 2, individuality: 0, luxury: 2, toughness: 0, utility: 0, rarity: 0, meaning: 2, adult: 1, maniac: 0, anime: 0, sexy: 2);

		EquipItem(Pocket.Face, 3900, 0x0087C5EC, 0x00D5E029, 0x0001945D);
		EquipItem(Pocket.Hair, 3144, 0x00D25D5D, 0x00D25D5D, 0x00D25D5D);
		EquipItem(Pocket.Armor, 15657, 0x00394254, 0x00394254, 0x00574747);
		EquipItem(Pocket.Shoe, 17285, 0x00394254, 0x00000000, 0x00000000);
		EquipItem(Pocket.RightHand1, 40418, 0x00808080, 0x00000000, 0x00000000);
		EquipItem(Pocket.LeftHand1, 46023, 0x00808080, 0x00000000, 0x00000000);

		AddPhrase("....");
		AddPhrase("And I have to supervise an advancement test.");
		AddPhrase("Come to think of it, I have to come up with questions for the test.");
		AddPhrase("Funny, I had never thought I would do this kind of work when I was young.");
		AddPhrase("I hope I can go gather some herbs soon...");
		AddPhrase("I should put more clothes on. It's kind of cold.");
		AddPhrase("I think I'll wait a little longer...");
		AddPhrase("Is there any problem in my teaching method?");
		AddPhrase("Perhaps I could take today off...");
		AddPhrase("The weather will be fine for some time, it seems.");
		AddPhrase("Will things get better tomorrow, I wonder?");
	}

	protected override async Task Talk()
	{
		SetBgm("NPC_Lassar.mp3");

		await Intro(L("Waves of her red hair come down to her shoulders.<br/>Judging by her somewhat small stature, well-proportioned body, and a neat two-piece school uniform, it isn't hard to tell that she is a teacher.<br/>The intelligent look in her eyes, the clear lip line and eyebrows present her as a charming lady."));

		Msg("Is there anything I can help you with?", Button("Start a Conversation", "@talk"), Button("Shop", "@shop"), Button("Repair Item", "@repair"), Button("Upgrade item", "@upgrade"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());

				if (Player.IsUsingTitle(10061)) // is a friend of Malcolm
				{
					var today = ErinnTime.Now.ToString("yyyyMMdd");
					if (today != Player.Vars.Perm["lassar_title_gift"])
					{
						Player.Vars.Perm["lassar_title_gift"] = today;

						Player.GiveItem(51006, 3); // MP 10 Potion x3
						Player.Notice(L("Received MP 10 Potion from Lassar."));
						Player.SystemMsg(L("Received MP 10 Potion from Lassar."));

						Msg(L("Hahaha. I was wondering who you were.<br/>You must be Malcolm's friend, <username/>, right?<br/>I would like to give you this MP Potion.<br/>Will you accept it?"));
					}
				}
				else if (Player.IsUsingTitle(11001))
				{
					Msg("Hmm... So you rescued the Goddess?<br/>And... that means you've done something the Three Missing Warriors couldn't do, right?<br/>This is a bit hard to believe. Hahaha...");
					Msg("If you saved the Goddess, why hasn't she descend down upon Erinn as of yet?");
				}
				else if (Player.IsUsingTitle(11002))
				{
					Msg("Hm? <username/>, you're the Guardian of Erinn?<br/>Are you <username/>, the one<br/>who used to train magic and combat here?");
					Msg("...Wow... I'm amazed.<br/>I never knew a day like this would come.");
					Msg("Congratulations. <username/>. Hehe.");
				}

				await Conversation();
				break;

			case "@shop":
				Msg("If you want to learn magic, you've come to the right place.");
				OpenShop("LassarShop");
				return;

			case "@repair":
				Msg("You want to repair a magic weapon?<br/>Don't ask Ferghus to repair magic weapons. Although he won't even do it..<br/>I can't imagine what would happen...if you tried to repair it like a regular weapon.<br/><repair rate='92' stringid='(*/magic_school_repairable/*)' />");

				while (true)
				{
					var repair = await Select();

					if (!repair.StartsWith("@repair"))
						break;

					var result = Repair(repair, 92, "/magic_school_repairable/");
					if (!result.HadGold)
					{
						RndMsg(
							"You need more money.",
							"I don't think you have enough money."
						);
					}
					else if (result.Points == 1)
					{
						if (result.Fails == 0)
							RndMsg(
								"I didn't make any mistakes.",
								"You have repaired 1 point."
							);
						else
							RndMsg( // Should be 3
								"There's been some mistakes.",
								"Repair has been finished, but I made some mistakes."
							);
					}
					else if (result.Points > 1)
					{
						if (result.Fails == 0)
							RndMsg(
								"I didn't make any mistakes. Hehe",
								"It has been repaired perfectly!",
								"It have been repaired completely. Hehe" // 100% Official http://i.imgur.com/GQH4uOT.png
							);
						else
						{
							// TODO: Use string format once we have XML dialogues.
							Msg("I did repair it but...<br/>" + result.Fails + " point(s) have not been repaired.<br/>I guess I need more training.");
							// I got this message when doing a full repair with only 2 points being repaired, and one failed
							// Msg("Repair has been finished, but I made some mistakes.");
						}
					}
				}

				Msg("Good luck studying magic!<repair hide='true'/>");
				break;

			case "@upgrade":
				Msg("You're looking to upgrade something?<br/>Hehe, how smart of you to come to a magic school teacher.<br/>Let me see what you're trying to upgrade.<br/>You know that the amount and type of upgrade available differs with each item, right?<upgrade />");

				while (true)
				{
					var reply = await Select();

					if (!reply.StartsWith("@upgrade:"))
						break;

					var result = Upgrade(reply);
					if (result.Success)
						Msg("It was easier than I expected.<br/>Do you want anything else upgraded?");
					else
						Msg("(Error)");
				}

				Msg("I'll see you next time then.<upgrade hide='true'/>");
				break;
		}

		End("Goodbye, <npcname/>. I'll see you later!");
	}

	private void Greet()
	{
		if (Memory <= 0)
		{
			Msg(FavorExpression(), L("Umm... Are you <username/>, by any chance?<p>Hahaha! You look just like what Bebhinn described.<br/>Excuse my laughing.<br/>Good to meet you.<br/>I am <npcname/>."));
		}
		else if (Memory == 1)
		{
			Msg(FavorExpression(), L("We met before, right?<br/>Yes, I am <npcname/>."));
		}
		else if (Memory == 2)
		{
			Msg(FavorExpression(), L("<username/>, right?<br/>Good to see you again."));
		}
		else if (Memory <= 6)
		{
			Msg(FavorExpression(), L("We meet again, <username/>."));
		}
		else
		{
			Msg(FavorExpression(), L("We run into each other frequently these days, <username/>."));
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
					Msg(FavorExpression(), "Hmm... Do I look that cold-hearted? Actually, it concerns me a little.<br/>How would you say I look, <username/>?");
					ModifyRelation(Random(2), 0, Random(2));
				}
				else if (Memory >= 15 && Favor >= 30 && Stress <= 5)
				{
					Msg(FavorExpression(), "Since childhood, I believed I was good at magic.<br/>I studied hard.<br/>And I even got to study in the southern city of Emain Macha...");
					Msg("But, there, I found many students who were a lot more capable than I.<br/>Eventually, I decided to come back to my hometown<br/>and foster apprentices who are more talented.");
					ModifyRelation(Random(2), 0, Random(2));
				}
				else if (Favor >= 10 && Stress <= 10)
				{
					Msg(FavorExpression(), "Do you want to learn magic? The tuition might be a little steep, but why don't you enroll in a class?");
					ModifyRelation(Random(2), Random(2), Random(2));
				}
				else if (Favor <= -10)
				{
					Msg(FavorExpression(), "People seem to avoid me because I am a magic teacher.<br/>You know. People may think that, if I hate someone,<br/>I would use magic to harm that person.");
					ModifyRelation(Random(2), 0, Random(1, 3));
				}
				else if (Favor <= -30 && Stress <= 10)
				{
					Msg(FavorExpression(), "The power of magic fundamentally comes from balance.<br/>The power of magic that breaks the balance may gain power temporarily<br/>but, in the end, it hurts the caster.");
					ModifyRelation(Random(2), 0, Random(1, 3));
				}
				else if (Favor <= -30 && Stress > 10)
				{
					Msg(FavorExpression(), "Did you ever think that you might be bothering me?");
					ModifyRelation(Random(2), -Random(2), Random(1, 4));
				}
				else
				{
					Player.GiveKeyword("school");
					Msg(FavorExpression(), "<npcname/> means 'flame'.<br/>My mother gave birth to me after having dreamed about a wildfire burning the field.");
					ModifyRelation(Random(2), 0, Random(3));
				}
				break;

			case "rumor":
				// Field Boss Info
				// Have you heard that Gigantic Black Wolf appeared at Southern Plains of Tir Chonaill?<br/>I wonder if everything's okay...

				if (Memory >= 15 && Favor >= 50 && Stress <= 5)
				{
					Msg(FavorExpression(), "Hmm... We may argue at times, but Dilys and I are old friends.<br/>Still, I cannot forgive her for not paying my money back...");
					ModifyRelation(Random(2), 0, Random(2));
				}
				else if (Memory >= 15 && Favor >= 30 && Stress <= 5)
				{
					Msg(FavorExpression(), "I hear Dilys would stop treating patients and only sell<br/>potions until she gets some treatment tables.<br/>She does everything she pleases, don't you think?<br/>You know your occupation pays you too much when...");
					ModifyRelation(Random(2), 0, Random(2));
				}
				else if (Favor >= 10 && Stress <= 10)
				{
					Msg(FavorExpression(), "The grass at the back of the School is herbs<br/>that we grow with students to be used as magic ingredients.<br/>It takes far more work than meets the eye.");
					Msg("It smells good, though, and it makes the atmosphere here tranquil.<br/>So I believe it is worth the work.");
					ModifyRelation(Random(2), Random(2), Random(2));
				}
				else if (Favor <= -10)
				{
					Msg(FavorExpression(), "It seems to me that Priestess Endelyon doesn't take magic power seriously<br/>But, in fact, even the power of gods has something in common with magic power.<br/>It is not right to put what you like above what someone else cherishes<br/>and then look down on that person.");
					Msg("Then again, Priestess Endelyon wouldn't do that...");
					ModifyRelation(Random(2), 0, Random(1, 3));
				}
				else if (Favor <= -30 && Stress <= 10)
				{
					Msg(FavorExpression(), "Be careful. People who aren't careful enough at night<br/>sometimes fall and get seriously hurt.<br/>If you get hurt, it is you who must bear the consequence. Well, maybe not Dilys.");
					ModifyRelation(Random(2), 0, Random(1, 3));
				}
				else if (Favor <= -30 && Stress > 10)
				{
					Msg(FavorExpression(), "Is there no one else to ask but me?<br/>I am just a magic teacher,<br/>not a person who counts which person owns how many clothes.");
					ModifyRelation(Random(2), -Random(2), Random(1, 4));
				}
				else
				{
					Player.GiveKeyword("farmland");
					Msg(FavorExpression(), "Farmland is just to the south of the School.<br/>They mainly grow wheat or barley, and the crop yields are enough<br/>for the people in Tir Chonaill.<br/>But I think there will be a shortage if travelers stay longer.");
					Msg("That means no stealing crops for you!");
					ModifyRelation(Random(2), 0, Random(3));
				}
				break;

			case "about_skill":
				Msg("You must be <username/> the so-called Elemental Master.<br/>How is your magic training going?<br/><title name='NONE'/>(You tell her about all the skills you've learned so far.)");
				Msg("Aha. You know the Smash skill.<br/>It seems to me that you have a great interest in combat magic...<br/>Is that why you're here?");
				Msg("Hmm. I am afraid I can't do that...<br/>Magic doesn't exist solely to kill.<br/>If you really are interested in such magic, then<br/>why don't you talk to Stewart?");
				Msg("Wait wait wait. Even if you can prove that you are an Elemental Master,<br/>you can't simply show up and tell him that you are interested in advanced magic.<br/>He probably wouldn't give you the time of day!");
				Msg("I'm only telling you this because it's you...<br/>But first, you must show Stewart the title of the Elemental Master.<br/>If you are really interested in magic-based combat, then<br/>you shouldn't have any trouble, eh?");
				Msg("Good luck...ha ha ha.");
				break;

			//case "about_study": // Handled in quest script

			case "shop_misc":
				Msg("You're going to have to walk a long distance.<br/>Follow this road to the left, and you'll see the Square.<br/>The General Shop is around there.<br/>If you can't find it, ask other people near the Square.");
				break;

			case "shop_grocery":
				Msg("When the smell of Caitin's cooking reaches here,<br/>I sometimes wonder if it's a restaurant rather than a grocery store.<br/>The Grocery Store is located over the hill and just to your right.<br/>If you cannot find it, look at the Minimap.");
				Msg("Hee hee. Don't worry. It is nearby so you can't miss it.");
				break;

			case "shop_healing":
				Msg("Healer's House? You must be talking about Dilys' house.<br/>Pass the Square and go straight down the road.<br/>The healer, Dilys, and I have known each other for a long time.<br/>You can trust her.");
				Msg("But she seems like a quack sometimes, so be careful. Tee hee...");
				break;

			case "shop_inn":
				Msg("What's interesting is... That so many customers go to the Inn,<br/>but the Inn is never fully booked.<br/>I wonder if they have a magic pathway or something.<br/>I don't remember learning that kind of magic before...");
				break;

			case "shop_bank":
				Msg("The Bank? Go up this way just a little and it's right there.<br/>It is not going to be easy talking with Bebhinn, but try to be patient with her... Hee hee.");
				Msg("By the way, I think you are supposed to pay interest to your deposit.<br/>It's weird that the Bank charges fees for deposits...<br/>What do you think?");
				break;

			case "shop_smith":
				Msg("If your equipment is destroyed<br/>or broken, you should pay a visit.<br/>Ferghus will help you.");
				break;

			case "skill_range":
				Msg("There are many kinds of long range attacks.<br/>But as far as magic is concerned,<br/>there are three basic elemental spells<br/>we can talk about.");
				Msg("Firebolt, Icebolt and Lightningbolt.<br/>These are the three.<br/>You can learn all of them if you take a class here.");
				Msg("Oh! If you want to teach yourself,<br/>you can buy the necessary books here.<br/>Of course, it just won't be the same as taking a class.");
				Msg("Hmm.<br/>By the way, weren't you on your way to the Blacksmith's Shop<br/>to buy the stuff Ranald asked you to get?<br/>If you are not interested in magic, feel free to leave. Hehehe.");
				break;

			case "skill_instrument":
				Msg("Have you forgotten?<br/>Endelyon knows about it.<br/>She's a priestess at Church.");
				break;

			case "skill_tailoring":
				Msg("Tailoring skill is just what the name implies.<br/>It is a skill to make clothes with fabric.<br/>Well... You would be better off talking with Caitin<br/>than me because I am so terrible at sewing.");
				Msg("Go and ask Caitin<br/>and do what she says. You will never be disappointed.");
				Msg("(Hmm... Perhaps I could create a magic needle?)");
				break;

			case "skill_magnum_shot":
				Msg("With that,<br/>Ranald over there is an expert.<br/>Go and talk with him,<br/>and you will get some useful information. Hmm...");
				Msg("(What kind of rumor is Dilys<br/>spreading about me?)");
				break;

			case "skill_counter_attack":
				Msg("With that, Ranald over there is an expert.<br/>Go and talk with him,<br/>and you will get some useful information. Hmm...");
				Msg("(Why are so many people asking me<br/>about Martial Arts these days?)");
				break;

			case "skill_smash":
				Msg("Have you ever talked to Ranald?<br/>He's just around the corner!");
				break;

			case "skill_gathering":
				Msg("Gathering is a very important skill.<br/>Even more for those who study magic.");
				Msg("You don't really need to learn it separately. Anyone with the right tools<br/>can immediately use the skill.<br/>You can often get the necessary materials for magic<br/>from nature.");
				break;

			case "square":
				Msg("No way! You must be kidding!<br/>Everyone knows where the Square is.<br/>Hee hee. Or are you flirting with me<br/>by trying to talk about anything?");
				break;

			case "pool":
				Player.GiveKeyword("windmill");
				Msg("The reservoir? Just over there. Hee hee.<br/>It was built to supply water to the farmland.<br/>People say that having a mill<br/>made it far easier to make it.");
				Msg("But, if you stay for too long near the reservoir,<br/>a water ghost might appear.<br/>You know, like Undine. Hee hee.");
				break;

			case "farmland":
				Msg("If you go down the road a little, it will be all farmlands.<br/>Be careful not to hurt the crops.");
				break;

			case "windmill":
				Player.GiveKeyword("brook");
				Msg("The mill is near the Adelia Stream.<br/>My sister Alissa works there.");
				Msg("She is kind of stubborn, but she's about that age, you know? Hee hee.");
				Msg("Working at the mill can be deceptively stressful...<br/>The mill draws water from<br/>Adelia Stream to the reservoir.");
				Msg("Sometimes it grinds crops.<br/>It might be fun to go sometime<br/>and watch it in action.");
				break;

			case "brook":
				Msg("It is said that Adelia Stream is named<br/>after a priestess who lived here long ago...<br/>I wonder what kind of great work she's accomplished<br/>to have the stream named after her.");
				Msg("Well, what I mean is,<br/>even with the help of Lymilark,<br/>I cannot fully understand how the priestess,<br/>as a woman, managed to fight monsters.");
				Msg("Think about it. There were probably many healthy and robust warriors.<br/>Why would a delicate pacifist priestess<br/>have fought against such monsters?");
				break;

			case "shop_headman":
				Player.GiveKeyword("square");
				Msg("You are looking for the Chief's House?<br/>The Chief's House is right over the hill from the Square.<br/>So you haven't been there but came here right away?");
				Msg("The Chief might be disappointed.<br/>You should go now!");
				break;

			case "temple":
				Msg("Hey! Church is just over there.<br/>You knew that already, didn't you?");
				Msg("I understand you want to<br/>keep flirting with me as long as you can, but...<br/>Hehehe...");
				break;

			case "school":
				Msg("*Laugh* You're right. This is the School.<br/>If you want to take a class,<br/>start a conversation<br/>by using the keyword [Class and Training].");
				break;

			case "skill_windmill":
				Msg("The Windmill skill?<br/>Well,<br/>if Master Ranald doesn't know it,<br/>who else would in this town?");
				break;

			case "skill_campfire":
				Msg("Don't waste your time on learning a useless skill like Campfire.<br/>Learn the Firebolt skill instead! The Firebolt skill!<br/>Same but a lot more useful.<br/>You can easily learn it from a book here!");
				break;

			case "shop_restaurant":
				Player.GiveKeyword("shop_grocery");
				Msg("Hmm. You're hungry?<br/>Actually, I am hungry too.<br/>We don't have a restaurant, so why don't you go to the Grocery Store<br/>and buy some food? We can share the food.");
				break;

			case "shop_armory":
				Player.GiveKeyword("shop_smith");
				Msg("Ranald has asked you to buy a weapon, right?<br/>But, to buy weapons in this town,<br/>you need to go to the Blacksmith's Shop.");
				break;

			case "shop_cloth":
				Player.GiveKeyword("shop_misc");
				Msg("Hmm... You seem more clothing-savvy than you look.<br/>We just might get along really well... Hehehe...<br/>But this town is so rural<br/>that you need to go to the General Shop to buy clothing.");
				Msg("Hmm... It would be really nice if someone opened a boutique...");
				break;

			case "shop_bookstore":
				Msg("Someone like you looking for books? Now, that's unexpected.");
				Msg("Hahaha! I'm just kidding. Don't make that kind of face at me.<br/>There is no bookstore in this town.<br/>Not too many people here like reading.");
				Msg("How do I know?<br/>Actually, I sell magic books.<br/>And few people ever bought any.");
				break;

			case "shop_goverment_office":
				Player.GiveKeyword("shop_healing");
				Msg("Huh? Did you say town office?<br/>Oh, you must have been to a city before.");
				Msg("Which one have you been in? Emain Macha? Tara?<br/>Or Dunbarton?<br/>Oh. Nothing, really.<br/>It's just that I myself studied in a city.");
				Msg("You know Emain Macha, don't you? Hee hee.");
				Msg("Dilys up there<br/>studied in the same city.<br/>You should go see her sometime. She is at the Healer's House.");
				break;

			case "graveyard":
				Msg("Hmm... Want to go to the graveyard, do you?<br/>Middle of the night is the best time to visit the graveyard! Hahaha!");
				Msg("Just kidding. Don't look at me like that.<br/>The graveyard is near the hill that the Chief's House is on.");
				Msg("They say that those who sacrificed their lives<br/>to protect this town with a long history of Partholon are buried there.<br/>But, it feels rather spooky.<br/>So, people don't like to go there.");
				Msg("That's why it's great fun to take turns<br/>going there and find items at night to test your guts.");
				break;

			case "bow":
				Msg("You can buy a bow at the Blacksmith's Shop.<br/>In this town, weapons are usually sold there.");
				Msg("Hmm. By the way, why should I inform you of something like this?<br/>Doesn't Ranald tell you things like this?<br/>... Perhaps he looks down on me 'cause I'm a woman...");
				Msg("Oh, forget I said that. Hehehehe");
				break;

			case "lute":
				Msg("A lute?<br/>Malcolm at the General Shop sells lutes.<br/>I guess Priestess Endelyon talked to you,<br/>but you came the wrong way.");
				Msg("Go in the opposite direction and go toward the Square.<br/>A lute will be useful in many ways.");
				break;

			case "tir_na_nog":
				Msg("Hmm... When I was young, I heard town folks say<br/>that Tir Na Nog was simply an utopia.<br/>But what I learned at Emain Macha<br/>was different.");
				Msg("Tir Na Nog is a lofty world of magic<br/>and of life where<br/>the powers of Gods and of magic are united.");
				Msg("Of course, I am not sure<br/>whether I fully understand what it means.<br/>But then, who else would?");
				Msg("I think it is a world<br/>well beyond my imagination...");
				break;

			case "mabinogi":
				Msg("You seem fairly interested in Mabinogi.<br/>As far as legends about swords and magic are concerned,<br/>it is better to learn from word of mouth than from dry books.");
				Msg("As you know, when a story moves on from people to people,<br/>it is naturally exaggerated<br/>with its original meaning distorted.");
				Msg("One of the best ways to keep the original form of<br/>a story intact is to preserve it through music.");
				Msg("Of course, that doesn't mean<br/>that the story is delivered in exactly the same form.<br/>But it would be helpful for you to understand Mabinogi this way.");
				break;

			case "musicsheet":
				Msg("If you want Music Scores, you should go to Malcolm's General Shop.<br/>Of course, they aren't free. There is no such thing as a free lunch.");
				break;

			case "jewel":
				Msg("I love Topaz the most among all the gems<br/>because it feels cool to the touch,<br/>like the air during the Eweca hours.");
				break;

			default:
				if (Memory >= 15 && Favor >= 30 && Stress <= 5)
				{
					Msg(FavorExpression(), "I don't really know much about this. I will find someone who can help you with it.<br/>It probably won't be easy, though.");
					ModifyRelation(0, 0, Random(2));
				}
				else if (Favor >= 10 && Stress <= 10)
				{
					Msg(FavorExpression(), "Actually, I don't really know too much about stuff like that.<br/>Oh, but don't think that I'm strange or anything.");
					ModifyRelation(0, 0, Random(2));
				}
				else if (Favor <= -10)
				{
					Msg(FavorExpression(), "Please get out of my way.");
					ModifyRelation(0, 0, Random(4));
				}
				else if (Favor <= -30)
				{
					Msg(FavorExpression(), "Please get out of my way. And shut your mouth while you're at it!");
					ModifyRelation(0, 0, Random(5));
				}
				else
				{
					RndFavorMsg(
						"Hmm... I don't know.",
						"Honestly, I don't know much.",
						"Being a teacher doesn't necessarily mean knowing everything.",
						"Why don't you ask other people? I am afraid I would be of little help.",
						"I thought I knew. But it is more difficult to actually explain it than I thought."
					);
					ModifyRelation(0, 0, Random(3));
				}
				break;
		}
	}

	protected override async Task Gift(Item item, GiftReaction reaction)
	{
		if (item.Info.Id >= 64042 && item.Info.Id <= 64050) // Gems
		{
			var size = item.MetaData1.GetFloat("SCALE") * 10;
			var rnd = Random(100);

			switch (item.Info.Id)
			{
				case 64042: // Topaz
					if (size >= 7)
						Msg(L("Wow, is this for me?<br/>How did you know that I love Topaz?<br/>Oh my, look at this size! Thank you so much. I'd like to give you this in return.<br/>I hope you'll like it."));
					else
						Msg(L("Wow, is this for me?<br/>I love Topaz!<br/>Thank you so much. I'd like to give you this in return.<br/>I hope you'll like it."));

					if (rnd < size)
					{
						switch (Random(2))
						{
							case 0:
								Player.GiveItem(Item.CreateEnchant(30706, 600)); // Formal Enchant
								Player.SystemNotice(L("You have received an Enchant Scroll from Lassar."));
								break;

							case 1:
								Player.GiveItem(62014); // Spirit Weapon Restoration Potion
								Player.SystemNotice(L("You have received a Spirit Weapon Restoration Potion from Lassar."));
								break;
						}
					}
					else
					{
						switch (Random(2))
						{
							case 0:
								Player.GiveItem(63001); // Wings of a Goddess
								Player.SystemNotice(L("You have received Wings of a Goddess from Lassar."));
								break;

							case 1:
								Player.GiveItem(62001); // Elite Magic Powder
								Player.SystemNotice(L("You have received Elite Magic Powder from Lassar."));
								break;
						}
					}
					break;

				case 64050: // Diamond
					Msg(L("Wow, are you really giving this to me?<br/>Are you sure? You know this is a valuable Diamond, right?<br/>Oh! Look at how the light refracts all the colors of the rainbow... It's no wonder it's called the queen of all gems.<br/>I didn't know you would care so much for me. Hoho.<br/>I should give you something in return... Oh, I've got this. Would you like to have it?"));

					var doubleSize = size * size;
					if (rnd < doubleSize)
					{
						switch (Random(3))
						{
							case 0:
								Player.GiveItem(Item.CreateEnchanted(40038, 308)); // Deadly Lightning Wand
								Player.SystemNotice(L("You have received a Deadly Lightning Wand from Lassar."));
								break;

							case 1:
								Player.GiveItem(Item.CreateEnchanted(40039, 308)); // Deadly Ice Wand
								Player.SystemNotice(L("You have received a Deadly Ice Wand from Lassar."));
								break;

							case 2:
								Player.GiveItem(Item.CreateEnchanted(40040, 308)); // Deadly Fire Wand
								Player.SystemNotice(L("You have received a Deadly Fire Wand from Lassar."));
								break;
						}
					}
					else if (rnd < doubleSize * 2)
					{
						switch (Random(3))
						{
							case 0:
								Player.GiveItem(40038); // Lightning Wand
								Player.SystemNotice(L("You have received a Lightning Wand from Lassar."));
								break;

							case 1:
								Player.GiveItem(40039); // Ice Wand
								Player.SystemNotice(L("You have received an Ice Wand from Lassar."));
								break;

							case 2:
								Player.GiveItem(40040); // Fire Wand
								Player.SystemNotice(L("You have received a Fire Wand from Lassar."));
								break;
						}
					}
					else
					{
						Player.GiveItem(51009, 10); // MP 100 Potion x10
						Player.SystemNotice(L("You have received 10 Mana 100 Potions from Lassar."));
					}
					break;

				default:
					Msg(L("Wow, what's this?<br/>Oh, it's a pretty gem. Thanks...<br/>(She looks disappointed somehow. I guess she doesn't really like this gem.)"));
					break;
			}
		}
		else
		{
			switch (reaction)
			{
				case GiftReaction.Love:
					Msg(L("Hmm, you're not going to ask for it back, are you?<br/>No way!<br/>Now that you've given it to me, it belongs to me. Hee hee."));
					break;

				case GiftReaction.Like:
					RndMsg(
							L("Ha ha. You are kind of cute."),
							L("You aren't my type, though.<br/>Hey! Just because I said that, don't even think about taking it back.")
						);
					break;

				default: // GiftReaction.Neutral
					RndMsg(
							L("Hmm."),
							L("Thanks.")
						);
					break;
			}
		}
	}
}

public class LassarShop : NpcShopScript
{
	public override void Setup()
	{
		Add("Magic Items", 62001, 1);  // Elite Magic Powder x1
		Add("Magic Items", 62001, 10); // Elite Magic Powder x10
		Add("Magic Items", 62003, 1);  // Blessed Magic Powder x1
		Add("Magic Items", 62003, 10); // Blessed Magic Powder x10
		Add("Magic Items", 62012);     // Elemental Remover
		Add("Magic Items", 63000, 1);  // Phoenix Feather x1
		Add("Magic Items", 63000, 10); // Phoenix Feather x10
		Add("Magic Items", 63001, 1);  // Wings of a Goddess x1
		Add("Magic Items", 63001, 5);  // Wings of a Goddess x5

		Add("Magic Book", 1007); // Healing: The Basics of Magic
		Add("Magic Book", 1008); // Icebolt Spell: Origin and Training
		Add("Magic Book", 1009); // A Guidebook on Firebolt

		Add("Magic Weapon", 40038); // Lightning Wand
		Add("Magic Weapon", 40039); // Ice Wand
		Add("Magic Weapon", 40040); // Fire Wand
		Add("Magic Weapon", 40041); // Combat Wand
		Add("Magic Weapon", 40090); // Healing Wand

		if (IsEnabled("SpiritWeapons"))
			Add("Magic Items", 62014); // Spirit Weapon Restoration Potion

		if (IsEnabled("WandUpgradeAndChainCasting"))
		{
			Add("Magic Weapon", 40231); // Crystal Lightning Wand
			Add("Magic Weapon", 40232); // Crown Ice Wand
			Add("Magic Weapon", 40233); // Phoenix Fire Wand
			Add("Magic Weapon", 40234); // Tikka Wood Healing Wand
		}
	}
}
