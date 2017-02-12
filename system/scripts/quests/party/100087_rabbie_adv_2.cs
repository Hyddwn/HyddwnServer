//--- Aura Script -----------------------------------------------------------
// [PQ] Defeat the Black Succubus (Rabbie Adv. for 2)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class RabbieAdv2PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100087);
		SetScrollId(70025);
		SetName(L("[PQ] Defeat the Black Succubus"));
		SetDescription(L("Please offer [Rabbie Adv. Fomor Pass for 2] on the altar of Rabbie Dungeon, and defeat a [Black Succubus] that can be found at the deepest part of the dungeon."));
		SetType(QuestType.Collect);
		SetCancelable(true);

		SetIcon(QuestIcon.Party);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("obj1", L("Eliminate 1 Black Succubus"), 0, 0, 0, Kill(1, "/succubus/female/black_succubus/not_swallow/"));
		AddObjective("obj2", L("Eliminate 12 Skeleton Imps"), 0, 0, 0, Kill(12, "/imp/undead/skeletonimp/"));

		AddReward(Exp(8200));
		AddReward(Gold(20000));
	}
}
