//--- Aura Script -----------------------------------------------------------
// Tir Another World(35)
//--- Description -----------------------------------------------------------
// Warp and spawn definitions for Tir Another World.
//---------------------------------------------------------------------------

public class TirAnotherWorldArenaRegionScript : RegionScript
{
	public override void LoadWarps()
	{
		// Albey Dungeon Entrance
		SetPropBehavior(0x00A000230009002F, PropWarp(35, 9280, 58440, 44, 3210, 2278));
		SetPropBehavior(0x00A0002C00010003, PropWarp(44, 3210, 2278, 35, 9280, 58440));
	}

	public override void LoadSpawns()
	{
		// Graveyard
		CreateSpawner(race: 180001, amount: 8, region: 35, coordinates: A(16115, 40531, 16115, 46119, 19130, 46119, 19130, 40531));  // Male Zombie
		CreateSpawner(race: 180002, amount: 8, region: 35, coordinates: A(16115, 40531, 16115, 46119, 19130, 46119, 19130, 40531));  // Female Zombie
		CreateSpawner(race: 180001, amount: 8, region: 35, coordinates: A(16115, 40531, 16115, 46119, 19130, 46119, 19130, 40531));  // Male Zombie
		CreateSpawner(race: 180002, amount: 8, region: 35, coordinates: A(16115, 40531, 16115, 46119, 19130, 46119, 19130, 40531));  // Female Zombie

		// Near Graveyard
		CreateSpawner(race: 20401, amount: 8, region: 35, delay: 100, delayMin: 10, delayMax: 20, coordinates: A(19167, 39826, 21138, 38744, 19281, 35358, 17310, 36439)); // Timber Wolf

		// Near Healer
		CreateSpawner(race: 20401, amount: 8, region: 35, delay: 100, delayMin: 10, delayMax: 20, coordinates: A(9938, 43120, 9923, 47580, 13341, 47591, 13356, 43131)); // Timber Wolf
		CreateSpawner(race: 150001, amount: 1, region: 35, delay: 1000, delayMin: 10, delayMax: 20, coordinates: A(9938, 43120, 9923, 47580, 13341, 47591, 13356, 43131)); // Giant Worm

		// Near Church
		CreateSpawner(race: 20401, amount: 8, region: 35, delay: 100, delayMin: 10, delayMax: 20, coordinates: A(8280, 40345, 8720, 38521, 6689, 38031, 6249, 39855)); // Timber Wolf
		CreateSpawner(race: 150001, amount: 1, region: 35, delay: 1000, delayMin: 10, delayMax: 20, coordinates: A(8280, 40345, 8720, 38521, 6689, 38031, 6249, 39855)); // Giant Worm

		// Near School
		CreateSpawner(race: 20401, amount: 8, region: 35, delay: 100, delayMin: 10, delayMax: 20, coordinates: A(2252, 27421, 2252, 31750, 7759, 31750, 7759, 27421)); // Timber Wolf
		CreateSpawner(race: 150001, amount: 1, region: 35, delay: 1000, delayMin: 10, delayMax: 20, coordinates: A(2252, 27421, 2252, 31750, 7759, 31750, 7759, 27421)); // Giant Worm

		// Near Alby
		CreateSpawner(race: 20401, amount: 8, region: 35, delay: 100, delayMin: 10, delayMax: 20, coordinates: A(10625, 49354, 8268, 52432, 9059, 53038, 11416, 49960)); // Timber Wolf

		// Inn Road
		CreateSpawner(race: 20401, amount: 3, region: 35, delay: 100, delayMin: 10, delayMax: 20, coordinates: A(12269, 34401, 12621, 34971, 15021, 33490, 14668, 32919)); // Timber Wolf

		// South
		CreateSpawner(race: 20401, amount: 8, region: 35, delay: 100, delayMin: 10, delayMax: 20, coordinates: A(26930, 37599, 21707, 31426, 20500, 32448, 25723, 38621)); // Timber Wolf

		// Southern Plains
		CreateSpawner(race: 20401, amount: 8, region: 35, delay: 100, delayMin: 10, delayMax: 20, coordinates: A(13416, 26080, 14125, 24278, 7143, 21533, 6434, 23335)); // Timber Wolf
		CreateSpawner(race: 150001, amount: 1, region: 35, delay: 1000, delayMin: 10, delayMax: 20, coordinates: A(13416, 26080, 14125, 24278, 7143, 21533, 6434, 23335)); // Giant Worm
	}
}


