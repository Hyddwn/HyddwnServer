//--- Aura Script -----------------------------------------------------------
// Chicken RP dungeon
//--- Description -----------------------------------------------------------
// Tells the story of a chick searching for its mother.
//---------------------------------------------------------------------------

[DungeonScript("rp_monster_chicken")]
public class ChickenRPDungeonScript : DungeonScript
{
	public override void OnCreation(Dungeon dungeon)
	{
		dungeon.SetRole(0, "#chick");
	}

	public override void OnPartyEntered(Dungeon dungeon, Creature creature)
	{
		dungeon.PlayCutscene("RP_Monster_Chicken_00_a");
	}

	public override void OnBoss(Dungeon dungeon)
	{
		dungeon.PlayCutscene("RP_Monster_Chicken_00_b", cutscene =>
		{
			var creature = cutscene.Leader.GetActualCreature();

			if (!creature.Keywords.Has("RP_Monster_Chicken_complete"))
			{
				creature.Keywords.Remove("RP_Monster_Chicken_start");
				creature.Keywords.Give("RP_Monster_Chicken_complete");
			}

			dungeon.RemoveAllPlayers();
		});
	}
}
