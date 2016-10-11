//--- Aura Script -----------------------------------------------------------
// Tin
//--- Description -----------------------------------------------------------
// There are 2 Tins, the counter part to Nao for pets in Soul Stream
// and the one in the Tir beginner area. Both are needed because the client
// sends LeaveSoulStream when ending the conversation with a special NPC.
//---------------------------------------------------------------------------

public class TinScript : NpcScript
{
	public override void Load()
	{
		SetRace(10002);
		SetName("_tin");
		SetBody(height: 0.1f);
		SetFace(skinColor: 15, eyeType: 15, eyeColor: 47);
		SetStand("human/male/anim/male_stand_Tarlach_anguish");
		SetLocation(125, 22211, 74946, 44);

		EquipItem(Pocket.Face, 4900);
		EquipItem(Pocket.Hair, 4021, 0xA64742);
		EquipItem(Pocket.Armor, 15069, 0xFFF7E1, 0xBC3412, 0x40460F);
		EquipItem(Pocket.Shoe, 17010, 0xAC6122, 0xFFFFFF, 0xFFFFFF);
		EquipItem(Pocket.Head, 18518, 0xA58E74, 0xFFFFFF, 0xFFFFFF);
	}

	protected override async Task Talk()
	{
		SetBgm("NPC_Tin.mp3");

		await Intro(L("A little boy with a heavy helmet is looking in my direction.<br/>The helmet is very well polished and features a dragon on the top, but prevents me from being able to see his face.<br/>He speaks in a low voice, and every once in a while places his left hand on his chin to keep his helmet on,<br/>as it slips off little by little."));

		/* As of r234 (or earlier), you get this greet message on first time rebirth for 0 memory (memory over 0 is the same as normal rebirths)
		Msg("<face name='normal'/>Welcome, <username/>.<br/>This is your first time here, yes?");
		Msg("I'm surprised by how exactly Nao described you."); */

		if (Player.Vars.Perm["EverRebirthed"] == null)
		{
			if (Memory <= 0)
			{
				Msg(FavorExpression(), L("Hey, who are you?"));
				Msg(L("You don't look like you're from this world. Am I right?<br/>Did you make your way down here from Soul Stream?<br/>Ahhh, so Nao sent you here!"));
				Msg(L("She's way too obedient to the Goddess' wishes.<br/>Anyway, she's a good girl, so be nice to her."));
			}
			else
			{
				Msg(FavorExpression(), L("Did you take a good look around?<br/>Since you're talking to me so much, you must have a lot of free time."));
			}
			UpdateRelationAfterGreet();

			Msg("Was there something else you wanted to talk about?");
			await StartConversation();

			Close(Hide.None, "Go all the way to the right and you will find Tir Chonaill. <br/>I wish you the best of luck.<br/>Have a great journey. <br/>I'll see you around...");
		}
		else
		{
			if (Memory <= 0)
			{
				Msg(FavorExpression(), L("Hey, <username/>. Were you reborn?<br/>Do you remember me?"));
				Msg(L("Well... your appearance changed a little bit, but you still seem the same to me."));
			}
			else
			{
				Msg(FavorExpression(), L("Well, it's nice to see you again after so long...<br/>But this is not where your journey ends. Think about the new world waiting for you out there."));
			}
			UpdateRelationAfterGreet();

			if (Title == 11002)
			{
				Msg("Well done. <username/>.<br/>But you know what? This isn't the<br/>end of the Fomors.");
				Msg("...Cichol and Dark Lord is still alive,<br/>and besides the fact that the goddess statue has been restored,<br/>much hasn't really changed...");
				Msg("...I get the feeling that<br/>you'll soon have to lift a heavier burden to save this world...");
				Msg("It might be a good thing<br/>or bad...");
			}

			Msg("Was there something else you wanted to talk about?");
			await StartConversation();

			// Check CreatureStates.FreeRebirth?

			if (!HasKeyword("tutorial_present") && !IsEnabled("NoRebirthPresent"))
			{
				Msg("Hmmm... How about I give you a present since you've been reborn and all.<br/>This is a Dye you can use to dye your clothes, but unlike regular Dye, the color is already set.<br/>I'll show you ten different colors in order, so choose the one that you'd like to keep.<br/>If you don't pick one while I'm showing it to you, I won't be able to give it to you, so be sure to pick one.");

				// Select color
				var rnd = RandomProvider.Get();
				var itemId = 63030; // Fixed Color Dye Ampoule
				uint color = 0x000000;
				int start = 1, max = 10;
				var options = "<button title='Yea, I like it.' keyword='@yes'/><button title='Show me another one.' keyword='@no'/>";

				for (int i = start; i <= max; ++i)
				{
					// There are 9 keywords to save the "progress" you make
					// on the dyes, so you can't disconnect to get another 9
					// chances. Once max is reached, you simply get whatever
					// color is selected, without any chances to cheat.
					if (i < max)
					{
						if (HasKeyword("Tin_ColorAmpul_" + i))
							continue;

						GiveKeyword("Tin_ColorAmpul_" + i);
					}

					// Completely random color
					// Officials probably used a predefined list of colors.
					// This call generates completely random colors, incl.
					// all kinds of flashies.
					color = (uint)rnd.Next(int.MinValue, int.MaxValue);

					var image = string.Format("<image item='{0}' col1='{1:X8}'/>", itemId, color);
					var result = "@no";

					if (i == 1)
					{
						Msg(image + "Okay, let's see...<br/>How do you like this color? Do you like it?" + options);
						result = await Select();
					}
					else if (i < max)
					{
						Msg(image + "Well then, what about this color?<br/>This one seems nice..." + options);
						result = await Select();
					}
					else
					{
						Msg(image + "This is the last one.<br/>Since this is the last one, you'll just have to take it.");
						result = "@yes";
					}

					if (result == "@yes")
						break;
				}

				// Give dye
				Player.AcquireItem(Item.Create(itemId, color1: color));
				GiveKeyword("tutorial_present");

				Msg("I'll give you a useful book here.<br/>You know how Nao keeps giving you all sorts of rare accessories?<br/>Well, try collecting all those things in this book.");
			}

			// As of r234 (or earlier), his close message is
			// "Go all the way to the right and you will find Tir Chonaill. <br/>I wish you the best of luck.<br/>Have a great journey. <br/>I'll see you around..."
			Close(Hide.None, "I wish you the best of luck.<br/>Have a great journey.<br/>I'll see you around.");
		}
	}

	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				Msg("You better not judge people by their appearances.<br/>You may not believe it, but I have been around this world for a long time.");
				Msg("Now that I think of it, I lived far too long in my previous life.<br/>It's gotten to the point where I got so tired of counting my age that I decided to be reborn.");
				break;

			case "rumor":
				GiveKeyword("shop_headman");
				Msg("Are you off to Tir Chonaill?<br/>I suppose you'll be meeting Duncan first, right?<br/>Keep walking towards the opposite end of where I am standing, and that should lead you to Tir Chonaill.");
				Msg("I created this temporary path.<br/>Shall I say, an invisible path to another world?");
				Msg("Make sure to have everything ready when you head there, because once you are out of here, you can never come back.<br/>And don't waste your time trying to find this path later because I made it for my own personal use only.");
				Msg("What personal reasons?<br/>Well, I need to find someone amongst this sea of newcomers and the reborn.<br/>I don't feel like doing it, but I have to...");
				Msg("Anyway, standing here all day doing nothing can get boring in a hurry,<br/>so I'll be your buddy for the time being.");
				break;

			case "about_skill":
				Msg("In this world, skills are of utmost importance for you to do anything.<br/>Don't worry, it's much easier than it sounds.");
				Msg("I mean, the fact that you are strong enough to eliminate raccoons<br/>that are constantly attacking the chickens... That's good enough.");
				Msg("Well, it's not the first time that these raccoons have attacked the chickens,<br/>but it's been getting much worse lately.");
				Msg("The chickens, in turn, act just as violently, so I made a wooden fence to keep the raccoons away.<br/>Even with the fence though, these raccoons just keep breaking in...<br/>It's really frustrating, you know.");
				Msg("Don't quote me on this, but I think it has something to do with the evil spirits called the Fomors.<br/>Before this, I have never heard of raccoons wandering around in groups, breaking fences, and viciously attacking chickens.");
				break;

			case "about_arbeit":
				Msg("Wow. You are full of energy.<br/>Will you take a part-time job if I give you one?");
				Msg("... Hahaha.<br/>If you head east, you'll find plenty of people who want to give you jobs.");
				Msg("I am sure everyone will welcome you.<br/>Since it's close by, you might want to check out the Church first.<br/>Priestess Endelyon is very popular <br/>so you'd better hurry if you want to get a job.");
				Msg("Now, just in case you're nervous about these part-time jobs, don't be. It's not difficult at all.<br/>You're just helping out with some easy chores.");
				Msg("However, don't ever forget to finish the job on time<br/>and report to the NPC.<br/>That's my insightful advice to you.");
				break;

			case "about_study":
				Msg("I usually don't go into details, <username/>, but I'll make an exception for you.<br/>So listen carefully.");
				Msg("You may learn new skills at the School.<br/>I'm sure you know by now the School is located east from here.");
				Msg("Inside the School campus there's a man near the training ground with long hair covering his face,<br/>who speaks in a serious manner. His name is Ranald.<br/>Talk to him if you're interested in learning Combat skills.");
				Msg("Also if you are interested in learning Magic skills,<br/>I suggest you go into the classroom and see Lassar.<br/>She's a bit talkative, but her skills are incredible.");
				Msg("Well, to tell the truth<br/>I learned my Magic skills from her as well.");
				Msg("If you've made up your mind to learn new things in life, I suggest you go all out and don't be lazy!");
				break;

			case "shop_misc":
				Msg("If you are looking for the General Shop, you don't need to ask me.<br/>Tir Chonaill is right out there.");
				Msg("By the way, don't forget about my accessories.");
				break;

			case "shop_grocery":
				Msg("Do you have something against the Grocery Store?<br/>You seem very anxious...");
				Msg("...Or maybe not.");
				break;

			case "shop_healing":
				Msg("...So you want to be<br/>a Healer in this life?<br/>Interesting...");
				break;

			case "shop_inn":
				Msg("...Did you want to go see Nora...?");
				Msg("...Or, Piaras?");
				break;

			case "shop_bank":
				Msg("You know you can better<br/>manage your Inventory<br/>by utilizing the Bank to hold your items, right?");
				Msg("...I was just checking...<br/>just in case you didn't know.");
				break;

			case "shop_smith":
				Msg("You've been to Ferghus' Blacksmith's Shop, right?");
				Msg("...");
				Msg("...Good, then I don't need to explain anything.");
				break;

			case "skill_rest":
				Msg("...Train it well.<br/>One day you'll find it handy.");
				break;

			case "skill_range":
				Msg("...Well, the word on the street is that<br/>you need to be born<br/>with the talent...<br/>but that's not true in this world.");
				Msg("...You can do anything you want as long as you put in the effort.<br/>I'm telling you...it's all about your training.");
				break;

			case "skill_instrument":
				Msg("Playing an instrument is such a romantic skill.<br/>If you're interested in that area,<br/>you'll probably make a lot of friends.");
				break;

			case "skill_composing":
				Msg("Composing should be done by those who are serious about making music...<br/>but some people just do it<br/>because it increases their intelligence...");
				Msg("Well, composing certainly is quite a bit difficult.");
				break;

			case "skill_tailoring":
				Msg("You can easily learn and make cheap clothes, <br/>but expensive clothes cost too much to make.  The material cost is no joke! <br/>But then again, you can sell them at a higher price...so I guess it evens out..");
				Msg("If you just want to raise your skill level fast,<br/>you should sign up for a part-time job at the Clothing Shop.<br/> They provide you with all the materials, so it's a great deal.");
				break;

			case "skill_magnum_shot":
				Msg("You know that the single-hit attack is the best strategy<br/>against opponents with the auto-defense skill, right?<br/>Just be careful against those who are immune to arrow attacks.");
				break;

			case "skill_counter_attack":
				Msg("...I heard that it's being used<br/>differently than what the creators of the skill<br/>had originally intended it to be.");
				Msg("Well, I guess skills all depend on<br/>your training anyway...");
				break;

			case "skill_smash":
				Msg("If you want to put everything on one hit,<br/>it's not a bad idea to continue investing in it.<br/>You might not notice any improvements at first,<br/>but I'm telling you, there's a reason why this skill is so popular.");
				break;

			case "skill_gathering":
				Msg("I guess you can call it a skill that teaches you about life...<br/>You can't be too rushed but you can't be too slow either...<br/>Something like that.");
				break;

			case "square":
				Msg("You must be really bad with directions.<br/>Well, I guess adjusting to a new body<br/>will take some time...");
				Msg("Once you leave here, you'll be around the Tir Chonaill School.<br/>...You can probably figure it out from there.");
				break;

			case "pool":
				Msg("...Do you want to stare at your own reflection<br/>in the water or something?");
				Msg("...Just don't fall in and drown.");
				break;

			case "farmland":
				Msg("...The life of a farmer doesn't seem so bad...");
				break;

			case "windmill":
				Msg("You can grind crops at the Windmill.<br/>Have you ever tried it?");
				Msg("Oh, I'm just asking, because<br/>there are surprisingly a lot of people who have never tried it.");
				break;

			case "brook":
				Msg("Once you leave this area,<br/>you'll be able to see Adelia stream nearby.<br/>You've been there before...right?");
				Msg("...I'll spare you the details. Cool?");
				break;

			case "shop_headman":
				Msg("Were you aware that you can see the your next destination of your quest by pressing 'Alt'?");
				Msg("For example, let's say you have Nao's Letter of Introduction<br/>and it tells you to find Duncan.");
				Msg("If you press 'Alt', it will show you the exact direction to Duncan's House.<br/>Just press 'Alt' and you will see what I mean.");
				break;

			case "temple":
				Msg("I don't really believe in God, but, I don't think it's a bad idea to find comfort<br/>in praying at the Church.");
				Msg("But, don't be a fanatic, though. That's dangerous.");
				break;

			case "school":
				Msg("...It's important to train your skills<br/>while you're in school<br/>but it's also important to make friends.");
				Msg("...Just listen to my advice,<br/>so you don't regret it in your next life.");
				Msg("...When you're reborn, only your friends will be<br/>waiting for you to welcome you back.");
				break;

			case "skill_windmill":
				Msg("Have you ever thought about how hard it would be to<br/>spin your entire body while balancing on your hands?");
				Msg("I hear it's a great way<br/>to get stronger and get fit.<br/>If you want a smokin' body, give it a try.");
				break;

			case "skill_campfire":
				Msg("It seems like this skill is not so popular nowadays...<br/>But you should know that this skill is generally used to help other people.<br/>If you don't want to help others...well...<br/>What more can I say...?");
				break;

			case "shop_restaurant":
				Msg("...Watch what you eat too.<br/>Don't just eat anything and everything.<br/>Go to a nice restaurant and treat yourself to a nice meal.<br/>...You've just been reborn. So go celebrate!");
				Msg("...It's all up to you.");
				break;

			case "shop_armory":
				Msg("Haha. Are you interested in my helmet?<br/>...it's quite expensive you know...hehe...");
				break;

			case "shop_cloth":
				Msg("Dress nicely<br/>even if you have to spend some money.");
				Msg("No matter how beautiful you are on the inside,<br/>you won't ever get the chance to show that to people<br/>if you're looking all shabby on the outside.");
				Msg("...But I'm sure you know what you're doing,<br/>and that you don't need me to be telling you about stuff like that. Right, <username/>?");
				break;

			case "shop_bookstore":
				Msg("Why...?<br/>Are you planning to actually read some books in your new life?  Haha...");
				break;

			case "graveyard":
				Msg("Hmm... You're not thinking that<br/>your former body is actually buried in a graveyard somewhere, are you?");
				Msg("Technically speaking, people from<br/>Soul Stream aren't<br/>really a part of this world.");
				Msg("That is, even if you lose your life<br/>in this body...");
				Msg("you won't ever end up ike the people in Tuatha De Danann,<br/>whose bodies are buried in the ground<br/>and their souls disappearing into eternity...");
				Msg("...Do you get what I'm saying?");
				break;

			case "shop_goverment_office":
				Msg("It's where you recover any items you've lost<br/>while getting knocked out during your adventures.<br/>...Don't tell me you didn't know?");
				Msg("That's basic knowledge!");
				break;

			case "bow":
				Msg("I was thinking of getting one myself,<br/>but it keeps dragging on the ground...");
				break;

			case "complicity":
				Msg("...Me? Pff...");
				break;

			case "tir_na_nog":
				Msg("Tir Na Nog...?");
				Msg("...Well, I do know of it...<br/>But It's not something I can just explain to you<br/>for you to understand...");
				Msg("You'll have to find out and experience it for yourself<br/>as you journey on.");
				break;

			case "mabinogi":
				Msg("...It's just the kind of thing you create as you live through your life.<br/>Your own song, your own Mabinogi...<br/>...It's not exactly something that someone else could do for you.");
				break;

			case "musicsheet":
				Msg("It's common sense that you write music on a Music Score...");
				Msg("...What I wanted to tell you is that...<br/>Composing isn't restricted to this world.<br/>Even when you're not in this world,<br/>you can continue to make music through your spirit.");
				Msg("...It just all depends on<br/>how much you want it to come alive.");
				break;

			default:
				Msg("...I'm chatting with you right now just because I'm bored,<br/>but...don't expect to have free access to all of my wisdom so easily.");
				Msg("What fun would it be if I were to tell you everything?  I shouldn't to spoil it for you. Right?");
				break;
		}
	}

	protected override async Task Gift(Item item, GiftReaction reaction)
	{
		End(L("...A gift? Thanks!"));
	}
}

public class TinSoulStreamScript : NpcScript
{
	public override void Load()
	{
		SetRace(10002);
		SetId(MabiId.Tin);
		SetName("_tin_soul_stream");
		SetLocation(22, 6313, 5712);
	}

	protected override async Task TalkPet()
	{
		Msg("Hi. <username/>.<br/>Nice to meet you..");
		Msg("I wish you the best of luck with all that you do here in Erinn...<br/>See ya.", Button("End Conversation"));
		await Select();

		Player.SetLocation(1, 15250, 38467);

		Close();
	}
}
