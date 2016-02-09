//--- Aura Script -----------------------------------------------------------
// [PQ] Defeat the Arachne (Alby Adv. Hardmode)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class AlbyAdvHardPartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100073);
		SetScrollId(70025);
		SetName("[PQ] Defeat the Arachne");
		SetDescription("Recently a new altar has been found at the back of Alby Dungeon. Try offering [Alby Adv. Fomor Pass], and defeat the [Arachne] that can be found at the deepest part of the dungeon.");
		SetType(QuestType.Collect);

		AddObjective("obj1", "Eliminate 1 Arachne", 0, 0, 0, Kill(1, "/arachne/boss/"));
		AddObjective("obj2", "Eliminate 6 Dark Blue Spiders", 0, 0, 0, Kill(6, "/spider/darkbluespider/hardmode/"));

		AddReward(Exp(55800));
		AddReward(Gold(25000));
	}
}
