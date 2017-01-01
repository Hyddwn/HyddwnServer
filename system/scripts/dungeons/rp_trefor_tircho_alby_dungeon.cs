//--- Aura Script -----------------------------------------------------------
// Trefor RP dungeon
//--- Description -----------------------------------------------------------
// Role-play as Trefor, protecting an NPC in Alby.
//--- Notes -----------------------------------------------------------------
// There are two versions of this dungeon, but they only differ in their
// single puzzle, rp_trefor or rp_trefor2, which affects the strength of
// the NPC you have to protect.
//---------------------------------------------------------------------------

[DungeonScript("rp_trefor_tircho_alby_dungeon")]
public class TreforRpDungeonScript : DungeonScript
{
	public override void OnCreation(Dungeon dungeon)
	{
		dungeon.SetRole(0, "#trefor");
	}

	public override void OnPartyEntered(Dungeon dungeon, Creature creature)
	{
		dungeon.PlayCutscene("RP_Trefor_00_a");
	}

	public override void OnPlayerEnteredFloor(Dungeon dungeon, Creature creature, int floor)
	{
		var leader = dungeon.GetCreators().First();

		if (!leader.Keywords.Has("RP_Trefor_Failed_1") && !leader.Keywords.Has("RP_Trefor_Failed_2") && !leader.Keywords.Has("RP_Trefor_Failed_3"))
			dungeon.PlayCutscene("RP_Trefor_00_e");
		else if (leader.Keywords.Has("RP_Trefor_Failed_1"))
			dungeon.PlayCutscene("RP_Trefor_00_f");
		else
			dungeon.PlayCutscene("RP_Trefor_00_g");
	}
}

[DungeonScript("rp_trefor_tircho_alby_dungeon2")]
public class TreforRp2DungeonScript : TreforRpDungeonScript
{
}
