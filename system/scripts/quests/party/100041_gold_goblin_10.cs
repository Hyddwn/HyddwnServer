//--- Aura Script -----------------------------------------------------------
// [PQ] Hunt Down the Gold Goblins (10)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class GoldGoblin10PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100041);
		SetScrollId(70025);
		SetName("[PQ] Hunt Down the Gold Goblins");
		SetDescription("Gold goblins that live deep inside the Rabbie Dungeon are scarier than ordinary goblins because of their powerful strength. They are from the lower class of Fomors. Please do us a favor and [hunt 10 gold goblins].");
		SetType(QuestType.Collect);

		AddObjective("obj", "Hunt 10 Gold Goblins", 0, 0, 0, Kill(10, "/goldgoblin/"));

		AddReward(Exp(159));
		AddReward(Gold(357));
	}
}
