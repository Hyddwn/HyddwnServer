//--- Aura Script -----------------------------------------------------------
// [PQ] Defeat the Giant Spider (Alby Normal Hardmode)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class AlbyNormalHardPartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100070);
		SetScrollId(70025);
		SetName("[PQ] Defeat the Giant Spider");
		SetDescription("Recently a new altar has been found at the back of Alby Dungeon. Try offering an item that isn't a Fomor Pass, and defeat the [Giant Spider] that can be found at the deepest part of the dungeon.");
		SetType(QuestType.Collect);

		AddObjective("obj1", "Eliminate 1 Giant Spider", 0, 0, 0, Kill(1, "/spider/boss/giantspider/hardmode/"));
		AddObjective("obj2", "Eliminate 6 Red Spiders", 0, 0, 0, Kill(6, "/spider/redspider/hardmode/"));

		AddReward(Exp(4600));
		AddReward(Gold(10000));
	}
}
