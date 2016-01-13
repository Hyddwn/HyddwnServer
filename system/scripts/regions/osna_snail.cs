//--- Aura Script -----------------------------------------------------------
// Osna Snail (70)
//--- Description -----------------------------------------------------------
// Warp and spawn definitions for Osna Snail.
// Region between Emain and Dunbarton.
//---------------------------------------------------------------------------

public class OsnaSnailRegionScript : RegionScript
{
	public override void LoadWarps()
	{
		// Dunbarton - Osna Sail
		SetPropBehavior(0x00A0000E0006000F, PropWarp(14, 16050, 33339, 70, 44316, 19980));
		SetPropBehavior(0x00A0004600010001, PropWarp(70, 44700, 20000, 14, 18169, 33718));

		// Emain Macha - Osna Sail
		SetPropBehavior(0x00A00034001700A3, PropWarp(52, 53323, 75511, 70, 6920, 13157));
		SetPropBehavior(0x00A0004600020001, PropWarp(70, 6400, 12800, 52, 52344, 74261));
	}

	public override void LoadSpawns()
	{
		// East
		CreateSpawner(race: 20005, amount: 3, region: 70, coordinates: A(38815, 19369, 41143, 19340, 41139, 19024, 38811, 19053)); // Brown Dire Wolf
		CreateSpawner(race: 20009, amount: 1, region: 70, coordinates: A(38815, 19369, 41143, 19340, 41139, 19024, 38811, 19053)); // Brown Dire Wolf Cub

		// Center 1
		CreateSpawner(race: 20005, amount: 4, region: 70, coordinates: A(31610, 14996, 29225, 14965, 29221, 15288, 31605, 15318)); // Brown Dire Wolf
		CreateSpawner(race: 20009, amount: 1, region: 70, coordinates: A(31610, 14996, 29225, 14965, 29221, 15288, 31605, 15318)); // Brown Dire Wolf Cub

		// Center 2
		CreateSpawner(race: 20005, amount: 4, region: 70, coordinates: A(23255, 16564, 20697, 16594, 20701, 16907, 23259, 16877)); // Brown Dire Wolf

		// West
		CreateSpawner(race: 20005, amount: 2, region: 70, coordinates: A(11651, 13379, 9501, 13430, 9507, 13711, 11658, 13660)); // Brown Dire Wolf
		CreateSpawner(race: 20009, amount: 2, region: 70, coordinates: A(11651, 13379, 9501, 13430, 9507, 13711, 11658, 13660)); // Brown Dire Wolf Cub
	}
}
