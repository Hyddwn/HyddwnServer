//--- Aura Script -----------------------------------------------------------
// Nekojima
//--- Description -----------------------------------------------------------
// Warp and spawn definitions for Nekojima
// Also includes a global drop for monsters with the /rat/ tag 
// for the cat bell item while in the Nekojima region
//---------------------------------------------------------------------------

public class NekoRegionScript : RegionScript
{
	public override void LoadSpawns()
	{
	//Nezumijima spawn group 1
	CreateSpawner(race: 120002, amount: 6, region: 600, coordinates: A(88898,72506, 90125,69636, 92898,71522, 91173,73342)); // Brown Town Rat
	CreateSpawner(race: 120003, amount: 4, region: 600, coordinates: A(88898,72506, 90125,69636, 92898,71522, 91173,73342)); // Grey Town Rat
	CreateSpawner(race: 120004, amount: 5, region: 600, coordinates: A(88898,72506, 90125,69636, 92898,71522, 91173,73342)); // Country Rat
	CreateSpawner(race: 120007, amount: 3, region: 600, coordinates: A(88898,72506, 90125,69636, 92898,71522, 91173,73342)); // Giant Forest Rat
	
	//Nezumijima spawn group 2
	CreateSpawner(race: 170301, amount: 4, region: 600, coordinates: A(92854,72980, 93426,70564, 93788,71873, 93802,72804)); // Ratman
	CreateSpawner(race: 120010, amount: 1, region: 600, coordinates: A(92854,72980, 93426,70564, 93788,71873, 93802,72804)); // Black Ship Rat
	}
}

public class CatsBellDropScript : GeneralScript
{
	[On("CreatureFinished")]
	public void OnCreatureFinished(Creature creature, Creature killer) 
	{ 
		if (creature.RegionId == 600 && creature.HasTag("/rat/"))
		{
			var rnd = RandomProvider.Get();
			var pos = creature.GetPosition();
			if (rnd.NextDouble() * 100 < 4 * ChannelServer.Instance.Conf.World.DropRate)
			{
				var catsBellDropPos = pos.GetRandomInRange(50, rnd);
				var catsBellItem = new Item(91108); //Cat Bell
				catsBellItem.Drop(creature.Region, catsBellDropPos, Item.DropRadius, killer, true);
			}
		}
	}
}

