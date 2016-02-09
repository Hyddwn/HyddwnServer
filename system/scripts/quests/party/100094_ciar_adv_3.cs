//--- Aura Script -----------------------------------------------------------
// [PQ] Defeat the Golem (Ciar Adv. for 3)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class CiarAdv3PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100094);
		SetScrollId(70025);
		SetName("[PQ] Defeat the Golem");
		SetDescription("Please offer [Ciar Adv. Fomor Pass for 3] on the altar of Ciar Dungeon, and defeat a [Golem] that can be found at the deepest part of the dungeon. The reward will be given to you outside the dungeon after completing the quest.");
		SetType(QuestType.Collect);

		AddObjective("obj1", "Eliminate 2 Golems", 0, 0, 0, Kill(2, "/golem/boss/mini_golem/ensemble/golem2/|/golem/boss/mini_golem/ensemble/golem1/"));
		AddObjective("obj2", "Eliminate 1 Giant Lightning Sprite", 0, 0, 0, Kill(1, "/elemental/giantlightningelemental/not_swallow/sprite1/"));
		AddObjective("obj3", "Clear Ciar Adv. Dungeon for 3", 0, 0, 0, ClearDungeon("TirCho_Ciar_High_3_Dungeon"));

		AddReward(Exp(18200));
		AddReward(Gold(20000));
	}
}
