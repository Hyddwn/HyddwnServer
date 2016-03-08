//--- Aura Script -----------------------------------------------------------
// Points Shop
//--- Description -----------------------------------------------------------
// Example of a shop that sells items for points, that you would normally
// buy in the Item Shop.
//---------------------------------------------------------------------------

public class CustomPointsShopScript : NpcScript
{
	public override void Load()
	{
		SetRace(10001);
		SetName(L("Pon Shop"));
		SetBody(weight: 0.7f, upper: 0.7f, lower: 0.7f);
		SetFace(skinColor: 23, eyeType: 152, eyeColor: 38, mouthType: 48);
		SetLocation(14, 40380, 37259, 94);

		EquipItem(Pocket.Face, 3907, 0x00366969, 0x0094B330, 0x00737474);
		EquipItem(Pocket.Hair, 4933, 0x00E3CCBA, 0x00E3CCBA, 0x00E3CCBA);
		EquipItem(Pocket.Armor, 15946, 0x00EBD2D2, 0x00FFFFFF, 0x00C6794A);
	}

	protected override async Task Talk()
	{
		Msg(L("What can I do for you?"));
		OpenShop("CustomPointsShop");
	}
}

public class CustomPointsShop : NpcShopScript
{
	public override void Setup()
	{
		SetPaymentMethod(L("Combat"), PaymentMethod.Points);
		SetPaymentMethod(L("Consumables"), PaymentMethod.Points);

		Add(L("Combat"), 45014, 1000, 50); // Arrows (1000)
		Add(L("Combat"), 45015, 1000, 50); // Bolts (1000)
		Add(L("Combat"), 63044, 1, 200);   // Party Phoenix Feather (1)
		Add(L("Combat"), 63044, 5, 800);   // Party Phoenix Feather (5)

		Add(L("Consumables"), 63029, 1, 150); // Campfire Kit (1)
		Add(L("Consumables"), 63029, 5, 600); // Campfire Kit (5)

		if (IsEnabled("NaoCoupon"))
		{
			Add(L("Consumables"), 85000, 1, 300);   // Nao Soul Stone (1)
			Add(L("Consumables"), 85000, 10, 2500); // Nao Soul Stone (10)
			Add(L("Consumables"), 85000, 30, 7000); // Nao Soul Stone (30)
		}
	}
}
