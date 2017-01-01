//--- Aura Script -----------------------------------------------------------
// [PQ] Defeat the Golem (Ciar Basic)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class CiarBasicPartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100090);
		SetScrollId(70025);
		SetName(L("[PQ] Defeat the Golem"));
		SetDescription(L("Please offer [Ciar Basic Fomor Pass] on the altar of Ciar Dungeon, and defeat a [Golem] that can be found at the deepest part of the dungeon. The reward will be given to you outside the dungeon after completing the quest."));
		SetType(QuestType.Collect);
		SetCancelable(true);

		SetIcon(QuestIcon.Party);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("obj1", L("Eliminate 1 Golem"), 0, 0, 0, Kill(1, "/golem/boss/golem2/"));
		AddObjective("obj2", L("Eliminate 6 Metal Skeletons"), 0, 0, 0, Kill(6, "/skeleton/undead/metalskeleton/armorA/"));
		AddObjective("obj3", L("Clear Ciar Basic Dungeon"), 0, 0, 0, ClearDungeon("TirCho_Ciar_Low_Dungeon"));

		AddReward(Exp(3500));
		AddReward(Gold(10000));
	}
}
