//--- Aura Script -----------------------------------------------------------
// Dougal
//--- Description -----------------------------------------------------------
// G1 Mainstream NPC Tir Chonaill (Another World)
//--- History ---------------------------------------------------------------
// 1.0 Added general keyword responses, Shop & Warp
// Missing: Any Quest related conversations
//---------------------------------------------------------------------------

public class DougalScript : NpcScript
{
	public override void Load()
	{
		SetRace(10002);
		SetName("_dougal");
		SetBody(height: 1.1f, weight: 0.9f, upper: 0.9f, lower: 0.9f);
		SetFace(skinColor: 16, eyeType: 5, eyeColor: 126, mouthType: 0);
		SetStand("human/male/anim/male_natural_stand_npc_Duncan");
		SetLocation(35, 15354, 38361, 130);

		EquipItem(Pocket.Face, 4900, 0x00737473, 0x00F14274, 0x00C3DC78);
		EquipItem(Pocket.Hair, 4152, 0x009D845E, 0x009D845E, 0x009D845E);
		EquipItem(Pocket.Armor, 15649, 0x00B8946B, 0x00735F48, 0x001F2922);
		EquipItem(Pocket.Shoe, 17281, 0x002E2E23, 0x00FFD195, 0x004EB964);
		EquipItem(Pocket.RightHand1, 40034, 0x00746C54, 0x00CCC8AB, 0x00A99DCD);

		AddPhrase("I heard someone's prayer... or did I just imagine that?");
		AddPhrase("Mm... Why does my head hurt so much...?");
		AddPhrase("Do they even know where this is...?");
		AddPhrase("What's with all these people?");
		AddPhrase("My leg's itchy...");
		AddPhrase("I don't like crowds...");
		AddPhrase("Hmm...");
		AddPhrase("Sigh...");
		AddPhrase("Mm... Why does my head hurt so much...?");
	}

	protected override async Task Talk()
	{
		SetBgm("NPC_Dougal.mp3");

		await Intro(
			"The young man is of medium height with sandy hair down to his shoulders, his eyes the color of ash.",
			"His leg seems to be bothering him, as he is shifting his weight onto the wooden cane in his right hand.",
			"His well defined chin, serene eyes and lips convey a handsome charm,",
			"but his eyes seem dry and desolate."
		);

		Msg("...How can I help you?", Button("Start a Conversation", "@talk"), Button("Trade", "@shop"), Button("Return to Erinn", "@return"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());
				if (Title == 11002)
				{
					Msg("...So you were able to defeat my body and<br/>push Mores and Cichol out even when you're only a Human.");
					Msg("I've learned that there are<br/>those even among Humans that can be trusted with that kind of abilities.");
				}
				await Conversation();
				break;

			case "@shop":
				Msg("What are you looking for?<br/>");
				OpenShop("DougalShop");
				return;

			case "@return":
				Msg("It seems you wish to leave this place.<br/>Well, I didn't get my hopes up anyway...<br/>I'll help you so you can get back on track.<br/>I wish you a safe trip.", Button("Continue", "@to_erinn"), Button("Cancel", "@cancel"));
				switch (await Select())
				{
					case "@to_erinn":
						Player.Warp(51, 10490, 10997);
						break;

					case "@cancel":
						Msg("It seems like you changed your mind.<br/>Oh well...");
						return;
				}
				return;
		}

		End("You have ended your conversation with <npcname/>.");
	}

	private void Greet()
	{
		if (Memory <= 0)
		{
			Msg(FavorExpression(), L("You're quite brave to come to a place like this. I'm Dougal.<br/>I'm here... alone."));
		}
		else if (Memory == 1)
		{
			Msg(FavorExpression(), L("We meet again.<br/>I didn't think you'd be brave enough to come here so often.<br/>Ahh, no harm intended though."));
		}
		else if (Memory == 2)
		{
			Msg(FavorExpression(), L("You're back. I was wondering why you hadn't come back yet."));
		}
		else if (Memory <= 6)
		{
			Msg(FavorExpression(), L("We meet again, <username/>. How may I help you today?"));
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
				if (Memory == 1)
				{
					Msg("You're quite brave to come to a place like this. I'm Dougal.<br/>I'm here... alone.");
					ModifyRelation(1, 0, Random(2));
				}
				else
				{
					Msg(FavorExpression(), "About me? You want to know all sort of things, don't you?");
					Msg("I'm just a loser who was left alone to guard this town.");
				}
				break;

			case "rumor":
				Msg(FavorExpression(), "Have you, by chance, ever heard of the parallel world?<br/>Well, it's nothing special...<br/>It's just that whoever comes here<br/>tells me that this place looks like some other place they've been to.");
				Msg("If this really is a parallel world,<br/>it must have some sort of a connection to the real world.<br/>Still, I'm not sure which world is the real one and which one is the mirror of it.");
				Msg("If a change is to occur in one world,<br/>that would mean that a change would occur in the other world as well.<br/>I'm not sure just how it would change, but...");
				ModifyRelation(Random(2), 0, Random(2));
				break;

			case "shop_misc":
				Msg("I'm sure you're able to see it for yourself, but you shouldn't expect<br/>such things around here.");
				break;

			case "shop_grocery":
				Msg("We used to have it before, but as you can see right now...<br/>But it's ok...<br/>In fact, we hardly miss it.");
				Msg("Hmm...<br/>Now that you mention it, it is strange.<br/>Food, you say?");
				Msg("Ahh, let's forget it.<br/>There's no need to waste time on such trivial matters.");
				break;

			case "shop_healing":
				Msg("If you don't know how to heal yourself,<br/>you shouldn't plan on sticking around here.");
				Msg("As I've said, there's no one else besides me.<br/>Obviously, there's no Healer either.");
				Msg("If you really need something, I can sell you some<br/>of the items left in town.<br/>If you need something, press Trade.");
				break;

			case "shop_inn":
				Msg("I wouldn't stop you from going into<br/>any of the houses to sleep, but...");
				Msg("You should be careful.<br/>Make sure you don't get taken out by those zombies.");
				Msg("Me?<br/>There's no way mere zombies can bother me.<br/>You see...");
				Msg("......<br/>What am I babbling about?<br/>Oh... Forget what I said. ");
				break;

			case "shop_bank":
				Msg("You are so brave to come here<br/>without even any essential items.");
				Msg("How about calling for an owl?<br/>Of course, that's only if they actually fly out this far.<br/>Hahaha...");
				break;

			case "skill_rest":
				Msg("Not being able to rest when you're tired<br/>is quite exhausting.");
				Msg("What does that mean?");
				Msg("I'm not sure.<br/>I wonder what I was trying to say.");
				break;

			case "skill_range":
				Msg("It's an excellent skill to keep your enemy<br/>in check from a distance.<br/>The skill won't be of much use by itself, though.");
				Msg("Well, the choice is up to you.");
				break;

			case "skill_instrument":
				Msg("Instrument playing?");
				Msg("(Dougal doesn't seem to understand for a short while.)");
				Msg("Ahh!<br/>Are you talking about those things Humans use to make a loud noise?");
				Msg("...?<br/>Is something wrong?<br/>Oh, what I meant was, how loudly people play their instruments.");
				break;

			case "skill_composing":
				Msg("Do I look like someone who would waste time on such a thing?<br/>Hahaha.");
				break;

			case "skill_magnum_shot":
				Msg("Come to think of it,<br/>I was the only one who could teach this in the last town I was in.<br/>No one asks me about that now, though.<br/>Ah, except for you, <username/>. Haha...");
				break;

			case "skill_counter_attack":
				Msg("It's a skill that counters the force of an opponent's attack,<br/>inflicting a lot of damage. It cannot be used without having a<br/>certain level of skill to accurately see through an opponent's attack.");
				Msg("Once again, it's not a skill you can use if you don't have<br/>the stamina to stay on your toes while an opponent is targeting you.");
				Msg("In other words, don't try it without being fit to use it.");
				break;

			case "skill_smash":
				Msg("It is the best skill to break through the enemy's defense<br/>at once and attack.<br/>It will leave you to be that much more vulnerable.");
				Msg("It's better off to not use it if you aren't certain that<br/>the enemy can't block it.");
				break;

			case "skill_gathering":
				Msg("Primitive creatures are known for gathering whatever they can<br/>by all means. Does that mean Humans are primitive creatures as well?<br/>Haha...");
				Msg("......");
				Msg("Ah, what am I saying?<br/>I guess I was babbling again.<br/>Just forget what I said.");
				break;

			case "square":
				Msg("I just remembered that a lot of people asked about that paved-in spot.<br/>I heard that's where the town Square was supposed to be or something.");
				Msg("This may be a parallel world,<br/>but not everything is bound to be exactly the same.<br/>So I find people who ask such questions to be pretty amusing.");
				break;

			case "pool":
				Msg("This town's reservoir is what everyone put all their effort into<br/>for the sake of the town.<br/>The one who took care of it...");
				Msg("......");
				Msg("I'm sorry.<br/>I can't remember very well.");
				break;

			case "farmland":
				Msg("You reap what you sow.<br/>Meaning, there's nothing to reap if nothing was sown.");
				Msg("I never sowed,<br/>so isn't it obvious that there's no farmland here?");
				break;

			case "windmill":
				Msg("I'm not sure how that is related to the Windmill skill.");
				break;

			case "shop_headman":
				Msg("This is the place.<br/>This is the only house that is still sound and intact.<br/>Since I'm the only one here, there's no such thing as a Chief or anything.<br/>Haha... ");
				break;

			case "temple":
				Msg("I'm not sure what you're talking about, but no such thing exists here.<br/>God? Hm.. I don't know.<br/>If a God who watched over mankind existed,<br/>not everyone would be gone like this.");
				Msg("If there is a being who protects your people,<br/>then that's your God. It has nothing to do with me.");
				break;

			case "school":
				Msg("I'm not sure what you seek to learn here, but I think it's a useless question.<br/>No one teaches or learns here.<br/>I will be the only one eternally standing here.");
				break;

			case "skill_windmill":
				Msg("It's an appropriate skill to knock away surrounding enemies all at once.");
				Msg("Even so,<br/>you're not going to ask me to demonstrate it with this leg, are you?<br/>Haha...");
				break;

			case "skill_campfire":
				Msg("......");
				Msg("I don't like fire.");
				break;

			case "shop_restaurant":
				Msg("<username/>, you seem to have a habit of talking in your sleep<br/>with your eyes open.<br/>What makes you think this kind of town has such a thing?");
				break;

			case "shop_armory":
			case "shop_bookstore":
			case "shop_smith":
				Msg("You sure have high expectations from a place with a population of... one.");
				break;

			case "shop_cloth":
				Msg("Do you think that such a thing is needed in this town?");
				break;

			case "shop_goverment_office":
				Msg("That never existed in this town to begin with.");
				break;

			case "graveyard":
				Msg("This town used to have a Graveyard,<br/>but now the whole town is one. Haha...");
				Msg("It's just that it's quite painful to see corpses walking about.<br/>That should be taken care of somehow.´");
				Msg("Are those zombies the people that used to live here?<br/>I'm not sure. I guess so.<br/>I can't remember clearly.");
				break;

			case "bow":
				Msg("This town used to have a skilled blacksmith.<br/>Although, weapons breaking during repairs once in a while has been a problem.");
				Msg("We don't even have anyone now, so obviously...<br/>And no, we don't have anyone selling a bow either.");
				break;

			case "lute":
				Msg("Those instruments produce some good sounds.<br/>It does get on my nerves sometimes, though. ");
				Msg("I'm not sure why it does.");
				break;

			case "tir_na_nog":
				Msg("You still believe that this is a kind of paradise,don't you?");
				Msg("This is just another world that is similar to yours.<br/>It's no more or less than that.");
				Msg("You will be better off quickly forgetting about any fantasies about Tir Na Nog.");
				break;

			case "mabinogi":
				Msg("I'm not sure.<br/>Do you think this world befits such a song?");
				Msg("Even if Mabinogi did exist, there is no one left to sing it.");
				break;

			case "musicsheet":
				Msg("These noisy and fleeting moments...<br/>I'm not sure how you intend to capture them onto paper.");
				Msg("However, I do recall someone remarking that they are beautiful.<br/>I wonder who that was?");
				break;

			case "nao_friend":
				Msg("Nao?<br/>Who's Nao?");
				Msg("That name has a strangely familiar ring to it.<br/>I might have heard someone calling that name before.");
				break;

			case "nao_owl":
				Msg("Owls hang around this region<br/>but that has nothing to do with this person named Nao.");
				break;

			case "g3_01_BrokenSeal":
				Msg("Goddess Morrighan...seems like quite a needy goddess.<br/>Didn't you just rescue her not too long ago?<br/>She can't even maintain a seal properly...");
				Msg("Unless she's doing it intentionally<br/>to provide you<br/>with an adventure or something.");
				break;

			default:
				RndFavorMsg(
					"I'm not sure. I can't exactly answer that question.",
					"I don't know much about that.",
					"I'm much too limited to answer that.",
					"Hmm...I'm not sure. I really don't know.",
					"I'm simply not sure. Do you have any other questions?"
				);
				ModifyRelation(0, 0, Random(2));
				break;
		}
	}
}

public class DougalShop : NpcShopScript
{
	public override void Setup()
	{
		Add("Potions", 51001);     // HP 10 Potion 1x
		Add("Potions", 51002, 1);  // HP 30 Potion 1x
		Add("Potions", 51002, 10); // HP 30 Potion 10x
		Add("Potions", 51002, 20); // HP 30 Potion 20x
		Add("Potions", 51006);     // MP 10 Potion 1x
		Add("Potions", 51007, 1);  // MP 30 Potion 1x
		Add("Potions", 51007, 10); // MP 30 Potion 10x
		Add("Potions", 51007, 20); // MP 30 Potion 20x
		Add("Potions", 51011);     // Stamina 10 Potion 1x
		Add("Potions", 51012, 1);  // Stamina 30 Potion 1x
		Add("Potions", 51012, 10); // Stamina 30 Potion 10x
		Add("Potions", 51012, 20); // Stamina 30 Potion 20x

		Add("First Aid Kits", 60005, 10); // Bandage 10x
		Add("First Aid Kits", 60005, 20); // Bandage 20x
		Add("First Aid Kits", 63000, 10); // Phoenix Feather 10x
		Add("First Aid Kits", 63000, 20); // Phoenix Feather 20x

		AddQuest("Quest", 210021, 0); // Collect the Black Orb Fragments
		AddQuest("Quest", 71051, 30); // Collect the Dingo's Fomor Scrolls
	}
}
