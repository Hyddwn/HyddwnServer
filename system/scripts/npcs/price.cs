//--- Aura Script -----------------------------------------------------------
// Price
//--- Description -----------------------------------------------------------
// Wandering merchant, moves to a new location every day at midnight.
// Some items in his shop are being randomized at the same time.
//---------------------------------------------------------------------------

public class PriceNpcScript : NpcScript
{
	public override void Load()
	{
		SetRace(10002);
		SetName("_price");
		SetBody(height: 1.29f, weight: 1.12f, upper: 1.32f, lower: 1.06f);
		SetFace(skinColor: 19, eyeType: 4, eyeColor: 90, mouthType: 13);
		SetStand("human/male/anim/male_natural_stand_npc_Piaras");
		SetLocation(22, 6087, 5377, 80);
		SetGiftWeights(beauty: 0, individuality: 0, luxury: 1, toughness: -1, utility: 0, rarity: 2, meaning: 1, adult: 0, maniac: 2, anime: 0, sexy: 0);

		EquipItem(Pocket.Face, 4901, 0x00FEE12B, 0x00EAF0B0, 0x00F69929);
		EquipItem(Pocket.Hair, 4955, 0x00C1C298, 0x00C1C298, 0x00C1C298);
		EquipItem(Pocket.Armor, 15052, 0x00986C4B, 0x00181E13, 0x00C2B39E);
		EquipItem(Pocket.Shoe, 17044, 0x0074562E, 0x00FFFFFF, 0x00FFFFFF);
		EquipItem(Pocket.Head, 18024, 0x004E7271, 0x0024312F, 0x00FFFFFF);

		AddPhrase(L("..."));
		AddPhrase(L("...Maybe it's time for me to move to another town..."));
		AddPhrase(L("...The sales aren't like it used to be..."));
		AddPhrase(L("Hmm, Hmm..."));
		AddPhrase(L("I can already that customer is probably stingy. Haha."));
		AddPhrase(L("I can't even leave because of all those people who said they'll bring me the money..."));
		AddPhrase(L("I recognize that face..."));
		AddPhrase(L("If you want to see new and original items, come here!"));
		AddPhrase(L("Let's see..."));
		AddPhrase(L("Now, now, get in line! In line!"));
		AddPhrase(L("So many beautiful ladies here. Haha."));
		AddPhrase(L("Today's sales stink."));
		AddPhrase(L("What can I sell in the next town..."));
		AddPhrase(L("What... no merchants allowed... psh."));
		AddPhrase(L("You can't find this everyday!"));
		AddPhrase(L("You don't need other merchants besides me."));
		AddPhrase(L("You're not even going to buy..."));

		OnErinnMidnightTick(ErinnTime.Now);
	}

	private Location[] locations = new[]
	{
		new Location(1, 17360, 33370),
		new Location(16, 24200, 63540),
		new Location(14, 48970, 37300),
		new Location(31, 15722, 8155),
		new Location(53, 95407, 110140),
		new Location(52, 34887, 41805),
		new Location(56, 8154, 9973),
		new Location(52, 44866, 24701),
		new Location(53, 95407, 110140),
		new Location(30, 43940, 48460),
		new Location(31, 13840, 14746),
		new Location(14, 42600, 37900),
		new Location(16, 24200, 63540),
	};

	[On("ErinnMidnightTick")]
	private void OnErinnMidnightTick(ErinnTime now)
	{
		// Stay on GM Island if feature is not enabled.
		if (!IsEnabled("Price"))
			return;

		var timestamp = (now.Year * 7 * 40 + now.Month * 40 + now.Day);
		var locationIdx = (timestamp % locations.Length);
		var location = locations[locationIdx];

		NPC.Warp(location);
	}

	protected override async Task Talk()
	{
		SetBgm("NPC_Price.mp3");

		await Intro(L("You can spot this man from a mile away with his bulging muscles, thick arms and neck, as well as his protruding chest.<br/>His clothes are a bit shaggy and his headband that is tightly holding up his hair looks to be slightly soaked in sweat.<br/>Once you get close to him, you can't help but notice his big smile that covers his face,<br/>and his mustache that twitches each time he speaks."));

		Msg(L("Haha. What do you need from a wandering merchant like me?"), Button("Start a Conversation", "@talk"), Button("Shop", "@shop"));

		switch (await Select())
		{
			case "@talk":
				Greet();
				Msg(Hide.Name, GetMoodString(), FavorExpression());

				await Conversation();
				break;

			case "@shop":
				Msg(L("Is there anything you see you like?<br/>Pick one!"));
				OpenShop("PriceShop");
				return;
		}

		End(L("Goodbye Price."));
	}

	private void Greet()
	{
		if (Memory <= 0)
		{
			Msg(FavorExpression(), L("Didn't we meet not too long ago...?<br/>You know...business is business<br/>It's hard to remember each person... Haha"));
		}
		else if (Memory == 1)
		{
			Msg(FavorExpression(), L("(Dialog missing: Memory == 1)"));
		}
		else if (Memory == 2)
		{
			Msg(FavorExpression(), L("(Dialog missing: Memory == 2)"));
		}
		else if (Memory <= 6)
		{
			Msg(FavorExpression(), L("(Dialog missing: Memory <= 6)"));
		}
		else
		{
			Msg(FavorExpression(), L("(Dialog missing: Memory > 6)"));
		}

		UpdateRelationAfterGreet();
	}

	protected override async Task Keywords(string keyword)
	{
		switch (keyword)
		{
			case "personal_info":
				Msg(FavorExpression(), L("I travel all through Erinn and sell goods.<br/>It's my job to find and buy items for cheap<br/>that sell for an expensive price in other regions.<br/>If you're interested in what I do, let me know."));
				ModifyRelation(Random(2), 0, Random(3));
				break;

			case "rumor":
				Msg(FavorExpression(), L("Hmm...Since I travel all over the place<br/>I hear a quite a lot of rumors.<br/>Instead of asking me what I know,<br/>asking me what I don't know might be more appropriate. Haha."));
				ModifyRelation(Random(2), 0, Random(3));
				break;

			default:
				RndFavorMsg(
					L("Ah, I see what you're saying.<br/>But there's nothing I can say about that."),
					L("Hey, hey! How do you plan on surviving<br/>this rough life caring about things like that, huh?"),
					L("There was someone who asked me about that<br/>so this is what I said.<br/>That I don't know anything about that. Haha.")
				);
				ModifyRelation(0, 0, Random(3));
				break;
		}
	}
}

public class PriceShop : NpcShopScript
{
	public override void Setup()
	{
		Add(L("General Goods"), 61000);    // Score Scroll
		Add(L("General Goods"), 40004);    // Lute
		Add(L("General Goods"), 2001);     // Gold Pouch
		Add(L("General Goods"), 63001, 1); // Wings of Goddess 1x
		Add(L("General Goods"), 63001, 5); // Wings of Goddess 5x
		Add(L("General Goods"), 62012, 1); // Elemental Remover 1x
		Add(L("General Goods"), 62012, 5); // Elemental Remover 5x
		Add(L("General Goods"), 2006);     // Big Gold Pouch
		Add(L("General Goods"), 40017);    // Mandolin

		Add(L("Potions"), 51001, 5);  // HP 10 Potion 5x
		Add(L("Potions"), 51002, 5);  // HP 30 Potion 5x
		Add(L("Potions"), 51002, 10); // HP 30 Potion 10x
		Add(L("Potions"), 51011, 5);  // Stamina 10 Potion 5x
		Add(L("Potions"), 51012, 5);  // Stamina 30 Potion 5x
		Add(L("Potions"), 51012, 10); // Stamina 30 Potion 10x

		if (IsEnabled("Housing"))
			Add(L("General Goods"), 1124); // An Easy Guide to Taking Up Residence in a Home

		OnErinnMidnightTick(ErinnTime.Now);
	}

	protected override void OnErinnMidnightTick(ErinnTime time)
	{
		// Run base (color randomization)
		base.OnErinnMidnightTick(time);

		ClearTab(L("Interesting Items"));

		Add(L("Interesting Items"), 18541); // Bald Wig

		switch (Random(3))
		{
			case 0: Add(L("Interesting Items"), 16513); break; // Scissors Glove
			case 1: Add(L("Interesting Items"), 16514); break; // Rock Glove
			case 2: Add(L("Interesting Items"), 16515); break; // Paper Glove
		}

		switch (time.Month)
		{
			case ErinnMonth.Imbolic: // Sunday
				Add(L("Interesting Items"), 40059); // 2 Sign
				Add(L("Interesting Items"), 40060); // 3 Sign
				Add(L("Interesting Items"), 40063); // 6 Sign
				break;
			case ErinnMonth.AlbanEiler: // Monday
				Add(L("Interesting Items"), 40064); // 7 Sign
				Add(L("Interesting Items"), 40065); // 8 Sign
				Add(L("Interesting Items"), 40066); // 9 Sign
				break;
			case ErinnMonth.Baltane: // Tuesday
				Add(L("Interesting Items"), 40060); // 3 Sign
				Add(L("Interesting Items"), 40063); // 6 Sign
				Add(L("Interesting Items"), 40064); // 7 Sign
				break;
			case ErinnMonth.AlbanHeruin: // Wednesday
				Add(L("Interesting Items"), 40059); // 2 Sign
				Add(L("Interesting Items"), 40065); // 8 Sign
				Add(L("Interesting Items"), 40066); // 9 Sign
				break;
			case ErinnMonth.Lughnasadh: // Thursday
				Add(L("Interesting Items"), 40063); // 6 Sign
				Add(L("Interesting Items"), 40064); // 7 Sign
				Add(L("Interesting Items"), 40065); // 8 Sign
				break;
			case ErinnMonth.AlbanElved: // Friday
				Add(L("Interesting Items"), 40059); // 2 Sign
				Add(L("Interesting Items"), 40060); // 3 Sign
				Add(L("Interesting Items"), 40066); // 9 Sign
				break;
			case ErinnMonth.Samhain: // Saturday
				Add(L("Interesting Items"), 40067); // 10 Sign
				Add(L("Interesting Items"), 40061); // 4 Sign
				Add(L("Interesting Items"), 40062); // 5 Sign
				break;
		}
	}
}
