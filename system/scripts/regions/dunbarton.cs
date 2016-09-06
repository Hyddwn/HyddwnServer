//--- Aura Script -----------------------------------------------------------
// Dunbarton (16)
//--- Description -----------------------------------------------------------
// Warp and spawn definitions for Dunbarton.
//---------------------------------------------------------------------------

public class DunbartonRegionScript : RegionScript
{
	public override void LoadWarps()
	{
		// Dugald
		SetPropBehavior(0x00A0000E00030010, PropWarp(14,44774,72731, 16,19876,6724));
		SetPropBehavior(0x00A0001000070012, PropWarp(16,19802,3900, 14,44782,70919));

		// Clothing Shop
		SetPropBehavior(0x00A0000E000A0206, PropWarp(14,35110,38777, 17,1427,1365));
		SetPropBehavior(0x00A0001100010002, PropWarp(17,1600,1377, 14,35263,38723));

		// Healer
		SetPropBehavior(0x00A0000E000A0101, PropWarp(14,43531,33092, 19,1120,1048));
		SetPropBehavior(0x00A0001300010002, PropWarp(19,1328,1310, 14,43761,33283));

		// Bank
		SetPropBehavior(0x00A0000E000A0078, PropWarp(14,35735,37455, 20,1590,847));
		SetPropBehavior(0x00A0001400010002, PropWarp(20,1754,843, 14,36037,37442));

		// Church
		SetPropBehavior(0x00A0000E000A007A, PropWarp(14,34394,43229, 21,2171,914));
		SetPropBehavior(0x00A0001500010002, PropWarp(21,2369,701, 14,34593,43030));

		// Gairech
		SetPropBehavior(0x00A0000E00050014, PropWarp(14,58489,1403, 30,31718,98421));
		SetPropBehavior(0x00A0001E00030010, PropWarp(30,31372,100353, 14,58539,2418));

		// School
		SetPropBehavior(0x00A0000E000A01DB, PropWarp(14,45565,40000, 71,9111,9613));
		SetPropBehavior(0x00A0004700000001, PropWarp(71,8934,9610, 14,44615,39995));
		
		// School
		SetPropBehavior(0x00A0001200010001, PropWarp(18,2213,2015, 71,10337,8150));
		SetPropBehavior(0x00A0004700000003, PropWarp(71,10330,7921, 18,2180,1843));
		
		// Dunbarton School Library
		SetPropBehavior(0x00A0004700000002, PropWarp(71,10296,10955, 72,10200,7600));
		SetPropBehavior(0x00A0004800000002, PropWarp(72,10200,7600, 71,10383,11058));

		// Math
		SetPropBehavior(0x00A0000E00090011, PropWarp(14,58396,59080, 25,3233,2484));
		SetPropBehavior(0x00A0001900010002, PropWarp(25,3201,1992, 14,58399,58590));

		// Rabbie
		SetPropBehavior(0x00A0000E00110015, PropWarp(14,16807,59871, 24,3202,2541));
		SetPropBehavior(0x00A0001800010002, PropWarp(24,3200,1998, 14,16801,59368));
		
		// Rabbie Battle Arena - Rabbie Battle Arena Lobby
		SetPropBehavior(0x00A000610000000E, PropWarp(97,3432,7384, 98,1981,4260));
		SetPropBehavior(0x00A0006200010002, PropWarp(98,2000,2000, 24,3200,2833));

		// Rabbie Battle Arena - Rabbie Battle Arena Lobby
		SetPropBehavior(0x00A000610000000F, PropWarp(97,6956,7378, 98,1981,4260));

		// Rabbie Battle Arena - Rabbie Battle Arena Lobby
		SetPropBehavior(0x00A0006100000010, PropWarp(97,6969,3837, 98,1981,4260));

		// Rabbie Battle Arena - Rabbie Battle Arena Lobby
		SetPropBehavior(0x00A0006100000011, PropWarp(97,3429,3835, 98,1981,4260));
		
		// Rabbie Battle Arena Waiting - Rabbie Battle Arena Lobby
		SetPropBehavior(0x00A0006300000002, PropWarp(99,2504,2800, 98,1981,4260));
	}
	
	public override void LoadSpawns()
	{
		// West
		CreateSpawner(race: 40001, amount: 12, region: 14, coordinates: A(22854, 31209, 22854, 45691, 31613, 45691, 31613, 31209)); // Sheep
		CreateSpawner(race: 30014, amount: 10, region: 14, coordinates: A(22854, 31209, 22854, 45691, 31613, 45691, 31613, 31209)); // White Spider
		CreateSpawner(race: 30003, amount: 2, region: 14, coordinates: A(22854, 31209, 22854, 45691, 31613, 45691, 31613, 31209)); // Red Spider
		CreateSpawner(race: 20101, amount: 1, region: 14, coordinates: A(22854, 31209, 22854, 45691, 31613, 45691, 31613, 31209)); // Dog
		CreateSpawner(race: 20001, amount: 1, region: 14, delay: 900, delayMin: 10, delayMax: 20, coordinates: A(22854, 31209, 22854, 45691, 31613, 45691, 31613, 31209)); // Gray Wolf

		// North, potato fields
		CreateSpawner(race: 50001, amount: 10, region: 14, coordinates: A(41252, 48378, 41252, 54805, 49816, 54805, 49816, 48378)); // Brown Fox
		CreateSpawner(race: 60002, amount: 1, region: 14, coordinates: A(41252, 48378, 41252, 54805, 49816, 54805, 49816, 48378)); // Cock
		CreateSpawner(race: 60003, amount: 12, region: 14, coordinates: A(41252, 48378, 41252, 54805, 49816, 54805, 49816, 48378)); // Hen
		CreateSpawner(race: 60004, amount: 1, region: 14, coordinates: A(41252, 48378, 41252, 54805, 49816, 54805, 49816, 48378)); // Chicken

		// East
		CreateSpawner(race: 80101, amount: 1, region: 14, delay: 300, delayMin: 10, delayMax: 20, coordinates: A(53456, 43349, 53456, 49311, 59378, 49311, 59378, 43349)); // Will O' The Wisp
		CreateSpawner(race: 70005, amount: 1, region: 14, delay: 300, delayMin: 10, delayMax: 20, coordinates: A(53456, 43349, 53456, 49311, 59378, 49311, 59378, 43349)); // Red Grizzly Bear
		CreateSpawner(race: 70008, amount: 1, region: 14, delay: 300, delayMin: 10, delayMax: 20, coordinates: A(53456, 43349, 53456, 49311, 59378, 49311, 59378, 43349)); // Little Red Grizzly Bear
		CreateSpawner(race: 30014, amount: 5, region: 14, coordinates: A(53456, 43349, 53456, 49311, 59378, 49311, 59378, 43349)); // White Spider
		CreateSpawner(race: 70004, amount: 1, region: 14, delay: 400, delayMin: 10, delayMax: 20, coordinates: A(53456, 43349, 53456, 49311, 59378, 49311, 59378, 43349)); // Brown Grizzly Bear

		// South
		CreateSpawner(race: 120003, amount: 10, region: 14, coordinates: A(40982, 25298, 40982, 30553, 47415, 30553, 47415, 25298)); // Gray Town Rat
		CreateSpawner(race: 110002, amount: 1, region: 14, coordinates: A(40982, 25298, 40982, 30553, 47415, 30553, 47415, 25298)); // Cow
		CreateSpawner(race: 20005, amount: 1, region: 14, delay: 100, delayMin: 10, delayMax: 20, coordinates: A(40982, 25298, 40982, 30553, 47415, 30553, 47415, 25298)); // Brown Dire Wolf
		CreateSpawner(race: 20009, amount: 1, region: 14, coordinates: A(40982, 25298, 40982, 30553, 47415, 30553, 47415, 25298)); // Little Brown Dire Wolf

		// South West
		CreateSpawner(race: 110002, amount: 10, region: 14, coordinates: A(25803, 30136, 26445, 31916, 38263, 27650, 37621, 25870)); // Cow
		CreateSpawner(race: 20101, amount: 1, region: 14, coordinates: A(25803, 30136, 26445, 31916, 38263, 27650, 37621, 25870)); // Dog
		CreateSpawner(race: 20001, amount: 1, region: 14, delay: 900, delayMin: 10, delayMax: 20, coordinates: A(25803, 30136, 26445, 31916, 38263, 27650, 37621, 25870)); // Gray Wolf

		// South West, bears
		CreateSpawner(race: 80101, amount: 1, region: 14, delay: 300, delayMin: 10, delayMax: 20, coordinates: A(18055, 19837, 21674, 28646, 26784, 26547, 23165, 17738)); // Will O' The Wisp
		CreateSpawner(race: 70004, amount: 3, region: 14, delay: 300, delayMin: 10, delayMax: 20, coordinates: A(18055, 19837, 21674, 28646, 26784, 26547, 23165, 17738)); // Brown Grizzly Bear
		CreateSpawner(race: 70007, amount: 3, region: 14, delay: 300, delayMin: 10, delayMax: 20, coordinates: A(18055, 19837, 21674, 28646, 26784, 26547, 23165, 17738)); // Little Brown Grizzly Bear
		CreateSpawner(race: 70002, amount: 1, region: 14, delay: 300, delayMin: 10, delayMax: 20, coordinates: A(18055, 19837, 21674, 28646, 26784, 26547, 23165, 17738)); // Red Bear
		CreateSpawner(race: 70005, amount: 2, region: 14, delay: 300, delayMin: 10, delayMax: 20, coordinates: A(18055, 19837, 21674, 28646, 26784, 26547, 23165, 17738)); // Red Grizzly Bear
		CreateSpawner(race: 70008, amount: 2, region: 14, delay: 300, delayMin: 10, delayMax: 20, coordinates: A(18055, 19837, 21674, 28646, 26784, 26547, 23165, 17738)); // Little Red Grizzly Bear

		// South, bears
		CreateSpawner(race: 70006, amount: 2, region: 14, delay: 300, delayMin: 10, delayMax: 20, coordinates: A(34139, 19005, 35274, 24817, 46402, 22644, 45267, 16831)); // Black Grizzly Bear
		CreateSpawner(race: 70009, amount: 2, region: 14, delay: 300, delayMin: 10, delayMax: 20, coordinates: A(34139, 19005, 35274, 24817, 46402, 22644, 45267, 16831)); // Little Black Grizzly Bear
		CreateSpawner(race: 70004, amount: 2, region: 14, delay: 300, delayMin: 10, delayMax: 20, coordinates: A(34139, 19005, 35274, 24817, 46402, 22644, 45267, 16831)); // Brown Grizzly Bear
		CreateSpawner(race: 70007, amount: 2, region: 14, delay: 300, delayMin: 10, delayMax: 20, coordinates: A(34139, 19005, 35274, 24817, 46402, 22644, 45267, 16831)); // Little Brown Grizzly Bear

		// South East, path to Gairech
		CreateSpawner(race: 50003, amount: 2, region: 14, delay: 300, delayMin: 10, delayMax: 20, coordinates: A(56305, 3579, 53783, 19181, 56999, 19701, 59521, 4099)); // Gray Fox
		CreateSpawner(race: 70004, amount: 1, region: 14, delay: 400, delayMin: 10, delayMax: 20, coordinates: A(56305, 3579, 53783, 19181, 56999, 19701, 59521, 4099)); // Brown Grizzly Bear
		CreateSpawner(race: 50001, amount: 2, region: 14, coordinates: A(56305, 3579, 53783, 19181, 56999, 19701, 59521, 4099)); // Brown Fox
		CreateSpawner(race: 50101, amount: 1, region: 14, coordinates: A(56305, 3579, 53783, 19181, 56999, 19701, 59521, 4099)); // Raccoon

		// East, potato fields
		CreateSpawner(race: 60002, amount: 5, region: 14, coordinates: A(47576, 32078, 47576, 41177, 52976, 41177, 52976, 32078)); // Cock
		CreateSpawner(race: 60003, amount: 20, region: 14, coordinates: A(47576, 32078, 47576, 41177, 52976, 41177, 52976, 32078)); // Hen
		CreateSpawner(race: 60004, amount: 5, region: 14, coordinates: A(47576, 32078, 47576, 41177, 52976, 41177, 52976, 32078)); // Chicken

		// North West
		CreateSpawner(race: 70006, amount: 1, region: 14, delay: 1000, delayMin: 10, delayMax: 20, coordinates: A(27727, 54510, 27727, 59745, 36588, 59745, 36588, 54510)); // Black Grizzly Bear
		CreateSpawner(race: 70009, amount: 1, region: 14, delay: 300, delayMin: 10, delayMax: 20, coordinates: A(27727, 54510, 27727, 59745, 36588, 59745, 36588, 54510)); // Little Black Grizzly Bear
		CreateSpawner(race: 70004, amount: 1, region: 14, delay: 400, delayMin: 10, delayMax: 20, coordinates: A(27727, 54510, 27727, 59745, 36588, 59745, 36588, 54510)); // Brown Grizzly Bear
		CreateSpawner(race: 70007, amount: 1, region: 14, delay: 300, delayMin: 10, delayMax: 20, coordinates: A(27727, 54510, 27727, 59745, 36588, 59745, 36588, 54510)); // Little Brown Grizzly Bear
		CreateSpawner(race: 50002, amount: 2, region: 14, coordinates: A(27727, 54510, 27727, 59745, 36588, 59745, 36588, 54510)); // Red Fox
		CreateSpawner(race: 50005, amount: 1, region: 14, coordinates: A(27727, 54510, 27727, 59745, 36588, 59745, 36588, 54510)); // Little Red Fox
		CreateSpawner(race: 20001, amount: 1, region: 14, coordinates: A(27727, 54510, 27727, 59745, 36588, 59745, 36588, 54510)); // Gray Wolf
		CreateSpawner(race: 20005, amount: 1, region: 14, coordinates: A(27727, 54510, 27727, 59745, 36588, 59745, 36588, 54510)); // Brown Dire Wolf
		CreateSpawner(race: 10601, amount: 1, region: 14, delay: 500, delayMin: 10, delayMax: 20, coordinates: A(27727, 54510, 27727, 59745, 36588, 59745, 36588, 54510)); // Imp

		// North East
		CreateSpawner(race: 70006, amount: 1, region: 14, delay: 1000, delayMin: 10, delayMax: 20, coordinates: A(52767, 58017, 48746, 54015, 44436, 58345, 48457, 62347)); // Black Grizzly Bear
		CreateSpawner(race: 70009, amount: 1, region: 14, delay: 300, delayMin: 10, delayMax: 20, coordinates: A(52767, 58017, 48746, 54015, 44436, 58345, 48457, 62347)); // Little Black Grizzly Bear
		CreateSpawner(race: 70004, amount: 1, region: 14, delay: 400, delayMin: 10, delayMax: 20, coordinates: A(52767, 58017, 48746, 54015, 44436, 58345, 48457, 62347)); // Brown Grizzly Bear
		CreateSpawner(race: 70007, amount: 1, region: 14, delay: 300, delayMin: 10, delayMax: 20, coordinates: A(52767, 58017, 48746, 54015, 44436, 58345, 48457, 62347)); // Little Brown Grizzly Bear
		CreateSpawner(race: 50002, amount: 2, region: 14, coordinates: A(52767, 58017, 48746, 54015, 44436, 58345, 48457, 62347)); // Red Fox
		CreateSpawner(race: 50005, amount: 1, region: 14, coordinates: A(52767, 58017, 48746, 54015, 44436, 58345, 48457, 62347)); // Little Red Fox
		CreateSpawner(race: 20001, amount: 1, region: 14, coordinates: A(52767, 58017, 48746, 54015, 44436, 58345, 48457, 62347)); // Gray Wolf
		CreateSpawner(race: 20005, amount: 1, region: 14, coordinates: A(52767, 58017, 48746, 54015, 44436, 58345, 48457, 62347)); // Brown Dire Wolf
		CreateSpawner(race: 10206, amount: 1, region: 14, delay: 500, delayMin: 10, delayMax: 20, coordinates: A(52767, 58017, 48746, 54015, 44436, 58345, 48457, 62347)); // Kobold Bandit

		// West, bears
		CreateSpawner(race: 70002, amount: 2, region: 14, delay: 300, delayMin: 10, delayMax: 20, coordinates: A(21942, 38999, 21942, 49905, 26152, 49905, 26152, 38999)); // Red Bear
	}
}
