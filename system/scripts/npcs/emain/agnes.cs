//--- Aura Script -----------------------------------------------------------
// Agnes
//--- Description -----------------------------------------------------------
// Healer in Emain Macha
//---------------------------------------------------------------------------

public class AgnesScript : NpcScript
{
	public override void Load()
	{
		SetRace(10001);
		SetName("_agnes");
		SetFace(skinColor: 17, eyeType: 3, eyeColor: 47);
		SetStand("human/female/anim/female_natural_stand_npc_01");
		SetLocation(52, 42538, 46984, 244);
		SetGiftWeights(beauty: 2, individuality: 0, luxury: 1, toughness: -1, utility: 2, rarity: 1, meaning: 2, adult: 0, maniac: -1, anime: -1, sexy: 0);

		EquipItem(Pocket.Face, 3900, 0x00C4028E, 0x00F88C61, 0x0077B63F);
		EquipItem(Pocket.Hair, 3041, 0x000B5562, 0x000B5562, 0x000B5562);
		EquipItem(Pocket.Armor, 15080, 0x00FFFCE8, 0x00A5C261, 0x00275A49);
		EquipItem(Pocket.Shoe, 17040, 0x00227775, 0x00002F2F, 0x00576D8D);

		AddPhrase("This should do it...");
		AddPhrase("Should I clean up...?");
		AddPhrase("I need to get out of this routine.");
		AddPhrase("I'm bored...");
		AddPhrase("Maybe I should take care of this another time...");
		AddPhrase("Should I hire an employee...?");
		AddPhrase("Isn't there something fun to do...?");
		AddPhrase("Hmm... We are out of herbal medicine...");
		AddPhrase("This should do it...");
		AddPhrase("...");
		AddPhrase("This isn't good...");
		AddPhrase("Ah... I'm bored...");
	}

	protected override async Task Talk()
	{
		SetBgm("NPC_Agnes.mp3");

		await Intro(L("Her dark eyes are full of innocence, and shine on her pale white face.<br/>Her green hair is neatly tied in a double pony-tail which gives off a sense of calmness.<br/>She is dressed in a stylish white and yellow green Healer dress.<br/>As she becomes aware of us, she smiles and turns my way with her hands folded together."));

		Msg("Hello there.<br/>Did you get hurt? Do you need some potions?", Button("Start a Conversation", "@talk"), Button("Shop", "@shop"), Button("Get Treatment", "@healerscare"), Button("Heal Pet", "@petheal"));

		switch (await Select())
		{

			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());

				if (Player.IsUsingTitle(11001))
				{
					Msg("Oh gosh... Is that right...?<br/><username/>, who is your Goddess...?");
					Msg("She was in so much danger that she needed to be rescued...?<br/>How sad...");
				}
				else if (Player.IsUsingTitle(11002))
				{
					Msg("Oh, it's you, <username/>!");
					Msg("Wait, should I be calling you<br/>the 'Guardian of Erinn' now? Hehe...");
					Msg("...I know you were busy doing something important...<br/>But I'm a little disappointed<br/>that you didn't even come<br/>visit me.");
				}

				await Conversation();
				break;

			case "@shop":
				if (Memory >= 15 && Favor >= 50 && Stress <= 5)
					Msg("I'm only showing you this because it's you, <username/>.<br/>This is a very special item that I don't just sell to anyone.");
				else
					Msg("What are you looking for? Tell me if you see anything you like.");
				OpenShop("AgnesShop");
				return;

			case "@healerscare":
				if (Player.Life == Player.LifeMax)
				{
					Msg("I don't see any injuries that you need treatment for.<br/>If you have a broken heart, I think you need to go see a Counselor instead of a Healer...");
				}
				else
				{
					Msg("Oh dear... You look like you need treatment right away.<br/>It'll cost you 90 Gold. Do you want to get treated?", Button("Receive Treatment", "@gethealing"), Button("Decline", "@end"));
					if (await Select() == "@gethealing")
					{
						if (Gold >= 90)
						{
							Gold -= 90;
							Player.FullLifeHeal();
							Player.Mana = Player.ManaMax;
							Msg("It's all done. Try to be careful out there.");
						}
						else
						{
							Msg("Umm, <username/>?<br/>It seems you're short on money...I can't treat you then.");
						}
					}
				}
				break;

			case "@petheal":
				if (Player.Pet == null)
				{
					Msg("Would you like to show me your animal friend first?");
				}
				else if (Player.Pet.IsDead)
				{
					Msg("I can't fix your pet right now.<br/>This has to be revived first.");
				}
				else if (Player.Pet.Life == Player.Pet.LifeMax)
				{
					Msg("<username/>, your animal friend seems to be just fine.<br/>How about bringing some other animal friend of yours...?");
				}
				else
				{
					Msg("Oh my goodness... <username/>, your animal friend needs to be treated right away.<br/>It will cost 180 Gold to go ahead with it... would you like to treat your pet?", Button("Recieve Treatment", "@recieveheal"), Button("Decline the Treatment", "@end"));
					if (await Select() == "@recieveheal")
					{
						if (Gold < 180)
						{
							Msg("Ummm...<username/>?<br/>I am sorry, but I think you are short on cash, I can't treat your pet then.");
						}
						else
						{
							Gold -= 180;
							Player.Pet.FullLifeHeal();
							Msg("The treatment is complete.<br/>I wish you'd treat your pet with more respect, as much as you'd have for yourself.");
						}
					}
				}
				break;
		}

		End("Thank you, <npcname/>. I'll see you later!");
	}

	private void Greet()
	{
		if (Memory <= 0)
		{
			Msg(FavorExpression(), L("How are you doing? Is there something that's bothering you...?"));
		}
		else if (Memory == 1)
		{
			Msg(FavorExpression(), L("I think I've seen you before...<br/>Didn't I ask you to come back later?"));
		}
		else if (Memory == 2)
		{
			Msg(FavorExpression(), L("<username/>, I was expecting you."));
		}
		else if (Memory <= 6)
		{
			Msg(FavorExpression(), L("How are you doing. <username/>?<br/>What brings you here this time?"));
		}
		else
		{
			Msg(FavorExpression(), L("It not good to come here too often, <username/>.<br/>People who need a Healer's help are those that are seriously ill..."));
		}

		UpdateRelationAfterGreet();
	}

	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				Msg(FavorExpression(), "Hehe... Am I that pretty?<br/>Whether a man or a woman, people seem to always ask me at least one personal question.");
				ModifyRelation(Random(2), 0, Random(3));
				break;

			case "rumor":
				Msg(FavorExpression(), "By the way, did you know about this?<br/>You know the lady who sells flowers at the Square?<br/>Yea, Del and Delen...");
				Msg("They are twins. Don't you think they look alike?<br/>The average person can't tell them apart.. Hehe...");
				Msg("Hm, I wish i had a twin sister.<br/>It would've been so much fun...<br/>I'm jealous of Del and Delen!");
				Msg("...How about you, <username/>? Have you ever wished you'd had a twin?");
				ModifyRelation(Random(2), 0, Random(3));
				break;

			case "shop_misc":
				Msg("Hmm, come to think of it, we don't have a General Shop in this town...<br/>Galvin from the Observatory usually brings goods from a General Shop somewhere and sells them here<br/>so I didn't even realize that we didn't have one around here...");
				Msg("...Wait a minute... So why does Galvin still work at the Observatory<br/>instead of just opening a General Shop...?");
				Msg("...Maybe he likes the Observatory better...");
				break;

			case "shop_grocery":
				Msg("Are you looking for the Grocery Store?<br/>Gosh, I'm sorry to tell you this but...<br/>This town doesn't have a Grocery Store...");
				Msg("But we have a Restaurant.<br/>It is called Loch Lios...<br/>It is a beautiful restaurant near the lake.");
				Msg("...If you go toward the lake, you'll see it right by the lake.<br/>Why don't you take a visit?<br/>Shena will probably be your waitress today.");
				Msg("She is a cute young gal, and if you treat her nice,<br/>she'll probably give you better service.");
				break;

			case "shop_healing":
				Msg("Yes, this is the Healer's House.<br/>But it's kind of weird how everyone calls it the \"Healer's House\".<br/>...Every other store or shop seems to have a better name than mine...<br/>I need to name it something more fancy, like Tre'imehse Cairde or Loch Lios.");
				Msg("...If you can think of a good name,<br/>share it with me, ok? Maybe I'll really change it to that.");
				break;

			case "shop_bank":
				Msg("Hmmm... the Bank of Emain Macha...<br/>All I could think of is Jocelin's place right over there...");
				Msg("...Or are you looking for<br/>some place else?");
				break;

			case "skill_range":
				Msg("I am no expert on Long-Range Attack skills, but...<br/> I heard you can learn it fairly easily if you use<br/>a weapon like the bow or a crossbow.");
				Msg("...Since injuries caused by bows can leave a scar,<br/>you should be careful where you aim the weapon...");
				break;

			case "skill_instrument":
				Msg("If you want to know about music skills<br/>why don't you try asking a musician?");
				Msg("Oh! That's right! There is a guy named Nele at the Square.<br/>He might know a thing or two about instruments. Have you asked him?<br/>If not, why don't you go talk to him?");
				break;

			case "skill_tailoring":
				Msg("If you want to know about tailoring...<br/>you should go visit Tre'imhse Cairde<br/>and ask Ailionoa.");
				Msg("...But, I don't know why Ailionoa is<br/>so grumpy all the time.<br/>If she would just smile, it would make her look so much prettier!<br/>...I think people find her difficult.");
				break;

			case "skill_magnum_shot":
				Msg("I don't know much about the Magnum Shot skill,<br/>but I've seen somebody get shot by the Magnum Shot.<br/>It leaves a very deep wound that is difficult to heal<br/>without proper emergency treatment...");
				Msg("If you are an adventurer, you should be careful...");
				break;

			case "skill_smash":
				Msg("Now that I've been a Healer for quite some time,<br/>I'm increasingly able to tell what's happened...<br/>just by looking at someone's wound or injury.");
				Msg("...I've seen a lot of men with swollen faces<br/>come in after being hit by Smash...<br/>They all seem to get attacked hard at Rabbi dungeon. <br/>Haha...");
				break;

			case "skill_gathering":
				Msg("You want to learn about the Gathering skill?<br/>I don't know much about anything<br/>besides digging up herbs...");
				Msg("From time to time, I'll meet people who tell me<br/>that they'd learned Herbalism from Dunbarton somewhere but...");
				break;

			case "square":
				Msg("You mean the Square, right?<br/>You should be able to see it as soon as you start going south from here...");
				Msg("...Hmm..I don't know about you.<br/>How could you not know about this...?");
				break;

			case "pool":
				Msg("A reservoir? I can't believe this...<br/>Does that lake really look like a reservoir to you...?<br/>You are hilarious... Haha...");
				Msg("...<username/>,<br/>I know I can always count on you for a good laugh.");
				break;

			case "farmland":
				Msg("Hmm... In my opinion,<br/>it'd be nice to have a farm<br/>that was dedicated to growing herbs.");
				Msg("Can you imagine that?<br/>A field filled with Sunlight Herbs...<br/>Haha, doesn't that make you all excited just thinking about it?");
				Msg("...I wouldn't have to ask people to<br/>gather any herbs for me then...<br/>... If only someone would make an Herb field for me...");
				break;

			case "shop_headman":
				Msg("Chief, huh...?<br/>Maybe you should go ask around the countryside...<br/>I'm not really sure... Hehe...");
				Msg("<username/>,<br/>you don't seem like a country person though...");
				break;

			case "temple":
				Msg("Do you have to get to the church?<br/>Just go straight down toward the south on that road.");
				Msg("How did you end up coming all the way over here...?<br/><username/>, you must be pretty bad when it comes to directions. Hahaha...");
				break;

			case "skill_campfire":
				Msg("Here is a tip on how to quickly regain your HP when you are hurt.<br/>Raise your body temperature.");
				Msg("This is why campfires are so useful.<br/>If you don't have the campfire skill,<br/>a campfire kit may be useful to you.");
				Msg("...Galvin might be selling one...");
				break;

			case "shop_restaurant":
				Msg("Are you looking for a Restaurant?<br/>There is a great Restaurant by the lake.<br/>I have to tell you about that place!");
				Msg("...Master Chef Gordon<br/>is an excellent Chef.<br/>I go there and eat pretty often. Sometimes I get it delivered as well.");
				Msg("...If you get the chance,<br/>you should check it out!<br/>You have to try the cake over there.<br/>You'll feel like you've been to Tir Na Nog and back! Haha...");
				break;

			case "shop_armory":
				Msg("What? You want to heal your weapons too?");
				Msg("Haha... Just kidding,<br/>You'll see it on the west side of the Square.<br/>If you go there, just mention my name to Osla.<br/>She'll take good care of you.");
				break;

			case "shop_cloth":
				Msg("...You mean, Tre'imhse Cairde?<br/>Oh, it's the place run by Ailionoa,<br/>The dress I'm wearing is made by her...");
				Msg("...Isn't it pretty? Don't you think it looks good on me?<br/>It was this dress that really convinced me to become a Healer...");
				Msg("Hey now, you're not questioning my skills, are you? Hehe...");
				break;

			case "graveyard":
				Msg("Are you headed towards the Graveyard?<br/>...To pay your respects to the dead...?<br/>Then first go to the Square<br/>and follow the eastern road.");
				Msg("...I don't go there too often...<br/>Graveyards make Healers feel guilty for some reason...");
				Msg("...If you're going there for a specific person<br/>you might want to visit Del or Delen at the Square<br/>and get some flowers...");
				break;

			case "bow":
				Msg("Well... I'm just a Healer.<br/>I don't know much about bows...<br/>Why don't you ask Osla at the Weapons Shop?<br/>She might know something about that.");
				Msg("...Do you know her? Osla?<br/>She always makes this nasal sound when she talks...<br/>Like this... hmm...hmm...hmm...");
				break;

			case "tir_na_nog":
				Msg("Tir Na Nog...<br/>Ah yes, it is said that there is a world of gods that is unlike our own.<br/>People say that its beauty is beyond one's imagination,<br/>the problem being that I don't know how to get there.");
				Msg("Sometimes I imagine what that would must be like...");
				break;

			case "musicsheet":
				Msg("Looking for a Score?<br/>...I heard that Galvin used to sell them<br/>a long time ago. I think that's what I'd heard...");
				break;

			case "Cooker_qualification_test":
				Msg("You should be able to check on all information pertaining to the cooking contest<br/>in the other channels.");
				break;

			case "EG_C2_Unknown_Continet":
				Msg("<username/>, I realize... it is, for the sake of your adventures<br/>a great thing to be traveling around so much<br/>but I want to caution you from spending too much time dreaming about nonsense... it's just not good for you.");
				break;

			case "breast":
				Msg("Geez <username/>...");
				Msg("Pervert...?");
				Msg("Er..., it's like it's the first time you've seen them...");
				break;

			default:
				if (Memory >= 15 && Favor >= 30 && Stress <= 5)
				{
					RndFavorMsg(
						"Hmm... Sorry, I don't really know.",
						"Um... Can we talk about something else...?",
						"Sorry, I'm not really interested in that...",
						"To tell you the truth, I don't have a clue...",
						"Wow, you are very knowledgeable <username/>...",
						"I don't really know... Please don't think of me differently.",
						"I'm sorry, I don't really like talking about things like that..."
					);
					ModifyRelation(0, 0, Random(2));
				}
				else if (Favor >= 10 && Stress <= 10)
				{
					RndFavorMsg(
						"...Why don't we change the topic...?",
						"I'm sorry, I'm not really interested in that.",
						"My gosh, did you expect me to know about that?",
						"I don't really want to pay any attention to that...",
						"...I don't think I would be of much help with that.",
						"I'm not sure...?<br/>I'm sorry I can't really help you.",
						"Darn...I wish I could be of more help...<br/>I don't think I would be of much help."
					);
					ModifyRelation(0, 0, Random(2));
				}
				else if (Favor <= -10)
				{
					RndFavorMsg(
						"Why don't you tell me instead?",
						"Hmm. Why are you asking me this?",
						"I don't know what you are talking about.",
						"We can talk about things like that later...",
						"I don't feel comfortable answering that...",
						"...<username/><br/>You have no idea how to talk to a lady."
					);
					ModifyRelation(0, 0, Random(4));
				}
				else if (Favor <= -30)
				{
					RndFavorMsg(
						"...",
						"...I don't know.",
						"...Let's not talk about that.",
						"I'm a little busy right now...",
						"Can we stop talking about that?",
						"Do we have to talk about that right now?",
						"Don't you think it's rude to bring up topics others don't like to talk about?"
					);
					ModifyRelation(0, 0, Random(5));
				}
				else
				{
					RndFavorMsg(
						"That is difficult to answer.",
						"Um, I don't know much about that.",
						"I have nothing to say about that.",
						"Hah... please talk to someone else.",
						"Umm... I don't really kow about that.",
						"Is this something you don't know, <username/>?<br/>You should ask me then, haha...",
						"Are those the kinds of things you're interested in, <username/>...?<br/>We seem to have different interests..."
					);
					ModifyRelation(0, 0, Random(3));
				}
				break;
		}
	}

	protected override async Task Gift(Item item, GiftReaction reaction)
	{
		switch (reaction)
		{
			case GiftReaction.Love:
				RndMsg(
					"Are you sure this is for me?<br/>My dear.... this is precious...<br/>Thank you, <username/>.",
					"My gosh... Are you sure you want to give this to me?",
					"This is really sweet...<br/>I dont know how to thank you...",
					"Holy Moly! You're giving me this...?<br/>Wow...<br/>Thank you <username/>.",
					"My gosh... oh dear... I can't believe you gave me this...<br/>I don't know if I can thank you enough..."
				);
				break;

			case GiftReaction.Like:
				RndMsg(
					"What a present... <br/>Thank you. I like it a lot.",
					"I didn't see this side of you, <username/>...<br/>Thank you.",
					"Are you sure this is for me? <br/>Because, I'm not giving it back! Hahaha...",
					"Presents always make people happy no matter what they are. <br/>Especially when they're coming from someone like you, <username/>!",
					"Wow... is it okay for me to accept this?<br/>It is a little too much, but thanks!",
					"This present makes me nostalgic for some reason, haha...<br/>Thanks.",
					"Is this... for me?<br/>Then this must be a... present?<p/>I'm so excited, <username/><br/>Thanks.",
					"I'm sorry... are you sure this is for me?<br/>...oh, nothing. I just can't believe this is for me..."
				);
				break;

			case GiftReaction.Neutral:
				RndMsg(
					"Is this a present? haha... Thanks. I'll be sure to use it.",
					"Huh? Is this a present? Hehehe...<br/>Thanks.",
					"You don't have to be this nice to me...<br/>But thanks, I'll accept it with gratitude.",
					"I've been wanting one of these.<br/>Thanks, <username/>.",
					"I'm happy that you show your interest in me with this gift!<br/>Hahaha...",
					"Is this for me?<br/>It seems like you enjoy giving gifts, <username/>. Haha...<br/>Thank you, I'll make a good use of it.",
					"Huh? Present? For me?<br/>Hahaha... Thank you very much!",
					"This is really nice.<br/>I appreciate it.",
					"I'm not going to change the way I treat you just because of this present. hahaha...<br/>However I'll make a note of it!"
				);
				break;

			case GiftReaction.Dislike:
				RndMsg(
					"...I have plenty of space for this, but...<br/>I'm a little hesitant to accept this. But I'll accept it, since it's coming from you, <username/> Haha...",
					"Hahaha... This present is... Oh my, you must be joking...",
					"Um... I'm sorry to tell you this, but... <br/>the present you just gave me, is not so useful to me.  <br/>But I'll be happy to accept it...",
					"...Why are you giving me this present? <br/>I'm totally disappointed, <username/>.",
					"...I'd be happy to accept this.",
					"This is hardly... a present...<br/>I don't appreciate jokes like this.",
					"...Umm, I'm sorry...<br/>This kind of a gift... is interesting...<br/>But I'll accept it...",
					"Hmm... This isn't exactly one of my favorite things in the world,<br/>but since you are giving it to me, <username/>,<br/>I will gladly accept it."
				);
				break;
		}
	}
}

public class AgnesShop : NpcShopScript
{
	public override void Setup()
	{
		Add("Potions", 51000);     // Potion Concoction Kit
		Add("Potions", 51001);     // HP 10 Potion
		Add("Potions", 51002, 1);  // HP 30 Potion x1
		Add("Potions", 51002, 10); // HP 30 Potion x10
		Add("Potions", 51002, 20); // HP 30 Potion x20
		Add("Potions", 51006);     // MP 10 Potion
		Add("Potions", 51007, 1);  // MP 30 Potion x1
		Add("Potions", 51007, 10); // MP 30 Potion x10
		Add("Potions", 51007, 20); // MP 30 Potion x20
		Add("Potions", 51011);     // Stamina 10 Potion
		Add("Potions", 51012, 1);  // Stamina 30 Potion x1
		Add("Potions", 51012, 10); // Stamina 30 Potion x10
		Add("Potions", 51012, 20); // Stamina 30 Potion x20
		Add("Potions", 51037, 10); // Base Potion x10

		Add("First Aid Kits", 60005, 10); // Bandage x10
		Add("First Aid Kits", 60005, 20); // Bandage x20
		Add("First Aid Kits", 63000, 10); // Phoenix Feather x10
		Add("First Aid Kits", 63000, 20); // Phoenix Feather x20

		if (IsEnabled("SystemPet"))
			Add("First Aid Kits", 63032); // Pet First-Aid Kit

		if (IsEnabled("G16HotSpringRenewal"))
		{
			Add("Etc.", 91563, 1); // Hot Spring Ticket x1
			Add("Etc.", 91563, 5); // Hot Spring Ticket x5
		}

		if (IsEnabled("PuppetMasterJob"))
		{
			Add("Potions", 51201, 1);  // Marionette 30 Potion x1
			Add("Potions", 51201, 10); // Marionette 30 Potion x10
			Add("Potions", 51201, 20); // Marionette 30 Potion x20
			Add("Potions", 51202, 1);  // Marionette 50 Potion x1
			Add("Potions", 51202, 10); // Marionette 50 Potion x10
			Add("Potions", 51202, 20); // Marionette 50 Potion x20

			Add("First Aid Kits", 63715, 10); // Fine Marionette Repair Set x10
			Add("First Aid Kits", 63715, 20); // Fine Marionette Repair Set x20
			Add("First Aid Kits", 63716, 10); // Marionette Repair Set x10
			Add("First Aid Kits", 63716, 20); // Marionette Repair Set x20
		}

		Add("Books", (c, o) => o.GetMemory(c) >= 15 && o.GetFavor(c) >= 50 && o.GetStress(c) <= 5);
		Add("Books", 1084); // Quick and Effective First Aid
	}
}