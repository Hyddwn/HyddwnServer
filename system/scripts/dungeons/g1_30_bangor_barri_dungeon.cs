//--- Aura Script -----------------------------------------------------------
// Barri to Tir Na Nog dungeon
//--- Description -----------------------------------------------------------
// A version of Barri Normal with a portal to TNN at the end.
//---------------------------------------------------------------------------

[DungeonScript("g1_30_bangor_barri_dungeon")]
public class BarriTnnDungeonScript : DungeonScript
{
	private const int GatePropId = 23038;
	private const int GatePortalPropId = 23039;
	private const int BreakerTitle = 10007; // the Seal-Breaker of the Other World

	public override void OnBoss(Dungeon dungeon)
	{
		dungeon.AddBoss(80101, 4); // Wisp
		dungeon.AddBoss(190001, 3); // Flying Sword

		SpawnGate(dungeon);
	}

	private void SpawnGate(Dungeon dungeon)
	{
		var region = dungeon.Regions.Last();
		var endLocation = dungeon.GetEndRoomCenter();
		var direction = MabiMath.DirectionToRadian(0, -1);

		var gate = new Prop(GatePropId, endLocation.RegionId, endLocation.X, endLocation.Y, direction);
		gate.Info.Color2 = 0xffffffff;
		gate.Behavior = OnTouchGate;
		region.AddProp(gate);

		var portal = new Prop(GatePortalPropId, endLocation.RegionId, endLocation.X, endLocation.Y, direction);
		portal.Behavior = OnTouchPortal;
		region.AddProp(portal);
	}

	public void OnTouchGate(Creature creature, Prop gate)
	{
		if (gate.State != "closed")
			return;

		var canOpen = creature.Keywords.Has("g1_36") || creature.Titles.IsUsable(BreakerTitle);
		var saturday = (ErinnTime.Now.Month == ErinnMonth.Samhain || IsEnabled("AllWeekBreaker"));

		if (!canOpen)
		{
			Send.Notice(creature, Localization.Get("You are unable to open the gate."));
		}
		else if (!saturday)
		{
			Send.Notice(creature, Localization.Get("The gate can only be opened on Samhain."));
		}
		else
		{
			creature.Titles.Enable(BreakerTitle);

			gate.SetState("open");

			var portal = gate.Region.GetProp(a => a.Info.Id == GatePortalPropId);
			portal.Extensions.Add(new ConfirmationPropExtension("portal(45360425219915779)", L("Would you like to go to the world across the Sealed door?")));
		}
	}

	public void OnTouchPortal(Creature creature, Prop portal)
	{
		if (portal.Extensions.HasAny)
			creature.Warp("Ula_Crossroad/_Ula_Crossroad/Cross_from_BanWarpdoor");
	}
}
