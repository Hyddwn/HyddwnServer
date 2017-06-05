//--- Aura Script -----------------------------------------------------------
// Aodhan
//--- Description -----------------------------------------------------------
// Emain Macha Royal Guard Captain. PQ and Fomor scroll quest merchant.
//---------------------------------------------------------------------------

public class AodhanScript : NpcScript
{
	public override void Load()
	{
		SetRace(10002);
		SetName("_aodhan");
		SetBody(height: 1.3f, upper: 1.2f);
		SetFace(skinColor: 20, eyeType: 12, eyeColor: 98, mouthType: 0);
		SetStand("monster/anim/ghostarmor/Tequip_C/ghostarmor_Tequip_C01_stand_friendly");
		SetLocation(52, 34544, 46247, 225);
		SetGiftWeights(beauty: 0, individuality: -1, luxury: 0, toughness: 2, utility: 1, rarity: 0, meaning: 2, adult: 0, maniac: -1, anime: -1, sexy: 0);

		EquipItem(Pocket.Face, 4900, 0x006A4200, 0x0048676C, 0x00F36097);
		EquipItem(Pocket.Hair, 4043, 0x00BCD5AE, 0x00BCD5AE, 0x00BCD5AE);
		EquipItem(Pocket.Armor, 13024, 0x00CEBFB5, 0x003F524E, 0x00161D29);
		EquipItem(Pocket.RightHand2, 40033, 0x00B7B6B8, 0x00C48246, 0x009AAFA2);


		AddPhrase("We should make night training twice as hard.");
		AddPhrase("Another peaceful day.");
		AddPhrase("Is the quality of the trainees getting worse...?");
		AddPhrase("This seems too easy...");
		AddPhrase("......Did the patrol officer come back?");
		AddPhrase("You need to get permission in order to enter the castle.");
		AddPhrase("No monsters in sight, Sir.");
		AddPhrase("I wonder if the weapons I ordered came in yet...");
		AddPhrase("...");
	}

	protected override async Task Talk()
	{
		SetBgm("NPC_Aodhan.mp3");

		await Intro(L("Aodhan's sharp features mirror his expression.<br/>He has thick eyebrows above sharp azure eyes, a thin nose, and an angular chin.<br/>Aodhan's thin lips are taut with determination. The single braid hanging from his short hair instantly identifies him as a Knight.<br/>Tall and sure of himself, he sticks out his chest as he talks, leaving a powerful impression over those around him."));

		Msg("Good to see you.", Button("Start Conversation", "@talk"), Button("Shop", "@shop"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());

				if (Player.IsUsingTitle(11001))
				{
					Msg("I'm always thankful for your sacrifice<br/>and for rescuing the Goddess.");
				}
				else if (Player.IsUsingTitle(11002))
				{
					Msg("...I will remember...<br/>helping you<br/>for the rest of my life.");
					Msg("...Thank you <username/>.");
				}

				await Conversation();
				break;

			case "@shop":
				Msg("Do you need something?");
				OpenShop("AodhanShop");
				return;
		}

		End("Thank you, <npcname/>. I'll see you later!");
	}

	private void Greet()
	{
		if (Memory <= 0)
		{
			Msg(FavorExpression(), L("Nice to meet you. I'm the Captain of the Guards, Aodhan."));
		}
		else if (Memory == 1)
		{
			Msg(FavorExpression(), L("What brings you here?"));
		}
		else if (Memory == 2)
		{
			Msg(FavorExpression(), L("It's you again, <username/>."));
		}
		else if (Memory <= 6)
		{
			Msg(FavorExpression(), L("I'm seeing you a lot these days, <username/>."));
		}
		else
		{
			Msg(FavorExpression(), L("How may I help you, <username/>?"));
		}

		UpdateRelationAfterGreet();
	}

	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				Msg(FavorExpression(), "I am the Captain of the Guards.<br/>It is the responsibility of the Royal Guards to protect the Lord and his family<br/>but since we don't have a separate army, we are also in charge of protecting the city.");
				ModifyRelation(Random(2), 0, Random(3));
				break;

			case "rumor":
				Msg(FavorExpression(), "There is something uneasy in the air as of late.<br/>We have tightened up our security but you should still be careful if you are ever traveling during the night.<br/>You should especially keep your distance from the Club.<br/>You never know what that Lucas is really up to...");
				ModifyRelation(Random(2), 0, Random(3));
				break;

			case "about_skill":
				Msg("It is difficult for me to teach anything<br/>to someone who is not a soldier...");
				break;

			case "shop_misc":
				Msg("The General Shop? Why don't you go visit Galvin at the Observatory?<br/>You can get there by walking along the Square.<br/>It's on the opposite side of the Castle.");
				break;

			case "shop_grocery":
				Msg("If you are looking for a Restaurant, follow the road toward south.<br/>There is a Restaurant by the lake.<br/>It is called \"Loch Lios\".<br/>You shouldn't have trouble finding it since they have a sign outside.");
				break;

			case "shop_healing":
				Msg("The Healer's House is located East,<br/>a little way past the Bank.<br/>It is run by Agnes.");
				break;

			case "shop_inn":
				Msg("There isn't a place<br/>like the Inn around here...<br/>Sorry, I can't help you.");
				break;

			case "shop_bank":
				Msg("Erskin Bank is right in front of here.<br/>You'll see it if you just turn around a bit.");
				break;

			case "shop_smith":
				Msg("We don't have Blacksmith's Shop... But we do have a Weapons Shop.<br/>Follow the alley southeast,<br/>and you'll be able to see a sign and a lady standing next to it.<br/>That place is Osla's Weapons Shop.");
				break;

			case "skill_rest":
				Msg("As a Warrior, knowing when to rest is important<br/>so that you can prepare for what lies ahead.");
				break;

			case "skill_range":
				Msg("Spotting your enemy from a distance and attacking it before<br/>it spots you is the basic strategy of field warfare.<br/>Moreover, I believe that using this strategy in other daily<br/>life activities can be beneficial as well.");
				break;

			case "skill_instrument":
				Msg("I enjoy listening to music...");
				break;

			case "skill_composing":
				Msg("If you ask Nele at the Square, he'll be able to give you an answer.<br/>I heard that he was once a pupil of a famous musician in Tara.");
				break;

			case "skill_tailoring":
				Msg("You can Ailionoa at the Clothing Shop about that.");
				break;

			case "skill_magnum_shot":
				Msg("With this skill, you'll be able to knock down an enemy in a single blow.<br/>Since Archers are at a disadvantage when enemies are in close proximity,<br/>this skill can be very useful for those who use the bow as their main weapon.");
				break;

			case "skill_counter_attack":
				Msg("It uses the enemy's force against them.<br/>You can say it is the most sophisticated skill, at least for attacking.");
				break;

			case "skill_smash":
				Msg("This is a skill that reminds me of the important of striking first.");
				break;

			case "skill_windmill":
				Msg("In order to be able to face multiple enemies at once,<br/>you'll need to learn this skill.<br/>The stronger your enemies become,<br/>the better this skill can be used.");
				break;

			case "square":
				Msg("The area where the large water fountain is<br/>in the middle of the city is known as the Square.");
				break;

			case "farmland":
				Msg("If you go outside of the city, you'll see a farm cultivated by the town's people.");
				break;

			case "windmill":
				Msg("There's no Windmill around here.");
				break;

			case "brook":
				Msg("I don't think there's anything like that around here.");
				break;

			case "shop_headman":
				Msg("This town is under the Lord's authority.<br/>We don't need a Chief.");
				break;

			case "temple":
				Msg("If you go across the bridge that crosses over the lake,<br/>you'll find the Church of Emain Macha.");
				break;

			case "school":
				Msg("There used to be a large Magic School here...<br/>but I think it was closed down.");
				break;

			case "skill_campfire":
				Msg("Please be careful<br/>when you're handling fire around the town.");
				break;

			case "shop_restaurant":
				Msg("There's a Restaurant called \"Loch Lios\".<br/>I don't usually go there all that often, but<br/>it is a very popular spot amongst travelers.");
				break;

			case "shop_armory":
				Msg("Osla may be a woman,<br/>but she is very talented at repairing weapons.<br/>You can definitely trust the quality of the weapons in her shop.");
				break;

			case "shop_cloth":
				Msg("Tre'imhse Cairde' is located at the eastern end of the town.<br/>It is right across the Auditorium.<br/>It shouldn't be difficult to find.");
				break;

			case "shop_bookstore":
				Msg("Mr. Buchanan is probably in Tara right now.<br/>His main bookstore is in Tara<br/>but I've heard that he takes orders from bookstores in each town and mass prints them.");
				break;

			case "shop_goverment_office":
				Msg("All the administrative work for the city is conducted inside the castle.<br/>We don't have a separate Town Hall.");
				Msg("If it about lost items,<br/>Galvin normally takes care of things like that<br/>so go talk to him.");
				Msg("You can find him on the eastside of the Church entrance,<br/>near the fountain at the Square.");
				break;

			case "graveyard":
				Msg("For the sake of all who have passed away...");
				Msg("......");
				Msg("I'll protect it no matter what this time.");
				break;

			case "skill_fishing":
				Msg("It is fine to enjoy fishing at the lake, but<br/>please don't pollute the lake by<br/>throwing trash anywhere on the ground.");
				break;

			case "bow":
				Msg("Bows and bow strings should be ready at all times just in case anything happens.<br/>Not to mention the supply of arrows.");
				break;

			case "lute":
				Msg("I enjoy listening to music...");
				break;

			case "complicity":
				Msg("Do you mean... Galvin?<br/>Try not to get offended by him, he means no harm.<br/>His problem is that he is just too naive...");
				break;

			case "tir_na_nog":
				Msg("It is supposed to be the ideal place, where everyone can be happy....<br/>...But I'm not too sure if it's real.");
				break;

			case "mabinogi":
				Msg("It is more like a hymn for heroes.<br/>You can ask a Bard if you want to hear it.");
				break;

			case "musicsheet":
				Msg("You should ask Nele.<br/>He always seems to be at the Square...");
				break;

			case "breast":
				Msg("...");
				break;

			case "g3_DarkKnight":
				Msg("Dark Knight...<br/>I've heard of them<br/>awhile ago.");
				Msg("They are those who have alligned with the Fomors<br/>and use the power and rage of humans as the source of their power...");
				Msg("...I cannot forgive them.<br/>Becoming a Dark Knight means...");
				Msg("Though you are a human<br/>you choose to lead the way<br/>in killing other humans.");
				Msg("...");
				Msg("Sorry for the long answer.<br/>That's all I have to say about them.");
				break;

			case "EG_C2_HowTo_Shipboard":
				Msg("As far as I know, there<br/>are no harbor cities around here where you could board a ship.<br/>...Sorry I'm not much help.");
				break;

			case "two_sword_skill":
				Msg("According to people who've gone to Iria,<br/> there's a very specialized technique that is related to the use of the double sword.<br/>At least that's what I've heard.");
				Msg("...If you are interested in the double sword,<br/>you'll have the best luck in Iria.");
				break;

			default:
				RndFavorMsg(
					"I've never heard of it.",
					"I don't know much about that person.",
					"My knowledge is somewhat limited in that area.",
					"Sorry, I don't have an answer.",
					"I don't have an answer to that question."
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
					"I'll accept it.",
					"I'll keep it in the barrack.",
					"Thank you.",
					"Thanks."
				);
				break;
		}
	}
}

public class AodhanShop : NpcShopScript
{
	public override void Setup()
	{
		AddQuest("Quest", 71026, 30); // Collect the Red Bear's Fomor Scrolls
		AddQuest("Quest", 71028, 30); // Collect the Brown Grizzly Bear's Fomor Scrolls
		AddQuest("Quest", 71030, 30); // Collect the Black Grizzly Bear's Fomor Scrolls
		AddQuest("Quest", 71044, 30); // Collect the Imp Fomor Scrolls
		AddQuest("Quest", 71063, 30); // Collect the Lightning Sprite Fomor Scrolls
		AddQuest("Quest", 71064, 30); // Collect the Ice Sprite Fomor Scrolls
		AddQuest("Quest", 71065, 30); // Collect the Fire Sprite Fomor Scrolls
		AddQuest("Quest", 71070, 30); // Collect the Black Ship Rat Fomor Scrolls
		AddQuest("Quest", 71066, 30); // Collect the Flying Sword Fomor Scrolls
		AddQuest("Quest", 71025, 30); // Collect the Brown Bear Fomor Scrolls
		AddQuest("Quest", 71029, 30); // Collect the Red Grizzly Bear Fomor Scrolls
		AddQuest("Quest", 71049, 30); // Collect the Snake Fomor Scrolls
		AddQuest("Quest", 71017, 30); // Collect the White Spider Fomor Scrolls
		AddQuest("Quest", 71018, 30); // Collect the Black Spider Fomor Scrolls
		AddQuest("Quest", 71019, 30); // Collect the Red Spider Fomor Scrolls
		AddQuest("Quest", 71032, 30); // Collect the Mimic Fomor Scrolls
		AddQuest("Quest", 71006, 30); // Collect the Skeleton Fomor Scrolls
		AddQuest("Quest", 71007, 30); // Collect the Red Skeleton Fomor Scrolls
		AddQuest("Quest", 71012, 30); // Collect the Skeleton Wolf Fomor Scrolls

		AddQuest("Party Quest", 100019, 5);  // [PQ] Hunt Down the Brown Bears (10)
		AddQuest("Party Quest", 100020, 30);  // [PQ] Hunt Down the Brown Bears (30)
		AddQuest("Party Quest", 100032, 30);  // [PQ] Hunt Down the Brown Grizzly Bears (30)
		AddQuest("Party Quest", 100034, 30);  // [PQ] Hunt Down the Brown Grizzly Bears (30)
		AddQuest("Party Quest", 100048, 30);  // [PQ] Hunt Down the Grizzly Bears (10)
		AddQuest("Party Quest", 100049, 30);  // [PQ] Hunt Down the Grizzly Bears Cubs (15)
		AddQuest("Party Quest", 100067, 30);  // [PQ] Hunt Down the Dire Sprites (10)
	}
}
