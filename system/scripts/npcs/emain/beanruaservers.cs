//--- Aura Script -----------------------------------------------------------
// Bean Rua Servers
//--- Description -----------------------------------------------------------
// Script for the servers inside Bean Rua.
//---------------------------------------------------------------------------

public class BeanRua01Script : NpcScript
{
	public override void Load()
	{
		SetRace(10001);
		SetName("_beanrua01");
		SetBody(height: 0.97f, weight: 0.97f, upper: 1.09f);
		SetFace(skinColor: 15, eyeType: 3, eyeColor: 156);
		SetStand("human/female/anim/female_stand_npc_emain_Rua_02");
		SetLocation(57, 6383, 5308, 132);

		EquipItem(Pocket.Face, 3900, 0x00484863, 0x00F3F5C0, 0x00D4AD54);
		EquipItem(Pocket.Hair, 3034, 0x008E132F, 0x008E132F, 0x008E132F);
		EquipItem(Pocket.Armor, 15084, 0x00171211, 0x002E2623, 0x00875253);
		EquipItem(Pocket.Head, 18061, 0x00000000, 0x00FFFFFF, 0x00FFFFFF);

		AddPhrase("Good to see you!");
		AddPhrase("Welcome!");
	}

	protected override async Task Talk()
	{
		Msg("Welcome to our club!!!");
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

public class BeanRua02Script : BeanRua01Script { public override void Load() { base.Load(); SetName("_beanrua02"); SetBody(height: 1.00f, weight: 1.00f, upper: 1.00f); SetFace(skinColor: 17, eyeColor: 39); SetStand("human/female/anim/female_stand_npc_emain_Rua"); SetLocation(57, 6927, 6356, 143); EquipItem(Pocket.Face, 3901, 0x0001918B, 0x00C4DD83, 0x00F79825); EquipItem(Pocket.Hair, 3032, 0x00942C12, 0x00942C12, 0x00942C12); } }
public class BeanRua03Script : BeanRua01Script { public override void Load() { base.Load(); SetName("_beanrua03"); SetFace(skinColor: 15, eyeType: 3, eyeColor: 113); SetStand("human/female/anim/female_stand_npc_emain_05"); SetLocation(57, 6618, 6271, 157); EquipItem(Pocket.Face, 3900, 0x00F78F4C, 0x00C3B35D, 0x00F9A142); EquipItem(Pocket.Hair, 3024, 0x00980A0A, 0x00980A0A, 0x00980A0A); } }
public class BeanRua04Script : BeanRua01Script { public override void Load() { base.Load(); SetName("_beanrua04"); SetFace(skinColor: 15, eyeType: 3, eyeColor: 37, mouthType: 2); SetStand("human/female/anim/female_stand_npc_emain_Rua_02"); SetLocation(57, 6164, 6507, 183); EquipItem(Pocket.Face, 3900, 0x00FAF5AB, 0x00006B45, 0x00383661); EquipItem(Pocket.Hair, 3036, 0x00911411, 0x00911411, 0x00911411); } }
