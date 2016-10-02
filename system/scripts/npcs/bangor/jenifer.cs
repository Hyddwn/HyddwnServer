//--- Aura Script -----------------------------------------------------------
// Jenifer
//--- Description -----------------------------------------------------------
// Bartender
//---------------------------------------------------------------------------

public class JeniferScript : NpcScript
{
	public override void Load()
	{
		SetRace(10001);
		SetName("_jenifer");
		SetBody(height: 1.1f, weight: 1.1f, lower: 1.1f);
		SetFace(skinColor: 17, eyeType: 4, eyeColor: 119, mouthType: 1);
		SetStand("human/female/anim/female_natural_stand_npc_lassar");
		SetLocation(31, 14628, 8056, 26);
		SetGiftWeights(beauty: 1, individuality: 2, luxury: -1, toughness: 2, utility: 2, rarity: 0, meaning: -1, adult: 2, maniac: -1, anime: 2, sexy: 0);

		EquipItem(Pocket.Face, 3901, 0x00D9E9F7, 0x00930B6A, 0x00474C00);
		EquipItem(Pocket.Hair, 3001, 0x00240C1A, 0x00240C1A, 0x00240C1A);
		EquipItem(Pocket.Armor, 15020, 0x00F98C84, 0x00FBDDD7, 0x00351311);
		EquipItem(Pocket.Shoe, 17013, 0x00000000, 0x00366961, 0x00DAD6EB);

		AddPhrase("Ah, I'm so bored...");
		AddPhrase("Ah. What an unbelievably beautiful weather...");
		AddPhrase("I could never keep this place clean... It always gets dirty.");
		AddPhrase("I thought there was something else that needed to be done...");
		AddPhrase("I wish it would rain so I could take a day off.");
		AddPhrase("I'm gonna get drunk if I drink too much...");
		AddPhrase("I'm so tired...");
		AddPhrase("It would be nice if Riocard drank...");
		AddPhrase("Perhaps I should lose some weight...");
		AddPhrase("Riocard! Did you finish everything I asked you to do?");
		AddPhrase("Riocard. Come play with me.");
		AddPhrase("Today's fortune is... no profit?");
		AddPhrase("Wait a minute...");
	}

	protected override async Task Talk()
	{
		SetBgm("NPC_Jenifer.mp3");

		await Intro(L("Well-groomed purple hair, a face as smooth as flawless porcelain,<br/>and brown eyes with thick mascara complemented by a mole that adds beauty to her oval face.<br/>The jasmine scent fills the air every time her light sepia healer dress moves,<br/>and her red cross earrings dangle and shine as her smile spreads across her lips."));

		Msg("Mmm? How can I help you?", Button("Start Conversation", "@talk"), Button("Shop", "@shop"), Button("Repair Item", "@repair"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());

				if (Title == 11001)
				{
					Msg("So, you rescued the Goddess?<br/>I bet there's an interesting story behind that...");
					Msg("What do you say?<br/>If you don't mind, would you tell me the story sometime...?");
					Msg("...Who knows...your tale may turn into a song that's passed down through generations to come...");
				}
				if (Title == 11002)
				{
					Msg("<username/>, protecting Erinn is a noble thing,<br/>but how about protecting someone near you first?<br/>You won't regret taking my advice.");
				}

				await Conversation();
				break;

			case "@shop":
				Msg("What do you need?<br/>Right now, we're just selling a few food items.");
				OpenShop("JeniferShop");
				return;

			case "@repair":
				Msg(L("I can fix accessories.<br/>I know this sounds funny coming from me, but it's not very good to repair accessories.<br/>First off, it costs too much.<br/>You might be better off buying a new one than repairing it. But if you still want to repair it...<repair rate='98' stringid='(*/accessary/*)' />"));

				while (true)
				{
					var repair = await Select();

					if (!repair.StartsWith("@repair"))
						break;

					var result = Repair(repair, 98, "/accessary/");
					if (!result.HadGold)
					{
						RndMsg(
							L("I have to collect repair fees to cover the repairing cost.."),
							L("I'm sorry, but you'll need more Gold than that to fix this item."),
							L("It's expensive to repair items like this.<br/>I'm sorry, but there's nothing I can do.")
						);
					}
					else if (result.Points == 1)
					{
						if (result.Fails == 0)
							Msg(L("1 point has been restored.")); // Should be 3
						else
							Msg(L("(Missing dialog: Jennifer failing 1 point repair.)")); // Should be 3
					}
					else if (result.Points > 1)
					{
						if (result.Fails == 0)
							Msg(L("Done! Perfect restoration, I say.")); // Should be 3
						else
							Msg(string.Format(L("(Missing dialog: Jennifer failing multi point repair, fails: {0}, successes: {1}.)"), result.Fails, result.Successes));
					}
				}

				Msg(L("It must be very precious to you if you want to repair an accessory.<br/>I totally understand. But it takes on another kind of charm as it tarnishes, you know?<br/>Well, see you again.<repair hide='true'/>"));
				break;
		}

		End("Thank you, <npcname/>. I'll see you later!");
	}

	private void Greet()
	{
		if (Memory <= 0)
		{
			Msg(FavorExpression(), L("Welcome to the Bangor Pub. Are you a first-time visitor?"));
		}
		else if (Memory == 1)
		{
			Msg(FavorExpression(), L("I think I've met you once before... You name is... <username/>, am I right?"));
		}
		else if (Memory == 2)
		{
			Msg(FavorExpression(), L("Hahaha...<username/>, you are here again. You came to see me, didn't you?"));
		}
		else if (Memory <= 6)
		{
			Msg(FavorExpression(), L("<username/>, come on in. I was wondering why you haven't been coming. Heheh..."));
		}
		else
		{
			Msg(FavorExpression(), L("You come here quite often, <username/>. But don't be so obvious with your feeling..<br/>Riocard is going to get jealous. Ha ha..."));
		}

		UpdateRelationAfterGreet();
	}

	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				Msg(FavorExpression(), "My name? It's <npcname/>. Ha ha.<br/>Welcome to my Pub.");
				ModifyRelation(Random(2), 0, Random(3));
				break;

			case "rumor":
				Msg(FavorExpression(), "If you're curious, try talking to Riocard over there.<br/>He may not look it, but he's smart and has a good memory.<br/>And he knows a lot of rumors from various places. Ha ha.");
				Msg("Yeah, the one over there in yellow wearing a hat.");
				ModifyRelation(Random(2), 0, Random(3));
				break;

			case "about_skill":
				Msg("Even the gentlest of the Bards can be downright scary if they get angry.<br/>When they fight, they throw their instruments like weapons!");
				break;

			case "about_arbeit":
				Msg("Well...<br/>Riocard is enough help for now...");
				Msg("Let me talk to Riocard first and I'll get back to you.");
				break;

			case "shop_misc":
				Msg("Ha ha... You're looking for Gilmore's shop.<br/>It's just over there. You might have a hard time finding it though, 'cause you have to go through an alley...");
				break;

			case "shop_grocery":
				Msg("Yes, you've come to the right place. We carry food items, too.<br/>Would you like to see if there's something you need?");
				break;

			case "shop_healing":
				Msg("Unfortunately we don't have a Healer's House in this town.<br/>Although, I thought I saw<br/>Comgan carrying some potions and bandages.");
				Msg("You should go there and check it out.");
				break;

			case "shop_inn":
				Msg("Ha ha... A lot of people ask me that.<br/>Maybe I should start an inn, too.");
				Msg("Hey, Riocard! What do you think?");
				Msg("...");
				Msg("He's not answering... jerk...");
				break;

			case "shop_bank":
				Msg("Hmm.<br/>You should go talk to Bryce for that.");
				Msg("Yeah, you know. That dandy gentleman<br/>by the General Shop. Tee hee.");
				break;

			case "shop_smith":
				Msg("You mean Elen's Blacksmith's Shop?<br/>It's just over there, hehehe...");
				break;

			case "skill_rest":
				Msg("...Why don't you use it at this Pub? Tee hee.");
				break;

			case "skill_instrument":
				Msg("It's a romantic skill.");
				Msg("You can easily pick it up<br/>as long as you have an instrument...");
				break;

			case "skill_composing":
				Msg("...Many people misunderstand.<br/>Composition is not merely a transcription<br/>but a way to express the melody inside your heart.");
				Msg("Listening to some people play nowadays,<br/>it seems that only a very few people actually play what they truly feel inside.<br/>Others seem to just repeat what they heard.");
				Msg("...If being a bard was so easy,<br/>I wouldn't have given up that path...");
				break;

			case "skill_tailoring":
				Msg("...'I hear you need a Tailoring Kit for that.<br/>The General Shop probably carries it.");
				break;

			case "skill_gathering":
				Msg("I don't know... I'll tell you if I need it. Ha ha.");
				break;

			case "temple":
				Msg("Now that you mention it, Comgan does want to build a church in this town.");
				Msg("The way I look at it, I doubt he'll be able to build one even if he grows up<br/>to be as old as priests from other villages.<br/>If you'd like, why don't you go lend him a hand?");
				break;

			case "school":
				Msg("No, this town doesn't have a school.<br/>Business is pretty slow during the day, so it would nice to learn something...");
				break;

			case "skill_campfire":
				Msg("If you're going to make fire, please go to a safe place.<br/>I can't have my Pub set on fire.");
				break;

			case "shop_restaurant":
				Msg("Ha ha... You're quite picky aren't you...?<br/>You can just buy food here.<br/>Who needs a restaurants?");
				break;

			case "shop_armory":
				Msg("Hmm... You want to buy weapons?");
				Msg("Then go over to the Blacksmith and<br/>speak with Elen.");
				Msg("Elen seems to be selling the items<br/>that Edern makes. Hehe...");
				break;

			case "shop_cloth":
				Msg("I'm not happy about not having a clothing shop here either.<br/>I have nowhere to go if I feel like getting new clothes.");
				Msg("Gilmore carries some clothes at his shop,<br/>but he doesn't have anything beautiful and I only end up getting nagged there.");
				Msg("Well, I guess on the bright side. I save money that way... Hehehe.");
				break;

			case "shop_bookstore":
				Msg("So you like books, huh?<br/>Oh, me too.");
				Msg("I've read all kinds of books since I was a child.<br/>Tales of Dungeon Explorations, The White Doe Story, The Story of Sealstones...<br/>The White Doe Story is particularly touching, don't you think...?");
				Msg("But this town doesn't really have a bookstore where you can buy decent books.<br/>Maybe I should even start a bookshop?");
				Msg("Hey, Riocard! What do you think?");
				Msg("...");
				Msg("He's ignoring me again. I should just fire him.");
				break;

			case "shop_goverment_office":
				Msg("A town office? In this town?<br/>Then I would have to pay taxes. No way!");
				break;

			case "graveyard":
				Msg("You seem to enjoy scary stories.");
				Msg("You should probably go talk to Riocard then.<br/>I don't think it's something you should talk about with a frail lady like me.");
				break;

			case "lute":
				Msg("If you really want to buy it,<br/>try the General Shop just over there.");
				Msg("Be careful, though.<br/>If you take too long<br/>you might get yelled at...");
				break;

			case "complicity":
				Msg("Actually, I hired Riocard to attract some customers...<br/>Shh.");
				Msg("But he's not too bright when it comes to that. Tee hee.<br/>So now I just give him chores to do around the Pub.");
				Msg("Don't tell Riocard I said that.");
				break;

			case "tir_na_nog":
				Msg("Romantic, isn't it?<br/>That there is a place in this world that is connected to Tir Na Nog,<br/>where there's no death or pain.  Just full of youth and joy,<br/>everyone living in harmony...");
				Msg("I don't believe it entirely,<br/>but I still think it's a wonderful story.");
				break;

			case "mabinogi":
				Msg("I know a thing or two about it...<br/>but I'm very good with words. Hehe....");
				Msg("Would you like to show me if you know some?");
				break;

			case "musicsheet":
				Msg("Now you're talking.<br/>I used to read music scores and play music too.");
				Msg("But I haven't played in such a long time. I might've forgotten everything...");
				break;

			case "jewel":
				Msg("Hohoho, no woman can resist gems.<br/>I love Garnets for their mysterious purple color.");
				Msg("Pure Garnets shed mysterious, clear light<br/>tinted in a deep wine color.<br/>Doesn't the gem sound perfect for me?<br/>What do you think, Riocard?");
				break;

			default:
				RndFavorMsg(
					"Hang on... Riocard! Where are you? I've got something to ask you.",
					"Hmm, it sounds like an interesting story, but can you tell other ones too?",
					"Hmm. It seems like an interesting topic, but I don't know too much about it.<br/>Can we talk about something else?",
					"Trying to capture a woman's heart with a story like that simply doesn't work. Ha ha.<br/>Ha ha. Just for you, <username/>, I'll let this one go."
				);
				ModifyRelation(0, 0, Random(3));
				break;
		}
	}
}

public class JeniferShop : NpcShopScript
{
	public override void Setup()
	{
		Add("Food", 50001);    // Big Lump of Cheese
		Add("Food", 50002);    // Slice of Cheese
		Add("Food", 50004);    // Bread
		Add("Food", 50005);    // Large Meat
		Add("Food", 50006, 5); // Sliced Meat x5

		Add("Gift", 52010); // Ramen
		Add("Gift", 52019); // Heart Cake
		Add("Gift", 52021); // Slice of Cake
		Add("Gift", 52022); // Wine
		Add("Gift", 52023); // Wild Ginseng

		Add("Cooking Tools", 40042); // Cooking Knife
		Add("Cooking Tools", 40043); // Rolling Pin
		Add("Cooking Tools", 40044); // Ladle
		Add("Cooking Tools", 46004); // Cooking Pot
		Add("Cooking Tools", 46005); // Cooking Table

		Add("Event"); // Empty
	}
}