//--- Aura Script -----------------------------------------------------------
// Hidden Mine Ore Deposit Room
//--- Description -----------------------------------------------------------
// Spawns ore deposits in a room.
// Note: These Ores have a high chance to drop copper, silver, and gold ores
// and a lower chance to drop iron ore.
//---------------------------------------------------------------------------

[PuzzleScript("collectprop_gold_ore")]
public class CollectPropGoldOreScript : PuzzleScript
{
	public override void OnPrepare(Puzzle puzzle)
	{
		var propPlace = puzzle.NewPlace("PropPlace");
		propPlace.ReservePlace();
		propPlace.ReserveDoors();
	}

	public override void OnPuzzleCreate(Puzzle puzzle)
	{
		var propPlace = puzzle.GetPlace("PropPlace");

		for (int i = 1; i <= 4; ++i)
		{
			var oreDeposit = new OreDeposit(41106, "Deposit" + i);
			propPlace.AddProp(oreDeposit, Placement.Ore);
		}
	}
}