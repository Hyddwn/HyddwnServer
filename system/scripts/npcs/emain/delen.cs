//--- Aura Script -----------------------------------------------------------
// Delen
//--- Description -----------------------------------------------------------
// Florist, younger sister of Del.
//---------------------------------------------------------------------------

public class DelenScript : NpcScript
{
	public override void Load()
	{
		SetRace(10001);
		SetName("_delen");
		SetBody(height: 0.8f, upper: 1.1f);
		SetFace(skinColor: 16, eyeType: 3, eyeColor: 113);
		SetStand("human/female/anim/female_natural_stand_npc_01");
		SetLocation(52, 41198, 38978, 94);
		SetGiftWeights(beauty: 2, individuality: -1, luxury: 2, toughness: -1, utility: 0, rarity: 2, meaning: -1, adult: 0, maniac: 2, anime: 2, sexy: 1);

		EquipItem(Pocket.Face, 3900, 0x00018E77, 0x00F7A154, 0x0087582F);
		EquipItem(Pocket.Hair, 3031, 0x00FAA974, 0x00FAA974, 0x00FAA974);
		EquipItem(Pocket.Armor, 15082, 0x00FA8D8D, 0x00F7B4AC, 0x00202020);
		EquipItem(Pocket.Shoe, 17040, 0x005E2922, 0x0041002C, 0x00683D0D);

		AddPhrase("Hi there! Check out these flowers!");
		AddPhrase("Am I doing something wrong...?");
		AddPhrase("Pretty flowers! Pick all you want!");
		AddPhrase("Need flowers?");
		AddPhrase("These flowers smell so good!");
		AddPhrase("Surprise your beloved with these beautiful flowers.");
		AddPhrase("Fresh flowers!");
		AddPhrase("Flowers are a must if you're wooing a lady!");
		AddPhrase("Look at these flowers! They are gorgeous!");
		AddPhrase("What kind of flowers are you looking for?");
	}

	protected override async Task Talk()
	{
		SetBgm("NPC_Del.mp3");

		await Intro(L("With a basket of flowers in one hand, she wears a constant smile on her face.<br/>Wearing a long apron and lace-trimmed dress, she looks like a beautiful flower.<br/>Her long shiny hair is tied over to one side, gently touching her bare shoulder.<br/>As soon as our eyes meet, she gives me a friendly smile and starts a conversation."));

		Msg("Do you like flowers?<br/>I have a bunch!", Button("Start a Conversation", "@talk"), Button("Shop", "@shop"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());

				if (Player.IsUsingTitle(11001))
				{
					Msg("...If you try talking to Del with that topic, she'll try to convince you to buy flowers from her.");
					Msg("...I know, because she's just predictable like that.");
				}
				else if (Player.IsUsingTitle(11002))
				{
					Msg("Hey. You're <username/>,<br/>the one who asked me about Goddess Macha recently, right?<br/>Something about you looks different today...");
					Msg("Do you have something else to say?");
					Msg("...Haha, look at how sad you just got.<br/>I know, I know. You wanted to brag<br/>about becoming the Guardian of Erinn. Hehe...");
					Msg("...Thank you. <username/>.<br/>It's because of people like you<br/>that someone like me<br/>can live in peace without any fear. Hehe.");
				}

				await Conversation();
				break;

			case "@shop":
				Msg("Do you like...flowers?");
				OpenShop("DelenShop");
				return;
		}

		End("Thank you, <npcname/>. I'll see you later!");
	}

	private void Greet()
	{
		if (DoingPtjForNpc())
		{
			Msg(FavorExpression(), L("(Missing Dialogue, talking to npc during PTJ.)"));
		}
		else if (Memory <= 0)
		{
			Msg(FavorExpression(), L("Can I help you with something?"));
		}
		else if (Memory == 1)
		{
			Msg(FavorExpression(), L("<username/>... <username/>, right?"));
		}
		else if (Memory == 2)
		{
			Msg(FavorExpression(), L("It's you again, <username/>. What brings you here today?"));
		}
		else if (Memory <= 6)
		{
			Msg(FavorExpression(), L("You must really love flowers, <username/>.<br/>You can buy as many as you'd like!"));
		}
		else
		{
			Msg(FavorExpression(), L("<username/>. Everyone in town has been talking about how close we are. Did you know that?"));
		}

		UpdateRelationAfterGreet();
	}

	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				Msg(FavorExpression(), "Grrr. All Del does is nag, nag, nag.<br/>Just because she's older by a couple of minutes...<br/>Always trying to act like<br/>she knows so much more than me, like she's sooo much older than me.");
				Msg("...I don't think she even understands<br/>half the things she says...");
				ModifyRelation(Random(2), 0, Random(3));
				break;

			case "rumor":
				Msg(FavorExpression(), "You know... the Captain of the Guards at the Lord's castle... Aodhan?<br/>Isn't he sooo handome...?<br/>...He must be really skilled to have gotten that position at such a young age...<br/>He also has good manners, good character...");
				Msg("...Probably has some savings...<br/>Hehe! What? Don't look at me like that!");
				ModifyRelation(Random(2), 0, Random(3));
				break;

			case "shop_misc":
				Msg("What do you need from the General Shop?<br/>You should be able to find almost anything you need<br/>from Galvin at the Observatory!");
				Msg("...Are you planning to<br/>buy me a present or something? Hehe...");
				break;

			case "shop_grocery":
				Msg("Grocery Store...?<br/>That's where they sell food, right?");
				Msg("Well, let me see...");
				Msg("...So you want to buy some ingredients<br/>and not already cooked meals, right...?<br/>...So you must know how to use the Cooking skill well then?");
				Msg("...Do you think you can make me some snacks?<br/>I love eating sweets...");
				break;

			case "shop_healing":
				Msg("Are you looking for a Healer?<br/>...Did you get hurt?<br/>You look fine to me!");
				Msg("...Maybe you got hurt somewhere where I can't see...? *Giggle*<br/>Anyways, you can find the Healer's House<br/>if you turn right at the first corner.");
				Msg("Look for Agnes there.<br/>Oh, she really loves sweets like cakes and such,<br/>so if you take her some, you'll score some points! Hehe...");
				break;

			case "shop_bank":
				Msg("Wow! Do you have a lot of things to deposit?<br/>You must be rich, <username/>!");
				Msg("...Well, you don't really look it...<br/>but looks can be deceiving, I guess...<br/>Just head west on the road all the way down, and you will see the Bank.<br/>Yes, it's towards the castle.");
				Msg("Go talk to Jocelin there.<br/>She is very insightful with many things,<br/>not just about Bank stuff.");
				break;

			case "shop_smith":
				Msg("If you're looking for the Blacksmith's Shop,<br/>I'm assuming you have a sword or something to repair...?<br/>I cut myself a while back, while cutting fruit,<br/>and I've been afraid to touch any knives since then...");
				Msg("...Better safe than sorry, right...?");
				break;

			case "skill_instrument":
				Msg("Hmm... Have you met Nele?<br/>Um... He's that guy over there playing the instrument...<br/>I mean, a street musician<br/>is still a musician...");
				Msg("If you are interested,<br/>you should go over there<br/>and talk to him.");
				break;

			case "skill_tailoring":
				Msg("The Weaving skill...<br/>Well, as far as I know,<br/>Ailionoa in the Clothing Shop is the best at that!");
				Msg("Not only does she make clothes,<br/>but she also makes hats, gloves, etc.<br/>I don't know how she does it...");
				Msg("...I would love to buy one of her clothes,<br/>but her clothes<br/>have a big price tag.");
				Msg("Ahhh...<br/>I really want one of her dresses...");
				break;

			case "skill_magnum_shot":
				Msg("How could you ask such a question to a lady like me?<br/>Don't you think it's barbaric?");
				Msg("...<username/>, you must be a very aggressive person.");
				break;

			case "skill_counter_attack":
				Msg("My, why would you ask a frail lady a question like that...?<br/>Why don't you ask Aodhan by the castle...");
				Msg("Hehe, by the way...<br/>Don't you think Aodhan is so handsome?<br/>I think he likes it when I visit him...<br/>But I'm not sure whether he likes me or Del.");
				Msg("...I hope it's me and not Del...<br/> I would love to find out who Aodhan likes...<br/>Me or Del...");
				break;

			case "skill_smash":
				Msg("Del is stronger than she looks...<br/>The the last time she'd hit me,<br/>I thought she used the Smash attack on me.");
				Msg("...She gave me a big fat bruise on my shoulder...<br/>She acts like she's my \"older\" sister,<br/>when she's only older than me by a few minutes....");
				Msg("...You wouldn't understand unless you had a twin sibling...");
				Msg("I lose out on so much because of her...<br/>Gosh, I just can't stand her sometimes.");
				break;

			case "skill_gathering":
				Msg("Every flower here is home grown from our garden.<br/>Although natural flowers in the fields are beautiful.<br/>It can't beat the beauty<br/>of home grown flowers that are grown to perfection...");
				Msg("What do you think?<br/>If you want to look at them, just open the Shop window and take a look.");
				break;

			case "square":
				Msg("Yep. This is the Square...<br/>Were you supposed to meet someone here...?");
				break;

			case "farmland":
				Msg("You just reminded me of something...<br/>Do you know what my dream is...?");
				Msg("I'm only telling because it's you, <username/>.<br/>...I dream of one day having a farm full of flowers.");
				Msg("...Just imaging a field full of daisies, roses, and freesias.<br/>There would also be tulips near the water fountain...<br/>I would choose the prettiest flowers and make them into beautiful bouquets...");
				Msg("...Sigh...<br/>Huh? When am I going to make enough money for that? Haha...");
				Msg("You must be joking, right?<br/>Maybe if I meet a good man...");
				break;

			case "temple":
				Msg("To tell you the truth...<br/>I don't see the point of having a Church.<br/>To tell you the truth, I don't know why people go to church, either.<br/>Don't you think fate is something you create<br/>as you make your own choices in life?");
				Msg("...Although sometimes a good man could just come along and sweep you off of your feet.");
				break;

			case "school":
				Msg("Well...<br/>I haven't been to a place like that so I wouldn't know.");
				break;

			case "shop_restaurant":
				Msg("Are you looking for a Restaurant?<br/>There's one all the way west. It's called Loch Lios...<br/>It's the city's pride and joy!");
				Msg("...The food there is amazing. It's a bit pricey, but definitely worth it...");
				Msg("If you have some money, you should definitely check it out.<br/>...You can always take me out with you. Hehe...");
				break;

			case "shop_armory":
				Msg("Are you looking for Osla?");
				Msg("...Oh, she is the owner of the Weapons Shop.<br/>She seems to daydream a lot...");
				Msg("...She thinks that's attractive or something.<br/>Maybe she thinks that guys will find her sophisticated...<br/>I don't think any guy will think that... Haha...");
				Msg("...Guys like girls<br/>who are organized and frugal.");
				break;

			case "shop_cloth":
				Msg("If you are looking for a dress,<br/>visit Ailionoa's Clothing Shop.<br/>The name of the shop is Tre'imhse Cairde...");
				break;

			case "graveyard":
				Msg("I don't go to creepy places like that!");
				break;

			case "lute":
				Msg("A Lute?<br/>I always hear Galvin bragging about how<br/>the Lute from Emain Macha is the best.");
				Msg("...But then again, judging by the things Galvin typically says,<br/>I don't know how much you can trust him.<br/>I'm not saying he's a bad person,<br/>it's just that he's not someone I would trust...");
				break;

			case "complicity":
				Msg("We need some help,<br/>but it's hard to find someone reliable...");
				Msg("...I can't trust Galvin,<br/>Fraser is too clumsy,<br/>Priest James is too serious...<br/>Lucas is too creepy...");
				Msg("Nele is not my type...<br/>...Well, I guess the only person left is Aodhan!");
				Msg("...I wonder if he'll get mad...");
				break;

			case "tir_na_nog":
				Msg("Del believes in Tir Na Nog.<br/>But I beg to differ.");
				Msg("If there was such a place,<br/>there should be at least one person who's been there,<br/>but I haven't heard of anyone who's been to Tir Na Nog, ever.<br/>At least, not yet.");
				Msg("...Ah. Of course there are some people<br/>who \"claim\" to have been there,<br/>but I think they are lying...");
				break;

			case "mabinogi":
				Msg("It sounds like a bedtime story,<br/>doesn't it?<br/>I remember how our grandmother used to<br/>tell us that story when we were young.");
				Msg("...Del and I still love that story...<br/>The story is a mabinogi... right?");
				Msg("...Our grandma passed away a while ago<br/>and now she's buried at the Graveyard over there.");
				break;

			case "breast":
				Msg("Chest...<br/>Chest...<br/>If it's on a guy, would it be better for the chest to be wide?<br/>Girls are good!");
				break;

			case "EG_C2_Unknown_Continet":
				Msg("Are you saying that there is an unknown uncharted territory out there somewhere?<br/>Haha, that sounds very romantic.<br/>The only thing is that it doesn't sound so realistic or probable...");
				break;

			default:
				RndFavorMsg(
					"Haha... You really don't even know that?<br/>I'm disappointed, <username/>.",
					"...Is that supposed to be funny?",
					"Come on<br/>Can you be more considerate of the person you're talking to?<br/>Is that so hard?",
					"...<username/>, is that interesting to you?",
					"You really don't mince your words, do you...<br/>especially compared to other travlers I've met...",
					"Ugh, never mind.",
					"...Haha!<br/>Can we just skip that?",
					"Maybe you should ask me after you buy some flowers...",
					"I didn't think you were like that, <username/>...<br/>You are impossible to talk to.",
					"...Can you stop using such difficult words?",
					"Haha... you don't even know that, <username/>?<br/>I'm disappointed! La, la, la..."
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
					"Wow, a gift!<br/>You just made my day.<br/>Thanks, <username/>.",
					"A gift? For me? Woohoo!<br/><username/>...<br/>Do you want to become a member of my fan club? Hehe..",
					"Well, this isn't bad...<br/>there seemed to be a lot of great, new items at the shops...<br/>Hehe... I'm just saying."
				);
				break;
		}
	}
}

public class DelenShop : NpcShopScript
{
	public override void Setup()
	{
		OnErinnMidnightTick(ErinnTime.Now);
	}

	protected override void OnErinnMidnightTick(ErinnTime time)
	{
		// Run base (color randomization)
		base.OnErinnMidnightTick(time);

		ClearTab(L("Flowers"));
		ClearTab(L("Bouquet of Flowers"));

		Add(L("Flowers"), 40046); // Single Rose
		Add(L("Flowers"), 40046); // Single Rose
		Add(L("Flowers"), 40046); // Single Rose
		Add(L("Flowers"), 40046); // Single Rose
		Add(L("Flowers"), 40046); // Single Rose
		if (time.Month == ErinnMonth.Samhain) // Saturday
			Add(L("Flowers"), 40052); // Single Blue Rose

		Add(L("Bouquet of Flowers"), 40047); // Rose Bouquet
		Add(L("Bouquet of Flowers"), 40047); // Rose Bouquet
		Add(L("Bouquet of Flowers"), 40047); // Rose Bouquet
		Add(L("Bouquet of Flowers"), 40047); // Rose Bouquet
		Add(L("Bouquet of Flowers"), 40047); // Rose Bouquet
		if (time.Month == ErinnMonth.AlbanHeruin) // Wednesday
			Add(L("Bouquet of Flowers"), 40054); // Blue Rose Bouquet
	}
}
