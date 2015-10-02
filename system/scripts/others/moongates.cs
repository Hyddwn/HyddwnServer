//--- Aura Script -----------------------------------------------------------
// Moongates
//--- Description -----------------------------------------------------------
// New version of the moon gates, the "Moon Tunnels". Traveling between
// specific locations at night.
// --- Notes ----------------------------------------------------------------
// It doesn't seem to be possible to create custom gates, the client just
// doesn't display them.
//---------------------------------------------------------------------------

public class MoongateScript : GeneralScript
{
	// Setup ----------------------------------------------------------------

	// Moongates are always open
	private const bool AlwaysOpen = false;

	// Anybody can use any moongate without activating it first
	private const bool FreeRoaming = false;

	public override void Load()
	{
		RegisterGate("_moontunnel_tirchonaill", 0xA000010004001F);
		RegisterGate("_moontunnel_ciar_dungeon", 0xA000010005001B);
		RegisterGate("_moontunnel_alby_dungeon", 0xA0000100080006);
		RegisterGate("_moontunnel_math_dungeon", 0xA0000E00090034);
		RegisterGate("_moontunnel_dunbarton", 0xA0000E000A02B4);
		RegisterGate("_moontunnel_rabbie_dungeon", 0xA0000E00110039);
		RegisterGate("_moontunnel_duntir02", 0xA000100008002B);
		RegisterGate("_moontunnel_loggingcamp", 0xA00010000D0011);
		RegisterGate("_moontunnel_cobh_harbor", 0xA0001700040199);
		RegisterGate("_moontunnel_reighinalt", 0xA0001E00010062);
		RegisterGate("_moontunnel_dragonruin", 0xA0001E00020064);
		RegisterGate("_moontunnel_fiodh_dungeon", 0xA0001E00050002);
		RegisterGate("_moontunnel_bangor", 0xA0001E0008002D);
		RegisterGate("_moontunnel_sidhe_sneachta", 0xA0003000020003);
		RegisterGate("_moontunnel_emainmacha", 0xA000340008003D);
		RegisterGate("_moontunnel_emain_macha_w", 0xA00034000F001D);
		RegisterGate("_moontunnel_coill_dungeon", 0xA0003400150099);
		RegisterGate("_moontunnel_emain_macha_n", 0xA000340016001E);
		RegisterGate("_moontunnel_sen_mag_plains", 0xA0003500000007);
		RegisterGate("_moontunnel_peaca_dungeon", 0xA0003500070004);
		RegisterGate("_moontunnel_ceoisland", 0xA0003800010001);
		RegisterGate("_moontunnel_morva_aisle", 0xA0006000010003);
		RegisterGate("_moontunnel_ceann_harbor", 0xA0006400000006);
		RegisterGate("_moontunnel_tailteann_w", 0xA0012C000A0001);
		RegisterGate("_moontunnel_tailteann_farm01", 0xA0012C00170082);
		RegisterGate("_moontunnel_tailteann_druid", 0xA0012C0019002E);
		RegisterGate("_moontunnel_taillteann_west", 0xA0012C001900C3);
		RegisterGate("_moontunnel_tailteann_s", 0xA0012C00240012);
		RegisterGate("_moontunnel_tailteann_e", 0xA0012C00260016);
		RegisterGate("_moontunnel_tailteann_cemetery", 0xA0012C002F0001);
		RegisterGate("_moontunnel_sliab_cuilin01", 0xA0012D00040005);
		RegisterGate("_moontunnel_sliab_cuilin02", 0xA0012D0004001B);
		RegisterGate("_moontunnel_abb_neagh01", 0xA0012E00190018);
		RegisterGate("_moontunnel_abb_neagh02", 0xA0012E001A0016);
		RegisterGate("_moontunnel_tara_01", 0xA001910008000A);
		RegisterGate("_moontunnel_tara_west", 0xA00191000802F2);
		RegisterGate("_moontunnel_tara_s", 0xA001910010002F);
		RegisterGate("_moontunnel_blago_prairie_e", 0xA001920000003B);
		RegisterGate("_moontunnel_blago_prairie_02", 0xA0019200020108);
		RegisterGate("_moontunnel_blago_prairie_01", 0xA001920006002C);
		RegisterGate("_moontunnel_tara_kingdom", 0xA001B5000E0003);
		RegisterGate("_moontunnel_belfast_01", 0xA00FA500020024);
		RegisterGate("_moontunnel_belfast_02", 0xA00FA5000B0011);
		RegisterGate("_moontunnel_scathach_03", 0xA00FAE00020004);
		RegisterGate("_moontunnel_scathach_04", 0xA00FAE00030005);
		RegisterGate("_moontunnel_scathach_01", 0xA00FAE000A00B5);
		RegisterGate("_moontunnel_scathach_02", 0xA00FAE000A00B8);
		RegisterGate("_moontunnel_scathach_05", 0xA00FAE000C004B);

		// Keywords don't exist, do the gates still exist?
		//RegisterGate("_moontunnel_dugaldaisle", 0xA0001000060014);
		//RegisterGate("_moontunnel_moonsurface_enterance", 0xA003EB00000001);
		//RegisterGate("_moontunnel_moonsurface_exit", 0xA003EB00000003);

		AddPacketHandler(Op.MoonGateUse, MoonGateUse);
	}

	// End Setup ------------------------------------------------------------

	private Dictionary<long, MoonGate> gates = new Dictionary<long, MoonGate>();
	private Dictionary<string, MoonGate> gatesStr = new Dictionary<string, MoonGate>();

	private void RegisterGate(string keyword, long entityId)
	{
		// Get prop
		// The moon gate props are client props, they are added to the world
		// on startup and they can't be removed.
		var prop = ChannelServer.Instance.World.GetProp(entityId);
		if (prop == null)
		{
			Log.Error("MoongateScript: Prop '{0:X16}' not found.", entityId);
			return;
		}

		// Get keyword data for id
		var keywordData = AuraData.KeywordDb.Find(keyword);
		if (keywordData == null)
		{
			Log.Error("MoongateScript: Unknown keyword '{0}'.", keyword);
			return;
		}

		prop.State = ErinnTime.Now.IsNight || AlwaysOpen ? "open" : "closed";

		// Open map window when prop is clicked
		SetPropBehavior(entityId, OpenMapWindow);

		// Save gate
		var gate = new MoonGate(keyword, keywordData.Id, prop);
		gates.Add(entityId, gate);
		gatesStr.Add(keyword, gate);
	}

	private void OpenMapWindow(Creature creature, Prop prop)
	{
		// Get gate
		// Sanity check, technically it shouldn't ever fail, because we wouldn't
		// even get here without the gate being added in this script.
		MoonGate gate;
		if (!gates.TryGetValue(prop.EntityId, out gate))
		{
			Send.Notice(creature, Localization.Get("This moon gate is currently not operable. Please report."));
			return;
		}

		// Add keyword if creature doesn't have it yet, this "marks" the
		// moon gate as potential target in the new system, you can only
		// warp to those you have visited before.
		if (!creature.Keywords.Has(gate.Keyword))
			creature.Keywords.Give(gate.Keyword);

		// Get list of moon gates the creature can use
		var freeRoaming = (FreeRoaming || creature.Keywords.Has("freemoongate"));
		var mygates = gates.Values.Where(a => CanUseGate(creature, a));

		SendMoonGateMap(creature, gate, mygates);
	}

	private bool CanUseGate(Creature creature, MoonGate gate)
	{
		var freeRoaming = (FreeRoaming || creature.Keywords.Has("freemoongate"));
		return (freeRoaming || creature.Keywords.Has(gate.Keyword));
	}

	private void MoonGateUse(ChannelClient client, Packet packet)
	{
		var origin = packet.GetString();
		var destination = packet.GetString();

		var creature = client.GetCreatureSafe(packet.Id);

		// Check gates
		MoonGate originGate, destinationGate;
		if (!gatesStr.TryGetValue(origin, out originGate) || !gatesStr.TryGetValue(destination, out destinationGate))
		{
			Send.Notice(creature, Localization.Get("This moon gate is currently not operable. Please report."));
			return;
		}

		// Check origin gate
		if (originGate.Prop.State == "closed")
		{
			// Don't log, someone could be waiting with the map open and
			// select a destination too late.
			SendMoonGateUseR(creature, false);
			return;
		}

		// Check range to origin gate
		if (creature.RegionId != originGate.Prop.Info.Region || !creature.GetPosition().InRange(originGate.Prop.GetPosition(), 1000))
		{
			// Could happen due to desync? The range at least.
			SendMoonGateUseR(creature, false);
			return;
		}

		// Check if char has target
		if (!CanUseGate(creature, destinationGate))
		{
			Log.Warning("MoongateScript.MoonGateUse: Creature '{0:X16}' tried to warp to moon gate that he can't use.", creature.EntityId);
			SendMoonGateUseR(creature, false);
			return;
		}

		creature.Warp(destinationGate.Prop.GetLocation());

		SendMoonGateUseR(creature, true);
	}

	private void SendMoonGateMap(Creature creature, MoonGate fromGate, IEnumerable<MoonGate> gates)
	{
		var packet = new Packet(Op.MoonGateMap, creature.EntityId);

		packet.PutInt(2);
		packet.PutString(fromGate.Keyword);
		packet.PutByte((byte)gates.Count());
		foreach (var gate in gates)
		{
			packet.PutUShort((ushort)gate.KeywordId);
			packet.PutByte(1);
			packet.PutInt(gate.Prop.Info.Region);
			packet.PutInt((int)gate.Prop.Info.X);
			packet.PutInt((int)gate.Prop.Info.Y);
		}

		creature.Client.Send(packet);
	}

	private void SendMoonGateUseR(Creature creature, bool success)
	{
		var packet = new Packet(Op.MoonGateUseR, creature.EntityId);
		packet.PutByte(success);
		creature.Client.Send(packet);
	}

	[On("ErinnDaytimeTick")]
	public void OnErinnDaytimeTick(ErinnTime now)
	{
		var state = now.IsNight || AlwaysOpen ? "open" : "closed";

		foreach (var gate in gates.Values)
			gate.Prop.SetState(state);
	}

	private class MoonGate
	{
		public string Keyword { get; set; }
		public int KeywordId { get; set; }
		public Prop Prop { get; set; }

		public MoonGate(string keyword, int keywordId, Prop prop)
		{
			this.Keyword = keyword;
			this.KeywordId = keywordId;
			this.Prop = prop;
		}
	}
}
