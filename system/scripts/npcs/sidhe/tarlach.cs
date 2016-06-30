//--- Aura Script -----------------------------------------------------------
// Tarlach
//--- Description -----------------------------------------------------------
// Druid form
//--- History ---------------------------------------------------------------
// 1.0 Added general keyword responses and day/night cycle
// Missing: Any Quest related conversations and Spirit Weapon
//---------------------------------------------------------------------------

public class TarlachScript : NpcScript
{
	public override void Load()
	{
		SetRace(10002);
		SetName("_tarlach");
		SetBody(height: 1.1f, weight: 0.9f, upper: 0.4f, lower: 0.6f);
		SetFace(skinColor: 15, eyeType: 4, eyeColor: 54, mouthType: 0);
		SetStand("human/male/anim/male_natural_stand_npc_Duncan");
		if (ErinnTime.Now.IsNight)
			SetLocation(48, 11100, 30400, 167);
		else
			SetLocation(22, 5800, 7100, 167);

		EquipItem(Pocket.Face, 4901, 0x00724645, 0x0055695D, 0x0000A0DD);
		EquipItem(Pocket.Hair, 4021, 0x10000023, 0x10000023, 0x10000023);
		EquipItem(Pocket.Armor, 15002, 0x00B80026, 0x00B80026, 0x00576D8D);
		EquipItem(Pocket.Shoe, 17009, 0x00563211, 0x00FCDCD5, 0x007DA834);
		EquipItem(Pocket.Head, 18028, 0x00625F44, 0x00C0C0C0, 0x00601469);
		EquipItem(Pocket.Robe, 19004, 0x00CA7B34, 0x00B45031, 0x00DABC87);
		SetHoodDown();

		AddPhrase("(Cough, Cough)");
		AddPhrase("Sigh...");
		AddPhrase("...My head...");
		AddPhrase("...Is it not morning yet...?");
		AddPhrase("...I'll just wait a little longer...");
		AddPhrase("I guess not yet...");
		AddPhrase("...It's definitely cold at night...");
		AddPhrase("Ah...");
		AddPhrase("...I can take it...");
	}

	[On("ErinnDaytimeTick")]
	public void OnErinnDaytimeTick(ErinnTime time)
	{
		if (time.IsNight)
			NPC.WarpFlash(48, 11100, 30400);
		else
			NPC.WarpFlash(22, 5800, 7100);
	}

	protected override async Task Talk()
	{
		SetBgm("NPC_Tarlach.mp3");

		await Intro(
			"A man wearing a light brown robe silently glares this way.",
			"He has wavy blonde hair and white skin with a well defined chin that gives off a gentle impression.",
			"Behind his thick glasses, however, are his cold emerald eyes filled with silent gloom."
		);

		Msg("...Mmm...", Button("Start a Conversation", "@talk"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());
				if (Title == 11002)
				{
					Msg("...You've accomplished what<br/>Mari, Ruairi and myself could not do....<br/>...Thank you.");
					Msg("......");
					Msg("...But...Ruairi...<br/>What's going to happen to him...?");
					Msg("...Please. <username/>...<br/>If you hear anything about Ruairi...");
					Msg("...Please let me know...");
				}
				await Conversation();
				break;
		}

		End("You have ended your conversation with <npcname/>.");
	}

	private void Greet()
	{
		if (Memory <= 0)
		{
			Msg(FavorExpression(), L("(Cough, Cough)...<br/>So you have made it through the barrier and have reached this desolate place."));
		}
		else if (Memory == 1)
		{
			Msg(FavorExpression(), L("...It seems like you want to ask me something...<br/>Don't be shy about it...just ask me.<br/>(Cough)..."));
		}
		else if (Memory == 2)
		{
			Msg(FavorExpression(), L("...You came, <username/>...mmm...<br/>I was just thinking it was about time for you to show up."));
		}
		else if (Memory <= 6)
		{
			Msg(FavorExpression(), L("(Missing)"));
		}
		else
		{
			Msg(FavorExpression(), L("(Missing)"));
		}

		UpdateRelationAfterGreet();
	}

	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				Msg(FavorExpression(), "...Don't try to find out too much about me.");
				ModifyRelation(Random(2), 0, Random(2));
				break;

			case "rumor":
				Msg(FavorExpression(), "This is Sidhe Sneachta...<br/>a land that is blocked away from the rest of the world.");
				Msg("...I'm surprised that you got through to here.");
				ModifyRelation(Random(2), 0, Random(2));
				break;

			case "shop_misc":
				Msg("...There's no such thing here.");
				break;

			case "shop_grocery":
			case "shop_restaurant":
				Msg("...Are you looking for food?");
				Msg("Haha... Around here, you have to find your own food.<br/>That's how I've survived until now.");
				break;

			case "shop_healing":
				Msg("...Are you okay?");
				break;

			case "shop_inn":
			case "shop_bank":
			case "lute":
				Msg("...You really expect to find that here...?");
				Msg("You should think about what you're asking...before you say it. ");
				break;

			case "shop_smith":
				Msg("...Even if I were a blacksmith, I wouldn't want to set up a Blacksmith's Shop in a place like this.<br/>Would you...? ");
				break;

			case "skill_rest":
				Msg("...It should help you recover your HP.");
				Msg("...Don't tell me you've come all the way here without even learning that...?");
				break;

			case "skill_range":
				Msg("...So you broke through the barrier and made it all the way here<br/>...just to learn about the long-ranged attack?");
				Msg("I would imagine there are easier ways...<br/>it seems like you enjoy making things difficult for yourself...");
				break;

			case "skill_instrument":
				Msg("...Practice in the dungeon, and play your instrument at the Square.");
				break;

			case "skill_tailoring":
				Msg("...it's not easy to be in cold places like this when your hands are getting numb.");
				break;

			case "skill_magnum_shot":
			case "skill_counter_attack":
			case "skill_smash":
				Msg("...");
				Msg("...Sorry...<br/>You just reminded me of someone I know...");
				break;

			case "skill_gathering":
				Msg("...When I change into a bear, gathering Mana Herbs becomes a little bit easier...");
				Msg("But, please don't try gathering Mana Herbs here.");
				break;

			case "square":
				Msg("...If you wish to go there, you'll have to leave this place.");
				break;

			case "pool":
				Msg("...I guess you could make a reservoir by melting all this snow into one place... ");
				Msg("...But why would you want to do that...?");
				break;

			case "farmland":
				Msg("...As you can see, it's a little chilly here ...so you can't really farm here.");
				break;

			case "windmill":
				Msg("...You're asking someone inside the barrier about things outside of it...<br/>I don't know how you expect me to answer that...");
				break;

			case "brook":
				Msg("...That's the name of the stream near Tir Chonaill.... ");
				break;

			case "shop_headman":
				Msg("...By Chief, do you mean the Chief of Tir Chonaill?");
				Msg("...You're not asking me because you don't know where Tir Chonaill is...are you?");
				break;

			case "temple":
				Msg("...You're talking about the Church of Lymilark...<br/>All the Priests and Priestesses there are so handsome and beautiful that some even wonder if the church chooses their clergy by looks...");
				Msg("...But that's not true...<br/>It's only natural that those who have peace in their hearts would become more beautiful on the outside. ");
				Msg("...Just because there are beautiful people who don't have peace in their hearts doesn't mean what I'm saying is not true...");
				break;

			case "school":
				Msg("...Education is important, but don't expect too much from Schools.");
				Msg("A School can teach you knowledge, but ultimately, the training is up to you.");
				break;

			case "skill_windmill":
				Msg("...It's a powerful regional-attack combat skill.");
				Msg("I've never used it myself, but...<br/>if you want to learn it, you should go to the School.<br/>You should be able to learn it there. ");
				break;

			case "skill_campfire":
				Msg("...I don't want to stop you from starting a fire, but please keep it quiet and make sure it doesn't spread anywhere else...");
				break;

			case "shop_armory":
				Msg("...You're asking about a Weapons Shop at a remote place like this...?<br/>If there was a Weapons Shop here and people began coming in here freely...");
				Msg("there would probably be hunters who would try to hunt innocent bears minding their own business...");
				break;

			case "shop_cloth":
				Msg("...Do you need winter clothing or something...?");
				break;

			case "shop_bookstore":
				Msg("...There's a Bookstore in Dunbarton.<br/>A cute lady works there who's also very intelligent;<br/>she sells all different kinds of books.");
				Msg("...If you haven't been there already, I suggest you drop by sometime.");
				break;

			case "shop_goverment_office":
				Msg("...There's a Town Office in Dunbarton.<br/>Sometimes I go there as a bear...<br/>but I've been attacked by travelers on a few occasions which was very painful.");
				Msg("I've heard that you can retrieve lost items there.<br/>I don't really have a problem losing things so I never had to use the service.");
				Msg("...But to be honest, you have to be pretty clumsy to drop items like that or lose them...<br/>Don't you think so...?");
				break;

			case "graveyard":
				Msg("...The nearest Graveyard from here is in Tir Chonaill.<br/>You should go there and ask.");
				break;

			case "bow":
				Msg("...");
				Msg("...Sorry... You just reminded me of someone I know...");
				break;

			case "tir_na_nog":
				Msg("...That name is special to me...<br/>But I don't want to think about it...");
				Msg("...It's hard enough trying to shake off the memories of each day...");
				break;

			case "mabinogi":
				Msg("True Mabinogi is passed down through word of mouth by the Druids. ");
				break;

			case "musicsheet":
				Msg("...Are you interested in music?<br/>It would be nice to have Music Scores but, a true Druid doesn't need a score to play.");
				Msg("...He plays everything from memory...<br/>the flow of the song, rhythm, and even the meaning of the song...");
				break;

			case "nao_friend":
				if (HasKeyword("g3_complete"))
					Msg("Mari... I mean, Nao... said that...?<br/>......<br/>...Okay... ");
				else
					Msg("...Who's Nao?<br/>That's the first time I've heard of that name.");
				break;

			case "nao_owl":
				Msg("...If there's anyone who could communicate with animalsand control them,<br/>that person would probably be someone who is capable of advanced magic skills...");
				Msg("I used to do things like that, but...<br/>that was a long time ago...");
				break;

			case "nao_blacksuit":
				Msg("...Black clothes are usually worn by people who are in mourning...<br/>Have you ever worn them...?");
				Msg("If someone is dressed in black, you can assume that someone close to them has passed away.");
				break;

			case "nao_owlscroll":
				Msg("...Yes.<br/>An owl is a sacred animal that symbolizes wisdom.<br/>People say that owls are capable of memorizing everything they see and reporting it to their masters.");
				Msg("...That means that every scroll that is delivered by an owl must be important.");
				break;

			default:
				RndFavorMsg(
					"...It'd be nice if I'd had the chance to find out more about it, but...<br/>It doesn't seem like that's going to happen anytime soon...",
					"...Have you asked anyone else?",
					"...Honestly, I don't know much about that.",
					"Hmm...I'm not sure. I really don't know.",
					"...Is that right?  But, that's something I can't really comment on.",
					"...It's outside of my interests, so I don't really have an answer for you."
				);
				ModifyRelation(0, 0, Random(2));
				break;
		}
	}
}