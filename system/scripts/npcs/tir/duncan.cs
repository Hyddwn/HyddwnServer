//--- Aura Script -----------------------------------------------------------
// Duncan
//--- Description -----------------------------------------------------------
// Chief of Tir Chonaill (outside Chief's House)
//---------------------------------------------------------------------------

public class DuncanBaseScript : NpcScript
{
	public override void Load()
	{
		SetRace(10002);
		SetName("_duncan");
		SetBody(height: 1.3f);
		SetFace(skinColor: 20, eyeType: 17);
		SetStand("human/male/anim/male_natural_stand_npc_duncan_new", "male_natural_stand_npc_Duncan_talk");
		SetLocation(1, 15409, 38310, 122);
		SetGiftWeights(beauty: 0, individuality: 0, luxury: 0, toughness: 1, utility: 2, rarity: 0, meaning: 2, adult: 1, maniac: -1, anime: -1, sexy: 0);

		EquipItem(Pocket.Face, 4950, 0x93005C);
		EquipItem(Pocket.Hair, 4083, 0xBAAD9A);
		EquipItem(Pocket.Armor, 15004, 0x5E3E48, 0xD4975C, 0x3D3645);
		EquipItem(Pocket.Shoe, 17021, 0xCBBBAD);

		AddGreeting(0, "Welcome to Tir Chonaill.");
		AddGreeting(1, "What did you say your name was again...?<br/>Anyway, welcome.");
		AddGreeting(2, "<username/>, I could recognize you from afar.");
		AddGreeting(6, "I was just thinking... <username/> should be visiting right about now.");
		AddGreeting(7, "Hoho, I will definitely remember your face, <username/>!");

		AddPhrase("Ah, that bird in the tree is still sleeping.");
		AddPhrase("Ah, who knows how many days are left in these old bones?");
		AddPhrase("Everything appears to be fine, but something feels off.");
		AddPhrase("Hmm....");
		AddPhrase("It's quite warm today.");
		AddPhrase("Sometimes, my memories sneak up on me and steal my breath away.");
		AddPhrase("That tree has been there for quite a long time, now that I think about it.");
		AddPhrase("The graveyard has been left unattended far too long.");
		AddPhrase("Watch your language.");
	}

	protected override async Task Talk()
	{
		SetBgm("NPC_Duncan.mp3");

		await Intro(
			"An elderly man gazes softly at the world around him with a calm air of confidence.",
			"Although his face appears weather-beaten, and his hair and beard are gray, his large beaming eyes make him look youthful somehow.",
			"As he speaks, his voice resonates with a kind of gentle authority."
		);

		Msg("Please let me know if you need anything.", Button("Start Conversation", "@talk"), Button("Shop", "@shop"), Button("Retrive Lost Items", "@lostandfound"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());
				if (Title == 11002)
				{
					Msg("Oh. <username/>! You finally did it...<br/>I can't believe you became the Knight of Light and saved Erinn...<br/>Nao would be so proud.");
					Msg("I'm starting to understand Goddess Morrighan and Nao's will<br/>for sending people like you to this world.");
				}
				await Conversation();
				break;

			case "@shop":
				Msg("Choose a quest you would like to do.");
				OpenShop("DuncanShop");
				return;

			case "@lostandfound":
				Msg("If you are knocked unconcious in a dungeon or field, any item you've dropped will be lost unless you get resurrected right at the spot.<br/>Lost items can usually be recovered from a Town Office or a Lost-and-Found.");
				Msg("Unfortunatly, Tir Chonaill does not have a Town Office, so I run the Lost-and-Found myself.<br/>The lost items are recovered with magic,<br/>so unless you've dropped them on purpose, you can recover those items with their blessings intact.<br/>You will, however, need to pay a fee.");
				Msg("As you can see, I have limited space in my home. So I can only keep 20 items for you.<br/>If there are more than 20 lost items, I'll have to throw out the oldest items to make room.<br/>I strongly suggest you retrieve any lost items you don't want to lose as soon as possible.");
				break;
		}

		End();
	}

	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				if (Favor > 40)
				{
					Msg("See that bird on the tree over there? When I was young, he used to help me on the battlefield.<br/>Now he's as old as I am and sleeps all the time.<br/>Perhaps he has closed his heart in disappointment at my present appearance, so old and changed...");
				}
				else
				{
					GiveKeyword("shop_headman");
					Msg("Once again, welcome to Tir Chonaill.");
				}

				ModifyRelation(Random(2), 0, Random(2));
				break;

			case "rumor":
				if (Favor > 40)
					Msg("The weather here changes unpredictably because Tir Chonaill is located high up in the mountains.<br/>There are instances where bridges collapse and roads are destroyed after a heavy rainfall,<br/>and people lose all contact with the outside world.<br/>Despite that, I think you've done quite well here.");
				else
					Msg("I heard a rumor that this is just a copy of the world of Erin. Trippy, huh?");


				ModifyRelation(Random(2), 0, Random(2));

				/* More data - unsure where it goes

				// This message came up first
				Msg("Talk to the good people in Tir Chonaill as much as you can, and pay close attention to what they say.<br/>Once you become friends with them, they will help you in many ways.<br/>Why don't you start off by visiting the buildings around the Square?");

				// Then this message always came up afterwards, except on field boss info
				Msg("<face name='normal'/>Have you heard of field bosses?<br/>They are very powerful monsters that appear randomly in places outside dungeons, like open fields.<br/>Field bosses are either a Fomor or an animal affected by the forces of evil and transformed into a huge, savage creature.");
				Msg("Field bosses usually show up with several monsters with them,<br/>so they pose a big threat to travelers.<br/>If you want to face a field boss, the people in town will tell you about them<br/>if you ask about nearby rumors a few times.");
				Msg("<title name='NONE'/>(The conversation drew a lot of interest.)"); 

				// Message from Field Boss Spawns
				Msg("<face name='normal'/>I have something to tell you.<br/>Can you feel the evil presence of Gigantic White Wolf spreading around Southern Plains of Tir Chonaill?<br/>I think something bad will happen in around 3Days late Afternoon...");
				Msg("<title name='NONE'/>(That was a great conversation!)"); */
				break;

			case "about_skill":
				// Duncan used to check for race, but stopped some time after G13, so a feature check should go here
				if (HasSkill(SkillId.RangedAttack) && !HasSkill(SkillId.MagnumShot))
				{
					GiveKeyword("skill_magnum_shot");
					Msg("You seem much more comfortable conversing with people using the 'Skills' keyword now.<br/>Wait. You can shoot an arrow? Congratulations!<br/>You've only been here for a short time, yet you pick up things so fast.");
					Msg("Now, have you heard about Magnum Shot?<br/>You see, a bow is great for attacking enemies from a distance,<br/>yet it's frustrating when you miss a target.<br/>Plus, the damage from a bow isn't as strong as a melee attack.");
					Msg("Since you were so diligent, I will teach you<br/>Magnum Shot, which I learned from Ranald.");
					Msg("The Magnum Shot skill helps you to shoot a powerful arrow<br/>with the power you have concentrated in your bow.<br/>Go on and work on your training...");
					// Duncan started giving the skill some time after G13, so a feature check should go here
					// On official, he adds the skill as rNovice, then adds on 100 skill training
					GiveSkill(SkillId.MagnumShot, SkillRank.RF);
					break;
				}
				else
				{
					if (HasSkill(SkillId.Rest))
					{
						Msg("You know about the Combat Mastery skill?<br/>It's one of the basic skills needed to protect yourself in combat.<br/>It may look simple, but never underestimate its efficiency.<br/>Continue training the skill diligently and you will soon reap the rewards. That's a promise.");
					}
					else
					{
						Msg("Whatever you do, skills will be an essential part of your life.<br/>There are various ways to learn skills,<br/>but the best way is to talk to people in town using the 'Skills' keyword.");
						Msg("First, go and meet the people of Tir Chonaill, and use this keyword to ask them questions.<br/>They will teach you everything about skills they know,<br/>but that doesn't mean they will tell you everything YOU want to know.<br/>The town residents aren't experts.");
						Msg("So my advice is to not get frustrated.<br/>Even if you don't learn a skill right away, if you follow their guidance,<br/>you'll eventually find someone who can teach you the skill.<br/>If I were you, I would listen carefully to what people say.");
					}
				}
				break;

			case "about_arbeit":
				Msg("Are you interested in a part-time job?<br/>It's great to see young people eager to work!<br/>To get one, talk to the people in town with the 'Part-Time Jobs' keyword.<br/>If you go at the right time, you'll be offered a job.");
				Msg("If you do a good job, you will be duly rewarded.<br/>Just make sure to return to the person who gave you the job and report the results before the deadline.<br/>If you miss the deadline, you will not be rewarded regardless of how hard you worked.");
				Msg("Part-time jobs aren't available 24 hours a day.<br/>You have to get there at the right time.");
				Msg("The sign-up period usually begins between 7:00 am and 9:00 am.<br/>Since there are only a limited number of jobs available,<br/>others may take them all if you're too late.<br/>Also, you can do only one part-time job per day.");
				Msg("It looks like Nora and Caitin could use your help,<br/>so head to the Grocery Store or the Inn and talk to them.<br/>Start the conversation with them with the keyword 'Part-Time Jobs' and make sure it's between 7 and 9 am.<br/>Good luck!");

				// Msg("I don't have any jobs for you, but you can get a part time job in town.");
				break;

			case "about_study":
				GiveKeyword("school");
				Msg("Ah, you'll need to go to the School for that.<br/>Talk to one of the teachers with that keyword.<br/>That should get you started with classes.");
				Msg("Find the guidepost near the Bank down the street.<br/>Once you do, it should be easy to locate the School.<br/>Keep in mind that the guideposts around town are there to help you out.");
				break;

			case "shop_misc":
				Msg("If you look down at the Square, you can see a building with a dark roof.<br/>That's the General Shop,<br/>where Malcolm sells homemade products.<br/>The quality of his products are quite good.");
				break;

			case "shop_grocery":
				Msg("Caitin from the Grocery Store is a diligent girl.<br/>Growing all those vegetables, cooking all that food, and running the business all by herself,<br/>it's certainly not as easy as it sounds.<br/>To find her, you can check the Minimap at the upper right corner of your screen.<br/>Press <hotkey name='MiniMapView'/> to toggle the Minimap.");
				break;

			case "shop_healing":
				GiveKeyword("graveyard");
				Msg("The Healer's House is right past here.<br/>Some people get freaked out because it's so close to the graveyard. Hahaha!<br/>When you visit, please say hello to Dilys for me.");
				break;

			case "shop_inn":
				Msg("Are you tired?<br/>The Inn is near the town entrance, so just go further down.<br/>Nora will be at the door to greet you.<br/>If you have time, go and talk to her.");
				break;


			case "shop_bank":
				Msg("It's been a while since the Erskin Bank first opened its doors...<br/>It's that big building with a tiled roof below in the Square.<br/>There, you'll find Bebhinn, the teller.<br/>She knows a lot of gossip, so talk to her if you're curious.");
				break;

			case "shop_smith":
				Msg("Anyone without a weapon should head there immediately.<br/>The shop does not carry a wide variety of weapons,<br/>since they're short-handed and can only make few weapons at a time.<br/>The quality, however, is good, and they last a while...<br/>Best of all, they are quite affordable...");
				break;

			case "skill_range":
				Msg("Hmm. I could tell you some things about long-ranged attacks,<br/>but I think it's better for you to ask Ranald.<br/>Don't take it personally! I just think you should learn from an expert.");
				Msg("Long-ranged attacks consist of attacking a monster at a distance with a bow or a rock.<br/>But you need to spend time training, as long-ranged attacks and melee attacks use different muscles.");
				break;

			case "skill_instrument":
				GiveKeyword("lute");
				Msg("The shepherd boy, Deian, told you that?<br/>I know Deian wants to learn how to play the lute.<br/>But I am a little concerned he may neglect his sheep<br/>if he gets carried away playing his lute...");
				Msg("I expect Deian told you to go to Priestess Endelyon to get the Instrument Playing skill.<br/>Priestess Endelyon is at the Church. Go on and meet her.");
				break;

			case "skill_composing":
				Msg("The Composing skill, you said? Bebhinn mentioned that, didn't she?<br/>Bebhinn loves to talk to travelers about that skill.<br/>I can't figure out why, though...");
				Msg("Listen carefully, <username/>.<br/>Whether they realize it or not, people often try to influence other people's actions.<br/>In other words, it is dangerous to believe everything someone says.<br/>Be careful about what you believe, and you'll be able to make wiser decisions down the line.");
				break;

			case "skill_tailoring":
				Msg("The Tailoring skill? I'm sure someone's told you to go to Caitin for that?<br/>No one in this town is as meticulous as she is.<br/>She's just down the hill, at the Grocery Store, so go talk to her!");
				break;

			case "skill_magnum_shot":
				Msg("Hmmm. I told you to ask Trefor...<br/>Go farther up the hill past the Healer's House, and you will see him. He works at the border up north.");
				break;

			case "skill_counter_attack":
				Msg("Haha, I am just the Chief of a small town. I don't know the details of that...<br/>I'm too old to give demonstrations, don't you think?<br/>Why don't you go ask Trefor to teach you?<br/>Go farther up the hill past the Healer's House, and you will see him.");
				break;

			case "skill_smash":
				Msg("Haha. I'd be happy to teach you,<br/>but I'm too old to demonstrate the Smash skill...<br/>Head to the School and talk to Ranald about it.");
				break;

			case "skill_gathering":
				Msg("Gathering... I don't have much to say about that, but remember this.<br/>What we gather is a gift from nature.<br/>Do not grumble if you don't get what you want. Be grateful instead.");
				break;

			case "square":
				Msg("The Square is perhaps the liveliest area in town.<br/>Many people use it as a meeting place.");
				break;

			case "pool":
				GiveKeyword("shop_grocery");
				Msg("To get to the reservoir, go down the hill and walk around the Grocery Store.");
				break;

			case "farmland":
				Msg("Head south, and you'll find farmland spread across the fields.<br/>I'm concerned, though. Travelers walk through the fields as they please,<br/>and end up damaging our crops.");
				Msg("Now I don't think they have bad intentions...<br/>But it's so hard to maintain viable farmland<br/>in such an inhospitable location at the foot of a mountain.<br/>And that farmland is essential to the well-being of my town.");
				Msg("Hey, I'm not pointing any fingers, I'm just saying you should be careful, okay?<br/>Knowledge is power, right? And now you know...");
				break;

			case "windmill":
				GiveKeyword("shop_smith");
				Msg("The Windmill is near the Blacksmith's Shop.<br/>It pulls water up to the reservoir and grinds grain. Watch your fingers, though.<br/>If you go there, say hello to Alissa for me.<br/>For some reason, that girls loves to play with my beard...");
				break;

			case "brook":
				Msg("Ah, the Adelia Stream. That's an old story...<br/>Adelia is the name of a priestess who lived in Tir Chonaill.<br/>When we were still fighting the Fomors, this town was often attacked.<br/>In fact, the town graveyard was made for the victims of these raids...");
				Msg("Anyway, Priestess Adelia sacrificed herself to stop their raids.<br/>Countless lives were saved. In memory and appreciation of her bravery,<br/>we named the stream after her.");
				break;

			case "shop_headman":
				Msg("That's right. This is my house.<br/>A strange cat appeared one day and made himself at home.<br/>I think it might even have some special powers.<br/>If you're curious, try talking to it.");
				break;

			case "temple":
				GiveKeyword("school");
				Msg("Have you talked to Priest Meven at the Church?<br/>Walk down near the School and you should easily find the Church.");
				break;

			case "school":
				GiveKeyword("temple");
				Msg("The School? It's near the Church.<br/>Walk down the path by the Bank and head past the Church to get to the School.<br/>Lassar and Ranald, the teachers there, should be of great help for you.<br/>They'll be more than willing to answer questions.");
				break;

			case "skill_windmill":
				Msg("Oh. You know about that? Ranald probably told you.<br/>It is indeed a difficult skill. I hope you learn it well.");
				break;

			case "skill_campfire":
				Msg("As a matter of fact, I have a book about campfires at home.<br/>Or rather, had. The shepherd boy Deian snuck it out some time ago.<br/>Haha. Why don't you go and talk to him?<br/>But don't mention that I know he took my book.");
				break;

			case "shop_restaurant":
				Msg("You're looking for a restaurant? You must be starving...<br/>Then head to the Grocery Store.<br/>Caitin's an amazing cook, and her food should fill you right up.");
				break;

			case "shop_armory":
				GiveKeyword("shop_smith");
				Msg("Are you looking for a better weapon?<br/>If so, head south and you'll find the Blacksmith's Shop where Ferghus works.<br/>Tir Chonaill is known for its fearsome warriors, but interestingly enough, there isn't a single Weapon Shop in town.");
				Msg("Why? Because only beginners and cowards entrust their lives to weapons!<br/>If you want to become a true warrior, never mistake your weapon's power for your own.");
				break;

			case "shop_cloth":
				Msg("You don't like the clothes you're  wearing?<br/>But clothes are quite expensive...<br/>Today's young people sure do care a lot about fashion...");
				Msg("But there are no Clothing Shops in town.<br/>If you want to buy a new outfit, go to the General Shop.<br/>Malcolm will probably have some items available...<br/>Haha! And don't worry, Nora does all the essentail tailoring.");
				break;

			case "shop_bookstore":
				GiveKeyword("shop_misc");
				Msg("Unfortunately, Tir Chonaill does not have a bookstore.<br/>In fact, when it comes to passing down the history and heritage of our town,<br/>we believe that books are a supplementary form of record-keeping.");
				Msg("If you are determined to buy a book, go see Malcolm.<br/>I'm sure there are some available at the General Shop.<br/>Piaras at the Inn should have a couple, too...<br/>In fact, all the town residents probably own a book or two, so talk to them.<br/>I hope you gain much wisdom from the time you spend reading books.");
				break;

			case "shop_goverment_office":
				Msg("The Town Office...<br/>I don't know if you're interested in the Town Office building itself,<br/>or in the services a Town Office provides.");
				Msg("This is Tir Chonaill in the Ulaid province.<br/>We are not under the control of the Aliech Kingdom, which is located far to the south.<br/>Most Town Offices are governed by the Aliech Kingdom.<br/>The nearest one is in Dunbarton, which is located just south of here.<br/>If you're just looking to recover lost items, I can help you with that.");
				break;

			case "graveyard":
				Msg("The graveyard is holy ground. Much of Tir Chonaill's honor and tradition is buried there...<br/>Do not be disrespectful.");
				break;

			case "skill_fishing":
				Msg("You have a good hobby.<br/>Make sure to buy plenty of Bait Tins.<br/>Otherwise you might end up regretting it later.<br/>Hahaha.");
				break;

			default:
				RndFavorMsg(
					"Hm?",
					"I have no idea...",
					"I don't really know about that...",
					"I don't know anything about that...",
					"Hmm, I wonder who might know about that...",
					"I think it'd be better for you to ask someone else."
				);

				ModifyRelation(0, 0, Random(2));
				break;
		}
	}

	protected override async Task Gift(Item item, GiftReaction reaction)
	{
		switch (reaction)
		{
			case GiftReaction.Love:
				Msg("Oh! How did you know I like this?<br/>Thank you very much.");
				break;

			case GiftReaction.Dislike:
				RndMsg(
					"Hmm. Not exactly to my taste...",
					"Hmm. I'll keep it safe for someone who may need it."
				);
				break;

			default:
				RndMsg(
					"Is that for me?",
					"You didn't need to do this..."
				);
				break;
		}
	}
}

public class DuncanShop : NpcShopScript
{
	public override void Setup()
	{
		// Quest
		Add("Quest", 70083, "QSTTIP:s:N_Collect the Gray Wolf's Fomor Scrolls|D_The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Gray Wolf Fomor Scrolls].|A_|R_* 4100G|T_0;", 30);
		Add("Quest", 70084, "QSTTIP:s:N_Collect the Black Wolf's Fomor Scrolls|D_The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Black Wolf Fomor Scrolls].|A_|R_* 5050G|T_0;", 30);
		Add("Quest", 70085, "QSTTIP:s:N_Collect the White Wolf's Fomor Scrolls|D_The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 White Wolf Fomor Scrolls].|A_|R_* 5900G|T_0;", 30);
		Add("Quest", 70087, "QSTTIP:s:N_Collect the Brown Dire Wolf's Fomor Scrolls|D_The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Brown Dire Wolf Fomor Scrolls].|A_|R_* 5000G|T_0;", 30);
		Add("Quest", 70091, "QSTTIP:s:N_Collect the White Spider's Fomor Scrolls|D_The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 White Spider Fomor Scrolls].|A_|R_* 520G|T_0;", 30);
		Add("Quest", 70092, "QSTTIP:s:N_Collect the Black Spider's Fomor Scrolls|D_The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Black Spider Fomor Scrolls].|A_|R_* 1900G|T_0;", 30);
		Add("Quest", 70093, "QSTTIP:s:N_Collect the Red Spider's Fomor Scrolls|D_The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Red Spider Fomor Scrolls].|A_|R_* 3000G|T_0;", 30);
		Add("Quest", 70095, "QSTTIP:s:N_Collect the Brown Fox's Fomor Scrolls|D_The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Brown Fox Fomor Scrolls].|A_|R_* 760G|T_0;", 30);
		Add("Quest", 70096, "QSTTIP:s:N_Collect the Red Fox's Fomor Scrolls|D_The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Red Fox Fomor Scrolls].|A_|R_* 850G|T_0;", 30);
		Add("Quest", 70097, "QSTTIP:s:N_Collect the Gray Fox's Fomor Scrolls|D_The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Gray Fox Fomor Scrolls].|A_|R_* 1050G|T_0;", 30);
		Add("Quest", 70105, "QSTTIP:s:N_Collect the Bat's Fomor Scrolls|D_The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Bat Fomor Scrolls].|A_|R_* 900G|T_0;", 30);
		Add("Quest", 70106, "QSTTIP:s:N_Collect the Mimic's Fomor Scrolls|D_We'd like to study some Fomor Scrolls from Mimics because they're very useful for our research on Fomors. You will be rewarded for collecting [10 Mimic Fomor Scrolls].|A_|R_* 6000G|T_0;", 30);
		Add("Quest", 70109, "QSTTIP:s:N_Collect the Gray Town Rat's Fomor Scrolls|D_The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Gray Town Rat Fomor Scrolls].|A_|R_* 1740G|T_0;", 30);
		Add("Quest", 70111, "QSTTIP:s:N_Collect the Goblin's Fomor Scrolls|D_We'd like to study some Fomor Scrolls from Goblins because they're very useful for our research on Fomors. You will be rewarded for collecting [10 Goblin Fomor Scrolls].|A_|R_* 3100G|T_0;", 30);
		Add("Quest", 70112, "QSTTIP:s:N_Poison Goblin's Fomor Scrolls|D_We'd like to study some Fomor Scrolls from Poison Goblins because they're very useful for our research on Fomors. You will be rewarded for collecting [10 Poison Goblin Fomor Scrolls].|A_|R_* 3240G|T_0;", 30);
		Add("Quest", 70113, "QSTTIP:s:N_Collect the Gold Goblin's Fomor Scrolls|D_We'd like to study some Fomor Scrolls from Gold Goblins because they're very useful for our research on Fomors. You will be rewarded for collecting [10 Gold Goblin Fomor Scrolls].|A_|R_* 3500G|T_0;", 30);
		Add("Quest", 70119, "QSTTIP:s:N_Collect the Wisp's Fomor Scrolls|D_The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Wisp Fomor Scrolls].|A_|R_* 4800G|T_0;", 30);
		Add("Quest", 70123, "QSTTIP:s:N_Collect the Snake's Fomor Scrolls|D_The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Snake Fomor Scrolls].|A_|R_* 2200G|T_0;", 30);
		Add("Quest", 70124, "QSTTIP:s:N_Collect the Coyote's Fomor Scrolls|D_The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Coyote Fomor Scrolls].|A_|R_* 3700G|T_0;", 30);
		Add("Quest", 70129, "QSTTIP:s:N_Collect the Blue Grizzly Bear's Fomor Scrolls|D_The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Blue Grizzly Bear Fomor Scrolls].|A_|R_* 12200G|T_0;", 30);
		Add("Quest", 70131, "QSTTIP:s:N_Collect the Burgundy Spider Fomor Scrolls|D_The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Burgundy Spider Fomor Scrolls].|A_|R_* 2000G|T_0;", 30);
		Add("Quest", 70140, "QSTTIP:s:N_Collect the Rat Man's Fomor Scrolls|D_The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Rat Man Fomor Scrolls].|A_|R_* 3120G|T_0;", 30);
		Add("Quest", 70141, "QSTTIP:s:N_Collect the Blue Wolf's Fomor Scrolls|D_The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Blue Wolf Fomor Scrolls].|A_|R_* 2370G|T_0;", 30);
		Add("Quest", 70142, "QSTTIP:s:N_Collect the Dark Blue Spider's Fomor Scrolls|D_The evil Fomors are controlling various creatures in the neighborhood. Retrieve Fomor Scrolls from these animals in order to free them from the reign of these evil spirits. You will be rewarded for collecting [10 Dark Blue Spider Fomor Scrolls].|A_|R_* 1930G|T_0;", 30);

		// Party Quest
		Add("Party Quest", 70025, "QSTTIP:s:N_[PQ] Hunt Down the Goblins|D_Goblins are small ugly monsters with dull green skin and red eyes. These small bogeyman-like creatures are from the lower class of Fomors. Please do me a favor and [hunt 30 Goblins].|A_|R_* 426 Experience Point,* 957G|T_72000000;", 10);
		Add("Party Quest", 70025, "QSTTIP:s:N_[PQ] Hunt Down the Poison Goblins|D_Poison Goblins are different from ordinary goblins because of their poisonous purple skin. This threatening creature is from the lower class of Fomors. Please do me a favor and [hunt 30 Poison Goblins].|A_|R_* 447 Experience Point,* 1005G|T_72000000;", 10);
		Add("Party Quest", 70025, "QSTTIP:s:N_[PQ] Hunt Down the Laghodessas|D_Laghodessas under the control of an evil power are attacking travelers. Please do us a favor and [hunt 30 Laghodessas].|A_|R_* 1200 Experience Point,* 2631G|T_72000000;", 20);
		Add("Party Quest", 70025, "QSTTIP:s:N_[PQ] Hunt Down the Rat Men|D_Rat Men under the control of an evil power are attacking travelers. Please [hunt 10 Rat Men].|A_|R_* 1500 Experience Point,* 1362G|T_72000000;", 20);
		Add("Party Quest", 70025, "QSTTIP:s:N_[PQ] Defeat the Golem|D_Please offer [Ciar Basic Fomor Pass] on the altar of Ciar Dungeon, and defeat a [Golem] that can be found at the deepest part of the dungeon. The reward will be given to you outside the dungeon after completing the quest.|A_|R_* 3500 Experience Point,* 10000G|T_108000000;", 500);
		Add("Party Quest", 70025, "QSTTIP:s:N_[PQ] Defeat the Golem|D_Please offer [Ciar Intermediate Fomor Pass for 2] on the altar of Ciar Dungeon, and defeat a [Golem] that can be found at the deepest part of the dungeon. The reward will be given to you outside the dungeon after completing the quest.|A_|R_* 6000 Experience Point,* 15000G|T_108000000;", 500);
		Add("Party Quest", 70025, "QSTTIP:s:N_[PQ] Defeat the Golem|D_Please offer [Ciar Intermediate Fomor Pass for 4] on the altar of Ciar Dungeon, and defeat a [Golem] that can be found at the deepest part of the dungeon. The reward will be given to you outside the dungeon after completing the quest.|A_|R_* 11700 Experience Point,* 15000G|T_108000000;", 500);
		Add("Party Quest", 70025, "QSTTIP:s:N_[PQ] Defeat the Golem|D_Please offer [Ciar Adv. Fomor Pass for 2] on the altar of Ciar Dungeon, and defeat a [Golem] that can be found at the deepest part of the dungeon. The reward will be given to you outside the dungeon after completing the quest.|A_|R_* 13500 Experience Point,* 20000G|T_108000000;", 500);
		Add("Party Quest", 70025, "QSTTIP:s:N_[PQ] Defeat the Golem|D_Please offer [Ciar Adv. Fomor Pass for 3] on the altar of Ciar Dungeon, and defeat a [Golem] that can be found at the deepest part of the dungeon. The reward will be given to you outside the dungeon after completing the quest.|A_|R_* 18200 Experience Point,* 20000G|T_108000000;", 500);
		Add("Party Quest", 70025, "QSTTIP:s:N_[PQ] Defeat the Golem|D_Please offer [Ciar Adv. Fomor Pass] on the altar of Ciar Dungeon, and defeat a [Golem] that can be found at the deepest part of the dungeon. The reward will be given to you outside the dungeon after completing the quest.|A_|R_* 19800 Experience Point,* 20000G|T_108000000;", 1000);
		Add("Party Quest", 70025, "QSTTIP:s:N_[PQ] Defeat the Giant Spider|D_Recently a new altar has been found at the back of Alby Dungeon. Try offering an item that isn't a Fomor Pass, and defeat the [Giant Spider] that can be found at the deepest part of the dungeon.|A_|R_* 4600 Experience Point,* 10000G|T_108000000;", 1000);
		Add("Party Quest", 70025, "QSTTIP:s:N_[PQ] Defeat the Giant Red Spider|D_Recently a new altar has been found at the back of Alby Dungeon. Try offering [Alby Basic Fomor Pass], and defeat [Giant Red Spider] that can be found at the deepest part of the dungeon.|A_|R_* 13800 Experience Point,* 15000G|T_108000000;", 1500);
		Add("Party Quest", 70025, "QSTTIP:s:N_[PQ] Defeat the Lycanthrope (Hard Mode)|D_Recently a new altar has been found at the back of Alby Dungeon. Try offering [Alby Intermediate Fomor Pass], and defeat [Lycanthrope] that can be found at the deepest part of the dungeon.|A_|R_* 22600 Experience Point,* 20000G|T_108000000;", 2000);
		Add("Party Quest", 70025, "QSTTIP:s:N_[PQ] Defeat the Arachne|D_Recently a new altar has been found at the back of Alby Dungeon. Try offering [Alby Adv. Fomor Pass], and defeat [Arachne] that can be found at the deepest part of the dungeon.|A_|R_* 55800 Experience Point,* 25000G|T_108000000;", 2500);
		Add("Party Quest", 70025, "QSTTIP:s:N_[PQ] Defeat the Golem|D_Recently a new altar has been found at the back of Ciar Dungeon. Try offering an item that is not a Fomor Pass, and defeat a [Golem] that can be found at the deepest part of the dungeon.|A_|R_* 10000 Experience Point,* 10000G|T_108000000;", 1000);
		Add("Party Quest", 70025, "QSTTIP:s:N_[PQ] Defeat the Golem|D_Recently a new altar has been found at the back of Ciar Dungeon. Try offering [Ciar Basic Fomor Pass], and defeat a [Golem] that can be found at the deepest part of the dungeon.|A_|R_* 21000 Experience Point,* 15000G|T_108000000;", 1500);
		Add("Party Quest", 70025, "QSTTIP:s:N_[PQ] Defeat the Golem|D_Recently a new altar has been found at the back of Ciar Dungeon. Try offering [Ciar Intermediate Fomor Pass], and defeat a [Golem] that can be found at the deepest part of the dungeon.|A_|R_* 16000 Experience Point,* 20000G|T_108000000;", 2000);
		Add("Party Quest", 70025, "QSTTIP:s:N_[PQ] Defeat the Golem|D_Recently a new altar has been found at the back of Ciar Dungeon. Try offering [Ciar Adv. Fomor Pass], and defeat a [Golem] that can be found at the deepest part of the dungeon.|A_|R_* 56800 Experience Point,* 25000G|T_108000000;", 2500);

		// Etc.
		Add("Etc.", 1045); // Hit What You See
	}
}
