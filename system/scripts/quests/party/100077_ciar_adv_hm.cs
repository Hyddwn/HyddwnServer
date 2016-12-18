//--- Aura Script -----------------------------------------------------------
// [PQ] Defeat the Golem (Ciar Adv. Hardmode)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class CiarAdvHardPartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100077);
		SetScrollId(70025);
		SetName(L("[PQ] Defeat the Golem"));
		SetDescription(L("Recently a new altar has been found at the back of Ciar Dungeon. Try offering [Ciar Adv. Fomor Pass], and defeat a [Golem] that can be found at the deepest part of the dungeon."));
		SetType(QuestType.Collect);
		SetCancelable(true);

		SetIcon(QuestIcon.Party);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("obj1", L("Eliminate 2 Golems"), 0, 0, 0, Kill(2, "/golem/boss/mini_golem/ensemble/hardmode/golem1/|/golem/boss/mini_golem/ensemble/hardmode/golem2/"));
		AddObjective("obj2", L("Eliminate 2 Giant Lightning"), 0, 0, 0, Kill(2, "/elemental/giantlightningelemental/not_swallow/hardmode/"));

		AddReward(Exp(56800));
		AddReward(Gold(25000));
	}
}
