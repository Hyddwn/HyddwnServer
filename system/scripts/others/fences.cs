//--- Aura Script -----------------------------------------------------------
// Fences
//--- Description -----------------------------------------------------------
// Restrict access to specific areas that are off-limits until certain
// features are enabled.
//---------------------------------------------------------------------------

public class FeatureFencesScript : GeneralScript
{
	public override void Load()
	{
		Prop prop;

		if (!IsEnabled("PortCeann"))
		{
			// Bangor -> Morva Aisle
			SpawnProp(41277, 31, 12400, 4900, 4.712389f);
		}

		if (!IsEnabled("TaraSealStone"))
		{
			// Emain Macha -> Blago Prairie
			SpawnProp(41894, 52, 18131, 46040, 5.85575f);
		}

		// Dugald Aisle -> Dugald Residential Area
		prop = ChannelServer.Instance.World.GetRegion(16).GetProp(a => a.Info.Id == 25219); // toggleable fence
		if (prop != null) prop.SetState(IsEnabled("Housing") ? "open" : "close");

		// Sen Mag -> Sen Mag Residential Area
		prop = ChannelServer.Instance.World.GetRegion(53).GetProp(a => a.Info.Id == 25219); // toggleable fence
		if (prop != null) prop.SetState(IsEnabled("Housing") ? "open" : "close");

		if (!IsEnabled("CobhWorld"))
		{
			// Dunbarton -> Port Cobh
			for (int i = 0; i < 6; ++i)
			{
				prop = SpawnProp(15035, 14, 62400, 37300 + i * 700, 3.14159f);
				prop.Info.Color1 = 0xFFFFFF;
			}
		}

		if (!IsEnabled("PeacaDungeon"))
		{
			// Peaca Dungeon entrance
			SpawnProp(40000, 53, 75600, 118000, 1.57f);
			SpawnProp(40000, 53, 75600, 117600, 2.00f);
		}
	}
}
