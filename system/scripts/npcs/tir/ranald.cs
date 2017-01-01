//--- Aura Script -----------------------------------------------------------
// Ranald
//--- Description -----------------------------------------------------------
// The Combat Instructor located outside Tir's School
//---------------------------------------------------------------------------

public class RanaldScript : NpcScript
{
	public override void Load()
	{
		SetRace(10002);
		SetName("_ranald");
		SetBody(upper: 1.1f);
		SetFace(skinColor: 20);
		SetStand("human/male/anim/male_natural_stand_npc_ranald02", "human/male/anim/male_natural_stand_npc_ranald_talk");
		SetLocation(1, 4651, 32166, 195);
		SetGiftWeights(beauty: 0, individuality: 0, luxury: -1, toughness: 2, utility: 1, rarity: 0, meaning: 0, adult: 2, maniac: 0, anime: 0, sexy: 1);

		EquipItem(Pocket.Face, 4900, 0xF88B4A);
		EquipItem(Pocket.Hair, 4154, 0x4D4B53);
		EquipItem(Pocket.Armor, 15652, 0xAC9271, 0x4D4F48, 0x7C6144);
		EquipItem(Pocket.Shoe, 17012, 0x9C7D6C, 0xFFC9A3, 0xF7941D);
		EquipItem(Pocket.LeftHand1, 40012, 0xDCDCDC, 0xC08B48, 0x808080);

		AddPhrase("I need a drink...");
		AddPhrase("I guess I drank too much last night...");
		AddPhrase("I need a nap...");
		AddPhrase("I should drink in moderation...");
		AddPhrase("I should sharpen my blade later.");
		AddPhrase("It's really dusty here.");
		AddPhrase("What's with the hair styles of today's kids?");
	}

	protected override async Task Talk()
	{
		SetBgm("NPC_Ranald.mp3");

		await Intro(L("From his appearance and posture, there is no doubt that he is well into middle age, but he is surprisingly well-built and in good shape.<br/>Long fringes of hair cover half of his forehead and right cheek. A strong nose bridge stands high between his shining hawkish eyes.<br/>His deep, low voice has the power to command other people's attention."));

		Msg("How can I help you?", Button("Start Conversation", "@talk"), Button("Shop", "@shop"), Button("Modify Item", "@upgrade"), Button("Get Ciar Beginner Dungeon Pass", "@ciarpass"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());

				var today = ErinnTime.Now.ToString("yyyyMMdd");
				if (today != Player.Vars.Perm["ranald_title_gift"])
				{
					string message = null;

					switch (Title)
					{
						case 10059: // is a friend of Trefor
							message = L("Here you are, <username/>.<br/>A friend of Trefor it is...");
							break;

						case 10060: // is a friend of Deian
							message = L("Here you are, <username/>.<br/>A friend of Deian it is...");
							break;

						case 10061: // is a friend of Malcolm
							message = L("Here you are, <username/>.<br/>A friend of Malcolm it is...");
							break;
					}

					if (message != null)
					{
						Msg(message);

						Player.Vars.Perm["ranald_title_gift"] = today;

						GiveItem(51011, 3); // Stamina 10 Potion x3
						Notice(L("Received Stamina 10 Potion from Ranald."));
						SystemMsg(L("Received Stamina 10 Potion from Ranald."));

						Msg(L("I think you'll need this Stamina Potion quite often<br/>for your combat training."));
					}
				}

				if (Title == 11001)
				{
					Msg("...");
					Msg(".......");
					Msg("Well, I don't care much about titles.<br/>Just train! Continue to train! That's what will make you stronger!<br/>Don't slack off and focus on your training!");
				}
				else if (Title == 11002)
				{
					Msg("Hah... I can't believe<br/>you've become the Guardian of Erinn.<br/>I still remember you practicing your combat skills on those dummies...");
					Msg("...I can't be more proud as your teacher.<br/>These are the moments that make teachers feel rewarded...");
					Msg("...I'm proud of you. <username/>.");
				}

				await Conversation();
				break;

			case "@shop":
				Msg("Tell me if you need a Quest Scroll.<br/>Working on these quests can also be a good way to train yourself.");
				OpenShop("RanaldShop");
				break;

			case "@upgrade":
				Msg("Hmm... You want me to modify your item? You got some nerve!<br/>Ha ha. Just joking. Do you need to modify an item? Count on <npcname/>.<br/>Pick an item to modify.<br/>Oh, before that. Types or numbers of modifications are different depending on what item you want to modify. Always remember that.<upgrade />");

				while (true)
				{
					var reply = await Select();

					if (!reply.StartsWith("@upgrade:"))
						break;

					var result = Upgrade(reply);
					if (result.Success)
						Msg("Hmm... It was easier than I thought. Good.<br/>You got anything else to modify?");
					else
						Msg("(Error)");
				}

				Msg("If you don't have anything to modify for now, then I'll see you next time.<upgrade hide='true'/>");
				break;

			case "@ciarpass":
				GiveItem(63139); // Ciar Beginner Dungeon Pass
				Notice("Recieved Ciar Beginner Dungeon Pass from Ranald.");
				Msg("OK, here's the pass.<br/>You can ask for it again if you need it.<br/>That doesn't mean you can fill up the inventory with a pile of passes.");
				break;
		}

		End("Goodbye, <npcname/>. I'll see you later!");
	}

	private void Greet()
	{
		if (Memory <= 0)
		{
			Msg(FavorExpression(), L("Hmm...<br/>Nice to meet you."));
		}
		else if (Memory == 1)
		{
			Msg(FavorExpression(), L("My name is <npcname/>.<br/>I assume you remember my name?"));
		}
		else if (Memory == 2)
		{
			Msg(FavorExpression(), L("<username/>.... Not a bad name.<br/>I'll remember that."));
		}
		else if (Memory <= 6)
		{
			Msg(FavorExpression(), L("<username/>?<br/>Tell me what you're here for."));
		}
		else
		{
			Msg(FavorExpression(), L("<username/>...I see you here often."));
		}

		UpdateRelationAfterGreet();
	}

	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				if (Favor > 20 && !HasKeyword("RP_Ranald_Complete"))
				{
					if (!HasItem(73101))
					{
						GiveItem(73101); // Ranald's Medal
						SystemNotice(L("Received Ranald's Medal from Ranald."));
					}

					Msg(L("I used to be a Dunbarton mercenary.<br/>I'd spent my youth as a mercenary and then built a School here at Tir Chonaill.<br/>You'll be able to see my past if you enter Rabbie Dungeon alone with this. Haha."));
				}
				else
				{
					GiveKeyword("school");
					Msg(FavorExpression(), "Hello, there. I teach combat skills at the School in Tir Chonaill.<br/>If you're interested, talk to me with the 'Classes and Training' keyword.");
					Msg("Hey, hey... This is not free. You'll need to pay tuition for my classes...");
					ModifyRelation(Random(2), 0, Random(3));
				}
				break;

			case "rumor":
				if (!HasSkill(SkillId.RangedAttack))
				{
					GiveKeyword("complicity");
					RemoveKeyword("bow");
					Msg(FavorExpression(), "Hmm... Did you hear the news?<br/>Ferghus can't stop smiling these days.<br/>I heard his arrow sales have jumped up lately.");
					Msg("It seems like Trefor received a huge gift from Ferghus.<br/>People are assuming that Trefor is helping Ferghus with something.");
				}
				else
				{
					Msg("A dinner with Ferghus usually leads to a bit of drinking at the end.<br/>You know he loves to drink, right? As a matter of fact, I like to drink, too. Hahaha...");
				}
				ModifyRelation(Random(2), 0, Random(3));
				break;

			case "about_skill":
				if (HasSkill(SkillId.Windmill))
				{
					Msg("How was the Windmill skill? Was it of any help?<br/>You will one day become a great warrior<br/>as long as you remain an ardent student eager for training just like you are now.");
					break;
				}
				goto default;

			//case "about_study": // Handled in quest script

			case "shop_misc":
				GiveKeyword("shop_armory");
				Msg("You're not going to the General Shop for weapons, are you?<br/>For weapons, you would need to go to the Blacksmith's Shop.");
				break;

			case "shop_grocery":
				Msg("The Grocery Store is located near the Square. Right now, you're at the School.");
				break;

			case "shop_healing":
				Msg("There's no way to avoid getting wounded while battling, and some of the wounds can't be healed by magic or basic treatment.<br/>If the injuries keep piling up, your body will get noticeably weaker.");
				Msg("That's why it's important to receive proper treatments for your wounds.");
				break;

			case "shop_inn":
				Msg("In my time, inns were not a daily sight, so I had to pitch a tent and make the best of it.<br/>Compared to that, you're having it way too easy here.");
				break;

			case "shop_bank":
				Msg("It's a good habit to deposit your items at the Bank whenever possible.<br/>If you run out of inventory space,<br/>you won't be able to pick up or carry additional items.<br/>If that happens, you would be doing all the work and someone else would be reaping the rewards.<br/>You don't want that, do you?");
				break;

			case "shop_smith":
				Msg("The Blacksmith's Shop is on the bank of the Adelia Stream, located close to the town entrance.<br/>Tell Ferghus I sent you, and he'll take good care of you.");
				break;

			case "skill_range":
				GiveKeyword("bow");
				RemoveKeyword("skill_range");
				if (Player.IsGiant)
				{
					// Giants can't get this keyword anymore
					// If anyone has a giant with this keyword, please log it
					Msg("(Missing Dialog: Small Description)");
				}
				else
				{
					Msg("Long range attacks?<br/>Hmm... Desire alone doesn't cut it. You'll also need to be equipped with an appropriate weapon.<br/>There are a number of long range weapons, but go get a bow first.");
					Msg("Just use it a couple of times. You won't need any help from others in understanding the basics.");
					Msg("If you practice a few times and get to know about the Ranged Attack skill, it means you're doing your job.<br/>Ferghus is usually the source for weapons in this town, so go see him at the Blacksmith's Shop.");
				}
				break;

			case "skill_instrument":
				GiveKeyword("church");
				Msg("You want to learn the Instrument Playing skill?<br/>Hmm... Did you forget that I am a combat instructor?<br/>I can't believe you are asking me for that. I am a little disappointed");
				Msg("Ha ha... Don't be so disheartened about what I said, though.<br/>I didn't know you would be easily affected by my words. Hahaha.<br/>You're just like Malcolm when he was a kid.");
				Msg("OK, about the Instrument Playing skill...<br/>Go ask Priestess Endelyon at the Church about it.<br/>I'm sure she will teach you well.");
				break;

			case "skill_composing":
				Msg("You're interested in the Composing skill?<br/>Hmm... today's a strange day.<br/>Most of the questions people are asking are about petty skills that have nothing to do with combat.<br/>Quite disappointing, indeed...");
				Msg("You look like a prototypical warrior to me.<br/>For you to ask me about skills like that... I feel like you're straying away from the path to becoming a great warrior...");
				Msg("But anyway, about the Composing skill...<br/>Endelyon knows a lot about it. Make sure you talk with her first.");
				Msg("Once you get her advice, make sure to follow her instructions.<br/>Don't even think about getting something out of me without listening to her first.");
				break;

			case "skill_tailoring":
				GiveKeyword("shop_grocery");
				Msg("You want to make clothes for yourself?<br/>Hmm... Why would you come to the School to ask me, a combat instructor,<br/>how to make clothes?<br/>I just don't get it. But, anyway...");
				Msg("You have asked me a question, and as a teacher, I feel obligated to answer it.<br/>Talk to Caitin at the Grocery Store.<br/>She knows more about that skill than anyone else.");
				Msg("From what I've heard, most of the clothes at Malcolm's shop<br/>were either designed or tailored by her...");
				break;

			case "skill_magnum_shot":
				RemoveKeyword("skill_magnum_shot");
				Msg("Oh... You already know about that?<br/>I only gave you a brief summary about the Ranged Attack,<br/>and you already picked up that much! Quite impressive!<br/>Alright! Let's move on then!");
				Msg("Magnum Shot skill helps you to shoot a powerful blow<br/>with the power you have concentrated in the bow.<br/>Go on and work on the training.<br/>");
				GiveSkill(SkillId.MagnumShot, SkillRank.RF); // This is removed in later updates
				break;

			case "skill_counter_attack":
				Msg("Counterattack skill...<br/>Simply knowing the skill won't help you.<br/>It's all about timing.");
				Msg("This skill cannot be mastered unless you practice it in a real combat situation, which means...<br/>You'll need to get hit first.");
				Msg("I could show you a demonstration right now and teach you this skill, but... I'll probably break you in half.<br/>You should go to Trefor instead and ask him to show you the Counterattack skill.<br/>It would be a lot safer for you. Go now.");
				break;

			case "skill_smash":
				Msg("Smash skill...<br/>More than anything, balancing your power is the most important thing for this skill.<br/>In that respect, Ferghus knows a lot more than I do.<br/>Go learn from him.");
				break;

			case "skill_gathering":
				Msg("Paying too much attention to gathering is against the path of a warrior.<br/>But we do need skills to help us survive on barren fields.<br/>Being familiar with the Gathering skill would be useful, I guess.");
				Msg("At least, an Axe or axe-type weapon would help you gather firewood,<br/>while also using it as a devastating weapon.");
				Msg("Speaking of firewood,<br/>you could ask Tracy at the Ulaid Logging Camp in Dugald Aisle for something like this.");
				Msg("There's no doubt that he's a sleazebag, but he's still a lumberjack.");
				break;

			case "square":
				Msg("Follow the path up in front of the School<br/>and you will see the Square.");
				Msg("The Square is usually the place to meet for many people,<br/>talking about various topics and exchanging information.");
				Msg("So, if you see any travelers there, don't hesitate to say hello.<br/>It never hurts to make friends.");
				break;

			case "pool":
				GiveKeyword("brook");
				Msg("The reservoir? Go up north a little bit, and that's where you'll find it.<br/>The water in the reservoir comes from the Adelia Stream.<br/>We use the Windmill to get water up from the stream to fill the reservoir.<br/>It's critical for irrigating the farmland.");
				break;

			case "farmland":
				GiveKeyword("shop_grocery");
				Msg("The farmland?<br/>Well, it's right in front of the School. You didn't see it?<br/>If you're talking about another farmland,<br/>there's a small one next to Caitin's Grocery Store, but...");
				Msg("Don't step on those crops!<br/>Let the scarecrow take care of it.");
				break;

			case "windmill":
				GiveKeyword("shop_inn");
				Msg("Hmm... are you looking for the windmill?<br/>I see... The windmill here is well worth a visit. It's picturesque.");
				Msg("If you want to see the windmill up close, go near the Inn.");
				break;

			case "brook":
				GiveKeyword("farmland");
				Msg("Hmm... you want to know how to reach the Adelia Stream?<br/>It will take some time to get there...<br/>It's not easy to explain where it is...");
				Msg("Walk outside the School and follow along the farmland.<br/>After a while, you will see a small stream and a bridge.<br/>Well, that's the Adelia Stream.");
				break;

			case "shop_headman":
				Msg("The Chief's House is at the top of the hill with the big tree,<br/>north of the Square.<br/>You can see the whole town from up there.");
				break;

			case "temple":
				Msg("The Church?<br/>It's very close from here.<br/>Just take a few steps up north and you'll see it.");
				Msg("Hmm... Hey,<br/>can you do me a favor?<br/>Can you go to the Church and see what Priestess Endelyon is doing?<br/>It's nothing, really. I'm just wondering what she's doing, you know...");
				Msg("But don't let Priest Meven see you, okay?");
				break;

			case "school":
				Msg("Tir Chonaill is such a small town that establishing a school was almost unnecessary.<br/>But I thought it was necessary that we establish a place for our children to learn the traditions and wisdom of our forefathers.<br/>Otherwise, our proud heritage would be been lost after only a few generations.<br/>We could not let that happen.");
				Msg("Fortunately, Lassar came back from her studies around that time, and decided to help out.<br/>That's how this School came to be.");
				break;

			case "skill_windmill":
				Msg("Do you want to know more about the Windmill skill?<br/>You should go to Dunbarton then.");
				Msg("The combat instructor in Dunbarton is really beautiful... I mean, ummm, she's a really good teacher.<br/>Anyway, go to Dunbarton and look for her.");
				Msg("Just go all the way down south from Tir Chonaill. That's where Dunbarton is.<br/>One thing before you leave. You'd better stay away from that man Tracy, hanging around the Logging Camp.<br/>He's just not a good person to be with.");
				break;

			case "skill_campfire":
				Msg("Hmm... A warrior who also knows the Campfire skill.<br/>I guess that could be helpful.<br/>However, you can end up spending precious time chopping wood<br/>instead of training for your combat skills...");
				Msg("You may end up as a lumberjack, not as a warrior...");
				Msg("...<br/>You don't want to be like Tracy when you grow up, right?");
				Msg("A warrior must always practice self-discipline.<br/>Don't stay around a campfire too long and waste precious training time.");
				break;

			case "shop_restaurant":
				GiveKeyword("shop_grocery");
				Msg("Hmm... As far as I know, there are no restaurants in Tir Chonaill.<br/>If you're looking for something to eat, then talk to Caitin.");
				Msg("She spends most of her time working at the Grocery Store,<br/>so why don't you go there and say hi to her?");
				break;

			case "shop_armory":
				GiveKeyword("shop_smith");
				Msg("You can't find the Weapons Shop?<br/>Hmm... Well, this is very heartbreaking to say, but there is no Weapons Shop in this town.<br/>You will need to go to the Blacksmith's Shop if you need a weapon.");
				Msg("All weapons and armor are made at the Blacksmith's Shop by Ferghus himself.");
				Msg("There's not much of a variety there,<br/>but talk to Ferghus if you need something.");
				Msg("Perhaps it's better this way. You know that weapons and armor won't make you a real warrior, don't you?<br/>If you depend on them too much, all your training may end up in vain.");
				break;

			case "shop_cloth":
				Msg("A Clothing Shop? You need new clothes?<br/>Personally, I think you look just fine.<br/>I didn't realize you were so self-conscious...");
				Msg("Perhaps you should reconsider your priorities.<br/>A minute ago, you were so intent on becoming a warrior that you were ready to start training right away.<br/>And now all you talk about is clothes and style.<br/>Do you think you have what it takes to become a true warrior?");
				Msg("If you think the clothes you're wearing now are uncomfortable or worn out,<br/>then go to Malcolm's General Shop.");
				Msg("But if that's not the case, and you just want to look nice,<br/>then it's nothing but a waste of money.");
				break;

			case "shop_bookstore":
				Msg("A bookstore? You won't find one in this town.<br/>If you really want to buy some books,<br/>it would be better for you to go to another town.");
				Msg("A friend of mine who's also a combat instructor told me<br/>that Dunbarton has a bookstore.");
				Msg("Hmm... reading books is certainly a good thing,<br/>but spending too much time on it won't help you get your job done.<br/>I suggest you read in moderation.");
				Msg("If you spend too much time on books,<br/>you can end up a scrawny nerd like that Malcolm guy.");
				break;

			case "shop_government_office":
				Msg("Hmm.... Probably a big town or city may have it,<br/>since they are usually under the lordship of Aliech.<br/>But there's no town office around here.");
				Msg("When the descendants of Ulaid built Tir Chonaill and its neighborhood,<br/>they wanted no one to govern us but ourselves.");
				Msg("If you'd like to know more about our town,<br/>why don't you visit the Chief's House?<br/>You can go and see him in case you've lost something.<br/>The Chief usually keeps it for a while.");
				break;

			case "graveyard":
				GiveKeyword("shop_headman");
				Msg("The graveyard? Hmm...<br/>There is one located over the hill behind the Chief's House but...");
				Msg("If I were you,<br/>I would stay away from that place.<br/>It's not right to cause a commotion literally on top of the dead.<br/>It is simply not the right thing to do.");
				Msg("I want you to respect the dead,<br/>and let them rest in peace.");
				break;

			case "skill_fishing":
				Msg("Mmm? Fishing skill?");
				Msg("Hmm... Well, I suppose you can't train for combat skills all the time.<br/>You have to relax sometimes.");
				Msg("But I should tell you<br/>that even fishing has its own set of rules.");
				Msg("It wouldn't be right to sit right next to someone who's already fishing<br/>and throw your line in to fish.");
				break;

			case "complicity":
				Msg("Hmm... as a strong advocate of ethics and education,<br/>I don't think it's the right thing to do...");
				Msg("But I think sometimes it's necessary to open our minds<br/>and be tolerant of such things.");
				break;

			case "lute":
				Msg("Do you want to know where to buy a lute?<br/>You must have seen some people carrying them around, didn't you?");
				Msg("You know, you could have just gone over to one of them and ask where they bought it.");
				Msg("Go to Malcolm at the General Shop if you want one of those.<br/>lute is the cheapest instrument you can buy,<br/>so I think you can easily afford it.");
				Msg("Don't feel sorry for yourself for not having enough money.<br/>You need to start doing some part-time jobs or party quests, so you can earn some money to buy something you like.");
				break;

			case "tir_na_nog":
				Msg("Tir Na Nog is the land of paradise everyone in Erinn dreams of.<br/>People say there is no pain or suffering in Tir Na Nog, but only eternal life and happiness.");
				Msg("Personally, I have never been there,<br/>and I don't really know what it's like...");
				Msg("Still,<br/>I don't believe Endelyon would say anything false.<br/>She is not the type of person that lies.");
				break;

			case "mabinogi":
				Msg("Hmm... Mabinogi is a song written and sung to praise mighty warriors.<br/>It's a song about the story of fearless warriors who fought against<br/>the evil Fomors in endless battles a long time ago.");
				Msg("You know, Mabinogi is like a living, breathing creature.<br/>It keeps growing and evolving,<br/>its contents revised, extended, sung and heard through endless generations.");
				Msg("So, who knows? If you become a great warrior yourself,<br/>you may someday have a song dedicated to your heroic efforts.<br/>People will write and sing about you...");
				break;

			case "musicsheet":
				Msg("Music Score?");
				Msg("Why... you want to write a song?<br/>Ask Malcolm if your creativity extends to composition.<br/>He was drawing on some sheets of paper to write some scores the last time I saw him...");
				Msg("How he keeps his concentration while doodling on a sheet of paper is a complete mystery to me.");
				break;

			default:
				RndFavorMsg(
					"You know I've been busy...",
					"Well, I don't really know...",
					"I am not very interested in that.",
					"Hmm... Actually, I forgot my lines... Haha.",
					"I haven't paid much attention to it, especially on a topic like that."
				);
				ModifyRelation(0, 0, Random(3));
				break;
		}
	}
}

public class RanaldShop : NpcShopScript
{
	public override void Setup()
	{
		Add("Arena", 63019, 10);  // Alby Battle Arena Coin 10x
		Add("Arena", 63019, 20);  // Alby Battle Arena Coin 20x
		Add("Arena", 63019, 50);  // Alby Battle Arena Coin 50x
		Add("Arena", 63019, 100); // Alby Battle Arena Coin 100x

		AddQuest("Quest", 1010, 0); // [Collect 10 Branches]
		AddQuest("Quest", 1011, 0); // [Collect 10 Berries]
		AddQuest("Quest", 1012, 0); // [Collect 10 Large Nails]

		Add("Reference Book", 1078); // Don't give up!
	}
}