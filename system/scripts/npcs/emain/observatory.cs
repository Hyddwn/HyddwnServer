//--- Aura Script -----------------------------------------------------------
// Emain Observatory tools
//--- Description -----------------------------------------------------------
// telescope and viewscope npcs found on top of the observatory
//---------------------------------------------------------------------------

public class telescopenpcScript : NpcScript
{
	public override void Load()
	{
		SetRace(990002);
		SetName("_telescopenpc");
		SetLocation(52, 43699, 36138, 255);
	}

	protected override async Task Talk()
	{
		if (ErinnHour(20, 4))
			End("It's hard to see since it's dark outside.<br/>Please use this during the daylight.");
		else
			Msg("This is a telescope that allows you to enjoy the beautiful scenery of Emain Macha.<br/>Use this to see places that are far from the Observatory in detail.<br/>Costs 10 Gold per use.", Button("End", "@end"), Button("View", "@view"));

		switch (await Select())
		{
			case "@end":
				End("Try using this another time.");
				return;

			case "@view":
				if (Player.Inventory.Gold >= 10)
				{
					Player.Inventory.Gold -= 10;
					Cutscene.Play("etc_event_Emainmach_telescope", Player);
					// Numbers should randomize
					// Notice should not be sent until after cutscene ends
					Send.Notice(Player, L("5425...\nWho would write such numbers, and why?"));
					Close2();
				}
				else
					End("You don't have enough money.");
				return;
		}
	}
}

public class viewscopenpcScript : NpcScript
{
	public override void Load()
	{
		SetRace(990004);
		SetName("_viewscopenpc");
		SetLocation(52, 43713, 36412, 194);
	}

	protected override async Task Talk()
	{
		Msg("This is a viewscope that allows you to enjoy the beautiful scenery of Emain Macha.<br/>Use this to see the beautiful Emain Macha in detail!<br/>Costs 15 Gold per use.", Button("End", "@end"), Button("View", "@view"));

		switch (await Select())
		{
			case "@end":
				End("Try using this another time.");
				return;

			case "@view":
				if (Player.Inventory.Gold >= 15)
				{
					Player.Inventory.Gold -= 15;
					Cutscene.Play("into_the_Emainmach", Player);
					Close2();
				}
				else
					End("You don't have enough money.");
				return;
		}
	}
}