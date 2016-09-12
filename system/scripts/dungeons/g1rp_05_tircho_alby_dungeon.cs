//--- Aura Script -----------------------------------------------------------
// The Three Missing Warriors RP dungeon
//--- Description -----------------------------------------------------------
// First RP dungeon in the G1 mainstream quests, featuring a party of 3,
// playing Tarlach, Mari, Ruairi.
//---------------------------------------------------------------------------

[DungeonScript("g1rp_05_tircho_alby_dungeon")]
public class TheThreeMissingWarriorsRPDungeonScript : DungeonScript
{
	public override void OnCreation(Dungeon dungeon)
	{
		//dungeon.SetRole(0, "#tarlach");
		//dungeon.SetRole(1, "#mari");
		//dungeon.SetRole(2, "#ruairi");
	}

	public override void OnBoss(Dungeon dungeon)
	{
		dungeon.AddBoss(30004, 1); // Giant Spider
		dungeon.AddBoss(30003, 6); // Red Spider

		dungeon.PlayCutscene("bossroom_GiantSpider");
	}

	public override void OnPartyEntered(Dungeon dungeon, Creature creature)
	{
		dungeon.PlayCutscene("G1_5_a_3WarriorsRP");
	}

	public override void OnSectionCleared(Dungeon dungeon, int floor, int section)
	{
		if (floor == 1 && section == 2)
			dungeon.PlayCutscene("G1_5_b_3WarriorsRP");
	}

	public override void OnLeftEarly(Dungeon dungeon, Creature creature)
	{
		dungeon.PlayCutscene("G1_LeaveDungeon");
	}

	public override void OnCleared(Dungeon dungeon)
	{
		dungeon.PlayCutscene("G1_5_c_3WarriorsRP", cutscene =>
		{
			// Switch keywords for all members
			// Iirc officials do it only for the leader, but really,
			// who wants to run this thrice...?
			var creators = dungeon.GetCreators();
			foreach (var member in creators)
			{
				if (member.Keywords.Has("g1_03"))
				{
					member.Keywords.Remove("g1_03");
					member.Keywords.Give("g1_04");
					member.Keywords.Remove("g1_tarlach1");
					member.Keywords.Remove("g1_tarlach2");
					member.Keywords.Give("g1_goddess");
				}
			}

			// Get out
			dungeon.RemoveAllPlayers();
		});
	}
}
