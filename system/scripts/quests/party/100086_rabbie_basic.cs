//--- Aura Script -----------------------------------------------------------
// [PQ] Defeat the Lycanthrope (Rabbie Basic)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class RabbieBasicPartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100086);
		SetScrollId(70025);
		SetName("[PQ] Defeat the Lycanthrope");
		SetDescription("Please offer [Rabbie Basic Fomor Pass] on the altar of Rabbie Dungeon, and defeat a [Lycanthrope] that can be found at the deepest part of the dungeon.");
		SetType(QuestType.Collect);

		AddObjective("obj1", "Eliminate 2 Lycanthropes", 0, 0, 0, Kill(1, "/lycanthrope/lycanthropeteam1/boss/|/lycanthrope/lycanthropeteam2/boss/"));
		AddObjective("obj2", "Clear Rabbie Basic Dungeon", 0, 0, 0, ClearDungeon("Dunbarton_Rabbie_Low_Dungeon"));

		AddReward(Exp(3000));
		AddReward(Gold(10000));
	}
}
