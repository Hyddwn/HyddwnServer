//--- Aura Script -----------------------------------------------------------
// Valentine's Event 
//--- Description -----------------------------------------------------------
// 2017 Valentine's Dungeon Event coded by Thunderbro
// Loosely based on the official Valentine's Day Dungeon event
// http://wiki.mabi.world/view/Valentine%27s_Day_Cake_Event_(2013)
//---------------------------------------------------------------------------

public class ValentineEventScript : GameEventScript
{
	public override void Load()
	{
		SetId("aura_valentines_event");
		SetName(L("Valentine's Day Lover's Chocolate Cake"));
	}

	public override void AfterLoad()
	{
		//ScheduleEvent(DateTime.Parse("2017-02-01 00:00"), DateTime.Parse("2017-03-01 00:00"));
	}

	protected override void OnStart()
	{
		AddEventItemToShop("CaitinShop", itemId: 50706); // Cacao
		AddEventItemToShop("CaitinShop", itemId: 50119); // Flour Dough
		AddEventItemToShop("GlenisShop", itemId: 50706); // Cacao
		AddEventItemToShop("GlenisShop", itemId: 50119); // Flour Dough
	}
}

public class AbbeyEventNpcScript : NpcScript
{
	private const string LastReceivedVar = "ValentinesEventLastGift2017";

	public override void Load()
	{
		SetRace(10001);
		SetName("_<mini>NPC</mini> Abbey");
		SetBody(height: 0.7f, weight: 0.7f, upper: 0.7f, lower: 0.7f);
		SetFace(skinColor: 16, eyeType: 284, eyeColor: 238, mouthType: 20);

		if (IsEventActive("aura_valentines_event"))
			SetLocation(14, 41072, 45502, 59);

		EquipItem(Pocket.Face, 3938, 0x00366969, 0x0094B330, 0x00737474);
		EquipItem(Pocket.Hair, 8166, 0x100000F1, 0x100000F1, 0x100000F1);
		EquipItem(Pocket.Armor, 80736, 0x00FFDDF5, 0x00FFFFFF, 0x00FF305D);
		EquipItem(Pocket.Shoe, 17429, 0x00FF305D, 0x00FFFFFF, 0x00FFDDF5);
		EquipItem(Pocket.Head, 28610, 0x00FF305D, 0x00FFFFFF, 0x00FFDDF5);

		AddPhrase("What is that feeling in my chest?");
		AddPhrase("What should I do?");
		AddPhrase("Nnnnnggg");
		AddPhrase("Love is in the air.");
	}

	protected override async Task Talk()
	{
		Msg(SetDefaultName("Abbey"));
		SetBgm("Boss_candysheep.mp3");

		await Intro(L("A bashful young lady stands before you.<br/>Her elegant pink curly twin-tails really make her stand out.<br/>She greets you in a cheerful, bubbly voice, but it seems something is on her mind."));

		if ((!Player.IsMale) && (Player.QuestCompleted(400020)))
		{
			var now = DateTime.Now;
			var nowDay = now.Day;
			var lastReceivedDay = GetLastReceivedDay(now);

			if (QuestActive(400021, "talk_abbey")) // Check to see if the quest is active
			{
				SaveLastReceivedDay(now);
				Hook("valentines_daily"); // Trigger hook
			}

			else if (QuestActive(400022, "talk_abbey")) // Check to see if the quest is active
			{
				SaveLastReceivedDay(now);
				Hook("valentines_daily"); // Trigger hook
			}

			else if (Player.QuestActive(400021) || Player.QuestActive(400022))
			{
				Msg("You haven't delivered the cake yet?");
				Msg("Cacao chocolate cake can be made with sugar,<br/>flour dough, and cacao by mixing with your cooking skill.<br/>All the ingredients are available at any grocery store.");
			}
			else if (!Player.QuestCompleted(400021) && nowDay != lastReceivedDay)
			{
				StartQuest(400021);
				Msg("Hello, what did you need?");
				Msg(Hide.Name, "You explain that you want to help Abbey express her feelings to Conor.");
				Msg("W-w-what?<br/>...Well I suppose it is true that I have a crush on him...<br/>But what will I do if he rejects me?");
				Msg(Hide.Name, "You explain that Conor seems to have a crush on Abbey as well.");
				Msg("Is that so?<br/>Well... I suppose I could try something.");
				Msg("How about we make a cacao chocolate cake?<br/>Yes of course we...<br/>I'm pretty terrible at cooking.");
				Msg(Hide.Name, "You decide to agree to make the cake for her.");
				Msg("Okay, well its pretty simple to make.<br/>Cacao chocolate cake can be made with sugar,<br/>flour dough, and cacao by mixing with your cooking skill.<br/>All the ingredients are available at any grocery store.");
				Msg("Do you think you could deliver that when you're done<br/>and tell him it was from me?");
				Msg("I feel like I wouldn't be able to talk straight<br/>if I delivered it myself.");
				Msg("<username/>, thank you so much for the help.");
			}
			else if (nowDay != lastReceivedDay)
			{
				StartQuest(400022);
				Msg("He seemed to like the cake yesterday...");
				Msg("Lets try making him another one today!");
				Msg("Go ahead and deliver the cake to him when its finished!");
			}
			else
			{
				Msg("It looks like you already made a cake today.");
				Msg("Come back again tomorrow and we can make another one.");
			}
		}
		else
			Msg("Hello! I'm Abbey!");
	}

	private int GetLastReceivedDay(DateTime now)
	{
		var last = Player.Vars.Perm[LastReceivedVar] as DateTime?;
		if (last == null)
			return 0;

		return ((DateTime)last).Day;
	}

	private void SaveLastReceivedDay(DateTime now)
	{
		Player.Vars.Perm[LastReceivedVar] = now;
	}
}

public class ConorEventNpcScript : NpcScript
{
	private const string LastReceivedVar = "ValentinesEventLastGift2017";

	public override void Load()
	{
		SetRace(10002);
		SetName("_<mini>NPC</mini> Conor");
		SetBody(height: 1.1f);
		SetFace(skinColor: 16, eyeType: 220, eyeColor: 45, mouthType: 71);

		if (IsEventActive("aura_valentines_event"))
			SetLocation(14, 39279, 40546, 195);

		EquipItem(Pocket.Face, 4978, 0x00366969, 0x0094B330, 0x00737474);
		EquipItem(Pocket.Hair, 8110, 0x1000004C, 0x100000F1, 0x100000F1);
		EquipItem(Pocket.Armor, 80735, 0x00702B0C, 0x00FFFFFF, 0x00FFDDF5);
		EquipItem(Pocket.Shoe, 17428, 0x00FFDDF5, 0x00FFFFFF, 0x00702B0C);

		AddPhrase("What are these complex emotions I'm feeling?");
		AddPhrase("Hmm... What should I do?");
		AddPhrase("Why can't I confess?");
		AddPhrase("Hmm... today feels like a good day.");
	}

	protected override async Task Talk()
	{
		Msg(SetDefaultName("Conor"));
		SetBgm("NPC_Huw.mp3");

		await Intro(L("A handsome young man stands before you.<br/>He has shaggy dark brown hair and a slick outfit.<br/>He greets you in a friendly tone, but it seems something is on his mind."));

		if ((Player.IsMale) && (Player.QuestCompleted(400020)))
		{
			var now = DateTime.Now;
			var nowDay = now.Day;
			var lastReceivedDay = GetLastReceivedDay(now);

			if (QuestActive(400023, "talk_conor")) // Check to see if the quest is active
			{
				SaveLastReceivedDay(now);
				Hook("valentines_daily"); // Trigger hook
			}

			else if (QuestActive(400024, "talk_conor")) // Check to see if the quest is active
			{
				SaveLastReceivedDay(now);
				Hook("valentines_daily"); // Trigger hook
			}

			else if (Player.QuestActive(400023) || Player.QuestActive(400024))
			{
				Msg("You haven't delivered the cake yet?");
				Msg("Cacao chocolate cake can be made with sugar,<br/>flour dough, and cacao by mixing with your cooking skill.<br/>All the ingredients are available at any grocery store.");
			}
			else if (!Player.QuestCompleted(400023) && nowDay != lastReceivedDay)
			{
				StartQuest(400023);
				Msg("Hello, did you need something?");
				Msg(Hide.Name, "You explain that you want to help Conor express her feelings to Abbey.");
				Msg("Oh...<br/>Well I suppose it is true that I have a crush on her...<br/>I don't have the courage to tell her how I feel though...");
				Msg(Hide.Name, "You explain that Abbey seems to have a crush on Conor as well.");
				Msg("Really?<br/>I can't say I was expecting that...<br/>Hmm... What should I do then...");
				Msg("How about we make a cacao chocolate cake?<br/>Yes I said we...<br/>I'm a disaster in the kitchen.");
				Msg(Hide.Name, "You decide to agree to make the cake for him.");
				Msg("Okay, well it doesn't seem too hard to make.<br/>Cacao chocolate cake can be made with sugar,<br/>flour dough, and cacao by mixing with your cooking skill.<br/>All the ingredients are available at any grocery store.");
				Msg("Do you think you could deliver that when you're done<br/>and tell her it was from me?");
				Msg("I'm not sure if I could muster up the courage<br/>to deliver it myself...");
				Msg("<username/>, thanks a bunch for the help.");
			}
			else if (nowDay != lastReceivedDay)
			{
				StartQuest(400024);
				Msg("She seemed to have enjoyed the cake yesterday...");
				Msg("Lets try making her another one today!");
				Msg("Go ahead and deliver the cake to her when its finished!");
			}
			else
			{
				Msg("It looks like you already made a cake today.");
				Msg("Come back again tomorrow and we can make another one.");
			}
		}
		else
			Msg("Hello, I'm Conor.");
	}

	private int GetLastReceivedDay(DateTime now)
	{
		var last = Player.Vars.Perm[LastReceivedVar] as DateTime?;
		if (last == null)
			return 0;

		return ((DateTime)last).Day;
	}

	private void SaveLastReceivedDay(DateTime now)
	{
		Player.Vars.Perm[LastReceivedVar] = now;
	}
}

public class ValentineIntroQuestScript : QuestScript
{
	public override void Load()
	{
		SetId(400020);
		SetName(L("Pair of Shy Lovers"));
		SetDescription(L("Ahh... Love is in the air this month! There were a couple of lovestruck teens that seemed to be attracted to each other, but they don't know how to convey their feelings. Mind giving them a hand? Come see me for details if you want to help them out. - Eavan -"));
		SetCancelable(true);

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Event);

		SetReceive(Receive.Automatically);
		AddPrerequisite(EventActive("aura_valentines_event"));

		AddObjective("talk", "Talk to Eavan at the Dunbarton town office.", 14, 40024, 41041, Talk("eavan"));

		AddReward(Exp(1000));

		AddHook("_eavan", "after_intro", AfterIntro);
	}
	public async Task<HookResult> AfterIntro(NpcScript npc, params object[] args)
	{
		if (npc.QuestActive(this.Id, "talk"))
		{
			npc.FinishQuest(this.Id, "talk");

			npc.Msg("Hi, <username/>, thank you for stopping by.");
			npc.Msg("So lets get down to business.<br/>I've noticed a couple of teens in town with crushes on each other.");
			npc.Msg("They don't seem to realize that the both have a crush on each other.");
			npc.Msg("Both of them seem like they are having trouble expressing their feelings to each other though...<br/>It pains me to see that, do you think you can lend them a helping hand?");
			if (npc.Player.IsMale)
			{
				npc.Msg("Anyways, if you so choose,<br/>I think it would be best for you to help out Conor.");
				npc.Msg("Conor is actually right over there by the housing board.");
				npc.Msg("Go ahead and talk to him,<br/>I'm sure you can find a way to help him out.");
			}
			else
			{
				npc.Msg("Anyways, if you so choose,<br/>I think it would be best for you to help out Abbey.");
				npc.Msg("Abbey was standing by the northern gate,<br/>you should be able to find her there.");
				npc.Msg("Go ahead and talk to her,<br/>I'm sure you can find a way to help her out.");
			}
			return HookResult.Break;
		}

		return HookResult.Continue;
	}
}

public class ValentinesAbbeyCakeScript : QuestScript
{
	public override void Load()
	{
		SetId(400021);
		SetName("Abbey's Chocolate Cake");
		SetDescription("I'm having a little trouble... Mind helping me out with something? I'm at Dunbarton's Northern Gate. - Abbey -");
		SetCancelable(true);

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Event);

		AddObjective("talk_conor", "Deliver cake to Conor", 14, 39279, 40546, Talk("<mini>NPC</mini> Conor"));
		AddObjective("talk_abbey", "Return to Abbey", 14, 41072, 45502, Talk("<mini>NPC</mini> Abbey"));

		AddReward(Exp(15000));
		AddReward(Item(75496)); // Valentine's Day Dungeon Pass
		AddReward(Item(75498)); // Courtship Gesture Card
		AddReward(Item(91514)); // Valentine's Gift Box (female)

		AddHook("_<mini>NPC</mini> Conor", "after_intro", TalkConor);
		AddHook("_<mini>NPC</mini> Abbey", "valentines_daily", TalkAbbey);
	}

	public async Task<HookResult> TalkConor(NpcScript npc, params object[] args)
	{
		if (npc.QuestActive(this.Id, "talk_conor") && npc.HasItem(50707))
		{
			npc.FinishQuest(this.Id, "talk_conor");

			npc.RemoveItem(50707); // Cacao Chocolate Cake
			npc.Msg(Hide.Name, "You present the cake to Conor,<br/>You explain that its from Abbey.");
			npc.Msg("Abbey?!<br/>Really? I never knew she felt that way about me...");
			npc.Msg("I'm sorry... I'm really happy.<br/>Thank you for the cake.");

			return HookResult.Break;
		}
		return HookResult.Continue;
	}

	public async Task<HookResult> TalkAbbey(NpcScript npc, params object[] args)
	{
		if (npc.QuestActive(this.Id, "talk_abbey"))
		{
			npc.FinishQuest(this.Id, "talk_abbey");

			npc.Msg(Hide.Name, "You explain Conor's reaction to the cake to Abbey.");
			npc.Msg("Oh?<br/>I guess he really does feel that way about me.");
			npc.Msg("I'm really happy to hear that.<br/>Do you think you could help me make another cake tomorrow?");
			npc.Msg("Oh right, its not much, but take these for your trouble.");
			npc.Msg("Thanks again for the help.");

			return HookResult.Break;
		}
		return HookResult.Continue;
	}
}

public class ValentinesAbbeyCake2Script : QuestScript
{
	public override void Load()
	{
		SetId(400022);
		SetName("Abbey's Chocolate Cake");
		SetDescription("Mind helping me make another cake? - Abbey -");
		SetCancelable(true);

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Event);

		AddObjective("talk_conor", "Deliver cake to Conor", 14, 39279, 40546, Talk("<mini>NPC</mini> Conor"));
		AddObjective("talk_abbey", "Return to Abbey", 14, 41072, 45502, Talk("<mini>NPC</mini> Abbey"));

		AddReward(Exp(10000));
		AddReward(Item(75496)); // Valentine's Day Dungeon Pass
		AddReward(Item(91514)); // Valentine's Gift Box (female)

		AddHook("_<mini>NPC</mini> Conor", "after_intro", TalkConor);
		AddHook("_<mini>NPC</mini> Abbey", "valentines_daily", TalkAbbey);
	}

	public async Task<HookResult> TalkConor(NpcScript npc, params object[] args)
	{
		if (npc.QuestActive(this.Id, "talk_conor") && npc.HasItem(50707))
		{
			npc.FinishQuest(this.Id, "talk_conor");

			npc.RemoveItem(50707); // Cacao Chocolate Cake
			npc.Msg(Hide.Name, "You present the cake to Conor,<br/>You explain that its from Abbey.");
			npc.Msg("Another cake?<br/>Perhaps I should return the favor one day...");
			npc.Msg("Anyways, thank you for the cake.");

			return HookResult.Break;
		}
		return HookResult.Continue;
	}

	public async Task<HookResult> TalkAbbey(NpcScript npc, params object[] args)
	{
		if (npc.QuestActive(this.Id, "talk_abbey"))
		{
			npc.FinishQuest(this.Id, "talk_abbey");

			npc.Msg(Hide.Name, "You tell Abbey that you delivered the cake.");
			npc.Msg("It seemed that he liked it right?<br/>He even said that he should return the favor?");
			npc.Msg("I'm really happy to hear that.<br/>Do you think you could help me make another cake tomorrow?");
			npc.Msg("Oh right, its not much, but take these for your trouble.");
			npc.Msg("Thanks again for the help.");

			return HookResult.Break;
		}
		return HookResult.Continue;
	}
}

public class ValentinesConorCakeScript : QuestScript
{
	public override void Load()
	{
		SetId(400023);
		SetName("Conor's Chocolate Cake");
		SetDescription("I'm having a little trouble... Mind helping me out with something? I'm near Dunbarton Town Office. - Conor -");
		SetCancelable(true);

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Event);

		AddObjective("talk_abbey", "Deliver cake to Abbey", 14, 41072, 45502, Talk("<mini>NPC</mini> Abbey"));
		AddObjective("talk_conor", "Return to Conor", 14, 39279, 40546, Talk("<mini>NPC</mini> Conor"));

		AddReward(Exp(15000));
		AddReward(Item(75496)); // Valentine's Day Dungeon Pass
		AddReward(Item(75498)); // Courtship Gesture Card
		AddReward(Item(91515)); // Valentine's Gift Box (male)

		AddHook("_<mini>NPC</mini> Abbey", "after_intro", TalkAbbey);
		AddHook("_<mini>NPC</mini> Conor", "valentines_daily", TalkConor);
	}

	public async Task<HookResult> TalkAbbey(NpcScript npc, params object[] args)
	{
		if (npc.QuestActive(this.Id, "talk_abbey") && npc.HasItem(50707))
		{
			npc.FinishQuest(this.Id, "talk_abbey");

			npc.RemoveItem(50707); // Cacao Chocolate Cake
			npc.Msg(Hide.Name, "You present the cake to Abbey,<br/>You explain that its from Conor.");
			npc.Msg("Conor did...?<br/>Wow, I didn't realize he felt that way about me...");
			npc.Msg("I'm really happy to know that.<br/>Thank you for the cake.");

			return HookResult.Break;
		}
		return HookResult.Continue;
	}

	public async Task<HookResult> TalkConor(NpcScript npc, params object[] args)
	{
		if (npc.QuestActive(this.Id, "talk_conor"))
		{
			npc.FinishQuest(this.Id, "talk_conor");

			npc.Msg(Hide.Name, "You explain Abbey's reaction to the cake to Conor.");
			npc.Msg("Oh?<br/>She really does like me then...");
			npc.Msg("Well, I'm glad to hear that's the case.<br/>Do you think you could help me make another cake tomorrow?");
			npc.Msg("Oh right, its not much, but take these for your trouble.");
			npc.Msg("Thanks again for the help.");

			return HookResult.Break;
		}
		return HookResult.Continue;
	}
}

public class ValentinesConorCake2Script : QuestScript
{
	public override void Load()
	{
		SetId(400024);
		SetName("Conor's Chocolate Cake");
		SetDescription("Mind helping me make another cake? - Conor -");
		SetCancelable(true);

		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Event);

		AddObjective("talk_abbey", "Deliver cake to Abbey", 14, 41072, 45502, Talk("<mini>NPC</mini> Abbey"));
		AddObjective("talk_conor", "Return to Conor", 14, 39279, 40546, Talk("<mini>NPC</mini> Conor"));

		AddReward(Exp(10000));
		AddReward(Item(75496)); // Valentine's Day Dungeon Pass
		AddReward(Item(91515)); // Valentine's Gift Box (male)

		AddHook("_<mini>NPC</mini> Abbey", "after_intro", TalkAbbey);
		AddHook("_<mini>NPC</mini> Conor", "valentines_daily", TalkConor);
	}

	public async Task<HookResult> TalkAbbey(NpcScript npc, params object[] args)
	{
		if (npc.QuestActive(this.Id, "talk_abbey") && npc.HasItem(50707))
		{
			npc.FinishQuest(this.Id, "talk_abbey");

			npc.RemoveItem(50707); // Cacao Chocolate Cake
			npc.Msg(Hide.Name, "You present the cake to Abbey,<br/>You explain that its from Conor.");
			npc.Msg("Another cake?<br/>That's sweet of him...<br/>I feel like I should do something for him.");
			npc.Msg("Anyways, thank you for the cake.");

			return HookResult.Break;
		}
		return HookResult.Continue;
	}

	public async Task<HookResult> TalkConor(NpcScript npc, params object[] args)
	{
		if (npc.QuestActive(this.Id, "talk_conor"))
		{
			npc.FinishQuest(this.Id, "talk_conor");

			npc.Msg(Hide.Name, "You tell Conor that you delivered the cake.");
			npc.Msg("It seemed that she liked it right?<br/>She even said that she should return the favor?");
			npc.Msg("Well, I'm glad to hear that's the case.<br/>Do you think you could help me make another cake tomorrow?");
			npc.Msg("Oh right, its not much, but take these for your trouble.");
			npc.Msg("Thanks again for the help.");

			return HookResult.Break;
		}
		return HookResult.Continue;
	}
}

// Male and Female variants of the Valentine's Gift Boxes

[ItemScript(91514)]
public class FemaleValentinesBox : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		List<DropData> list;

		list = new List<DropData>();

		// Hats
		list.Add(new DropData(itemId: 18955, chance: 30)); // Heart Eyepatch
		list.Add(new DropData(itemId: 18957, chance: 30)); // Lollipop Heart Eyepatch
		list.Add(new DropData(itemId: 18392, chance: 30)); // Heart-shaped Glasses
		list.Add(new DropData(itemId: 28610, chance: 20)); // Macaroon Mistress Hat
		list.Add(new DropData(itemId: 210363, chance: 20)); // Macaroon Mistress Wig
		list.Add(new DropData(itemId: 18834, chance: 20)); // Lady Waffle Cone Bow
		list.Add(new DropData(itemId: 28725, chance: 10)); // Waffle Witch Wig
		list.Add(new DropData(itemId: 28726, chance: 10)); // Waffle Witch Wig and Hat
		list.Add(new DropData(itemId: 28727, chance: 10)); // Waffle Witch Hat

		// Clothing
		list.Add(new DropData(itemId: 80736, chance: 20)); // Macaroon Mistress Dress
		list.Add(new DropData(itemId: 15785, chance: 20)); // Lady Waffle Cone Dress
		list.Add(new DropData(itemId: 80858, chance: 10)); // Waffle Witch Dress

		// Shoes
		list.Add(new DropData(itemId: 17429, chance: 20));  // Macaroon Mistress Shoes
		list.Add(new DropData(itemId: 17373, chance: 20));  // Lady Waffle Cone Ribbon Shoes
		list.Add(new DropData(itemId: 17826, chance: 10));  // Waffle Witch Shoes

		// Gloves
		list.Add(new DropData(itemId: 16200, chance: 20));  // Lady Waffle Cone Heart Ring

		// Weapons
		list.Add(new DropData(itemId: 40723, chance: 20)); // Heart Lightning Wand
		list.Add(new DropData(itemId: 40724, chance: 20)); // Heart Fire Wand
		list.Add(new DropData(itemId: 40725, chance: 20)); // Heart Ice Wand
		list.Add(new DropData(itemId: 41141, chance: 20)); // Lady Waffle Cone Heart Clutch
		list.Add(new DropData(itemId: 41275, chance: 15)); // Heart Glow Stick (red)

		// Wings
		list.Add(new DropData(itemId: 19194, chance: 5)); // Hot Pink Heart Wings

		// Useables
		list.Add(new DropData(itemId: 45118, chance: 30, amount: 5)); // Heart Shaped Fireworks Kit

		var rnd = RandomProvider.Get();
		var item = Item.GetRandomDrop(rnd, list);

		cr.Inventory.Add(item, true);
	}
}

[ItemScript(91515)]
public class MaleValentinesBox : ItemScript
{
	public override void OnUse(Creature cr, Item i, string param)
	{
		List<DropData> list;

		list = new List<DropData>();

		// Hats
		list.Add(new DropData(itemId: 18955, chance: 30)); // Heart Eyepatch
		list.Add(new DropData(itemId: 18957, chance: 30)); // Lollipop Heart Eyepatch
		list.Add(new DropData(itemId: 18392, chance: 30)); // Heart-shaped Glasses
		list.Add(new DropData(itemId: 28609, chance: 20)); // Count Cookie Hat (M)
		list.Add(new DropData(itemId: 210361, chance: 20)); // Count Cookie Wig
		list.Add(new DropData(itemId: 18833, chance: 20)); // Lord Waffle Cone Hat
		list.Add(new DropData(itemId: 28722, chance: 10)); // Waffle Wizard Wig
		list.Add(new DropData(itemId: 28723, chance: 10)); // Waffle Wizard Wig and Hat
		list.Add(new DropData(itemId: 28724, chance: 10)); // Waffle Wizard Hat

		// Clothing
		list.Add(new DropData(itemId: 80735, chance: 20)); // Count Cookie Suit
		list.Add(new DropData(itemId: 15784, chance: 20)); // Lord Waffle Cone Suit
		list.Add(new DropData(itemId: 80857, chance: 10)); // Waffle Wizard Suit

		// Shoes
		list.Add(new DropData(itemId: 17428, chance: 20));  // Count Cookie Shoes
		list.Add(new DropData(itemId: 17372, chance: 20));  // Lord Waffle Cone Shoes
		list.Add(new DropData(itemId: 17825, chance: 10));  // Waffle Wizard Shoes

		// Gloves
		list.Add(new DropData(itemId: 16199, chance: 20));  // Lord Waffle Cone Bracelet

		// Weapons
		list.Add(new DropData(itemId: 40723, chance: 20)); // Heart Lightning Wand
		list.Add(new DropData(itemId: 40724, chance: 20)); // Heart Fire Wand
		list.Add(new DropData(itemId: 40725, chance: 20)); // Heart Ice Wand
		list.Add(new DropData(itemId: 41140, chance: 20)); // Lord Waffle Cone Heart Key
		list.Add(new DropData(itemId: 41275, chance: 15)); // Heart Glow Stick (red)

		// Wings
		list.Add(new DropData(itemId: 19194, chance: 5)); // Hot Pink Heart Wings

		// Useables
		list.Add(new DropData(itemId: 45118, chance: 30, amount: 5)); // Heart Shaped Fireworks Kit

		var rnd = RandomProvider.Get();
		var item = Item.GetRandomDrop(rnd, list);

		cr.Inventory.Add(item, true);
	}
}
