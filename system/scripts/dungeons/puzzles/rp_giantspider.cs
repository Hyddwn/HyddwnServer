//--- Aura Script -----------------------------------------------------------
// Spider and Rat Pit Puzzle
//--- Description -----------------------------------------------------------
// Spawns several rats and spiders in an alley.
//---------------------------------------------------------------------------

[PuzzleScript("rp_giantspider")]
internal class rp_giantspiderPuzzle : PuzzleScript
{
	public override void OnPrepare(Puzzle puzzle)
	{
		var place = puzzle.NewPlace("PuzzlePlace");
		place.ReservePlace();
	}

	public override void OnPuzzleCreate(Puzzle puzzle)
	{
		var place = puzzle.GetPlace("PuzzlePlace");

		place.SpawnSingleMob("Mob1", 120006, 3); // Young Country Rat
		place.SpawnSingleMob("Mob2", 30005, 8);  // White Spiderling
	}
}
