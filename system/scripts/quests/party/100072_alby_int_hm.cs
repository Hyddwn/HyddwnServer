//--- Aura Script -----------------------------------------------------------
// [PQ] Defeat the Lycanthrope (Alby Int. Hardmode)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class AlbyIntHardPartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100072);
		SetScrollId(70025);
		SetName("[PQ] Defeat the Lycanthrope");
		SetDescription("Recently a new altar has been found at the back of Alby Dungeon. Try offering [Alby Intermediate Fomor Pass], and defeat the [Lycanthrope] that can be found at the deepest part of the dungeon.");
		SetType(QuestType.Collect);

		AddObjective("obj1", "Eliminate 2 Lycanthropes", 0, 0, 0, Kill(2, "/lycanthrope/lycanthropeteam7/boss/hardmode/"));
		AddObjective("obj2", "Eliminate 5 Gorgons", 0, 0, 0, Kill(5, "/gorgon/normalgorgon/hardmode/"));

		AddReward(Exp(22600));
		AddReward(Gold(20000));
	}
}
