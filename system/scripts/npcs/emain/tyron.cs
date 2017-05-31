//--- Aura Script -----------------------------------------------------------
// Tyron
//--- Description -----------------------------------------------------------
// Paladin Trainee
//---------------------------------------------------------------------------

public class TyronScript : NpcScript
{
	public override void Load()
	{
		SetRace(10002);
		SetName("_tyron");
		SetBody(height: 1.2f, weight: 0.97f, upper: 1.2f, lower: 1.03f);
		SetFace(skinColor: 15, eyeType: 9, eyeColor: 32, mouthType: 12);
		SetStand("human/anim/male_natural_sit_02.ani");
		SetLocation(52, 26103, 59831, 155);
		SetGiftWeights(beauty: 2, individuality: 1, luxury: 1, toughness: 1, utility: 2, rarity: 2, meaning: 0, adult: 1, maniac: -1, anime: 2, sexy: 1);

		EquipItem(Pocket.Face, 4900, 0x00F9A647, 0x009BD4AC, 0x00C20176);
		EquipItem(Pocket.Hair, 4030, 0x001D1712, 0x001D1712, 0x001D1712);
		EquipItem(Pocket.Armor, 13011, 0x00000000, 0x00B6A48B, 0x00241D13);
		EquipItem(Pocket.Glove, 16503, 0x006F5840, 0x00FFFFFF, 0x00FFFFFF);
		EquipItem(Pocket.Shoe, 17508, 0x00CBAF9C, 0x00CAB8A4, 0x00FFFFFF);
		EquipItem(Pocket.RightHand1, 40011, 0x00C0C0C0, 0x00304544, 0x00FFFFFF);


		AddPhrase("Hey boss, can I rest for a second?");
		AddPhrase("Training is the only way!");
		AddPhrase("I need to fix my armor...");
		AddPhrase("Yes, I am training!");
		AddPhrase("One, Two! One, Two!");
		AddPhrase("Should I go hunt down Kobold Miners again?");
		AddPhrase("Why are my sword skills not improving...");
		AddPhrase("Ahh, my back!");
	}

	protected override async Task Talk()
	{
		await Intro(L("He's a young trainee with innocent eyes and spiky hair.<br/>Even with a ragged armor that seems worn out, and a sword that seems right at home in his hands,<br/>he does not exude the aura of a veteran warrior.<br/>His voice is a bit hoarse from screaming, but he seems very enthusiastic."));

		Msg("Hey! How are you feeling today?", Button("Start Conversation", "@talk"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());

				if (Player.IsUsingTitle(11001))
				{
					Msg("...don't lie to me like that.<br/>I mean, you saved the Goddess? Whatever...");
				}
				else if (Player.IsUsingTitle(11002))
				{
					Msg("Huh? Guardian of Erinn?<br/>Wow... Congratulations! <username/>.");
					Msg("...To be honest,<br/>when you quit the Paladin training and left,<br/>I didn't understand your rationale...<br/>...But, you're definitely special. <username/>...");
					Msg("...Anyways,<br/>what benefits do I get for being your friend? Haha!<br/>I'm just kidding! No need to stress out!");
				}

				await Conversation();
				break;
		}

		End("Thank you, <npcname/>. I'll see you later!");
	}

	private void Greet()
	{
		if (Memory <= 0)
		{
			Msg(FavorExpression(), L("Eh, I don't think I've seen you before.<br/>Are you also in training?"));
		}
		else if (Memory == 1)
		{
			Msg(FavorExpression(), L("Welcome, <username/>!<br/>We have a tough training session awaiting us."));
		}
		else if (Memory == 2)
		{
			// Placeholder
			Msg(FavorExpression(), L("Welcome, <username/>!<br/>We have a tough training session awaiting us."));
		}
		else if (Memory <= 6)
		{
			// Placeholder
			Msg(FavorExpression(), L("Welcome, <username/>!<br/>We have a tough training session awaiting us."));
		}
		else
		{
			// Placeholder
			Msg(FavorExpression(), L("Welcome, <username/>!<br/>We have a tough training session awaiting us."));
		}

		UpdateRelationAfterGreet();
	}

	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			// Tyron's Private Story does not seem to affect his relationship.
			case "personal_info":
				Msg("Me? I'm Tyron, and I've been here for a few years now,<br/>still taking Paladin classes.<br/>Which means... I've been here longer than you!");
				Msg("Well, that doesn't really matter. Wanna be friends?");
				break;

			case "about_skill":
				Msg("Sigh, what do I have to learn<br/>to become a Paladin?<br/>I feel like I've trained a lot,<br/>but I don't see any improvements.");
				break;

			case "about_arbeit":
				Msg("I can't believe you're trying to take Paladin classes while doing a Part-Time Job!<br/>You must be in ridiculous shape... I envy you.");
				break;

			case "about_study":
				Msg("That, you'll have to talk to Craig, my boss.<br/>Not me!");
				break;

			case "shop_misc":
				Msg("For that, you might want to ask Galvin at the Observatory.<br/>He sells some random things.");
				Msg("Oh, one thing. Be careful if you're doing a part-time job for him.<br/>The man changes when he becomes your boss.<br/>He...is not likeable then.");
				break;

			case "shop_grocery":
				Msg("You won't find a grocery store here.<br/>There's a restaurant, though! The big one.<br/>If you want to buy some ingredients,<br/>ask the cute waitress Shena.");
				break;

			case "shop_healing":
				Msg("Healer? Agnes?<br/>Ahhh, she's beautiful too.<br/>A bit of a princess, but...<br/>all's forgiven because she's so pretty!");
				break;

			case "shop_inn":
				Msg("I don't think there's an inn here.<br/>It's weird, if there's a lot of people around, there should be at least one...");
				break;

			case "shop_bank":
				Msg("Bank? Wow, you must be rich.<br/>Hehe, just kidding.<br/>The bank is located right in front of the castle gate.");
				Msg("You know, every time I pass by that place, I get scared like a small kid.<br/>Those guards, you know. Don't their armors look awesome?<br/>Seriously, why can't we get one of those?");
				break;

			case "shop_smith":
				Msg("Blacksmith's Shop? You won't find one here...<br/>instead, there's a Weapons Shop that Osla runs.<br/>It's a little bit west of the town square.");
				break;

			case "skill_range":
				Msg("I heard you can learn that skill simply by holding the bow...<br/>but seriously, it's much easier said than done.<br/>I envy anyone that's good in firing arrows.");
				break;

			case "skill_instrument":
				Msg("Hmm, how about going into town and<br/>looking for a bard?<br/>If you go to the town square, you'll see someone<br/>that's always playing an instrument there.");
				Msg("What was his name...Nele?<br/>I'm sure it's Nele...");
				break;

			case "skill_tailoring":
				Msg("For that, how about asking Ailionoa?<br/>Just remember, Ailionoa has some serious personality...<br/>I don't know if she'll really help you or anything...");
				Msg("But then again, that's what makes a beautiful girl even more attractive...right?");
				break;

			case "skill_magnum_shot":
				Msg("Hmmmph... <username/>...");
				Msg("How do you know that skill?<br/> I've been here longer than you, and I don't know that skill...!<br/>I need to start training harder...");
				break;

			case "skill_counter_attack":
				Msg("Don't you think it's a terrible skill to learn when<br/>the only way you can learn it is by getting hit?");
				Msg("I tried learning that once by getting attacked by a wolf...<br/>I don't even want to think about what happened next...<br/>sigh.");
				break;

			case "skill_smash":
				Msg("The Smash? It's spectacular...<br/>don't you think so?");
				break;

			case "skill_windmill":
				Msg("It's all good except for one thing...<br/>If I am not careful,<br/>I'll be the one dropping down instead of them.");
				break;

			case "square":
				Msg("The Town square?<br/>Go all the way to the center of the town...<br/>and you'll see a fountain there. That's the square.<br/>You'll find the beautiful twin sisters selling flowers there.");
				Msg("They all look so alike it's hard to tell them apart, but their personalities can't be more different!<br/>The worst thing is that every once in a while, one of them will impersonate the other<br/>and there's no way to tell who's who!");
				break;

			case "farmland":
				Msg("There's a lot right around the outer skirts of the town.<br/>It's mostly a cornfield, so it's the perfect place to hide during the summer.<br/>Why would I hide in there? Well that's because...");
				Msg("Hmmm hmmph! I've never skipped any training sessions before, okay?!!");
				break;

			case "shop_headman":
				Msg("<username/>... This is Emain Macha...<br/>not some farmland in the middle of nowhere.");
				break;

			case "temple":
				Msg("The Church... You mean the cathedral down south, right?<br/>There's a Priest there named James... I randomly saw him at Bean Rua the other night.<br/>So even though he looks innocent, he was a beast at the poker table...");
				Msg("Bwahahaha! You didn't hear that from me, though!<br/>If Craig ever finds out I went to Bean Rua, I'm toast! So don't tell him, okay?");
				Msg("So that's that, and...<br/>I was surprised Lucas is so close with Priest James that they played poker together<br/>almost every night! I really didn't see that one coming...");
				Msg("But then again, with Priest James, he seems friendly enough<br/>to be friends with everyone.");
				break;

			case "school":
				Msg("I'm talking about our training ground.<br/>I heard this place used to be a school for magic.<br/>I don't know why it closed down, but...");
				Msg("Come to think of it, I heard there was a statue here, too...<br/>But now that it's gone without a trace... Oh well, I guess that's life.");
				break;

			case "shop_restaurant":
				Msg("Have you tried eating there?<br/>I went there the other day and<br/>seriously, I had to say \"Oh My Lymilark\" multiple times.");
				Msg("The Chef looked really rough, so<br/>I wasn't sure how it'd taste, but...<br/>seriously, never judge a book by its cover.");
				break;

			case "shop_armory":
				Msg("Osla at the Weapons Shop is quite dependable, despite how she looks.<br/>Every once in a while, you'll have to double-check her calculations, though...");
				break;

			case "shop_cloth":
				Msg("Hey, <username/>...<br/>what do you think about Ailionoa from the Clothing Shop?<br/>Tell me, tell me,<br/>she's beautiful, right?");
				Msg("The thing that scares me is that...<br/>she seems to be looking for Simon or Seemon or whatever...<br/>He's not her boyfriend, is he?");
				break;

			case "shop_bookstore":
				Msg("Until last year, there was this person called Buchanan<br/>that used to drop by pretty often... But I guess he's not coming by anymore.<br/>A while ago, this one book had so much demand, that<br/>he kept printing them until he decided to just go straight to the headquarters.");
				Msg("Seriously, who orders so many of that book?");
				break;

			case "skill_fishing":
				Msg("Fishing?<br/>Hey, you have that much time on your hands?<br/>I get so tired whenever I come back from training that<br/>I can't think of anything else.");
				break;

			case "tir_na_nog":
				Msg("Isn't that thing from the ledgends?<br/>I don't know much about it...");
				break;

			case "mabinogi":
				Msg("I wish I could become an awesome Paladin like Redire,<br/>so I can leave my name in the history of Mabinogi!<br/>Seriously, how awesome does that sound?");
				Msg("...The problem is, it's not possible... *sniff*...");
				break;

			case "nao_blacksuit":
				Msg("Don't you think a lady with the black dress looks quite stylish?<br/>You know, Rua at Bean Rua...");
				Msg("Hmph hmph! I'm not saying that I went in there!");
				Msg("...Hey, you're not going to tell on my boss, are you?<br/>Come on!");
				break;

			case "breast":
				Msg("The chest of Osla...<br/>Are they secretly big?");
				Msg("Oh no, it's no secret.<br/>But its difficult when you hear such a claim.");
				break;

			case "g3_DarkKnight":
				Msg("Eh? Dark Knight?<br/><username/>... Where did you hear this from?<br/>I mean, I'm interested in that field as well...");
				Msg("...I'm just here getting Paladin training<br/>cus I don't know where else to learn...");
				Msg("...Yikes! The leader is looking this way!!");
				break;

			case "EG_C2_Unknown_Continet":
				Msg("I'd love to follow you around with all those adventures, sure,<br/>but I've got to graduate first, buddy.<br/>I don't know when this training's going to end...");
				break;

			case "EG_C2_HowTo_Shipboard":
				Msg("Wow, to sail on a ship! Quite romantic, I must say,<br/>but what would you do if you were to run into pirates or something?<br/>...That's right, you should turn yourself into a Paladin first, right?");
				break;

			default:
				RndFavorMsg(
					"Psssh, I had no idea...",
					"Why do you keep asking me something I don't know...",
					"I've been here longer, so don't look at me like that!",
					"Well, what would I know...",
					"I don't know..."
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
					"Haha, nice!",
					"Is this for me? Thanks!",
					"Thank you for this!"
				);
				break;
		}
	}
}
