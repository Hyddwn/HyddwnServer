//--- Aura Script -----------------------------------------------------------
// Mores RP dungeon
//--- Description -----------------------------------------------------------
// G1 RP dungeon, starring player as Mores.
//---------------------------------------------------------------------------

[DungeonScript("g1rp_18_dunbarton_math_dungeon")]
public class MoresRPDungeonScript : DungeonScript
{
	public override void OnCreation(Dungeon dungeon)
	{
		//dungeon.SetRole(0, "#mores");
	}

	public override void OnBoss(Dungeon dungeon)
	{
		dungeon.AddBoss(12001, 6); // Ghost Armor

		dungeon.PlayCutscene("G1_18_b_MoresRP");
	}

	public override void OnSectionCleared(Dungeon dungeon, int floor, int section)
	{
		if (floor == 1 && section == 3)
			dungeon.PlayCutscene("G1_18_a_MoresRP");
	}

	public override void OnLeftEarly(Dungeon dungeon, Creature creature)
	{
		dungeon.PlayCutscene("G1_LeaveDungeon");
	}

	public override void OnCleared(Dungeon dungeon)
	{
		dungeon.PlayCutscene("G1_18_c_MoresRP", cutscene =>
		{
			// Switch keywords
			var creators = dungeon.GetCreators();
			foreach (var member in creators)
			{
				if (member.Keywords.Has("g1_21"))
				{
					member.Keywords.Remove("g1_21");
					member.Keywords.Give("g1_22");
					member.Keywords.Remove("g1_memo_of_lost_thing");
					member.Keywords.Give("g1_goddess_morrighan1");
				}
			}

			// Get out
			dungeon.RemoveAllPlayers();
		});
	}
}
