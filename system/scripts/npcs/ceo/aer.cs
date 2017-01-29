//--- Aura Script -----------------------------------------------------------
// Aer
//--- Description -----------------------------------------------------------
// Water spirit in the underground spring at Ceo Island
// Gives a Rundal Siren Pass when gifted with a Suspicious Fomor Pass
//---------------------------------------------------------------------------

public class AerScript : NpcScript
{
	public override void Load()
	{
		SetRace(19);
		SetName("_aer");
		SetBody(height: 1.3f, lower: 1.2f);
		SetLocation(68, 5599, 8550, 192);
		SetGiftWeights(beauty: 1, individuality: 2, luxury: -1, toughness: 2, utility: 2, rarity: 0, meaning: -1, adult: 2, maniac: -1, anime: 2, sexy: 0);

		AddPhrase("I hear the water's grieving...");
		AddPhrase("How do I look...?");
		AddPhrase("Why do people...");
		AddPhrase("......");
		AddPhrase("This place is pretty cozy...for a spirit...");
	}

	protected override async Task Talk()
	{
		await Intro(L("With hair that seems as pale as water,<br/>she is an elf whose body is so slender that she actually seems as if she could slip through the cracks of my hand.<br/>She is looking my way with her endlessly deep dark blue eyes.<br/>Her pale skin and lips show fear and wariness, as if at the slightest movement, she might run away."));

		Msg("Who are you... And why are you here...?", Button("Start a Conversation", "@talk"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());

				if (Player.IsUsingTitle(11002))
				{
					Msg("...The story about you, <username/>,<br/>has already spread throughout the world of the spirits...");
					Msg("...Your courage of how you<br/>risked your life to save the world...");
					Msg("Because of people such as yourself,<br/>we believe that humans are beautiful creatures...");
					Msg("Thank you... <username/>...");
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
			Msg(FavorExpression(), L("Did you come all the way here just to throw a stone at me...?"));
		}
		else if (Memory == 1)
		{
			Msg(FavorExpression(), L("Ah...We've met before....Right?"));
		}
		else if (Memory == 2)
		{
			Msg(FavorExpression(), L("Ah... You must be...<username/>... Right?"));
		}
		else if (Memory <= 6)
		{
			Msg(FavorExpression(), L("Ah... It's you, <username/>. I was wondering who it was..."));
		}
		else
		{
			// Placeholder? Could not get a 5th greeting to appear, does she not have one?
			Msg(FavorExpression(), L("Ah... It's you, <username/>. I was wondering who it was..."));
		}

		UpdateRelationAfterGreet();
	}

	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				Msg(FavorExpression(), "I am...only the Spirit of water...");
				ModifyRelation(Random(2), 0, Random(3));
				break;

			case "rumor":
				Msg(FavorExpression(), "Humans believe... that people who die near the water<br/>become water spirits...<br/>So sometimes people are afraid of me...");
				Msg("......");
				Msg("But I have no idea...whether it's really like that or not.");
				ModifyRelation(Random(2), 0, Random(3));
				break;

			case "skill_instrument":
				Msg("I love...music...");
				Msg("but...");
				break;

			case "pool":
				Msg("Trapping water in one place is<br/>such a cruel act.<br/>If a spirit were to live in that kind of an environment,<br/>it wouldn't be able to survive ...");
				break;

			case "skill_fishing":
				Msg("Humans too are part of nature...<br/>I gues I can't say anything<br/>about hunting for food...");
				Msg("But why do Humans<br/>throw away garbage into rivers and lakes<br/>that don't belong in there...?");
				Msg("I wish they would keep such things out of the water...");
				break;

			case "g3_DarkKnight":
				Msg("Dark Knight...?<br/>They are a sect of human warriors<br/>who believe that<br/>humans can coexist with Fomors.");
				Msg("...Most people despise them<br/>for betraying their own kind...");
				Msg("But in my opinion,<br/>I believe they are the most human,<br/>in that they are willing to understand<br/>other beings and creatures firsthand.");
				break;

			default:
				RndFavorMsg(
					"I'm just a spirit...<br/>I don't know about...",
					"Do you think I would know about such things...?",
					"The only things I do know about are...<br/>things that happen in the water...",
					"I'm not a human...<br/>I wouldn't know about things like that...",
					"Sorry... I don't know anything about that."
				);
				ModifyRelation(0, 0, Random(3));
				break;
		}
	}

	protected override async Task Gift(Item item, GiftReaction reaction)
	{
		if (IsEnabled("RundalSirenDungeon") && item.Info.Id == 63103) // Suspicious Fomor Pass
		{
			Player.GiveItem(63102);
			Player.SystemNotice(L("Received Rundal Siren Dungeon Pass from Aer."));

			if (Player.Vars.Perm["rundalSirenClear"] == null)
			{
				Msg(L("...This is ...a dungeon pass...isn't it...?<br/>What is it for...?<br/>Oh, I see..."));
				Msg(L("<username/>,<br/>have you ever heard of Siren?"));
				Msg(L("Usually, spirits of water, are created by the influence of nature's forces...<br/>However, from time to time,<br/>there are spirits created by the influence of the Human soul."));
				Msg(L("I don't know how, but Sirens are<br/>created by these influences of the Human soul...<br/>especially Humans who had drowned...their hatred...sorrows...<br/>And it is all those negative emotions which cause these Sirens to become Fomors as everyone knows them."));
				Msg(L("Sirens try to harm those<br/>who come close to water.<br/>They seduce Humans with a song that is hard to resist ..."));
				Msg(L("And just when a Human approaches one of them,<br/>they will drown the person to death and eerily laugh at them.<br/>They also like to trick Humans with other mischievous tricks and will sometimes even steal peoples' belongings."));
				Msg(L("<username/>, can you... stop these Sirens' mischievous behavior?<br/>I think it's about time for them to be stopped...<br/>I'll give you a pass to go to the Sirens' Dungeon."));
				Msg(L("If you drop this pass onto the altar of Rundal dungeon,<br/>you will be able to get into the dungeon where the Sirens live..."));
			}
			else
			{
				Msg(L("<username/>...<br/>Have you been to the Sirens' Dungeon<br/>that we talked about?"));
				Msg(L("That's strange...<br/>If you've visted their dungeon, <username/>,<br/>all the rumors about them should've stopped by now ..."));
				Msg(L("From what I'd heard from my friends,<br/>the Sirens have been acting exactly the same!<br/>I wish they would learn their lesson already..."));
				Msg(L("Since you brought me this pass,<br/>I'll change it so that<br/>you can re-enter the Sirens' dungeon once more..."));
				Msg(L("I wish they would stop bothering people<br/>once and for all..."));
			}
		}
		else
		{
			switch (reaction)
			{
				case GiftReaction.Love:
					Msg(L("Wow, for me...?<br/>I'm just a Spirit..."));
					break;

				default:
					RndMsg(
							L("This gift...won't look so good on a spirit like me ...but...<br/>Thank you..."),
							L("A gift... Why, thank you..."),
							L("I still appreciate it...")
						);
					break;
			}
		}
	}
}
