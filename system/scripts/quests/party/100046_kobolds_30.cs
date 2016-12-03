//--- Aura Script -----------------------------------------------------------
// [PQ] Hunt Kobolds (30)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class VariousKobolds30PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100046);
		SetScrollId(70025);
		SetName(L("[PQ] Hunt Kobolds"));
		SetDescription(L("Recently, various Kobolds have been showing up. Although Kobold's are not as intelligent as humans, they are becoming a threat. I will give you a reward if you [Hunt 10 Poison Kobolds and 20 Kobold Archers]."));
		SetType(QuestType.Collect);
		SetCancelable(true);

		SetIcon(QuestIcon.Party);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("obj1", L("Hunt 10 Poison Kobolds"), 0, 0, 0, Kill(10, "/poisonkobold/"));
		AddObjective("obj2", L("Hunt 20 Kobold Archers"), 0, 0, 0, Kill(20, "/koboldarcher/"));

		AddReward(Exp(696));
		AddReward(Gold(1596));
	}
}
