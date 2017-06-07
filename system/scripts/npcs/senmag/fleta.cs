//--- Aura Script -----------------------------------------------------------
// Fleta
//--- Description -----------------------------------------------------------
// Traveling Clothes Merchant in Sen Mag. Offers magical clothing repairs.
//---------------------------------------------------------------------------

public class FletaScript : NpcScript
{
	public override void Load()
	{
		SetRace(10001);
		SetName("_fleta");
		SetBody(height: 0.1f, weight: 1.06f, upper: 1.09f);
		SetFace(skinColor: 15, eyeType: 8, eyeColor: 155, mouthType: 2);
		if (IsEnabled("Fleta"))
		{
			if (ErinnHour(9, 11) || ErinnHour(15, 17) || ErinnHour(19, 21))
				SetLocation(53, 103364, 109290, 0);
			else
				SetLocation(22, 6500, 4800, 0);
		}
		SetGiftWeights(beauty: 2, individuality: 2, luxury: 2, toughness: -1, utility: 0, rarity: 2, meaning: -1, adult: -1, maniac: 0, anime: 0, sexy: 0);
		SetAi("npc_fleta");

		EquipItem(Pocket.Face, 3900, 0x0000738C, 0x00F49D32, 0x00CD7287);
		EquipItem(Pocket.Hair, 3004, 0xFFBC8B63, 0xFFBC8B63, 0xFFBC8B63);
		EquipItem(Pocket.Armor, 15078, 0xFF301D16, 0xFF1B100E, 0xFF6D5034);
		EquipItem(Pocket.Shoe, 17007, 0x00151515, 0x00FFFFFF, 0x00FFFFFF);


		AddPhrase("Meow.");
		AddPhrase("Chirp Chirp.");
		AddPhrase("Should I go play somewhere else?");
		AddPhrase("Are you scared?");
		AddPhrase("Roar.");
		AddPhrase("...Ah, I'm bored.");
		AddPhrase("I love hiking.");
		AddPhrase("...Hehe");
		AddPhrase("La la la.");
		AddPhrase("Should I go play somewhere else...");
	}

	// Officially, Fleta is hidden from the player instead of warped
	[On("ErinnTimeTick")]
	public void OnErinnTimeTick(ErinnTime time)
	{
		if ((ErinnHour(9, 11)) || (ErinnHour(15, 17)) || (ErinnHour(19, 21)))
		{
			if (NPC.RegionId != 53)
				NPC.WarpFlash(53, 103364, 109290);
		}
		else if (NPC.RegionId != 22)
		{
			NPC.WarpFlash(22, 6500, 4800);
		}
	}

	protected override async Task Talk()
	{
		SetBgm("NPC_Fleta.mp3");

		await Intro(L("A petite girl wearing a brown dress, with straight blonde hair flowing down.<br/>There is not a hint of smile on her small lips,<br/>and her big, deep eyes stare straight this way without even a slight hesitation."));

		if ((Player.HasItem(70076)) && (!Player.HasItem(52044)) && (!Player.HasKeyword("errand_hiddenA")))
		{
			GiveKeyword("errand_of_fleta");
			GiveKeyword("errand_hiddenA");
			Player.RemoveItem(70076); // Empty Treasure Chest
			Msg("This is...");
			Msg(Hide.Name, "(Fleta took all the empty treasure chests.)");
			Msg("You opened it, didn't you?<br/>Thanks to you, they all ran away.");
			Msg("...It was already opened?<br/>Are you sure?");
			Msg("...I swear, he can never<br/>make a decent item.");
			Msg("Well, oh well.<br/>Since you brought an already opened one,<br/>it's partially your fault too.");
			Msg("You'll need to bring me<br/>spices that Imps like,<br/>herbs grown by angels,");
			Msg("and flowers that glow in the dark<br/>to fill this chest again.<br/>I need to prepare.");
			Msg("Can you bring me those?<br/>You just need to go to the closest city from here.<br/>You know, the place with a fountain and a castle.<br/>Go there when you have time. I'll be waiting.");
			Msg("Do you need anything else...?", Button("End conversation", "@exit"), Button("Start a Conversation", "@talk"), Button("Shop", "@shop"), Button("Repair item", "@repair"));
		}
		else if ((Player.HasKeyword("errand_hiddenA")) && (Player.HasItem(70074)) && (Player.HasItem(70075) && (Player.HasItem(70077))))
		{
			Player.RemoveKeyword("errand_of_fleta");
			Player.RemoveKeyword("errand_hiddenA");
			Player.RemoveItem(70074); // St. John's Wort
			Player.RemoveItem(70075); // Angelic Herb
			Player.RemoveItem(70077); // Peppermint
			Player.GiveItem(52044); // Aeira's Special Coupon
			Send.Notice(Player, ("Received Aeira's Special Coupon from Fleta"));
			Msg("Let's see...");
			Msg(Hide.Name, "(Gave Fleta the items.)");
			Msg("Mm, these are perfect. Good job.");
			Msg("Now, I'll put these...");
			Msg(Hide.Name, "<image name='mini_fleta_jewelbox01'/>(Fleta opened the chest and put the ingredients in there.)"); //display image of fleta putting ingredients into chest
			Msg(Hide.Name, "<image name='mini_fleta_jewelbox02'/>(As Fleta began casting a spell<br/>all these firefly like light particles<br/>started to gather around.)"); //display image of fleta casting a spell
			Msg("There... all done.<br/>They know they're going to eventually all come back<br/>yet they're always running away...");
			Msg("Oh, the ticket I just gave you is for bringing me all the ingredients.<br/>If you take it to the Dunbarton Bookstore<br/>I think you can buy a skill book for a high rank.");
			Msg("I found it somewhere in my house<br/>but it's no use to me so you can have it.<br/>You might need it.");
			Msg("Do you need anything else...?", Button("End conversation", "@exit"), Button("Start a Conversation", "@talk"), Button("Shop", "@shop"), Button("Repair item", "@repair"));
		}
		else if ((Player.HasItem(70071)) || (Player.HasItem(70072)) || (Player.HasItem(70073)) && (Player.HasKeyword("dogcollar_hiddenA")))
		{
			Player.RemoveKeyword("making_dogcollar_of_rab");
			Player.RemoveKeyword("dogcollar_hiddenA");
			Player.RemoveItem(70071); // Leather Dog Collar
			Player.RemoveItem(70072); // Gold Dog Collar
			Player.RemoveItem(70073); // Chain Dog Collar
			Player.GiveItem(52043); // Manus's Special Coupon
			Send.Notice(Player, ("Received Manus's Special Coupon from Fleta"));
			Msg(Hide.Name, "(Handed the collar to Fleta.)");
			Msg("Thank you, well done.<br/>This should keep him from wandering off for awhile.");
			Msg("As a reward for bringing me the collar,<br/>I'll give you a ticket<br/>that allows you to buy several skill books.");
			Msg("It was rolling around somewhere at my house...<br/>Give it to a guy named Manus in Dunbarton.<br/>It's not useful to me<br/>but it might be useful to you.");
			Msg("Do you need anything else...?", Button("End conversation", "@exit"), Button("Start a Conversation", "@talk"), Button("Shop", "@shop"), Button("Repair item", "@repair"));
		}
		else
			Msg("...If you have something to say, say it.", Button("Start a Conversation", "@talk"), Button("Shop", "@shop"), Button("Repair Item", "@repair"));

		switch (await Select())
		{
			case "@exit":
				break;

			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());

				if (Player.IsUsingTitle(11001))
				{
					Msg("...You rescued the Goddess?<br/>...Not bad for someone like you.");
				}
				else if (Player.IsUsingTitle(11002))
				{
					Msg("Wow, don't you feel a bit unworthy to have a title like that?<br/>Guardian...? Sounds dumb.");
				}

				await Conversation();
				break;

			case "@shop":
				Msg("The clothes I sell are special because they are made using magic.<br/>I'm the only one who can repair them,<br/>so keep that in mind if you buy any clothes from me.");
				OpenShop("FletaShop");
				return;

			case "@repair":
				Msg("I'm the only one who can repair my clothes.<br/>You know that, right?<repair rate='98' stringid='(*/agelimit_cloth/*)|(*/agelimit_robe/*)|(*/agelimit_glove/*)|(*/agelimit_shoes/*)|(*/magicsmith_repairable/*)' />");

				while (true)
				{
					var repair = await Select();

					if (!repair.StartsWith("@repair"))
						break;

					var result = Repair(repair, 98, "/agelimit_cloth/", "/agelimit_robe/", "/agelimit_glove/", "/agelimit_shoes/", "/magicsmith_repairable/");
					if (!result.HadGold)
					{
						RndMsg(
							"I don't take IOU's.<br/>Have the money first before you ask.",
							"Hey...bring the money first."
						);
					}
					else if (result.Points == 1)
					{
						if (result.Fails == 0)
							RndMsg(
								"Here, I repaired 1 point..",
								"1 point is a piece of cake.",
								"1 point repaired, here.",
								"I don't even think there's much to repair but...<br/>Here, 1 point."
							);
						else
							Msg("The repair was a failure."); // Unofficial
					}
					else if (result.Points > 1)
					{
						if (result.Fails == 0)
							RndMsg(
								"It's finished."
							);
						else
							Msg(string.Format(L("Sorry, looks like I couldn't repair {0} point(s).<br/>My magic isn't quite perfect."), result.Fails, result.Successes)); // Unofficial
					}
				}

				Msg("Do you have anything else you want to repair?<br/>I'll repair it for you if I have nothing else to do, so bring it.<repair hide='true'/>");
				break;
		}

		End("Goodbye <npcname/>.");
	}

	private void Greet()
	{
		if (Memory <= 0)
		{
			Msg(FavorExpression(), L("...I wasn't expecting people here."));
		}
		else if (Memory == 1)
		{
			Msg(FavorExpression(), L("Hey, you again."));
		}
		else if (Memory == 2)
		{
			Msg(FavorExpression(), L("Hello again, this is the third time I've seen you."));
		}
		else if (Memory <= 6)
		{
			Msg(FavorExpression(), L("Do you have hidden treasure here or something?<br/>You're here pretty often."));
		}
		else
		{
			Msg(FavorExpression(), L("Hey, <username/>.<br/>Do you have something to say?"));
		}

		UpdateRelationAfterGreet();
	}

	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				if (Favor >= 10 && !Player.HasItem(52043) && !Player.HasKeyword("dogcollar_hiddenA"))
				{
					GiveKeyword("making_dogcollar_of_rab");
					GiveKeyword("dogcollar_hiddenA");
					Msg("I've been worried lately.<br/>Rab keeps chewing off his collar<br/>and wanders off on his own.");
					Msg("Every time I get him a new one,<br/>he keeps destroying it.<br/>He should know better by now.");
					Msg("...Wait a minute...<br/>Can you get me another dog collar?<br/>I'm sure you will be able<br/>to get me a decent one.");
					Msg("Don't worry, I'm not asking you to do it for free.<br/>I'll give you something in return for the dog collar.");
					Msg("Where do you get it?<br/>That's for you to find out.<br/>You should probably ask someone<br/>who makes that kind of thing.");
				}
				else
				{
					Msg(FavorExpression(), "My name is Fleta.<br/>I come out here when I get bored.");
					ModifyRelation(1, 0, 0);
				}
				break;

			case "rumor":
				Msg(FavorExpression(), "This is a plain called Sen Mag.<br/>A lot of poeple have died here.<br>I like it because it's quiet and calm.");
				ModifyRelation(Random(2), 0, Random(3));
				break;

			case "shop_misc":
				Msg("I'm not interested in cheap items.");
				break;

			case "shop_grocery":
				Msg("I'm not interested in cheap foods.");
				break;

			case "shop_healing":
				Msg("Isn't it wise not to engage in things you can get hurt doing?");
				break;

			case "shop_inn":
				Msg("Why would you leave home and sleep at a place like this...tsk, tsk.");
				break;

			case "shop_bank":
				Msg("That's why it's called a gamble.<br/>You got to be quick with it in order to make a lot of money.");
				break;

			case "shop_smith":
				Msg("Do you need such a barbaric shop?<br/>I just do it to kill time.");
				break;

			case "skill_rest":
				Msg("Rest all you want.<br/>No one's stopping you.");
				break;

			case "skill_range":
				Msg("...You're easily scared, aren't you?");
				break;

			case "skill_composing":
				Msg("When I look at people who brag about how artistic<br/>they are after they compose one song<br/>...I don't know whether it's pathetic or sad.<br/>I hope you're not like that.");
				break;

			case "skill_instrument":
				Msg("I can tolerate it if they're good...<br/>but when people that aren't even good are making noise, it's pure torture...");
				break;

			case "skill_tailoring":
				Msg("Isn't that a skill for commoners?<br/>Me? I don't know that.");
				Msg("I only make magic.<br/>I just do it because I'm bored.");
				break;

			case "skill_gathering":
				Msg("Do I look like someone who would do that?");
				break;

			case "square":
				Msg("You must have bad eyes.<br/>This isn't an open field.");
				break;

			case "temple":
				Msg("I don't really like it.<br/>There's a lot of Priests who are crooked.");
				break;

			case "school":
				Msg("There are people who ask why I don't go to school...<br/>Just know that, that's very rude.");
				break;

			case "shop_restaurant":
				Msg("I'm not interested in cheap food.<br/>...Although I heard there's some<br/>some places in Emain Macha.");
				break;

			case "shop_cloth":
				Msg("If you don't have the skills, you'll just have to buy it.<br/>I have some I made for fun,<br/>do you want to check them out?");
				break;

			case "shop_bookstore":
				Msg("You're smarter than I thought.<br/>I wasn't expecting you to ask me such things.");
				break;

			case "graveyard":
				Msg("I like it quiet.<br/>But I don't like being around filthy things.");
				break;

			case "skill_fishing":
				Msg("Do you like looking shabby...?<br/>Look at the way you're sitting.");
				break;

			case "lute":
				Msg("It's a cheap instrument...<br/>but it sounds good so it shouldn't matter.");
				break;

			case "complicity":
				Msg("What are you talking about?");
				break;

			case "tir_na_nog":
				Msg("Hm, I don't know. No comment.");
				break;

			case "mabinogi":
				Msg("They don't even live long.<br/>I don't understand why they want their stories to be remembered and passed on.<br/>Those songs are going to be forgotten some day too anyway.");
				break;

			case "nao_blacksuit":
				Msg("I don't make clothes like that.");
				break;

			case "EG_C2_Unknown_Continet":
				Msg("...I guess you like fairy tales.");
				break;

			case "EG_C2_HowTo_Shipboard":
				Msg("...Can't you just fly over using magic?<br/>Why do you need to bother with getting on a ship and all that?");
				break;

			case "breast":
				Msg("Have you really not been weaned yet?");
				break;

			case "errand_of_fleta":
				Msg("Items to put in the chest?");
				Msg("Spices that Imps like,<br/>flowers that glow in the dark,<br/>and herbs grown by angels.");
				Msg("Wouldn't someone who would sell such things know?<br/>I told you to go to the nearest city,<br/>so you figure out the rest.");
				break;

			case "making_dogcollar_of_rab":
				Msg("Who makes collars?<br/>I'm not sure. Maybe you should go ask a blacksmith.");
				break;

			default:
				RndFavorMsg(
					"That's what you want to talk to me about?",
					"Well...",
					"I don't want to answer...I'm too lazy.",
					"I don't really want to talk about that.",
					"...Why are you asking me?",
					"You don't have anything else you want to talk about?",
					"I'm not really interested.",
					"I don't feel like talking about that."
				);
				ModifyRelation(0, 0, Random(3));
				break;
		}
	}

	protected override async Task Gift(Item item, GiftReaction reaction)
	{
		switch (reaction)
		{
			case GiftReaction.Love:
				Msg(L("We have similar styles... I'm impressed."));
				break;

			default: // GiftReaction.Neutral
				RndMsg(
						"Ha, I guess I'll take it.",
						"This is a gift?<br/>Pick out something better another time.",
						"I don't know about this..but I guess I'll take it."
					);
				break;
		}
	}
}

public class FletaShop : NpcShopScript
{
	public override void Setup()
	{
		Add("Outfit", 19009); // Coco Rabbit Robe
		Add("Outfit", 19010); // Selina Panel Robe
		Add("Outfit", 19012); // Trudy Layered Robe
		Add("Outfit", 19014); // Trudy's Rain Robe
		Add("Outfit", 15128); // Two-Tone Bizot Dress
		Add("Outfit", 15127); // Selina Traditional Coat
		Add("Outfit", 15124); // Classic Sleeve Wear
		Add("Outfit", 15134); // Gothic Laced Skirt
		Add("Outfit", 19018); // Jabu-shinseon's Imperial Robe
		Add("Outfit", 19019); // Lacard's Layered Muffler Robe
		Add("Outfit", 19020); // Nathane Snow Mountain Coat

		// Japan Server Only

		// Event Only
		//Add("Outfit", 15175); // Yukata (F)
		//Add("Outfit", 15176); // Yukata (M)
		//Add("Outfit", 15176); // Japanese Fan
		//Add("Outfit", 15176); // Japanese Sandals
		//Add("Outfit", 15347); // Yukata (F) Type 2
		//Add("Outfit", 15346); // Yukata (M) Type 2
		//Add("Outfit", 15254); // Yukata (F) Type 3
		//Add("Outfit", 15253); // Yukata (M) Type 3

		// 2008 Costume Contest Winners
		Add("Outfit", 14047); // Flame Patterned Leather Armor (M)
		Add("Outfit", 14048); // Flame Patterned Leather Armor (F)
		Add("Outfit", 19048); // Jester Robe
		Add("Outfit", 15325); // Bat Jacket (M)
		Add("Outfit", 15326); // Bat Jacket (F)

		//if (IsEnabled("Giant"))
		{
			Add("Outfit", 14049); // Primitive Fox Armor (M)
			Add("Outfit", 14050); // Primitive Fox Armor (F)
			Add("Outfit", 15327); // Fur-trimmed Coat
			Add("Outfit", 15328); // Fur-trimmed Dress
			Add("Outfit", 19049); // Royal Striped Robe (M)
			Add("Outfit", 19050); // Royal Striped Robe (F)
		}

		// 2009 Costume Contest Winners
		Add("Outfit", 19061); // Mushroom Robe	
		Add("Outfit", 15459); // Clover Pig Costume (F)
		Add("Outfit", 15460); // Clover Pig Costume (M)
		Add("Outfit", 14057); // Elegant Lamellar Armor (M)
		Add("Outfit", 14058); // Elegant Lamellar Armor (F)

		//if (IsEnabled("Giant"))
		{
			Add("Outfit", 19062); // Giant Walrus Robe (M)
			Add("Outfit", 19063); // Giant Walrus Robe (F)
			Add("Outfit", 15461); // Traveler Bolero (F Giant)
			Add("Outfit", 15462); // Traveler Bolero (M Giant)
			Add("Outfit", 13068); // Giant Half Guard Leather Armor (M)
			Add("Outfit", 13069); // Giant Half Guard Leather Armor (F)
		}

		// 2010 Costume Contest Winners
		Add("Outfit", 19070); // Owl Robe
		Add("Outfit", 15589); // Traveler Outfit (F)
		Add("Outfit", 15590); // Traveler Outfit (M)
		Add("Outfit", 14063); // Diamond Patterned Leather Armor (M)
		Add("Outfit", 14064); // Diamond Patterned Leather Armor (F)

		//if (IsEnabled("Giant"))
		{
			Add("Outfit", 15598); // Delightful Orchestra Costume (F Giant)
			Add("Outfit", 15599); // Delightful Orchestra Costume (M Giant)
			Add("Outfit", 14065); // Quilted Light Armor (Male Giant)
			Add("Outfit", 14066); // Quilted Light Armor (Female Giant)
			Add("Outfit", 19071); // Protector's Robe
		}

		Add("Fancy Outfit", 14015); // Claus Muffler Leather Mail
		Add("Fancy Outfit", 15110); // Selina Suit
		Add("Fancy Outfit", 15109); // Tipping Suit
		Add("Fancy Outfit", 15115); // Jagged Mini Skirt
		Add("Fancy Outfit", 15116); // Wizard Suit for Women
		Add("Fancy Outfit", 15117); // Wizard Suit for Men
		Add("Fancy Outfit", 15118); // Short Swordsmanship School Uniform (M)
		Add("Fancy Outfit", 15119); // Long Swordsmanship School Uniform (F)
		Add("Fancy Outfit", 15139); // Xiao-Lung Juen's Formal Suit (F)
		Add("Fancy Outfit", 15140); // Xiao-Lung Juen's Formal Suit (M)
		Add("Fancy Outfit", 15157); // Wis' Intelligence Soldier Uniform (M)
		Add("Fancy Outfit", 15158); // Wis' Intelligence Soldier Uniform (F)
		Add("Fancy Outfit", 15095); // Selina Open Leather Jacket
		Add("Fancy Outfit", 15260); // Daby Scots Plaid Wear for Men
		Add("Fancy Outfit", 15261); // Daby Scots Plaid Wear for Women

		Add("Sewing Patterns", 60044, "FORMID:4:135;", 27000);   // Sewing Pattern - Adventurer's Suit (M)
		Add("Sewing Patterns", 60044, "FORMID:4:138;", 67000);   // Sewing Pattern - Sleeveless and Bell-Bottoms (F)
		Add("Sewing Patterns", 60044, "FORMID:4:144;", 217000);   // Sewing Pattern - Chic Suit

		Add("Special Sewing Patterns", 60044, "FORMID:4:133;", 5800);   // Sewing Pattern - Coco Sailor Mini (M)
		Add("Special Sewing Patterns", 60044, "FORMID:4:136;", 50000);   // Sewing Pattern - Coco Panda Robe
		Add("Special Sewing Patterns", 60044, "FORMID:4:137;", 75000);   // Sewing Pattern - Selina Open Leather Jacket
		Add("Special Sewing Patterns", 60044, "FORMID:4:139;", 82000);   // Sewing Pattern - Selina Sexy Bare Look
		Add("Special Sewing Patterns", 60044, "FORMID:4:140;", 92000);   // Sewing Pattern - Bianca Drawers Layered Skirt.
		Add("Special Sewing Patterns", 60044, "FORMID:4:141;", 95000);   // Sewing Pattern - Selina Lady Dress
		Add("Special Sewing Patterns", 60044, "FORMID:4:142;", 132000);   // Sewing Pattern - Long Swordsmanship School Uniform (M)
		Add("Special Sewing Patterns", 60044, "FORMID:4:143;", 107000);   // Sewing Pattern - Short Swordsmanship School Uniform (F)
		Add("Special Sewing Patterns", 60044, "FORMID:4:192;", 115500);   // Sewing Pattern - Daby Scots Plaid Wear for Men
		Add("Special Sewing Patterns", 60044, "FORMID:4:193;", 115500);   // Sewing Pattern - Daby Scots Plaid Wear for Women
		Add("Special Sewing Patterns", 60044, "FORMID:4:194;", 15000);   // Sewing Pattern - Daby Scots Plaid Boots for Men
		Add("Special Sewing Patterns", 60044, "FORMID:4:195;", 15000);   // Sewing Pattern - Daby Scots Plaid Boots for Women

		Add("Blueprint", 64581, "FORMID:4:20118;", 55000);   // Blacksmith Manual - Scale Armor
		Add("Blueprint", 64581, "FORMID:4:20122;", 93000);   // Blacksmith Manual - Plate Mail
		Add("Blueprint", 64581, "FORMID:4:20143;", 148000);   // Blacksmith Manual - Valencia's Cross Line Plate Armor(M)
		Add("Blueprint", 64581, "FORMID:4:20144;", 162000);   // Blacksmith Manual - Valencia's Cross Line Plate Armor(F)
		Add("Blueprint", 64581, "FORMID:4:20152;", 51000);   // Blacksmith Manual - Valencia Crossline Plate Gauntlets
		Add("Blueprint", 64581, "FORMID:4:20153;", 56000);   // Blacksmith Manual - Valencia Crossline Plate Boots
		Add("Blueprint", 64581, "FORMID:4:20157;", 274000);   // Blacksmith Manual - Dustin Silver Knight Armor
		Add("Blueprint", 64581, "FORMID:4:20158;", 138000);   // Blacksmith Manual - Dustin Silver Knight Vambrace
		Add("Blueprint", 64581, "FORMID:4:20159;", 162000);   // Blacksmith Manual - Dustin Silver Knight Greaves
		Add("Blueprint", 64581, "FORMID:4:20197;", 131000);   // Blacksmith Manual - Valencia's Cross Line Plated Armor (Giant)
		Add("Blueprint", 64581, "FORMID:4:20198;", 48000);   // Blacksmith Manual - Valencia's Cross Line Plated Gauntlet (Giant)

		Add("Shoes & Gloves", 17045); // V-neck Leather Boots
		Add("Shoes & Gloves", 17046); // Cryssea Greaves
		Add("Shoes & Gloves", 17048); // Calix Leather Boots
		Add("Shoes & Gloves", 17049); // Rolo Roper Shoes
		Add("Shoes & Gloves", 16522); // Claus' Wooden Gauntlet
		Add("Shoes & Gloves", 16520); // Long Leather Glove
		Add("Shoes & Gloves", 17513); // Fabien Greaves
		Add("Shoes & Gloves", 16511); // Silver Teany Glove
		Add("Shoes & Gloves", 17053); // Selina's Trendy Boots
		Add("Shoes & Gloves", 17056); // Xiao-Lung Juen's Dress Shoes (F)
		Add("Shoes & Gloves", 17057); // Xiao-Lung Juen's Dress Shoes (M)
		Add("Shoes & Gloves", 17062); // Wis' Intelligence Soldier Boots (M)
		Add("Shoes & Gloves", 16032); // Elven Glove
		Add("Shoes & Gloves", 16032); // Two-lined Belt Glove
		Add("Shoes & Gloves", 17099); // Daby Scots Plaid Boots for Men
		Add("Shoes & Gloves", 17099); // Daby Scots Plaid Boots for Women

		// Japan Server Only

		// 2008 Costume Contest Winners
		Add("Shoes & Gloves", 17130); // Flame Patterned Leather Boots
		Add("Shoes & Gloves", 16054); // Flame Patterned Leather Gloves	
		Add("Shoes & Gloves", 17132); // Bat Boots

		//if (IsEnabled("Giant"))
		{
			Add("Shoes & Gloves", 16055); // Primitive Fox Gloves (M)
			Add("Shoes & Gloves", 16056); // Primitive Fox Gloves (F)
			Add("Shoes & Gloves", 17133); // Fur-trimmed Boots (M)
			Add("Shoes & Gloves", 17134); // Fur-trimmed Boots (F)
		}

		// 2009 Costume Contest Winners
		Add("Shoes & Gloves", 16063); // Elegant Lamellar Gloves
		Add("Shoes & Gloves", 17164); // Elegant Lamellar Boots

		//if (IsEnabled("Giant"))
		{
			Add("Shoes & Gloves", 17197); // Traveler Bolero Shoes (F Giant)
			Add("Shoes & Gloves", 17198); // Traveler Bolero Shoes (M Giant)
			Add("Shoes & Gloves", 16552); // Giant Half Guard Leather Gauntlets (M)
			Add("Shoes & Gloves", 16553); // Giant Half Guard Leather Gauntlets (F)
			Add("Shoes & Gloves", 17535); // Giant Half Guard Leather Greaves (M)
			Add("Shoes & Gloves", 17536); // Giant Half Guard Leather Greaves (F)
		}

		// 2010 Costume Contest Winners
		Add("Shoes & Gloves", 17229); // Traveler Boots
		Add("Shoes & Gloves", 16143); // Diamond Patterned Leather Gloves
		Add("Shoes & Gloves", 17553); // Diamond Patterned Leather Boots

		if (IsEnabled("Giant"))
		{
			Add("Shoes & Gloves", 17233); // Delightful Orchestra Shoes (F Giant)
			Add("Shoes & Gloves", 17234); // Delightful Orchestra Shoes (M Giant)
			Add("Shoes & Gloves", 16144); // Quilted Protector (Giants)
			Add("Shoes & Gloves", 16144); // Quilted Boots (Giants)
		}

		Add("Hats", 18125); // Wis' Intelligence Soldier Cap (F)

		// Japan/Korea/China Servers Only

		// Event Only
		Add("Hats", 18107); // Romantic Straw Hat
		Add("Hats", 18107); // Romantic Straw Hat
		Add("Hats", 18107); // Romantic Straw Hat
		Add("Hats", 18107); // Romantic Straw Hat
		Add("Hats", 18107); // Romantic Straw Hat

		// Japan Server Only

		// Event Only
		Add("Hats", 18129); // Watermelon Hat
		Add("Hats", 18130); // Straw Hat

		// 2008 Costume Contest Winners
		Add("Hats", 18228); // Viseo's Flight Cap
		Add("Hats", 18559); // Flame Patterned Leather Cap
		Add("Hats", 18566); // Bat Hat
		Add("Hats", 18262); // Clover Pig Hat		
		Add("Hats", 18272); // Elegant Lamellar Helmet

		if (IsEnabled("Giant"))
		{
			Add("Hats", 18263); // Traveler Bolero Bandana (F Giant)
			Add("Hats", 18264); // Traveler Bolero Bandana (M Giant)
			Add("Hats", 18315); // Delightful Orchestra Hat
		}
		// 2010 Costume Contest Winners
		Add("Hats", 18597); // Diamond Patterned Leather Helmet

		if (IsEnabled("Giant"))
			Add("Hats", 18598); // Quilted Hairband

		Add("Event"); // Empty

		// Japan Server Only
		//Add("Event", 2034); // 우사단고 아이템 가방 (Usa Tango Bag)
		//Add("Event", 15450); // 우사단고 보이시 캐주얼 (Usa Boise Casual Tango)
		//Add("Event", 15451); // 우사단고 걸리시 캐주얼 (Usa Tango Casual geolrisi)

		// Japan and Europe Server Only
		//Add("Event", 2039); // Football Item Bag

		// Taiwan Dungeon Event 2009
		//Add("Event", 75329); // Dungeon Collection Book Vol. 1
		//Add("Event", 75330); // Dungeon Collection Book Vol. 2
		//Add("Event", 75331); // Dungeon Collection Book Vol. 3
		//Add("Event", 75332); // Dungeon Collection Book Vol. 4
	}
}
