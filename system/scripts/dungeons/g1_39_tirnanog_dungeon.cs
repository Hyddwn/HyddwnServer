//--- Aura Script -----------------------------------------------------------
// Albey Final Dungeon
//--- Description -----------------------------------------------------------
// Final dungeon in G1, with 6 floors and Glas Ghaibhleann at the end.
// On the last floor there's a chest with a Goddess Pass, so you can leave
// and come back to the boss right away. The dungeon you come back to only
// has one floor, the last one.
//---------------------------------------------------------------------------

[DungeonScript("g1_39_tirnanog_dungeon")]
public class AlbeyFinalDungeonScript : DungeonScript
{
	public const int DarkLord = 8;
	public const int GhostArmor = 12001;
	public const int Glas = 6;
	public const int WhoSavedTheGoddessTitle = 11001;
	public const int GoddessEnchant = 30310;
	public const int GlasPropId = 26054;

	public override void OnCreation(Dungeon dungeon)
	{
		var region = dungeon.Regions.Last();
		var bossLocation = dungeon.GetBossRoomCenter();

		// Glas on the ceiling
		region.AddProp(new Prop(GlasPropId, bossLocation.RegionId, bossLocation.X, bossLocation.Y, MabiMath.DegreeToRadian(90)));

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
		dungeon.AddBoss(DarkLord, 1);
		dungeon.AddBoss(GhostArmor, 2);

		dungeon.CompleteManually(true);

		dungeon.PlayCutscene("G1_40_a_Cichol");
	}

	public override void OnBossDeath(Dungeon dungeon, Creature deadBoss, Creature killer)
	{
		if (deadBoss.RaceId == DarkLord)
		{
			var glasProp = dungeon.Regions.Last().GetProp(a => a.Info.Id == GlasPropId);
			glasProp.SetState("released");

			dungeon.PlayCutscene("G1_40_b_Cichol", cutscene =>
			{
				dungeon.AddBoss(Glas, 1);
			});
		}
		else if (deadBoss.RaceId == Glas)
		{
			dungeon.Complete();
		}
	}

	public override void OnCleared(Dungeon dungeon)
	{
		var creators = dungeon.GetCreators();
		var leader = creators[0].Party.Leader;

		if (leader.Keywords.Has("g1_38"))
		{
			leader.Keywords.Remove("g1");
			leader.Keywords.Remove("g1_38");
			leader.Keywords.Remove("g1_revive_of_glasgavelen");
			leader.Keywords.Give("g1_complete");
			leader.Keywords.Give("g1_KnightOfTheLight");

			leader.Titles.Enable(WhoSavedTheGoddessTitle);
			leader.AcquireItem(Item.CreateEnchant(GoddessEnchant, 3600));
		}

		dungeon.PlayCutscene("G1_41_b_Glas", cutscene =>
		{
			foreach (var member in dungeon.GetCreators())
			{
				member.Warp("Uladh_main/town_TirChonaill/TirChonaill_Spawn_A");
			}
		});
	}

	public override void OnLeftEarly(Dungeon dungeon, Creature creature)
	{
		dungeon.PlayCutscene("G1_LeaveDungeon");
	}
}

[DungeonScript("g1_39_tirnanog_dungeon_again")]
public class AlbeyFinalAgainDungeonScript : AlbeyFinalDungeonScript
{
}
