//--- Aura Script -----------------------------------------------------------
// Succubus RP dungeon
//--- Description -----------------------------------------------------------
// G1 RP dungeon, starring player as Tarlach as he meets Kristell in Rabbie.
//---------------------------------------------------------------------------

[DungeonScript("g1rp_15_dunbarton_rabbie_dungeon")]
public class SuccubusRPDungeonScript : DungeonScript
{
	public override void OnCreation(Dungeon dungeon)
	{
		dungeon.SetRole(0, "#tarlach2");
	}

	public override void OnBoss(Dungeon dungeon)
	{
		dungeon.AddBoss(5, 1); // Succubus Kristell

		dungeon.PlayCutscene("G1_15_b_TarlachRP");
	}

	public override void OnPartyEntered(Dungeon dungeon, Creature creature)
	{
		dungeon.PlayCutscene("G1_15_a_TarlachRP");
	}

	public override void OnLeftEarly(Dungeon dungeon, Creature creature)
	{
		dungeon.PlayCutscene("G1_LeaveDungeon");
	}

	public override void OnCleared(Dungeon dungeon)
	{
		dungeon.PlayCutscene("G1_15_c_TarlachRP", cutscene =>
		{
			// Switch keywords
			var creators = dungeon.GetCreators();
			foreach (var member in creators)
			{
				if (member.HasKeyword("g1_15"))
				{
					member.RemoveKeyword("g1_15");
					member.GiveKeyword("g1_16");
					member.RemoveKeyword("g1_dulbrau2");
					member.GiveKeyword("g1_succubus");
				}
			}

			// Get out
			dungeon.RemoveAllPlayers();
		});
	}
}
