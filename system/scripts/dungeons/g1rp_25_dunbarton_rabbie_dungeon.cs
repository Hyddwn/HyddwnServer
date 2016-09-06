//--- Aura Script -----------------------------------------------------------
// Rabbie Tarlach RP dungeon
//--- Description -----------------------------------------------------------
// RP dungeon in the G1 mainstream quests, featuring a party of 3,
// playing Tarlach, Mari, and Ruairi in Rabbie.
//---------------------------------------------------------------------------

[DungeonScript("g1rp_25_dunbarton_rabbie_dungeon")]
public class RabbieTarlachRpDungeonScript : DungeonScript
{
	public override void OnCreation(Dungeon dungeon)
	{
		//dungeon.SetRole(0, "#tarlach2");
		//dungeon.SetRole(1, "#mari2");
		//dungeon.SetRole(2, "#ruairi2");
	}

	public override void OnPartyEntered(Dungeon dungeon, Creature creature)
	{
		dungeon.PlayCutscene("G1_25_a_3WarriorsRP2");
	}

	public override void OnBoss(Dungeon dungeon)
	{
		dungeon.AddBoss(120005, 10); // Black Town Rat

		dungeon.PlayCutscene("G1_25_d_3WarriorsRP2", cutscene =>
		{
			var creators = dungeon.GetCreators();
			foreach (var member in creators)
			{
				if (member.Keywords.Has("g1_29"))
				{
					member.Keywords.Remove("g1_29");
					member.Keywords.Give("g1_30");
					member.Keywords.Remove("g1_bone_of_glasgavelen");
					member.Keywords.Give("g1_goddess_morrighan2");
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
