//--- Aura Script -----------------------------------------------------------
// Sidhe Sneachta (47/48)
//--- Description -----------------------------------------------------------
// Warp and spawn definitions for Sidhe, two fields north of Tir.
//---------------------------------------------------------------------------

public class SidheRegionScript : RegionScript
{
	public override void LoadProperties()
	{
		SetProperty(47, "Snow", true);
		SetProperty(48, "Snow", true);
	}

	public override void LoadWarps()
	{
		// Tir
		SetPropBehavior(0x00A0000100080067, PropWarp(1, 1516, 59999, 47, 9985, 6522));
		SetPropBehavior(0x00A0002F000100C9, PropWarp(47, 10007, 5680, 1, 1748, 59187));

		// Sidhe North - Sidhe South
		SetPropBehavior(0x00A0002F000100C1, PropWarp(47, 9593, 19201, 48, 12514, 7711));
		SetPropBehavior(0x00A00030000100FD, PropWarp(48, 12802, 6898, 47, 9558, 18288));
	}

	public override void LoadSpawns()
	{
		// Southern Fields
		CreateSpawner(race: 20301, amount: 7, region: 47, delay: 100, delayMin: 10, delayMax: 20, coordinates: A(7535, 7296, 7535, 11284, 12529, 11284, 12529, 7296)); // Coyote
		CreateSpawner(race: 20301, amount: 7, region: 47, delay: 100, delayMin: 10, delayMax: 20, coordinates: A(7151, 14799, 7151, 17485, 12240, 17485, 12240, 14799)); // Coyote
		CreateSpawner(race: 20301, amount: 5, region: 47, delay: 100, delayMin: 10, delayMax: 20, coordinates: A(6379, 11752, 6379, 15262, 13352, 15262, 13352, 11752)); // Coyote

		// Northern Fields
		CreateSpawner(race: 20301, amount: 7, region: 48, delay: 100, delayMin: 10, delayMax: 20, coordinates: A(8714, 15997, 12860, 8391, 11789, 7807, 7642, 15413)); // Coyote
		CreateSpawner(race: 20301, amount: 5, region: 48, delay: 100, delayMin: 10, delayMax: 20, coordinates: A(7498, 15867, 7498, 23639, 8938, 23639, 8938, 15867)); // Coyote
		//CreateSpawner(race: 20301, amount: 5, region: 48, delay: 100, delayMin: 10, delayMax: 20, coordinates: A(7274,23943, 9210,28627, 10612,28047, 8675,23363)); // Coyote disabled on official
	}
}
