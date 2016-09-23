//--- Aura Script -----------------------------------------------------------
// Final G1 dungeon puzzle
//--- Description -----------------------------------------------------------
// A room with a chest, containing the boss room key and a pass to get back
// to the last floor of the last dungeon.
//---------------------------------------------------------------------------

[PuzzleScript("final_dungeon")]
public class FinalDungeonScript : PuzzleScript
{
	private const int GoddessPass = 73034;

	public override void OnPrepare(Puzzle puzzle)
	{
		var lockedPlace = puzzle.NewPlace("LockedPlace");

		lockedPlace.DeclareLockSelf();
		lockedPlace.ReservePlace();
	}

	public override void OnPuzzleCreate(Puzzle puzzle)
	{
		var lockedPlace = puzzle.GetPlace("LockedPlace");
		var key = puzzle.LockPlace(lockedPlace, "Boss");

		var chest = new Chest(puzzle, "KeyChest");
		lockedPlace.AddProp(chest, Placement.Center);

		chest.Info.Direction = MabiMath.DegreeToRadian(90);
		chest.Add(Item.Create(GoddessPass));
		chest.Add(key);
	}
}
