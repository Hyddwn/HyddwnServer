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
		Msg(Hide.Both, "(Fleta's dog. I think it's name is Rab)");
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