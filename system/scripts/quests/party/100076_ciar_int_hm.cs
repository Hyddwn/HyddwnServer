//--- Aura Script -----------------------------------------------------------
// [PQ] Defeat the Golem (Ciar Int. Hardmode)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class CiarIntHardPartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100076);
		SetScrollId(70025);
		SetName(L("[PQ] Defeat the Golem"));
		SetDescription(L("Recently a new altar has been found at the back of Ciar Dungeon. Try offering [Ciar Intermediate Fomor Pass], and defeat a [Golem] that can be found at the deepest part of the dungeon."));
		SetType(QuestType.Collect);
		SetCancelable(true);

		SetIcon(QuestIcon.Party);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("obj1", L("Eliminate 1 Golem"), 0, 0, 0, Kill(1, "/golem/boss/golem4/hardmode/"));
		AddObjective("obj2", L("Eliminate 6 Metal Skeletons"), 0, 0, 0, Kill(6, "/skeleton/undead/metalskeleton/armora/hardmode/"));

		AddReward(Exp(16000));
		AddReward(Gold(20000));
	}
}
