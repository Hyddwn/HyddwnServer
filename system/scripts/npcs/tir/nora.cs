//--- Aura Script -----------------------------------------------------------
// Nora
//--- Description -----------------------------------------------------------
// Inn helper (located right outside the inn)
//---------------------------------------------------------------------------

public class NoraBaseScript : NpcScript
{
	public override void Load()
	{
		SetRace(10001);
		SetName("_nora");
		SetBody(height: 0.85f);
		SetFace(skinColor: 17);
		SetStand("human/female/anim/female_natural_stand_npc_nora02");
		SetLocation(1, 15933, 33363, 186);

		EquipItem(Pocket.Face, 3900, 0xDED7EA, 0xA2C034, 0x004A18);
		EquipItem(Pocket.Hair, 3025, 0xD39A81, 0xD39A81, 0xD39A81);
		EquipItem(Pocket.Armor, 15010, 0x34696E, 0xFDEEEA, 0xC6D8EA);
		EquipItem(Pocket.Shoe, 17006, 0x34696E, 0x9C558F, 0x901D55);

		AddPhrase("I hope the clothes dry quickly.");
		AddPhrase("I would love to listen to some music, but I don't see any musicians around.");
		AddPhrase("No way! There's no such thing as a huge spider.");
		AddPhrase("Oh no! Rats!");
		AddPhrase("Perhaps I should consider taking a day off.");
		AddPhrase("Please wait.");
		AddPhrase("Wait a second.");
		AddPhrase("Wow! Look at that owl! Beautiful!");
	}

	protected override async Task Talk()
	{
		SetBgm("NPC_Nora.mp3");

		// I noticed the intro message is different as of r218 on 1/7/16
		// "A girl in a neat green apron leans out to get a better look at her surroundings. Her hands work busily at this task or the other, always moving, always jingling the cross-shaped earrings in her honey-blonde hair."
		await Intro(
			"A girl wearing a well-ironed green apron leans forward, gazing cheerfully at her sorroundings.",
			"Her bright eyes are azure blue and a faint smile plays on her lips.",
			"Cross-shaped earrings dangle from her ears, dancing playfully between her honey-blonde hair.",
			"Her hands are always busy, as she engages in some chore or another, though she often looks into the distance as if deep in thought."
		);

		Msg("How can I help you?", Button("Start Conversation", "@talk"), Button("Shop", "@shop"), Button("Repair Item", "@repair"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());

				if (Title == 10062) // is a friend of Nora
				{
					var today = ErinnTime.Now.ToString("yyyyMMdd");
					if (today != Player.Vars.Perm["nora_title_gift"])
					{
						Player.Vars.Perm["nora_title_gift"] = today;

						GiveItem(60005, 5); // Bandage x5
						Notice(L("Received Bandage from Nora."));
						SystemMsg(L("Received Bandage from Nora."));

						Msg(L("Welcome, my dear friend.<br/>Don't you think Uncle Piaras has noticed<br/>how much time you and I spend talking to each other?"));
					}
				}

				if (Title == 11002)
				{
					Msg("<username/>, the Guardian of Erinn?<br/>Perfect timing.<br/>Rats keep appearing around town...<br/>Can you kill them for us?");
					Msg("Malcom at the General Shop<br/>is so scared that he won't even step outside...");
				}

				await Conversation();
				break;

			case "@shop":
				Msg("Are you looking for a Tailoring Kit and materials?<br/>If so, you've come to the right place.");
				OpenShop("NoraShop");
				return;

			case "@repair":
				Msg("Do you want to repair your clothes?<br/>Well I can't say I'm perfect at it,<br/>but I'll do my best.<br/>Just in case, when in doubt, you can always go to a professional tailor.<repair rate='94' stringid='(*/cloth/*)|(*/glove/*)|(*/bracelet/*)|(*/shoes/*)|(*/headgear/*)|(*/robe/*)|(*/headband/*)' />");

				while (true)
				{
					var repair = await Select();

					if (!repair.StartsWith("@repair"))
						break;

					var result = Repair(repair, 94, "/cloth/", "/glove/", "/bracelet/", "/shoes/", "/headgear/", "/robe/", "/headband/");
					if (!result.HadGold)
					{
						RndMsg(
							"Do you have enough Gold?",
							"I'm sorry, but you need to pay more to repair that.",
							"Yes, it's my side job, but it certainly is a job.<br/>If you don't have enough money, I can't repair it."
						);
					}
					else if (result.Points == 1)
					{
						if (result.Fails == 0)
							RndMsg(
								"The repair is done.",
								"OK, it was a success.",
								"1 point, I think the clothes were well repaired."
							);
						else
							RndMsg(
								"Oops!",
								"Oh, sorry. I made a mistake.",
								"It says, \"My hands slipped\".<br/>What does that mean?"
							);
					}
					else if (result.Points > 1)
					{
						if (result.Fails == 0)
							RndMsg(
								"It's as good as new!",
								"Perfect repair! Done!",
								"What a surprise! It's repaired perfectly."
						);
						else
							// TODO: Use string format once we have XML dialogues.
							Msg("I hoped for a perfect repair, but there were some mistakes.<br/>The object lost " + result.Fails + " point(s). You won't be too hard on me, right?");
					}
				}

				Msg("Even if you don't wear them carelessly,<br/>clothes wear out after a while,<br/>just like a knife becomes dull over time.<br/>Well, come again!<repair hide='true'/>");
				break;
		}

		End();
	}

	private void Greet()
	{
		if (Memory <= 0)
		{
			Msg(FavorExpression(), L("Welcome!"));
		}
		else if (Memory == 1)
		{
			Msg(FavorExpression(), L("We've met before, right? I remember you!"));
		}
		else if (Memory == 2)
		{
			Msg(FavorExpression(), L("I'm always glad to see you, <username/>!"));
		}
		else if (Memory <= 6)
		{
			Msg(FavorExpression(), L("Nice to see you, <username/>."));
		}
		else
		{
			Msg(FavorExpression(), L("Hello, <username/>!<br/>It looks like Uncle Piaras is keeping an eye on you<br/>since we talk so often. Didn't you notice?"));
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
					Msg("My name is <npcname/>. Please don't forget it.");
					ModifyRelation(1, 0, Random(2));
				}
				else
				{
					Msg(FavorExpression(), "I take care of chores at the Inn.<br/>I sometimes get tired of it, but after trying other jobs,<br/>I realized this is the job for me.<br/>It allows me to daydream.");
					ModifyRelation(Random(2), 0, Random(2));
				}
				break;

			case "rumor":
				GiveKeyword("square");
				Msg("The Square is right up the little hill next to us.<br/>It's worth a visit if you have some time.");
				ModifyRelation(Random(2), 0, Random(2));
				break;

			case "about_skill":
				Msg("Are you making good use of the Rest skill?<br/>Here's a tip. Only for you, <username/>.<br/>If you continue to rank up your Rest skill,<br/>your HP will increase steadily.");
				break;

			case "about_arbeit":
				Msg("Are you interested in the Inn business?<br/>If so, why don't you ask Uncle Piaras?<br/>He is in the Inn.");
				break;

			case "shop_misc":
				Msg("Go up the hill.<br/>It's just the next building.");
				Msg("Petty Malcolm will probably be at his General Shop.<br/>Don't pay any attention to what he says!");
				break;

			case "shop_grocery":
				Msg("Caitin occasionally visits our Inn<br/>to share the food she makes.<br/>I want to learn how to cook from her,<br/>but I'm tied to this Inn.");
				break;

			case "shop_healing":
				GiveKeyword("school");
				Msg("Dilys bickers a lot with Lassar at the School.<br/>They seem to be close friends,<br/>so I don't understand why they don't get along.");
				break;

			case "shop_inn":
				Msg("There are no other inns in this town.<br/>Why do you ask? You don't like this one?");
				break;

			case "shop_bank":
				Msg("Bebhinn is the clerk at the Bank.<br/>She loves interesting stories.<br/>But be careful.<br/>I learned the hard way that it's easy to become the subject of her gossip...");
				break;

			case "shop_smith":
				GiveKeyword("graveyard");
				Msg("Ferghus at the Blacksmith's Shop? I hope he stops working at night.<br/>His hammering always wakes me up!");
				Msg("The graveyard is up that hill. It's scary to be awake in the middle of the night!");
				break;

			case "skill_range":
				GiveKeyword("school");
				Msg("Ranald is the man to ask about such things.<br/>You'll have to head to the School.");
				Msg("You need to go up to the Square first,<br/>and then follow the path down from the Bank until you see it.");
				break;

			case "skill_instrument":
				Msg("Have you talked to Priestess Endelyon at the Church about that?<br/>She's the beautiful lady with clear skin<br/>and dark long hair that reaches down to her shoulders.<br/>*Sigh* The perfect heroine to an adventure novel, if you ask me.");
				break;

			case "skill_composing":
				Msg("Bebhinn must have told you about that skill.<br/>She really wants to learn it, but it's a little too difficult for her.");
				Msg("She's not really the musical type, but don't tell her I said that!<br/>She'd be very upset.");
				Msg("Anyway, have you talked to Priestess Endelyon at the Church?<br/>Go talk to her about it.");
				break;

			case "skill_tailoring":
				Msg("Oh, just in time!<br/>You're looking for something<br/>for tailoring and making clothes?<br/>Then finish the conversation,<br/>and press 'Shop'.");
				break;

			case "skill_magnum_shot":
				Msg("Hmm, I think you should ask Ranald or Trefor about those kinds of skills.");
				break;

			case "skill_counter_attack":
				Msg("Have you talked to Ranald?<br/>If you haven't, go to the School to find him.");
				break;

			case "skill_smash":
				Msg("Ranald knows a lot about the Smash skill.");
				Msg("Do I look like someone who smashes people?");
				break;

			case "skill_gathering":
				Msg("A tool is the most important thing for successful gathering.<br/>But the problem is,<br/>good tools are way too expensive.");
				Msg("So let's see. Hmm. Do you have a Gathering Knife?<br/>If you don't, go to Ferghus's Blacksmith's Shop. He sells them.<br/>They're cheap, and quite handy.");
				break;

			case "square":
				Msg("The Square is just up there. Walk up the slope to reach it.");
				break;

			case "pool":
				Msg("Are you looking for the reservoir?<br/>It's... Just go down the...<br/>No... Argh, it's hard to explain.");
				Msg("Ask someone in the Town Square.<br/>My head's starting to hurt. Sorry.");
				break;

			case "farmland":
				Msg("Why are you looking for farmland?<br/>You don't look like a farmer to me.");
				break;

			case "windmill":
				Msg("The Windmill? It's right there!<br/>Ferghus at the Blacksmith's Shop helped<br/>when we were making the Windmill.<br/>Now life is easier thanks to him.");
				Msg("Although Alissa seems to be the only one taking care of it now...");
				break;

			case "brook":
				Msg("Adelia Stream?<br/>It's right in front of you!<br/>Did you think it would be a big river or something? Ahahaha!");
				Msg("Sorry. I didn't mean to laugh at you.<br/>If it makes you feel better, many people make the same mistake.");
				break;

			case "shop_headman":
				Msg("The Chief's House? It's right up there.<br/>You already met him, I suppose?<br/>He has a lot of experience from the old days.");
				break;

			case "temple":
				GiveKeyword("shop_bank");
				Msg("To get to the Church,<br/>walk up the hill and follow the path down from the Bank.<br/>It's not far.");
				Msg("Can you say hello to Priestess Endelyon for me?<br/>If she's not in,<br/>ask Priest Meven to do it on my behalf.");
				break;

			case "school":
				GiveKeyword("temple");
				Msg("The School is just a few seconds' walk down from the Church.<br/>Lassar teaches magic, and Ranald teaches combat skills.<br/>Both teachers are very knowledgeable.");
				break;

			case "skill_windmill":
				Msg("The Windmill is right in front of...<br/>Oops. You're not talking about the Windmill over there<br/>but the skill, right? Heh.<br/>Hey, no need to make fun of me.<br/>If you keep teasing me, I won't tell you anything.");
				break;

			case "skill_campfire":
				Msg("No way! I can't tell you about that skill.<br/>We would run out of business!<br/>You don't know how hard it was to get permission to open our Inn!<br/>Hey, business is business.");
				break;

			case "shop_restaurant":
				GiveKeyword("shop_grocery");
				Msg("Are you looking for a place to have a nice meal?<br/>Many people buy food at the Grocery Store<br/>and come here to eat with others.<br/>Didn't Caitin at the Grocery Store<br/>tell you?");
				break;

			case "shop_armory":
				GiveKeyword("shop_smith");
				Msg("Are you looking for a Weapons Shop?<br/>Hmm... I can't remember...<br/>Oh, right! Head to the Blacksmith's Shop.<br/>Ferghus is good at making things.<br/>I'm sure he can make all sorts of weapons.");
				break;

			case "shop_cloth":
				GiveKeyword("shop_misc");
				Msg("Sorry.<br/>There are no Clothing Shops in this town.<br/>Malcolm at the General Shop sells some clothes,<br/>but I wouldn't call them fashionable.");
				break;

			case "shop_bookstore":
				Msg("Tir Chonaill doesn't have a bookstore.<br/>But if you need a book, go to the General Shop.<br/>It's beyond the hill over there.");
				break;

			case "shop_goverment_office":
				GiveKeyword("shop_headman");
				Msg("A town office in this small town?<br/>Yeah, right!<br/>The Chief's House is probably the closest thing, though.");
				Msg("It's right on the hill<br/>near the Square.");
				break;

			case "graveyard":
				Msg("Graveyard... Graveyard...<br/>Why, I know nothing about that frightening place.<br/>Why don't you ask someone else about it?");
				break;

			default:
				RndMsg(
					"Huh?",
					"I don't... I don't know.",
					"Can we change the subject?",
					"What are you talking about?",
					"I don't know much about that.",
					"I can't understand what you're asking."
				);
				ModifyRelation(0, 0, Random(2));
				break;
		}
	}
}

public class NoraShop : NpcShopScript
{
	public override void Setup()
	{
		Add("Tailoring", 60001);    // Tailoring Kit
		Add("Tailoring", 60015);    // Cheap Finishing Thread
		Add("Tailoring", 60015, 5); // Cheap Finishing Thread
		Add("Tailoring", 60031);    // Regular Silk Weaving Gloves
		Add("Tailoring", 60019);    // Cheap Fabric
		Add("Tailoring", 60019, 5); // Cheap Fabric
		Add("Tailoring", 60016);    // Common Finishing Thread
		Add("Tailoring", 60016, 5); // Common Finishing Thread
		Add("Tailoring", 60017);    // Fine Finishing Thread
		Add("Tailoring", 60055);    // Fine Silk Weaving Gloves
		Add("Tailoring", 60017, 5); // Fine Finishing Thread
		Add("Tailoring", 60046);    // Finest Silk Weaving Gloves
		Add("Tailoring", 60018);    // Finest Finishing Thread
		Add("Tailoring", 60057);    // Fine Fabric Weaving Gloves
		Add("Tailoring", 60056);    // Finest Fabric Weaving Gloves
		Add("Tailoring", 60020);    // Common Fabric
		Add("Tailoring", 60018);    // Finest Finishing Thread
		Add("Tailoring", 60020, 5); // Common Fabric

		Add("Sewing Patterns", 60000, "FORMID:4:101;", 3000);   // Sewing Pattern - Cores' Healer Dress (F)
		Add("Sewing Patterns", 60000, "FORMID:4:102;", 1700);   // Sewing Pattern - Magic School Uniform (M)
		Add("Sewing Patterns", 60000, "FORMID:4:103;", 11500);  // Sewing Pattern - Magic School Uniform (F)
		Add("Sewing Patterns", 60000, "FORMID:4:104;", 4600);   // Sewing Pattern - Cores' Healer Gloves
		Add("Sewing Patterns", 60000, "FORMID:4:105;", 11250);  // Sewing Pattern - Mongo's Hat
		Add("Sewing Patterns", 60000, "FORMID:4:106;", 400);    // Sewing Pattern - Popo's Skirt (F)
		Add("Sewing Patterns", 60000, "FORMID:4:107;", 1250);   // Sewing Pattern - Mongo's Traveler Suit (F)
		Add("Sewing Patterns", 60000, "FORMID:4:108;", 1250);   // Sewing Pattern - Mongo's Traveler Suit (M)
		Add("Sewing Patterns", 60000, "FORMID:4:109;", 25500);  // Sewing Pattern - Cloth Mail
		Add("Sewing Patterns", 60000, "FORMID:4:110;", 4000);   // Sewing Pattern - Cores' Healer Suit (M)
		Add("Sewing Patterns", 60044, "FORMID:4:180;", 73000);  // Sewing Pattern - Riding Suit
		Add("Sewing Patterns", 60044, "FORMID:4:185;", 121000); // Sewing Pattern - Spark Leather Armor

		Add("Gift", 52014); // Teddy Bear
		Add("Gift", 52015); // Pearl Necklace
		Add("Gift", 52016); // Bunny Doll
		Add("Gift", 52025); // Gift Ring

		AddQuest("Quest", 1001, 0);   // Collecting Quest [5 Small Gems]
		AddQuest("Quest", 1002, 1);   // Collecting Quest [5 Small Green Gems (1 Small Blue Gem reward)]
		AddQuest("Quest", 1003, 5);   // Collecting Quest [5 Small Blue Gems (1 Small Red Gem reward)]
		AddQuest("Quest", 1004, 25);  // Collecting Quest [5 Small Red Gems (1 Small Silver Gem reward)]
		AddQuest("Quest", 1005, 25);  // Collecting Quest [5 Small Red Gems (Gold reward)]
		AddQuest("Quest", 1006, 5);   // Collecting Quest [5 Small Blue Gems (Gold reward)]
		AddQuest("Quest", 1007, 1);   // Collecting Quest [5 Small Green Gems (Gold reward)]
		AddQuest("Quest", 1009, 100); // Collecting Quest [5 Small Silver Gems]

		Add("Cooking Appliances", 40042); // Cooking Knife
		Add("Cooking Appliances", 40043); // Rolling Pin
		Add("Cooking Appliances", 40044); // Ladle
		Add("Cooking Appliances", 46004); // Cooking Pot
		Add("Cooking Appliances", 46005); // Cooking Table

		Add("Skill Book", (c, o) => o.GetFavor(c) >= 50); // Allow access with >= 50 favor
		Add("Skill Book", 1082); // Resting Guide
	}
}
