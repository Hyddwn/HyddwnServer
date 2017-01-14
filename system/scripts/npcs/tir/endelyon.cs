//--- Aura Script -----------------------------------------------------------
// Endelyon
//--- Description -----------------------------------------------------------
// Priestess in front of Tir's Church
//---------------------------------------------------------------------------

public class EndelyonScript : NpcScript
{
	public override void Load()
	{
		SetRace(10001);
		SetName("_endelyon");
		SetBody(height: 1.06f);
		SetFace(skinColor: 17);
		SetStand("human/female/anim/female_natural_stand_npc_Endelyon");
		SetLocation(1, 5975, 36842, 0);
		SetGiftWeights(beauty: 2, individuality: 1, luxury: 1, toughness: 0, utility: 0, rarity: 0, meaning: 2, adult: 0, maniac: 0, anime: 0, sexy: -1);

		EquipItem(Pocket.Face, 3900, 0x00F49D31, 0x00605765, 0x0000B8C3);
		EquipItem(Pocket.Hair, 3022, 0x005E423E, 0x005E423E, 0x005E423E);
		EquipItem(Pocket.Armor, 15009, 0x00303133, 0x00C6D8EA, 0x00DBC741);
		EquipItem(Pocket.Shoe, 17015, 0x00303133, 0x00A0927D, 0x004F548D);

		AddPhrase("Hm... Something doesn't feel right.");
		AddPhrase("Why do people like such things?");
		AddPhrase("It's so hard to do this all by myself!");
		AddPhrase("I really need some help here...");
	}

	protected override async Task Talk()
	{
		SetBgm("NPC_Endelyon.mp3");

		await Intro(L("An elegant young woman in the simple black dress of a<br/>Lymilark priestess stands in front of the church."));

		// May I help you on anything else?<button title='End Conversation' keyword='@end' />
		Msg("May I help you?", Button("Start a Conversation", "@talk"), Button("Shop", "@shop"), Button("Modify Item", "@upgrade"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());

				var today = ErinnTime.Now.ToString("yyyyMMdd");
				if (today != Player.Vars.Perm["endelyon_title_gift"])
				{
					string message = null;

					switch (Player.Title)
					{
						case 10060: // is a friend of Deian
							message = L("Do you like boiled eggs?<p>I think these will help when you get hungry.");
							break;

						case 10062: // is a friend of Nora
							message = L("I prepared some boiled eggs.<br/>Would you like to try one?");
							break;
					}

					if (message != null)
					{
						Player.Vars.Perm["endelyon_title_gift"] = today;

						Player.GiveItem(50126); // Hard-Boiled Egg
						Player.Notice(L("Received Hard-Boiled Egg from Endelyon."));
						Player.SystemMsg(L("Received Hard-Boiled Egg from Endelyon."));

						Msg(message);
					}
				}

				if (Player.IsUsingTitle(11001))
				{
					Msg("So you rescued Morrighan the goddess, <username/>?<br/>But the goddess is supposed to be at Tir Na Nog.<br/>Does that mean you've been to Tir Na Nog, <username/>?");
					Msg("Hmm... Well, then, that must mean that I am right now talking to an extraordinary individual, aren't I? Haha.");
				}
				else if (Player.IsUsingTitle(11002))
				{
					Msg("I already heard the news! You became the Guardian of Erinn.<br/>The whole town seems to be talking about it. Hehe...<br/>Congratulations!");
				}

				await Conversation();
				break;

			case "@shop":
				Msg("What are you looking for?");
				OpenShop("EndelyonShop");
				return;

			case "@upgrade":
				Msg("Are you asking me...to modify your item?<br/>Honestly, I am not sure if I can, but if you still want me to, I'll give it a try.<br/>Please choose an item to modify. <upgrade />");

				while (true)
				{
					var reply = await Select();

					if (!reply.StartsWith("@upgrade:"))
						break;

					var result = Upgrade(reply);
					if (result.Success)
						Msg("That came out better than I thought.<br/>Was it supposed to be this easy?<br/>Do you want to modify anything else?");
					else
						Msg("(Error)");
				}

				Msg("Do you want me to stop...? Well, then... Next time...<br/><upgrade hide='true'/>");
				break;
		}

		End();
	}

	private void Greet()
	{
		if (Memory <= 0)
		{
			Msg(FavorExpression(), L("I don't think we've ever met. Nice to meet you."));
		}
		else if (Memory == 1)
		{
			Msg(FavorExpression(), L("Glad to see you again."));
		}
		else if (Memory == 2)
		{
			Msg(FavorExpression(), L("Hello.<br/><username/>, I knew you would be back."));
		}
		else if (Memory <= 6)
		{
			Msg(FavorExpression(), L("Nice to meet you again, <username/>."));
		}
		else
		{
			Msg(FavorExpression(), L("<username/>, may the blessings of Lymilark be with you."));
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
					Msg(FavorExpression(), "You must be interested in Lymilark.<br/>Lymilark is the head of the three main gods worshipping Aton Cimeni, the Absolute God Almighty.");
					Msg("Lymilark also represents the Uladh Continent where Tir Chonaill is located.<br/>Whenever the flames of war engulfed people in the past,<br/>Lymilark rose to save humans from misery.");
					ModifyRelation(Random(2), 0, Random(2));
				}
				else if (Memory >= 15 && Favor >= 30 && Stress <= 5)
				{
					Msg(FavorExpression(), "Lymilark is one of three main gods of Erinn.<br/>There are three virtues that form as the pillars of the land: love, freedom and peace.<br/>Lymilark presides over Love.");
					Msg("If I had not experienced Lymilark's love during childhood,<br/>I would have grown up to be just an ordinary girl like everyone else.");
					ModifyRelation(Random(2), 0, Random(2));
				}
				else if (Favor >= 10 && Stress <= 10)
				{
					Msg(FavorExpression(), "We worship and serve gods, not necessarily to be blessed<br/>but to be one step closer to the truth and possess a peace of mind.<br/>Unfortunately, most of the people don't understand this point.<br/>What about you, <username/>?");
					ModifyRelation(Random(2), Random(2), Random(2));
				}
				else if (Favor <= -10)
				{
					Msg(FavorExpression(), "Where do you come from, <username/>?<br/>I wasn't born in Tir Chonaill, but I really love this place.<br/>Since you already saw how beautiful this town is,<br/>I believe you can understand what I mean.");
					ModifyRelation(Random(2), 0, Random(1, 3));
				}
				else if (Favor <= -30 && Stress <= 10)
				{
					Msg(FavorExpression(), "I know that you ask questions with good intention, but I am a priestess.<br/>Think about that for a moment, and please stop asking me inappropriate questions.");
					ModifyRelation(Random(2), 0, Random(1, 3));
				}
				else if (Favor <= -30 && Stress > 10)
				{
					Msg(FavorExpression(), "It seems you are more interested in me, a servant of the gods,<br/>and not in my teachings about the gods.<br/>This is unacceptable.");
					ModifyRelation(Random(2), -Random(2), Random(1, 4));
				}
				else
				{
					Player.GiveKeyword("temple");
					Msg(FavorExpression(), "I don't have much knowledge, but I am here if you need help.");
					ModifyRelation(Random(2), 0, Random(3));
				}
				break;

			case "rumor":
				if (Memory >= 15 && Favor >= 50 && Stress <= 5)
				{
					Msg(FavorExpression(), "People might complain about Bebhinn.<br/>However, she helps people in her own way,<br/>and never means any harm.<br/>If you meet anyone with a grudge against her,<br/>please tell remind them of this.");
					ModifyRelation(Random(2), 0, Random(2));
				}
				else if (Memory >= 15 && Favor >= 30 && Stress <= 5)
				{
					Msg(FavorExpression(), "Priest Meven is not an ordinary man.<br/>If he had stayed with the Pontiff's Court, he could have earned admiration and respect from many.<br/>Instead, he chose to help people in a remote town hidden in the middle of the mountains.<br/>It's really not as easy as it sounds.");
					ModifyRelation(Random(2), 0, Random(2));
				}
				else if (Favor >= 10 && Stress <= 10)
				{
					Msg(FavorExpression(), "Hmm, want to hear something?<br/>When someone runs out of HP and falls to the ground,<br/>they may lose an item.<br/>Lost items can be retrieved from Town Offices in big cities like Dunbarton,<br/>but it can be quite costly.");
					Msg("Worse, you can't do it over and over.<br/>I hear the clerks at Town Offices sometimes get upset and refuse to return found items.<br/>It's better to get up immediately and recover your lost items yourself,<br/>but that probably won't happen often.");
					Msg("Some folks went to great expense to discover a way around this.<br/>Turns out that the Holy Water of Lymilark used at Churches prevents items from getting lost!<br/>It also slows down durability loss.");
					Msg("All you have to do is spinkle some Holy Water of Lymilark onto your important items to bless them.<br/>But there's always a catch.<br/>When you fall to the ground, the impact may cause the blessings on the items to disappear.<br/>The chance is about 50-50.");
					Msg("Even so, let me know if you need some Holy Water of Lymilark.<br/>I can't give it to you for free,<br/>but I can let you have some if you help me.");
					ModifyRelation(Random(2), Random(2), Random(2));
				}
				else if (Favor <= -10)
				{
					Msg(FavorExpression(), "Nora is a cute girl who's also very tough.<br/>Whenever I talk with her, I'm amazed that she is so positive and determined despite her hardships.<br/>If only she realized how highly Malcolm thinks of her...");
					ModifyRelation(Random(2), 0, Random(1, 3));
				}
				else if (Favor <= -30 && Stress <= 10)
				{
					Msg(FavorExpression(), "I'm worried that Ferghus drinks too much.<br/>Looks like he's drinking almost every day.<br/>Working all day and drinking all night... I'm really worried about his health.");
					ModifyRelation(Random(2), 0, Random(1, 3));
				}
				else if (Favor <= -30 && Stress > 10)
				{
					Msg(FavorExpression(), "I don't think it's a good idea to talk about people behind their backs.<br/>That's not the way to win friends in Tir Chonaill.");
					ModifyRelation(Random(2), -Random(2), Random(1, 4));
				}
				else
				{
					Player.GiveKeyword("shop_healing");
					Msg(FavorExpression(), "Have you met Dilys? She's the town's Healer.<br/>Walk along the road heading northeast, and you will find the Healer's House.<br/>Make sure to meet her if you pass by there.");
					ModifyRelation(Random(2), 0, Random(3));
				}
				break;

			case "about_skill":
				if (!Player.QuestActive(204002) && !Player.QuestCompleted(204002))
				{
					//StartQuest(204002); // Quest log: http://pastebin.com/1c9XFc5Y
					Msg("Skills? I don't know if I can call it a skill, but you can gather eggs from hens.<br/>Some of the less fortunate among the faithful come to the Church seeking the blessings of Lymilark.<br/>To relieve their hunger, we prepare boiled eggs for them.");
					Msg("If you bring me eggs, I'll reward you for your work.<br/>This will be a good time for you to learn how to collect eggs, too.");
				}
				else
				{
					if (!Player.HasSkill(SkillId.Tailoring))
					{
						Player.GiveKeyword("skill_tailoring");
						Msg("I'm sorry, but I don't know much about skills.<br/>The only skill I know is tailoring.<br/>If you want to know more about making clothes,<br/>talk with Caitin at the Grocery Store.<br/>She's the best tailor in town.");
					}
					else
					{
						Msg("Did you make the clothes you are wearing right now with the Tailoring skill?<br/>If not, I'm sorry. It's just that your clothes are very beautiful.");
					}
				}
				break;

			case "shop_misc":
				Msg("Everybody knows the General Shop sells good products that are also durable.<br/>Want to see for yourself? Then go to the General Shop and talk to Malcolm.");
				break;

			case "shop_grocery":
				Msg("Caitin's mom actually owns the store, but her eyes have been failing,<br/>so Caitin manages and takes care of her mom all by herself.<br/>It must be really tough, but she never complains.");
				Msg("Oh oops, I forgot to tell you. Her store is located near the Square.<br/>I'm sure you will find it quickly,<br/>since food ingredients are displayed outside and there's a signboard.");
				break;

			case "shop_healing":
				Msg("Small wounds can be cured through spells or potions, but not serious injuries.");
				Msg("For a serious injury, head to the Healer's House.<br/>Tell Dilys about the injury, and she will take good care of you.");
				break;

			case "shop_inn":
				Msg("This town used to let visitors stay with private citizens,<br/>but the number of visitors shot up over the last few years, so Piaras opened an inn.<br/>To get there, walk downhill from the Square that leads to the town entrance.");
				break;

			case "shop_bank":
				Msg("If you are curious about rumors around town, go talk to Bebhinn.<br/>She knows all the latest gossip.");
				break;

			case "shop_smith":
				Msg("The Blacksmith's Shop? Are you looking for Ferghus?<br/>Oh... That's pretty far, actually...<br/>I think you are heading in the wrong direction.");
				break;

			case "skill_range":
				Msg("Hmm... It's true we humans obtain meat and hides from animals,<br/>but reckless hunting will cause the sorrow of Lymilark and the wrath of Kernunnos.<br/>Even if you learn a skill, I hope you don't use it to hunt animals.");
				break;

			case "skill_insrument":
				Player.GiveKeyword("lute");
				Msg("Hmm. I learned how to play the organ and the lute from an early age,<br/>but I don't think books can teach you those skills.<br/>It's something you must learn by trying it with your own two hands.");
				Msg("Lutes are common instruments, so talk to Malcolm about buying one.");
				break;

			case "skill_composing":
				if (Player.HasSkill(SkillId.Composing))
				{
					Player.RemoveKeyword("skill_composing");
					Msg("So you know about the Composing skill.<br/>How about writing a song for a loved one?");
				}
				else
				{
					if (!Player.QuestActive(20002))
					{
						//StartQuest(20002); // Quest Log: http://pastebin.com/e6JLQi55
						Player.RemoveKeyword("skill_composing");
						Msg("What? Bebhinn told you to ask me about the Composing skill?<br/>I like composing music, sure, but I started not too long ago.<br/>I can't even imagine how I could actually teach someone.");
						Msg("Hmm... Now that we're talking about composing music,<br/>would you do me a favor?");
						Msg(Hide.Name, "(Received a Quest Scroll containing <npcname/>'s request.)");
					}
					else
					{
						Msg("Speaking of the Composing skill,<br/>have you read the Quest Scroll with my request?");
					}
				}
				break;

			case "skill_tailoring":
				Player.GiveKeyword("shop_grocery");
				Msg("You sure have a lot of doubts. Hahaha...<br/>All you need to do is ask Caitin at the Grocery Store.<br/>You couldn't possibly think that I don't tell you all I know?<br/>Hahaha. I hope you don't feel that way. Now, go to the Grocery Store.");
				break;

			case "skill_magnum_shot":
				Player.GiveKeyword("school");
				Msg("I say it would be better to talk to Trefor the guard or Ranald the combat instructor.<br/>They are the experts on melee combat skills in this town.<br/>Trefor is on guard at the path to Alby Dungeon in the northern part of town,<br/>and Ranald is in the School right below where we are right now.");
				break;

			case "skill_counter_attack":
				Player.GiveKeyword("school");
				Msg("Hmm... I think you should talk with Ranald about that.<br/>He must be at the School right below where we are right now.");
				break;

			case "skill_smash":
				Msg("As for the Smash skill, I don't know very much about it,<br/>but Ranald or Ferghus can help you out.");
				Msg("I heard some people bring up their names<br/>when discussing the skill the other day.");
				break;

			case "skill_gathering":
				Msg("Do you know about the Gathering skill? Hahaha, don't be nervous. Anyone can do it.<br/>It's a skill to collect wood or wool from Mother Nature with the proper tools.<br/>I think it's a good idea to talk to Caitin about gathering,<br/>unless you've already met her and talked about it.");
				break;

			case "square":
				Msg("Go straight up this way, and you will find the Square.<br/>Since it is always crowded,<br/>you won't miss it.");
				break;

			case "pool":
				Player.GiveKeyword("farmland");
				Msg("The reservoir is near here.<br/>The Windmill draws water out of the reservoir to irrigate the farmland.");
				break;

			case "farmland":
				Msg("Hmm... I don't know if I should tell you where it is.<br/>The farmland is right there in front, but know this.<br/>People in town are quite depressed when they see their precious crops ruined<br/>by travelers trespassing on their farmland.");
				break;

			case "windmill":
				Msg("The Windmill?<br/>You should head to the town entrance.<br/>But there is nothing much to see, I think.");
				Msg("Wait, you want to go there to see Alissa, right?");
				break;

			case "brook":
				Msg("Adelia Stream was named after Priestess Adelia,<br/>who was at this church before Meven and myself.<br/>When the town was attacked by an unknown monster a long time ago,<br/>Priestess Adelia faced it alone, even though she was but a fragile woman,<br/>and drove the monster away with the protection of Lymilark.");
				Msg("The faithful followers of Lymilark all share the same power of belief,<br/>but I'm still amazed she stood up to such a devastating monster.<br/>I can't even imagine myself in her shoes.");
				Msg("Her portrait shows only her feminine side, and I have to admit she looks fragile in the paintings.<br/>How could she have such unbelievable courage?<br/>It must have been her faith.");
				Msg("Whenever I think of her,<br/>I always wonder if I am capable of leaving a mark like that in this town.");
				break;

			case "shop_headman":
				Msg("The Chief's House is right up there.<br/>Walk up to the Square, and keep going up the hill with the wall that has paintings of animals.");
				break;

			case "temple":
				Msg("Welcome to the Lymilark Church.<br/>This is the place to worship Lymilark.");
				break;

			case "school":
				Msg("The School? The School is right below here.<br/>That's where you'll find the two instructors, Lassar and Ranald.<br/>If you are interested in spells or combat, you'll want to go there.");
				break;

			case "skill_windmill":
				Player.GiveKeyword("windmill");
				Msg("There is a Windmill in this town,<br/>but I don't know if the skill you mentioned has anything to do with that.");
				break;

			case "skill_campfire":
				Player.GiveKeyword("shop_inn");
				Msg("Hmm... sounds like a useful skill,<br/>but you should be careful before starting a campfire in town.<br/>There's always a risk of fire getting out of control,<br/>and it's not very easy to clean up afterwards.<br/>If you need rest, why not just go to the Inn?");
				break;

			case "shop_restaurant":
				Player.GiveKeyword("shop_grocery");
				Msg("A restaurant? Are you looking for something to eat?<br/>I'd love to share what we have at the Church, but we recently ran out of food.<br/>Why don't you go to Caitin's Grocery Store?<br/>Everyone in town buys food from her store.");
				break;

			case "shop_armory":
				Msg("Are you looking for the Weapons Shop?<br/>While weapons can be used to protect yourself,<br/>they also lead you closer to the shadow of violence and death.<br/>That is not in line with the teachings of Lymilark, I'm afraid...");
				break;

			case "shop_cloth":
				Player.GiveKeyword("shop_misc");
				Msg("You want to buy clothes, don't you?<br/>Clothes are sold at Malcolm's General Shop near the Square.");
				break;

			case "shop_bookstore":
				Player.GiveKeyword("shop_misc");
				Msg("Are you looking for a bookstore?<br/>Unfortunately, there are no bookstores in this town.<br/>Books are expensive and take time to read,<br/>so they are only for people with spare time and money.");
				Msg("People here are simply too busy trying to make ends meet.<br/>The only books you can find in this town are probably some books on spells, I guess.<br/>Sometimes, Ferghus or Ranald give books as presents<br/>but it's a stretch to call them real books.");
				break;

			case "shop_government_office":
				Msg("The Town Office?<br/>Well...<br/>I don't think I saw one in Tir Chonaill.<br/>If you need the services of a Town Office, talk to Duncan.");
				break;

			case "graveyard":
				Player.GiveKeyword("shop_headman");
				Msg("Have you been to Duncan's house? The graveyard is right behind it.<br/>It may look a little creepy,<br/>but it's the resting place for the people who died defending Tir Chonaill.<br/>Oh, there are some big spiders roaming around the graveyard. Be careful.");
				break;

			case "bow":
				Player.GiveKeyword("show_smith");
				Msg("Bows?<br/>Well, the Blacksmith's Shop might have them.<br/>Bows are usually made of wood, but arrowheads are made of iron, so...");
				break;

			case "lute":
				Msg("The lute is a musical instrument with a neck and musical strings attached to a deep, round soundbox.<br/>To play it, simply pluck the strings.<br/>It's an easily accessible instrument. You should try it!<br/>You can find one at Malcolm's General Shop.");
				break;

			case "complicity":
				Msg("I have no idea why people do that, but I'm afraid anyone who does such things often<br/>may lose divine protection despite love and blessings from Lymilark.");
				break;

			case "tir_na_nog":
				Msg("Tir Na Nog is the land of gods<br/>under the support of Lymilark and the other two main gods,<br/>Jeamiderark and Hymerark.<br/>It is where the king of all gods, Aton Cimeni,<br/>sits on the throne to govern everything.");
				break;

			case "mabinogi":
				Msg("Well, all I know about Mabinogi is that it's a song written and sung by the bards.<br/>You could call it folklore, inherited and sung by generations of bards.<br/>I think Priest Meven may know more about Mabinogi than I do.<br/>Chief Duncan might be another person you could talk to.");
				break;

			case "musicsheet":
				Player.GiveKeyword("shop_misc");
				Msg("For casual notes, you may not need a music score,<br/>but if you want to play a serious tune, you'll need one.<br/>You can even create your own music score<br/>by writing what you've composed on a blank score sheet.<br/>Go to Malcolm's General Shop to buy a blank score.");
				break;

			default:
				if (Memory >= 15 && Favor >= 30 && Stress <= 5)
				{
					Msg(FavorExpression(), "Can I ask you for a favor? Can you tell me about the things you know, too?");
					ModifyRelation(0, 0, Random(2));
				}
				else if (Favor >= 10 && Stress <= 10)
				{
					Msg(FavorExpression(), "Oh, it's just that... I think you know more things than I do.");
					ModifyRelation(0, 0, Random(2));
				}
				else if (Favor <= -10)
				{
					Msg(FavorExpression(), "I'm sorry, but I just realized I have some errands to take care of. Can we talk later?");
					ModifyRelation(0, 0, Random(4));
				}
				else if (Favor <= -30)
				{
					Msg(FavorExpression(), "Sorry, I'm too busy right now.");
					ModifyRelation(0, 0, Random(5));
				}
				else
				{
					RndFavorMsg(
						"I have no idea...",
						"Well, I'm afraid I can't comment on that.",
						"It doesn't sound familiar to me, I mean...",
						"I guess you'd better ask someone else about such things.",
						"I don't think I can help you with that. Can we talk about something else?"
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
				Msg(L("Oh, I am overwhelmed. Thank you so much for the gift.<br/>May the blessings of Lymilark be with you."));
				break;

			case GiftReaction.Like:
				Msg(L("Thank you.<br/>May Lymilark be with you always."));
				break;

			case GiftReaction.Neutral:
				Msg(L("I'm afraid Meven won't be happy if he found out I kept things given by travelers.<br/>Oh, no. It's fine. I'll keep it at the Church for the time being."));
				break;

			case GiftReaction.Dislike:
				Msg(L("I'm sorry, but I can't take this."));
				break;
		}
	}
}

public class EndelyonShop : NpcShopScript
{
	public override void Setup()
	{
		Add("Gift", 52012); // Candlestick
		Add("Gift", 52013); // Flowerpot (150 G)
		Add("Gift", 52020); // Flowerpot (200 G)
		Add("Gift", 52024); // Bouquet
	}
}
