//--- Aura Script -----------------------------------------------------------
// Trefor in Tir Chonaill
//--- Description -----------------------------------------------------------
// The guard located in Northern Tir, near Alby Dungeon
//---------------------------------------------------------------------------

public class TreforScript : NpcScript
{
	public override void Load()
	{
		SetRace(10002);
		SetName("_trefor");
		SetBody(height: 1.35f);
		SetFace(skinColor: 20, eyeColor: 27);
		SetStand("human/male/anim/male_natural_stand_npc_trefor02", "human/male/anim/male_natural_stand_npc_trefor_talk");
		SetLocation(1, 8692, 52637, 220);
		SetGiftWeights(beauty: 0, individuality: 2, luxury: 0, toughness: 2, utility: 1, rarity: 0, meaning: 1, adult: 0, maniac: 0, anime: -1, sexy: 1);

		EquipItem(Pocket.Face, 4909, 0x93005C);
		EquipItem(Pocket.Hair, 4023, 0xD43F34);
		EquipItem(Pocket.Armor, 14076, 0x1F1F1F, 0x303F36, 0x1F1F1F);
		EquipItem(Pocket.Glove, 16097, 0x303F36, 0x000000, 0x000000);
		EquipItem(Pocket.Shoe, 17282, 0x1F1F1F, 0x1F1F1F);
		EquipItem(Pocket.Head, 18405, 0x191919, 0x293D52);
		EquipItem(Pocket.LeftHand2, 40005, 0xB6B6C2, 0x404332, 0x22B653);

		AddPhrase("(Fart)...");
		AddPhrase("(Spits out a loogie)");
		AddPhrase("Ah-choo!");
		AddPhrase("Ahem");
		AddPhrase("Burp.");
		AddPhrase("Cough cough...");
		AddPhrase("I heard people can go bald when they wear a helmet for too long...");
		AddPhrase("I need to get married...");
		AddPhrase("It's been a while since I took a shower");
		AddPhrase("Seems like I caught a cold...");
		AddPhrase("Soo itchy... and I can't scratch it!");
		AddPhrase("This helmet's really making me sweat");
	}

	protected override async Task Talk()
	{
		SetBgm("NPC_Guard.mp3");

		// I noticed the intro message is different as of r226 on 5/11/16
		// "A specimen of physical fitness stands at attention in a suit of<br/>immaculate armor. Through his lowered visor, you catch the<br/>slightest flash of his determined eyes."
		await Intro(L("Quite a specimen of physical fitness appears before you wearing well-polished armor that fits closely the contours of his body.<br/>A medium-length sword hangs delicately from the scabbard at his waist. While definitely a sight to behold,<br/>it's difficult to see much of his face because of his lowered visor, but one cannot help but notice the flash in his eyes<br/>occasionally catching the light between the slits on his helmet. His tightly pursed lips seem to belie his desire to not shot any emotion."));

		Msg("How can I help you?", Button("Start Conversation", "@talk"), Button("Shop"), Button("Upgrade Item", "@upgrade"), Button("Get Alby Beginner Dungeon Pass", "@pass"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());

				var today = ErinnTime.Now.ToString("yyyyMMdd");
				if (today != Player.Vars.Perm["trefor_title_gift"])
				{
					string message = null;

					switch (Title)
					{
						case 10059: // is a friend of Trefor
							message = L("Ha ha, welcome back, <username/>.<br/>How are things going for you?");
							break;

						case 10061: // is a friend of Malcolm
							message = L("Malcolm is a little stingy fellow but he is a good friend of mine.<br/>If you're a friend of Malcolm, then you're a friend of mine as well.");
							break;
					}

					if (message != null)
					{
						Msg(message);

						Player.Vars.Perm["trefor_title_gift"] = today;

						GiveItem(71017); // White Spider Fomor Scroll
						Notice(L("Received White Spider Fomor Scroll from Trefor."));
						SystemMsg(L("Received White Spider Fomor Scroll from Trefor."));

						Msg(L("You might be a little puzzled about the Fomor Scroll I have here.<br/>But you get to experience all sorts of things<br/>if you work as a guard here."));
					}
				}

				if (Title == 11001)
				{
					Msg("...");
					Msg("......");
					Msg("Can you do me a favor?<br/>Please don't go to Dilys and show off your strength and skills.");
					Msg("To be honest, I am staying in this town only because of her.<br/>I don't want Dilys comparing me to you in any way, shape or form.");
				}
				else if (Title == 11002)
				{
					Msg("Wha...? You're the Guardian of Erinn...?<br/>You, <username/>...?");
					Msg("......");
					Msg("...Wow... I'm speechless...<br/>I guess I should<br/>congratulate you for now.");
					Msg("...And now you've<br/>become my rival...");
				}

				await Conversation();
				break;

			case "@shop":
				Msg("Do you need a Quest Scroll?");
				OpenShop("TreforShop");
				return;

			case "@upgrade":
				Msg("Do you want to modify an item?<br/>You don't need to go too far; I'll do it for you. Select an item that you'd like me to modify.<br/>I'm sure you know that the number of times it can be modified, as well as the types of modifications available depend on the item, right? <upgrade />");

				while (true)
				{
					var reply = await Select();

					if (!reply.StartsWith("@upgrade:"))
						break;

					var result = Upgrade(reply);
					if (result.Success)
						Msg("Finished!<br/>I don't want to brag, but I'm quite talented at this.<br/>Do you want to modify another item?");
					else
						Msg("(Error)");
				}

				Msg("Feel free to drop by when you have something you'd like to modify. <upgrade hide='true'/>");
				break;

			case "@pass":
				GiveItem(63140);
				Notice("Recieved Alby Beginner Dungeon Pass from Trefor.");
				Msg("Do you need an Alby Beginner Dungeon Pass?<br/>No problem. Here you go.<br/>Drop by anytime when you need more.<br/>I'm a generous man, ha ha.");
				break;
		}

		End("Goodbye, <npcname/>. I'll see you later!");
	}

	private void Greet()
	{
		if (Memory <= 0)
		{
			Msg(FavorExpression(), L("Hmm? Are you a new traveler?"));
		}
		else if (Memory == 1)
		{
			Msg(FavorExpression(), L("Hello <username/>, nice to see you."));
		}
		else if (Memory == 2)
		{
			Msg(FavorExpression(), L("Hey, <username/>.<br/>You should look out for yourself more."));
		}
		else if (Memory <= 6)
		{
			Msg(FavorExpression(), L("Hello <username/>, nice to see you.")); // Bad localization, it should probably say "<username/>, I'm happy to see you."
		}
		else
		{
			Msg(FavorExpression(), L("Hahaha welcome back, <username/>.<br/>How are you doing these days?"));
		}

		UpdateRelationAfterGreet();
	}

	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				Msg(FavorExpression(), "Hmm... Have something to ask me?<br/>I'm nothing but a regular fellow from this town.<br/>I am but a humble servant of Lymirark, whose duty is to protect this town.");
				ModifyRelation(Random(2), 0, Random(3));
				break;

			case "rumor":
				Msg(FavorExpression(), "Recently, the people in this town have become somewhat anxious<br/>about the howling of wild animals outside.<br/>For some reason, their howling seems to be getting a little bit closer each day.");
				Msg("That's why I'm standing guard like this.");
				ModifyRelation(Random(2), 0, Random(3));
				break;

			case "about_skill":
				if (Player.IsGiant)
				{
					Msg("Hmm... I'm sorry, but I can't think of any skill that'd be useful to you at the moment, <username/>.");
					return;
				}
				if (!HasSkill(SkillId.RangedAttack))
				{
					GiveKeyword("skill_range");
					Msg("I've been observing your combat style for some time now.<br/>If you want to be a warrior, you shouldn't limit yourself to just melee attacks.");
					Msg("I'm sure Ranald at the School can teach you some things about ranged attacks<br/>which will allow you to attack monsters from a distance.");
				}
				else if (!HasSkill(SkillId.SupportShot))
				{
					Msg("Ah! <username/>. Haha. Has your archery skills improved since I last saw you?<br/>Hmm...It seems like you improved quite a bit, even though you're not as skilled as I am.");
					Msg("Are you interested in learning the Support Shot skill?<br/>It's a skill that will help<br/>other members when you're in a party.", Button("I am interested!", "@yes"), Button("Can... I trust you?", "@no"));
					switch (await Select())
					{
						case "@yes":
							Msg("I knew you'd be interested. Hahaha.<br/>Then, as a special courtesy, I'll teach you.<br/>Listen carefully and do as I instruct.");
							Msg("Now, close your eyes and imagine yourself holding a bow.<br/>In front of you, your friend is struggling with a big sword against an enemy.<br/>Your friend calculates the right timing to hit the enemy<br/>while causing steady damage.");
							Msg("...In this case, how would you shoot your arrows?<br/>How can you shoot so that<br/>you won't interrupt your friend, while still injuring the enemy?<br/>Why don't you close your eyes and visualize it?", Button("I visualized it."));
							await Select();

							GiveSkill(SkillId.SupportShot, SkillRank.RF);
							Msg("I'm not certain how well you followed<br/>my instructions with your eyes closed, but it's all good.<br/>I gave you an easy-to-follow guide,<br/>so you shouldn't have any difficulties using Support Shot.");
							Msg("I pray in the name of Morrighan the Goddess<br/>that you, whose arrows fly with bravery, will always be surrounded by glory.");
							Msg("Also, don't forget to drop by the Blacksmith's Shop when you run out of arrows.");
							break;

						case "@no":
							Msg("Are you saying that you won't travel with other people?<br/>You're pretty confident.");
							Msg("But you see <username/>,<br/>there are limits to how much you can accomplish all by yourself.<br/>I hope you don't end up regretting not taking my advice, <username/>.");
							Msg("...Come by anytime if you change your mind.<br/>I'll show you the true art of archery.");
							break;
					}
				}
				else
				{
					Msg("Since you've learned the Support Shot skill now,<br/>why don't you start your training by going back to Alby Dungeon.<br/>You can hone your archery skills there.");
					Msg("If you form a party,<br/>you'll be able to learn how archers assist warriors engaged in melee combat.<br/>I guarantee it, in the name of the number one town guard of Tir Chonaill.");
				}
				break;

			case "about_arbeit":
				Msg("Hmm... I don't have any work for you.<br/>But I have something to tell you that might help.<br/>I've noticed travelers aren't good at the following two things.");
				Msg("First, when you're done with your job,<br/>you must always go back<br/>and report your results.");
				Msg("When you're finished with your work,<br/>report your results by using<br/>the [About Part-Time Jobs] keyword.");
				Msg("You wouldn't believe how many people just sit around after finishing their work,<br/>having no clue on how to report their results when the deadline comes.<br/>I really feel bad for them.");
				Msg("The next one would be when to get a part-time job.<br/>You can only do 1 part-time job per day.<br/>... Most of them are given in the morning.");
				Msg("Stand still and click the Auto Camera button located on the lower right screen.<br/>The camera will automatically return to its default view.");
				break;

			case "shop_misc":
				Msg("Don't you think the General Shop in this town doesn't carry enough items?<br/>I know that Malcolm runs it by himself, but still...");
				break;

			case "shop_grocery":
				Msg("Speaking of the Grocery Store, people like us that have laborious jobs<br/>must always wash our hands before eating.<br/>Keep that in mind. You don't want to get sick like that.");
				break;

			case "shop_healing":
				Msg("I owe her a lot.<br/>Don't you think the healer lady is really gorgeous?<br/>Her name is... Dilys.");
				Msg("My heart pounds just by saying her name.");
				break;

			case "shop_inn":
				Msg("It's a good idea to stay at the Inn when you're tired and worn out.<br/>I'd say they have a pretty good service.");
				break;

			case "shop_bank":
				Msg("Bebhinn? Sure, she's cute... But I think she gossips way too much.<br/>Definitely not my style.");
				break;

			case "shop_smith":
				Msg("You can only repair metal items at the Blacksmith's Shop.<br/>It would not be right to try to repair anything else.");
				break;

			case "skill_range":
				GiveKeyword("school");
				Msg("Well, I'm quite busy right now.<br/>Why don't you ask Ranald at the School?<br/>I CAN say that Ranged Attack is really useful, though.<br/>I strongly recommend you master it... It's THAT useful.");
				break;

			case "skill_instrument":
				Msg("Hmm... I'm sure life will be much eaiser if I got to master an instrument,<br/>and it might be perfect for me to sing and play for the person I love dearly.<br/>It'll definitely make me look good on many levels.");
				Msg("But then again, have you seen my hands?<br/>They have become callous from numerous battles.<br/>I did try once to play an instrument, but it didn't work no matter how hard I tried.");
				Msg("I wish I could train my musical talents just like any other combat skill.");
				break;

			case "skill_composing":
				Msg("I wish I had time to learn the Composing skill, I really do...");
				Msg("But it's hard enough standing here all day,<br/>regulating all the travelers that are passing by.");
				Msg("I've always wanted to learn,<br/>and thought the Composing skill would come in handy.");
				Msg("The one thing I would really like to do is express my feelings for her with music.");
				Msg("Yes, I'm talking about Dilys...");
				break;

			case "skill_magnum_shot":
				GiveSkill(SkillId.MagnumShot, SkillRank.Novice);
				Msg("Magnum Shot is a skill that lets you shoot your arrow with greater power.<br/>The problem is, I don't know how to use it myself.<br/>I am sure Ranald could teach you, though.");
				break;

			case "skill_counter_attack":
				Msg("So you're aware of the Counterattack skill.<br/>Counterattack is a skill that utilizes your enemy's power.<br/>This means the more powerful the enemy's attack is, the more damage it will deliver.");
				Msg("The way to use Counterattack is actually very simple.<br/>All you need to do is turn on the skill and just wait. Wait until the enemy attacks you.<br/>As soon as the enemy attacks, you know you will have the last laugh!", Image("Tutorial_dungeon_skill_06", 200, 200));
				Msg("This skill, however, requires a high level of concentration<br/>in that you'll have to accurately anticipate your enemy's next move.<br/>The main drawbacks of using this skill is that your Stamina will be continually spent,<br/>and you won't be able to move during the skill.");
				Msg("Now let's see your stance.<br/>Oh no... What kind of a stance is that? You won't be able to react quick enough with that.<br/>Actually, you won't be able to fight a raccoon with that.");
				Msg("The basics of the Counterattack skill involves utilizing your enemy's power.<br/>This skill does not involve utilizing YOUR strength.");
				Msg("Your legs are all tensed up. You can't react in time like that.<br/>Loosen your left leg to make sure you can absorb your enemy's strength...<br/>Yes. That's it. Now you look like you're ready.");
				Msg("Now all you need to do is actually pull it off in the heat of the battle!<br/>Please don't try it on other villagers, though.", Button("Continue"));
				await Select();

				RemoveKeyword("skill_counter_attack");
				GiveSkill(SkillId.Counterattack, SkillRank.RF);
				break;

			case "skill_smash":
				GiveKeyword("shop_smith");
				Msg("Hmm... Lassar teaches Magic at the School,<br/>and yet she seems to be very interested in the Smash skill.");
				Msg("Isn't it funny that a magic teacher is interested in a melee skill?<br/>She is a friend of Dilys, yet they are so different when it comes to their femininity.");
				break;

			case "skill_gathering":
				Msg("I understand that you are interested in the Gathering skill,<br/>but I'm a little insulted that you'are asking me,<br/>the quintessential warrior.");
				Msg("The Gathering skill is not something you learn, as everyone already knows how to do it.");
				Msg("What matters is that you have the appropriate tools.<br/>You can buy a Gathering Knife from Ferghus at the Blacksmith's Shop or from Deian.");
				break;

			case "square":
				Msg("Are you talking about the Square?<br/>The Square is just down there.");
				Msg("Hmm... <username/>,<br/>if you were asking such a silly question to test my patience,<br/>I'd be very annoyed and disappointed.");
				break;

			case "pool":
				GiveKeyword("shop_bank");
				Msg("Looking for the reservoir?<br/>The reservoir will be on your left when you go down the path near the Bank.");
				break;

			case "farmland":
				Msg("The farmland?<br/>Isn't there a small garden by Caitin's Grocery Store?<br/>Hmm... I think there is one in front of the School.");
				Msg("Do not just walk in there to gather the wheat.<br/>You might easily ruin a year's effort.");
				break;

			case "windmill":
				GiveKeyword("shop_inn");
				Msg("Are you looking for the Windmill?<br/>Head down south, and you'll easily find the Windmill near the Inn.<br/>Go to the bridge where the barrels are stacked.");
				Msg("Make sure not to get too close,<br/>as the blades and the mill can be very dangerous.");
				Msg("And if Alissa says anything about me...<br/>Well, just ignore it.");
				break;

			case "brook":
				GiveKeyword("shop_inn");
				Msg("Adelia Stream runs by the Inn.<br/>It's not far from here. Just head straight down.<br/>I don't know why you'd want to go there, though.");
				break;

			case "shop_headman":
				GiveKeyword("square");
				Msg("You want to know where the Chief's House is?<br/>Hmm... It's on the hill on the opposite side of the Square, but...<br/>You haven't gone to see him yet?");
				Msg("You must have, right?<br/>I'll just assume that you came here because you like me.");
				break;

			case "temple":
				GiveKeyword("shop_bank");
				Msg("The Church is located down south, following the road behind the Bank.<br/>The people there are really nice. They will treat you well.");
				break;

			case "school":
				GiveKeyword("shop_bank");
				GiveKeyword("temple");
				Msg("The School... Hmm... Go right from the Bank,<br/>then straight down past the Church.");
				Msg("You can find my mentor Ranald at School.<br/>He's a really tough combat instructor.<br/>If you ask him about combat in general,<br/>he'll be able to teach you a lot about it.");
				Msg("If you go to the back,<br/>there is another teacher named Lassar.<br/>She's really beautiful, but not as much as Dilys.");
				Msg("Um... Don't tell Lassar that, though.<br/>She might cast a Firebolt on me if she finds out.");
				break;

			case "skill_windmill":
				Msg("Windmill skill? Wow, you already know it?<br/>It's a very difficult skill, you know. Even I can't use it very well.");
				Msg("What did Ranald tell you?<br/>Ahhhh... He must have suggested you learn it from Aranwen at Dunbarton.<br/>Okay, you should hurry and make your way to Dunbarton?");
				Msg("I hope you master the skill and demonstrate for me later.");
				break;

			case "skill_campfire":
				Msg("You want to know what the Campfire skill is?<br/>It's a skill that all adventurers should learn.");
				Msg("If you use the Campfire skill, you can rest more comfortably<br/>while recovering your health faster.");
				Msg("Piaras traveled to a lot of places, so he would definitely know about this skill.<br/>However, I'm afraid that he might not be willing to teach you, since he runs the Inn now.");
				Msg("Aha!!! Last time I saw Deian, he was trying to start a campfire.<br/>How about asking him?");
				break;

			case "shop_restaurant":
				GiveKeyword("shop_grocery");
				GiveKeyword("skill_campfire");
				Msg("After running around working up a sweat, people tend to get hungry.<br/>It'd be good if we had a decent restaurant in town,<br/>but people here usually go to the Grocery Store.");
				Msg("I think the Campfire skill is mainly responsible for that.");
				Msg("What? You don't know what I'm talking about?<br/>Hmm, so you haven't used the skill to share food with others, right?");
				Msg("You heard me right. With the Campfire skill,<br/>you can cook your food by the fire and share it with the people around you.<br/>If you haven't done it before, why don't you try it now?");
				Msg("Food always tastes better when you share it with the people you love.");
				break;

			case "shop_armory":
				GiveKeyword("shop_smith");
				Msg("You are looking for the Weapons Shop?<br/>Hahaha. You should go to the Blacksmith's Shop.<br/>Go and get a bunch of arrows!");
				Msg("Hmm... You don't have a bow?");
				break;

			case "shop_cloth":
				GiveKeyword("shop_misc");
				Msg("If you need some clothes, you can go to the General Shop,<br/>but if you want an armor like mine, then you must go to the Blacksmith's Shop.");
				break;

			case "shop_bookstore":
				GiveKeyword("shop_misc");
				Msg("You need a book?<br/>Malcolm at the General Shop is an avid reader with a collection of books at his shop.<br/>It looks like he's selling some of them, too.");
				Msg("Why don't you go there and talk to him about it?<br/>He's not selling too many books and chances are, you might have read them all...");
				Msg("Just so you know, Malcolm absolutely HATES lending his stuff.<br/>If you want one of his books, you'll probably have to pay for it.");
				break;

			case "shop_goverment_office":
				Msg("Tir Chonaill was founded by the descendents of Ulaid,<br/>inheritors of the proud bloodline of Partholon.<br/>It's worthy to note that it's not governed by the Aliech Kingdom.");
				Msg("If you wish to find any items you might have lost in a dungeon,<br/>you will need to see Chief Duncan near the Square.<br/>He's aware of everything that goes on around here.");
				Msg("If you are looking for a town office,<br/>you should head all the way down south into the Kingdom's territory.");
				break;

			case "graveyard":
				Msg("Looking for the graveyard?<br/>It's not far from Dily's place.");
				Msg("You might know this already, but there are lots of giant spiders near the graveyard.<br/>The place was built in memory of the fallen that sacrificed their lives for this town,<br/>but it's rarely visited these days because of the spiders.");
				Msg("They are not that strong, but can be pretty annoying.<br/>They will sometimes attack people nearby.<br/>Please be careful when you make your way there.");
				break;

			case "bow":
				Msg("Ah! You are looking for a bow?<br/>Ferghus sells them at his Blacksmith's Shop.<br/>Remember, you can't do much with just a bow, so buy some arrows as well.");
				Msg("Even if you're not buying any from Ferghus, I'll give you some pointers that'll help you decide which bow is best for you.");
				Msg("I recommend buying either a Long Bow or Composite Bow for many reasons.<br/>Long Bows are more powerful than a regular bow,<br/>and Composite Bows are made of several materials, giving it a great elasticity compared to its size.");
				Msg("What? You say the price is the only thing that matters?");
				Msg("Hmm...<br/>Well, you're not entirely wrong but you are overlooking one very important aspect.");
				Msg("You know, the more a bow is used, the more it adjusts to the owner's preferences.<br/>I can't say that using a new weapon is always a better option.");
				Msg("Depending on the situation,<br/>sometimes a Short Bow might be your best option.");
				break;

			case "lute":
				GiveKeyword("shop_misc");
				Msg("Are you looking for a lute?<br/>I noticed Malcolm at the General Shop selling some.");
				Msg("Malcolm is terrible at playing musical instruments,<br/>but he keeps on making lutes anyway.<br/>Isn't that strange?");
				break;

			case "complicity":
				RemoveKeyword("complicity");
				Msg("(We previously discussed the Ranged Attack and Ferghus's story on his arrow sales.)");
				Msg("An instigator?<br/>Haha, I have no idea what you are talking about.");
				Msg("Hey, why are you making such a big deal out of this?<br/>We should help each other out, you know.");
				break;

			case "tir_na_nog":
				GiveKeyword("temple");
				Msg("I believe Priest Meven at the Church would know better on this subject.<br/>I heard it's what everyone dreams of as the perfect place,<br/>but frankly, I don't know much about it.");
				break;

			case "mabinogi":
				Msg("Hmm... I heard some things about it, but if you want more information,<br/>you should talk to Ranald or Duncan.<br/>They know a lot more than I do.");
				break;

			case "musicsheet":
				GiveKeyword("shop_misc");
				Msg("Hmm... A Music Score... Are you planning to compose a song?<br/>Why don't you go see Malcolm at the General Shop then?");
				Msg("Anyway, just letting you know, it takes quite a lot of money to do anything related to music,<br/>be it Music Scores or Instruments.<br/>I'm telling you, Malcolm makes a living on selling those items.");
				Msg("I've known Malcolm for many years, but I still have a hard time getting close to him.<br/>I think he doesn't appreciate manly tastes such as swords and combat training.");
				Msg("He might seem a little weird to you after what I have just told you,<br/>but it's just my personal opinion. In fact, Malcolm is a great guy.");
				Msg("I realized, after meeting him a few times in the past,<br/>that I've never seen anyone as meticulous as Malcolm.");
				break;

			default:
				RndMsg(
					"Oh, is that so?",
					"That was quite boring...",
					"I don't know anything about that...",
					"I'm bored. Why don't we talk about something else?",
					"Do you have anything more interesting to talk about?",
					"Never heard of it. I don't think that has anything to do with me."
				);
				ModifyRelation(0, 0, Random(3));
				break;
		}
	}
}
public class TreforShop : NpcShopScript
{
	public override void Setup()
	{
		AddQuest("Party Quest", 100013, 5);  // [PQ] Hunt White Spiders (10)
		AddQuest("Party Quest", 100014, 10); // [PQ] Hunt White Spiders (30)
		AddQuest("Party Quest", 100015, 5);  // [PQ] Hunt Black Spiders (10)
		AddQuest("Party Quest", 100016, 10); // [PQ] Hunt Black Spiders (30)
		AddQuest("Party Quest", 100017, 5);  // [PQ] Hunt Red Spiders (10)
		AddQuest("Party Quest", 100018, 20); // [PQ] Hunt Red Spiders (30)
		AddQuest("Party Quest", 100055, 20); // [PQ] Hunt Down the Coyotes (100)

		Add("Etc.", 1051); // Battle Arena: Aim for a Giant Star
	}
}