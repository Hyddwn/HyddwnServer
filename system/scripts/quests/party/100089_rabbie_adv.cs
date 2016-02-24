//--- Aura Script -----------------------------------------------------------
// [PQ] Defeat the Red Succubus (Rabbie Adv.)
//--- Description -----------------------------------------------------------
// Party quest to kill certain monsters.
//---------------------------------------------------------------------------

public class RabbieAdvPartyQuest : QuestScript
{
	public override void Load()
	{
		SetId(100089);
		SetScrollId(70025);
		SetName("[PQ] Defeat the Red Succubus");
		SetDescription("Please offer [Rabbie Adv. Fomor Pass] on the altar of Rabbie Dungeon, and defeat a [Red Succubus] that can be found at the deepest part of the dungeon.");
		SetType(QuestType.Collect);

		AddObjective("obj1", "Eliminate 1 Red Succubus", 0, 0, 0, Kill(1, "/boss/succubus/female/armysuccubus/red/ensemble/"));
		AddObjective("obj2", "Eliminate 1 Yellow Succubus", 0, 0, 0, Kill(1, "/boss/succubus/female/armysuccubus/yellow/"));
		AddObjective("obj3", "Eliminate 4 Giant Lightning Sprites", 0, 0, 0, Kill(4, "/elemental/giantlightningelemental/not_swallow/sprite1/"));

		AddReward(Exp(15200));
		AddReward(Gold(20000));
	}
}
