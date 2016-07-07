//--- Aura Script -----------------------------------------------------------
// Piaras
//--- Description -----------------------------------------------------------
// The Inn Keeper inside the Inn
//---------------------------------------------------------------------------

public class PiarasScript : NpcScript
{
	public override void Load()
	{
		SetRace(10002);
		SetName("_piaras");
		SetBody(height: 1.28f, weight: 0.9f, upper: 1.2f);
		SetFace(skinColor: 22, eyeType: 1);
		SetStand("human/male/anim/male_natural_stand_npc_Piaras");
		SetLocation(7, 1344, 1225, 182);
		SetGiftWeights(beauty: 0, individuality: 2, luxury: 0, toughness: 0, utility: 2, rarity: 2, meaning: 1, adult: 0, maniac: 0, anime: 1, sexy: 1);

		EquipItem(Pocket.Face, 4900, 0x00BDAF73, 0x000AB0D4, 0x00E50072);
		EquipItem(Pocket.Hair, 4004, 0x003F4959, 0x003F4959, 0x003F4959);
		EquipItem(Pocket.Armor, 15003, 0x00355047, 0x00F6E2B1, 0x00FBFBF3);
		EquipItem(Pocket.Shoe, 17012, 0x009C936F, 0x00724548, 0x0050685C);

		AddPhrase("Ah... The weather is just right to go on a journey.");
		AddPhrase("Do you ever wonder who lives up that mountain?");
		AddPhrase("Hey, you need to take your part-time job more seriously!");
		AddPhrase("I haven't seen Malcolm around here today. He used to come by every day.");
		AddPhrase("Nora, where are you? Nora?");
		AddPhrase("The Inn is always bustling with people.");
	}

	protected override async Task Talk()
	{
		await Intro(L("His straight posture gives him a strong, resolute impression even though he's only slightly taller than average height.<br/>Clean shaven, well groomed hair, spotless appearance and dark green vest make him look like a dandy.<br/>His neat looks, dark and thick eyebrows and the strong jaw line harmonized with the deep baritone voice complete the impression of an affable gentleman."));

		Msg("Welcome to my Inn.", Button("Start a Conversation", "@talk"), Button("Shop", "@shop"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());

				if (Title == 10062) // is a friend of Nora
				{
					var today = ErinnTime.Now.ToString("yyyyMMdd");
					if (today != Player.Vars.Perm["piaras_title_gift"])
					{
						Player.Vars.Perm["piaras_title_gift"] = today;

						GiveItem(63002, 5); // Firewood x5
						Notice(L("Received Firewood from Piaras."));
						SystemMsg(L("Received Firewood from Piaras."));

						Msg(L("If you are a friend of Nora, you are my friend as well.<br/>Would you like to take some?"));
					}
				}
				else if (Title == 11001)
				{
					Msg("I imagine what you did is incredible.");
					Msg("... Although I do wonder why<br/>the Goddess won't descend upon us.");
					Msg("But, really, I believe you.<br/>Follow the will of the Goddess and do the best you can do.");
				}
				if (Title == 11002)
				{
					Msg("<username/>.<br/>I was wondering where you've been...");
					Msg("...You must've went on a great adventure.<br/>You know I love adventure stories...");
					Msg("...But you can tell me about it later.<br/>There's plenty of time. Haha...");
					Msg("...How does that sound?");
				}

				await Conversation();
				break;

			case "@shop":
				Msg("May I ask what you are looking for?");
				OpenShop("PiarasShop");
				return;
		}

		End("Goodbye, <npcname/>. I'll see you later!");
	}

	private void Greet()
	{
		if (Memory <= 0)
		{
			Msg(FavorExpression(), L("Hello, nice to meet you.<br/>I am <npcname/>."));
		}
		else if (Memory == 1)
		{
			Msg(FavorExpression(), L("Nice to meet you."));
		}
		else if (Memory == 2)
		{
			Msg(FavorExpression(), L("<username/>, right? Nice to meet you."));
		}
		else if (Memory <= 6)
		{
			Msg(FavorExpression(), L("Good to see you, <username/>."));
		}
		else
		{
			Msg(FavorExpression(), L("How's it going, <username/>? Is everything alright?"));
		}

		UpdateRelationAfterGreet();
	}

	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				GiveKeyword("shop_inn");
				Msg(FavorExpression(), "I might sound too proud,<br/>but I put a lot of effort into making this place as comfortable for my guests as possible.<br/>Please visit us when you need a place to stay.");
				ModifyRelation(Random(2), 0, Random(3));
				break;

			case "rumor":
				GiveKeyword("square");
				Msg(FavorExpression(), "Why don't you talk to others in town? There's a good spot to meet people. The Town Square is right up this way. I suggest you try there first.");
				ModifyRelation(Random(2), 0, Random(3));
				break;

			case "about_skill":
				// When the beginner quests changed, he no longer removes the keyword. Instead he says "I'm sorry.<br/>I don't have the time to talk about that right now."
				// He also checks for race, but until we get the respective quests in for other races, we'll leave it out
				if (Player.Skills.Has(SkillId.Campfire))
				{
					RemoveKeyword("skill_campfire");
					Msg("Ha ha. Now you know how to use the Campfire skill.<br/>It's something I didn't want to teach you, to be honest,<br/>but I am impressed that you have mastered it so well.<br/>With this, another young adventurer is born today, ha ha.");
				}
				else
				{
					GiveKeyword("skill_campfire");
					Msg("Do you by chance know about the Campfire Skill?");
					Msg("If you start a fire using the Campfire Skill,<br/>people would come by one at a time after seeing the bright fire from afar...");
					Msg("People share what they have in their inventory<br/>and spend long summer nights sharing stories about their adventures.");
					Msg("But really, if all travelers knew the Campfire Skill,<br/>inn owners like myself would have to pack up and find a different profession.");
				}
				break;

			case "about_arbeit":
				Msg("(Unimplemented)");
				//Msg("Hmm... It's not a good time for this.<br/>Can you come back when it is time for part-time jobs?");
				break;

			case "shop_misc":
				Msg("So, you are looking for Malcolm's General Shop? It's over this hill.<br/>You'll get there in no time if you follow this road.");
				break;

			case "shop_grocery":
				Msg("Ah, I just remembered Caitin said she'd bring some food ingredients to my Inn.<br/>I keep forgetting these days.");
				break;

			case "shop_healing":
				Msg("Are you ill? Is anything bothering you?<br/>It gets cold at night. Would you like to have more heat in your room?");
				Msg("If that's not the problem, you might need a check-up from healer Dilys.");
				break;

			case "shop_inn":
				Msg("Are you looking for another Inn?<br/>I'm afraid you probably suspected you're being overcharged here? Ha ha.<br/>");
				break;

			case "shop_bank":
				Msg("Are you interested in the Bank?<br/>The Erskin Bank is a huge franchise. There are branch offices in other cities, too.<br/>You can deposit your items or money there<br/>and easily retrieve them from another office.");
				Msg("Wherever you are, you'll find a bank that can retrieve your items,<br/>so you can deposit your belongings and not worry.<br/>");
				break;

			case "shop_smith":
				Msg("The Blacksmith's Shop is literally right around the corner. You just need to cross the bridge.");
				break;

			case "skill_range":
				GiveKeyword("school");
				Msg("Are you interested in long range attack?<br/>It would be better if you went to the School<br/>and asked Ranald the instructor.");
				Msg("Long range attack is<br/>the act of attacking your opponent from a distance.<br/>Magic or arrows are common methods.");
				Msg("When you use a bow and arrows,<br/>you can inflict damage and heavily injure your enemies.<br/>In contrast, magic only reduces their HP<br/>without causing any injuries.");
				Msg("But that doesn't mean a bow is always better.<br/>You must always consider the cost of arrows.<br/>And it takes time to aim as well.");
				Msg("Magic, on the other hand, does not have such restrictions.<br/>Well, except for Mana, of course.");
				Msg("...<br/>I would suggest you think it over,<br/>and talk to a teacher<br/>who specializes in that particular area.");
				break;

			case "skill_instrument":
				Msg("What a music lover you are.<br/>The romance of a traveler...<br/>Its pinnacle can be reached<br/>only through the performance of music.");
				Msg("If you want to be good at playing an instrument,<br/>I'd suggest you buy any musical instrument you can get<br/>and keep practicing.");
				break;

			case "skill_composing":
				Msg("Do you wish to compose a tune?<br/>Hmm... the Composing skill is usually more complicated than you think<br/>and requires some effort.");
				Msg("You can't compose with only the melody<br/>that pops into your head.<br/>The critical part is to put all your musical ideas<br/>onto a sheet of paper.");
				Msg("I remember seeing a book<br/>explaining how to do that somewhere.");
				Msg("Oh, right! It's Malcolm's General Shop.<br/>Go to the General Shop and find<br/>the book titled \"Introduction to Composing Tunes\" written by Baird.");
				Msg("The book is an excellent guide<br/>on what you need to know.<br/>It would help immensely if you wish to<br/>exhibit your creativity and compose music.");
				break;

			case "skill_tailoring":
				Msg("Excuse me? You want to make clothes?<br/>Hmm... It's not an easy task.<br/>It's somewhat of a surprise that you have such interest in it.");
				Msg("Did you by any chance talk to<br/>Caitin from the Grocery Store?<br/>Caitin is the most skilled person in that area.<br/>Try talking to her, I think you will<br/>receive great assistance.");
				Msg("Hahaha. If I could design clothes,<br/>I would have opened up a Clothing Shop instead of an Inn.<br/>Please, go to Caitin at once.");
				break;

			case "skill_magnum_shot":
				GiveKeyword("school");
				Msg("Magnum Shot skill?<br/>Well, that is just one of many archery skills.<br/>For this type of skill, it would be better to learn it at the School.<br/>I will just give you a quick overview.");
				Msg("That is... the Magnum Shot skill maximizes the bow's elasticity.<br/>You pull the bowstring as far back as you can<br/>and shoot the arrow at that instant.");
				Msg("You need a keen sense of elasticity in the first place,<br/>but strong arm and chest muscles are required too.<br/>I'm afraid you'll see a rough path of training<br/>before you master the skill yourself.");
				Msg("Um... That's all I know.<br/>I'd recommend you talk to either Ranald,<br/>or Trefor standing way up there.<br/>Talking to them will help you.");
				break;

			case "skill_counter_attack":
				GiveKeyword("school");
				Msg("Melee Counterattack skill?<br/>Hmm... It's very difficult to explain with words.<br/>You'd better learn it at the School.");
				Msg("Hey, don't give me that look.<br/>I really don't know that skill.");
				Msg("How about talking to Ranald at the School<br/>or Trefor guarding this town?<br/>I'm sure they can help you better.");
				break;

			case "skill_smash":
				GiveKeyword("school");
				Msg("Want to know about the Smash skill?<br/>Many people ask me about that.<br/>How about going to the School and<br/>asking Ranald about it?");
				Msg("Ah, yes.<br/>There were some guests talking about it at my Inn.<br/>I overhear this and that because of my business.<br/>I've heard it many times, so I think the story is true.");
				Msg("By the way, for this type of skill,<br/>don't you think it's best to ask<br/>a combat instructor?");
				break;

			case "skill_gathering":
				Msg("If you want to gather something yourself,<br/>I think you need to get tools at the Blacksmith's Shop or the General Shop.");
				Msg("But I often see people pounding on the village property.<br/>I guess they would have no trouble cutting down<br/>the trees with their bare hands.");
				Msg("The sheep may ask you to use tools for their sake, though...");
				Msg("...<br/>A Gathering Knife can be bought at a very affordable price<br/>at Ferghus' Blacksmith's Shop.<br/>I'd recommend you go there if you do not have one.");
				break;

			case "square":
				Msg("Well, Nora will be more than happy to explain to you about the Square.<br/>Frankly, I talked so much that my throat is getting sore.");
				break;

			case "pool":
				GiveKeyword("farmland");
				Msg("The reservoir? It's near the farmland.<br/>If you want to go to the reservoir,<br/>cross the bridge near the Windmill and go around it. Not the bridge close to the Blacksmith's Shop.<br/>You can just follow the fence.");
				break;

			case "farmland":
				Msg("You are looking for the farmland?<br/>There were only a few people who wanted to go there.");
				Msg("Um... No offense, but you have a very unique interest.<br/>What do you think?<br/>Don't you hear that a lot?");
				break;

			case "windmill":
				Msg("The Windmill is right out there.<br/>The wind blowing down the valley moves the blades and draws water to the reservoir,<br/>and also grinds the crops.");
				Msg("I noticed more and more people going to the Windmill<br/>with wool these days.<br/>Nora asked them what they're up to,<br/>and they said they're trying to spin yarn from wool.");
				Msg("There is a spinning wheel at Malcolm's General Shop,<br/>but I guess that doesn't satisfy their needs.<br/>Perhaps they think they can spin yarn faster<br/>with a bigger wheel.");
				Msg("By the way, can it actually spin out yarn?<br/>I don't think it can.");
				break;

			case "brook":
				GiveKeyword("windmill");
				Msg("Adelia Stream?<br/>The small stream in front of my shop is the Adelia Stream.<br/>Yes, the one near the Windmill.<br/>You must have missed it. Hahaha.");
				Msg("A lot of people missed that just like you.<br/>Perhaps I should talk with Ferghus<br/>and put a sign there.");
				break;

			case "shop_headman":
				GiveKeyword("square");
				Msg("Are you looking for the Chief's House?<br/>Hm, it's very close.");
				Msg("Go up the hill with the big tree from the Square<br/>and you'll find it right there.");
				Msg("If you happen to go there,<br/>please say hello for me and<br/>try not to do anything inappropriate.");
				break;

			case "temple":
				Msg("Looking for the Church?<br/>Walk up to the Square first,<br/>and go down along the narrow path toward the Bank<br/>and you'll find it.");
				Msg("When you get there, will you please<br/>tell the generous Priest Meven and the beautiful Priestess Endelyon that<br/>Piaras from the Inn<br/>gives his fullest regards?");
				break;

			case "school":
				GiveKeyword("temple");
				Msg("You are looking for the School?<br/>It's near the Church.<br/>It's not that far from here.");
				Msg("There are teachers teaching magic and swordsmanship in the School.<br/>So you can ask them if you need anything from them.<br/>They will kindly explain to you about many things.");
				Msg("If you can afford it,<br/>perhaps it's worthwhile to pay the tuition fee and take a class.");
				break;

			case "skill_windmill":
				Msg("Hmm... Let me see. I think it's a skill for weaving fabric<br/>using the Windmill as a spinning wheel.");
				Msg("Hahaha. It was a joke. Only a joke.");
				Msg("I know it's silly, ha ha ha...<br/>I am sorry, I won't do that again.");
				break;

			case "skill_campfire":
				// When the beginner quests changed, he no longer removes the keyword. Instead he says "I'm sorry.<br/>I don't have the time to talk about that right now."
				// He also checks for race, but until we get the respective quests in for other races, we'll leave it out
				if (Player.Skills.Has(SkillId.Campfire))
				{
					RemoveKeyword("skill_campfire");
					Msg("Ha ha. Now you know how to use the Campfire skill.<br/>It's something I didn't want to teach you, to be honest,<br/>but I am impressed that you have mastered it so well.<br/>With this, another young adventurer is born today, ha ha.");
				}
				else
				{
					Msg("Do you by chance know about the Campfire Skill?");
					Msg("If you start a fire using the Campfire Skill,<br/>people would come by one at a time after seeing the bright fire from afar...");
					Msg("People share what they have in their inventory<br/>and spend long summer nights sharing stories about their adventures.");
					Msg("But really, if all travelers knew the Campfire Skill,<br/>inn owners like myself would have to pack up and find a different profession.");
				}
				break;

			case "shop_restaurant":
				GiveKeyword("shop_grocery");
				Msg("Yes, many people eat around here, but...");
				Msg("Are you looking for something to eat?<br/>Hmm... It just happens that I'm low on supplies.<br/>Could you go and see Caitin at the Grocery Store yourself?");
				Msg("All the food served in the Inn<br/>is made and brought in<br/>by Caitin.");
				break;

			case "shop_armory":
				GiveKeyword("shop_smith");
				Msg("Weapons Shop?<br/>There isn't one in this town, but...<br/>If you are in need of some weapons,<br/>you might want to visit the Blacksmith's Shop right over there.");
				Msg("Tell Ferghus I sent you<br/>and he'll take care of you.");
				break;

			case "shop_cloth":
				GiveKeyword("shop_misc");
				Msg("Are you looking for something to wear?<br/>Hmm... What you are wearing right now seems good enough.");
				Msg("You must be interested in fashion.<br/>It would be quite hard to find a better outfit than what you have.<br/>Nevertheless, you can go talk to Malcolm at the General Shop.");
				break;

			case "shop_bookstore":
				Msg("Are you looking for a book?<br/>I saw Lassar<br/>selling some spell books.");
				Msg("However,<br/>most people don't actually read them.<br/>They just carry them around.");
				Msg("It makes me sad to see books becoming<br/>nothing more than fashion items.");
				break;

			case "shop_goverment_office":
				Msg("A town office?<br/>Surely you are new to this town.<br/>There is no town office here.");
				Msg("But if you want to get some help<br/>or talk to the town elders,<br/>you'd want to pay a visit to<br/>Chief Duncan.");
				break;

			case "graveyard":
				Msg("The graveyard is on the hill behind the Chief's House.<br/>I heard it's the resting place for the brave souls<br/>who fought against the Fomors to defend this village,<br/>the shelter of the descendants of Ulaid.");
				Msg("Unfortunately, I have been on the road for a long time,<br/>so it's hard for me to tell you all the details.<br/>Perhaps it's best that you talk to the Chief about this.");
				break;

			case "bow":
				Msg("Hmm... I knew you were interested in archery.<br/>Bows are sold at the Blacksmith's Shop.<br/>Go and ask Ferghus. He will be able to help you find it.");
				Msg("To give you some pointers,<br/>Long Bows or Composite Bows<br/>perform better than Short Bows.");
				Msg("Oh, you already figured from the price. I see.");
				break;

			case "lute":
				Msg("Do you need a lute?<br/>I would really like to give you one,<br/>but so many people are asking these days.<br/>So, I can't make an exception... Even if it's you, hahaha.");
				Msg("If you visit the General Shop up there,<br/>you'll be able to find a few instruments.<br/>They are decent enough to play<br/>even though you may not find the lute you're looking for.");
				break;

			case "complicity":
				Msg("Some people might... if their business is slow.<br/>But personally I do not feel like doing it.");
				Msg("Hmm... Did you ask why Nora stays outside?");
				Msg("Ha ha, I think you've misunderstood something here.");
				break;

			case "tir_na_nog":
				Msg("Tir Na Nog? Ha ha...");
				Msg("Well... I don't know if it's good to talk about it<br/>standing here like this...");
				Msg("Because it may take a while to tell you what I know.<br/>Hahaha. Let's talk about it another time.");
				break;

			case "mabinogi":
				Msg("Wow, you knew about this?<br/>Mabinogi...<br/>As a matter of fact, people have different ideas about it<br/>depending on where they come from.");
				Msg("In the good old days of mine,<br/>I traveled to many different places.<br/>I was really surprised seeing how people from different regions<br/>talk differently about Mabinogi.");
				Msg("Some of them say<br/>it is an ancient lullaby with a story in it.");
				Msg("Others say it's a song of praise dedicated to<br/>the heroes and warriors who built<br/>Erinn of this day.");
				Msg("And I heard from someone that<br/>any song can be Mabinogi<br/>as long as it is old<br/>and sung by bards.");
				Msg("Oh, I am sorry. Am I talking too much?<br/>It might have been boring, but I'm just telling you what I heard.");
				break;

			case "musicsheet":
				GiveKeyword("shop_misc");
				Msg("Music Score?<br/>I thought the Scores were sold up at the General Shop.<br/>I can't imagine Malcolm just watching and doing nothing<br/>until he's out of supplies at the shop.");
				Msg("If he says he's short, then you can come back to me.<br/>I bought some from Malcolm long ago.<br/>I can probably sell some<br/>at a similar price.");
				break;

			case "g3_DarkKnight":
				Msg("...I heard about them long ago<br/>when I used to travel around.");
				Msg("Dark Knights are the ones who<br/>betrayed humans and attacked their own brothers and sisters<br/>alongside Fomors during the war at Mag Tuireadh in the past...");
				Msg("...The previous tyrant Breath is said to have been a Dark Knight as well...");
				break;

			default:
				RndMsg(
					"?",
					"I don't know about that.",
					"To be honest, I don't know.",
					"I'd love to listen to you, but about something else.",
					"I'm afraid this conversation isn't very interesting to me."
				);
				ModifyRelation(0, 0, Random(3));
				break;
		}
	}
}

public class PiarasShop : NpcShopScript
{
	public override void Setup()
	{
		Add("Book", 1015); // Seal Stone Research Almanac : Rabbie Dungeon
		Add("Book", 1016); // Seal Stone Research Almanac : Ciar Dungeon
		Add("Book", 1017); // Seal Stone Research Almanac : Dugald Aisle
		Add("Book", 1037); // Experiencing the Miracle of Resurrection with 100 Gold
		Add("Book", 1038); // Nora Talks about the Tailoring Skill
		Add("Book", 1039); // Easy Part-Time Jobs
		Add("Book", 1041); // A Story About Eggs
		Add("Book", 1048); // My Fluffy Life with Wool
		Add("Book", 1049); // The Holy Water of Lymilark
		Add("Book", 1054); // Behold the Dungeon - Advice for Young Generations
		Add("Book", 1055); // The Road to Becoming a Magic Warrior
		Add("Book", 1056); // How to Enjoy Field Hunting
		Add("Book", 1057); // Introduction to Field Bosses
		Add("Book", 1058); // Understanding Wisps
		Add("Book", 1062); // The Greedy Snow Imp
		Add("Book", 1124); // An Easy Guide to Taking Up Residence in a Home
		Add("Book", 1505); // The World of Handicrafts

		Add("Gift", 52008); // Anthology
		Add("Gift", 52009); // Cubic Puzzle
		Add("Gift", 52011); // Socks
		Add("Gift", 52017); // Underwear Set
		Add("Gift", 52018); // Hammer
	}
}
