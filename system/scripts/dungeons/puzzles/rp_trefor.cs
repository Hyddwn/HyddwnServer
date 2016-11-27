//--- Aura Script -----------------------------------------------------------
// NPC Protection Puzzle
//--- Description -----------------------------------------------------------
// Part of Trefor's RP, protection an NPC from goblins. There are two
// versions puzzle, with one having a stronger NPC, after you've failed
// the first time.
//---------------------------------------------------------------------------

[PuzzleScript("rp_trefor")]
public class rp_treforPuzzleScript : PuzzleScript
{
	public override void OnPrepare(Puzzle puzzle)
	{
		var lockedPlace = puzzle.NewPlace("LockedPlace");

		lockedPlace.DeclareLockSelf();
		lockedPlace.ReservePlace();
		lockedPlace.ReserveDoors();
	}

	public override void OnPuzzleCreate(Puzzle puzzle)
	{
		var lockedPlace = puzzle.GetPlace("LockedPlace");
		puzzle.LockPlace(lockedPlace);

		lockedPlace.SpawnSingleMob("Mob1", 10101, 6); // Goblins
		lockedPlace.SpawnSingleMob("Mob2", 1000, 1); // Traveler A

		lockedPlace.CloseAllDoors();
	}

	public override void OnMonsterDead(Puzzle puzzle, MonsterGroup group)
	{
		var dungeon = puzzle.Dungeon;

		if (group.Name == "Mob1")
		{
			// Killed goblins
			if (group.Remaining == 0)
			{
				var leader = dungeon.GetCreators().First();

				// You only get the reward keyword if you've failed less
				// than three times.
				if (!leader.Keywords.Has("RP_Trefor_Failed_3"))
				{
					dungeon.PlayCutscene("RP_Trefor_00_c", cutscene => dungeon.RemoveAllPlayers());
					leader.Keywords.Give("RP_Trefor_LifeGuard");
				}
				else
				{
					dungeon.PlayCutscene("RP_Trefor_00_d", cutscene => dungeon.RemoveAllPlayers());
				}

				leader.Keywords.Give("RP_Trefor_Complete");
				leader.Keywords.Remove("RP_Trefor_Failed_1");
				leader.Keywords.Remove("RP_Trefor_Failed_2");
				leader.Keywords.Remove("RP_Trefor_Failed_3");
			}
		}
		// Traveler died
		else if (group.Name == "Mob2")
		{
			var leader = dungeon.GetCreators().First();

			if (leader.Keywords.Has("RP_Trefor_Failed_1"))
			{
				leader.Keywords.Remove("RP_Trefor_Failed_1");
				leader.Keywords.Give("RP_Trefor_Failed_2");
			}
			else if (leader.Keywords.Has("RP_Trefor_Failed_2"))
			{
				leader.Keywords.Remove("RP_Trefor_Failed_2");
				leader.Keywords.Give("RP_Trefor_Failed_3");
			}
			else if (!leader.Keywords.Has("RP_Trefor_Failed_3"))
			{
				leader.Keywords.Give("RP_Trefor_Failed_1");
			}

			dungeon.PlayCutscene("RP_Trefor_00_b", cutscene => dungeon.RemoveAllPlayers());
		}
	}
}

[PuzzleScript("rp_trefor2")]
public class rp_trefor2PuzzleScript : rp_treforPuzzleScript
{
	public override void OnPuzzleCreate(Puzzle puzzle)
	{
		var lockedPlace = puzzle.GetPlace("LockedPlace");
		puzzle.LockPlace(lockedPlace);

		lockedPlace.SpawnSingleMob("Mob1", 10101, 6); // Goblins
		lockedPlace.SpawnSingleMob("Mob2", 1001, 1); // Traveler A (2)

		lockedPlace.CloseAllDoors();
	}
}
