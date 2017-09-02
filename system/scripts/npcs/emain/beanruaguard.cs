//--- Aura Script -----------------------------------------------------------
// Bean Rua Guards
//--- Description -----------------------------------------------------------
// Script for the Doormen outside Bean Rua, used to purchase entrance passes.
//--- Notes -----------------------------------------------------------------
// Currently the doormen warp to the GM map (region 22), but they are
// hidden from the client rather than warped on official.
//---------------------------------------------------------------------------

public class BeanRuaGuard1Script : NpcScript
{
	private const int BeanRuaBrooch = 73110;
	private const int TicketToBeanRua = 73111;

	public override void Load()
	{
		SetRace(10002);
		SetName("_beanruaguard01");
		SetBody(height: 1.26f, weight: 1.09f, upper: 1.26f);
		SetFace(skinColor: 15, eyeType: 9, eyeColor: 167, mouthType: 0);
		SetLocation(52, 47115, 47272, 183);

		EquipItem(Pocket.Face, 4900, 0x009BB287, 0x00DDE998, 0x00F9A35D);
		EquipItem(Pocket.Hair, 4030, 0x00612314, 0x00612314, 0x00612314);
		EquipItem(Pocket.Armor, 15014, 0x00000000, 0x00000000, 0x00715B44);
		EquipItem(Pocket.Glove, 16006, 0x002E231F, 0x00FFFFFF, 0x00FFFFFF);
		EquipItem(Pocket.Shoe, 17010, 0x00000000, 0x00FFFFFF, 0x00FFFFFF);

		AddPhrase("Alright, go in.");
		AddPhrase("It's open!!");
		AddPhrase("Did your guild reserve a spot?");
		AddPhrase("Hurry up and go in!");
		AddPhrase("Are you looking for Bean Rua? It's right here.");
		AddPhrase("Welcome welcome!");
		AddPhrase("Okay, you're in!");
		AddPhrase("Hey hey, beautiful, right here!");
		AddPhrase("This is Bean Rua. We're now accepting people.");
		AddPhrase("Please come in.");
		AddPhrase("Don't hesitate, just walk in!");
		AddPhrase("Right this way. There's a seat available.");
		AddPhrase("We're open now. Line up!");
	}

	protected virtual Location NormalLocation { get { return new Location(52, 47115, 47272); } }
	protected virtual Location TempLocation { get { return new Location(22, 6500, 4800); } }

	[On("ErinnTimeTick")]
	public void OnErinnTimeTick(ErinnTime time)
	{
		if (ErinnHour(16, 6))
		{
			if (NPC.RegionId != 52)
				NPC.Warp(NormalLocation);
		}
		else if (NPC.RegionId != 22)
		{
			NPC.Warp(TempLocation);
		}
	}

	protected override async Task Talk()
	{
		var msg = "";
		if (ErinnHour(16, 18))
		{
			switch (Random(7))
			{
				case 0: msg = "Okay, you can start filing in later.<br/>Hold on for now."; break;
				case 1: msg = "Bean Rua opens late in the afternoon..."; break;
				case 2: msg = "Now now. Maybe later..."; break;
				case 3: msg = "You're here too early. Please come back in a bit."; break;
				case 4: msg = "You can't go in right now...<br/>stop being so stubborn."; break;
				case 5: msg = "I was told you'd come, but... you're too early."; break;
				case 6: msg = "Can you please wait for a bit?"; break;
			}
		}
		else if (ErinnHour(18, 6))
		{
			switch (Random(9))
			{
				case 0: msg = "Would you prefer talking to me, instead of just walking on in..."; break;
				case 1: msg = "Are you looking for your party...?"; break;
				case 2: msg = "You're staring at me...<p/>...Do I have something on my face?"; break;
				case 3: msg = "Hello there. Looking good."; break;
				case 4: msg = "Welcome to Bean Rua."; break;
				case 5: msg = "The door's open. You can walk in, you know..."; break;
				case 6: msg = "Do you have anything for me?"; break;
				case 7: msg = "Welcome! Welcome! Welcome to Bean Rua, where the beautiful redheads rule."; break;
				case 8: msg = "Did you make a reservation?"; break;
			}
		}
		else
		{
			// Unofficial, Doormen should be hidden during the day.
			End("Bean Rua is currently closed,<br/>please come back during the night.");
		}

		Msg(msg, Button("Buy Ticket", "@ticket"), Button("End Conversation", "@exit"));
		if (await Select() == "@ticket")
		{
			var price = 1000;

			if (Player.HasItem(BeanRuaBrooch))
			{
				msg = "You must be a member of our club. We have a member's discount for the ticket.<br/>The ticket is good for one-time use.<br/>The ticket will cost you 500 Gold. Would you like to buy one?";
				price = 500;
			}
			else
			{
				msg = "To enter Club Bean Rua, you must first buy a ticket.<br/>The ticket is only good for a one-time use and will expire after a certain amount of time.<br/>The ticket will cost you 1,000 Gold. Would you like to buy one now?";
				price = 1000;
			}

			Msg(msg, Button("Buy", "@buy"), Button("Cancel", "@exit"));
			if (await Select() == "@buy")
			{
				if (Player.Inventory.Gold < price)
					End("Hmmm, you're short on cash.");

				Player.Inventory.Gold -= price;
				Player.GiveItem(TicketToBeanRua);
				Send.Notice(Player, L("Received Ticket to Bean Rua from Bean Rua Doorman."));
				End("Thank you! Have a great time!");			
			}
			else
			{
				End("Please come back later!");
			}
		}
		else
		{
			End("Have a good time!");
		}
	}

	protected override async Task Gift(Item item, GiftReaction reaction)
	{
		switch (reaction)
		{
			default:
				Msg(L("Thank you."));
				break;
		}
	}
}

public class BeanRuaGuard2Script : BeanRuaGuard1Script
{
	public override void Load() { base.Load(); SetName("_beanruaguard02"); SetFace(skinColor: 15, eyeType: 4, eyeColor: 32, mouthType: 0); SetLocation(52, 48270, 48122, 155); EquipItem(Pocket.Face, 4900, 0x00F8E24C, 0x00707072, 0x006C706C); EquipItem(Pocket.Hair, 4038, 0x00AA7840, 0x00AA7840, 0x00AA7840); }
	protected override Location NormalLocation { get { return new Location(52, 48270, 48122); } }
}
