//--- Aura Script -----------------------------------------------------------
// Nao
//--- Description -----------------------------------------------------------
// First NPC players encounter. Also met on every rebirth.
// Located on an unaccessible map, can be talked to from anywhere,
// given the permission.
//---------------------------------------------------------------------------

public class NaoScript : NpcScript
{
	int privateStoryCount;

	public override void Load()
	{
		SetRace(1);
		SetId(MabiId.Nao);
		SetName("_nao");
		SetLocation(22, 6013, 5712);
	}

	protected override async Task Talk()
	{
		SetBgm("Nao_talk.mp3");

		await Intro(L("A beautiful girl in a black dress with intricate patterns.<br/>Her deep azure eyes remind everyone of an endless blue sea full of mystique.<br/>With her pale skin and her distinctively sublime silhouette, she seems like she belongs in another world."));

		if (!Player.HasEverEnteredWorld)
			await FirstTime();
		else if (Player.CanReceiveBirthdayPresent)
			await Birthday();
		else
			await Rebirth();
	}

	private async Task FirstTime()
	{
		await Introduction();
		await Questions();
		// Destiny/Talent...
		await EndIntroduction();
	}

	private async Task Introduction()
	{
		if (Player.IsMale)
			Msg(L("Hello, there... You are <username/>, right?<br/>I have been waiting for you.<br/>It's good to see a gentleman like you here."));
		else
			Msg(L("Hello, there... You are <username/>, right?<br/>I have been waiting for you.<br/>It's good to see a lady like you here."));

		Msg(L("My name is Nao.<br/>It is my duty to lead pure souls like yours to Erinn."));
	}

	private async Task Questions()
	{
		Msg(L("<username/>, we have some time before I guide you to Erinn.<br/>Do you have any questions for me?"), Button(L("No"), "@no"), Button(L("Yes"), "@yes"));
		if (await Select() != "@yes")
			return;

		while (true)
		{
			Msg(RandomPhrase(),
				Button(L("End Conversation"), "@endconv"),
				List(L("Talk to Nao"), 4, "@endconv",
					Button(L("About Mabinogi"), "@mabinogi"),
					Button(L("About Erinn"), "@erinn"),
					Button(L("What to do?"), "@what"),
					Button(L("About Adventures"), "@adventures")
				)
			);

			switch (await Select())
			{
				case "@mabinogi":
					Msg(L("Mabinogi can be defined as the songs of bards, although in some cases, the bards themselves are referred to as Mabinogi.<br/>To the residents at Erinn, music is a big part of their lives and nothing brings joy to them quite like music and Mabinogi.<br/>Once you get there, I highly recommend joining them in composing songs and playing musical instruments."));
					break;

				case "@erinn":
					Msg(L("Erinn is the name of the place you will be going to, <username/>.<br/>The place commonly known as the world of Mabinogi is called Erinn.<br/>It has become so lively since outsiders such as yourself began to come."));
					Msg(L("Some time ago, adventurers discovered a land called Iria,<br/>and others even conquered Belvast Island, between the continents.<br/>Now, these places have become home to adventurers like yourself, <username/>.<p/>You can go to Tir Chonaill of Uladh now,<br/>but you should try catching a boat from Uladh and<br/>crossing the ocean to Iria or Belvast Island."));
					break;

				case "@what":
					Msg(L("That purely depends on what you wish to do.<br/>You are not obligated to do anything, <username/>.<br/>You set your own goals in life, and pursue them during your adventures in Erinn.<p/>Sure, it may be nice to be recognized as one of the best, be it the most powerful, most resourceful, etc., but <br/>I don't believe your goal in life should necessarily have to be becoming 'the best' at everything.<br/>Isn't happiness a much better goal to pursue?<p/>I think you should experience what Erinn has to offer <br/>before deciding what you really want to do there."));
					break;

				case "@adventures":
					Msg(L("There are so many things to do and adventures to go on in Erinn.<br/>Hunting and exploring dungeons in Uladh...<br/>Exploring the ruins of Iria...<br/>Learning the stories of the Fomors in Belvast...<p/>Explore all three regions to experience brand new adventures!<br/>Whatever you wish to do, <username/>, if you follow your heart,<br/>I know you will become a great adventurer before you know it!"));
					break;

				default:
					return;
			}
		}
	}

	private async Task EndIntroduction()
	{
		Msg(L("Are you ready to take the next step?"));
		Msg(L("You will be headed to Erinn right now.<br/>Don't worry, once you get there, someone else is there to take care of you, my little friend by the name of Tin.<br/>After you receive some pointers from Tin, head Northeast and you will see a town."));
		Msg(L("It's a small town called Tir Chonaill.<br/>I have already talked to Chef Duncan about you, so all you need to do is show him the letter of introduction I wrote right here."), Image("tir_chonaill"));
		Msg(L("You can find Chief Duncan on the east side of the Square.<br/>When you get there, try to find a sign that says 'Chief's House'."), Image("npc_duncan"));
		Msg(L("I will give you some bread I have personally baked, and a book with some information you may find useful.<br/>To see those items, open your inventory once you get to Erinn."));
		Msg(Hide.Both, L("(Received a Bread and a Traveler's Guide from Nao.)"), Image("novice_items"));
		Msg(L("I wish you the best of luck in Erinn.<br/>See you around."), Button(L("End Conversation")));
		await Select();

		// Move to Uladh Beginner Area
		Player.SetLocation(125, 21489, 76421);
		Player.Direction = 233;

		GiveItem(1000, 1);  // Traveler's Guide
		GiveItem(50004, 1); // Bread

		// Add keyword, so players can't possibly get dyes without rebirth.
		GiveKeyword("tutorial_present");

		Close();
	}

	private string RandomPhrase()
	{
		switch (Random(3))
		{
			default:
			case 0: return L("If there is something you'd like to know more of, please ask me now.");
			case 1: return L("Do not hesitate to ask questions. I am more than happy to answer them for you.");
			case 2: return L("If you have any questions before heading off to Erinn, please feel free to ask.");
		}
	}

	private async Task Rebirth()
	{
		Msg(L("Hello, <username/>!<br/>Is life here in Erinn pleasant for you?"));

		if (!IsEnabled("Rebirth"))
		{
			// Unofficial
			Msg(L("I'm afraid I can't let you rebirth just yet, the gods won't allow it."));
			goto L_End;
		}

		if (!RebirthAllowed())
		{
			Msg(L("Barely any time has passed since your last rebirth.<br/>Why don't you enjoy your current life in Erinn for a bit longer?"));
			goto L_End;
		}

		Msg(L("If you wish, you can abandon your current body and be reborn into a new one, <username/>."));

		while (true)
		{
			Msg(L("Feel free to ask me any questions you have about rebirth.<br/>Once you've made up your mind to be reborn, press Rebirth."),
				Button("Rebirth"), Button("About Rebirths"), Button("Cancel"));

			switch (await Select())
			{
				case "@rebirth":
					Msg("<rebirth style='-1'/>");
					switch (await Select())
					{
						case "@rebirth":
							for (int i = 1; i < 10; ++i)
								RemoveKeyword("Tin_ColorAmpul_" + i);
							RemoveKeyword("tutorial_present");

							Player.Vars.Perm["EverRebirthed"] = true;

							// Old:
							//   Msg("Would you like to be reborn with the currently selected features?<br/><button title='Yes' keyword='@rebirthyes' /><button title='No' keyword='@rebirthhelp' />");
							//   Msg("<username/>, you have been reborn with a new appearance.<br/>Did you enjoy having Close Combat as your active Talent?<br/>Would you like to choose a different active Talent for this life?<button title='New Talent' keyword='@yes' /><button title='Keep Old Talent' keyword='@no' />");
							//   Msg("Then I will show you the different Talents available to you.<br/>Please select your new active Talent after you consider everything.<talent_select />")
							//   Msg("You have selected Close Combat.<br/>May your courage and skill grow.<br/>I will be cheering you on from afar.");
							Close(Hide.None, L("May your new appearance bring you happiness!<br/>Though you'll be different when next we meet,<br/>but I'll still be able to recognize you, <username/>.<p/>We will meet again, right?<br/>Until then, take care."));
							return;

						default:
							goto L_Cancel;
					}
					break;

				case "@about_rebirths":
					await RebirthAbout();
					break;

				default:
					goto L_Cancel;
			}
		}

	L_Cancel:
		Msg(L("There are plenty more opportunities to be reborn.<br/>Perhaps another time.") + "<rebirth hide='true'/>");

	L_End:
		Close(Hide.None, L("Until we meet again, then.<br/>I wish you the best of luck in Erinn.<br/>I'll see you around."));
	}

	private async Task RebirthAbout()
	{
		while (true)
		{
			Msg(L("When you rebirth, you will be able to have a new body.<br/>Aside from your looks, you can also change your age and starting location.<br/>Please feel free to ask me more."),
				Button(L("What is Rebirth?"), "@whatis"), Button(L("What changes after a Rebirth?"), "@whatchanges"), Button(L("What does not change after a Rebirth?"), "@whatnot"), Button(L("Done")));

			switch (await Select())
			{
				case "@whatis":
					Msg(L("You can choose a new body between the age of 10 and 17.<br/>Know that you won't receive the extra 7 AP just for being 17,<br/>as you did at the beginning of your journey.<br/>You will keep the AP that you have right now."));
					Msg(L("Also, your Level and Exploration Level will reset to 1.<br/>You'll get to keep all of your skills from your previous life, though."));
					Msg(L("You'll have to<br/>start at a low level for the Exploration Quests,<br/>but I doubt that it will be an issue for you."));
					Msg(L("If you wish, you can even just change your appearance<br/>without resetting your levels or your age.<br/>Just don't select the 'Reset Levels and Age' button<br/>to remake yourself without losing your levels."), Image("Rebirth_01_c2", true, 200, 200));
					Msg(L("You can even change your gender<br/>by clicking on 'Change Gender and Look.'<br/>If you want to maintain your current look, then don't select that button."), Image("Rebirth_02_c2", true, 200, 200));
					Msg(L("You can choose where you would like to rebirth.<br/>Choose between Tir Chonaill, Qilla Base Camp,<br/>or the last location you were at<br/>in your current life."), Image("Rebirth_03", true, 200, 200));
					break;

				case "@whatchanges":
					Msg(L("You can choose a new body between the ages of 10 and 17.<br/>though you won't receive the extra 7 AP just for being 17<br/>as you did at the beginning of your journey."));
					Msg(L("You'll keep all the AP that you have right now<br/>and your level will reset to 1.<br/>You'll keep all of your skills from your previous life, though."));
					Msg(L("If you wish, you can even change your appearance without<br/>resetting your levels or your age.<br/>Just don't select the 'Reset Levels and Age' button,<br/>and you'll be able to remake yourself without losing your current levels."), Image("Rebirth_01", true));
					Msg(L("You can even change your gender by selecting 'Change Gender and Look.'<br/>If you want to keep your current look, just don't select that button."), Image("Rebirth_02", true));
					Msg(L("Lastly, if you would like to return to your last location,<br/>select 'Move to the Last Location'.<br/>Otherwise, you'll be relocated to the Forest of Souls<br/>near Tir Chonaill."));
					break;

				case "@whatnot":
					Msg(L("First of all, know that you cannot change the<br/>name you chose upon entering Erinn.<br/>Your name is how others know you<br/>even when all else changes."));
					Msg(L("<username/>, you can also bring all the knowledge you'd earned<br/>in this life into your next one.<br/>Skills, keywords, remaining AP, titles, and guild will all be carried over.<br/>The items you have and your banking information will also remain intact."));
					break;

				default:
					return;
			}
		}
	}

	private bool RebirthAllowed()
	{
		var player = (PlayerCreature)Player;
		return (player.LastRebirth + ChannelServer.Instance.Conf.World.RebirthTime < DateTime.Now);
	}

	private async Task Birthday()
	{
		var potentialGifts = new int[] { 12000, 12001, 12002, 12003, 12004, 12005, 12006, 12007, 12008, 12009, 12010, 12011, 12012, 12013, 12014, 12015, 12016, 12017, 12018, 12019, 12020, 12021, 12022, 12023 };

		var rndGift = potentialGifts.Random();
		var prefix = 0;
		var suffix = 0;

		// Enchant if it's the 20th birthday.
		if (Player.Age == 20)
		{
			if (Player.IsHuman)
			{
				switch (Random(18))
				{
					case 00: prefix = 20610; break; // Shiny
					case 01: prefix = 20710; break; // Posh
					case 02: prefix = 20810; break; // Well-groomed
					case 03: prefix = 20910; break;	// Holy
					case 04: prefix = 20911; break;	// Beautiful
					case 05: prefix = 20912; break;	// Resplendent
					case 06: suffix = 30410; break;	// Capricornus
					case 07: suffix = 30510; break;	// Sagittarius
					case 08: suffix = 30511; break;	// Aquarius
					case 09: suffix = 30512; break;	// Pisces
					case 10: suffix = 30610; break;	// Libra
					case 11: suffix = 30611; break;	// Scorpius
					case 12: suffix = 30710; break;	// Taurus
					case 13: suffix = 30711; break;	// Virgo
					case 14: suffix = 30911; break;	// Aries
					case 15: suffix = 30912; break;	// Cancer
					case 16: suffix = 31010; break;	// Gemini
					case 17: suffix = 31011; break;	// Leo
				}
			}
			else if (Player.IsElf)
			{
				switch (Random(18))
				{
					case 00: prefix = 20610; break; // Shiny
					case 01: prefix = 20710; break; // Posh
					case 02: prefix = 20810; break; // Well-groomed
					case 03: prefix = 20910; break;	// Holy
					case 04: prefix = 20911; break;	// Beautiful
					case 05: prefix = 20912; break;	// Resplendent
					case 06: suffix = 30413; break;	// Sundrop
					case 07: suffix = 30518; break;	// Violet
					case 08: suffix = 30519; break;	// Forget-me-not
					case 09: suffix = 30520; break;	// Rose
					case 10: suffix = 30621; break;	// Clover
					case 11: suffix = 30622; break;	// Sweet Pea
					case 12: suffix = 30721; break;	// Otter
					case 13: suffix = 30722; break;	// Lilly
					case 14: suffix = 31012; break;	// Cornflower
					case 15: suffix = 31013; break;	// Cosmos
					case 16: suffix = 30816; break;	// Marguerite
					case 17: suffix = 30817; break;	// Hyacinth
				}
			}
			else if (Player.IsGiant)
			{
				switch (Random(18))
				{
					case 00: prefix = 20610; break; // Shiny
					case 01: prefix = 20710; break; // Posh
					case 02: prefix = 20810; break; // Well-groomed
					case 03: prefix = 20910; break;	// Holy
					case 04: prefix = 20911; break;	// Beautiful
					case 05: prefix = 20912; break;	// Resplendent
					case 06: suffix = 31501; break;	// Freezing
					case 07: suffix = 31502; break;	// Frost
					case 08: suffix = 31503; break;	// Hurricane's
					case 09: suffix = 31504; break;	// Hail
					case 10: suffix = 31505; break;	// Sleet
					case 11: suffix = 31506; break;	// Whirlpool
					case 12: suffix = 31507; break;	// Earthquake's
					case 13: suffix = 31508; break;	// Downpour
					case 14: suffix = 31509; break;	// Blizzard's
					case 15: suffix = 31510; break;	// Thunder
					case 16: suffix = 31511; break;	// Tempest
					case 17: suffix = 31512; break;	// Snowfield
				}
			}
		}

		Player.GiveItem(Item.CreateEnchanted(rndGift, prefix, suffix));
		Player.Vars.Perm["NaoLastPresentDate"] = DateTime.Now.Date;

		// Unofficial
		Msg(L("Happy Birthday, <username/>! "));
		Msg(L("I have a little something for you on this special day,<br/>please accept it."));

		if (IsEnabled("NaoDressUp") && !HasKeyword("present_to_nao"))
			GiveKeyword("present_to_nao");

		await Conversation();

		Close(Hide.None, "Until we meet again, then.<br/>I wish you the best of luck in Erinn.<br/>I'll see you around.");
	}

	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			// Gifts and clothes
			// --------------------------------------------------------------

			case "present_to_nao":
				await KeywordPresentToNao();
				break;

			case "nao_cloth0":
				Msg(L("(Missing dialog: Nao asking if she should wear Black Dress."), Button("Yes"), Button("No"));
				if (await Select() == "@yes")
				{
					Player.NaoOutfit = NaoOutfit.BlackDress;
					Msg(L("(Missing dialog: Nao responding to wearing Black Dress."));
				}
				else
					Msg(L("(Missing dialog: Nao responding to not wearing Black Dress."));
				break;

			case "nao_cloth1":
				Msg(L("(Missing dialog: Nao asking if she should wear Rua's Dress."), Button("Yes"), Button("No"));
				if (await Select() == "@yes")
				{
					Player.NaoOutfit = NaoOutfit.RuasDress;
					Msg(L("(Missing dialog: Nao responding to wearing Rua's Dress."));
				}
				else
					Msg(L("(Missing dialog: Nao responding to not wearing Rua's Dress."));
				break;

			case "nao_cloth2":
				Msg(L("(Missing dialog: Nao asking if she should wear Pink Coat."), Button("Yes"), Button("No"));
				if (await Select() == "@yes")
				{
					Player.NaoOutfit = NaoOutfit.PinkCoat;
					Msg(L("(Missing dialog: Nao responding to wearing Pink Coat."));
				}
				else
					Msg(L("(Missing dialog: Nao responding to not wearing Pink Coat."));
				break;

			case "nao_cloth3":
				Msg(L("(Missing dialog: Nao asking if she should wear Black Coat."), Button("Yes"), Button("No"));
				if (await Select() == "@yes")
				{
					Player.NaoOutfit = NaoOutfit.BlackCoat;
					Msg(L("(Missing dialog: Nao responding to wearing Black Coat."));
				}
				else
					Msg(L("(Missing dialog: Nao responding to not wearing Black Coat."));
				break;

			case "nao_cloth4":
				Msg(L("(Missing dialog: Nao asking if she should wear Yellow Spring Dress."), Button("Yes"), Button("No"));
				if (await Select() == "@yes")
				{
					Player.NaoOutfit = NaoOutfit.YellowSpringDress;
					Msg(L("(Missing dialog: Nao responding to wearing Yellow Spring Dress."));
				}
				else
					Msg(L("(Missing dialog: Nao responding to not wearing Yellow Spring Dress."));
				break;

			case "nao_cloth5":
				Msg(L("(Missing dialog: Nao asking if she should wear White Spring Dress."), Button("Yes"), Button("No"));
				if (await Select() == "@yes")
				{
					Player.NaoOutfit = NaoOutfit.WhiteSpringDress;
					Msg(L("(Missing dialog: Nao responding to wearing White Spring Dress."));
				}
				else
					Msg(L("(Missing dialog: Nao responding to not wearing White Spring Dress."));
				break;

			case "nao_cloth6":
				Msg(L("(Missing dialog: Nao asking if she should wear Pink Spring Dress."), Button("Yes"), Button("No"));
				if (await Select() == "@yes")
				{
					Player.NaoOutfit = NaoOutfit.PinkSpringDress;
					Msg(L("(Missing dialog: Nao responding to wearing Pink Spring Dress."));
				}
				else
					Msg(L("(Missing dialog: Nao responding to not wearing Pink Spring Dress."));
				break;

			case "nao_cloth7":
				Msg(L("(Missing dialog: Nao asking if she should wear Explorer Suit."), Button("Yes"), Button("No"));
				if (await Select() == "@yes")
				{
					Player.NaoOutfit = NaoOutfit.PinkSpringDress;
					Msg(L("(Missing dialog: Nao responding to wearing Explorer Suit."));
				}
				else
					Msg(L("(Missing dialog: Nao responding to not wearing Explorer Suit."));
				break;

			case "nao_cloth8":
				Msg(L("(Missing dialog: Nao asking if she should wear Iria Casual Wear."), Button("Yes"), Button("No"));
				if (await Select() == "@yes")
				{
					Player.NaoOutfit = NaoOutfit.PinkSpringDress;
					Msg(L("(Missing dialog: Nao responding to wearing Iria Casual Wear."));
				}
				else
					Msg(L("(Missing dialog: Nao responding to not wearing Iria Casual Wear."));
				break;

			case "nao_yukata":
				Msg(L("(Missing dialog: Nao asking if she should wear Yukata."), Button("Yes"), Button("No"));
				if (await Select() == "@yes")
				{
					Player.NaoOutfit = NaoOutfit.Yukata;
					Msg(L("(Missing dialog: Nao responding to wearing Yukata."));
				}
				else
					Msg(L("(Missing dialog: Nao responding to not wearing Yukata."));
				break;

			case "nao_cloth_santa":
				Msg(L("(Missing dialog: Nao asking if she should wear Santa Suit."), Button("Yes"), Button("No"));
				if (await Select() == "@yes")
				{
					Player.NaoOutfit = NaoOutfit.SantaSuit;
					Msg(L("(Missing dialog: Nao responding to wearing Santa Suit."));
				}
				else
					Msg(L("(Missing dialog: Nao responding to not wearing Santa Suit."));
				break;

			case "nao_cloth_summer":
				Msg(L("(Missing dialog: Nao asking if she should wear White Dress."), Button("Yes"), Button("No"));
				if (await Select() == "@yes")
				{
					Player.NaoOutfit = NaoOutfit.WhiteDress;
					Msg(L("(Missing dialog: Nao responding to wearing White Dress."));
				}
				else
					Msg(L("(Missing dialog: Nao responding to not wearing White Dress."));
				break;

			case "nao_cloth_kimono":
				Msg(L("(Missing dialog: Nao asking if she should wear Kimono."), Button("Yes"), Button("No"));
				if (await Select() == "@yes")
				{
					Player.NaoOutfit = NaoOutfit.Kimono;
					Msg(L("(Missing dialog: Nao responding to wearing Kimono."));
				}
				else
					Msg(L("(Missing dialog: Nao responding to not wearing Kimono."));
				break;

			case "nao_cloth_summer_2008":
				Msg(L("(Missing dialog: Nao asking if she should wear Sky-Blue Dress."), Button("Yes"), Button("No"));
				if (await Select() == "@yes")
				{
					Player.NaoOutfit = NaoOutfit.SkyBlueDress;
					Msg(L("(Missing dialog: Nao responding to wearing Sky-Blue Dress."));
				}
				else
					Msg(L("(Missing dialog: Nao responding to not wearing Sky-Blue Dress."));
				break;

			case "nao_cloth_shakespeare":
				Msg(L("(Missing dialog: Nao asking if she should wear Playwright Costume."), Button("Yes"), Button("No"));
				if (await Select() == "@yes")
				{
					Player.NaoOutfit = NaoOutfit.PlaywrightCostume;
					Msg(L("(Missing dialog: Nao responding to wearing Playwright Costume."));
				}
				else
					Msg(L("(Missing dialog: Nao responding to not wearing Playwright Costume."));
				break;

			case "nao_cloth_farmer":
				Msg(L("(Missing dialog: Nao asking if she should wear Farming Outfit."), Button("Yes"), Button("No"));
				if (await Select() == "@yes")
				{
					Player.NaoOutfit = NaoOutfit.FarmingOutfit;
					Msg(L("(Missing dialog: Nao responding to wearing Farming Outfit."));
				}
				else
					Msg(L("(Missing dialog: Nao responding to not wearing Farming Outfit."));
				break;

			// Breast
			// http://mabination.com/threads/85165-quot-Breast-quot-and-other-keywords.
			// --------------------------------------------------------------

			case "nao_blacksuit":
				GiveKeyword("breast");

				Msg(L("I really like these clothes.<br/>I think the skirt is sort of erotic but, despite the appearance, it's very comfortable.<br/>But...the chest is probably a bit tight."));
				break;

			case "breast":
				RemoveKeyword("breast");

				Msg(L("Uhm... <username/>, this discussion is a little..."));
				Msg(Hide.Name, L("(Nao is blushing uncomfortably.)"));
				Msg(L("...<p/>......<p/>A long time ago my friends would poke fun at me for that like you...<br/>It makes those friends spring to mind.<br/>In some ways it's similar to those feelings after all..."));
				Msg(L("I don't think they had any ill intent when they said it.<br/>Honestly, because that was thought of me ever since I was a child,<br/>I had a complex about it."));
				Msg(L("Do you think that way too, <username/>?"), Button(L("They look big to me"), "@big"), Button(L("It is not like that"), "@notlikethat"), Button(L("I think it is adorable"), "@adorable"), Button(L("They are not all that big"), "@notbig"), Button(L("Can I touch them just once?"), "@touch"));

				switch (await Select())
				{
					case "@big":
						NPC.ModifyFavor(Player, -3);
						Msg(L("...<br/>You really do think that way, huh? Fuu......<br/>Even though I didn't fatten up in other places..."));
						break;

					case "@notlikethat":
						NPC.ModifyFavor(Player, +3);
						Msg(Hide.Name, L("(After hearing that, Nao smiled cutely while avoiding my eyes.)"));
						Msg(L("Thank you for giving me courage. There's nothing else to really say..."));
						break;

					case "@adorable":
						NPC.ModifyFavor(Player, +1);
						Msg(Hide.Name, L("(Nao looked surprised after hearing that.)"));
						Msg(L("Umm... r-really? Thank you. That made me a little more confident."));
						break;

					case "@notbig":
						NPC.ModifyFavor(Player, -7);
						Msg(Hide.Name, L("(After hearing that, Nao looked a little displeased and avoided my eyes.)"));
						Msg(L("I-is that so? ...sh-shall we stop this conversation now?"));
						break;

					case "@touch":
						NPC.ModifyFavor(Player, -10);
						Msg(L("Whaaa!! <username/>! What do you think you're saying!? Th-that could never happen!"));
						Msg(Hide.Name, L("(Nao looks really angry.)"));
						Msg(L("...<p/>...Ah, I got so upset, sorry... I went overboard, huh..."));
						break;
				}
				break;

			// Others
			// --------------------------------------------------------------

			case "personal_info":
				switch (privateStoryCount)
				{
					case 0:
						Msg(L("My full name is 'Nao Mariota Pryderi'.<br/>I know it is not the easiest name to pronounce.<br/>Don't worry, <username/>, you can just call me Nao."));
						break;

					case 1:
						Msg(L("If you right-click and drag your cursor during the conversation,<br/>you can view different angles. You are staring at me while we're talking,<br/>and honestly, it's a little embarrassing. Please roll down the<br/>mouse wheel to zoom out and take a few steps back."));
						break;

					case 2:
						Msg(L("I believe everyone should cultivate his or her own unique style instead of simply following trends.<br/>I'm not just talking about hair style or fashion. I'm talking about lifestyle.<br/>It's about doing what you want to do, in a style that's uniquely yours."));
						break;

					case 3:
						GiveKeyword("nao_owl");
						Msg(L("I have a pet owl. He's a great friend that takes care of many things for me."));
						break;

					case 4:
						Msg(L("I love to exchange gifts.<br/>I can tell from the gift how the other person really thinks of me.<br/>The people in Erinn are very fond of exchanging gifts."));
						Msg(L("<username/>, what's your opinion on exchanging gifts with others?<br/>Do you like it?"), Button(L("Of course, I do!"), "@yes"), Button(L("I like receiving gifts."), "@receiving"), Button(L("If it is someone I like, then yes."), "@like"), Button(L("I think it is a waste."), "@waste"), Button(L("No, not really."), "@no"));

						switch (await Select())
						{
							case "@yes":
								Msg(L("Wow! I knew it! I think you and I have something in common.<br/>Personally, I feel that people from other worlds are<br/>generally not so used to the idea of exchanging gifts.<br/>I even heard it from some people that they<br/>were surprised to hear such a question."));
								break;

							case "@receiving":
							case "@like":
								Msg(L("That's an interesting answer. If you're currently with someone,<br/>then I definitely envy that lucky person.<br/>If not, then I sincerely hope you'll find someone soon."));
								break;

							case "@waste":
								Msg(L("What? Really? I am sorry. I shouldn't have asked."));
								break;

							case "@no":
								Msg(L("Oh, I see. But I'm sure you will change your mind<br/>if someone surprises you with an unexpected gift."));
								break;
						}
						break;

					case 5:
						GiveKeyword("nao_friend");
						Msg(L("A few years ago, I was locked in a dungeon by the evil Fomor.<br/>I do not ever want to go near a dungeon now...<br/>I don't even want to think about it.<br/>Fortunately, a friend of mine rescued me from there."));
						Msg(L("Dungeons are very dark and dangerous, but some claim that they<br/>are some of the best places for training and adrenaline rush.<br/>The power of the evil Fomors can change a dungeon every time it's visited,<br/>but that is the exact reason why the daredevil adventurers who prefer constant<br/>changes are that much more attracted to dungeons."));
						break;

					case 6:
						Msg(L("Perhaps those that aspire to quickly become the most powerful usually<br/>end up unhappy and unsatisfied as their lust for power grows.<br/>My friend was one of those that constantly pursued limitless power,<br/>and he had it at the very end. Unfortunately,<br/>that was the seed that brought his downfall in the end.<p/>....."));
						break;

					case 7:
						GiveKeyword("nao_blacksuit");
						Msg(L("There are some people who suspect I might be one of the Fomors because<br/>of my black dress. I mean, what I wear is none of their business,<br/>but someone even speculated that I was the messenger of death.<br/>Honestly, I felt really weird when I heard that."));
						Msg(L("These days, I don't even know who I am anymore.<br/>Maybe I really am one of them, you know."));
						Msg(L("...<p/>Please don't tell me you believe that..."));
						break;

					case 8:
						Msg(L("(Missing dialog: Last response to Nao's Private Story."));
						break;
				}

				if (privateStoryCount < 8)
					privateStoryCount++;

				break;

			// Default
			// --------------------------------------------------------------

			default:
				RndMsg(
					L("Ummm...why don't we talk about something else?")
				);
				break;
		}
	}

	protected async Task KeywordPresentToNao()
	{
		Msg(L("(Missing dialog: Nao awaiting present.)"), SelectItem("Present", "Select an item.", "*/nao_dress/*"));

		var selection = await Select();
		Item item = null;

		// If an item was selected.
		if (selection.StartsWith("@select:"))
		{
			var args = selection.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
			long itemEntityId;

			if (!long.TryParse(args[1], out itemEntityId))
			{
				Log.Error("NaoScript: Invalid item selection response '{0}'.", selection);
			}
			else
			{
				item = Player.Inventory.GetItem(itemEntityId);

				if (item == null)
					Log.Warning("NaoScript: Player '{0:X16}' (Account: {1}) tried to gift item they don't possess.", Player.EntityId, Player.Client.Account.Id);

				if (!item.HasTag("/nao_dress/"))
				{
					item = null;
					Log.Warning("NaoScript: Player '{0:X16}' (Account: {1}) tried to use an invalid item, without nao_dress tag.", Player.EntityId, Player.Client.Account.Id);
				}
			}
		}

		// If no item selected or error.
		if (item == null)
		{
			Msg(L("(Missing dialog: Nao disappointed about not getting a present?)"));
			return;
		}

		// Nao's outfits are in the id range 80,000~80256, with the
		// first byte corresponding with the outfit id.
		var itemId = item.Info.Id;
		var outfit = (NaoOutfit)(item.Info.Id - 80000);

		switch (outfit)
		{
			case NaoOutfit.BlackDress:
				GiveItem(80012); // White Dress

				Msg(L("(Missing dialog: Nao receiving Black Dress."));
				break;

			case NaoOutfit.RuasDress:
				GiveKeyword("nao_cloth0");
				GiveKeyword("nao_cloth1");

				Msg(L("(Missing dialog: Nao receiving Rua's Dress."));
				break;

			case NaoOutfit.PinkCoat:
				GiveKeyword("nao_cloth0");
				GiveKeyword("nao_cloth2");

				Msg(L("(Missing dialog: Nao receiving Pink Coat."));
				break;

			case NaoOutfit.BlackCoat:
				GiveKeyword("nao_cloth0");
				GiveKeyword("nao_cloth3");

				Msg(L("(Missing dialog: Nao receiving Pink Coat."));
				break;

			case NaoOutfit.YellowSpringDress:
				GiveKeyword("nao_cloth0");
				GiveKeyword("nao_cloth4");

				Msg(L("(Missing dialog: Nao receiving Yellow Spring Dress."));
				break;

			case NaoOutfit.WhiteSpringDress:
				GiveKeyword("nao_cloth0");
				GiveKeyword("nao_cloth5");

				Msg(L("(Missing dialog: Nao receiving White Spring Dress."));
				break;

			case NaoOutfit.PinkSpringDress:
				GiveKeyword("nao_cloth0");
				GiveKeyword("nao_cloth6");

				Msg(L("(Missing dialog: Nao receiving Pink Spring Dress."));
				break;

			case NaoOutfit.ExplorerSuit:
				GiveKeyword("nao_cloth0");
				GiveKeyword("nao_cloth7");

				Msg(L("(Missing dialog: Nao receiving Explorer Suit."));
				break;

			case NaoOutfit.IriaCasualWear:
				GiveKeyword("nao_cloth0");
				GiveKeyword("nao_cloth8");

				Msg(L("(Missing dialog: Nao receiving Iria Casual Wear."));
				break;

			case NaoOutfit.Yukata:
				GiveKeyword("nao_cloth0");
				GiveKeyword("nao_yukata");

				Msg(L("(Missing dialog: Nao receiving Yukuta."));
				break;

			case NaoOutfit.SantaSuit:
				GiveKeyword("nao_cloth0");
				GiveKeyword("nao_cloth_santa");

				Msg(L("(Missing dialog: Nao receiving Santa Suit."));
				break;

			case NaoOutfit.WhiteDress:
				GiveKeyword("nao_cloth0");
				GiveKeyword("nao_cloth_summer");

				Msg(L("(Missing dialog: Nao receiving White Dress."));
				break;

			case NaoOutfit.Kimono:
				GiveKeyword("nao_cloth0");
				GiveKeyword("nao_cloth_kimono");

				Msg(L("(Missing dialog: Nao receiving Kimono."));
				break;

			case NaoOutfit.SkyBlueDress:
				GiveKeyword("nao_cloth0");
				GiveKeyword("nao_cloth_summer_2008");

				Msg(L("(Missing dialog: Nao receiving Sky-Blue Dress."));
				break;

			case NaoOutfit.PlaywrightCostume:
				GiveKeyword("nao_cloth0");
				GiveKeyword("nao_cloth_shakespeare");

				Msg(L("(Missing dialog: Nao receiving Playwright Costume."));
				break;

			case NaoOutfit.FarmingOutfit:
				GiveKeyword("nao_cloth0");
				GiveKeyword("nao_cloth_farmer");

				Msg(L("(Missing dialog: Nao receiving Farming Outfit."));
				break;

			default:
				Msg(L("(Error: Unknown outfit."));
				return;
		}

		Player.NaoOutfit = outfit;
		RemoveItem(itemId);
	}
}
