//--- Aura Script -----------------------------------------------------------
// Switch Room Puzzle
//--- Description -----------------------------------------------------------
// Creates a room with 1 switch in the center that opens the door.
//---------------------------------------------------------------------------

[PuzzleScript("rp_giantspider2")]
public class rp_giantspider2PuzzleScript : PuzzleScript
{
	public override void OnPrepare(Puzzle puzzle)
	{
		var lockedPlace = puzzle.NewPlace("LockedPlace");

		lockedPlace.DeclareLockSelf();
		lockedPlace.ReservePlace();

		puzzle.Set("activated", false);
	}

	public override void OnPuzzleCreate(Puzzle puzzle)
	{
		var lockedPlace = puzzle.GetPlace("LockedPlace");

		var zwitch = new Switch("Switch", lockedPlace.LockColor);
		lockedPlace.AddProp(zwitch, Placement.Center);

		puzzle.LockPlace(lockedPlace);
	}

	public override void OnPropEvent(Puzzle puzzle, Prop prop)
	{
		var Switch = prop as Switch;
		if (Switch == null)
			return;

		if (puzzle.Get("activated"))
			return;

		var lockedPlace = puzzle.GetPlace("LockedPlace");
		lockedPlace.Open();

		puzzle.Set("activated", true);
	}
}
