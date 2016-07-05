//--- Aura Script -----------------------------------------------------------
// Meven
//--- Description -----------------------------------------------------------
// Priest in the Church of Lymilark
//---------------------------------------------------------------------------

public class MevenScript : NpcScript
{
	public override void Load()
	{
		SetRace(10002);
		SetName("_meven");
		SetFace(skinColor: 21, eyeType: 5, eyeColor: 27);
		SetStand("human/male/anim/male_natural_stand_npc_Meven");
		SetLocation(4, 954, 2271, 198);
		SetGiftWeights(beauty: 1, individuality: 0, luxury: -1, toughness: 0, utility: 2, rarity: 0, meaning: 2, adult: 1, maniac: 0, anime: 0, sexy: 0);

		EquipItem(Pocket.Face, 4900, 0x00D45D8D, 0x0087178A, 0x0024AF7C);
		EquipItem(Pocket.Hair, 4026, 0x00EBE0C0, 0x00EBE0C0, 0x00EBE0C0);
		EquipItem(Pocket.Armor, 15006, 0x00313727, 0x00282C2B, 0x00F0DA4A);
		EquipItem(Pocket.Shoe, 17012, 0x00313727, 0x00FFFFFF, 0x00A0927D);

		AddPhrase("...");
		AddPhrase("(Smile)");
		AddPhrase("!");
		AddPhrase("?");
		AddPhrase("??");
		AddPhrase("???");
		AddPhrase("Oops! I forgot to iron my robes!");
		AddPhrase("Ah, I forgot I have some plowing to do.");
		AddPhrase("Perhaps I could take a quick rest now.");
		AddPhrase("Hiccup! Hiccup! (Confused)");
	}

	protected override async Task Talk()
	{
		SetBgm("NPC_Meven.mp3");

		await Intro(L("Dressed in a robe, this composed man of moderate build maintains a very calm posture.<br/>Every bit of his appearance and the air surrounding him show that he is unfailingly a man of the clergy.<br/>Silvery hair frames his friendly face, and his gentle eyes suggest a rather quaint and quiet mood with flashes of hidden humor."));

		Msg("Welcome to the Church of Lymilark.", Button("Start a Conversation", "@talk"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());

				if (Title == 11001)
				{
					Msg("Even Tarlach had failed,<br/>but you managed to do it...<br/>I'd like to congratulate you.");
					Msg("You saved this world from great danger.<br/>Have confidence in your thoughts and actions,<br/>and try to live up to your reputation.");
					Msg("...Just a piece of advice for you that should be taken with a pinch of salt.");
				}
				else if (Title == 11002)
				{
					Msg("...I see...<br/>So you're the one<br/>who prevented Macha from being reborn...");
					Msg("Good job. <username/>...<br/>The sky is the limit for you<br/>to change this world to a better place...");
				}

				await Conversation();
				break;
		}

		End("Goodbye <npcname/>. I'll see you later!");
	}

	private void Greet()
	{
		if (Memory <= 0)
		{
			Msg(FavorExpression(), L("Lymilark must have led you here."));
		}
		else if (Memory == 1)
		{
			Msg(FavorExpression(), L("It's nice to see you again."));
		}
		else if (Memory == 2)
		{
			Msg(FavorExpression(), L("<username/> you are, I believe.<br/>How can I help you?"));
		}
		else if (Memory <= 6)
		{
			Msg(FavorExpression(), L("Nice to see you, <username/>."));
		}
		else
		{
			Msg(FavorExpression(), L("It seems we meet each other often these days, <username/>.<br/>Nice to see you."));
		}

		UpdateRelationAfterGreet();
	}

	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				GiveKeyword("temple");
				Msg(FavorExpression(), "I am Priest <npcname/>.<br/>It's so nice to see someone cares for an old man.<br/>Ha ha.");
				ModifyRelation(Random(2), 0, Random(3));
				break;

			case "rumor":
				Msg(FavorExpression(), "The General Shop, Grocery Store and the Bank<br/>surround the Square of the town.<br/>A bit higher up the hill is the Chief's House.");
				ModifyRelation(Random(2), 0, Random(3));
				break;

			case "about_arbeit":
				Msg("Hmmm...");
				break;

			case "shop_misc":
				GiveKeyword("square");
				Msg("I haven't been to the General Shop for a long time.<br/>I should go and see brother Malcolm someday.<br/>If you happen to pass by the Square,<br/>please give my regards to Malcolm, would you?");
				break;

			case "shop_grocery":
				Msg("I can smell something pleasantly appetizing<br/>at meal time from here.<br/>Caitin must be cooking at the Grocery Store up there.");
				break;

			case "shop_healing":
				GiveKeyword("tir_na_nog");
				Msg("A diseased body can be treated at the Healer's House, but an ill mind should be treated at the Church.<br/>I hope you can stay away from an obsession with the external appearance of a person<br/>since it may erode the innocence of your soul.<br/>A soul that has lost its innocence further delays the advent of Tir Na Nog.");
				Msg("Go to the other side of the Square and walk up the path a little and you'll find the Healer's House.");
				break;

			case "shop_inn":
				Msg("Nora is a relative of Piaras at the Inn.<br/>She takes care of all sorts of chores there.<br/>Please don't ask them about their relationship.<br/>For some unkown reason, they don't like to talk about it.");
				break;

			case "shop_bank":
				Msg("It looks like the Bank needs some help these days.<br/>It is not in full operation yet, I'm afraid.<br/>And yet, Bebhinn is trying to handle everything at the Bank all by herself. She's just that stubborn sometimes.");
				Msg("Anyway, it is not for me to decide...<br/>Ah, have you been there? It's just up there.");
				break;

			case "shop_smith":
				Msg("That's right, I just remembered I broke my Pickaxe.<br/>I forgot to take it to the Blacksmith's Shop again.");
				break;

			case "skill_range":
				GiveKeyword("bow");
				Msg("As far as I know, you need a bow before anything else<br/>to attack an enemy from a distance.<br/>You could perhaps learn magic or something<br/>for the same purpose, but");
				Msg("magic requires AP to learn and books are also costly.<br/>So if it's just the long range attack you're interested in,<br/>I suggest you buy a bow and practice on your own.<br/>It would take less time and Gold in the end.");
				break;

			case "skill_instrument":
				Msg("My vision is failing, and my hands are shaking. Must be my old age.<br/>Speaking of playing instruments,<br/>Priestess Endelyon handles everything during the service at Church.<br/>It's better for you to talk to her directly, I believe.");
				break;

			case "skill_composing":
				Msg("Do you want to know about the Composing skill?<br/>Priestess Endelyon is excellent at playing instruments.<br/>She would probably know about writing songs too.");
				Msg("It sounds as if I'm making her<br/>take care of everything.");
				break;

			case "skill_tailoring":
				GiveKeyword("shop_misc");
				GiveKeyword("shop_grocery");
				Msg("Oh! You heard about this from Priestess Endelyon a minute ago?<br/>Indeed. She seems to have a great interest in the skill these days.<br/>I saw her talking to Caitin many times<br/>after buying Tailoring kits from the General Shop.");
				Msg("You could probably receive a lot of help from Caitin.<br/>Why don't you go and visit her?<br/>She is always at the Grocery Store.");
				Msg("I'm telling you she's always there<br/>because some people wander into the field<br/>and ask the shepherd boy where she is.<br/>I'm serious, some people did that before.");
				break;

			case "skill_magnum_shot":
				Msg("Magnum Shot skill? Are you going to ask that sort of question to Chief Duncan?<br/>Ranald would probably know about that.<br/>Who else would know about it in this town<br/>if it's not our martial art teacher?");
				Msg("Ah, Trefor might also know about that.<br/>Then I suggest you go and talk to Trefor first. He's in the northern part of the town.<br/>If he says he doesn't know,<br/>then you can go to Ranald and talk about the skill.");
				break;

			case "skill_counter_attack":
				Msg("Melee Counterattack skill? Are you asking me about the Melee Counterattack skill?<br/>I am a priest, serving Lymilark to spread the divine love and blessings.<br/>not a person who fights or seeks revenge.");
				Msg("If you really need to know about this, go and see Ranald<br/>or talk to Trefor in the north.");
				break;

			case "skill_smash":
				Msg("Do you want to know about the Smash skill?<br/>I think there is a book on the Smash skill.<br/>I do not have knowledge of it myself.<br/>I guess you can talk to Ranald about it.");
				Msg("By the way, speaking of Ranald. I haven't seen him today.<br/>He seems quite interested in striking up a conversation with Priestess Endelyon.<br/>He comes here often and talks nonsense.");
				break;

			case "skill_gathering":
				Msg("I don't have much to tell you about Gathering,<br/>but it will be very helpful for you<br/>to talk to Ferghus at the Blacksmith's Shop.");
				break;

			case "square":
				Msg("Walk up the path near the Church to get to the Square.<br/>Don't worry. It's near here.<br/>You will find it very easily.");
				break;

			case "pool":
				GiveKeyword("windmill");
				Msg("The reservoir? It's right in front of here.<br/>Oh, you must have missed it on your way here.<br/>It's all right. Everyone makes mistakes.");
				Msg("Well, have you walked up along the waterway from the reservoir?<br/>You can see the Windmill from where the waterway meets the Adelia Stream.");
				break;

			case "farmland":
				Msg("The farmland is right down there.<br/>Please don't walk through farmland without permission.<br/>People would be very disappointed if they saw the crops trodden and ruined.");
				Msg("It happens a lot these days, and I am deeply troubled just like others.");
				break;

			case "windmill":
				Msg("Are you looking for the Windmill? Well...<br/>It's quite far from here.");
				Msg("You could either walk against the Adelia flow,<br/>or walk to the Square first,<br/>and follow the path down to the Inn.<br/>Then go around the town's boundary.");
				Msg("Ah, that's right. Just go to the bridge near the wood barrels piled up.<br/>You can go straight to the Windmill!<br/>Was it too difficult to remember all I said?<br/>Then you can simply refer to your Minimap.");
				break;

			case "brook":
				GiveKeyword("windmill");
				Msg("Adelia Stream flows in front of our town, next to the Windmill.<br/>It was named after Saint Adelia,<br/>who used to be the priestess of the town.");
				Msg("If you knew how important Adelia Stream is to Tir Chonaill,<br/>as a water source for drinking and farming,<br/>I believe you could understand<br/>how much respect we have for Saint Adelia.");
				break;

			case "shop_headman":
				Msg("Are you looking for Chief Duncan's house?<br/>He lives up there near the Square.<br/>It's not that far from here. You'll find it easily.");
				Msg("He has been in this town for a long time.<br/>There's not so much in Tir Chonaill he doesn't know about.<br/>Talk to him often. He would be happy to give you good advice.");
				break;

			case "temple":
				Msg("This is the Church of Lymilark.<br/>In this Church, we spread God's grace and love<br/>to anyone who seeks for wisdom and comfort.");
				break;

			case "school":
				Msg("The School is just down there.");
				Msg("You will find Ranald who teaches martial arts,<br/>or Lassar who teaches magic.<br/>If you ask them whatever you're curious about,<br/>they will kindly teach you.");
				Msg("By the way,<br/>I haven't seen Ranald today.");
				break;

			case "skill_windmill":
				GiveKeyword("windmill");
				Msg("Can you tell the difference between the Windmill and the Windmill skill?<br/>If you can't, you may be like Ferghus.");
				Msg("Ha ha, I am just kidding.<br/>You don't have to be upset like that<br/>about a silly joke of an old man, ha ha.");
				break;

			case "skill_campfire":
				GiveKeyword("brook");
				Msg("Well...<br/>Did Piaras tell you this?<br/>Then, he must have told you to go and see Deian<br/>across the Adelia Stream.");
				Msg("Oh, please don't misunderstand.<br/>I just heard about it<br/>from someone who visited the Church before.");
				Msg("You can find Deian across the Adelia Stream,<br/>located at the entrance of the town.<br/>You should go and see him.");
				break;

			case "shop_restaurant":
				GiveKeyword("shop_grocery");
				Msg("Well, I am quite hungry now.<br/>Why don't we eat something?<br/>Caitin's Grocery Store sells some food.<br/>It would be wonderful if you could buy some from there.");
				Msg("Hahaha. I am a humble priest serving God at the Church. What Gold could I possibly have?<br/>You can count this as your contribution.");
				break;

			case "shop_armory":
				GiveKeyword("shop_smith");
				Msg("You wish to talk about the Weapons Shop here at the Church?<br/>I'm afraid you have a peculiar sense of choosing questions.<br/>You should ask about weapons<br/>not at the Church, but at the Blacksmith's Shop.");
				Msg("You're not going to ask about the Church<br/>at the Blacksmith's Shop, are you?");
				break;

			case "shop_cloth":
				Msg("Hahaha. As you can see,<br/>this robe I'm wearing is the one and only piece of clothing I have.<br/>Why don't you ask Priestess Endelyon?");
				Msg("Oh, but I suppose she's not much different.");
				break;

			case "shop_bookstore":
				GiveKeyword("mabinogi");
				Msg("If you are looking for a book, please go to the General Shop.<br/>Malcolm may have some books on the Composing skill, at least.<br/>This town has few books, if any. Not even a book on Mabinogi, which I know is pretty common in other cities.");
				Msg("I guess everybody is busy making a living.<br/>That's why nobody has time to read books.");
				break;

			case "shop_government_office":
				GiveKeyword("shop_headman");
				Msg("Are you looking for a town office?<br/>A small town like Tir Chonaill doesn't have a town office.<br/>Moreover, we are not under the control of the Aliech Kingdom.<br/>Tir Chonaill is sort of an autonomous district built by the descendants of Ulaid.");
				Msg("If you must go to a town office,<br/>why don't you try a larger city?<br/>I believe there's one in Dunbarton south of here.");
				Msg("If you simply want to know<br/>more about what's going on in town,<br/>you could try the Chief Duncan's House.");
				break;

			case "graveyard":
				GiveKeyword("shop_headman");
				Msg("The graveyard?<br/>It's near the Chief's House.");
				Msg("There are big spiders spotted there.<br/>You should be very careful.");
				Msg("Some people venture out there on purpose<br/>to get cobwebs,<br/>I heard.");
				break;

			case "bow":
				Msg("If you need a bow, you can go to the Blacksmith's Shop.<br/>It's not made of iron,<br/>but you would need arrows too.<p/>Go and ask Ferghus.<br/>He is the expert.<p/>Could you perhaps tell him<br/>to stop drinking<br/>and come to the services..?");
				break;

			case "lute":
				Msg("Are you looking for a lute?<br/>You could get a lute<br/>at Malcolm's General Shop.<br/>Tell him I sent you. Probably you could negotiate over the price.");
				Msg("Well, he may charge you more, as a matter fact.");
				break;

			case "complicity":
				Msg("My Lord!<br/>Please enrich the poor souls of your children, suffering from doubts and suspicions.");
				break;

			case "tir_na_nog":
				Msg("Tir Na Nog. It's the land free of death.<br/>It is the world supported by<br/>Lymilark, God of Love, Hymerark, God of Freedom,<br/>and Jeamiderark, God of Peace.");
				Msg("That's where all the humans like us should aspire to go.<br/>It's the land of gods.<br/>For anyone to enter the land of gods,<br/>this world must be full of love and devotion toward the gods.");
				Msg("But in reality, it's almost the opposite and I am very worried.");
				break;

			case "mabinogi":
				Msg("Mabinogi is a song that<br/>wandering bards have sung for many years.<br/>It is mostly made of stories of legendary heroes.<br/>I believe you must have heard of them before.");
				Msg("The great king, Nuadha<br/>who fought against the evil armies even after losing one arm.<br/>Or perhaps about Lugh, the God of light,<br/>who brought peace back to this world after fighting off the Fomor.");
				Msg("Or how about the story about the evil wizard Jabchiel,<br/>who sold his soul to the evil creatures.");
				break;

			case "musicsheet":
				Msg("A Music Score? Well...<br/>I think it's better to talk to Priestess Endelyon<br/>if it's music you're interested in.<br/>Much better than an old man like me.");
				Msg("Why don't you go and ask her? She's just right there.<br/>Priestess Endelyon may not look so easy to talk to.<br/>But, she's a kind person.<br/>She will be a great help to you.");
				break;

			default:
				RndMsg(
					"...?",
					"...<br/>I really don't know.",
					"I am sorry, but ignorance is not a sin.",
					"I don't think I heard of that, I'm sorry.",
					"How could I know about that, I'm just a priest."
				);
				ModifyRelation(0, 0, Random(3));
				break;
		}
	}
}