//--- Aura Script -----------------------------------------------------------
// [PQ] Defeat the Golem (Ciar Int. for 2)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class CiarInt2PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100091);
		SetScrollId(70025);
		SetName("[PQ] Defeat the Golem");
		SetDescription("Please offer [Ciar Intermediate Fomor Pass for 2] on the altar of Ciar Dungeon, and defeat a [Golem] that can be found at the deepest part of the dungeon. The reward will be given to you outside the dungeon after completing the quest.");
		SetType(QuestType.Collect);

		AddObjective("obj1", "Eliminate 1 Golem", 0, 0, 0, Kill(1, "/golem/boss/golem4/"));
		AddObjective("obj2", "Eliminate 6 Metal Skeletons", 0, 0, 0, Kill(6, "/skeleton/undead/metalskeleton/armora/"));
		AddObjective("obj3", "Clear Ciar Int. Dungeon for 2", 0, 0, 0, ClearDungeon("TirCho_Ciar_Middle_2_Dungeon"));

		AddReward(Exp(6000));
		AddReward(Gold(15000));
	}
}
