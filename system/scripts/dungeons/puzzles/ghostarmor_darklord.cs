//--- Aura Script -----------------------------------------------------------
// Ghost Armor and Dark Lord puzzle
//--- Description -----------------------------------------------------------
// Spawns Ghost Armors first and then Dark Lord. Used in G1 Rabbie RP,
// g1rp_25_dunbarton_rabbie_dungeon.
//---------------------------------------------------------------------------

[PuzzleScript("ghostarmor_darklord")]
public class GhostArmorDarkLordScript : PuzzleScript
{
	public override void OnPrepare(Puzzle puzzle)
	{
		var lockedPlace = puzzle.NewPlace("LockedPlace");

		lockedPlace.DeclareLockSelf();
		lockedPlace.ReservePlace();
	}

	public override void OnPuzzleCreate(Puzzle puzzle)
	{
		var lockedPlace = puzzle.GetPlace("LockedPlace");
		puzzle.LockPlace(lockedPlace, "Boss");

		lockedPlace.SpawnSingleMob("Mob1", 12001, 6); // Ghost Armor
		lockedPlace.CloseAllDoors();
	}

	public override void OnMonsterDead(Puzzle puzzle, MonsterGroup group)
	{
		var dungeon = puzzle.Dungeon;

		if (group.Name == "Mob1")
		{
			if (group.Remaining == 0)
			{
				dungeon.PlayCutscene("G1_25_b_3WarriorsRP2", cutscene =>
				{
					var lockedPlace = puzzle.GetPlace("LockedPlace");
					lockedPlace.SpawnSingleMob("Mob2", 7, 1); // Dark Lord

					var darkLordGroup = puzzle.GetMonsterGroup("Mob2");
					darkLordGroup.AddKeyForLock(lockedPlace);
				});
			}
		}
		else
		{
			dungeon.PlayCutscene("G1_25_c_3WarriorsRP2");
		}
	}
}
