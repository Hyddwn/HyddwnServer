//--- Aura Script -----------------------------------------------------------
// Stewart
//--- Description -----------------------------------------------------------
// Librarian and Magic Teacher
//---------------------------------------------------------------------------

public class StewartScript : NpcScript
{
	public override void Load()
	{
		SetRace(10002);
		SetName("_stewart");
		SetFace(skinColor: 16, eyeType: 3, eyeColor: 120);
		SetLocation(18, 2671, 1771, 99);
		SetGiftWeights(beauty: 0, individuality: 1, luxury: -1, toughness: 0, utility: 2, rarity: 2, meaning: 2, adult: 0, maniac: 0, anime: 0, sexy: 0);

		EquipItem(Pocket.Face, 4900, 0x00C89568, 0x00157B42, 0x00004944);
		EquipItem(Pocket.Hair, 4010, 0x00997744, 0x00997744, 0x00997744);
		EquipItem(Pocket.Armor, 15002, 0x00F7941D, 0x00A0927D, 0x00B80026);
		EquipItem(Pocket.Shoe, 17012, 0x00B80026, 0x004F548D, 0x00904959);
		EquipItem(Pocket.Head, 18029, 0x00625F44, 0x00C1C1C1, 0x00CEA96B);
		EquipItem(Pocket.Robe, 19003, 0x00993333, 0x00221111, 0x00664444);
		SetHoodDown();

		AddPhrase("Hmm... I'll have to talk with Kristell about this.");
		AddPhrase("Hmm... There aren't enough textbooks available.");
		AddPhrase("I wonder if Aeira has prepared all the books.");
		AddPhrase("It's not going to work like this.");
		AddPhrase("Maybe I should ask Aranwen...");
		AddPhrase("More and more people are not showing up...");
		AddPhrase("Oh dear! I've already run out of magic materials.");
		AddPhrase("Perhaps there's something wrong with my lecture?");
	}

	protected override async Task Talk()
	{
		SetBgm("NPC_Stewart.mp3");

		await Intro(L("He is a young man with nerdy spectacles and tangled hair.<br/>Beneath his glasses, his soft eyes are somewhat appealing,<br/>but his stained tunic and his hands which reek of herbs confirm that he is clumsy and unkempt."));

		Msg("How can I help you?", Button("Start a Conversation", "@talk"), Button("Shop", "@shop"), Button("Repair Item", "@repair"), Button("Upgrade Item", "@upgrade"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());

				if (Title == 11001)
				{
					Msg("<username/>, you saved the Goddess?");
					Msg("Wow!!! What an accomplishment!<br/>Does that mean you've been to Tir Na Nog too?<br/>And does that mean that the Goddess has");
					Msg("arrived here in Erinn...?<br/>Is the Goddess here in Erinn right now?<br/>Are we in Tir Na Nog...?");
					Msg("...");
					Msg("...Judging by your expression,<br/>I'll take that as a no.");
				}
				if (Title == 11002)
				{
					Msg("Welcome, <username/>, Guardian of Erinn.<br/>It seems like we should already start writing about your legacy<br/>along with the legendary three warriors. Haha...");
				}

				await Conversation();
				break;

			case "@shop":
				Msg("I have a few items related to magic here.<br/>You can buy some if you need any.");
				OpenShop("StewartShop");
				return;

			case "@repair":
				Msg("Do you want to repair your magic weapon?<br/>All magic weapons are laden with Mana, so it's impossible to physically fix them.<br/>If you fix them the way blacksmiths fix swords, then they may lose all the magic powers that come with them.<repair rate='95' stringid='(*/magic_school_repairable/*)' />");

				while (true)
				{
					var repair = await Select();

					if (!repair.StartsWith("@repair"))
						break;

					var result = Repair(repair, 95, "/magic_school_repairable/");
					if (!result.HadGold)
					{
						RndMsg(
							"I'm afraid you don't have enough money to repair the wand...",
							"I don't think you have enough money to fix it...",
							"I don't think you have enough money to repair the item..."
						);
					}
					else if (result.Points == 1)
					{
						if (result.Fails == 0)
							RndMsg(
								"I was able to fix it without any errors.",
								"1 point has been fixed."
							);
						else
							Msg("Oh no... I think I made a mistake."); // Should be 2
					}
					else if (result.Points > 1)
					{
						if (result.Fails == 0)
							RndMsg(
								"Thankfully, everything went well. I didn't make any mistakes.",
								"The item is completely repaired."
							);
						else
							// TODO: Use string format once we have XML dialogues.
							Msg("The item is repaired...but<br/>I was unable to repair " + result.Fails + " point(s). I'm sorry.");
					}
				}

				Msg("Please handle with care..<repair hide='true'/>");
				break;

			case "@upgrade":
				Msg("You want to upgrade something?<br/>First, let me see the item.<br/>Remember that the amount and type of upgrade varies with each item.<upgrade />");

				while (true)
				{
					var reply = await Select();

					if (!reply.StartsWith("@upgrade:"))
						break;

					var result = Upgrade(reply);
					if (result.Success)
						Msg("Haha, the upgrade was successful.<br/>Do you have anything else to upgrade?");
					else
						Msg("(Error)");
				}

				Msg("Come see me again next time if you have something else to upgrade.<upgrade hide='true'/>");
				break;
		}

		End("Thank you, <npcname/>. I'll see you later!");
	}

	private void Greet()
	{
		if (Memory <= 0)
		{
			Msg(FavorExpression(), L("Mmm... How can I help you?"));
		}
		else if (Memory == 1)
		{
			Msg(FavorExpression(), L("Excuse me, but have we met before?"));
		}
		else if (Memory == 2)
		{
			Msg(FavorExpression(), L("You are... <username/>, right?"));
		}
		else if (Memory <= 6)
		{
			Msg(FavorExpression(), L("I think I've met you before, you're <username/> right?"));
		}
		else
		{
			Msg(FavorExpression(), L("Welcome, <username/>. I've been wondering how you were doing."));
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
					GiveKeyword("school");
					Msg(FavorExpression(), "Yes, I'm Stewart. And you are?<br/>Hmm... <username/>?<br/>I teach magic to students here.<br/>That makes me a... magic teacher.");
					ModifyRelation(1, 0, 0);
				}
				else
				{
					Player.Keywords.Give("shop_misc");
					Msg(FavorExpression(), "Mmm... many of the people in this town came from other towns.<br/>But some like me or Walter at the General Shop<br/>have been here for a long time.<br/>I'm not saying there are any particular advantages to that but...haha...");
					ModifyRelation(Random(2), 0, Random(3));
				}
				break;

			case "rumor":
				Player.Keywords.Give("shop_bookstore");
				Msg(FavorExpression(), "If you're looking for books on magic or enchantments,<br/>you'll find useful learning resources at the Bookstore nearby.<br/>Just say I sent you and Aeira will be pleased to help you.<br/>It'll take a long time to explain these topics, so let's talk after you've read the books.");
				ModifyRelation(Random(2), 0, Random(3));
				break;

			case "about_skill":
				// if Ice Spear quest...
				// else if Fireball quest...
				// else if Thunder quest
				// else --v
				if (!HasSkill(SkillId.Meditation, SkillRank.RF))
				{
					// Teach Meditation if bolts are rF, otherwise teach
					// about Meditation.
					if (HasSkill(SkillId.Icebolt, SkillRank.RF) && HasSkill(SkillId.Firebolt, SkillRank.RF) && HasSkill(SkillId.Lightningbolt, SkillRank.RF))
					{
						Msg(L("You've learned the magic involving all three of the Elements!<br/>I'm impressed that you worked so hard even though it was not a quest or a part-time job...<br/>To be honest, I'm amazed!"));
						Msg(L("Now that you know how to deal with the three Elements,<br/>I think you understand the elemental rules governing this world.<br/>There's nothing more I can tell you about this.<br/>By the way, there is a reason why I explained about controlling the flow of Mana through meditation."));
						Msg(L("As you use all three magic,<br/>you'll often feel there's not enough Mana.<br/>Just remember to use the Meditation skill."));

						if (!HasSkill(SkillId.Meditation))
							GiveSkill(SkillId.Meditation, SkillRank.Novice);
						if (IsSkill(SkillId.Meditation, SkillRank.Novice))
							TrainSkill(SkillId.Meditation, 1);
					}
					else
					{
						Msg(L("Do you know anything about the Meditation skill?<br/>This is used to control the flow of Mana through meditation.<br/>This magic is useful when you are short on Mana<br/>before the Eweca has risen."));
						Msg(L("I'd really like to teach you how to use this magic, but without the basic understanding of Elements,<br/>it could confuse you.<br/>I'll teach you this skill when you learn the three basic Elements of magic.<br/>If you're interested, learn these other magic as well."));

						GiveSkill(SkillId.Meditation, SkillRank.Novice);
					}
				}
				else
				{
					Msg("Haha, <username/>, you seem to be a very curious person.<br/>This is not the right time, though...<br/>...I'll let you know when the time is right.");
				}
				break;

			case "shop_misc":
				Msg("Hmm? The General Shop?<br/>It's a bit far from here.<br/>Did someone tell you to go this way?<br/>Why don't you go towards the Square? It's near there.");
				Msg("When you get to the General Shop,<br/>try talking to Walter.<br/>He might be a bit blunt but he's a really nice person.");
				break;

			case "shop_grocery":
				GiveKeyword("shop_restaurant");
				Msg("The Grocery Store? I don't know...<br/>If groceries are what you need,<br/>you might as well go to the Restaurant.<br/>They sell food ingredients as well. I think you'll find what you're looking for there.");
				break;

			case "shop_healing":
				Msg("Manus the muscle head...<br/>You can take the southern road.<br/>Why... are you not feeling well?");
				break;

			case "shop_inn":
				Msg("Hmm... I don't think there is an inn in this town...");
				break;

			case "shop_bank":
				Msg("The Bank is near the Square.<br/>You'll find it if you take the western road.<br/>If you're going there,<br/>could you send my regards to Austeyn?");
				break;

			case "shop_smith":
				GiveKeyword("shop_armory");
				Msg("A blacksmith's shop?<br/>There isn't a blacksmith's shop in this town.<br/>But, Nerys at the Weapons Shop might know something.");
				break;

			case "skill_range":
				Msg("If you're talking about long range attack using magic,<br/>how about learning<br/>the three Elemental magic?");
				Msg("Say...Lightning Bolt, Icebolt,<br/>or Firebolt.<br/>You can perform long range attacks against the target<br/>with this magic.");
				Msg("After you learn all three skills,<br/>please stop by and see me.<br/>I think I can help you.");
				break;

			case "skill_instrument":
				Msg("I used to think about playing an instruments but<br/>I don't think I'm cut out for it. Haha...");
				Msg("You know, no one in this town<br/>seems to be<br/>good at music.");
				Msg("Maybe except for<br/>Glenis' late husband<br/>but other than him...");
				Msg("Oh... I shouldn't have brought that up.<br/>Forget what I just said. Haha...");
				break;

			case "skill_composing":
				GiveKeyword("shop_bookstore");
				Msg("I don't think<br/>there is anyone in this town who plays music professionally.<br/>But there must be some books on it.<br/>Right! You might find something at Aeira's Bookstore!");
				break;

			case "skill_tailoring":
				GiveKeyword("shop_cloth");
				Msg("If you want to learn Tailoring,<br/>go and see Simon near the Square.<br/>He runs the Clothing Shop.<br/>He can be a bit awkward sometimes but<br/>generally he's okay.");
				Msg("I'm not sure if he would teach you the skills though...");
				break;

			case "skill_magnum_shot":
				GiveKeyword("school");
				Msg("Mmm... I think you're talking about attacks<br/>using bows...<br/>You might want to talk to Aranwen<br/>about that.");
				Msg("Aranwen is outside the School.<br/>She teaches combat skills but she might know something about that.");
				break;

			case "skill_counter_attack":
				GiveKeyword("school");
				Msg("Oh! Aranwen should know.<br/>Once I saw her defeating a man twice her size during a training session<br/>and she used the Counterattack skill.");
				Msg("I wonder if anyone would want to get married to<br/>such a tomboy? Haha...");
				break;

			case "skill_smash":
				GiveKeyword("school");
				Msg("Smash skill... It's one of Aranwen's specialties.<br/>She teaches combat skills.<br/>But everyone always asks her and it seems like<br/>she's getting tired of answering questions.");
				break;

			case "skill_gathering":
				Msg("Ah..<br/>Sometimes I go outside this town and<br/>gather herbs.<br/>It's not as easy as it may seem.");
				Msg("I don't know if this would be of any help but...<br/>whatever it is that you want to gather,<br/>you need to have the right tool.<br/>Otherwise, you might find yourself in trouble, so be careful.");
				break;

			case "square":
				GiveKeyword("shop_goverment_office");
				Msg("The Town Square naturally formed<br/>as people began loading and unloading goods in front of the Town Office.<br/>As shops began setting up near the Square<br/>it has turned into what it is today.");
				break;

			case "pool":
				Msg("I'm not sure if there is a reservoir around here...<br/>I don't think I've ever seen one.");
				break;

			case "farmland":
				Msg("The fields around this town are mostly farmland.<br/>You can see it yourself<br/>if you go along the town walls.");
				break;

			case "windmill":
				Msg("The windmill is used to draw water or grind crops<br/>using wind power...<br/>but the wind doesn't blow hard enough around here<br/>so it's not very useful.");
				Msg("The problem is that there's no place to get water nearby...");
				break;

			case "brook":
				Msg("Adelia Stream...<br/>Do you mean the stream<br/>near Tir Chonaill?");
				Msg("I heard<br/>it is named after<br/>a Lymilark priestess.");
				break;

			case "shop_headman":
				Msg("Haha...<br/>There's no chief in this town.<br/>If you want to know who represents this town,<br/>I would say it's the Lord.");
				Msg("I'm sure in a place like Tir Chonaill,<br/>there is a chief who represents the town...");
				break;

			case "temple":
				Msg("Are you talking about the Church where Kristell serves?<br/>Then go outside, cross the road and<br/>go straight up north.");
				Msg("You can get an excellent view of the land up there,<br/>which makes it a good date spot.");
				break;

			case "school":
				Msg("Yes, this is the School.<br/>I'm Stewart and I teach magic.");
				Msg("Hmm... I've already introduced myself before... heh.");
				break;

			case "skill_windmill":
				Msg("The Windmill skill?<br/>Ah... for a split second I thought you were talking about the windmill.<br/>It's a fighting skill so...<br/>you should ask Aranwen.");
				Msg("I wonder where Aranwen learn all that...<br/>She doesn't talk<br/>much about herself, you know...");
				Msg("And I get the feeling she would smack me hard<br/>if I said something wrong...<br/>Haha...");
				break;

			case "skill_campfire":
				Msg("Campfire skill is very useful for travelers.<br/>But I don't know how to use that skill.<br/>I can light a fire with magic, though.");
				Msg("Haha... I'm afraid I can't teach you that now.");
				break;

			case "shop_restaurant":
				Msg("If you're talking about Glenis' Restaurant,<br/>it's in the northern corner of the Square.<br/>Yes, near the alley.<br/>You'll have to walk a bit.");
				Msg("But it's not that far away so don't worry.");
				break;

			case "shop_armory":
				Msg("Nerys' Weapons Shop is near Manus the Healer's house.<br/>Just keep going along the southern road.<br/>By the way... You didn't come here to learn magic?");
				break;

			case "shop_cloth":
				Msg("The man running the Clothing Shop<br/>near the Square is Simon.<br/>They say he was<br/>a well-known designer in Emain Macha.");
				Msg("There must be a story behind<br/>how such a man ended up in this small town but...<br/>he doesn't really talk about it.");
				break;

			case "shop_bookstore":
				Msg("Have you met Aeira?<br/>She is a nice girl.<br/>She can probably help you a lot.<br/>Her Bookstore is very close from here, so you might want to stop by on the way.");
				Msg("You can take the northern road up in front of the School.");
				break;

			case "shop_goverment_office":
				Msg("The large stone building in the north Square is the Town Office.<br/>You can talk to Eavan there.<br/>I heard that's where the Lord and the Captain of the Royal Guards are...<br/>But I can't be sure because I've never been inside.");
				break;

			case "bow":
				GiveKeyword("shop_armory");
				Msg("Bows are sold at Nerys' Weapons Shop.<br/>Excuse me? They're not free, of course. Haha...");
				break;

			case "lute":
				GiveKeyword("shop_armory");
				Msg("If you go to the General Shop,<br/>you'll find Lutes<br/>that Walter made himself.");
				Msg("You'll find other instruments as well,<br/>so go and have a look.");
				break;

			case "tir_na_nog":
				Msg("Tir Na Nog refers to an ultimate paradise that<br/>the people of this world dream of.<br/>There's no death nor pain,<br/>it's just full of light and joy.");
				Msg("Of course,<br/>it remains controversial<br/>whether such a place exists,<br/>or if it's merely a concept.");
				Msg("What do you think?");
				break;

			case "mabinogi":
				Msg("The songs about heroes that<br/>bards have been singing from ancient times are called Mabinogi.");
				Msg("Why don't you go to Aeira's Bookstore and<br/>look for some books on it<br/>if you want to understand<br/>Mabinogi or Erinn's history better?");
				break;

			case "musicsheet":
				GiveKeyword("shop_misc");
				Msg("Go to Walter's General Shop for a Music Score.<br/>He doesn't compose himself but...");
				Msg("Haha... I was just imagining Walter writing music<br/>with that serious look on his face.");
				break;

			default:
				RndFavorMsg(
					"I have no idea.",
					"Haha... I still don't know..",
					"Do I really have to know all that?",
					"Mmm... I have no idea. I'll write a note.",
					"Many people have asked me the same thing but...",
					"Did Aeira tell me she had books on that topic...?"
				);
				ModifyRelation(0, 0, Random(3));
				break;
		}
	}
}

public class StewartShop : NpcShopScript
{
	public override void Setup()
	{
		Add("Magic Items", 62001, 1);  // Elite Magic Powder x1
		Add("Magic Items", 62001, 10); // Elite Magic Powder x10
		Add("Magic Items", 62003, 1);  // Blessed Magic Powder x1
		Add("Magic Items", 62003, 10); // Blessed Magic Powder x10
		Add("Magic Items", 63000, 1);  // Phoenix Feather x1
		Add("Magic Items", 63000, 10); // Phoenix Feather x10
		Add("Magic Items", 63001, 1);  // Wings of a Goddess x1
		Add("Magic Items", 63001, 5);  // Wings of a Goddess x5

		Add("Spellbook", 1007); // Healing: The Basics of Magic
		Add("Spellbook", 1008); // Icebolt Spell: Origin and Training
		Add("Spellbook", 1009); // A Guidebook on Firebolt
		Add("Spellbook", 1010); // Basics of Lightning Magic: the Lightning Bolt

		Add("Magic Weapons", 40038); // Lightning Wand
		Add("Magic Weapons", 40039); // Ice Wand
		Add("Magic Weapons", 40040); // Fire Wand
		Add("Magic Weapons", 40041); // Combat Wand
		Add("Magic Weapons", 40090); // Healing Wand

		//AddQuests("Quest", InsertQuestId); // Collecting Quest - 10 Ice Elementals
		//AddQuests("Quest", InsertQuestId); // Collecting Quest - 10 Fire Elementals
		//AddQuests("Quest", InsertQuestId); // Collecting Quest - 10 Lightning Elementals

		if (IsEnabled("SpiritWeapons"))
			Add("Magic Items", 62014); // Spirit Weapon Restoration Potion

		if (IsEnabled("DarkKnight"))
		{
			Add("Fomor Scroll", 71072, 1);  // Black Fomor Scroll x1
			Add("Fomor Scroll", 71072, 10); // Black Fomor Scroll x10
		}

		if (IsEnabled("WandUpgradeAndChainCasting"))
		{
			Add("Magic Weapons", 40231); // Crystal Lightning Wand
			Add("Magic Weapons", 40232); // Crown Ice Wand
			Add("Magic Weapons", 40233); // Phoenix Fire Wand
			Add("Magic Weapons", 40234); // Tikka Wood Healing Wand
		}
	}
}