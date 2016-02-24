//--- Aura Script -----------------------------------------------------------
// [PQ] Hunt Black Spiders (30)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class BlackSpider30PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100016);
		SetScrollId(70025);
		SetName("[PQ] Hunt Black Spiders");
		SetDescription("The Black Spiders in dungeons are a good source of cobwebs for collectors, but they also pose a serious threat to their safety. I will reward you for hunting [30 Black Spiders].");
		SetType(QuestType.Collect);

		AddObjective("obj", "Hunt 30 Black Spiders", 0, 0, 0, Kill(30, "/blackspider/"));

		AddReward(Exp(330));
		AddReward(Gold(1053));
	}
}
