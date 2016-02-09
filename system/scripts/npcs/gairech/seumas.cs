//--- Aura Script -----------------------------------------------------------
// Seumas
//--- Description -----------------------------------------------------------
// Miner
//---------------------------------------------------------------------------

public class SeumasScript : NpcScript
{
	public override void Load()
	{
		SetRace(10002);
		SetName("_seumas");
		SetBody(height: 1.00f);
		SetFace(skinColor: 24, eyeType: 7, eyeColor: 39, mouthType: 4);
		SetStand("human/anim/tool/Rhand_A/female_tool_Rhand_A02_mining");
		SetLocation(30, 38334, 48677, 238);

		EquipItem(Pocket.Face, 4950, 0x0000A2C7, 0x00F9A547, 0x00ECD5E8);
		EquipItem(Pocket.Hair, 4015, 0x002D2B13, 0x002D2B13, 0x002D2B13);
		EquipItem(Pocket.Armor, 15044, 0x00514940, 0x00CDAD7C, 0x00E6E6E6);
		EquipItem(Pocket.Glove, 16017, 0x00827148, 0x00A2003A, 0x000073A5);
		EquipItem(Pocket.Shoe, 17022, 0x00000000, 0x00100041, 0x00B60180);
		EquipItem(Pocket.Head, 18024, 0x00964D25, 0x00CAA859, 0x0001A958);
		EquipItem(Pocket.RightHand1, 40025, 0x00454545, 0x00745D2F, 0x00EEA140);

		AddGreeting(0, "You must be a traveler. (gasp, gasp) Have you seen the ruins?");
		AddGreeting(1, "I think I've seen you before.... (gasp, gasp) I forget your name...");
		AddGreeting(2, "How are you these days? (gasp, gasp) Enjoying life?");

		AddPhrase("Let's go! Let's do it!");
		AddPhrase("This is nothing!");
		AddPhrase("(gasp, gasp)");
		AddPhrase("Start all over again!");
		AddPhrase("I keep getting stuck on these rocks...");
		AddPhrase("Oh, no! I forgot.");
		AddPhrase("75... 76... 77...");
		AddPhrase("We'd better hurry.");
		AddPhrase("La la la!");
		AddPhrase("A little bit more... A little bit more and I'll rest...");
		AddPhrase("Yo-ho! Yo-ho!");
	}

	protected override async Task Talk()
	{
		SetBgm("NPC_Seumas.mp3");

		await Intro(
			"The man with dark-brown shoulder straps and a worn down shirt has a nice tan.",
			"He digs the ground with a Pickaxe in his callused hands, occasionally wiping off sweat.",
			"His strong chin is covered with a short dark beard, while his hair and clothes are covered in white dust.",
			"As you approach him, he turns his face and greets you with his smiling eyes."
		);

		Msg("Is there... something you want to say?", Button("Start a Conversation", "@talk"), Button("Shop", "@shop"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());
				if (Title == 11002)
					Msg("Gasp... Gasp... You've done enough<br/>to be called, Gasp... Gasp...a Guardian .<br/>Either way, Gasp... Gasp... Thanks a lot.");
				await Conversation();
				break;

			case "@shop":
				Msg("(gasp, gasp) Are you looking for a scroll?");
				OpenShop("SeumasShop");
				return;
		}

		End("Thank you, <npcname/>. I'll see you later!");
	}

	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				Msg(FavorExpression(), "My name is Seumas... *Pant*<br/>I'm the Director of this Dragon Ruin excavation site...*Pant*");
				Msg("Put simply, I'm like an on-site construction manager. *Pant*");
				ModifyRelation(Random(2), 0, Random(2));
				break;

			case "rumor":
				Msg(FavorExpression(), "Nearby...? Well...<br/>follow the hilly path heading south, and you'll find Bangor.<br/>It's where I live.");
				Msg("As you know, the opposite direction will lead you to Dunbarton.");
				Msg("Oh, boy... I can't talk and work at the same time...(gasp, gasp)<br/>Wait. Let me catch my breath.<br/>(gasp, gasp, gasp)");
				Msg("(gasp, gasp) Let's see, it's not really safe... (gasp, gasp)<br/>around here. (gasp, gasp)");
				Msg("Try not to use the shortcut<br/>on the left on the way to Bangor... (gasp, gasp)<br/>Especially somewhere like Reinhart... (gasp, gasp)<br/>It's full of Kobolds. (gasp, gasp) You need to be careful.");
				ModifyRelation(Random(2), 0, Random(2));
				break;

			case "shop_misc":
				Msg("A General Shop nearby? (gasp, gasp)<br/>The way to Bangor. (gasp, gasp)<br/>That's where you should go....*panting*<br/>Go to the village. Gasp... And go southwest from there.");
				Msg("Oh, and I say this from experience... (gasp, gasp)<br/>When you go to the General Shop in Bangor.... (gasp, gasp)<br/>It's best to<br/>know what you want before you go in.");
				Msg("I'm telling you from my extensive experience... (gasp, gasp)<br/>So it'll be good...(gasp, gasp)... to remember that.");
				break;

			case "shop_healing":
				Msg("If you're looking for the Healer's House... (gasp, gasp)<br/>The closest way... (gasp, gasp) is Dunbarton.<br/>There isn't one in Bangor. (gasp, gasp)");
				Msg("(gasp, gasp) Since there are no healers...<br/>(gasp, gasp) It's quite inconvenient.<br/>If someone opens a Healer's House... (gasp, gasp)<br/>They can make big money.");
				Msg("Hmm... Should I open one? (gasp, gasp)");
				break;

			case "shop_grocery":
				Msg("(gasp, gasp)Food?");
				Msg("If you keep looking for food (gasp, gasp)you're going to get fat...<br/>If you're really hungry... (gasp, gasp) You should talk to Jennifer.<br/>She sells food at the local Pub... (gasp, gasp)");
				Msg("Well, there are those who just drink... (gasp, gasp)<br/>With the money they should buy food....<br/>You're... (gasp, gasp)not one of them, are you?");
				break;

			case "shop_bank":
				Msg("A Bank in the middle of a mountain... (gasp, gasp)<br/>You... (gasp, gasp) are quite odd.<br/>Most people look for water. (gasp, gasp)<br/>The Bangor General Shop... (gasp, gasp) Nearby... (gasp, gasp) Go... (gasp, gasp) It'll be there.");
				Msg("Oh, boy. (gasp, gasp) Let me catch my breath.<br/>*Panting heavily*");
				Msg("Actually, there isn't a building for the Bank... (gasp, gasp)<br/>There... (gasp, gasp) is a guy named Bryce.");
				Msg("An arrogant fellow. (gasp, gasp) Quite frustrating to talk to as well....<br/>Don't get... (gasp, gasp) Stressed out talking to him...");
				break;

			case "shop_smith":
				Msg("The Blacksmith... (gasp, gasp)<br/>in Bangor... (gasp, gasp) It's not... (gasp, gasp)<br/>too difficult... (gasp, gasp) to find.");
				Msg("Look for an old guy named...(gasp)... Edern.<br/>He may be quite difficult to deal with... (gasp, gasp)<br/>but he's the best there is... So be extra respectful... (gasp, gasp)");
				break;

			case "skill_rest":
				Msg("...");
				Msg("You mean... You haven't learned it yet?");
				break;

			case "skill_range":
				Msg("A long range attack... (gasp, gasp)<br/>sounds like a skill for... (gasp, gasp)<br/>cowards. What do you think? ");
				break;

			case "skill_instrument":
				Msg("You shouldn't think of all instruments ... (gasp, gasp)<br/>has to... (gasp, gasp) look the same...<br/>...");
				Msg("My Pickaxe here... (gasp, gasp)<br/>Can carry... (gasp)... (gasp)...a rhythm and a tune....");
				Msg("My Pickaxe is my instrument.");
				break;

			case "skill_composing":
				Msg("Writing musical notes... (gasp, gasp)I guess it's a skill.<br/>But, it's more important to... (gasp, gasp) sort out the music that you hear inside your head.");
				Msg("True composition... (gasp, gasp) is not about having the skill to write notes.<br/>It's the ability to express... (gasp, gasp) and come up with beautiful melodies.");
				Msg("However... (gasp, gasp) It seems most people... (gasp, gasp)<br/>aren't so interested... (gasp, gasp) in that. (gasp, gasp)");
				break;

			case "skill_tailoring":
				Msg("Hahaha! (gasp, gasp) I am not good at that. (gasp, gasp)<br/>Well, my wife mends all my clothes. (gasp, gasp)<br/>Haha... Haha... (gasp, gasp)");
				Msg("Ha...haha ... It seems you're giving me the<br/>'So, you're that kind of man' look... (gasp, gasp)");
				break;

			case "skill_magnum_shot":
				Msg("(gasp, gasp) It must be the skill to shoot powerful arrows.");
				Msg("But did you know...? (gasp, gasp)<br/>True fighters... (gasp, gasp)<br/>don't need... (gasp, gasp)<br/>weapons like that. ");
				break;

			case "skill_counter_attack":
				Msg("Haha... so, you must've heard... (gasp, gasp) I used to be a great fighter in my prime...(gasp, gasp)<br/>I used the Counterattack skill... (gasp, gasp) but, the most important thing is...(gasp, gasp)<br/>understanding... (gasp, gasp) your opponent's attack. (gasp, gasp)");
				Msg("There is nothing more foolish... (gasp, gasp)<br/>than using the Counterattack skill... (gasp, gasp)<br/>to an opponent who has... (gasp, gasp) no intention of attacking. ");
				Msg("The Counterattack skill shines... (gasp, gasp)<br/>the more aggressive... (gasp, gasp)<br/>your opponent becomes. ");
				Msg("If you get this... (gasp, gasp)<br/>go try it against those Kobolds over there... (gasp, gasp)<br/>and knock em out, arite? (gasp, gasp)");
				break;

			case "skill_smash":
				Msg("Smash concentrates the force within your entire body... (gasp, gasp)<br/>to a single point... (gasp, gasp) and gives a powerful blow.<br/>Mere defense cannot stand... (gasp, gasp)<br/>against the Smash skill. (gasp, gasp)<br/>You probably know this.");
				Msg("However, you must beware. (gasp, gasp)<br/>If your opponent is using the Counterattack skill while you use the Smash skill,<br/>(gasp, gasp)you will receive a deadly counter blow...");
				break;

			case "skill_gathering":
				Msg("I have no idea... (gasp, gasp) what you can gather in a place like this.");
				Msg("In Bangor... (gasp, gasp)<br/>You'll need a Pickaxe... (gasp, gasp)<br/>Having one... (gasp, gasp) will be useful in many ways. (gasp, gasp) ");
				Msg("A Pickaxe is a very useful tool. (gasp, gasp)<br/>Those who never used one will never know. (gasp, gasp) ");
				break;

			case "square":
				Msg("This is the Dragon Ruins. (gasp, gasp) It's not the Town Square...");
				break;

			case "farmland":
				Msg("(gasp, gasp) It will be on your way to Dunbarton.<br/>(gasp, gasp) The land in Bangor is barren....<br/>(gasp, gasp) So it is difficult to farm.<br/>All the food... (gasp, gasp) is usually imported from other places.");
				break;

			case "pool":
				Msg("A reservoir? (gasp, gasp)<br/>If there is one nearby... (gasp, gasp) Would I be like this?<br/>(gasp, gasp) I  would be the first one to jump in there.");
				Msg("Ah... Even the thought of it... (gasp, gasp)");
				Msg("...It's making me sweat more. (gasp, gasp)");
				break;

			case "temple":
				Msg("Hmm, the Church...<br/>It would be best to head... (gasp, gasp)<br/>towards Dunbarton.  (gasp, gasp)");
				Msg("There are no churches in Bangor. (gasp, gasp)<br/>But we do have a priest here. (gasp, gasp)");
				Msg("To tell you the truth... (gasp, gasp)<br/>there used to be one here... (gasp, gasp)<br/>but something bad happened and it... (gasp, gasp) burned down.");
				Msg("I shouldn't be saying all this...(gasp)... (gasp)...<br/>It's none of my business...");
				break;

			case "school":
				Msg("Sounds like... (gasp)... (gasp)...you're mocking this town...");
				Msg("(gasp, gasp) as if you have some ill feelings... (gasp, gasp)<br/>towards Bangor.  (gasp, gasp)");
				Msg("It's still a nice town, you know... (gasp, gasp)");
				break;

			case "skill_windmill":
				Msg("When Kobolds... (gasp, gasp)<br/>attack in groups, the Windmill skill... (gasp, gasp)<br/>Is said to be... (gasp, gasp) quite useful.<br/>If you have a chance to learn it... (gasp, gasp) you should. ");
				Msg("Ah...don't look at me! (gasp, gasp)<br/>I don't know it...hah. *cough* ");
				break;

			case "shop_restaurant":
				Msg("A restaurant? (gasp, gasp)<br/>If you need food... (gasp, gasp)<br/>go to Bangor Pub. (gasp, gasp) ");
				Msg("It originally opened as a restaurant. (gasp, gasp)<br/>and, they still serve food... (gasp, gasp) ");
				break;

			case "shop_armory":
				Msg("If you wish to buy weapons... (gasp, gasp)<br/>How about... (gasp, gasp) visiting the Blacksmith?<br/>And... Can you stop asking questions... (gasp, gasp) Can't you see I'm working...?");
				break;

			case "shop_cloth":
				Msg("In Dunbarton... (gasp, gasp)<br/>There is a place. (gasp, gasp)<br/>I heard this from Sion. (gasp, gasp)");
				Msg("Wait. (gasp, gasp)<br/>Then where did Sion hear this from? (gasp, gasp)<br/>That kid... (gasp, gasp) has never stepped outside of Bangor...<br/>... (gasp, gasp)Not even once in his life. (gasp, gasp)");
				break;

			case "shop_bookstore":
				Msg("Well... (gasp, gasp) I never heard of a place like that in Bangor. (gasp, gasp)");
				break;

			case "graveyard":
				Msg("Some people... (gasp, gasp)<br/>Say this was... (gasp, gasp) some kind of graveyard. (gasp, gasp)<br/>But, I have a different idea. (gasp, gasp)");
				Msg("This place seems like... (gasp, gasp)<br/>It was a place for religious rituals... (gasp, gasp)<br/>For the legendary dragon, Cromm Cruaich. (gasp, gasp)");
				break;

			case "bow":
				Msg("To tell you the truth... (gasp, gasp) I'm not so interested in those kinds of weapons.");
				break;

			case "musicsheet":
				Msg("Music Score? (gasp, gasp) You want to write music?<br/>Everyone seems to be so serious... (gasp, gasp)<br/>about music.");
				Msg("You don't have to have musical scores... (gasp, gasp)<br/>to play music. (gasp, gasp)<br/>You can just sing... (gasp, gasp)<br/>That's good enough sometimes.");
				Msg("You don't need scores... (gasp, gasp)<br/>when you sing. (gasp, gasp)<br/>Isn't the expressing your heart... (gasp, gasp)<br/>through your voice a beautiful thing?");
				Msg("(gasp, gasp)");
				break;

			case "g1_way_to_tirnanog1":
				Msg("(Gasp)... Bryce? (gasp, gasp)  He's<br/>talking nonsense again, isn't he?");
				Msg("(gasp, gasp)  All the children are going crazy<br/>with curiosity, asking all sorts of questions. (gasp, gasp)");
				Msg("You know, (gasp, gasp)  Please tell him not to tell<br/>this nonsense to the children. (gasp, gasp)");
				break;

			case "g1_goddess_morrighan1":
				Msg("Goddess Morrighan? (gasp, gasp)<br/>Well (gasp)... I've got nothing to say...(gasp)...but I heard she looks after Humans...(gasp, gasp)<br/>so I guess she's a good Goddess...(gasp, gasp)");
				break;

			case "tir_na_nog":
				Msg("...You seem to be interested... (gasp, gasp)<br/>in such strange things. I advise you... (gasp, gasp)<br/>to focus... (gasp, gasp) on what lies in front of you.");
				Msg("If you spend too much time on ideas that are far out there... (gasp, gasp)<br/>you'll lose sense... (gasp, gasp)<br/>of reality. (gasp, gasp)");
				break;

			case "g3_DarkKnight":
				Msg("Gasp... Gasp... Aren't the Dark Knights...<br/>Gasp... Gasp... those who stood with Fomors,<br/>Gasp... Gasp... and annihilated humans?<br/>Is that who you're talking about?");
				Msg("I'm not sure... (gasp, gasp) whether it's true or not...<br/>Either way, there have been some rumors... (gasp, gasp)<br/>about the appearance of Dark Knights during the restoration of the Goddess statue... (gasp, gasp)<br/>So, you better be careful... Gasp...");
				break;

			case "nao_owlscroll":
				Msg("Ah, the Owl Scroll! (gasp, gasp)<br/>Careful how you catch it... (gasp, gasp)<br/>If you miss... (gasp, gasp) you could break your nose.");
				Msg("There's no Healer's House nearby... (gasp, gasp)<br/>Even if you have the First Aid skill... (gasp, gasp) it's better just to be careful.");
				break;

			default:
				RndFavorMsg(
					"Hmm... Do you expect someone like me to know something like that? (gasp, gasp)<br/>You've overestimated me. Hah...",
					"I really have no clue. (gasp, gasp)<br/>I don't know what to tell you...",
					"Hmm... I'm sorry to tell you this... (gasp, gasp)<br/>But I don't really know anything about that.",
					"Hmm... I really don't know. (gasp, gasp)<br/>If you find out, could you tell me, too?",
					"Hey, don't look at me like that. (gasp, gasp)<br/>I'm telling you the truth... I really don't know.",
					"Hmm... I don't know anything about that either... (gasp, gasp)"
				);
				ModifyRelation(0, 0, Random(2));
				break;
		}
	}
}

public class SeumasShop : NpcShopScript
{
	public override void Setup()
	{
		AddQuest("Quest", 71006, 30); // Collect the Skeleton's Fomor Scrolls
		AddQuest("Quest", 71012, 30); // Collect the Skeleton Wolf's Fomor Scrolls
		AddQuest("Quest", 71013, 30); // Collect the Brown Dire Wolf's Fomor Scrolls
		AddQuest("Quest", 71015, 30); // Collect the Black Dire Wolf's Fomor Scrolls
		AddQuest("Quest", 71016, 30); // Collect the White Dire Wolf's Fomor Scrolls
		AddQuest("Quest", 71023, 30); // Collect the Gray Fox's Fomor Scrolls
		AddQuest("Quest", 71025, 30); // Collect the Brown Bear's Fomor Scrolls
		AddQuest("Quest", 71026, 30); // Collect the Red Bear's Fomor Scrolls
		AddQuest("Quest", 71028, 30); // Collect the Brown Grizzly Bear's Fomor Scrolls
		AddQuest("Quest", 71029, 30); // Collect the Red Grizzly Bear's Fomor Scrolls
		AddQuest("Quest", 71030, 30); // Collect the Black Grizzly Bear's Fomor Scrolls
		AddQuest("Quest", 71040, 30); // Collect the Kobold's Fomor Scrolls
		AddQuest("Quest", 71043, 30); // Collect the Raccoon's Fomor Scrolls
		AddQuest("Quest", 71045, 30); // Collect the Wisp's Fomor Scrolls
		AddQuest("Quest", 71047, 30); // Collect the Skeleton Soldier's Fomor Scrolls
		AddQuest("Quest", 71052, 30); // Collect the Jackal's Fomor Scrolls
		AddQuest("Quest", 71057, 30); // Collect the Burgundy Grizzly Bear's Fomor Scrolls
		AddQuest("Quest", 71059, 30); // Collect the Blue Dire Wolf's Fomor Scrolls
		AddQuest("Quest", 71060, 30); // Collect the Burgundy Dire Wolf's Fomor Scrolls
		AddQuest("Quest", 71061, 30); // Collect the Wood Jackal's Fomor Scrolls
		AddQuest("Quest", 71062, 30); // Collect the Wild Boar's Fomor Scrolls

		AddQuest("Party Quest", 100026, 30); // [PQ] Hunt Down the Skeletons (30)
		AddQuest("Party Quest", 100035, 20); // [PQ] Hunt Down the Brown Dire Wolves (30)
		AddQuest("Party Quest", 100045, 30); // [PQ] Hunt Down the Skeleton Wolves (30)
		AddQuest("Party Quest", 100052, 30); // [PQ] Hunt Down the Dire Wolves (40)
		AddQuest("Party Quest", 100053, 50); // [PQ] Hunt Down the Dire Wolves and the Grizzly Bears (40)
		AddQuest("Party Quest", 100062, 20); // [PQ] Hunt Down the Werewolves (10)
		AddQuest("Party Quest", 100063, 10); // [PQ] Hunt Down the Wood Jackals (30)
		AddQuest("Party Quest", 100064, 10); // [PQ] Hunt Down the Blue Dire Wolves (30)
		AddQuest("Party Quest", 100065, 10); // [PQ] Hunt Down the Burgundy Dire Wolves (30)
	}
}