//--- Aura Script -----------------------------------------------------------
// Rab
//--- Description -----------------------------------------------------------
// Script for Fleta's pet dog, Rab
//---------------------------------------------------------------------------

public class RabScript : NpcScript
{
	public override void Load()
	{
		SetRace(20);
		SetName("_rab");
		SetBody(height: 0.9f);
		SetColor(0x00000000, 0x00404040, 0x00C0C0C0);
		if (IsEnabled("Fleta"))
		{
			if (ErinnHour(9, 11) || ErinnHour(15, 17) || ErinnHour(19, 21))
				SetLocation(53, 103322, 109446, 0);
			else
				SetLocation(22, 6500, 4800, 0);
		}
		SetAi("npc_rab");


		AddPhrase("Ruff");
		AddPhrase("Whimper");
		AddPhrase("Hmmph...");
		AddPhrase("...");
		AddPhrase("......");
		AddPhrase("Kmmmph");
		AddPhrase("Ruff, ruff");
	}

	[On("ErinnTimeTick")]
	public void OnErinnTimeTick(ErinnTime time)
	{
		if ((ErinnHour(9, 11)) || (ErinnHour(15, 17)) || (ErinnHour(19, 21)))
		{
			if (NPC.RegionId != 53)
				NPC.Warp(53, 103322, 109446);
		}
		else if (NPC.RegionId != 22)
		{
			NPC.Warp(22, 6500, 4800);
		}
	}

	protected override async Task Talk()
	{
		await Hook("after_intro");

		Msg(Hide.Both, "(Fleta's dog. I think it's name is Rab)");
	}

	protected override async Task TalkPet()
	{
		if (!Player.HasItem(74160))
		{
			Msg(Hide.Both, "(Rab pushes forward his empty bowl)");
			Player.GiveItem(74160); // Rab's Empty Plate
			Send.Notice(Player, ("Received Rab's Empty Plate from Fleta's Rab."));
			Msg("Grr! (Look at this empty bowl!)<br/>Ruff Ruff! (I'm starving!)<br/>Bark Bark. Grrr Woof Woof! (I want something new and tasty!)<br/>Ruff, Howl. (My master knows what I like but she's too lazy to cook it for me.)");
			Msg("Woof Woof! (All I have are these empty bowls to chew on!)<br/>Woof Woof Woof! (Now I'm getting even tired of that, so you can have it.)<br/>Whimper. (Every time you see the empty bowl)<br/>Whimpppper. (pray that I'll be able to eat some good food)");
		}
		else
			Msg("Ruff? Ruff!! (Hey, you have my bowl!)<br/>*Whimper* (Oh, how I wish the bowl was full)");
	}

	protected override async Task Gift(Item item, GiftReaction reaction)
	{
		switch (reaction)
		{
			default:
				Msg(L("Woof!"));
				break;
		}
	}
}