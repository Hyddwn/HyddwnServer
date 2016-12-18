//--- Aura Script -----------------------------------------------------------
// Bebhinn
//--- Description -----------------------------------------------------------
// Bank Manager
//---------------------------------------------------------------------------

public class BebhinnScript : NpcScript
{
	public override void Load()
	{
		SetRace(10001);
		SetName("_bebhinn");
		SetFace(skinColor: 27, eyeType: 59, eyeColor: 55, mouthType: 1);
		SetStand("human/female/anim/female_natural_stand_npc_Bebhinn");
		SetLocation(2, 1364, 1785, 228);
		SetGiftWeights(beauty: 2, individuality: 1, luxury: 2, toughness: 0, utility: -1, rarity: 0, meaning: 0, adult: 0, maniac: 0, anime: 1, sexy: 0);

		EquipItem(Pocket.Face, 3900, 0xF78042);
		EquipItem(Pocket.Hair, 3100, 0x201C1A);
		EquipItem(Pocket.Armor, 90106, 0xFFE4BF, 0x1E649D, 0x175884);
		EquipItem(Pocket.Shoe, 17040, 0x996633, 0x6175AD, 0x808080);

		AddPhrase("Any city would be better than here, right?");
		AddPhrase("I prefer rainy days over clear days.");
		AddPhrase("It's soooo boring.");
		AddPhrase("No matter what, I am going hiking this weekend.");
		AddPhrase("Should I move out to the city?");
		AddPhrase("So many good-looking men stopped by today...");
		AddPhrase("There's nothing worse than a man who makes a woman wait.");
		AddPhrase("Where would be a good place to spend my vacation?");
		AddPhrase("Wow... I'm so pretty... Hehe.");
	}

	protected override async Task Talk()
	{
		SetBgm("NPC_Bebhinn.mp3");

		await Intro(L("A young lady is admiring her nails as you enter.<br/>When she notices you, she looks up expectantly, as if waiting for you to liven things up.<br/>Her big, blue eyes sparkle with charm and fun, and her subtle smile creates irresistable dimples."));

		Msg("May I help you?", Button("Start Conversation", "@talk"), Button("Open My Account", "@bank"), Button("Redeem Coupon", "@redeem"), Button("Shop", "@shop"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());

				if (Title == 11001)
					Msg("Oh? You rescued the Goddess, <username/>? How amazing!<br/>So, was she beautiful? Not prettier than me, right? Haha.<br/>Otherwise, why would you leave the Goddess of Tir Na Nog<br/>to come here and tell ME about it? Hehe!");
				else if (Title == 11002)
					Msg("What? <username/>, you're the Guardian of Erinn?<br/>I don't get it!<br/>I've worked so hard all my life, and you just walk in and you're already a Guardian of Erinn...<br/>It's so unfair!<p>Hey, I'm just teasing. Don't get all upset.");

				await Conversation();
				break;

			case "@bank":
				OpenBank("TirChonaillBank");
				return;

			case "@redeem":
				Msg("Are you here to redeem your coupon?<br/>Please enter the coupon number you wish to redeem.", Input("Exchange Coupon", "Enter your coupon number"));
				var input = await Select();

				if (input == "@cancel")
					return;

				if (!RedeemCoupon(input))
				{
					Msg("I checked the number at our Head Office, and they say this coupon does not exist.<br/>Please double check the coupon number.");
				}
				else
				{
					// Unofficial response.
					Msg("There you go, have a nice day.");
				}
				break;

			case "@shop":
				Msg("So, does that mean you're looking for a Personal Shop License then?<br/>You must have something you want to sell around here!<br/>Hahaha...");
				OpenShop("BebhinnShop");
				return;
		}

		End();
	}

	private void Greet()
	{
		if (Memory <= 0)
		{
			Msg(FavorExpression(), L("Is this your first time here? Nice to meet you."));
		}
		else if (Memory == 1)
		{
			Msg(FavorExpression(), L("I think we've met before... nice to see you again."));
		}
		else if (Memory == 2)
		{
			RndFavorMsg(
				L("<username/>, right? Good to see you again."),
				L("I have some bad news...<br/>A strange man came here<br/>and took all the items and money from your account!<p>Juuuuust kidding, haha! That would definitely be bad, huh?")
			);
		}
		else if (Memory <= 6)
		{
			Msg(FavorExpression(), L("Nice to meet you again, <username/>."));
		}
		else
		{
			Msg(FavorExpression(), L("It seems we meet quite often, <username/>."));
		}

		UpdateRelationAfterGreet();
	}

	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				GiveKeyword("shop_bank");
				if (Memory == 1)
				{
					Msg(FavorExpression(), "My name is <npcname/>. Don't forget it!");
					ModifyRelation(1, 0, 0);
				}
				else if (Memory >= 15 && Favor >= 50 && Stress <= 5)
				{
					Msg(FavorExpression(), "Malcolm is so adorable sometimes...<br/>What do you think, <username/>? Isn't he cute?");
					ModifyRelation(Random(2), 0, Random(2));
				}
				else if (Memory >= 15 && Favor >= 30 && Stress <= 5)
				{
					Msg(FavorExpression(), "Boys are so clueless and simple sometimes.<br/>You just give them a nice smile and they start growing a huge ego.<br/>Malcom and a few old folks might be the only exceptions, haha.");
					ModifyRelation(Random(2), 0, Random(2));
				}
				else if (Favor >= 10 && Stress <= 10)
				{
					Msg(FavorExpression(), "Being a small town, this place wasn't very busy before.<br/>But I've gotten more customers lately.<br/>The problem is that the road has been obstructed somewhere.<br/>I'm having a hard time contacting the main headquarters.");
					Msg("I feel bad for inconveniencing the customers...<br/>Hopefully, it will work out.");
					ModifyRelation(Random(2), Random(2), Random(2));
				}
				else if (Favor <= -10)
				{
					Msg(FavorExpression(), "On the day this bank opened, the bank president, Erskin, came by.<br/>He said he was looking for a clerk to work at the new bank.<br/>I applied for the job and was hired on the spot.<br/>I was hoping to go out to the city... But so it goes...");
					ModifyRelation(Random(2), 0, Random(1, 3));
				}
				else if (Favor <= -30 && Stress <= 10)
				{
					Msg(FavorExpression(), "It's important to have a good memory when you work at a bank,<br/>not to mention that I must always maintain good relationships<br/>with the people in town.");
					ModifyRelation(Random(2), 0, Random(1, 3));
				}
				else if (Favor <= -30 && Stress > 10)
				{
					Msg(FavorExpression(), "Why do you want to know so much about me?");
					ModifyRelation(Random(2), -Random(2), Random(1, 4));
				}
				else
				{
					RndFavorMsg(
						"Allow me to introduce myself. I'm the head teller of the Erskin Bank,<br/>but you can just call me <npcname/>.",
						"Excuse me!<br/>Did an item just disappear from your Inventory?<br/>Please press <hotkey name='InventoryView'/> and check.<p>Did you really press it? Hahaha... So gullible!"
					);
					ModifyRelation(Random(2), 0, Random(3));
				}
				break;

			case "rumor":
				if (Memory >= 15 && Favor >= 50 && Stress <= 5)
				{
					Msg(FavorExpression(), "Hmm... Did you know? Malcolm has this big crush on Nora.<br/>He even sent her a love letter.<br/>I secretly read it...<br/>And he definitely needs to learn how to write a real love letter...hehe...");
					ModifyRelation(Random(2), 0, Random(2));
				}
				else if (Memory >= 15 && Favor >= 30 && Stress <= 5)
				{
					Msg(FavorExpression(), "It seems like Ranald has feelings for Priestess Endelyon.<br/>Did you notice that, <username/>?<br/>Strange how people pursue love that has no hope...<br/>I know this is mean, but people need to recognize their limits...");
					ModifyRelation(Random(2), 0, Random(2));
				}
				else if (Favor >= 10 && Stress <= 10)
				{
					Msg(FavorExpression(), "Dilys is quite a beauty.<br/>I don't know how she ended up here, working as a healer.<br/>If I were her, I would go out to a big city, party, dance, date, and all that. What a life!<br/>Hey, don't look at me like that. I am just saying that because I'm jealous!");
					ModifyRelation(Random(2), Random(2), Random(2));
				}
				else if (Favor <= -10)
				{
					Msg(FavorExpression(), "Something's strange about Piaras.<br/>They say it's because he traveled a lot and experienced many things.<br/>But the way he talks and acts... Does that really stem from having more experiences?");
					ModifyRelation(Random(2), 0, Random(1, 3));
				}
				else if (Favor <= -30 && Stress <= 10)
				{
					Msg(FavorExpression(), "A lot of people have been complaining about the Bank not working properly lately.<br/>Well, it's not my fault!<br/>I haven't stolen any money, geez!<br/>Hey, <username/>. Don't tell me you think I'm doing something illegal?");
					ModifyRelation(Random(2), 0, Random(1, 3));
				}
				else if (Favor <= -30 && Stress > 10)
				{
					Msg(FavorExpression(), "<username/>,<br/>I think you may love gossiping more than I do.");
					ModifyRelation(Random(2), -Random(2), Random(1, 4));
				}
				else
				{
					GiveKeyword("farmland");
					Msg(FavorExpression(), "Oh, you know what?<br/>Some people were hitting the scarecrow at the School to practice their skills,<br/>and they wandered off and ruined the crops in the farmland.<br/>The owner got pretty upset about it.");
					ModifyRelation(Random(2), 0, Random(3));
				}
				break;

			case "about_skill":
				if (Player.Skills.Has(SkillId.Composing))
				{
					Msg("Wow! You know about the Composition skill?<br/>Can you write me a song someday? Hehe...");
				}
				else if (Player.Skills.Has(SkillId.PlayingInstrument))
				{
					GiveKeyword("skill_composing");
					Msg("Haha. You do work hard at everything! Impressive.<br/>You seem to love music. Do you know anything about the Composition skill?<br/>I think that skill allows you to create your own tunes<br/>if you're tired of the songs at the General Shop.");
					Msg("I know Priestess Endelyon knows a lot about it.<br/>Try talking to her with this keyword.");
				}
				else
				{
					Msg("Hmm...There's a skill you might be interested in,<br/>but I don't think you can handle it yet.<br/>Can you come back after you learn how to play a musical instrument?");
				}
				break;

			case "shop_misc":
				Msg("Are you looking for the General Shop? It's just across the road.<br/>You know who's there, right? Just ask for Malcolm.");
				break;

			case "shop_grocery":
				GiveKeyword("skill_gathering");
				Msg("The Grocery Store is next to the Bank.<br/>There's a big chef sign next to it.<br/>And chat with Caitin while you're there.");
				Msg("Her food is fresh because she uses ingredients harvested directly<br/>from the farm next to the shop.");
				break;

			case "shop_healing":
				Msg("Isn't Dilys pretty? I heard she studied in Emain Macha,<br/>but came all the way back here to work as a healer.<br/>If it had been either Nora or me, we would have stayed in the city forever. Haha...");
				break;

			case "shop_inn":
				Msg("Hmm... I would rate the Inn here...mmm 1 star? Is that too harsh?");
				break;

			case "shop_bank":
				Msg("Do you have something for me?<br/>The Bank charges you for making deposits,<br/>but I accept things for free!<br/>...<br/>However, I never give them back...");
				break;

			case "shop_smith":
				Msg("If you have anything that's broken and needs to be repaired, get it fixed at the General Shop or the Blacksmith's Shop<br/>before depositing it at the Bank.<br/>Once it starts to rust, there's just no stopping it!");
				Msg("Oh, and send my regards to Ferghus if you go to the Blacksmith's Shop.");
				break;

			case "skill_range":
				Msg("I don't know but it sounds like a combat skill for warriors...<br/>For things like that, it will be best to ask Ranald<br/>at the School.");
				break;

			case "skill_instrument":
				GiveKeyword("temple");
				Msg("The shepherd boy Deian and Priestess Endelyon<br/>seem to have great knowledge in that field...<br/>Did you talk to them?");
				break;

			case "skill_composing":
				GiveKeyword("temple");
				Msg("Didn't I already tell you that<br/>you can talk to Priestess Endelyon at the Church<br/>about the Composition skill?");
				break;

			case "skill_tailoring":
				GiveKeyword("temple");
				Msg("Tailoring...<br/>Caitin from the Grocery Store<br/>or Endelyon at the Church know about that better than I do...hehe...");
				break;

			case "skill_magnum_shot":
				GiveKeyword("school");
				Msg("I don't know but it sounds like a combat skill for warriors...<br/>For things like that, you should ask Ranald<br/>about it at the School.<br/>Trefor might know about it as well...");
				break;

			case "skill_counter_attack":
				GiveKeyword("school");
				Msg("I don't know but it sounds like a combat skill for warriors...<br/>For things like that, you should ask Ranald<br/>about it at the School.<br/>Trefor might know about it as well...");
				break;

			case "skill_smash":
				GiveKeyword("school");
				Msg("Umm... Wouldn't it be better<br/>to ask Ranald about that?");
				break;

			case "skill_gathering":
				Msg("Gathering?<br/>I wouldn't know anything about that...I grew up never getting my hands dirty...<br/>Hehe...");
				break;

			case "square":
				Msg("The Square is just in front of here.<br/>Walk out and it's right there.<br/>I guess it is a bit small for a square, huh?");
				break;

			case "pool":
				Msg("The reservoir...let me see.<br/>Walk out the door, turn right, and walk down the hill.<br/>But why are you looking for the reservoir?");
				break;

			case "farmland":
				GiveKeyword("skill_gathering");
				Msg("The farmland is located south of here, just past the Church.<br/>There is nothing to see there...<br/>That doesn't mean you can harvest anything you want.<br/>Even boring farms have owners.");
				break;

			case "windmill":
				GiveKeyword("shop_smith");
				GiveKeyword("shop_inn");
				Msg("Windmill?<br/>The Windmill is located near the Blacksmith's Shop and the Inn.<br/>A little kid name Alissa should be there.<br/>Do whatever she says.");
				Msg("You can't go into the milling area without permission.<br/>First, it's a safety hazard.<br/>More importantly, I don't want to eat bread with contaminated substances in it.");
				break;

			case "brook":
				GiveKeyword("shop_smith");
				Msg("Adelia Stream?<br/>You mean the stream flowing in front of the town, right?");
				Msg("The Adelia Stream is always icy cold and crystal clear.<br/>I think it's because it flows from the north. The ice caps, you know...<br/>And there's a bridge that goes over the stream.<br/>I think you can see it near the Blacksmith's Shop.");
				break;

			case "shop_headman":
				GiveKeyword("square");
				Msg("The Chief's House is just over there.<br/>Take the stairs from the Square<br/>and go up the hill.<p/>You know his name is Duncan, right?");
				break;

			case "temple":
				Msg("The Church?<br/>It's downhill from here.<br/>When you go out, go downhill towards the right.<br/>It will come up shortly.<p/>Can you send my regards to Priest Meven<br/>and Priestess Endelyon?");
				break;

			case "school":
				Msg("You're looking for the School?<br/>Go downhill towards the right when you exit.<br/>It's the building that has a big yard on both sides with a weird giant plant and a scarecrow.");
				Msg("It's right next to the Church. Very easy to find.<br/>You can meet Ranald and Lassar at the School.<br/>You can learn a lot from them.");
				break;

			case "shop_restaurant":
				GiveKeyword("shop_grocery");
				Msg("You mean, a restaurant?<br/>If you are looking for something to eat,<br/>you can go see Caitin.<br/>The grocery store sells food ingredients,<br/>as well as homemade food prepared  by Caitin herself.<br/>Why don't you go and ask her?");
				break;

			case "shop_armory":
				GiveKeyword("shop_smith");
				Msg("We don't have a dedicated Weapons Shop,<br/>but I guess you could find some weapons at the Blacksmith's Shop.<br/>Ferghus is the owner of the Blacksmith's Shop,<br/>so tell him what you want<br/>and he might be able to get it for you.");
				break;

			case "shop_cloth":
				GiveKeyword("shop_misc");
				Msg("There are no clothing shops in this town.<br/>Malcolm does sell some clothes though.<br/>If you're looking for something to wear,<br/>just go to the General Shop.<br/>But you might not find anything you like...");
				break;

			case "shop_bookstore":
				Msg("A bookstore?<br/>Well... people around here don't really read that much...");
				break;

			case "shop_goverment_office":
				GiveKeyword("shop_headman");
				Msg("Haha! You're joking, right? We have the Chief's House and that's it.<br/>A town office? In this small town? Please!<br/>Since this town is in the Ulaid region,<br/>we are not governed directly by the king.");
				Msg("Instead, Duncan represents our town.<br/>If you are looking for an elder,<br/>go up the hill to Duncan's House.");
				break;

			case "graveyard":
				GiveKeyword("shop_headman");
				Msg("The graveyard is behind the Chief's House.<br/>Why are you interested in people's graves?<br/>Don't you think that's a bit creepy?");
				break;

			case "skill_fishing":
				Msg("Some people just keep fishing<br/>even when their Inventory is filled to the brim.<br/>You can't fish for very long like that.");
				Msg("You know, I can spot amateurs like that at a glance.<br/>If you want to fish the right way,<br/>you should drop off most of your belongings at the bank<br/>so that your Inventory has lots of room.");
				break;

			case "bow":
				Msg("Bow? You mean the weapon?<br/>They are sold at the Blacksmith's Shop.<br/>You can also find some armor for yourself. Go and check out the shop.<br/>Oh, don't forget to tell Ferghus that <npcname/> says hello.");
				break;

			case "lute":
				GiveKeyword("shop_misc");
				Msg("I saw Malcolm selling a lute.<br/>You should go to the General Shop.");
				break;

			case "complicity":
				GiveKeyword("shop_bank");
				Msg("Well...<br/>I could use some help...<br/>But I don't think I can afford to pay another employee...<br/>Come on, don't look at me like that. Do you know how hard it is to make a living these days...?");
				break;

			case "tir_na_nog":
				Msg("Tir Na Nog? The legendary paradise?<br/>Well, it sounds like a romantic story...<br/>But I don't think fairy tales can help you.<br/>Get your head out of the clouds and come back to reality...<br/>Ask about things like items, Gold, skills...you know.");
				break;

			case "musicsheet":
				GiveKeyword("shop_misc");
				Msg("A Music Score?<br/>Hmm... Malcolm sells some at his shop...");
				Msg("But they're pretty expensive.<br/>Especially compared to the price of an instrument... Don't you think?");
				break;

			default:
				if (Memory >= 15 && Favor >= 30 && Stress <= 5)
				{
					Msg(FavorExpression(), "Haha, I'm not usually interested in such things. But you make it sound so intriguing...");
					ModifyRelation(0, 0, Random(2));
				}
				else if (Favor >= 10 && Stress <= 10)
				{
					Msg(FavorExpression(), "Interesting story, but I don't really care. Sorry.");
					ModifyRelation(0, 0, Random(2));
				}
				else if (Favor <= -10)
				{
					Msg(FavorExpression(), "You shouldn't stop by so often...");
					ModifyRelation(0, 0, Random(4));
				}
				else if (Favor <= -30)
				{
					Msg(FavorExpression(), "*yawn* Boring...");
					ModifyRelation(0, 0, Random(5));
				}
				else
				{
					RndFavorMsg(
						"What's that?",
						"Well...what do you mean?",
						"Can we change the subject?",
						"I don't know anything about that.",
						"Hehe... I don't know what you're talking about...",
						"I have no idea... Why don't you ask someone else?",
						"Hmm... You know a story I've never heard of... How could that be?",
						"A lot of people have asked me that...I didn't know it was that important!",
						"I said I don't know! Why do you keep rubbing it in my face? That's mean... Hehe.",
						"Where did you hear that from? It looks like you have better sources than I do..."
					);
					ModifyRelation(0, 0, Random(3));
				}
				break;
		}
	}

	protected override async Task Gift(Item item, GiftReaction reaction)
	{
		switch (reaction)
		{
			case GiftReaction.Love:
				RndMsg(
					L("This is it! Thank you!"),
					L("This is so wonderful. Thank you!"),
					L("I don't know how to thank you enough. Thank you."),
					L("I don't know what else to say! Thank you so much for this wonderful gift...")
				);
				break;

			case GiftReaction.Like:
				RndMsg(
					L("I love it!"),
					L("How did you know I like this?"),
					L("Wow! This is exactly what I wanted."),
					L("Wow, unbelievable! Thank you! I am so happy.")
				);
				break;

			case GiftReaction.Neutral:
				RndMsg(
					L("Thank you again..."),
					L("Thank you for this great gift."),
					L("You are so kind. A gift for me..."),
					L("It's been a while since I received a gift. Thank you!")
				);
				break;

			case GiftReaction.Dislike:
				RndMsg(
					L("Ah, this is quite awkward."),
					L("Why are you giving me this?"),
					L("Hmm... I don't know what to do with this..."),
					L("I will keep it because you gave it to me, but this is just... Hehe...")
				);
				break;
		}
	}
}

public class BebhinnShop : NpcShopScript
{
	public override void Setup()
	{
		if (IsEnabled("PersonalShop"))
		{
			Add("License", 60101); // Tir Chonaill Merchant License
			Add("License", 81010); // Purple Personal Shop Brownie Work-For-Hire Contract
			Add("License", 81011); // Pink Personal Shop Brownie Work-For-Hire Contract
			Add("License", 81012); // Green Personal Shop Brownie Work-For-Hire Contract
		}
	}
}