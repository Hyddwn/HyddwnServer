//--- Aura Script -----------------------------------------------------------
// Points Shop
//--- Description -----------------------------------------------------------
// Example of a shop that sells items for points, that you would normally
// buy in the Item Shop.
//---------------------------------------------------------------------------

public class CustomPointsShopScript : NpcScript
{
	public CustomPointsShopScript()
	{
		CharacterCards = new List<Card>();
		PetCards = new List<Card>();

		//-- Options ------------------------------------------------------------

		CharacterCards.Add(new Card(L("Basic Character Card"), id: 0, price: 7900));
		CharacterCards.Add(new Card(L("Premium Character Card"), id: 1, price: 9500));

		PetCards.Add(new Card(L("Yellow Jindo"), id: 200001, price: 2900));
		PetCards.Add(new Card(L("Orange Pixie"), id: 201001, price: 2900));

		// For the item shop, see end of file.

		//-- Options End --------------------------------------------------------
	}

	private List<Card> CharacterCards { get; set; }
	private List<Card> PetCards { get; set; }

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
		var menu = Elements(L("What can I do for you?"));

		if (CharacterCards.Count != 0)
			menu.Add(Button(L("Character Cards"), "@characters"));
		if (IsEnabled("SystemPet") && PetCards.Count != 0)
			menu.Add(Button(L("Pet Cards"), "@pets"));
		menu.Add(Button(L("Item Shop"), "@items"));
		menu.Add(Button(L("End Conversation"), "@end"));

		Msg(menu);

		var result = await Select();
		if (result != "@end")
		{
			Msg(L("Please have a look around."));
			switch (result)
			{
				case "@characters":
					await CardShop(CharacterCards, false);
					break;

				case "@pets":
					if (IsEnabled("SystemPet"))
						await CardShop(PetCards, true);
					break;

				case "@items":
					OpenShop("CustomPointsShop");
					break;
			}
		}

		Close(Hide.None, "Come back any time!");
	}

	protected virtual async Task CardShop(List<Card> cardList, bool pets)
	{
		var list = List("", Math.Min(10, cardList.Count));

		for (int i = 0; i < cardList.Count; ++i)
		{
			var card = cardList[i];
			list.Add(Button(string.Format("{0} ({1:n0})", card.Name, card.Price), "@card" + i));
		}

		while (true)
		{
			list.Text = string.Format(L("Cards - Your Pon: {0:n0}"), Player.Points);

			Msg(list);

			var result = await Select();
			if (result == "@end")
				break;

			var indexStr = result.Replace("@card", "");

			int index;
			if (!int.TryParse(indexStr, out index) || index > cardList.Count - 1)
			{
				Close(L("(Error: Invalid response, please report.)"));
				Log.Error("Invalid response '{0}' in points shop script.", result);
				break;
			}

			var card = cardList[index];
			var cardId = card.Id;
			var price = card.Price;

			if (Player.Points < price)
			{
				Msg(L("Oh, it seems like you can't afford that right now."));
				continue;
			}

			Player.Points -= price;
			if (!pets)
				ChannelServer.Instance.Database.AddCard(Player.Client.Account.Id, cardId, 0);
			else
				ChannelServer.Instance.Database.AddCard(Player.Client.Account.Id, MabiId.PetCardType, cardId);

			Msg(L("Thank you! Anything else?"));
		}
	}

	protected class Card
	{
		public int Id { get; private set; }
		public int Price { get; private set; }
		public string Name { get; private set; }

		public Card(string name, int id, int price)
		{
			this.Id = id;
			this.Name = name;
			this.Price = price;
		}
	}
}

public class CustomPointsShop : NpcShopScript
{
	public override void Setup()
	{
		SetPaymentMethod(L("Combat"), PaymentMethod.Points);
		SetPaymentMethod(L("Consumables"), PaymentMethod.Points);
		SetPaymentMethod(L("Appearance"), PaymentMethod.Points);

		Add(L("Combat"), itemId: 45014, amount: 1000, price: 50); // Arrows (1000)
		Add(L("Combat"), itemId: 45015, amount: 1000, price: 50); // Bolts (1000)
		Add(L("Combat"), itemId: 63044, amount: 1, price: 200);   // Party Phoenix Feather (1)
		Add(L("Combat"), itemId: 63044, amount: 5, price: 800);   // Party Phoenix Feather (5)

		Add(L("Consumables"), itemId: 63029, amount: 1, price: 150);  // Campfire Kit (1)
		Add(L("Consumables"), itemId: 63029, amount: 5, price: 600);  // Campfire Kit (5)
		Add(L("Consumables"), itemId: 63025, amount: 1, price: 600);  // Massive Holy Water of Lymilark (1)
		Add(L("Consumables"), itemId: 63025, amount: 5, price: 2300); // Massive Holy Water of Lymilark (5)

		Add(L("Appearance"), itemId: 63037, amount: 1, price: 990); // Dye Ampoule

		if (IsEnabled("NaoCoupon"))
		{
			Add(L("Consumables"), itemId: 85000, amount: 1, price: 300);   // Nao Soul Stone (1)
			Add(L("Consumables"), itemId: 85000, amount: 10, price: 2500); // Nao Soul Stone (10)
			Add(L("Consumables"), itemId: 85000, amount: 30, price: 7000); // Nao Soul Stone (30)
		}
	}
}
