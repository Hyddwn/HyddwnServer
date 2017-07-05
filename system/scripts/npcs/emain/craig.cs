//--- Aura Script -----------------------------------------------------------
// Craig
//--- Description -----------------------------------------------------------
// Paladin Training Captain
//---------------------------------------------------------------------------

public class CraigScript : NpcScript
{
	public override void Load()
	{
		SetRace(10002);
		SetName("_craig");
		SetBody(height: 1.29f, upper: 1.2f);
		SetFace(skinColor: 20, eyeType: 9, eyeColor: 125, mouthType: 12);
		SetStand("monster/anim/ghostarmor/Tequip_C/ghostarmor_Tequip_C01_stand_friendly");
		SetLocation(52, 25168, 59202, 125);
		SetGiftWeights(beauty: 1, individuality: 2, luxury: -1, toughness: 2, utility: 2, rarity: 0, meaning: -1, adult: 2, maniac: -1, anime: 2, sexy: 0);

		EquipItem(Pocket.Face, 4902, 0x00EA0168, 0x00A3CB39, 0x00DDF0F4);
		EquipItem(Pocket.Hair, 4030, 0x00453012, 0x00453012, 0x00453012);
		EquipItem(Pocket.Armor, 13024, 0x0091755B, 0x00463424, 0x0036241B);
		EquipItem(Pocket.RightHand2, 40033, 0x00B7B6B8, 0x00C48246, 0x009AAFA2);

		AddPhrase("These trainees aren't nearly as good as they used to be...");
		AddPhrase("...");
		AddPhrase("They certainly don't live up to their name as the \"Mighty Knights\"...");
		AddPhrase("I can't believe those guys...");
	}

	protected override async Task Talk()
	{
		SetBgm("NPC_Craig.mp3");

		await Intro(L("He wears an exquisite armor, more refined and detailed than the kinds normally found in regular Weapons Shops.<br/>His solid build cannot be hidden, even with the larger-than-normal armor. His thick neck supports a lively face,<br/>while his sharp nose and jawline are amongst the most noticeable features on his tanned face.<br/>His tidy haircut and smooth skin give you the impression of authority."));

		Msg("...If you don't have any specific business around here, you really should stay away from here.", Button("Continue", "@talk"));
		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());

				if (Player.IsUsingTitle(11002))
				{
					Msg("...I see...<br/>So you followed your dreams<br/>and accomplished something worthwhile...");
					Msg("...I'm not ashamed of the life I've lived;<br/>however, I am ashamed<br/>of doubting yours...");
					Msg("...Sorry, <username/>.<br/>I will pray for the blessing of the Goddess<br/>to be with you for the path that lies ahead of you.");
				}

				await Conversation();
				break;
		}

		End("Thank you, <npcname/>. I'll see you later!");
	}

	private void Greet()
	{
		if (Memory <= 0)
		{
			Msg(FavorExpression(), L("I'm Craig.<br/>State your name and rank."));
		}
		else if (Memory == 1)
		{
			Msg(FavorExpression(), L("<username/>? What do you want from me?"));
		}
		else if (Memory == 2)
		{
			Msg(FavorExpression(), L("You seem very interested in the Holy Knights."));
		}
		else if (Memory <= 6)
		{
			Msg(FavorExpression(), L("Is there something you want to say?"));
		}
		else
		{
			Msg(FavorExpression(), L("I was wondering who it was.<br/>How could I forget? You come here so often, I wouldn't forget you even if I'd tried."));
		}

		UpdateRelationAfterGreet();
	}

	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				Msg(FavorExpression(), "I'm the leader of the Paladins,<br/>and I'm in charge of the training program here.<br/>If you want to dedicate your life to fighting Fomors,<br/>please sign up for the Paladin Training Course!");
				ModifyRelation(1, 0, 0);
				break;

			case "rumor":
				Msg(FavorExpression(), "I heard more people have been traveling to Ceo Island.<br/>I hope this is just a rumor...<br/>I just hope no one loses their lives<br/>going after anything foolish there.");
				ModifyRelation(Random(2), 0, Random(3));
				break;

			case "about_study":
				// After G2, keyword could be wrong, may change in the middle 
				// of g2 after quitting paladins, requires testing.
				if (Player.HasKeyword("g2_complete"))
				{
					Msg("You want to take the Paladin Training Course again...?<br/>Hmmm...");
					Msg("Our policy is that you can't rejoin the training course once you've quit.");
				}
				else
					Msg("...I'm sorry to tell you this, but...<br/>you don't have what it takes to become a Paladin.");
				break;

			case "shop_misc":
				Msg("Have you gone to visit Galvin yet?<br/>It's not necessarily a General Shop<br/>but it's the closest thing to it.");
				break;

			case "shop_grocery":
				Msg("If you are looking for food ingredients, go visit Loch Lios.<br/>You may be able to find what you need there...<br/>But Loch Lios isn't just a Grocery Store, that's for sure.");
				break;

			case "shop_healing":
				Msg("Go talk to Agnes.<br/>You can find her northeast of the Square.");
				break;

			case "shop_inn":
				Msg("There's no Inn around Emain Macha<br/>that I can recall...<br/>If you need a place to camp, use the training ground.");
				break;

			case "shop_bank":
				Msg("Madam Jocelin is such a solid, reliable person.<br/>She's not only knowledgeable with regard to money<br/>but is also good to consult with about other things as well.");
				break;

			case "shop_smith":
				Msg("There's a Blacksmith's Shop for Paladins and Royal Guards, but<br/>you probably can't go there...<br/>Look for a Weapons Shop. It is located west of the Square.");
				break;

			case "skill_range":
				Msg("The Ranged Attack skill uses bows or crossbows to attack enemies from a distance.<br/>You can use it right away without any training.<br/>However, in order master it, you will need a lot of practice.");
				break;

			case "skill_composing":
				Msg("Maybe the Bard at the Square would know something about it...<br/>Why don't you go talk to him.");
				break;

			case "skill_tailoring":
				Msg("Go ask Ailionoa at the Clothing Shop.<br/>I don't know why you're asking me...");
				break;

			case "skill_magnum_shot":
				Msg("With this skill, you'll be able to knock down an enemy in a single blow.<br/>Since Archers are at a disadvantage when enemies are in close proximity,<br/>this skill can be very useful for those who use the bow as their main weapon.");
				break;

			case "skill_counter_attack":
				Msg("You can only learn the Counterattack skill by experiencing it first hand.<br/>You should be able to learn it quickly by going through<br/>the live training in the dungeon.");
				break;

			case "skill_smash":
				Msg("You can knock enemies down with a forceful blow,<br/>but it takes time to find your balance.<br/>Therefore, it is better to have good defense skills<br/>as it is important that you become a balanced fighter.");
				break;

			case "skill_windmill":
				Msg("Didn't they teach you that at the School...?<br/>Well if you haven't learned it yet, you should.");
				break;

			case "farmland":
				Msg("Corn is the main crop<br/>around Emain Macha.<br/>It is also our specialty.");
				break;

			case "shop_headman":
				Msg("Don't you know this town is under the authority of the Lord?<br/>There's no Chief in this town.");
				break;

			case "temple":
				Msg("If you walk down towards the lake, you'll see the Emain Macha Cathedral.<br/>Bishop Wyllow at the Cathedral is a very quiet man.<br/>Priest James, however, is the one who handles most of the administrative work of the church.");
				break;

			case "school":
				Msg("If you are talking about the Magic School,<br/>it is no longer here.<br/>It was closed down due to a number of reasons.");
				break;

			case "shop_restaurant":
				Msg("Loch Lios is a famous restaurant,<br/>not only within Emain Macha but in Tara as well.<br/>I've heard that even the Lord of the manor enjoys the food there.");
				Msg("I hope he gets well soon...");
				break;

			case "shop_armory":
				Msg("Osla's Weapons Shop is pretty reliable...<br/>Well, besides the fact that her calculations are<br/>slightly off from time to time...");
				break;

			case "shop_cloth":
				Msg("Although I haven't had the chance to visit as of yet,<br/>I hear that Ailionoa's Clothing Shop is know throughout.<br/>Even extremely wealthy people all the way from Tara<br/>ask for her designs, which obviously proves just how good she must be.");
				break;

			case "shop_bookstore":
				Msg("We don't have a Bookstore...<br/>If you have a book you want to read,<br/>then either get permission from the Church board");
				Msg("or you may be able to get it from Buchanan.<br/>He is currently in Tara so I don't know when he'll be back.");
				break;

			case "graveyard":
				Msg("Shadows appear their darkest when the light is the brightest.<br/>If you are determined to see the shadows of Emain Macha,<br/>I won't stop you.");
				Msg("However, I hope you don't blame the shadow<br/>on those who are giving the light.");
				break;

			case "skill_fishing":
				Msg("I don't know much about that...<br/>Why don't you ask Priest James?<br/>He seems to have some interests in that.");
				break;

			case "bow":
				Msg("Go visit Osla's Weapons Shop.<br/>The Weapons Shop is just west of the Square.");
				break;

			case "musicsheet":
				Msg("The Bard at the Square was selling one.<br/>That guy used to do many live performances, but<br/>for some reason, he seems to have stopped performing regularly...");
				break;

			case "nao_owlscroll":
				Msg("That's a very useful communication tool.<br/>It's usually for Officials, but anyone can use it as long<br/>as they receive permission from the Castle.");
				break;

			case "g3_DarkKnight":
				Msg("Dark Knight...");
				Msg("...Please don't tell anyone.");
				Msg("...But I recently saw the previous Lord's eldest son.<br/>He's the one who left the Castle one day out of the blue..<br/>He also quit mid way through his Paladin training.");
				Msg("...He became a Dark Knight.");
				Msg("......");
				Msg("To be honest...<br/>I'm worried that<br/>you will follow in his footsteps.");
				Msg("There's something about you...<br/>that resembles him a great deal...");
				break;

			default:
				RndFavorMsg(
					"I have no idea.",
					"What was that...?",
					"Huh?",
					"...You should ask<br/>someone else about him.",
					"Why don't you ask something else.",
					"I don't know.",
					"Well..."
				);
				ModifyRelation(0, 0, Random(3));
				break;
		}
	}

	protected override async Task Gift(Item item, GiftReaction reaction)
	{
		switch (reaction)
		{
			default: // GiftReaction.Neutral
				Msg("...As long as you don't have any hidden intentions, I'll thankfully take it.");
				break;
		}
	}
}
