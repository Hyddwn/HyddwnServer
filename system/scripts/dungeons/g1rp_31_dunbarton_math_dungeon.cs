//--- Aura Script -----------------------------------------------------------
// Shiela and Mores RP dungeon
//--- Description -----------------------------------------------------------
// G1 RP dungeon, starring Shiela's Ghost and Mores.
//---------------------------------------------------------------------------

[DungeonScript("g1rp_31_dunbarton_math_dungeon")]
public class ShielaMoresRPDungeonScript : DungeonScript
{
	public override void OnCreation(Dungeon dungeon)
	{
		//dungeon.SetRole(0, "#shiela");
		//dungeon.SetRole(1, "#mores");
	}

	public virtual void OnPartyEntered(Dungeon dungeon, Creature creature)
	{
		dungeon.PlayCutscene("G1_31_0_ShielaRP");
	}

	public override void OnSectionCleared(Dungeon dungeon, int floor, int section)
	{
		if (floor == 1 && section == 3)
			dungeon.PlayCutscene("G1_31_a_ShielaRP");
	}

	public override void OnBoss(Dungeon dungeon)
	{
		dungeon.AddBoss(12001, 6); // Ghost Armor

		dungeon.PlayCutscene("G1_31_b_ShielaRP");
	}

	public override void OnCleared(Dungeon dungeon)
	{
		dungeon.PlayCutscene("G1_31_c_ShielaRP", cutscene =>
		{
			// Switch keywords
			var creators = dungeon.GetCreators();
			foreach (var member in creators)
			{
				if (member.Keywords.Has("g1_34_1"))
				{
					member.Keywords.Remove("g1_34_1");
					member.Keywords.Give("g1_34_2");
					member.Keywords.Remove("g1_memorial4");
					member.Keywords.Give("g1_cichol");
				}
			}

			// Get out
			dungeon.RemoveAllPlayers();
		});
	}

	public override void OnLeftEarly(Dungeon dungeon, Creature creature)
	{
		dungeon.PlayCutscene("G1_LeaveDungeon");
	}
}
