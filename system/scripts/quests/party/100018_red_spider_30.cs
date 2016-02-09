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
		SetName("[PQ] Hunt Red Spiders");
		SetDescription("The Red Spiders in dungeons are a good source of cobwebs for collectors, but they also pose a serious threat to their safety. I will reward you for hunting [30 Red Spiders].");
		SetType(QuestType.Collect);

		AddObjective("obj", "Hunt 30 Red Spiders", 0, 0, 0, Kill(30, "/redspider/"));

		AddReward(Exp(378));
		AddReward(Gold(1278));
	}
}
