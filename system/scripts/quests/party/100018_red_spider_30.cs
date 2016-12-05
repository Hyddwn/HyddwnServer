//--- Aura Script -----------------------------------------------------------
// [PQ] Hunt Red Spiders (30)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class RedSpider30PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100018);
		SetScrollId(70025);
		SetName(L("[PQ] Hunt Red Spiders"));
		SetDescription(L("The Red Spiders in dungeons are a good source of cobwebs for collectors, but they also pose a serious threat to their safety. I will reward you for hunting [30 Red Spiders]."));
		SetType(QuestType.Collect);
		SetCancelable(true);

		SetIcon(QuestIcon.Party);
		if (IsEnabled("QuestViewRenewal"))
			SetCategory(QuestCategory.Repeat);

		AddObjective("obj", L("Hunt 30 Red Spiders"), 0, 0, 0, Kill(30, "/redspider/"));

		AddReward(Exp(378));
		AddReward(Gold(1278));
	}
}
