//--- Aura Script -----------------------------------------------------------
// Albey Black Orb Dungeon
//--- Description -----------------------------------------------------------
// Yields keyword that is required to get into the last dungeon of G1.
//---------------------------------------------------------------------------

[DungeonScript("g1_37_black_tirnanog_dungeon")]
public class AlbeyBlackOrbDungeonScript : DungeonScript
{
	public override void OnBoss(Dungeon dungeon)
	{
		dungeon.AddBoss(12001, 6); // Ghost Armor
	}

	public override void OnCleared(Dungeon dungeon)
	{
		dungeon.PlayCutscene("G1_38_a_Morrighan", cutscene =>
		{
			// Switch keywords
			var creators = dungeon.GetCreators();
			foreach (var member in creators)
			{
				if (member.Keywords.Has("g1_36"))
				{
					member.Keywords.Remove("g1_36");
					member.Keywords.Give("g1_37");
					member.Keywords.Give("g1_revive_of_glasgavelen");
				}
			}

			// Get out
			dungeon.RemoveAllPlayers();
		});
	}
}
