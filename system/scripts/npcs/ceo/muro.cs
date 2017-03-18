//--- Aura Script -----------------------------------------------------------
// Muro
//--- Description -----------------------------------------------------------
// Ceo Island guard. Sells basic supplies. 
// Also provides a one time only warp to the last town visited.
//---------------------------------------------------------------------------

public class MuroScript : NpcScript
{
	public override void Load()
	{
		SetRace(10105);
		SetName("_muro");
		SetBody(height: 0.3f);
		SetFace(skinColor: 133, eyeType: 3, eyeColor: 7, mouthType: 2);
		SetLocation(56, 11035, 11011, 159);
		SetGiftWeights(beauty: 1, individuality: 2, luxury: -1, toughness: 2, utility: 2, rarity: 0, meaning: -1, adult: 2, maniac: -1, anime: 2, sexy: 0);

		EquipItem(Pocket.Shoe, 17005, 0x00441A19, 0x00695C66, 0x0000BADB);
		EquipItem(Pocket.RightHand1, 40007, 0x00B0B0B0, 0x00745D2F, 0x00019586);
		EquipItem(Pocket.LeftHand1, 46001, 0x00ADADAD, 0x00746C54, 0x0000835F);

		AddPhrase("That human looks like me. I really think so.");
		AddPhrase("...Are you going to hit me?");
		AddPhrase("Humans... Passing by me.");
		AddPhrase("Humans...have...no fear, as I suspected... Not at all...");
		AddPhrase("I miss female Goblins!");
		AddPhrase("Hehe... It's a Human! A Human!");
		AddPhrase("Keep coming, Humans...Hehe...");
		AddPhrase("Make me laugh... give it your best shot...");
		AddPhrase("...Am I bothering you? Am I?");
		AddPhrase("Ah...Humans. I like watching Humans!");
		AddPhrase("I like this island. This island is good!");
		AddPhrase("Murrrrrrrrr....");
		AddPhrase("Boring. Tell me something funny.");
	}

	protected override async Task Talk()
	{
		await Intro(L("With his rough skin, menacing face, and his constant hard-breathing,<br/>he has the sure look of a Goblin.<br/>Yet, there is something different about this one.<br/>Strangely, it appears to have a sense of noble demeanor that does not match its rugged looks."));

		if (Player.Vars.Perm["ceoWarp"] == null)
			Msg("Because of the Moon Gate nearby, too many people walk by this place. Annoying.<br/>What do you want from me.... Aren't you looking for a Golem?<br/>Is that what you want to say to me?", Button("How Do I Get Out of Here?", "@warp"), Button("Challenge", "@challenge"), Button("Shop", "@shop"));
		else
			Msg("Again...come...?", Button("Start a Conversation", "@talk"), Button("Shop", "@shop"));

		switch (await Select())
		{
			case "@warp":
				Msg("Hmm.. I see... So you came here by accident...?<br/>Well, since you sound really desperate, I'll let you return...<br/>Thank me, Muro, for doing a good deed.", Button("Return", "@return"), Button("End Conversation", "@end"));
				if (await Select() == "@return")
				{
					Player.Vars.Perm["ceoWarp"] = true;
					Player.Warp(Player.LastTown);
					return;
				}
				break;

			case "@challenge":
				Msg("There's a Golem inside.<br/>Hmm... Although you are a Human, you seem to be able to handle Golems here...<br/>Good luck.", Button("Start a Conversation", "@talk"), Button("Shop", "@shop"));
				switch (await Select())
				{
					case "@talk":
						Greet();

						if (Player.IsUsingTitle(11001))
						{
							Msg("...That's a lie. No way our Goddess was rescued by a pitiful Human being like you.<br/>...like I said.");
							Msg("...Bring the Goddess you saved,<br/>in front of me,<br/>and introduce me to her.");
							Msg("...Otherwise, I won't believe you. Mumumu...");
						}
						else if (Player.IsUsingTitle(11002))
						{
							Msg("Hee... Human, a human is Erinn's Guardian.<br/>A human that claims to be the Guardian of Erinn is standing in front of me.<br/>Very, very intriguing.");
							Msg("...I want to touch you, just once.");
						}

						await StartConversation();
						break;

					case "@shop":
						Msg("Need anything...?<br/>...You must be mighty prepared...if you are to face off against a Golem.");
						OpenShop("MuroShop");
						return;
				}
				break;

			case "@talk":
				Greet();

				if (Player.IsUsingTitle(11001))
				{
					Msg("...That's a lie. No way our Goddess was rescued by a pitiful Human being like you.<br/>...like I said.");
					Msg("...Bring the Goddess you saved,<br/>in front of me,<br/>and introduce me to her.");
					Msg("...Otherwise, I won't believe you. Mumumu...");
				}
				else if (Player.IsUsingTitle(11002))
				{
					Msg("Hee... Human, a human is Erinn's Guardian.<br/>A human that claims to be the Guardian of Erinn is standing in front of me.<br/>Very, very intriguing.");
					Msg("...I want to touch you, just once.");
				}

				await StartConversation();
				break;

			case "@shop":
				Msg("Need anything...?<br/>...You must be mighty prepared...if you are to face off against a Golem.");
				OpenShop("MuroShop");
				return;
		}

		End("Thank you, <npcname/>. I'll see you later!");
	}

	private void Greet()
	{
		if (Memory <= 0)
		{
			Msg(FavorExpression(), L("First time seeing your face...<br/>...What, why that dirty look?<br/>...You don't like me?"));
		}
		else if (Memory == 1)
		{
			Msg(FavorExpression(), L("You look familiar....<br/>Have I introduced myself...?<br/>I'm a Goblin. My name is Muro. Muro. Muro. Mumumumu...."));
		}
		else if (Memory == 2)
		{
			Msg(FavorExpression(), L("Ah, you are a Human being named <username/>...<br/>I remember your name."));
		}
		else if (Memory <= 6)
		{
			Msg(FavorExpression(), L("<username/>... <username/>, the Human...<br/>what brings you to Ceo island again... Do you like it here?"));
		}
		else
		{
			Msg(FavorExpression(), L("<username/>... A Human being I see very often...<br/>...like I said."));
			Msg("....Why don't you just live here with me....?");
		}

		UpdateRelationAfterGreet();
	}

	public override async Task Conversation()
	{
		while (true)
		{
			this.ShowKeywords();

			Msg("What, you want to talk to me?<br/>...You don't speak Goblin language, so I, Muro, will speak in Human language.<br/>....I'll let you speak first.", Button("End Conversation", "@end"), Button("Ceo Island", "@reply1"), Button("A Talking Goblin", "@reply2"));

			var keyword = await Select();
			switch (keyword)
			{
				case "@reply1":
					Msg("Questions about this island...?<br/>Many people come here for rare flowers and herbs...");
					Msg("...But this is no island of flowers and herbs; this is an island of Golems.<br/>Do you know how dangerous Golems are...?<br/>You need focus on the battle if you want to face a Golem...");
					Msg("I don't think it's worth risking your life<br/>for stupid flowers and herbs,<br/>but I guess that's what's good about Humans...");
					break;

				case "@reply2":
					Msg("Surprised to see a talking Goblin...?<br/>There are plenty of them here that speak Human languages.<br/>My father is the only Human who understands Goblins. We learned from him.<br/>My father roams around all over Erinn selling the rarest items.");
					Msg("I want to do that...<br/>I want to be a Goblin merchant and travel throughout Erinn...");
					break;

				case "@end":
					return;

				case "rumor":
					Msg("Many Humans come here for Golems.<br/>That is why Muro built a Shop here.");
					Msg("...There are many clueless Humans who want to face the Golem,<br/>so if I sell items that are important for facing Golem,");
					Msg("Muro will be a great merchant very soon!");
					ModifyRelation(Random(2), 0, Random(3));
					break;

				case "g3_DarkKnight":
					Msg("Dark Knights, they are the Knights of Darkness.<br/>They will bring glory unto Fomors.");
					break;

				default:
					await Hook("before_keywords", keyword);
					await this.Keywords(keyword);
					break;
			}
		}
	}

	protected override async Task Keywords(string kw)
	{
		if (Favor >= 10 && Stress <= 10)
		{
			Msg("You are a Human being with a lot of knowledge.<br/>...I think I can learn a lot from you if we could talk more...<br/>...Like I said...");
		}
		else
		{
			RndFavorMsg(
				"I don't know... I don't...",
				"Do you like asking me that kind of a question...?",
				"Muro has no idea what that story is about.",
				"Humans are very difficult to understand...."
			);
		}
		ModifyRelation(0, 0, Random(3));
	}

	protected override async Task Gift(Item item, GiftReaction reaction)
	{
		switch (reaction)
		{
			case GiftReaction.Love:
				Msg(L("Oh... A lovely gift! A lovely gift!<br/>You are only a Human being, but I am very touched. I am impressed.<br/>Muro is mucho happy...<br/>...Like I said."));
				break;

			default:
				Msg(L("...I'm glad to know there is at least one Human being out there that has manners.<br/>...like I said...<br/>Your gift, very appreciated. ."));
				break;
		}
	}
}

public class MuroShop : NpcShopScript
{
	public override void Setup()
	{
		Add("Potions", 51001, 1);  // HP 10 Potion x1
		Add("Potions", 51002, 1);  // HP 30 Potion x1
		Add("Potions", 51002, 10); // HP 30 Potion x10
		Add("Potions", 51002, 20); // HP 30 Potion x20
		Add("Potions", 51006, 1);  // MP 10 Potion x1
		Add("Potions", 51007, 1);  // MP 30 Potion x1
		Add("Potions", 51007, 10); // MP 30 Potion x10
		Add("Potions", 51007, 20); // MP 30 Potion x20
		Add("Potions", 51011, 1);  // Stamina 10 Potion x1
		Add("Potions", 51012, 1);  // Stamina 30 Potion x1
		Add("Potions", 51012, 10); // Stamina 30 Potion x10
		Add("Potions", 51012, 20); // Stamina 30 Potion x20

		Add("First Aid Kits", 60005, 10); // Bandage x10
		Add("First Aid Kits", 60005, 20); // Bandage x20
		Add("First Aid Kits", 63000, 10); // Phoenix Feather x10
		Add("First Aid Kits", 63000, 20); // Phoenix Feather x20
		Add("First Aid Kits", 63001, 1);  // Wings of a Goddess x1
		Add("First Aid Kits", 63001, 5);  // Wings of a Goddess x5
	}
}
