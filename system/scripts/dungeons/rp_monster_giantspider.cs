//--- Aura Script -----------------------------------------------------------
// Giant Spider RP dungeon
//--- Description -----------------------------------------------------------
// Short origin story of the Giant Spider in Alby dungeon.
//---------------------------------------------------------------------------

[DungeonScript("rp_monster_giantspider")]
public class GiantSpiderRPDungeonScript : DungeonScript
{
	public override void OnCreation(Dungeon dungeon)
	{
		dungeon.SetRole(0, "#giantspider");
	}

	public override void OnPlayerEnteredFloor(Dungeon dungeon, Creature creature, int floor)
	{
		if (floor == 1)
			dungeon.PlayCutscene("RP_Monster_GiantSpider_00_a");
	}

	public override void OnBoss(Dungeon dungeon)
	{
		dungeon.PlayCutscene("RP_Monster_GiantSpider_00_b", cutscene =>
		{
			var creature = cutscene.Leader.GetActualCreature();

			if (!creature.Keywords.Has("RP_Monster_GiantSpider_complete"))
			{
				creature.Keywords.Remove("RP_Monster_GiantSpider_start");
				creature.Keywords.Give("RP_Monster_GiantSpider_complete");
				creature.Keywords.Give("RP_Monster_GiantSpider_Born");
			}

			dungeon.RemoveAllPlayers();
		});
	}
}
