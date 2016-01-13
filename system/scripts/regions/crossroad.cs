//--- Aura Script -----------------------------------------------------------
// Crossroad (51)
//--- Description -----------------------------------------------------------
// Crossroad between Tir na nog and Erinn
//---------------------------------------------------------------------------

public class CrossroadRegionScript : RegionScript
{
	public override void LoadWarps()
	{
		// Crossroad - Tir Another World
		SetPropBehavior(0x00A0003300000001, PropWarp(51, 10422, 10959, 35, 12813, 38398));

		// Crossroad - Bangor
		SetPropBehavior(0x00A0003300000002, PropWarp(51, 10422, 10959, 32, 3176, 2514));
	}

	public override void LoadSpawns()
	{
	}
}