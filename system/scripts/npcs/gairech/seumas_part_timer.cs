//--- Aura Script -----------------------------------------------------------
// Seumas Part Timer
//--- Description -----------------------------------------------------------
// Miner
//---------------------------------------------------------------------------

public class SeumasAlbaFemaleScript : NpcScript
{
	public override void Load()
	{
		SetRace(10001);
		SetName("_seumas_alba_female");
		SetBody(weight: 1.3f);
		SetFace(skinColor: 15, eyeType: 4, eyeColor: 8, mouthType: 12);
		SetStand("human/anim/tool/Rhand_A/female_tool_Rhand_A02_mining");
		SetLocation(30, 38230, 48540, 225);

		EquipItem(Pocket.Face, 3900, 0x00556A5B, 0x00006655, 0x000A84AE);
		EquipItem(Pocket.Hair, 3083, 0x00DCAF01, 0x00DCAF01, 0x00DCAF01);
		EquipItem(Pocket.Armor, 15059, 0x00814F35, 0x00F3DEC0, 0x003C2A25);
		EquipItem(Pocket.Glove, 16017, 0x003F3F5D, 0x00D2AC01, 0x0052B3EA);
		EquipItem(Pocket.Shoe, 17022, 0x00232325, 0x00D2AC01, 0x00808080);
		EquipItem(Pocket.Head, 18024, 0x00964D25, 0x00CAA859, 0x00440000);
		EquipItem(Pocket.RightHand1, 40025, 0x00454545, 0x00745D2F, 0x00EEA140);

		AddPhrase("I can take it.");
	}

	protected override async Task Talk()
	{
		End("I might be a part-timer right now, but one day I will be a great archaeologist.");
	}
}

public class SeumasAlbaMaleScript : NpcScript
{
	public override void Load()
	{
		SetRace(10002);
		SetName("_seumas_alba_male");
		SetFace(skinColor: 15, eyeType: 18, eyeColor: 0, mouthType: 21);
		SetStand("human/anim/tool/Rhand_A/female_tool_Rhand_A02_mining");
		SetLocation(30, 38425, 48274, 122);

		EquipItem(Pocket.Face, 4900, 0x00521070, 0x00F8934D, 0x00BC6DAD);
		EquipItem(Pocket.Hair, 4019, 0x00C61400, 0x00C61400, 0x00C61400);
		EquipItem(Pocket.Armor, 15044, 0x00514940, 0x00CDAD7C, 0x007B9AF7);
		EquipItem(Pocket.Glove, 16017, 0x00263A6C, 0x00673F00, 0x00B0C7FF);
		EquipItem(Pocket.Shoe, 17022, 0x0017181A, 0x00100041, 0x00808080);
		EquipItem(Pocket.Head, 18024, 0x00964D25, 0x00CAA859, 0x0001A958);
		EquipItem(Pocket.RightHand1, 40025, 0x00454545, 0x00745D2F, 0x00EEA140);

		AddPhrase("*Pant*... Can we take a short break?");
	}

	protected override async Task Talk()
	{
		End("I might be a part-timer right now, but one day I will be a great archaeologist.");
	}
}