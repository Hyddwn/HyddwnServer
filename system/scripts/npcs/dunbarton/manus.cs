//--- Aura Script -----------------------------------------------------------
// Manus
//--- Description -----------------------------------------------------------
// Healer
//---------------------------------------------------------------------------

public class ManusScript : NpcScript
{
	public override void Load()
	{
		SetRace(25);
		SetName("_manus");
		SetFace(skinColor: 27, eyeType: 12, eyeColor: 27, mouthType: 18);
		SetLocation(19, 881, 1194, 0);
		SetGiftWeights(beauty: 0, individuality: 1, luxury: 0, toughness: 1, utility: 2, rarity: -1, meaning: 0, adult: 0, maniac: 0, anime: 1, sexy: 1);

		EquipItem(Pocket.Face, 4900, 0x003C3161, 0x00737F39, 0x00856430);
		EquipItem(Pocket.Hair, 4096, 0x002B2822, 0x002B2822, 0x002B2822);
		EquipItem(Pocket.Armor, 15030, 0x00CFD0B5, 0x00006600, 0x00006600);
		EquipItem(Pocket.Shoe, 17035, 0x00223846, 0x00574662, 0x00808080);

		AddPhrase("A healthy body for a healthy mind!");
		AddPhrase("Alright! Here we go! Woo-hoo!");
		AddPhrase("Come! A special potion concocted by Manus for sale now!");
		AddPhrase("Here, let's have a look.");
		AddPhrase("I wish there was something I could spend this extra energy on...");
		AddPhrase("Perhaps Stewart could tell me about this...");
		AddPhrase("There's nothing like a massage for relief when your muscles are tight! Hahaha!");
		AddPhrase("Why did you let it go this bad?!");
		AddPhrase("You should exercise more. You're so thin.");
	}

	protected override async Task Talk()
	{
		SetBgm("NPC_Manus.mp3");

		await Intro(L("This man is wearing a green and white healer's dress.<br/>His thick, dark hair is immaculately combed and reaches down to his neck,<br/>his straight bangs accentuating a strong jaw and prominent cheekbones."));

		Msg("Ha! Tell me everything you need!", Button("Start a Conversation", "@talk"), Button("Shop", "@shop"), Button("Get Treatment", "@healerscare"), Button("Heal Pet", "@petheal"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());

				if (Title == 11001)
				{
					Msg("Oh. <username/>? Good to see you!");
					Msg("By the way...<br/>There are so many titles nowadays that<br/>it's not easy to remember them all.");
					Msg("What do you think?");
					Msg("Hey, hey. Are you ticked off at me?<br/>I'm just joking... Hahaha. Sorry, sorry.");
				}
				else if (Title == 11002)
				{
					Msg("Wow, what a title!<br/><username/>, I feel like<br/>I need to treat you differently. Haha!");
				}

				await Conversation();
				break;

			case "@shop":
				Msg("Is there something I can help you with?");
				OpenShop("ManusShop");
				return;

			case "@healerscare":
				if (Player.Life == Player.LifeMax)
				{
					Msg("Huh? What's this? You are fine. What do you mean you need treatment?<br/>Foot fungus, by any chance? Hahaha!");
				}
				else
				{
					Msg("My, how did you manage to get hurt this badly?! We'll have to bandage it right now!<br/>Oh, but don't forget that you have to pay the fee. It's 90 Gold.", Button("Receive Treatment", "@gethealing"), Button("Decline", "@cancel"));
					if (await Select() == "@gethealing")
					{
						if (Gold >= 90)
						{
							Gold -= 90;
							Player.FullLifeHeal();
							Msg("There, how do you like my skills?! Don't be so careless with your body.");
						}
						else
						{
							Msg("Gee, that's not enough money.<br/>I may be generous, but I can't do this for free.");
						}
					}
				}
				break;

			case "@petheal":
				if (Player.Pet == null)
				{
					Msg("You'll need to show me your pet first before I can diagnose it.<br/>Don't you think so?");
				}
				else if (Player.Pet.IsDead)
				{
					Msg("Your pet is already knocked unconscious! Revive it first, immediately.");
				}
				else if (Player.Pet.Life == Player.Pet.LifeMax)
				{
					Msg("What? Your pet seems perfectly fine. Why would it need to be treated?");
				}
				else
				{
					Msg("How did you get your pet to be hurt this badly?! I'll treat it right now!<br/>By the way, it will cost you 180 Gold. Don't forget that.", Button("Recieve Treatment", "@recieveheal"), Button("Decline the Treatment", "@end"));
					if (await Select() == "@recieveheal")
					{
						if (Gold < 180)
						{
							Msg("Hmmm...I think you are short.<br/>I may be a generous person, but I can't do business like this... for free...");
						}
						else
						{
							Gold -= 180;
							Player.Pet.FullLifeHeal();
							Msg("Your pet is fixed, and ready to go! Take care of your pet as much as you'd take care of yourself.");
						}
					}
				}
				break;
		}

		End("Thank you, <npcname/>. I'll see you later!");
	}

	private void Greet()
	{
		if (DoingPtjForNpc())
		{
			Msg(FavorExpression(), L("I trust that the task you're working on is going well?"));
		}
		else if (Memory <= 0)
		{
			Msg(FavorExpression(), L("You've never been here before, have you? Where does it hurt?"));
		}
		else if (Memory == 1)
		{
			Msg(FavorExpression(), L("You look familiar. Haven't we met before?"));
		}
		else if (Memory == 2)
		{
			Msg(FavorExpression(), L("You are back, <username/>."));
		}
		else if (Memory <= 6)
		{
			Msg(FavorExpression(), L("I seem to be seeing you often."));
		}
		else
		{
			Msg(FavorExpression(), L("<username/>, you made this your second home, didn't you? Ha ha.<br/>Try visiting other places, too."));
		}

		UpdateRelationAfterGreet();
	}

	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				if (Memory == 1)
				{
					Msg(FavorExpression(), "My name is <npcname/>.");
					ModifyRelation(1, 0, 0);
				}
				else
				{
					GiveKeyword("shop_healing");
					Msg(FavorExpression(), "I am the healer in this town. I'm good at what I do,<br/>so feel free to come by if you get sick.");
					ModifyRelation(Random(2), 0, Random(3));
				}
				break;

			case "rumor":
				GiveKeyword("shop_restaurant");
				Msg(FavorExpression(), "Have you been to Glenis' Restaurant yet?<br/>Make sure you pay a visit and order something.<br/>Eating well is the most important thing in maintaining good health. Hahaha!");
				ModifyRelation(Random(2), 0, Random(3));
				break;

			case "about_skill":
				if (!HasSkill(SkillId.Rest))
				{
					Msg("Really tired, aren't you? And you become easily fatigued.<br/>It's because your health keeps on getting spent without getting replenished.<br/>How about learning the Resting skill?<br/>You can ask other people to find out where you can learn it.");
				}
				else if (IsSkill(SkillId.Rest, SkillRank.RF))
				{
					if (GetPtjDoneCount(PtjType.HealersHouse) < 3)
					{
						Msg("Oh, I see that you know the Resting skill. Do you find it handy?<br/>If you want, I could raise the skill level for you by one.");
						Msg("Ah! Ah! Of course, it comes at a price.<br/>Hahah... Let's see...");
						Msg("I know! I always need more help, so why don't you do some work around here?<br/>It doesn't have to be with me. You can help any healer in any town.<br/>I'll raise the skill level for you if you do some healer part-time work. Any questions?");
					}
					else
					{
						TrainSkill(SkillId.Rest, 1);
						Msg("(Missing Dialog: Manus Trains Rest)");
					}
				}
				else
				{
					Msg("Hmm. You know the Resting skill fairly well.<br/>I don't know if you're expecting anything more from me,<br/>but I don't know anything more advanced, either.<br/>You... Aren't you being a little too ambitious? Hahaha...");
				}
				break;

			case "shop_misc":
				Msg("Hmm. Don't tell me you really don't know.<br/>I saw it on your Minimap a while ago. You will find Walter there.<br/>I don't know what else to tell you.");
				Msg("Yes, it's the General Shop, that's why Walter sells general goods.<br/>I know it's obvious. Haha.");
				break;

			case "shop_grocery":
				GiveKeyword("shop_restaurant");
				Msg("The Grocery Store...? You mean the Restaurant.");
				break;

			case "shop_healing":
				Msg("You're right there.");
				Msg("Looking to go somewhere else?");
				break;

			case "shop_inn":
				Msg("I've never heard that there is an inn in this town.");
				Msg("Who told you that?");
				break;

			case "shop_bank":
				Msg("You have to go to the Square for the Bank.<br/>Look for a sign with a winged chest.");
				Msg("Don't tell me you don't know what a Bank sign looks like?");
				break;

			case "shop_smith":
				GiveKeyword("shop_armory");
				Msg("There is a Blacksmith's Shop in our town?<br/>Well Nerys would know.<br/>Go find her at the Weapons Shop.");
				Msg("If she doesn't know,<br/>then I think it would be safe to assume that there isn't one here.");
				break;

			case "skill_composing":
				Msg("Anyone can compose these days...<br/>Everything sounds the same.<br/>This is the age of the copycats.");
				break;

			case "skill_tailoring":
				GiveKeyword("shop_cloth");
				Msg("Simon knows clothing-related skills well.<br/>Although, knowing him, I doubt he would pass on anything.");
				Msg("Why don't you go see him anyway?<br/>He's at the Clothing Shop.");
				break;

			case "skill_magnum_shot":
				GiveKeyword("school");
				Msg("That's something you should ask that<br/>tomboy teacher, Aranwen.");
				Msg("You would probably find her at the School, eh?");
				break;

			case "skill_counter_attack":
				GiveKeyword("school");
				Msg("Go to the School and ask Aranwen there.");
				break;

			case "skill_smash":
				GiveKeyword("school");
				Msg("Hmm. I'd like to show off and teach it to you myself,<br/>but if you develop bad habits or anything,<br/>you would curse me for the rest of your life. Ha ha.");
				Msg("Go talk to Aranwen.<br/>She teaches combat skills, so she should be at the School.");
				break;

			case "skill_gathering":
				Msg("Sometimes Stewart and I go<br/>gather herbs together.");
				Msg("It's no fun to do it by yourself.<br/>By the way, rumor has it that<br/>people sometimes fight over who gets to gather more.");
				Msg("But it's not my place to tell people what to do...<br/>Just come see me if you get hurt while fighting!");
				break;

			case "square":
				Msg("Go outside and follow the road and you'll see it.<br/>When you see the bell tower, that's the Square.<br/>I guess you missed it on your way.");
				Msg("(What a careless kid.)");
				break;

			case "pool":
				Msg("Haha. This town mostly uses wells,<br/>so there's no need for a reservoir.");
				break;

			case "farmland":
				Msg("Right. I hear there are lots of<br/>abnormally large rats eating away the crops.<br/>If you happen to see them,<br/>do us a favor and take them out.");
				break;

			case "shop_headman":
				Msg("Ha ha. The town is ruled by a Lord.<br/>A chief? A chief... Hahaha!");
				break;

			case "temple":
				Msg("Want to see Kristell?<br/>Look at the Minimap! This would be a very good time to use your eyes for once!<br/>...But then again, I suppose she could be hard to locate on the Minimap.");
				Msg("Oh, what the heck! I'll be nice and tell you where she is.<br/>Leave here and follow the road. You'll hit the Square.");
				Msg("From there, go to an alley near Glenis' Restaurant.<br/>Go up the steps there and turn left, and the Church will be right there.");
				Msg("If you can't remember all this, take a look at your Minimap<br/>when you get to the Square.<br/>If you're still not sure, ask other people.");
				break;

			case "school":
				Msg("The School? Follow this road all the way.<br/>You should be able to see it on your Minimap.<br/>It's on the right side of the road,<br/>so it shouldn't be too difficult to find.");
				break;

			case "skill_windmill":
				GiveKeyword("school");
				Msg("So I look like someone who would know about<br/>such a barbaric skill, do I?");
				Msg("Go talk to Aranwen. She's at the School.");
				break;

			case "shop_restaurant":
				Msg("The Restaurant! The Restaurant! Yes, the Restaurant!");
				Msg("Take a look at your Minimap. It should be on there.");
				break;

			case "shop_armory":
				Msg("The Weapons Shop is just over there.<br/>Nerys is often outside.<br/>If you see a slender girl with sharp eyes and red hair, that's Nerys.");
				Msg("By the way... what are you buying?");
				break;

			case "shop_cloth":
				Msg("The Clothing Shop? Oh, you mean Simon's.<br/>It's between the Bank and the General Shop.<br/>Look around the Square and you'll find it.");
				Msg("Say hello to Simon for me.<br/>Also, tell him that I'm very much enjoying this robe.");
				break;

			case "shop_bookstore":
				Msg("You don't look like the book-ish type.<br/>Just play the game. What good are books?");
				Msg("Ha ha. I'm just kidding. Did I offend you?<br/>I'll tell you, so quit looking at me like that.");
				Msg("Exit here and follow the road to the north.<br/>There is an alley in one corner of the north entrance.<br/>Go down there and you'll find the Bookstore.");
				Msg("If you see a little girl with a pair of glasses,<br/>that's where the Bookstore is.");
				break;

			case "shop_goverment_office":
				Msg("That's where the beautiful Eavan works.<br/>The Lord and... whatchama call it...<br/>the Captain of the Royal Guards are also there,<br/>but they hardly leave the Town Office.");
				Msg("If you have any business there, talk to Eavan.<br/>Not only is she pretty, but she's kind, too.<br/>She'll kindly answer any of your questions.");
				break;

			case "bow":
				GiveKeyword("shop_armory");
				Msg("Just across from here is the Weapons Shop. Why don't you buy one there?");
				break;

			case "tir_na_nog":
				Msg("Hmm. Tir Na Nog?<br/>I didn't think you'd be the type to be interested in the afterlife. Haha.");
				Msg("If you're interested in the afterlife,<br/>just do a lot of good deeds.<br/>I don't really care for it... Hahaha...");
				break;

			case "mabinogi":
				GiveKeyword("school");
				Msg("Well, I did hear about it...<br/>I don't know exactly what it entails...<br/>Stewart would probably know about it since he's the instructor at the School.");
				Msg("Go ask Stewart at the School.");
				break;

			default:
				RndFavorMsg(
					"Well... I don't know.",
					"Don't ask me about that.",
					"I don't know what that is.",
					"Hmm. You have strange interests.",
					"Hmm. I don't know anything about that.",
					"You sure like to ask about strange things...",
					"I think that's enough. It's not even a topic I'm interested in.",
					"Wait... Have I heard about that before? If I did, I don't really remember.",
					"Talking about unfamiliar subjects makes us both tired. Let's talk about something else."
				);
				ModifyRelation(0, 0, Random(3));
				break;
		}
	}
}

public class ManusShop : NpcShopScript
{
	public override void Setup()
	{
		Add("Potions", 51037, 10); // Base Potion
		Add("Potions", 51001);     // HP 10 Potion
		Add("Potions", 51011);     // Stamina 10 Potion
		Add("Potions", 51000);     // Potion Concoction Kit
		Add("Potions", 51002);     // HP 30 Potion x1
		Add("Potions", 51002, 10); // HP 30 Potion x10
		Add("Potions", 51002, 20); // HP 30 Potion x20
		Add("Potions", 51012);     // Stamina 30 Potion x1
		Add("Potions", 51012, 10); // Stamina 30 Potion x10
		Add("Potions", 51012, 20); // Stamina 30 Potion x20

		Add("First Aid Kits", 60005, 10); // Bandage x10
		Add("First Aid Kits", 60005, 20); // Bandage x20
		Add("First Aid Kits", 63000, 10); // Phoenix Feather x10
		Add("First Aid Kits", 63000, 20); // Phoenix Feather x20

		Add("Etc", 1044);     // Reshaping Your Body
		Add("Etc", 1047);     // On Effective Treatment of Wounds

		if (IsEnabled("SystemPet"))
			Add("First Aid Kits", 63032); // Pet First-Aid Kit

		if (IsEnabled("G16HotSpringRenewal"))
		{
			Add("Etc", 91563);    // Hot Spring Ticket x1
			Add("Etc", 91563, 5); // Hot Spring Ticket x5
		}

		if (IsEnabled("PuppetMasterJob"))
		{
			Add("Potions", 51201);     // Marionette 30 Potion x1
			Add("Potions", 51201, 10); // Marionette 30 Potion x10
			Add("Potions", 51201, 20); // Marionette 30 Potion x20

			Add("First Aid Kits", 63716, 10); // Marionette Repair Set x10
			Add("First Aid Kits", 63716, 20); // Marionette Repair Set x20
		}
	}
}
