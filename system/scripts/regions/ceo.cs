//--- Aura Script -----------------------------------------------------------
// Ceo Island (56)
//--- Description -----------------------------------------------------------
// Warp and spawn definitions for Ceo.
//---------------------------------------------------------------------------

public class CeoRegionScript : RegionScript
{
	public override void LoadProperties()
	{
		SetProperty(56, "GuildStonesDisabled", true);
		SetProperty(56, "PvpDisabled", true);
	}

	public override void LoadWarps()
	{
		// Ceo Cellar
		SetPropBehavior(0x00A0003800020069, PropWarp(56, 18800, 16600, 68, 5600, 4284));
		SetPropBehavior(0x00A0004400000001, PropWarp(68, 5616, 3885, 56, 18799, 15876));
	}

	public override void LoadSpawns()
	{
		// Center
		CreateSpawner(race: 130006, amount: 50, region: 56, delay: 120, delayMin: 10, delayMax: 20, coordinates: A(20463, 9178, 20463, 17283, 28986, 17283, 28986, 9178)); // Golem

		// East
		CreateSpawner(race: 80201, amount: 1, region: 56, coordinates: A(29159, 7224, 29159, 9403, 31423, 9403, 31423, 7224)); // Lightning Sprite
		CreateSpawner(race: 80202, amount: 1, region: 56, coordinates: A(29159, 7224, 29159, 9403, 31423, 9403, 31423, 7224)); // Fire Sprite
		CreateSpawner(race: 80203, amount: 1, region: 56, coordinates: A(29159, 7224, 29159, 9403, 31423, 9403, 31423, 7224)); // Ice Sprite
	}
}
