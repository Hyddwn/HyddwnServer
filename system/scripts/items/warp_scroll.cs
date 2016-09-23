//--- Aura Script -----------------------------------------------------------
// Warp Scroll Items
//--- Description -----------------------------------------------------------
// Handles items like wings, that warp you to a pre-defined location.
//---------------------------------------------------------------------------

using Aura.Channel.Skills.Hidden;

[ItemScript("/warp_scroll/")]
public class WarpScrollItemScript : ItemScript
{
	const int AvonRegionId = 501;
	const int AvonX = 64182;
	const int AvonY = 63252;
	const int AvonCoolDown = 5;

	public override void OnUse(Creature creature, Item item, string parameter)
	{
		// Avon Feather
		if (item.HasTag("/feather_of_avon/"))
		{
			var regionId = item.MetaData1.GetInt("AFRR");
			var x = item.MetaData1.GetInt("AFRX");
			var y = item.MetaData1.GetInt("AFRY");
			var lastWarp = item.MetaData1.GetDateTime("AFLU");
			var inAvon = (creature.RegionId == AvonRegionId);
			var pos = creature.GetPosition();

			// Stop if on cooldown or creature is in a temp region
			if (DateTime.Now < lastWarp.AddMinutes(AvonCoolDown) || creature.Region.IsTemp)
			{
				Send.ServerMessage(creature, L("Safety error, failed to warp."));
				return;
			}

			// Don't warp if it's the first time and the creature already
			// is in Avon, or if creature already is in the target region.
			if ((regionId == 0 && creature.RegionId == AvonRegionId) || creature.RegionId == regionId)
			{
				Send.ServerMessage(creature, L("Region error, failed to warp."));
				return;
			}

			// Update feather's destination
			item.MetaData1.SetInt("AFRR", creature.RegionId);
			item.MetaData1.SetInt("AFRX", inAvon ? AvonX : pos.X);
			item.MetaData1.SetInt("AFRY", inAvon ? AvonY : pos.Y);
			item.MetaData1.SetLong("AFLU", DateTime.Now);
			Send.ItemUpdate(creature, item);

			// Warp, default to Avon if region is 0
			if (regionId == 0)
				creature.Warp(AvonRegionId, AvonX, AvonY);
			else
				creature.Warp(regionId, x, y);

			return;
		}

		// A few items prepare a skill, instead of sending use, to reduce
		// redundancy we'll call into the skill handler.
		HiddenTownBack.Warp(creature, item, false);
	}
}
