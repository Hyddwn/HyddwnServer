//--- Aura Script -----------------------------------------------------------
// [PQ] Defeat the Red Succubus (Rabbie Adv. for 3)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class RabbieAdv3PartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100088);
		SetScrollId(70025);
		SetName("[PQ] Defeat the Red Succubus");
		SetDescription("Please offer [Rabbie Adv. Fomor Pass for 3] on the altar of Rabbie Dungeon, and defeat a [Red Succubus] that can be found at the deepest part of the dungeon.");
		SetType(QuestType.Collect);

		AddObjective("obj1", "Eliminate 1 Red Succubus", 0, 0, 0, Kill(1, "/succubus/female/red_succubus/"));
		AddObjective("obj2", "Eliminate 6 Skeleton Laghodessas", 0, 0, 0, Kill(6, "/laghodessa/skeletonlaghodessa/"));

		AddReward(Exp(11300));
		AddReward(Gold(20000));
	}
}
