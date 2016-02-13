//--- Aura Script -----------------------------------------------------------
// [PQ] Hunt Red Spiders (10)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class RedSpider10PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100017);
		SetScrollId(70025);
		SetName("[PQ] Hunt Red Spiders");
		SetDescription("The Red Spiders in dungeons are a good source of cobwebs for collectors, but they also pose a serious threat to their safety. I will reward you for hunting [10 Red Spiders].");
		SetType(QuestType.Collect);

		AddObjective("obj", "Hunt 10 Red Spiders", 0, 0, 0, Kill(10, "/redspider/"));

		AddReward(Exp(129));
		AddReward(Gold(450));
		AddReward(QuestScroll(100018)); // [PQ] Hunt Red Spiders (30)
	}
}
