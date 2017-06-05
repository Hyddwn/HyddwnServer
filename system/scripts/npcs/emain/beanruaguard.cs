//--- Aura Script -----------------------------------------------------------
// Bean Rua Guards
//--- Description -----------------------------------------------------------
// Script for the Doormen outside Bean Rua, used to purchase entrance passes
// Currently the doormen warp to the gm map (region 22), but they are
// hidden from the client rather than warped on official
//---------------------------------------------------------------------------

public class BeanRuaGuard1Script : NpcScript
{
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

	// Officially, the Bean Rua Guards are hidden instead of warped
	[On("ErinnTimeTick")]
	public virtual void OnErinnTimeTick(ErinnTime time)
	{
		if (ErinnHour(16, 6))
		{
			if (NPC.RegionId != 52)
				NPC.Warp(52, 47115, 47272);
		}
		else if (NPC.RegionId != 22)
		{
			NPC.Warp(22, 6500, 4800);
		}
	}

	protected override async Task Talk()
	{
		if (ErinnHour(16, 18))
		{
			switch (Random(7))
			{
				case 0:
					Msg("Okay, you can start filing in later.<br/>Hold on for now.", Button("Buy Ticket", "@ticket"), Button("End Conversation", "@exit"));
					break;
				case 1:
					Msg("Bean Rua opens late in the afternoon...", Button("Buy Ticket", "@ticket"), Button("End Conversation", "@exit"));
					break;
				case 2:
					Msg("Now now. Maybe later...", Button("Buy Ticket", "@ticket"), Button("End Conversation", "@exit"));
					break;
				case 3:
					Msg("You're here too early. Please come back in a bit.", Button("Buy Ticket", "@ticket"), Button("End Conversation", "@exit"));
					break;
				case 4:
					Msg("You can't go in right now...<br/>stop being so stubborn.", Button("Buy Ticket", "@ticket"), Button("End Conversation", "@exit"));
					break;
				case 5:
					Msg("I was told you'd come, but... you're too early.", Button("Buy Ticket", "@ticket"), Button("End Conversation", "@exit"));
					break;
				case 6:
					Msg("Can you please wait for a bit?", Button("Buy Ticket", "@ticket"), Button("End Conversation", "@exit"));
					break;
			}
		}
		else if (ErinnHour(18, 6))
		{
			switch (Random(9))
			{
				case 0:
					Msg("Would you prefer talking to me, instead of just walking on in...", Button("Buy Ticket", "@ticket"), Button("End Conversation", "@exit"));
					break;
				case 1:
					Msg("Are you looking for your party...?", Button("Buy Ticket", "@ticket"), Button("End Conversation", "@exit"));
					break;
				case 2:
					Msg("You're staring at me...<p/>...Do I have something on my face?", Button("Buy Ticket", "@ticket"), Button("End Conversation", "@exit"));
					break;
				case 3:
					Msg("Hello there. Looking good.", Button("Buy Ticket", "@ticket"), Button("End Conversation", "@exit"));
					break;
				case 4:
					Msg("Welcome to Bean Rua.", Button("Buy Ticket", "@ticket"), Button("End Conversation", "@exit"));
					break;
				case 5:
					Msg("The door's open. You can walk in, you know...", Button("Buy Ticket", "@ticket"), Button("End Conversation", "@exit"));
					break;
				case 6:
					Msg("Do you have anything for me?", Button("Buy Ticket", "@ticket"), Button("End Conversation", "@exit"));
					break;
				case 7:
					Msg("Welcome! Welcome! Welcome to Bean Rua, where the beautiful redheads rule.", Button("Buy Ticket", "@ticket"), Button("End Conversation", "@exit"));
					break;
				case 8:
					Msg("Did you make a reservation?", Button("Buy Ticket", "@ticket"), Button("End Conversation", "@exit"));
					break;
			}
		}
		else
		{
			//unofficial, Doormen should be hidden during the day
			End("Bean Rua is currently closed,<br/>please come back during the night.");
		}

		switch (await Select())
		{
			case "@ticket":
				if (Player.HasItem(73110))
					Msg("You must be a member of our club. We have a member's discount for the ticket.<br/>The ticket is good for one-time use.<br/>The ticket will cost you 500 Gold. Would you like to buy one?", Button("Buy", "@buy"), Button("Cancel", "@exit"));
				else
					Msg("To enter Club Bean Rua, you must first buy a ticket.<br/>The ticket is only good for a one-time use and will expire after a certain amount of time.<br/>The ticket will cost you 1,000 Gold. Would you like to buy one now?", Button("Buy", "@buy"), Button("Cancel", "@exit"));
				switch (await Select())
				{
					case "@buy":
						if ((Player.Inventory.Gold >= 500) && (Player.HasItem(73110)))
						{
							Player.Inventory.Gold -= 500;
							Player.GiveItem(73111);
							Msg("Thank you! Have a great time!");
							Send.Notice(Player, L("Received Ticket to Bean Rua from Bean Rua Doorman."));
							Close2();
						}
						else if (Player.Inventory.Gold >= 1000)
						{
							Player.Inventory.Gold -= 1000;
							Player.GiveItem(73111);
							Msg("Thank you! Have a great time!");
							Send.Notice(Player, L("Received Ticket to Bean Rua from Bean Rua Doorman."));
							Close2();
						}
						else
							End("Hmmm, you're short on cash.");
						return;

					case "@exit":
						End("Please come back later!");
						return;
				}
				return;

			case "@exit":
				End("Have a good time!");
				return;
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


public class BeanRuaGuard2Script : BeanRuaGuard1Script { public override void Load() { base.Load(); SetName("_beanruaguard02"); SetFace(skinColor: 15, eyeType: 4, eyeColor: 32, mouthType: 0); SetLocation(52, 48270, 48122, 155); EquipItem(Pocket.Face, 4900, 0x00F8E24C, 0x00707072, 0x006C706C); EquipItem(Pocket.Hair, 4038, 0x00AA7840, 0x00AA7840, 0x00AA7840); } 
[On("ErinnTimeTick")]
	public override void OnErinnTimeTick(ErinnTime time)
	{
		if (ErinnHour(16, 6))
		{
			if (NPC.RegionId != 52)
				NPC.Warp(52, 48270, 48122);
		}
		else if (NPC.RegionId != 22)
		{
			NPC.Warp(22, 6500, 4800);
		}
	}
}
