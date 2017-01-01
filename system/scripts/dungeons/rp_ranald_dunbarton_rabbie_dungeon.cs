//--- Aura Script -----------------------------------------------------------
// Ranald RP dungeon
//--- Description -----------------------------------------------------------
// Tells the story of Ranald visiting Rabbie dungeon.
//---------------------------------------------------------------------------

[DungeonScript("rp_ranald_dunbarton_rabbie_dungeon")]
public class RanaldRPDungeonScript : DungeonScript
{
	public override void OnCreation(Dungeon dungeon)
	{
		dungeon.SetRole(0, "#ranald");
	}

	public override void OnPartyEntered(Dungeon dungeon, Creature creature)
	{
		dungeon.PlayCutscene("RP_Ranald_00_a");
	}

	public override void OnBoss(Dungeon dungeon)
	{
		dungeon.AddBoss(10301, 1); // Black Succubus

		dungeon.PlayCutscene("RP_Ranald_00_b");
	}

	public override void OnCleared(Dungeon dungeon)
	{
		dungeon.PlayCutscene("RP_Ranald_00_c", cutscene =>
		{
			var creature = cutscene.Leader.GetActualCreature();

			creature.Keywords.Give("RP_Ranald_Complete");

			dungeon.RemoveAllPlayers();
		});
	}
}
