//--- Aura Script -----------------------------------------------------------
// [PQ] Defeat the New Gremlin (Barri Adv. for 3)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class BarriAdv3PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100084);
		SetScrollId(70025);
		SetName("[PQ] Defeat the New Gremlin");
		SetDescription("Please offer [Barri Adv. Fomor Pass for 3] on the altar of Barri Dungeon, and defeat [New Gremlin] that can be found at the deepest part of the dungeon.");
		SetType(QuestType.Collect);

		AddObjective("obj1", "Eliminate 1 New Blue Gremlin", 0, 0, 0, Kill(1, "/boss/gremlin/armedgremlin/blue/ensemble/"));
		AddObjective("obj2", "Eliminate 1 New Pink Gremlin", 0, 0, 0, Kill(1, "/boss/gremlin/armedgremlin/pink/ensemble/"));
		AddObjective("obj3", "Eliminate 1 New Green Gremlin", 0, 0, 0, Kill(1, "/boss/gremlin/armedgremlin/green/"));

		AddReward(Exp(17200));
		AddReward(Gold(20000));
	}
}
