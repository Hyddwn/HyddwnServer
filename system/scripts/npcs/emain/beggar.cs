//--- Aura Script -----------------------------------------------------------
// Emain Macha Beggar
//--- Description -----------------------------------------------------------
// Script for Emain Macha's Beggar
//---------------------------------------------------------------------------

public class BeggarScript : NpcScript
{
	public override void Load()
	{
		SetRace(10002);
		SetName("_emainbeggar");
		SetBody(height: 0.62f, weight: 0.94f);
		SetFace(skinColor: 15, eyeType: 0, eyeColor: 27, mouthType: 0);
		SetLocation(52, 41435, 42285, 190);
		SetAi("npc_begger");

		EquipItem(Pocket.Face, 4900, 0x00EE012D, 0x00FB964C, 0x00A40182);
		EquipItem(Pocket.Hair, 4006, 0x00A2917B, 0x00A2917B, 0x00A2917B);
		EquipItem(Pocket.Robe, 19001, 0x0083827C, 0x00FFFFFF, 0x00FFFFFF);
		SetHoodDown();

		AddPhrase("You know I'm here!!");
		AddPhrase("Spare change, pleeeeeease");
		AddPhrase("Help out the poor. You'll be blessed with longevity!");
	}

	protected override async Task Talk()
	{
		RndMsg(
			"Spare change, please...",
			"I'm hungry...",
			"Some Gold, please..."
		);
	}

	protected override async Task Gift(Item item, GiftReaction reaction)
	{
		if (item.Info.Id == 2000)
			Msg("You don't have to feel bad about giving me more...<br/>Thank you.");
		else
			Msg("Thank you... I would have appreciated it even more if you could give me more...");
		Msg(Hide.Both, "Finished talking to the beggar.");
	}
}