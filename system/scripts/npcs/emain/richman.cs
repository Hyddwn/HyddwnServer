//--- Aura Script -----------------------------------------------------------
// Emain Macha Rich Man
//--- Description -----------------------------------------------------------
// Script for Emain Macha's Rich Man
//---------------------------------------------------------------------------

public class RichManScript : NpcScript
{
	public override void Load()
	{
		SetRace(10002);
		SetName("_emainrichman");
		SetBody(height: 0.94f, weight: 1.58f);
		SetFace(skinColor: 17, eyeType: 8, eyeColor: 27, mouthType: 0);
		SetLocation(52, 39162, 39778, 116);
		SetAi("npc_richman");

		EquipItem(Pocket.Face, 4950, 0x006A0027, 0x0083D0CF, 0x00005AA7);
		EquipItem(Pocket.Hair, 4084, 0x00AC9D64, 0x00AC9D64, 0x00AC9D64);
		EquipItem(Pocket.Armor, 15074, 0x0085673A, 0x00000000, 0x00FFFFFF);
		EquipItem(Pocket.Glove, 16000, 0x00FDF8EE, 0x00FFFFFF, 0x00FFFFFF);
		EquipItem(Pocket.Shoe, 17042, 0x001D241A, 0x00FFFFFF, 0x00FFFFFF);
		EquipItem(Pocket.Head, 18034, 0x002B685F, 0x00FFFFFF, 0x00FFFFFF);


		AddPhrase("Oh no, I dropped the money again");
		AddPhrase("Oh, I dropped a coin");
		AddPhrase("Money's flowing everywhere...");
		AddPhrase("Is this town full of beggars...?");
		AddPhrase("Oh, the change fell out");
	}

	protected override async Task Talk()
	{
		Msg(Hide.Both, "(This man has the look and bearing of a patrician. Maybe some coins will fall out of his pockets...)");
	}

	protected override async Task Gift(Item item, GiftReaction reaction)
	{
		switch (reaction)
		{
			default:
				Msg(L("What is this? Are you richer than me? You're giving me a present..."));
				break;
		}
	}
}