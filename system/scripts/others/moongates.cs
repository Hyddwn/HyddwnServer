//--- Aura Script -----------------------------------------------------------
// Moon Gates
//--- Description -----------------------------------------------------------
// This script manages all aspects of Moon Gates/Tunnels across Uladh and
// Belvast. It switches between old-school Moon Gates, the different
// timetables, and the newer Moon Tunnels, based on the features enabled
// for the server.
// 
// Features used:
// - MoonTunnel: Switches between gates and tunnels
// - G2: Make use of G2 timetable
// - G4: Make use of G4 timetable
// - G9: Make use of G9 timetable
// - G10: Make use of G10 timetable
// - G11: Make use of G11 timetable
// - G18: Gates/tunnels always open
// --- Notes ----------------------------------------------------------------
// It doesn't seem to be possible to create custom gates, the client just
// doesn't display them.
//---------------------------------------------------------------------------

public class MoonGateScript : GeneralScript
{
	// Setup ----------------------------------------------------------------

	// Moongates are always open
	private const bool AlwaysOpen = false;

	// Anybody can use any moongate without activating it first
	private const bool FreeRoaming = false;

	public override void Load()
	{
		RegisterGate("Tir Chonaill", "_moontunnel_tirchonaill", "Uladh_main/field_Tir_S_ba/_moongate_tirchonaill");
		RegisterGate("Ciar Dungeon", "_moontunnel_ciar_dungeon", "Uladh_main/field_Tir_E_ab/_moongate_ciar_dungeon");
		RegisterGate("Alby Dungeon", "_moontunnel_alby_dungeon", "Uladh_main/field_Tir_N_aa/_moongate_alby_dungeon");
		RegisterGate("Math Dungeon", "_moontunnel_math_dungeon", "Uladh_Dunbarton/field_Dunbarton_03/_moongate_math_dungeon");
		RegisterGate("Dunbarton", "_moontunnel_dunbarton", "Uladh_Dunbarton/_Uladh_Dunbarton/_moongate_dunbarton");
		RegisterGate("Rabbie Dungeon", "_moontunnel_rabbie_dungeon", "Uladh_Dunbarton/field_Dunbarton_00/_moongate_rabbie_dungeon");
		RegisterGate("Spiral Hill", "_moontunnel_duntir02", "Uladh_Dun_to_Tircho/Field_DunTir_06/_moongate_duntir02");
		RegisterGate("Logging Camp", "_moontunnel_loggingcamp", "Uladh_Dun_to_Tircho/Field_DunTir_11/_moongate_loggingcamp");
		RegisterGate("Lighthouse", "_moontunnel_cobh_harbor", "Uladh_Cobh_harbor/_Uladh_Cobh_harbor_4/_moongate_cobh_harbor");
		RegisterGate("Reighinalt", "_moontunnel_reighinalt", "Ula_Dun_to_Bangor/Field_DunBan_S2/_moongate_reighinalt");
		RegisterGate("Dragon Ruins", "_moontunnel_dragonruin", "Ula_Dun_to_Bangor/Field_DunBan_Center/_moongate_dragonruin");
		RegisterGate("Fiodh Dungeon", "_moontunnel_fiodh_dungeon", "Ula_Dun_to_Bangor/Field_DunBan_N3/_moongate_fiodh_dungeon");
		RegisterGate("Bangor", "_moontunnel_bangor", "Ula_Dun_to_Bangor/Field_DunBan_S3/_moongate_bangor");
		RegisterGate("Sidhe Sneachta", "_moontunnel_sidhe_sneachta", "Sidhe_Sneachta_N/_Sidhe_Sneachta_N2/_moongate_sidhe_sneachta");
		RegisterGate("Emain Macha", "_moontunnel_emainmacha", "Ula_Emainmacha/_Ula_Emainmacha_lake_water_3/_moongate_emainmacha");
		RegisterGate("Emain Macha Western", "_moontunnel_emain_macha_w", "Ula_Emainmacha/_Ula_Emainmacha_lake_water_5/_moongate_emain_macha_w");
		RegisterGate("Coill Dungeon", "_moontunnel_coill_dungeon", "Ula_Emainmacha/_Ula_Emainmacha_N_Coill/_moongate_coill_dungeon");
		RegisterGate("Emain Macha Northern", "_moontunnel_emain_macha_n", "Ula_Emainmacha/_Ula_Emainmacha_N_2/_moongate_emain_macha_n");
		RegisterGate("Sen Mag Plains", "_moontunnel_sen_mag_plains", "Ula_Sen_Mag/_Ula_Sen_Mag_C1/_moongate_sen_mag_plains");
		RegisterGate("Peaca Dungeon", "_moontunnel_peaca_dungeon", "Ula_Sen_Mag/_Ula_Sen_Mag_dg/_moongate_peaca_dungeon");
		RegisterGate("Ceo Island", "_moontunnel_ceoisland", "Ula_Emainmacha_Ceo/_Ula_Emainmacha_Ceo_W2/_moongate_ceoisland");
		RegisterGate("Morva Aisle", "_moontunnel_morva_aisle", "Ula_Morva_Aisle/_Ula_Morva_Aisle_02/_moongate_morva_aisle");
		RegisterGate("Ceann Harbor", "_moontunnel_ceann_harbor", "Ula_Ceann_Harbor/_Ula_Ceann_Harbor_01/_moongate_ceann_harbor");
		RegisterGate("Tailteann", "_moontunnel_tailteann_w", "taillteann_main_field/_taillteann_main_field_0010/_moongate_tailteann_w");
		RegisterGate("Tailteann Farm", "_moontunnel_tailteann_farm01", "taillteann_main_field/_taillteann_main_field_0023/_moongate_tailteann_farm01");
		RegisterGate("Druid's House", "_moontunnel_tailteann_druid", "taillteann_main_field/_taillteann_main_field_0025/_moongate_tailteann_druid");
		RegisterGate("Tailteann West", "_moontunnel_taillteann_west", "taillteann_main_field/_taillteann_main_field_0025/_moongate_taillteann_west");
		RegisterGate("Tailteann South", "_moontunnel_tailteann_s", "taillteann_main_field/_taillteann_main_field_0036/_moongate_tailteann_s");
		RegisterGate("Tailteann Eastern", "_moontunnel_tailteann_e", "taillteann_main_field/_taillteann_main_field_0038/_moongate_tailteann_e");
		RegisterGate("Graveyard", "_moontunnel_tailteann_cemetery", "taillteann_main_field/_taillteann_main_field_0047/_moongate_tailteann_cemetery");
		RegisterGate("Sliab Cuilin 1", "_moontunnel_sliab_cuilin01", "Taillteann_E_field/_Taillteann_E_field_0004/_moongate_sliab_cuilin01");
		RegisterGate("Sliab Cuilin 2", "_moontunnel_sliab_cuilin02", "Taillteann_E_field/_Taillteann_E_field_0004/_moongate_sliab_cuilin02");
		RegisterGate("Abb Neagh 1", "_moontunnel_abb_neagh01", "Taillteann_SE_field/_Taillteann_SE_field_0007/_moongate_abb_neagh01");
		RegisterGate("Abb Neagh 2", "_moontunnel_abb_neagh02", "Taillteann_SE_field/_Taillteann_SE_field_0004/_moongate_abb_neagh02");
		RegisterGate("Jousting Arena", "_moontunnel_tara_01", "Tara_main_field/_Tara_main_field_0008/_moongate_tara_01");
		RegisterGate("Tara", "_moontunnel_tara_west", "Tara_main_field/_Tara_main_field_0008/_moongate_tara_west");
		RegisterGate("Tara South", "_moontunnel_tara_s", "Tara_main_field/_Tara_main_field_0016/_moongate_tara_s");
		RegisterGate("East Blago Prairie", "_moontunnel_blago_prairie_e", "Tara_SE_Field/_Tara_SE_Field_0006/_moongate_blago_prairie_e");
		RegisterGate("Eluned Winery", "_moontunnel_blago_prairie_02", "Tara_SE_Field/_Tara_SE_Field_0003/_moongate_blago_prairie_02");
		RegisterGate("Lezarro Winery", "_moontunnel_blago_prairie_01", "Tara_SE_Field/_Tara_SE_Field_0002/_moongate_blago_prairie_01");
		RegisterGate("Rath Royal Castle", "_moontunnel_tara_kingdom", "Tara_main_kingdom_field/_Tara_main_kingdom_field_18/_moongate_tara_kingdom");
		RegisterGate("Mykeeness Cliffs", "_moontunnel_belfast_01", "Belfast_human/_Belfast_human_2/_moongate_belfast_01");
		RegisterGate("Garden", "_moontunnel_belfast_02", "Belfast_human/_Belfast_human_11/_moongate_belfast_02");
		RegisterGate("Springs", "_moontunnel_scathach_03", "Belfast_Skatha_main_field/_Belfast_Skatha_main_field_7/_moongate_scathach_03");
		RegisterGate("Black Beach", "_moontunnel_scathach_04", "Belfast_Skatha_main_field/_Belfast_Skatha_main_field_10/_moongate_scathach_04");
		RegisterGate("Scathach Patrol Camp", "_moontunnel_scathach_01", "Belfast_Skatha_main_field/_Belfast_Skatha_main_field_6/_moongate_scathach_01");
		RegisterGate("Fishing Area", "_moontunnel_scathach_02", "Belfast_Skatha_main_field/_Belfast_Skatha_main_field_6/_moongate_scathach_02");
		RegisterGate("Witch's Cave", "_moontunnel_scathach_05", "Belfast_Skatha_main_field/_Belfast_Skatha_main_field_11/_moongate_scathach_05");

		// Keywords don't exist, do the gates still exist?
		//RegisterGate("_moontunnel_dugaldaisle", 0xA0001000060014);
		//RegisterGate("_moontunnel_moonsurface_enterance", 0xA003EB00000001);
		//RegisterGate("_moontunnel_moonsurface_exit", 0xA003EB00000003);

		RegisterTimetable("G1",
			"_moontunnel_tirchonaill",
			"_moontunnel_ciar_dungeon",
			"_moontunnel_dunbarton",
			"_moontunnel_bangor",
			"_moontunnel_bangor",
			"_moontunnel_math_dungeon",
			"_moontunnel_dunbarton",
			"_moontunnel_rabbie_dungeon",
			"_moontunnel_loggingcamp",
			"_moontunnel_fiodh_dungeon",
			"_moontunnel_alby_dungeon",
			"_moontunnel_ciar_dungeon"
		);

		RegisterTimetable("G2",
			"_moontunnel_fiodh_dungeon",
			"_moontunnel_math_dungeon",
			"_moontunnel_rabbie_dungeon",
			"_moontunnel_bangor",
			"_moontunnel_tirchonaill",
			"_moontunnel_dunbarton",
			"_moontunnel_tirchonaill",
			"_moontunnel_dunbarton",
			"_moontunnel_tirchonaill",
			"_moontunnel_ciar_dungeon",
			"_moontunnel_peaca_dungeon",
			"_moontunnel_bangor",
			"_moontunnel_dragonruin",
			"_moontunnel_dunbarton",
			"_moontunnel_alby_dungeon",
			"_moontunnel_loggingcamp",
			"_moontunnel_bangor"
		);

		RegisterTimetable("G2Emain",
			"_moontunnel_coill_dungeon",
			"_moontunnel_alby_dungeon",
			"_moontunnel_ceoisland",
			"_moontunnel_peaca_dungeon",
			"_moontunnel_dunbarton",
			"_moontunnel_dragonruin",
			"_moontunnel_ceoisland",
			"_moontunnel_ceoisland",
			"_moontunnel_dunbarton",
			"_moontunnel_rabbie_dungeon",
			"_moontunnel_emainmacha",
			"_moontunnel_emainmacha",
			"_moontunnel_bangor",
			"_moontunnel_math_dungeon",
			"_moontunnel_tirchonaill",
			"_moontunnel_dunbarton",
			"_moontunnel_tirchonaill",
			"_moontunnel_tirchonaill",
			"_moontunnel_bangor",
			"_moontunnel_bangor",
			"_moontunnel_fiodh_dungeon",
			"_moontunnel_emainmacha",
			"_moontunnel_loggingcamp",
			"_moontunnel_ciar_dungeon"
		);

		RegisterTimetable("G4",
			"_moontunnel_emainmacha",
			"_moontunnel_emainmacha",
			"_moontunnel_ceann_harbor",
			"_moontunnel_bangor",
			"_moontunnel_math_dungeon",
			"_moontunnel_coill_dungeon",
			"_moontunnel_alby_dungeon",
			"_moontunnel_peaca_dungeon",
			"_moontunnel_ceann_harbor",
			"_moontunnel_loggingcamp",
			"_moontunnel_tirchonaill",
			"_moontunnel_tirchonaill",
			"_moontunnel_dunbarton",
			"_moontunnel_rabbie_dungeon",
			"_moontunnel_bangor",
			"_moontunnel_dunbarton",
			"_moontunnel_ciar_dungeon",
			"_moontunnel_ceoisland",
			"_moontunnel_fiodh_dungeon",
			"_moontunnel_dragonruin",
			"_moontunnel_ceoisland"
		);

		RegisterTimetable("G9",
			"_moontunnel_tirchonaill",
			"_moontunnel_alby_dungeon",
			"_moontunnel_ceoisland",
			"_moontunnel_ceoisland",
			"_moontunnel_tailteann_w",
			"_moontunnel_bangor",
			"_moontunnel_tirchonaill",
			"_moontunnel_fiodh_dungeon",
			"_moontunnel_peaca_dungeon",
			"_moontunnel_emainmacha",
			"_moontunnel_dunbarton",
			"_moontunnel_tirchonaill",
			"_moontunnel_coill_dungeon",
			"_moontunnel_dunbarton",
			"_moontunnel_bangor",
			"_moontunnel_dunbarton",
			"_moontunnel_emainmacha",
			"_moontunnel_ceann_harbor",
			"_moontunnel_rabbie_dungeon",
			"_moontunnel_emainmacha",
			"_moontunnel_dragonruin",
			"_moontunnel_ceoisland",
			"_moontunnel_bangor",
			"_moontunnel_math_dungeon",
			"_moontunnel_tailteann_w",
			"_moontunnel_ciar_dungeon",
			"_moontunnel_tailteann_w",
			"_moontunnel_loggingcamp"
		);

		RegisterTimetable("G10",
			"_moontunnel_tailteann_w",
			"_moontunnel_tirchonaill",
			"_moontunnel_dunbarton",
			"_moontunnel_emainmacha",
			"_moontunnel_coill_dungeon",
			"_moontunnel_tara_west",
			"_moontunnel_ceoisland",
			"_moontunnel_bangor",
			"_moontunnel_ceoisland",
			"_moontunnel_tara_west",
			"_moontunnel_ciar_dungeon",
			"_moontunnel_tailteann_w",
			"_moontunnel_tara_west",
			"_moontunnel_bangor",
			"_moontunnel_emainmacha",
			"_moontunnel_tailteann_w",
			"_moontunnel_dunbarton",
			"_moontunnel_math_dungeon",
			"_moontunnel_emainmacha",
			"_moontunnel_ceoisland",
			"_moontunnel_fiodh_dungeon",
			"_moontunnel_peaca_dungeon",
			"_moontunnel_ceann_harbor",
			"_moontunnel_loggingcamp",
			"_moontunnel_tara_west",
			"_moontunnel_alby_dungeon",
			"_moontunnel_tirchonaill",
			"_moontunnel_tirchonaill",
			"_moontunnel_dragonruin",
			"_moontunnel_dunbarton",
			"_moontunnel_tara_west",
			"_moontunnel_rabbie_dungeon"
		);

		RegisterTimetable("G11",
			"_moontunnel_tara_west",
			"_moontunnel_ceoisland",
			"_moontunnel_tirchonaill",
			"_moontunnel_tailteann_w",
			"_moontunnel_emainmacha",
			"_moontunnel_tara_west",
			"_moontunnel_dunbarton",
			"_moontunnel_ceann_harbor",
			"_moontunnel_bangor",
			"_moontunnel_emainmacha",
			"_moontunnel_tara_west",
			"_moontunnel_tirchonaill",
			"_moontunnel_tailteann_w",
			"_moontunnel_ceoisland",
			"_moontunnel_emainmacha",
			"_moontunnel_tara_west",
			"_moontunnel_bangor",
			"_moontunnel_dunbarton",
			"_moontunnel_ceann_harbor",
			"_moontunnel_tara_west",
			"_moontunnel_tailteann_w",
			"_moontunnel_tirchonaill",
			"_moontunnel_dunbarton",
			"_moontunnel_bangor"
		);

		AddPacketHandler(Op.MoonGateUse, HandleMoonGateUse);
		AddPacketHandler(Op.MoonGateInfoRequest, HandleMoonGateInfoRequest);

		OnErinnDaytimeTick(ErinnTime.Now);
	}

	// End Setup ------------------------------------------------------------

	private Dictionary<long, MoonGate> gates = new Dictionary<long, MoonGate>();
	private Dictionary<string, MoonGate> gatesStr = new Dictionary<string, MoonGate>();
	private Dictionary<string, string[]> tables = new Dictionary<string, string[]>();
	private string currentGateKeyword, nextGateKeyword;
	private MoonGate currentGate;

	private void RegisterGate(string name, string keyword, string fullName)
	{
		// Get prop
		// The moon gate props are client props, they are added to the world
		// on startup and they can't be removed.
		var prop = ChannelServer.Instance.World.GetProp(fullName);
		if (prop == null)
		{
			Log.Error("MoonGateScript: Prop '{0}' not found.", fullName);
			return;
		}

		// Get keyword data for id
		var keywordData = AuraData.KeywordDb.Find(keyword);
		if (keywordData == null)
		{
			Log.Error("MoonGateScript: Unknown keyword '{0}'.", keyword);
			return;
		}

		// Open map window when prop is clicked
		prop.Behavior = MoonGateBehavior;

		// Save gate
		var gate = new MoonGate(name, keyword, keywordData.Id, prop);
		gates.Add(prop.EntityId, gate);
		gatesStr.Add(keyword, gate);
	}

	private void RegisterTimetable(string name, params string[] gates)
	{
		if (gates.Length < 2)
			throw new ArgumentException("Timetables require at least two gates.");

		tables[name] = gates;
	}

	private void MoonGateBehavior(Creature creature, Prop prop)
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

		// Check state
		if (gate.Prop.State == "closed")
		{
			Log.Warning("MoonGateScript.MoonGateBehavior: Creature '{0:X16}' tried to use closed moon gate.", creature.EntityId);
			return;
		}

		if (IsEnabled("MoonTunnel") && creature.IsCharacter)
		{
			// Add keyword if creature doesn't have it yet, this "marks" the
			// moon gate as potential target in the new system, you can only
			// warp to those you have visited before.
			if (!creature.Keywords.Has(gate.Keyword))
				creature.Keywords.Give(gate.Keyword);

			// Get list of moon gates the creature can use
			var freeRoaming = (FreeRoaming || creature.Keywords.Has("freemoongate"));
			var mygates = gates.Values.Where(a => CanWarpTo(creature, a));

			SendMoonGateMap(creature, gate, mygates);
		}
		else
		{
			var origin = gate.Keyword;
			var destination = currentGateKeyword;

			UseMoonGate(creature, origin, destination);
		}
	}

	private string[] GetTable()
	{
		string[] result;

		// G11
		if (IsEnabled("G11") && tables.TryGetValue("G11", out result))
			return result;

		// G10
		if (IsEnabled("G10") && tables.TryGetValue("G10", out result))
			return result;

		// G9
		if (IsEnabled("G9") && tables.TryGetValue("G9", out result))
			return result;

		// G4
		if (IsEnabled("G4") && tables.TryGetValue("G4", out result))
			return result;

		// G2, table changes once Emain is open
		if (IsEnabled("G2"))
		{
			var sealBroken = (GlobalVars.Perm["SealStoneId_sealstone_osnasail"] != null || GlobalVars.Perm["SealStoneId_sealstone_south_emainmacha"] != null);

			if (tables.TryGetValue("G2Emain", out result) && sealBroken)
				return result;

			if (tables.TryGetValue("G2", out result))
				return result;
		}

		// G1
		// Was there another one, for when the Dugald seal stone hadn't
		// been broken yet?
		if (tables.TryGetValue("G1", out result))
			return result;

		// Fallback
		Log.Warning("MoonGateScript.GetTable: No suitable timetable found.");

		return new string[] { "_moontunnel_tirchonaill", "_moontunnel_dunbarton" };
	}

	private bool CanWarpTo(Creature creature, MoonGate gate)
	{
		if (!IsEnabled("MoonTunnel"))
			return true;

		var freeRoaming = (FreeRoaming || creature.Keywords.Has("freemoongate"));
		return (freeRoaming || creature.Keywords.Has(gate.Keyword));
	}

	private void HandleMoonGateUse(ChannelClient client, Packet packet)
	{
		var origin = packet.GetString();
		var destination = packet.GetString();

		var creature = client.GetCreatureSafe(packet.Id);

		var success = UseMoonGate(creature, origin, destination);

		SendMoonGateUseR(creature, success);
	}

	private void HandleMoonGateInfoRequest(ChannelClient client, Packet packet)
	{
		var creature = client.GetCreatureSafe(packet.Id);

		SendMoonGateInfoRequestR(creature, currentGateKeyword, nextGateKeyword);
	}

	private bool UseMoonGate(Creature creature, string origin, string destination)
	{
		// Check locations
		if (origin == destination)
		{
			Send.Notice(creature, Localization.Get("You cannot teleport using the same Moon Gate."));
			return false;
		}

		// Check gates
		MoonGate originGate, destinationGate;
		if (!gatesStr.TryGetValue(origin, out originGate) || !gatesStr.TryGetValue(destination, out destinationGate))
		{
			Send.Notice(creature, Localization.Get("This moon gate is currently not operable. Please report."));
			return false;
		}

		// Check origin gate
		if (originGate.Prop.State == "closed")
		{
			// Don't log, someone could be waiting with the map open and
			// select a destination too late.
			return false;
		}

		// Check range to origin gate
		if (creature.RegionId != originGate.Prop.Info.Region || !creature.GetPosition().InRange(originGate.Prop.GetPosition(), 1000))
		{
			// Could happen due to desync? The range at least.
			Send.Notice(creature, L("You're too far away."));
			return false;
		}

		// Check if char has target
		if (!CanWarpTo(creature, destinationGate))
		{
			Log.Warning("MoongateScript.MoonGateUse: Creature '{0:X16}' tried to warp to moon gate that he can't use.", creature.EntityId);
			return false;
		}

		creature.Warp(destinationGate.Prop.GetLocation());

		return true;
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

	private void SendMoonGateInfoRequestR(string current, string next)
	{
		var packet = new Packet(Op.MoonGateInfoRequestR, MabiId.Broadcast);
		packet.PutString(current);
		packet.PutString(next);
		ChannelServer.Instance.World.Broadcast(packet);
	}

	private void SendMoonGateInfoRequestR(Creature creature, string current, string next)
	{
		var packet = new Packet(Op.MoonGateInfoRequestR, MabiId.Broadcast);
		packet.PutString(current);
		packet.PutString(next);
		creature.Client.Send(packet);
	}

	private void UpdateCurrentGates()
	{
		var table = GetTable();

		// Add 9 minutes, to compensate for the 6 in-game hours between
		// 18:00 and 00:00, otherwise we get the gates for the next day
		// when starting the server between 00:00 and 05:59.
		var cycles = (int)((DateTime.Now.AddMinutes(9) - ErinnTime.BeginOfTime).TotalSeconds / 2160);

		currentGateKeyword = table[cycles % table.Length];
		nextGateKeyword = table[(cycles + 1) % table.Length];

		if (!gatesStr.TryGetValue(currentGateKeyword, out currentGate))
			throw new Exception("Gate '" + currentGateKeyword + "' not found.");

		SendMoonGateInfoRequestR(currentGateKeyword, nextGateKeyword);
	}

	[On("ErinnDaytimeTick")]
	public void OnErinnDaytimeTick(ErinnTime now)
	{
		var firstRun = (currentGateKeyword == null);

		if (now.IsDusk || currentGateKeyword == null)
			UpdateCurrentGates();

		// Just update gates on first run, to set initial state.
		if (firstRun)
		{
			UpdateGates(now);
			return;
		}

		// Open gates after 12s, after the Eweca msg disappeared.
		Task.Delay(12000).ContinueWith(_ =>
		{
			UpdateGates(now);
			if (!IsEnabled("MoonTunnel"))
				Send.Notice(NoticeType.MiddleSystem, string.Format(L("Moon Gates leading to {0} have appeared all across Erinn."), currentGate.Name));
		});
	}

	private void UpdateGates(ErinnTime now)
	{
		var state = now.IsNight || AlwaysOpen || IsEnabled("Tunnel24Free") ? "open" : "closed";

		foreach (var gate in gates.Values)
		{
			gate.Prop.State = state;

			// Adds destination name to prop title and a confirmation request
			// when clicking the prop. With MoonTunnel enabled, this depends on
			// whether you're a pet or a character, Aura doesn't support that atm.
			if (!IsEnabled("MoonTunnel"))
			{
				gate.Prop.Xml.SetAttributeValue("target", currentGateKeyword);
				gate.Prop.Xml.SetAttributeValue("nopick", 0); // 1 disables clicking the prop
				gate.Prop.Extensions.Clear();
				gate.Prop.Extensions.Add(new ConfirmationPropExtension("_devent_ask_warp", string.Format(L("Do you wish to travel to the {0} Moon Gate?"), currentGate.Name), ""));
			}

			Send.PropUpdate(gate.Prop);
		}
	}

	private class MoonGate
	{
		public string Name { get; set; }
		public string Keyword { get; set; }
		public int KeywordId { get; set; }
		public Prop Prop { get; set; }

		public MoonGate(string name, string keyword, int keywordId, Prop prop)
		{
			this.Name = name;
			this.Keyword = keyword;
			this.KeywordId = keywordId;
			this.Prop = prop;
		}
	}
}
