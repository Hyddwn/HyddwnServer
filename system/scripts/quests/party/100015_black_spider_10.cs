//--- Aura Script -----------------------------------------------------------
// [PQ] Hunt Black Spiders (10)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class BlackSpider10PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100015);
		SetScrollId(70025);
		SetName("[PQ] Hunt Black Spiders");
		SetDescription("The Black Spiders in dungeons are a good source of cobwebs for collectors, but they also pose a serious threat to their safety. I will reward you for hunting [10 Black Spiders].");
		SetType(QuestType.Collect);

		AddObjective("obj", "Hunt 10 Black Spiders", 0, 0, 0, Kill(10, "/blackspider/"));

		AddReward(Exp(117));
		AddReward(Gold(360));
		AddReward(QuestScroll(100016)); // [PQ] Hunt Black Spiders (30)
	}
}
