//--- Aura Script ----------------------------------------------------------
// Custom BGM
//--- Description ----------------------------------------------------------
// Automatically changes BGM upon entering regions or certain areas,
// depending on the configuration. Any MP3 file found in the client's
// "mp3" folder can be used.
// To play unofficial tracks not found in the client, without explicitly
// adding them, one can utilize the "resourcesvr" feature of the client.
// https://github.com/aura-project/aura/wiki/Client-parameters#resourcesvr
//--- By -------------------------------------------------------------------
// exec, xero
//--- History --------------------------------------------------------------
// v1.0.0 - Initial script [exec]
// v1.0.1 - Update for Aura 2 [xero]
// v1.0.2 - Clean up and optimization [exec]
//--------------------------------------------------------------------------

public class CustomBgmScript : GeneralScript
{
	public override void Load()
	{
		//-- <configuration> ------------------------------------------------

		// Add(<region>, <file name>[, <repeat mode>]);
		// The files reside in the mp3 folder; repeat mode can be Once,
		// or Indefinitely (the default, if omitted).

		//Add(1, "NPC_Castanea.mp3", BgmRepeat.Once);
		//Add(16, "Boss.mp3", BgmRepeat.Indefinitely);
		//Add(3001, "Field_Osna_Sail.mp3");

		//-- </configuration> -----------------------------------------------
	}

	private Dictionary<long, string> _playerStorage = new Dictionary<long, string>();
	private static Dictionary<int, Track> _regions = new Dictionary<int, Track>();

	protected static void Add(int regionId, string fileName, BgmRepeat repeat = BgmRepeat.Indefinitely)
	{
		_regions[regionId] = new Track(fileName, repeat);
	}

	[On("PlayerEntersRegion")]
	public void OnPlayerEntersRegion(Creature creature)
	{
		if (!creature.IsPlayer)
			return;

		// Set BGM if there is one set for creature's region, this will
		// replace BGMs set before.
		Track track;
		if (_regions.TryGetValue(creature.RegionId, out track))
		{
			Send.SetBgm(creature, track.FileName, track.Repeat);
			_playerStorage[creature.EntityId] = track.FileName;
			return;
		}

		// If no BGM is available for the new region, but one was set before,
		// unset it.
		lock (_playerStorage)
		{
			if (_playerStorage.ContainsKey(creature.EntityId))
			{
				Send.UnsetBgm(creature, _playerStorage[creature.EntityId]);
				_playerStorage.Remove(creature.EntityId);
			}
		}
	}

	[On("PlayerLoggedIn")]
	public void OnPlayerLoggedIn(Creature creature)
	{
		OnPlayerEntersRegion(creature);
	}

	private class Track
	{
		public string FileName { get; private set; }
		public BgmRepeat Repeat { get; private set; }

		public Track(string fileName, BgmRepeat repeat)
		{
			FileName = fileName;
			Repeat = repeat;
		}
	}
}
