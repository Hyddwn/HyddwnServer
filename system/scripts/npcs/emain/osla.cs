//--- Aura Script -----------------------------------------------------------
// Osla
//--- Description -----------------------------------------------------------
// Emain Macha Weapons Dealer
//---------------------------------------------------------------------------

public class OslaScript : NpcScript
{
	public override void Load()
	{
		SetRace(10001);
		SetName("_osla");
		SetFace(skinColor: 17, eyeType: 2, eyeColor: 47, mouthType: 0);
		SetStand("human/male/anim/male_natural_stand_npc_bryce");
		SetLocation(52, 37398, 42382, 28);
		SetGiftWeights(beauty: 2, individuality: 2, luxury: 1, toughness: 0, utility: 1, rarity: 0, meaning: 0, adult: 1, maniac: 2, anime: 0, sexy: 2);

		EquipItem(Pocket.Face, 3900, 0x00009945, 0x0000A893, 0x00F8F9E3);
		EquipItem(Pocket.Hair, 3037, 0x00E29B45, 0x00E29B45, 0x00E29B45);
		EquipItem(Pocket.Armor, 13023, 0x00D6C5BA, 0x00947E6B, 0x00000000);
		EquipItem(Pocket.Shoe, 17510, 0x0061534E, 0x00F6D493, 0x00644356);


		AddPhrase("Hmm... where's my knight in shining armor?");
		AddPhrase("It's a problem if I have too many customers to take care of...");
		AddPhrase("...But I guess it's okay since I'm prettty...");
		AddPhrase("I keep getting the numbers wrong...");
		AddPhrase("Hmm...I don't look that bad...not bad at all...");
		AddPhrase("Why do I keep forgetting things...");
		AddPhrase("Oh no... another bounced check...");
		AddPhrase("Narrrr....");
		AddPhrase("Your item is repaired! Please take it.");
		AddPhrase("What was I thinking...");
		AddPhrase("...Ahhh... What's wrong with me...");
	}

	protected override async Task Talk()
	{
		SetBgm("NPC_Osla.mp3");

		await Intro(L("A beautiful lady wearing a detailed plate mail armor holds her hand up to her face, as if curious but slightly troubled.<br/>Her deep green eyes sparkle underneath her thin raised eyebrows. Her light brown hair is tied in a pony-tail behind her head which<br/>hangs down past her shoulders.<br/>As she talks she seems distracted, and whenever she is not talking she quietly hums to herself."));

		Msg("...(with a vacant stare)", Button("Start Conversation", "@talk"), Button("Shop", "@shop"), Button("Repair Item", "@repair"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());

				if (Player.IsUsingTitle(11001))
				{
					Msg("...Did you rescue the Goddess....?");
					Msg("Narrrr.... What did she give you?");
					Msg("...I can't believe you rescued the Goddess just to get that...");
					Msg("and if I say it's not worth it, it means it really isn't worth your effort.<br/>Why didn't you ask for more...?");
				}
				else if (Player.IsUsingTitle(11002))
				{
					Msg("Whaaa...?<username/>, you...?<br/>Guardian of Erinn? Are you seriousss?");
					Msg("...Mm... I was wrong about you... <username/>....<br/>I think I learned a great lesson from thisss...");
					Msg("...Never judge a book<br/>by it's coverrrr...");
				}

				await Conversation();
				break;

			case "@shop":
				Msg("Take your time...");
				OpenShop("OslaShop");
				return;

			case "@repair":
				Msg(L("...Do you have things to repair...?<br/>...If you have any, let me repair it...<br/>....<br/>...If you can't trust me, then that's your loss...<repair rate='95' stringid='(*/smith_repairable/*)' />"));

				while (true)
				{
					var repair = await Select();

					if (!repair.StartsWith("@repair"))
						break;

					var result = Repair(repair, 95, "/smith_repairable/");
					if (!result.HadGold)
					{
						RndMsg(
							L("You must pay up front before the repair...<br/>you should pay first..."),
							L("...Well...you don't have enough money to get it repaired...?<br/>....<br/>Hey... I can't do this for free...<br/>I have to pay the rent for this building, toooooo..."),
							L("Sorry, you need to pay first...<br/>If you want to fix your item, you must paaaay...")
						);
					}
					else if (result.Points == 1)
					{
						if (result.Fails == 0)
							RndMsg(
								L("...Don't worry...I do this everyday...<br/>...Repairing is easy...<br/>Here's your item. I fixed 1 point..."),
								L("Feeling anxious..? Me tooooo...<br/>...But don't worry...<br/>Here, I fixed 1 point..."),
								L("Whoa...this was much easier than I'd thought...<br/>I increased 1 point of durability..."),
								L("Ah! What's this? This is gooooood..<br/>I fixed 1 point..")
							);
						else
							// Lines roughly translated from Japanese
							RndMsg(
								L("...I was worried...<br/>Soooorry... *Sniffle*"),
								L("Ahh... My hands...<br/>It huuuurts... Its bleeding...<br/>Ah, I'm soooorry."),
								L("Ah... I've failed...<br/>But its alsooooo a problem when you handle<br/>your equipment soooo roughly...")
							);
					}
					else if (result.Points > 1)
					{
						if (result.Fails == 0)
							RndMsg(
								L("...Nervous? Me toooo...<br/>hehehe.. don't worry...<username/>.<br/>I didn't make any boo boos..."),
								L("...Feeling nervous? Me tooo...<br/>but everything went well, and it came out like new...<br/>Well, sometimes you need something like this to happen...."),
								L("...Nervous...? Me tooo...<br/>But since you took care of your item very well, I was able to fix it easily...<br/>Yes... just like new...")
							);
						else
							// Line roughly translated from Japanese
							Msg(string.Format(L("...Are you worried...? I am as wellll<br/>...<p/>Yes, the repair is finished.<br/>However, the state of your item was very bad from the starrrt...<br/>...{1} point(s) was the most I could repairrr...<br/>...From now oooon, please use your equipment carefully."), result.Fails, result.Successes));
					}
				}

				Msg(L("...Have you heard about the Holy Water of Lymilark?<br/>It is esier to fix items with the Holy Water on it...<br/>If you haven't used it, then I strongly recommend that you dooooo...<br/>If you help Priest James at church, he will give you some Holy Water of Lymilark....<repair hide='true'/>"));
				break;
		}

		End("Thank you, <npcname/>. I'll see you later!");
	}

	private void Greet()
	{
		if (Player.IsDoingPtjFor(NPC))
		{
			Msg(FavorExpression(), L("Missing text, introduction during PTJ"));
		}
		else if (Memory <= 0)
		{
			Msg(FavorExpression(), L("Hello...umm...Are you a customer...?"));
		}
		else if (Memory == 1)
		{
			Msg(FavorExpression(), L("Wow... You ARE a customer... Nice to meet you..."));
		}
		else if (Memory == 2)
		{
			Msg(FavorExpression(), L("I bet you hear that a lot, that you have that familiar look... You look like someone I've met before..."));
		}
		else if (Memory <= 6)
		{
			Msg(FavorExpression(), L("Well... So your name is...umm...<br/>Haha... I forgot...I'm sorry..."));
		}
		else
		{
			Msg(FavorExpression(), L("...Your name must be <username/>, riiighht?<br/>No, I thought I'd met someone with a similar name..."));
		}

		UpdateRelationAfterGreet();
	}

	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				Msg(FavorExpression(), "Heeheehee... Osla, that is my name...");
				Msg("Heh... You already knew...?<br/>Are you interested in me?<br/>That makes me feel so gooooooood...");
				ModifyRelation(Random(2), 0, Random(3));
				break;

			case "rumor":
				Msg(FavorExpression(), "Around here...? Well...<br/>It's been a long time since I've gone out of this town... heee...");
				ModifyRelation(Random(2), 0, Random(3));
				break;

			case "shop_misc":
				Msg("A General Shop?<br/>Hmmmmm... I don't know if we have that in our town...<br/>Well, if you go around different places, then you'll find everything you'll need...<br/>...I've never had a problem with not having a General Shop around here before...");
				break;

			case "shop_grocery":
				Msg("A Grocery... Store?<br/>...Ah, you mean the restaurant...");
				Msg("Then you should got to Loch Lios.<br/>There's a girl with cherry-colored hair...<br/>Her name was Shena... or was it Sherry...?<br/>Well, she seems a little too smart to be a waitress, really...");
				Msg("..Ooops, what am I saying...");
				break;

			case "shop_healing":
				Msg("The Healer's Hoooouse?<br/>You must be talking about the lovely Agnes, am I correct...?<br/>Where was it...?");
				Msg("...It's a little too complicated to explain...<br/>I think it'll be easier for you to check the minimap...");
				Msg("...");
				Msg("I'm sorry...");
				break;

			case "shop_inn":
				Msg("...An inn?<br/>Why would you need one when you can just set up a campfire and rest??");
				break;

			case "shop_bank":
				Msg("Are you loojking for Jocelin...?<br/>She's right there...");
				Msg("Don't tell me you haven't met her yet...");
				break;

			case "shop_smith":
				Msg("A Blacksmith's Shop?<br/>Ehh... why would you want to go there...?");
				Msg("I can repair your items right here...");
				Msg("...You can't trust my work?<br/>That makes me sad.");
				break;

			case "skill_rest":
				Msg("...Yes... It is a necessary skill...the resting skilll...<br/>Actually, my legs have been hurting, and I wanted to rest, but...");
				Msg("...If I keep working like this, then<br/>my legs will get thick, and... it'll ruin my perfect body... *sigh*....");
				break;

			case "skill_range":
				Msg("You know you can learn that simply by buying a<br/>bow or a crossbow and use it a couple of times...");
				Msg("...You eyes tell me you're thinking I'm trying<br/>too hard to sell my weapons to you...<br/>You're mean.");
				break;

			case "skill_instrument":
				Msg("...Nele knows a lot about stuff like that...<br/>Well, although he looks little...you know...but<br/>he is a bard, that's the most important thing, right...?<br/>You can find him at the Square, and...");
				Msg("...when you see him, can you tell him he needs a haircut?");
				break;

			case "skill_composing":
				Msg("The Composing skill...?");
				Msg("...You don't look like a person who's interested in that...<br/>I'm surprised.");
				break;

			case "skill_tailoring":
				Msg("The Tailoring skill...?<br/>Why would you want to wear regular clothes...?<br/>The defense rate is sooooo low...");
				Msg("...If you want style, armors are the way to go.<br/>I have some on display, so<br/>do you want to open up your Shop window and look at them?");
				Msg("I'll even let you touch them if you want...");
				break;

			case "skill_magnum_shot":
				Msg("I only know how to make them...<br/>I don't know how to use them...");
				Msg("Not!<br/>Magnum shot is a skill that focuses your energy onto the bow,<br/>and deliver a single deadly blow to your enemy.<br/>If you are a bowman, you'll need to master this skill...");
				Msg("...Narrrr...<br/>I haven't done this in a whiiiiiile...<br/>Didn't it sound weird that I talked like that?");
				break;

			case "skill_counter_attack":
				Msg("Counterattack...<br/>Just like it says, you are countering the enemy's attack with yours...");
				Msg("...I shouldn't be dating someone who's experienced in Counterattack...<br/>They just don't know what a lady like I would want...");
				break;

			case "skill_smash":
				Msg("Are you interested in the Smash skill, <username/>?<br/>Hahaha...");
				Msg("...");
				Msg("I don't know anything about that skill...<br/>...Were you expecting something from me?");
				break;

			case "skill_windmill":
				Msg("What? Was there a skill like that...?<br/>Ohhh... I know what it is.<br/><username/>, you make it sound a lot different.<br/>Sounds so fresh...");
				break;

			case "skill_gathering":
				Msg("...For people like me...<br/>We love people who dig ore from mines...");
				Msg("...Wait, I'd rather have an iron ingot than an iron ore...<br/>What was that skill that turns ore into ingots...<br/>...Ah, I can't remember the name... Arrrrggh...");
				break;

			case "square":
				Msg("The Square....<br/>It's right in front of this place...");
				Msg("Whenever you ask me these questions<br/>it feels like you are trying to test me or something<br/>...");
				Msg("...If you know what you did was wrong, then I'll forgive you.");
				break;

			case "pool":
				Msg("You must be talking about the one over there...hmmm...?<br/>...Well, it does look a lot like a reservoir afterall..");
				Msg("...I thought it was a lake all this time...");
				break;

			case "farmland":
				Msg("You'll see it around the outer edges of this town.<br/>Fields full of corn and wheat...");
				break;

			case "shop_headman":
				Msg("A town Chief...?");
				Msg("Hmm... I don't like the sound of it...<br/><username/>, does this place look country to you...?");
				Msg("This is a big town, B.I.G.!<br/>You are lucky that Aodhan or our Lord<br/>didn't hear what you just said.");
				break;

			case "temple":
				Msg("If you head south of the Square, you'll find the church...<br/>There's a Priest named James, and he's pretty handsome...<br/>Have you seen him?");
				Msg("...If you happen to see him, please say hi for me...");
				break;

			case "school":
				Msg("There was a school back then...<br/>Maybe their teachings weren't worth the tuition, and<br/>I think that's why they closed down...");
				break;

			case "skill_campfire":
				Msg("Campfire skill is<br/>the most essential skill for travelers...<br/>...If you are a bowman<br/>you can use it as your ally during the battles.");
				break;

			case "shop_restaurant":
				Msg("Restaurant? There's one around here...<br/>it's called...umm... Loch Lio...s...<br/>or something like that...");
				Msg("Anyway it is near the lake...<br/>it is right over there...");
				break;

			case "shop_armory":
				Msg("Yes, I run the Weapons Shop...hmmmm...<br/>I have my own business at a relatively early age,<br/>and I have the looks...");
				Msg("Don't you think I'm a very attractive catch...?<br/>...But I don't know why I still don't have a boyfriend.... *giggle*...");
				break;

			case "shop_cloth":
				Msg("Ailionoa runs the Clothing Shop.<br/>It is pretty big...<br/>The name... is...<br/>Claims...Cairde...or is it...?");
				Msg("Well, I don't know...<br/>I'm not sure of the name, but I know where it is...hmmmmm...<br/>Her store is behind the Auditorium over there.");
				break;

			case "shop_bookstore":
				Msg("A bookstore...?<br/>There's no bookstore in this town...hmmmm...<br/>Do you like to read books...hmmm...?");
				Msg("...Why do you like to read books...?<br/>They are no good for anything except to cause headaches...");
				Msg("...since I know that you, <username/>, like to read books,<br/>I'll ask you questions<br/>whenever there's something I don't know, <username/>...hmmm...");
				Msg("...I shouldn't forget this...");
				break;

			case "shop_goverment_office":
				Msg("Instead of a town office, we have our Lord's castle over there.");
				Msg("...I've heard that our Lord's legs have been bugging him.<br/>If not, he would have been my prince charming indeed...hehe.");
				break;

			case "graveyard":
				Msg("...Do you have something to bury?");
				break;

			case "bow":
				Msg("...Come to think of it, a lot of people are looking for bows...<br/>Maybe I need to get some more...<br/>...but I still have some available,<br/>so do you want to take a look once more?");
				break;

			case "lute":
				Msg("Lute...?<br/>Lute... I've heard about it...");
				Msg("..Ahh... I keep forgetting things these days.");
				break;

			case "complicity":
				Msg("...I know it doesn't sound good...<br/>but I have no idea what you are talking about...");
				break;

			case "tir_na_nog":
				Msg("I think I heard something like that<br/>from my old friend Nerys...<br/>What did she say...?");
				Msg("...Hold on...was it Nerys...?<br/>Or was it Aodhan...?");
				break;

			case "nao_friend":
				Msg("Everyone's been talking about Nao this, and Nao that.<br/>Where does she live...");
				break;

			case "nao_owl":
				Msg("I see owls from time to time, but...<br/>...does Nao raise owls or something?");
				break;

			case "nao_blacksuit":
				Msg("It isn't an armor...right?<br/>...I don't have much interest in regular clothes...");
				break;

			case "nao_owlscroll":
				Msg("...Well, owls seem to carry anything you can imagine...<br/>so it doesn't seem weird or anything for them to be carrying scrolls...");
				break;

			case "breast":
				Msg("...If you're talking about that, I am totally sexy～♪");
				Msg("...eh...that was just a joke,<br/>but it's like I'm getting more weird looks that usual...<br/>*sniffle*");
				break;

			case "EG_C2_Unknown_Continet":
				Msg("An uncharted territory, did you say?<br/>Does that mean you're looking for a piece of land called uncharted? Huh?<br/>Ok, don't mind me, if it isn't.");
				break;

			case "EG_C2_HowTo_Shipboard":
				Msg("You should ask me about things like that!<br/>Ahh, but... The little mermaid instead of the knight in white shining armor puleez!<br/>Hehehe! Lucky!");
				break;

			case "two_sword_skill":
				Msg("Twin swords...?<br/>I really don't know much about stuff like that...<br/>You should go ask Aodhan over there about things like that...");
				break;

			default:
				RndFavorMsg(
					"That means that... ummmm...",
					"Have I heard that story before....?",
					"Narrr.... It is... something that I think I used to know....",
					"Umm... so that's... ummm... I don't remember...",
					"Narrr... It is... something that I think I used to know....",
					"I think I'd heard about that somewhere...I don't remember it though.",
					"I have no idea what you're talking about."
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
					"Hah...another day, another gift...<br/>I don't know what to do with it...",
					"I feel like I'm always on the receiving end... Thank you for your kindness...",
					"A gift... Is this for meeeee...?<br/>Thank you...",
					"Do you want to give me a gift? Is that how it is?"
				);
				break;
		}
	}
}

public class OslaShop : NpcShopScript
{
	public override void Setup()
	{
		Add("Weapon", 45001, 20);  // 20 Arrows
		Add("Weapon", 45001, 100); // 100 Arrows
		Add("Weapon", 40023);      // Gathering Knife
		Add("Weapon", 40022);      // Gathering Axe
		Add("Weapon", 45002, 50);  // 50 bolts
		Add("Weapon", 45002, 200); // 100 bolts
		Add("Weapon", 40027);      // Weeding Hoe
		Add("Weapon", 40003);      // Short Bow
		Add("Weapon", 40026);      // Sickle
		Add("Weapon", 40006);      // Dagger
		Add("Weapon", 40243);      // Battle Short Sword
		Add("Weapon", 40015);      // Fluted Short Sword
		Add("Weapon", 40005);      // Short Sword
		Add("Weapon", 40007);      // Hatchet
		Add("Weapon", 40014);      // Composite Bow
		Add("Weapon", 40013);      // Long Bow
		Add("Weapon", 40011);      // Broad Sword
		Add("Weapon", 40010);      // Longsword
		Add("Weapon", 40016);      // War Hammer
		Add("Weapon", 40012);      // Bastard Sword
		Add("Weapon", 40242);      // Battle Sword
		Add("Weapon", 46001);      // Round Shield
		Add("Weapon", 46006);      // Kite Shield
		Add("Weapon", 40031);      // Crossbow
		Add("Weapon", 40020);      // Wooden Club

		Add("Shoes & Gloves", 16501); // Leather Protector
		Add("Shoes & Gloves", 16504); // Counter Gauntlet
		Add("Shoes & Gloves", 16007); // Cores Ninja Gloves
		Add("Shoes & Gloves", 16005); // Wood Plate Cannon
		Add("Shoes & Gloves", 16009); // Tork's Hunter Gloves
		Add("Shoes & Gloves", 16017); // Standard Gloves
		Add("Shoes & Gloves", 16500); // Ulna Protector Gloves
		Add("Shoes & Gloves", 16505); // Fluted Gauntlet
		Add("Shoes & Gloves", 17500); // High Polean Plate Boots
		Add("Shoes & Gloves", 17501); // Solleret Shoes
		Add("Shoes & Gloves", 17506); // Long Greaves
		Add("Shoes & Gloves", 16523); // Graceful Gauntlet
		Add("Shoes & Gloves", 17515); // Graceful Greaves

		Add("Helmet", 18501); // Guardian Helm
		Add("Helmet", 18502); // Bone Helm
		Add("Helmet", 18505); // Spiked Helm
		Add("Helmet", 18506); // Wing Half Helm
		Add("Helmet", 18500); // Ring Mail Helm
		Add("Helmet", 18504); // Cross Full Helm
		Add("Helmet", 18508); // Slit Full Helm
		Add("Helmet", 18509); // Bascinet
		Add("Helmet", 18515); // Spiked Cap
		Add("Helmet", 18525); // Waterdrop Cap
		Add("Helmet", 18520); // Steel Headgear
		Add("Helmet", 18521); // European Comb
		Add("Helmet", 18522); // Pelican Protector
		Add("Helmet", 18524); // Four Wings Cap
		Add("Helmet", 18519); // Panache Head Protector
		Add("Helmet", 18545); // Graceful Helmet
		Add("Helmet", 18546); // Norman Warrior Helmet

		Add("Armor", 13017); // Surcoat Chain Mail
		Add("Armor", 13001); // Melka Chain Mail
		Add("Armor", 13010); // Round Pauldron Chainmail
		Add("Armor", 13015); // Brigandine
		Add("Armor", 14005); // Drandos Leather Mail (F)
		Add("Armor", 14011); // Drandos Leather Mail (M)	
		Add("Armor", 14008); // Full Leather Armor Set	
		Add("Armor", 14010); // Light Leather Mail (M)
		Add("Armor", 14007); // Padded Armor with Breastplate
		Add("Armor", 14013); // Lorica Segmentata
		Add("Armor", 14016); // Cross Belt Leather Coat
		Add("Armor", 14017); // Three-Belt Leather Mail
		Add("Armor", 14018); // Norman Warrior Armor
		Add("Armor", 14019); // Graceful Plate Armor
		Add("Armor", 13000); // Full Ring Mail
		Add("Armor", 13004); // Endoria Lorica Hamata
		Add("Armor", 13013); // Endoria Lorica Hamata
		Add("Armor", 13008); // Phoenix Surcoat Plate
		Add("Armor", 13018); // Double Ring Mail

		Add("Event"); // Empty
		//Add("Event", 45103, 20); // 20 Toy Arrows
		//Add("Event", 45103, 20); // 100 Toy Arrows

		if (IsEnabled("FighterJob"))
		{
			Add("Weapon", 40179); // Spiked Knuckle
			Add("Weapon", 40180); // Hobnail Knuckle
			Add("Weapon", 40244); // Bear Knuckle
		}

		if (IsEnabled("FineArrows"))
		{
			Add("Arrowhead", 64011); // Bundle of Arrowheads
			Add("Arrowhead", 64015); // Bundle of Boltheads
			Add("Arrowhead", 64013); // Bundle of Fine Arrowheads
			Add("Arrowhead", 64016); // Bundle of Fine Boltheads
			Add("Arrowhead", 64014); // Bundle of Finest Arrowheads
			Add("Arrowhead", 64017); // Bundle of Finest Boltheads
		}
	}
}
