//--- Aura Script -----------------------------------------------------------
// [PQ] Hunt Down the Dire Sprites (10)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class DireSprites10_1PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100067);
		SetScrollId(70025);
		SetName(L("[PQ] Hunt Down the Dire Sprites"));
		SetDescription(L("Sprites are under an evil spell and are attacking travelers. Please Hunt [10 Ice Sprites, 10 Fire Sprites, and 10 Lightning Sprites.]"));
		SetType(QuestType.Collect);
		SetCancelable(true);

		SetIcon(QuestIcon.Party);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("obj1", L("Hunt 10 Ice Sprites"), 0, 0, 0, Kill(10, "/lightningsprite/"));
		AddObjective("obj2", L("Hunt 10 Fire Sprites"), 0, 0, 0, Kill(10, "/firesprite/"));
		AddObjective("obj3", L("Hunt 10 Lightning Sprites"), 0, 0, 0, Kill(10, "/icesprite/"));

		AddReward(Exp(1926));
		AddReward(Gold(2166));
	}
}
