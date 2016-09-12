//--- Aura Script -----------------------------------------------------------
// Albey Red Dungeon
//--- Description -----------------------------------------------------------
// Dungeon to get one of the four fragments for the Black Orb in.
//---------------------------------------------------------------------------

[DungeonScript("g1_37_red_tirnanog_dungeon")]
public class AlbeyRedDungeonScript : DungeonScript
{
	private const int OrbHits = 5;
	private const int FragmentId = 73031;

	public override void OnCreation(Dungeon dungeon)
	{
		var region = dungeon.Regions.Last();
		var bossLocation = dungeon.GetBossRoomCenter();

		// Gargoyles
		region.AddProp(new Prop(29001, bossLocation.RegionId, bossLocation.X, bossLocation.Y, 0));
		region.AddProp(new Prop(29002, bossLocation.RegionId, bossLocation.X, bossLocation.Y, 0));
		region.AddProp(new Prop(29003, bossLocation.RegionId, bossLocation.X, bossLocation.Y, MabiMath.DegreeToRadian(90)));
		region.AddProp(new Prop(29004, bossLocation.RegionId, bossLocation.X, bossLocation.Y, MabiMath.DegreeToRadian(90)));
		region.AddProp(new Prop(29005, bossLocation.RegionId, bossLocation.X, bossLocation.Y, 0));
		region.AddProp(new Prop(29006, bossLocation.RegionId, bossLocation.X, bossLocation.Y, 0));
	}

	public override void OnBoss(Dungeon dungeon)
	{
		dungeon.AddBoss(170001, 1); // Nightmare Humanoid
	}

	public override void OnCleared(Dungeon dungeon)
	{
		var rnd = RandomProvider.Get();
		var creators = dungeon.GetCreators();
		var count = Math.Min(7, creators.Count);

		for (int i = 0; i < count; ++i)
		{
			var member = creators[i];
			var treasureChest = new TreasureChest();

			treasureChest.AddGold(rnd.Next(320, 1280)); // Gold
			treasureChest.Add(GetRandomTreasureItem(rnd)); // Random item

			dungeon.AddChest(treasureChest);

			member.GiveItemWithEffect(Item.CreateKey(70028, "chest"));
		}

		var orb = new Prop(25032, 0, 0, 0, 0);
		orb.SetState("off");
		orb.Behavior = OnOrbHit;
		dungeon.AddChest(orb);
	}

	private void OnOrbHit(Creature creature, Prop prop)
	{
		if (prop.State == "off")
		{
			var hits = prop.Vars.Temp.Get("orbHit", 0) + 1;
			prop.Vars.Temp["orbHit"] = hits;

			if (hits == OrbHits)
			{
				Send.Notice(creature, L("You broke the seal and received the Black Orb Fragment!\nCollect all four to receive the Black Orb."));

				prop.SetState("on");
				new Item(FragmentId).Drop(prop.Region, prop.GetPosition(), 50);
			}
		}
	}

	List<DropData> drops;
	public Item GetRandomTreasureItem(Random rnd)
	{
		if (drops == null)
		{
			drops = new List<DropData>();
			drops.Add(new DropData(itemId: 62004, chance: 20, amountMin: 1, amountMax: 2)); // Magic Powder
			drops.Add(new DropData(itemId: 51102, chance: 20, amountMin: 1, amountMax: 2)); // Mana Herb
			drops.Add(new DropData(itemId: 51003, chance: 20, amountMin: 1, amountMax: 2)); // HP 50 Potion
			drops.Add(new DropData(itemId: 51008, chance: 20, amountMin: 1, amountMax: 2)); // MP 50 Potion
		}

		return Item.GetRandomDrop(rnd, drops);
	}
}
