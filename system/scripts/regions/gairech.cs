//--- Aura Script -----------------------------------------------------------
// Gairech (30)
//--- Description -----------------------------------------------------------
// Warp and spawn definitions for Gairech.
// Region between Dunbarton and Bangor.
//---------------------------------------------------------------------------

public class GairechRegionScript : RegionScript
{
	public override void LoadWarps()
	{
		// Gairech - Bangor
		SetPropBehavior(0x00A0001E00080019, PropWarp(30, 39171, 16618, 31, 13083, 23128));
		SetPropBehavior(0x00A0001F00010014, PropWarp(31, 13103, 24027, 30, 39167, 17906));

		// Gairech - Fiodh Altar
		SetPropBehavior(0x00A0001E00050039, PropWarp(30, 10705, 83742, 49, 3516, 5317));
		SetPropBehavior(0x00A0003100000003, PropWarp(49, 3454, 4430, 30, 10707, 82575));

		// Gairech - Sen Mag
		SetPropBehavior(0x00A0001E00050060, PropWarp(30,8230,72405, 53,137680,121906));
		SetPropBehavior(0x00A0003500030005, PropWarp(53, 139366, 121883, 30, 9825, 72620));
	}

	public override void LoadSpawns()
	{
		// North-East top
		CreateSpawner(race: 20005, amount: 1, region: 30, delay: 100, delayMin: 10, delayMax: 20, coordinates: A(29368, 85327, 29511, 96434, 35235, 96360, 35091, 85253)); // Brown Dire Wolf
		CreateSpawner(race: 20008, amount: 1, region: 30, coordinates: A(29368, 85327, 29511, 96434, 35235, 96360, 35091, 85253)); // White Dire Wolf
		CreateSpawner(race: 20009, amount: 1, region: 30, coordinates: A(29368, 85327, 29511, 96434, 35235, 96360, 35091, 85253)); // Brown Dire Wolf Cub
		CreateSpawner(race: 20012, amount: 1, region: 30, coordinates: A(29368, 85327, 29511, 96434, 35235, 96360, 35091, 85253)); // White Dire Wolf Cub
		CreateSpawner(race: 50002, amount: 1, region: 30, coordinates: A(29368, 85327, 29511, 96434, 35235, 96360, 35091, 85253)); // Red Fox
		CreateSpawner(race: 50003, amount: 1, region: 30, coordinates: A(29368, 85327, 29511, 96434, 35235, 96360, 35091, 85253)); // Gray Fox
		CreateSpawner(race: 50005, amount: 1, region: 30, coordinates: A(29368, 85327, 29511, 96434, 35235, 96360, 35091, 85253)); // Young Red Fox
		CreateSpawner(race: 50103, amount: 1, region: 30, coordinates: A(29368, 85327, 29511, 96434, 35235, 96360, 35091, 85253)); // Young Gray Raccoon
		CreateSpawner(race: 70002, amount: 1, region: 30, delay: 100, delayMin: 10, delayMax: 20, coordinates: A(29368, 85327, 29511, 96434, 35235, 96360, 35091, 85253)); // Red Bear
		CreateSpawner(race: 70004, amount: 1, region: 30, delay: 300, delayMin: 10, delayMax: 20, coordinates: A(29368, 85327, 29511, 96434, 35235, 96360, 35091, 85253)); // Brown Grizzly Bear
		CreateSpawner(race: 70006, amount: 1, region: 30, delay: 300, delayMin: 10, delayMax: 20, coordinates: A(29368, 85327, 29511, 96434, 35235, 96360, 35091, 85253)); // Black Grizzly Bear
		CreateSpawner(race: 70007, amount: 1, region: 30, delay: 200, delayMin: 10, delayMax: 20, coordinates: A(29368, 85327, 29511, 96434, 35235, 96360, 35091, 85253)); // Brown Grizzly Bear Cub
		CreateSpawner(race: 80101, amount: 1, region: 30, delay: 300, delayMin: 10, delayMax: 20, coordinates: A(29368, 85327, 29511, 96434, 35235, 96360, 35091, 85253)); // Wisp

		// North-West top
		CreateSpawner(race: 20005, amount: 1, region: 30, delay: 100, delayMin: 10, delayMax: 20, coordinates: A(14222, 78478, 14222, 85558, 21876, 85558, 21876, 78478)); // Brown Dire Wolf
		CreateSpawner(race: 20008, amount: 1, region: 30, coordinates: A(14222, 78478, 14222, 85558, 21876, 85558, 21876, 78478)); // White Dire Wolf
		CreateSpawner(race: 20009, amount: 1, region: 30, coordinates: A(14222, 78478, 14222, 85558, 21876, 85558, 21876, 78478)); // Brown Dire Wolf Cub
		CreateSpawner(race: 20012, amount: 1, region: 30, coordinates: A(14222, 78478, 14222, 85558, 21876, 85558, 21876, 78478)); // White Dire Wolf Cub
		CreateSpawner(race: 50002, amount: 1, region: 30, coordinates: A(14222, 78478, 14222, 85558, 21876, 85558, 21876, 78478)); // Red Fox
		CreateSpawner(race: 50003, amount: 1, region: 30, coordinates: A(14222, 78478, 14222, 85558, 21876, 85558, 21876, 78478)); // Gray Fox
		CreateSpawner(race: 50005, amount: 1, region: 30, coordinates: A(14222, 78478, 14222, 85558, 21876, 85558, 21876, 78478)); // Young Red Fox
		CreateSpawner(race: 50103, amount: 1, region: 30, coordinates: A(14222, 78478, 14222, 85558, 21876, 85558, 21876, 78478)); // Young Gray Raccoon
		CreateSpawner(race: 70002, amount: 1, region: 30, delay: 100, delayMin: 10, delayMax: 20, coordinates: A(14222, 78478, 14222, 85558, 21876, 85558, 21876, 78478)); // Red Bear
		CreateSpawner(race: 70004, amount: 1, region: 30, delay: 300, delayMin: 10, delayMax: 20, coordinates: A(14222, 78478, 14222, 85558, 21876, 85558, 21876, 78478)); // Brown Grizzly Bear
		CreateSpawner(race: 70006, amount: 1, region: 30, delay: 300, delayMin: 10, delayMax: 20, coordinates: A(14222, 78478, 14222, 85558, 21876, 85558, 21876, 78478)); // Black Grizzly Bear
		CreateSpawner(race: 70007, amount: 1, region: 30, delay: 200, delayMin: 10, delayMax: 20, coordinates: A(14222, 78478, 14222, 85558, 21876, 85558, 21876, 78478)); // Brown Grizzly Bear Cub
		CreateSpawner(race: 80101, amount: 1, region: 30, delay: 300, delayMin: 10, delayMax: 20, coordinates: A(14222, 78478, 14222, 85558, 21876, 85558, 21876, 78478)); // Wisp

		// North-West bottom
		CreateSpawner(race: 20005, amount: 1, region: 30, delay: 100, delayMin: 10, delayMax: 20, coordinates: A(12566, 67949, 12566, 73085, 24321, 73085, 24321, 67949)); // Brown Dire Wolf
		CreateSpawner(race: 20008, amount: 1, region: 30, coordinates: A(12566, 67949, 12566, 73085, 24321, 73085, 24321, 67949)); // White Dire Wolf
		CreateSpawner(race: 20009, amount: 1, region: 30, coordinates: A(12566, 67949, 12566, 73085, 24321, 73085, 24321, 67949)); // Brown Dire Wolf Cub
		CreateSpawner(race: 20012, amount: 1, region: 30, coordinates: A(12566, 67949, 12566, 73085, 24321, 73085, 24321, 67949)); // White Dire Wolf Cub
		CreateSpawner(race: 50002, amount: 1, region: 30, coordinates: A(12566, 67949, 12566, 73085, 24321, 73085, 24321, 67949)); // Red Fox
		CreateSpawner(race: 50003, amount: 1, region: 30, coordinates: A(12566, 67949, 12566, 73085, 24321, 73085, 24321, 67949)); // Gray Fox
		CreateSpawner(race: 50005, amount: 1, region: 30, coordinates: A(12566, 67949, 12566, 73085, 24321, 73085, 24321, 67949)); // Young Red Fox
		CreateSpawner(race: 50103, amount: 1, region: 30, coordinates: A(12566, 67949, 12566, 73085, 24321, 73085, 24321, 67949)); // Young Gray Raccoon
		CreateSpawner(race: 70002, amount: 1, region: 30, delay: 100, delayMin: 10, delayMax: 20, coordinates: A(12566, 67949, 12566, 73085, 24321, 73085, 24321, 67949)); // Red Bear
		CreateSpawner(race: 70004, amount: 1, region: 30, delay: 300, delayMin: 10, delayMax: 20, coordinates: A(12566, 67949, 12566, 73085, 24321, 73085, 24321, 67949)); // Brown Grizzly Bear
		CreateSpawner(race: 70006, amount: 1, region: 30, delay: 300, delayMin: 10, delayMax: 20, coordinates: A(12566, 67949, 12566, 73085, 24321, 73085, 24321, 67949)); // Black Grizzly Bear
		CreateSpawner(race: 70007, amount: 1, region: 30, delay: 200, delayMin: 10, delayMax: 20, coordinates: A(12566, 67949, 12566, 73085, 24321, 73085, 24321, 67949)); // Brown Grizzly Bear Cub
		CreateSpawner(race: 80101, amount: 1, region: 30, delay: 300, delayMin: 10, delayMax: 20, coordinates: A(12566, 67949, 12566, 73085, 24321, 73085, 24321, 67949)); // Wisp
		CreateSpawner(race: 100001, amount: 1, region: 30, delay: 500, delayMin: 10, delayMax: 20, coordinates: A(12566, 67949, 12566, 73085, 24321, 73085, 24321, 67949)); // Ogre

		// North middle
		CreateSpawner(race: 20005, amount: 1, region: 30, delay: 100, delayMin: 10, delayMax: 20, coordinates: A(22604, 75129, 30934, 82622, 34536, 78618, 26206, 71125)); // Brown Dire Wolf
		CreateSpawner(race: 20008, amount: 1, region: 30, coordinates: A(22604, 75129, 30934, 82622, 34536, 78618, 26206, 71125)); // White Dire Wolf
		CreateSpawner(race: 20009, amount: 1, region: 30, coordinates: A(22604, 75129, 30934, 82622, 34536, 78618, 26206, 71125)); // Brown Dire Wolf Cub
		CreateSpawner(race: 20012, amount: 1, region: 30, coordinates: A(22604, 75129, 30934, 82622, 34536, 78618, 26206, 71125)); // White Dire Wolf Cub
		CreateSpawner(race: 50002, amount: 1, region: 30, coordinates: A(22604, 75129, 30934, 82622, 34536, 78618, 26206, 71125)); // Red Fox
		CreateSpawner(race: 50003, amount: 1, region: 30, coordinates: A(22604, 75129, 30934, 82622, 34536, 78618, 26206, 71125)); // Gray Fox
		CreateSpawner(race: 50005, amount: 1, region: 30, coordinates: A(22604, 75129, 30934, 82622, 34536, 78618, 26206, 71125)); // Young Red Fox
		CreateSpawner(race: 50103, amount: 1, region: 30, coordinates: A(22604, 75129, 30934, 82622, 34536, 78618, 26206, 71125)); // Young Gray Raccoon
		CreateSpawner(race: 70002, amount: 1, region: 30, delay: 100, delayMin: 10, delayMax: 20, coordinates: A(22604, 75129, 30934, 82622, 34536, 78618, 26206, 71125)); // Red Bear
		CreateSpawner(race: 70004, amount: 1, region: 30, delay: 300, delayMin: 10, delayMax: 20, coordinates: A(22604, 75129, 30934, 82622, 34536, 78618, 26206, 71125)); // Brown Grizzly Bear
		CreateSpawner(race: 70006, amount: 1, region: 30, delay: 300, delayMin: 10, delayMax: 20, coordinates: A(22604, 75129, 30934, 82622, 34536, 78618, 26206, 71125)); // Black Grizzly Bear
		CreateSpawner(race: 70007, amount: 1, region: 30, delay: 200, delayMin: 10, delayMax: 20, coordinates: A(22604, 75129, 30934, 82622, 34536, 78618, 26206, 71125)); // Brown Grizzly Bear Cub
		CreateSpawner(race: 80101, amount: 1, region: 30, delay: 300, delayMin: 10, delayMax: 20, coordinates: A(22604, 75129, 30934, 82622, 34536, 78618, 26206, 71125)); // Wisp

		// North-East bottom
		CreateSpawner(race: 20005, amount: 1, region: 30, delay: 100, delayMin: 10, delayMax: 20, coordinates: A(37973, 76264, 34328, 86597, 38306, 88000, 41952, 77668)); // Brown Dire Wolf
		CreateSpawner(race: 20008, amount: 1, region: 30, coordinates: A(37973, 76264, 34328, 86597, 38306, 88000, 41952, 77668)); // White Dire Wolf
		CreateSpawner(race: 20009, amount: 1, region: 30, coordinates: A(37973, 76264, 34328, 86597, 38306, 88000, 41952, 77668)); // Brown Dire Wolf Cub
		CreateSpawner(race: 20012, amount: 1, region: 30, coordinates: A(37973, 76264, 34328, 86597, 38306, 88000, 41952, 77668)); // White Dire Wolf Cub
		CreateSpawner(race: 50002, amount: 1, region: 30, coordinates: A(37973, 76264, 34328, 86597, 38306, 88000, 41952, 77668)); // Red Fox
		CreateSpawner(race: 50003, amount: 1, region: 30, coordinates: A(37973, 76264, 34328, 86597, 38306, 88000, 41952, 77668)); // Gray Fox
		CreateSpawner(race: 50005, amount: 1, region: 30, coordinates: A(37973, 76264, 34328, 86597, 38306, 88000, 41952, 77668)); // Young Red Fox
		CreateSpawner(race: 50103, amount: 1, region: 30, coordinates: A(37973, 76264, 34328, 86597, 38306, 88000, 41952, 77668)); // Young Gray Raccoon
		CreateSpawner(race: 70002, amount: 1, region: 30, delay: 100, delayMin: 10, delayMax: 20, coordinates: A(37973, 76264, 34328, 86597, 38306, 88000, 41952, 77668)); // Red Bear
		CreateSpawner(race: 70004, amount: 1, region: 30, delay: 300, delayMin: 10, delayMax: 20, coordinates: A(37973, 76264, 34328, 86597, 38306, 88000, 41952, 77668)); // Brown Grizzly Bear
		CreateSpawner(race: 70006, amount: 1, region: 30, delay: 300, delayMin: 10, delayMax: 20, coordinates: A(37973, 76264, 34328, 86597, 38306, 88000, 41952, 77668)); // Black Grizzly Bear
		CreateSpawner(race: 70007, amount: 1, region: 30, delay: 200, delayMin: 10, delayMax: 20, coordinates: A(37973, 76264, 34328, 86597, 38306, 88000, 41952, 77668)); // Brown Grizzly Bear Cub
		CreateSpawner(race: 80101, amount: 1, region: 30, delay: 300, delayMin: 10, delayMax: 20, coordinates: A(37973, 76264, 34328, 86597, 38306, 88000, 41952, 77668)); // Wisp

		// Dragon Ruins North
		CreateSpawner(race: 20005, amount: 1, region: 30, delay: 100, delayMin: 10, delayMax: 20, coordinates: A(30607, 56455, 30607, 61153, 45986, 61153, 45986, 56455)); // Brown Dire Wolf
		CreateSpawner(race: 20008, amount: 1, region: 30, coordinates: A(30607, 56455, 30607, 61153, 45986, 61153, 45986, 56455)); // White Dire Wolf
		CreateSpawner(race: 20009, amount: 1, region: 30, coordinates: A(30607, 56455, 30607, 61153, 45986, 61153, 45986, 56455)); // Brown Dire Wolf Cub
		CreateSpawner(race: 20012, amount: 1, region: 30, coordinates: A(30607, 56455, 30607, 61153, 45986, 61153, 45986, 56455)); // White Dire Wolf Cub
		CreateSpawner(race: 50002, amount: 1, region: 30, coordinates: A(30607, 56455, 30607, 61153, 45986, 61153, 45986, 56455)); // Red Fox
		CreateSpawner(race: 50003, amount: 1, region: 30, coordinates: A(30607, 56455, 30607, 61153, 45986, 61153, 45986, 56455)); // Gray Fox
		CreateSpawner(race: 50005, amount: 1, region: 30, coordinates: A(30607, 56455, 30607, 61153, 45986, 61153, 45986, 56455)); // Young Red Fox
		CreateSpawner(race: 50103, amount: 1, region: 30, coordinates: A(30607, 56455, 30607, 61153, 45986, 61153, 45986, 56455)); // Young Gray Raccoon
		CreateSpawner(race: 70002, amount: 1, region: 30, delay: 100, delayMin: 10, delayMax: 20, coordinates: A(30607, 56455, 30607, 61153, 45986, 61153, 45986, 56455)); // Red Bear
		CreateSpawner(race: 70004, amount: 1, region: 30, delay: 300, delayMin: 10, delayMax: 20, coordinates: A(30607, 56455, 30607, 61153, 45986, 61153, 45986, 56455)); // Brown Grizzly Bear
		CreateSpawner(race: 70006, amount: 1, region: 30, delay: 300, delayMin: 10, delayMax: 20, coordinates: A(30607, 56455, 30607, 61153, 45986, 61153, 45986, 56455)); // Black Grizzly Bear
		CreateSpawner(race: 70007, amount: 1, region: 30, delay: 200, delayMin: 10, delayMax: 20, coordinates: A(30607, 56455, 30607, 61153, 45986, 61153, 45986, 56455)); // Brown Grizzly Bear Cub
		CreateSpawner(race: 80101, amount: 1, region: 30, delay: 300, delayMin: 10, delayMax: 20, coordinates: A(30607, 56455, 30607, 61153, 45986, 61153, 45986, 56455)); // Wisp

		// Dragon Ruins South-West
		CreateSpawner(race: 11001, amount: 4, region: 30, delay: 300, delayMin: 10, delayMax: 20, coordinates: A(32479, 40966, 27453, 49275, 31760, 51881, 36787, 43572)); // Skeleton
		CreateSpawner(race: 20004, amount: 6, region: 30, delay: 100, delayMin: 10, delayMax: 20, coordinates: A(32479, 40966, 27453, 49275, 31760, 51881, 36787, 43572)); // Skeleton Wolf
		CreateSpawner(race: 20005, amount: 2, region: 30, delay: 100, delayMin: 10, delayMax: 20, coordinates: A(32479, 40966, 27453, 49275, 31760, 51881, 36787, 43572)); // Brown Dire Wolf
		CreateSpawner(race: 20009, amount: 2, region: 30, coordinates: A(32479, 40966, 27453, 49275, 31760, 51881, 36787, 43572)); // Brown Dire Wolf Cub
		CreateSpawner(race: 50003, amount: 2, region: 30, coordinates: A(32479, 40966, 27453, 49275, 31760, 51881, 36787, 43572)); // Gray Fox

		// Dragon Ruins South-East
		CreateSpawner(race: 11001, amount: 4, region: 30, delay: 300, delayMin: 10, delayMax: 20, coordinates: A(40203, 44033, 45537, 52304, 49571, 49702, 44237, 41432)); // Skeleton
		CreateSpawner(race: 20004, amount: 6, region: 30, delay: 100, delayMin: 10, delayMax: 20, coordinates: A(40203, 44033, 45537, 52304, 49571, 49702, 44237, 41432)); // Skeleton Wolf
		CreateSpawner(race: 20005, amount: 2, region: 30, delay: 100, delayMin: 10, delayMax: 20, coordinates: A(40203, 44033, 45537, 52304, 49571, 49702, 44237, 41432)); // Brown Dire Wolf
		CreateSpawner(race: 20009, amount: 2, region: 30, coordinates: A(40203, 44033, 45537, 52304, 49571, 49702, 44237, 41432)); // Brown Dire Wolf Cub
		CreateSpawner(race: 50003, amount: 2, region: 30, coordinates: A(40203, 44033, 45537, 52304, 49571, 49702, 44237, 41432)); // Gray Fox

		// South-West
		CreateSpawner(race: 10206, amount: 4, region: 30, delay: 200, delayMin: 10, delayMax: 20, coordinates: A(35942, 31172, 35942, 40521, 42232, 40521, 42232, 31172)); // Kobold Bandit
		CreateSpawner(race: 20004, amount: 6, region: 30, delay: 100, delayMin: 10, delayMax: 20, coordinates: A(35942, 31172, 35942, 40521, 42232, 40521, 42232, 31172)); // Skeleton Wolf
		CreateSpawner(race: 20005, amount: 2, region: 30, delay: 100, delayMin: 10, delayMax: 20, coordinates: A(35942, 31172, 35942, 40521, 42232, 40521, 42232, 31172)); // Brown Dire Wolf
		CreateSpawner(race: 20009, amount: 2, region: 30, coordinates: A(35942, 31172, 35942, 40521, 42232, 40521, 42232, 31172)); // Brown Dire Wolf Cub
		CreateSpawner(race: 50003, amount: 2, region: 30, coordinates: A(35942, 31172, 35942, 40521, 42232, 40521, 42232, 31172)); // Gray Fox

		// South-East
		CreateSpawner(race: 10206, amount: 5, region: 30, delay: 100, delayMin: 10, delayMax: 20, coordinates: A(35876, 21306, 35876, 30257, 42424, 30257, 42424, 21306)); // Kobold Bandit
		CreateSpawner(race: 20005, amount: 2, region: 30, delay: 100, delayMin: 10, delayMax: 20, coordinates: A(35876, 21306, 35876, 30257, 42424, 30257, 42424, 21306)); // Brown Dire Wolf
		CreateSpawner(race: 20009, amount: 2, region: 30, coordinates: A(35876, 21306, 35876, 30257, 42424, 30257, 42424, 21306)); // Brown Dire Wolf Cub
		CreateSpawner(race: 50003, amount: 2, region: 30, coordinates: A(35876, 21306, 35876, 30257, 42424, 30257, 42424, 21306)); // Gray Fox

		// Reighinalt North-East
		CreateSpawner(race: 10206, amount: 8, region: 30, delay: 150, delayMin: 10, delayMax: 20, coordinates: A(60859, 26229, 53507, 33808, 55933, 36161, 63284, 28582)); // Kobold Bandit
		CreateSpawner(race: 20005, amount: 2, region: 30, delay: 100, delayMin: 10, delayMax: 20, coordinates: A(60859, 26229, 53507, 33808, 55933, 36161, 63284, 28582)); // Brown Dire Wolf
		CreateSpawner(race: 50003, amount: 1, region: 30, coordinates: A(60859, 26229, 53507, 33808, 55933, 36161, 63284, 28582)); // Gray Fox

		// Reighinalt South-East
		CreateSpawner(race: 10206, amount: 10, region: 30, delay: 150, delayMin: 10, delayMax: 20, coordinates: A(55254, 18665, 45982, 32896, 47412, 33828, 56684, 19597)); // Kobold Bandit
		CreateSpawner(race: 50003, amount: 1, region: 30, coordinates: A(55254, 18665, 45982, 32896, 47412, 33828, 56684, 19597)); // Gray Fox

		// Reighinalt South
		CreateSpawner(race: 10206, amount: 10, region: 30, delay: 150, delayMin: 10, delayMax: 20, coordinates: A(54054, 19424, 54054, 24342, 66254, 24342, 66254, 19424)); // Kobold Bandit
		CreateSpawner(race: 10401, amount: 1, region: 30, delay: 500, delayMin: 10, delayMax: 20, coordinates: A(54054, 19424, 54054, 24342, 66254, 24342, 66254, 19424)); // Troll
		CreateSpawner(race: 50003, amount: 2, region: 30, coordinates: A(54054, 19424, 54054, 24342, 66254, 24342, 66254, 19424)); // Gray Fox

		// Black Wiz: 10096 with 8 wisps 80101
		// Giant Ogre: 100023 with 6 Skeleton Soldier 11004
	}
}