//--- Aura Script -----------------------------------------------------------
// [PQ] Defeat the Giant Red Spider (Alby Basic Hardmode)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class AlbyBasicHardPartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100071);
		SetScrollId(70025);
		SetName("[PQ] Defeat the Giant Red Spider");
		SetDescription("Recently a new altar has been found at the back of Alby Dungeon. Try offering [Alby Basic Fomor Pass], and defeat the [Giant Red Spider] that can be found at the deepest part of the dungeon.");
		SetType(QuestType.Collect);

		AddObjective("obj1", "Eliminate 1 Giant Red Spider", 0, 0, 0, Kill(1, "/spider/boss/redgiantspider/hardmode/"));
		AddObjective("obj2", "Eliminate 6 Dark Blue Spiders", 0, 0, 0, Kill(6, "/spider/darkbluespider/hardmode/"));

		AddReward(Exp(13800));
		AddReward(Gold(15000));
	}
}
