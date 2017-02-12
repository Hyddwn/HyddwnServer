//--- Aura Script -----------------------------------------------------------
// Mentum
//--- Description -----------------------------------------------------------
// Blacksmith and Weapons shop dealer
//---------------------------------------------------------------------------

public class MentumScript : NpcScript
{
	public override void Load()
	{
		SetRace(10002);
		SetName("_mentum");
		SetBody(height: 1.2f, weight: 1.2f, upper: 1.2f, lower: 1.2f);
		SetFace(skinColor: 16, eyeType: 12, eyeColor: 76, mouthType: 2);
		SetLocation(600, 104421, 97757, 188);
		SetGiftWeights(beauty: 2, individuality: -1, luxury: 1, toughness: 2, utility: 0, rarity: 2, meaning: 1, adult: -1, maniac: 1, anime: 0, sexy: 2);

		EquipItem(Pocket.Face, 4907, 0x002E055F, 0x00D19D62, 0x005E741B);
		EquipItem(Pocket.Hair, 4043, 0xFF666666, 0xFF666666, 0xFF666666);
		EquipItem(Pocket.Armor, 13021, 0x00656E54, 0x009B7D06, 0x00656E54);
		EquipItem(Pocket.Glove, 16508, 0x00656E54, 0x00656E54, 0x00656E54);
		EquipItem(Pocket.Shoe, 17509, 0x00656E54, 0x00656E54, 0x00656E54);
		EquipItem(Pocket.RightHand2, 40002, 0x00000000, 0x00000000, 0x00000000);

		AddPhrase("I need to train the Neko Knights.");
		AddPhrase("Well, I'd love to eat some silvervine fruit");
		AddPhrase("I need to get rid of those rats as soon as possible.");
		AddPhrase("I've been shirking on my knight training recently...");
	}

	protected override async Task Talk()
	{
		Msg(SetDefaultName("Mentum"));

		await Intro(L("His sturdy armor and dark colored hair gives a strong impression.<br/>Despite being on an island far from the Uladh continent,<br/>his armor is adorned with many Celtic Knots."));

		Msg("Hello, I'm Mentum.", Button("Start a Conversation", "@talk"), Button("Shop", "@shop"), Button("Repair", "@repair"), Button("Upgrade", "@upgrade"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());
				await Conversation();
				break;

			case "@shop":
				Msg("While I train the Neko Knight Trainees, I try not to have extra weapons.<br/>I do not want to destroy the atmosphere of this island with conspicuous weapons.");
				OpenShop("MentumShop");
				return;
				
			case "@repair":
				Msg(L("Lets see if I can repair it.<br/><repair rate='92' stringid='(*/smith_repairable/*)' />"));

				while (true)
				{
					var repair = await Select();

					if (!repair.StartsWith("@repair"))
						break;

					var result = Repair(repair, 92, "/smith_repairable/");
					if (!result.HadGold)
					{
						RndMsg(
							L("It's difficult for me to repair it by myself."),
							L("I'll fix it as soon as you bring the repair fee.")
						);
					}
					else if (result.Points == 1)
					{
						if (result.Fails == 0)
							RndMsg(
								L("I fixed one point."),
								L("The repair is finished."),
								L("It is finished.")
							);
						else
							Msg(L("Sorry.<br/>The repair seems to have failed."));								
					}
					else if (result.Points > 1)
					{
						if (result.Fails == 0)
							RndMsg(
								L("The repair is finished."),
								L("The repair has been fully completed."),
								L("It is completed.")
							);
						else
							Msg(string.Format(L("I finished the repair, but {0} points could not be repaired."), result.Fails, result.Successes));
					}
				}

				Msg(L("See you next time.<repair hide='true'/>"));
				break;

			case "@upgrade":
				Msg(L("Please choose an item to upgrade.<br/>I think you should know the number of times it can be upgraded.<br/>So I don't think I need to explain how this works.<upgrade />"));

				while (true)
				{
					var reply = await Select();

					if (!reply.StartsWith("@upgrade:"))
						break;

					var result = Upgrade(reply);
					if (result.Success)
						Msg(L("The upgrade is finished.<br/>Do you need any other modifications?"));
					else
						Msg(L("(Error)"));
				}

				Msg(L("If you need another upgrade, please visit again.<upgrade hide='true'/>"));
				break;
		}

		End("Goodbye, <npcname/>. I'll see you later!");
	}

	private void Greet()
	{
		if (Memory <= 0)
		{
			Msg(FavorExpression(), L("<username/>, I look forward to our first meeting.<br/>If we have met before, please forgive me.<br/>I do not have a great memory."));
		}
		else if (Memory == 1)
		{
			Msg(FavorExpression(), L("Have we met before?<br/>Anyway, I am pleased to meet you."));
		}
		else if (Memory == 2)
		{
			Msg(FavorExpression(), L("<username/>, are you also interested in training?"));
		}
		else if (Memory <= 6)
		{
			Msg(FavorExpression(), L("I've seen you a lot recently, <username/>."));
		}
		else
		{
			Msg(FavorExpression(), L("Yo! <username/>.<br/>What can I do for you today?"));
		}

		UpdateRelationAfterGreet();
	}

	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				if (Memory >= 15 && Favor >= 50 && Stress <= 5)
				{
					RndFavorMsg(
						"I hope to train as many trainees as possible.<br/>I think I would also like to have more weapons.<br/>However, I do not wish to destroy the peace of the island with unnecessary greed.",
						"Sometimes I find silvervine fruit in front of my house.<br/>I don't think its a lost item, but someone is going out of their way to put it here.<br/>I don't know who is doing this, but I am grateful for it.",
						"I'm aware that my tone does not suit this island.<br/>However, I've its hard to change when I've been like this for a long time.<br/>I personally feel that my tone suits myself. But do you think it does...?"
					);
					ModifyRelation(Random(2), 0, Random(2));
				}
				else if (Memory >= 15 && Favor >= 30 && Stress <= 5)
				{
					RndFavorMsg(
						"I hope to train as many trainees as possible.<br/>I think I would also like to have more weapons.<br/>However, I do not wish to destroy the peace of the island with unnecessary greed.",
						"Sometimes I find silvervine fruit in front of my house.<br/>I don't think its a lost item, but someone is going out of their way to put it here.<br/>I don't know who is doing this, but I am grateful for it.",
						"I'm aware that my tone does not suit this island.<br/>However, I've its hard to change when I've been like this for a long time.<br/>I personally feel that my tone suits myself. But do you think it does...?"
					);
					ModifyRelation(Random(2), 0, Random(2));
				}
				else if (Favor >= 10 && Stress <= 10)
				{
					RndFavorMsg(
						"I hope to train as many trainees as possible.<br/>I think I would also like to have more weapons.<br/>However, I do not wish to destroy the peace of the island with unnecessary greed.",
						"Sometimes I find silvervine fruit in front of my house.<br/>I don't think its a lost item, but someone is going out of their way to put it here.<br/>I don't know who is doing this, but I am grateful for it.",
						"I'm aware that my tone does not suit this island.<br/>However, I've its hard to change when I've been like this for a long time.<br/>I personally feel that my tone suits myself. But do you think it does...?"
					);
					ModifyRelation(Random(2), Random(2), Random(2));
				}
				else if (Favor <= -10)
				{
					Msg(FavorExpression(), "Yeah, I don't really want to talk about myself to you, <username/>.");
					ModifyRelation(Random(2), 0, Random(1, 3));
				}
				else if (Favor <= -30 && Stress <= 10)
				{
					Msg(FavorExpression(), "I do not want to be bothered right now.");
					ModifyRelation(Random(2), 0, Random(1, 3));
				}
				else if (Favor <= -30 && Stress > 10)
				{
					Msg(FavorExpression(), "I do not want to be bothered right now.");
					ModifyRelation(Random(2), -Random(2), Random(1, 4));
				}
				else
				{
					RndFavorMsg(
						"The elder is a jokester, Dagon prepares great cuisine, and Bastet is very lively.<br/>I like the simple residents and of the cats, its very peaceful here.<br/>I want to protect this place for my own happiness.",
						"I'm not good with the elder's jokes.<br/>However, if you think carefully, the joke seems to have some sort of hidden message in it that is hard to pick out.<br/>It's true."
					);
					ModifyRelation(Random(2), 0, Random(3));
				}
				break;

			case "rumor":
				if (Memory >= 15 && Favor >= 50 && Stress <= 5)
				{
					RndFavorMsg(
						"I don't know when this started, but rats began to appear unnoticed by everyone.<br/>There has to be some secret to their appearance that I don't know.",
						"Did you check out the metallurgy collection site?<br/>If you're just looking for valuable jewels it may not seem appealing<br/>but, if you know true value, the area cannot be ignored.",
						"There is absolutely no way rats should exist in this village in any form.<br/>Even if they are friends of yours, <username/>.<br/>regardless of what you think <username/>, this island has tradition, culture, and principles."
					);
					ModifyRelation(Random(2), 0, Random(2));
				}
				else if (Memory >= 15 && Favor >= 30 && Stress <= 5)
				{
					RndFavorMsg(
						"I don't know when this started, but rats began to appear unnoticed by everyone.<br/>There has to be some secret to their appearance that I don't know.",
						"Did you check out the metallurgy collection site?<br/>If you're just looking for valuable jewels it may not seem appealing<br/>but, if you know true value, the area cannot be ignored.",
						"There is absolutely no way rats should exist in this village in any form.<br/>Even if they are friends of yours, <username/>.<br/>regardless of what you think <username/>, this island has tradition, culture, and principles."
					);
					ModifyRelation(Random(2), 0, Random(2));
				}
				else if (Favor >= 10 && Stress <= 10)
				{
					RndFavorMsg(
						"I don't know when this started, but rats began to appear unnoticed by everyone.<br/>There has to be some secret to their appearance that I don't know.",
						"Did you check out the metallurgy collection site?<br/>If you're just looking for valuable jewels it may not seem appealing<br/>but, if you know true value, the area cannot be ignored.",
						"There is absolutely no way rats should exist in this village in any form.<br/>Even if they are friends of yours, <username/>.<br/>regardless of what you think <username/>, this island has tradition, culture, and principles."
					);
					ModifyRelation(Random(2), Random(2), Random(2));
				}
				else if (Favor <= -10)
				{
					RndFavorMsg(
						"I've been watching the ocean from the town square.<br/>I was actually trying to watch the movement of the rats at night,<br/>but I ended up gazing at the water instead.<br/><username/>, if you took a gander you would understand why I got distracted.",
						"The other day Dagon fished up a silvervine drink.<br/>Silvervine drink... even just thinking about it seems fantastic.<br/>Dagon didn't let me have a single drop of it...<p/>However! There is no way a man such as myself would be depressed by that.<br/>I mean there is no way he really fished up such a thing.",
						"The village elder once belonged to the Neko Knights like me.<br/>I can hardly imagine seeing him being a part of that looking at him now.<br/>But, I do feel that the elder is a wonderful person.<br/>It does scare me that he is able to hide his past disposition so easily though."
					);
					ModifyRelation(Random(2), 0, Random(1, 3));
				}
				else if (Favor <= -30 && Stress <= 10)
				{
					Msg(FavorExpression(), "Have you been to the southern island?<br/>I've been there, I don't think I'll use my time to go there again in its current state.");
					ModifyRelation(Random(2), 0, Random(1, 3));
				}
				else if (Favor <= -30 && Stress > 10)
				{
					Msg(FavorExpression(), "Have you been to the southern island?<br/>I've been there, I don't think I'll use my time to go there again in its current state.");
					ModifyRelation(Random(2), -Random(2), Random(1, 4));
				}
				else
				{
					RndFavorMsg(
						"I've been watching the ocean from the town square.<br/>I was actually trying to watch the movement of the rats at night,<br/>but I ended up gazing at the water instead.<br/><username/>, if you took a gander you would understand why I got distracted.",
						"The other day Dagon fished up a silvervine drink.<br/>Silvervine drink... even just thinking about it seems fantastic.<br/>Dagon didn't let me have a single drop of it...<p/>However! There is no way a man such as myself would be depressed by that.<br/>I mean there is no way he really fished up such a thing.",
						"The village elder once belonged to the Neko Knights like me.<br/>I can hardly imagine seeing him being a part of that looking at him now.<br/>But, I do feel that the elder is a wonderful person.<br/>It does scare me that he is able to hide his past disposition so easily though."
					);
					ModifyRelation(Random(2), 0, Random(3));
				}
				break;
				
			case "about_study":
				Msg("In order to receive training you must first be qualified.<br/>Besides, I'm not thinking about taking any new trainees right now.");
				break;

			case "about_arbeit":
				Msg("Not implemented.");
				break;

			// TODO: Implement Quest id 601101 Talk with Mentum
			//case "Recommendation_Nekojima":
			//	if (QuestActive(601101) && QuestCompleted(601101))
			//	{
			//		Msg("I already told <username/>, how to obtain a letter of recommendation.<br/>Did you still need my letter of recommendation?");
			//	}
			//	else
			//	{
			//		Msg("You want to prove that you like cats?<br/>Can it not be said that the fact that you came here proves that already?<br/>Well... actually the rats are overpopulating on the southern island, so its in trouble.<br/>All of the Neko Knight meetings have been focused on getting rid of the rats occupying that island.");
			//		Msg("One way to prove you like cats is to eliminate rats.<br/>Alright! Go to that island and eliminate one rat of each type<br/>and I'll write a letter of recommendation!");
			//		StartQuest(601101);
			//	}
			//	break;

			case "shop_misc":
				Msg("You're looking for Bastet. She is right over there.");
				break;

			case "shop_grocery":
				GiveKeyword("shop_restaurant");
				Msg("Dagon sells cooking ingredients.");
				break;

			case "shop_restaurant":
				Msg("We don't have a restaurant on the island.<br/>Dagon does sometimes cook for the island's inhabitants,<br/>but its rare for him to cook for an outsider.");
				break;

			case "shop_healing":
				Msg("Oh you mean Tahthis?<br/>People sometimes mistake Dagon for the elder because of his tone.<br/>but, the elder is easy to recognize from afar.");
				Msg("He has some truly boring jokes,<br/>but he is also as tranquil as the earth and as sharp as a lightning bolt.<br/>The elder also has emergency first-aid supplies.");
				break;

			case "shop_armory":
				Msg("If you need weapons, open the store with the 'shop' button.<br/>Of course, I also offer repairs.");
				break;

			default:
				if (Memory >= 15 && Favor >= 30 && Stress <= 5)
				{
					RndFavorMsg(
						"I'm obsessed with rat elimination, so I don't know much about other things.<br/>I'll have to take a look at that.",
						"Is that a story you like <username/>?<br/>I'm sorry, but I am not very interested."
					);
					ModifyRelation(0, 0, Random(2));
				}
				else if (Favor >= 10 && Stress <= 10)
				{
					RndFavorMsg(
						"I'm obsessed with rat elimination, so I don't know much about other things.<br/>I'll have to take a look at that.",
						"Is that a story you like <username/>?<br/>I'm sorry, but I am not very interested."
					);
					ModifyRelation(0, 0, Random(2));
				}
				else if (Favor <= -10)
				{
					Msg(FavorExpression(),"I can't talk now.<br/>Talk with the other residents.");
					ModifyRelation(0, 0, Random(4));
				}
				else if (Favor <= -30)
				{
					Msg(FavorExpression(),"I can't talk now.<br/>Talk with the other residents.");
					ModifyRelation(0, 0, Random(5));
				}
				else
				{
					RndFavorMsg(
						"I don't think that's something to talk to me about.",
						"We should talk about other things.",
						"Is that topic popular in Uladh?"
					);
					ModifyRelation(0, 0, Random(3));
				}
				break;
		}
	}

	protected override async Task Gift(Item item, GiftReaction reaction)
	{
		Msg(SetDefaultName("Mentum"));
	
		if (item.Info.Id >= 64057 && item.Info.Id <= 64066) // Nekojima Gems
		{
			RndMsg(
				"I appreciate this!<br/>I must express my gratitude,<br/>I'm not sure what else to say.",
				"I'm impressed <username/>, to think you would be this thoughtful."
			);
			return;
		}
		else if (item.Info.Id == 50085) // Silvervine Fruits
		{
			RndMsg(
				"This is a silvervine fruit!<br/>Did you really bring this for me?<br/>Oh, thank you!",
				"Silvervine fruit!!...<br/>Did you get them from the field over there?<br/>Thank you!",
				"Are you the one who has been bringing the silvervine fruits until now, <username/>?<br/>No, it doesn't matter.<br/>Thank you, <username/>."
			);
			return;
		}		
			
		switch (reaction)
		{
			case GiftReaction.Love:
				RndMsg(
					"Thank you very much.<br/>I don't know how to express my gratitude,<br/>but again, thank you.",
					"You bothered bringing that for me...<br/><username/>, you really are are quite friendly."
				);
				break;

			case GiftReaction.Like:
				RndMsg(
					"Let's not hold back.",
					"<username/>, I can feel your sincerity."
				);
				break;

			case GiftReaction.Dislike:
				RndMsg(
					"...I'll hand this to other visitors.",
					"When you don't know what to bring me...<br/>Just bring a silvervine fruit."
				);
				break;

			default: // GiftReaction.Neutral
				Msg("Thank you.");
				break;
		}
	}
}

public class MentumShop : NpcShopScript
{
	public override void Setup()
	{
		Add("Weapon", 40022); // Gathering Axe
		Add("Weapon", 40023); // Gathering Knife
		Add("Weapon", 40026); // Sickle
		Add("Weapon", 40027); // Weeding Hoe
		Add("Weapon", 40003); // Short Bow
		Add("Weapon", 40031); // Crossbow
		Add("Weapon", 40002); // Wooden Blade
		Add("Weapon", 45001, 100); // Arrow x100
		Add("Weapon", 45001, 20); // Arrow x20
		Add("Weapon", 45002, 50); // Bolt x50
		Add("Weapon", 45002, 200); // Bolt x200
	}
}
