//--- Aura Script -----------------------------------------------------------
// Fox Pit Puzzle
//--- Description -----------------------------------------------------------
// Spawns foxes in an alley.
//---------------------------------------------------------------------------

[PuzzleScript("rp_chicken")]
public class rp_chickenPuzzle : PuzzleScript
{
	public override void OnPrepare(Puzzle puzzle)
	{
		var place = puzzle.NewPlace("PuzzlePlace");
		place.ReservePlace();
	}

	public override void OnPuzzleCreate(Puzzle puzzle)
	{
		var place = puzzle.GetPlace("PuzzlePlace");

		place.SpawnSingleMob("Mob1", 50002, 3); // Red Fox
		place.SpawnSingleMob("Mob2", 50003, 2); // Gray Fox
	}
}
