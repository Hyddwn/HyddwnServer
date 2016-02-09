//--- Aura Script -----------------------------------------------------------
// [PQ] Defeat the Golem (Ciar Normal Hardmode)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class CiarNormalHardPartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100074);
		SetScrollId(70025);
		SetName("[PQ] Defeat the Golem");
		SetDescription("Recently a new altar has been found at the back of Ciar Dungeon. Try offering an item that is not a Fomor Pass, and defeat a [Golem] that can be found at the deepest part of the dungeon.");
		SetType(QuestType.Collect);

		AddObjective("obj1", "Eliminate 1 Golem", 0, 0, 0, Kill(1, "/golem/boss/hardmode/"));
		AddObjective("obj2", "Eliminate 6 Metal Skeletons", 0, 0, 0, Kill(6, "/skeleton/undead/metalskeleton/hardmode/"));

		AddReward(Exp(10000));
		AddReward(Gold(10000));
	}
}
