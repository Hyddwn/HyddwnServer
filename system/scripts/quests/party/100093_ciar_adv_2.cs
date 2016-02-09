//--- Aura Script -----------------------------------------------------------
// [PQ] Defeat the Golem (Ciar Adv. for 2)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class CiarAdv2PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100093);
		SetScrollId(70025);
		SetName("[PQ] Defeat the Golem");
		SetDescription("Please offer [Ciar Adv. Fomor Pass for 2] on the altar of Ciar Dungeon, and defeat a [Golem] that can be found at the deepest part of the dungeon. The reward will be given to you outside the dungeon after completing the quest.");
		SetType(QuestType.Collect);

		AddObjective("obj1", "Eliminate 1 Golem", 0, 0, 0, Kill(1, "/golem/boss/mini_golem/ensemble/golem3/"));
		AddObjective("obj2", "Eliminate 3 Dark Rat Men", 0, 0, 0, Kill(3, "/rat/beast/darkratman/"));
		AddObjective("obj3", "Clear Ciar Adv. Dungeon for 2", 0, 0, 0, ClearDungeon("TirCho_Ciar_High_2_Dungeon"));

		AddReward(Exp(13500));
		AddReward(Gold(20000));
	}
}
