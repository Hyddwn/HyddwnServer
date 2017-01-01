//--- Aura Script -----------------------------------------------------------
// Tracy RP dungeon
//--- Description -----------------------------------------------------------
// Tells a story about Tracy and how he is still being  bullied for his name.
//---------------------------------------------------------------------------

[DungeonScript("rp_tracy_tircho_ciar_dungeon")]
public class TracyRPDungeonScript : DungeonScript
{
	public override void OnCreation(Dungeon dungeon)
	{
		dungeon.SetRole(0, "#tracy");
		dungeon.SetRole(1, "#walter");
	}

	public override void OnPartyEntered(Dungeon dungeon, Creature creature)
	{
		dungeon.PlayCutscene("RP_Tracy_00_a");
	}

	public override void OnBoss(Dungeon dungeon)
	{
		dungeon.PlayCutscene("RP_Tracy_00_b", cutscene =>
		{
			var creature = cutscene.Leader.GetActualCreature();

			creature.Keywords.Give("RP_Tracy_Complete");

			dungeon.RemoveAllPlayers();
		});
	}
}
